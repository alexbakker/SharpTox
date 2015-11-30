using System;
using SharpTox.Core;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace SharpTox.HL
{
    public class ToxHL
    {
        public ToxOptions Options { get; private set; }
        internal Tox Core { get; private set; }

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
        }

        public ToxHL(ToxOptions options, ToxData data)
        {
            Core = new Tox(options, data);
            Options = options;

            //initial population of the friend list (no need to lock here)
            foreach (int friendNumber in Core.Friends)
                _friends.Add(new ToxFriend(this, friendNumber));
        }

        public ToxFriend AddFriend(ToxId id, string message)
        {
            var error = ToxErrorFriendAdd.Ok;
            int friendNumber = Core.AddFriend(id, message, out error);

            if (error != ToxErrorFriendAdd.Ok)
                throw new Exception("Could not add friend");

            var friend = new ToxFriend(this, friendNumber);
            AddFriendToList(friend);
            return friend;
        }

        public ToxFriend AddFriendNoRequest(ToxKey publicKey)
        {
            var error = ToxErrorFriendAdd.Ok;
            int friendNumber = Core.AddFriendNoRequest(publicKey, out error);

            if (error != ToxErrorFriendAdd.Ok)
                throw new Exception("Could not add friend");

            var friend = new ToxFriend(this, friendNumber);
            AddFriendToList(friend);
            return friend;
        }

        private void AddFriendToList(ToxFriend friend)
        {
            lock (_friendsLock)
                _friends.Add(friend);
        }
    }
}
