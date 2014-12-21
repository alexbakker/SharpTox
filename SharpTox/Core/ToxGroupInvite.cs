using System;

namespace SharpTox.Core
{
    public class ToxGroupInvite
    {
        public ToxFriend Friend { get; private set; }
        public ToxGroupType GroupType { get; private set; }
        public byte[] Data { get; private set; }

        public ToxGroupInvite(ToxFriend friend, ToxGroupType type, byte[] data)
        {
            Friend = friend;
            GroupType = type;
            Data = data;
        }

        ToxGroup _group;
        /// <summary>
        /// Accept the group invitation.
        /// </summary>
        public ToxGroup Accept()
        {
            Friend.Tox.CheckDisposed();

            if (_group == null)
                _group = Friend.Tox.GroupFromGroupNumber(ToxFunctions.JoinGroupchat(Friend.Tox.Handle, Friend.Number, Data, (ushort)Data.Length));

            return _group;
        }
    }
}

