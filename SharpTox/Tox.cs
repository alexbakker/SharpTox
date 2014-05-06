using System;
using System.IO;
using System.Text;
using System.Threading;

namespace SharpTox
{
    #region Event Delegates
    public delegate void OnFriendRequestDelegate(string id, string message);
    public delegate void OnConnectionStatusDelegate(int friendnumber, byte status);
    public delegate void OnFriendMessageDelegate(int friendnumber, string message);
    public delegate void OnFriendActionDelegate(int friendnumber, string action);
    public delegate void OnNameChangeDelegate(int friendnumber, string newname);
    public delegate void OnStatusMessageDelegate(int friendnumber, string newstatus);
    public delegate void OnUserStatusDelegate(int friendnumber, ToxUserStatus status);
    public delegate void OnTypingChangeDelegate(int friendnumber, bool is_typing);

    public delegate void OnGroupInviteDelegate(int friendnumber, string group_public_key);
    public delegate void OnGroupMessageDelegate(int groupnumber, int friendgroupnumber, string message);
    public delegate void OnGroupActionDelegate(int groupnumber, int friendgroupnumber, string action);
    public delegate void OnGroupNamelistChangeDelegate(int groupnumber, int peernumber, ToxChatChange change);
    #endregion

    public class Tox
    {
        public event OnFriendRequestDelegate OnFriendRequest;
        public event OnConnectionStatusDelegate OnConnectionStatusChanged;
        public event OnFriendMessageDelegate OnFriendMessage;
        public event OnFriendActionDelegate OnFriendAction;
        public event OnNameChangeDelegate OnNameChange;
        public event OnStatusMessageDelegate OnStatusMessage;
        public event OnUserStatusDelegate OnUserStatus;
        public event OnTypingChangeDelegate OnTypingChange;

        public event OnGroupActionDelegate OnGroupAction;
        public event OnGroupMessageDelegate OnGroupMessage;
        public event OnGroupInviteDelegate OnGroupInvite;
        public event OnGroupNamelistChangeDelegate OnGroupNamelistChange;

        #region Callback Delegates
        private ToxDelegates.CallbackFriendRequestDelegate friendrequestdelegate;
        private ToxDelegates.CallbackConnectionStatusDelegate connectionstatusdelegate;
        private ToxDelegates.CallbackFriendMessageDelegate friendmessagedelegate;
        private ToxDelegates.CallbackFriendActionDelegate friendactiondelegate;
        private ToxDelegates.CallbackNameChangeDelegate namechangedelegate;
        private ToxDelegates.CallbackStatusMessageDelegate statusmessagedelegate;
        private ToxDelegates.CallbackUserStatusDelegate userstatusdelegate;
        private ToxDelegates.CallbackTypingChangeDelegate typingchangedelegate;

        private ToxDelegates.CallbackGroupInviteDelegate groupinvitedelegate;
        private ToxDelegates.CallbackGroupActionDelegate groupactiondelegate;
        private ToxDelegates.CallbackGroupMessageDelegate groupmessagedelegate;
        private ToxDelegates.CallbackGroupNamelistChangeDelegate groupnamelistchangedelegate;
        #endregion

        private IntPtr tox;
        private Thread thread;

        public bool Ipv6Enabled { get; private set; }

        public Tox(bool ipv6enabled)
        {
            tox = ToxFunctions.New(ipv6enabled);
            Ipv6Enabled = ipv6enabled;

            callbacks();
        }

        public bool IsConnected()
        {
            return ToxFunctions.IsConnected(tox);
        }

        public bool Load(string filename)
        {
            try
            {
                FileInfo info = new FileInfo(filename);
                FileStream stream = new FileStream(filename, FileMode.Open);
                byte[] bytes = new byte[info.Length];

                stream.Read(bytes, 0, (int)info.Length);
                stream.Close();

                if (!ToxFunctions.Load(tox, bytes, (uint)bytes.Length))
                    return false;
                else
                    return true;
            }
            catch { return false; }
        }

        public bool LoadEncrypted(string filename, string key)
        {
            try
            {
                FileInfo info = new FileInfo(filename);
                FileStream stream = new FileStream(filename, FileMode.Open);
                byte[] bytes = new byte[info.Length];

                stream.Read(bytes, 0, (int)info.Length);
                stream.Close();

                byte[] k = Encoding.UTF8.GetBytes(key);

                if (!ToxFunctions.LoadEncrypted(tox, bytes, k))
                    return false;
                else
                    return true;
            }
            catch { return false; }
        }

        public void Start()
        {
            thread = new Thread(loop);
            thread.Start();
        }

        private void loop()
        {
            while (true)
            {
                //tox_do should be called at least 20 times per second
                ToxFunctions.Do(tox);
                Thread.Sleep(25);
            }
        }

