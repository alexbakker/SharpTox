#pragma warning disable 1591

namespace SharpTox.Av
{
    public struct ToxAvCodecSettings
    {
        public uint video_bitrate; /* In bits/s */
        public ushort max_video_width; /* In px */
        public ushort max_video_height; /* In px */

        public uint audio_bitrate; /* In bits/s */
        public ushort audio_frame_duration; /* In ms */
        public uint audio_sample_rate; /* In Hz */
        public uint audio_channels;
        public int audio_VAD_tolerance; /* In ms */

        public uint jbuf_capacity; /* Size of jitter buffer */
    }
}

#pragma warning restore 1591
