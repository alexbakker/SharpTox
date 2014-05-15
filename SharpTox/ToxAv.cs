#pragma warning disable 1591

using System;

namespace SharpTox
{
	public delegate void CallstateChangedDelegate(IntPtr args);

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

		public delegate object InvokeDelegate(Delegate method, params object[] p);

		/// <summary>
		/// The invoke delegate to use when raising events.
		/// </summary>
		public InvokeDelegate Invoker;
		private object obj;

        public static ToxAvCodecSettings DefaultSettings = new ToxAvCodecSettings() 
		{
            video_bitrate = 1000000,
            video_width = 800,
            video_height = 600,

            audio_bitrate = 64000,
            audio_frame_duration = 20,
            audio_sample_rate = 48000,
            audio_channels = 1,

            jbuf_capacity = 20
        };

        private IntPtr toxav;

        public ToxAv(IntPtr tox, ToxAvCodecSettings settings)
        {
			toxav = ToxAvFunctions.New (tox, settings);

			obj = new object ();
			Invoker = new InvokeDelegate(dummyinvoker);

			callbacks ();
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

		public ToxAvError Cancel(int friend_number, string reason)
		{
			lock (obj)
			{
				if (toxav == IntPtr.Zero)
					throw null;

				return ToxAvFunctions.Cancel (toxav, friend_number, reason);
			}
		}

		public ToxAvError Answer(ToxAvCallType call_type)
		{
			lock (obj)
			{
				if (toxav == IntPtr.Zero)
					throw null;

				return ToxAvFunctions.Answer (toxav, call_type);
			}
		}

		public ToxAvError Call(int friend_number, ToxAvCallType call_type, int ringing_seconds)
		{
			lock (obj)
			{
				if (toxav == IntPtr.Zero)
					throw null;

				return ToxAvFunctions.Call (toxav, friend_number, call_type, ringing_seconds);
			}
		}

        public ToxAvError Hangup()
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.Hangup(toxav);
            }
        }

        public ToxAvError Reject(string reason)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.Reject(toxav, reason);
            }
        }

        public ToxAvError StopCall()
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.StopCall(toxav);
            }
        }

        public ToxAvError PrepareTransmission(bool support_video)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.PrepareTransmission(toxav, support_video);
            }
        }

        public ToxAvError KillTransmission()
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.KillTransmission(toxav);
            }
        }

        public int GetPeerID(int peer)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.GetPeerID(toxav, peer);
            }
        }

        public bool CapabilitySupported(ToxAvCapabilities capability)
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.CapabilitySupported(toxav, capability);
            }
        }

        public IntPtr GetTox()
        {
            lock (obj)
            {
                if (toxav == IntPtr.Zero)
                    throw null;

                return ToxAvFunctions.GetTox(toxav);
            }
        }

		private object dummyinvoker(Delegate method, params object[] p)
		{
			return method.DynamicInvoke(p);
		}

		private void callbacks()
		{
			ToxAvFunctions.RegisterCallstateCallback(oncancelcallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnCancel != null)
					Invoker(OnCancel, args);
			}), ToxAvCallbackID.OnCancel);

			ToxAvFunctions.RegisterCallstateCallback(onendcallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnEnd != null)
					Invoker(OnEnd, args);
			}), ToxAvCallbackID.OnEnd);

			ToxAvFunctions.RegisterCallstateCallback(onendingcallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnEnding != null)
					Invoker(OnEnding, args);
			}), ToxAvCallbackID.OnEnding);

			ToxAvFunctions.RegisterCallstateCallback(onerrorcallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnError != null)
					Invoker(OnError, args);
			}), ToxAvCallbackID.OnError);

			ToxAvFunctions.RegisterCallstateCallback(oninvitecallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnInvite != null)
					Invoker(OnInvite, args);
			}), ToxAvCallbackID.OnInvite);

			ToxAvFunctions.RegisterCallstateCallback(onpeertimeoutcallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnPeerTimeout != null)
					Invoker(OnPeerTimeout, args);
			}), ToxAvCallbackID.OnPeerTimeout);

			ToxAvFunctions.RegisterCallstateCallback(onrejectcallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnReject != null)
					Invoker(OnReject, args);
			}), ToxAvCallbackID.OnReject);

			ToxAvFunctions.RegisterCallstateCallback(onrequesttimeoutcallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnRequestTimeout != null)
					Invoker(OnRequestTimeout, args);
			}), ToxAvCallbackID.OnRequestTimeout);

			ToxAvFunctions.RegisterCallstateCallback(onringingcallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnRinging != null)
					Invoker(OnRinging, args);
			}), ToxAvCallbackID.OnRinging);

			ToxAvFunctions.RegisterCallstateCallback(onstartcallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnStart != null)
					Invoker(OnStart, args);
			}), ToxAvCallbackID.OnStart);

			ToxAvFunctions.RegisterCallstateCallback(onstartingcallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnStarting != null)
					Invoker(OnStarting, args);
			}), ToxAvCallbackID.OnStarting);
		}
	}
}

#pragma warning restore 1591
