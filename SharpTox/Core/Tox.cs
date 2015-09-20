using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpTox.Encryption;

namespace SharpTox.Core
{
    /// <summary>
    /// Represents an instance of Tox.
    /// </summary>
    public class Tox : IDisposable
    {
        private ToxHandle _tox;
        private CancellationTokenSource _cancelTokenSource;

        private bool _running = false;
        private bool _disposed = false;

        #region Callback delegates
        private ToxDelegates.CallbackFriendRequestDelegate _onFriendRequestCallback;
        private ToxDelegates.CallbackFriendMessageDelegate _onFriendMessageCallback;
        private ToxDelegates.CallbackNameChangeDelegate _onNameChangeCallback;
        private ToxDelegates.CallbackStatusMessageDelegate _onStatusMessageCallback;
        private ToxDelegates.CallbackUserStatusDelegate _onUserStatusCallback;
        private ToxDelegates.CallbackTypingChangeDelegate _onTypingChangeCallback;
        private ToxDelegates.CallbackConnectionStatusDelegate _onConnectionStatusCallback;
        private ToxDelegates.CallbackFriendConnectionStatusDelegate _onFriendConnectionStatusCallback;
        private ToxDelegates.CallbackReadReceiptDelegate _onReadReceiptCallback;
        private ToxDelegates.CallbackFileControlDelegate _onFileControlCallback;
        private ToxDelegates.CallbackFileReceiveChunkDelegate _onFileReceiveChunkCallback;
        private ToxDelegates.CallbackFileReceiveDelegate _onFileReceiveCallback;
        private ToxDelegates.CallbackFileRequestChunkDelegate _onFileRequestChunkCallback;
        private ToxDelegates.CallbackFriendPacketDelegate _onFriendLossyPacketCallback;
        private ToxDelegates.CallbackFriendPacketDelegate _onFriendLosslessPacketCallback;

        private ToxGroupDelegates.CallbackInviteDelegate _onGroupInviteCallback;
        private ToxGroupDelegates.CallbackJoinFailDelegate _onGroupJoinFailCallback;
        private ToxGroupDelegates.CallbackMessageDelegate _onGroupMessageCallback;
        private ToxGroupDelegates.CallbackModerationDelegate _onGroupModerationCallback;
        private ToxGroupDelegates.CallbackPasswordDelegate _onGroupPasswordCallback;
        private ToxGroupDelegates.CallbackPeerExitDelegate _onGroupPeerExitCallback;
        private ToxGroupDelegates.CallbackPeerJoinDelegate _onGroupPeerJoinCallback;
        private ToxGroupDelegates.CallbackPeerLimitDelegate _onGroupPeerLimitCallback;
        private ToxGroupDelegates.CallbackPeerNameDelegate _onGroupPeerNameCallback;
        private ToxGroupDelegates.CallbackPeerStatusDelegate _onGroupPeerStatusCallback;
        private ToxGroupDelegates.CallbackPrivacyStateDelegate _onGroupPrivacyStateCallback;
        private ToxGroupDelegates.CallbackPrivateMessageDelegate _onGroupPrivateMessageCallback;
        private ToxGroupDelegates.CallbackSelfJoinDelegate _onGroupSelfJoinCallback;
        private ToxGroupDelegates.CallbackTopicDelegate _onGroupTopicCallback;
        #endregion

        /// <summary>
        /// Options that are used for this instance of Tox.
        /// </summary>
        public ToxOptions Options { get; private set; }

        /// <summary>
        /// Whether or not we're connected to the DHT.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                ThrowIfDisposed();

