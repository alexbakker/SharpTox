using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpTox
{
    public class ToxFileInfo
    {
        public int Number { get; private set; }
        public byte[] Id { get; private set; }

        internal ToxFileInfo(int number, byte[] id)
        {
            Number = number;
            Id = id;
        }
    }
}
