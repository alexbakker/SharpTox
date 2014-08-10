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

    /// <summary>
    /// Represents an instance of tox.
    /// </summary>
    public class Tox : IDisposable
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

        private ToxHandle tox;
        private Thread thread;

        private bool disposed = false;

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

            if (tox == null || tox.IsInvalid)
                throw new Exception("Could not create a new instance of toxav.");

            Ipv6Enabled = ipv6enabled;
            Invoker = new InvokeDelegate(dummyinvoker);

            callbacks();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //dispose pattern as described on msdn for a class that uses a safe handle
        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (thread != null)
                {
                    thread.Abort();
                    thread.Join();
                }
            }

            if (!tox.IsInvalid && !tox.IsClosed && tox != null)
                tox.Dispose();

            disposed = true;
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
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.IsConnected(tox);
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
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.NewFileSender(tox, friendnumber, filesize, filename);
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
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.FileSendControl(tox, friendnumber, (byte)send_receive, (byte)filenumber, (byte)message_id, data, (ushort)data.Length);
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
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.FileSendData(tox, friendnumber, filenumber, data);
        }

        /// <summary>
        /// Retrieves the recommended/maximum size of the filedata to send with FileSendData.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public int FileDataSize(int friendnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.FileDataSize(tox, friendnumber);
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
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.FileDataRemaining(tox, friendnumber, filenumber, send_receive);
        }

        /// <summary>
        /// Loads the tox data file from a location specified by filename.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool Load(string filename)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

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

        /// <summary>
        /// Retrieves an array of group member names. Not implemented yet.
        /// </summary>
        /// <param name="groupnumber"></param>
        /// <returns></returns>
        public string[] GetGroupNames(int groupnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GroupGetNames(tox, groupnumber);
        }

        /// <summary>
        /// Starts the main tox_do loop.
        /// </summary>
        public void Start()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            thread = new Thread(loop);
            thread.Start();
        }

        private void loop()
        {
            while (true)
            {
                ToxFunctions.Do(tox);
                Thread.Sleep((int)ToxFunctions.DoInterval(tox));
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
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int result = ToxFunctions.AddFriend(tox, id, message);

            if (result < 0)
                throw new ToxAFException((ToxAFError)result);
            else
                return result;
        }

        /// <summary>
        /// Adds a friend with a default message.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>friendnumber</returns>
        public int AddFriend(string id)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int result = ToxFunctions.AddFriend(tox, id, "No message.");

            if (result < 0)
                throw new ToxAFException((ToxAFError)result);
            else
                return result;
        }

        /// <summary>
        /// Adds a friend without sending a request.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>friendnumber</returns>
        public int AddFriendNoRequest(string id)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int result = ToxFunctions.AddFriendNoRequest(tox, id);

            if (result < 0)
                throw new ToxAFException((ToxAFError)result);
            else
                return result;
        }

        /// <summary>
        /// Bootstraps the tox client with a ToxNode.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool BootstrapFromNode(ToxNode node)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.BootstrapFromAddress(tox, node.Address, node.Ipv6Enabled, Convert.ToUInt16(node.Port), node.PublicKey.GetString());
        }

        /// <summary>
        /// Checks if there exists a friend with given friendnumber.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public bool FriendExists(int friendnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.FriendExists(tox, friendnumber);
        }

        /// <summary>
        /// Retrieves the number of friends in this tox instance.
        /// </summary>
        /// <returns></returns>
        public int GetFriendlistCount()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return (int)ToxFunctions.CountFriendlist(tox);
        }

        /// <summary>
        /// Retrieves an array of friendnumbers of this tox instance.
        /// </summary>
        /// <returns></returns>
        public int[] GetFriendlist()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GetFriendlist(tox);
        }

        /// <summary>
        /// Retrieves the name of a friendnumber.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public string GetName(int friendnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxTools.RemoveNull(ToxFunctions.GetName(tox, friendnumber));
        }

        /// <summary>
        /// Retrieves the nickname of this tox instance.
        /// </summary>
        /// <returns></returns>
        public string GetSelfName()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxTools.RemoveNull(ToxFunctions.GetSelfName(tox));
        }

        /// <summary>
        /// Retrieves a DateTime object of the last time friendnumber was seen online.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public DateTime GetLastOnline(int friendnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxTools.EpochToDateTime((long)ToxFunctions.GetLastOnline(tox, friendnumber));
        }

        /// <summary>
        /// Retrieves the string of a 32 byte long address to share with others.
        /// </summary>
        /// <returns></returns>
        public string GetAddress()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxTools.HexBinToString(ToxFunctions.GetAddress(tox));
        }

        /// <summary>
        /// Retrieves the typing status of a friend.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public bool GetIsTyping(int friendnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GetIsTyping(tox, friendnumber);
        }

        /// <summary>
        /// Retrieves the friendnumber associated to the specified public address/id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetFriendNumber(string id)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GetFriendNumber(tox, id);
        }

        /// <summary>
        /// Retrieves the status message of a friend.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public string GetStatusMessage(int friendnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxTools.RemoveNull(ToxFunctions.GetStatusMessage(tox, friendnumber));
        }

        /// <summary>
        /// Retrieves the status message of this tox instance.
        /// </summary>
        /// <returns></returns>
        public string GetSelfStatusMessage()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxTools.RemoveNull(ToxFunctions.GetSelfStatusMessage(tox));
        }

        /// <summary>
        /// Retrieves the amount of friends who are currently online.
        /// </summary>
        /// <returns></returns>
        public int GetOnlineFriendsCount()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return (int)ToxFunctions.GetNumOnlineFriends(tox);
        }

        /// <summary>
        /// Retrieves a friend's connection status.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public int GetFriendConnectionStatus(int friendnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GetFriendConnectionStatus(tox, friendnumber);
        }

        /// <summary>
        /// Retrieves a friend's public id/address.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public string GetClientID(int friendnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GetClientID(tox, friendnumber);
        }

        /// <summary>
        /// Retrieves a friend's current user status.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public ToxUserStatus GetUserStatus(int friendnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GetUserStatus(tox, friendnumber);
        }

        /// <summary>
        /// Retrieves the current user status of this tox instance.
        /// </summary>
        /// <returns></returns>
        public ToxUserStatus GetSelfUserStatus()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GetSelfUserStatus(tox);
        }

        /// <summary>
        /// Sets the name of this tox instance.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool SetName(string name)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.SetName(tox, name);
        }

        /// <summary>
        /// Sets the user status of this tox instance.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool SetUserStatus(ToxUserStatus status)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.SetUserStatus(tox, status);
        }

        /// <summary>
        /// Sets the status message of this tox instance.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SetStatusMessage(string message)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.SetStatusMessage(tox, message);
        }

        /// <summary>
        /// Sets the typing status of this tox instance.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="is_typing"></param>
        /// <returns></returns>
        public bool SetUserIsTyping(int friendnumber, bool is_typing)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.SetUserIsTyping(tox, friendnumber, is_typing);
        }

        /// <summary>
        /// Send a message to a friend.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public int SendMessage(int friendnumber, string message)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.SendMessage(tox, friendnumber, message);
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
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.SendMessageWithID(tox, friendnumber, id, message);
        }

        /// <summary>
        /// Sends an action to a friend.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public int SendAction(int friendnumber, string action)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.SendAction(tox, friendnumber, action);
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
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.SendActionWithID(tox, friendnumber, id, message);
        }

        /// <summary>
        /// Saves the data of this tox instance at the given file location.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool Save(string filename)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

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

        /// <summary>
        /// Ends the tox_do loop and kills this tox instance.
        /// </summary>
        [Obsolete("This function is obsolete, use Dispose() instead", true)]
        public void Kill()
        {
            if (thread != null)
            {
                thread.Abort();
                thread.Join();
            }

            if (tox.IsClosed || tox.IsInvalid)
                throw null;

            tox.Dispose();
        }

        /// <summary>
        /// Deletes a friend.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public bool DeleteFriend(int friendnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.DeleteFriend(tox, friendnumber);
        }

        /// <summary>
        /// Joins a group with the given public key of the group.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="group_public_key"></param>
        /// <returns></returns>
        public int JoinGroup(int friendnumber, string group_public_key)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.JoinGroupchat(tox, friendnumber, group_public_key);
        }

        /// <summary>
        /// Retrieves the name of a group member.
        /// </summary>
        /// <param name="groupnumber"></param>
        /// <param name="peernumber"></param>
        /// <returns></returns>
        public string GetGroupMemberName(int groupnumber, int peernumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxTools.RemoveNull(ToxFunctions.GroupPeername(tox, groupnumber, peernumber));
        }

        /// <summary>
        /// Retrieves the number of group members in a group chat.
        /// </summary>
        /// <param name="groupnumber"></param>
        /// <returns></returns>
        public int GetGroupMemberCount(int groupnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GroupNumberPeers(tox, groupnumber);
        }

        /// <summary>
        /// Deletes a group chat.
        /// </summary>
        /// <param name="groupnumber"></param>
        /// <returns></returns>
        public bool DeleteGroupChat(int groupnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.DeleteGroupchat(tox, groupnumber);
        }

        /// <summary>
        /// Invites a friend to a group chat.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="groupnumber"></param>
        /// <returns></returns>
        public bool InviteFriend(int friendnumber, int groupnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.InviteFriend(tox, friendnumber, groupnumber);
        }

        /// <summary>
        /// Sends a message to a group.
        /// </summary>
        /// <param name="groupnumber"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendGroupMessage(int groupnumber, string message)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GroupMessageSend(tox, groupnumber, message);
        }

        /// <summary>
        /// Sends an action to a group.
        /// </summary>
        /// <param name="groupnumber"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool SendGroupAction(int groupnumber, string action)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GroupActionSend(tox, groupnumber, action);
        }

        /// <summary>
        /// Creates a new group and retrieves the group number.
        /// </summary>
        /// <returns></returns>
        public int NewGroup()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.AddGroupchat(tox);
        }

        /// <summary>
        /// Retrieves the nospam value.
        /// </summary>
        /// <returns></returns>
        public uint GetNospam()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GetNospam(tox);
        }

        /// <summary>
        /// Sets the nospam value.
        /// </summary>
        /// <param name="nospam"></param>
        public void SetNospam(uint nospam)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            ToxFunctions.SetNospam(tox, nospam);
        }

        /// <summary>
        /// Retrieves the handle of this tox instance.
        /// </summary>
        /// <returns></returns>
        public ToxHandle GetHandle()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return tox;
        }

        /// <summary>
        /// Whether to send read receipts for the specified friendnumber or not.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="send_receipts"></param>
        public void SetSendsReceipts(int friendnumber, bool send_receipts)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            ToxFunctions.SetSendsReceipts(tox, friendnumber, send_receipts);
        }

        /// <summary>
        /// Returns a pair of tox keys that belong to this instance.
        /// </summary>
        /// <returns></returns>
        public ToxKeyPair GetKeys()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GetKeys(tox);
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