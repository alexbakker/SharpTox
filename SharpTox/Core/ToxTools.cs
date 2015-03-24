using System;
using System.Text;

namespace SharpTox.Core
{
    public static class ToxTools
    {
        internal static string HexBinToString(byte[] b)
        {
            StringBuilder sb = new StringBuilder(2 * b.Length);

            for (int i = 0; i < b.Length; i++)
                sb.AppendFormat("{0:X2}", b[i]);

            return sb.ToString();
        }

        internal static byte[] StringToHexBin(string s)
        {
            byte[] bin = new byte[s.Length / 2];

            for (int i = 0; i < bin.Length; i++)
                bin[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);

            return bin;
        }

        internal static DateTime EpochToDateTime(long epoch)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble(epoch));
        }

        public static byte[] Hash(byte[] data)
        {
            byte[] hash = new byte[ToxConstants.HashLength];
            ToxFunctions.Hash(hash, data, (uint)data.Length);
            return hash;
        }

        internal static string RemoveNull(string s)
        {
            if (s.Length != 0)
            {
                int index = s.IndexOf(Char.MinValue);
                if (!(index >= 0))
                    return s;
                else
                    return s.Substring(0, index);
            }

            return s;
        }
    }
}
