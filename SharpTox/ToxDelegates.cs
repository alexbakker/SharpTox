using System;
using System.Runtime.InteropServices;

namespace SharpTox
{
    internal class ToxDelegates
    {
        public delegate void CallbackTypingChangeDelegate(IntPtr tox, int friendnumber, byte istyping, IntPtr userdata);
        public delegate void CallbackUserStatusDelegate(IntPtr tox, int friendnumber, ToxUserStatus status, IntPtr userdata);
        public delegate void CallbackStatusMessageDelegate(IntPtr tox, int friendnumber, [MarshalAs(UnmanagedType.LPArray, SizeConst = ToxConstants.MAX_STATUSMESSAGE_LENGTH * sizeof(char))] byte[] newstatus, ushort length, IntPtr userdata);
        public delegate void CallbackNameChangeDelegate(IntPtr tox, int friendnumber, [MarshalAs(UnmanagedType.LPArray, SizeConst = ToxConstants.MAX_NAME_LENGTH * sizeof(char))] byte[] newname, ushort length, IntPtr userdata);
        public delegate void CallbackFriendActionDelegate(IntPtr tox, int friendnumber, [MarshalAs(UnmanagedType.LPArray, /*somone should check my math on this*/ SizeConst = ToxConstants.MAX_MESSAGE_LENGTH * sizeof(char))] byte[] action, ushort length, IntPtr userdata);
        public delegate void CallbackFriendMessageDelegate(IntPtr tox, int friendnumber, [MarshalAs(UnmanagedType.LPArray, SizeConst = ToxConstants.MAX_MESSAGE_LENGTH * sizeof(char))] byte[] message, ushort length, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackConnectionStatusDelegate(IntPtr tox, int friendnumber, byte status, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackFriendRequestDelegate(IntPtr tox, [MarshalAs(UnmanagedType.LPArray, SizeConst = 38)] byte[] address, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1003)] byte[] message, ushort length, IntPtr userdata);
    }
}
