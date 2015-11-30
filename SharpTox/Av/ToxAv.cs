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
        #region Callback delegates
        private ToxAvDelegates.CallCallback _onCallCallback;
        private ToxAvDelegates.CallStateCallback _onCallStateCallback;
        private ToxAvDelegates.AudioReceiveFrameCallback _onReceiveAudioFrameCallback;
        private ToxAvDelegates.VideoReceiveFrameCallback _onReceiveVideoFrameCallback;
        private ToxAvDelegates.BitrateStatusCallback _onBitrateStatusCallback;
        #endregion

        private List<ToxAvDelegates.GroupAudioReceiveCallback> _groupAudioHandlers = new List<ToxAvDelegates.GroupAudioReceiveCallback>();
        private bool _disposed = false;
        private bool _running = false;
        private CancellationTokenSource _cancelTokenSource;

        private ToxAvHandle _toxAv;

        /// <summary>
        /// The handle of this toxav instance.
        /// </summary>
        internal ToxAvHandle Handle
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
        internal ToxHandle ToxHandle
        {
            get
            {
                return _tox;
            }
        }

        /// <summary>
        /// Initialises a new instance of toxav.
        /// </summary>
        /// <param name="tox"></param>
        internal ToxAv(ToxHandle tox)
        {
            _tox = tox;

            var error = ToxAvErrorNew.Ok;
            _toxAv = ToxAvFunctions.New(tox, ref error);

            if (_toxAv == null || _toxAv.IsInvalid || error != ToxAvErrorNew.Ok)
                throw new Exception("Could not create a new instance of toxav.");

            //register audio/video callbacks early on
            //due to toxav being silly, we can't start calls without registering those beforehand
            RegisterAudioVideoCallbacks();
        }

        /// <summary>
        /// Initialises a new instance of toxav.
        /// </summary>
        /// <param name="tox"></param>
        public ToxAv(Tox tox)
            : this(tox.Handle) { }

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

            if (_toxAv != null && !_toxAv.IsInvalid && !_toxAv.IsClosed)
                _toxAv.Dispose();

            _disposed = true;
        }

        private void RegisterAudioVideoCallbacks()
        {
            _onReceiveAudioFrameCallback = (IntPtr toxAv, uint friendNumber, IntPtr pcm, uint sampleCount, byte channels, uint samplingRate, IntPtr userData) =>
            {
                if (OnAudioFrameReceived != null)
                    OnAudioFrameReceived(this, new ToxAvEventArgs.AudioFrameEventArgs(ToxTools.Map(friendNumber), new ToxAvAudioFrame(pcm, sampleCount, samplingRate, channels)));
            };

            _onReceiveVideoFrameCallback = (IntPtr toxAv, uint friendNumber, ushort width, ushort height, IntPtr y, IntPtr u, IntPtr v, int yStride, int uStride, int vStride, IntPtr userData) =>
            {
                if (OnVideoFrameReceived != null)
                    OnVideoFrameReceived(this, new ToxAvEventArgs.VideoFrameEventArgs(ToxTools.Map(friendNumber), new ToxAvVideoFrame(width, height, y, u, v, yStride, uStride, vStride)));
            };

            ToxAvFunctions.RegisterAudioReceiveFrameCallback(_toxAv, _onReceiveAudioFrameCallback, IntPtr.Zero);
            ToxAvFunctions.RegisterVideoReceiveFrameCallback(_toxAv, _onReceiveVideoFrameCallback, IntPtr.Zero);
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
                    await Task.Delay(delay / 4);
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
            ToxAvFunctions.Iterate(_toxAv);
            return (int)ToxAvFunctions.IterationInterval(_toxAv);
        }

        public bool Call(int friendNumber, int audioBitrate, int videoBitrate, out ToxAvErrorCall error)
        {
            ThrowIfDisposed();

            error = ToxAvErrorCall.Ok;
            return ToxAvFunctions.Call(_toxAv, ToxTools.Map(friendNumber), (uint)audioBitrate, (uint)videoBitrate, ref error);
        }

        public bool Call(int friendNumber, int audioBitrate, int videoBitrate)
        {
            var error = ToxAvErrorCall.Ok;
            return Call(friendNumber, audioBitrate, videoBitrate, out error);
        }

        public bool Answer(int friendNumber, int audioBitrate, int videoBitrate, out ToxAvErrorAnswer error)
        {
            ThrowIfDisposed();

            error = ToxAvErrorAnswer.Ok;
            return ToxAvFunctions.Answer(_toxAv, ToxTools.Map(friendNumber), (uint)audioBitrate, (uint)videoBitrate, ref error);
        }

        public bool Answer(int friendNumber, int audioBitrate, int videoBitrate)
        {
            var error = ToxAvErrorAnswer.Ok;
            return Answer(friendNumber, audioBitrate, videoBitrate, out error);
        }

        public bool SendControl(int friendNumber, ToxAvCallControl control, out ToxAvErrorCallControl error)
        {
            ThrowIfDisposed();

            error = ToxAvErrorCallControl.Ok;
            return ToxAvFunctions.CallControl(_toxAv, ToxTools.Map(friendNumber), control, ref error);
        }

        public bool SendControl(int friendNumber, ToxAvCallControl control)
        {
            var error = ToxAvErrorCallControl.Ok;
            return SendControl(friendNumber, control, out error);
        }

        public bool SetAudioBitrate(int friendNumber, int bitrate, out ToxAvErrorSetBitrate error)
        {
            ThrowIfDisposed();

            error = ToxAvErrorSetBitrate.Ok;
            return ToxAvFunctions.BitrateSet(_toxAv, ToxTools.Map(friendNumber), bitrate, -1, ref error);
        }

        public bool SetAudioBitrate(int friendNumber, int bitrate)
        {
            var error = ToxAvErrorSetBitrate.Ok;
            return SetAudioBitrate(friendNumber, bitrate, out error);
        }

        public bool SetVideoBitrate(int friendNumber, int bitrate, out ToxAvErrorSetBitrate error)
        {
            ThrowIfDisposed();

            error = ToxAvErrorSetBitrate.Ok;
            return ToxAvFunctions.BitrateSet(_toxAv, ToxTools.Map(friendNumber), -1, bitrate, ref error);
        }

        public bool SetVideoBitrate(int friendNumber, int bitrate)
        {
            var error = ToxAvErrorSetBitrate.Ok;
            return SetVideoBitrate(friendNumber, bitrate, out error);
        }

        public bool SendVideoFrame(int friendNumber, ToxAvVideoFrame frame, out ToxAvErrorSendFrame error)
        {
            ThrowIfDisposed();

            error = ToxAvErrorSendFrame.Ok;
            return ToxAvFunctions.VideoSendFrame(_toxAv, ToxTools.Map(friendNumber), (ushort)frame.Width, (ushort)frame.Height, frame.Y, frame.U, frame.V, ref error);
        }

        public bool SendVideoFrame(int friendNumber, ToxAvVideoFrame frame)
        {
            var error = ToxAvErrorSendFrame.Ok;
            return SendVideoFrame(friendNumber, frame, out error);
        }

        public bool SendAudioFrame(int friendNumber, ToxAvAudioFrame frame, out ToxAvErrorSendFrame error)
        {
            ThrowIfDisposed();

            error = ToxAvErrorSendFrame.Ok;
            return ToxAvFunctions.AudioSendFrame(_toxAv, ToxTools.Map(friendNumber), frame.Data, (uint)(frame.Data.Length / frame.Channels), (byte)frame.Channels, (uint)frame.SamplingRate, ref error);
        }

        public bool SendAudioFrame(int friendNumber, ToxAvAudioFrame frame)
        {
            var error = ToxAvErrorSendFrame.Ok;
            return SendAudioFrame(friendNumber, frame, out error);
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

        #region Events
        private EventHandler<ToxAvEventArgs.CallRequestEventArgs> _onCallRequestReceived;

        /// <summary>
        /// Occurs when a friend sends a call request.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallRequestEventArgs> OnCallRequestReceived
        {
            add
            {
                if (_onCallCallback == null)
                {
                    _onCallCallback = (IntPtr toxAv, uint friendNumber, bool audioEnabled, bool videoEnabled, IntPtr userData) =>
                    {
                        if (_onCallRequestReceived != null)
                            _onCallRequestReceived(this, new ToxAvEventArgs.CallRequestEventArgs(ToxTools.Map(friendNumber), audioEnabled, videoEnabled));
                    };

                    ToxAvFunctions.RegisterCallCallback(_toxAv, _onCallCallback, IntPtr.Zero);
                }

                _onCallRequestReceived += value;
            }
            remove
            {
                if (_onCallRequestReceived.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterCallCallback(_toxAv, null, IntPtr.Zero);
                    _onCallCallback = null;
                }

                _onCallRequestReceived -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.CallStateEventArgs> _onCallStateChanged;

        /// <summary>
        /// Occurs when the state of a call changed.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnCallStateChanged
        {
            add
            {
                if (_onCallStateCallback == null)
                {
                    _onCallStateCallback = (IntPtr toxAv, uint friendNumber, ToxAvFriendCallState state, IntPtr userData) =>
                    {
                        if (_onCallStateChanged != null)
                            _onCallStateChanged(this, new ToxAvEventArgs.CallStateEventArgs(ToxTools.Map(friendNumber), state));
                    };

                    ToxAvFunctions.RegisterCallStateCallback(_toxAv, _onCallStateCallback, IntPtr.Zero);
                }

                _onCallStateChanged += value;
            }
            remove
            {
                if (_onCallStateChanged.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterCallStateCallback(_toxAv, null, IntPtr.Zero);
                    _onCallStateCallback = null;
                }

                _onCallStateChanged -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.BitrateStatusEventArgs> _onBitrateSuggestion;

        /// <summary>
        /// Occurs when a friend changed their bitrate during a call.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.BitrateStatusEventArgs> OnBitrateSuggestion
        {
            add
            {
                if (_onBitrateStatusCallback == null)
                {
                    _onBitrateStatusCallback = (IntPtr toxAv, uint friendNumber, uint audioBitrate, uint videoBitrate, IntPtr userData) =>
                    {
                            if (_onBitrateSuggestion != null)
                                _onBitrateSuggestion(this, new ToxAvEventArgs.BitrateStatusEventArgs(ToxTools.Map(friendNumber), (int)audioBitrate, (int)videoBitrate));
                    };

                    ToxAvFunctions.RegisterBitrateStatusCallback(_toxAv, _onBitrateStatusCallback, IntPtr.Zero);
                }

                _onBitrateSuggestion += value;
            }
            remove
            {
                if (_onBitrateSuggestion.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterBitrateStatusCallback(_toxAv, null, IntPtr.Zero);
                    _onBitrateStatusCallback = null;
                }

                _onBitrateSuggestion -= value;
            }
        }

        /// <summary>
        /// Occurs when an video frame is received.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.VideoFrameEventArgs> OnVideoFrameReceived;

        /// <summary>
        /// Occurs when an audio frame is received.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.AudioFrameEventArgs> OnAudioFrameReceived;

        /// <summary>
        /// Occurs when an audio frame was received from a group.
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
