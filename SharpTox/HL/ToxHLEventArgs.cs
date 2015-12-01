using System;

namespace SharpTox.HL
{
    public class ToxHLEventArgs
    {
        public class FileSendRequestEventArgs : EventArgs
        {
            public ToxIncomingTransfer Transfer { get; private set; }

            public FileSendRequestEventArgs(ToxIncomingTransfer transfer)
            {
                Transfer = transfer;
            }
        }
    }
}

