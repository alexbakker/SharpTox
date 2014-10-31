using System;
using System.Runtime.InteropServices;

using SharpTox.Core;

namespace SharpTox.Av
{
    /// <summary>
    /// Represents an instance of toxav.
    /// </summary>
    public class ToxAv : IDisposable
    {
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
        public ToxAv(ToxHandle tox, int maxCalls)
        {
            _toxHandle = tox;
            _toxAv = ToxAvFunctions.New(tox, maxCalls);

            if (_toxAv == null || _toxAv.IsInvalid)
                throw new Exception("Could not create a new instance of toxav.");

            MaxCalls = maxCalls;
            Invoker = DummyInvoker;
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

            ClearEventSubscriptions();

            _disposed = true;
        }

        private void ClearEventSubscriptions()
        {
            _onCancel = null;
            _onEnd = null;
            _onEnding = null;
            _onInvite = null;
            _onMediaChange = null;
            _onPeerTimeout = null;
            _onReceivedAudio = null;
            _onReceivedVideo = null;
            _onReject = null;
            _onRequestTimeout = null;
            _onRinging = null;
            _onStart = null;
            _onStarting = null;
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

        #region Events
        private EventHandler<ToxAvEventArgs.CallStateEventArgs> _onCancel;

        /// <summary>
        /// Occurs when a call gets canceled.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnCancel
        {
            add
            {
                if (_onCancelCallback == null)
                {
                    _onCancelCallback = (IntPtr agent, int callIndex, IntPtr args) =>
                    {
                        if (_onCancel != null)
                            Invoker(_onCancel, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnCancel));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onCancelCallback, ToxAvCallbackID.OnCancel, IntPtr.Zero);
                }

                _onCancel += value;
            }
            remove
            {
                _onCancel -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.CallStateEventArgs> _onEnd;

        /// <summary>
        /// Occurs when a call ends.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnEnd
        {
            add
            {
                if (_onEndCallback == null)
                {
                    _onEndCallback = (IntPtr agent, int callIndex, IntPtr args) =>
                    {
                        if (_onEnd != null)
                            Invoker(_onEnd, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnEnd));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onEndCallback, ToxAvCallbackID.OnEnd, IntPtr.Zero);
                }

                _onEnd += value;
            }
            remove
            {
                _onEnd -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.CallStateEventArgs> _onEnding;

        /// <summary>
        /// Occurs when a call is ending.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnEnding
        {
            add
            {
                if (_onEndingCallback == null)
                {
                    _onEndingCallback = (IntPtr agent, int callIndex, IntPtr args) =>
                    {
                        if (_onEnding != null)
                            Invoker(_onEnding, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnEnding));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onEndingCallback, ToxAvCallbackID.OnEnding, IntPtr.Zero);
                }

                _onEnding += value;
            }
            remove
            {
                _onEnding -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.CallStateEventArgs> _onInvite;

        /// <summary>
        /// Occurs when an invite for a call is received.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnInvite
        {
            add
            {
                if (_onInviteCallback == null)
                {
                    _onInviteCallback = (IntPtr agent, int callIndex, IntPtr args) =>
                    {
                        if (_onInvite != null)
                            Invoker(_onInvite, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnInvite));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onInviteCallback, ToxAvCallbackID.OnInvite, IntPtr.Zero);
                }

                _onInvite += value;
            }
            remove
            {
                _onInvite -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.CallStateEventArgs> _onPeerTimeout;

        /// <summary>
        /// Occurs when the person on the other end timed out.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnPeerTimeout
        {
            add
            {
                if (_onPeerTimeoutCallback == null)
                {
                    _onPeerTimeoutCallback = (IntPtr agent, int callIndex, IntPtr args) =>
                    {
                        if (_onPeerTimeout != null)
                            Invoker(_onPeerTimeout, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnPeerTimeout));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onPeerTimeoutCallback, ToxAvCallbackID.OnPeerTimeout, IntPtr.Zero);
                }

                _onPeerTimeout += value;
            }
            remove
            {
                _onPeerTimeout -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.CallStateEventArgs> _onReject;

        /// <summary>
        /// Occurs when a call gets rejected.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnReject
        {
            add
            {
                if (_onRejectCallback == null)
                {
                    _onRejectCallback = (IntPtr agent, int callIndex, IntPtr args) =>
                    {
                        if (_onReject != null)
                            Invoker(_onReject, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnReject));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onRejectCallback, ToxAvCallbackID.OnReject, IntPtr.Zero);
                }

                _onReject += value;
            }
            remove
            {
                _onReject -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.CallStateEventArgs> _onRequestTimeout;

        /// <summary>
        /// Occurs when a call request times out.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnRequestTimeout
        {
            add
            {
                if (_onRequestTimeoutCallback == null)
                {
                    _onRequestTimeoutCallback = (IntPtr agent, int callIndex, IntPtr args) =>
                    {
                        if (_onRequestTimeout != null)
                            Invoker(_onRequestTimeout, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnRequestTimeout));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onRequestTimeoutCallback, ToxAvCallbackID.OnRequestTimeout, IntPtr.Zero);
                }

                _onRequestTimeout += value;
            }
            remove
            {
                _onRequestTimeout -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.CallStateEventArgs> _onRinging;

        /// <summary>
        /// Occurs when the person on the other end received the invite.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnRinging
        {
            add
            {
                if (_onRingingCallback == null)
                {
                    _onRingingCallback = (IntPtr agent, int callIndex, IntPtr args) =>
                    {
                        if (_onRinging != null)
                            Invoker(_onRinging, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnRinging));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onRingingCallback, ToxAvCallbackID.OnRinging, IntPtr.Zero);
                }

                _onRinging += value;
            }
            remove
            {
                _onRinging -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.CallStateEventArgs> _onStart;

        /// <summary>
        /// Occurs when the call is supposed to start.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnStart
        {
            add
            {
                if (_onStartCallback == null)
                {
                    _onStartCallback = (IntPtr agent, int callIndex, IntPtr args) =>
                    {
                        if (_onStart != null)
                            Invoker(_onStart, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnStart));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onStartCallback, ToxAvCallbackID.OnStart, IntPtr.Zero);
                }

                _onStart += value;
            }
            remove
            {
                _onStart -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.CallStateEventArgs> _onStarting;

        /// <summary>
        /// Occurs when the person on the other end has started the call.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnStarting
        {
            add
            {
                if (_onStartingCallback == null)
                {
                    _onStartingCallback = (IntPtr agent, int callIndex, IntPtr args) =>
                    {
                        if (_onStarting != null)
                            Invoker(_onStarting, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnStarting));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onStartingCallback, ToxAvCallbackID.OnStarting, IntPtr.Zero);
                }

                _onStarting += value;
            }
            remove
            {
                _onStarting -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.CallStateEventArgs> _onMediaChange;

        /// <summary>
        /// Occurs when a peer wants to change the call type.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnMediaChange
        {
            add
            {
                if (_onMediaChangeCallback == null)
                {
                    _onMediaChangeCallback = (IntPtr agent, int callIndex, IntPtr args) =>
                    {
                        if (_onMediaChange != null)
                            Invoker(_onMediaChange, this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnMediaChange));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onMediaChangeCallback, ToxAvCallbackID.OnMediaChange, IntPtr.Zero);
                }

                _onMediaChange += value;
            }
            remove
            {
                _onMediaChange -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.AudioDataEventArgs> _onReceivedAudio;

        /// <summary>
        /// Occurs when an audio frame was received.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.AudioDataEventArgs> OnReceivedAudio
        {
            add
            {
                if (_onReceivedAudioCallback == null)
                {
                    _onReceivedAudioCallback = (IntPtr ptr, int callIndex, IntPtr frame, int frameSize, IntPtr userData) =>
                    {
                        if (_onReceivedAudio != null)
                        {
                            int channels = (int)GetPeerCodecSettings(callIndex, 0).AudioChannels;
                            short[] samples = new short[frameSize * channels];

                            Marshal.Copy(frame, samples, 0, samples.Length);

                            Invoker(_onReceivedAudio, this, new ToxAvEventArgs.AudioDataEventArgs(callIndex, samples));
                        }
                    };

                    ToxAvFunctions.RegisterAudioReceiveCallback(_toxAv, _onReceivedAudioCallback, IntPtr.Zero);
                }

                _onReceivedAudio += value;
            }
            remove
            {
                _onReceivedAudio -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.VideoDataEventArgs> _onReceivedVideo;

        /// <summary>
        /// Occurs when a video frame was received.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.VideoDataEventArgs> OnReceivedVideo
        {
            add
            {
                if (_onReceivedVideoCallback == null)
                {
                    _onReceivedVideoCallback = (IntPtr ptr, int callIndex, IntPtr frame, IntPtr userData) =>
                    {
                        if (_onReceivedVideo != null)
                            Invoker(_onReceivedVideo, this, new ToxAvEventArgs.VideoDataEventArgs(callIndex, frame));
                    };

                    ToxAvFunctions.RegisterVideoReceiveCallback(_toxAv, _onReceivedVideoCallback, IntPtr.Zero);
                }

                _onReceivedVideo += value;
            }
            remove
            {
                _onReceivedVideo -= value;
            }
        }
        #endregion
    }
}
