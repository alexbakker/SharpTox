namespace SharpTox.Core
{
    /// <summary>
    /// Represents a pair of tox keys.
    /// </summary>
    public class ToxKeyPair
    {
        /// <summary>
        /// The public key.
        /// </summary>
        public ToxKey PublicKey { get; private set; }

        /// <summary>
        /// The secret key (the one that should not be shared with anyone).
        /// </summary>
        public ToxKey SecretKey { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToxKeyPair"/> class.
        /// </summary>
        /// <param name="public_key"></param>
        /// <param name="secret_key"></param>
        internal ToxKeyPair(ToxKey public_key, ToxKey secret_key)
        {
            PublicKey = public_key;
            SecretKey = secret_key;
        }
    }
}
