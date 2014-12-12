using System.Linq;

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

        public static bool operator ==(ToxAvatar avatar1, ToxAvatar avatar2)
        {
            if (object.ReferenceEquals(avatar1, avatar2))
                return true;

            if ((object)avatar1 == null ^ (object)avatar2 == null)
                return false;

            return avatar1.Hash.SequenceEqual(avatar2.Hash);
        }

        public static bool operator !=(ToxAvatar avatar1, ToxAvatar avatar2)
        {
            return !(avatar1.Hash == avatar2.Hash);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            ToxAvatar avatar = obj as ToxAvatar;
            if ((object)avatar == null)
                return false;

            return this == avatar;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
