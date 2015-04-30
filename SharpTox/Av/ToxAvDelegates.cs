using System;
using System.Runtime.InteropServices;

namespace SharpTox.Av
{
    internal class ToxAvDelegates
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallstateCallback(IntPtr agent, int callIndex, IntPtr args);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void AudioReceiveCallback(IntPtr toxav, int callIndex, IntPtr frame, int frameSize, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void GroupAudioReceiveCallback(IntPtr tox, int groupNumber, int peerNumber, IntPtr frame, uint sampleCount, byte channels, uint sampleRate, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void VideoReceiveCallback(IntPtr toxav, int callIndex, IntPtr frame, IntPtr userData);
    }
}
