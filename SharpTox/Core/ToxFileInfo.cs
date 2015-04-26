using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpTox.Core
{
    /// <summary>
    /// Represent information about a file transfer.
    /// </summary>
    public class ToxFileInfo
    {
        /// <summary>
        /// The number of this file transfer.
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// The unique ID if this file transfer. This can be used to resume file transfer across restarts.
        /// </summary>
        public byte[] Id { get; private set; }

        internal ToxFileInfo(int number, byte[] id)
        {
            Number = number;
            Id = id;
        }
    }
}
