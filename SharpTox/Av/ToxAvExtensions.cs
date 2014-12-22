using System;
using SharpTox.Core;

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

        /// <summary>
        /// Creates a new call.
        /// </summary>
        /// <param name="friend"></param>
        /// <param name="settings"></param>
        /// <param name="ringingSeconds"></param>
        /// <returns>a call</returns>
        public static ToxAvCall Call(this ToxFriend friend, ToxAvCodecSettings settings, int ringingSeconds)
        {
            friend.Tox.CheckDisposed();
            friend.Tox.ToxAv.CheckDisposed();

            int index;
            ToxAvException.Check(ToxAvFunctions.Call(friend.Tox.ToxAv.Handle, out index, friend.Number, ref settings, ringingSeconds));
            return friend.Tox.ToxAv.CallFromCallIndex(index);
        }
    }
}

