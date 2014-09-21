#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;

namespace SharpTox.Core
{
    public class ToxDelegates
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackTypingChangeDelegate(IntPtr tox, int friendnumber, byte istyping, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackUserStatusDelegate(IntPtr tox, int friendnumber, ToxUserStatus status, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackStatusMessageDelegate(IntPtr tox, int friendnumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] newstatus, ushort length, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackNameChangeDelegate(IntPtr tox, int friendnumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] newname, ushort length, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackFriendActionDelegate(IntPtr tox, int friendnumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] action, ushort length, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackFriendMessageDelegate(IntPtr tox, int friendnumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] message, ushort length, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackConnectionStatusDelegate(IntPtr tox, int friendnumber, byte status, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackFriendRequestDelegate(IntPtr tox, [MarshalAs(UnmanagedType.LPArray, SizeConst = 38)] byte[] address, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] message, ushort length, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackGroupInviteDelegate(IntPtr tox, int friendnumber, [MarshalAs(UnmanagedType.LPArray, SizeConst = 32)] byte[] group_public_key, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackGroupMessageDelegate(IntPtr tox, int groupnumber, int friendgroupnumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] message, ushort length, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackGroupActionDelegate(IntPtr tox, int groupnumber, int friendgroupnumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] action, ushort length, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackGroupNamelistChangeDelegate(IntPtr tox, int groupnumber, int peernumber, ToxChatChange change, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackFileDataDelegate(IntPtr tox, int friendnumber, byte filenumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] data, ushort length, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackFileControlDelegate(IntPtr tox, int friendnumber, byte receive_send, byte filenumber, byte control_type, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 6)] byte[] data, ushort length, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackFileSendRequestDelegate(IntPtr tox, int friendnumber, byte filenumber, ulong filesize, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] byte[] filename, ushort filename_length, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackReadReceiptDelegate(IntPtr tox, int friendnmber, uint receipt, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int CallbackPacketDelegate(IntPtr obj, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] data, uint length);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackAvatarInfoDelegate(IntPtr tox, int friendnumber, byte format, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] hash, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackAvatarDataDelegate(IntPtr tox, int friendnumber, byte format, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] hash, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] data, uint datalen, IntPtr userdata);
        
    }
}

#pragma warning restore 1591