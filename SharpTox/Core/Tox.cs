#pragma warning disable 1591

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using SharpTox.Encryption;

namespace SharpTox.Core
{
    /// <summary>
    /// Represents an instance of Tox.
    /// </summary>
    public class Tox : IDisposable
    {
        public delegate object InvokeDelegate(Delegate method, params object[] p);

        /// <summary>
        /// The invoke delegate to use when raising events.
        /// </summary>
        public InvokeDelegate Invoker;

        #region Callback Delegates
        private ToxDelegates.CallbackFriendRequestDelegate _onFriendRequestCallback;
        private ToxDelegates.CallbackConnectionStatusDelegate _onConnectionStatusCallback;
        private ToxDelegates.CallbackFriendMessageDelegate _onFriendMessageCallback;
        private ToxDelegates.CallbackFriendActionDelegate _onFriendActionCallback;
        private ToxDelegates.CallbackNameChangeDelegate _onNameChangeCallback;
        private ToxDelegates.CallbackStatusMessageDelegate _onStatusMessageCallback;
        private ToxDelegates.CallbackUserStatusDelegate _onUserStatusCallback;
        private ToxDelegates.CallbackTypingChangeDelegate _onTypingChangeCallback;

        private ToxDelegates.CallbackGroupInviteDelegate _onGroupInviteCallback;
        private ToxDelegates.CallbackGroupActionDelegate _onGroupActionCallback;
        private ToxDelegates.CallbackGroupMessageDelegate _onGroupMessageCallback;
        private ToxDelegates.CallbackGroupNamelistChangeDelegate _onGroupNamelistChangeCallback;
        private ToxDelegates.CallbackGroupTitleDelegate _onGroupTitleCallback;

        private ToxDelegates.CallbackFileControlDelegate _onFileControlCallback;
        private ToxDelegates.CallbackFileDataDelegate _onFileDataCallback;
        private ToxDelegates.CallbackFileSendRequestDelegate _onFileSendRequestCallback;

        private ToxDelegates.CallbackReadReceiptDelegate _onReadReceiptCallback;

        private ToxDelegates.CallbackAvatarDataDelegate _onAvatarDataCallback;
        private ToxDelegates.CallbackAvatarInfoDelegate _onAvatarInfoCallback;
        #endregion

        private ToxHandle _tox;
        private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();

        private bool _disposed = false;
        private bool _connected = false;

        private List<ToxDelegates.CallbackPacketDelegate> _lossyPacketHandlers = new List<ToxDelegates.CallbackPacketDelegate>();
        private List<ToxDelegates.CallbackPacketDelegate> _losslessPacketHandlers = new List<ToxDelegates.CallbackPacketDelegate>();

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
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                byte format = 0;
                uint length = 0;

                byte[] buf = new byte[ToxConstants.MaxAvatarDataLength];
                byte[] hash = new byte[ToxConstants.ToxHashLength];

                if (ToxFunctions.GetSelfAvatar(_tox, ref format, buf, ref length, ToxConstants.MaxAvatarDataLength, hash) != 0)
                    return null;

