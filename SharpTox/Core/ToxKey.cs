namespace SharpTox.Core
{
    public class ToxKey
    {
        private byte[] key;
        public ToxKeyType KeyType { get; private set; }

        public ToxKey(ToxKeyType type, byte[] key)
        {
            KeyType = type;
            this.key = key;
        }

        public ToxKey(ToxKeyType type, string key)
        {
            KeyType = type;
            this.key = ToxTools.StringToHexBin(key);
        }

        public byte[] GetBytes()
        {
            return key;
        }

        public string GetString()
        {
            return ToxTools.HexBinToString(key);
        }
    }
}
