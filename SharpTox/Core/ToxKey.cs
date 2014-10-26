using System.Linq;

namespace SharpTox.Core
{
    /// <summary>
    /// Represents a 32 byte long tox key (either public or secret).
    /// </summary>
    public class ToxKey
    {
        private byte[] _key;

        /// <summary>
        /// The key type (either public or secret).
        /// </summary>
        public ToxKeyType KeyType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToxKey"/> class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        public ToxKey(ToxKeyType type, byte[] key)
        {
            KeyType = type;
            _key = key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToxKey"/> class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        public ToxKey(ToxKeyType type, string key)
        {
            KeyType = type;
            _key = ToxTools.StringToHexBin(key);
        }

        /// <summary>
        /// Retrieves a byte array of the tox key.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            return _key;
        }

        /// <summary>
        /// Retrieves a string of the tox key.
        /// </summary>
        /// <returns></returns>
        public string GetString()
        {
            return ToxTools.HexBinToString(_key);
        }

        public static bool operator ==(ToxKey key1, ToxKey key2)
        {
            if (object.ReferenceEquals(key1, key2))
                return true;

            if ((object)key1 == null ^ (object)key2 == null)
                return false;

            return (key1._key.SequenceEqual(key2._key) && key1.KeyType == key2.KeyType);
        }

        public static bool operator !=(ToxKey key1, ToxKey key2)
        {
            return !(key1 == key2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            ToxKey key = obj as ToxKey;
            if ((object)key == null)
                return false;

            return this == key;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return ToxTools.HexBinToString(_key);
        }
    }
}