                byte[] data = new byte[length];
                Array.Copy(buf, 0, data, 0, (int)length);

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
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return ToxFunctions.IsConnected(_tox) != 0;
            }
        }

        /// <summary>
        /// The number of friends in this Tox instance.
        /// </summary>
        public int FriendCount
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return (int)ToxFunctions.CountFriendlist(_tox);
            }
        }

        /// <summary>
        /// An array of friendnumbers of this Tox instance.
        /// </summary>
        public int[] FriendList
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                uint count = ToxFunctions.CountFriendlist(_tox);
                int[] friends = new int[count];
                uint[] trunc = new uint[0];
                uint result = ToxFunctions.GetFriendlist(_tox, friends, trunc);

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
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                byte[] bytes = new byte[129];
                ToxFunctions.GetSelfName(_tox, bytes);

                return ToxTools.RemoveNull(Encoding.UTF8.GetString(bytes, 0, bytes.Length));
            }
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                byte[] bytes = Encoding.UTF8.GetBytes(value);
                ToxFunctions.SetName(_tox, bytes, (ushort)bytes.Length);
            }
        }

        /// <summary>
        /// The pair of Tox keys that belong to this instance.
        /// </summary>
        public ToxKeyPair Keys
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                byte[] publicKey = new byte[32];
                byte[] secretKey = new byte[32];

                ToxFunctions.GetKeys(_tox, publicKey, secretKey);

                return new ToxKeyPair(
                    new ToxKey(ToxKeyType.Public, publicKey),
                    new ToxKey(ToxKeyType.Secret, secretKey)
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
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                byte[] address = new byte[38];
                ToxFunctions.GetAddress(_tox, address);

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
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                int size = ToxFunctions.GetSelfStatusMessageSize(_tox);
                byte[] status = new byte[size];

                ToxFunctions.GetSelfStatusMessage(_tox, status, status.Length);

                return ToxTools.RemoveNull(Encoding.UTF8.GetString(status, 0, status.Length));
            }
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                byte[] msg = Encoding.UTF8.GetBytes(value);
                ToxFunctions.SetStatusMessage(_tox, msg, (ushort)msg.Length);
            }
        }

        /// <summary>
        /// Current user status of this Tox instance.
        /// </summary>
        public ToxUserStatus Status
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return (ToxUserStatus)ToxFunctions.GetSelfUserStatus(_tox);
            }
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                ToxFunctions.SetUserStatus(_tox, (byte)value);
            }
        }

        /// <summary>
        /// An array of valid chat IDs.
        /// </summary>
        public int[] ChatList
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                int[] chats = new int[ToxFunctions.CountChatlist(_tox)];
                uint[] trunc = new uint[0];
                uint result = ToxFunctions.GetChatlist(_tox, chats, trunc);

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
                return _tox;
            }
        }

        /// <summary>
        /// Initializes a new instance of Tox.
        /// </summary>
        /// <param name="options"></param>
        public Tox(ToxOptions options)
        {
            _tox = ToxFunctions.New(ref options);

            if (_tox == null || _tox.IsInvalid)
                throw new Exception("Could not create a new instance of toxav.");

            Options = options;
            Invoker = DummyInvoker;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //dispose pattern as described on msdn for a class that uses a safe handle
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_cancelTokenSource != null)
                {
                    _cancelTokenSource.Cancel();
                    _cancelTokenSource.Dispose();
                }
            }

            if (!_tox.IsInvalid && !_tox.IsClosed && _tox != null)
                _tox.Dispose();

            ClearEventSubscriptions();

            _disposed = true;
        }

        private void ClearEventSubscriptions()
        {
            _onAvatarData = null;
            _onAvatarInfo = null;
            _onConnectionStatusChanged = null;
            _onFileControl = null;
            _onFileData = null;
            _onFileSendRequest = null;
            _onFriendAction = null;
            _onFriendMessage = null;
            _onFriendRequest = null;
            _onGroupAction = null;
            _onGroupInvite = null;
            _onGroupMessage = null;
            _onGroupNamelistChange = null;
            _onNameChange = null;
            _onReadReceipt = null;
            _onStatusMessage = null;
            _onTypingChange = null;
            _onUserStatus = null;

            OnLosslessPacket = null;
            OnLossyPacket = null;
            OnConnected = null;
            OnDisconnected = null;
        }

        private object DummyInvoker(Delegate method, params object[] p)
        {
            return method.DynamicInvoke(p);
        }

        /// <summary>
        /// Sends a file send request to the given friendNumber.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="fileSize"></param>
        /// <param name="filename">Maximum filename length is 255 bytes.</param>
        /// <returns>the filenumber on success and -1 on failure.</returns>
        public int NewFileSender(int friendNumber, ulong fileSize, string filename)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] name = Encoding.UTF8.GetBytes(filename);
            if (name.Length > 255)
                throw new Exception("Filename is too long (longer than 255 bytes)");

            int result = ToxFunctions.NewFileSender(_tox, friendNumber, fileSize, name, (ushort)name.Length);
            if (result != -1)
                return result;
            else
                throw new Exception("Could not create new file sender");
        }

        /// <summary>
        /// Sends a file control request.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="sendReceive">0 if we're sending and 1 if we're receiving.</param>
        /// <param name="fileNumber"></param>
        /// <param name="messageId"></param>
        /// <param name="data"></param>
        /// <returns>true on success and false on failure.</returns>
        public bool FileSendControl(int friendNumber, int sendReceive, int fileNumber, ToxFileControl messageId, byte[] data)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.FileSendControl(_tox, friendNumber, (byte)sendReceive, (byte)fileNumber, (byte)messageId, data, (ushort)data.Length) == 0;
        }

        /// <summary>
        /// Sends file data.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="fileNumber"></param>
        /// <param name="data"></param>
        /// <returns>true on success and false on failure.</returns>
        public bool FileSendData(int friendNumber, int fileNumber, byte[] data)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.FileSendData(_tox, friendNumber, (byte)fileNumber, data, (ushort)data.Length) == 0;
        }

        /// <summary>
        /// Retrieves the recommended/maximum size of the filedata to send with FileSendData.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public int FileDataSize(int friendNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.FileDataSize(_tox, friendNumber);
        }

        /// <summary>
        /// Retrieves the number of bytes left to be sent/received.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="fileNumber"></param>
        /// <param name="sendReceive">0 if we're sending and 1 if we're receiving.</param>
        /// <returns></returns>
        public ulong FileDataRemaining(int friendNumber, int fileNumber, int sendReceive)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.FileDataRemaining(_tox, friendNumber, (byte)fileNumber, (byte)sendReceive);
        }

        /// <summary>
        /// Retrieves an array of group member names.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <returns></returns>
        public string[] GetGroupNames(int groupNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int count = ToxFunctions.GroupNumberPeers(_tox, groupNumber);

            //just return an empty string array before we get an overflow exception
            if (count <= 0)
                return new string[0];

            ushort[] lengths = new ushort[count];
            byte[,] matrix = new byte[count, ToxConstants.MaxNameLength];

            int result = ToxFunctions.GroupGetNames(_tox, groupNumber, matrix, lengths, (ushort)count);

            string[] names = new string[count];
            for (int i = 0; i < count; i++)
            {
                byte[] name = new byte[lengths[i]];

                for (int j = 0; j < name.Length; j++)
                    name[j] = matrix[i, j];

                names[i] = ToxTools.RemoveNull(Encoding.UTF8.GetString(name, 0, name.Length));
            }

            return names;
        }

        /// <summary>
        /// Starts the main tox_do loop.
        /// </summary>
        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            Loop();
        }

        private void Loop()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (_cancelTokenSource.IsCancellationRequested)
                        break;

                    if (IsConnected && !_connected)
                    {
                        if (OnConnected != null)
                            Invoker(OnConnected, this, new ToxEventArgs.ConnectionEventArgs(true));

                        _connected = true;
                    }
                    else if (!IsConnected && _connected)
                    {
                        if (OnDisconnected != null)
                            Invoker(OnDisconnected, this, new ToxEventArgs.ConnectionEventArgs(false));

                        _connected = false;
                    }

                    ToxFunctions.Do(_tox);

#if IS_PORTABLE
                    Task.Delay((int)ToxFunctions.DoInterval(_tox));
#else
                    Thread.Sleep((int)ToxFunctions.DoInterval(_tox));
