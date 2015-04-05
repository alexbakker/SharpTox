using System;

namespace SharpTox.Encryption
{
    public class ToxEncryptionKey
    {
        public byte[] Bytes { get; private set; }

        public byte[] Salt { get; private set; }

        public ToxEncryptionKey(string passphrase, byte[] salt = null)
        {
            var key = salt == null ? ToxEncryption.DeriveKey(passphrase) : ToxEncryption.DeriveKey(passphrase, salt);
            if (key == null)
                throw new Exception("Could not derive key from passphrase");

            Bytes = key.Value.Key;
            Salt = key.Value.Salt;
        }

        internal ToxPassKey ToPassKey()
        {
            return new ToxPassKey(Bytes, Salt);
        }
    }
}
