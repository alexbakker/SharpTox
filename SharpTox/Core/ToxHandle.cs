using System;
using System.Runtime.InteropServices;

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

    public abstract class SafeHandleZeroOrMinusOneIsInvalid : SafeHandle
    {
        protected SafeHandleZeroOrMinusOneIsInvalid(bool ownsHandle)
            : base(IntPtr.Zero, ownsHandle)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero || handle == new IntPtr(-1); }
        }
    }
}
