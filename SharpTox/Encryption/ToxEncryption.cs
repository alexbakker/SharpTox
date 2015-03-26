using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpTox.Encryption
{
    public static class ToxEncryption
    {
        public static byte[] EncryptData(byte[] data, ToxEncryptionKey key)
        {
            byte[] output = new byte[data.Length + ToxEncryptionFunctions.PassEncryptionExtraLength()];

            if (ToxEncryptionFunctions.PassKeyEncrypt(data, (uint)data.Length, key.Bytes, output) == -1)
                return null;

            return output;
        }

        public static byte[] DecryptData(byte[] data, ToxEncryptionKey key)
        {
            byte[] output = new byte[data.Length - ToxEncryptionFunctions.PassEncryptionExtraLength()];

            if (ToxEncryptionFunctions.PassKeyDecrypt(data, (uint)data.Length, key.Bytes, output) == -1)
                return null;

            return output;
        }

        public static bool IsDataEncrypted(byte[] data)
        {
            return ToxEncryptionFunctions.IsDataEncrypted(data) == 1;
        }

        public static byte[] GetSalt(byte[] data)
        {
            byte[] salt = new byte[ToxEncryptionFunctions.PassSaltLength()];

            if (ToxEncryptionFunctions.GetSalt(data, salt) == -1)
                return null;

            return salt;
        }

        internal static byte[] DeriveKey(string passphrase)
        {
            byte[] pp = Encoding.UTF8.GetBytes(passphrase);
            byte[] key = new byte[ToxEncryptionFunctions.PassKeyLength()];

            if (ToxEncryptionFunctions.DeriveKeyFromPass(pp, (uint)pp.Length, key) == -1)
                return null;

            return key;
        }

        internal static byte[] DeriveKey(string passphrase, byte[] salt)
        {
            if (salt.Length < ToxEncryptionFunctions.PassSaltLength())
                return null;

            byte[] pp = Encoding.UTF8.GetBytes(passphrase);
            byte[] key = new byte[ToxEncryptionFunctions.PassKeyLength()];

            if (ToxEncryptionFunctions.DeriveKeyWithSalt(pp, (uint)pp.Length, salt, key) == -1)
                return null;

            return key;
        }
    }
}
