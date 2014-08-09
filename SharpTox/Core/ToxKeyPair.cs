namespace SharpTox.Core
{
    public class ToxKeyPair
    {
        public ToxKey PublicKey { get; private set; }
        public ToxKey SecretKey { get; private set; }

        internal ToxKeyPair(ToxKey public_key, ToxKey secret_key)
        {
            PublicKey = public_key;
            SecretKey = secret_key;
        }
    }
}
