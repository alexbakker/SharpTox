#pragma warning disable 1591

using System;
using System.IO;
using System.Text;
using System.Threading;

namespace SharpTox.Core
{
    #region Event Delegates
    public delegate void OnFriendRequestDelegate(string id, string message);
    public delegate void OnConnectionStatusDelegate(int friendnumber, int status);
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
    public delegate void OnFileSendRequestDelegate(int friendnumber, int filenumber, ulong filesize, string filename);
    public delegate void OnReadReceiptDelegate(int friendnumber, uint receipt);
    #endregion

    public class Tox
    {
        /// <summary>
        /// Occurs when a friend request is received.
        /// </summary>
        public event OnFriendRequestDelegate OnFriendRequest;

        /// <summary>
        /// Occurs when the connection status of a friend has changed.
        /// </summary>
        public event OnConnectionStatusDelegate OnConnectionStatusChanged;

        /// <summary>
        /// Occurs when a message is received from a friend.
        /// </summary>
        public event OnFriendMessageDelegate OnFriendMessage;

        /// <summary>
        /// Occurs when an action is received from a friend.
        /// </summary>
        public event OnFriendActionDelegate OnFriendAction;

        /// <summary>
        /// Occurs when a friend has changed his/her name.
        /// </summary>
        public event OnNameChangeDelegate OnNameChange;

        /// <summary>
        /// Occurs when a friend has changed their status message.
        /// </summary>
        public event OnStatusMessageDelegate OnStatusMessage;

        /// <summary>
        /// Occurs when a friend has changed their user status.
        /// </summary>
        public event OnUserStatusDelegate OnUserStatus;

        /// <summary>
        /// Occurs when a friend's typing status has changed.
        /// </summary>
        public event OnTypingChangeDelegate OnTypingChange;

        /// <summary>
        /// Occurs when an action is received from a group.
        /// </summary>
        public event OnGroupActionDelegate OnGroupAction;

        /// <summary>
        /// Occurs when a message is received from a group.
        /// </summary>
        public event OnGroupMessageDelegate OnGroupMessage;

        /// <summary>
        /// Occurs when a friend has sent an invite to a group.
        /// </summary>
        public event OnGroupInviteDelegate OnGroupInvite;

        /// <summary>
        /// Occurs when the name list of a group has changed.
        /// </summary>
        public event OnGroupNamelistChangeDelegate OnGroupNamelistChange;

        /// <summary>
        /// Occurs when a file control request is received.
        /// </summary>
        public event OnFileControlDelegate OnFileControl;

        /// <summary>
        /// Occurs when file data is received.
        /// </summary>
        public event OnFileDataDelegate OnFileData;

        /// <summary>
        /// Occurs when a file send request is received.
        /// </summary>
        public event OnFileSendRequestDelegate OnFileSendRequest;

        /// <summary>
        /// Occurs when a read receipt is received.
        /// </summary>
        public event OnReadReceiptDelegate OnReadReceipt;

        public delegate object InvokeDelegate(Delegate method, params object[] p);

        /// <summary>
        /// The invoke delegate to use when raising events.
        /// </summary>
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

        private ToxDelegates.CallbackReadReceiptDelegate readreceiptdelegate;
        #endregion

        private IntPtr tox;
        private Thread thread;

        private object obj;

        /// <summary>
        /// Setting this to false will make sure that resolving sticks strictly to IPv4 addresses.
        /// </summary>
        public bool Ipv6Enabled { get; private set; }

        /// <summary>
        /// Initializes a new instance of tox.
        /// </summary>
        /// <param name="ipv6enabled"></param>
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

