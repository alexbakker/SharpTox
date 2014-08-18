using System.Runtime.InteropServices;

namespace SharpTox.Av
{
    /// <summary>
    /// Represents toxav encoding settings.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ToxAvCodecSettings
    {
        /// <summary>
        /// Call type (audio or video).
        /// </summary>
        public ToxAvCallType CallType;

        /// <summary>
        /// Video bitrate in bits/s.
        /// </summary>
        public uint VideoBitrate;

        /// <summary>
        /// Maximum video width in pixels.
        /// </summary>
        public ushort MaxVideoWidth;

        /// <summary>
        /// Maximum video height in pixels.
        /// </summary>
        public ushort MaxVideoHeight;

        /// <summary>
        /// Audio bitrate in bits/s.
        /// </summary>
        public uint AudioBitrate;

        /// <summary>
        /// Audio frame duration in ms.
        /// </summary>
        public ushort AudioFrameDuration;

        /// <summary>
        /// Audio sample rate in Hz.
        /// </summary>
        public uint AudioSampleRate;

        /// <summary>
        /// Audio channels.
        /// </summary>
        public uint AudioChannels;
    }
}
