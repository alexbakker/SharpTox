using System;
using SharpTox.Core;

namespace SharpTox.Av
{
    /// <summary>
    /// Represents an instance of toxav.
    /// </summary>
    public class ToxAv : IDisposable
    {
        /// <summary>
        /// Occurs when a call gets canceled.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnCancel;

        /// <summary>
        /// Occurs when a call ends.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnEnd;

        /// <summary>
        /// Occurs when a call is ending.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnEnding;

        /// <summary>
        /// Occurs when an invite for a call is received.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnInvite;

        /// <summary>
        /// Occurs when the person on the other end timed out.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnPeerTimeout;

        /// <summary>
        /// Occurs when a call gets rejected.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnReject;

        /// <summary>
        /// Occurs when a call request times out.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnRequestTimeout;

        /// <summary>
        /// Occurs when the person on the other end received the invite.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnRinging;

        /// <summary>
        /// Occurs when the call is supposed to start.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnStart;

        /// <summary>
        /// Occurs when the person on the other end has started the call.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnStarting;

        /// <summary>
        /// Occurs when a peer wants to change the call type.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnMediaChange;

        /// <summary>
        /// Occurs when an audio frame was received.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.AudioDataEventArgs> OnReceivedAudio;

        /// <summary>
        /// Occurs when a video frame was received.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.VideoDataEventArgs> OnReceivedVideo;

        #region Event delegates
        private ToxAvDelegates.CallstateCallback _onCancelCallback;
        private ToxAvDelegates.CallstateCallback _onEndCallback;
        private ToxAvDelegates.CallstateCallback _onEndingCallback;
        private ToxAvDelegates.CallstateCallback _onInviteCallback;
        private ToxAvDelegates.CallstateCallback _onPeerTimeoutCallback;
        private ToxAvDelegates.CallstateCallback _onRejectCallback;
        private ToxAvDelegates.CallstateCallback _onRequestTimeoutCallback;
        private ToxAvDelegates.CallstateCallback _onRingingCallback;
        private ToxAvDelegates.CallstateCallback _onStartCallback;
        private ToxAvDelegates.CallstateCallback _onStartingCallback;
        private ToxAvDelegates.CallstateCallback _onMediaChangeCallback;
        private ToxAvDelegates.AudioReceiveCallback _onReceivedAudioCallback;
        private ToxAvDelegates.VideoReceiveCallback _onReceivedVideoCallback;
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

        private bool _disposed = false;

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

        private ToxAvHandle _toxAv;

        /// <summary>
        /// The handle of this toxav instance.
        /// </summary>
        public ToxAvHandle Handle
        {
            get
            {
                return _toxAv;
            }
        }

        private ToxHandle _toxHandle;

        /// <summary>
        /// The Tox instance that this toxav instance belongs to.
        /// </summary>
        public ToxHandle ToxHandle
        {
            get
            {
                return _toxHandle;
            }
        }

        /// <summary>
        /// The maximum amount of calls this instance of toxav is allowed to have.
        /// </summary>
        public readonly int MaxCalls;

        /// <summary>
        /// Initialises a new instance of toxav.
        /// </summary>
        /// <param name="tox"></param>
        /// <param name="settings"></param>
        /// <param name="maxCalls"></param>
        public ToxAv(ToxHandle tox, ToxAvCodecSettings settings, int maxCalls)
        {
            _toxHandle = tox;
            _toxAv = ToxAvFunctions.New(tox, maxCalls);

            if (_toxAv == null || _toxAv.IsInvalid)
                throw new Exception("Could not create a new instance of toxav.");

            MaxCalls = maxCalls;
            CodecSettings = settings;

            Invoker = DummyInvoker;

            Callbacks();
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
            if (_disposed)
                return;

            if (disposing) { }

            if (!_toxAv.IsInvalid && !_toxAv.IsClosed && _toxAv != null)
                _toxAv.Dispose();

            _disposed = true;
        }

        /// <summary>
        /// Kills this toxav instance.
        /// </summary>
        [Obsolete("Use Dispose() instead", true)]
        public void Kill()
        {
            if (_toxAv.IsClosed || _toxAv.IsInvalid)
                throw null;

            _toxAv.Dispose();
        }

