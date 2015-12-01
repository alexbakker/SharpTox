using System;
using System.IO;
using SharpTox.Core;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace SharpTox.HL
{
    public class ToxFriend
    {
        public int Number { get; private set; }
        public ToxHL Tox { get; private set; }

        public event EventHandler<ToxHLEventArgs.FileSendRequestEventArgs> OnFileSendRequestReceived;

        public ReadOnlyCollection<ToxFileTransfer> Transfers 
        {
            get
            {
                lock (_transfersLock)
                    return _transfers.AsReadOnly();
            }
        }

        public string Name 
        { 
            get { return QueryFriend(Tox.Core.GetFriendName, Number); }
        }

        public string StatusMessage 
        { 
            get { return QueryFriend(Tox.Core.GetFriendStatusMessage, Number); }
        }

        public ToxUserStatus Status 
        { 
            get { return QueryFriend(Tox.Core.GetFriendStatus, Number); }
        }

        public ToxConnectionStatus ConnectionStatus 
        {
            get { return QueryFriend(Tox.Core.GetFriendConnectionStatus, Number); }
        }

        public bool IsTyping
        {
            get { return QueryFriend(Tox.Core.GetFriendTypingStatus, Number); }
        }

        public ToxKey PublicKey
        {
            get 
            { 
                var error = ToxErrorFriendGetPublicKey.Ok;
                var result = Tox.Core.GetFriendPublicKey(Number, out error);

                if (error != ToxErrorFriendGetPublicKey.Ok)
                    throw new ToxException<ToxErrorFriendGetPublicKey>(error);

                return result;
            }
        }

        public DateTime LastOnline
        {
            get 
            { 
                var error = ToxErrorFriendGetLastOnline.Ok;
                var result = Tox.Core.GetFriendLastOnline(Number, out error);

                if (error != ToxErrorFriendGetLastOnline.Ok)
                    throw new ToxException<ToxErrorFriendGetLastOnline>(error);

                return result;
            }
        }

        private readonly List<ToxFileTransfer> _transfers = new List<ToxFileTransfer>();
        private readonly object _transfersLock = new object();

        internal ToxFriend(ToxHL tox, int friendNumber)
        {
            Tox = tox;
            Number = friendNumber;

            Tox.Core.OnFileSendRequestReceived += Core_OnFileSendRequestReceived;
        }

        private void Core_OnFileSendRequestReceived (object sender, ToxEventArgs.FileSendRequestEventArgs e)
        {
            if (e.FriendNumber != Number)
                return;

            var transfer = new ToxIncomingTransfer(Tox, this, new ToxFileInfo(e.FileNumber, Tox.Core.FileGetId(Number, e.FileNumber)), e.FileName, e.FileSize, e.FileKind);
            AddTransferToList(transfer);

            OnFileSendRequestReceived(this, new ToxHLEventArgs.FileSendRequestEventArgs(transfer));
        }

        public int SendMessage(string message, ToxMessageType type)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            var error = ToxErrorSendMessage.Ok;
            int messageNumber = Tox.Core.SendMessage(Number, message, type, out error);

            if (error != ToxErrorSendMessage.Ok)
                throw new ToxException<ToxErrorSendMessage>(error);

            return messageNumber;
        }

        public ToxFileTransfer SendFile(Stream stream, string fileName, ToxFileKind kind)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (fileName == null)
                throw new ArgumentNullException("fileName");

            var error = ToxErrorFileSend.Ok;
            var fileInfo = Tox.Core.FileSend(Number, kind, stream.Length, fileName, out error);

            if (error != ToxErrorFileSend.Ok)
                throw new ToxException<ToxErrorFileSend>(error);

            var transfer = new ToxOutgoingTransfer(Tox, stream, this, fileInfo, fileName, kind);
            AddTransferToList(transfer);
            return transfer;
        }

        private void AddTransferToList(ToxFileTransfer transfer)
        {
            transfer.OnCanceled += OnRemoveTransfer;
            transfer.OnFinished += OnRemoveTransfer;

            lock (_transfersLock)
                _transfers.Add(transfer);
        }

        private void OnRemoveTransfer(object sender, EventArgs args)
        {
            lock (_transfersLock)
                _transfers.Add(sender as ToxFileTransfer);
        }

        private delegate T QueryFriendDelegate<T>(int friendNumber, out ToxErrorFriendQuery error);

        private T QueryFriend<T>(QueryFriendDelegate<T> func, int friendNumber)
        {
            var error = ToxErrorFriendQuery.Ok;
            var result = func(friendNumber, out error);

            if (error != ToxErrorFriendQuery.Ok)
                throw new ToxException<ToxErrorFriendQuery>(error);

            return result;
        }
    }
}
