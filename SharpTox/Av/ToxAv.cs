using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SharpTox.Core;

namespace SharpTox.Av
{
    /// <summary>
    /// Represents an instance of toxav.
    /// </summary>
    public class ToxAv : IDisposable
    {
        #region Callback delegates
        private ToxAvDelegates.FrameRequestCallback _onAudioFrameRequestCallback;
        private ToxAvDelegates.CallCallback _onCallCallback;
        private ToxAvDelegates.CallStateCallback _onCallStateCallback;
        private ToxAvDelegates.ReceiveAudioFrameCallback _onReceiveAudioFrameCallback;
        private ToxAvDelegates.ReceiveVideoFrameCallback _onReceiveVideoFrameCallback;
        private ToxAvDelegates.FrameRequestCallback _onVideoFrameRequestCallback;
        #endregion

        private List<ToxAvDelegates.GroupAudioReceiveCallback> _groupAudioHandlers = new List<ToxAvDelegates.GroupAudioReceiveCallback>();
        private bool _disposed = false;
        private bool _running = false;
        private CancellationTokenSource _cancelTokenSource;

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
        /// Initialises a new instance of toxav.
        /// </summary>
        /// <param name="tox"></param>
        public ToxAv(ToxHandle tox)
        {
            _tox = tox;

            var error = ToxAvErrorNew.Ok;
            _toxAv = ToxAvFunctions.New(tox, ref error);

            if (_toxAv == null || _toxAv.IsInvalid || error != ToxAvErrorNew.Ok)
                throw new Exception("Could not create a new instance of toxav.");
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

            if (!_toxAv.IsInvalid && !_toxAv.IsClosed && _toxAv != null)
                _toxAv.Dispose();

            _disposed = true;
        }

        /// <summary>
        /// Starts the main toxav_do loop.
        /// </summary>
        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_running)
                return;

