#pragma warning disable 1591

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SharpTox.Core
{
    public delegate object InvokeDelegate(Delegate method, params object[] p);

    /// <summary>
    /// Represents an instance of Tox.
    /// </summary>
    public class Tox : IDisposable
    {
        private ToxHandle _tox;
        private CancellationTokenSource _cancelTokenSource;

        private bool _running = false;
        private bool _disposed = false;
        private bool _connected = false;

        #region Callback delegates
        private ToxDelegates.CallbackFriendRequestDelegate _onFriendRequestCallback;
        private ToxDelegates.CallbackFriendMessageDelegate _onFriendMessageCallback;
        private ToxDelegates.CallbackFriendMessageDelegate _onFriendActionCallback;
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

                return ToxFunctions.GetConnectionStatus(_tox) != ToxConnectionStatus.None;
            }
        }

        /// <summary>
        /// An array of friendnumbers of this Tox instance.
        /// </summary>
        public int[] Friends
        {
            get
            {
                uint size = ToxFunctions.FriendListSize(_tox);
                uint[] friends = new uint[size];
                ToxFunctions.FriendList(_tox, friends);

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

                return ToxTools.RemoveNull(Encoding.UTF8.GetString(bytes, 0, bytes.Length));
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

        public string StatusMessage
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                uint size = ToxFunctions.SelfGetStatusMessageSize(_tox);
                byte[] status = new byte[size];

                ToxFunctions.SelfGetStatusMessage(_tox, status);

                return ToxTools.RemoveNull(Encoding.UTF8.GetString(status, 0, status.Length));
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
                ToxFunctions.GetDhtId(_tox, publicKey);

                return new ToxKey(ToxKeyType.Public, publicKey);
            }
        }

        /// <summary>
        /// Current user status of this Tox instance.
        /// </summary>
        public ToxStatus Status
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
        /// <param name="options"></param>
        public Tox(ToxOptions options)
        {
            var error = ToxErrorNew.Ok;
            _tox = ToxFunctions.New(ref options, null, 0, ref error);

            if (_tox == null || _tox.IsInvalid || error != ToxErrorNew.Ok)
                throw new Exception("Could not create a new instance of tox");

            Options = options;
        }

        /// <summary>
        /// Initializes a new instance of Tox.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="data"></param>
        public Tox(ToxOptions options, byte[] data)
        {
            var error = ToxErrorNew.Ok;
            _tox = ToxFunctions.New(ref options, data, (uint)data.Length, ref error);

            if (_tox == null || _tox.IsInvalid || error != ToxErrorNew.Ok)
                throw new Exception("Could not create a new instance of tox");

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
        /// Runs the loop once in the current thread and returns the next timeout.
        /// </summary>
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
            ToxFunctions.Iteration(_tox);
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

                    if (IsConnected && !_connected)
                    {
                        //if (OnConnected != null)
                        //    Invoker(OnConnected, this, new ToxEventArgs.ConnectionEventArgs(true));

                        _connected = true;
                    }
                    else if (!IsConnected && _connected)
                    {
                        //if (OnDisconnected != null)
                        //    Invoker(OnDisconnected, this, new ToxEventArgs.ConnectionEventArgs(false));

                        _connected = false;
                    }

                    int delay = DoIterate();
                    await Task.Delay(delay);
                }
            }, _cancelTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// Adds a friend.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="message"></param>
        /// <param name="error"></param>
        /// <returns>friendNumber</returns>
        public int AddFriend(ToxId id, string message, out ToxErrorFriendAdd error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] msg = Encoding.UTF8.GetBytes(message);
            error = ToxErrorFriendAdd.Ok;

            return (int)ToxFunctions.FriendAdd(_tox, id.Bytes, msg, (uint)msg.Length, ref error);
        }

        /// <summary>
        /// Adds a friend.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="message"></param>
        /// <returns>friendNumber</returns>
        public int AddFriend(ToxId id, string message)
        {
            var error = ToxErrorFriendAdd.Ok;
            return AddFriend(id, message, out error);
        }

        /// <summary>
        /// Adds a friend without sending a request.
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="error"></param>
        /// <returns>friendNumber</returns>
        public int AddFriendNoRequest(ToxKey publicKey, out ToxErrorFriendAdd error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorFriendAdd.Ok;
            return (int)ToxFunctions.FriendAddNoRequest(_tox, publicKey.GetBytes(), ref error);
        }

        /// <summary>
        /// Adds a friend without sending a request.
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns>friendNumber</returns>
        public int AddFriendNoRequest(ToxKey publicKey)
        {
            var error = ToxErrorFriendAdd.Ok;
            return AddFriendNoRequest(publicKey, out error);
        }

        public bool AddTcpRelay(ToxNode node, out ToxErrorBootstrap error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorBootstrap.Ok;
            return ToxFunctions.AddTcpRelay(_tox, node.Address, (ushort)node.Port, node.PublicKey.GetBytes(), ref error);
        }

        public bool AddTcpRelay(ToxNode node)
        {
            var error = ToxErrorBootstrap.Ok;
            return AddTcpRelay(node, out error);
        }

        /// <summary>
        /// Bootstraps this Tox instance with a ToxNode.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool Bootstrap(ToxNode node, out ToxErrorBootstrap error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorBootstrap.Ok;
            return ToxFunctions.Bootstrap(_tox, node.Address, (ushort)node.Port, node.PublicKey.GetBytes(), ref error);
        }

        /// <summary>
        /// Bootstraps this Tox instance with a ToxNode.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool Bootstrap(ToxNode node)
        {
            var error = ToxErrorBootstrap.Ok;
            return Bootstrap(node, out error);
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

            return ToxFunctions.FriendExists(_tox, (uint)friendNumber);
        }

        /// <summary>
        /// Retrieves the typing status of a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool GetIsTyping(int friendNumber, out ToxErrorFriendQuery error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorFriendQuery.Ok;
            return ToxFunctions.FriendGetTyping(_tox, (uint)friendNumber, ref error);
        }

        /// <summary>
        /// Retrieves the typing status of a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public bool GetIsTyping(int friendNumber)
        {
            var error = ToxErrorFriendQuery.Ok;
            return GetIsTyping(friendNumber, out error);
        }

        /// <summary>
        /// Retrieves the friendNumber associated to the specified public address/id.
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public int GetFriendByPublicKey(string publicKey, out ToxErrorFriendByPublicKey error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorFriendByPublicKey.Ok;
            return (int)ToxFunctions.FriendByPublicKey(_tox, ToxTools.StringToHexBin(publicKey), ref error);
        }

        /// <summary>
        /// Retrieves the friendNumber associated to the specified public address/id.
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public int GetFriendByPublicKey(string publicKey)
        {
            var error = ToxErrorFriendByPublicKey.Ok;
            return GetFriendByPublicKey(publicKey, out error);
        }

        /// <summary>
        /// Retrieves a friend's connection status.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public ToxConnectionStatus GetFriendConnectionStatus(int friendNumber, out ToxErrorFriendQuery error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorFriendQuery.Ok;
            return ToxFunctions.FriendGetConnectionStatus(_tox, (uint)friendNumber, ref error);
        }

        /// <summary>
        /// Retrieves a friend's connection status.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public ToxConnectionStatus GetFriendConnectionStatus(int friendNumber)
        {
            var error = ToxErrorFriendQuery.Ok;
            return GetFriendConnectionStatus(friendNumber, out error);
        }

        /// <summary>
        /// Retrieves a friend's public id/address.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public ToxKey GetPublicKey(int friendNumber, out ToxErrorFriendGetPublicKey error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] address = new byte[ToxConstants.PublicKeySize];
            error = ToxErrorFriendGetPublicKey.Ok;
            ToxFunctions.FriendGetPublicKey(_tox, (uint)friendNumber, address, ref error);

            return new ToxKey(ToxKeyType.Public, address);
        }

        /// <summary>
        /// Retrieves a friend's public id/address.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public ToxKey GetPublicKey(int friendNumber)
        {
            var error = ToxErrorFriendGetPublicKey.Ok;
            return GetPublicKey(friendNumber, out error);
        }

        /// <summary>
        /// Retrieves a friend's current user status.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public ToxStatus GetStatus(int friendNumber, out ToxErrorFriendQuery error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorFriendQuery.Ok;
            return ToxFunctions.FriendGetStatus(_tox, (uint)friendNumber, ref error);
        }

        /// <summary>
        /// Retrieves a friend's current user status.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public ToxStatus GetStatus(int friendNumber)
        {
            var error = ToxErrorFriendQuery.Ok;
            return GetStatus(friendNumber, out error);
        }

        /// <summary>
        /// Sets the typing status of this Tox instance.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="isTyping"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool SetSelfTyping(int friendNumber, bool isTyping, out ToxErrorSetTyping error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorSetTyping.Ok;
            return ToxFunctions.SelfSetTyping(_tox, (uint)friendNumber, isTyping, ref error);
        }

        /// <summary>
        /// Sets the typing status of this Tox instance.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="isTyping"></param>
        /// <returns></returns>
        public bool SetSelfTyping(int friendNumber, bool isTyping)
        {
            var error = ToxErrorSetTyping.Ok;
            return SetSelfTyping(friendNumber, isTyping, out error);
        }

        /// <summary>
        /// Send a message to a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="message"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public int SendMessage(int friendNumber, string message, out ToxErrorSendMessage error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] bytes = Encoding.UTF8.GetBytes(message);
            error = ToxErrorSendMessage.Ok;

            return (int)ToxFunctions.SendMessage(_tox, (uint)friendNumber, bytes, (uint)bytes.Length, ref error);
        }

        /// <summary>
        /// Send a message to a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public int SendMessage(int friendNumber, string message)
        {
            var error = ToxErrorSendMessage.Ok;
            return SendMessage(friendNumber, message, out error);
        }

        /// <summary>
        /// Sends an action to a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="action"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public int SendAction(int friendNumber, string action, out ToxErrorSendMessage error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] bytes = Encoding.UTF8.GetBytes(action);
            error = ToxErrorSendMessage.Ok;

            return (int)ToxFunctions.SendAction(_tox, (uint)friendNumber, bytes, (uint)bytes.Length, ref error);
        }

        /// <summary>
        /// Sends an action to a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public int SendAction(int friendNumber, string action)
        {
            var error = ToxErrorSendMessage.Ok;
            return SendAction(friendNumber, action, out error);
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
        /// <param name="error"></param>
        /// <returns></returns>
        public bool DeleteFriend(int friendNumber, out ToxErrorFriendDelete error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorFriendDelete.Ok;
            return ToxFunctions.FriendDelete(_tox, (uint)friendNumber, ref error);
        }

        /// <summary>
        /// Deletes a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
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

            byte[] bytes = new byte[ToxFunctions.SaveSize(_tox)];
            ToxFunctions.Save(_tox, bytes);

            return new ToxData(bytes);
        }

        public ToxKey GetPrivateKey()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] key = new byte[ToxConstants.PublicKeySize];
            ToxFunctions.SelfGetPrivateKey(_tox, key);

            return new ToxKey(ToxKeyType.Secret, key);
        }

        /// <summary>
        /// Retrieves the name of a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="error"></param>
        /// <returns></returns>
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

            return Encoding.UTF8.GetString(name);
        }

        /// <summary>
        /// Retrieves the name of a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public string GetFriendName(int friendNumber)
        {
            var error = ToxErrorFriendQuery.Ok;
            return GetFriendName(friendNumber, out error);
        }

        /// <summary>
        /// Retrieves the status message of a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public string GetFriendStatusMessage(int friendNumber, out ToxErrorFriendQuery error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorFriendQuery.Ok;
            uint size = ToxFunctions.FriendGetNameSize(_tox, (uint)friendNumber, ref error);

            if (error != ToxErrorFriendQuery.Ok)
                return string.Empty;

            byte[] message = new byte[size];
            ToxFunctions.FriendGetName(_tox, (uint)friendNumber, message, ref error);

            return Encoding.UTF8.GetString(message);
        }

        /// <summary>
        /// Retrieves the status message of a friend.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <returns></returns>
        public string GetFriendStatusMessage(int friendNumber)
        {
            var error = ToxErrorFriendQuery.Ok;
            return GetFriendStatusMessage(friendNumber, out error);
        }

        /// <summary>
        /// Retrieves the UDP port this instance of Tox is bound to.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public int GetUdpPort(out ToxErrorGetPort error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorGetPort.Ok;
            return ToxFunctions.GetUdpPort(_tox, ref error);
        }

        /// <summary>
        /// Retrieves the UDP port this instance of Tox is bound to.
        /// </summary>
        /// <returns></returns>
        public int GetUdpPort()
        {
            var error = ToxErrorGetPort.Ok;
            return GetUdpPort(out error);
        }

        /// <summary>
        /// Retrieves the TCP port this instance of Tox is bound to.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public int GetTcpPort(out ToxErrorGetPort error)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            error = ToxErrorGetPort.Ok;
            return ToxFunctions.GetTcpPort(_tox, ref error);
        }

        /// <summary>
        /// Retrieves the TCP port this instance of Tox is bound to.
        /// </summary>
        /// <returns></returns>
        public int GetTcpPort()
        {
            var error = ToxErrorGetPort.Ok;
            return GetTcpPort(out error);
        }

        public byte[] Hash(byte[] data)
        {
            byte[] hash = new byte[ToxConstants.ToxHashLength];
            ToxFunctions.Hash(hash, data, (uint)data.Length);
            return hash;
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
                    _onFriendMessageCallback = (IntPtr tox, uint friendNumber, byte[] message, uint length, IntPtr userData) =>
                    {
                        if (_onFriendMessage != null)
                            _onFriendMessage(this, new ToxEventArgs.FriendMessageEventArgs((int)friendNumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(message, 0, (int)length))));
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
        /// Occurs when a message is received from a friend.
        /// </summary>
        public event EventHandler<ToxEventArgs.FriendActionEventArgs> OnFriendActionReceived
        {
            add
            {
                if (_onFriendActionCallback == null)
                {
                    _onFriendActionCallback = (IntPtr tox, uint friendNumber, byte[] action, uint length, IntPtr userData) =>
                    {
                        if (_onFriendAction != null)
                            _onFriendAction(this, new ToxEventArgs.FriendActionEventArgs((int)friendNumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(action, 0, (int)length))));
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
        public event EventHandler<ToxEventArgs.NameChangeEventArgs> OnFriendNameChanged
        {
            add
            {
                if (_onNameChangeCallback == null)
                {
                    _onNameChangeCallback = (IntPtr tox, uint friendNumber, byte[] newName, uint length, IntPtr userData) =>
                    {
                        if (_onNameChange != null)
                            _onNameChange(this, new ToxEventArgs.NameChangeEventArgs((int)friendNumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(newName, 0, (int)length))));
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
                            _onStatusMessage(this, new ToxEventArgs.StatusMessageEventArgs((int)friendNumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(newStatus, 0, (int)length))));
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
                    _onUserStatusCallback = (IntPtr tox, uint friendNumber, ToxStatus status, IntPtr userData) =>
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
                            _onFileChunk(this, new ToxEventArgs.FileChunkEventArgs((int)friendNumber, (int)fileNumber, data, position));
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
                            _onFileReceive(this, new ToxEventArgs.FileSendRequestEventArgs((int)friendNumber, (int)fileNumber, fileSize, Encoding.UTF8.GetString(filename)));
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
                            _onFileChunkRequested(this, new ToxEventArgs.FileRequestChunkEventArgs((int)friendNumber, (int)fileNumber, position, (int)length));
                    };

                    ToxFunctions.RegisterFileRequestChunkCallback(_tox, _onFileRequestChunkCallback, IntPtr.Zero);
                }

                _onFileChunkRequested += value;
            }
            remove
            {
                if (_onFileChunkRequested.GetInvocationList().Length == 1)
                {
                    ToxFunctions.RegisterFileRequestChunkCallback(_tox, null, IntPtr.Zero);
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

        #endregion
    }
}

#pragma warning restore 1591