        /// <summary>
        /// Cancels a call.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <param name="friendNumber"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public ToxAvError Cancel(int callIndex, int friendNumber, string reason)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.Cancel(_toxAv, callIndex, friendNumber, reason);
        }

        /// <summary>
        /// Answers a call.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public ToxAvError Answer(int callIndex, ToxAvCodecSettings settings)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.Answer(_toxAv, callIndex, ref settings);
        }

        /// <summary>
        /// Creates a new call.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <param name="friendNumber"></param>
        /// <param name="settings"></param>
        /// <param name="ringingSeconds"></param>
        /// <returns></returns>
        public ToxAvError Call(int friendNumber, ToxAvCodecSettings settings, int ringingSeconds, out int callIndex)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int index = new int();
            ToxAvError result = ToxAvFunctions.Call(_toxAv, ref index, friendNumber, ref settings, ringingSeconds);

            callIndex = index;
            return result;
        }

        /// <summary>
        /// Hangs up an in-progress call.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <returns></returns>
        public ToxAvError Hangup(int callIndex)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.Hangup(_toxAv, callIndex);
        }

        /// <summary>
        /// Rejects an incoming call.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public ToxAvError Reject(int callIndex, string reason)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.Reject(_toxAv, callIndex, reason);
        }

        /// <summary>
        /// Stops a call and terminates the transmission without notifying the remote peer.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <returns></returns>
        public ToxAvError StopCall(int callIndex)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.StopCall(_toxAv, callIndex);
        }

        /// <summary>
        /// Prepares transmission. Must be called before any transmission occurs.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <param name="supportVideo"></param>
        /// <param name="jitterBufferSize"></param>
        /// <param name="vadTreshold"></param>
        /// <returns></returns>
        public ToxAvError PrepareTransmission(int callIndex, int jitterBufferSize, int vadTreshold, bool supportVideo)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.PrepareTransmission(_toxAv, callIndex, (uint)jitterBufferSize, (uint)vadTreshold, supportVideo ? 1 : 0);
        }

        /// <summary>
        /// Kills the transmission of a call. Should be called at the end of the transmission.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <returns></returns>
        public ToxAvError KillTransmission(int callIndex)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.KillTransmission(_toxAv, callIndex);
        }

        /// <summary>
        /// Get the friend_number of peer participating in conversation
        /// </summary>
        /// <param name="callIndex"></param>
        /// <param name="peer"></param>
        /// <returns></returns>
        public int GetPeerID(int callIndex, int peer)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.GetPeerID(_toxAv, callIndex, peer);
        }

        /// <summary>
        /// Checks whether a certain capability is supported.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <param name="capability"></param>
        /// <returns></returns>
        public bool CapabilitySupported(int callIndex, ToxAvCapabilities capability)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.CapabilitySupported(_toxAv, callIndex, capability) == 1;
        }

        /// <summary>
        /// Sends an encoded audio frame.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <param name="frame"></param>
        /// <param name="frameSize"></param>
        /// <returns></returns>
        public ToxAvError SendAudio(int callIndex, byte[] frame, int frameSize)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.SendAudio(_toxAv, callIndex, frame, (uint)frameSize);
        }

        /// <summary>
        /// Encodes an audio frame.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <param name="dest"></param>
        /// <param name="destMax"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public int PrepareAudioFrame(int callIndex, byte[] dest, int destMax, ushort[] frame) //TODO: use 'out' keyword to get the encoded frame
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.PrepareAudioFrame(_toxAv, callIndex, dest, destMax, frame, frame.Length);
        }

        /// <summary>
        /// Detects whether some activity is present in the call.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <param name="pcm"></param>
        /// <param name="frameSize"></param>
        /// <param name="refEnergy"></param>
        /// <returns></returns>
        public int HasActivity(int callIndex, short[] pcm, ushort frameSize, float refEnergy)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.HasActivity(_toxAv, callIndex, pcm, frameSize, refEnergy);
        }

        /// <summary>
        /// Retrieves the state of a call.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <returns></returns>
        public ToxAvCallState GetCallState(int callIndex)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.GetCallState(_toxAv, callIndex);
        }

        /// <summary>
        /// Changes the type of an in-progress call
        /// </summary>
        /// <param name="callIndex"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public ToxAvError ChangeSettings(int callIndex, ToxAvCodecSettings settings)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.ChangeSettings(_toxAv, callIndex, ref settings);
        }

        /// <summary>
        /// Retrieves a peer's codec settings.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="peer"></param>
        /// <returns></returns>
        public ToxAvCodecSettings GetPeerCodecSettings(int callIndex, int peer)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            ToxAvCodecSettings settings = new ToxAvCodecSettings();
            ToxAvFunctions.GetPeerCodecSettings(_toxAv, callIndex, peer, ref settings);

            return settings;
        }

        private object DummyInvoker(Delegate method, params object[] p)
        {
            return method.DynamicInvoke(p);
        }

        private void Callbacks()
        {
            ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onCancelCallback = (IntPtr agent, int callIndex, IntPtr args) =>
            {
                if (OnCancel != null)
                    Invoker(OnCancel, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnCancel));
            }, ToxAvCallbackID.OnCancel, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onEndCallback = (IntPtr agent, int callIndex, IntPtr args) =>
            {
                if (OnEnd != null)
                    Invoker(OnEnd, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnEnd));
            }, ToxAvCallbackID.OnEnd, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onEndingCallback = (IntPtr agent, int callIndex, IntPtr args) =>
            {
                if (OnEnding != null)
                    Invoker(OnEnding, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnEnding));
            }, ToxAvCallbackID.OnEnding, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onInviteCallback = (IntPtr agent, int callIndex, IntPtr args) =>
            {
                if (OnInvite != null)
                    Invoker(OnInvite, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnInvite));
            }, ToxAvCallbackID.OnInvite, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onPeerTimeoutCallback = (IntPtr agent, int callIndex, IntPtr args) =>
            {
                if (OnPeerTimeout != null)
                    Invoker(OnPeerTimeout, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnPeerTimeout));
            }, ToxAvCallbackID.OnPeerTimeout, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onRejectCallback = (IntPtr agent, int callIndex, IntPtr args) =>
            {
                if (OnReject != null)
                    Invoker(OnReject, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnReject));
            }, ToxAvCallbackID.OnReject, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onRequestTimeoutCallback = (IntPtr agent, int callIndex, IntPtr args) =>
            {
                if (OnRequestTimeout != null)
                    Invoker(OnRequestTimeout, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnRequestTimeout));
            }, ToxAvCallbackID.OnRequestTimeout, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onRingingCallback = (IntPtr agent, int callIndex, IntPtr args) =>
            {
                if (OnRinging != null)
                    Invoker(OnRinging, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnRinging));
            }, ToxAvCallbackID.OnRinging, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onStartCallback = (IntPtr agent, int callIndex, IntPtr args) =>
            {
                if (OnStart != null)
                    Invoker(OnStart, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnStart));
            }, ToxAvCallbackID.OnStart, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onStartingCallback = (IntPtr agent, int callIndex, IntPtr args) =>
            {
                if (OnStarting != null)
                    Invoker(OnStarting, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnStarting));
            }, ToxAvCallbackID.OnStarting, IntPtr.Zero);

            ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onMediaChangeCallback = (IntPtr agent, int callIndex, IntPtr args) =>
            {
                if (OnMediaChange != null)
                    Invoker(OnMediaChange, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnMediaChange));
            }, ToxAvCallbackID.OnMediaChange, IntPtr.Zero);

            ToxAvFunctions.RegisterAudioReceiveCallback(_toxAv, _onReceivedAudioCallback = (IntPtr ptr, int callIndex, short[] frame, int frameSize, IntPtr userData) =>
            {
                if (OnReceivedAudio != null)
                    Invoker(OnReceivedAudio, this, new ToxAvEventArgs.AudioDataEventArgs(callIndex, frame));
            }, IntPtr.Zero);

            ToxAvFunctions.RegisterVideoReceiveCallback(_toxAv, _onReceivedVideoCallback = (IntPtr ptr, int callIndex, IntPtr frame, IntPtr userData) =>
            {
                if (OnReceivedVideo != null)
                    Invoker(OnReceivedVideo, this, new ToxAvEventArgs.VideoDataEventArgs(callIndex, frame));
            }, IntPtr.Zero);
        }
    }
}
