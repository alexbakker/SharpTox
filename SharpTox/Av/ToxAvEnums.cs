using System;
namespace SharpTox.Av
{
    public enum ToxAvErrorNew
    {
        Ok,
        Null,
        Malloc,
        Multiple
    }

    public enum ToxAvErrorCall
    {
        Ok,
        Malloc,
        FriendNotFound,
        FriendNotConnected,
        FriendAlreadyInCall,
        InvalidBitRate
    }

    public enum ToxAvErrorAnswer
    {
        Ok,
        CodecInitialization,
        FriendNotFound,
        FriendNotCalling,
        InvalidBitRate
    }

    [Flags]
    public enum ToxAvFriendCallState
    {
        Paused = 0,
        Error = 1 << 0,
        Finished = 1 << 1,
        SendingAudio = 1 << 2,
        SendingVideo = 1 << 3,
        ReceivingAudio = 1 << 4,
        ReceivingVideo = 1 << 5,
    }

    public enum ToxAvCallControl
    {
        Resume,
        Pause,
        Cancel,
        MuteAudio,
        UnmuteAudio,
        HideVideo,
        ShowVideo
    }

    public enum ToxAvErrorCallControl
    {
        Ok,
        FriendNotFound,
        FriendNotInCall,
        InvalidTransition
    }

    public enum ToxAvErrorSetBitrate
    {
        Ok,
        Invalid,
        FriendNotFound,
        FriendNotInCall
    }

    public enum ToxAvErrorSendFrame
    {
        Ok,
        Null,
        FrienNotFound,
        FriendNotInCall,
        FrameInvalid,
        BitrateNotSet,
        RtpFailed
    }
}
