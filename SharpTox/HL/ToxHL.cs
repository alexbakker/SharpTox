using System;
using SharpTox.Core;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace SharpTox.HL
{
    public class ToxHL : IDisposable
    {
        public ToxOptions Options { get; private set; }
        internal Tox Core { get; private set; }

        public event EventHandler<ToxEventArgs.FriendRequestEventArgs> OnFriendRequestReceived;

        public ToxId Id
        {
            get { return Core.Id; }
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

            Core.OnFriendRequestReceived += (sender, e) => OnFriendRequestReceived(this, e);
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

        private void AddFriendToList(ToxFriend friend)
        {
            lock (_friendsLock)
                _friends.Add(friend);
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
    }
}
