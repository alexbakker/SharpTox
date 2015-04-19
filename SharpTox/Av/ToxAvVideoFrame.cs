using System;
using System.Runtime.InteropServices;

namespace SharpTox.Av
{
    public class ToxAvVideoFrame
    {
        public byte[] Y { get; private set; }
        public byte[] U { get; private set; }
        public byte[] V { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        //this relies on the caller calling vpx_img_free (which is currently the case in toxav)
        //otherwise we'll be leaking memory like crazy
        internal ToxAvVideoFrame(ushort width, ushort height, IntPtr y, IntPtr u, IntPtr v)
        {
            Width = width;
            Height = height;

            //TODO (?): use unsafe code to access the data instead of copying it over
            Y = new byte[width * height];
            U = new byte[(width / 2) * (height / 2)];
            V = new byte[(width / 2) * (height / 2)];

            Marshal.Copy(y, Y, 0, Y.Length);
            Marshal.Copy(u, U, 0, U.Length);
            Marshal.Copy(v, V, 0, V.Length);
        }

        public ToxAvVideoFrame(int width, int height, byte[] y, byte[] u, byte[] v)
        {
            Width = width;
            Height = height;

            Y = y;
            U = u;
            V = v;
        }
    }
}
