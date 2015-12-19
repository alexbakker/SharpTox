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

        public class TransferStateEventArgs : EventArgs
        {
            public ToxTransferState State { get; private set; }

            public TransferStateEventArgs(ToxTransferState state)
            {
                State = state;
            }
        }

        public class TransferErrorEventArgs : EventArgs
        {
            public ToxFileTransferError Error { get; private set; }

            public TransferErrorEventArgs(ToxFileTransferError error)
            {
                Error = error;
            }
        }
    }
}
