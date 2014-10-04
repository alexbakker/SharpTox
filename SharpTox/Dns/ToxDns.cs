using System;
using System.Text;

namespace SharpTox.Dns
{
    /// <summary>
    /// Represents an instance of tox dns3.
    /// </summary>
    public class ToxDns : IDisposable
    {
        private ToxDnsHandle tox_dns3;
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of tox dns3.
        /// </summary>
        /// <param name="public_key"></param>
        public ToxDns(string public_key)
        {
            tox_dns3 = ToxDnsFunctions.New(ToxTools.StringToHexBin(public_key));

            if (tox_dns3 == null || tox_dns3.IsInvalid)
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
            if (disposed)
                return;

            if (disposing) { }

            if (!tox_dns3.IsInvalid && !tox_dns3.IsClosed && tox_dns3 != null)
                tox_dns3.Dispose();

            disposed = true;
        }

        /// <summary>
        /// Destroys the tox dns3 object.
        /// </summary>
        [Obsolete("Use Dispose() instead", true)]
        public void Kill()
        {
            tox_dns3.Dispose();
        }

        /// <summary>
        /// Generates a dns3 string used to query the dns server.
        /// </summary>
        /// <param name="name">Name of the registered user.</param>
        /// <param name="request_id"></param>
        /// <returns></returns>
        public string GenerateDns3String(string name, out uint request_id)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] bytes = Encoding.UTF8.GetBytes(name);
            byte[] result = new byte[1024];

            uint id = new uint();
            int length = ToxDnsFunctions.GenerateDns3String(tox_dns3, result, (ushort)result.Length, ref id, bytes, (byte)bytes.Length);
            request_id = id;

            if (length != -1)
                return Encoding.UTF8.GetString(result, 0, length);
            else
                throw new Exception("Failed to generate a dns3 string");
        }

        /// <summary>
        /// Decodes and decrypts the dns3 string returned by <see cref="GenerateDns3String"/>.
        /// </summary>
        /// <param name="dns3_string"></param>
        /// <param name="request_id"></param>
        /// <returns></returns>
        public string DecryptDns3TXT(string dns3_string, uint request_id)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            byte[] id = new byte[32 + sizeof(uint) + sizeof(ushort)];
            byte[] id_record_bytes = Encoding.UTF8.GetBytes(dns3_string);

            int result = ToxDnsFunctions.DecryptDns3TXT(tox_dns3, id, id_record_bytes, (uint)id_record_bytes.Length, (uint)request_id);

            if (result == 0)
                return ToxTools.HexBinToString(id);
            else
                throw new Exception("Could not decrypt and decode id_record");
        }

        /// <summary>
        /// Retrieves the handle of this tox dns3 object.
        /// </summary>
        /// <returns></returns>
        public ToxDnsHandle GetHandle()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return tox_dns3;
        }
    }
}
