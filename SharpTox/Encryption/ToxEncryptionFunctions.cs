using System.Runtime.InteropServices;

namespace SharpTox.Encryption
{
    internal static class ToxEncryptionFunctions
    {
#if POSIX
        const string dll = "libtoxencryptsave.so";
#else 
		const string dll = "libtox";
#endif

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_derive_key_from_pass")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool DeriveKeyFromPass(byte[] passphrase, uint passphraseLength, ref ToxPassKey outputKey, ref ToxErrorKeyDerivation error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_derive_key_with_salt")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool DeriveKeyWithSalt(byte[] passphrase, uint passphraseLength, byte[] salt, ref ToxPassKey outputKey, ref ToxErrorKeyDerivation error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_get_salt")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool GetSalt(byte[] data, byte[] salt);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_pass_key_encrypt")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool PassKeyEncrypt(byte[] data, uint dataLength, ref ToxPassKey key, byte[] output, ref ToxErrorEncryption error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_pass_encrypt")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool PassEncrypt(byte[] data, uint len, byte[] passphrase, uint passphraseLength, byte[] output, ref ToxErrorEncryption error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_pass_key_decrypt")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool PassKeyDecrypt(byte[] data, uint length, ref ToxPassKey key, byte[] output, ref ToxErrorDecryption error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_pass_decrypt")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool PassDecrypt(byte[] data, uint length, byte[] passphrase, uint passphraseLength, byte[] output, ref ToxErrorDecryption error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tox_is_data_encrypted")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool IsDataEncrypted(byte[] data);
    }
}