        public int AddFriend(string id, string message)
        {
            int result = ToxFunctions.AddFriend(tox, id, message);

            if (result < 0)
                throw new Exception("Could not add friend: " + (ToxAFError)result);
            else
                return result;
        }
        public int AddFriend(string id)
        {
            int result = ToxFunctions.AddFriend(tox, id, "No message.");

            if (result < 0)
                throw new Exception("Could not add friend: " + (ToxAFError)result);
            else
                return result;
        }

        public int AddFriendNoRequest(string id)
        {
            int result = ToxFunctions.AddFriendNoRequest(tox, id);

            if (result < 0)
                throw new Exception("Could not add friend: " + (ToxAFError)result);
            else
                return result;
        }

        public bool TryBootstrap(ToxNode node)
        {
            return ToxFunctions.BootstrapFromAddress(tox, node.Address, node.Ipv6Enabled, Convert.ToUInt16(node.Port), node.PublicKey);
        }

        public bool FriendExists(int friendnumber)
        {
            return ToxFunctions.FriendExists(tox, friendnumber);
        }

        public int GetFriendlistCount()
        {
            return (int)ToxFunctions.CountFriendlist(tox);
        }

        public int[] GetFriendlist()
        {
            return ToxFunctions.GetFriendlist(tox);
        }

        public string GetName(int friendnumber)
        {
            return ToxFunctions.GetName(tox, friendnumber);
        }

        public string GetSelfName()
        {
            return ToxTools.RemoveNull(ToxFunctions.GetSelfName(tox));
        }

        public long GetLastOnline(int friendnumber)
        {
            return (long)ToxFunctions.GetLastOnline(tox, friendnumber);
        }

        public string GetAddress()
        {
            return ToxTools.HexBinToString(ToxFunctions.GetAddress(tox));
        }

        public bool GetIsTyping(int friendnumber)
        {
            return ToxFunctions.GetIsTyping(tox, friendnumber);
        }

        public int GetFriendNumber(string id)
        {
            return ToxFunctions.GetFriendNumber(tox, id);
        }

        public string GetStatusMessage(int friendnumber)
        {
            return ToxFunctions.GetStatusMessage(tox, friendnumber);
        }

        public string GetSelfStatusMessage()
        {
            return ToxFunctions.GetSelfStatusMessage(tox);
        }

        public int GetOnlineFriendsCount()
        {
            return (int)ToxFunctions.GetNumOnlineFriends(tox);
        }
        
        public int GetFriendConnectionStatus(int friendnumber)
        {
            return ToxFunctions.GetFriendConnectionStatus(tox, friendnumber);
        }

        public string GetClientID(int friendnumber)
        {
            return ToxFunctions.GetClientID(tox, friendnumber);
        }

        public ToxUserStatus GetUserStatus(int friendnumber)
        {
            return ToxFunctions.GetUserStatus(tox, friendnumber);
        }

        public ToxUserStatus GetSelfUserStatus()
        {
            return ToxFunctions.GetSelfUserStatus(tox);
        }

        public void SetName(string name)
        {
            ToxFunctions.SetName(tox, name);
        }

        public bool SetUserStatus(ToxUserStatus status)
        {
            return ToxFunctions.SetUserStatus(tox, status);
        }

        public bool SetStatusMessage(string message)
        {
            return ToxFunctions.SetStatusMessage(tox, message);
        }

        public bool SetUserIsTyping(int friendnumber, bool is_typing)
        {
            return ToxFunctions.SetUserIsTyping(tox, friendnumber, is_typing);
        }

        public void SendMessage(int friendnumber, string message)
        {
            ToxFunctions.SendMessage(tox, friendnumber, message);
        }

        public void SendAction(int friendnumber, string action)
        {
            ToxFunctions.SendAction(tox, friendnumber, action);
        }

        public bool Save(string filename)
        {
            return ToxFunctions.Save(tox, filename);
        }

        public bool SaveEncrypted(string filename, string key)
        {
            return ToxFunctions.SaveEncrypted(tox, filename, key);
        }

        public void Kill()
        {
            thread.Abort();
            ToxFunctions.Kill(tox);
        }

        public bool DeleteFriend(int friendnumber)
        {
            return ToxFunctions.DeleteFriend(tox, friendnumber);
        }

        public int JoinGroup(int friendnumber, string group_public_key)
        {
            return ToxFunctions.JoinGroupchat(tox, friendnumber, group_public_key);
        }

        public string GetGroupMemberName(int groupnumber, int peernumber)
        {
            return ToxFunctions.GroupPeername(tox, groupnumber, peernumber);
        }

        public int GetGroupMemberCount(int groupnumber)
        {
            return ToxFunctions.GroupNumberPeers(tox, groupnumber);
        }

        public int DeleteGroupChat(int groupnumber)
        {
            return ToxFunctions.DeleteGroupchat(tox, groupnumber);
        }

        public bool InviteFriend(int friendnumber, int groupnumber)
        {
            return ToxFunctions.InviteFriend(tox, friendnumber, groupnumber);
        }

