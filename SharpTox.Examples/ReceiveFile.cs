using System;
using System.IO;

using SharpTox.Core;

namespace SharpTox.Examples
{
    /// <summary>
    /// A very basic example on how to receive a file. This does not include pausing or resuming the file transfer.
    /// All of the other stuff like bootstrapping and accepting friend requests have been left out. Check Basic.cs for that.
    /// </summary>
    class ReceiveFile
    {
        private Tox _tox;
        private FileStream _stream;

        public ReceiveFile()
        {
            _tox = new Tox(new ToxOptions(true, false));
            _tox.OnFileControl += tox_OnFileControl;
            _tox.OnFileData += tox_OnFileData;
            _tox.OnFileSendRequest += tox_OnFileSendRequest;
            _tox.Start();
        }

        private void tox_OnFileSendRequest(object sender, ToxEventArgs.FileSendRequestEventArgs e)
        {
            _tox.FileSendControl(e.FriendNumber, 1, e.FileNumber, ToxFileControl.Accept, new byte[0]);
        }

        private void tox_OnFileData(object sender, ToxEventArgs.FileDataEventArgs e)
        {
            if (_stream == null)
                _stream = new FileStream("file.dat", FileMode.Create);

            _stream.Write(e.Data, 0, e.Data.Length);
        }

        private void tox_OnFileControl(object sender, ToxEventArgs.FileControlEventArgs e)
        {
            switch (e.Control)
            {
                case ToxFileControl.Kill:
                case ToxFileControl.Finished:
                    {
                        _stream.Dispose();
                        break;
                    }
            }
        }
    }
}
