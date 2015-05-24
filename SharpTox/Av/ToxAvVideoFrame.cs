using System;
using System.Runtime.InteropServices;

namespace SharpTox.Av
{
    public class ToxAvVideoFrame
    {
        public byte[] Y { get; private set; }
        public byte[] U { get; private set; }
        public byte[] V { get; private set; }
        public byte[] A { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        //this relies on the caller calling vpx_img_free (which is currently the case in toxav)
        //if this changes in the future, call Marshal.FreeHGlobal to free things manually
        internal ToxAvVideoFrame(ushort width, ushort height, IntPtr y, IntPtr u, IntPtr v, IntPtr a, int yStride, int uStride, int vStride, int aStride)
        {
            Width = width;
            Height = height;

            Y = new byte[Math.Max(width, Math.Abs(yStride)) * height];
            U = new byte[Math.Max(width / 2, Math.Abs(uStride)) * (height / 2)];
            V = new byte[Math.Max(width / 2, Math.Abs(vStride)) * (height / 2)];
            A = new byte[Math.Max(width, Math.Abs(aStride)) * height];

            //TODO (?): use unsafe code to access the data directly instead of copying it over
            Marshal.Copy(y, Y, 0, Y.Length);
            Marshal.Copy(u, U, 0, U.Length);
            Marshal.Copy(v, V, 0, V.Length);
            Marshal.Copy(a, A, 0, A.Length);
        }

        public ToxAvVideoFrame(int width, int height, byte[] y, byte[] u, byte[] v, byte[] a)
        {
            Width = width;
            Height = height;

            Y = y;
            U = u;
            V = v;
            A = a;
        }
    }
}
