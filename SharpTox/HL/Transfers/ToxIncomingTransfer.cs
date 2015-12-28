using System;
using System.IO;
using SharpTox.Core;

namespace SharpTox.HL.Transfers
{
    public class ToxIncomingTransfer : ToxTransfer
    {
        internal ToxIncomingTransfer(ToxHL tox, ToxFriend friend, ToxFileInfo info, string name, long size, ToxFileKind kind)
            : base(tox, friend, info, name, size, kind)
        {
            Tox.Core.OnFileChunkReceived += OnFileChunkReceived;
        }

        public void Accept(Stream stream)
        {
            _stream = stream;
            Resume();
        }

        private void OnFileChunkReceived (object sender, ToxEventArgs.FileChunkEventArgs e)
        {
            if (ShouldntHandle(e))
                return;

            if (e.Data == null || e.Data.Length == 0)
            {
                //end of file transfer, fire the finished event
                Finish();
                return;
            }

            if (e.Position < _stream.Position)
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
            else if (e.Position > _stream.Position)
            {
                //if this happens, we're missing some bytes for sure and we can't recover from that, cancel the transfer and fire an error event
                OnError(new ToxTransferError("Missing some chunks. This should never happen!"), true);
                return;
            }

            try { _stream.Write(e.Data, 0, e.Data.Length); }
            catch (Exception ex) 
            {
                //couldn't write to stream, cancel the transfer and fire an error event
                OnError(new ToxTransferError("Failed to write to the stream", ex), true);
            }

            TransferredBytes += e.Data.Length;
        }
    }
}
