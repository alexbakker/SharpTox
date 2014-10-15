using System.Net;
using System.Linq;
using System.Collections.Generic;

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
        
        /// <summary>
        /// Parses and returns an array of nodes grabbed from https://wiki.tox.im/Nodes. Only use this if you're desperate.
        /// </summary>
        /// <returns></returns>
        public static ToxNode[] GetNodes()
        {
            WebClient client = new WebClient();
            string content = "";

            try
            {
                content = client.DownloadString("https://wiki.tox.im/Nodes");
                client.Dispose();
            }
            catch
            {
                client.Dispose();
                return new ToxNode[0];
            }

            var list = new List<ToxNode>();
            int index = content.IndexOf("<table");
            string table = content.Substring(index, content.IndexOf("</table") - index + "</table".Length);

            for (int i = 0; i < table.Length; i++)
            {
                if (table[i] != '<')
                    continue;

                if (string.Concat(table[i], table[i+1], table[i+2]) == "<tr")
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
    }
}
