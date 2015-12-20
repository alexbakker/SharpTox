using System;
using System.Linq;

namespace SharpTox.Core
{
    /// <summary>
    /// Represents a Tox ID (38 bytes long)
    /// </summary>
    public class ToxId
    {
        private byte[] _id;

        /// <summary>
        /// Retrieves the public key of this Tox ID.
        /// </summary>
        public ToxKey PublicKey
        {
            get
            {
                byte[] key = new byte[ToxConstants.PublicKeySize];
                Array.Copy(_id, 0, key, 0, ToxConstants.PublicKeySize);

                return new ToxKey(ToxKeyType.Public, key);
            }
        }

        /// <summary>
        /// Retrieves the Tox ID, represented in an array of bytes.
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Retrieves the nospam value of this Tox ID.
        /// </summary>
        public int Nospam
        {
            get
            {
                byte[] nospam = new byte[sizeof(uint)];
                Array.Copy(_id, ToxConstants.PublicKeySize, nospam, 0, sizeof(uint));

                return BitConverter.ToInt32(nospam, 0);
            }
        }

        /// <summary>
        /// Retrieves the checksum of this Tox ID.
        /// </summary>
        public short Checksum
        {
            get
            {
                byte[] checksum = new byte[sizeof(ushort)];
                Array.Copy(_id, ToxConstants.PublicKeySize + sizeof(uint), checksum, 0, sizeof(ushort));

                return BitConverter.ToInt16(checksum, 0);
            }
        }

        /// <summary>
        /// Initializes a new instance of the ToxId class.
        /// </summary>
        /// <param name="id">A (ToxConstant.AddressSize * 2) character long hexadecimal string, containing a Tox ID.</param>
        public ToxId(string id)
            : this(ToxTools.StringToHexBin(id)) { }

        /// <summary>
        /// Initializes a new instance of the ToxId class.
        /// </summary>
        /// <param name="id">A byte array with a length of ToxConstant.AddressSize, containing a Tox ID.</param>
        public ToxId(byte[] id)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            _id = id;

            if (CalcChecksum(_id, ToxConstants.PublicKeySize + sizeof(uint)) != unchecked((ushort)Checksum))
                throw new Exception("This Tox ID is invalid");
        }

        /// <summary>
        /// Creates a new tox id with the specified public key and nospam.
        /// </summary>
        /// <param name="publicKey">Public key to create this Tox ID with.</param>
        /// <param name="nospam">Nospam value to create this Tox ID with.</param>
        public ToxId(byte[] publicKey, int nospam)
        {
            if (publicKey == null)
                throw new ArgumentNullException("publicKey");

            byte[] id = new byte[ToxConstants.AddressSize];

            Array.Copy(publicKey, 0, id, 0, ToxConstants.PublicKeySize);
            Array.Copy(BitConverter.GetBytes(nospam), 0, id, ToxConstants.PublicKeySize, sizeof(uint));

            ushort checksum = CalcChecksum(id, ToxConstants.PublicKeySize + sizeof(uint));
            Array.Copy(BitConverter.GetBytes(checksum), 0, id, ToxConstants.PublicKeySize + sizeof(uint), sizeof(ushort));

            _id = id;
        }

        /// <summary>
        /// Creates a new tox id with the specified public key and nospam.
        /// </summary>
        /// <param name="publicKey">Public key to create this Tox ID with.</param>
        /// <param name="nospam">Nospam value to create this Tox ID with.</param>
        public ToxId(string publicKey, int nospam)
            : this(ToxTools.StringToHexBin(publicKey), nospam) { }

        public static bool operator ==(ToxId id1, ToxId id2)
        {
            if (object.ReferenceEquals(id1, id2))
                return true;

            if ((object)id1 == null ^ (object)id2 == null)
                return false;

            return (id1._id.SequenceEqual(id2._id));
        }

        public static bool operator !=(ToxId id1, ToxId id2)
        {
            return !(id1 == id2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            ToxId id = obj as ToxId;
            if ((object)id == null)
                return false;

            return this == id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return ToxTools.HexBinToString(_id);
        }

        /// <summary>
        /// Checks whether or not the given Tox ID is valid.
        /// </summary>
        /// <param name="id">A (ToxConstant.AddressSize * 2) character long hexadecimal string, containing a Tox ID.</param>
        /// <returns>True if the ID is valid, false if the ID is invalid.</returns>
        public static bool IsValid(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            byte[] bytes = null;

            try { bytes = ToxTools.StringToHexBin(id); }
            catch { return false; }

            return IsValid(bytes);
        }

        /// <summary>
        /// Checks whether or not the given Tox ID is valid.
        /// </summary>
        /// <param name="id">A byte array with a length of ToxConstant.AddressSize, containing a Tox ID.</param>
        /// <returns>True if the ID is valid, false if the ID is invalid.</returns>
        public static bool IsValid(byte[] id)
        {
            if (id == null)
                return false;

            try
            {
                byte[] checksum = new byte[sizeof(ushort)];
                int index = ToxConstants.PublicKeySize + sizeof(uint);
                ushort check;

                Array.Copy(id, index, checksum, 0, sizeof(ushort));
                check = BitConverter.ToUInt16(checksum, 0);

                return CalcChecksum(id, index) == check;
            }
            catch
            {
                return false;
            }
        }

        private static ushort CalcChecksum(byte[] address, int length)
        {
            byte[] checksum = new byte[sizeof(ushort)];

            for (uint i = 0; i < length; i++)
                checksum[i % 2] ^= address[i];

            return BitConverter.ToUInt16(checksum, 0);
        }
    }
}
