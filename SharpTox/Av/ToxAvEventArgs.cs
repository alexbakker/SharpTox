using System;
using SharpTox.Core;

namespace SharpTox.Av
{
    public class ToxAvEventArgs
    {
        public abstract class CallBaseEventArgs : EventArgs
        {
            public ToxAvCall Call { get; private set; }

            protected CallBaseEventArgs(ToxAvCall call)
            {
                Call = call;
            }
        }

        public class CallStateEventArgs : CallBaseEventArgs
        {
            public ToxAvCallbackID Id { get; private set; }

            public CallStateEventArgs(ToxAvCall call, ToxAvCallbackID id)
                : base(call)
            {
                Id = id;
            }
        }

        public class AudioDataEventArgs : CallBaseEventArgs
        {
            public short[] Data { get; private set; }

            public AudioDataEventArgs(ToxAvCall call, short[] data)
                : base(call)
            {
                Data = (short[])data.Clone();
            }
        }

        public class VideoDataEventArgs : CallBaseEventArgs
        {
            public IntPtr Frame { get; private set; }

            public VideoDataEventArgs(ToxAvCall call, IntPtr frame)
                : base(call)
            {
                Frame = frame;
            }
        }

        public class GroupAudioDataEventArgs : EventArgs
        {
            public int GroupNumber { get; private set; }
            public int PeerNumber { get; private set; }

            public short[] Data { get; private set; }

            public int Channels { get; private set; }
            public int SampleRate { get; private set; }

            public GroupAudioDataEventArgs(int groupNumber, int peerNumber, short[] data, int channels, int sampleRate)
            {
                GroupNumber = groupNumber;
                PeerNumber = peerNumber;
                Data = (short[])data.Clone();
                Channels = channels;
                SampleRate = sampleRate;
            }
        }
    }
}
