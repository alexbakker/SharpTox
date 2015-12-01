using System;

namespace SharpTox.HL
{
    public class ToxException<T> : Exception
    {
        public T Error { get; private set; }

        public ToxException(T error)
        {
            Error = error;
        }
    }

    public class ToxException : Exception
    {
        public ToxException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
