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
    }
}

