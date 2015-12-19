using System;

namespace SharpTox.HL.Transfers
{
    public class ToxTransferError
    {
        public string Message { get; private set; }

        /// <summary>
        /// If no exception occurred, this will be null.
        /// </summary>
        public Exception Exception { get; private set; }

        public ToxTransferError(string message, Exception innerException = null)
        {
            Message = message;
            Exception = innerException;
        }
    }
}
