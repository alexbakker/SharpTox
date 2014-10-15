using System;
using System.Text;

namespace SharpTox.Dns
{
    /// <summary>
    /// Represents an instance of tox dns3.
    /// </summary>
    public class ToxDns : IDisposable
    {
        private ToxDnsHandle _toxDns3;
        private bool _disposed = false;

        /// <summary>
        /// The handle of this tox dns3 instance.
        /// </summary>
        public ToxDnsHandle Handle
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return _toxDns3;
            }
        }

        /// <summary>
        /// Initializes a new instance of tox dns3.
        /// </summary>
        /// <param name="publicKey"></param>
        public ToxDns(string publicKey)
        {
            _toxDns3 = ToxDnsFunctions.New(ToxTools.StringToHexBin(publicKey));

            if (_toxDns3 == null || _toxDns3.IsInvalid)
                throw new Exception("Could not create a new tox_dns3 instance with the provided public_key");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //dispose pattern as described on msdn for a class that uses a safe handle
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing) { }

            if (!_toxDns3.IsInvalid && !_toxDns3.IsClosed && _toxDns3 != null)
                _toxDns3.Dispose();

            _disposed = true;
        }

        /// <summary>
        /// Destroys the tox dns3 object.
        /// </summary>
        [Obsolete("Use Dispose() instead", true)]
        public void Kill()
        {
            _toxDns3.Dispose();
        }

        /// <summary>
        /// Generates a dns3 string used to query the dns server.
        /// </summary>
        /// <param name="name">Name of the registered user.</param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public string GenerateDns3String(string name, out uint requestId)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] bytes = Encoding.UTF8.GetBytes(name);
            byte[] result = new byte[1024];

            uint id = new uint();
            int length = ToxDnsFunctions.GenerateDns3String(_toxDns3, result, (ushort)result.Length, ref id, bytes, (byte)bytes.Length);
            requestId = id;

            if (length != -1)
                return Encoding.UTF8.GetString(result, 0, length);
            else
                throw new Exception("Failed to generate a dns3 string");
        }

        /// <summary>
        /// Decodes and decrypts the dns3 string returned by <see cref="GenerateDns3String"/>.
        /// </summary>
        /// <param name="dns3String"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public string DecryptDns3TXT(string dns3String, uint requestId)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] id = new byte[32 + sizeof(uint) + sizeof(ushort)];
            byte[] idRecordBytes = Encoding.UTF8.GetBytes(dns3String);

            int result = ToxDnsFunctions.DecryptDns3TXT(_toxDns3, id, idRecordBytes, (uint)idRecordBytes.Length, (uint)requestId);

            if (result == 0)
                return ToxTools.HexBinToString(id);
            else
                throw new Exception("Could not decrypt and decode id_record");
        }
    }
}
