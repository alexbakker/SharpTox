using System;

namespace SharpTox.Av
{
    /// <summary>
    /// Useful ToxAv extensions.
    /// </summary>
    public static class ToxAvExtensions
    {
        /// <summary>
        /// Answer a call with default codec settings.
        /// </summary>
        /// <param name="call">Call.</param>
        public static ToxAvError Answer(this ToxAvCall call)
        {
            return call.Answer(ToxAv.DefaultCodecSettings);
        }
    }
}

