using System;
using SharpTox.Core;

namespace SharpTox.Av
{
    public class ToxAvEventArgs
    {
        public abstract class CallBaseEventArgs : EventArgs
        {
            public int FriendNumber { get; private set; }

            protected CallBaseEventArgs(int friendNumber)
            {
                FriendNumber = friendNumber;
            }
        }

        public class CallRequestEventArgs : CallBaseEventArgs
        {
            public ToxAvFriendCallState State { get; private set; }

            public bool AudioEnabled { get; private set; }
            public bool VideoEnabled { get; private set; }

            public CallRequestEventArgs(int friendNumber, bool audioEnabled, bool videoEnabled)
                : base(friendNumber)
            {
                AudioEnabled = audioEnabled;
                VideoEnabled = videoEnabled;
            }
        }

        public class CallStateEventArgs : CallBaseEventArgs
        {
            public ToxAvFriendCallState State { get; private set; }

            public CallStateEventArgs(int friendNumber, ToxAvFriendCallState state)
                : base(friendNumber)
            {
                State = state;
            }
        }

        public class BitrateStatusEventArgs : CallBaseEventArgs
        {
            public int AudioBitrate { get; private set; }
            public int VideoBitrate { get; private set; }

            public BitrateStatusEventArgs(int friendNumber, int audioBitrate, int videoBitrate)
                : base(friendNumber)
            {
                AudioBitrate = audioBitrate;
                VideoBitrate = videoBitrate;
            }
        }

        public class AudioFrameEventArgs : CallBaseEventArgs
        {
            public ToxAvAudioFrame Frame { get; private set; }

            public AudioFrameEventArgs(int friendNumber, ToxAvAudioFrame frame)
                : base(friendNumber)
            {
                Frame = frame;
            }
        }

        public class VideoFrameEventArgs : CallBaseEventArgs
        {
            public ToxAvVideoFrame Frame { get; private set; }

            public VideoFrameEventArgs(int friendNumber, ToxAvVideoFrame frame)
                : base(friendNumber)
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
