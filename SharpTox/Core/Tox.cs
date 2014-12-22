#pragma warning disable 1591

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using SharpTox.Encryption;

namespace SharpTox.Core
{
    public delegate object InvokeDelegate(Delegate method, params object[] p);

    /// <summary>
    /// Represents an instance of Tox.
    /// </summary>
    public class Tox : IDisposable
    {
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
        private CancellationTokenSource _cancelTokenSource;

        private bool _running = false;
        private bool _disposed = false;
        private bool _connected = false;

        private List<ToxDelegates.CallbackPacketDelegate> _lossyPacketHandlers = new List<ToxDelegates.CallbackPacketDelegate>();
        private List<ToxDelegates.CallbackPacketDelegate> _losslessPacketHandlers = new List<ToxDelegates.CallbackPacketDelegate>();

        private Dictionary<int, ToxFriend> _friends = new Dictionary<int, ToxFriend>();

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
                CheckDisposed();

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
                CheckDisposed();

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
                CheckDisposed();

                return (int)ToxFunctions.CountFriendlist(_tox);
            }
        }

        /// <summary>
        /// An array of friendnumbers of this Tox instance.
        /// </summary>
        private int[] _friendList
        {
            get
            {
                CheckDisposed();

                uint count = ToxFunctions.CountFriendlist(_tox);
                int[] friends = new int[count];
                uint[] trunc = new uint[0];
                uint result = ToxFunctions.GetFriendlist(_tox, friends, trunc);

                if (result == 0)
                    return new int[0];

                return friends;
            }
        }

        public ToxFriend[] Friends
        {
            get
            {
                CheckDisposed();

                return _friendList.Select(FriendFromFriendNumber).ToArray();
            }
        }

        /// <summary>
        /// The nickname of this Tox instance.
        /// </summary>
        public string Name
        {
            get
            {
                CheckDisposed();

                byte[] bytes = new byte[ToxConstants.MaxNameLength];
                ToxFunctions.GetSelfName(_tox, bytes);

                return ToxTools.GetString(bytes);
            }
            set
            {
                CheckDisposed();

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
                CheckDisposed();

                byte[] publicKey = new byte[ToxConstants.ClientIdSize];
                byte[] secretKey = new byte[ToxConstants.ClientIdSize];

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
        public ToxId Id
        {
            get
            {
                CheckDisposed();

                byte[] address = new byte[38];
                ToxFunctions.GetAddress(_tox, address);

                return new ToxId(address);
            }
        }

        /// <summary>
        /// The status message of this Tox instance.
        /// </summary>
        public string StatusMessage
        {
            get
            {
                CheckDisposed();

                int size = ToxFunctions.GetSelfStatusMessageSize(_tox);
                byte[] status = new byte[size];

                ToxFunctions.GetSelfStatusMessage(_tox, status, status.Length);

                return ToxTools.GetString(status);
            }
            set
            {
                CheckDisposed();

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
                CheckDisposed();
                return (ToxUserStatus)ToxFunctions.GetSelfUserStatus(_tox);
            }
            set
            {
                CheckDisposed();
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
                CheckDisposed();

                int[] chats = new int[ToxFunctions.CountChatlist(_tox)];
                uint[] trunc = new uint[0];
                uint result = ToxFunctions.GetChatlist(_tox, chats, trunc);

                if (result == 0)
                    return new int[0];
                else
                    return chats;
            }
        }

        /// <summary>
        /// The handle of this instance of Tox.
        /// </summary>
        public ToxHandle Handle
        {
            get
            {
                return _tox;
            }
        }

        /// <summary>
        /// Initializes a new instance of Tox with default options.
        /// </summary>
        public Tox()
            : this(new ToxOptions(true, false))
        {
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

            ClearEventSubscriptions();

            if (!_tox.IsInvalid && !_tox.IsClosed && _tox != null)
                _tox.Dispose();

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

        /// <summary>
        /// Starts the main tox_do loop.
        /// </summary>
        public void Start()
        {
            CheckDisposed();

            if (_running)
                return;

            Loop();
        }

        /// <summary>
        /// Stops the main tox_do loop if it's running.
        /// </summary>
        public void Stop()
        {
            CheckDisposed();

            if (!_running)
                return;

            if (_cancelTokenSource != null)
            {
                _cancelTokenSource.Cancel();
                _cancelTokenSource.Dispose();

                _running = false;
            }
        }

        /// <summary>
        /// Runs the loop once in the current thread and returns the next timeout.
        /// </summary>
        public int Iterate()
        {
            CheckDisposed();

            if (_running)
                throw new Exception("Loop already running");

            return DoIterate();
        }

        private int DoIterate()
        {
            ToxFunctions.Do(_tox);
            return (int)ToxFunctions.DoInterval(_tox);
        }

        private void Loop()
        {
            _cancelTokenSource = new CancellationTokenSource();
            _running = true;

            Task.Factory.StartNew(() =>
            {
                while (_running)
                {
                    if (_cancelTokenSource.IsCancellationRequested)
                        break;

                    if (IsConnected && !_connected)
                    {
                        if (OnConnected != null)
                            OnConnected(this, new ToxEventArgs.ConnectionEventArgs(true));

                        _connected = true;
                    }
                    else if (!IsConnected && _connected)
                    {
                        if (OnDisconnected != null)
                            OnDisconnected(this, new ToxEventArgs.ConnectionEventArgs(false));

                        _connected = false;
                    }

                    int delay = DoIterate();

#if IS_PORTABLE
                    Task.Delay(delay);
#else
                    Thread.Sleep(delay);
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
        public int AddFriend(ToxId id, string message)
        {
            CheckDisposed();

            byte[] msg = Encoding.UTF8.GetBytes(message);
            int result = ToxFunctions.AddFriend(_tox, id.Bytes, msg, (ushort)msg.Length);

            if (result < 0)
                throw new ToxAFException((ToxAFError)result);

            return result;
        }

        /// <summary>
        /// Adds a friend without sending a request.
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns>friendNumber</returns>
        public int AddFriendNoRequest(ToxKey publicKey)
        {
            CheckDisposed();
            return ToxFunctions.AddFriendNoRequest(_tox, publicKey.GetBytes());
        }

        /// <summary>
        /// Bootstraps this Tox instance with a ToxNode.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool BootstrapFromNode(ToxNode node)
        {
            CheckDisposed();
            return ToxFunctions.BootstrapFromAddress(_tox, node.Address, (ushort)node.Port, node.PublicKey.GetBytes()) == 1;
        }

        /// <summary>
        /// Checks if there exists a friend with given friendNumber.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public bool FriendExists(int friendNumber)
        {
            CheckDisposed();
            return ToxFunctions.FriendExists(_tox, friendNumber) != 0;
        }

        /// <summary>
        /// Retrieves the friendNumber associated to the specified public address/id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetFriendNumber(string id)
        {
            CheckDisposed();
            return ToxFunctions.GetFriendNumber(_tox, ToxTools.StringToHexBin(id));
        }

        /// <summary>
        /// Retrieves the amount of friends who are currently online.
        /// </summary>
        /// <returns></returns>
        public int GetOnlineFriendsCount()
        {
            CheckDisposed();
            return (int)ToxFunctions.GetNumOnlineFriends(_tox);
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
        /// Retrieves the name of a group member.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="peerNumber"></param>
        /// <returns></returns>
        public string GetGroupMemberName(int groupNumber, int peerNumber)
        {
            CheckDisposed();

            byte[] name = new byte[ToxConstants.MaxNameLength];
            if (ToxFunctions.GroupPeername(_tox, groupNumber, peerNumber, name) == -1)
                throw new Exception("Could not get peer name");

            return ToxTools.GetString(name);
        }

        /// <summary>
        /// Creates a new group.
        /// </summary>
        /// <returns></returns>
        public ToxGroup CreateGroup()
        {
            CheckDisposed();

            return new ToxGroup(this);
        }

        /// <summary>
        /// Retrieves the nospam value.
        /// </summary>
        /// <returns></returns>
        public uint GetNospam()
        {
            CheckDisposed();

            return ToxFunctions.GetNospam(_tox);
        }

        /// <summary>
        /// Sets the nospam value.
        /// </summary>
        /// <param name="nospam"></param>
        public void SetNospam(uint nospam)
        {
            CheckDisposed();

            ToxFunctions.SetNospam(_tox, nospam);
        }

        /// <summary>
        /// Registers a handler for lossy packets starting with start_byte. These packets can be captured with <see cref="OnLossyPacket"/>.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="startByte"></param>
        /// <returns></returns>
        public bool RegisterLossyPacketHandler(int friendNumber, byte startByte)
        {
            CheckDisposed();

            if (startByte < 200 || startByte > 254)
                throw new ArgumentException("start_byte is not in the 200-254 range.");

            ToxDelegates.CallbackPacketDelegate del = ((IntPtr tox, int friendNum, byte[] data, uint length, IntPtr obj) =>
            {
                if (OnLossyPacket != null)
                    OnLossyPacket(this, new ToxEventArgs.CustomPacketEventArgs(FriendFromFriendNumber(friendNum), data));

                return 1;

            });
            _lossyPacketHandlers.Add(del);

            return ToxFunctions.RegisterLossyPacketCallback(_tox, friendNumber, startByte, del, IntPtr.Zero) == 0;
        }

        /// <summary>
        /// Registers a handler for lossless packets starting with start_byte. These packets can be captured with <see cref="OnLosslessPacket"/>.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="startByte"></param>
        /// <returns></returns>
        public bool RegisterLosslessPacketHandler(int friendNumber, byte startByte)
        {
            CheckDisposed();

            if (startByte < 160 || startByte > 191)
                throw new ArgumentException("start_byte is not in the 160-191 range.");

            ToxDelegates.CallbackPacketDelegate del = ((IntPtr tox, int friendNum, byte[] data, uint length, IntPtr obj) =>
            {
                if (OnLosslessPacket != null)
                    OnLosslessPacket(this, new ToxEventArgs.CustomPacketEventArgs(FriendFromFriendNumber(friendNum), data));

                return 1;

            });
            _losslessPacketHandlers.Add(del);

            return ToxFunctions.RegisterLosslessPacketCallback(_tox, friendNumber, startByte, del, IntPtr.Zero) == 0;
        }

        /// <summary>
        /// Retrieves a ToxData object that contains the data of this Tox instance.
        /// </summary>
        /// <returns></returns>
        public ToxData GetData()
        {
            CheckDisposed();

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
            CheckDisposed();

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
            CheckDisposed();
            return ToxFunctions.AddTcpRelay(_tox, node.Address, (ushort)node.Port, node.PublicKey.GetBytes()) == 1;
        }

        /// <summary>
        /// Loads Tox data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Load(ToxData data)
        {
            CheckDisposed();

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
            CheckDisposed();

            if (!data.IsEncrypted)
                return Load(data);

            byte[] phrase = Encoding.UTF8.GetBytes(passphrase);
            return ToxEncryptionFunctions.EncryptedLoad(_tox, data.Bytes, (uint)data.Bytes.Length, phrase, (uint)phrase.Length) == 0;
        }

        /// <summary>
        /// Sets the avatar of this Tox instance.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SetAvatar(ToxAvatarFormat format, byte[] data)
        {
            CheckDisposed();
            return ToxFunctions.SetAvatar(_tox, (byte)format, data, (uint)data.Length) == 0;
        }

        /// <summary>
        /// Retrieves a cryptographic hash of the given data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] GetHash(byte[] data)
        {
            CheckDisposed();

            byte[] hash = new byte[ToxConstants.ToxHashLength];

            if (ToxFunctions.Hash(hash, data, (uint)data.Length) != 0)
                return new byte[0];

            return hash;
        }

        /// <summary>
        /// Unsets the avatar of this Tox instance.
        /// </summary>
        /// <returns></returns>
        public bool UnsetAvatar()
        {
            CheckDisposed();
            return ToxFunctions.UnsetAvatar(_tox) == 0;
        }

        /// <summary>
        /// Retrieves the public key of a peer.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="peerNumber"></param>
        /// <returns></returns>
        public ToxKey GetGroupPeerPublicKey(int groupNumber, int peerNumber)
        {
            CheckDisposed();

            byte[] key = new byte[ToxConstants.ClientIdSize];
            int result = ToxFunctions.GroupPeerPubkey(_tox, groupNumber, peerNumber, key);

            if (result != 0)
                return null;

            return new ToxKey(ToxKeyType.Public, key);
        }

        private ToxFriend FriendFromFriendNumber(int friendNumber)
        {
            ToxFriend friend;
            if (_friends.TryGetValue(friendNumber, out friend))
            {
                return friend;
            }

            friend = new ToxFriend(this, friendNumber);

            _friends[friendNumber] = friend;

            return friend;
        }

        internal void DeleteFriend(ToxFriend friend)
        {
            _friends.Remove(friend.Number);
        }

        internal void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        Dictionary<int, ToxGroup> _groups = new Dictionary<int, ToxGroup>();
        internal ToxGroup GroupFromGroupNumber(int groupNumber)
        {
            ToxGroup group;
            if (_groups.TryGetValue(groupNumber, out group))
            {
                return group;
            }

            group = new ToxGroup(this, groupNumber);

            _groups[groupNumber] = group;

            return group;
        }

        internal void DeleteGroup(ToxGroup group)
        {
            _groups.Remove(group.Number);
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
                    _onFriendRequestCallback = (IntPtr tox, byte[] publicKey, byte[] message, ushort length, IntPtr userData) =>
                    {
                        var e = new ToxEventArgs.FriendRequestEventArgs(ToxTools.RemoveNull(ToxTools.HexBinToString(publicKey)), Encoding.UTF8.GetString(message, 0, length));
                        _onFriendRequest(this, e);
                    };

                    ToxFunctions.RegisterFriendRequestCallback(_tox, _onFriendRequestCallback, IntPtr.Zero);
                }

                _onFriendRequest += value;
            }
            remove
            {
                if (_onFriendRequest.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFriendRequestCallback(_tox, null, IntPtr.Zero);
                    _onFriendRequestCallback = null;
                }

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
                        var e = new ToxEventArgs.ConnectionStatusEventArgs(FriendFromFriendNumber(friendNumber), (ToxFriendConnectionStatus)status);
                        _onConnectionStatusChanged(this, e);
                    };

                    ToxFunctions.RegisterConnectionStatusCallback(_tox, _onConnectionStatusCallback, IntPtr.Zero);
                }

                _onConnectionStatusChanged += value;
            }
            remove
            {
                if (_onConnectionStatusChanged.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterConnectionStatusCallback(_tox, null, IntPtr.Zero);
                    _onConnectionStatusCallback = null;
                }

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
                        var e = new ToxEventArgs.FriendMessageEventArgs(FriendFromFriendNumber(friendNumber), ToxTools.GetString(message));
                        _onFriendMessage(this, e);
                    };

                    ToxFunctions.RegisterFriendMessageCallback(_tox, _onFriendMessageCallback, IntPtr.Zero);
                }

                _onFriendMessage += value;
            }
            remove
            {
                if (_onFriendMessage.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFriendMessageCallback(_tox, null, IntPtr.Zero);
                    _onFriendMessageCallback = null;
                }

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
                        var e = new ToxEventArgs.FriendActionEventArgs(FriendFromFriendNumber(friendNumber), ToxTools.GetString(action));
                        _onFriendAction(this, e);
                    };

                    ToxFunctions.RegisterFriendActionCallback(_tox, _onFriendActionCallback, IntPtr.Zero);
                }

                _onFriendAction += value;
            }
            remove
            {
                if (_onFriendAction.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFriendActionCallback(_tox, null, IntPtr.Zero);
                    _onFriendActionCallback = null;
                }

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
                        var e = new ToxEventArgs.NameChangeEventArgs(FriendFromFriendNumber(friendNumber), ToxTools.GetString(newName));
                        _onNameChange(this, e);
                    };

                    ToxFunctions.RegisterNameChangeCallback(_tox, _onNameChangeCallback, IntPtr.Zero);
                }

                _onNameChange += value;
            }
            remove
            {
                if (_onNameChange.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterNameChangeCallback(_tox, null, IntPtr.Zero);
                    _onNameChangeCallback = null;
                }

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
                        var e = new ToxEventArgs.StatusMessageEventArgs(FriendFromFriendNumber(friendNumber), ToxTools.GetString(newStatus));
                        _onStatusMessage(this, e);
                    };

                    ToxFunctions.RegisterStatusMessageCallback(_tox, _onStatusMessageCallback, IntPtr.Zero);
                }

                _onStatusMessage += value;
            }
            remove
            {
                if (_onStatusMessage.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterStatusMessageCallback(_tox, null, IntPtr.Zero);
                    _onStatusMessageCallback = null;
                }

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
                        var e = new ToxEventArgs.UserStatusEventArgs(FriendFromFriendNumber(friendNumber), status);
                        _onUserStatus(this, e);
                    };

                    ToxFunctions.RegisterUserStatusCallback(_tox, _onUserStatusCallback, IntPtr.Zero);
                }

                _onUserStatus += value;
            }
            remove
            {
                if (_onUserStatus.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterUserStatusCallback(_tox, null, IntPtr.Zero);
                    _onUserStatusCallback = null;
                }

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

                        var e = new ToxEventArgs.TypingStatusEventArgs(FriendFromFriendNumber(friendNumber), isTyping);
                       _onTypingChange(this, e);
                    };

                    ToxFunctions.RegisterTypingChangeCallback(_tox, _onTypingChangeCallback, IntPtr.Zero);
                }

                _onTypingChange += value;
            }
            remove
            {
                if (_onTypingChange.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterTypingChangeCallback(_tox, null, IntPtr.Zero);
                    _onTypingChangeCallback = null;
                }

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
                        var group = GroupFromGroupNumber(groupNumber);
                        var peer = group.PeerFromPeerNumber(peerNumber);
                        var e = new ToxEventArgs.GroupActionEventArgs(group, peer, ToxTools.GetString(action));
                        _onGroupAction(this, e);
                    };

                    ToxFunctions.RegisterGroupActionCallback(_tox, _onGroupActionCallback, IntPtr.Zero);
                }

                _onGroupAction += value;
            }
            remove
            {
                if (_onGroupAction.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterGroupActionCallback(_tox, null, IntPtr.Zero);
                    _onGroupActionCallback = null;
                }

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
                        var group = GroupFromGroupNumber(groupNumber);
                        var peer = group.PeerFromPeerNumber(peerNumber);
                        var e = new ToxEventArgs.GroupMessageEventArgs(group, peer, ToxTools.GetString(message));
                        _onGroupMessage(this, e);
                    };

                    ToxFunctions.RegisterGroupMessageCallback(_tox, _onGroupMessageCallback, IntPtr.Zero);
                }

                _onGroupMessage += value;
            }
            remove
            {
                if (_onGroupMessage.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterGroupMessageCallback(_tox, null, IntPtr.Zero);
                    _onGroupMessageCallback = null;
                }

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
                    _onGroupInviteCallback = (IntPtr tox, int friendNumber, byte type, byte[] data, ushort length, IntPtr userData) =>
                    {
                        var e = new ToxEventArgs.GroupInviteEventArgs(new ToxGroupInvite(FriendFromFriendNumber(friendNumber), (ToxGroupType)type, data));
                        _onGroupInvite(this, e);
                    };

                    ToxFunctions.RegisterGroupInviteCallback(_tox, _onGroupInviteCallback, IntPtr.Zero);
                }

                _onGroupInvite += value;
            }
            remove
            {
                if (_onGroupInvite.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterGroupInviteCallback(_tox, null, IntPtr.Zero);
                    _onGroupInviteCallback = null;
                }

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
                        var group = GroupFromGroupNumber(groupNumber);
                        var peer = group.PeerFromPeerNumber(peerNumber);
                        group.Change(peer, change);
                        var e = new ToxEventArgs.GroupNamelistChangeEventArgs(group, peer, change);

                        _onGroupNamelistChange(this, e);
                    };

                    ToxFunctions.RegisterGroupNamelistChangeCallback(_tox, _onGroupNamelistChangeCallback, IntPtr.Zero);
                }

                _onGroupNamelistChange += value;
            }
            remove
            {
                if (_onGroupNamelistChange.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterGroupNamelistChangeCallback(_tox, null, IntPtr.Zero);
                    _onGroupNamelistChangeCallback = null;
                }

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
                        var e = new ToxEventArgs.FileControlEventArgs(FriendFromFriendNumber(friendNumber), fileNumber, receiveSend == 1, (ToxFileControl)controlYype, data);
                        _onFileControl(this, e);
                    };

                    ToxFunctions.RegisterFileControlCallback(_tox, _onFileControlCallback, IntPtr.Zero);
                }

                _onFileControl += value;
            }
            remove
            {
                if (_onFileControl.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFileControlCallback(_tox, null, IntPtr.Zero);
                    _onFileControlCallback = null;
                }

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
                        var e = new ToxEventArgs.FileDataEventArgs(FriendFromFriendNumber(friendNumber), fileNumber, data);
                        _onFileData(this, e);
                    };

                    ToxFunctions.RegisterFileDataCallback(_tox, _onFileDataCallback, IntPtr.Zero);
                }

                _onFileData += value;
            }
            remove
            {
                if (_onFileData.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFileDataCallback(_tox, null, IntPtr.Zero);
                    _onFileDataCallback = null;
                }

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
                        var e = new ToxEventArgs.FileSendRequestEventArgs(FriendFromFriendNumber(friendNumber), fileNumber, fileSize, ToxTools.GetString(filename));
                        _onFileSendRequest(this, e);
                    };

                    ToxFunctions.RegisterFileSendRequestCallback(_tox, _onFileSendRequestCallback, IntPtr.Zero);
                }

                _onFileSendRequest += value;
            }
            remove
            {
                if (_onFileSendRequest.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFileSendRequestCallback(_tox, null, IntPtr.Zero);
                    _onFileSendRequestCallback = null;
                }

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
                        var e = new ToxEventArgs.ReadReceiptEventArgs(FriendFromFriendNumber(friendNumber), (int)receipt);
                        _onReadReceipt(this, e);
                    };

                    ToxFunctions.RegisterReadReceiptCallback(_tox, _onReadReceiptCallback, IntPtr.Zero);
                }

                _onReadReceipt += value;
            }
            remove
            {
                if (_onReadReceipt.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterReadReceiptCallback(_tox, null, IntPtr.Zero);
                    _onReadReceiptCallback = null;
                }

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
                        var e = new ToxEventArgs.AvatarInfoEventArgs(FriendFromFriendNumber(friendNumber), (ToxAvatarFormat)format, hash);
                        _onAvatarInfo(this, e);
                    };

                    ToxFunctions.RegisterAvatarInfoCallback(_tox, _onAvatarInfoCallback, IntPtr.Zero);
                }

                _onAvatarInfo += value;
            }
            remove
            {
                if (_onAvatarInfo.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterAvatarInfoCallback(_tox, null, IntPtr.Zero);
                    _onAvatarInfoCallback = null;
                }

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
                       var e = new ToxEventArgs.AvatarDataEventArgs(FriendFromFriendNumber(friendNumber), new ToxAvatar((ToxAvatarFormat)format, (byte[])data.Clone(), hash));
                       _onAvatarData(this, e);
                    };

                    ToxFunctions.RegisterAvatarDataCallback(_tox, _onAvatarDataCallback, IntPtr.Zero);
                }

                _onAvatarData += value;
            }
            remove
            {
                if (_onAvatarData.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterAvatarDataCallback(_tox, null, IntPtr.Zero);
                    _onAvatarDataCallback = null;
                }

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
                        var group = GroupFromGroupNumber(groupNumber);
                        var peer = group.PeerFromPeerNumber(peerNumber);
                        var e = new ToxEventArgs.GroupTitleEventArgs(group, peer, Encoding.UTF8.GetString(title, 0, length));
                        _onGroupTitleChanged(this, e);
                    };

                    ToxFunctions.RegisterGroupTitleCallback(_tox, _onGroupTitleCallback, IntPtr.Zero);
                }

                _onGroupTitleChanged += value;
            }
            remove
            {
                if (_onGroupTitleChanged.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterGroupTitleCallback(_tox, null, IntPtr.Zero);
                    _onGroupTitleCallback = null;
                }

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
