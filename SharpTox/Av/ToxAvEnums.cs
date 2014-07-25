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

        //Responses
        /// <summary>
        /// The peer has received the call invite.
        /// </summary>
        OnRinging,

        /// <summary>
        /// Call starting on the other end.
        /// </summary>
        OnStarting,

        /// <summary>
        /// Call is ending on the other end.
        /// </summary>
        OnEnding,

        //Protocol
        /// <summary>
        /// The call invite timed out.
        /// </summary>
        OnRequestTimeout,

        /// <summary>
        /// A peer has timed out.
        /// </summary>
        OnPeerTimeout,

        /// <summary>
        /// Peer wants to change the call type.
        /// </summary>
        OnMediaChange
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
        /// Internal error.
        /// </summary>
        Internal = -1,

        /// <summary>
        /// Already has an active call.
        /// </summary>
        AlreadyInCall = -2,

        /// <summary>
        /// Trying to perform call action while not in a call.
        /// </summary>
        NoCall = -3,

        /// <summary>
        /// Trying to perform call action while in invalid state.
        /// </summary>
        InvalidState = -4,

        /// <summary>
        /// Trying to perform rtp action on invalid session.
        /// </summary>
        NoRtpSession = -5,

        /// <summary>
        /// Indicating packet loss.
        /// </summary>
        AudioPacketLost = -6,

        /// <summary>
        /// Error in toxav_prepare_transmission()
        /// </summary>
        StartingAudioRtp = -7,

        /// <summary>
        /// Error in toxav_prepare_transmission()
        /// </summary>
        StartingVideoRtp = -8,

        /// <summary>
        /// Returned in toxav_kill_transmission()
        /// </summary>
        TerminatingAudioRtp = -9,

        /// <summary>
        /// Returned in toxav_kill_transmission()
        /// </summary>
        TerminatingVideoRtp = -10,

        /// <summary>
        /// Returned in toxav_kill_transmission()
        /// </summary>
        PacketTooLarge = -11,

        /// <summary>
        /// Codec state not initialized.
        /// </summary>
        InvalidCodecState = -12
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
