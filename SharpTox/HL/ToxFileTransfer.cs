using System;
using System.IO;
using SharpTox.Core;

namespace SharpTox.HL
{
    public class ToxFileTransfer
    {
        public readonly ToxHL Tox;
        public readonly ToxFileInfo Info;

        protected Stream _stream;

        internal ToxFileTransfer(ToxHL tox, Stream stream, ToxFileInfo info)
        {
            Tox = tox;
            Info = info;

            _stream = stream;
        }
    }
}
