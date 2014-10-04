#pragma warning disable 1591

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
    public static class ToxFunctions
    {
        const string dll = "libtox";

        #region Functions
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_new")]
        public static extern ToxHandle New(ref ToxOptions options);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_bootstrap_from_address")]
        public static extern int BootstrapFromAddress(ToxHandle tox, string address, ushort port, byte[] public_key);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_isconnected")]
        public static extern int IsConnected(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_address")]
        public static extern void GetAddress(ToxHandle tox, byte[] address);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_friend_number")]
        public static extern int GetFriendNumber(ToxHandle tox, byte[] client_id);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_client_id")]
        public static extern int GetClientID(ToxHandle tox, int friendnumber, byte[] address);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_load")]
        public static extern int Load(ToxHandle tox, byte[] bytes, uint length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_do")]
        public static extern void Do(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_do_interval")]
        public static extern uint DoInterval(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_kill")]
        public static extern void Kill(IntPtr tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_del_friend")]
        public static extern int DelFriend(ToxHandle tox, int id);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_friend_connection_status")]
        public static extern int GetFriendConnectionStatus(ToxHandle tox, int friendnumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_user_status")]
        public static extern byte GetUserStatus(ToxHandle tox, int friendnumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_self_user_status")]
        public static extern byte GetSelfUserStatus(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_friend_exists")]
        public static extern int FriendExists(ToxHandle tox, int friendnumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_last_online")]
        public static extern ulong GetLastOnline(ToxHandle tox, int friendnumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_count_friendlist")]
        public static extern uint CountFriendlist(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_size")]
        public static extern uint Size(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_send_message")]
        public static extern uint SendMessage(ToxHandle tox, int friendnumber, byte[] message, int length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_send_action")]
        public static extern uint SendAction(ToxHandle tox, int friendnumber, byte[] action, int length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_save")]
        public static extern void Save(ToxHandle tox, byte[] bytes);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_add_friend")]
        public static extern int AddFriend(ToxHandle tox, byte[] id, byte[] message, ushort length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_add_friend_norequest")]
        public static extern int AddFriendNoRequest(ToxHandle tox, byte[] id);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_set_name")]
        public static extern int SetName(ToxHandle tox, byte[] name, ushort length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_set_user_is_typing")]
        public static extern int SetUserIsTyping(ToxHandle tox, int friendnumber, byte is_typing);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_self_name")]
        public static extern ushort GetSelfName(ToxHandle tox, byte[] name);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_is_typing")]
        public static extern byte GetIsTyping(ToxHandle tox, int friendnumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_set_user_status")]
        public static extern int SetUserStatus(ToxHandle tox, byte userstatus);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_set_status_message")]
        public static extern int SetStatusMessage(ToxHandle tox, byte[] message, ushort length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_friendlist")]
        public static extern uint GetFriendlist(ToxHandle tox, int[] friends, uint[] trunc);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_num_online_friends")]
        public static extern uint GetNumOnlineFriends(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_name")]
        public static extern int GetName(ToxHandle tox, int friendnumber, byte[] name);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_status_message")]
        public static extern int GetStatusMessage(ToxHandle tox, int friendnumber, byte[] message, int maxlen);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_self_status_message")]
        public static extern int GetSelfStatusMessage(ToxHandle tox, byte[] message, int maxlen);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_status_message_size")]
        public static extern int GetStatusMessageSize(ToxHandle tox, int friendnumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_self_status_message_size")]
        public static extern int GetSelfStatusMessageSize(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_name_size")]
        public static extern int GetNameSize(ToxHandle tox, int friendnumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_self_name_size")]
        public static extern int GetSelfNameSize(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_add_groupchat")]
        public static extern int AddGroupchat(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_del_groupchat")]
        public static extern int DelGroupchat(ToxHandle tox, int groupnumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_peername")]
        public static extern int GroupPeername(ToxHandle tox, int groupnumber, int peernumber, byte[] name);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_invite_friend")]
        public static extern int InviteFriend(ToxHandle tox, int friendnumber, int groupnumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_join_groupchat")]
        public static extern int JoinGroupchat(ToxHandle tox, int friendnumber, byte[] data, ushort length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_message_send")]
        public static extern int GroupMessageSend(ToxHandle tox, int groupnumber, byte[] message, ushort length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_action_send")]
        public static extern int GroupActionSend(ToxHandle tox, int groupnumber, byte[] action, ushort length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_group_number_peers")]
        public static extern int GroupNumberPeers(ToxHandle tox, int groupnumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "tox_group_get_names")]
        public static extern int GroupGetNames(ToxHandle tox, int groupnumber, byte[,] names, ushort[] lengths, ushort length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_count_chatlist")]
        public static extern uint CountChatlist(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_chatlist")]
        public static extern uint GetChatlist(ToxHandle tox, int[] out_list, uint[] list_size);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_new_file_sender")]
        public static extern int NewFileSender(ToxHandle tox, int friendnumber, ulong filesize, byte[] filename, ushort filename_length); //max filename length is 255 bytes

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_file_send_control")]
        public static extern int FileSendControl(ToxHandle tox, int friendnumber, byte send_receive, byte filenumber, byte message_id, byte[] data, ushort length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_file_send_data")]
        public static extern int FileSendData(ToxHandle tox, int friendnumber, byte filenumber, byte[] data, ushort length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_file_data_size")]
        public static extern int FileDataSize(ToxHandle tox, int friendnumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_file_data_remaining")]
        public static extern ulong FileDataRemaining(ToxHandle tox, int friendnumber, byte filenumber, byte send_receive);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_nospam")]
        public static extern uint GetNospam(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_set_nospam")]
        public static extern void SetNospam(ToxHandle tox, uint nospam);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_keys")]
        public static extern void GetKeys(ToxHandle tox, byte[] public_key, byte[] secret_key);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_send_lossy_packet")]
        public static extern int SendLossyPacket(ToxHandle tox, int friendnumber, byte[] data, uint length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "tox_send_lossless_packet")]
        public static extern int SendLosslessPacket(ToxHandle tox, int friendnumber, byte[] data, uint length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_add_tcp_relay")]
        public static extern int AddTcpRelay(ToxHandle tox, string address, ushort port, byte[] public_key);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_encrypted_size")]
        public static extern uint EncryptedSize(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_encrypted_save")]
        public static extern int EncryptedSave(ToxHandle tox, byte[] data, byte[] passphrase, uint pplength);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_encrypted_load")]
        public static extern int EncryptedLoad(ToxHandle tox, byte[] data, uint length, byte[] passphrase, uint pplength);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_is_data_encrypted")]
        public static extern int IsDataEncrypted(byte[] data);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_set_avatar")]
        public static extern int SetAvatar(ToxHandle tox, byte format, byte[] data, uint length);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_self_avatar")]
        public static extern int GetSelfAvatar(ToxHandle tox, ref byte format, byte[] buf, ref uint length, uint maxlen, byte[] hash);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_hash")]
        public static extern int Hash(byte[] hash, byte[] data, uint datalen);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_request_avatar_info")]
        public static extern int RequestAvatarInfo(ToxHandle tox, int friendnumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_send_avatar_info")]
        public static extern int SendAvatarInfo(ToxHandle tox, int friendnumber);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_request_avatar_data")]
        public static extern int RequestAvatarData(ToxHandle tox, int friendnumber);

        #endregion

        #region Callbacks
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_friend_request")]
        public static extern void CallbackFriendRequest(ToxHandle tox, ToxDelegates.CallbackFriendRequestDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_connection_status")]
        public static extern void CallbackConnectionStatus(ToxHandle tox, ToxDelegates.CallbackConnectionStatusDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_friend_message")]
        public static extern void CallbackFriendMessage(ToxHandle tox, ToxDelegates.CallbackFriendMessageDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_friend_action")]
        public static extern void CallbackFriendAction(ToxHandle tox, ToxDelegates.CallbackFriendActionDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_name_change")]
        public static extern void CallbackNameChange(ToxHandle tox, ToxDelegates.CallbackNameChangeDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_status_message")]
        public static extern void CallbackStatusMessage(ToxHandle tox, ToxDelegates.CallbackStatusMessageDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_user_status")]
        public static extern void CallbackUserStatus(ToxHandle tox, ToxDelegates.CallbackUserStatusDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "tox_callback_typing_change")]
        public static extern void CallbackTypingChange(ToxHandle tox, ToxDelegates.CallbackTypingChangeDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_invite")]
        public static extern void CallbackGroupInvite(ToxHandle tox, ToxDelegates.CallbackGroupInviteDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_message")]
        public static extern void CallbackGroupMessage(ToxHandle tox, ToxDelegates.CallbackGroupMessageDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_action")]
        public static extern void CallbackGroupAction(ToxHandle tox, ToxDelegates.CallbackGroupActionDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_group_namelist_change")]
        public static extern void CallbackGroupNamelistChange(ToxHandle tox, ToxDelegates.CallbackGroupNamelistChangeDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_file_send_request")]
        public static extern void CallbackFileSendRequest(ToxHandle tox, ToxDelegates.CallbackFileSendRequestDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_file_control")]
        public static extern void CallbackFileControl(ToxHandle tox, ToxDelegates.CallbackFileControlDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_file_data")]
        public static extern void CallbackFileData(ToxHandle tox, ToxDelegates.CallbackFileDataDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_read_receipt")]
        public static extern void CallbackReadReceipt(ToxHandle tox, ToxDelegates.CallbackReadReceiptDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_lossy_packet_registerhandler")]
        public static extern int CallbackLossyPacket(ToxHandle tox, int friendnumber, byte start_byte, ToxDelegates.CallbackPacketDelegate callback, IntPtr obj);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_lossless_packet_registerhandler")]
        public static extern int CallbackLosslessPacket(ToxHandle tox, int friendnumber, byte start_byte, ToxDelegates.CallbackPacketDelegate callback, IntPtr obj);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_avatar_info")]
        public static extern void CallbackAvatarInfo(ToxHandle tox, ToxDelegates.CallbackAvatarInfoDelegate callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_callback_avatar_data")]
        public static extern void CallbackAvatarData(ToxHandle tox, ToxDelegates.CallbackAvatarDataDelegate callback, IntPtr userdata);

        #endregion
    }
}

#pragma warning restore 1591