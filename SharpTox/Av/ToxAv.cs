using System;
using SharpTox.Core;

namespace SharpTox.Av
{
    /// <summary>
    /// Delegate for callstate events.
    /// </summary>
    /// <param name="call_index"></param>
    /// <param name="args"></param>
    public delegate void CallstateChangedDelegate(int call_index, IntPtr args);

    /// <summary>
    /// Delegate for event <see cref="ToxAv.OnReceivedAudio"/>
    /// </summary>
    /// <param name="toxav"></param>
    /// <param name="call_index"></param>
    /// <param name="frame"></param>
    /// <param name="frame_size"></param>
    /// <param name="userdata"></param>
    public delegate void ReceivedAudioDelegate(IntPtr toxav, int call_index, short[] frame, int frame_size, IntPtr userdata);

    /// <summary>
    /// Delegate for event <see cref="ToxAv.OnReceivedVideo"/>
    /// </summary>
    /// <param name="toxav"></param>
    /// <param name="call_index"></param>
    /// <param name="frame"></param>
    /// <param name="userdata"></param>
    public delegate void ReceivedVideoDelegate(IntPtr toxav, int call_index, IntPtr frame, IntPtr userdata);

    /// <summary>
    /// Represents an instance of toxav.
    /// </summary>
    public class ToxAv : IDisposable
    {
        /// <summary>
        /// Occurs when a call gets canceled.
        /// </summary>
        public event CallstateChangedDelegate OnCancel;

        /// <summary>
        /// Occurs when a call ends.
        /// </summary>
        public event CallstateChangedDelegate OnEnd;

        /// <summary>
        /// Occurs when a call is ending.
        /// </summary>
        public event CallstateChangedDelegate OnEnding;

        /// <summary>
        /// Occurs when an invite for a call is received.
        /// </summary>
        public event CallstateChangedDelegate OnInvite;

        /// <summary>
        /// Occurs when the person on the other end timed out.
        /// </summary>
        public event CallstateChangedDelegate OnPeerTimeout;

        /// <summary>
        /// Occurs when a call gets rejected.
        /// </summary>
        public event CallstateChangedDelegate OnReject;

        /// <summary>
        /// Occurs when a call request times out.
        /// </summary>
        public event CallstateChangedDelegate OnRequestTimeout;

        /// <summary>
        /// Occurs when the person on the other end received the invite.
        /// </summary>
        public event CallstateChangedDelegate OnRinging;

        /// <summary>
        /// Occurs when the call is supposed to start.
        /// </summary>
        public event CallstateChangedDelegate OnStart;

        /// <summary>
        /// Occurs when the person on the other end has started the call.
        /// </summary>
        public event CallstateChangedDelegate OnStarting;

        /// <summary>
        /// Occurs when a peer wants to change the call type.
        /// </summary>
        public event CallstateChangedDelegate OnMediaChange;

        /// <summary>
        /// Occurs when an audio frame was received.
        /// </summary>
        public event ReceivedAudioDelegate OnReceivedAudio;

        /// <summary>
        /// Occurs when a video frame was received.
        /// </summary>
        public event ReceivedVideoDelegate OnReceivedVideo;

        #region Event delegates
        private ToxAvDelegates.CallstateCallback oncancelcallback;
        private ToxAvDelegates.CallstateCallback onendcallback;
        private ToxAvDelegates.CallstateCallback onendingcallback;
        private ToxAvDelegates.CallstateCallback oninvitecallback;
        private ToxAvDelegates.CallstateCallback onpeertimeoutcallback;
        private ToxAvDelegates.CallstateCallback onrejectcallback;
        private ToxAvDelegates.CallstateCallback onrequesttimeoutcallback;
        private ToxAvDelegates.CallstateCallback onringingcallback;
        private ToxAvDelegates.CallstateCallback onstartcallback;
        private ToxAvDelegates.CallstateCallback onstartingcallback;
        private ToxAvDelegates.CallstateCallback onmediachangecallback;
        private ToxAvDelegates.AudioReceiveCallback onreceivedaudiocallback;
        private ToxAvDelegates.VideoReceiveCallback onreceivedvideocallback;
        #endregion

