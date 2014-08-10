#pragma warning disable 1591

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace SharpTox.Dns
{
    public static class ToxDnsFunctions
    {
        const string dll = "libtox";

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_dns3_new")]
        public static extern ToxDnsHandle New(byte[] public_key);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_dns3_kill")]
        public static extern void Kill(IntPtr dns3_object);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_generate_dns3_string")]
        public static extern int GenerateDns3String(ToxDnsHandle dns3_object, byte[] str, ushort str_max_len, ref uint request_id, byte[] name, byte name_len);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_decrypt_dns3_TXT")]
        public static extern int DecryptDns3TXT(ToxDnsHandle dns3_object, byte[] tox_id, byte[] id_record, uint id_record_len, uint request_id);
    }
}

#pragma warning restore 1591
