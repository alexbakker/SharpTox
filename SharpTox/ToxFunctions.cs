using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpTox
{
    static class ToxFunctions
    {
        #region Functions
        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tox_new(byte ipv6enabled);
        public static IntPtr New(bool ipv6enabled)
        {
            if (ipv6enabled)
                return tox_new(1);
            else
                return tox_new(0);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_bootstrap_from_address(IntPtr tox, string address, byte ipv6enabled, ushort port, byte[] public_key);
        public static bool BootstrapFromAddress(IntPtr tox, string address, bool ipv6enabled, int port, string public_key)
        {
            return tox_bootstrap_from_address(tox, address, ipv6enabled ? (byte)1 : (byte)0, (ushort)port, ToxTools.StringToHexBin(public_key)) == 1 ? true: false;
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_isconnected(IntPtr tox);
        public static bool IsConnected(IntPtr tox)
        {
            if (tox_isconnected(tox) == 0)
                return false;
            else
                return true;
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_get_address(IntPtr tox, byte[] address);
        public static byte[] GetAddress(IntPtr tox)
        {
            byte[] address = new byte[38];
            tox_get_address(tox, address);

            return address;
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_friend_number(IntPtr tox, byte[] client_id);
        public static int GetFriendNumber(IntPtr tox, string client_id)
        {
            return tox_get_friend_number(tox, ToxTools.StringToHexBin(client_id));
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_client_id(IntPtr tox, int friendnumber, byte[] address);
        public static string GetClientID(IntPtr tox, int friendnumber)
        {
            byte[] address = new byte[38];
            tox_get_client_id(tox, friendnumber, address);

            return ToxTools.HexBinToString(address);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_load(IntPtr tox, byte[] bytes, uint length);
        public static bool Load(IntPtr tox, byte[] bytes, uint length)
        {
            int result = tox_load(tox, bytes, length);

            if (result == -1)
                return false;
            else if (result == 0)
                return true;
            else
                throw new Exception("Unexpected result from tox_load");
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_do(IntPtr tox);
        public static void Do(IntPtr tox)
        {
            tox_do(tox);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_kill(IntPtr tox);
        public static void Kill(IntPtr tox)
        {
            tox_kill(tox);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_del_friend(IntPtr tox, int id);
        public static bool DeleteFriend(IntPtr tox, int id)
        {
            return tox_del_friend(tox, id) == 0 ? true : false;
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_friend_connection_status(IntPtr tox, int friendnumber);
        public static int GetFriendConnectionStatus(IntPtr tox, int friendnumber)
        {
            return tox_get_friend_connection_status(tox, friendnumber);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern byte tox_get_user_status(IntPtr tox, int friendnumber);
        public static ToxUserStatus GetUserStatus(IntPtr tox, int friendnumber)
        {
            return (ToxUserStatus)tox_get_user_status(tox, friendnumber);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern byte tox_get_self_user_status(IntPtr tox);
        public static ToxUserStatus GetSelfUserStatus(IntPtr tox)
        {
            return (ToxUserStatus)tox_get_self_user_status(tox);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_friend_exists(IntPtr tox, int friendnumber);
        public static bool FriendExists(IntPtr tox, int friendnumber)
        {
            return tox_friend_exists(tox, friendnumber) == 0 ? false : true;
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong tox_get_last_online(IntPtr tox, int friendnumber);
        public static ulong GetLastOnline(IntPtr tox, int friendnumber)
        {
            return tox_get_last_online(tox, friendnumber);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_count_friendlist(IntPtr tox);
        public static uint CountFriendlist(IntPtr tox)
        {
            return tox_count_friendlist(tox);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_size(IntPtr tox);
        public static uint Size(IntPtr tox)
        {
            return tox_size(tox);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_send_message(IntPtr tox, int friendnumber, byte[] message, int length);
        public static uint SendMessage(IntPtr tox, int friendnumber, string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            return tox_send_message(tox, friendnumber, bytes, bytes.Length);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_send_action(IntPtr tox, int friendnumber, byte[] action, int length);
        public static uint SendAction(IntPtr tox, int friendnumber, string action)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(action);
            return tox_send_action(tox, friendnumber, bytes, bytes.Length);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_save(IntPtr tox, byte[] bytes);
        public static bool Save(IntPtr tox, string filename)
        {
            try
            {
                byte[] bytes = new byte[tox_size(tox)];
                tox_save(tox, bytes);

                FileStream stream = new FileStream(filename, FileMode.Create);
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();

                return true;
            }
            catch { return false; }
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_add_friend(IntPtr tox, byte[] id, byte[] message, ushort length);
        public static int AddFriend(IntPtr tox, string id, string message)
        {
            byte[] binid = ToxTools.StringToHexBin(id);
            byte[] binmsg = Encoding.UTF8.GetBytes(message);

            return tox_add_friend(tox, binid, binmsg, (ushort)binmsg.Length);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_add_friend_norequest(IntPtr tox, byte[] id);
        public static int AddFriendNoRequest(IntPtr tox, string id)
        {
            byte[] binid = ToxTools.StringToHexBin(id);

            return tox_add_friend_norequest(tox, binid);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_set_name(IntPtr tox, byte[] name, ushort length);
        public static int SetName(IntPtr tox, string name)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(name);
            return tox_set_name(tox, bytes, (ushort)bytes.Length);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_set_user_is_typing(IntPtr tox, int friendnumber, byte is_typing);
        public static bool SetUserIsTyping(IntPtr tox, int friendnumber, bool is_typing)
        {
            byte typing = is_typing ? (byte)1 : (byte)0;
            return tox_set_user_is_typing(tox, friendnumber, typing) == 0 ? true : false;
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern ushort tox_get_self_name(IntPtr tox, byte[] name);
        public static string GetSelfName(IntPtr tox)
        {
            byte[] bytes = new byte[129];
            tox_get_self_name(tox, bytes);

            return Encoding.UTF8.GetString(bytes);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern byte tox_get_is_typing(IntPtr tox, int friendnumber);
        public static bool GetIsTyping(IntPtr tox, int friendnumber)
        {
            return tox_get_is_typing(tox, friendnumber) == 1 ? true : false;
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_set_user_status(IntPtr tox, byte userstatus);
        public static bool SetUserStatus(IntPtr tox, ToxUserStatus status)
        {
            if (tox_set_user_status(tox, (byte)status) == 0)
                return true;
            else
                return false;
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_set_status_message(IntPtr tox, byte[] message, ushort length);
        public static bool SetStatusMessage(IntPtr tox, string message)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            return tox_set_status_message(tox, msg, (ushort)msg.Length) == 0 ? true : false;
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_get_friendlist(IntPtr tox, int[] friends, uint[] trunc);
        public static int[] GetFriendlist(IntPtr tox)
        {
            int count = (int)tox_count_friendlist(tox);
            return GetFriendlist(tox, count);
        }

        public static int[] GetFriendlist(IntPtr tox, int count)
        {
            int[] friends = new int[count];
            uint[] trunc = new uint[0]; //shouldn't be needed anyways
            uint result = tox_get_friendlist(tox, friends, trunc);

            if (result == 0)
                return new int[0];
            else
                return friends;
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_get_num_online_friends(IntPtr tox);
        public static uint GetNumOnlineFriends(IntPtr tox)
        {
            return tox_get_num_online_friends(tox);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_name(IntPtr tox, int friendnumber, byte[] name);
        public static string GetName(IntPtr tox, int friendnumber)
        {
            int size = tox_get_name_size(tox, friendnumber);
            byte[] name = new byte[size];
            tox_get_name(tox, friendnumber, name);

            return Encoding.UTF8.GetString(name);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_status_message(IntPtr tox, int friendnumber, byte[] message, int maxlen);
        public static string GetStatusMessage(IntPtr tox, int friendnumber)
        {
            int size = tox_get_status_message_size(tox, friendnumber);
            byte[] status = new byte[size];
            tox_get_status_message(tox, friendnumber, status, status.Length);

            return Encoding.UTF8.GetString(status);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_self_status_message(IntPtr tox, byte[] message, int maxlen);
        public static string GetSelfStatusMessage(IntPtr tox)
        {
            int size = tox_get_self_status_message_size(tox);
            byte[] status = new byte[size];
            tox_get_self_status_message(tox, status, status.Length);

            return Encoding.UTF8.GetString(status);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_status_message_size(IntPtr tox, int friendnumber);
        public static int GetStatusMessageSize(IntPtr tox, int friendnumber)
        {
            return tox_get_status_message_size(tox, friendnumber);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_self_status_message_size(IntPtr tox);
        public static int GetSelfStatusMessageSize(IntPtr tox)
        {
            return tox_get_self_status_message_size(tox);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_name_size(IntPtr tox, int friendnumber);
        public static int GetNameSize(IntPtr tox, int friendnumber)
        {
            return tox_get_name_size(tox, friendnumber);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_get_self_name_size(IntPtr tox);
        public static int GetSelfNameSize(IntPtr tox)
        {
            return tox_get_self_name_size(tox);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_save_encrypted(IntPtr tox, byte[] data, byte[] key, ushort key_length);
        public static bool SaveEncrypted(IntPtr tox, string filename, string key)
        {
            try
            {
                byte[] bytes = new byte[tox_size_encrypted(tox)];
                byte[] k = Encoding.UTF8.GetBytes(key);

                if (tox_save_encrypted(tox, bytes, k, (ushort)k.Length) != 0)
                    return false;

                FileStream stream = new FileStream(filename, FileMode.Create);
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();

                return true;
            }
            catch { return false; }
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_size_encrypted(IntPtr tox);
        public static uint SizeEncrypted(IntPtr tox)
        {
            return tox_size_encrypted(tox);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_load_encrypted(IntPtr tox, byte[] data, uint length, byte[] key, ushort key_length);
        public static bool LoadEncrypted(IntPtr tox, byte[] data, byte[] key)
        {
            return tox_load_encrypted(tox, data, (uint)data.Length, key, (ushort)key.Length) == 0 ? true : false;
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_add_groupchat(IntPtr tox);
        public static int AddGroupchat(IntPtr tox)
        {
            return tox_add_groupchat(tox);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_del_groupchat(IntPtr tox, int groupnumber);
        public static int DeleteGroupchat(IntPtr tox, int groupnumber)
        {
            return tox_del_groupchat(tox, groupnumber);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_group_peername(IntPtr tox, int groupnumber, int peernumber, byte[] name);
        public static string GroupPeername(IntPtr tox, int groupnumber, int peernumber)
        {
            byte[] name = new byte[ToxConstants.MAX_NAME_LENGTH];
            if (tox_group_peername(tox, groupnumber, peernumber, name) == -1)
                throw new Exception("Could not get peer name");
            else
                return Encoding.UTF8.GetString(name);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_invite_friend(IntPtr tox, int friendnumber, int groupnumber);
        public static bool InviteFriend(IntPtr tox, int friendnumber, int groupnumber)
        {
            return tox_invite_friend(tox, friendnumber, groupnumber) == 0 ? true : false;
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_join_groupchat(IntPtr tox, int friendnumber, byte[] friend_group_public_key);
        public static int JoinGroupchat(IntPtr tox, int friendnumber, string friend_group_public_key)
        {
            return tox_join_groupchat(tox, friendnumber, ToxTools.StringToHexBin(friend_group_public_key));
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_group_message_send(IntPtr tox, int groupnumber, byte[] message, uint length);
        public static bool GroupMessageSend(IntPtr tox, int groupnumber, string message)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            return tox_group_message_send(tox, groupnumber, msg, (uint)msg.Length) == 0 ? true: false;
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_group_action_send(IntPtr tox, int groupnumber, byte[] action, uint length);
        public static bool GroupActionSend(IntPtr tox, int groupnumber, string action)
        {
            byte[] act = Encoding.UTF8.GetBytes(action);
            return tox_group_action_send(tox, groupnumber, act, (uint)act.Length) == 0 ? true : false;
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_group_number_peers(IntPtr tox, int groupnumber);
        public static int GroupNumberPeers(IntPtr tox, int groupnumber)
        {
            return tox_group_number_peers(tox, groupnumber);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_count_chatlist(IntPtr tox);
        public static int CountChatlist(IntPtr tox)
        {
            return (int)tox_count_chatlist(tox);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint tox_get_chatlist(IntPtr tox, int[] out_list, uint[] list_size);
        public static int[] GetChatlist(IntPtr tox)
        {
            int[] chats = new int[tox_count_chatlist(tox)];
            uint[] trunc = new uint[0]; //shouldn't be needed anyways
            uint result = tox_get_chatlist(tox, chats, trunc);

            if (result == 0)
                return new int[0];
            else
                return chats;
        }



        #endregion

        #region Callbacks
        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_friend_request(IntPtr tox, ToxDelegates.CallbackFriendRequestDelegate callback, IntPtr userdata);
        public static void CallbackFriendRequest(IntPtr tox, ToxDelegates.CallbackFriendRequestDelegate callback)
        {
            tox_callback_friend_request(tox, callback, IntPtr.Zero);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_connection_status(IntPtr tox, ToxDelegates.CallbackConnectionStatusDelegate callback, IntPtr userdata);
        public static void CallbackConnectionStatus(IntPtr tox, ToxDelegates.CallbackConnectionStatusDelegate callback)
        {
            tox_callback_connection_status(tox, callback, IntPtr.Zero);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_friend_message(IntPtr tox, ToxDelegates.CallbackFriendMessageDelegate callback, IntPtr userdata);
        public static void CallbackFriendMessage(IntPtr tox, ToxDelegates.CallbackFriendMessageDelegate callback)
        {
            tox_callback_friend_message(tox, callback, IntPtr.Zero);
        }
        
        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_friend_action(IntPtr tox, ToxDelegates.CallbackFriendActionDelegate callback, IntPtr userdata);
        public static void CallbackFriendAction(IntPtr tox, ToxDelegates.CallbackFriendActionDelegate callback)
        {
            tox_callback_friend_action(tox, callback, IntPtr.Zero);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_name_change(IntPtr tox, ToxDelegates.CallbackNameChangeDelegate callback, IntPtr userdata);
        public static void CallbackNameChange(IntPtr tox, ToxDelegates.CallbackNameChangeDelegate callback)
        {
            tox_callback_name_change(tox, callback, IntPtr.Zero);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_status_message(IntPtr tox, ToxDelegates.CallbackStatusMessageDelegate callback, IntPtr userdata);
        public static void CallbackStatusMessage(IntPtr tox, ToxDelegates.CallbackStatusMessageDelegate callback)
        {
            tox_callback_status_message(tox, callback, IntPtr.Zero);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_callback_user_status(IntPtr tox, ToxDelegates.CallbackUserStatusDelegate callback, IntPtr userdata);
        public static void CallbackUserStatus(IntPtr tox, ToxDelegates.CallbackUserStatusDelegate callback)
        {
            tox_callback_user_status(tox, callback, IntPtr.Zero);
        }

        [DllImport("libtoxcore.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void tox_callback_typing_change(IntPtr tox, ToxDelegates.CallbackTypingChangeDelegate callback, IntPtr userdata);
        public static void CallbackTypingChange(IntPtr tox, ToxDelegates.CallbackTypingChangeDelegate callback)
        {
            tox_callback_typing_change(tox, callback, IntPtr.Zero);
        }

        #endregion
    }
}
