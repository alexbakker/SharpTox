namespace SharpTox.Av
{
    /// <summary>
    /// Callbacks ids that handle the call states.
    /// </summary>
    public enum ToxAvCallbackID
    {
        //Requests
        /// <summary>
        /// Call invite received.
        /// </summary>
        OnInvite,

        /// <summary>
        /// Peer has received the call invite.
        /// </summary>
        OnRinging,

        /// <summary>
        /// Call starting on our end.
        /// </summary>
        OnStart,

        /// <summary>
        /// The call has been canceled.
        /// </summary>
        OnCancel,

        /// <summary>
        /// The call invite was rejected.
        /// </summary>
        OnReject,

        /// <summary>
        /// The call is ending.
        /// </summary>
        OnEnd,

        /// <summary>
        /// The call invite timed out.
        /// </summary>
        OnRequestTimeout,

        /// <summary>
        /// A peer has timed out.
        /// </summary>
        OnPeerTimeout,

        /// <summary>
        /// Peer changed codec settings.
        /// </summary>
        OnPeerCSChange,

        /// <summary>
        /// Codec settings change confirmation.
        /// </summary>
        OnSelfCSChange,
    }

    /// <summary>
    /// Call type identifier.
    /// </summary>
    public enum ToxAvCallType
    {
        /// <summary>
        /// Audio call.
        /// </summary>
        Audio = 192,

        /// <summary>
        /// Audio and video call.
        /// </summary>
        Video
    }

    /// <summary>
    /// Error indicators.
    /// </summary>
    public enum ToxAvError
    {
        /// <summary>
        /// No error.
        /// </summary>
        None = 0,

        /// <summary>
        /// Unknown error.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Trying to perform call action while not in a call.
        /// </summary>
        NoCall = -20,

        /// <summary>
        /// Trying to perform call action while in invalid state.
        /// </summary>
        InvalidState = -21,

        /// <summary>
        /// Trying to call peer when already in a call with peer.
        /// </summary>
        AlreadyInCallWithPeer = -22,

        /// <summary>
        /// Cannot handle more calls.
        /// </summary>
        ReachedCallLimit = -23,

        /// <summary>
        /// Failed creating CSSession.
        /// </summary>
        InitializingCodecs = -30,

        /// <summary>
        /// Error setting resolution.
        /// </summary>
        SettingVideoResolution = -31,

        /// <summary>
        /// Error setting bitrate.
        /// </summary>
        SettingVideoBitrate = -32,

        /// <summary>
        /// Error splitting video payload.
        /// </summary>
        SplittingVideoPayload = -33,

        /// <summary>
        /// vpx_codec_encode failed.
        /// </summary>
        EncodingVideo = -34,

        /// <summary>
        /// opus_encode failed.
        /// </summary>
        EncodingAudio = -35,

        /// <summary>
        /// Sending lossy packet failed.
        /// </summary>
        SendingPayload = -40,

        /// <summary>
        /// One of the rtp sessions failed to initialize.
        /// </summary>
        CreatingRtpSessions = -41,

        /// <summary>
        /// Trying to perform rtp action on invalid session.
        /// </summary>
        NoRtpSession = -50,

        /// <summary>
        /// Codec state not initialized.
        /// </summary>
        InvalidCodecState = -51,

        /// <summary>
        /// Split packet exceeds it's limit.
        /// </summary>
        PacketTooLarge = -52,
    }

    /// <summary>
    /// Locally supported capabilities.
    /// </summary>
    public enum ToxAvCapabilities
    {
        /// <summary>
        /// No capability.
        /// </summary>
        None,

        /// <summary>
        /// Audio encoding capability.
        /// </summary>
        AudioEncoding = 1 << 0,

        /// <summary>
        /// Audio decoding capability.
        /// </summary>
        AudioDecoding = 1 << 1,

        /// <summary>
        /// Video encoding capability.
        /// </summary>
        VideoEncoding = 1 << 2,

        /// <summary>
        /// Video decoding capability.
        /// </summary>
        VideoDecoding = 1 << 3
    }

    /// <summary>
    /// Call state identifier.
    /// </summary>
    public enum ToxAvCallState
    {
        /// <summary>
        /// The call doesn't exist.
        /// </summary>
        CallNonExistent = -1,

        /// <summary>
        /// Sending call invite.
        /// </summary>
        CallInviting,

        /// <summary>
        /// Getting call invite.
        /// </summary>
        CallStarting,

        /// <summary>
        /// The call is in progress.
        /// </summary>
        CallActive,

        /// <summary>
        /// The call is on hold.
        /// </summary>
        CallHold,

        /// <summary>
        /// The call has been hung up.
        /// </summary>
        CallHungUp
    }
}
