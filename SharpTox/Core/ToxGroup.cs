using System;
using System.Text;

namespace SharpTox.Core
{
    public class ToxGroup
    {
        public Tox Tox { get; private set; }
        public int Number { get; private set; }

        public ToxGroup(Tox tox)
        {
            if (tox == null)
                throw new ArgumentNullException("tox");

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
    }
}

