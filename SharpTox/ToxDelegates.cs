using System;
using System.Runtime.InteropServices;

namespace SharpTox
{
    internal class ToxDelegates
    {
        /*someone should REALLY check my math on some of this stuff*/

        public delegate void CallbackTypingChangeDelegate(IntPtr tox, int friendnumber, byte istyping, IntPtr userdata);
        public delegate void CallbackUserStatusDelegate(IntPtr tox, int friendnumber, ToxUserStatus status, IntPtr userdata);
        public delegate void CallbackStatusMessageDelegate(IntPtr tox, int friendnumber, [MarshalAs(UnmanagedType.LPArray, SizeConst = ToxConstants.MAX_STATUSMESSAGE_LENGTH)] byte[] newstatus, ushort length, IntPtr userdata);
        public delegate void CallbackNameChangeDelegate(IntPtr tox, int friendnumber, [MarshalAs(UnmanagedType.LPArray, SizeConst = ToxConstants.MAX_NAME_LENGTH)] byte[] newname, ushort length, IntPtr userdata);
        public delegate void CallbackFriendActionDelegate(IntPtr tox, int friendnumber, [MarshalAs(UnmanagedType.LPArray, SizeConst = ToxConstants.MAX_MESSAGE_LENGTH)] byte[] action, ushort length, IntPtr userdata);
        public delegate void CallbackFriendMessageDelegate(IntPtr tox, int friendnumber, [MarshalAs(UnmanagedType.LPArray, SizeConst = ToxConstants.MAX_MESSAGE_LENGTH)] byte[] message, ushort length, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackConnectionStatusDelegate(IntPtr tox, int friendnumber, byte status, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackFriendRequestDelegate(IntPtr tox, [MarshalAs(UnmanagedType.LPArray, SizeConst = 38)] byte[] address, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1003)] byte[] message, ushort length, IntPtr userdata);

        public delegate void CallbackGroupInviteDelegate(IntPtr tox, int friendnumber, [MarshalAs(UnmanagedType.LPArray, SizeConst = 38)] byte[] group_public_key, IntPtr userdata);
        public delegate void CallbackGroupMessageDelegate(IntPtr tox, int groupnumber, int friendgroupnumber, [MarshalAs(UnmanagedType.LPArray, SizeConst = ToxConstants.MAX_MESSAGE_LENGTH)] byte[] message, ushort length, IntPtr userdata);
        public delegate void CallbackGroupActionDelegate(IntPtr tox, int groupnumber, int friendgroupnumber, [MarshalAs(UnmanagedType.LPArray, SizeConst = ToxConstants.MAX_MESSAGE_LENGTH)] byte[] action, ushort length, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackGroupNamelistChangeDelegate(IntPtr tox, int groupnumber, int peernumber, ToxChatChange change, IntPtr userdata);
    }
}
