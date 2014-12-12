using System;
using System.Runtime.InteropServices;

using SharpTox.Core;

namespace SharpTox.Encryption
{
    public static class ToxEncryptionFunctions
    {
#if POSIX
		const string dll = "libtoxencryptsave.so";
#else 
		const string dll = "libtox";
#endif

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_pass_encryption_extra_length")]
        public static extern int PassEncryptionExtraLength();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_pass_key_length")]
        public static extern int PassKeyLength();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_pass_salt_length")]
        public static extern int PassSaltLength();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_encrypted_size")]
        public static extern uint EncryptedSize(ToxHandle tox);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_derive_key_from_pass")]
        public static extern int DeriveKeyFromPass(byte[] passphrase, uint passphraseLength, byte[] outputKey);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_derive_key_with_salt")]
        public static extern int DeriveKeyWithSalt(byte[] passphrase, uint passphraseLength, byte[] salt, byte[] outputKey);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_salt")]
        public static extern int GetSalt(byte[] data, byte[] salt);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_pass_key_encrypt")]
        public static extern int PassKeyEncrypt(byte[] data, uint dataLength, byte[] key, byte[] output);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_pass_encrypt")]
        public static extern int PassEncrypt(byte[] data, uint len, byte[] passphrase, uint passphraseLength, byte[] output);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_encrypted_save")]
        public static extern int EncryptedSave(ToxHandle tox, byte[] data, byte[] passphrase, uint passphraseLength);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_pass_key_decrypt")]
        public static extern int PassKeyDecrypt(byte[] data, uint length, byte[] key, byte[] output);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_pass_decrypt")]
        public static extern int PassDecrypt(byte[] data, uint length, byte[] passphrase, uint passphraseLength, byte[] output);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_encrypted_load")]
        public static extern int EncryptedLoad(ToxHandle tox, byte[] data, uint length, byte[] passphrase, uint passphraseLength);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_is_save_encrypted")]
        [Obsolete("Use IsDataEncrypted instead")]
        public static extern int IsSaveEncrypted(byte[] data);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_is_data_encrypted")]
        public static extern int IsDataEncrypted(byte[] data);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_encrypted_key_save")]
        public static extern int EncryptedKeySave(ToxHandle tox, byte[] data, byte[] key);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_encrypted_key_load")]
        public static extern int EncryptedKeyLoad(ToxHandle tox, byte[] data, uint dataLength, byte[] key);
    }
}
