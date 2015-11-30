using SharpTox.Core;

namespace SharpTox.Av
{
    /// <summary>
    /// Represents a handle for an instance of toxav.
    /// </summary>
    internal class ToxAvHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private ToxAvHandle()
            : base(true) { }

        /// <summary>
        /// Executes toxav_kill to free the tox handle.
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle()
        {
            ToxAvFunctions.Kill(handle);
            return true;
        }
    }
}
