using System;
using SharpTox.Core;

namespace SharpTox.Examples
{
    /// <summary>
    /// A very basic example on how to use SharpTox.
    /// </summary>
    class Basic
    {
        private Tox tox;

        public Basic()
        {
            ToxOptions options = new ToxOptions(true, false);

            tox = new Tox(options);
            tox.OnFriendRequest += tox_OnFriendRequest;
            tox.OnFriendMessage += tox_OnFriendMessage;

            foreach (ToxNode node in Nodes)
                tox.BootstrapFromNode(node);

            tox.Name = "SharpTox";
            tox.StatusMessage = "Testing SharpTox";

            tox.Start();

            string id = tox.Id.ToString();
            Console.WriteLine("ID: {0}", id);

            Console.ReadKey();
            tox.Dispose();
        }

        //check https://wiki.tox.im/Nodes for an up-to-date list of nodes
        private ToxNode[] Nodes = new ToxNode[]
        {
            new ToxNode("192.254.75.98", 33445, new ToxKey(ToxKeyType.Public, "951C88B7E75C867418ACDB5D273821372BB5BD652740BCDF623A4FA293E75D2F")),
            new ToxNode("144.76.60.215", 33445, new ToxKey(ToxKeyType.Public, "04119E835DF3E78BACF0F84235B300546AF8B936F035185E2A8E9E0A67C8924F"))
        };

        private void tox_OnFriendMessage(object sender, ToxEventArgs.FriendMessageEventArgs e)
        {
            //get the name associated with the friendnumber
            string name = tox.GetName(e.FriendNumber);

            //print the message to the console
            Console.WriteLine("<{0}> {1}", name, e.Message);
        }

        private void tox_OnFriendRequest(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            //automatically accept every friend request we receive
            tox.AddFriendNoRequest(new ToxKey(ToxKeyType.Public, e.Id));
        }
    }
}
