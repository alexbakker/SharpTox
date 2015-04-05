using System;
using System.Runtime.InteropServices;

namespace SharpTox.Encryption
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ToxPassKey
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ToxEncryption.SaltLength)]
        internal byte[] Salt;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ToxEncryption.KeyLength)]
        internal byte[] Key;

        internal ToxPassKey(byte[] key, byte[] salt = null)
        {
            Key = key;
            Salt = salt;
        }
    }
}
