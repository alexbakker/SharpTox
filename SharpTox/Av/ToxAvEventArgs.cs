using System;

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
                Data = (short[])data.Clone();
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
    }
}
