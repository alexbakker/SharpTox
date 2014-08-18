using System;
using System.Runtime.InteropServices;

namespace SharpTox.Core
{
    /// <summary>
    /// Represents settings to be used by an instance of tox.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ToxOptions
    {
        /// <summary>
        /// Whether or not IPv6 should be enabled.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool Ipv6Enabled;

        /// <summary>
        /// Whether or not UDP should be disabled.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool UdpDisabled;

        /// <summary>
        /// Whether or not proxy support should be enabled.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool ProxyEnabled;

        /// <summary>
        /// Proxy ip or domain.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public readonly char[] ProxyAddress;

        /// <summary>
        /// Proxy port, in host byte order.
        /// </summary>
        public readonly ushort ProxyPort;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToxOptions"/> struct.
        /// </summary>
        /// <param name="ipv6_enabled"></param>
        /// <param name="udp_disabled"></param>
        public ToxOptions(bool ipv6_enabled, bool udp_disabled)
        {
            Ipv6Enabled = ipv6_enabled;
            UdpDisabled = udp_disabled;
            ProxyEnabled = false;
            ProxyAddress = null;
            ProxyPort = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToxOptions"/> struct.
        /// </summary>
        /// <param name="ipv6_enabled"></param>
        /// <param name="proxy_address"></param>
        /// <param name="proxy_port"></param>
        public ToxOptions(bool ipv6_enabled, string proxy_address, int proxy_port)
        {
            Ipv6Enabled = ipv6_enabled;
            UdpDisabled = true;
            ProxyEnabled = true;

            char[] dest = new char[256];
            char[] source_array = proxy_address.ToCharArray();

            if (source_array.Length > dest.Length)
                throw new Exception("Argument proxy_address is longer than 256 chars");

            Array.Copy(source_array, 0, dest, 0, source_array.Length);

            ProxyAddress = dest;
            ProxyPort = (ushort)proxy_port;
        }
    }
}
