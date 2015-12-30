using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpTox.Core;

namespace SharpTox.HL.Transfers
{
    internal class ToxTransferCollection : IReadOnlyList<ToxTransfer>
    {
        private readonly ToxHL _tox;
        private readonly ToxFriend _friend;
        private readonly object _transfersLock = new object();
        private readonly List<ToxTransfer> _transfers = new List<ToxTransfer>();

        public event EventHandler<ToxTransferEventArgs.RequestEventArgs> TransferRequestReceived;

        internal ToxTransferCollection(ToxHL tox, ToxFriend friend)
        {
            _tox = tox;
            _friend = friend;

            _tox.Core.OnFileSendRequestReceived += OnFileSendRequestReceived;
        }

        public IEnumerator<ToxTransfer> GetEnumerator()
        {
            lock (_transfersLock)
                return _transfers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return _transfers.Count; }
        }

        public ToxTransfer this[int index]
        {
            get
            {
                lock (_transfersLock)
                    return _transfers[index];
            }
        }

        private void OnFileSendRequestReceived(object sender, ToxEventArgs.FileSendRequestEventArgs e)
        {
            if (e.FriendNumber != _friend.Number)
                return;

            var id = _tox.Core.FileGetId(_friend.Number, e.FileNumber);
            var resumed = TryResumeBrokenIncomingTransfer(id);

            if (!resumed)
            {
                // If we couldn't find it amongst our broken incoming transfers, then it's a new incoming transfer:

                var transfer = new ToxIncomingTransfer(_tox, _friend,
                    new ToxFileInfo(e.FileNumber, id), e.FileName, e.FileSize, e.FileKind);
                AddTransferToList(transfer);

                if (TransferRequestReceived != null)
                    TransferRequestReceived(this, new ToxTransferEventArgs.RequestEventArgs(transfer));
            }
        }

        private bool TryResumeBrokenIncomingTransfer(byte[] id)
        {
            lock (_transfersLock)
            {
                foreach (var transfer in _transfers)
                {
                    if (transfer is ToxIncomingTransfer && transfer.State == ToxTransferState.Broken &&
                        transfer.Info.Id.SequenceEqual(id))
                    {
                        (transfer as ToxIncomingTransfer).Resume();
                        return true;
                    }
                }
            }

            return false;
        }

        public ToxOutgoingTransfer SendFile(Stream stream, string fileName, ToxFileKind kind)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (fileName == null)
                throw new ArgumentNullException("fileName");

            var error = ToxErrorFileSend.Ok;
            var fileInfo = _tox.Core.FileSend(_friend.Number, kind, stream.Length, fileName, out error);

            if (error != ToxErrorFileSend.Ok)
                throw new ToxException<ToxErrorFileSend>(error);

            var transfer = new ToxOutgoingTransfer(_tox, stream, _friend, fileInfo, fileName, kind);
            AddTransferToList(transfer);
            return transfer;
        }

        private void AddTransferToList(ToxTransfer transfer)
        {
            transfer.StateChanged += OnStateChanged;

            lock (_transfersLock)
                _transfers.Add(transfer);
        }

        private void OnStateChanged(object sender, ToxTransferEventArgs.StateEventArgs e)
        {
            if (e.State != ToxTransferState.Canceled && e.State != ToxTransferState.Finished)
                return;

            lock (_transfersLock)
                _transfers.Remove(sender as ToxTransfer);
        }

        public void ResumeBrokenTransfer(ToxTransferResumeData resumeData)
        {
            if (resumeData == null)
                throw new ArgumentNullException("resumeData");

            if (resumeData.FriendNumber != _friend.Number)
                throw new ArgumentException("Invalid friend number!");

            if (IsTransferAlreadyResumed(resumeData)) // TODO: Write some tests for it!
                throw new ArgumentException("Transfer is already resumed!");

            switch (resumeData.Direction)
            {
                case ToxTransferDirection.Outgoing:
                    AddTransferToList(new ToxOutgoingTransfer(_tox, _friend, resumeData));
                    break;
                case ToxTransferDirection.Incoming:
                    AddTransferToList(new ToxIncomingTransfer(_tox, _friend, resumeData));
                    break;
            }
        }

        private bool IsTransferAlreadyResumed(ToxTransferResumeData resumeData)
        {
            lock (_transfersLock)
            {
                return _transfers.Any(transfer => transfer.Info.Id.SequenceEqual(resumeData.Info.Id));
            }
        }
    }
}