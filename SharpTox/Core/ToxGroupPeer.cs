using System;
using System.Linq;

namespace SharpTox.Core
{
    public class ToxGroupPeer
    {
        public ToxGroup Group { get; private set; }

        public string Name { get; internal set; }

        public ToxKey PublicKey { get; private set; }

        internal ToxGroupPeer(ToxGroup group, ToxKey publicKey)
        {
            Group = group;
            PublicKey = publicKey;
        }

        /// <summary>
        /// Check if the given peernumber corresponds to ours.
        /// </summary>
        /// <returns></returns>
        public bool IsSelf
        {
            get
            {
                Group.Tox.CheckDisposed();
                return ToxFunctions.GroupPeerNumberIsOurs(Group.Tox.Handle, Group.Number, Number) == 1;
            }
        }

        internal static ToxKey GetPublicKey(ToxGroup group, int peerNumber)
        {
            group.Tox.CheckDisposed();

            byte[] key = new byte[ToxConstants.ClientIdSize];
            int result = ToxFunctions.GroupPeerPubkey(group.Tox.Handle, group.Number, peerNumber, key);

            if (result != 0)
                return null;

            return new ToxKey(ToxKeyType.Public, key);
        }

        public int Number
        {
            get
            {
                Group.Tox.CheckDisposed();

                for (int i = 0; i < Group.MemberCount; i++)
                {
                    if (GetPublicKey(Group, i) == PublicKey)
                        return i;
                }

                return -1;
            }
        }

        public ToxFriend Friend
        {
            get
            {
                return Group.Tox.Friends.Where((friend) => friend.PublicKey == PublicKey).FirstOrDefault();
            }
        }
    }
}
