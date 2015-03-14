using System;
using SharpTox.Core;

namespace SharpTox.Av
{
    public class ToxAvEventArgs
    {
        public abstract class CallBaseEventArgs : EventArgs
        {
            public int CallIndex { get; private set; }

            protected CallBaseEventArgs(int callIndex)
            {
                CallIndex = callIndex;
            }
        }

        public class CallStateEventArgs : CallBaseEventArgs
        {
            public ToxAvCallbackID Id { get; private set; }

            public CallStateEventArgs(int callIndex, ToxAvCallbackID id)
                : base(callIndex)
            {
                Id = id;
            }
        }

        public class AudioDataEventArgs : CallBaseEventArgs
        {
            public short[] Data { get; private set; }

            public AudioDataEventArgs(int callIndex, short[] data)
                : base(callIndex)
            {
                Data = data;
            }
        }

        public class VideoDataEventArgs : CallBaseEventArgs
        {
            public IntPtr Frame { get; private set; }

            public VideoDataEventArgs(int callIndex, IntPtr frame)
                : base(callIndex)
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
                Data = data;
                Channels = channels;
                SampleRate = sampleRate;
            }
        }
    }
}
