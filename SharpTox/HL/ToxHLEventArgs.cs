using System;

namespace SharpTox.HL
{
    public static class ToxHLEventArgs
    {
        public class FileSendRequestEventArgs : EventArgs
        {
            public ToxIncomingTransfer Transfer { get; private set; }

            public FileSendRequestEventArgs(ToxIncomingTransfer transfer)
            {
                Transfer = transfer;
            }
        }

        public class FileStateEventArgs : EventArgs
        {
            public ToxTransferState State { get; private set; }

            public FileStateEventArgs(ToxTransferState state)
            {
                State = state;
            }
        }
    }
}

