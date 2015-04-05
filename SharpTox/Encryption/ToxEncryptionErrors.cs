namespace SharpTox.Encryption
{
    public enum ToxErrorKeyDerivation
    {
        Ok,
        Null,
        Failed
    }

    public enum ToxErrorEncryption
    {
        Ok,
        Null,
        KeyDerivationFailed,
        Failed
    }

    public enum ToxErrorDecryption
    {
        Ok,
        Null,
        InvalidLength,
        BadFormat,
        KeyDerivationFailed,
        Failed
    }
}
