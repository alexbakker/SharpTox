using System;

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
    }
}