        /// <summary>
        /// Check whether we are connected to the DHT.
        /// </summary>
        /// <returns>true if we are and false if we aren't.</returns>
        public bool IsConnected()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.IsConnected(tox);
            }
        }

        /// <summary>
        /// Sends a file send request to the given friendnumber.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="filesize"></param>
        /// <param name="filename">Maximum filename length is 255 bytes.</param>
        /// <returns>the filenumber on success and -1 on failure.</returns>
        public int NewFileSender(int friendnumber, ulong filesize, string filename)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.NewFileSender(tox, friendnumber, filesize, filename);
            }
        }

        /// <summary>
        /// Sends a file control request.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="send_receive">0 if we're sending and 1 if we're receiving.</param>
        /// <param name="filenumber"></param>
        /// <param name="message_id"></param>
        /// <param name="data"></param>
        /// <returns>true on success and false on failure.</returns>
        public bool FileSendControl(int friendnumber, int send_receive, int filenumber, ToxFileControl message_id, byte[] data)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.FileSendControl(tox, friendnumber, (byte)send_receive, (byte)filenumber, (byte)message_id, data, (ushort)data.Length);
            }
        }

        /// <summary>
        /// Sends file data.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="filenumber"></param>
        /// <param name="data"></param>
        /// <returns>true on success and false on failure.</returns>
        public bool FileSendData(int friendnumber, int filenumber, byte[] data)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.FileSendData(tox, friendnumber, filenumber, data);
            }
        }

        /// <summary>
        /// Retrieves the recommended/maximum size of the filedata to send with FileSendData.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public int FileDataSize(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.FileDataSize(tox, friendnumber);
            }
        }

        /// <summary>
        /// Retrieves the number of bytes left to be sent/received.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="filenumber"></param>
        /// <param name="send_receive">0 if we're sending and 1 if we're receiving.</param>
        /// <returns></returns>
        public ulong FileDataRemaining(int friendnumber, int filenumber, int send_receive)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.FileDataRemaining(tox, friendnumber, filenumber, send_receive);
            }
        }

        /// <summary>
        /// Loads the tox data file from a location specified by filename.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Retrieves an array of group member names. Not implemented yet.
        /// </summary>
        /// <param name="groupnumber"></param>
        /// <returns></returns>
        public string[] GetGroupNames(int groupnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GroupGetNames(tox, groupnumber);
            }
        }

        /// <summary>
        /// Starts the main tox_do loop.
        /// </summary>
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

        /// <summary>
        /// Adds a friend.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="message"></param>
        /// <returns>friendnumber</returns>
        public int AddFriend(string id, string message)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                int result = ToxFunctions.AddFriend(tox, id, message);

                if (result < 0)
                    throw new ToxAFException((ToxAFError)result);
                else
                    return result;
            }
        }

        /// <summary>
        /// Adds a friend with a default message.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>friendnumber</returns>
        public int AddFriend(string id)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                int result = ToxFunctions.AddFriend(tox, id, "No message.");

                if (result < 0)
                    throw new ToxAFException((ToxAFError)result);
                else
                    return result;
            }
        }

        /// <summary>
        /// Adds a friend without sending a request.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>friendnumber</returns>
        public int AddFriendNoRequest(string id)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                int result = ToxFunctions.AddFriendNoRequest(tox, id);

                if (result < 0)
                    throw new ToxAFException((ToxAFError)result);
                else
                    return result;
            }
        }

        /// <summary>
        /// Bootstraps the tox client with a ToxNode.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool BootstrapFromNode(ToxNode node)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.BootstrapFromAddress(tox, node.Address, node.Ipv6Enabled, Convert.ToUInt16(node.Port), node.PublicKey);
            }
        }

        /// <summary>
        /// Checks if there exists a friend with given friendnumber.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public bool FriendExists(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.FriendExists(tox, friendnumber);
            }
        }

        /// <summary>
        /// Retrieves the number of friends in this tox instance.
        /// </summary>
        /// <returns></returns>
        public int GetFriendlistCount()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return (int)ToxFunctions.CountFriendlist(tox);
            }
        }

        /// <summary>
        /// Retrieves an array of friendnumbers of this tox instance.
        /// </summary>
        /// <returns></returns>
        public int[] GetFriendlist()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetFriendlist(tox);
            }
        }

        /// <summary>
        /// Retrieves the name of a friendnumber.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public string GetName(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxTools.RemoveNull(ToxFunctions.GetName(tox, friendnumber));
            }
        }

        /// <summary>
        /// Retrieves the nickname of this tox instance.
        /// </summary>
        /// <returns></returns>
        public string GetSelfName()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxTools.RemoveNull(ToxFunctions.GetSelfName(tox));
            }
        }

        /// <summary>
        /// Retrieves a DateTime object of the last time friendnumber was seen online.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public DateTime GetLastOnline(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxTools.EpochToDateTime((long)ToxFunctions.GetLastOnline(tox, friendnumber));
            }
        }

        /// <summary>
        /// Retrieves the string of a 32 byte long address to share with others.
        /// </summary>
        /// <returns></returns>
        public string GetAddress()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxTools.HexBinToString(ToxFunctions.GetAddress(tox));
            }
        }

        /// <summary>
        /// Retrieves the typing status of a friend.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public bool GetIsTyping(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetIsTyping(tox, friendnumber);
            }
        }

        /// <summary>
        /// Retrieves the friendnumber associated to the specified public address/id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetFriendNumber(string id)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetFriendNumber(tox, id);
            }
        }

        /// <summary>
        /// Retrieves the status message of a friend.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public string GetStatusMessage(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxTools.RemoveNull(ToxFunctions.GetStatusMessage(tox, friendnumber));
            }
        }

        /// <summary>
        /// Retrieves the status message of this tox instance.
        /// </summary>
        /// <returns></returns>
        public string GetSelfStatusMessage()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxTools.RemoveNull(ToxFunctions.GetSelfStatusMessage(tox));
            }
        }

        /// <summary>
        /// Retrieves the amount of friends who are currently online.
        /// </summary>
        /// <returns></returns>
        public int GetOnlineFriendsCount()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return (int)ToxFunctions.GetNumOnlineFriends(tox);
            }
        }

        /// <summary>
        /// Retrieves a friend's connection status.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public int GetFriendConnectionStatus(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetFriendConnectionStatus(tox, friendnumber);
            }
        }

        /// <summary>
        /// Retrieves a friend's public id/address.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public string GetClientID(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetClientID(tox, friendnumber);
            }
        }

        /// <summary>
        /// Retrieves a friend's current user status.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public ToxUserStatus GetUserStatus(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetUserStatus(tox, friendnumber);
            }
        }

        /// <summary>
        /// Retrieves the current user status of this tox instance.
        /// </summary>
        /// <returns></returns>
        public ToxUserStatus GetSelfUserStatus()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetSelfUserStatus(tox);
            }
        }

        /// <summary>
        /// Sets the name of this tox instance.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool SetName(string name)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.SetName(tox, name);
            }
        }

        /// <summary>
        /// Sets the user status of this tox instance.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool SetUserStatus(ToxUserStatus status)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.SetUserStatus(tox, status);
            }
        }

        /// <summary>
        /// Sets the status message of this tox instance.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SetStatusMessage(string message)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.SetStatusMessage(tox, message);
            }
        }

        /// <summary>
        /// Sets the typing status of this tox instance.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="is_typing"></param>
        /// <returns></returns>
        public bool SetUserIsTyping(int friendnumber, bool is_typing)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.SetUserIsTyping(tox, friendnumber, is_typing);
            }
        }

        /// <summary>
        /// Send a message to a friend.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public int SendMessage(int friendnumber, string message)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.SendMessage(tox, friendnumber, message);
            }
        }

        /// <summary>
        /// Send a message to a friend. The given id will be used as the message id.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="id"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public int SendMessageWithID(int friendnumber, int id, string message)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.SendMessageWithID(tox, friendnumber, id, message);
            }
        }

        /// <summary>
        /// Sends an action to a friend.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public int SendAction(int friendnumber, string action)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.SendAction(tox, friendnumber, action);
            }
        }

        /// <summary>
        /// Send an action to a friend. The given id will be used as the message id.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="id"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public int SendActionWithID(int friendnumber, int id, string message)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.SendActionWithID(tox, friendnumber, id, message);
            }
        }

        /// <summary>
        /// Saves the data of this tox instance at the given file location.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool Save(string filename)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                try
                {
                    byte[] bytes = new byte[ToxFunctions.Size(tox)];
                    ToxFunctions.Save(tox, bytes);

                    FileStream stream = new FileStream(filename, FileMode.Create);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Close();

                    return true;
                }
                catch { return false; }
            }
        }

        /// <summary>
        /// Ends the tox_do loop and kills this tox instance.
        /// </summary>
        public void Kill()
        {
            lock (obj)
            {
                if (thread != null)
                {
                    thread.Abort();
                    thread.Join();
                }

                if (tox == IntPtr.Zero)
                    throw null;

                ToxFunctions.Kill(tox);
            }
        }

        /// <summary>
        /// Deletes a friend.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public bool DeleteFriend(int friendnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.DeleteFriend(tox, friendnumber);
            }
        }

        /// <summary>
        /// Joins a group with the given public key of the group.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="group_public_key"></param>
        /// <returns></returns>
        public int JoinGroup(int friendnumber, string group_public_key)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.JoinGroupchat(tox, friendnumber, group_public_key);
            }
        }

        /// <summary>
        /// Retrieves the name of a group member.
        /// </summary>
        /// <param name="groupnumber"></param>
        /// <param name="peernumber"></param>
        /// <returns></returns>
        public string GetGroupMemberName(int groupnumber, int peernumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxTools.RemoveNull(ToxFunctions.GroupPeername(tox, groupnumber, peernumber));
            }
        }

        /// <summary>
        /// Retrieves the number of group members in a group chat.
        /// </summary>
        /// <param name="groupnumber"></param>
        /// <returns></returns>
        public int GetGroupMemberCount(int groupnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GroupNumberPeers(tox, groupnumber);
            }
        }

        /// <summary>
        /// Deletes a group chat.
        /// </summary>
        /// <param name="groupnumber"></param>
        /// <returns></returns>
        public bool DeleteGroupChat(int groupnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.DeleteGroupchat(tox, groupnumber);
            }
        }

        /// <summary>
        /// Invites a friend to a group chat.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="groupnumber"></param>
        /// <returns></returns>
        public bool InviteFriend(int friendnumber, int groupnumber)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.InviteFriend(tox, friendnumber, groupnumber);
            }
        }

        /// <summary>
        /// Sends a message to a group.
        /// </summary>
        /// <param name="groupnumber"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendGroupMessage(int groupnumber, string message)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GroupMessageSend(tox, groupnumber, message);
            }
        }

        /// <summary>
        /// Sends an action to a group.
        /// </summary>
        /// <param name="groupnumber"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool SendGroupAction(int groupnumber, string action)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GroupActionSend(tox, groupnumber, action);
            }
        }

        /// <summary>
        /// Creates a new group and retrieves the group number.
        /// </summary>
        /// <returns></returns>
        public int NewGroup()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.AddGroupchat(tox);
            }
        }

        /// <summary>
        /// Retrieves the nospam value.
        /// </summary>
        /// <returns></returns>
        public uint GetNospam()
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                return ToxFunctions.GetNospam(tox);
            }
        }

        /// <summary>
        /// Sets the nospam value.
        /// </summary>
        /// <param name="nospam"></param>
        public void SetNospam(uint nospam)
        {
            lock (obj)
            {
                if (tox == IntPtr.Zero)
                    throw null;

                ToxFunctions.SetNospam(tox, nospam);
            }
        }

        /// <summary>
        /// Retrieves the pointer of this tox instance.
        /// </summary>
        /// <returns></returns>
        public IntPtr GetPointer()
        {
            return tox;
        }

        /// <summary>
        /// Whether to send read receipts for the specified friendnumber or not.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="send_receipts"></param>
        public void SetSendsReceipts(int friendnumber, bool send_receipts)
        {
            ToxFunctions.SetSendsReceipts(tox, friendnumber, send_receipts);
        }

        private void callbacks()
        {
            ToxFunctions.CallbackFriendRequest(tox, friendrequestdelegate = new ToxDelegates.CallbackFriendRequestDelegate((IntPtr t, byte[] id, byte[] message, ushort length, IntPtr userdata) =>
            {
                if (OnFriendRequest != null)
                    Invoker(OnFriendRequest, ToxTools.RemoveNull(ToxTools.HexBinToString(id)), Encoding.UTF8.GetString(message, 0, length));
            }));

            ToxFunctions.CallbackConnectionStatus(tox, connectionstatusdelegate = new ToxDelegates.CallbackConnectionStatusDelegate((IntPtr t, int friendnumber, byte status, IntPtr userdata) =>
            {
                if (OnConnectionStatusChanged != null)
                    Invoker(OnConnectionStatusChanged, friendnumber, (int)status);
            }));

            ToxFunctions.CallbackFriendMessage(tox, friendmessagedelegate = new ToxDelegates.CallbackFriendMessageDelegate((IntPtr t, int friendnumber, byte[] message, ushort length, IntPtr userdata) =>
            {
                if (OnFriendMessage != null)
                    Invoker(OnFriendMessage, friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(message, 0, length)));
            }));

            ToxFunctions.CallbackFriendAction(tox, friendactiondelegate = new ToxDelegates.CallbackFriendActionDelegate((IntPtr t, int friendnumber, byte[] action, ushort length, IntPtr userdata) =>
            {
                if (OnFriendAction != null)
                    Invoker(OnFriendAction, friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(action, 0, length)));
            }));

            ToxFunctions.CallbackNameChange(tox, namechangedelegate = new ToxDelegates.CallbackNameChangeDelegate((IntPtr t, int friendnumber, byte[] newname, ushort length, IntPtr userdata) =>
            {
                if (OnNameChange != null)
                    Invoker(OnNameChange, friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(newname, 0, length)));
            }));

            ToxFunctions.CallbackStatusMessage(tox, statusmessagedelegate = new ToxDelegates.CallbackStatusMessageDelegate((IntPtr t, int friendnumber, byte[] newstatus, ushort length, IntPtr userdata) =>
            {
                if (OnStatusMessage != null)
                    Invoker(OnStatusMessage, friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(newstatus, 0, length)));
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
                    Invoker(OnGroupAction, groupnumber, friendgroupnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(action, 0, length)));
            }));

            ToxFunctions.CallbackGroupMessage(tox, groupmessagedelegate = new ToxDelegates.CallbackGroupMessageDelegate((IntPtr t, int groupnumber, int friendgroupnumber, byte[] message, ushort length, IntPtr userdata) =>
            {
                if (OnGroupMessage != null)
                    Invoker(OnGroupMessage, groupnumber, friendgroupnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(message, 0, length)));
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
                    Invoker(OnFileSendRequest, friendnumber, filenumber, filesize, ToxTools.RemoveNull(Encoding.UTF8.GetString(filename, 0, filename_length)));
            }));

            ToxFunctions.CallbackReadReceipt(tox, readreceiptdelegate = new ToxDelegates.CallbackReadReceiptDelegate((IntPtr t, int friendnumber, uint receipt, IntPtr userdata) =>
            {
                if (OnReadReceipt != null)
                    Invoker(OnReadReceipt, friendnumber, receipt);
            }));
        }
    }
}

#pragma warning restore 1591