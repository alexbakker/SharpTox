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
        public ToxOptions Options { get; private set; }

        /// <summary>
        /// Initializes a new instance of tox.
        /// </summary>
        /// <param name="options"></param>
        public Tox(ToxOptions options)
        {
            tox = ToxFunctions.New(ref options);

            if (tox == null || tox.IsInvalid)
                throw new Exception("Could not create a new instance of toxav.");

            Options = options;
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

            return ToxFunctions.IsConnected(tox) != 0;
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

            byte[] name = Encoding.UTF8.GetBytes(filename);
            if (name.Length > 255)
                throw new Exception("Filename is too long (longer than 255 bytes)");

            int result = ToxFunctions.NewFileSender(tox, friendnumber, filesize, name, (ushort)name.Length);
            if (result != -1)
                return result;
            else
                throw new Exception("Could not create new file sender");
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

            return ToxFunctions.FileSendControl(tox, friendnumber, (byte)send_receive, (byte)filenumber, (byte)message_id, data, (ushort)data.Length) == 0;
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

            return ToxFunctions.FileSendData(tox, friendnumber, (byte)filenumber, data, (ushort)data.Length) == 0;
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

            return ToxFunctions.FileDataRemaining(tox, friendnumber, (byte)filenumber, (byte)send_receive);
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

                return ToxFunctions.Load(tox, bytes, (uint)bytes.Length) == 0;
            }
            catch { return false; }
        }

        /// <summary>
        /// Retrieves an array of group member names.
        /// </summary>
        /// <param name="groupnumber"></param>
        /// <returns></returns>
        public string[] GetGroupNames(int groupnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int count = ToxFunctions.GroupNumberPeers(tox, groupnumber);

            //just return an empty string array before we get an overflow exception
            if (count <= 0)
                return new string[0];

            ushort[] lengths = new ushort[count];
            byte[,] matrix = new byte[count, ToxConstants.MaxNameLength];

            int result = ToxFunctions.GroupGetNames(tox, groupnumber, matrix, lengths, (ushort)count);

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

            byte[] binid = ToxTools.StringToHexBin(id);
            byte[] binmsg = Encoding.UTF8.GetBytes(message);

            int result = ToxFunctions.AddFriend(tox, binid, binmsg, (ushort)binmsg.Length);

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

            int result = ToxFunctions.AddFriendNoRequest(tox, ToxTools.StringToHexBin(id));

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

            return ToxFunctions.BootstrapFromAddress(tox, node.Address, (ushort)node.Port, node.PublicKey.GetBytes()) == 1;
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

            return ToxFunctions.FriendExists(tox, friendnumber) != 0;
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

            uint count = ToxFunctions.CountFriendlist(tox);
            int[] friends = new int[count];
            uint[] trunc = new uint[0];
            uint result = ToxFunctions.GetFriendlist(tox, friends, trunc);

            if (result == 0)
                return new int[0];
            else
                return friends;
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

            int size = ToxFunctions.GetNameSize(tox, friendnumber);
            byte[] name = new byte[size];

            ToxFunctions.GetName(tox, friendnumber, name);

            return ToxTools.RemoveNull(Encoding.UTF8.GetString(name));
        }

        /// <summary>
        /// Retrieves the nickname of this tox instance.
        /// </summary>
        /// <returns></returns>
        public string GetSelfName()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] bytes = new byte[129];
            ToxFunctions.GetSelfName(tox, bytes);

            return ToxTools.RemoveNull(Encoding.UTF8.GetString(bytes));
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

            byte[] address = new byte[38];
            ToxFunctions.GetAddress(tox, address);

            return ToxTools.HexBinToString(address);
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

            return ToxFunctions.GetIsTyping(tox, friendnumber) == 1;
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

            return ToxFunctions.GetFriendNumber(tox, ToxTools.StringToHexBin(id));
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

            int size = ToxFunctions.GetStatusMessageSize(tox, friendnumber);
            byte[] status = new byte[size];

            ToxFunctions.GetStatusMessage(tox, friendnumber, status, status.Length);

            return ToxTools.RemoveNull(Encoding.UTF8.GetString(status));
        }

        /// <summary>
        /// Retrieves the status message of this tox instance.
        /// </summary>
        /// <returns></returns>
        public string GetSelfStatusMessage()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int size = ToxFunctions.GetSelfStatusMessageSize(tox);
            byte[] status = new byte[size];

            ToxFunctions.GetSelfStatusMessage(tox, status, status.Length);

            return ToxTools.RemoveNull(Encoding.UTF8.GetString(status));
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
        public ToxKey GetClientID(int friendnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] address = new byte[32];
            ToxFunctions.GetClientID(tox, friendnumber, address);

            return new ToxKey(ToxKeyType.Public, address);
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

            return (ToxUserStatus)ToxFunctions.GetUserStatus(tox, friendnumber);
        }

        /// <summary>
        /// Retrieves the current user status of this tox instance.
        /// </summary>
        /// <returns></returns>
        public ToxUserStatus GetSelfUserStatus()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return (ToxUserStatus)ToxFunctions.GetSelfUserStatus(tox);
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

            byte[] bytes = Encoding.UTF8.GetBytes(name);
            return ToxFunctions.SetName(tox, bytes, (ushort)bytes.Length) == 0;
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

            return ToxFunctions.SetUserStatus(tox, (byte)status) == 0;
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

            byte[] msg = Encoding.UTF8.GetBytes(message);
            return ToxFunctions.SetStatusMessage(tox, msg, (ushort)msg.Length) == 0;
        }

        /// <summary>
        /// Sets the typing status of this tox instance.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="is_typing"></param>
        /// <returns></returns>
        public bool SetUserIsTyping(int friendnumber, bool is_typing)
        {
            

            byte typing = is_typing ? (byte)1 : (byte)0;
            return ToxFunctions.SetUserIsTyping(tox, friendnumber, typing) == 0;
        }

        /// <summary>
        /// Retrieves an array of valid chat IDs.
        /// </summary>
        /// <returns></returns>
        public int[] GetChatList()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int[] chats = new int[ToxFunctions.CountChatlist(tox)];
            uint[] trunc = new uint[0];
            uint result = ToxFunctions.GetChatlist(tox, chats, trunc);

            if (result == 0)
                return new int[0];
            else
                return chats;
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

            byte[] bytes = Encoding.UTF8.GetBytes(message);
            return (int)ToxFunctions.SendMessage(tox, friendnumber, bytes, bytes.Length);
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

            byte[] bytes = Encoding.UTF8.GetBytes(action);
            return (int)ToxFunctions.SendAction(tox, friendnumber, bytes, bytes.Length);
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

            return ToxFunctions.DelFriend(tox, friendnumber) == 0;
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

            return ToxFunctions.JoinGroupchat(tox, friendnumber, ToxTools.StringToHexBin(group_public_key));
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

            byte[] name = new byte[ToxConstants.MaxNameLength];
            if (ToxFunctions.GroupPeername(tox, groupnumber, peernumber, name) == -1)
                throw new Exception("Could not get peer name");
            else
                return ToxTools.RemoveNull(Encoding.UTF8.GetString(name));
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

            return ToxFunctions.DelGroupchat(tox, groupnumber) == 0;
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

            return ToxFunctions.InviteFriend(tox, friendnumber, groupnumber) == 0;
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

            byte[] msg = Encoding.UTF8.GetBytes(message);
            return ToxFunctions.GroupMessageSend(tox, groupnumber, msg, (uint)msg.Length) == 0;
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

            byte[] act = Encoding.UTF8.GetBytes(action);
            return ToxFunctions.GroupActionSend(tox, groupnumber, act, (uint)act.Length) == 0;
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
        /// Returns a pair of tox keys that belong to this instance.
        /// </summary>
        /// <returns></returns>
        public ToxKeyPair GetKeys()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] public_key = new byte[32];
            byte[] secret_key = new byte[32];

            ToxFunctions.GetKeys(tox, public_key, secret_key);

            return new ToxKeyPair(
                new ToxKey(ToxKeyType.Public, public_key),
                new ToxKey(ToxKeyType.Secret, secret_key)
                );
        }

        /// <summary>
        /// Retrieves a byte array that contains the data of this tox instance.
        /// </summary>
        /// <returns></returns>
        public byte[] GetDataBytes()
        {
            byte[] bytes = new byte[ToxFunctions.Size(tox)];
            ToxFunctions.Save(tox, bytes);

            return bytes;
        }

        /// Similar to BootstrapFromNode, except this is for tcp relays only.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool AddTcpRelay(ToxNode node)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.AddTcpRelay(tox, node.Address, (ushort)node.Port, node.PublicKey.GetBytes()) == 1;
        }

        private void callbacks()
        {
            ToxFunctions.CallbackFriendRequest(tox, friendrequestdelegate = new ToxDelegates.CallbackFriendRequestDelegate((IntPtr t, byte[] id, byte[] message, ushort length, IntPtr userdata) =>
            {
                if (OnFriendRequest != null)
                    Invoker(OnFriendRequest, ToxTools.RemoveNull(ToxTools.HexBinToString(id)), Encoding.UTF8.GetString(message, 0, length));
            }), IntPtr.Zero);

            ToxFunctions.CallbackConnectionStatus(tox, connectionstatusdelegate = new ToxDelegates.CallbackConnectionStatusDelegate((IntPtr t, int friendnumber, byte status, IntPtr userdata) =>
            {
                if (OnConnectionStatusChanged != null)
                    Invoker(OnConnectionStatusChanged, friendnumber, (int)status);
            }), IntPtr.Zero);

            ToxFunctions.CallbackFriendMessage(tox, friendmessagedelegate = new ToxDelegates.CallbackFriendMessageDelegate((IntPtr t, int friendnumber, byte[] message, ushort length, IntPtr userdata) =>
            {
                if (OnFriendMessage != null)
                    Invoker(OnFriendMessage, friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(message, 0, length)));
            }), IntPtr.Zero);

            ToxFunctions.CallbackFriendAction(tox, friendactiondelegate = new ToxDelegates.CallbackFriendActionDelegate((IntPtr t, int friendnumber, byte[] action, ushort length, IntPtr userdata) =>
            {
                if (OnFriendAction != null)
                    Invoker(OnFriendAction, friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(action, 0, length)));
            }), IntPtr.Zero);

            ToxFunctions.CallbackNameChange(tox, namechangedelegate = new ToxDelegates.CallbackNameChangeDelegate((IntPtr t, int friendnumber, byte[] newname, ushort length, IntPtr userdata) =>
            {
                if (OnNameChange != null)
                    Invoker(OnNameChange, friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(newname, 0, length)));
            }), IntPtr.Zero);

            ToxFunctions.CallbackStatusMessage(tox, statusmessagedelegate = new ToxDelegates.CallbackStatusMessageDelegate((IntPtr t, int friendnumber, byte[] newstatus, ushort length, IntPtr userdata) =>
            {
                if (OnStatusMessage != null)
                    Invoker(OnStatusMessage, friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(newstatus, 0, length)));
            }), IntPtr.Zero);

            ToxFunctions.CallbackUserStatus(tox, userstatusdelegate = new ToxDelegates.CallbackUserStatusDelegate((IntPtr t, int friendnumber, ToxUserStatus status, IntPtr userdata) =>
            {
                if (OnUserStatus != null)
                    Invoker(OnUserStatus, friendnumber, status);
            }), IntPtr.Zero);

            ToxFunctions.CallbackTypingChange(tox, typingchangedelegate = new ToxDelegates.CallbackTypingChangeDelegate((IntPtr t, int friendnumber, byte typing, IntPtr userdata) =>
            {
                bool is_typing = typing == 0 ? false : true;

                if (OnTypingChange != null)
                    Invoker(OnTypingChange, friendnumber, is_typing);
            }), IntPtr.Zero);

            ToxFunctions.CallbackGroupAction(tox, groupactiondelegate = new ToxDelegates.CallbackGroupActionDelegate((IntPtr t, int groupnumber, int friendgroupnumber, byte[] action, ushort length, IntPtr userdata) =>
            {
                if (OnGroupAction != null)
                    Invoker(OnGroupAction, groupnumber, friendgroupnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(action, 0, length)));
            }), IntPtr.Zero);

            ToxFunctions.CallbackGroupMessage(tox, groupmessagedelegate = new ToxDelegates.CallbackGroupMessageDelegate((IntPtr t, int groupnumber, int friendgroupnumber, byte[] message, ushort length, IntPtr userdata) =>
            {
                if (OnGroupMessage != null)
                    Invoker(OnGroupMessage, groupnumber, friendgroupnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(message, 0, length)));
            }), IntPtr.Zero);

            ToxFunctions.CallbackGroupInvite(tox, groupinvitedelegate = new ToxDelegates.CallbackGroupInviteDelegate((IntPtr t, int friendnumber, byte[] group_public_key, IntPtr userdata) =>
            {
                if (OnGroupInvite != null)
                    Invoker(OnGroupInvite, friendnumber, ToxTools.HexBinToString(group_public_key));
            }), IntPtr.Zero);

            ToxFunctions.CallbackGroupNamelistChange(tox, groupnamelistchangedelegate = new ToxDelegates.CallbackGroupNamelistChangeDelegate((IntPtr t, int groupnumber, int peernumber, ToxChatChange change, IntPtr userdata) =>
            {
                if (OnGroupNamelistChange != null)
                    Invoker(OnGroupNamelistChange, groupnumber, peernumber, change);
            }), IntPtr.Zero);

            ToxFunctions.CallbackFileControl(tox, filecontroldelegate = new ToxDelegates.CallbackFileControlDelegate((IntPtr t, int friendnumber, byte receive_send, byte filenumber, byte control_type, byte[] data, ushort length, IntPtr userdata) =>
            {
                if (OnFileControl != null)
                    Invoker(OnFileControl, friendnumber, receive_send, filenumber, control_type, data);
            }), IntPtr.Zero);

            ToxFunctions.CallbackFileData(tox, filedatadelegate = new ToxDelegates.CallbackFileDataDelegate((IntPtr t, int friendnumber, byte filenumber, byte[] data, ushort length, IntPtr userdata) =>
            {
                if (OnFileData != null)
                    Invoker(OnFileData, friendnumber, filenumber, data);
            }), IntPtr.Zero);

            ToxFunctions.CallbackFileSendRequest(tox, filesendrequestdelegate = new ToxDelegates.CallbackFileSendRequestDelegate((IntPtr t, int friendnumber, byte filenumber, ulong filesize, byte[] filename, ushort filename_length, IntPtr userdata) =>
            {
                if (OnFileSendRequest != null)
                    Invoker(OnFileSendRequest, friendnumber, filenumber, filesize, ToxTools.RemoveNull(Encoding.UTF8.GetString(filename, 0, filename_length)));
            }), IntPtr.Zero);

            ToxFunctions.CallbackReadReceipt(tox, readreceiptdelegate = new ToxDelegates.CallbackReadReceiptDelegate((IntPtr t, int friendnumber, uint receipt, IntPtr userdata) =>
            {
                if (OnReadReceipt != null)
                    Invoker(OnReadReceipt, friendnumber, receipt);
            }), IntPtr.Zero);
        }
    }
}

#pragma warning restore 1591