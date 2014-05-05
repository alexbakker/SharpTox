namespace SharpTox
{
    public enum ToxAFError
    {
        TOOLONG = -1,
        NOMESSAGE = -2,
        OWNKEY = -3,
        ALREADYSENT = -4,
        UNKNOWN = -5,
        BADCHECKSUM = -6,
        SETNEWNOSPAM = -7,
        NOMEM = -8
    }

    public enum ToxUserStatus
    {
        NONE,
        AWAY,
        BUSY,
        INVALID
    }

    public enum ToxUserConStatus
    {
        OFFLINE,
        ONLINE
    }

    public enum ToxFileControl
    {
        ACCEPT,
        PAUSE,
        KILL,
        FINISHED,
        RESUME_BROKEN
    }

    public enum ToxChatChange
    {
        TOX_CHAT_CHANGE_PEER_ADD,
        TOX_CHAT_CHANGE_PEER_DEL,
        TOX_CHAT_CHANGE_PEER_NAME
    }
}
