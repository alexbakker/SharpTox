namespace SharpTox.Core
{
    /// <summary>
    /// Errors while adding a friend.
    /// </summary>
    public enum ToxAFError
    {
        /// <summary>
        /// Message length is too long.
        /// </summary>
        TooLong = -1,

        /// <summary>
        /// No message.
        /// </summary>
        NoMessage = -2,

        /// <summary>
        /// Trying to add our own key.
        /// </summary>
        OwnKey = -3,

        /// <summary>
        /// Friend already added or friend request already sent.
        /// </summary>
        AlreadySent = -4,

        /// <summary>
        /// Unknown error.
        /// </summary>
        Unknown = -5,

        /// <summary>
        /// Bad checksum in the address.
        /// </summary>
        BadChecksum = -6,

        /// <summary>
        /// Friend was already added but the nospam was different.
        /// </summary>
        SetNewNospam = -7,

        /// <summary>
        /// Increasing the friend list size failed.
        /// </summary>
        NoMem = -8
    }

    /// <summary>
    /// UserStatus of a tox friend.
    /// </summary>
    public enum ToxUserStatus
    {
        /// <summary>
        /// No status.
        /// </summary>
        None,

        /// <summary>
        /// Away.
        /// </summary>
        Away,

        /// <summary>
        /// Busy.
        /// </summary>
        Busy,

        /// <summary>
        /// Invalid status.
        /// </summary>
        Invalid
    }

    /// <summary>
    /// Connection status of a friend.
    /// </summary>
    public enum ToxFriendConnectionStatus
    {
        /// <summary>
        /// Not connected.
        /// </summary>
        Offline,

        /// <summary>
        /// Connected.
        /// </summary>
        Online
    }

    /// <summary>
    /// Controls to be used for file transfers.
    /// </summary>
    public enum ToxFileControl
    {
        /// <summary>
        /// Accept a file send request.
        /// </summary>
        Accept,

        /// <summary>
        /// Pause a file transfer.
        /// </summary>
        Pause,

        /// <summary>
        /// Kill a file transfer.
        /// </summary>
        Kill,

        /// <summary>
        /// Fully received/sent a file.
        /// </summary>
        Finished,

        /// <summary>
        /// Resume a paused file transfer.
        /// </summary>
        ResumeBroken
    }

    /// <summary>
    /// Changes that can occur in a group chat.
    /// </summary>
    public enum ToxChatChange
    {
        /// <summary>
        /// A new peer joined the group.
        /// </summary>
        PeerAdd,

        /// <summary>
        /// A peer left the group.
        /// </summary>
        PeerDel,

        /// <summary>
        /// A peer changed its name.
        /// </summary>
        PeerName
    }

    /// <summary>
    /// Tox key type.
    /// </summary>
    public enum ToxKeyType
    {
        /// <summary>
        /// A public tox key.
        /// </summary>
        Public,

        /// <summary>
        /// A secret tox key.
        /// </summary>
        Secret
    }

    /// <summary>
    /// Avatar formats.
    /// </summary>
    public enum ToxAvatarFormat
    {
        /// <summary>
        /// No avatar.
        /// </summary>
        None,

        /// <summary>
        /// PNG format.
        /// </summary>
        Png,
    }

    public enum ToxGroupType
    {
        Text,
        Av
    }

    public enum ToxFileSenderType : byte
    {
        Send = 0,
        Receive = 1
    }

    public enum ToxProxyType : byte
    {
        None,
        Socks5,
        Http
    }
}
