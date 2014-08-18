#pragma warning disable 1591

namespace SharpTox.Core
{
    public enum ToxAFError
    {
        TooLong = -1,
        NoMessage = -2,
        OwnKey = -3,
        AlreadySent = -4,
        Unknown = -5,
        BadChecksum = -6,
        SetNewNospam = -7,
        NoMem = -8
    }

    public enum ToxUserStatus
    {
        None,
        Away,
        Busy,
        Invalid
    }

    public enum ToxUserConStatus
    {
        Offline,
        Online
    }

    public enum ToxFileControl
    {
        Accept,
        Pause,
        Kill,
        Finished,
        ResumeBroken
    }

    public enum ToxChatChange
    {
        PeerAdd,
        PeerDel,
        PeerName
    }

    public enum ToxKeyType
    {
        Public,
        Secret
    }
}

#pragma warning restore 1591