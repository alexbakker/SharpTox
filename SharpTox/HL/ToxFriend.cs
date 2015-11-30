using System;
using System.IO;
using SharpTox.Core;

namespace SharpTox.HL
{
    public class ToxFriend
    {
        public readonly int Number;
        public readonly ToxHL Tox;

        internal ToxFriend(ToxHL tox, int friendNumber)
        {
            Tox = tox;
            Number = friendNumber;
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
                throw new Exception("Could not create file sender");

            return new ToxFileTransfer(Tox, stream, fileInfo);
        }
    }
}
