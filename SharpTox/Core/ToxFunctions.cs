using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpTox.Core
{
    /// <summary>
    /// Represents a collection of native functions found in the tox core library.
    /// </summary>
    internal static class ToxFunctions
    {
#if POSIX
		const string dll = "libtoxcore.so";
#else 
		const string dll = "libtox";
#endif

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_new")]
        internal static extern ToxHandle New(ref ToxOptions options, byte[] data, uint length, ref ToxErrorNew error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_options_default")]
        internal static extern void OptionsDefault(ref ToxOptions options);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_options_new")]
        internal static extern IntPtr OptionsNew(ref ToxErrorOptionsNew error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_options_free")]
        internal static extern void OptionsFree(ref ToxOptions options);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_options_free")]
        internal static extern void OptionsFree(IntPtr options);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_version_major")]
        internal static extern uint VersionMajor();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_version_minor")]
        internal static extern uint VersionMinor();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_version_patch")]
        internal static extern uint VersionPatch();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_version_is_compatible")]
        internal static extern bool VersionIsCompatible(uint major, uint minor, uint patch);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_bootstrap")]
        internal static extern bool Bootstrap(ToxHandle tox, string host, ushort port, byte[] publicKey, ref ToxErrorBootstrap error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_get_connection_status")]
        internal static extern ToxConnectionStatus SelfGetConnectionStatus(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_get_address")]
        internal static extern void SelfGetAddress(ToxHandle tox, byte[] address);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_by_public_key")]
        internal static extern uint FriendByPublicKey(ToxHandle tox, byte[] publicKey, ref ToxErrorFriendByPublicKey error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_get_public_key")]
        internal static extern bool FriendGetPublicKey(ToxHandle tox, uint friendNumber, byte[] publicKey, ref ToxErrorFriendGetPublicKey error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_iterate")]
        internal static extern void Iterate(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_iteration_interval")]
        internal static extern uint IterationInterval(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_kill")]
        internal static extern void Kill(IntPtr tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_delete")]
        internal static extern bool FriendDelete(ToxHandle tox, uint friendNumber, ref ToxErrorFriendDelete error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_get_connection_status")]
        internal static extern ToxConnectionStatus FriendGetConnectionStatus(ToxHandle tox, uint friendNumber, ref ToxErrorFriendQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_get_status")]
        internal static extern ToxUserStatus FriendGetStatus(ToxHandle tox, uint friendNumber, ref ToxErrorFriendQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_get_status")]
        internal static extern ToxUserStatus SelfGetStatus(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_exists")]
        internal static extern bool FriendExists(ToxHandle tox, uint friendNumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_get_friend_list_size")]
        internal static extern uint SelfGetFriendListSize(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_get_friend_list")]
        internal static extern void SelfGetFriendList(ToxHandle tox, uint[] list);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_savedata_size")]
        internal static extern uint GetSaveDataSize(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_savedata")]
        internal static extern void GetSaveData(ToxHandle tox, byte[] bytes);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_send_message")]
        internal static extern uint FriendSendMessage(ToxHandle tox, uint friendNumber, ToxMessageType messageType, byte[] message, uint length, ref ToxErrorSendMessage error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_add")]
        internal static extern uint FriendAdd(ToxHandle tox, byte[] address, byte[] message, uint length, ref ToxErrorFriendAdd error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_add_norequest")]
        internal static extern uint FriendAddNoRequest(ToxHandle tox, byte[] publicKey, ref ToxErrorFriendAdd error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_set_name")]
        internal static extern bool SelfSetName(ToxHandle tox, byte[] name, uint length, ref ToxErrorSetInfo error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_get_name")]
        internal static extern void SelfGetName(ToxHandle tox, byte[] name);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_get_name_size")]
        internal static extern uint SelfGetNameSize(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_set_typing")]
        internal static extern bool SelfSetTyping(ToxHandle tox, uint friendNumber, bool is_typing, ref ToxErrorSetTyping error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_get_typing")]
        internal static extern bool FriendGetTyping(ToxHandle tox, uint friendNumber, ref ToxErrorFriendQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_add_tcp_relay")]
        internal static extern bool AddTcpRelay(ToxHandle tox, string host, ushort port, byte[] publicKey, ref ToxErrorBootstrap error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_set_nospam")]
        internal static extern void SelfSetNospam(ToxHandle tox, uint nospam);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_get_nospam")]
        internal static extern uint SelfGetNospam(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_get_public_key")]
        internal static extern void SelfGetPublicKey(ToxHandle tox, byte[] publicKey);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_get_secret_key")]
        internal static extern void SelfGetSecretKey(ToxHandle tox, byte[] secretKey);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_get_status_message")]
        internal static extern void SelfGetStatusMessage(ToxHandle tox, byte[] status);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_get_status_message_size")]
        internal static extern uint SelfGetStatusMessageSize(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_set_status_message")]
        internal static extern void SelfSetStatusMessage(ToxHandle tox, byte[] status, uint length, ref ToxErrorSetInfo error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_set_status")]
        internal static extern void SelfSetStatus(ToxHandle tox, ToxUserStatus status);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_get_name_size")]
        internal static extern uint FriendGetNameSize(ToxHandle tox, uint friendNumber, ref ToxErrorFriendQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_get_name")]
        internal static extern bool FriendGetName(ToxHandle tox, uint friendNumber, byte[] name, ref ToxErrorFriendQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_get_status_message_size")]
        internal static extern uint FriendGetStatusMessageSize(ToxHandle tox, uint friendNumber, ref ToxErrorFriendQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_get_status_message")]
        internal static extern bool FriendGetStatusMessage(ToxHandle tox, uint friendNumber, byte[] message, ref ToxErrorFriendQuery error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_get_udp_port")]
        internal static extern ushort SelfGetUdpPort(ToxHandle tox, ref ToxErrorGetPort error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_get_tcp_port")]
        internal static extern ushort SelfGetTcpPort(ToxHandle tox, ref ToxErrorGetPort error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_self_get_dht_id")]
        internal static extern void SelfGetDhtId(ToxHandle tox, byte[] dhtId);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_hash")]
        internal static extern bool Hash(byte[] hash, byte[] data, uint length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_file_control")]
        internal static extern bool FileControl(ToxHandle tox, uint friendNumber, uint fileNumber, ToxFileControl control, ref ToxErrorFileControl error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_file_send")]
        internal static extern uint FileSend(ToxHandle tox, uint friendNumber, ToxFileKind kind, ulong fileSize, byte[] fileId, byte[] fileName, uint fileNameLength, ref ToxErrorFileSend error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_file_send_chunk")]
        internal static extern bool FileSendChunk(ToxHandle tox, uint friendNumber, uint fileNumber, ulong position, byte[] data, uint length, ref ToxErrorFileSendChunk error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_file_get_file_id")]
        internal static extern bool FileGetFileId(ToxHandle tox, uint friendNumber, uint fileNumber, byte[] fileId, ref ToxErrorFileGet error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_file_seek")]
        internal static extern bool FileSeek(ToxHandle tox, uint friendNumber, uint fileNumber, ulong position, ref ToxErrorFileSeek error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_send_lossy_packet")]
        internal static extern bool FriendSendLossyPacket(ToxHandle tox, uint friendNumber, byte[] data, uint length, ref ToxErrorFriendCustomPacket error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_send_lossless_packet")]
        internal static extern bool FriendSendLosslessPacket(ToxHandle tox, uint friendNumber, byte[] data, uint length, ref ToxErrorFriendCustomPacket error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_get_last_online")]
        internal static extern ulong FriendGetLastOnline(ToxHandle tox, uint friendNumber, ref ToxErrorFriendGetLastOnline error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_add_groupchat")]
        internal static extern int AddGroupchat(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_del_groupchat")]
        internal static extern int DelGroupchat(ToxHandle tox, int groupnumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_peername")]
        internal static extern int GroupPeername(ToxHandle tox, int groupnumber, int peernumber, byte[] name);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_invite_friend")]
        internal static extern int InviteFriend(ToxHandle tox, int friendnumber, int groupnumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_join_groupchat")]
        internal static extern int JoinGroupchat(ToxHandle tox, int friendnumber, byte[] data, ushort length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_message_send")]
        internal static extern int GroupMessageSend(ToxHandle tox, int groupnumber, byte[] message, ushort length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_action_send")]
        internal static extern int GroupActionSend(ToxHandle tox, int groupnumber, byte[] action, ushort length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_number_peers")]
        internal static extern int GroupNumberPeers(ToxHandle tox, int groupnumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "tox_group_get_names")]
        internal static extern int GroupGetNames(ToxHandle tox, int groupnumber, byte[,] names, ushort[] lengths, ushort length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_peernumber_is_ours")]
        internal static extern uint GroupPeerNumberIsOurs(ToxHandle tox, int groupnumber, int peernumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_set_title")]
        internal static extern int GroupSetTitle(ToxHandle tox, int groupnumber, byte[] title, byte length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_get_type")]
        internal static extern int GroupGetType(ToxHandle tox, int groupnumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_get_title")]
        internal static extern int GroupGetTitle(ToxHandle tox, int groupnumber, byte[] title, uint max_length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_peer_pubkey")]
        internal static extern int GroupPeerPubkey(ToxHandle tox, int groupnumber, int peernumber, byte[] pk);

        #region Register callback functions
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_friend_request")]
        internal static extern void RegisterFriendRequestCallback(ToxHandle tox, ToxDelegates.CallbackFriendRequestDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_friend_message")]
        internal static extern void RegisterFriendMessageCallback(ToxHandle tox, ToxDelegates.CallbackFriendMessageDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_friend_name")]
        internal static extern void RegisterNameChangeCallback(ToxHandle tox, ToxDelegates.CallbackNameChangeDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_friend_status_message")]
        internal static extern void RegisterStatusMessageCallback(ToxHandle tox, ToxDelegates.CallbackStatusMessageDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_friend_status")]
        internal static extern void RegisterUserStatusCallback(ToxHandle tox, ToxDelegates.CallbackUserStatusDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_friend_typing")]
        internal static extern void RegisterTypingChangeCallback(ToxHandle tox, ToxDelegates.CallbackTypingChangeDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_self_connection_status")]
        internal static extern void RegisterConnectionStatusCallback(ToxHandle tox, ToxDelegates.CallbackConnectionStatusDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_friend_connection_status")]
        internal static extern void RegisterFriendConnectionStatusCallback(ToxHandle tox, ToxDelegates.CallbackFriendConnectionStatusDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_friend_read_receipt")]
        internal static extern void RegisterFriendReadReceiptCallback(ToxHandle tox, ToxDelegates.CallbackReadReceiptDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_file_recv")]
        internal static extern void RegisterFileReceiveCallback(ToxHandle tox, ToxDelegates.CallbackFileReceiveDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_file_recv_control")]
        internal static extern void RegisterFileControlRecvCallback(ToxHandle tox, ToxDelegates.CallbackFileControlDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_file_recv_chunk")]
        internal static extern void RegisterFileReceiveChunkCallback(ToxHandle tox, ToxDelegates.CallbackFileReceiveChunkDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_file_chunk_request")]
        internal static extern void RegisterFileChunkRequestCallback(ToxHandle tox, ToxDelegates.CallbackFileRequestChunkDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_friend_lossy_packet")]
        internal static extern void RegisterFriendLossyPacketCallback(ToxHandle tox, ToxDelegates.CallbackFriendPacketDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_friend_lossless_packet")]
        internal static extern void RegisterFriendLosslessPacketCallback(ToxHandle tox, ToxDelegates.CallbackFriendPacketDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_invite")]
        internal static extern void RegisterGroupInviteCallback(ToxHandle tox, ToxDelegates.CallbackGroupInviteDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_message")]
        internal static extern void RegisterGroupMessageCallback(ToxHandle tox, ToxDelegates.CallbackGroupMessageDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_action")]
        internal static extern void RegisterGroupActionCallback(ToxHandle tox, ToxDelegates.CallbackGroupActionDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_namelist_change")]
        internal static extern void RegisterGroupNamelistChangeCallback(ToxHandle tox, ToxDelegates.CallbackGroupNamelistChangeDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_title")]
        internal static extern void RegisterGroupTitleCallback(ToxHandle tox, ToxDelegates.CallbackGroupTitleDelegate callback, IntPtr userdata);

        #endregion
    }
}
