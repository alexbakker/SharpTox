using System;
using System.Linq;

namespace SharpTox.Core
{
    public class ToxGroupPeer
    {
        public ToxGroup Group { get; private set; }

        public string Name { get; internal set; }

        public int Number { get; private set; }

        internal ToxGroupPeer(ToxGroup group, int peerNumber)
        {
            Group = group;
            Number = peerNumber;
        }
        /// <summary>
        /// Check if the given peernumber corresponds to ours.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="peerNumber"></param>
        /// <returns></returns>
        public bool IsMe
        {
            get
            {
                Group.Tox.CheckDisposed();

                return ToxFunctions.GroupPeerNumberIsOurs(Group.Tox.Handle, Group.Number, Number) == 1;
            }
        }

        /// <summary>
        /// Retrieves the public key of a peer.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="peerNumber"></param>
        /// <returns></returns>
        public ToxKey PublicKey
        {
            get {
                Group.Tox.CheckDisposed();

                byte[] key = new byte[ToxConstants.ClientIdSize];
                int result = ToxFunctions.GroupPeerPubkey(Group.Tox.Handle, Group.Number, Number, key);

                if (result != 0)
                    return null;

                return new ToxKey(ToxKeyType.Public, key);
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

