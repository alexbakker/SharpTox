using System;

using SharpTox.Core;

namespace SharpTox.Core
{
    /// <summary>
    /// The exception that is thrown when an error occured while adding a friend
    /// </summary>
    public class ToxAFException : Exception
    {
        /// <summary>
        /// The ToxAFError for this exception.
        /// </summary>
        public ToxAFError Error { get { return error; } }
        private ToxAFError error;

        /// <summary>
        /// Initialises a new instance of the ToxAFException class with a specified error.
        /// </summary>
        /// <param name="error"></param>
        public ToxAFException(ToxAFError error)
            : base("Could not add address to the friend list")
        {
            this.error = error;
        }
    }
}
