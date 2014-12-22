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
        public static void Answer(this ToxAvCall call)
        {
            call.Answer(ToxAv.DefaultCodecSettings);
        }
    }
}

