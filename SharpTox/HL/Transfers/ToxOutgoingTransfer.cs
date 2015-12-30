using System;
using System.IO;
using SharpTox.Core;

namespace SharpTox.HL.Transfers
{
    public class ToxOutgoingTransfer : ToxTransfer
    {
        internal ToxOutgoingTransfer(ToxHL tox, Stream stream, ToxFriend friend, ToxFileInfo info, string name, ToxFileKind kind)
            : base(tox, stream, friend, info, name, kind)
        {
            Tox.Core.OnFileChunkRequested += OnFileChunkRequested;
        }

        private void OnFileChunkRequested (object sender, ToxEventArgs.FileRequestChunkEventArgs e)
        {
            if (ShouldntHandle(e))
                return;

            if (e.Length == 0)
            {
                //we're nice people, let's send an empty chunk so the other end knows we're done sending
                Tox.Core.FileSendChunk(Friend.Number, Info.Number, e.Position, new byte[0]);
                Finish();
                return;
            }

            if (_stream.Position != e.Position)
            {
                //we have to rewind the stream
                try { _stream.Seek(e.Position, SeekOrigin.Begin); }
                catch (Exception ex)
                {
                    //failed to rewind stream, we can't recover from this
                    OnError(new ToxTransferError("Failed to rewind the stream", ex), true);
                    return;
                }
            }

            byte[] chunk = new byte[e.Length];
            try { _stream.Read(chunk, 0, e.Length); }
            catch (Exception ex)
            {
                //could not read from stream, cancel the transfer and fire the error event
                OnError(new ToxTransferError("Failed to read from the stream", ex), true);
                return;
            }

            var error = ToxErrorFileSendChunk.Ok;
            Tox.Core.FileSendChunk(Friend.Number, Info.Number, e.Position, chunk, out error);

            if (error != ToxErrorFileSendChunk.Ok)
            {
                //could not send a chunk, cancel the transfer and fire the error event
                //TODO: or should we retry in a bit?
                OnError(new ToxTransferError("Could not send the next chunk"), true);
            }

            TransferredBytes += e.Length;
        }
    }
}