            Loop();
        }

        /// <summary>
        /// Stops the main toxav_do loop if it's running.
        /// </summary>
        public void Stop()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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

            Task.Factory.StartNew(async() =>
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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxAvErrorCall.Ok;
            return ToxAvFunctions.Call(_toxAv, (uint)friendNumber, (uint)audioBitrate, (uint)videoBitrate, ref error);
        }

        public bool Call(int friendNumber, int audioBitrate, int videoBitrate)
        {
            var error = ToxAvErrorCall.Ok;
            return Call(friendNumber, audioBitrate, videoBitrate, out error);
        }

        public bool Answer(int friendNumber, int audioBitrate, int videoBitrate, out ToxAvErrorAnswer error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxAvErrorAnswer.Ok;
            return ToxAvFunctions.Answer(_toxAv, (uint)friendNumber, (uint)audioBitrate, (uint)videoBitrate, ref error);
        }

        public bool Answer(int friendNumber, int audioBitrate, int videoBitrate)
        {
            var error = ToxAvErrorAnswer.Ok;
            return Answer(friendNumber, audioBitrate, videoBitrate, out error);
        }

        public bool SendControl(int friendNumber, ToxAvCallControl control, out ToxAvErrorCallControl error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxAvErrorCallControl.Ok;
            return ToxAvFunctions.CallControl(_toxAv, (uint)friendNumber, control, ref error);
        }

        public bool SendControl(int friendNumber, ToxAvCallControl control)
        {
            var error = ToxAvErrorCallControl.Ok;
            return SendControl(friendNumber, control, out error);
        }

        public bool SetAudioBitrate(int friendNumber, int bitrate, out ToxAvErrorBitrate error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxAvErrorBitrate.Ok;
            return ToxAvFunctions.SetAudioBitrate(_toxAv, (uint)friendNumber, (uint)bitrate, ref error);
        }

        public bool SetAudioBitrate(int friendNumber, int bitrate)
        {
            var error = ToxAvErrorBitrate.Ok;
            return SetAudioBitrate(friendNumber, bitrate, out error);
        }

        public bool SetVideoBitrate(int friendNumber, int bitrate, out ToxAvErrorBitrate error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxAvErrorBitrate.Ok;
            return ToxAvFunctions.SetVideoBitrate(_toxAv, (uint)friendNumber, (uint)bitrate, ref error);
        }

        public bool SetVideoBitrate(int friendNumber, int bitrate)
        {
            var error = ToxAvErrorBitrate.Ok;
            return SetVideoBitrate(friendNumber, bitrate, out error);
        }

        public bool SendVideoFrame(int friendNumber, ToxAvVideoFrame frame, out ToxAvErrorSendFrame error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxAvErrorSendFrame.Ok;
            return ToxAvFunctions.SendVideoFrame(_toxAv, (uint)friendNumber, (ushort)frame.Width, (ushort)frame.Height, frame.Y, frame.U, frame.V, ref error);
        }

        public bool SendVideoFrame(int friendNumber, ToxAvVideoFrame frame)
        {
            var error = ToxAvErrorSendFrame.Ok;
            return SendVideoFrame(friendNumber, frame, out error);
        }

        public bool SendAudioFrame(int friendNumber, ToxAvAudioFrame frame, out ToxAvErrorSendFrame error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxAvErrorSendFrame.Ok;
            return ToxAvFunctions.SendAudioFrame(_toxAv, (uint)friendNumber, frame.Data, (uint)(frame.Data.Length / frame.Channels), (byte)frame.Channels, (uint)frame.SamplingRate, ref error);
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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxAvFunctions.GroupSendAudio(_tox, groupNumber, pcm, (uint)perframe, (byte)channels, (uint)sampleRate) == 0;
        }

        #region Events
        private EventHandler<ToxAvEventArgs.CallRequestEventArgs> _onCall;

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
                        if (_onCall != null)
                            _onCall(this, new ToxAvEventArgs.CallRequestEventArgs((int)friendNumber, audioEnabled, videoEnabled));
                    };

                    ToxAvFunctions.RegisterCallCallback(_toxAv, _onCallCallback, IntPtr.Zero);
                }

                _onCall += value;
            }
            remove
            {
                if (_onCall.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterCallCallback(_toxAv, null, IntPtr.Zero);
                    _onCallCallback = null;
                }

                _onCall -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.CallStateEventArgs> _onCallState;

        /// <summary>
        /// Occurs when the state of a call changed.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> OnCallStateChanged
        {
            add
            {
                if (_onCallStateCallback == null)
                {
                    _onCallStateCallback = (IntPtr toxAv, uint friendNumber, ToxAvCallState state, IntPtr userData) =>
                    {
                        if (_onCallState != null)
                            _onCallState(this, new ToxAvEventArgs.CallStateEventArgs((int)friendNumber, state));
                    };

                    ToxAvFunctions.RegisterCallStateCallback(_toxAv, _onCallStateCallback, IntPtr.Zero);
                }

                _onCallState += value;
            }
            remove
            {
                if (_onCallState.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterCallStateCallback(_toxAv, null, IntPtr.Zero);
                    _onCallStateCallback = null;
                }

                _onCallState -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.FrameRequestEventArgs> _onAudioFrameRequest;

        /// <summary>
        /// Occurs when the core requests an audio frame.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.FrameRequestEventArgs> OnAudioFrameRequested
        {
            add
            {
                if (_onAudioFrameRequestCallback == null)
                {
                    _onAudioFrameRequestCallback = (IntPtr toxAv, uint friendNumber, IntPtr userData) =>
                    {
                        if (_onAudioFrameRequest != null)
                            _onAudioFrameRequest(this, new ToxAvEventArgs.FrameRequestEventArgs((int)friendNumber));
                    };

                    ToxAvFunctions.RegisterAudioFrameRequestCallback(_toxAv, _onAudioFrameRequestCallback, IntPtr.Zero);
                }

                _onAudioFrameRequest += value;
            }
            remove
            {
                if (_onAudioFrameRequest.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterAudioFrameRequestCallback(_toxAv, null, IntPtr.Zero);
                    _onAudioFrameRequestCallback = null;
                }

                _onAudioFrameRequest -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.FrameRequestEventArgs> _onVideoFrameRequest;

        /// <summary>
        /// Occurs when the core requests a video frame.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.FrameRequestEventArgs> OnVideoFrameRequested
        {
            add
            {
                if (_onVideoFrameRequestCallback == null)
                {
                    _onVideoFrameRequestCallback = (IntPtr toxAv, uint friendNumber, IntPtr userData) =>
                    {
                        if (_onVideoFrameRequest != null)
                            _onVideoFrameRequest(this, new ToxAvEventArgs.FrameRequestEventArgs((int)friendNumber));
                    };

                    ToxAvFunctions.RegisterVideoFrameRequestCallback(_toxAv, _onVideoFrameRequestCallback, IntPtr.Zero);
                }

                _onVideoFrameRequest += value;
            }
            remove
            {
                if (_onVideoFrameRequest.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterVideoFrameRequestCallback(_toxAv, null, IntPtr.Zero);
                    _onVideoFrameRequestCallback = null;
                }

                _onVideoFrameRequest -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.VideoFrameEventArgs> _onReceiveVideoFrame;

        /// <summary>
        /// Occurs when an video frame is received.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.VideoFrameEventArgs> OnVideoFrameReceived
        {
            add
            {
                if (_onReceiveVideoFrameCallback == null)
                {
                    _onReceiveVideoFrameCallback = (IntPtr toxAv, uint friendNumber, ushort width, ushort height, IntPtr y, IntPtr u, IntPtr v, int yStride, int uStride, int vStride, IntPtr userData) =>
                    {
                        if (_onReceiveVideoFrame != null)
                            _onReceiveVideoFrame(this, new ToxAvEventArgs.VideoFrameEventArgs((int)friendNumber, new ToxAvVideoFrame(width, height, y, u, v)));
                    };

                    ToxAvFunctions.RegisterReceiveVideoFrameCallback(_toxAv, _onReceiveVideoFrameCallback, IntPtr.Zero);
                }

                _onReceiveVideoFrame += value;
            }
            remove
            {
                if (_onReceiveVideoFrameCallback.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterReceiveVideoFrameCallback(_toxAv, null, IntPtr.Zero);
                    _onReceiveVideoFrameCallback = null;
                }

                _onReceiveVideoFrame -= value;
            }
        }

        private EventHandler<ToxAvEventArgs.AudioFrameEventArgs> _onReceiveAudioFrame;

        /// <summary>
        /// Occurs when an audio frame is received.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.AudioFrameEventArgs> OnAudioFrameReceived
        {
            add
            {
                if (_onReceiveAudioFrameCallback == null)
                {
                    _onReceiveAudioFrameCallback = (IntPtr toxAv, uint friendNumber, IntPtr pcm, uint sampleCount, byte channels, uint samplingRate, IntPtr userData) =>
                    {
                        if (_onReceiveAudioFrame != null)
                            _onReceiveAudioFrame(this, new ToxAvEventArgs.AudioFrameEventArgs((int)friendNumber, new ToxAvAudioFrame(pcm, sampleCount, samplingRate, channels)));
                    };

                    ToxAvFunctions.RegisterReceiveAudioFrameCallback(_toxAv, _onReceiveAudioFrameCallback, IntPtr.Zero);
                }

                _onReceiveAudioFrame += value;
            }
            remove
            {
                if (_onReceiveAudioFrameCallback.GetInvocationList().Length == 1)
                {
                    ToxAvFunctions.RegisterReceiveAudioFrameCallback(_toxAv, null, IntPtr.Zero);
                    _onReceiveAudioFrameCallback = null;
                }

                _onReceiveAudioFrame -= value;
            }
        }

        /// <summary>
        /// Occurs when an audio frame was received from a group.
        /// </summary>
        public event EventHandler<ToxAvEventArgs.GroupAudioDataEventArgs> OnReceivedGroupAudio;

        #endregion
    }
}
