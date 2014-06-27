#pragma warning disable 1591

namespace SharpTox.Core
{
    public class ToxNode
    {
        public string Address { get; private set; }
        public int Port { get; private set; }
        public bool Ipv6Enabled { get; private set; }
        public string PublicKey { get; private set; }

        public ToxNode(string address, int port, string public_key, bool ipv6enabled)
        {
            Address = address;
            Port = port;
            PublicKey = public_key;
            Ipv6Enabled = ipv6enabled;
        }
    }
}

#pragma warning restore 1591