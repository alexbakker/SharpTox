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

        public static bool operator ==(ToxKeyPair pair1, ToxKeyPair pair2)
        {
            if (object.ReferenceEquals(pair1, pair2))
                return true;

            if ((object)pair1 == null ^ (object)pair2 == null)
                return false;

            return (pair1.PublicKey == pair2.PublicKey && pair1.SecretKey == pair2.SecretKey);
        }

        public static bool operator !=(ToxKeyPair pair1, ToxKeyPair pair2)
        {
            return !(pair1 == pair2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            ToxKeyPair pair = obj as ToxKeyPair;
            if ((object)pair == null)
                return false;

            return this == pair;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
