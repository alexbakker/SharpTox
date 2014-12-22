using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace SharpTox.Core
{
    public class ToxGroup
    {
        public Tox Tox { get; private set; }
        public int Number { get; private set; }

        internal ToxGroup(Tox tox)
        {
            if (tox == null)
                throw new ArgumentNullException("tox");

            Tox = tox;

            Tox.CheckDisposed();
            Number = ToxFunctions.AddGroupchat(Tox.Handle);
        }

        internal ToxGroup(Tox tox, int groupNumber)
        {
            Tox = tox;
            Number = groupNumber;
        }

        /// <summary>
        /// Retrieves the number of group members in a group chat.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <returns></returns>
        public int GetGroupMemberCount
        {
            get
            {
                Tox.CheckDisposed();
                return ToxFunctions.GroupNumberPeers(Tox.Handle, Number);
            }
        }

        /// <summary>
        /// Deletes a group chat.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <returns></returns>
        public bool DeleteGroupChat(int groupNumber)
        {
            Tox.CheckDisposed();
            Tox.DeleteGroup(this);
            return ToxFunctions.DelGroupchat(Tox.Handle, groupNumber) == 0;
        }

        /// <summary>
        /// Invites a friend to a group chat.
        /// </summary>
        /// <param name="friendNumber"></param>
        /// <param name="groupNumber"></param>
        /// <returns></returns>
        public bool InviteFriend(ToxFriend friend)
        {
            Tox.CheckDisposed();
            return ToxFunctions.InviteFriend(Tox.Handle, friend.Number, Number) == 0;
        }

        /// <summary>
        /// Sends a message to a group.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendGroupMessage(int groupNumber, string message)
        {
            Tox.CheckDisposed();
            byte[] msg = Encoding.UTF8.GetBytes(message);
            return ToxFunctions.GroupMessageSend(Tox.Handle, groupNumber, msg, (ushort)msg.Length) == 0;
        }

        /// <summary>
        /// Sends an action to a group.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool SendGroupAction(string action)
        {
            Tox.CheckDisposed();
            byte[] act = Encoding.UTF8.GetBytes(action);
            return ToxFunctions.GroupActionSend(Tox.Handle, Number, act, (ushort)act.Length) == 0;
        }

        /// <summary>
        /// Gets or sets the title of the group chat.
        /// </summary>
        /// <value>The title of the group chat.</value>
        public string Title
        {
            set
            {
                Tox.CheckDisposed();

                if (Encoding.UTF8.GetByteCount(value) > ToxConstants.MaxNameLength)
                    throw new ArgumentException("The specified group title is longer than 256 bytes");

                byte[] bytes = Encoding.UTF8.GetBytes(value);

                if (ToxFunctions.GroupSetTitle(Tox.Handle, Number, bytes, (byte)bytes.Length) != 0)
                    throw new Exception("Couldn't change the group title");
            }
            get
            {
                Tox.CheckDisposed();

                byte[] title = new byte[ToxConstants.MaxNameLength];
                int length = ToxFunctions.GroupGetTitle(Tox.Handle, Number, title, (uint)title.Length);

                if (length == -1)
                    return string.Empty;

                return ToxTools.GetString(title);
            }
        }


        /// <summary>
        /// Gets the type of the group.
        /// </summary>
        /// <value>The type of the group.</value>
        public ToxGroupType GroupType
        {
            get
            {
                Tox.CheckDisposed();
                return (ToxGroupType)ToxFunctions.GroupGetType(Tox.Handle, Number);
            }
        }

        /// <summary>
        /// Retrieves an array of group member names.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <returns></returns>
        public string[] PeerNames
        {
            get
            {
                Tox.CheckDisposed();

                int count = ToxFunctions.GroupNumberPeers(Tox.Handle, Number);

                //just return an empty string array before we get an overflow exception
                if (count <= 0)
                    return new string[0];

                ushort[] lengths = new ushort[count];
                byte[,] matrix = new byte[count, ToxConstants.MaxNameLength];

                int result = ToxFunctions.GroupGetNames(Tox.Handle, Number, matrix, lengths, (ushort)count);
                if (result != 0)
                    throw new Exception("Error while trying to retrieve PeerNames");

                string[] names = new string[count];
                for (int i = 0; i < count; i++)
                {
                    byte[] name = new byte[lengths[i]];

                    for (int j = 0; j < name.Length; j++)
                        name[j] = matrix[i, j];

                    names[i] = ToxTools.GetString(name);
                }

                return names;
            }
        }

        Dictionary<int, ToxGroupPeer> peers = new Dictionary<int, ToxGroupPeer>();
        internal ToxGroupPeer PeerFromPeerNumber(int peerNumber)
        {
            ToxGroupPeer peer;
            if (!peers.TryGetValue(peerNumber, out peer))
                peer = new ToxGroupPeer(this, peerNumber);

            return peer;
        }

        internal void Change(ToxGroupPeer peer, ToxChatChange change)
        {
            if (change == ToxChatChange.PeerAdd)
                peers.Add(peer.Number, peer);
            else if (change == ToxChatChange.PeerDel)
                peers.Remove(peer.Number);
            else if (change == ToxChatChange.PeerName)
                peer.Name = GetGroupMemberName(peer.Number);
        }

        /// <summary>
        /// Retrieves the name of a group member.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="peerNumber"></param>
        /// <returns></returns>
        private string GetGroupMemberName(int peerNumber)
        {
            Tox.CheckDisposed();

            byte[] name = new byte[ToxConstants.MaxNameLength];
            if (ToxFunctions.GroupPeername(Tox.Handle, Number, peerNumber, name) == -1)
                throw new Exception("Could not get peer name");

            return ToxTools.GetString(name);
        }

        /// <summary>
        /// Returns peers in the group chat.
        /// </summary>
        /// <value>Peers of this group chat.</value>
        public ToxGroupPeer[] Peers
        {
            get
            {
                return peers.Select((kvp) => kvp.Value).ToArray();
            }
        }
    }
}

