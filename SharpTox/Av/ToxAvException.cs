using System;
using SharpTox.Core;
using SharpTox.Av;

namespace SharpTox
{
    public class ToxAvException : ToxException
    {
        public ToxAvError Error { get; private set; }

        public ToxAvException(ToxAvError error)
            : base(error.ToString())
        {
            Error = error;
        }

        internal static void Check(ToxAvError error)
        {
            if (error != ToxAvError.None)
                throw new ToxAvException(error);
        }
    }
}

