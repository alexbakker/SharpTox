using System;
using System.IO;
using SharpTox.Core;

namespace SharpTox.HL
{
    public class ToxFileTransfer
    {
        public ToxHL Tox { get; private set; }
        public ToxFileInfo Info { get; private set; }
        public ToxFriend Friend { get; private set; }

        protected readonly Stream _stream;

        internal ToxFileTransfer(ToxHL tox, Stream stream, ToxFriend friend, ToxFileInfo info)
        {
            Tox = tox;
            Friend = friend;
            Info = info;

            _stream = stream;
        }

        public void Pause()
        {
            SendControl(ToxFileControl.Pause);
        }

        public void Resume()
        {
            SendControl(ToxFileControl.Resume);
        }

        public void Cancel()
        {
            SendControl(ToxFileControl.Cancel);
        }

        private void SendControl(ToxFileControl control)
        {
            var error = ToxErrorFileControl.Ok;
            Tox.Core.FileControl(Friend.Number, Info.Number, control, out error);

            if (error != ToxErrorFileControl.Ok)
                throw new Exception("Could not send file control");
        }
    }
}
