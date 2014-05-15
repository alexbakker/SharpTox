#pragma warning disable 1591

using System;

namespace SharpTox
{
    public class ToxAv
    {
		private ToxAvDelegates.CallstateCallback callstatecallback;

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

			callbacks ();
        }

		private void callbacks()
		{
			ToxAvFunctions.RegisterCallstateCallback(callstatecallback = new ToxAvDelegates.CallstateCallback((IntPtr args) =>
			{
				//if (OnStatusMessage != null)
				//	Invoker(OnStatusMessage, friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(newstatus)));
			}), ToxAvCallbackID.OnCancel);
		}
	}
}

#pragma warning restore 1591
