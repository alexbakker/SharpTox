using System.IO;
using SharpTox.Core;

namespace SharpTox.HL.Transfers
{
    public class ToxTransferResumeData
    {
        public int FriendNumber { get; set; }
        public ToxFileInfo Info { get; set; }
        public string Name { get; set; }
        public ToxFileKind Kind { get; set; }
        public long TransferredBytes { get; set; }
        public Stream Stream { get; set; }
        public ToxTransferDirection Direction { get; set; }
    }
}