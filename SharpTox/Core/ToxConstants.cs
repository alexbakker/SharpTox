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
    }
}
