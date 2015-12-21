using System;

namespace SharpTox.HL.Transfers
{
    public static class ToxTransferEventArgs
    {
        public class RequestEventArgs : EventArgs
        {
            public ToxIncomingTransfer Transfer { get; private set; }

            public RequestEventArgs(ToxIncomingTransfer transfer)
            {
                Transfer = transfer;
            }
        }

        public class StateEventArgs : EventArgs
        {
            public ToxTransferState State { get; private set; }

            public StateEventArgs(ToxTransferState state)
            {
                State = state;
            }
        }

        public class ErrorEventArgs : EventArgs
        {
            public ToxTransferError Error { get; private set; }

            public ErrorEventArgs(ToxTransferError error)
            {
                Error = error;
            }
        }

        public class ProgressEventArgs : EventArgs
        {
            public float Progress { get; private set; }

            public ProgressEventArgs(float progress)
            {
                Progress = progress;
            }
        }

        public class SpeedEventArgs : EventArgs
        {
            public float Speed { get; private set; }

            public SpeedEventArgs(float speed)
            {
                Speed = speed;
            }
        }
    }
}
