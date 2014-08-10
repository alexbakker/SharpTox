using System;

namespace SharpTox.Dns
{
    /// <summary>
    /// Represents an instance of tox dns3.
    /// </summary>
    public class ToxDns : IDisposable
    {
        private ToxDnsHandle tox_dns3;

        /// <summary>
        /// Initializes a new instance of tox dns3.
        /// </summary>
        /// <param name="public_key"></param>
        public ToxDns(string public_key)
        {
            tox_dns3 = ToxDnsFunctions.New(public_key);

            if (tox_dns3.IsClosed || tox_dns3.IsInvalid)
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
            if (disposing) { }

            if (!tox_dns3.IsInvalid && !tox_dns3.IsClosed && tox_dns3 != null)
                tox_dns3.Dispose();
        }

        /// <summary>
        /// Destroys the tox dns3 object.
        /// </summary>
        [Obsolete("This function is obsolete, use Dispose() instead", true)]
        public void Kill()
        {
            if (tox_dns3.IsClosed || tox_dns3.IsInvalid)
                throw null;

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
            if (tox_dns3.IsClosed || tox_dns3.IsInvalid)
                throw null;

            uint id = new uint();
            string result = ToxDnsFunctions.GenerateDns3String(tox_dns3, name, ref id);

            request_id = id;
            return result;
        }

        /// <summary>
        /// Decodes and decrypts the dns3 string returned by GenerateDns3String.
        /// </summary>
        /// <param name="dns3_string"></param>
        /// <param name="request_id"></param>
        /// <returns></returns>
        public string DecryptDns3TXT(string dns3_string, uint request_id)
        {
            if (tox_dns3.IsClosed || tox_dns3.IsInvalid)
                throw null;

            return ToxDnsFunctions.DecryptDns3TXT(tox_dns3, dns3_string, request_id);
        }

        /// <summary>
        /// Retrieves the handle of this tox dns3 object.
        /// </summary>
        /// <returns></returns>
        public ToxDnsHandle GetHandle()
        {
            return tox_dns3;
        }
    }
}
