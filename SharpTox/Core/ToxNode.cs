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

#if !IS_PORTABLE
        /// <summary>
        /// Parses and returns an array of nodes grabbed from https://wiki.tox.im/Nodes. Only use this if you're desperate.
        /// </summary>
        /// <returns></returns>
        public static ToxNode[] GetNodes()
        {
            WebClient client = new WebClient();
            string content = string.Empty;

            try
            {
                content = client.DownloadString("https://wiki.tox.im/Nodes");
            }
            catch
            {
                return new ToxNode[0];
            }
            finally
            {
                client.Dispose();
            }

            var list = new List<ToxNode>();
            int index = content.IndexOf("<table");
            string table = content.Substring(index, content.IndexOf("</table") - index + "</table".Length);

            for (int i = 0; i < table.Length; i++)
            {
                if (table[i] != '<')
                    continue;

                if (string.Concat(table[i], table[i + 1], table[i + 2]) == "<tr")
                {
                    index = table.IndexOf(">", i + 3) + 1;
                    string row = table.Substring(index, table.IndexOf("</tr>", i) - index).Replace("\n", "");

                    if (row.StartsWith("<th"))
                        continue;

                    string[] cells = new string[7];
                    int count = 0;
                    for (int j = 0; j < row.Length; j++)
                    {
                        if (row[j] != '<')
                            continue;

                        if (string.Concat(row[j], row[j + 1], row[j + 2], row[j + 3]) == "<td>")
                        {
                            index = row.IndexOf("<td>", j) + 4;
                            string cell = row.Substring(index, row.IndexOf("</td>", j) - index);
                            cells[count] = cell;

                            count++;
                        }
                    }

                    list.Add(new ToxNode(cells[0], int.Parse(cells[2]), new ToxKey(ToxKeyType.Public, cells[3])));
                }
            }

            return list.ToArray();
        }
#endif
    }
}
