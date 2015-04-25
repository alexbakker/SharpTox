using System;
using System.IO;
using System.Linq;

using SharpTox.Encryption;

namespace SharpTox.Core
{
    /// <summary>
    /// Represents Tox data (unencrypted or encrypted).
    /// </summary>
    public class ToxData
    {
        private bool _encrypted;
        private byte[] _data;

        /// <summary>
        /// Whether or not this data is encrypted.
        /// </summary>
        public bool IsEncrypted
        {
            get
            {
                return _encrypted;
            }
        }

        /// <summary>
        /// The Tox data in a byte array.
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                return _data;
            }
        }

        internal ToxData(byte[] data)
        {
            _data = data;
            _encrypted = ToxEncryptionFunctions.IsDataEncrypted(data);
        }

#if !IS_PORTABLE
        /// <summary>
        /// Saves this Tox data to the disk with the specified filename.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool Save(string filename)
        {
            try
            {
                if (_data.Length == 0)
                    return false;

                FileStream stream = new FileStream(filename, FileMode.Create);
                stream.Write(_data, 0, _data.Length);
                stream.Close();

                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Loads Tox data from disk and creates a new instance of ToxData.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static ToxData FromDisk(string filename)
        {
            byte[] bytes;
            try
            {
                FileInfo info = new FileInfo(filename);
                FileStream stream = new FileStream(filename, FileMode.Open);
                bytes = new byte[info.Length];

                stream.Read(bytes, 0, (int)info.Length);
                stream.Close();
            }
            catch { return null; }

            if (bytes == null || bytes.Length == 0)
                return null;

            return new ToxData(bytes);
        }
#endif

        /// <summary>
        /// Creates a new instance of ToxData from the specified byte array.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static ToxData FromBytes(byte[] bytes)
        {
            return new ToxData(bytes);
        }

        public static bool operator ==(ToxData data1, ToxData data2)
        {
            if (data1 == null)
                throw new ArgumentNullException("data1");

            if (data2 == null)
                throw new ArgumentNullException("data2");

            if (object.ReferenceEquals(data1, data2))
                return true;

            if ((object)data1 == null ^ (object)data2 == null)
                return false;

            return data1._data.SequenceEqual(data2._data);
        }

        public static bool operator !=(ToxData data1, ToxData data2)
        {
            return !(data1 == data2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            ToxData data = obj as ToxData;
            if ((object)data == null)
                return false;

            return this == data;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
