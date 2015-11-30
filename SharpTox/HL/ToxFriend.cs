using System;
using System.IO;
using SharpTox.Core;

namespace SharpTox.HL
{
    public class ToxFriend
    {
        public int Number { get; private set; }
        public ToxHL Tox { get; private set; }

        internal ToxFriend(ToxHL tox, int friendNumber)
        {
            Tox = tox;
            Number = friendNumber;
        }

        public int SendMessage(string message, ToxMessageType type)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            var error = ToxErrorSendMessage.Ok;
            int messageNumber = Tox.Core.SendMessage(Number, message, type, out error);

            if (error != ToxErrorSendMessage.Ok)
                throw new Exception("Could not send message");

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
                throw new Exception("Could not create file sender");

            return new ToxFileTransfer(Tox, stream, this, fileInfo);
        }
    }
}
