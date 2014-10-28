using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpTox.Encryption
{
    public static class ToxEncryption
    {
        public static bool IsDataEncrypted(byte[] data)
        {
            return ToxEncryptionFunctions.IsDataEncrypted(data) == 1;
        }

        public static byte[] DeriveKey(string passphrase)
        {
            byte[] pp = Encoding.UTF8.GetBytes(passphrase);
            byte[] key = new byte[ToxEncryptionFunctions.PassKeyLength()];

            if (ToxEncryptionFunctions.DeriveKeyFromPass(pp, (uint)pp.Length, key) == -1)
                return null;

            return key;
        }

        public static byte[] DeriveKey(string passphrase, byte[] salt)
        {
            if (salt.Length < ToxEncryptionFunctions.PassSaltLength())
                return null;

            byte[] pp = Encoding.UTF8.GetBytes(passphrase);
            byte[] key = new byte[ToxEncryptionFunctions.PassKeyLength()];

            if (ToxEncryptionFunctions.DeriveKeyWithSalt(pp, (uint)pp.Length, salt, key) == -1)
                return null;

            return key;
        }

        public static byte[] GetSalt(byte[] data)
        {
            byte[] salt = new byte[ToxEncryptionFunctions.PassSaltLength()];

            if (ToxEncryptionFunctions.GetSalt(data, salt) == -1)
                return null;

            return salt;
        }

        public static byte[] EncryptData(byte[] data, string passphrase)
        {
            byte[] output = new byte[data.Length + ToxEncryptionFunctions.PassEncryptionExtraLength()];
            byte[] pp = Encoding.UTF8.GetBytes(passphrase);

            if (ToxEncryptionFunctions.PassEncrypt(data, (uint)data.Length, pp, (uint)pp.Length, output) == -1)
                return null;

            return output;
        }

        public static byte[] DecryptData(byte[] data, string passphrase)
        {
            byte[] output = new byte[data.Length + ToxEncryptionFunctions.PassEncryptionExtraLength()];
            byte[] pp = Encoding.UTF8.GetBytes(passphrase);
            byte[] result;

            int length = ToxEncryptionFunctions.PassDecrypt(data, (uint)data.Length, pp, (uint)pp.Length, output);
            if (length == -1)
                return null;

            result = new byte[length];
            Array.Copy(output, 0, result, 0, length);

            return result;
        }
    }
}
