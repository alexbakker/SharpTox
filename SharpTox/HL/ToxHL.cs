using System;
using SharpTox.Core;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using SharpTox.Encryption;

namespace SharpTox.HL
{
    public class ToxHL : IDisposable
    {
        public ToxOptions Options { get; private set; }
        internal Tox Core { get; private set; }

        public event EventHandler<ToxEventArgs.FriendRequestEventArgs> FriendRequestReceived;
        public event EventHandler<ToxEventArgs.ConnectionStatusEventArgs> ConnectionStatusChanged;

        public ToxId Id
        {
            get { return Core.Id;}
        }

        public ToxKey DhtId
        {
            get { return Core.DhtId; }
        }

        public int TcpPort
        {
            get { return GetPort(Core.GetTcpPort); }
        }

        public int UdpPort
        {
            get { return GetPort(Core.GetUdpPort); } 
        }

        public bool IsConnected
        {
            get { return Core.IsConnected; }
        }

        public ToxConnectionStatus ConnectionStatus
        {
            get { return Core.ConnectionStatus; }
        }

        public string Name
        {
            get { return Core.Name; }
            set { Core.Name = value; }
        }

        public string StatusMessage
        {
            get { return Core.StatusMessage; }
            set { Core.StatusMessage = value; }
        }

        public ToxUserStatus Status
        {
            get { return Core.Status; }
            set { Core.Status = value; }
        }

        private readonly List<ToxFriend> _friends = new List<ToxFriend>();
        private readonly object _friendsLock = new object();

        public ReadOnlyCollection<ToxFriend> Friends
        {
            get 
            {
                lock (_friendsLock)
                    return _friends.AsReadOnly(); 
            }
        }

        public ToxHL(ToxOptions options)
        {
            Core = new Tox(options);
            Options = options;

            HookEvents();
        }

        public ToxHL(ToxOptions options, ToxData data)
        {
            Core = new Tox(options, data);
            Options = options;

            //initial population of the friend list (no need to lock here)
            foreach (int friendNumber in Core.Friends)
                _friends.Add(new ToxFriend(this, friendNumber));
        }
            
        public void Dispose()
        {
            Core.Dispose();
        }

        public void Start()
        {
            Core.Start();
        }

        public void Stop()
        {
            Core.Stop();
        }

        public void Bootstrap(ToxNode node)
        {
            AddNode(Core.Bootstrap, node);
        }

        public void AddTcpRelay(ToxNode node)
        {
            AddNode(Core.AddTcpRelay, node);
        }

        public ToxFriend AddFriend(ToxId id, string message)
        {
            var error = ToxErrorFriendAdd.Ok;
            int friendNumber = Core.AddFriend(id, message, out error);

            if (error != ToxErrorFriendAdd.Ok)
                throw new ToxException<ToxErrorFriendAdd>(error);

            var friend = new ToxFriend(this, friendNumber);
            AddFriendToList(friend);
            return friend;
        }

        public ToxFriend AddFriendNoRequest(ToxKey publicKey)
        {
            var error = ToxErrorFriendAdd.Ok;
            int friendNumber = Core.AddFriendNoRequest(publicKey, out error);

            if (error != ToxErrorFriendAdd.Ok)
                throw new ToxException<ToxErrorFriendAdd>(error);

            var friend = new ToxFriend(this, friendNumber);
            AddFriendToList(friend);
            return friend;
        }

        public void RemoveFriend(ToxFriend friend)
        {
            var error = ToxErrorFriendDelete.Ok;
            Core.DeleteFriend(friend.Number, out error);

            if (error != ToxErrorFriendDelete.Ok)
                throw new ToxException<ToxErrorFriendDelete>(error);

            lock (_friendsLock)
                _friends.Remove(friend);
        }

        public ToxData GetData()
        {
            return Core.GetData();
        }

        public ToxData GetData(ToxEncryptionKey key)
        {
            return Core.GetData(key);
        }

        public ToxData GetData(string password)
        {
            return Core.GetData(password);
        }

        private void AddFriendToList(ToxFriend friend)
        {
            lock (_friendsLock)
                _friends.Add(friend);
        }

        private void HookEvents()
        {
            Core.OnFriendRequestReceived += (sender, e) =>
            {
                if (FriendRequestReceived != null)
                    FriendRequestReceived(this, e);
            };

            Core.OnConnectionStatusChanged += (sender, e) =>
            {
                if (ConnectionStatusChanged != null)
                    ConnectionStatusChanged(this, e);
            };
        }

        private delegate T GetPortDelegate<T>(out ToxErrorGetPort error);
        private delegate T AddNodeDelegate<T>(ToxNode node, out ToxErrorBootstrap error);

        private T GetPort<T>(GetPortDelegate<T> func)
        {
            var error = ToxErrorGetPort.Ok;
            var result = func(out error);

            if (error != ToxErrorGetPort.Ok)
                throw new ToxException<ToxErrorGetPort>(error);

            return result;
        }

        private T AddNode<T>(AddNodeDelegate<T> func, ToxNode node)
        {
            var error = ToxErrorBootstrap.Ok;
            var result = func(node, out error);

            if (error != ToxErrorBootstrap.Ok)
                throw new ToxException<ToxErrorBootstrap>(error);

            return result;
        }
    }
}
