using System;
using System.Runtime.InteropServices;

namespace SharpTox.Av
{
    internal class ToxAvDelegates
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallCallback(IntPtr toxAv, uint friendNumber, bool audioEnabled, bool videoEnabled, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallStateCallback(IntPtr toxAv, uint friendNumber, ToxAvFriendCallState state, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void FrameRequestCallback(IntPtr toxAv, uint friendNumber, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void BitrateStatusCallback(IntPtr toxAv, uint friendNumber, uint audioBitrate, uint videoBitrate, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void VideoReceiveFrameCallback(IntPtr toxAv, uint friendNumber, ushort width, ushort height, IntPtr y, IntPtr u, IntPtr v, int yStride, int uStride, int vStride, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void AudioReceiveFrameCallback(IntPtr toxAv, uint friendNumber, IntPtr pcm, uint sampleCount, byte channels, uint samplingRate, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void GroupAudioReceiveCallback(IntPtr tox, int groupNumber, int peerNumber, IntPtr frame, uint sampleCount, byte channels, uint sampleRate, IntPtr userData);
    }
}
