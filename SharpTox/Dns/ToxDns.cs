#pragma warning disable 1591

using System;

namespace SharpTox.Dns
{
    public class ToxDns
    {
        private IntPtr tox_dns3;

        private Object obj;

        /// <summary>
        /// Initializes a new instance of tox dns3.
        /// </summary>
        /// <param name="public_key"></param>
        public ToxDns(string public_key)
        {
            obj = new Object();

            tox_dns3 = ToxDnsFunctions.New(public_key);

            if (tox_dns3 == IntPtr.Zero)
                throw new Exception("Could not create a new tox_dns3 instance with the provided public_key");
        }

        /// <summary>
        /// Destroys the tox dns3 object.
        /// </summary>
        public void Kill()
        {
            lock (obj)
            {
                if (tox_dns3 == IntPtr.Zero)
                    throw null;

                ToxDnsFunctions.Kill(tox_dns3);
            }
        }

        /// <summary>
        /// Generates a dns3 string used to query the dns server.
        /// </summary>
        /// <param name="name">Name of the registered user.</param>
        /// <param name="request_id"></param>
        /// <returns></returns>
        public string GenerateDns3String(string name, out uint request_id)
        {
            lock (obj)
            {
                if (tox_dns3 == IntPtr.Zero)
                    throw null;

                uint id = new uint();
                string result = ToxDnsFunctions.GenerateDns3String(tox_dns3, name, ref id);

                request_id = id;
                return result;
            }
        }

        /// <summary>
        /// Decodes and decrypts the dns3 string returned by GenerateDns3String.
        /// </summary>
        /// <param name="dns3_string"></param>
        /// <param name="request_id"></param>
        /// <returns></returns>
        public string DecryptDns3TXT(string dns3_string, uint request_id)
        {
            lock (obj)
            {
                if (tox_dns3 == IntPtr.Zero)
                    throw null;

                return ToxDnsFunctions.DecryptDns3TXT(tox_dns3, dns3_string, request_id);
            }
        }

        /// <summary>
        /// Retrieves the pointer of this tox dns3 object.
        /// </summary>
        /// <returns></returns>
        public IntPtr GetPointer()
        {
            return tox_dns3;
        }
    }
}

#pragma warning restore 1591