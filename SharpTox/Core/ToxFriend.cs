using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace SharpTox.Core
{
    public class ToxFriend
    {
        /// <summary>
        /// The Tox instance that belongs to this friend.
        /// </summary>
        public Tox Tox { get; private set; }

        /// <summary>
        /// The number that is associated with this friend.
        /// </summary>
        public int Number { get; private set; }

        private Dictionary<int, ToxFileSender> _fileSenders = new Dictionary<int, ToxFileSender>();
        private bool _userIsTyping;

        internal ToxFriend(Tox tox, int friendNumber)
        {
            Tox = tox;
            Number = friendNumber;
        }

        /// <summary>
        /// Returns the name of a ToxFriend.
        /// </summary>
        /// <value>The name of a ToxFriend.</value>
        public string Name
        {
            get
            {
                Tox.CheckDisposed();

                int size = ToxFunctions.GetNameSize(Tox.Handle, Number);
                byte[] name = new byte[size];

                ToxFunctions.GetName(Tox.Handle, Number, name);

                return ToxTools.GetString(name);
            }
        }

        /// <summary>
        /// Retrieves a friend's current user status.
        /// </summary>
        /// <value>The user status of a friend.</value>
        public ToxUserStatus UserStatus
        {
            get
            {
                Tox.CheckDisposed();
                return (ToxUserStatus)ToxFunctions.GetUserStatus(Tox.Handle, Number);
            }
        }

        /// <summary>
        /// Retrieves a DateTime object of the last time friend was seen online.
        /// </summary>
        /// <value>Last DateTime the friend has been seen online.</value>
        public DateTime LastOnline
        {
            get
            {
                Tox.CheckDisposed();
                return ToxTools.EpochToDateTime((long)ToxFunctions.GetLastOnline(Tox.Handle, Number));
            }
        }

        /// <summary>
        /// Whether or not this friend exists.
        /// </summary>
        public bool Exists
        {
            get
            {
                Tox.CheckDisposed();
                return ToxFunctions.FriendExists(Tox.Handle, Number) != 0;
            }
        }

        /// <summary>
        /// Retrieves a friend's connection status.
        /// </summary>
        /// <value>Friend's connection status.</value>
        public ToxFriendConnectionStatus ConnectionStatus
        {
            get
            {
                Tox.CheckDisposed();
                return (ToxFriendConnectionStatus)ToxFunctions.GetFriendConnectionStatus(Tox.Handle, Number);
            }
        }

        /// <summary>
        /// Checks if a friend is online..
        /// </summary>
        /// <value>Friend online status.</value>
        public bool IsOnline
        {
            get
            {
                return ConnectionStatus == ToxFriendConnectionStatus.Online;
            }
        }

        /// <summary>
        /// Send a message to a friend.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <returns>Number of the message.</returns>
        public int SendMessage(string message)
        {
            Tox.CheckDisposed();
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            return (int)ToxFunctions.SendMessage(Tox.Handle, Number, bytes, bytes.Length);
        }

        /// <summary>
        /// Sends an action to a friend.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <returns>The action.</returns>
        public int SendAction(string action)
        {
            Tox.CheckDisposed();
            byte[] bytes = Encoding.UTF8.GetBytes(action);
            return (int)ToxFunctions.SendAction(Tox.Handle, Number, bytes, bytes.Length);
        }

        /// <summary>
        /// Retrieves a friend's public key.
        /// </summary>
        /// <value>Friend's pubic id/address.</value>
        public ToxKey PublicKey
        {
            get
            {
                Tox.CheckDisposed();

                byte[] address = new byte[ToxConstants.ClientIdSize];
                ToxFunctions.GetClientID(Tox.Handle, Number, address);

                return new ToxKey(ToxKeyType.Public, address);
            }
        }

        /// <summary>
        /// Retrieves the typing status of a friend.
        /// </summary>
        /// <value><c>true</c> if this friend is typing; otherwise, <c>false</c>.</value>
        public bool IsTyping
        {
            get
            {
                Tox.CheckDisposed();
                return ToxFunctions.GetIsTyping(Tox.Handle, Number) == 1;
            }
        }

        /// <summary>
        /// Informs your user friend whether you are typing or not.
        /// </summary>
        /// <value><c>true</c> if user has been informed that you are typing; otherwise <c>false</c>.</value>
        public bool UserIsTyping
        {
            get
            {
                return _userIsTyping;
            }
            set
            {
                Tox.CheckDisposed();
                byte typing = value ? (byte)1 : (byte)0;
                if (ToxFunctions.SetUserIsTyping(Tox.Handle, Number, typing) != 0)
                    throw new Exception("Couldn't set isTyping for " + Number);
                else
                    _userIsTyping = value;
            }
        }

        /// <summary>
        /// Deletes the friend.
        /// </summary>
        public bool Delete()
        {
            Tox.CheckDisposed();
            Tox.DeleteFriend(this);
            return ToxFunctions.DelFriend(Tox.Handle, Number) == 0;
        }

        /// <summary>
        /// Sends the avatar info to a friend.
        /// This is not done automatically, you have to send specifically
        /// for every friend.
        /// </summary>
        /// <returns><c>true</c>, if avatar info was sent, <c>false</c> otherwise.</returns>
        public bool SendAvatarInfo()
        {
            Tox.CheckDisposed();
            return ToxFunctions.SendAvatarInfo(Tox.Handle, Number) == 0;
        }

        /// <summary>
        /// Requests avatar data from a friend.
        /// </summary>
        /// <returns></returns>
        public bool RequestAvatarData()
        {
            Tox.CheckDisposed();
            return ToxFunctions.RequestAvatarData(Tox.Handle, Number) == 0;
        }

        /// <summary>
        /// Requests avatar info from a friend.
        /// </summary>
        /// <returns></returns>
        public bool RequestAvatarInfo()
        {
            Tox.CheckDisposed();
            return ToxFunctions.RequestAvatarInfo(Tox.Handle, Number) == 0;
        }

        /// <summary>
        /// Retrieves the recommended/maximum size of the filedata to send with FileSendData
        /// </summary>
        /// <value>Recommended/maximum size of the filedata.</value>
        public int FileDataSize
        {
            get
            {
                Tox.CheckDisposed();
                return ToxFunctions.FileDataSize(Tox.Handle, Number);
            }
        }

        /// <summary>
        /// Retrieves the status message of a friend.
        /// </summary>
        /// <value>The status message.</value>
        public string StatusMessage
        {
            get
            {
                Tox.CheckDisposed();

                int size = ToxFunctions.GetStatusMessageSize(Tox.Handle, Number);
                byte[] status = new byte[size];

                ToxFunctions.GetStatusMessage(Tox.Handle, Number, status, status.Length);

                return ToxTools.GetString(status);
            }
        }

        /// <summary>
        /// Sends a lossy packet to the friend..
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SendLossyPacket(byte[] data)
        {
            Tox.CheckDisposed();

            if (data.Length > ToxConstants.MaxCustomPacketSize)
                throw new ArgumentException("Packet size is bigger than ToxConstants.MaxCustomPacketSize");

            if (data[0] < 200 || data[0] > 254)
                throw new ArgumentException("First byte of data is not in the 200-254 range.");

            return ToxFunctions.SendLossyPacket(Tox.Handle, Number, data, (uint)data.Length) == 0;
        }

        /// <summary>
        /// Sends a lossless packet to the friend..
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SendLosslessPacket(byte[] data)
        {
            Tox.CheckDisposed();

            if (data.Length > ToxConstants.MaxCustomPacketSize)
                throw new ArgumentException("Packet size is bigger than ToxConstants.MaxCustomPacketSize");

            if (data[0] < 160 || data[0] > 191)
                throw new ArgumentException("First byte of data is not in the 160-191 range.");

            return ToxFunctions.SendLosslessPacket(Tox.Handle, Number, data, (uint)data.Length) == 0;
        }

        internal ToxFileSender FileSenderFromFileNumber(int fileNumber, Func<ToxFileSender> creator)
        {
            ToxFileSender fileSender;
            if (_fileSenders.TryGetValue(fileNumber, out fileSender))
                return fileSender;

            fileSender = creator();
            _fileSenders.Add(fileSender.Number, fileSender);

            return fileSender;
        }

        /// <summary>
        /// Returns all file senders currently active for this user.
        /// </summary>
        /// <value>Array of file senders.</value>
        public ToxFileSender[] FileSenders
        {
            get
            {
                return _fileSenders.Values.ToArray();
            }
        }

        /// <summary>
        /// Send a file to a friend.
        /// </summary>
        /// <returns>The file sender instance.</returns>
        /// <param name="filename">Filename.</param>
        /// <param name="size">Size of the file.</param>
        public ToxFileSender SendFile(string filename, int size)
        {
            var fileSender = new ToxFileSender(this, filename, (ulong)size);
            fileSender = FileSenderFromFileNumber(fileSender.Number, () => fileSender);

            return fileSender;
        }
    }
}
