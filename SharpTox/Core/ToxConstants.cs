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
        public const int MaxMessageLength = 1372;

        /// <summary>
        /// The maximum status message length in bytes.
        /// </summary>
        public const int MaxStatusMessageLength = 1007;

        /// <summary>
        /// The maximum friend request message length in bytes.
        /// </summary>
        public const int MaxFriendRequestLenght = 1016;

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
        public const int HashLength = 32;

        /// <summary>
        /// The size of a public key.
        /// </summary>
        public const int PublicKeySize = 32;

        /// <summary>
        /// The size of a secret key.
        /// </summary>
        public const int SecretKeySize = 32;

        /// <summary>
        /// The size of an address.
        /// </summary>
        public const int AddressSize = PublicKeySize + sizeof(uint) + sizeof(ushort);

        /// <summary>
        /// Length of a file identifier.
        /// </summary>
        public const int FileIdLength = 32;

        /// <summary>
        /// The maximum length of a filename.
        /// </summary>
        public const int MaxFileNameLength = 255;

        //Constants for the the tox data file
        internal const uint Cookie = 0x15ed1b1f;
        internal const uint CookieInner = 0x01ce;
    }
}
