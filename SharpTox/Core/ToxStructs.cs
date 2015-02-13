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
        /// Proxy type.
        /// </summary>
        public readonly ToxProxyType ProxyType;

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
        /// <param name="ipv6Enabled"></param>
        /// <param name="udpDisabled"></param>
        public ToxOptions(bool ipv6Enabled, bool udpDisabled)
        {
            Ipv6Enabled = ipv6Enabled;
            UdpDisabled = udpDisabled;
            ProxyType = ToxProxyType.None;
            ProxyAddress = null;
            ProxyPort = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToxOptions"/> struct.
        /// </summary>
        /// <param name="ipv6Enabled"></param>
        /// <param name="type"></param>
        /// <param name="proxyAddress"></param>
        /// <param name="proxyPort"></param>
        public ToxOptions(bool ipv6Enabled, ToxProxyType type, string proxyAddress, int proxyPort)
        {
            Ipv6Enabled = ipv6Enabled;
            UdpDisabled = true;
            ProxyType = type;

            char[] dest = new char[256];
            char[] sourceArray = proxyAddress.ToCharArray();

            if (sourceArray.Length > dest.Length)
                throw new Exception("Argument proxy_address is longer than 256 chars");

            Array.Copy(sourceArray, 0, dest, 0, sourceArray.Length);

            ProxyAddress = dest;
            ProxyPort = (ushort)proxyPort;
        }
    }
}
