using System;
using SharpTox.Core;

namespace SharpTox.HL.Transfers
{
    [Serializable]
    public class ToxTransferResumeData
    {
        public int FriendNumber { get; set; }
        public ToxFileInfo Info { get; set; }
        public string Name { get; set; }
        public ToxFileKind Kind { get; set; }
        public long TransferredBytes { get; set; }
        public ToxTransferDirection Direction { get; set; }
    }
}