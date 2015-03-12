#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;

namespace SharpTox.Core
{
    public class ToxDelegates
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackFriendMessageDelegate(IntPtr tox, int friendNumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] message, uint length, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackFriendRequestDelegate(IntPtr tox, [MarshalAs(UnmanagedType.LPArray, SizeConst = ToxConstants.ClientIdSize)] byte[] publicKey, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] message, uint length, IntPtr userData);
    }
}

#pragma warning restore 1591