        /// <summary>
        /// The delegate used for <see cref="Invoker"/>
        /// </summary>
        /// <param name="method"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public delegate object InvokeDelegate(Delegate method, params object[] p);

        /// <summary>
        /// The invoke delegate to use when raising events. Note that <see cref="OnReceivedAudio"/> and <see cref="OnReceivedVideo"/> will not use this.
        /// </summary>
        public InvokeDelegate Invoker;

        private bool disposed = false;

        /// <summary>
        /// The codec settings used for this instance of toxav.
        /// </summary>
        public readonly ToxAvCodecSettings CodecSettings;

        /// <summary>
        /// The default codec settings.
        /// </summary>
        public static readonly ToxAvCodecSettings DefaultCodecSettings = new ToxAvCodecSettings()
        {
            CallType = ToxAvCallType.Audio,
            VideoBitrate = 500,
            MaxVideoWidth = 1200,
            MaxVideoHeight = 720,

            AudioBitrate = 64000,
            AudioFrameDuration = 20,
            AudioSampleRate = 48000,
            AudioChannels = 1
        };

        private ToxAvHandle toxav;

        /// <summary>
        /// The maximum amount of calls this instance of toxav is allowed to have.
        /// </summary>
        public readonly int MaxCalls;

        /// <summary>
        /// Initialises a new instance of toxav.
        /// </summary>
        /// <param name="tox"></param>
        /// <param name="settings"></param>
        /// <param name="max_calls"></param>
        public ToxAv(ToxHandle tox, ToxAvCodecSettings settings, int max_calls)
        {
            toxav = ToxAvFunctions.New(tox, max_calls);

            if (toxav == null || toxav.IsInvalid)
                throw new Exception("Could not create a new instance of toxav.");

            MaxCalls = max_calls;
            CodecSettings = settings;

            Invoker = new InvokeDelegate(dummyinvoker);

            callbacks();
        }

        /// <summary>
        /// Releases all resources used by this instance of tox.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //dispose pattern as described on msdn for a class that uses a safe handle
        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing) { }

            if (!toxav.IsInvalid && !toxav.IsClosed && toxav != null)
                toxav.Dispose();

