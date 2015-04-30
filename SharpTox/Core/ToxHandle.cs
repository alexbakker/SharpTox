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

    /// <summary>
    /// Provides a base class for Win32 safe handle implementations in which the value of either 0 or -1 indicates an invalid handle.
    /// </summary>
    public abstract class SafeHandleZeroOrMinusOneIsInvalid : SafeHandle
    {
        /// <summary>
        /// Initializes a new instance of the SafeHandleZeroOrMinusOneIsInvalid class, specifying whether the handle is to be reliably released.
        /// </summary>
        /// <param name="ownsHandle">true to reliably release the handle during the finalization phase; false to prevent reliable release (not recommended).</param>
        protected SafeHandleZeroOrMinusOneIsInvalid(bool ownsHandle)
            : base(IntPtr.Zero, ownsHandle)
        {
        }

        /// <summary>
        /// Gets a value that indicates whether the handle is invalid.
        /// </summary>
        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero || handle == new IntPtr(-1); }
        }
    }
}
