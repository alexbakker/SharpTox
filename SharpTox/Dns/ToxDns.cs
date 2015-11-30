using SharpTox.Core;
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
        internal ToxDnsHandle Handle
        {
            get { return _toxDns3; }
        }

        /// <summary>
        /// Initializes a new instance of tox dns3.
        /// </summary>
        /// <param name="publicKey">The public key that this instance of toxdns should be initialized with.</param>
        public ToxDns(ToxKey publicKey)
        {
            _toxDns3 = ToxDnsFunctions.New(publicKey.GetBytes());

            if (_toxDns3 == null || _toxDns3.IsInvalid)
                throw new Exception("Could not create a new tox_dns3 instance with the provided publicKey");
        }

        /// <summary>
        /// Releases all resources used by this instance of tox dns3.
        /// </summary>
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

            if (_toxDns3 != null && !_toxDns3.IsInvalid && !_toxDns3.IsClosed)
                _toxDns3.Dispose();

            _disposed = true;
        }

        /// <summary>
        /// Generates a dns3 string used to query the dns server.
        /// </summary>
        /// <param name="name">Name of the registered user.</param>
        /// <param name="requestId">The request id, to be used when calling DecryptDns3TXT.</param>
        /// <returns></returns>
        public string GenerateDns3String(string name, out int requestId)
        {
            ThrowIfDisposed();

            byte[] bytes = Encoding.UTF8.GetBytes(name);
            byte[] result = new byte[1024];

            uint id = new uint();
            int length = ToxDnsFunctions.GenerateDns3String(_toxDns3, result, (ushort)result.Length, ref id, bytes, (byte)bytes.Length);
            requestId = ToxTools.Map(id);

            if (length != -1)
                return Encoding.UTF8.GetString(result, 0, length);
            else
                throw new Exception("Failed to generate a dns3 string");
        }

        /// <summary>
        /// Decodes and decrypts the dns3 string returned by <see cref="GenerateDns3String"/>.
        /// </summary>
        /// <param name="dns3String">String to decrypt.</param>
        /// <param name="requestId">The request id retrieved with GenerateDns3String.</param>
        /// <returns></returns>
        public string DecryptDns3TXT(string dns3String, int requestId)
        {
            ThrowIfDisposed();

            byte[] id = new byte[ToxConstants.AddressSize];
            byte[] idRecordBytes = Encoding.UTF8.GetBytes(dns3String);

            int result = ToxDnsFunctions.DecryptDns3TXT(_toxDns3, id, idRecordBytes, (uint)idRecordBytes.Length, ToxTools.Map(requestId));

            if (result == 0)
                return ToxTools.HexBinToString(id);
            else
                throw new Exception("Could not decrypt and decode id_record");
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
