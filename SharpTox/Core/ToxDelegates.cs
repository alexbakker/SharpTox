#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;

namespace SharpTox.Core
{
    public class ToxDelegates
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackFriendMessageDelegate(IntPtr tox, uint friendNumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] message, uint length, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackFriendRequestDelegate(IntPtr tox, [MarshalAs(UnmanagedType.LPArray, SizeConst = ToxConstants.PublicKeySize)] byte[] publicKey, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] message, uint length, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackTypingChangeDelegate(IntPtr tox, uint friendNumber, bool isTyping, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackUserStatusDelegate(IntPtr tox, uint friendNumber, ToxStatus status, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackStatusMessageDelegate(IntPtr tox, uint friendNumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] newStatus, uint length, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackNameChangeDelegate(IntPtr tox, uint friendNumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] newName, uint length, IntPtr userData);
    }
}

#pragma warning restore 1591