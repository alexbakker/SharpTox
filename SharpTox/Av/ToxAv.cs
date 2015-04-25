using SharpTox.Core;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

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
        private ToxAvDelegates.CallstateCallback _onInviteCallback;
        private ToxAvDelegates.CallstateCallback _onPeerTimeoutCallback;
        private ToxAvDelegates.CallstateCallback _onRejectCallback;
        private ToxAvDelegates.CallstateCallback _onRequestTimeoutCallback;
        private ToxAvDelegates.CallstateCallback _onRingingCallback;
        private ToxAvDelegates.CallstateCallback _onStartCallback;
        private ToxAvDelegates.CallstateCallback _onPeerCSChangeCallback;
        private ToxAvDelegates.CallstateCallback _onSelfCSChangeCallback;
        private ToxAvDelegates.AudioReceiveCallback _onReceivedAudioCallback;
        private ToxAvDelegates.VideoReceiveCallback _onReceivedVideoCallback;
        #endregion

        private List<ToxAvDelegates.GroupAudioReceiveCallback> _groupAudioHandlers = new List<ToxAvDelegates.GroupAudioReceiveCallback>();
        private bool _disposed = false;
        private bool _running = false;
        private CancellationTokenSource _cancelTokenSource;

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

        private ToxHandle _tox;

        /// <summary>
        /// The Tox instance that this toxav instance belongs to.
        /// </summary>
        public ToxHandle ToxHandle
        {
            get
            {
                return _tox;
            }
        }

        /// <summary>
        /// Retrieves the number of active calls.
        /// </summary>
        public int ActiveCalls
        {
            get
            {
                ThrowIfDisposed();

                int count = ToxAvFunctions.GetActiveCount(_toxAv);
                return count == -1 ? 0 : count;
            }
        }

        /// <summary>
        /// The maximum amount of calls this instance of toxav is allowed to have.
        /// </summary>
        public int MaxCalls { get; private set; }

        /// <summary>
        /// Initialises a new instance of toxav.
        /// </summary>
        /// <param name="tox"></param>
        /// <param name="maxCalls"></param>
        public ToxAv(ToxHandle tox, int maxCalls)
        {
            _tox = tox;
            _toxAv = ToxAvFunctions.New(tox, maxCalls);

            if (_toxAv == null || _toxAv.IsInvalid)
                throw new Exception("Could not create a new instance of toxav.");

            MaxCalls = maxCalls;
        }

        /// <summary>
        /// Initialises a new instance of toxav.
        /// </summary>
        /// <param name="tox"></param>
        /// <param name="maxCalls"></param>
        public ToxAv(Tox tox, int maxCalls)
            : this(tox.Handle, maxCalls) { }

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

            if (disposing)
            {
                if (_cancelTokenSource != null)
                {
                    _cancelTokenSource.Cancel();
                    _cancelTokenSource.Dispose();
                }
            }

            ClearEventSubscriptions();

            if (!_toxAv.IsInvalid && !_toxAv.IsClosed && _toxAv != null)
                _toxAv.Dispose();

            _disposed = true;
        }

        private void ClearEventSubscriptions()
        {
            _onCancel = null;
            _onEnd = null;
            _onInvite = null;
            _onPeerTimeout = null;
            _onReceivedAudio = null;
            _onReceivedVideo = null;
            _onReject = null;
            _onRequestTimeout = null;
            _onRinging = null;
            _onStart = null;
            _onPeerCSChange = null;
            _onSelfCSChange = null;

            OnReceivedGroupAudio = null;
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
        /// Starts the main toxav_do loop.
        /// </summary>
        public void Start()
        {
            ThrowIfDisposed();

            if (_running)
                return;

            Loop();
        }

        /// <summary>
        /// Stops the main toxav_do loop if it's running.
        /// </summary>
        public void Stop()
        {
            ThrowIfDisposed();

            if (!_running)
                return;

            if (_cancelTokenSource != null)
            {
                _cancelTokenSource.Cancel();
                _cancelTokenSource.Dispose();

                _running = false;
            }
        }

        private void Loop()
        {
            _cancelTokenSource = new CancellationTokenSource();
            _running = true;

            Task.Factory.StartNew(async () =>
            {
                while (_running)
                {
                    if (_cancelTokenSource.IsCancellationRequested)
                        break;

                    int delay = DoIterate();
                    await Task.Delay(delay);
                }
            }, _cancelTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// Runs the loop once in the current thread and returns the next timeout.
        /// </summary>
        public int Iterate()
        {
            ThrowIfDisposed();

            if (_running)
                throw new Exception("Loop already running");

            return DoIterate();
        }

        private int DoIterate()
        {
            ToxAvFunctions.Do(_toxAv);
            return (int)ToxAvFunctions.DoInterval(_toxAv);
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
            ThrowIfDisposed();

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
            ThrowIfDisposed();

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
            ThrowIfDisposed();

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
            ThrowIfDisposed();

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
            ThrowIfDisposed();

            return ToxAvFunctions.Reject(_toxAv, callIndex, reason);
        }

        /// <summary>
        /// Stops a call and terminates the transmission without notifying the remote peer.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <returns></returns>
        public ToxAvError StopCall(int callIndex)
        {
            ThrowIfDisposed();

            return ToxAvFunctions.StopCall(_toxAv, callIndex);
        }

        /// <summary>
        /// Prepares transmission. Must be called before any transmission occurs.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <param name="supportVideo"></param>
        /// <returns></returns>
        public ToxAvError PrepareTransmission(int callIndex, bool supportVideo)
        {
            ThrowIfDisposed();

            return ToxAvFunctions.PrepareTransmission(_toxAv, callIndex, supportVideo ? 1 : 0);
        }

        /// <summary>
        /// Kills the transmission of a call. Should be called at the end of the transmission.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <returns></returns>
        public ToxAvError KillTransmission(int callIndex)
        {
            ThrowIfDisposed();

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
            ThrowIfDisposed();

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
            ThrowIfDisposed();

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
            ThrowIfDisposed();

            return ToxAvFunctions.SendAudio(_toxAv, callIndex, frame, (uint)frameSize);
        }

        /// <summary>
        /// Encodes an audio frame.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <param name="dest"></param>
        /// <param name="destMax"></param>
        /// <param name="frames"></param>
        /// <param name="perframe"></param>
        /// <returns></returns>
        public int PrepareAudioFrame(int callIndex, byte[] dest, int destMax, short[] frames, int perframe) //TODO: use 'out' keyword to get the encoded frame
        {
            ThrowIfDisposed();

            return ToxAvFunctions.PrepareAudioFrame(_toxAv, callIndex, dest, destMax, frames, perframe);
        }

        /// <summary>
        /// Retrieves the state of a call.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <returns></returns>
        public ToxAvCallState GetCallState(int callIndex)
        {
            ThrowIfDisposed();

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
            ThrowIfDisposed();

            return ToxAvFunctions.ChangeSettings(_toxAv, callIndex, ref settings);
        }

        /// <summary>
        /// Retrieves a peer's codec settings.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <param name="peer"></param>
        /// <returns></returns>
        public ToxAvCodecSettings GetPeerCodecSettings(int callIndex, int peer)
        {
            ThrowIfDisposed();

            ToxAvCodecSettings settings = new ToxAvCodecSettings();
            ToxAvFunctions.GetPeerCodecSettings(_toxAv, callIndex, peer, ref settings);

            return settings;
        }

        /// <summary>
        /// Creates a new audio groupchat.
        /// </summary>
        /// <returns></returns>
        public int AddAvGroupchat()
        {
            ThrowIfDisposed();

            ToxAvDelegates.GroupAudioReceiveCallback callback = (IntPtr tox, int groupNumber, int peerNumber, IntPtr frame, uint sampleCount, byte channels, uint sampleRate, IntPtr userData) =>
            {
                if (OnReceivedGroupAudio != null)
                {
                    short[] samples = new short[sampleCount * channels];
                    Marshal.Copy(frame, samples, 0, samples.Length);

                    OnReceivedGroupAudio(this, new ToxAvEventArgs.GroupAudioDataEventArgs(groupNumber, peerNumber, samples, (int)channels, (int)sampleRate));
                }
            };

            int result = ToxAvFunctions.AddAvGroupchat(_tox, callback, IntPtr.Zero);
            if (result != -1)
                _groupAudioHandlers.Add(callback);

            return result;
        }

        /// <summary>
        /// Joins an audio groupchat.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public int JoinAvGroupchat(int friendNumber, byte[] data)
        {
            ThrowIfDisposed();

            if (data == null)
                throw new ArgumentNullException("data");

            ToxAvDelegates.GroupAudioReceiveCallback callback = (IntPtr tox, int groupNumber, int peerNumber, IntPtr frame, uint sampleCount, byte channels, uint sampleRate, IntPtr userData) =>
            {
                if (OnReceivedGroupAudio != null)
                {
                    short[] samples = new short[sampleCount * channels];
                    Marshal.Copy(frame, samples, 0, samples.Length);

                    OnReceivedGroupAudio(this, new ToxAvEventArgs.GroupAudioDataEventArgs(groupNumber, peerNumber, samples, (int)channels, (int)sampleRate));
                }
            };

            int result = ToxAvFunctions.JoinAvGroupchat(_tox, friendNumber, data, (ushort)data.Length, callback, IntPtr.Zero);
            if (result != -1)
                _groupAudioHandlers.Add(callback);

            return result;
        }

        /// <summary>
        /// Sends an audio frame to a group.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="pcm"></param>
        /// <param name="perframe"></param>
        /// <param name="channels"></param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        public bool GroupSendAudio(int groupNumber, short[] pcm, int perframe, int channels, int sampleRate)
        {
            ThrowIfDisposed();

            return ToxAvFunctions.GroupSendAudio(_tox, groupNumber, pcm, (uint)perframe, (byte)channels, (uint)sampleRate) == 0;
        }

        public int PrepareVideoFrame(int callIndex, byte[] dest, IntPtr img)
        {
            ThrowIfDisposed();

            if (dest == null)
                throw new ArgumentNullException("dest");

            return ToxAvFunctions.PrepareVideoFrame(_toxAv, callIndex, dest, dest.Length, img);
        }

        public ToxAvError SendVideo(int callIndex, byte[] frame)
        {
            ThrowIfDisposed();

            if (frame == null)
                throw new ArgumentNullException("frame");

            return ToxAvFunctions.SendVideo(_toxAv, callIndex, frame, (uint)frame.Length);
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
                            _onCancel(this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnCancel));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onCancelCallback, ToxAvCallbackID.OnCancel, IntPtr.Zero);
                }

                _onCancel += value;
            }
            remove
            {
                if (_onCancel.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, null, ToxAvCallbackID.OnCancel, IntPtr.Zero);
                    _onCancelCallback = null;
                }

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
                            _onEnd(this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnEnd));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onEndCallback, ToxAvCallbackID.OnEnd, IntPtr.Zero);
                }

                _onEnd += value;
            }
            remove
            {
                if (_onEnd.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, null, ToxAvCallbackID.OnEnd, IntPtr.Zero);
                    _onEndCallback = null;
                }

                _onEnd -= value;
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
                            _onInvite(this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnInvite));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onInviteCallback, ToxAvCallbackID.OnInvite, IntPtr.Zero);
                }

                _onInvite += value;
            }
            remove
            {
                if (_onInvite.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, null, ToxAvCallbackID.OnInvite, IntPtr.Zero);
                    _onInviteCallback = null;
                }

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
                            _onPeerTimeout(this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnPeerTimeout));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onPeerTimeoutCallback, ToxAvCallbackID.OnPeerTimeout, IntPtr.Zero);
                }

                _onPeerTimeout += value;
            }
            remove
            {
                if (_onPeerTimeout.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, null, ToxAvCallbackID.OnPeerTimeout, IntPtr.Zero);
                    _onPeerTimeoutCallback = null;
                }

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
                            _onReject(this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnReject));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onRejectCallback, ToxAvCallbackID.OnReject, IntPtr.Zero);
                }

                _onReject += value;
            }
            remove
            {
                if (_onReject.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, null, ToxAvCallbackID.OnReject, IntPtr.Zero);
                    _onRejectCallback = null;
                }

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
                            _onRequestTimeout(this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnRequestTimeout));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onRequestTimeoutCallback, ToxAvCallbackID.OnRequestTimeout, IntPtr.Zero);
                }

                _onRequestTimeout += value;
            }
            remove
            {
                if (_onRequestTimeout.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, null, ToxAvCallbackID.OnRequestTimeout, IntPtr.Zero);
                    _onRequestTimeoutCallback = null;
                }

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
                            _onRinging(this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnRinging));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onRingingCallback, ToxAvCallbackID.OnRinging, IntPtr.Zero);
                }

                _onRinging += value;
            }
            remove
            {
                if (_onRinging.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, null, ToxAvCallbackID.OnRinging, IntPtr.Zero);
                    _onRingingCallback = null;
                }

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
                            _onStart(this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnStart));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onStartCallback, ToxAvCallbackID.OnStart, IntPtr.Zero);
                }

                _onStart += value;
            }
            remove
            {
                if (_onStart.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, null, ToxAvCallbackID.OnStart, IntPtr.Zero);
                    _onStartCallback = null;
                }

                _onStart -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.CallStateEventArgs> _onPeerCSChange;

        /// <summary>
        /// Occurs when a peer wants to change the call type.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnPeerCodecSettingsChanged
        {
            add
            {
                if (_onPeerCSChangeCallback == null)
                {
                    _onPeerCSChangeCallback = (IntPtr agent, int callIndex, IntPtr args) =>
                    {
                        if (_onPeerCSChange != null)
                            _onPeerCSChange(this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnPeerCSChange));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onPeerCSChangeCallback, ToxAvCallbackID.OnPeerCSChange, IntPtr.Zero);
                }

                _onPeerCSChange += value;
            }
            remove
            {
                if (_onPeerCSChange.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, null, ToxAvCallbackID.OnPeerCSChange, IntPtr.Zero);
                    _onPeerCSChangeCallback = null;
                }

                _onPeerCSChange -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.CallStateEventArgs> _onSelfCSChange;

        /// <summary>
        /// Occurs when a peer wants to change the call type.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnSelfCodecSettingsChanged
        {
            add
            {
                if (_onSelfCSChangeCallback == null)
                {
                    _onSelfCSChangeCallback = (IntPtr agent, int callIndex, IntPtr args) =>
                    {
                        if (_onSelfCSChange != null)
                            _onSelfCSChange(this, new ToxAvEventArgs.CallStateEventArgs(callIndex, ToxAvCallbackID.OnSelfCSChange));
                    };

                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, _onSelfCSChangeCallback, ToxAvCallbackID.OnSelfCSChange, IntPtr.Zero);
                }

                _onSelfCSChange += value;
            }
            remove
            {
                if (_onSelfCSChange.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterCallstateCallback(_toxAv, null, ToxAvCallbackID.OnSelfCSChange, IntPtr.Zero);
                    _onSelfCSChangeCallback = null;
                }

                _onSelfCSChange -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.AudioDataEventArgs> _onReceivedAudio;

        /// <summary>
        /// Occurs when an audio frame was received. Note: doesn't use 'Invoker'.
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

                            _onReceivedAudio(this, new ToxAvEventArgs.AudioDataEventArgs(callIndex, samples));
                        }
                    };

                    ToxAvFunctions.RegisterAudioReceiveCallback(_toxAv, _onReceivedAudioCallback, IntPtr.Zero);
                }

                _onReceivedAudio += value;
            }
            remove
            {
                if (_onReceivedAudio.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterAudioReceiveCallback(_toxAv, null, IntPtr.Zero);
                    _onReceivedAudioCallback = null;
                }

                _onReceivedAudio -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.VideoDataEventArgs> _onReceivedVideo;

        /// <summary>
        /// Occurs when a video frame was received. Note: doesn't use 'Invoker'.
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
                            _onReceivedVideo(this, new ToxAvEventArgs.VideoDataEventArgs(callIndex, frame));
                    };

                    ToxAvFunctions.RegisterVideoReceiveCallback(_toxAv, _onReceivedVideoCallback, IntPtr.Zero);
                }

                _onReceivedVideo += value;
            }
            remove
            {
                if (_onReceivedVideo.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterVideoReceiveCallback(_toxAv, null, IntPtr.Zero);
                    _onReceivedVideoCallback = null;
                }

                _onReceivedVideo -= value;
            }
        }

        /// <summary>
        /// Occurs when an audio was received from a group. Note: doesn't use 'Invoker'.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.GroupAudioDataEventArgs> OnReceivedGroupAudio;

        #endregion

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
