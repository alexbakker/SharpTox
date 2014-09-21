namespace SharpTox.Core
{
    /// <summary>
    /// Represents a Tox avatar.
    /// </summary>
    public class ToxAvatar
    {
        /// <summary>
        /// The format of this avatar.
        /// </summary>
        public ToxAvatarFormat Format { get; private set; }

        /// <summary>
        /// The image data of this avatar.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// The hash of this avatar. Used for verification.
        /// </summary>
        public byte[] Hash { get; private set; }

        internal ToxAvatar(ToxAvatarFormat format, byte[] data, byte[] hash)
        {
            Format = format;
            Data = data;
            Hash = hash;
        }
    }
}
