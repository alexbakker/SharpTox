using System.Collections.Generic;
using System.Net;

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
        /// The public key of this node.
        /// </summary>
        public ToxKey PublicKey { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToxNode"/> class.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="publicKey"></param>
        public ToxNode(string address, int port, ToxKey publicKey)
        {
            Address = address;
            Port = port;
            PublicKey = publicKey;
        }

        public static bool operator ==(ToxNode node1, ToxNode node2)
        {
            if (object.ReferenceEquals(node1, node2))
                return true;

            if ((object)node1 == null ^ (object)node2 == null)
                return false;

            return (node1.PublicKey == node2.PublicKey && node1.Port == node2.Port && node1.Address == node2.Address);
        }

        public static bool operator !=(ToxNode node1, ToxNode node2)
        {
            return !(node1 == node2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            ToxNode node = obj as ToxNode;
            if ((object)node == null)
                return false;

            return this == node;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