                return ToxFunctions.SelfGetConnectionStatus(_tox) != ToxConnectionStatus.None;
            }
        }

        /// <summary>
        /// An array of friendnumbers of this Tox instance.
        /// </summary>
        public int[] Friends
        {
            get
            {
                uint size = ToxFunctions.SelfGetFriendListSize(_tox);
                uint[] friends = new uint[size];
                ToxFunctions.SelfGetFriendList(_tox, friends);

                return (int[])(object)friends;
            }
        }

        /// <summary>
        /// The nickname of this Tox instance.
        /// </summary>
        public string Name
        {
            get
            {
                ThrowIfDisposed();

                byte[] bytes = new byte[ToxFunctions.SelfGetNameSize(_tox)];
                ToxFunctions.SelfGetName(_tox, bytes);

                return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            }
            set
            {
                ThrowIfDisposed();

                byte[] bytes = Encoding.UTF8.GetBytes(value);
                var error = ToxErrorSetInfo.Ok;

                ToxFunctions.SelfSetName(_tox, bytes, (ushort)bytes.Length, ref error);
            }
        }

        /// <summary>
        /// The status message of this Tox instance.
        /// </summary>
        public string StatusMessage
        {
            get
            {
                ThrowIfDisposed();

                uint size = ToxFunctions.SelfGetStatusMessageSize(_tox);
                byte[] status = new byte[size];

                ToxFunctions.SelfGetStatusMessage(_tox, status);

                return Encoding.UTF8.GetString(status, 0, status.Length);
            }
            set
            {
                ThrowIfDisposed();

                byte[] msg = Encoding.UTF8.GetBytes(value);
                var error = ToxErrorSetInfo.Ok;
                ToxFunctions.SelfSetStatusMessage(_tox, msg, (uint)msg.Length, ref error);
            }
        }

        /// <summary>
        /// The Tox ID of this Tox instance.
        /// </summary>
        public ToxId Id
        {
            get
            {
                ThrowIfDisposed();

                byte[] address = new byte[ToxConstants.AddressSize];
                ToxFunctions.SelfGetAddress(_tox, address);

                return new ToxId(address);
            }
        }

        /// <summary>
        /// Retrieves the temporary DHT public key of this Tox instance.
        /// </summary>
        public ToxKey DhtId
        {
            get
            {
                ThrowIfDisposed();

                byte[] publicKey = new byte[ToxConstants.PublicKeySize];
                ToxFunctions.SelfGetDhtId(_tox, publicKey);

                return new ToxKey(ToxKeyType.Public, publicKey);
            }
        }

        /// <summary>
        /// Current user status of this Tox instance.
        /// </summary>
        public ToxUserStatus Status
        {
            get
            {
                ThrowIfDisposed();

                return ToxFunctions.SelfGetStatus(_tox);
            }
            set
            {
                ThrowIfDisposed();

                ToxFunctions.SelfSetStatus(_tox, value);
            }
        }

        /// <summary>
        /// The handle of this instance of Tox. 
        /// Do not dispose this handle manually, use the Dispose method in this class instead.
        /// </summary>
        public ToxHandle Handle
        {
            get
            {
                return _tox;
            }
        }

        /// <summary>
        /// Initializes a new instance of Tox. If no secret key is specified, toxcore will generate a new keypair.
        /// </summary>
        /// <param name="options">The options to initialize this instance of Tox with.</param>
        /// <param name="secretKey">Optionally, specify the secret key to initialize this instance of Tox with. Must be ToxConstants.SecretKeySize bytes in size.</param>
        public Tox(ToxOptions options, ToxKey secretKey = null)
        {
            var error = ToxErrorNew.Ok;
            var optionsStruct = options.Struct;

            if (secretKey != null)
                optionsStruct.SetData(secretKey.GetBytes(), ToxSaveDataType.SecretKey);

            _tox = ToxFunctions.New(ref optionsStruct, ref error);

            if (_tox == null || _tox.IsInvalid || error != ToxErrorNew.Ok)
                throw new Exception("Could not create a new instance of tox, error: " + error.ToString());

            optionsStruct.Free();
            Options = options;
        }

        /// <summary>
        /// Initializes a new instance of Tox.
        /// </summary>
        /// <param name="options">The options to initialize this instance of Tox with.</param>
        /// <param name="data">A byte array containing Tox save data.</param>
        /// <param name="key">The key to decrypt the given encrypted Tox profile data. If the data is not encrypted, this should be null.</param>
        public Tox(ToxOptions options, ToxData data, ToxEncryptionKey key = null)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            var optionsStruct = options.Struct;

            if (key == null || !data.IsEncrypted)
            {
                var error = ToxErrorNew.Ok;
                optionsStruct.SetData(data.Bytes, ToxSaveDataType.ToxSave);

                _tox = ToxFunctions.New(ref optionsStruct, ref error);

                if (_tox == null || _tox.IsInvalid || error != ToxErrorNew.Ok)
                    throw new Exception("Could not create a new instance of tox, error: " + error.ToString());
            }
            else
            {
                var error = ToxErrorNew.Ok;
                var decryptError = ToxErrorDecryption.Ok;
                byte[] decryptedData = ToxEncryption.DecryptData(data.Bytes, key, out decryptError);
                optionsStruct.SetData(decryptedData, ToxSaveDataType.ToxSave);

                _tox = ToxFunctions.New(ref optionsStruct, ref error);

                if (_tox == null || _tox.IsInvalid || error != ToxErrorNew.Ok || decryptError != ToxErrorDecryption.Ok)
                    throw new Exception(string.Format("Could not create a new instance of tox, error: {0}, decrypt error: {1}" + error.ToString(), decryptError.ToString()));
            }

            optionsStruct.Free();
            Options = options;
        }

        /// <summary>
        /// Releases all resources used by this instance of Tox.
        /// </summary>
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

            if (_tox != null && !_tox.IsInvalid && !_tox.IsClosed)
                _tox.Dispose();

            _disposed = true;
        }

        /// <summary>
        /// Starts the main 'tox_iterate' loop at an interval retrieved with 'tox_iteration_interval'.
        /// If you want to manage your own loop, use the Iterate method instead.
        /// </summary>
        public void Start()
        {
            ThrowIfDisposed();

            if (_running)
                return;

            Loop();
        }

        /// <summary>
        /// Stops the main tox_do loop if it's running.
        /// </summary>
        public void Stop()
        {
            ThrowIfDisposed();

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
        /// Runs the tox_iterate once in the current thread.
        /// </summary>
        /// <returns>The next timeout.</returns>
        public int Iterate()
        {
            ThrowIfDisposed();

            if (_running)
                throw new Exception("Loop already running");

            return DoIterate();
        }

        private int DoIterate()
        {
            ToxFunctions.Iterate(_tox);
            return (int)ToxFunctions.IterationInterval(_tox);
        }

        private void Loop()
        {
            _cancelTokenSource = new CancellationTokenSource();
            _running = true;

            Task.Factory.StartNew(async () =>
            {
                while (_running)
                {
                    if (_cancelTokenSource.IsCancellationRequested)
                        break;

                    int delay = DoIterate();
                    await Task.Delay(delay);
                }
            }, _cancelTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// Adds a friend to the friend list and sends a friend request.
        /// </summary>
        /// <param name="id">The Tox id of the friend to add.</param>
        /// <param name="message">The message that will be sent along with the friend request.</param>
        /// <param name="error"></param>
        /// <returns>The friend number.</returns>
        public int AddFriend(ToxId id, string message, out ToxErrorFriendAdd error)
        {
            ThrowIfDisposed();

            if (id == null)
                throw new ArgumentNullException("id");

            byte[] msg = Encoding.UTF8.GetBytes(message);
            error = ToxErrorFriendAdd.Ok;

            return (int)ToxFunctions.FriendAdd(_tox, id.Bytes, msg, (uint)msg.Length, ref error);
        }

        /// <summary>
        /// Adds a friend to the friend list and sends a friend request.
        /// </summary>
        /// <param name="id">The Tox id of the friend to add.</param>
        /// <param name="message">The message that will be sent along with the friend request.</param>
        /// <returns>The friend number.</returns>
        public int AddFriend(ToxId id, string message)
        {
            var error = ToxErrorFriendAdd.Ok;
            return AddFriend(id, message, out error);
        }

        /// <summary>
        /// Adds a friend to the friend list without sending a friend request.
        /// This method should be used to accept friend requests.
        /// </summary>
        /// <param name="publicKey">The public key of the friend to add.</param>
        /// <param name="error"></param>
        /// <returns>The friend number.</returns>
        public int AddFriendNoRequest(ToxKey publicKey, out ToxErrorFriendAdd error)
        {
            ThrowIfDisposed();

            if (publicKey == null)
                throw new ArgumentNullException("publicKey");

            error = ToxErrorFriendAdd.Ok;
            return (int)ToxFunctions.FriendAddNoRequest(_tox, publicKey.GetBytes(), ref error);
        }

        /// <summary>
        /// Adds a friend to the friend list without sending a friend request.
        /// This method should be used to accept friend requests.
        /// </summary>
        /// <param name="publicKey">The public key of the friend to add.</param>
        /// <returns>The friend number.</returns>
        public int AddFriendNoRequest(ToxKey publicKey)
        {
            var error = ToxErrorFriendAdd.Ok;
            return AddFriendNoRequest(publicKey, out error);
        }

        /// <summary>
        /// Adds a node as a TCP relay. 
        /// This method can be used to initiate TCP connections to different ports on the same bootstrap node, or to add TCP relays without using them as bootstrap nodes.
        /// </summary>
        /// <param name="node">The node to add.</param>
        /// <param name="error"></param>
        /// <returns>True on success.</returns>
        public bool AddTcpRelay(ToxNode node, out ToxErrorBootstrap error)
        {
            ThrowIfDisposed();

            if (node == null)
                throw new ArgumentNullException("node");

            error = ToxErrorBootstrap.Ok;
            return ToxFunctions.AddTcpRelay(_tox, node.Address, (ushort)node.Port, node.PublicKey.GetBytes(), ref error);
        }

        /// <summary>
        /// Adds a node as a TCP relay. 
        /// This method can be used to initiate TCP connections to different ports on the same bootstrap node, or to add TCP relays without using them as bootstrap nodes.
        /// </summary>
        /// <param name="node">The node to add.</param>
        /// <returns>True on success.</returns>
        public bool AddTcpRelay(ToxNode node)
        {
            var error = ToxErrorBootstrap.Ok;
            return AddTcpRelay(node, out error);
        }

        /// <summary>
        /// Attempts to bootstrap this Tox instance with a ToxNode. A 'getnodes' request is sent to the given node.
        /// </summary>
        /// <param name="node">The node to bootstrap off of.</param>
        /// <param name="error"></param>
        /// <returns>True if the 'getnodes' request was sent successfully.</returns>
        public bool Bootstrap(ToxNode node, out ToxErrorBootstrap error)
        {
            ThrowIfDisposed();

            if (node == null)
                throw new ArgumentNullException("node");

            error = ToxErrorBootstrap.Ok;
            return ToxFunctions.Bootstrap(_tox, node.Address, (ushort)node.Port, node.PublicKey.GetBytes(), ref error);
        }

        /// <summary>
        /// Attempts to bootstrap this Tox instance with a ToxNode. A 'getnodes' request is sent to the given node.
        /// </summary>
        /// <param name="node">The node to bootstrap off of.</param>
        /// <returns>True if the 'getnodes' request was sent successfully.</returns>
        public bool Bootstrap(ToxNode node)
        {
            var error = ToxErrorBootstrap.Ok;
            return Bootstrap(node, out error);
        }

        /// <summary>
        /// Checks if there exists a friend with given friendNumber.
        /// </summary>
        /// <param name="friendNumber">The friend number to check.</param>
        /// <returns>True if the friend exists.</returns>
        public bool FriendExists(int friendNumber)
        {
            ThrowIfDisposed();

            return ToxFunctions.FriendExists(_tox, (uint)friendNumber);
        }

        /// <summary>
        /// Retrieves the typing status of a friend.
        /// </summary>
        /// <param name="friendNumber">The friend number to retrieve the typing status of.</param>
        /// <param name="error"></param>
        /// <returns>True if the friend is typing.</returns>
        public bool GetFriendTypingStatus(int friendNumber, out ToxErrorFriendQuery error)
        {
            ThrowIfDisposed();

            error = ToxErrorFriendQuery.Ok;
            return ToxFunctions.FriendGetTyping(_tox, (uint)friendNumber, ref error);
        }

        /// <summary>
        /// Retrieves the typing status of a friend.
        /// </summary>
        /// <param name="friendNumber">The friend number to retrieve the typing status of.</param>
        /// <returns>True if the friend is typing.</returns>
        public bool GetFriendTypingStatus(int friendNumber)
        {
            var error = ToxErrorFriendQuery.Ok;
            return GetFriendTypingStatus(friendNumber, out error);
        }

        /// <summary>
        /// Retrieves the friendNumber associated with the specified public key.
        /// </summary>
        /// <param name="publicKey">The public key to look for.</param>
        /// <param name="error"></param>
        /// <returns>The friend number on success.</returns>
        public int GetFriendByPublicKey(ToxKey publicKey, out ToxErrorFriendByPublicKey error)
        {
            ThrowIfDisposed();

            if (publicKey == null)
                throw new ArgumentNullException("publicKey");

            error = ToxErrorFriendByPublicKey.Ok;
            return (int)ToxFunctions.FriendByPublicKey(_tox, publicKey.GetBytes(), ref error);
        }

        /// <summary>
        /// Retrieves the friendNumber associated with the specified public key.
        /// </summary>
        /// <param name="publicKey">The public key to look for.</param>
        /// <returns>The friend number on success.</returns>
        public int GetFriendByPublicKey(ToxKey publicKey)
        {
            var error = ToxErrorFriendByPublicKey.Ok;
            return GetFriendByPublicKey(publicKey, out error);
        }

        /// <summary>
        /// Retrieves a friend's connection status.
        /// </summary>
        /// <param name="friendNumber">The friend number to retrieve the connection status of.</param>
        /// <param name="error"></param>
        /// <returns>The connection status on success.</returns>
        public ToxConnectionStatus GetFriendConnectionStatus(int friendNumber, out ToxErrorFriendQuery error)
        {
            ThrowIfDisposed();

            error = ToxErrorFriendQuery.Ok;
            return ToxFunctions.FriendGetConnectionStatus(_tox, (uint)friendNumber, ref error);
        }

        /// <summary>
        /// Retrieves a friend's connection status.
        /// </summary>
        /// <param name="friendNumber">The friend number to retrieve the connection status of.</param>
        /// <returns>The connection status on success.</returns>
        public ToxConnectionStatus GetFriendConnectionStatus(int friendNumber)
        {
            var error = ToxErrorFriendQuery.Ok;
            return GetFriendConnectionStatus(friendNumber, out error);
        }

        /// <summary>
        /// Check whether or not a friend is online.
        /// </summary>
        /// <param name="friendNumber">The friend number.</param>
        /// <returns>True if the friend is online.</returns>
        public bool IsFriendOnline(int friendNumber)
        {
            return GetFriendConnectionStatus(friendNumber) != ToxConnectionStatus.None;
        }

        /// <summary>
        /// Retrieves a friend's public key.
        /// </summary>
        /// <param name="friendNumber">The friend number to retrieve the public key of.</param>
        /// <param name="error"></param>
        /// <returns>The friend's public key on success.</returns>
        public ToxKey GetFriendPublicKey(int friendNumber, out ToxErrorFriendGetPublicKey error)
        {
            ThrowIfDisposed();

            byte[] address = new byte[ToxConstants.PublicKeySize];
            error = ToxErrorFriendGetPublicKey.Ok;

            if (!ToxFunctions.FriendGetPublicKey(_tox, (uint)friendNumber, address, ref error))
                return null;

            return new ToxKey(ToxKeyType.Public, address);
        }

        /// <summary>
        /// Retrieves a friend's public key.
        /// </summary>
        /// <param name="friendNumber">The friend number to retrieve the public key of.</param>
        /// <returns>The friend's public key on success.</returns>
        public ToxKey GetFriendPublicKey(int friendNumber)
        {
            var error = ToxErrorFriendGetPublicKey.Ok;
            return GetFriendPublicKey(friendNumber, out error);
        }

        /// <summary>
        /// Retrieves a friend's current status.
        /// </summary>
        /// <param name="friendNumber">The friend number to retrieve the status of.</param>
        /// <param name="error"></param>
        /// <returns>The friend's status on success.</returns>
        public ToxUserStatus GetFriendStatus(int friendNumber, out ToxErrorFriendQuery error)
        {
            ThrowIfDisposed();

            error = ToxErrorFriendQuery.Ok;
            return ToxFunctions.FriendGetStatus(_tox, (uint)friendNumber, ref error);
        }

        /// <summary>
        /// Retrieves a friend's current status.
        /// </summary>
        /// <param name="friendNumber">The friend number to retrieve the status of.</param>
        /// <returns>The friend's status on success.</returns>
        public ToxUserStatus GetFriendStatus(int friendNumber)
        {
            var error = ToxErrorFriendQuery.Ok;
            return GetFriendStatus(friendNumber, out error);
        }

        /// <summary>
        /// Sets the typing status of this Tox instance for a friend.
        /// </summary>
        /// <param name="friendNumber">The friend number to set the typing status for.</param>
        /// <param name="isTyping">Whether or not we're typing.</param>
        /// <param name="error"></param>
        /// <returns>True on success.</returns>
        public bool SetTypingStatus(int friendNumber, bool isTyping, out ToxErrorSetTyping error)
        {
            ThrowIfDisposed();

            error = ToxErrorSetTyping.Ok;
            return ToxFunctions.SelfSetTyping(_tox, (uint)friendNumber, isTyping, ref error);
        }

        /// <summary>
        /// Sets the typing status of this Tox instance for a friend.
        /// </summary>
        /// <param name="friendNumber">The friend number to set the typing status for.</param>
        /// <param name="isTyping">Whether or not we're typing.</param>
        /// <returns>True on success.</returns>
        public bool SetTypingStatus(int friendNumber, bool isTyping)
        {
            var error = ToxErrorSetTyping.Ok;
            return SetTypingStatus(friendNumber, isTyping, out error);
        }

        /// <summary>
        /// Sends a message to a friend.
        /// </summary>
        /// <param name="friendNumber">The friend number to send the message to.</param>
        /// <param name="message">The message to be sent. Maximum length: <see cref="ToxConstants.MaxMessageLength"/></param>
        /// <param name="type">The type of this message.</param>
        /// <param name="error"></param>
        /// <returns>Message ID on success.</returns>
        public int SendMessage(int friendNumber, string message, ToxMessageType type, out ToxErrorSendMessage error)
        {
            ThrowIfDisposed();

            byte[] bytes = Encoding.UTF8.GetBytes(message);
            error = ToxErrorSendMessage.Ok;

            return (int)ToxFunctions.FriendSendMessage(_tox, (uint)friendNumber, type, bytes, (uint)bytes.Length, ref error);
        }

        /// <summary>
        /// Sends a message to a friend.
        /// </summary>
        /// <param name="friendNumber">The friend number to send the message to.</param>
        /// <param name="message">The message to be sent. Maximum length: <see cref="ToxConstants.MaxMessageLength"/></param>
        /// <param name="type">The type of this message.</param>
        /// <returns>Message ID on success.</returns>
        public int SendMessage(int friendNumber, string message, ToxMessageType type)
        {
            var error = ToxErrorSendMessage.Ok;
            return SendMessage(friendNumber, message, type, out error);
        }

        /// <summary>
        /// Deletes a friend from the friend list.
        /// </summary>
        /// <param name="friendNumber">The friend number to be deleted.</param>
        /// <param name="error"></param>
        /// <returns>True on success.</returns>
        public bool DeleteFriend(int friendNumber, out ToxErrorFriendDelete error)
        {
            ThrowIfDisposed();

            error = ToxErrorFriendDelete.Ok;
            return ToxFunctions.FriendDelete(_tox, (uint)friendNumber, ref error);
        }

        /// <summary>
        /// Deletes a friend from the friend list.
        /// </summary>
        /// <param name="friendNumber">The friend number to be deleted.</param>
        /// <returns>True on success.</returns>
        public bool DeleteFriend(int friendNumber)
        {
            var error = ToxErrorFriendDelete.Ok;
            return DeleteFriend(friendNumber, out error);
        }

        /// <summary>
        /// Retrieves a ToxData object that contains the profile data of this Tox instance.
        /// </summary>
        /// <returns></returns>
        public ToxData GetData()
        {
            ThrowIfDisposed();

            byte[] bytes = new byte[ToxFunctions.GetSaveDataSize(_tox)];
            ToxFunctions.GetSaveData(_tox, bytes);

            return ToxData.FromBytes(bytes);
        }

        /// <summary>
        /// Retrieves a ToxData object that contains the profile data of this Tox instance, encrypted with the provided key.
        /// </summary>
        /// <param name="key">The key to encrypt the Tox data with.</param>
        /// <returns></returns>
        public ToxData GetData(ToxEncryptionKey key)
        {
            ThrowIfDisposed();

            var data = GetData();
            byte[] encrypted = ToxEncryption.EncryptData(data.Bytes, key);

            return ToxData.FromBytes(encrypted);
        }

        /// <summary>
        /// Retrieves the private key of this Tox instance.
        /// </summary>
        /// <returns>The private key of this Tox instance.</returns>
        public ToxKey GetPrivateKey()
        {
            ThrowIfDisposed();

            byte[] key = new byte[ToxConstants.PublicKeySize];
            ToxFunctions.SelfGetSecretKey(_tox, key);

            return new ToxKey(ToxKeyType.Secret, key);
        }

        /// <summary>
        /// Retrieves the name of a friend.
        /// </summary>
        /// <param name="friendNumber">The friend number to retrieve the name of.</param>
        /// <param name="error"></param>
        /// <returns>The friend's name on success.</returns>
        public string GetFriendName(int friendNumber, out ToxErrorFriendQuery error)
        {
            ThrowIfDisposed();

            error = ToxErrorFriendQuery.Ok;
            uint size = ToxFunctions.FriendGetNameSize(_tox, (uint)friendNumber, ref error);

            if (error != ToxErrorFriendQuery.Ok)
                return string.Empty;

            byte[] name = new byte[size];
            if (!ToxFunctions.FriendGetName(_tox, (uint)friendNumber, name, ref error))
                return string.Empty;

            return Encoding.UTF8.GetString(name, 0, name.Length);
        }

        /// <summary>
        /// Retrieves the name of a friend.
        /// </summary>
        /// <param name="friendNumber">The friend number to retrieve the name of.</param>
        /// <returns>The friend's name on success.</returns>
        public string GetFriendName(int friendNumber)
        {
            var error = ToxErrorFriendQuery.Ok;
            return GetFriendName(friendNumber, out error);
        }

        /// <summary>
        /// Retrieves the status message of a friend.
        /// </summary>
        /// <param name="friendNumber">The friend number to retrieve the status message of.</param>
        /// <param name="error"></param>
        /// <returns>The friend's status message on success.</returns>
        public string GetFriendStatusMessage(int friendNumber, out ToxErrorFriendQuery error)
        {
            ThrowIfDisposed();

            error = ToxErrorFriendQuery.Ok;
            uint size = ToxFunctions.FriendGetStatusMessageSize(_tox, (uint)friendNumber, ref error);

            if (error != ToxErrorFriendQuery.Ok)
                return string.Empty;

            byte[] message = new byte[size];
            if (!ToxFunctions.FriendGetStatusMessage(_tox, (uint)friendNumber, message, ref error))
                return string.Empty;

            return Encoding.UTF8.GetString(message, 0, message.Length);
        }

        /// <summary>
        /// Retrieves the status message of a friend.
        /// </summary>
        /// <param name="friendNumber">The friend number to retrieve the status message of.</param>
        /// <returns>The friend's status message on success.</returns>
        public string GetFriendStatusMessage(int friendNumber)
        {
            var error = ToxErrorFriendQuery.Ok;
            return GetFriendStatusMessage(friendNumber, out error);
        }

        /// <summary>
        /// Retrieves the UDP port this instance of Tox is bound to.
        /// </summary>
        /// <param name="error"></param>
        /// <returns>The UDP port on success.</returns>
        public int GetUdpPort(out ToxErrorGetPort error)
        {
            ThrowIfDisposed();

            error = ToxErrorGetPort.Ok;
            return ToxFunctions.SelfGetUdpPort(_tox, ref error);
        }

        /// <summary>
        /// Retrieves the UDP port this instance of Tox is bound to.
        /// </summary>
        /// <returns>The UDP port on success.</returns>
        public int GetUdpPort()
        {
            var error = ToxErrorGetPort.Ok;
            return GetUdpPort(out error);
        }

        /// <summary>
        /// Retrieves the TCP port this instance of Tox is bound to.
        /// </summary>
        /// <param name="error"></param>
        /// <returns>The TCP port on success.</returns>
        public int GetTcpPort(out ToxErrorGetPort error)
        {
            ThrowIfDisposed();

            error = ToxErrorGetPort.Ok;
            return ToxFunctions.SelfGetTcpPort(_tox, ref error);
        }

        /// <summary>
        /// Retrieves the TCP port this instance of Tox is bound to.
        /// </summary>
        /// <returns>The TCP port on success.</returns>
        public int GetTcpPort()
        {
            var error = ToxErrorGetPort.Ok;
            return GetTcpPort(out error);
        }

        /// <summary>
        /// Sets the nospam value for this Tox instance.
        /// </summary>
        /// <param name="nospam">The nospam value to set.</param>
        [CLSCompliant(false)]
        public void SetNospam(uint nospam)
        {
            ThrowIfDisposed();

            ToxFunctions.SelfSetNospam(_tox, nospam);
        }

        /// <summary>
        /// Retrieves the nospam value of this Tox instance.
        /// </summary>
        /// <returns>The nospam value.</returns>
        [CLSCompliant(false)]
        public uint GetNospam()
        {
            ThrowIfDisposed();

            return ToxFunctions.SelfGetNospam(_tox);
        }

        /// <summary>
        /// Sends a file control command to a friend for a given file transfer.
        /// </summary>
        /// <param name="friendNumber">The friend to send the file control to.</param>
        /// <param name="fileNumber">The file transfer that this control is meant for.</param>
        /// <param name="control">The control to send.</param>
        /// <param name="error"></param>
        /// <returns>True on success.</returns>
        public bool FileControl(int friendNumber, int fileNumber, ToxFileControl control, out ToxErrorFileControl error)
        {
            ThrowIfDisposed();

            error = ToxErrorFileControl.Ok;

            return ToxFunctions.FileControl(_tox, (uint)friendNumber, (uint)fileNumber, control, ref error);
        }

        /// <summary>
        /// Sends a file control command to a friend for a given file transfer.
        /// </summary>
        /// <param name="friendNumber">The friend to send the file control to.</param>
        /// <param name="fileNumber">The file transfer that this control is meant for.</param>
        /// <param name="control">The control to send.</param>
        /// <returns>True on success.</returns>
        public bool FileControl(int friendNumber, int fileNumber, ToxFileControl control)
        {
            var error = ToxErrorFileControl.Ok;
            return FileControl(friendNumber, fileNumber, control, out error);
        }

        /// <summary>
        /// Send a file transmission request.
        /// </summary>
        /// <param name="friendNumber">The friend number to send the request to.</param>
        /// <param name="kind">The kind of file that will be transferred.</param>
        /// <param name="fileSize">The size of the file that will be transferred.</param>
        /// <param name="fileName">The filename of the file that will be transferred.</param>
        /// <param name="error"></param>
        /// <returns>Info about the file transfer on success.</returns>
        public ToxFileInfo FileSend(int friendNumber, ToxFileKind kind, long fileSize, string fileName, out ToxErrorFileSend error)
        {
            ThrowIfDisposed();

            error = ToxErrorFileSend.Ok;
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
            int fileNumber = (int)ToxFunctions.FileSend(_tox, (uint)friendNumber, kind, (ulong)fileSize, null, fileNameBytes, (uint)fileNameBytes.Length, ref error);

            if (error == ToxErrorFileSend.Ok)
                return new ToxFileInfo(fileNumber, FileGetId(friendNumber, fileNumber));

            return null;
        }

        /// <summary>
        /// Send a file transmission request.
        /// </summary>
        /// <param name="friendNumber">The friend number to send the request to.</param>
        /// <param name="kind">The kind of file that will be transferred.</param>
        /// <param name="fileSize">The size of the file that will be transferred.</param>
        /// <param name="fileName">The filename of the file that will be transferred.</param>
        /// <returns>Info about the file transfer on success.</returns>
        public ToxFileInfo FileSend(int friendNumber, ToxFileKind kind, long fileSize, string fileName)
        {
            var error = ToxErrorFileSend.Ok;
            return FileSend(friendNumber, kind, fileSize, fileName, out error);
        }

        /// <summary>
        /// Send a file transmission request.
        /// </summary>
        /// <param name="friendNumber">The friend number to send the request to.</param>
        /// <param name="kind">The kind of file that will be transferred.</param>
        /// <param name="fileSize">The size of the file that will be transferred.</param>
        /// <param name="fileName">The filename of the file that will be transferred.</param>
        /// <param name="fileId">The id to identify this transfer with. Should be ToxConstants.FileIdLength bytes long.</param>
        /// <param name="error"></param>
        /// <returns>Info about the file transfer on success.</returns>
        public ToxFileInfo FileSend(int friendNumber, ToxFileKind kind, long fileSize, string fileName, byte[] fileId, out ToxErrorFileSend error)
        {
            ThrowIfDisposed();

            if (fileId.Length != ToxConstants.FileIdLength)
                throw new ArgumentException("fileId should be exactly ToxConstants.FileIdLength bytes long", "fileId");

            error = ToxErrorFileSend.Ok;
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
            int fileNumber = (int)ToxFunctions.FileSend(_tox, (uint)friendNumber, kind, (ulong)fileSize, fileId, fileNameBytes, (uint)fileNameBytes.Length, ref error);

            if (error == ToxErrorFileSend.Ok)
                return new ToxFileInfo(fileNumber, fileId);

            return null;
        }
        
        /// <summary>
        /// Send a file transmission request.
        /// </summary>
        /// <param name="friendNumber">The friend number to send the request to.</param>
        /// <param name="kind">The kind of file that will be transferred.</param>
        /// <param name="fileSize">The size of the file that will be transferred.</param>
        /// <param name="fileName">The filename of the file that will be transferred.</param>
        /// <param name="fileId">The id to identify this transfer with. Should be ToxConstants.FileIdLength bytes long.</param>
        /// <returns>Info about the file transfer on success.</returns>
        public ToxFileInfo FileSend(int friendNumber, ToxFileKind kind, long fileSize, string fileName, byte[] fileId)
        {
            var error = ToxErrorFileSend.Ok;
            return FileSend(friendNumber, kind, fileSize, fileName, fileId, out error);
        }

        /// <summary>
        /// Sends a file seek control command to a friend for a given file transfer.
        /// </summary>
        /// <param name="friendNumber">The friend to send the seek command to.</param>
        /// <param name="fileNumber">The file transfer that this command is meant for.</param>
        /// <param name="position">The position that the friend should change his stream to.</param>
        /// <param name="error"></param>
        /// <returns>True on success.</returns>
        public bool FileSeek(int friendNumber, int fileNumber, long position, out ToxErrorFileSeek error)
        {
            ThrowIfDisposed();

            error = ToxErrorFileSeek.Ok;
            return ToxFunctions.FileSeek(_tox, (uint)friendNumber, (uint)fileNumber, (ulong)position, ref error);
        }

        /// <summary>
        /// Sends a file seek control command to a friend for a given file transfer.
        /// </summary>
        /// <param name="friendNumber">The friend to send the seek command to.</param>
        /// <param name="fileNumber">The file transfer that this command is meant for.</param>
        /// <param name="position">The position that the friend should change his stream to.</param>
        /// <returns>True on success.</returns>
        public bool FileSeek(int friendNumber, int fileNumber, long position)
        {
            var error = ToxErrorFileSeek.Ok;
            return FileSeek(friendNumber, fileNumber, position, out error);
        }

        /// <summary>
        /// Sends a chunk of file data to a friend. This should be called in response to OnFileChunkRequested.
        /// </summary>
        /// <param name="friendNumber">The friend to send the chunk to.</param>
        /// <param name="fileNumber">The file transfer that this chunk belongs to.</param>
        /// <param name="position">The position from which to continue reading.</param>
        /// <param name="data">The data to send. (should be equal to 'Length' received through OnFileChunkRequested).</param>
        /// <param name="error"></param>
        /// <returns>True on success.</returns>
        public bool FileSendChunk(int friendNumber, int fileNumber, long position, byte[] data, out ToxErrorFileSendChunk error)
        {
            ThrowIfDisposed();

            if (data == null)
                throw new ArgumentNullException("data");

            error = ToxErrorFileSendChunk.Ok;

            return ToxFunctions.FileSendChunk(_tox, (uint)friendNumber, (uint)fileNumber, (ulong)position, data, (uint)data.Length, ref error);
        }

        /// <summary>
        /// Sends a chunk of file data to a friend. This should be called in response to OnFileChunkRequested.
        /// </summary>
        /// <param name="friendNumber">The friend to send the chunk to.</param>
        /// <param name="fileNumber">The file transfer that this chunk belongs to.</param>
        /// <param name="position">The position from which to continue reading.</param>
        /// <param name="data">The data to send. (should be equal to 'Length' received through OnFileChunkRequested).</param>
        /// <returns>True on success.</returns>
        public bool FileSendChunk(int friendNumber, int fileNumber, long position, byte[] data)
        {
            var error = ToxErrorFileSendChunk.Ok;
            return FileSendChunk(friendNumber, fileNumber, position, data, out error);
        }

        /// <summary>
        /// Retrieves the unique id of a file transfer. This can be used to uniquely identify file transfers across core restarts.
        /// </summary>
        /// <param name="friendNumber">The friend number that's associated with this transfer.</param>
        /// <param name="fileNumber">The target file transfer.</param>
        /// <param name="error"></param>
        /// <returns>File transfer id on success.</returns>
        public byte[] FileGetId(int friendNumber, int fileNumber, out ToxErrorFileGet error)
        {
            ThrowIfDisposed();

            error = ToxErrorFileGet.Ok;
            byte[] id = new byte[ToxConstants.FileIdLength];

            if (!ToxFunctions.FileGetFileId(_tox, (uint)friendNumber, (uint)fileNumber, id, ref error))
                return null;

            return id;
        }

        /// <summary>
        /// Retrieves the unique id of a file transfer. This can be used to uniquely identify file transfers across core restarts.
        /// </summary>
        /// <param name="friendNumber">The friend number that's associated with this transfer.</param>
        /// <param name="fileNumber">The target file transfer.</param>
        /// <returns>File transfer id on success.</returns>
        public byte[] FileGetId(int friendNumber, int fileNumber)
        {
            var error = ToxErrorFileGet.Ok;
            return FileGetId(friendNumber, fileNumber, out error);
        }

        /// <summary>
        /// Sends a custom lossy packet to a friend. 
        /// Lossy packets are like UDP packets, they may never reach the other side, arrive more than once or arrive in the wrong order.
        /// </summary>
        /// <param name="friendNumber">The friend to send the packet to.</param>
        /// <param name="data">The data to send. The first byte must be in the range 200-254. The maximum length of the data is ToxConstants.MaxCustomPacketSize</param>
        /// <param name="error"></param>
        /// <returns>True on success.</returns>
        public bool FriendSendLossyPacket(int friendNumber, byte[] data, out ToxErrorFriendCustomPacket error)
        {
            ThrowIfDisposed();

            if (data == null)
                throw new ArgumentNullException("data");

            error = ToxErrorFriendCustomPacket.Ok;

            return ToxFunctions.FriendSendLossyPacket(_tox, (uint)friendNumber, data, (uint)data.Length, ref error);
        }

        /// <summary>
        /// Sends a custom lossy packet to a friend. 
        /// Lossy packets are like UDP packets, they may never reach the other side, arrive more than once or arrive in the wrong order.
        /// </summary>
        /// <param name="friendNumber">The friend to send the packet to.</param>
        /// <param name="data">The data to send. The first byte must be in the range 200-254. The maximum length of the data is ToxConstants.MaxCustomPacketSize</param>
        /// <returns>True on success.</returns>
        public bool FriendSendLossyPacket(int friendNumber, byte[] data)
        {
            var error = ToxErrorFriendCustomPacket.Ok;
            return FriendSendLossyPacket(friendNumber, data, out error);
        }

        /// <summary>
        /// Sends a custom lossless packet to a friend.
        /// Lossless packets behave like TCP, they're reliable and arrive in order. The difference is that it's not a stream.
        /// </summary>
        /// <param name="friendNumber">The friend to send the packet to.</param>
        /// <param name="data">The data to send. The first byte must be in the range 160-191. The maximum length of the data is ToxConstants.MaxCustomPacketSize</param>
        /// <param name="error"></param>
        /// <returns>True on success.</returns>
        public bool FriendSendLosslessPacket(int friendNumber, byte[] data, out ToxErrorFriendCustomPacket error)
        {
            ThrowIfDisposed();

            if (data == null)
                throw new ArgumentNullException("data");

            error = ToxErrorFriendCustomPacket.Ok;

            return ToxFunctions.FriendSendLosslessPacket(_tox, (uint)friendNumber, data, (uint)data.Length, ref error);
        }

        /// <summary>
        /// Sends a custom lossless packet to a friend.
        /// Lossless packets behave like TCP, they're reliable and arrive in order. The difference is that it's not a stream.
        /// </summary>
        /// <param name="friendNumber">The friend to send the packet to.</param>
        /// <param name="data">The data to send. The first byte must be in the range 160-191. The maximum length of the data is ToxConstants.MaxCustomPacketSize</param>
        /// <returns>True on success.</returns>
        public bool FriendSendLosslessPacket(int friendNumber, byte[] data)
        {
            var error = ToxErrorFriendCustomPacket.Ok;
            return FriendSendLosslessPacket(friendNumber, data, out error);
        }

        /// <summary>
        /// Retrieves the time a friend was last seen online.
        /// </summary>
        /// <param name="friendNumber">The friend to retrieve the 'last online' of.</param>
        /// <param name="error"></param>
        /// <returns>The time this friend was last seen online, on success.</returns>
        public DateTime GetFriendLastOnline(int friendNumber, out ToxErrorFriendGetLastOnline error)
        {
            error = ToxErrorFriendGetLastOnline.Ok;
            ulong time = ToxFunctions.FriendGetLastOnline(_tox, (uint)friendNumber, ref error);

            return ToxTools.EpochToDateTime(time);
        }

        /// <summary>
        /// Retrieves the time a friend was last seen online.
        /// </summary>
        /// <param name="friendNumber">The friend to retrieve the 'last online' of.</param>
        /// <returns>The time this friend was last seen online, on success.</returns>
        public DateTime GetFriendLastOnline(int friendNumber)
        {
            var error = ToxErrorFriendGetLastOnline.Ok;
            return GetFriendLastOnline(friendNumber, out error);
        }

        /// <summary>
        /// Creates a new group.
        /// </summary>
        /// <returns>The group.</returns>
        /// <param name="privacyState">Privacy state.</param>
        /// <param name="groupName">Group name.</param>
        /// <param name="error">Error.</param>
        public int CreateGroup(ToxGroupPrivacyState privacyState, string groupName, out ToxErrorGroupNew error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupNew.Ok;
            byte[] nameBytes = Encoding.UTF8.GetBytes(groupName);

            return (int)ToxGroupFunctions.New(_tox, privacyState, nameBytes, (uint)nameBytes.Length, ref error);
        }

        /// <summary>
        /// Creates a new group.
        /// </summary>
        /// <returns>The group.</returns>
        /// <param name="privacyState">Privacy state.</param>
        /// <param name="groupName">Group name.</param>
        public int CreateGroup(ToxGroupPrivacyState privacyState, string groupName)
        {
            var error = ToxErrorGroupNew.Ok;
            return CreateGroup(privacyState, groupName, out error);
        }

        /// <summary>
        /// Joins a group.
        /// </summary>
        /// <returns>The group.</returns>
        /// <param name="chatId">Chat identifier.</param>
        /// <param name="password">Password.</param>
        /// <param name="error">Error.</param>
        public int JoinGroup(byte[] chatId, string password, out ToxErrorGroupJoin error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupJoin.Ok;
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            return (int)ToxGroupFunctions.Join(_tox, chatId, passwordBytes, (uint)passwordBytes.Length, ref error);
        }

        /// <summary>
        /// Joins a group.
        /// </summary>
        /// <returns>The group.</returns>
        /// <param name="chatId">Chat identifier.</param>
        /// <param name="password">Password.</param>
        public int JoinGroup(byte[] chatId, string password)
        {
            var error = ToxErrorGroupJoin.Ok;
            return JoinGroup(chatId, password, out error);
        }

        /// <summary>
        /// Attempts to reconnect to the specified group.
        /// </summary>
        /// <param name="groupNumber">Group number.</param>
        /// <param name="error">Error.</param>
        public bool ReconnectToGroup(int groupNumber, out ToxErrorGroupReconnect error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupReconnect.Ok;
            return ToxGroupFunctions.Reconnect(_tox, (uint)groupNumber, ref error);
        }

        /// <summary>
        /// Attempts to reconnect to the specified group.
        /// </summary>
        /// <param name="groupNumber">Group number.</param>
        public bool ReconnectToGroup(int groupNumber)
        {
            var error = ToxErrorGroupReconnect.Ok;
            return ReconnectToGroup(groupNumber, out error);
        }

        /// <summary>
        /// Leaves a group.
        /// </summary>
        /// <returns><c>true</c>, if group was left, <c>false</c> otherwise.</returns>
        /// <param name="groupNumber">Group number.</param>
        /// <param name="partMessage">Message.</param>
        /// <param name="error">Error.</param>
        public bool LeaveGroup(int groupNumber, string partMessage, out ToxErrorGroupLeave error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupLeave.Ok;
            byte[] messageBytes = Encoding.UTF8.GetBytes(partMessage);

            return ToxGroupFunctions.Leave(_tox, (uint)groupNumber, messageBytes, (uint)messageBytes.Length, ref error);
        }

        /// <summary>
        /// Leaves a group.
        /// </summary>
        /// <returns><c>true</c>, if group was left, <c>false</c> otherwise.</returns>
        /// <param name="groupNumber">Group number.</param>
        /// <param name="partMessage">Part message.</param>
        public bool LeaveGroup(int groupNumber, string partMessage)
        {
            var error = ToxErrorGroupLeave.Ok;
            return LeaveGroup(groupNumber, partMessage, out error);
        }

        /// <summary>
        /// Get our name for the specified group.
        /// </summary>
        /// <returns>The name.</returns>
        /// <param name="groupNumber">Group number.</param>
        /// <param name="error">Error.</param>
        public string GroupGetSelfName(int groupNumber, out ToxErrorGroupSelfQuery error)
        {
            ThrowIfDisposed();

            return GetNameGeneric((uint)groupNumber, ToxGroupFunctions.SelfGetNameSize, ToxGroupFunctions.SelfGetName, out error);
        }

        /// <summary>
        /// Get our name for the specified group.
        /// </summary>
        /// <returns>The name.</returns>
        /// <param name="groupNumber">Group number.</param>
        public string GroupGetSelfName(int groupNumber)
        {
            var error = ToxErrorGroupSelfQuery.Ok;
            return GroupGetSelfName(groupNumber, out error);
        }

        /// <summary>
        /// Set our displayed name for the specified group.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="name"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool GroupSetSelfName(int groupNumber, string name, out ToxErrorGroupSelfNameSet error)
        {
            ThrowIfDisposed();

            return SetNameGeneric((uint)groupNumber, name, ToxGroupFunctions.SelfSetName, out error);
        }

        /// <summary>
        /// Set our displayed name for the specified group.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool GroupSetSelfName(int groupNumber, string name)
        {
            var error = ToxErrorGroupSelfNameSet.Ok;
            return GroupSetSelfName(groupNumber, name, out error);
        }

        public bool GroupSetSelfStatus(int groupNumber, ToxUserStatus status, out ToxErrorGroupSelfStatusSet error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupSelfStatusSet.Ok;
            return ToxGroupFunctions.SelfSetStatus(_tox, (uint)groupNumber, status, ref error);
        }

        public bool GroupSetSelfStatus(int groupNumber, ToxUserStatus status)
        {
            var error = ToxErrorGroupSelfStatusSet.Ok;
            return GroupSetSelfStatus(groupNumber, status, out error);
        }

        public ToxUserStatus GroupGetSelfStatus(int groupNumber, out ToxErrorGroupSelfQuery error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupSelfQuery.Ok;
            return ToxGroupFunctions.SelfGetStatus(_tox, (uint)groupNumber, ref error);
        }

        public ToxUserStatus GroupGetSelfStatus(int groupNumber)
        {
            var error = ToxErrorGroupSelfQuery.Ok;
            return GroupGetSelfStatus(groupNumber, out error);
        }

        public ToxGroupRole GroupGetSelfRole(int groupNumber, out ToxErrorGroupSelfQuery error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupSelfQuery.Ok;
            return ToxGroupFunctions.SelfGetRole(_tox, (uint)groupNumber, ref error);
        }

        public ToxGroupRole GroupGetSelfRole(int groupNumber)
        {
            var error = ToxErrorGroupSelfQuery.Ok;
            return GroupGetSelfRole(groupNumber, out error);
        }

        public string GroupGetPeerName(int groupNumber, int peerNumber, out ToxErrorGroupPeerQuery error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupPeerQuery.Ok;

            uint nameLength = ToxGroupFunctions.PeerGetNameSize(_tox, (uint)groupNumber, (uint)peerNumber, ref error);
            if (error != ToxErrorGroupPeerQuery.Ok)
                return null;

            byte[] nameBytes = new byte[nameLength];
            if (!ToxGroupFunctions.PeerGetName(_tox, (uint)groupNumber, (uint)peerNumber, nameBytes, ref error))
                return null;

            return Encoding.UTF8.GetString(nameBytes, 0, nameBytes.Length);
        }

        public string GroupGetPeerName(int groupNumber, int peerNumber)
        {
            var error = ToxErrorGroupPeerQuery.Ok;
            return GroupGetPeerName(groupNumber, peerNumber, out error);
        }

        public ToxUserStatus GroupGetPeerStatus(int groupNumber, int peerNumber, out ToxErrorGroupPeerQuery error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupPeerQuery.Ok;
            return ToxGroupFunctions.PeerGetStatus(_tox, (uint)groupNumber, (uint)peerNumber, ref error);
        }

        public ToxUserStatus GroupGetPeerStatus(int groupNumber, int peerNumber)
        {
            var error = ToxErrorGroupPeerQuery.Ok;
            return GroupGetPeerStatus(groupNumber, peerNumber, out error);
        }

        public ToxGroupRole GroupGetPeerRole(int groupNumber, int peerNumber, out ToxErrorGroupPeerQuery error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupPeerQuery.Ok;
            return ToxGroupFunctions.PeerGetRole(_tox, (uint)groupNumber, (uint)peerNumber, ref error);
        }

        public ToxGroupRole GroupGetPeerRole(int groupNumber, int peerNumber)
        {
            var error = ToxErrorGroupPeerQuery.Ok;
            return GroupGetPeerRole(groupNumber, peerNumber, out error);
        }

        public bool GroupSetTopic(int groupNumber, string topic, out ToxErrorGroupTopicSet error)
        {
            ThrowIfDisposed();

            return SetNameGeneric((uint)groupNumber, topic, ToxGroupFunctions.SetTopic, out error);
        }

        public bool GroupSetTopic(int groupNumber, string topic)
        {
            var error = ToxErrorGroupTopicSet.Ok;
            return GroupSetTopic(groupNumber, topic, out error);
        }

        public string GroupGetTopic(int groupNumber, out ToxErrorGroupStateQueries error)
        {
            ThrowIfDisposed();

            return GetNameGeneric((uint)groupNumber, ToxGroupFunctions.GetTopicSize, ToxGroupFunctions.GetTopic, out error);
        }

        public string GroupGetTopic(int groupNumber)
        {
            var error = ToxErrorGroupStateQueries.Ok;
            return GroupGetTopic(groupNumber, out error);
        }

        public string GroupGetName(int groupNumber, out ToxErrorGroupStateQueries error)
        {
            ThrowIfDisposed();

            return GetNameGeneric((uint)groupNumber, ToxGroupFunctions.GetNameSize, ToxGroupFunctions.GetName, out error);
        }

        public string GroupGetName(int groupNumber)
        {
            var error = ToxErrorGroupStateQueries.Ok;
            return GroupGetName(groupNumber, out error);
        }

        public ToxKey GroupGetChatId(int groupNumber, out ToxErrorGroupStateQueries error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupStateQueries.Ok;
            byte[] chatId = new byte[ToxGroupConstants.ChatIdSize];

            if (!ToxGroupFunctions.GetChatId(_tox, (uint)groupNumber, chatId, ref error))
                return null;

            return new ToxKey(ToxKeyType.Public, chatId);
        }

        public ToxKey GroupGetChatId(int groupNumber)
        {
            var error = ToxErrorGroupStateQueries.Ok;
            return GroupGetChatId(groupNumber, out error);
        }

        public int GetGroupCount()
        {
            ThrowIfDisposed();

            return (int)ToxGroupFunctions.GetNumberGroups(_tox);
        }

        public string GroupGetPassword(int groupNumber, out ToxErrorGroupStateQueries error)
        {
            ThrowIfDisposed();

            return GetNameGeneric((uint)groupNumber, ToxGroupFunctions.GetPasswordSize, ToxGroupFunctions.GetPassword, out error);
        }

        public string GroupGetPassword(int groupNumber)
        {
            var error = ToxErrorGroupStateQueries.Ok;
            return GroupGetPassword(groupNumber, out error);
        }

        public ToxGroupPrivacyState GroupGetPrivacyState(int groupNumber, out ToxErrorGroupStateQueries error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupStateQueries.Ok;
            return ToxGroupFunctions.GetPrivacyState(_tox, (uint)groupNumber, ref error);
        }

        public ToxGroupPrivacyState GroupGetPrivacyState(int groupNumber)
        {
            var error = ToxErrorGroupStateQueries.Ok;
            return GroupGetPrivacyState(groupNumber, out error);
        }

        public int GroupGetPeerLimit(int groupNumber, out ToxErrorGroupStateQueries error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupStateQueries.Ok;
            return (int)ToxGroupFunctions.GetPeerLimit(_tox, (uint)groupNumber, ref error);
        }

        public int GroupGetPeerLimit(int groupNumber)
        {
            var error = ToxErrorGroupStateQueries.Ok;
            return GroupGetPeerLimit(groupNumber, out error);
        }

        public bool GroupSendMessage(int groupNumber, ToxMessageType messageType, string message, out ToxErrorGroupSendMessage error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupSendMessage.Ok;
            byte[] msgBytes = Encoding.UTF8.GetBytes(message);

            return ToxGroupFunctions.SendMessage(_tox, (uint)groupNumber, messageType, msgBytes, (uint)msgBytes.Length, ref error);
        }

        public bool GroupSendMessage(int groupNumber, ToxMessageType messageType, string message)
        {
            var error = ToxErrorGroupSendMessage.Ok;
            return GroupSendMessage(groupNumber, messageType, message, out error);
        }

        public bool GroupSendPrivateMessage(int groupNumber, int peerNumber, string message, out ToxErrorGroupSendPrivateMessage error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupSendPrivateMessage.Ok;
            byte[] msgBytes = Encoding.UTF8.GetBytes(message);

            return ToxGroupFunctions.SendPrivateMessage(_tox, (uint)groupNumber, (uint)peerNumber, msgBytes, (uint)msgBytes.Length, ref error);
        }

        public bool GroupSendPrivateMessage(int groupNumber, int peerNumber, string message)
        {
            var error = ToxErrorGroupSendPrivateMessage.Ok;
            return GroupSendPrivateMessage(groupNumber, peerNumber, message, out error);
        }

        public bool GroupInviteFriend(int groupNumber, int friendNumber, out ToxErrorGroupInviteFriend error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupInviteFriend.Ok;
            return ToxGroupFunctions.InviteFriend(_tox, (uint)groupNumber, (uint)friendNumber, ref error);
        }

        public bool GroupInviteFriend(int groupNumber, int friendNumber)
        {
            var error = ToxErrorGroupInviteFriend.Ok;
            return GroupInviteFriend(groupNumber, friendNumber, out error);
        }

        public bool GroupAcceptInvite(byte[] inviteData, string password, out ToxErrorGroupInviteAccept error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupInviteAccept.Ok;
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            return ToxGroupFunctions.InviteAccept(_tox, inviteData, (uint)inviteData.Length, passwordBytes, (uint)passwordBytes.Length, ref error);
        }

        public bool GroupAcceptInvite(byte[] inviteData, out ToxErrorGroupInviteAccept error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupInviteAccept.Ok;
            return ToxGroupFunctions.InviteAccept(_tox, inviteData, (uint)inviteData.Length, null, 0, ref error);
        }

        public bool GroupAcceptInvite(byte[] inviteData, string password = null)
        {
            var error = ToxErrorGroupInviteAccept.Ok;

            if (password == null)
                GroupAcceptInvite(inviteData, out error);

            return GroupAcceptInvite(inviteData, password, out error);
        }

        public bool GroupSetPassword(int groupNumber, string password, out ToxErrorGroupFounderSetPassword error)
        {
            ThrowIfDisposed();

            return SetNameGeneric((uint)groupNumber, password, ToxGroupFunctions.FounderSetPassword, out error);
        }

        public bool GroupSetPassword(int groupNumber, string password)
        {
            var error = ToxErrorGroupFounderSetPassword.Ok;
            return GroupSetPassword(groupNumber, password, out error);
        }

        public bool GroupSetPrivacyState(int groupNumber, ToxGroupPrivacyState state, out ToxErrorGroupFounderSetPrivacyState error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupFounderSetPrivacyState.Ok;
            return ToxGroupFunctions.FounderSetPrivacyState(_tox, (uint)groupNumber, state, ref error);
        }

        public bool GroupSetPrivacyState(int groupNumber, ToxGroupPrivacyState state)
        {
            var error = ToxErrorGroupFounderSetPrivacyState.Ok;
            return GroupSetPrivacyState(groupNumber, state, out error);
        }

        public bool GroupSetPeerLimit(int groupNumber, int maxPeers, out ToxErrorGroupFounderSetPeerLimit error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupFounderSetPeerLimit.Ok;
            return ToxGroupFunctions.FounderSetPeerLimit(_tox, (uint)groupNumber, (uint)maxPeers, ref error);
        }

        public bool GroupSetPeerLimit(int groupNumber, int maxPeers)
        {
            var error = ToxErrorGroupFounderSetPeerLimit.Ok;
            return GroupSetPeerLimit(groupNumber, maxPeers, out error);
        }

        public bool GroupToggleIgnore(int groupNumber, int peerNumber, bool ignore, out ToxErrorGroupToggleIgnore error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupToggleIgnore.Ok;
            return ToxGroupFunctions.ToggleIgnore(_tox, (uint)groupNumber, (uint)peerNumber, ignore, ref error);
        }

        public bool GroupToggleIgnore(int groupNumber, int peerNumber, bool ignore)
        {
            var error = ToxErrorGroupToggleIgnore.Ok;
            return GroupToggleIgnore(groupNumber, peerNumber, ignore, out error);
        }

        public bool GroupSetRole(int groupNumber, int peerNumber, ToxGroupRole role, out ToxErrorGroupModSetRole error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupModSetRole.Ok;
            return ToxGroupFunctions.ModSetRole(_tox, (uint)groupNumber, (uint)peerNumber, role, ref error);
        }

        public bool GroupSetRole(int groupNumber, int peerNumber, ToxGroupRole role)
        {
            var error = ToxErrorGroupModSetRole.Ok;
            return GroupSetRole(groupNumber, peerNumber, role, out error);
        }

        public bool GroupRemovePeer(int groupNumber, int peerNumber, bool setBan, out ToxErrorGroupModRemovePeer error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupModRemovePeer.Ok;
            return ToxGroupFunctions.ModRemovePeer(_tox, (uint)groupNumber, (uint)peerNumber, setBan, ref error);
        }

        public bool GroupRemovePeer(int groupNumber, int peerNumber, bool setBan)
        {
            var error = ToxErrorGroupModRemovePeer.Ok;
            return GroupRemovePeer(groupNumber, peerNumber, setBan, out error);
        }

        public bool GroupRemoveBan(int groupNumber, int banId, out ToxErrorGroupModRemoveBan error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupModRemoveBan.Ok;
            return ToxGroupFunctions.ModRemoveBan(_tox, (uint)groupNumber, (uint)banId, ref error);
        }

        public bool GroupRemoveBan(int groupNumber, int banId)
        {
            var error = ToxErrorGroupModRemoveBan.Ok;
            return GroupRemoveBan(groupNumber, banId, out error);
        }

        public int[] GroupGetBanList(int groupNumber, out ToxErrorGroupBanQuery error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupBanQuery.Ok;

            uint listSize = ToxGroupFunctions.BanGetListSize(_tox, (uint)groupNumber, ref error);
            if (error != ToxErrorGroupBanQuery.Ok)
                return null;

            uint[] bans = new uint[listSize];
            if (!ToxGroupFunctions.BanGetList(_tox, (uint)groupNumber, bans, ref error))
                return null;

            return (int[])(object)bans;
        }

        public int[] GroupGetBanList(int groupNumber)
        {
            var error = ToxErrorGroupBanQuery.Ok;
            return GroupGetBanList(groupNumber, out error);
        }

        public string GroupGetBanName(int groupNumber, int banId, out ToxErrorGroupBanQuery error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupBanQuery.Ok;

            uint nameLength = ToxGroupFunctions.BanGetNameSize(_tox, (uint)groupNumber, (uint)banId, ref error);
            if (error != ToxErrorGroupBanQuery.Ok)
                return null;

            byte[] nameBytes = new byte[nameLength];
            if (!ToxGroupFunctions.BanGetName(_tox, (uint)groupNumber, (uint)banId, nameBytes, ref error))
                return null;

            return Encoding.UTF8.GetString(nameBytes, 0, nameBytes.Length);
        }

        public string GroupGetBanName(int groupNumber, int banId)
        {
            var error = ToxErrorGroupBanQuery.Ok;
            return GroupGetBanName(groupNumber, banId, out error);
        }

        public DateTime GroupGetBanTime(int groupNumber, int banId, out ToxErrorGroupBanQuery error)
        {
            ThrowIfDisposed();

            error = ToxErrorGroupBanQuery.Ok;
            ulong time = ToxGroupFunctions.BanGetTimeSet(_tox, (uint)groupNumber, (uint)banId, ref error);

            return ToxTools.EpochToDateTime(time);
        }

        public DateTime GroupGetBanTime(int groupNumber, int banId)
        {
            var error = ToxErrorGroupBanQuery.Ok;
            return GroupGetBanTime(groupNumber, banId, out error);
        }

        public int GroupSelfGetPeerNumber(int groupNumber, out ToxErrorGroupSelfQuery error)
        {
            error = ToxErrorGroupSelfQuery.Ok;
            return (int)ToxGroupFunctions.SelfGetPeerId(_tox, (uint)groupNumber, ref error);
        }

        public int GroupSelfGetPeerNumber(int groupNumber)
        {
            var error = ToxErrorGroupSelfQuery.Ok;
            return GroupSelfGetPeerNumber(groupNumber, out error);
        }

        public ToxKey GroupGetPeerPublicKey(int groupNumber, int peerNumber, out ToxErrorGroupPeerQuery error)
        {
            error = ToxErrorGroupPeerQuery.Ok;
            byte[] publicKey = new byte[ToxGroupConstants.PeerPublicKeySize];

            if (!ToxGroupFunctions.PeerGetPublicKey(_tox, (uint)groupNumber, (uint)peerNumber, publicKey, ref error))
                return null;

            return new ToxKey(ToxKeyType.Public, publicKey);
        }

        public ToxKey GroupGetPeerPublicKey(int groupNumber, int peerNumber)
        {
            var error = ToxErrorGroupPeerQuery.Ok;
            return GroupGetPeerPublicKey(groupNumber, peerNumber, out error);
        }

        public ToxKey GroupGetSelfPublicKey(int groupNumber, out ToxErrorGroupSelfQuery error)
        {
            error = ToxErrorGroupSelfQuery.Ok;
            byte[] publicKey = new byte[ToxGroupConstants.PeerPublicKeySize];

            if (!ToxGroupFunctions.SelfGetPublicKey(_tox, (uint)groupNumber, publicKey, ref error))
                return null;

            return new ToxKey(ToxKeyType.Public, publicKey);
        }

        public ToxKey GroupGetSelfPublicKey(int groupNumber)
        {
            var error = ToxErrorGroupSelfQuery.Ok;
            return GroupGetSelfPublicKey(groupNumber, out error);
        }

        public bool GroupSendCustomPacket(int groupNumber, bool lossless, byte[] data, out ToxErrorGroupSendCustomPacket error)
        {
            error = ToxErrorGroupSendCustomPacket.Ok;
            return ToxGroupFunctions.SendCustomPacket(_tox, (uint)groupNumber, lossless, data, (uint)data.Length, ref error);
        }

        public bool GroupSendCustomPacket(int groupNumber, bool lossless, byte[] data)
        {
            var error = ToxErrorGroupSendCustomPacket.Ok;
            return GroupSendCustomPacket(groupNumber, lossless, data, out error);
        }

        #region Generics

        private delegate uint GetNameSizeDelegate<T>(ToxHandle handle, uint groupNumber, ref T error);
        private delegate bool GetNameDelegate<T>(ToxHandle handle, uint groupNumber, byte[] buffer, ref T error);
        private delegate bool SetNameDelegate<T>(ToxHandle handle, uint groupNumber, byte[] name, uint length, ref T error);

        private string GetNameGeneric<T>(uint groupNumber, GetNameSizeDelegate<T> sizeFunc, GetNameDelegate<T> nameFunc, out T error) 
            where T : struct, IConvertible
        {
            error = (T)(object)0;

            uint nameLength = sizeFunc(_tox, groupNumber, ref error);
            if ((int)(object)error != 0)
                return null;

            byte[] nameBytes = new byte[nameLength];
            if (!nameFunc(_tox, groupNumber, nameBytes, ref error))
                return null;

            return Encoding.UTF8.GetString(nameBytes, 0, nameBytes.Length);
        }

        private bool SetNameGeneric<T>(uint groupNumber, string name, SetNameDelegate<T> nameFunc, out T error)
        {
            error = (T)(object)0;
            byte[] topicBytes = Encoding.UTF8.GetBytes(name);

            return nameFunc(_tox, groupNumber, topicBytes, (uint)topicBytes.Length, ref error);
        }

        #endregion

        #region Events
        private EventHandler<ToxEventArgs.FriendRequestEventArgs> _onFriendRequestReceived;

        /// <summary>
        /// Occurs when a friend request is received.
        /// Friend requests should be accepted with AddFriendNoRequest.
        /// </summary>
        public event EventHandler<ToxEventArgs.FriendRequestEventArgs> OnFriendRequestReceived
        {
            add
            {
                if (_onFriendRequestCallback == null)
                {
                    _onFriendRequestCallback = (IntPtr tox, byte[] publicKey, byte[] message, uint length, IntPtr userData) =>
                    {
                        if (_onFriendRequestReceived != null)
                            _onFriendRequestReceived(this, new ToxEventArgs.FriendRequestEventArgs(new ToxKey(ToxKeyType.Public, ToxTools.HexBinToString(publicKey)), Encoding.UTF8.GetString(message, 0, (int)length)));
                    };

                    ToxFunctions.RegisterFriendRequestCallback(_tox, _onFriendRequestCallback, IntPtr.Zero);
                }

                _onFriendRequestReceived += value;
            }
            remove
            {
                if (_onFriendRequestReceived.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFriendRequestCallback(_tox, null, IntPtr.Zero);
                    _onFriendRequestCallback = null;
                }

                _onFriendRequestReceived -= value;
            }
        }

        private EventHandler<ToxEventArgs.FriendMessageEventArgs> _onFriendMessageReceived;

        /// <summary>
        /// Occurs when a message is received from a friend.
        /// </summary>
        public event EventHandler<ToxEventArgs.FriendMessageEventArgs> OnFriendMessageReceived
        {
            add
            {
                if (_onFriendMessageCallback == null)
                {
                    _onFriendMessageCallback = (IntPtr tox, uint friendNumber, ToxMessageType type, byte[] message, uint length, IntPtr userData) =>
                    {
                        if (_onFriendMessageReceived != null)
                            _onFriendMessageReceived(this, new ToxEventArgs.FriendMessageEventArgs((int)friendNumber, Encoding.UTF8.GetString(message, 0, (int)length), type));
                    };

                    ToxFunctions.RegisterFriendMessageCallback(_tox, _onFriendMessageCallback, IntPtr.Zero);
                }

                _onFriendMessageReceived += value;
            }
            remove
            {
                if (_onFriendMessageReceived.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFriendMessageCallback(_tox, null, IntPtr.Zero);
                    _onFriendMessageCallback = null;
                }

                _onFriendMessageReceived -= value;
            }
        }

        private EventHandler<ToxEventArgs.NameChangeEventArgs> _onFriendNameChanged;

        /// <summary>
        /// Occurs when a friend has changed his/her name.
        /// </summary>
        public event EventHandler<ToxEventArgs.NameChangeEventArgs> OnFriendNameChanged
        {
            add
            {
                if (_onNameChangeCallback == null)
                {
                    _onNameChangeCallback = (IntPtr tox, uint friendNumber, byte[] newName, uint length, IntPtr userData) =>
                    {
                        if (_onFriendNameChanged != null)
                            _onFriendNameChanged(this, new ToxEventArgs.NameChangeEventArgs((int)friendNumber, Encoding.UTF8.GetString(newName, 0, (int)length)));
                    };

                    ToxFunctions.RegisterNameChangeCallback(_tox, _onNameChangeCallback, IntPtr.Zero);
                }

                _onFriendNameChanged += value;
            }
            remove
            {
                if (_onFriendNameChanged.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterNameChangeCallback(_tox, null, IntPtr.Zero);
                    _onNameChangeCallback = null;
                }

                _onFriendNameChanged -= value;
            }
        }

        private EventHandler<ToxEventArgs.StatusMessageEventArgs> _onFriendStatusMessageChanged;

        /// <summary>
        /// Occurs when a friend has changed their status message.
        /// </summary>
        public event EventHandler<ToxEventArgs.StatusMessageEventArgs> OnFriendStatusMessageChanged
        {
            add
            {
                if (_onStatusMessageCallback == null)
                {
                    _onStatusMessageCallback = (IntPtr tox, uint friendNumber, byte[] newStatus, uint length, IntPtr userData) =>
                    {
                        if (_onFriendStatusMessageChanged != null)
                            _onFriendStatusMessageChanged(this, new ToxEventArgs.StatusMessageEventArgs((int)friendNumber, Encoding.UTF8.GetString(newStatus, 0, (int)length)));
                    };

                    ToxFunctions.RegisterStatusMessageCallback(_tox, _onStatusMessageCallback, IntPtr.Zero);
                }

                _onFriendStatusMessageChanged += value;
            }
            remove
            {
                if (_onFriendStatusMessageChanged.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterStatusMessageCallback(_tox, null, IntPtr.Zero);
                    _onStatusMessageCallback = null;
                }

                _onFriendStatusMessageChanged -= value;
            }
        }

        private EventHandler<ToxEventArgs.StatusEventArgs> _onFriendStatusChanged;

        /// <summary>
        /// Occurs when a friend has changed their user status.
        /// </summary>
        public event EventHandler<ToxEventArgs.StatusEventArgs> OnFriendStatusChanged
        {
            add
            {
                if (_onUserStatusCallback == null)
                {
                    _onUserStatusCallback = (IntPtr tox, uint friendNumber, ToxUserStatus status, IntPtr userData) =>
                    {
                        if (_onFriendStatusChanged != null)
                            _onFriendStatusChanged(this, new ToxEventArgs.StatusEventArgs((int)friendNumber, status));
                    };

                    ToxFunctions.RegisterUserStatusCallback(_tox, _onUserStatusCallback, IntPtr.Zero);
                }

                _onFriendStatusChanged += value;
            }
            remove
            {
                if (_onFriendStatusChanged.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterUserStatusCallback(_tox, null, IntPtr.Zero);
                    _onUserStatusCallback = null;
                }

                _onFriendStatusChanged -= value;
            }
        }

        private EventHandler<ToxEventArgs.TypingStatusEventArgs> _onFriendTypingChanged;

        /// <summary>
        /// Occurs when a friend's typing status has changed.
        /// </summary>
        public event EventHandler<ToxEventArgs.TypingStatusEventArgs> OnFriendTypingChanged
        {
            add
            {
                if (_onTypingChangeCallback == null)
                {
                    _onTypingChangeCallback = (IntPtr tox, uint friendNumber, bool typing, IntPtr userData) =>
                    {
                        if (_onFriendTypingChanged != null)
                            _onFriendTypingChanged(this, new ToxEventArgs.TypingStatusEventArgs((int)friendNumber, typing));
                    };

                    ToxFunctions.RegisterTypingChangeCallback(_tox, _onTypingChangeCallback, IntPtr.Zero);
                }

                _onFriendTypingChanged += value;
            }
            remove
            {
                if (_onFriendTypingChanged.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterTypingChangeCallback(_tox, null, IntPtr.Zero);
                    _onTypingChangeCallback = null;
                }

                _onFriendTypingChanged -= value;
            }
        }

        private EventHandler<ToxEventArgs.ConnectionStatusEventArgs> _onConnectionStatusChanged;

        /// <summary>
        /// Occurs when the connection status of this Tox instance has changed.
        /// </summary>
        public event EventHandler<ToxEventArgs.ConnectionStatusEventArgs> OnConnectionStatusChanged
        {
            add
            {
                if (_onConnectionStatusCallback == null)
                {
                    _onConnectionStatusCallback = (IntPtr tox, ToxConnectionStatus status, IntPtr userData) =>
                    {
                        if (_onConnectionStatusChanged != null)
                            _onConnectionStatusChanged(this, new ToxEventArgs.ConnectionStatusEventArgs(status));
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

        private EventHandler<ToxEventArgs.FriendConnectionStatusEventArgs> _onFriendConnectionStatusChanged;

        /// <summary>
        /// Occurs when the connection status of a friend has changed.
        /// </summary>
        public event EventHandler<ToxEventArgs.FriendConnectionStatusEventArgs> OnFriendConnectionStatusChanged
        {
            add
            {
                if (_onFriendConnectionStatusCallback == null)
                {
                    _onFriendConnectionStatusCallback = (IntPtr tox, uint friendNumber, ToxConnectionStatus status, IntPtr userData) =>
                    {
                        if (_onFriendConnectionStatusChanged != null)
                            _onFriendConnectionStatusChanged(this, new ToxEventArgs.FriendConnectionStatusEventArgs((int)friendNumber, (ToxConnectionStatus)status));
                    };

                    ToxFunctions.RegisterFriendConnectionStatusCallback(_tox, _onFriendConnectionStatusCallback, IntPtr.Zero);
                }

                _onFriendConnectionStatusChanged += value;
            }
            remove
            {
                if (_onFriendConnectionStatusChanged.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFriendConnectionStatusCallback(_tox, null, IntPtr.Zero);
                    _onFriendConnectionStatusCallback = null;
                }

                _onFriendConnectionStatusChanged -= value;
            }
        }

        private EventHandler<ToxEventArgs.ReadReceiptEventArgs> _onReadReceiptReceived;

        /// <summary>
        /// Occurs when a read receipt is received.
        /// </summary>
        public event EventHandler<ToxEventArgs.ReadReceiptEventArgs> OnReadReceiptReceived
        {
            add
            {
                if (_onReadReceiptCallback == null)
                {
                    _onReadReceiptCallback = (IntPtr tox, uint friendNumber, uint receipt, IntPtr userData) =>
                    {
                        if (_onReadReceiptReceived != null)
                            _onReadReceiptReceived(this, new ToxEventArgs.ReadReceiptEventArgs((int)friendNumber, (int)receipt));
                    };

                    ToxFunctions.RegisterFriendReadReceiptCallback(_tox, _onReadReceiptCallback, IntPtr.Zero);
                }

                _onReadReceiptReceived += value;
            }
            remove
            {
                if (_onReadReceiptReceived.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFriendReadReceiptCallback(_tox, null, IntPtr.Zero);
                    _onReadReceiptCallback = null;
                }

                _onReadReceiptReceived -= value;
            }
        }

        private EventHandler<ToxEventArgs.FileControlEventArgs> _onFileControlReceived;

        /// <summary>
        /// Occurs when a file control is received.
        /// </summary>
        public event EventHandler<ToxEventArgs.FileControlEventArgs> OnFileControlReceived
        {
            add
            {
                if (_onFileControlCallback == null)
                {
                    _onFileControlCallback = (IntPtr tox, uint friendNumber, uint fileNumber, ToxFileControl control, IntPtr userData) =>
                    {
                        if (_onFileControlReceived != null)
                            _onFileControlReceived(this, new ToxEventArgs.FileControlEventArgs((int)friendNumber, (int)fileNumber, control));
                    };

                    ToxFunctions.RegisterFileControlRecvCallback(_tox, _onFileControlCallback, IntPtr.Zero);
                }

                _onFileControlReceived += value;
            }
            remove
            {
                if (_onFileControlReceived.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFileControlRecvCallback(_tox, null, IntPtr.Zero);
                    _onFileControlCallback = null;
                }

                _onFileControlReceived -= value;
            }
        }

        private EventHandler<ToxEventArgs.FileChunkEventArgs> _onFileChunkReceived;

        /// <summary>
        /// Occurs when a chunk of data from a file transfer is received
        /// </summary>
        public event EventHandler<ToxEventArgs.FileChunkEventArgs> OnFileChunkReceived
        {
            add
            {
                if (_onFileReceiveChunkCallback == null)
                {
                    _onFileReceiveChunkCallback = (IntPtr tox, uint friendNumber, uint fileNumber, ulong position, byte[] data, uint length, IntPtr userData) =>
                    {
                        if (_onFileChunkReceived != null)
                            _onFileChunkReceived(this, new ToxEventArgs.FileChunkEventArgs((int)friendNumber, (int)fileNumber, data, (long)position));
                    };

                    ToxFunctions.RegisterFileReceiveChunkCallback(_tox, _onFileReceiveChunkCallback, IntPtr.Zero);
                }

                _onFileChunkReceived += value;
            }
            remove
            {
                if (_onFileChunkReceived.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFileReceiveChunkCallback(_tox, null, IntPtr.Zero);
                    _onFileReceiveChunkCallback = null;
                }

                _onFileChunkReceived -= value;
            }
        }

        private EventHandler<ToxEventArgs.FileSendRequestEventArgs> _onFileSendRequestReceived;

        /// <summary>
        /// Occurs when a new file transfer request has been received.
        /// </summary>
        public event EventHandler<ToxEventArgs.FileSendRequestEventArgs> OnFileSendRequestReceived
        {
            add
            {
                if (_onFileReceiveCallback == null)
                {
                    _onFileReceiveCallback = (IntPtr tox, uint friendNumber, uint fileNumber, ToxFileKind kind, ulong fileSize, byte[] filename, uint filenameLength, IntPtr userData) =>
                    {
                        if (_onFileSendRequestReceived != null)
                            _onFileSendRequestReceived(this, new ToxEventArgs.FileSendRequestEventArgs((int)friendNumber, (int)fileNumber, kind, (long)fileSize, filename == null ? string.Empty : Encoding.UTF8.GetString(filename, 0, filename.Length)));
                    };

                    ToxFunctions.RegisterFileReceiveCallback(_tox, _onFileReceiveCallback, IntPtr.Zero);
                }

                _onFileSendRequestReceived += value;
            }
            remove
            {
                if (_onFileSendRequestReceived.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFileReceiveCallback(_tox, null, IntPtr.Zero);
                    _onFileReceiveCallback = null;
                }

                _onFileSendRequestReceived -= value;
            }
        }

        private EventHandler<ToxEventArgs.FileRequestChunkEventArgs> _onFileChunkRequested;

        /// <summary>
        /// Occurs when the core requests the next chunk of the file.
        /// </summary>
        public event EventHandler<ToxEventArgs.FileRequestChunkEventArgs> OnFileChunkRequested
        {
            add
            {
                if (_onFileRequestChunkCallback == null)
                {
                    _onFileRequestChunkCallback = (IntPtr tox, uint friendNumber, uint fileNumber, ulong position, uint length, IntPtr userData) =>
                    {
                        if (_onFileChunkRequested != null)
                            _onFileChunkRequested(this, new ToxEventArgs.FileRequestChunkEventArgs((int)friendNumber, (int)fileNumber, (long)position, (int)length));
                    };

                    ToxFunctions.RegisterFileChunkRequestCallback(_tox, _onFileRequestChunkCallback, IntPtr.Zero);
                }

                _onFileChunkRequested += value;
            }
            remove
            {
                if (_onFileChunkRequested.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFileChunkRequestCallback(_tox, null, IntPtr.Zero);
                    _onFileRequestChunkCallback = null;
                }

                _onFileChunkRequested -= value;
            }
        }

        private EventHandler<ToxEventArgs.FriendPacketEventArgs> _onFriendLossyPacketReceived;

        /// <summary>
        /// Occurs when a lossy packet from a friend is received.
        /// </summary>
        public event EventHandler<ToxEventArgs.FriendPacketEventArgs> OnFriendLossyPacketReceived
        {
            add
            {
                if (_onFriendLossyPacketCallback == null)
                {
                    _onFriendLossyPacketCallback = (IntPtr tox, uint friendNumber, byte[] data, uint length, IntPtr userData) =>
                    {
                        if (_onFriendLossyPacketReceived != null)
                            _onFriendLossyPacketReceived(this, new ToxEventArgs.FriendPacketEventArgs((int)friendNumber, data));
                    };

                    ToxFunctions.RegisterFriendLossyPacketCallback(_tox, _onFriendLossyPacketCallback, IntPtr.Zero);
                }

                _onFriendLossyPacketReceived += value;
            }
            remove
            {
                if (_onFriendLossyPacketReceived.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFriendLossyPacketCallback(_tox, null, IntPtr.Zero);
                    _onFriendLossyPacketCallback = null;
                }

                _onFriendLossyPacketReceived -= value;
            }
        }

        private EventHandler<ToxEventArgs.FriendPacketEventArgs> _onFriendLosslessPacketReceived;

        /// <summary>
        /// Occurs when a lossless packet from a friend is received.
        /// </summary>
        public event EventHandler<ToxEventArgs.FriendPacketEventArgs> OnFriendLosslessPacketReceived
        {
            add
            {
                if (_onFriendLosslessPacketCallback == null)
                {
                    _onFriendLosslessPacketCallback = (IntPtr tox, uint friendNumber, byte[] data, uint length, IntPtr userData) =>
                    {
                        if (_onFriendLosslessPacketReceived != null)
                            _onFriendLosslessPacketReceived(this, new ToxEventArgs.FriendPacketEventArgs((int)friendNumber, data));
                    };

                    ToxFunctions.RegisterFriendLosslessPacketCallback(_tox, _onFriendLosslessPacketCallback, IntPtr.Zero);
                }

                _onFriendLosslessPacketReceived += value;
            }
            remove
            {
                if (_onFriendLosslessPacketReceived.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFriendLosslessPacketCallback(_tox, null, IntPtr.Zero);
                    _onFriendLosslessPacketCallback = null;
                }

                _onFriendLosslessPacketReceived -= value;
            }
        }

        private EventHandler<ToxGroupEventArgs.PeerNameEventArgs> _onGroupPeerNameChanged;

        /// <summary>
        /// Occurs when a peer in a group changed his name.
        /// </summary>
        public event EventHandler<ToxGroupEventArgs.PeerNameEventArgs> OnGroupPeerNameChanged
        {
            add
            {
                if (_onGroupPeerNameCallback == null)
                {
                    _onGroupPeerNameCallback = (IntPtr tox, uint groupNumber, uint peerNumber, byte[] name, uint length, IntPtr userData) =>
                    {
                        if (_onGroupPeerNameChanged != null)
                            _onGroupPeerNameChanged(this, new ToxGroupEventArgs.PeerNameEventArgs((int)groupNumber, (int)peerNumber, Encoding.UTF8.GetString(name, 0, (int)length)));
                    };

                    ToxGroupFunctions.RegisterPeerNameCallback(_tox, _onGroupPeerNameCallback, IntPtr.Zero);
                }

                _onGroupPeerNameChanged += value;
            }
            remove
            {
                if (_onGroupPeerNameChanged.GetInvocationList().Length == 1)
                {
                    ToxGroupFunctions.RegisterPeerNameCallback(_tox, null, IntPtr.Zero);
                    _onGroupPeerNameCallback = null;
                }

                _onGroupPeerNameChanged -= value;
            }
        }

        private EventHandler<ToxGroupEventArgs.PeerStatusEventArgs> _onGroupPeerStatusChanged;

        /// <summary>
        /// Occurs when a peer in a group changed his status.
        /// </summary>
        public event EventHandler<ToxGroupEventArgs.PeerStatusEventArgs> OnGroupPeerStatusChanged
        {
            add
            {
                if (_onGroupPeerStatusCallback == null)
                {
                    _onGroupPeerStatusCallback = (IntPtr tox, uint groupNumber, uint peerNumber, ToxUserStatus status, IntPtr userData) =>
                    {
                        if (_onGroupPeerStatusChanged != null)
                            _onGroupPeerStatusChanged(this, new ToxGroupEventArgs.PeerStatusEventArgs((int)groupNumber, (int)peerNumber, status));
                    };

                    ToxGroupFunctions.RegisterPeerStatusCallback(_tox, _onGroupPeerStatusCallback, IntPtr.Zero);
                }

                _onGroupPeerStatusChanged += value;
            }
            remove
            {
                if (_onGroupPeerStatusChanged.GetInvocationList().Length == 1)
                {
                    ToxGroupFunctions.RegisterPeerStatusCallback(_tox, null, IntPtr.Zero);
                    _onGroupPeerStatusCallback = null;
                }

                _onGroupPeerStatusChanged -= value;
            }
        }

        private EventHandler<ToxGroupEventArgs.TopicEventArgs> _onGroupTopicChanged;

        /// <summary>
        /// Occurs when a peer changed the topic of a group.
        /// </summary>
        public event EventHandler<ToxGroupEventArgs.TopicEventArgs> OnGroupTopicChanged
        {
            add
            {
                if (_onGroupTopicCallback == null)
                {
                    _onGroupTopicCallback = (IntPtr tox, uint groupNumber, uint peerNumber, byte[] topic, uint length, IntPtr userData) =>
                    {
                        if (_onGroupTopicChanged != null)
                            _onGroupTopicChanged(this, new ToxGroupEventArgs.TopicEventArgs((int)groupNumber, (int)peerNumber, Encoding.UTF8.GetString(topic, 0, (int)length)));
                    };

                    ToxGroupFunctions.RegisterTopicCallback(_tox, _onGroupTopicCallback, IntPtr.Zero);
                }

                _onGroupTopicChanged += value;
            }
            remove
            {
                if (_onGroupTopicChanged.GetInvocationList().Length == 1)
                {
                    ToxGroupFunctions.RegisterTopicCallback(_tox, null, IntPtr.Zero);
                    _onGroupTopicCallback = null;
                }

                _onGroupTopicChanged -= value;
            }
        }

        private EventHandler<ToxGroupEventArgs.PrivacyStateEventArgs> _onGroupPrivacyStateChanged;

        /// <summary>
        /// Occurs when the privacy status of a group was changed.
        /// </summary>
        public event EventHandler<ToxGroupEventArgs.PrivacyStateEventArgs> OnGroupPrivacyStateChanged
        {
            add
            {
                if (_onGroupPrivacyStateCallback == null)
                {
                    _onGroupPrivacyStateCallback = (IntPtr tox, uint groupNumber, ToxGroupPrivacyState privacyState, IntPtr userData) =>
                    {
                        if (_onGroupPrivacyStateChanged != null)
                            _onGroupPrivacyStateChanged(this, new ToxGroupEventArgs.PrivacyStateEventArgs((int)groupNumber, privacyState));
                    };

                    ToxGroupFunctions.RegisterPrivacyStateCallback(_tox, _onGroupPrivacyStateCallback, IntPtr.Zero);
                }

                _onGroupPrivacyStateChanged += value;
            }
            remove
            {
                if (_onGroupPrivacyStateChanged.GetInvocationList().Length == 1)
                {
                    ToxGroupFunctions.RegisterPrivacyStateCallback(_tox, null, IntPtr.Zero);
                    _onGroupPrivacyStateCallback = null;
                }

                _onGroupPrivacyStateChanged -= value;
            }
        }

        private EventHandler<ToxGroupEventArgs.PeerLimitEventArgs> _onGroupPeerLimitChanged;

        /// <summary>
        /// Occurs when a the peer limit of a group was changed.
        /// </summary>
        public event EventHandler<ToxGroupEventArgs.PeerLimitEventArgs> OnGroupPeerLimitChanged
        {
            add
            {
                if (_onGroupPeerLimitCallback == null)
                {
                    _onGroupPeerLimitCallback = (IntPtr tox, uint groupNumber, uint peerLimit, IntPtr userData) =>
                    {
                        if (_onGroupPeerLimitChanged != null)
                            _onGroupPeerLimitChanged(this, new ToxGroupEventArgs.PeerLimitEventArgs((int)groupNumber, (int)peerLimit));
                    };

                    ToxGroupFunctions.RegisterPeerLimitCallback(_tox, _onGroupPeerLimitCallback, IntPtr.Zero);
                }

                _onGroupPeerLimitChanged += value;
            }
            remove
            {
                if (_onGroupPeerLimitChanged.GetInvocationList().Length == 1)
                {
                    ToxGroupFunctions.RegisterPeerLimitCallback(_tox, null, IntPtr.Zero);
                    _onGroupPeerLimitCallback = null;
                }

                _onGroupPeerLimitChanged -= value;
            }
        }

        private EventHandler<ToxGroupEventArgs.PasswordEventArgs> _onGroupPasswordhanged;

        /// <summary>
        /// Occurs when a the password of a group was changed.
        /// </summary>
        public event EventHandler<ToxGroupEventArgs.PasswordEventArgs> OnGroupPasswordhanged
        {
            add
            {
                if (_onGroupPasswordCallback == null)
                {
                    _onGroupPasswordCallback = (IntPtr tox, uint groupNumber, byte[] password, uint length, IntPtr userData) =>
                    {
                        if (_onGroupPasswordhanged != null)
                            _onGroupPasswordhanged(this, new ToxGroupEventArgs.PasswordEventArgs((int)groupNumber, Encoding.UTF8.GetString(password, 0, (int)length)));
                    };

                    ToxGroupFunctions.RegisterPasswordCallback(_tox, _onGroupPasswordCallback, IntPtr.Zero);
                }

                _onGroupPasswordhanged += value;
            }
            remove
            {
                if (_onGroupPasswordhanged.GetInvocationList().Length == 1)
                {
                    ToxGroupFunctions.RegisterPasswordCallback(_tox, null, IntPtr.Zero);
                    _onGroupPasswordCallback = null;
                }

                _onGroupPasswordhanged -= value;
            }
        }

        private EventHandler<ToxGroupEventArgs.MessageEventArgs> _onGroupMessageReceived;

        /// <summary>
        /// Occurs when message from a group was received.
        /// </summary>
        public event EventHandler<ToxGroupEventArgs.MessageEventArgs> OnGroupMessageReceived
        {
            add
            {
                if (_onGroupMessageCallback == null)
                {
                    _onGroupMessageCallback = (IntPtr tox, uint groupNumber, uint peerNumber, ToxMessageType type, byte[] message, uint length, IntPtr userData) =>
                    {
                        if (_onGroupMessageReceived != null)
                            _onGroupMessageReceived(this, new ToxGroupEventArgs.MessageEventArgs((int)groupNumber, (int)peerNumber, type, Encoding.UTF8.GetString(message, 0, (int)length)));
                    };

                    ToxGroupFunctions.RegisterMessageCallback(_tox, _onGroupMessageCallback, IntPtr.Zero);
                }

                _onGroupMessageReceived += value;
            }
            remove
            {
                if (_onGroupMessageReceived.GetInvocationList().Length == 1)
                {
                    ToxGroupFunctions.RegisterMessageCallback(_tox, null, IntPtr.Zero);
                    _onGroupMessageCallback = null;
                }

                _onGroupMessageReceived -= value;
            }
        }

        private EventHandler<ToxGroupEventArgs.PrivateMessageEventArgs> _onGroupPrivateMessageReceived;

        /// <summary>
        /// Occurs when private message from a peer in a group was received.
        /// </summary>
        public event EventHandler<ToxGroupEventArgs.PrivateMessageEventArgs> OnGroupPrivateMessageReceived
        {
            add
            {
                if (_onGroupPrivateMessageCallback == null)
                {
                    _onGroupPrivateMessageCallback = (IntPtr tox, uint groupNumber, uint peerNumber, byte[] message, uint length, IntPtr userData) =>
                    {
                        if (_onGroupPrivateMessageReceived != null)
                            _onGroupPrivateMessageReceived(this, new ToxGroupEventArgs.PrivateMessageEventArgs((int)groupNumber, (int)peerNumber, Encoding.UTF8.GetString(message, 0, (int)length)));
                    };

                    ToxGroupFunctions.RegisterPrivateMessageCallback(_tox, _onGroupPrivateMessageCallback, IntPtr.Zero);
                }

                _onGroupPrivateMessageReceived += value;
            }
            remove
            {
                if (_onGroupPrivateMessageReceived.GetInvocationList().Length == 1)
                {
                    ToxGroupFunctions.RegisterPrivateMessageCallback(_tox, null, IntPtr.Zero);
                    _onGroupPrivateMessageCallback = null;
                }

                _onGroupPrivateMessageReceived -= value;
            }
        }

        private EventHandler<ToxGroupEventArgs.InviteEventArgs> _onGroupInviteReceived;

        /// <summary>
        /// Occurs when an invite to a group was received from a friend.
        /// </summary>
        public event EventHandler<ToxGroupEventArgs.InviteEventArgs> OnGroupInviteReceived
        {
            add
            {
                if (_onGroupInviteCallback == null)
                {
                    _onGroupInviteCallback = (IntPtr tox, uint friendNumber, byte[] inviteData, uint length, IntPtr userData) =>
                    {
                        if (_onGroupInviteReceived != null)
                            _onGroupInviteReceived(this, new ToxGroupEventArgs.InviteEventArgs((int)friendNumber, inviteData));
                    };

                    ToxGroupFunctions.RegisterInviteCallback(_tox, _onGroupInviteCallback, IntPtr.Zero);
                }

                _onGroupInviteReceived += value;
            }
            remove
            {
                if (_onGroupInviteReceived.GetInvocationList().Length == 1)
                {
                    ToxGroupFunctions.RegisterInviteCallback(_tox, null, IntPtr.Zero);
                    _onGroupInviteCallback = null;
                }

                _onGroupInviteReceived -= value;
            }
        }

        private EventHandler<ToxGroupEventArgs.PeerJoinEventArgs> _onGroupPeerJoined;

        /// <summary>
        /// Occurs when a new peer joined a groupchat (will not be triggered if we join the group ourselves).
        /// </summary>
        public event EventHandler<ToxGroupEventArgs.PeerJoinEventArgs> OnGroupPeerJoined
        {
            add
            {
                if (_onGroupPeerJoinCallback == null)
                {
                    _onGroupPeerJoinCallback = (IntPtr tox, uint groupNumber, uint peerNumber, IntPtr userData) =>
                    {
                        if (_onGroupPeerJoined != null)
                            _onGroupPeerJoined(this, new ToxGroupEventArgs.PeerJoinEventArgs((int)groupNumber, (int)peerNumber));
                    };

                    ToxGroupFunctions.RegisterPeerJoinCallback(_tox, _onGroupPeerJoinCallback, IntPtr.Zero);
                }

                _onGroupPeerJoined += value;
            }
            remove
            {
                if (_onGroupPeerJoined.GetInvocationList().Length == 1)
                {
                    ToxGroupFunctions.RegisterPeerJoinCallback(_tox, null, IntPtr.Zero);
                    _onGroupPeerJoinCallback = null;
                }

                _onGroupPeerJoined -= value;
            }
        }

        private EventHandler<ToxGroupEventArgs.PeerExitEventArgs> _onGroupPeerLeft;

        /// <summary>
        /// Occurs when a new peer left a groupchat (will not be triggered if we leave the group ourselves).
        /// </summary>
        public event EventHandler<ToxGroupEventArgs.PeerExitEventArgs> OnGroupPeerLeft
        {
            add
            {
                if (_onGroupPeerExitCallback == null)
                {
                    _onGroupPeerExitCallback = (IntPtr tox, uint groupNumber, uint peerNumber, byte[] message, uint length, IntPtr userData) =>
                    {
                        if (_onGroupPeerLeft != null)
                            _onGroupPeerLeft(this, new ToxGroupEventArgs.PeerExitEventArgs((int)groupNumber, (int)peerNumber, Encoding.UTF8.GetString(message, 0, (int)length)));
                    };

                    ToxGroupFunctions.RegisterPeerExitCallback(_tox, _onGroupPeerExitCallback, IntPtr.Zero);
                }

                _onGroupPeerLeft += value;
            }
            remove
            {
                if (_onGroupPeerLeft.GetInvocationList().Length == 1)
                {
                    ToxGroupFunctions.RegisterPeerExitCallback(_tox, null, IntPtr.Zero);
                    _onGroupPeerExitCallback = null;
                }

                _onGroupPeerLeft -= value;
            }
        }

        private EventHandler<ToxGroupEventArgs.SelfJoinEventArgs> _onGroupJoined;

        /// <summary>
        /// Occurs when we successfully joined a groupchat.
        /// </summary>
        public event EventHandler<ToxGroupEventArgs.SelfJoinEventArgs> OnGroupJoined
        {
            add
            {
                if (_onGroupSelfJoinCallback == null)
                {
                    _onGroupSelfJoinCallback = (IntPtr tox, uint groupNumber, IntPtr userData) =>
                    {
                        if (_onGroupJoined != null)
                            _onGroupJoined(this, new ToxGroupEventArgs.SelfJoinEventArgs((int)groupNumber));
                    };

                    ToxGroupFunctions.RegisterSelfJoinCallback(_tox, _onGroupSelfJoinCallback, IntPtr.Zero);
                }

                _onGroupJoined += value;
            }
            remove
            {
                if (_onGroupJoined.GetInvocationList().Length == 1)
                {
                    ToxGroupFunctions.RegisterSelfJoinCallback(_tox, null, IntPtr.Zero);
                    _onGroupSelfJoinCallback = null;
                }

                _onGroupJoined -= value;
            }
        }

        private EventHandler<ToxGroupEventArgs.JoinFailEventArgs> _onGroupJoinFailed;

        /// <summary>
        /// Occurs when we failed to join a groupchat.
        /// </summary>
        public event EventHandler<ToxGroupEventArgs.JoinFailEventArgs> OnGroupJoinFailed
        {
            add
            {
                if (_onGroupJoinFailCallback == null)
                {
                    _onGroupJoinFailCallback = (IntPtr tox, uint groupNumber, ToxGroupJoinFail failType, IntPtr userData) =>
                    {
                        if (_onGroupJoinFailed != null)
                            _onGroupJoinFailed(this, new ToxGroupEventArgs.JoinFailEventArgs((int)groupNumber, failType));
                    };

                    ToxGroupFunctions.RegisterJoinFailCallback(_tox, _onGroupJoinFailCallback, IntPtr.Zero);
                }

                _onGroupJoinFailed += value;
            }
            remove
            {
                if (_onGroupJoinFailed.GetInvocationList().Length == 1)
                {
                    ToxGroupFunctions.RegisterJoinFailCallback(_tox, null, IntPtr.Zero);
                    _onGroupJoinFailCallback = null;
                }

                _onGroupJoinFailed -= value;
            }
        }

        private EventHandler<ToxGroupEventArgs.ModActionEventArgs> _onGroupModerationAction;

        /// <summary>
        /// Occurs when a moderation action occurred in a groupchat.
        /// </summary>
        public event EventHandler<ToxGroupEventArgs.ModActionEventArgs> OnGroupModerationAction
        {
            add
            {
                if (_onGroupModerationCallback == null)
                {
                    _onGroupModerationCallback = (IntPtr tox, uint groupNumber, uint sourcePeerNumber, uint targetPeerNumber, ToxGroupModEvent modEvent, IntPtr userData) =>
                    {
                        if (_onGroupModerationAction != null)
                            _onGroupModerationAction(this, new ToxGroupEventArgs.ModActionEventArgs((int)groupNumber, (int)sourcePeerNumber, (int)targetPeerNumber, modEvent));
                    };

                    ToxGroupFunctions.RegisterModerationCallback(_tox, _onGroupModerationCallback, IntPtr.Zero);
                }

                _onGroupModerationAction += value;
            }
            remove
            {
                if (_onGroupModerationAction.GetInvocationList().Length == 1)
                {
                    ToxGroupFunctions.RegisterModerationCallback(_tox, null, IntPtr.Zero);
                    _onGroupModerationCallback = null;
                }

                _onGroupModerationAction -= value;
            }
        }

        #endregion

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
