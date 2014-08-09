#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;

namespace SharpTox.Av
{
    public class ToxAvDelegates
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallstateCallback(IntPtr agent, int call_index, IntPtr args);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AudioReceiveCallback(IntPtr toxav, int call_index, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] short[] frame, int frame_size, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void VideoReceiveCallback(IntPtr toxav, int call_index, IntPtr frame, IntPtr userdata);
    }
}

#pragma warning restore 1591