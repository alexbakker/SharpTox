using Microsoft.Win32.SafeHandles;

namespace SharpTox.Core
{
    /// <summary>
    /// Represents a handle for an instance of tox.
    /// </summary>
    public class ToxHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private ToxHandle()
            : base(true) { }

        /// <summary>
        /// Executes tox_kill to free the tox handle.
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle()
        {
            ToxFunctions.Kill(handle);
            return true;
        }
    }
}
