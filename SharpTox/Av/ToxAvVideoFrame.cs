using System;
using System.Runtime.InteropServices;

namespace SharpTox.Av
{
    public class ToxAvVideoFrame
    {
        public byte[] Y { get; private set; }
        public byte[] U { get; private set; }
        public byte[] V { get; private set; }

        public int YStride { get; private set; }
        public int UStride { get; private set; }
        public int VStride { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        //this relies on the caller to call vpx_img_free (which is currently the case in toxav)
        internal ToxAvVideoFrame(ushort width, ushort height, IntPtr y, IntPtr u, IntPtr v, int yStride, int uStride, int vStride)
        {
            Width = width;
            Height = height;

            YStride = yStride;
            UStride = uStride;
            VStride = vStride;

            Y = new byte[Math.Max(width, Math.Abs(yStride)) * height];
            U = new byte[Math.Max(width / 2, Math.Abs(uStride)) * (height / 2)];
            V = new byte[Math.Max(width / 2, Math.Abs(vStride)) * (height / 2)];

            //TODO (?): use unsafe code to access the data directly instead of copying it over
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

            YStride = width;
            UStride = width / 2;
            VStride = width / 2;
        }
    }
}