            disposed = true;
        }

        /// <summary>
        /// Kills this toxav instance.
        /// </summary>
        [Obsolete("This function is obsolete, use Dispose() instead", true)]
        public void Kill()
        {
            if (toxav.IsClosed || toxav.IsInvalid)
                throw null;

            toxav.Dispose();
        }

        /// <summary>
        /// Cancels a call.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="friend_number"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public ToxAvError Cancel(int call_index, int friend_number, string reason)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.Cancel(toxav, call_index, friend_number, reason);
        }

        /// <summary>
        /// Answers a call.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public ToxAvError Answer(int call_index, ToxAvCodecSettings settings)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.Answer(toxav, call_index, ref settings);
        }

        /// <summary>
        /// Creates a new call.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="friend_number"></param>
        /// <param name="settings"></param>
        /// <param name="ringing_seconds"></param>
        /// <returns></returns>
        public ToxAvError Call(int friend_number, ToxAvCodecSettings settings, int ringing_seconds, out int call_index)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int index = new int();
            ToxAvError result = ToxAvFunctions.Call(toxav, ref index, friend_number, ref settings, ringing_seconds);

            call_index = index;
            return result;
        }

        /// <summary>
        /// Hangs up an in-progress call.
        /// </summary>
        /// <param name="call_index"></param>
        /// <returns></returns>
        public ToxAvError Hangup(int call_index)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.Hangup(toxav, call_index);
        }

        /// <summary>
        /// Rejects an incoming call.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public ToxAvError Reject(int call_index, string reason)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.Reject(toxav, call_index, reason);
        }

        /// <summary>
        /// Stops a call and terminates the transmission without notifying the remote peer.
        /// </summary>
        /// <param name="call_index"></param>
        /// <returns></returns>
        public ToxAvError StopCall(int call_index)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.StopCall(toxav, call_index);
        }

        /// <summary>
        /// Prepares transmission. Must be called before any transmission occurs.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="support_video"></param>
        /// <param name="jbuf_size"></param>
        /// <param name="VAD_treshold"></param>
        /// <returns></returns>
        public ToxAvError PrepareTransmission(int call_index, int jbuf_size, int VAD_treshold, bool support_video)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.PrepareTransmission(toxav, call_index, (uint)jbuf_size, (uint)VAD_treshold, support_video ? 1 : 0);
        }

        /// <summary>
        /// Kills the transmission of a call. Should be called at the end of the transmission.
        /// </summary>
        /// <param name="call_index"></param>
        /// <returns></returns>
        public ToxAvError KillTransmission(int call_index)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.KillTransmission(toxav, call_index);
        }

        /// <summary>
        /// Get the friend_number of peer participating in conversation
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="peer"></param>
        /// <returns></returns>
        public int GetPeerID(int call_index, int peer)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.GetPeerID(toxav, call_index, peer);
        }

        /// <summary>
        /// Checks whether a certain capability is supported.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="capability"></param>
        /// <returns></returns>
        public bool CapabilitySupported(int call_index, ToxAvCapabilities capability)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.CapabilitySupported(toxav, call_index, capability) == 1;
        }

        /// <summary>
        /// Retrieves the tox instance that this toxav instance belongs to.
        /// </summary>
        /// <returns></returns>
        public IntPtr GetTox()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.GetTox(toxav);
        }

        /// <summary>
        /// Sends an encoded audio frame.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="frame"></param>
        /// <param name="frame_size"></param>
        /// <returns></returns>
        public ToxAvError SendAudio(int call_index, byte[] frame, int frame_size)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.SendAudio(toxav, call_index, frame, (uint)frame_size);
        }

        /// <summary>
        /// Encodes an audio frame.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="dest"></param>
        /// <param name="dest_max"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public int PrepareAudioFrame(int call_index, byte[] dest, int dest_max, ushort[] frame)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.PrepareAudioFrame(toxav, call_index, dest, dest_max, frame, frame.Length);
        }

        /// <summary>
        /// Gets the handle of this toxav instance.
        /// </summary>
        /// <returns></returns>
        public ToxAvHandle GetHandle()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return toxav;
        }

        /// <summary>
        /// Detects whether some activity is present in the call.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="pcm"></param>
        /// <param name="frame_size"></param>
        /// <param name="ref_energy"></param>
        /// <returns></returns>
        public int HasActivity(int call_index, short[] pcm, ushort frame_size, float ref_energy)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.HasActivity(toxav, call_index, pcm, frame_size, ref_energy);
        }

        /// <summary>
        /// Retrieves the state of a call.
        /// </summary>
        /// <param name="call_index"></param>
        /// <returns></returns>
        public ToxAvCallState GetCallState(int call_index)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.GetCallState(toxav, call_index);
        }

        /// <summary>
        /// Changes the type of an in-progress call
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public ToxAvError ChangeSettings(int call_index, ToxAvCodecSettings settings)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.ChangeSettings(toxav, call_index, ref settings);
        }

        /// <summary>
        /// Retrieves a peer's codec settings.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="peer"></param>
        /// <returns></returns>
        public ToxAvCodecSettings GetPeerCodecSettings(int call_index, int peer)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            ToxAvCodecSettings settings = new ToxAvCodecSettings();
            ToxAvFunctions.GetPeerCodecSettings(toxav, call_index, peer, ref settings);

            return settings;
        }

        private object dummyinvoker(Delegate method, params object[] p)
        {
            return method.DynamicInvoke(p);
        }

        private void callbacks()
        {
            ToxAvFunctions.RegisterCallstateCallback(toxav, oncancelcallback = new ToxAvDelegates.CallstateCallback((IntPtr agent, int call_index, IntPtr args) =>
            {
                if (OnCancel != null)
                    Invoker(OnCancel, call_index, args);
            }), ToxAvCallbackID.OnCancel, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(toxav, onendcallback = new ToxAvDelegates.CallstateCallback((IntPtr agent, int call_index, IntPtr args) =>
            {
                if (OnEnd != null)
                    Invoker(OnEnd, call_index, args);
            }), ToxAvCallbackID.OnEnd, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(toxav, onendingcallback = new ToxAvDelegates.CallstateCallback((IntPtr agent, int call_index, IntPtr args) =>
            {
                if (OnEnding != null)
                    Invoker(OnEnding, call_index, args);
            }), ToxAvCallbackID.OnEnding, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(toxav, oninvitecallback = new ToxAvDelegates.CallstateCallback((IntPtr agent, int call_index, IntPtr args) =>
            {
                if (OnInvite != null)
                    Invoker(OnInvite, call_index, args);
            }), ToxAvCallbackID.OnInvite, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(toxav, onpeertimeoutcallback = new ToxAvDelegates.CallstateCallback((IntPtr agent, int call_index, IntPtr args) =>
            {
                if (OnPeerTimeout != null)
                    Invoker(OnPeerTimeout, call_index, args);
            }), ToxAvCallbackID.OnPeerTimeout, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(toxav, onrejectcallback = new ToxAvDelegates.CallstateCallback((IntPtr agent, int call_index, IntPtr args) =>
            {
                if (OnReject != null)
                    Invoker(OnReject, call_index, args);
            }), ToxAvCallbackID.OnReject, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(toxav, onrequesttimeoutcallback = new ToxAvDelegates.CallstateCallback((IntPtr agent, int call_index, IntPtr args) =>
            {
                if (OnRequestTimeout != null)
                    Invoker(OnRequestTimeout, call_index, args);
            }), ToxAvCallbackID.OnRequestTimeout, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(toxav, onringingcallback = new ToxAvDelegates.CallstateCallback((IntPtr agent, int call_index, IntPtr args) =>
            {
                if (OnRinging != null)
                    Invoker(OnRinging, call_index, args);
            }), ToxAvCallbackID.OnRinging, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(toxav, onstartcallback = new ToxAvDelegates.CallstateCallback((IntPtr agent, int call_index, IntPtr args) =>
            {
                if (OnStart != null)
                    Invoker(OnStart, call_index, args);
            }), ToxAvCallbackID.OnStart, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(toxav, onstartingcallback = new ToxAvDelegates.CallstateCallback((IntPtr agent, int call_index, IntPtr args) =>
            {
                if (OnStarting != null)
                    Invoker(OnStarting, call_index, args);
            }), ToxAvCallbackID.OnStarting, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(toxav, onmediachangecallback = new ToxAvDelegates.CallstateCallback((IntPtr agent, int call_index, IntPtr args) =>
            {
                if (OnMediaChange != null)
                    Invoker(OnMediaChange, call_index, args);
            }), ToxAvCallbackID.OnMediaChange, IntPtr.Zero);

            ToxAvFunctions.RegisterAudioReceiveCallback(toxav, onreceivedaudiocallback = new ToxAvDelegates.AudioReceiveCallback((IntPtr ptr, int call_index, short[] frame, int frame_size, IntPtr userdata) =>
            {
                if (OnReceivedAudio != null)
                    OnReceivedAudio(ptr, call_index, frame, frame_size, userdata);
            }), IntPtr.Zero);

            ToxAvFunctions.RegisterVideoReceiveCallback(toxav, onreceivedvideocallback = new ToxAvDelegates.VideoReceiveCallback((IntPtr ptr, int call_index, IntPtr frame, IntPtr userdata) =>
            {
                if (OnReceivedVideo != null)
                    OnReceivedVideo(ptr, call_index, frame, userdata);
            }), IntPtr.Zero);
        }
    }
}

