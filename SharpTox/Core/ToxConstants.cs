namespace SharpTox.Core
{
    /// <summary>
    /// Represents a collection of tox constants.
    /// </summary>
    public static class ToxConstants
    {
        /// <summary>
        /// The maximum message length in bytes.
        /// </summary>
        public const int MaxMessageLength = 1368;

        /// <summary>
        /// The maximum status message length in bytes.
        /// </summary>
        public const int MaxStatusMessageLength = 1007;

        /// <summary>
        /// The maximum name length in bytes.
        /// </summary>
        public const int MaxNameLength = 128;

        /// <summary>
        /// The maximum length of a custom packet.
        /// </summary>
        public const int MaxCustomPacketSize = 1373;

        /// <summary>
        /// The exact length of the hash of an avatar in bytes.
        /// </summary>
        public const int ToxHashLength = 32;

        /// <summary>
        /// The maximum size of an avatar in bytes.
        /// </summary>
        public const int MaxAvatarDataLength = 16384;

        /// <summary>
        /// The size of a public key.
        /// </summary>
        public const int PublicKeySize = 32;
    }
}
