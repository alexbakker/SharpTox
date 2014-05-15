#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;

namespace SharpTox
{
    public class ToxAvDelegates
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallstateCallback(IntPtr args);
    }
}

#pragma warning restore 1591