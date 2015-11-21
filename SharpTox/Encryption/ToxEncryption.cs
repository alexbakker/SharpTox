using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpTox.Encryption
{
    public static class ToxEncryption
    {
        internal const int SaltLength = 32;
        internal const int KeyLength = 32;
        internal const int EncryptionExtraLength = 80;

        public static byte[] EncryptData(byte[] data, ToxEncryptionKey key, out ToxErrorEncryption error)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (key == null)
                throw new ArgumentNullException("key");

            byte[] output = new byte[data.Length + EncryptionExtraLength];
            var pass = key.ToPassKey();
            error = ToxErrorEncryption.Ok;

            if (!ToxEncryptionFunctions.PassKeyEncrypt(data, (uint)data.Length, ref pass, output, ref error) || error != ToxErrorEncryption.Ok)
                return null;

            return output;
        }

        public static byte[] EncryptData(byte[] data, ToxEncryptionKey key)
        {
            var error = ToxErrorEncryption.Ok;
            return EncryptData(data, key, out error);
        }

        public static byte[] EncryptData(byte[] data, string password, out ToxErrorEncryption error)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (password == null)
                throw new ArgumentNullException("password");

            byte[] output = new byte[data.Length + EncryptionExtraLength];
            byte[] passBytes = Encoding.UTF8.GetBytes(password);
            error = ToxErrorEncryption.Ok;

            if (!ToxEncryptionFunctions.PassEncrypt(data, (uint)data.Length, passBytes, (uint)passBytes.Length, output, ref error) || error != ToxErrorEncryption.Ok)
                return null;

            return output;
        }

        public static byte[] EncryptData(byte[] data, string password)
        {
            var error = ToxErrorEncryption.Ok;
            return EncryptData(data, password, out error);
        }

        public static byte[] DecryptData(byte[] data, ToxEncryptionKey key, out ToxErrorDecryption error)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (key == null)
                throw new ArgumentNullException("key");

            byte[] output = new byte[data.Length - EncryptionExtraLength];
            var pass = key.ToPassKey();
            error = ToxErrorDecryption.Ok;

            if (!ToxEncryptionFunctions.PassKeyDecrypt(data, (uint)data.Length, ref pass, output, ref error) || error != ToxErrorDecryption.Ok)
                return null;

            return output;
        }

        public static byte[] DecryptData(byte[] data, ToxEncryptionKey key)
        {
            var error = ToxErrorDecryption.Ok;
            return DecryptData(data, key, out error);
        }

        public static byte[] DecryptData(byte[] data, string password, out ToxErrorDecryption error)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (password == null)
                throw new ArgumentNullException("password");

            byte[] output = new byte[data.Length - EncryptionExtraLength];
            byte[] passBytes = Encoding.UTF8.GetBytes(password);
            error = ToxErrorDecryption.Ok;

            if (!ToxEncryptionFunctions.PassDecrypt(data, (uint)data.Length, passBytes, (uint)passBytes.Length, output, ref error) || error != ToxErrorDecryption.Ok)
                return null;

            return output;
        }

        public static byte[] DecryptData(byte[] data, string password)
        {
            var error = ToxErrorDecryption.Ok;
            return DecryptData(data, password, out error);
        }

        public static bool IsDataEncrypted(byte[] data)
        {
            return ToxEncryptionFunctions.IsDataEncrypted(data);
        }

        public static byte[] GetSalt(byte[] data)
        {
            byte[] salt = new byte[SaltLength];

            if (!ToxEncryptionFunctions.GetSalt(data, salt))
                return null;

            return salt;
        }

        internal static ToxPassKey? DeriveKey(string passphrase)
        {
            byte[] pp = Encoding.UTF8.GetBytes(passphrase);
            var error = ToxErrorKeyDerivation.Ok;
            var key = new ToxPassKey();

            if (!ToxEncryptionFunctions.DeriveKeyFromPass(pp, (uint)pp.Length, ref key, ref error) || error != ToxErrorKeyDerivation.Ok)
                return null;

            return key;
        }

        internal static ToxPassKey? DeriveKey(string passphrase, byte[] salt)
        {
            if (salt.Length < SaltLength)
                return null;

            byte[] pp = Encoding.UTF8.GetBytes(passphrase);
            var error = ToxErrorKeyDerivation.Ok;
            var key = new ToxPassKey();

            if (!ToxEncryptionFunctions.DeriveKeyWithSalt(pp, (uint)pp.Length, salt, ref key, ref error) || error != ToxErrorKeyDerivation.Ok)
                return null;

            return key;
        }
    }
}
