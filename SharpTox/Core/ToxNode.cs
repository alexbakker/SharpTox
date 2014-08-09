namespace SharpTox.Core
{
    /// <summary>
    /// Represents a tox node.
    /// </summary>
    public class ToxNode
    {
        /// <summary>
        /// The address of this node.
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// The port on which this node listens.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Whether IPv6 should be enabled or not.
        /// </summary>
        public bool Ipv6Enabled { get; private set; }

        /// <summary>
        /// The public key of this node.
        /// </summary>
        public ToxKey PublicKey { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToxNode"/> class.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="public_key"></param>
        /// <param name="ipv6enabled"></param>
        public ToxNode(string address, int port, ToxKey public_key, bool ipv6enabled)
        {
            Address = address;
            Port = port;
            PublicKey = public_key;
            Ipv6Enabled = ipv6enabled;
        }
    }
}
