using System;

namespace SharpTox.HL.Transfers
{
    public enum ToxTransferState
    {
        Pending,
        PausedByUser,
        PausedByFriend,
        InProgress,
        Finished,
        Canceled,
        Broken
    }
}
