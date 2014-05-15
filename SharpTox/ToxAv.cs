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

		private ToxAvDelegates.CallstateCallback callstatecallback;

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
            toxav = ToxAvFunctions.New(tox, settings);

			obj = new object ();
			Invoker = new InvokeDelegate(dummyinvoker);

			//callbacks ();
        }

		private object dummyinvoker(Delegate method, params object[] p)
		{
			return method.DynamicInvoke(p);
		}

		private void callbacks()
		{
			ToxAvFunctions.RegisterCallstateCallback(callstatecallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnCancel != null)
					OnCancel(args);
			}), ToxAvCallbackID.OnCancel);

			ToxAvFunctions.RegisterCallstateCallback(callstatecallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnEnd != null)
					OnEnd(args);
			}), ToxAvCallbackID.OnEnd);

			ToxAvFunctions.RegisterCallstateCallback(callstatecallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnEnding != null)
					OnEnding(args);
			}), ToxAvCallbackID.OnEnding);

			ToxAvFunctions.RegisterCallstateCallback(callstatecallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnError != null)
					OnError(args);
			}), ToxAvCallbackID.OnError);

			ToxAvFunctions.RegisterCallstateCallback(callstatecallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnInvite != null)
					OnInvite(args);
			}), ToxAvCallbackID.OnInvite);

			ToxAvFunctions.RegisterCallstateCallback(callstatecallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnPeerTimeout != null)
					OnPeerTimeout(args);
			}), ToxAvCallbackID.OnPeerTimeout);

			ToxAvFunctions.RegisterCallstateCallback(callstatecallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnReject != null)
					OnReject(args);
			}), ToxAvCallbackID.OnReject);

			ToxAvFunctions.RegisterCallstateCallback(callstatecallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnRequestTimeout != null)
					OnRequestTimeout(args);
			}), ToxAvCallbackID.OnRequestTimeout);

			ToxAvFunctions.RegisterCallstateCallback(callstatecallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnRinging != null)
					OnRinging(args);
			}), ToxAvCallbackID.OnRinging);

			ToxAvFunctions.RegisterCallstateCallback(callstatecallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnStart != null)
					OnStart(args);
			}), ToxAvCallbackID.OnStart);

			ToxAvFunctions.RegisterCallstateCallback(callstatecallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				if (OnStarting != null)
					OnStarting(args);
			}), ToxAvCallbackID.OnStarting);
		}
	}
}

#pragma warning restore 1591