        public bool SendGroupMessage(int groupnumber, string message)
        {
            return ToxFunctions.GroupMessageSend(tox, groupnumber, message);
        }

        public bool SendGroupAction(int groupnumber, string action)
        {
            return ToxFunctions.GroupActionSend(tox, groupnumber, action);
        }

        public int NewGroup()
        {
            return ToxFunctions.AddGroupchat(tox);
        }

        private void callbacks()
        {
            ToxFunctions.CallbackFriendRequest(tox, friendrequestdelegate = new ToxDelegates.CallbackFriendRequestDelegate((IntPtr t, byte[] id, byte[] message, ushort length, IntPtr userdata) =>
            {
                if (OnFriendRequest != null)
                    OnFriendRequest(ToxTools.RemoveNull(ToxTools.HexBinToString(id)), Encoding.UTF8.GetString(message));
            }));

            ToxFunctions.CallbackConnectionStatus(tox, connectionstatusdelegate = new ToxDelegates.CallbackConnectionStatusDelegate((IntPtr t, int friendnumber, byte status, IntPtr userdata) =>
            {
                if (OnConnectionStatusChanged != null)
                    OnConnectionStatusChanged(friendnumber, status);
            }));

            ToxFunctions.CallbackFriendMessage(tox, friendmessagedelegate = new ToxDelegates.CallbackFriendMessageDelegate((IntPtr t, int friendnumber, byte[] message, ushort length, IntPtr userdata) =>
            {
                if (OnFriendMessage != null)
                    OnFriendMessage(friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(message)));
            }));

            ToxFunctions.CallbackFriendAction(tox, friendactiondelegate = new ToxDelegates.CallbackFriendActionDelegate((IntPtr t, int friendnumber, byte[] action, ushort length, IntPtr userdata) =>
            {
                if (OnFriendAction != null)
                    OnFriendAction(friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(action)));
            }));

            ToxFunctions.CallbackNameChange(tox, namechangedelegate = new ToxDelegates.CallbackNameChangeDelegate((IntPtr t, int friendnumber, byte[] newname, ushort length, IntPtr userdata) =>
            {
                if (OnNameChange != null)
                    OnNameChange(friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(newname)));
            }));

            ToxFunctions.CallbackStatusMessage(tox, statusmessagedelegate = new ToxDelegates.CallbackStatusMessageDelegate((IntPtr t, int friendnumber, byte[] newstatus, ushort length, IntPtr userdata) =>
            {
                if (OnStatusMessage != null)
                    OnStatusMessage(friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(newstatus)));
            }));

            ToxFunctions.CallbackUserStatus(tox, userstatusdelegate = new ToxDelegates.CallbackUserStatusDelegate((IntPtr t, int friendnumber, ToxUserStatus status, IntPtr userdata) =>
            {
                if (OnUserStatus != null)
                    OnUserStatus(friendnumber, status);
            }));

            ToxFunctions.CallbackTypingChange(tox, typingchangedelegate = new ToxDelegates.CallbackTypingChangeDelegate((IntPtr t, int friendnumber, byte typing, IntPtr userdata) =>
            {
                bool is_typing = typing == 0 ? false : true;

                if (OnTypingChange != null)
                    OnTypingChange(friendnumber, is_typing);
            }));

            ToxFunctions.CallbackGroupAction(tox, groupactiondelegate = new ToxDelegates.CallbackGroupActionDelegate((IntPtr t, int groupnumber, int friendgroupnumber, byte[] action, ushort length, IntPtr userdata) =>
            {
                if (OnGroupAction != null)
                    OnGroupAction(groupnumber, friendgroupnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(action)));
            }));

            ToxFunctions.CallbackGroupMessage(tox, groupmessagedelegate = new ToxDelegates.CallbackGroupMessageDelegate((IntPtr t, int groupnumber, int friendgroupnumber, byte[] message, ushort length, IntPtr userdata) =>
            {
                if (OnGroupMessage != null)
                    OnGroupMessage(groupnumber, friendgroupnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(message)));
            }));

            ToxFunctions.CallbackGroupInvite(tox, groupinvitedelegate = new ToxDelegates.CallbackGroupInviteDelegate((IntPtr t, int friendnumber, byte[] group_public_key, IntPtr userdata) =>
            {
                if (OnGroupInvite != null)
                    OnGroupInvite(friendnumber, ToxTools.HexBinToString(group_public_key));
            }));

            ToxFunctions.CallbackGroupNamelistChange(tox, groupnamelistchangedelegate = new ToxDelegates.CallbackGroupNamelistChangeDelegate((IntPtr t, int groupnumber, int peernumber, ToxChatChange change, IntPtr userdata) =>
            {
                if (OnGroupNamelistChange != null)
                    OnGroupNamelistChange(groupnumber, peernumber, change);
            }));
        }
    }
}
