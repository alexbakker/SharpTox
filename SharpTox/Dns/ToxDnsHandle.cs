using Microsoft.Win32.SafeHandles;

namespace SharpTox.Dns
{
    /// <summary>
    /// Represents a handle for an instance of tox_dns3.
    /// </summary>
    public class ToxDnsHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private ToxDnsHandle()
            : base(true) { }

        /// <summary>
        /// Executes tox_dns3_kill to free the toxdns handle.
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle()
        {
            ToxDnsFunctions.Kill(handle);
            return true;
        }
    }
}
