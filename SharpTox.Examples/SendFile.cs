using System;
using System.IO;
using System.Threading;

using SharpTox.Core;

namespace SharpTox.Examples
{
    /// <summary>
    /// A very basic example on how to send a file. This does not include pausing or resuming the file transfer.
    /// All of the other stuff like bootstrapping and accepting friend requests have been left out. Check Basic.cs for that.
    /// </summary>
    class SendFile
    {
        private Tox _tox;
        private FileStream _stream;
        private Thread _thread;

        public SendFile()
        {
            _tox = new Tox(new ToxOptions(true, false));
            _tox.OnFileControl += tox_OnFileControl;
            _tox.Start();
        }

        private void tox_OnFileControl(object sender, ToxEventArgs.FileControlEventArgs e)
        {
            switch (e.Control)
            {
                case ToxFileControl.Kill:
                    {
                        _thread.Abort();
                        _thread.Join();

                        _stream.Dispose();
                        break;
                    }
                case ToxFileControl.Accept:
                    {
                        _stream = new FileStream("file.data", FileMode.Open);

                        _thread = new Thread(Loop);
                        _thread.Start(new FileTransfer() { FriendNumber = e.FriendNumber, FileNumber = e.FileNumber });
                        break;
                    }
            }
        }

        private void Loop(object t)
        {
            var transfer = (FileTransfer)t;

            int chunk_size = _tox.FileDataSize(transfer.FriendNumber);
            byte[] buffer = new byte[chunk_size];

            while (true)
            {
                ulong remaining = _tox.FileDataRemaining(transfer.FriendNumber, transfer.FileNumber, 0);
                if (remaining > (ulong)chunk_size)
                {
                    //read the next chunk, if we don't read anything, break;
                    if (_stream.Read(buffer, 0, chunk_size) == 0)
                        break;

                    //keep trying to send the data if it fails
                    while (!_tox.FileSendData(transfer.FriendNumber, transfer.FileNumber, buffer))
                    {
                        int time = (int)ToxFunctions.DoInterval(_tox.Handle);
                        Thread.Sleep(time);
                    }
                }
                else
                {
                    //send the last chunk of data
                    buffer = new byte[remaining];

                    if (_stream.Read(buffer, 0, (int)remaining) == 0)
                        break;

                    _tox.FileSendData(transfer.FriendNumber, transfer.FileNumber, buffer);
                }
            }

            _stream.Dispose();

            //notify our friend that we're done sending
            //we should wait for our friend to send ToxFileControl.Finished back to confirm that he received everything correctly (this is not covered in this example)
            _tox.FileSendControl(transfer.FriendNumber, 0, transfer.FileNumber, ToxFileControl.Finished, new byte[0]);
        }
    }

    /// <summary>
    /// Helper class for file transfers.
    /// </summary>
    class FileTransfer
    {
        public int FileNumber { get; set; }
        public int FriendNumber { get; set; }
    }
}
