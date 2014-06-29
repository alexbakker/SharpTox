#pragma warning disable 1591

using System;

namespace SharpTox.Av
{
    public delegate void CallstateChangedDelegate(int call_index, IntPtr args);

    public class ToxAv
    {
        public event CallstateChangedDelegate OnCancel;
        public event CallstateChangedDelegate OnEnd;
        public event CallstateChangedDelegate OnEnding;
        public event CallstateChangedDelegate OnError;
        public event CallstateChangedDelegate OnInvite;
        public event CallstateChangedDelegate OnPeerTimeout;
        public event CallstateChangedDelegate OnReject;
        public event CallstateChangedDelegate OnRequestTimeout;
        public event CallstateChangedDelegate OnRinging;
        public event CallstateChangedDelegate OnStart;
        public event CallstateChangedDelegate OnStarting;

        #region Event delegates
        private ToxAvDelegates.CallstateCallback oncancelcallback;
        private ToxAvDelegates.CallstateCallback onendcallback;
        private ToxAvDelegates.CallstateCallback onendingcallback;
        private ToxAvDelegates.CallstateCallback onerrorcallback;
        private ToxAvDelegates.CallstateCallback oninvitecallback;
        private ToxAvDelegates.CallstateCallback onpeertimeoutcallback;
        private ToxAvDelegates.CallstateCallback onrejectcallback;
        private ToxAvDelegates.CallstateCallback onrequesttimeoutcallback;
        private ToxAvDelegates.CallstateCallback onringingcallback;
        private ToxAvDelegates.CallstateCallback onstartcallback;
        private ToxAvDelegates.CallstateCallback onstartingcallback;
        #endregion

        public delegate object InvokeDelegate(Delegate method, params object[] p);

        /// <summary>
        /// The invoke delegate to use when raising events.
        /// </summary>
        public InvokeDelegate Invoker;
        private object obj;

        public readonly ToxAvCodecSettings CodecSettings;

        /// <summary>
        /// The default codec settings.
        /// </summary>
        public static readonly ToxAvCodecSettings DefaultCodecSettings = new ToxAvCodecSettings()
        {
            video_bitrate = 500,
            video_width = 800,
            video_height = 600,

            audio_bitrate = 64000,
            audio_frame_duration = 20,
            audio_sample_rate = 48000,
            audio_channels = 1,
            audio_VAD_tolerance = 600,

            jbuf_capacity = 6
        };

        private IntPtr toxav;

        /// <summary>
        /// The maximum amount of calls this instance of toxav is allowed to have.
        /// </summary>
        public int MaxCalls;

        public ToxAv(IntPtr tox, ToxAvCodecSettings settings, int max_calls)
        {
            toxav = ToxAvFunctions.New(tox, max_calls);

            MaxCalls = max_calls;
            CodecSettings = settings;

            obj = new object();
            Invoker = new InvokeDelegate(dummyinvoker);

            callbacks();
        }

