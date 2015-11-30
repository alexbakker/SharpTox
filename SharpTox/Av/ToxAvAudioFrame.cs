using System;
using System.Runtime.InteropServices;

namespace SharpTox.Av
{
    public class ToxAvAudioFrame
    {
        public short[] Data { get; private set; }

        public int SamplingRate { get; private set; }
        public int Channels { get; private set; }

        public ToxAvAudioFrame(short[] data, int samplingRate, int channels)
        {
            Data = data;
            SamplingRate = samplingRate;
            Channels = channels;
        }

        internal ToxAvAudioFrame(IntPtr data, uint sampleCount, uint samplingRate, byte channels)
        {
            uint length = sampleCount * channels;
            Data = new short[length];

            Marshal.Copy(data, Data, 0, Data.Length);

            SamplingRate = (int)samplingRate;
            Channels = channels;
        }
    }
}
