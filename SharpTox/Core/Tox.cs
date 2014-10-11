#pragma warning disable 1591

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using SharpTox.Encryption;

namespace SharpTox.Core
{
    #region Event Delegates
    public delegate void OnFriendRequestDelegate(string id, string message);
    public delegate void OnConnectionStatusDelegate(int friendnumber, ToxFriendConnectionStatus status);
    public delegate void OnFriendMessageDelegate(int friendnumber, string message);
    public delegate void OnFriendActionDelegate(int friendnumber, string action);
    public delegate void OnNameChangeDelegate(int friendnumber, string newname);
    public delegate void OnStatusMessageDelegate(int friendnumber, string newstatus);
    public delegate void OnUserStatusDelegate(int friendnumber, ToxUserStatus status);
    public delegate void OnTypingChangeDelegate(int friendnumber, bool is_typing);

    public delegate void OnGroupInviteDelegate(int friendnumber, byte[] data);
    public delegate void OnGroupMessageDelegate(int groupnumber, int friendgroupnumber, string message);
    public delegate void OnGroupActionDelegate(int groupnumber, int friendgroupnumber, string action);
    public delegate void OnGroupNamelistChangeDelegate(int groupnumber, int peernumber, ToxChatChange change);

    public delegate void OnFileControlDelegate(int friendnumber, int receive_send, int filenumber, int control_type, byte[] data);
    public delegate void OnFileDataDelegate(int friendnumber, int filenumber, byte[] data);
    public delegate void OnFileSendRequestDelegate(int friendnumber, int filenumber, ulong filesize, string filename);
    public delegate void OnReadReceiptDelegate(int friendnumber, uint receipt);

    public delegate void OnPacketDelegate(int friendnumber, byte[] data);
    public delegate void OnConnectionDelegate();

    public delegate void OnAvatarInfoDelegate(int friendnumber, ToxAvatarFormat format, byte[] hash);
    public delegate void OnAvatarDataDelegate(int friendnumber, ToxAvatar avatar);
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

        /// <summary>
        /// Occurs when a lossy packet is received.
        /// </summary>
        public event OnPacketDelegate OnLossyPacket;

        /// <summary>
        /// Occurs when a lossless packet is received.
        /// </summary>
        public event OnPacketDelegate OnLosslessPacket;

        /// <summary>
        /// Occurs when a connection to the DHT has been established.
        /// </summary>
        public event OnConnectionDelegate OnConnected;
        
        /// <summary>
        /// Occurs when the connection to the DHT was lost.
        /// </summary>
        public event OnConnectionDelegate OnDisconnected;

        /// <summary>
        /// Occurs when avatar info is received.
        /// </summary>
        public event OnAvatarInfoDelegate OnAvatarInfo;

        /// <summary>
        /// Occurs when avatar data is received.
        /// </summary>
        public event OnAvatarDataDelegate OnAvatarData;

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

        private ToxDelegates.CallbackAvatarDataDelegate avatardatadelegate;
        private ToxDelegates.CallbackAvatarInfoDelegate avatarinfodelegate;
        #endregion

        private ToxHandle tox;
        private Thread thread;

        private bool disposed = false;
        private bool connected = false;

        /// <summary>
        /// Options used for this instance of Tox.
        /// </summary>
        public ToxOptions Options { get; private set; }

