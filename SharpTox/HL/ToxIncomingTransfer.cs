using System;
using System.IO;
using SharpTox.Core;

namespace SharpTox.HL
{
    public class ToxIncomingTransfer : ToxFileTransfer
    {
        internal ToxIncomingTransfer(ToxHL tox, Stream stream, ToxFileInfo info)
            : base(tox, stream, info)
        {
        }
    }
}