#endif
                }
            }, _cancelTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// Adds a friend.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="message"></param>
        /// <returns>friendNumber</returns>
        public int AddFriend(string id, string message)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] binId = ToxTools.StringToHexBin(id);
            byte[] binMsg = Encoding.UTF8.GetBytes(message);

            int result = ToxFunctions.AddFriend(_tox, binId, binMsg, (ushort)binMsg.Length);

            if (result < 0)
                throw new ToxAFException((ToxAFError)result);

            return result;
        }

        /// <summary>
        /// Adds a friend without sending a request.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>friendNumber</returns>
        public int AddFriendNoRequest(string id)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.AddFriendNoRequest(_tox, ToxTools.StringToHexBin(id));
        }

        /// <summary>
        /// Bootstraps this Tox instance with a ToxNode.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool BootstrapFromNode(ToxNode node)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.BootstrapFromAddress(_tox, node.Address, (ushort)node.Port, node.PublicKey.GetBytes()) == 1;
        }

        /// <summary>
        /// Checks if there exists a friend with given friendNumber.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public bool FriendExists(int friendNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.FriendExists(_tox, friendNumber) != 0;
        }

        /// <summary>
        /// Retrieves the name of a friendNumber.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public string GetName(int friendNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int size = ToxFunctions.GetNameSize(_tox, friendNumber);
            byte[] name = new byte[size];

            ToxFunctions.GetName(_tox, friendNumber, name);

            return ToxTools.RemoveNull(Encoding.UTF8.GetString(name, 0, name.Length));
        }

        /// <summary>
        /// Retrieves a DateTime object of the last time friendNumber was seen online.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public DateTime GetLastOnline(int friendNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxTools.EpochToDateTime((long)ToxFunctions.GetLastOnline(_tox, friendNumber));
        }

        /// <summary>
        /// Retrieves the typing status of a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public bool GetIsTyping(int friendNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GetIsTyping(_tox, friendNumber) == 1;
        }

        /// <summary>
        /// Retrieves the friendNumber associated to the specified public address/id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetFriendNumber(string id)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GetFriendNumber(_tox, ToxTools.StringToHexBin(id));
        }

        /// <summary>
        /// Retrieves the status message of a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public string GetStatusMessage(int friendNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int size = ToxFunctions.GetStatusMessageSize(_tox, friendNumber);
            byte[] status = new byte[size];

            ToxFunctions.GetStatusMessage(_tox, friendNumber, status, status.Length);

            return ToxTools.RemoveNull(Encoding.UTF8.GetString(status, 0, status.Length));
        }

        /// <summary>
        /// Retrieves the amount of friends who are currently online.
        /// </summary>
        /// <returns></returns>
        public int GetOnlineFriendsCount()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return (int)ToxFunctions.GetNumOnlineFriends(_tox);
        }

        /// <summary>
        /// Retrieves a friend's connection status.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public ToxFriendConnectionStatus GetFriendConnectionStatus(int friendNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return (ToxFriendConnectionStatus)ToxFunctions.GetFriendConnectionStatus(_tox, friendNumber);
        }

        public bool IsOnline(int friendNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return GetFriendConnectionStatus(friendNumber) == ToxFriendConnectionStatus.Online;
        }

        /// <summary>
        /// Retrieves a friend's public id/address.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public ToxKey GetClientId(int friendNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] address = new byte[32];
            ToxFunctions.GetClientID(_tox, friendNumber, address);

            return new ToxKey(ToxKeyType.Public, address);
        }

        /// <summary>
        /// Retrieves a friend's current user status.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public ToxUserStatus GetUserStatus(int friendNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return (ToxUserStatus)ToxFunctions.GetUserStatus(_tox, friendNumber);
        }

        /// <summary>
        /// Sets the typing status of this Tox instance.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="isTyping"></param>
        /// <returns></returns>
        public bool SetUserIsTyping(int friendNumber, bool isTyping)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte typing = isTyping ? (byte)1 : (byte)0;
            return ToxFunctions.SetUserIsTyping(_tox, friendNumber, typing) == 0;
        }

        /// <summary>
        /// Send a message to a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public int SendMessage(int friendNumber, string message)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] bytes = Encoding.UTF8.GetBytes(message);
            return (int)ToxFunctions.SendMessage(_tox, friendNumber, bytes, bytes.Length);
        }

        /// <summary>
        /// Sends an action to a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public int SendAction(int friendNumber, string action)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] bytes = Encoding.UTF8.GetBytes(action);
            return (int)ToxFunctions.SendAction(_tox, friendNumber, bytes, bytes.Length);
        }

        /// <summary>
        /// Ends the tox_do loop and kills this Tox instance.
        /// </summary>
        [Obsolete("Use Dispose() instead", true)]
        public void Kill()
        {
            if (_cancelTokenSource != null)
            {
                _cancelTokenSource.Cancel();
                _cancelTokenSource.Dispose();
            }

            if (_tox.IsClosed || _tox.IsInvalid)
                return;

            _tox.Dispose();
        }

        /// <summary>
        /// Deletes a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public bool DeleteFriend(int friendNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.DelFriend(_tox, friendNumber) == 0;
        }

        /// <summary>
        /// Joins a group with the given public key of the group.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="data">Data obtained from the OnGroupInvite event.</param>
        /// <returns></returns>
        public int JoinGroup(int friendNumber, byte[] data)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.JoinGroupchat(_tox, friendNumber, data, (ushort)data.Length);
        }

        /// <summary>
        /// Retrieves the name of a group member.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="peerNumber"></param>
        /// <returns></returns>
        public string GetGroupMemberName(int groupNumber, int peerNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] name = new byte[ToxConstants.MaxNameLength];
            if (ToxFunctions.GroupPeername(_tox, groupNumber, peerNumber, name) == -1)
                throw new Exception("Could not get peer name");

            return ToxTools.RemoveNull(Encoding.UTF8.GetString(name, 0, name.Length));
        }

        /// <summary>
        /// Retrieves the number of group members in a group chat.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <returns></returns>
        public int GetGroupMemberCount(int groupNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GroupNumberPeers(_tox, groupNumber);
        }

        /// <summary>
        /// Deletes a group chat.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <returns></returns>
        public bool DeleteGroupChat(int groupNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.DelGroupchat(_tox, groupNumber) == 0;
        }

        /// <summary>
        /// Invites a friend to a group chat.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="groupNumber"></param>
        /// <returns></returns>
        public bool InviteFriend(int friendNumber, int groupNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.InviteFriend(_tox, friendNumber, groupNumber) == 0;
        }

        /// <summary>
        /// Sends a message to a group.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendGroupMessage(int groupNumber, string message)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] msg = Encoding.UTF8.GetBytes(message);
            return ToxFunctions.GroupMessageSend(_tox, groupNumber, msg, (ushort)msg.Length) == 0;
        }

        /// <summary>
        /// Sends an action to a group.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool SendGroupAction(int groupNumber, string action)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] act = Encoding.UTF8.GetBytes(action);
            return ToxFunctions.GroupActionSend(_tox, groupNumber, act, (ushort)act.Length) == 0;
        }

        /// <summary>
        /// Creates a new group and retrieves the group number.
        /// </summary>
        /// <returns></returns>
        public int NewGroup()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.AddGroupchat(_tox);
        }

        /// <summary>
        /// Check if the given peernumber corresponds to ours.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="peerNumber"></param>
        /// <returns></returns>
        public bool PeerNumberIsOurs(int groupNumber, int peerNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GroupPeerNumberIsOurs(_tox, groupNumber, peerNumber) == 1;
        }

        /// <summary>
        /// Retrieves the nospam value.
        /// </summary>
        /// <returns></returns>
        public uint GetNospam()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.GetNospam(_tox);
        }

        /// <summary>
        /// Sets the nospam value.
        /// </summary>
        /// <param name="nospam"></param>
        public void SetNospam(uint nospam)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            ToxFunctions.SetNospam(_tox, nospam);
        }

        /// <summary>
        /// Sends a lossy packet to the specified friendNumber.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SendLossyPacket(int friendNumber, byte[] data)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (data.Length > ToxConstants.MaxCustomPacketSize)
                throw new ArgumentException("Packet size is bigger than ToxConstants.MaxCustomPacketSize");

            if (data[0] < 200 || data[0] > 254)
                throw new ArgumentException("First byte of data is not in the 200-254 range.");

            return ToxFunctions.SendLossyPacket(_tox, friendNumber, data, (uint)data.Length) == 0;
        }

        /// <summary>
        /// Sends a lossless packet to the specified friendNumber.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SendLosslessPacket(int friendNumber, byte[] data)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (data.Length > ToxConstants.MaxCustomPacketSize)
                throw new ArgumentException("Packet size is bigger than ToxConstants.MaxCustomPacketSize");

            if (data[0] < 160 || data[0] > 191)
                throw new ArgumentException("First byte of data is not in the 160-191 range.");

            return ToxFunctions.SendLosslessPacket(_tox, friendNumber, data, (uint)data.Length) == 0;
        }

        /// <summary>
        /// Registers a handler for lossy packets starting with start_byte. These packets can be captured with <see cref="OnLossyPacket"/>.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="startByte"></param>
        /// <returns></returns>
        public bool RegisterLossyPacketHandler(int friendNumber, byte startByte)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (startByte < 200 || startByte > 254)
                throw new ArgumentException("start_byte is not in the 200-254 range.");

            ToxDelegates.CallbackPacketDelegate del = ((IntPtr obj, byte[] data, uint length) =>
            {
                if (OnLossyPacket != null)
                    Invoker(OnLossyPacket, this, new ToxEventArgs.CustomPacketEventArgs(friendNumber, data));

                return 1;

            });
            _lossyPacketHandlers.Add(del);

            return ToxFunctions.RegisterLossyPacketCallback(_tox, friendNumber, startByte, del, IntPtr.Zero) == 0;
        }

        /// <summary>
        /// Registers a handler for lossy packets starting with start_byte.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="startByte"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool RegisterLossyPacketHandler(int friendNumber, byte startByte, ToxDelegates.CallbackPacketDelegate callback)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (startByte < 200 || startByte > 254)
                throw new ArgumentException("start_byte is not in the 200-254 range.");

            return ToxFunctions.RegisterLossyPacketCallback(_tox, friendNumber, startByte, callback, IntPtr.Zero) == 0;
        }

        /// <summary>
        /// Registers a handler for lossless packets starting with start_byte. These packets can be captured with <see cref="OnLosslessPacket"/>.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="startByte"></param>
        /// <returns></returns>
        public bool RegisterLosslessPacketHandler(int friendNumber, byte startByte)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (startByte < 160 || startByte > 191)
                throw new ArgumentException("start_byte is not in the 160-191 range.");

            ToxDelegates.CallbackPacketDelegate del = ((IntPtr obj, byte[] data, uint length) =>
            {
                if (OnLosslessPacket != null)
                    Invoker(OnLosslessPacket, this, new ToxEventArgs.CustomPacketEventArgs(friendNumber, data));

                return 1;

            });
            _losslessPacketHandlers.Add(del);

            return ToxFunctions.RegisterLosslessPacketCallback(_tox, friendNumber, startByte, del, IntPtr.Zero) == 0;
        }

        /// <summary>
        /// Registers a handler for lossless packets starting with start_byte.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="startByte"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool RegisterLosslessPacketHandler(int friendNumber, byte startByte, ToxDelegates.CallbackPacketDelegate callback)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (startByte < 160 || startByte > 191)
                throw new ArgumentException("start_byte is not in the 160-191 range.");

            return ToxFunctions.RegisterLosslessPacketCallback(_tox, friendNumber, startByte, callback, IntPtr.Zero) == 0;
        }

        /// <summary>
        /// Retrieves a ToxData object that contains the data of this Tox instance.
        /// </summary>
        /// <returns></returns>
        public ToxData GetData()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] bytes = new byte[ToxFunctions.Size(_tox)];
            ToxFunctions.Save(_tox, bytes);

            return new ToxData(bytes);
        }

        /// <summary>
        /// Retrieves a ToxData object that contains the data of this Tox instance, encrypted with the given passphrase.
        /// </summary>
        /// <param name="passphrase"></param>
        /// <returns></returns>
        public ToxData GetData(string passphrase)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] bytes = new byte[ToxEncryptionFunctions.EncryptedSize(_tox)];
            byte[] phrase = Encoding.UTF8.GetBytes(passphrase);

            if (ToxEncryptionFunctions.EncryptedSave(_tox, bytes, phrase, (uint)phrase.Length) != 0)
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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.AddTcpRelay(_tox, node.Address, (ushort)node.Port, node.PublicKey.GetBytes()) == 1;
        }

        /// <summary>
        /// Loads Tox data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Load(ToxData data)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (data == null || data.IsEncrypted)
                return false;

            int result = ToxFunctions.Load(_tox, data.Bytes, (uint)data.Bytes.Length);

            return (result == 0 || result == -1);
        }

        /// <summary>
        /// Loads and decrypts Tox data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="passphrase"></param>
        /// <returns></returns>
        public bool Load(ToxData data, string passphrase)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] phrase = Encoding.UTF8.GetBytes(passphrase);

            if (data.IsEncrypted)
                return ToxEncryptionFunctions.EncryptedLoad(_tox, data.Bytes, (uint)data.Bytes.Length, phrase, (uint)phrase.Length) == 0;
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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.SetAvatar(_tox, (byte)format, data, (uint)data.Length) == 0;
        }

        /// <summary>
        /// Retrieves a cryptographic hash of the given data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] GetHash(byte[] data)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] hash = new byte[ToxConstants.ToxHashLength];

            if (ToxFunctions.Hash(hash, data, (uint)data.Length) != 0)
                return new byte[0];

            return hash;
        }

        /// <summary>
        /// Requests avatar info from a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public bool RequestAvatarInfo(int friendNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.RequestAvatarInfo(_tox, friendNumber) == 0;
        }

        /// <summary>
        /// Requests avatar data from a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public bool RequestAvatarData(int friendNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.RequestAvatarData(_tox, friendNumber) == 0;
        }

        /// <summary>
        /// Sends avatar info to a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public bool SendAvatarInfo(int friendNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.SendAvatarInfo(_tox, friendNumber) == 0;
        }

        /// <summary>
        /// Removes the avatar of this Tox instance.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use UnsetAvatar() instead")]
        public bool RemoveAvatar()
        {
            /*if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.SetAvatar(_tox, (byte)ToxAvatarFormat.None, null, 0) == 0;*/
            return UnsetAvatar();
        }

        /// <summary>
        /// Unsets the avatar of this Tox instance.
        /// </summary>
        /// <returns></returns>
        public bool UnsetAvatar()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ToxFunctions.UnsetAvatar(_tox) == 0;
        }

        /// <summary>
        /// Changes the title of a group.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public bool SetGroupTitle(int groupNumber, string title)
        {
            if (Encoding.UTF8.GetByteCount(title) > ToxConstants.MaxNameLength)
                throw new ArgumentException("The specified group title is longer than 256 bytes");

            byte[] bytes = Encoding.UTF8.GetBytes(title);

            return ToxFunctions.GroupSetTitle(_tox, groupNumber, bytes, (byte)bytes.Length) == 0;
        }

        #region Events
        private EventHandler<ToxEventArgs.FriendRequestEventArgs> _onFriendRequest;

        /// <summary>
        /// Occurs when a friend request is received.
        /// </summary>
        public event EventHandler<ToxEventArgs.FriendRequestEventArgs> OnFriendRequest
        {
            add
            {
                if (_onFriendRequestCallback == null)
                {
                    _onFriendRequestCallback = (IntPtr tox, byte[] id, byte[] message, ushort length, IntPtr userData) =>
                    {
                        if (_onFriendRequest != null)
                            Invoker(_onFriendRequest, this, new ToxEventArgs.FriendRequestEventArgs(ToxTools.RemoveNull(ToxTools.HexBinToString(id)), Encoding.UTF8.GetString(message, 0, length)));
                    };

                    ToxFunctions.RegisterFriendRequestCallback(_tox, _onFriendRequestCallback, IntPtr.Zero);
                }

                _onFriendRequest += value;
            }
            remove
            {
                _onFriendRequest -= value;
            }
        }

        private EventHandler<ToxEventArgs.ConnectionStatusEventArgs> _onConnectionStatusChanged;

        /// <summary>
        /// Occurs when the connection status of a friend has changed.
        /// </summary>
        public event EventHandler<ToxEventArgs.ConnectionStatusEventArgs> OnConnectionStatusChanged
        {
            add
            {
                if (_onConnectionStatusCallback == null)
                {
                    _onConnectionStatusCallback = (IntPtr tox, int friendNumber, byte status, IntPtr userData) =>
                    {
                        if (_onConnectionStatusChanged != null)
                            Invoker(_onConnectionStatusChanged, this, new ToxEventArgs.ConnectionStatusEventArgs(friendNumber, (ToxFriendConnectionStatus)status));
                    };

                    ToxFunctions.RegisterConnectionStatusCallback(_tox, _onConnectionStatusCallback, IntPtr.Zero);
                }

                _onConnectionStatusChanged += value;
            }
            remove
            {
                _onConnectionStatusChanged -= value;
            }
        }

        private EventHandler<ToxEventArgs.FriendMessageEventArgs> _onFriendMessage;

        /// <summary>
        /// Occurs when a message is received from a friend.
        /// </summary>
        public event EventHandler<ToxEventArgs.FriendMessageEventArgs> OnFriendMessage
        {
            add
            {
                if (_onFriendMessageCallback == null)
                {
                    _onFriendMessageCallback = (IntPtr tox, int friendNumber, byte[] message, ushort length, IntPtr userData) =>
                    {
                        if (_onFriendMessage != null)
                            Invoker(_onFriendMessage, this, new ToxEventArgs.FriendMessageEventArgs(friendNumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(message, 0, length))));
                    };

                    ToxFunctions.RegisterFriendMessageCallback(_tox, _onFriendMessageCallback, IntPtr.Zero);
                }

                _onFriendMessage += value;
            }
            remove
            {
                _onFriendMessage -= value;
            }
        }

        private EventHandler<ToxEventArgs.FriendActionEventArgs> _onFriendAction;

        /// <summary>
        /// Occurs when an action is received from a friend.
        /// </summary>
        public event EventHandler<ToxEventArgs.FriendActionEventArgs> OnFriendAction
        {
            add
            {
                if (_onFriendActionCallback == null)
                {
                    _onFriendActionCallback = (IntPtr tox, int friendNumber, byte[] action, ushort length, IntPtr userData) =>
                    {
                        if (_onFriendAction != null)
                            Invoker(_onFriendAction, this, new ToxEventArgs.FriendActionEventArgs(friendNumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(action, 0, length))));
                    };

                    ToxFunctions.RegisterFriendActionCallback(_tox, _onFriendActionCallback, IntPtr.Zero);
                }

                _onFriendAction += value;
            }
            remove
            {
                _onFriendAction -= value;
            }
        }

        private EventHandler<ToxEventArgs.NameChangeEventArgs> _onNameChange;

        /// <summary>
        /// Occurs when a friend has changed his/her name.
        /// </summary>
        public event EventHandler<ToxEventArgs.NameChangeEventArgs> OnNameChange
        {
            add
            {
                if (_onNameChangeCallback == null)
                {
                    _onNameChangeCallback = (IntPtr tox, int friendNumber, byte[] newName, ushort length, IntPtr userData) =>
                    {
                        if (_onNameChange != null)
                            Invoker(_onNameChange, this, new ToxEventArgs.NameChangeEventArgs(friendNumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(newName, 0, length))));
                    };

                    ToxFunctions.RegisterNameChangeCallback(_tox, _onNameChangeCallback, IntPtr.Zero);
                }

                _onNameChange += value;
            }
            remove
            {
                _onNameChange -= value;
            }
        }

        private EventHandler<ToxEventArgs.StatusMessageEventArgs> _onStatusMessage;

        /// <summary>
        /// Occurs when a friend has changed their status message.
        /// </summary>
        public event EventHandler<ToxEventArgs.StatusMessageEventArgs> OnStatusMessage
        {
            add
            {
                if (_onStatusMessageCallback == null)
                {
                    _onStatusMessageCallback = (IntPtr tox, int friendNumber, byte[] newStatus, ushort length, IntPtr userData) =>
                    {
                        if (_onStatusMessage != null)
                            Invoker(_onStatusMessage, this, new ToxEventArgs.StatusMessageEventArgs(friendNumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(newStatus, 0, length))));
                    };

                    ToxFunctions.RegisterStatusMessageCallback(_tox, _onStatusMessageCallback, IntPtr.Zero);
                }

                _onStatusMessage += value;
            }
            remove
            {
                _onStatusMessage -= value;
            }
        }

        private EventHandler<ToxEventArgs.UserStatusEventArgs> _onUserStatus;

        /// <summary>
        /// Occurs when a friend has changed their user status.
        /// </summary>
        public event EventHandler<ToxEventArgs.UserStatusEventArgs> OnUserStatus
        {
            add
            {
                if (_onUserStatusCallback == null)
                {
                    _onUserStatusCallback = (IntPtr tox, int friendNumber, ToxUserStatus status, IntPtr userData) =>
                    {
                        if (_onUserStatus != null)
                            Invoker(_onUserStatus, this, new ToxEventArgs.UserStatusEventArgs(friendNumber, status));
                    };

                    ToxFunctions.RegisterUserStatusCallback(_tox, _onUserStatusCallback, IntPtr.Zero);
                }

                _onUserStatus += value;
            }
            remove
            {
                _onUserStatus -= value;
            }
        }

        private EventHandler<ToxEventArgs.TypingStatusEventArgs> _onTypingChange;

        /// <summary>
        /// Occurs when a friend's typing status has changed.
        /// </summary>
        public event EventHandler<ToxEventArgs.TypingStatusEventArgs> OnTypingChange
        {
            add
            {
                if (_onTypingChangeCallback == null)
                {
                    _onTypingChangeCallback = (IntPtr tox, int friendNumber, byte typing, IntPtr userData) =>
                    {
                        bool isTyping = typing != 0;

                        if (_onTypingChange != null)
                            Invoker(_onTypingChange, this, new ToxEventArgs.TypingStatusEventArgs(friendNumber, isTyping));
                    };

                    ToxFunctions.RegisterTypingChangeCallback(_tox, _onTypingChangeCallback, IntPtr.Zero);
                }

                _onTypingChange += value;
            }
            remove
            {
                _onTypingChange -= value;
            }
        }

        private EventHandler<ToxEventArgs.GroupActionEventArgs> _onGroupAction;

        /// <summary>
        /// Occurs when an action is received from a group.
        /// </summary>
        public event EventHandler<ToxEventArgs.GroupActionEventArgs> OnGroupAction
        {
            add
            {
                if (_onGroupActionCallback == null)
                {
                    _onGroupActionCallback = (IntPtr tox, int groupNumber, int peerNumber, byte[] action, ushort length, IntPtr userData) =>
                    {
                        if (_onGroupAction != null)
                            Invoker(_onGroupAction, this, new ToxEventArgs.GroupActionEventArgs(groupNumber, peerNumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(action, 0, length))));
                    };

                    ToxFunctions.RegisterGroupActionCallback(_tox, _onGroupActionCallback, IntPtr.Zero);
                }

                _onGroupAction += value;
            }
            remove
            {
                _onGroupAction -= value;
            }
        }

        private EventHandler<ToxEventArgs.GroupMessageEventArgs> _onGroupMessage;

        /// <summary>
        /// Occurs when a message is received from a group.
        /// </summary>
        public event EventHandler<ToxEventArgs.GroupMessageEventArgs> OnGroupMessage
        {
            add
            {
                if (_onGroupMessageCallback == null)
                {
                    _onGroupMessageCallback = (IntPtr tox, int groupNumber, int peerNumber, byte[] message, ushort length, IntPtr userData) =>
                    {
                        if (_onGroupMessage != null)
                            Invoker(_onGroupMessage, this, new ToxEventArgs.GroupMessageEventArgs(groupNumber, peerNumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(message, 0, length))));
                    };

                    ToxFunctions.RegisterGroupMessageCallback(_tox, _onGroupMessageCallback, IntPtr.Zero);
                }

                _onGroupMessage += value;
            }
            remove
            {
                _onGroupMessage -= value;
            }
        }

        private EventHandler<ToxEventArgs.GroupInviteEventArgs> _onGroupInvite;

        /// <summary>
        /// Occurs when a friend has sent an invite to a group.
        /// </summary>
        public event EventHandler<ToxEventArgs.GroupInviteEventArgs> OnGroupInvite
        {
            add
            {
                if (_onGroupInviteCallback == null)
                {
                    _onGroupInviteCallback = (IntPtr tox, int friendNumber, byte[] data, ushort length, IntPtr userData) =>
                    {
                        if (_onGroupInvite != null)
                            Invoker(_onGroupInvite, this, new ToxEventArgs.GroupInviteEventArgs(friendNumber, data));
                    };

                    ToxFunctions.RegisterGroupInviteCallback(_tox, _onGroupInviteCallback, IntPtr.Zero);
                }

                _onGroupInvite += value;
            }
            remove
            {
                _onGroupInvite -= value;
            }
        }

        private EventHandler<ToxEventArgs.GroupNamelistChangeEventArgs> _onGroupNamelistChange;

        /// <summary>
        /// Occurs when the name list of a group has changed.
        /// </summary>
        public event EventHandler<ToxEventArgs.GroupNamelistChangeEventArgs> OnGroupNamelistChange
        {
            add
            {
                if (_onGroupNamelistChangeCallback == null)
                {
                    _onGroupNamelistChangeCallback = (IntPtr tox, int groupNumber, int peerNumber, ToxChatChange change, IntPtr userData) =>
                    {
                        if (_onGroupNamelistChange != null)
                            Invoker(_onGroupNamelistChange, this, new ToxEventArgs.GroupNamelistChangeEventArgs(groupNumber, peerNumber, change));
                    };

                    ToxFunctions.RegisterGroupNamelistChangeCallback(_tox, _onGroupNamelistChangeCallback, IntPtr.Zero);
                }

                _onGroupNamelistChange += value;
            }
            remove
            {
                _onGroupNamelistChange -= value;
            }
        }

        private EventHandler<ToxEventArgs.FileControlEventArgs> _onFileControl;

        /// <summary>
        /// Occurs when a file control request is received.
        /// </summary>
        public event EventHandler<ToxEventArgs.FileControlEventArgs> OnFileControl
        {
            add
            {
                if (_onFileControlCallback == null)
                {
                    _onFileControlCallback = (IntPtr tox, int friendNumber, byte receiveSend, byte fileNumber, byte controlYype, byte[] data, ushort length, IntPtr userData) =>
                    {
                        if (_onFileControl != null)
                            Invoker(_onFileControl, this, new ToxEventArgs.FileControlEventArgs(friendNumber, fileNumber, receiveSend == 1, (ToxFileControl)controlYype, data));
                    };

                    ToxFunctions.RegisterFileControlCallback(_tox, _onFileControlCallback, IntPtr.Zero);
                }

                _onFileControl += value;
            }
            remove
            {
                _onFileControl -= value;
            }
        }

        private EventHandler<ToxEventArgs.FileDataEventArgs> _onFileData;

        /// <summary>
        /// Occurs when file data is received.
        /// </summary>
        public event EventHandler<ToxEventArgs.FileDataEventArgs> OnFileData
        {
            add
            {
                if (_onFileDataCallback == null)
                {
                    _onFileDataCallback = (IntPtr tox, int friendNumber, byte fileNumber, byte[] data, ushort length, IntPtr userData) =>
                    {
                        if (_onFileData != null)
                            Invoker(_onFileData, this, new ToxEventArgs.FileDataEventArgs(friendNumber, fileNumber, data));
                    };

                    ToxFunctions.RegisterFileDataCallback(_tox, _onFileDataCallback, IntPtr.Zero);
                }

                _onFileData += value;
            }
            remove
            {
                _onFileData -= value;
            }
        }

        private EventHandler<ToxEventArgs.FileSendRequestEventArgs> _onFileSendRequest;

        /// <summary>
        /// Occurs when a file send request is received.
        /// </summary>
        public event EventHandler<ToxEventArgs.FileSendRequestEventArgs> OnFileSendRequest
        {
            add
            {
                if (_onFileSendRequestCallback == null)
                {
                    _onFileSendRequestCallback = (IntPtr tox, int friendNumber, byte fileNumber, ulong fileSize, byte[] filename, ushort filenameLength, IntPtr userData) =>
                    {
                        if (_onFileSendRequest != null)
                            Invoker(_onFileSendRequest, this, new ToxEventArgs.FileSendRequestEventArgs(friendNumber, fileNumber, fileSize, ToxTools.RemoveNull(Encoding.UTF8.GetString(filename, 0, filenameLength))));
                    };

                    ToxFunctions.RegisterFileSendRequestCallback(_tox, _onFileSendRequestCallback, IntPtr.Zero);
                }

                _onFileSendRequest += value;
            }
            remove
            {
                _onFileSendRequest -= value;
            }
        }

        private EventHandler<ToxEventArgs.ReadReceiptEventArgs> _onReadReceipt;

        /// <summary>
        /// Occurs when a read receipt is received.
        /// </summary>
        public event EventHandler<ToxEventArgs.ReadReceiptEventArgs> OnReadReceipt
        {
            add
            {
                if (_onReadReceiptCallback == null)
                {
                    _onReadReceiptCallback = (IntPtr tox, int friendNumber, uint receipt, IntPtr userData) =>
                    {
                        if (_onReadReceipt != null)
                            Invoker(_onReadReceipt, this, new ToxEventArgs.ReadReceiptEventArgs(friendNumber, (int)receipt));
                    };

                    ToxFunctions.RegisterReadReceiptCallback(_tox, _onReadReceiptCallback, IntPtr.Zero);
                }

                _onReadReceipt += value;
            }
            remove
            {
                _onReadReceipt -= value;
            }
        }

        private EventHandler<ToxEventArgs.AvatarInfoEventArgs> _onAvatarInfo;

        /// <summary>
        /// Occurs when avatar info is received.
        /// </summary>
        public event EventHandler<ToxEventArgs.AvatarInfoEventArgs> OnAvatarInfo
        {
            add
            {
                if (_onAvatarInfoCallback == null)
                {
                    _onAvatarInfoCallback = (IntPtr tox, int friendNumber, byte format, byte[] hash, IntPtr userData) =>
                    {
                        if (_onAvatarInfo != null)
                            Invoker(_onAvatarInfo, this, new ToxEventArgs.AvatarInfoEventArgs(friendNumber, (ToxAvatarFormat)format, hash));
                    };

                    ToxFunctions.RegisterAvatarInfoCallback(_tox, _onAvatarInfoCallback, IntPtr.Zero);
                }

                _onAvatarInfo += value;
            }
            remove
            {
                _onAvatarInfo -= value;
            }
        }

        private EventHandler<ToxEventArgs.AvatarDataEventArgs> _onAvatarData;

        /// <summary>
        /// Occurs when avatar data is received.
        /// </summary>
        public event EventHandler<ToxEventArgs.AvatarDataEventArgs> OnAvatarData
        {
            add
            {
                if (_onAvatarDataCallback == null)
                {
                    _onAvatarDataCallback = (IntPtr tox, int friendNumber, byte format, byte[] hash, byte[] data, uint dataLength, IntPtr userData) =>
                    {
                        if (_onAvatarData != null)
                            Invoker(_onAvatarData, this, new ToxEventArgs.AvatarDataEventArgs(friendNumber, new ToxAvatar((ToxAvatarFormat)format, (byte[])data.Clone(), hash)));
                    };

                    ToxFunctions.RegisterAvatarDataCallback(_tox, _onAvatarDataCallback, IntPtr.Zero);
                }

                _onAvatarData += value;
            }
            remove
            {
                _onAvatarData -= value;
            }
        }

        private EventHandler<ToxEventArgs.GroupTitleEventArgs> _onGroupTitleChanged;

        /// <summary>
        /// Occurs when the title of a groupchat is changed.
        /// </summary>
        public event EventHandler<ToxEventArgs.GroupTitleEventArgs> OnGroupTitleChanged
        {
            add
            {
                if (_onGroupTitleCallback == null)
                {
                    _onGroupTitleCallback = (IntPtr tox, int groupNumber, int peerNumber, byte[] title, byte length, IntPtr userData) =>
                    {
                        if (_onGroupTitleChanged != null)
                            Invoker(_onGroupTitleChanged, this, new ToxEventArgs.GroupTitleEventArgs(groupNumber, peerNumber, Encoding.UTF8.GetString(title, 0, length)));
                    };

                    ToxFunctions.RegisterGroupTitleCallback(_tox, _onGroupTitleCallback, IntPtr.Zero);
                }

                _onGroupTitleChanged += value;
            }
            remove
            {
                _onGroupTitleChanged -= value;
            }
        }

        /// <summary>
        /// Occurs when a lossy packet is received.
        /// </summary>
        public event EventHandler<ToxEventArgs.CustomPacketEventArgs> OnLossyPacket;

        /// <summary>
        /// Occurs when a lossless packet is received.
        /// </summary>
        public event EventHandler<ToxEventArgs.CustomPacketEventArgs> OnLosslessPacket;

        /// <summary>
        /// Occurs when a connection to the DHT has been established.
        /// </summary>
        public event EventHandler<ToxEventArgs.ConnectionEventArgs> OnConnected;

        /// <summary>
        /// Occurs when the connection to the DHT was lost.
        /// </summary>
        public event EventHandler<ToxEventArgs.ConnectionEventArgs> OnDisconnected;
        #endregion
    }
}

#pragma warning restore 1591