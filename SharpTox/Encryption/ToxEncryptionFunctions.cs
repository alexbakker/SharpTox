using System;
using System.Runtime.InteropServices;

using SharpTox.Core;

namespace SharpTox.Encryption
{
    public static class ToxEncryptionFunctions
    {
        const string dll = "libtox";

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_encrypted_size")]
        public static extern uint EncryptedSize(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_derive_key_from_pass")]
        public static extern int DeriveKeyFromPass(byte[] passphrase, uint pplength, byte[] out_key);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_pass_key_encrypt")]
        public static extern int PassKeyEncrypt(byte[] data, uint data_len, byte[] key, byte[] output);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_encrypted_save")]
        public static extern int EncryptedSave(ToxHandle tox, byte[] data, byte[] passphrase, uint pplength);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_pass_decrypt")]
        public static extern int PassDecrypt(byte[] data, uint length, uint[] passphrase, uint pplength, byte[] output);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_encrypted_load")]
        public static extern int EncryptedLoad(ToxHandle tox, byte[] data, uint length, byte[] passphrase, uint pplength);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_is_save_encrypted")]
        public static extern int IsSaveEncrypted(byte[] data);
    }
}
