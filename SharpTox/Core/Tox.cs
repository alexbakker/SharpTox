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
        private ToxDelegates.CallbackGroupInviteDelegate _onGroupInviteCallback;
        private ToxDelegates.CallbackGroupActionDelegate _onGroupActionCallback;
        private ToxDelegates.CallbackGroupMessageDelegate _onGroupMessageCallback;
        private ToxDelegates.CallbackGroupNamelistChangeDelegate _onGroupNamelistChangeCallback;
        private ToxDelegates.CallbackGroupTitleDelegate _onGroupTitleCallback;
        #endregion

        /// <summary>
        /// Options used for this instance of Tox.
        /// </summary>
        public ToxOptions Options { get; private set; }

        /// <summary>
        /// Whether or not we're connected to the DHT.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

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
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                byte[] bytes = new byte[ToxFunctions.SelfGetNameSize(_tox)];
                ToxFunctions.SelfGetName(_tox, bytes);

                return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            }
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

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
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                uint size = ToxFunctions.SelfGetStatusMessageSize(_tox);
                byte[] status = new byte[size];

                ToxFunctions.SelfGetStatusMessage(_tox, status);

                return Encoding.UTF8.GetString(status, 0, status.Length);
            }
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                byte[] msg = Encoding.UTF8.GetBytes(value);
                var error = ToxErrorSetInfo.Ok;
                ToxFunctions.SelfSetStatusMessage(_tox, msg, (uint)msg.Length, ref error);
            }
        }

        /// <summary>
        /// The string of a 32 byte long Tox Id to share with others.
        /// </summary>
        public ToxId Id
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                byte[] address = new byte[38];
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
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

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
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return ToxFunctions.SelfGetStatus(_tox);
            }
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                ToxFunctions.SelfSetStatus(_tox, value);
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
        /// Initializes a new instance of Tox.
        /// </summary>
        /// <param name="options">The options to initialize this instance of Tox with.</param>
        public Tox(ToxOptions options)
        {
            var error = ToxErrorNew.Ok;
            _tox = ToxFunctions.New(ref options, null, 0, ref error);

            if (_tox == null || _tox.IsInvalid || error != ToxErrorNew.Ok)
                throw new Exception("Could not create a new instance of tox, error: " + error.ToString());

            Options = options;
        }

        /// <summary>
        /// Initializes a new instance of Tox.
        /// </summary>
        /// <param name="options">The options to initialize this instance of Tox with.</param>
        /// <param name="data">A byte array containing Tox save data.</param>
        /// <param name="key">The key to decrypt encrypted Tox data. This should be null if the data is not encrypted.</param>
        public Tox(ToxOptions options, ToxData data, ToxEncryptionKey key = null)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (key == null || !data.IsEncrypted)
            {
                var error = ToxErrorNew.Ok;
                _tox = ToxFunctions.New(ref options, data.Bytes, (uint)data.Bytes.Length, ref error);

                if (_tox == null || _tox.IsInvalid || error != ToxErrorNew.Ok)
                    throw new Exception("Could not create a new instance of tox, error: " + error.ToString());
            }
            else
            {
                var error = ToxErrorNew.Ok;
                var decryptError = ToxErrorDecryption.Ok;
                byte[] decryptedData = ToxEncryption.DecryptData(data.Bytes, key, out decryptError);

                _tox = ToxFunctions.New(ref options, decryptedData, (uint)decryptedData.Length, ref error);

                if (_tox == null || _tox.IsInvalid || error != ToxErrorNew.Ok || decryptError != ToxErrorDecryption.Ok)
                    throw new Exception(string.Format("Could not create a new instance of tox, error: {0}, decrypt error: {1}" + error.ToString(), decryptError.ToString()));
            }

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

            if (!_tox.IsInvalid && !_tox.IsClosed && _tox != null)
                _tox.Dispose();

            _disposed = true;
        }

        /// <summary>
        /// Starts the main tox_do loop.
        /// </summary>
        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_running)
                return;

            Loop();
        }

        /// <summary>
        /// Stops the main tox_do loop if it's running.
        /// </summary>
        public void Stop()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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
        /// Runs the tox_do once in the current thread.
        /// </summary>
        /// <returns>The next timeout.</returns>
        public int Iterate()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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

            Task.Factory.StartNew(async() =>
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
        /// <param name="id">The address of the friend.</param>
        /// <param name="message">The message that will be sent along with the friend request.</param>
        /// <param name="error"></param>
        /// <returns>The friend number.</returns>
        public int AddFriend(ToxId id, string message, out ToxErrorFriendAdd error)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] msg = Encoding.UTF8.GetBytes(message);
            error = ToxErrorFriendAdd.Ok;

            return (int)ToxFunctions.FriendAdd(_tox, id.Bytes, msg, (uint)msg.Length, ref error);
        }

        /// <summary>
        /// Adds a friend to the friend list and sends a friend request.
        /// </summary>
        /// <param name="id">The address of the friend.</param>
        /// <param name="message">The message that will be sent along with the friend request.</param>
        /// <returns>The friend number.</returns>
        public int AddFriend(ToxId id, string message)
        {
            var error = ToxErrorFriendAdd.Ok;
            return AddFriend(id, message, out error);
        }

        /// <summary>
        /// Adds a friend to the friend list without sending a request.
        /// </summary>
        /// <param name="publicKey">The public key of the friend to add.</param>
        /// <param name="error"></param>
        /// <returns>The friend number.</returns>
        public int AddFriendNoRequest(ToxKey publicKey, out ToxErrorFriendAdd error)
        {
            if (publicKey == null)
                throw new ArgumentNullException("publicKey");

            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorFriendAdd.Ok;
            return (int)ToxFunctions.FriendAddNoRequest(_tox, publicKey.GetBytes(), ref error);
        }

        /// <summary>
        /// Adds a friend to the friend list without sending a request.
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
        /// </summary>
        /// <param name="node">The node to add.</param>
        /// <param name="error"></param>
        /// <returns>True on success.</returns>
        public bool AddTcpRelay(ToxNode node, out ToxErrorBootstrap error)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorBootstrap.Ok;
            return ToxFunctions.AddTcpRelay(_tox, node.Address, (ushort)node.Port, node.PublicKey.GetBytes(), ref error);
        }

        /// <summary>
        /// Adds a node as a TCP relay.
        /// </summary>
        /// <param name="node">The node to add.</param>
        /// <returns>True on success.</returns>
        public bool AddTcpRelay(ToxNode node)
        {
            var error = ToxErrorBootstrap.Ok;
            return AddTcpRelay(node, out error);
        }

        /// <summary>
        /// Attempts to bootstrap this Tox instance with a ToxNode.
        /// </summary>
        /// <param name="node">The node to bootstrap off of.</param>
        /// <param name="error"></param>
        /// <returns>True on success.</returns>
        public bool Bootstrap(ToxNode node, out ToxErrorBootstrap error)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorBootstrap.Ok;
            return ToxFunctions.Bootstrap(_tox, node.Address, (ushort)node.Port, node.PublicKey.GetBytes(), ref error);
        }

        /// <summary>
        /// Attempts to bootstrap this Tox instance with a ToxNode.
        /// </summary>
        /// <param name="node">The node to bootstrap off of.</param>
        /// <returns>True on success.</returns>
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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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
            if (publicKey == null)
                throw new ArgumentNullException("publicKey");

            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorFriendQuery.Ok;
            return ToxFunctions.FriendGetConnectionStatus(_tox, (uint)friendNumber, ref error);
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
        /// Retrieves a friend's public key.
        /// </summary>
        /// <param name="friendNumber">The friend number to retrieve the public key of.</param>
        /// <param name="error"></param>
        /// <returns>The friend's public key on success.</returns>
        public ToxKey GetFriendPublicKey(int friendNumber, out ToxErrorFriendGetPublicKey error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] address = new byte[ToxConstants.PublicKeySize];
            error = ToxErrorFriendGetPublicKey.Ok;
            ToxFunctions.FriendGetPublicKey(_tox, (uint)friendNumber, address, ref error);

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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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
        /// Retrieves a ToxData object that contains the data of this Tox instance.
        /// </summary>
        /// <returns></returns>
        public ToxData GetData()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] bytes = new byte[ToxFunctions.GetSaveDataSize(_tox)];
            ToxFunctions.GetSaveData(_tox, bytes);

            return new ToxData(bytes);
        }

        /// <summary>
        /// Retrieves a ToxData object that contains the data of this Tox instance, encrypted with the provided key.
        /// </summary>
        /// <param name="key">The key to encrypt the Tox data with.</param>
        /// <returns></returns>
        public ToxData GetData(ToxEncryptionKey key)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            var data = GetData();
            byte[] encrypted = ToxEncryption.EncryptData(data.Bytes, key);

            return new ToxData(encrypted);
        }

        /// <summary>
        /// Retrieves the private key of this Tox instance.
        /// </summary>
        /// <returns>The private key of this Tox instance.</returns>
        public ToxKey GetPrivateKey()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorFriendQuery.Ok;
            uint size = ToxFunctions.FriendGetNameSize(_tox, (uint)friendNumber, ref error);

            if (error != ToxErrorFriendQuery.Ok)
                return string.Empty;

            byte[] name = new byte[size];
            ToxFunctions.FriendGetName(_tox, (uint)friendNumber, name, ref error);

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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorFriendQuery.Ok;
            uint size = ToxFunctions.FriendGetStatusMessageSize(_tox, (uint)friendNumber, ref error);

            if (error != ToxErrorFriendQuery.Ok)
                return string.Empty;

            byte[] message = new byte[size];
            ToxFunctions.FriendGetStatusMessage(_tox, (uint)friendNumber, message, ref error);

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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorGetPort.Ok;
            return ToxFunctions.SelfGetUdpPort(_tox, ref error);
        }

        /// <summary>
        /// Retrieves the UDP port this instance of Tox is bound to.
        /// </summary>
        /// <param name="error"></param>
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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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
        public void SetNospam(uint nospam)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            ToxFunctions.SelfSetNospam(_tox, nospam);
        }

        /// <summary>
        /// Retrieves the nospam value of this Tox instance.
        /// </summary>
        /// <returns>The nospam value.</returns>
        public uint GetNospam()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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
        /// <param name="kind">The kind of file that will be transfered.</param>
        /// <param name="fileSize">The size of the file that will be transfered.</param>
        /// <param name="fileName">The filename of the file that will be transfered.</param>
        /// <param name="error"></param>
        /// <returns>Info about the file transfer on success.</returns>
        public ToxFileInfo FileSend(int friendNumber, ToxFileKind kind, long fileSize, byte[] fileName, out ToxErrorFileSend error)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorFileSend.Ok;
            byte[] fileId = new byte[ToxConstants.FileIdLength];
            int fileNumber = (int)ToxFunctions.FileSend(_tox, (uint)friendNumber, kind, (ulong)fileSize, fileId, fileName, (uint)fileName.Length, ref error);
            
            return new ToxFileInfo(fileNumber, fileId);
        }

        /// <summary>
        /// Send a file transmission request.
        /// </summary>
        /// <param name="friendNumber">The friend number to send the request to.</param>
        /// <param name="kind">The kind of file that will be transfered.</param>
        /// <param name="fileSize">The size of the file that will be transfered.</param>
        /// <param name="fileName">The filename of the file that will be transfered.</param>
        /// <returns>Info about the file transfer on success.</returns>
        public ToxFileInfo FileSend(int friendNumber, ToxFileKind kind, long fileSize, byte[] fileName)
        {
            var error = ToxErrorFileSend.Ok;
            return FileSend(friendNumber, kind, fileSize, fileName, out error);
        }

        /// <summary>
        /// Send a file transmission request.
        /// </summary>
        /// <param name="friendNumber">The friend number to send the request to.</param>
        /// <param name="kind">The kind of file that will be transfered.</param>
        /// <param name="fileSize">The size of the file that will be transfered.</param>
        /// <param name="fileName">The filename of the file that will be transfered.</param>
        /// <param name="error"></param>
        /// <returns>Info about the file transfer on success.</returns>
        public ToxFileInfo FileSend(int friendNumber, ToxFileKind kind, long fileSize, string fileName, out ToxErrorFileSend error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorFileSend.Ok;
            byte[] fileId = new byte[ToxConstants.FileIdLength];
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
            int fileNumber = (int)ToxFunctions.FileSend(_tox, (uint)friendNumber, kind, (ulong)fileSize, fileId, fileNameBytes, (uint)fileNameBytes.Length, ref error);

            return new ToxFileInfo(fileNumber, fileId);
        }

        /// <summary>
        /// Send a file transmission request.
        /// </summary>
        /// <param name="friendNumber">The friend number to send the request to.</param>
        /// <param name="kind">The kind of file that will be transfered.</param>
        /// <param name="fileSize">The size of the file that will be transfered.</param>
        /// <param name="fileName">The filename of the file that will be transfered.</param>
        /// <returns>Info about the file transfer on success.</returns>
        public ToxFileInfo FileSend(int friendNumber, ToxFileKind kind, long fileSize, string fileName)
        {
            var error = ToxErrorFileSend.Ok;
            return FileSend(friendNumber, kind, fileSize, fileName, out error);
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
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

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
        /// Sends a chunk of file data to a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="fileNumber"></param>
        /// <param name="position"></param>
        /// <param name="data"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool FileSendChunk(int friendNumber, int fileNumber, long position, byte[] data, out ToxErrorFileSendChunk error)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorFileSendChunk.Ok;

            return ToxFunctions.FileSendChunk(_tox, (uint)friendNumber, (uint)fileNumber, (ulong)position, data, (uint)data.Length, ref error);
        }

        /// <summary>
        /// Sends a chunk of file data to a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="fileNumber"></param>
        /// <param name="position"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool FileSendChunk(int friendNumber, int fileNumber, long position, byte[] data)
        {
            var error = ToxErrorFileSendChunk.Ok;
            return FileSendChunk(friendNumber, fileNumber, position, data, out error);
        }
        
        /// <summary>
        /// Retrieves the unique id of a file transfer. This can be used to uniquely identify file transfers across core restarts.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="fileNumber"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public byte[] FileGetId(int friendNumber, int fileNumber, out ToxErrorFileGet error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorFileGet.Ok;
            byte[] id = new byte[ToxConstants.FileIdLength];
            ToxFunctions.FileGetFileId(_tox, (uint)friendNumber, (uint)fileNumber, id, ref error);

            return id;
        }

        /// <summary>
        /// Retrieves the unique id of a file transfer. This can be used to uniquely identify file transfers across core restarts.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="fileNumber"></param>
        /// <returns></returns>
        public byte[] FileGetId(int friendNumber, int fileNumber)
        {
            var error = ToxErrorFileGet.Ok;
            return FileGetId(friendNumber, fileNumber, out error);
        }

        /// <summary>
        /// Send a custom lossy packet to a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="data"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool FriendSendLossyPacket(int friendNumber, byte[] data, out ToxErrorFriendCustomPacket error)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorFriendCustomPacket.Ok;

            return ToxFunctions.FriendSendLossyPacket(_tox, (uint)friendNumber, data, (uint)data.Length, ref error);
        }

        /// <summary>
        /// Send a custom lossy packet to a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool FriendSendLossyPacket(int friendNumber, byte[] data)
        {
            var error = ToxErrorFriendCustomPacket.Ok;
            return FriendSendLossyPacket(friendNumber, data, out error);
        }

        /// <summary>
        /// Send a custom lossless packet to a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="data"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool FriendSendLosslessPacket(int friendNumber, byte[] data, out ToxErrorFriendCustomPacket error)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorFriendCustomPacket.Ok;

            return ToxFunctions.FriendSendLosslessPacket(_tox, (uint)friendNumber, data, (uint)data.Length, ref error);
        }

        /// <summary>
        /// Send a custom lossless packet to a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool FriendSendLosslessPacket(int friendNumber, byte[] data)
        {
            var error = ToxErrorFriendCustomPacket.Ok;
            return FriendSendLosslessPacket(friendNumber, data, out error);
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

                names[i] = Encoding.UTF8.GetString(name, 0, name.Length);
            }

            return names;
        }

        /// <summary>
        /// Joins a group with the given public key of the group.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="data">Data obtained from the OnGroupInvite event.</param>
        /// <returns></returns>
        public int JoinGroup(int friendNumber, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

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
        /// Changes the title of a group.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public bool SetGroupTitle(int groupNumber, string title)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (Encoding.UTF8.GetByteCount(title) > ToxConstants.MaxNameLength)
                throw new ArgumentException("The specified group title is longer than 256 bytes");

            byte[] bytes = Encoding.UTF8.GetBytes(title);

            return ToxFunctions.GroupSetTitle(_tox, groupNumber, bytes, (byte)bytes.Length) == 0;
        }

        /// <summary>
        /// Retrieves the type of a group.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <returns></returns>
        public ToxGroupType GetGroupType(int groupNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return (ToxGroupType)ToxFunctions.GroupGetType(_tox, groupNumber);
        }

        /// <summary>
        /// Retrieves the title of a group.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <returns></returns>
        public string GetGroupTitle(int groupNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] title = new byte[ToxConstants.MaxNameLength];
            int length = ToxFunctions.GroupGetTitle(_tox, groupNumber, title, (uint)title.Length);

            if (length == -1)
                return string.Empty;

            return Encoding.UTF8.GetString(title, 0, length);
        }

        /// <summary>
        /// Retrieves the public key of a peer.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="peerNumber"></param>
        /// <returns></returns>
        public ToxKey GetGroupPeerPublicKey(int groupNumber, int peerNumber)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] key = new byte[ToxConstants.PublicKeySize];
            int result = ToxFunctions.GroupPeerPubkey(_tox, groupNumber, peerNumber, key);

            if (result != 0)
                return null;

            return new ToxKey(ToxKeyType.Public, key);
        }

        /// <summary>
        /// Retrieves the time a friend was last seen online.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public DateTime GetFriendLastOnline(int friendNumber, out ToxErrorFriendGetLastOnline error)
        {
            error = ToxErrorFriendGetLastOnline.Ok;
            ulong time = ToxFunctions.FriendGetLastOnline(_tox, (uint)friendNumber, ref error);

            return ToxTools.EpochToDateTime(time);
        }

        /// <summary>
        /// Retrieves the time a friend was last seen online.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public DateTime GetFriendLastOnline(int friendNumber)
        {
            var error = ToxErrorFriendGetLastOnline.Ok;
            return GetFriendLastOnline(friendNumber, out error);
        }

        #region Events
        private EventHandler<ToxEventArgs.FriendRequestEventArgs> _onFriendRequest;

        /// <summary>
        /// Occurs when a friend request is received.
        /// </summary>
        public event EventHandler<ToxEventArgs.FriendRequestEventArgs> OnFriendRequestReceived
        {
            add
            {
                if (_onFriendRequestCallback == null)
                {
                    _onFriendRequestCallback = (IntPtr tox, byte[] publicKey, byte[] message, uint length, IntPtr userData) =>
                    {
                        if (_onFriendRequest != null)
                            _onFriendRequest(this, new ToxEventArgs.FriendRequestEventArgs(new ToxKey(ToxKeyType.Public, ToxTools.HexBinToString(publicKey)), Encoding.UTF8.GetString(message, 0, (int)length)));
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

        private EventHandler<ToxEventArgs.FriendMessageEventArgs> _onFriendMessage;

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
                        if (_onFriendMessage != null)
                            _onFriendMessage(this, new ToxEventArgs.FriendMessageEventArgs((int)friendNumber, Encoding.UTF8.GetString(message, 0, (int)length), type));
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

        private EventHandler<ToxEventArgs.NameChangeEventArgs> _onNameChange;

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
                        if (_onNameChange != null)
                            _onNameChange(this, new ToxEventArgs.NameChangeEventArgs((int)friendNumber, Encoding.UTF8.GetString(newName, 0, (int)length)));
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
        public event EventHandler<ToxEventArgs.StatusMessageEventArgs> OnFriendStatusMessageChanged
        {
            add
            {
                if (_onStatusMessageCallback == null)
                {
                    _onStatusMessageCallback = (IntPtr tox, uint friendNumber, byte[] newStatus, uint length, IntPtr userData) =>
                    {
                        if (_onStatusMessage != null)
                            _onStatusMessage(this, new ToxEventArgs.StatusMessageEventArgs((int)friendNumber, Encoding.UTF8.GetString(newStatus, 0, (int)length)));
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

        private EventHandler<ToxEventArgs.TypingStatusEventArgs> _onTypingChange;

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
                        if (_onTypingChange != null)
                            _onTypingChange(this, new ToxEventArgs.TypingStatusEventArgs((int)friendNumber, typing));
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

        private EventHandler<ToxEventArgs.ConnectionStatusEventArgs> _onConnectionStatus;

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
                        if (_onConnectionStatus != null)
                            _onConnectionStatus(this, new ToxEventArgs.ConnectionStatusEventArgs(status));
                    };

                    ToxFunctions.RegisterConnectionStatusCallback(_tox, _onConnectionStatusCallback, IntPtr.Zero);
                }

                _onConnectionStatus += value;
            }
            remove
            {
                if (_onConnectionStatus.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterConnectionStatusCallback(_tox, null, IntPtr.Zero);
                    _onConnectionStatusCallback = null;
                }

                _onConnectionStatus -= value;
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

        private EventHandler<ToxEventArgs.ReadReceiptEventArgs> _onReadReceipt;

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
                        if (_onReadReceipt != null)
                            _onReadReceipt(this, new ToxEventArgs.ReadReceiptEventArgs((int)friendNumber, (int)receipt));
                    };

                    ToxFunctions.RegisterFriendReadReceiptCallback(_tox, _onReadReceiptCallback, IntPtr.Zero);
                }

                _onReadReceipt += value;
            }
            remove
            {
                if (_onReadReceipt.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFriendReadReceiptCallback(_tox, null, IntPtr.Zero);
                    _onReadReceiptCallback = null;
                }

                _onReadReceipt -= value;
            }
        }

        private EventHandler<ToxEventArgs.FileControlEventArgs> _onFileControl;

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
                        if (_onFileControl != null)
                            _onFileControl(this, new ToxEventArgs.FileControlEventArgs((int)friendNumber, (int)fileNumber, control));
                    };

                    ToxFunctions.RegisterFileControlRecvCallback(_tox, _onFileControlCallback, IntPtr.Zero);
                }

                _onFileControl += value;
            }
            remove
            {
                if (_onFileControl.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFileControlRecvCallback(_tox, null, IntPtr.Zero);
                    _onFileControlCallback = null;
                }

                _onFileControl -= value;
            }
        }

        private EventHandler<ToxEventArgs.FileChunkEventArgs> _onFileChunk;

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
                        if (_onFileChunk != null)
                            _onFileChunk(this, new ToxEventArgs.FileChunkEventArgs((int)friendNumber, (int)fileNumber, data, (long)position));
                    };

                    ToxFunctions.RegisterFileReceiveChunkCallback(_tox, _onFileReceiveChunkCallback, IntPtr.Zero);
                }

                _onFileChunk += value;
            }
            remove
            {
                if (_onFileChunk.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFileReceiveChunkCallback(_tox, null, IntPtr.Zero);
                    _onFileReceiveChunkCallback = null;
                }

                _onFileChunk -= value;
            }
        }

        private EventHandler<ToxEventArgs.FileSendRequestEventArgs> _onFileReceive;

        /// <summary>
        /// Occurs when a file control is received.
        /// </summary>
        public event EventHandler<ToxEventArgs.FileSendRequestEventArgs> OnFileSendRequestReceived
        {
            add
            {
                if (_onFileReceiveCallback == null)
                {
                    _onFileReceiveCallback = (IntPtr tox, uint friendNumber, uint fileNumber, ToxFileKind kind, ulong fileSize, byte[] filename, uint filenameLength, IntPtr userData) =>
                    {
                        if (_onFileReceive != null)
                            _onFileReceive(this, new ToxEventArgs.FileSendRequestEventArgs((int)friendNumber, (int)fileNumber, kind, (long)fileSize, filename == null ? string.Empty : Encoding.UTF8.GetString(filename, 0, filename.Length)));
                    };

                    ToxFunctions.RegisterFileReceiveCallback(_tox, _onFileReceiveCallback, IntPtr.Zero);
                }

                _onFileReceive += value;
            }
            remove
            {
                if (_onFileReceive.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFileReceiveCallback(_tox, null, IntPtr.Zero);
                    _onFileReceiveCallback = null;
                }

                _onFileReceive -= value;
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

        private EventHandler<ToxEventArgs.FriendPacketEventArgs> _onFriendLossyPacket;

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
                        if (_onFriendLossyPacket != null)
                            _onFriendLossyPacket(this, new ToxEventArgs.FriendPacketEventArgs((int)friendNumber, data));
                    };

                    ToxFunctions.RegisterFriendLossyPacketCallback(_tox, _onFriendLossyPacketCallback, IntPtr.Zero);
                }

                _onFriendLossyPacket += value;
            }
            remove
            {
                if (_onFriendLossyPacket.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFriendLossyPacketCallback(_tox, null, IntPtr.Zero);
                    _onFriendLossyPacketCallback = null;
                }

                _onFriendLossyPacket -= value;
            }
        }

        private EventHandler<ToxEventArgs.FriendPacketEventArgs> _onFriendLosslessPacket;

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
                        if (_onFriendLosslessPacket != null)
                            _onFriendLosslessPacket(this, new ToxEventArgs.FriendPacketEventArgs((int)friendNumber, data));
                    };

                    ToxFunctions.RegisterFriendLosslessPacketCallback(_tox, _onFriendLosslessPacketCallback, IntPtr.Zero);
                }

                _onFriendLosslessPacket += value;
            }
            remove
            {
                if (_onFriendLosslessPacket.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFriendLosslessPacketCallback(_tox, null, IntPtr.Zero);
                    _onFriendLosslessPacketCallback = null;
                }

                _onFriendLosslessPacket -= value;
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
                            _onGroupAction(this, new ToxEventArgs.GroupActionEventArgs(groupNumber, peerNumber, Encoding.UTF8.GetString(action, 0, length)));
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
                        if (_onGroupMessage != null)
                            _onGroupMessage(this, new ToxEventArgs.GroupMessageEventArgs(groupNumber, peerNumber, Encoding.UTF8.GetString(message, 0, length)));
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
                        if (_onGroupInvite != null)
                            _onGroupInvite(this, new ToxEventArgs.GroupInviteEventArgs(friendNumber, (ToxGroupType)type, data));
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
                        if (_onGroupNamelistChange != null)
                            _onGroupNamelistChange(this, new ToxEventArgs.GroupNamelistChangeEventArgs(groupNumber, peerNumber, change));
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
                            _onGroupTitleChanged(this, new ToxEventArgs.GroupTitleEventArgs(groupNumber, peerNumber, Encoding.UTF8.GetString(title, 0, length)));
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

        #endregion
    }
}

#pragma warning restore 1591