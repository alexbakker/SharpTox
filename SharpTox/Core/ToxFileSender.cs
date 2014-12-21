using System;
using System.Text;

namespace SharpTox.Core
{
    public class ToxFileSender
    {
        public int Number { get; private set; }
        public ToxFriend Friend { get; private set; }
        public string Filename { get; private set; }
        public long LongSize
        {
            get
            {
                return (long)_size;
            }
        }
        public int Size
        {
            get
            {
                return (int)_size;
            }
        }

        private ulong _size;

        private void CheckDisposed()
        {
            Friend.Tox.CheckDisposed();
        }

        public ToxFileSender(ToxFriend friend, string filename, ulong size)
        {

            Friend = friend;
            Filename = filename;
            _size = size;

            CheckDisposed();

            byte[] name = Encoding.UTF8.GetBytes(filename);
            if (name.Length > 255)
                throw new Exception("Filename is too long (longer than 255 bytes)");

            int result = ToxFunctions.NewFileSender(Friend.Tox.Handle, Friend.Number, _size, name, (ushort)name.Length);
            if (result != -1)
                Number = result;
            else
                throw new Exception("Could not create new file sender");
        }

        public ToxFileSender(ToxFriend friend, string filename, long size)
            : this (friend, filename, (ulong)size)
        {
        }

        public ToxFileSender(ToxFriend friend, string filename, int size)
            : this (friend, filename, (ulong)size)
        {
        }

        /// <summary>
        /// Sends a file control request.
        /// </summary>
        /// <param name="sendReceive">0 if we're sending and 1 if we're receiving.</param>
        /// <param name="messageId"></param>
        /// <param name="data"></param>
        /// <returns>true on success and false on failure.</returns>
        public bool Control(int sendReceive, ToxFileControl messageId, byte[] data)
        {
            CheckDisposed();
            return ToxFunctions.FileSendControl(Friend.Tox.Handle, Friend.Number, (byte)sendReceive, (byte)Number, (byte)messageId, data, (ushort)data.Length) == 0;
        }

        /// <summary>
        /// Sends file data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true on success and false on failure.</returns>
        public bool FileSendData(byte[] data)
        {
            CheckDisposed();
            return ToxFunctions.FileSendData(Friend.Tox.Handle, Friend.Number, (byte)Number, data, (ushort)data.Length) == 0;
        }

        /// <summary>
        /// Retrieves the number of bytes left to be sent/received.
        /// </summary>
        /// <param name="sendReceive">0 if we're sending and 1 if we're receiving.</param>
        /// <returns></returns>
        public ulong FileDataRemaining(int sendReceive)
        {
            CheckDisposed();
            return ToxFunctions.FileDataRemaining(Friend.Tox.Handle, Friend.Number, (byte)Number, (byte)sendReceive);
        }
    }
}

