using System;
using System.IO;
using SharpTox.Core;

namespace SharpTox.HL.Transfers
{
    public class ToxOutgoingTransfer : ToxTransfer
    {
        internal ToxOutgoingTransfer(ToxHL tox, Stream stream, ToxFriend friend, ToxFileInfo info, string name,
            ToxFileKind kind)
            : base(tox, stream, friend, info, name, kind)
        {
            Tox.Core.OnFileChunkRequested += OnFileChunkRequested;
        }

        internal ToxOutgoingTransfer(ToxHL tox, ToxFriend friend, ToxTransferResumeData resumeData, Stream stream)
            : base(tox, friend, resumeData, stream)
        {
            Tox.Core.OnFileChunkRequested += OnFileChunkRequested;
        }

        private void OnFileChunkRequested(object sender, ToxEventArgs.FileRequestChunkEventArgs e)
        {
            if (ShouldntHandle(e))
                return;

            if (e.Length == 0)
            {
                // We're nice people, let's send an empty chunk so the other end knows we're done sending:
                Tox.Core.FileSendChunk(Friend.Number, Info.Number, e.Position, new byte[0]);
                Finish();
                return;
            }

            if (_stream.Position != e.Position)
            {
                // We have to rewind the stream:
                try
                {
                    _stream.Seek(e.Position, SeekOrigin.Begin);
                }
                catch (Exception ex)
                {
                    // Failed to rewind stream, we can't recover from this.
                    OnError(new ToxTransferError("Failed to rewind the stream", ex), true);
                    return;
                }
            }

            byte[] chunk = new byte[e.Length];
            try
            {
                _stream.Read(chunk, 0, e.Length);
            }
            catch (Exception ex)
            {
                // Could not read from stream, cancel the transfer and fire the error event.
                OnError(new ToxTransferError("Failed to read from the stream", ex), true);
                return;
            }

            try
            {
                var error = ToxErrorFileSendChunk.Ok;
                Tox.Core.FileSendChunk(Friend.Number, Info.Number, e.Position, chunk, out error);

                if (error != ToxErrorFileSendChunk.Ok)
                {
                    // Could not send a chunk, cancel the transfer and fire the error event.
                    // TODO: or should we retry in a bit?
                    OnError(new ToxTransferError("Could not send the next chunk. Error: " + error), true);
                    return;
                }
            }
            catch (ObjectDisposedException ex)
            {
                // In case this Tox instance is disposed after the event is fired and before it's handled.
                State = ToxTransferState.Broken;
                return;
            }

            TransferredBytes = _stream.Position;
        }

        protected override void OnFriendConnectionStatusChanged(object sender,
            ToxFriendEventArgs.ConnectionStatusEventArgs connectionStatusEventArgs)
        {
            base.OnFriendConnectionStatusChanged(sender, connectionStatusEventArgs);

            // We automatically resume a broken outgoing transfer when the receiving friend comes back online:

            if (Friend.IsOnline && State == ToxTransferState.Broken)
            {
                var error = ToxErrorFileSend.Ok;
                Info = Tox.Core.FileSend(Friend.Number, Kind, Size, Name, Info.Id);

                if (error != ToxErrorFileSend.Ok)
                {
                    OnError(new ToxTransferError("Couldn't resume broken transfer"), true);
                }
                else
                {
                    State = ToxTransferState.Pending;
                }
            }
        }

        public override ToxTransferResumeData GetResumeData()
        {
            var resumeData = base.GetResumeData();
            resumeData.Direction = ToxTransferDirection.Outgoing;
            return resumeData;
        }
    }
}