using System;
using System.Runtime.InteropServices;

namespace SharpTox.Core
{
    internal static class ToxGroupFunctions
    {
#if POSIX
        const string dll = "libtoxcore.so";
#else
        const string dll = "libtox";
#endif

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_new")]
        internal static extern uint New(ToxHandle tox, ToxGroupPrivacyState privacyState, byte[] groupName, uint length, ref ToxErrorGroupNew error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_join")]
        internal static extern uint Join(ToxHandle tox, byte[] chatId, byte[] password, uint length, ref ToxErrorGroupJoin error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_reconnect")]
        internal static extern bool Reconnect(ToxHandle tox, uint groupNumber, ref ToxErrorGroupReconnect error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_leave")]
        internal static extern bool Leave(ToxHandle tox, uint groupNumber, byte[] message, uint length, ref ToxErrorGroupLeave error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_self_set_name")]
        internal static extern bool SelfSetName(ToxHandle tox, uint groupNumber, byte[] name, uint length, ref ToxErrorGroupSelfNameSet erorr);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_self_get_name_size")]
        internal static extern uint SelfGetNameSize(ToxHandle tox, uint groupNumber, ref ToxErrorGroupSelfQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_self_get_name")]
        internal static extern bool SelfGetName(ToxHandle tox, uint groupNumber, byte[] name, ref ToxErrorGroupSelfQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_self_set_status")]
        internal static extern bool SelfSetStatus(ToxHandle tox, uint groupNumber, ToxUserStatus status, ref ToxErrorGroupSelfStatusSet error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_self_get_status")]
        internal static extern ToxUserStatus SelfGetStatus(ToxHandle tox, uint groupNumber, ref ToxErrorGroupSelfQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_self_get_role")]
        internal static extern ToxGroupRole SelfGetRole(ToxHandle tox, uint groupNumber, ref ToxErrorGroupSelfQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_peer_get_name_size")]
        internal static extern uint PeerGetNameSize(ToxHandle tox, uint groupNumber, uint peerNumber, ref ToxErrorGroupPeerQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_peer_get_name")]
        internal static extern bool PeerGetName(ToxHandle tox, uint groupNumber, uint peerNumber, byte[] name, ref ToxErrorGroupPeerQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_peer_get_status")]
        internal static extern ToxUserStatus PeerGetStatus(ToxHandle tox, uint groupNumber, uint peerNumber, ref ToxErrorGroupPeerQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_peer_get_role")]
        internal static extern ToxGroupRole PeerGetRole(ToxHandle tox, uint groupNumber, uint peerNumber, ref ToxErrorGroupPeerQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_set_topic")]
        internal static extern bool SetTopic(ToxHandle tox, uint groupNumber, byte[] topic, uint length, ref ToxErrorGroupTopicSet error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_get_topic_size")]
        internal static extern uint GetTopicSize(ToxHandle tox, uint groupNumber, ref ToxErrorGroupStateQueries error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_get_topic")]
        internal static extern bool GetTopic(ToxHandle tox, uint groupNumber, byte[] topic, ref ToxErrorGroupStateQueries error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_get_name_size")]
        internal static extern uint GetNameSize(ToxHandle tox, uint groupNumber, ref ToxErrorGroupStateQueries error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_get_name")]
        internal static extern bool GetName(ToxHandle tox, uint groupNumber, byte[] name, ref ToxErrorGroupStateQueries error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_get_chat_id")]
        internal static extern bool GetChatId(ToxHandle tox, uint groupNumber, byte[] chatId, ref ToxErrorGroupStateQueries error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_get_number_peers")]
        internal static extern uint GetNumberPeers(ToxHandle tox, uint groupNumber, ref ToxErrorGroupStateQueries error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_get_number_groups")]
        internal static extern uint GetNumberGroups(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_get_privacy_state")]
        internal static extern ToxGroupPrivacyState GetPrivacyState(ToxHandle tox, uint groupNumber, ref ToxErrorGroupStateQueries error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_get_peer_limit")]
        internal static extern uint GetPeerLimit(ToxHandle tox, uint groupNumber, ref ToxErrorGroupStateQueries error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_get_password_size")]
        internal static extern uint GetPasswordSize(ToxHandle tox, uint groupNumber, ref ToxErrorGroupStateQueries error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_get_password")]
        internal static extern bool GetPassword(ToxHandle tox, uint groupNumber, byte[] password, ref ToxErrorGroupStateQueries error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_send_message")]
        internal static extern bool SendMessage(ToxHandle tox, uint groupNumber, ToxMessageType messageType, byte[] message, uint length, ref ToxErrorGroupSendMessage error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_send_private_message")]
        internal static extern bool SendPrivateMessage(ToxHandle tox, uint groupNumber, uint peerNumber, byte[] message, uint length, ref ToxErrorGroupSendPrivateMessage error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_invite_friend")]
        internal static extern bool InviteFriend(ToxHandle tox, uint groupNumber, uint friendNumber, ref ToxErrorGroupInviteFriend error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_invite_accept")]
        internal static extern bool InviteAccept(ToxHandle tox, byte[] inviteData, uint length, byte[] password, uint passwordLength, ref ToxErrorGroupInviteAccept error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_founder_set_password")]
        internal static extern bool FounderSetPassword(ToxHandle tox, uint groupNumber, byte[] password, uint length, ref ToxErrorGroupFounderSetPassword error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_founder_set_privacy_state")]
        internal static extern bool FounderSetPrivacyState(ToxHandle tox, uint groupNumber, ToxGroupPrivacyState privacyState, ref ToxErrorGroupFounderSetPrivacyState error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_founder_set_peer_limit")]
        internal static extern bool FounderSetPeerLimit(ToxHandle tox, uint groupNumber, uint maxPeers, ref ToxErrorGroupFounderSetPeerLimit error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_toggle_ignore")]
        internal static extern bool ToggleIgnore(ToxHandle tox, uint groupNumber, uint peerNumber, bool ignore, ref ToxErrorGroupToggleIgnore error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_mod_set_role")]
        internal static extern bool ModSetRole(ToxHandle tox, uint groupNumber, uint peerNumber, ToxGroupRole role, ref ToxErrorGroupModSetRole error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_mod_remove_peer")]
        internal static extern bool ModRemovePeer(ToxHandle tox, uint groupNumber, uint peerNumber, bool setBan, ref ToxErrorGroupModRemovePeer error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_mod_remove_ban")]
        internal static extern bool ModRemoveBan(ToxHandle tox, uint groupNumber, uint banId, ref ToxErrorGroupModRemoveBan error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_ban_get_list_size")]
        internal static extern uint BanGetListSize(ToxHandle tox, uint groupNumber, ref ToxErrorGroupBanQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_ban_get_list")]
        internal static extern bool BanGetList(ToxHandle tox, uint groupNumber, uint[] list, ref ToxErrorGroupBanQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_ban_get_name_size")]
        internal static extern uint BanGetNameSize(ToxHandle tox, uint groupNumber, uint banId, ref ToxErrorGroupBanQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_ban_get_name")]
        internal static extern bool BanGetName(ToxHandle tox, uint groupNumber, uint banId, byte[] name, ref ToxErrorGroupBanQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_ban_get_time_set")]
        internal static extern ulong BanGetTimeSet(ToxHandle tox, uint groupNumber, uint banId, ref ToxErrorGroupBanQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_peer_name")]
        internal static extern void RegisterPeerNameCallback(ToxHandle tox, ToxGroupDelegates.CallbackPeerNameDelegate callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_peer_status")]
        internal static extern void RegisterPeerStatusCallback(ToxHandle tox, ToxGroupDelegates.CallbackPeerStatusDelegate callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_topic")]
        internal static extern void RegisterTopicCallback(ToxHandle tox, ToxGroupDelegates.CallbackTopicDelegate callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_privacy_state")]
        internal static extern void RegisterPrivacyStateCallback(ToxHandle tox, ToxGroupDelegates.CallbackPrivacyStateDelegate callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_peer_limit")]
        internal static extern void RegisterPeerLimitCallback(ToxHandle tox, ToxGroupDelegates.CallbackPeerLimitDelegate callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_password")]
        internal static extern void RegisterPasswordCallback(ToxHandle tox, ToxGroupDelegates.CallbackPasswordDelegate callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_peerlist_update")]
        internal static extern void RegisterPeerListUpdateCallback(ToxHandle tox, ToxGroupDelegates.CallbackPeerListUpdateDelegate callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_message")]
        internal static extern void RegisterMessageCallback(ToxHandle tox, ToxGroupDelegates.CallbackMessageDelegate callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_private_message")]
        internal static extern void RegisterPrivateMessageCallback(ToxHandle tox, ToxGroupDelegates.CallbackPrivateMessageDelegate callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_invite")]
        internal static extern void RegisterInviteCallback(ToxHandle tox, ToxGroupDelegates.CallbackInviteDelegate callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_peer_join")]
        internal static extern void RegisterPeerJoinCallback(ToxHandle tox, ToxGroupDelegates.CallbackPeerJoinDelegate callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_peer_exit")]
        internal static extern void RegisterPeerExitCallback(ToxHandle tox, ToxGroupDelegates.CallbackPeerExitDelegate callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_self_join")]
        internal static extern void RegisterSelfJoinCallback(ToxHandle tox, ToxGroupDelegates.CallbackSelfJoinDelegate callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_join_fail")]
        internal static extern void RegisterJoinFailCallback(ToxHandle tox, ToxGroupDelegates.CallbackJoinFailDelegate callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_moderation")]
        internal static extern void RegisterModerationCallback(ToxHandle tox, ToxGroupDelegates.CallbackModerationDelegate callback, IntPtr userData);
    }
}
