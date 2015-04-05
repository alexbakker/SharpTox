namespace SharpTox.Core
{
    /// <summary>
    /// Errors that can occur when creating a new instance of Tox.
    /// </summary>
    public enum ToxErrorNew
    {
        Ok,
        Null,

        /// <summary>
        /// Failed to allocate enough memory.
        /// </summary>
        Malloc,

        /// <summary>
        /// Failed to bind to a port. This could mean that all ports have already been bound.
        /// </summary>
        PortAlloc,

        /// <summary>
        /// Specified proxy type is invalid.
        /// </summary>
        ProxyBadType,

        /// <summary>
        /// Specified host is invalid.
        /// </summary>
        ProxyBadHost,

        /// <summary>
        /// Specified port is invalid.
        /// </summary>
        ProxyBadPort,

        /// <summary>
        /// Specified host could not be resolved.
        /// </summary>
        ProxyNotFound,

        /// <summary>
        /// The specified byte array to be loaded contained encrypted data.
        /// </summary>
        LoadEncrypted,

        /// <summary>
        /// The specified byte array contains (partially) invalid data.
        /// </summary>
        LoadBadFormat
    }

    /// <summary>
    /// Errors that can occur when calling Bootstrap.
    /// </summary>
    public enum ToxErrorBootstrap
    {
        Ok,
        Null,

        /// <summary>
        /// The specified host could not be resolved to and IP address or the specified IP address is invalid.
        /// </summary>
        BadHost,

        /// <summary>
        /// The specified port is invalid.
        /// </summary>
        BadPort
    }

    /// <summary>
    /// Tox connection status.
    /// </summary>
    public enum ToxConnectionStatus
    {
        /// <summary>
        /// No connection established.
        /// </summary>
        None,

        /// <summary>
        /// A TCP connection has been established.
        /// </summary>
        Tcp,

        /// <summary>
        /// A UDP connection has been established.
        /// </summary>
        Udp
    }

    /// <summary>
    /// Errors that can occur when trying to retrieve a friend's public key.
    /// </summary>
    public enum ToxErrorFriendGetPublicKey
    {
        Ok,
        NotFound
    }

    public enum ToxErrorFriendByPublicKey
    {
        Ok,
        Null,
        NotFound
    }

    public enum ToxErrorFriendQuery
    {
        Ok,
        Null,
        NotFound
    }

    public enum ToxErrorFriendDelete
    {
        Ok,
        NotFound
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
    }

    /// <summary>
    /// Errors that can occur when trying to send a message to a friend.
    /// </summary>
    public enum ToxErrorSendMessage
    {
        Ok,
        Null,

        /// <summary>
        /// The specified friend number could not be found in the friend list.
        /// </summary>
        FriendNotFound,

        /// <summary>
        /// We're not connected to the specified friend.
        /// </summary>
        FriendNotConnected,

        /// <summary>
        /// An allocation error occurred while increasing the send queue size.
        /// </summary>
        SendQ,

        /// <summary>
        /// The specified message exceeded <see cref="ToxConstants.MaxMessageLength"/>.
        /// </summary>
        TooLong,

        /// <summary>
        /// The specified message is empty.
        /// </summary>
        Empty
    }

    /// <summary>
    /// Errrors that can occur when trying to add a friend.
    /// </summary>
    public enum ToxErrorFriendAdd
    {
        Ok,
        Null,
        TooLong,
        NoMessage,
        OwnKey,
        AlreadySent,
        BadChecksum,
        SetNewNospam,
        Malloc
    }

    public enum ToxErrorSetTyping
    {
        Ok,
        NotFound
    }

    public enum ToxErrorSetInfo
    {
        Ok,
        Null,
        TooLong
    }

    public enum ToxErrorOptionsNew
    {
        Ok,
        Malloc
    }

    public enum ToxErrorGetPort
    {
        Ok,
        NotBound
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

    public enum ToxProxyType
    {
        None,
        Http,
        Socks5
    }

    public enum ToxFileControl
    {
        Resume,
        Pause,
        Cancel
    }

    public enum ToxFileKind
    {
        Data,
        Avatar
    }

    public enum ToxMessageType
    {
        Message,
        Action
    }

    public enum ToxErrorFileControl
    {
        Ok,
        FriendNotFound,
        FriendNotConnected,
        NotFound,
        NotPaused,
        Denied,
        AlreadyPaused,
        SendQ
    }

    public enum ToxErrorFileSend
    {
        Ok,
        Null,
        FriendNotFound,
        FriendNotConnected,
        NameTooLong,
        TooMany
    }

    public enum ToxErrorFileSendChunk
    {
        Ok,
        Null,
        FriendNotFound,
        FriendNotConnected,
        NotFound,
        NotTransferring,
        InvalidLength,
        SendQ
    }

    public enum ToxErrorFileGet
    {
        Ok,
        FriendNotFound,
        NotFound
    }

    public enum ToxErrorFileSeek
    {
        Ok,
        FriendNotFound,
        FriendNotConnected,
        NotFound,
        SeekDenied,
        InvalidPosition,
        SendQ
    }

    public enum ToxErrorFriendCustomPacket
    {
        Ok,
        Null,
        FriendNotNull,
        FriendNotConnected,
        Invalid,
        Empty,
        TooLong,
        SendQ
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

    public enum ToxGroupType
    {
        Text,
        Av
    }
}
