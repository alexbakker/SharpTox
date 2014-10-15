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
        /// <param name="publicKey"></param>
        /// <param name="secretKey"></param>
        internal ToxKeyPair(ToxKey publicKey, ToxKey secretKey)
        {
            PublicKey = publicKey;
            SecretKey = secretKey;
        }
    }
}
