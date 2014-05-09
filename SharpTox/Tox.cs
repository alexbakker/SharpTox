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

    public delegate void OnFileControlDelegate(int friendnumber, int receive_send, int filenumber, int control_type, byte[] data);
    public delegate void OnFileDataDelegate(int friendnumber, int filenumber, byte[] data);
    public delegate void OnFileSendRequestDelegate(int friendnumber, int filenumber, ulong filesiz, string filename);
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

        public event OnFileControlDelegate OnFileControl;
        public event OnFileDataDelegate OnFileData;
        public event OnFileSendRequestDelegate OnFileSendRequest;

        public delegate object InvokeDelegate(Delegate method, params object[] p);
        public InvokeDelegate Invoker;

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

        private ToxDelegates.CallbackFileControlDelegate filecontroldelegate;
        private ToxDelegates.CallbackFileDataDelegate filedatadelegate;
        private ToxDelegates.CallbackFileSendRequestDelegate filesendrequestdelegate;
        #endregion

        private IntPtr tox;
        private Thread thread;

        private object obj;

        public bool Ipv6Enabled { get; private set; }

        public Tox(bool ipv6enabled)
        {
            tox = ToxFunctions.New(ipv6enabled);
            Ipv6Enabled = ipv6enabled;

            obj = new object();
            Invoker = new InvokeDelegate(dummyinvoker);

            callbacks();
        }

        private object dummyinvoker(Delegate method, params object[] p)
        {
            return method.DynamicInvoke(p);
        }

        public bool IsConnected()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.IsConnected(tox);
            }
        }

        public int NewFileSender(int friendnumber, ulong filesize, string filename)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.NewFileSender(tox, friendnumber, filesize, filename);
            }
        }

        public bool FileSendControl(int friendnumber, int send_receive, int filenumber, int message_id, byte[] data)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.FileSendControl(tox, friendnumber, (byte)send_receive, (byte)filenumber, (byte)message_id, data, (ushort)data.Length);
            }
        }

        public bool FileSendData(int friendnumber, int filenumber, byte[] data)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.FileSendData(tox, friendnumber, filenumber, data);
            }
        }

        public int FileDataSize(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.FileDataSize(tox, friendnumber);
            }
        }

        public int FileDataRemaining(int friendnumber, int filenumber, int send_receive)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.FileDataRemaining(tox, friendnumber, filenumber, send_receive);
            }
        }

        public bool Load(string filename)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

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
        }

        public string[] GetGroupNames(int groupnumber)
        {
            throw new NotImplementedException();

            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GroupGetNames(tox, groupnumber);
            }
        }

        public bool LoadEncrypted(string filename, string key)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

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
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                int result = ToxFunctions.AddFriend(tox, id, message);

                if (result < 0)
                    throw new Exception("Could not add friend: " + (ToxAFError)result);
                else
                    return result;
            }
        }
        public int AddFriend(string id)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                int result = ToxFunctions.AddFriend(tox, id, "No message.");

                if (result < 0)
                    throw new Exception("Could not add friend: " + (ToxAFError)result);
                else
                    return result;
            }
        }

        public int AddFriendNoRequest(string id)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                int result = ToxFunctions.AddFriendNoRequest(tox, id);

                if (result < 0)
                    throw new Exception("Could not add friend: " + (ToxAFError)result);
                else
                    return result;
            }
        }

        public bool BootstrapFromNode(ToxNode node)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.BootstrapFromAddress(tox, node.Address, node.Ipv6Enabled, Convert.ToUInt16(node.Port), node.PublicKey);
            }
        }

        public bool FriendExists(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.FriendExists(tox, friendnumber);
            }
        }

        public int GetFriendlistCount()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return (int)ToxFunctions.CountFriendlist(tox);
            }
        }

        public int[] GetFriendlist()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetFriendlist(tox);
            }
        }

        public string GetName(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetName(tox, friendnumber);
            }
        }

        public string GetSelfName()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxTools.RemoveNull(ToxFunctions.GetSelfName(tox));
            }
        }

        public DateTime GetLastOnline(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxTools.EpochToDateTime((long)ToxFunctions.GetLastOnline(tox, friendnumber));
            }
        }

        public string GetAddress()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxTools.HexBinToString(ToxFunctions.GetAddress(tox));
            }
        }

        public bool GetIsTyping(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetIsTyping(tox, friendnumber);
            }
        }

        public int GetFriendNumber(string id)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetFriendNumber(tox, id);
            }
        }

        public string GetStatusMessage(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetStatusMessage(tox, friendnumber);
            }
        }

        public string GetSelfStatusMessage()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetSelfStatusMessage(tox);
            }
        }

        public int GetOnlineFriendsCount()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return (int)ToxFunctions.GetNumOnlineFriends(tox);
            }
        }

        public int GetFriendConnectionStatus(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetFriendConnectionStatus(tox, friendnumber);
            }
        }

        public string GetClientID(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetClientID(tox, friendnumber);
            }
        }

        public ToxUserStatus GetUserStatus(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetUserStatus(tox, friendnumber);
            }
        }

        public ToxUserStatus GetSelfUserStatus()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetSelfUserStatus(tox);
            }
        }

        public bool SetName(string name)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.SetName(tox, name);
            }
        }

        public bool SetUserStatus(ToxUserStatus status)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.SetUserStatus(tox, status);
            }
        }

        public bool SetStatusMessage(string message)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.SetStatusMessage(tox, message);
            }
        }

        public bool SetUserIsTyping(int friendnumber, bool is_typing)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.SetUserIsTyping(tox, friendnumber, is_typing);
            }
        }

        public int SendMessage(int friendnumber, string message)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.SendMessage(tox, friendnumber, message);
            }
        }

        public int SendAction(int friendnumber, string action)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.SendAction(tox, friendnumber, action);
            }
        }

        public bool Save(string filename)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.Save(tox, filename);
            }
        }

        public bool SaveEncrypted(string filename, string key)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.SaveEncrypted(tox, filename, key);
            }
        }

        public void Kill()
        {
            lock (obj)
            {
                thread.Abort();
                thread.Join();

                if (tox == IntPtr.Zero)
                    throw null;

                ToxFunctions.Kill(tox);
            }
        }

        public bool DeleteFriend(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.DeleteFriend(tox, friendnumber);
            }
        }

        public int JoinGroup(int friendnumber, string group_public_key)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.JoinGroupchat(tox, friendnumber, group_public_key);
            }
        }

        public string GetGroupMemberName(int groupnumber, int peernumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GroupPeername(tox, groupnumber, peernumber);
            }
        }

        public int GetGroupMemberCount(int groupnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GroupNumberPeers(tox, groupnumber);
            }
        }

        public int DeleteGroupChat(int groupnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.DeleteGroupchat(tox, groupnumber);
            }
        }

        public bool InviteFriend(int friendnumber, int groupnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.InviteFriend(tox, friendnumber, groupnumber);
            }
        }

        public bool SendGroupMessage(int groupnumber, string message)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GroupMessageSend(tox, groupnumber, message);
            }
        }

        public bool SendGroupAction(int groupnumber, string action)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GroupActionSend(tox, groupnumber, action);
            }
        }

        public int NewGroup()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.AddGroupchat(tox);
            }
        }

        public uint GetNospam()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetNospam(tox);
            }
        }

        public void SetNospam(uint nospam)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                ToxFunctions.SetNospam(tox, nospam);
            }
        }

        private void callbacks()
        {
            ToxFunctions.CallbackFriendRequest(tox, friendrequestdelegate = new ToxDelegates.CallbackFriendRequestDelegate((IntPtr t, byte[] id, byte[] message, ushort length, IntPtr userdata) =>
            {
                if (OnFriendRequest != null)
                    Invoker(OnFriendRequest, ToxTools.RemoveNull(ToxTools.HexBinToString(id)), Encoding.UTF8.GetString(message));
            }));

            ToxFunctions.CallbackConnectionStatus(tox, connectionstatusdelegate = new ToxDelegates.CallbackConnectionStatusDelegate((IntPtr t, int friendnumber, byte status, IntPtr userdata) =>
            {
                if (OnConnectionStatusChanged != null)
                    Invoker(OnConnectionStatusChanged, friendnumber, status);
            }));

            ToxFunctions.CallbackFriendMessage(tox, friendmessagedelegate = new ToxDelegates.CallbackFriendMessageDelegate((IntPtr t, int friendnumber, byte[] message, ushort length, IntPtr userdata) =>
            {
                if (OnFriendMessage != null)
                    Invoker(OnFriendMessage, friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(message)));
            }));

            ToxFunctions.CallbackFriendAction(tox, friendactiondelegate = new ToxDelegates.CallbackFriendActionDelegate((IntPtr t, int friendnumber, byte[] action, ushort length, IntPtr userdata) =>
            {
                if (OnFriendAction != null)
                    Invoker(OnFriendAction, friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(action)));
            }));

            ToxFunctions.CallbackNameChange(tox, namechangedelegate = new ToxDelegates.CallbackNameChangeDelegate((IntPtr t, int friendnumber, byte[] newname, ushort length, IntPtr userdata) =>
            {
                if (OnNameChange != null)
                    Invoker(OnNameChange, friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(newname)));
            }));

            ToxFunctions.CallbackStatusMessage(tox, statusmessagedelegate = new ToxDelegates.CallbackStatusMessageDelegate((IntPtr t, int friendnumber, byte[] newstatus, ushort length, IntPtr userdata) =>
            {
                if (OnStatusMessage != null)
                    Invoker(OnStatusMessage, friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(newstatus)));
            }));

            ToxFunctions.CallbackUserStatus(tox, userstatusdelegate = new ToxDelegates.CallbackUserStatusDelegate((IntPtr t, int friendnumber, ToxUserStatus status, IntPtr userdata) =>
            {
                if (OnUserStatus != null)
                    Invoker(OnUserStatus, friendnumber, status);
            }));

            ToxFunctions.CallbackTypingChange(tox, typingchangedelegate = new ToxDelegates.CallbackTypingChangeDelegate((IntPtr t, int friendnumber, byte typing, IntPtr userdata) =>
            {
                bool is_typing = typing == 0 ? false : true;

                if (OnTypingChange != null)
                    Invoker(OnTypingChange, friendnumber, is_typing);
            }));

            ToxFunctions.CallbackGroupAction(tox, groupactiondelegate = new ToxDelegates.CallbackGroupActionDelegate((IntPtr t, int groupnumber, int friendgroupnumber, byte[] action, ushort length, IntPtr userdata) =>
            {
                if (OnGroupAction != null)
                    Invoker(OnGroupAction, groupnumber, friendgroupnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(action)));
            }));

            ToxFunctions.CallbackGroupMessage(tox, groupmessagedelegate = new ToxDelegates.CallbackGroupMessageDelegate((IntPtr t, int groupnumber, int friendgroupnumber, byte[] message, ushort length, IntPtr userdata) =>
            {
                if (OnGroupMessage != null)
                    Invoker(OnGroupMessage, groupnumber, friendgroupnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(message)));
            }));

            ToxFunctions.CallbackGroupInvite(tox, groupinvitedelegate = new ToxDelegates.CallbackGroupInviteDelegate((IntPtr t, int friendnumber, byte[] group_public_key, IntPtr userdata) =>
            {
                if (OnGroupInvite != null)
                    Invoker(OnGroupInvite, friendnumber, ToxTools.HexBinToString(group_public_key));
            }));

            ToxFunctions.CallbackGroupNamelistChange(tox, groupnamelistchangedelegate = new ToxDelegates.CallbackGroupNamelistChangeDelegate((IntPtr t, int groupnumber, int peernumber, ToxChatChange change, IntPtr userdata) =>
            {
                if (OnGroupNamelistChange != null)
                    Invoker(OnGroupNamelistChange, groupnumber, peernumber, change);
            }));

            ToxFunctions.CallbackFileControl(tox, filecontroldelegate = new ToxDelegates.CallbackFileControlDelegate((IntPtr t, int friendnumber, byte receive_send, byte filenumber, byte control_type, byte[] data, ushort length, IntPtr userdata) =>
            {
                if (OnFileControl != null)
                    Invoker(OnFileControl, friendnumber, receive_send, filenumber, control_type, data);
            }));

            ToxFunctions.CallbackFileData(tox, filedatadelegate = new ToxDelegates.CallbackFileDataDelegate((IntPtr t, int friendnumber, byte filenumber, byte[] data, ushort length, IntPtr userdata) =>
            {
                if (OnFileData != null)
                    Invoker(OnFileData, friendnumber, filenumber, data);
            }));

            ToxFunctions.CallbackFileSendRequest(tox, filesendrequestdelegate = new ToxDelegates.CallbackFileSendRequestDelegate((IntPtr t, int friendnumber, byte filenumber, ulong filesize, byte[] filename, ushort filename_length, IntPtr userdata) =>
            {
                if (OnFileSendRequest != null)
                    Invoker(OnFileSendRequest, friendnumber, filenumber, filesize, ToxTools.RemoveNull(Encoding.UTF8.GetString(filename)));
            }));
        }
    }
}
