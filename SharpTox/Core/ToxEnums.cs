namespace SharpTox.Core
{
    /// <summary>
    /// Errors that can occur when creating a new instance of Tox.
    /// </summary>
    public enum ToxErrorNew
    {
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,

        /// <summary>
        /// One of the arguments to the function was NULL when it was not expected.
        /// </summary>
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
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,

        /// <summary>
        /// One of the arguments to the function was NULL when it was not expected.
        /// </summary>
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
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,

        /// <summary>
        /// No friend with the given number exists on the friend list.
        /// </summary>
        NotFound
    }

    /// <summary>
    /// Errors that can occur when trying to find a friend by their public key.
    /// </summary>
    public enum ToxErrorFriendByPublicKey
    {
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,

        /// <summary>
        /// One of the arguments to the function was NULL when it was not expected.
        /// </summary>
        Null,

        /// <summary>
        /// No friend with the given public key exists on the friend list.
        /// </summary>
        NotFound
    }

    /// <summary>
    /// Errors that can occur when querying for information about a friend.
    /// </summary>
    public enum ToxErrorFriendQuery
    {
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,

        /// <summary>
        /// One of the arguments to the function was NULL when it was not expected.
        /// </summary>
        Null,

        /// <summary>
        /// The friend number did not designate a valid friend.
        /// </summary>
        NotFound
    }

    /// <summary>
    /// Errors that can occur when trying to delete a friend.
    /// </summary>
    public enum ToxErrorFriendDelete
    {
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,

        /// <summary>
        /// There was no friend with the given friend number. No friends were deleted.
        /// </summary>
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
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,

        /// <summary>
        /// One of the arguments to the function was NULL when it was not expected.
        /// </summary>
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
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,

        /// <summary>
        /// One of the arguments to the function was NULL when it was not expected.
        /// </summary>
        Null,

        /// <summary>
        /// The length of the friend request message was too long.
        /// </summary>
        TooLong,

        /// <summary>
        /// The friend request message was empty.
        /// </summary>
        NoMessage,

        /// <summary>
        /// The specified address is equal to our own.
        /// </summary>
        OwnKey,

        /// <summary>
        /// A friend request has already been sent to this address or the address belongs to a friend that is already in our friend list.
        /// </summary>
        AlreadySent,

        /// <summary>
        /// The address checksum failed.
        /// </summary>
        BadChecksum,

        /// <summary>
        /// The friend was already in the list, but the nospam value was different.
        /// </summary>
        SetNewNospam,

        /// <summary>
        /// A memory allocation failed when trying to increase the friend list size.
        /// </summary>
        Malloc
    }

    /// <summary>
    /// Errors that can occur when trying to set typing status.
    /// </summary>
    public enum ToxErrorSetTyping
    {
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,

        /// <summary>
        /// The friend number did not designate a valid friend.
        /// </summary>
        NotFound
    }

    /// <summary>
    /// Errors that can occur when changing our name or status message.
    /// </summary>
    public enum ToxErrorSetInfo
    {
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,

        /// <summary>
        /// One of the arguments to the function was NULL when it was not expected.
        /// </summary>
        Null,

        /// <summary>
        /// The friend number did not designate a valid friend.
        /// </summary>
        TooLong
    }

    internal enum ToxErrorOptionsNew
    {
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,

        /// <summary>
        /// The function failed to allocate enough memory for the options struct.
        /// </summary>
        Malloc
    }

    /// <summary>
    /// Errors that can occur when trying to retrieve the port an instance of Tox is bound to.
    /// </summary>
    public enum ToxErrorGetPort
    {
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,

        /// <summary>
        /// The instance was not bound to any port.
        /// </summary>
        NotBound
    }

    /// <summary>
    /// Tox key type.
    /// </summary>
    public enum ToxKeyType
    {
        /// <summary>
        /// A public key.
        /// </summary>
        Public,

        /// <summary>
        /// A secret key.
        /// </summary>
        Secret
    }

    /// <summary>
    /// The different kinds of proxies.
    /// </summary>
    public enum ToxProxyType
    {
        /// <summary>
        /// Don't use a proxy.
        /// </summary>
        None,

        /// <summary>
        /// HTTP proxy.
        /// </summary>
        Http,

        /// <summary>
        /// Socks5 proxy.
        /// </summary>
        Socks5
    }

    /// <summary>
    /// The different kinds of file controls that can be sent.
    /// </summary>
    public enum ToxFileControl
    {
        /// <summary>
        /// A control used to resume a paused file transfer. Also used to accept a file transfer request.
        /// </summary>
        Resume,

        /// <summary>
        /// A control used to pause a file transfer.
        /// </summary>
        Pause,

        /// <summary>
        /// A control used to kill a file transfer. Also used to decline a file transfer request.
        /// </summary>
        Cancel
    }

    /// <summary>
    /// The different kinds of files that can be transferred.
    /// </summary>
    public enum ToxFileKind
    {
        /// <summary>
        /// Arbitrary file data.
        /// </summary>
        Data,

        /// <summary>
        /// An avatar.
        /// </summary>
        Avatar
    }

    /// <summary>
    /// The different kinds of messages that can be sent.
    /// </summary>
    public enum ToxMessageType
    {
        /// <summary>
        /// A regular message.
        /// </summary>
        Message,

        /// <summary>
        /// An IRC-like action.
        /// </summary>
        Action
    }

    /// <summary>
    /// Errors that can occur when trying to send a file control.
    /// </summary>
    public enum ToxErrorFileControl
    {
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,
        
        /// <summary>
        /// The friend number passed did not designate a valid friend.
        /// </summary>
        FriendNotFound,

        /// <summary>
        /// This client is currently not connected to the friend.
        /// </summary>
        FriendNotConnected,

        /// <summary>
        /// No file transfer with the given file number was found for the given friend.
        /// </summary>
        NotFound,

        /// <summary>
        /// A RESUME control was sent, but the file transfer is running normally.
        /// </summary>
        NotPaused,

        /// <summary>
        /// A RESUME control was sent, but the file transfer was paused by the other party. Only the party that paused the transfer can resume it.
        /// </summary>
        Denied,

        /// <summary>
        /// A PAUSE control was sent, but the file transfer was already paused.
        /// </summary>
        AlreadyPaused,

        /// <summary>
        /// Packet queue is full.
        /// </summary>
        SendQ
    }

    /// <summary>
    /// Errors that can occur when trying to send a file transfer request.
    /// </summary>
    public enum ToxErrorFileSend
    {
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,

        /// <summary>
        /// One of the arguments to the function was NULL when it was not expected.
        /// </summary>
        Null,

        /// <summary>
        /// The friend number passed did not designate a valid friend.
        /// </summary>
        FriendNotFound,

        /// <summary>
        /// This client is currently not connected to the friend.
        /// </summary>
        FriendNotConnected,

        /// <summary>
        /// Filename length exceeded TOX_MAX_FILENAME_LENGTH bytes.
        /// </summary>
        NameTooLong,

        /// <summary>
        /// Too many ongoing transfers. The maximum number of concurrent file transfers is 256 per friend per direction (sending and receiving).
        /// </summary>
        TooMany
    }

    /// <summary>
    /// Errors that can occur when trying to send a chunk of a file.
    /// </summary>
    public enum ToxErrorFileSendChunk
    {
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,

        /// <summary>
        /// One of the arguments to the function was NULL when it was not expected.
        /// </summary>
        Null,

        /// <summary>
        /// The friend number passed did not designate a valid friend.
        /// </summary>
        FriendNotFound,

        /// <summary>
        /// This client is currently not connected to the friend.
        /// </summary>
        FriendNotConnected,

        /// <summary>
        /// No file transfer with the given file number was found for the given friend.
        /// </summary>
        NotFound,

        /// <summary>
        /// File transfer was found but isn't in a transferring state: (paused, done, broken, etc...) (happens only when not called from the request chunk callback).
        /// </summary>
        NotTransferring,

        /// <summary>
        /// Attempted to send more or less data than requested.
        /// </summary>
        InvalidLength,

        /// <summary>
        /// Packet queue is full.
        /// </summary>
        SendQ,

        /// <summary>
        /// Position parameter was wrong.
        /// </summary>
        WrongPosition
    }

    /// <summary>
    /// Errors that can occur when trying retrieve a file transfer by it's unique ID.
    /// </summary>
    public enum ToxErrorFileGet
    {
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,

        /// <summary>
        /// The friend number passed did not designate a valid friend.
        /// </summary>
        FriendNotFound,

        /// <summary>
        /// No file transfer with the given file number was found for the given friend.
        /// </summary>
        NotFound
    }

    /// <summary>
    /// Errors that can occur when trying to send a file seek command.
    /// </summary>
    public enum ToxErrorFileSeek
    {
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,
        
        /// <summary>
        /// The friend number passed did not designate a valid friend.
        /// </summary>
        FriendNotFound,

        /// <summary>
        /// This client is currently not connected to the friend.
        /// </summary>
        FriendNotConnected,

        /// <summary>
        /// No file transfer with the given file number was found for the given friend.
        /// </summary>
        NotFound,

        /// <summary>
        /// File was not in a state where it could be seeked.
        /// </summary>
        SeekDenied,

        /// <summary>
        /// Seek position was invalid.
        /// </summary>
        InvalidPosition,

        /// <summary>
        /// Packet queue is full.
        /// </summary>
        SendQ
    }

    /// <summary>
    /// Errors that can occur when trying to send a custom packet.
    /// </summary>
    public enum ToxErrorFriendCustomPacket
    {
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,

        /// <summary>
        /// One of the arguments to the function was NULL when it was not expected.
        /// </summary>
        Null,

        /// <summary>
        /// The friend number did not designate a valid friend.
        /// </summary>
        FriendNotNull,

        /// <summary>
        /// This client is currently not connected to the friend.
        /// </summary>
        FriendNotConnected,

        /// <summary>
        /// The first byte of data was not in the specified range for the packet type. This range is 200-254 for lossy, and 160-191 for lossless packets.
        /// </summary>
        Invalid,

        /// <summary>
        /// Attempted to send an empty packet.
        /// </summary>
        Empty,

        /// <summary>
        /// Packet data length exceeded TOX_MAX_CUSTOM_PACKET_SIZE.
        /// </summary>
        TooLong,

        /// <summary>
        /// Packet queue is full.
        /// </summary>
        SendQ
    }

    /// <summary>
    /// Errors that can occur when trying to retrieve the 'last online' of a friend.
    /// </summary>
    public enum ToxErrorFriendGetLastOnline
    {
        /// <summary>
        /// The function returned successfully.
        /// </summary>
        Ok,

        /// <summary>
        ///  No friend with the given number exists on the friend list.
        /// </summary>
        NotFound
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
    /// All of the different group types.
    /// </summary>
    public enum ToxGroupType
    {
        /// <summary>
        /// Text only, no audio in this groupchat.
        /// </summary>
        Text,

        /// <summary>
        /// Both text and audio in this groupchat.
        /// </summary>
        Av
    }

    internal enum ToxSaveDataType
    {
        None,
        ToxSave,
        SecretKey
    }
}
