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
        /// The number of friends in this Tox instance.
        /// </summary>
        public int FriendCount
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return (int)ToxFunctions.FriendListSize(_tox);
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

                byte[] bytes = new byte[ToxConstants.MaxNameLength];
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

            byte[] address = new byte[ToxConstants.ClientIdSize];
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
        public event EventHandler<ToxEventArgs.FriendMessageEventArgs> OnFriendMessage
        {
            add
            {
                if (_onFriendMessageCallback == null)
                {
                    _onFriendMessageCallback = (IntPtr tox, int friendNumber, byte[] message, uint length, IntPtr userData) =>
                    {
                        if (_onFriendMessage != null)
                            _onFriendMessage(this, new ToxEventArgs.FriendMessageEventArgs(friendNumber, ToxTools.RemoveNull(Encoding.UTF8.GetString(message, 0, (int)length))));
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
        #endregion
    }
}

#pragma warning restore 1591