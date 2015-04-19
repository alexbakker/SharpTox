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
        Malloc,
        FriendNotFound,
        FriendNotCalling,
        InvalidBitRate
    }

    [Flags]
    public enum ToxAvCallState
    {
        Paused = 0,
        SendingAudio = 1,
        SendingVideo = 2,
        ReceivingAudio = 4,
        ReceivingVideo = 8,
        End = 16,
        DecreaseAudioBitrate = 32,
        DecreaseVideoBitrate = 64,
        IncreaseAudioBitrate = 128,
        IncreaseVideoBitrate = 256,
        Error = 32768
    }

    public enum ToxAvCallControl
    {
        Resume,
        Pause,
        Cancel,
        ToggleMuteAudio,
        ToggleMuteVideo
    }

    public enum ToxAvErrorCallControl
    {
        Ok,
        FriendNotFound,
        FriendNotInCall,
        NotPaused,
        Denied,
        AlreadyPaused,
        NotMuted
    }

    public enum ToxAvErrorBitrate
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
        FrameNotRequested,
        FrameInvalid,
        RtpFailed
    }
}
