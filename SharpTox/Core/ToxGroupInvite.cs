using System;

namespace SharpTox.Core
{
    public class ToxGroupInvite
    {
        public ToxFriend Friend { get; private set; }

        public ToxGroupType GroupType { get; private set; }

        public byte[] Data { get; private set; }

        private ToxGroup _group;

        public ToxGroupInvite(ToxFriend friend, ToxGroupType type, byte[] data)
        {
            Friend = friend;
            GroupType = type;
            Data = data;
        }

        /// <summary>
        /// Accept the group invitation.
        /// </summary>
        public ToxGroup Accept()
        {
            Friend.Tox.CheckDisposed();

            if (_group == null)
            {
                if (GroupType == ToxGroupType.Text)
                    _group = Friend.Tox.GetGroup(ToxFunctions.JoinGroupchat(Friend.Tox.Handle, Friend.Number, Data, (ushort)Data.Length));
                else
                    _group = Friend.Tox.GetGroup(Friend.Tox.ToxAv.JoinAvGroupchat(Friend.Number, Data));
            }

            return _group;
        }
    }
}
