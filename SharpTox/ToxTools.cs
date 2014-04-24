using System;
using System.Text;

namespace SharpTox
{
    static class ToxTools
    {
        public static string HexBinToString(byte[] b)
        {
            StringBuilder sb = new StringBuilder(2 * b.Length);

            for (int i = 0; i < b.Length; i++)
                sb.AppendFormat("{0:X2}", b[i]);

            return sb.ToString();
        }

        public static byte[] StringToHexBin(string s)
        {
            byte[] bin = new byte[s.Length / 2];

            for (int i = 0; i < bin.Length; i++)
                bin[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);

            return bin;
        }

        //getting rid of string null terminations
        public static string RemoveNull(string s)
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
