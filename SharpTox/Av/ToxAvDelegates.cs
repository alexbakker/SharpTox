#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;

namespace SharpTox.Av
{
    public class ToxAvDelegates
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallstateCallback(int call_index, IntPtr args);
    }
}

#pragma warning restore 1591