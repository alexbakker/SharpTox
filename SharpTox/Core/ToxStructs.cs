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
        public static ToxOptions Default
        {
            get
            {
                ToxOptions options = new ToxOptions();
                ToxFunctions.OptionsDefault(ref options);

                return options;
            }
        }

        /// <summary>
        /// Whether or not IPv6 should be enabled.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool Ipv6Enabled;

        /// <summary>
        /// Whether or not UDP should be disabled.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool UdpEnabled;

        /// <summary>
        /// Proxy type.
        /// </summary>
        public readonly ToxProxyType ProxyType;

        /// <summary>
        /// Proxy ip or domain.
        /// </summary>
        public string ProxyAddress;

        /// <summary>
        /// Proxy port, in host byte order.
        /// </summary>
        public ushort ProxyPort;

        /// <summary>
        /// The start port of the inclusive port range to attempt to use.
        /// </summary>
        public ushort StartPort;

        /// <summary>
        /// The end port of the inclusive port range to attempt to use.
        /// </summary>
        public ushort EndPort;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToxOptions"/> struct.
        /// </summary>
        /// <param name="ipv6Enabled"></param>
        /// <param name="udpEnabled"></param>
        /// <param name="startPort"></param>
        /// <param name="endPort"></param>
        public ToxOptions(bool ipv6Enabled, bool udpEnabled)
        {
            Ipv6Enabled = ipv6Enabled;
            UdpEnabled = udpEnabled;
            ProxyType = ToxProxyType.None;
            ProxyAddress = null;
            ProxyPort = 0;
            StartPort = 0;
            EndPort = 0;
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
            UdpEnabled = false;
            ProxyType = type;

            if (proxyAddress.Length > 255)
                throw new Exception("Parameter proxyAddress is too long.");

            ProxyAddress = proxyAddress + Char.MinValue; //is this needed?
            ProxyPort = (ushort)proxyPort;
            StartPort = 0;
            EndPort = 0;
        }
    }
}
