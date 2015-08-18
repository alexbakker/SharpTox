using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpTox.Core
{
    /// <summary>
    /// A collection of constants for groupchats.
    /// </summary>
    public static class ToxGroupConstants
    {
        /// <summary>
        /// Maximum length of a group topic.
        /// </summary>
        public const int MaxTopicLength = 512;

        /// <summary>
        /// Maximum length of a peer part message.
        /// </summary>
        public const int MaxPartLength = 128;

        /// <summary>
        /// Maximum length of a group name.
        /// </summary>
        public const int MaxGroupNameLength = 48;

        /// <summary>
        /// Maximum length of a group password.
        /// </summary>
        public const int MaxPasswordSize = 32;

        /// <summary>
        /// Number of bytes in a group chat ID.
        /// </summary>
        public const int GroupChatIdSize = 32;
    }
}
