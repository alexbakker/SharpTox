#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;

namespace SharpTox.Core
{
    internal class ToxDelegates
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackFriendMessageDelegate(IntPtr tox, uint friendNumber, ToxMessageType type, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] message, uint length, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackFriendRequestDelegate(IntPtr tox, [MarshalAs(UnmanagedType.LPArray, SizeConst = ToxConstants.PublicKeySize)] byte[] publicKey, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] message, uint length, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackTypingChangeDelegate(IntPtr tox, uint friendNumber, bool isTyping, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackUserStatusDelegate(IntPtr tox, uint friendNumber, ToxUserStatus status, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackStatusMessageDelegate(IntPtr tox, uint friendNumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] newStatus, uint length, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackNameChangeDelegate(IntPtr tox, uint friendNumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] newName, uint length, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackConnectionStatusDelegate(IntPtr tox, ToxConnectionStatus status, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackFriendConnectionStatusDelegate(IntPtr tox, uint friendNumber, ToxConnectionStatus status, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackReadReceiptDelegate(IntPtr tox, uint friendNumber, uint messageId, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackFileReceiveChunkDelegate(IntPtr tox, uint friendNumber, uint fileNumber, ulong position, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] byte[] data, uint length, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackFileControlDelegate(IntPtr tox, uint friendNumber, uint fileNumber, ToxFileControl control, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackFileReceiveDelegate(IntPtr tox, uint friendNumber, uint fileNumber, ToxFileKind kind, ulong fizeSize, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 6)] byte[] filename, uint filenameLength, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackFileRequestChunkDelegate(IntPtr tox, uint friendNumber, uint fileNumber, ulong position, uint length, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackFriendPacketDelegate(IntPtr tox, uint friendNumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] data, uint length, IntPtr userData);
    }
}

#pragma warning restore 1591