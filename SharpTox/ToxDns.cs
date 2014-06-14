#pragma warning disable 1591

using System;

namespace SharpTox
{
    public class ToxDns
    {
        private IntPtr tox_dns3;

        private Object obj;

        public ToxDns(string public_key)
        {
            obj = new Object();

            tox_dns3 = ToxDnsFunctions.New(public_key);

            if (tox_dns3 == IntPtr.Zero)
                throw new Exception("Could not create a new tox_dns3 instance with the provided public_key");
        }

        public void Kill()
        {
            lock (obj)
            {
                if (tox_dns3 == IntPtr.Zero)
                    throw null;

                ToxDnsFunctions.Kill(tox_dns3);
            }
        }

        public string GenerateDns3String(string name)
        {
            lock (obj)
            {
                if (tox_dns3 == IntPtr.Zero)
                    throw null;

                return ToxDnsFunctions.GenerateDns3String(tox_dns3, name);
            }
        }

        public string DecryptDns3TXT(string dns3_string)
        {
            lock (obj)
            {
                if (tox_dns3 == IntPtr.Zero)
                    throw null;

                return ToxDnsFunctions.DecryptDns3TXT(tox_dns3, dns3_string);
            }
        }

        public IntPtr GetPointer()
        {
            return tox_dns3;
        }
    }
}

#pragma warning restore 1591