#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpTox.Core
{
    public static class ToxFunctions
    {
        const string dll = "libtox";

        #region Functions
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxHandle tox_new(byte ipv6enabled);
        public static ToxHandle New(bool ipv6enabled)
        {
            if (ipv6enabled)
                return tox_new(1);
            else
                return tox_new(0);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_bootstrap_from_address(ToxHandle tox, string address, byte ipv6enabled, ushort port, byte[] public_key);
        public static bool BootstrapFromAddress(ToxHandle tox, string address, bool ipv6enabled, ushort port, string public_key)
        {
            return tox_bootstrap_from_address(tox, address, ipv6enabled ? (byte)1 : (byte)0, (ushort)IPAddress.HostToNetworkOrder((short)port), ToxTools.StringToHexBin(public_key)) == 1;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_isconnected(ToxHandle tox);
        public static bool IsConnected(ToxHandle tox)
        {
            return tox_isconnected(tox) != 0;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_get_address(ToxHandle tox, byte[] address);
        public static byte[] GetAddress(ToxHandle tox)
        {
            byte[] address = new byte[38];
            tox_get_address(tox, address);

            return address;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_friend_number(ToxHandle tox, byte[] client_id);
        public static int GetFriendNumber(ToxHandle tox, string client_id)
        {
            return tox_get_friend_number(tox, ToxTools.StringToHexBin(client_id));
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_client_id(ToxHandle tox, int friendnumber, byte[] address);
        public static string GetClientID(ToxHandle tox, int friendnumber)
        {
            byte[] address = new byte[32];
            tox_get_client_id(tox, friendnumber, address);

            return ToxTools.HexBinToString(address);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_load(ToxHandle tox, byte[] bytes, uint length);
        public static bool Load(ToxHandle tox, byte[] bytes, uint length)
        {
            return tox_load(tox, bytes, length) == 0;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_do(ToxHandle tox);
        public static void Do(ToxHandle tox)
        {
            tox_do(tox);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_do_interval(ToxHandle tox);
        public static uint DoInterval(ToxHandle tox)
        {
            return tox_do_interval(tox);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_kill(IntPtr tox);
        public static void Kill(IntPtr tox)
        {
            tox_kill(tox);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_del_friend(ToxHandle tox, int id);
        public static bool DeleteFriend(ToxHandle tox, int id)
        {
            return tox_del_friend(tox, id) == 0;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_friend_connection_status(ToxHandle tox, int friendnumber);
        public static int GetFriendConnectionStatus(ToxHandle tox, int friendnumber)
        {
            return tox_get_friend_connection_status(tox, friendnumber);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern byte tox_get_user_status(ToxHandle tox, int friendnumber);
        public static ToxUserStatus GetUserStatus(ToxHandle tox, int friendnumber)
        {
            return (ToxUserStatus)tox_get_user_status(tox, friendnumber);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern byte tox_get_self_user_status(ToxHandle tox);
        public static ToxUserStatus GetSelfUserStatus(ToxHandle tox)
        {
            return (ToxUserStatus)tox_get_self_user_status(tox);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_friend_exists(ToxHandle tox, int friendnumber);
        public static bool FriendExists(ToxHandle tox, int friendnumber)
        {
            return tox_friend_exists(tox, friendnumber) != 0;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong tox_get_last_online(ToxHandle tox, int friendnumber);
        public static ulong GetLastOnline(ToxHandle tox, int friendnumber)
        {
            return tox_get_last_online(tox, friendnumber);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_count_friendlist(ToxHandle tox);
        public static uint CountFriendlist(ToxHandle tox)
        {
            return tox_count_friendlist(tox);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_size(ToxHandle tox);
        public static uint Size(ToxHandle tox)
        {
            return tox_size(tox);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_send_message(ToxHandle tox, int friendnumber, byte[] message, int length);
        public static int SendMessage(ToxHandle tox, int friendnumber, string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            return (int)tox_send_message(tox, friendnumber, bytes, bytes.Length);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_send_message_withid(ToxHandle tox, int friendnumber, int id, byte[] message, int length);
        public static int SendMessageWithID(ToxHandle tox, int friendnumber, int id, string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            return (int)tox_send_message_withid(tox, friendnumber, id, bytes, bytes.Length);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_send_action(ToxHandle tox, int friendnumber, byte[] action, int length);
        public static int SendAction(ToxHandle tox, int friendnumber, string action)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(action);
            return (int)tox_send_action(tox, friendnumber, bytes, bytes.Length);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_send_action_withid(ToxHandle tox, int friendnumber, int id, byte[] message, int length);
        public static int SendActionWithID(ToxHandle tox, int friendnumber, int id, string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            return (int)tox_send_action_withid(tox, friendnumber, id, bytes, bytes.Length);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_save(ToxHandle tox, byte[] bytes);
        public static void Save(ToxHandle tox, byte[] bytes)
        {
            tox_save(tox, bytes);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_add_friend(ToxHandle tox, byte[] id, byte[] message, ushort length);
        public static int AddFriend(ToxHandle tox, string id, string message)
        {
            byte[] binid = ToxTools.StringToHexBin(id);
            byte[] binmsg = Encoding.UTF8.GetBytes(message);

            return tox_add_friend(tox, binid, binmsg, (ushort)binmsg.Length);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_add_friend_norequest(ToxHandle tox, byte[] id);
        public static int AddFriendNoRequest(ToxHandle tox, string id)
        {
            byte[] binid = ToxTools.StringToHexBin(id);

            return tox_add_friend_norequest(tox, binid);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_set_name(ToxHandle tox, byte[] name, ushort length);
        public static bool SetName(ToxHandle tox, string name)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(name);
            return tox_set_name(tox, bytes, (ushort)bytes.Length) == 0;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_set_user_is_typing(ToxHandle tox, int friendnumber, byte is_typing);
        public static bool SetUserIsTyping(ToxHandle tox, int friendnumber, bool is_typing)
        {
            byte typing = is_typing ? (byte)1 : (byte)0;
            return tox_set_user_is_typing(tox, friendnumber, typing) == 0;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern ushort tox_get_self_name(ToxHandle tox, byte[] name);
        public static string GetSelfName(ToxHandle tox)
        {
            byte[] bytes = new byte[129];
            tox_get_self_name(tox, bytes);

            return Encoding.UTF8.GetString(bytes);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern byte tox_get_is_typing(ToxHandle tox, int friendnumber);
        public static bool GetIsTyping(ToxHandle tox, int friendnumber)
        {
            return tox_get_is_typing(tox, friendnumber) == 1;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_set_user_status(ToxHandle tox, byte userstatus);
        public static bool SetUserStatus(ToxHandle tox, ToxUserStatus status)
        {
            return tox_set_user_status(tox, (byte)status) == 0;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_set_status_message(ToxHandle tox, byte[] message, ushort length);
        public static bool SetStatusMessage(ToxHandle tox, string message)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            return tox_set_status_message(tox, msg, (ushort)msg.Length) == 0;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_get_friendlist(ToxHandle tox, int[] friends, uint[] trunc);
        public static int[] GetFriendlist(ToxHandle tox)
        {
            int count = (int)tox_count_friendlist(tox);
            return GetFriendlist(tox, count);
        }

        public static int[] GetFriendlist(ToxHandle tox, int count)
        {
            int[] friends = new int[count];
            uint[] trunc = new uint[0]; //shouldn't be needed anyways
            uint result = tox_get_friendlist(tox, friends, trunc);

            if (result == 0)
                return new int[0];
            else
                return friends;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_get_num_online_friends(ToxHandle tox);
        public static uint GetNumOnlineFriends(ToxHandle tox)
        {
            return tox_get_num_online_friends(tox);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_name(ToxHandle tox, int friendnumber, byte[] name);
        public static string GetName(ToxHandle tox, int friendnumber)
        {
            int size = tox_get_name_size(tox, friendnumber);
            byte[] name = new byte[size];
            tox_get_name(tox, friendnumber, name);

            return Encoding.UTF8.GetString(name);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_status_message(ToxHandle tox, int friendnumber, byte[] message, int maxlen);
        public static string GetStatusMessage(ToxHandle tox, int friendnumber)
        {
            int size = tox_get_status_message_size(tox, friendnumber);
            byte[] status = new byte[size];
            tox_get_status_message(tox, friendnumber, status, status.Length);

            return Encoding.UTF8.GetString(status);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_self_status_message(ToxHandle tox, byte[] message, int maxlen);
        public static string GetSelfStatusMessage(ToxHandle tox)
        {
            int size = tox_get_self_status_message_size(tox);
            byte[] status = new byte[size];
            tox_get_self_status_message(tox, status, status.Length);

            return Encoding.UTF8.GetString(status);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_status_message_size(ToxHandle tox, int friendnumber);
        public static int GetStatusMessageSize(ToxHandle tox, int friendnumber)
        {
            return tox_get_status_message_size(tox, friendnumber);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_self_status_message_size(ToxHandle tox);
        public static int GetSelfStatusMessageSize(ToxHandle tox)
        {
            return tox_get_self_status_message_size(tox);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_name_size(ToxHandle tox, int friendnumber);
        public static int GetNameSize(ToxHandle tox, int friendnumber)
        {
            return tox_get_name_size(tox, friendnumber);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_self_name_size(ToxHandle tox);
        public static int GetSelfNameSize(ToxHandle tox)
        {
            return tox_get_self_name_size(tox);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_add_groupchat(ToxHandle tox);
        public static int AddGroupchat(ToxHandle tox)
        {
            return tox_add_groupchat(tox);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_del_groupchat(ToxHandle tox, int groupnumber);
        public static bool DeleteGroupchat(ToxHandle tox, int groupnumber)
        {
            return tox_del_groupchat(tox, groupnumber) == 0;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_group_peername(ToxHandle tox, int groupnumber, int peernumber, byte[] name);
        public static string GroupPeername(ToxHandle tox, int groupnumber, int peernumber)
        {
            byte[] name = new byte[ToxConstants.MAX_NAME_LENGTH];
            if (tox_group_peername(tox, groupnumber, peernumber, name) == -1)
                throw new Exception("Could not get peer name");
            else
                return ToxTools.RemoveNull(Encoding.UTF8.GetString(name));
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_invite_friend(ToxHandle tox, int friendnumber, int groupnumber);
        public static bool InviteFriend(ToxHandle tox, int friendnumber, int groupnumber)
        {
            return tox_invite_friend(tox, friendnumber, groupnumber) == 0;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_join_groupchat(ToxHandle tox, int friendnumber, byte[] friend_group_public_key);
        public static int JoinGroupchat(ToxHandle tox, int friendnumber, string friend_group_public_key)
        {
            return tox_join_groupchat(tox, friendnumber, ToxTools.StringToHexBin(friend_group_public_key));
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_group_message_send(ToxHandle tox, int groupnumber, byte[] message, uint length);
        public static bool GroupMessageSend(ToxHandle tox, int groupnumber, string message)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            return tox_group_message_send(tox, groupnumber, msg, (uint)msg.Length) == 0;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_group_action_send(ToxHandle tox, int groupnumber, byte[] action, uint length);
        public static bool GroupActionSend(ToxHandle tox, int groupnumber, string action)
        {
            byte[] act = Encoding.UTF8.GetBytes(action);
            return tox_group_action_send(tox, groupnumber, act, (uint)act.Length) == 0;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_group_number_peers(ToxHandle tox, int groupnumber);
        public static int GroupNumberPeers(ToxHandle tox, int groupnumber)
        {
            return tox_group_number_peers(tox, groupnumber);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int tox_group_get_names(ToxHandle tox, int groupnumber, byte[,] names, ushort[] lengths, ushort length);
        public static string[] GroupGetNames(ToxHandle tox, int groupnumber)
        {
            int count = tox_group_number_peers(tox, groupnumber);

            //just return an empty string array before we get an overflow exception
            if (count <= 0)
                return new string[0];

            ushort[] lengths = new ushort[count];
            byte[,] matrix = new byte[count, ToxConstants.MAX_NAME_LENGTH];

            int result = tox_group_get_names(tox, groupnumber, matrix, lengths, (ushort)count);

            string[] names = new string[count];
            for (int i = 0; i < count; i++)
            {
                byte[] name = new byte[lengths[i]];

                for (int j = 0; j < name.Length; j++)
                    name[j] = matrix[i, j];

                names[i] = ToxTools.RemoveNull(Encoding.UTF8.GetString(name));
            }

            return names;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_count_chatlist(ToxHandle tox);
        public static int CountChatlist(ToxHandle tox)
        {
            return (int)tox_count_chatlist(tox);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_get_chatlist(ToxHandle tox, int[] out_list, uint[] list_size);
        public static int[] GetChatlist(ToxHandle tox)
        {
            int[] chats = new int[tox_count_chatlist(tox)];
            uint[] trunc = new uint[0]; //shouldn't be needed anyways
            uint result = tox_get_chatlist(tox, chats, trunc);

            if (result == 0)
                return new int[0];
            else
                return chats;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_new_file_sender(ToxHandle tox, int friendnumber, ulong filesize, byte[] filename, ushort filename_length); //max filename length is 255 bytes
        public static int NewFileSender(ToxHandle tox, int friendnumber, ulong filesize, string filename)
        {
            byte[] name = Encoding.UTF8.GetBytes(filename);
            if (name.Length > 255)
                throw new Exception("Filename is too long (longer than 255 bytes)");

            int result = tox_new_file_sender(tox, friendnumber, filesize, name, (ushort)name.Length);
            if (result != -1)
                return result;
            else
                throw new Exception("Could not create new file sender");
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_file_send_control(ToxHandle tox, int friendnumber, byte send_receive, byte filenumber, byte message_id, byte[] data, ushort length);
        public static bool FileSendControl(ToxHandle tox, int friendnumber, byte send_receive, byte filenumber, byte message_id, byte[] data, ushort length)
        {
            return tox_file_send_control(tox, friendnumber, send_receive, filenumber, message_id, data, length) == 0;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_file_send_data(ToxHandle tox, int friendnumber, byte filenumber, byte[] data, ushort length);
        public static bool FileSendData(ToxHandle tox, int friendnumber, int filenumber, byte[] data)
        {
            return tox_file_send_data(tox, friendnumber, (byte)filenumber, data, (ushort)data.Length) == 0;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_file_data_size(ToxHandle tox, int friendnumber);
        public static int FileDataSize(ToxHandle tox, int friendnumber)
        {
            int result = tox_file_data_size(tox, friendnumber);
            if (result == -1)
                throw new Exception("Could not get file data size");
            else
                return result;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong tox_file_data_remaining(ToxHandle tox, int friendnumber, byte filenumber, byte send_receive);
        public static ulong FileDataRemaining(ToxHandle tox, int friendnumber, int filenumber, int send_receive)
        {
            return tox_file_data_remaining(tox, friendnumber, (byte)filenumber, (byte)send_receive);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_get_nospam(ToxHandle tox);
        public static uint GetNospam(ToxHandle tox)
        {
            return tox_get_nospam(tox);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_set_nospam(ToxHandle tox, uint nospam);
        public static void SetNospam(ToxHandle tox, uint nospam)
        {
            tox_set_nospam(tox, nospam);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_set_sends_receipts(ToxHandle tox, int friendnumber, int yesno);
        public static void SetSendsReceipts(ToxHandle tox, int friendnumber, bool send_receipts)
        {
            tox_set_sends_receipts(tox, friendnumber, send_receipts ? 1 : 0);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_get_keys(ToxHandle tox, byte[] public_key, byte[] secret_key);
        public static ToxKeyPair GetKeys(ToxHandle tox)
        {
            byte[] public_key = new byte[32];
            byte[] secret_key = new byte[32];

            tox_get_keys(tox, public_key, secret_key);

            return new ToxKeyPair(
                new ToxKey(ToxKeyType.Public, public_key),
                new ToxKey(ToxKeyType.Secret, secret_key)
                );
        }

        #endregion

        #region Callbacks
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_friend_request(ToxHandle tox, ToxDelegates.CallbackFriendRequestDelegate callback, IntPtr userdata);
        public static void CallbackFriendRequest(ToxHandle tox, ToxDelegates.CallbackFriendRequestDelegate callback)
        {
            tox_callback_friend_request(tox, callback, IntPtr.Zero);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_connection_status(ToxHandle tox, ToxDelegates.CallbackConnectionStatusDelegate callback, IntPtr userdata);
        public static void CallbackConnectionStatus(ToxHandle tox, ToxDelegates.CallbackConnectionStatusDelegate callback)
        {
            tox_callback_connection_status(tox, callback, IntPtr.Zero);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_friend_message(ToxHandle tox, ToxDelegates.CallbackFriendMessageDelegate callback, IntPtr userdata);
        public static void CallbackFriendMessage(ToxHandle tox, ToxDelegates.CallbackFriendMessageDelegate callback)
        {
            tox_callback_friend_message(tox, callback, IntPtr.Zero);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_friend_action(ToxHandle tox, ToxDelegates.CallbackFriendActionDelegate callback, IntPtr userdata);
        public static void CallbackFriendAction(ToxHandle tox, ToxDelegates.CallbackFriendActionDelegate callback)
        {
            tox_callback_friend_action(tox, callback, IntPtr.Zero);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_name_change(ToxHandle tox, ToxDelegates.CallbackNameChangeDelegate callback, IntPtr userdata);
        public static void CallbackNameChange(ToxHandle tox, ToxDelegates.CallbackNameChangeDelegate callback)
        {
            tox_callback_name_change(tox, callback, IntPtr.Zero);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_status_message(ToxHandle tox, ToxDelegates.CallbackStatusMessageDelegate callback, IntPtr userdata);
        public static void CallbackStatusMessage(ToxHandle tox, ToxDelegates.CallbackStatusMessageDelegate callback)
        {
            tox_callback_status_message(tox, callback, IntPtr.Zero);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_user_status(ToxHandle tox, ToxDelegates.CallbackUserStatusDelegate callback, IntPtr userdata);
        public static void CallbackUserStatus(ToxHandle tox, ToxDelegates.CallbackUserStatusDelegate callback)
        {
            tox_callback_user_status(tox, callback, IntPtr.Zero);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void tox_callback_typing_change(ToxHandle tox, ToxDelegates.CallbackTypingChangeDelegate callback, IntPtr userdata);
        public static void CallbackTypingChange(ToxHandle tox, ToxDelegates.CallbackTypingChangeDelegate callback)
        {
            tox_callback_typing_change(tox, callback, IntPtr.Zero);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_group_invite(ToxHandle tox, ToxDelegates.CallbackGroupInviteDelegate callback, IntPtr userdata);
        public static void CallbackGroupInvite(ToxHandle tox, ToxDelegates.CallbackGroupInviteDelegate callback)
        {
            tox_callback_group_invite(tox, callback, IntPtr.Zero);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_group_message(ToxHandle tox, ToxDelegates.CallbackGroupMessageDelegate callback, IntPtr userdata);
        public static void CallbackGroupMessage(ToxHandle tox, ToxDelegates.CallbackGroupMessageDelegate callback)
        {
            tox_callback_group_message(tox, callback, IntPtr.Zero);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_group_action(ToxHandle tox, ToxDelegates.CallbackGroupActionDelegate callback, IntPtr userdata);
        public static void CallbackGroupAction(ToxHandle tox, ToxDelegates.CallbackGroupActionDelegate callback)
        {
            tox_callback_group_action(tox, callback, IntPtr.Zero);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_group_namelist_change(ToxHandle tox, ToxDelegates.CallbackGroupNamelistChangeDelegate callback, IntPtr userdata);
        public static void CallbackGroupNamelistChange(ToxHandle tox, ToxDelegates.CallbackGroupNamelistChangeDelegate callback)
        {
            tox_callback_group_namelist_change(tox, callback, IntPtr.Zero);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_file_send_request(ToxHandle tox, ToxDelegates.CallbackFileSendRequestDelegate callback, IntPtr userdata);
        public static void CallbackFileSendRequest(ToxHandle tox, ToxDelegates.CallbackFileSendRequestDelegate callback)
        {
            tox_callback_file_send_request(tox, callback, IntPtr.Zero);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_file_control(ToxHandle tox, ToxDelegates.CallbackFileControlDelegate callback, IntPtr userdata);
        public static void CallbackFileControl(ToxHandle tox, ToxDelegates.CallbackFileControlDelegate callback)
        {
            tox_callback_file_control(tox, callback, IntPtr.Zero);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_file_data(ToxHandle tox, ToxDelegates.CallbackFileDataDelegate callback, IntPtr userdata);
        public static void CallbackFileData(ToxHandle tox, ToxDelegates.CallbackFileDataDelegate callback)
        {
            tox_callback_file_data(tox, callback, IntPtr.Zero);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_read_receipt(ToxHandle tox, ToxDelegates.CallbackReadReceiptDelegate callback, IntPtr userdata);
        public static void CallbackReadReceipt(ToxHandle tox, ToxDelegates.CallbackReadReceiptDelegate callback)
        {
            tox_callback_read_receipt(tox, callback, IntPtr.Zero);
        }

        #endregion
    }
}

#pragma warning restore 1591