using System;
using System.Text;

namespace SharpTox.Core
{
    public class ToxFileSender
    {
        public int Number { get; private set; }
        public ToxFriend Friend { get; private set; }
        public string Filename { get; private set; }

        private ulong _size;
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

        public ToxFileSenderType Type { get; private set; }

        private void CheckDisposed()
        {
            Friend.Tox.CheckDisposed();
        }

        private static int Create(ToxFriend friend, string filename, ulong size)
        {
            byte[] name = Encoding.UTF8.GetBytes(filename);
            if (name.Length > 255)
                throw new Exception("Filename is too long (longer than 255 bytes)");

            int result = ToxFunctions.NewFileSender(friend.Tox.Handle, friend.Number, size, name, (ushort)name.Length);
            if (result != -1)
                return result;
            else
                throw new Exception("Could not create new file sender");
        }

        internal ToxFileSender(ToxFriend friend, string filename, ulong size)
            : this(friend, Create(friend, filename, size), filename, size, ToxFileSenderType.Send)
        {
            CheckDisposed();

        }

        internal ToxFileSender(ToxFriend friend, int number, string filename, ulong size, ToxFileSenderType type)
        {
            Friend = friend;
            Number = number;
            Filename = filename;
            _size = size;
            Type = type;
        }

        internal ToxFileSender(ToxFriend friend, int number, string filename, ulong size)
            : this(friend, number, filename, size, ToxFileSenderType.Receive)
        {
        }

        /// <summary>
        /// Sends a file control request.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="data"></param>
        /// <returns>true on success and false on failure.</returns>
        public bool Control(ToxFileControl messageId, byte[] data)
        {
            CheckDisposed();
            return ToxFunctions.FileSendControl(Friend.Tox.Handle, Friend.Number, (byte)Type, (byte)Number, (byte)messageId, data, (ushort)(data == null ? 0 : data.Length)) == 0;
        }

        /// <summary>
        /// Sends file data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true on success and false on failure.</returns>
        unsafe public bool SendData(ArraySegment<byte> data)
        {
            CheckDisposed();
            var array = data.Array;
            fixed (byte *ptr = array)
            return ToxFunctions.FileSendData(Friend.Tox.Handle, Friend.Number, (byte)Number, (IntPtr)ptr + data.Offset, (ushort)data.Count) == 0;
        }

        /// <summary>
        /// Retrieves the number of bytes left to be sent/received.
        /// </summary>
        /// <value>The file data remaining.</value>
        public ulong FileDataRemaining
        {
            get
            {
                CheckDisposed();
                return ToxFunctions.FileDataRemaining(Friend.Tox.Handle, Friend.Number, (byte)Number, (byte)Type);
            }
        }
    }
}