        /// <summary>
        /// The avatar of this Tox instance.
        /// </summary>
        public ToxAvatar Avatar
        {
            get
            {
                if (disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                byte format = 0;
                uint length = 0;

                byte[] buf = new byte[ToxConstants.MaxAvatarDataLength];
                byte[] hash = new byte[ToxConstants.ToxHashLength];

                if (ToxFunctions.GetSelfAvatar(tox, ref format, buf, ref length, ToxConstants.MaxAvatarDataLength, hash) != 0)
                    return null;

                byte[] data = new byte[length];
                Array.Copy(buf, 0, data, 0, length);

                return new ToxAvatar((ToxAvatarFormat)format, data, hash);
            }
        }

        /// <summary>
        /// Whether or not we're connected to the DHT.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return ToxFunctions.IsConnected(tox) != 0;
            }
        }

        /// <summary>
        /// The number of friends in this Tox instance.
        /// </summary>
        public int FriendCount
        {
            get
            {
                if (disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return (int)ToxFunctions.CountFriendlist(tox);
            }
        }

        /// <summary>
        /// An array of friendnumbers of this Tox instance.
        /// </summary>
        public int[] FriendList
        {
            get
            {
                if (disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                uint count = ToxFunctions.CountFriendlist(tox);
                int[] friends = new int[count];
                uint[] trunc = new uint[0];
                uint result = ToxFunctions.GetFriendlist(tox, friends, trunc);

                if (result == 0)
                    return new int[0];

                return friends;
            }
        }

        /// <summary>
        /// The nickname of this Tox instance.
        /// </summary>
        public string Name
        {
            get
            {
                if (disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                byte[] bytes = new byte[129];
                ToxFunctions.GetSelfName(tox, bytes);

                return ToxTools.RemoveNull(Encoding.UTF8.GetString(bytes));
            }
            set
            {
                if (disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                byte[] bytes = Encoding.UTF8.GetBytes(value);
                ToxFunctions.SetName(tox, bytes, (ushort)bytes.Length);
            }
        }

        /// <summary>
        /// The pair of Tox keys that belong to this instance.
        /// </summary>
        public ToxKeyPair Keys
        {
            get
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
        }

        /// <summary>
        /// The string of a 32 byte long Tox Id to share with others.
        /// </summary>
        public string Id
        {
            get
            {
                if (disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                byte[] address = new byte[38];
                ToxFunctions.GetAddress(tox, address);

                return ToxTools.HexBinToString(address);
            }
        }

        /// <summary>
        /// The status message of this Tox instance.
        /// </summary>
        public string StatusMessage
        {
            get
            {
                if (disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                int size = ToxFunctions.GetSelfStatusMessageSize(tox);
                byte[] status = new byte[size];

                ToxFunctions.GetSelfStatusMessage(tox, status, status.Length);

                return ToxTools.RemoveNull(Encoding.UTF8.GetString(status));
            }
            set
            {
                if (disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                byte[] msg = Encoding.UTF8.GetBytes(value);
                ToxFunctions.SetStatusMessage(tox, msg, (ushort)msg.Length);
            }
        }

        /// <summary>
        /// Current user status of this Tox instance.
        /// </summary>
        public ToxUserStatus Status
        {
            get
            {
                if (disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return (ToxUserStatus)ToxFunctions.GetSelfUserStatus(tox);
            }
            set
            {
                if (disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                ToxFunctions.SetUserStatus(tox, (byte)value);
            }
        }

        /// <summary>
        /// An array of valid chat IDs.
        /// </summary>
        public int[] ChatList
        {
            get
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
        }

        public ToxHandle Handle
        {
            get
            {
                return tox;
            }
        }

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
            Invoker = dummyinvoker;

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
                if (IsConnected && !connected)
                {
                    if (OnConnected != null)
                        Invoker(OnConnected);

                    connected = true;
                }
                else if (!IsConnected && connected)
                {
                    if (OnDisconnected != null)
                        Invoker(OnDisconnected);

                    connected = false;
                }

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
        public ToxFriendConnectionStatus GetFriendConnectionStatus(int friendnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return (ToxFriendConnectionStatus)ToxFunctions.GetFriendConnectionStatus(tox, friendnumber);
        }

        public bool IsOnline(int friendnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return GetFriendConnectionStatus(friendnumber) == ToxFriendConnectionStatus.Online;
        }

        /// <summary>
        /// Retrieves a friend's public id/address.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public ToxKey GetClientId(int friendnumber)
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
        /// Sets the typing status of this tox instance.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="is_typing"></param>
        /// <returns></returns>
        public bool SetUserIsTyping(int friendnumber, bool is_typing)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte typing = is_typing ? (byte)1 : (byte)0;
            return ToxFunctions.SetUserIsTyping(tox, friendnumber, typing) == 0;
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
        /// Ends the tox_do loop and kills this tox instance.
        /// </summary>
        [Obsolete("Use Dispose() instead", true)]
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
        /// <param name="data">Data obtained from the OnGroupInvite event.</param>
        /// <returns></returns>
        public int JoinGroup(int friendnumber, byte[] data)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.JoinGroupchat(tox, friendnumber, data, (ushort)data.Length);
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
            return ToxFunctions.GroupMessageSend(tox, groupnumber, msg, (ushort)msg.Length) == 0;
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
            return ToxFunctions.GroupActionSend(tox, groupnumber, act, (ushort)act.Length) == 0;
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
        /// Sends a lossy packet to the specified friendnumber.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SendLossyPacket(int friendnumber, byte[] data)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (data.Length > ToxConstants.MaxCustomPacketSize)
                throw new ArgumentException("Packet size is bigger than ToxConstants.MaxCustomPacketSize");

            if (data[0] < 200 || data[0] > 254)
                throw new ArgumentException("First byte of data is not in the 200-254 range.");

            return ToxFunctions.SendLossyPacket(tox, friendnumber, data, (uint)data.Length) == 0;
        }

        /// <summary>
        /// Sends a lossless packet to the specified friendnumber.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SendLosslessPacket(int friendnumber, byte[] data)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (data.Length > ToxConstants.MaxCustomPacketSize)
                throw new ArgumentException("Packet size is bigger than ToxConstants.MaxCustomPacketSize");

            if (data[0] < 160 || data[0] > 191)
                throw new ArgumentException("First byte of data is not in the 160-191 range.");

            return ToxFunctions.SendLosslessPacket(tox, friendnumber, data, (uint)data.Length) == 0;
        }

        /// <summary>
        /// Registers a handler for lossy packets starting with start_byte. These packets can be captured with <see cref="OnLossyPacket"/>.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="start_byte"></param>
        /// <returns></returns>
        public bool RegisterLossyPacketHandler(int friendnumber, byte start_byte)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (start_byte < 200 || start_byte > 254)
                throw new ArgumentException("start_byte is not in the 200-254 range.");

            ToxDelegates.CallbackPacketDelegate del = ((IntPtr obj, byte[] data, uint length) => 
            {
                if (OnLossyPacket != null)
                    Invoker(OnLossyPacket, friendnumber, data);

                return 1;

            });
            lossyPacketHandlers.Add(del);

            return ToxFunctions.CallbackLossyPacket(tox, friendnumber, start_byte, del, IntPtr.Zero) == 0;
        }

        private List<ToxDelegates.CallbackPacketDelegate> lossyPacketHandlers = new List<ToxDelegates.CallbackPacketDelegate>();
        private List<ToxDelegates.CallbackPacketDelegate> losslessPacketHandlers = new List<ToxDelegates.CallbackPacketDelegate>();

        /// <summary>
        /// Registers a handler for lossy packets starting with start_byte.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="start_byte"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool RegisterLossyPacketHandler(int friendnumber, byte start_byte, ToxDelegates.CallbackPacketDelegate callback)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (start_byte < 200 || start_byte > 254)
                throw new ArgumentException("start_byte is not in the 200-254 range.");

            return ToxFunctions.CallbackLossyPacket(tox, friendnumber, start_byte, callback, IntPtr.Zero) == 0;
        }

        /// <summary>
        /// Registers a handler for lossless packets starting with start_byte. These packets can be captured with <see cref="OnLosslessPacket"/>.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="start_byte"></param>
        /// <returns></returns>
        public bool RegisterLosslessPacketHandler(int friendnumber, byte start_byte)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (start_byte < 160 || start_byte > 191)
                throw new ArgumentException("start_byte is not in the 160-191 range.");

            ToxDelegates.CallbackPacketDelegate del = ((IntPtr obj, byte[] data, uint length) =>
            {
                if (OnLosslessPacket != null)
                    Invoker(OnLosslessPacket, friendnumber, data);

                return 1;

            });
            losslessPacketHandlers.Add(del);

            return ToxFunctions.CallbackLosslessPacket(tox, friendnumber, start_byte, del, IntPtr.Zero) == 0;
        }

        /// <summary>
        /// Registers a handler for lossless packets starting with start_byte.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <param name="start_byte"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool RegisterLosslessPacketHandler(int friendnumber, byte start_byte, ToxDelegates.CallbackPacketDelegate callback)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (start_byte < 160 || start_byte > 191)
                throw new ArgumentException("start_byte is not in the 160-191 range.");

            return ToxFunctions.CallbackLosslessPacket(tox, friendnumber, start_byte, callback, IntPtr.Zero) == 0;
        }

        /// <summary>
        /// Retrieves a ToxData object that contains the data of this tox instance.
        /// </summary>
        /// <returns></returns>
        public ToxData GetData()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] bytes = new byte[ToxFunctions.Size(tox)];
            ToxFunctions.Save(tox, bytes);

            return new ToxData(bytes);
        }

        /// <summary>
        /// Retrieves a ToxData object that contains the data of this tox instance, encrypted with the given passphrase.
        /// </summary>
        /// <param name="passphrase"></param>
        /// <returns></returns>
        public ToxData GetData(string passphrase)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] bytes = new byte[ToxEncryptionFunctions.EncryptedSize(tox)];
            byte[] phrase = Encoding.UTF8.GetBytes(passphrase);

            if (ToxEncryptionFunctions.EncryptedSave(tox, bytes, phrase, (uint)phrase.Length) != 0)
                return null;

            return new ToxData(bytes);
        }

        /// <summary>
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

        /// <summary>
        /// Loads tox data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Load(ToxData data)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (data == null || data.IsEncrypted)
                return false;

            int result = ToxFunctions.Load(tox, data.Bytes, (uint)data.Bytes.Length);

            return (result == 0 || result == -1);
        }

        /// <summary>
        /// Loads and decrypts tox data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="passphrase"></param>
        /// <returns></returns>
        public bool Load(ToxData data, string passphrase)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] phrase = Encoding.UTF8.GetBytes(passphrase);

            if (data.IsEncrypted)
                return ToxEncryptionFunctions.EncryptedLoad(tox, data.Bytes, (uint)data.Bytes.Length, phrase, (uint)phrase.Length) == 0;
            else 
                return Load(data);
        }

        /// <summary>
        /// Sets the avatar of this Tox instance.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SetAvatar(ToxAvatarFormat format, byte[] data)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.SetAvatar(tox, (byte)format, data, (uint)data.Length) == 0;
        }

        /// <summary>
        /// Retrieves a cryptographic hash of the given data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] GetHash(byte[] data)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] hash = new byte[ToxConstants.ToxHashLength];

            if (ToxFunctions.Hash(hash, data, (uint)data.Length) != 0)
                return new byte[0];

            return hash;
        }

        /// <summary>
        /// Requests avatar info from a friend.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public bool RequestAvatarInfo(int friendnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.RequestAvatarInfo(tox, friendnumber) == 0;
        }

        /// <summary>
        /// Requests avatar data from a friend.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public bool RequestAvatarData(int friendnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.RequestAvatarData(tox, friendnumber) == 0;
        }

        /// <summary>
        /// Sends avatar info to a friend.
        /// </summary>
        /// <param name="friendnumber"></param>
        /// <returns></returns>
        public bool SendAvatarInfo(int friendnumber)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.SendAvatarInfo(tox, friendnumber) == 0;
        }

        /// <summary>
        /// Removes the avatar of this Tox instance.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use UnsetAvatar() instead")]
        public bool RemoveAvatar()
        {
            /*if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.SetAvatar(tox, (byte)ToxAvatarFormat.None, null, 0) == 0;*/
            return UnsetAvatar();
        }

        /// <summary>
        /// Unsets the avatar of this Tox instance.
        /// </summary>
        /// <returns></returns>
        public bool UnsetAvatar()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.UnsetAvatar(tox) == 0;
        }

        private void callbacks()
        {
            ToxFunctions.CallbackFriendRequest(tox, friendrequestdelegate = (IntPtr t, byte[] id, byte[] message, ushort length, IntPtr userdata) =>
            {
                if (OnFriendRequest != null)
                    Invoker(OnFriendRequest, ToxTools.RemoveNull(ToxTools.HexBinToString(id)), Encoding.UTF8.GetString(message, 0, length));
            }, IntPtr.Zero);

            ToxFunctions.CallbackConnectionStatus(tox, connectionstatusdelegate = (IntPtr t, int friendnumber, byte status, IntPtr userdata) =>
            {
                if (OnConnectionStatusChanged != null)
                    Invoker(OnConnectionStatusChanged, friendnumber, (ToxFriendConnectionStatus)status);
            }, IntPtr.Zero);

            ToxFunctions.CallbackFriendMessage(tox, friendmessagedelegate = (IntPtr t, int friendnumber, byte[] message, ushort length, IntPtr userdata) =>
            {
                if (OnFriendMessage != null)
                    Invoker(OnFriendMessage, friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(message, 0, length)));
            }, IntPtr.Zero);

            ToxFunctions.CallbackFriendAction(tox, friendactiondelegate = (IntPtr t, int friendnumber, byte[] action, ushort length, IntPtr userdata) =>
            {
                if (OnFriendAction != null)
                    Invoker(OnFriendAction, friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(action, 0, length)));
            }, IntPtr.Zero);

            ToxFunctions.CallbackNameChange(tox, namechangedelegate = (IntPtr t, int friendnumber, byte[] newname, ushort length, IntPtr userdata) =>
            {
                if (OnNameChange != null)
                    Invoker(OnNameChange, friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(newname, 0, length)));
            }, IntPtr.Zero);

            ToxFunctions.CallbackStatusMessage(tox, statusmessagedelegate = (IntPtr t, int friendnumber, byte[] newstatus, ushort length, IntPtr userdata) =>
            {
                if (OnStatusMessage != null)
                    Invoker(OnStatusMessage, friendnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(newstatus, 0, length)));
            }, IntPtr.Zero);

            ToxFunctions.CallbackUserStatus(tox, userstatusdelegate = (IntPtr t, int friendnumber, ToxUserStatus status, IntPtr userdata) =>
            {
                if (OnUserStatus != null)
                    Invoker(OnUserStatus, friendnumber, status);
            }, IntPtr.Zero);

            ToxFunctions.CallbackTypingChange(tox, typingchangedelegate = (IntPtr t, int friendnumber, byte typing, IntPtr userdata) =>
            {
                bool is_typing = typing != 0;

                if (OnTypingChange != null)
                    Invoker(OnTypingChange, friendnumber, is_typing);
            }, IntPtr.Zero);

            ToxFunctions.CallbackGroupAction(tox, groupactiondelegate = (IntPtr t, int groupnumber, int friendgroupnumber, byte[] action, ushort length, IntPtr userdata) =>
            {
                if (OnGroupAction != null)
                    Invoker(OnGroupAction, groupnumber, friendgroupnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(action, 0, length)));
            }, IntPtr.Zero);

            ToxFunctions.CallbackGroupMessage(tox, groupmessagedelegate = (IntPtr t, int groupnumber, int friendgroupnumber, byte[] message, ushort length, IntPtr userdata) =>
            {
                if (OnGroupMessage != null)
                    Invoker(OnGroupMessage, groupnumber, friendgroupnumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(message, 0, length)));
            }, IntPtr.Zero);

            ToxFunctions.CallbackGroupInvite(tox, groupinvitedelegate = (IntPtr t, int friendnumber, byte[] data, ushort length, IntPtr userdata) =>
            {
                if (OnGroupInvite != null)
                    Invoker(OnGroupInvite, friendnumber, data);
            }, IntPtr.Zero);

            ToxFunctions.CallbackGroupNamelistChange(tox, groupnamelistchangedelegate = (IntPtr t, int groupnumber, int peernumber, ToxChatChange change, IntPtr userdata) =>
            {
                if (OnGroupNamelistChange != null)
                    Invoker(OnGroupNamelistChange, groupnumber, peernumber, change);
            }, IntPtr.Zero);

            ToxFunctions.CallbackFileControl(tox, filecontroldelegate = (IntPtr t, int friendnumber, byte receive_send, byte filenumber, byte control_type, byte[] data, ushort length, IntPtr userdata) =>
            {
                if (OnFileControl != null)
                    Invoker(OnFileControl, friendnumber, receive_send, filenumber, control_type, data);
            }, IntPtr.Zero);

            ToxFunctions.CallbackFileData(tox, filedatadelegate = (IntPtr t, int friendnumber, byte filenumber, byte[] data, ushort length, IntPtr userdata) =>
            {
                if (OnFileData != null)
                    Invoker(OnFileData, friendnumber, filenumber, data);
            }, IntPtr.Zero);

            ToxFunctions.CallbackFileSendRequest(tox, filesendrequestdelegate = (IntPtr t, int friendnumber, byte filenumber, ulong filesize, byte[] filename, ushort filename_length, IntPtr userdata) =>
            {
                if (OnFileSendRequest != null)
                    Invoker(OnFileSendRequest, friendnumber, filenumber, filesize, ToxTools.RemoveNull(Encoding.UTF8.GetString(filename, 0, filename_length)));
            }, IntPtr.Zero);

            ToxFunctions.CallbackReadReceipt(tox, readreceiptdelegate = (IntPtr t, int friendnumber, uint receipt, IntPtr userdata) =>
            {
                if (OnReadReceipt != null)
                    Invoker(OnReadReceipt, friendnumber, receipt);
            }, IntPtr.Zero);

            ToxFunctions.CallbackAvatarInfo(tox, avatarinfodelegate = (IntPtr t, int friendnumber, byte format, byte[] hash, IntPtr userdata) =>
            {
                if (OnAvatarInfo != null)
                    Invoker(OnAvatarInfo, friendnumber, (ToxAvatarFormat)format, hash);
            }, IntPtr.Zero);

            ToxFunctions.CallbackAvatarData(tox, avatardatadelegate = (IntPtr t, int friendnumber, byte format, byte[] hash, byte[] data, uint datalen, IntPtr userdata) =>
            {
                if (OnAvatarData != null)
                    Invoker(OnAvatarData, friendnumber, new ToxAvatar((ToxAvatarFormat)format, data, hash));
            }, IntPtr.Zero);
        }
    }
}

#pragma warning restore 1591