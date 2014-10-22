#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;

namespace SharpTox.Av
{
    public class ToxAvDelegates
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallstateCallback(IntPtr agent, int callIndex, IntPtr args);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AudioReceiveCallback(IntPtr toxav, int callIndex, IntPtr frame, int frameSize, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void VideoReceiveCallback(IntPtr toxav, int callIndex, IntPtr frame, IntPtr userData);
    }
}

#pragma warning restore 1591