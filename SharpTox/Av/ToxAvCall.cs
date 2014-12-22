using System;

namespace SharpTox.Av
{
    public class ToxAvCall
    {
        public ToxAv ToxAv { get; private set; }
        public int Index { get; private set; }

        public ToxAvCall(ToxAv toxAv, int callIndex)
        {
            ToxAv = toxAv;
            Index = callIndex;
        }

        /// <summary>
        /// Answers a call.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public ToxAvError Answer(ToxAvCodecSettings settings)
        {
            ToxAv.CheckDisposed();

            return ToxAvFunctions.Answer(ToxAv.Handle, Index, ref settings);
        }
    }
}

