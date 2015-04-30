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
        /// Default Tox Options.
        /// </summary>
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
        public bool Ipv6Enabled;

        /// <summary>
        /// Whether or not UDP should be enabled.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool UdpEnabled;

        /// <summary>
        /// Proxy type.
        /// </summary>
        public ToxProxyType ProxyType;

        /// <summary>
        /// Proxy ip or domain.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string ProxyHost;

        /// <summary>
        /// Proxy port, in host byte order.
        /// </summary>
        [CLSCompliant(false)]
        public ushort ProxyPort;

        /// <summary>
        /// The start port of the inclusive port range to attempt to use.
        /// </summary>
        [CLSCompliant(false)]
        public ushort StartPort;

        /// <summary>
        /// The end port of the inclusive port range to attempt to use.
        /// </summary>
        [CLSCompliant(false)]
        public ushort EndPort;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToxOptions"/> struct.
        /// </summary>
        /// <param name="ipv6Enabled">Whether or not IPv6 should be enabled.</param>
        /// <param name="udpEnabled">Whether or not UDP should be enabled.</param>
        public ToxOptions(bool ipv6Enabled, bool udpEnabled)
        {
            Ipv6Enabled = ipv6Enabled;
            UdpEnabled = udpEnabled;
            ProxyType = ToxProxyType.None;
            ProxyHost = null;
            ProxyPort = 0;
            StartPort = 0;
            EndPort = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToxOptions"/> struct.
        /// </summary>
        /// <param name="ipv6Enabled">Whether or not IPv6 should be enabled.</param>
        /// <param name="type">The type of proxy we want to connect to.</param>
        /// <param name="proxyAddress">The IP address or DNS name of the proxy to be used.</param>
        /// <param name="proxyPort">The port to use to connect to the proxy.</param>
        public ToxOptions(bool ipv6Enabled, ToxProxyType type, string proxyAddress, int proxyPort)
        {
            if (string.IsNullOrEmpty(proxyAddress))
                throw new ArgumentNullException("proxyAddress");

            if (proxyAddress.Length > 255)
                throw new Exception("Parameter proxyAddress is too long.");

            Ipv6Enabled = ipv6Enabled;
            UdpEnabled = false;
            ProxyType = type;
            ProxyHost = proxyAddress;
            ProxyPort = (ushort)proxyPort;
            StartPort = 0;
            EndPort = 0;
        }

        public static bool operator ==(ToxOptions options1, ToxOptions options2)
        {
            return options1.Equals(options2);
        }

        public static bool operator !=(ToxOptions options1, ToxOptions options2)
        {
            return !(options1 == options2);
        }
    }
}