        /// <summary>
        /// Kills this toxav instance.
        /// </summary>
        public void Kill()
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                ToxAvFunctions.Kill(toxav);
            }
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
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.Cancel(toxav, call_index, friend_number, reason);
            }
        }

        /// <summary>
        /// Answers a call.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="call_type"></param>
        /// <returns></returns>
        public ToxAvError Answer(int call_index, ToxAvCallType call_type)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.Answer(toxav, call_index, call_type);
            }
        }

        /// <summary>
        /// Creates a new call.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="friend_number"></param>
        /// <param name="call_type"></param>
        /// <param name="ringing_seconds"></param>
        /// <returns></returns>
        public ToxAvError Call(int friend_number, ToxAvCallType call_type, int ringing_seconds, out int call_index)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.Call(toxav, friend_number, call_type, ringing_seconds, out call_index);
            }
        }

        /// <summary>
        /// Hangs up an in-progress call.
        /// </summary>
        /// <param name="call_index"></param>
        /// <returns></returns>
        public ToxAvError Hangup(int call_index)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.Hangup(toxav, call_index);
            }
        }

        /// <summary>
        /// Rejects an incoming call.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public ToxAvError Reject(int call_index, string reason)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.Reject(toxav, call_index, reason);
            }
        }

        /// <summary>
        /// Stops a call and terminates the transmission without notifying the remote peer.
        /// </summary>
        /// <param name="call_index"></param>
        /// <returns></returns>
        public ToxAvError StopCall(int call_index)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.StopCall(toxav, call_index);
            }
        }

        /// <summary>
        /// Prepares transmission. Must be called before any transmission occurs.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="support_video"></param>
        /// <returns></returns>
        public ToxAvError PrepareTransmission(int call_index, bool support_video)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.PrepareTransmission(toxav, call_index, DefaultCodecSettings, support_video);
            }
        }

        /// <summary>
        /// Kills the transmission of a call. Should be called at the end of the transmission.
        /// </summary>
        /// <param name="call_index"></param>
        /// <returns></returns>
        public ToxAvError KillTransmission(int call_index)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.KillTransmission(toxav, call_index);
            }
        }

        /// <summary>
        /// Get the friend_number of peer participating in conversation
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="peer"></param>
        /// <returns></returns>
        public int GetPeerID(int call_index, int peer)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.GetPeerID(toxav, call_index, peer);
            }
        }

        /// <summary>
        /// Checks whether a certain capability is supported.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="capability"></param>
        /// <returns></returns>
        public bool CapabilitySupported(int call_index, ToxAvCapabilities capability)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.CapabilitySupported(toxav, call_index, capability);
            }
        }

        /// <summary>
        /// Retrieves the tox instance that this toxav instance belongs to.
        /// </summary>
        /// <returns></returns>
        public IntPtr GetTox()
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.GetTox(toxav);
            }
        }

        /// <summary>
        /// Receives a decoded audio frame.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="frame_size"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public int ReceiveAudio(int call_index, int frame_size, short[] dest)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.ReceiveAudio(toxav, call_index, frame_size, dest);
            }
        }

        /// <summary>
        /// Sends an encoded audio frame.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="frame"></param>
        /// <param name="frame_size"></param>
        /// <returns></returns>
        public ToxAvError SendAudio(int call_index, ref byte[] frame, int frame_size)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.SendAudio(toxav, call_index, ref frame, frame_size);
            }
        }

        /// <summary>
        /// Encodes an audio frame.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="dest"></param>
        /// <param name="dest_max"></param>
        /// <param name="frame"></param>
        /// <param name="frame_size"></param>
        /// <returns></returns>
        public int PrepareAudioFrame(int call_index, byte[] dest, int dest_max, ushort[] frame, int frame_size)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.PrepareAudioFrame(toxav, call_index, dest, dest_max, frame, frame_size);
            }
        }

        /// <summary>
        /// Gets the pointer of this toxav instance.
        /// </summary>
        /// <returns></returns>
        public IntPtr GetPointer()
        {
            return toxav;
        }

        /// <summary>
        /// Sets the audio queue limit.
        /// </summary>
        /// <param name="call_index"></param>
        /// <param name="limit"></param>
        public int SetAudioQueueLimit(int call_index, ulong limit)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.SetAudioQueueLimit(toxav, call_index, limit);
            }
        }

        public int HasActivity(int call_index, short[] pcm, ushort frame_size, float ref_energy)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.HasActivity(toxav, call_index, pcm, frame_size, ref_energy);
            }
        }

        private object dummyinvoker(Delegate method, params object[] p)
        {
            return method.DynamicInvoke(p);
        }

        private void callbacks()
        {
            ToxAvFunctions.RegisterCallstateCallback(oncancelcallback = new ToxAvDelegates.CallstateCallback((int call_index, IntPtr args) =>
            {
                if (OnCancel != null)
                    Invoker(OnCancel, call_index, args);
            }), ToxAvCallbackID.OnCancel);

            ToxAvFunctions.RegisterCallstateCallback(onendcallback = new ToxAvDelegates.CallstateCallback((int call_index, IntPtr args) =>
            {
                if (OnEnd != null)
                    Invoker(OnEnd, call_index, args);
            }), ToxAvCallbackID.OnEnd);

            ToxAvFunctions.RegisterCallstateCallback(onendingcallback = new ToxAvDelegates.CallstateCallback((int call_index, IntPtr args) =>
            {
                if (OnEnding != null)
                    Invoker(OnEnding, call_index, args);
            }), ToxAvCallbackID.OnEnding);

            ToxAvFunctions.RegisterCallstateCallback(onerrorcallback = new ToxAvDelegates.CallstateCallback((int call_index, IntPtr args) =>
            {
                if (OnError != null)
                    Invoker(OnError, call_index, args);
            }), ToxAvCallbackID.OnError);

            ToxAvFunctions.RegisterCallstateCallback(oninvitecallback = new ToxAvDelegates.CallstateCallback((int call_index, IntPtr args) =>
            {
                if (OnInvite != null)
                    Invoker(OnInvite, call_index, args);
            }), ToxAvCallbackID.OnInvite);

            ToxAvFunctions.RegisterCallstateCallback(onpeertimeoutcallback = new ToxAvDelegates.CallstateCallback((int call_index, IntPtr args) =>
            {
                if (OnPeerTimeout != null)
                    Invoker(OnPeerTimeout, call_index, args);
            }), ToxAvCallbackID.OnPeerTimeout);

            ToxAvFunctions.RegisterCallstateCallback(onrejectcallback = new ToxAvDelegates.CallstateCallback((int call_index, IntPtr args) =>
            {
                if (OnReject != null)
                    Invoker(OnReject, call_index, args);
            }), ToxAvCallbackID.OnReject);

            ToxAvFunctions.RegisterCallstateCallback(onrequesttimeoutcallback = new ToxAvDelegates.CallstateCallback((int call_index, IntPtr args) =>
            {
                if (OnRequestTimeout != null)
                    Invoker(OnRequestTimeout, call_index, args);
            }), ToxAvCallbackID.OnRequestTimeout);

            ToxAvFunctions.RegisterCallstateCallback(onringingcallback = new ToxAvDelegates.CallstateCallback((int call_index, IntPtr args) =>
            {
                if (OnRinging != null)
                    Invoker(OnRinging, call_index, args);
            }), ToxAvCallbackID.OnRinging);

            ToxAvFunctions.RegisterCallstateCallback(onstartcallback = new ToxAvDelegates.CallstateCallback((int call_index, IntPtr args) =>
            {
                if (OnStart != null)
                    Invoker(OnStart, call_index, args);
            }), ToxAvCallbackID.OnStart);

            ToxAvFunctions.RegisterCallstateCallback(onstartingcallback = new ToxAvDelegates.CallstateCallback((int call_index, IntPtr args) =>
            {
                if (OnStarting != null)
                    Invoker(OnStarting, call_index, args);
            }), ToxAvCallbackID.OnStarting);
        }
    }
}

#pragma warning restore 1591
