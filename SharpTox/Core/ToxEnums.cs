namespace SharpTox.Core
{
    public enum ToxErrorNew
    {
        Ok,
        Null,
        Malloc,
        PortAlloc,
        ProxyBadType,
        ProxyBadHost,
        ProxyBadPort,
        ProxyNotFound,
        LoadEncrypted,
        LoadDecryptionFailed,
        LoadBadFormat
    }

    public enum ToxErrorBootstrap
    {
        Ok,
        Null,
        BadHost,
        BadPort
    }

    public enum ToxConnectionStatus
    {
        None,
        Tcp,
        Udp
    }

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

    public enum ToxErrorSendMessage
    {
        Ok,
        Null,
        FriendNotFound,
        FriendNotConnected,
        SendQ,
        TooLong,
        Empty
    }

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
}
