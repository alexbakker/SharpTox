using System;
using System.Runtime.InteropServices;

namespace SharpTox.Core
{
    internal class ToxGroupDelegates
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackPeerNameDelegate(IntPtr tox, uint groupNumber, uint peerNumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] name, uint length, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackPeerStatusDelegate(IntPtr tox, uint groupNumber, uint peerNumber, ToxUserStatus status, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackTopicDelegate(IntPtr tox, uint groupNumber, uint peerNumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] topic, uint length, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackPrivacyStateDelegate(IntPtr tox, uint groupNumber, ToxGroupPrivacyState privacyState, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackPeerLimitDelegate(IntPtr tox, uint groupNumber, uint peerLimit, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackPasswordDelegate(IntPtr tox, uint groupNumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] password, uint length, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackMessageDelegate(IntPtr tox, uint groupNumber, uint peerNumber, ToxMessageType messageType, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] byte[] message, uint length, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackPrivateMessageDelegate(IntPtr tox, uint groupNumber, uint peerNumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] message, uint length, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackInviteDelegate(IntPtr tox, uint friendNumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] inviteData, uint length, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackPeerJoinDelegate(IntPtr tox, uint groupNumber, uint peerNumber, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackPeerExitDelegate(IntPtr tox, uint groupNumber, uint peerNumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] partMessage, uint length, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackSelfJoinDelegate(IntPtr tox, uint groupNumber, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackJoinFailDelegate(IntPtr tox, uint groupNumber, ToxGroupJoinFail failType, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackModerationDelegate(IntPtr tox, uint groupNumber, uint sourcePeerNumber, uint targetPeerNumber, ToxGroupModEvent modEvent, IntPtr userData);
    }
}
