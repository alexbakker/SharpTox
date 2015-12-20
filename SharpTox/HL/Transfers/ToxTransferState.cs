using System;

namespace SharpTox.HL.Transfers
{
    public enum ToxTransferState
    {
        Pending,
        Paused,
        InProgress,
        Finished,
        Canceled
    }
}
