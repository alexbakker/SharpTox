#pragma warning disable 1591

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace SharpTox
{
    public static class ToxDnsFunctions
    {
        const string dll = "libtoxav";

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tox_dns3_new(byte[] public_key);
        public static IntPtr New(string public_key)
        {
            return tox_dns3_new(ToxTools.StringToHexBin(public_key));
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void tox_dns3_kill(IntPtr dns3_object);
        public static void Kill(IntPtr dns3_object)
        {
            tox_dns3_kill(dns3_object);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_generate_dns3_string(IntPtr dns3_object, byte[] str, ushort str_max_len, ref uint request_id, byte[] name, byte name_len);
        public static string GenerateDns3String(IntPtr dns3_object, string name, ref uint request_id)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(name);
            byte[] result = new byte[1024];

            int length = tox_generate_dns3_string(dns3_object, result, (ushort)result.Length, ref request_id, bytes, (byte)bytes.Length);

            if (length != -1)
                return Encoding.UTF8.GetString(result, 0, length);
            else
                throw new Exception("Failed to generate a dns3 string");
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int tox_decrypt_dns3_TXT(IntPtr dns3_object, byte[] tox_id, byte[] id_record, uint id_record_len, uint request_id);
        public static string DecryptDns3TXT(IntPtr dns3_object, string id_record, uint request_id)
        {
            byte[] id = new byte[32 + sizeof(uint) + sizeof(ushort)];
            byte[] id_record_bytes = Encoding.UTF8.GetBytes(id_record);

            int result = tox_decrypt_dns3_TXT(dns3_object, id, id_record_bytes, (uint)id_record_bytes.Length, (uint)request_id);

            if (result == 0)
                return ToxTools.HexBinToString(id);
            else
                throw new Exception("Could not decrypt and decode id_record");
        }
    }
}

#pragma warning restore 1591
