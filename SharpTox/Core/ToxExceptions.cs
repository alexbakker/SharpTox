using System;

using SharpTox.Core;

namespace SharpTox.Core
{
    public class ToxAFException : Exception
    {
        public ToxAFError Error { get { return error; } }
        private ToxAFError error;

        public ToxAFException(ToxAFError error)
            : base("Could not add address to the friend list")
        {
            this.error = error;
        }
    }
}
