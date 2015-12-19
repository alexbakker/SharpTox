using System;

namespace SharpTox
{
    public class ToxFileTransferError
    {
        public string Message { get; private set; }

        /// <summary>
        /// If no exception occurred, this will be null.
        /// </summary>
        public Exception Exception { get; private set; }

        public ToxFileTransferError(string message, Exception innerException = null)
        {
            Message = message;
            Exception = innerException;
        }
    }
}
