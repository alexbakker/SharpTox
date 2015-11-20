using System;
using System.Runtime.InteropServices;

namespace SharpTox.Core
{
    /// <summary>
    /// Represents settings to be used by an instance of tox.
    /// </summary>
    public sealed class ToxOptions
    {
        /// <summary>
        /// Default Tox Options.
        /// </summary>
        public static ToxOptions Default { get { return new ToxOptions(ToxOptionsStruct.Default); } }

        /// <summary>
        /// Whether or not IPv6 should be enabled.
        /// </summary>
        public bool Ipv6Enabled
        {
            get { return _options.Ipv6Enabled; }
            set { _options.Ipv6Enabled = value; }
        }

        /// <summary>
        /// Whether or not UDP should be enabled.
        /// </summary>
        public bool UdpEnabled
        {
            get { return _options.UdpEnabled; }
            set { _options.UdpEnabled = value; }
        }

        /// <summary>
        /// Proxy type.
        /// </summary>
        public ToxProxyType ProxyType
        {
            get { return _options.ProxyType; }
            set { _options.ProxyType = value; }
        }

        /// <summary>
        /// Proxy ip or domain.
        /// </summary>
        public string ProxyHost
        {
            get { return _options.ProxyHost; }
            set { _options.ProxyHost = value; }
        }

        /// <summary>
        /// Proxy port, in host byte order.
        /// Underlying type is ushort, don't exceed ushort.MaxValue.
        /// </summary>
        public int ProxyPort
        {
            get { return _options.ProxyPort; }
            set { _options.ProxyPort = (ushort)value; }
        }

        /// <summary>
        /// The start port of the inclusive port range to attempt to use.
        /// Underlying type is ushort, don't exceed ushort.MaxValue.
        /// </summary>
        public int StartPort
        {
            get { return _options.StartPort; }
            set { _options.StartPort = (ushort)value; }
        }

        /// <summary>
        /// The end port of the inclusive port range to attempt to use.
        /// Underlying type is ushort, don't exceed ushort.MaxValue.
        /// </summary>
        public int EndPort
        {
            get { return _options.EndPort; }
            set { _options.EndPort = (ushort)value; }
        }

        /// <summary>
        /// The port to use for a TCP server. This can be disabled by assigning 0.
        /// Underlying type is ushort, don't exceed ushort.MaxValue.
        /// </summary>
        public int TcpPort
        {
            get { return _options.TcpPort; }
            set { _options.TcpPort = (ushort)value; }
        }

        private ToxOptionsStruct _options;
        internal ToxOptionsStruct Struct { get { return _options; } }

        internal ToxOptions(ToxOptionsStruct options)
        {
            _options = options;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToxOptions"/> struct.
        /// </summary>
        /// <param name="ipv6Enabled">Whether or not IPv6 should be enabled.</param>
        /// <param name="udpEnabled">Whether or not UDP should be enabled.</param>
        public ToxOptions(bool ipv6Enabled, bool udpEnabled)
        {
            _options = new ToxOptionsStruct();
            _options.Ipv6Enabled = ipv6Enabled;
            _options.UdpEnabled = udpEnabled;
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

            _options = new ToxOptionsStruct();
            _options.Ipv6Enabled = ipv6Enabled;
            _options.UdpEnabled = false;
            _options.ProxyType = type;
            _options.ProxyHost = proxyAddress;
            _options.ProxyPort = (ushort)proxyPort;
        }

        public static bool operator ==(ToxOptions options1, ToxOptions options2)
        {
            return options1.Struct.Equals(options2.Struct);
        }

        public static bool operator !=(ToxOptions options1, ToxOptions options2)
        {
            return !(options1 == options2);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ToxOptionsStruct
    {
        internal static ToxOptionsStruct Default
        {
            get
            {
                ToxOptionsStruct options = new ToxOptionsStruct();
                ToxFunctions.OptionsDefault(ref options);
                return options;
            }
        }

        internal void SetData(byte[] data, ToxSaveDataType type)
        {
            if (type == ToxSaveDataType.SecretKey && data.Length != ToxConstants.SecretKeySize)
                throw new ArgumentException("Data must have a length of ToxConstants.SecretKeySize bytes", "data");

            SaveDataType = type;
            SaveDataLength = (uint)data.Length;
            SaveData = Marshal.AllocHGlobal(data.Length);

            Marshal.Copy(data, 0, SaveData, data.Length);
        }

        internal void Free()
        {
            if (SaveData != IntPtr.Zero)
                Marshal.FreeHGlobal(SaveData);
        }

        [MarshalAs(UnmanagedType.I1)]
        internal bool Ipv6Enabled;

        [MarshalAs(UnmanagedType.I1)]
        internal bool UdpEnabled;

        internal ToxProxyType ProxyType;

        [MarshalAs(UnmanagedType.LPStr)]
        internal string ProxyHost;

        internal ushort ProxyPort;
        internal ushort StartPort;
        internal ushort EndPort;
        internal ushort TcpPort;

        internal ToxSaveDataType SaveDataType;
        internal IntPtr SaveData;
        internal uint SaveDataLength;
    }
}
