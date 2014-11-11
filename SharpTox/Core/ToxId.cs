using System;

namespace SharpTox.Core
{
    public class ToxId
    {
        private byte[] _id;

        public ToxKey PublicKey
        {
            get
            {
                return new ToxKey(ToxKeyType.Public, _id);
            }
        }

        public byte[] Bytes
        {
            get
            {
                return _id;
            }
        }

        public int Nospam
        {
            get
            {
                byte[] nospam = new byte[4];
                Array.Copy(_id, 32, nospam, 0, 4);

                return BitConverter.ToInt32(nospam, 0);
            }
        }

        public int Checksum
        {
            get
            {
                byte[] checksum = new byte[2];
                Array.Copy(_id, 36, checksum, 0, 2);

                return BitConverter.ToInt32(checksum, 0);
            }
        }

        public ToxId(string id)
            : this(ToxTools.StringToHexBin(id))
        {
        }

        public ToxId(byte[] id)
        {
            _id = (byte[])id.Clone();

            if ((BitConverter.ToUInt32(PublicKey.GetBytes(), 0) ^ Nospam) != Checksum)
                throw new Exception("This Tox ID is invalid");
        }

        public override string ToString()
        {
            return ToxTools.HexBinToString(_id);
        }
    }
}
