#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;

namespace SharpTox.Vpx
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public unsafe struct vpx_image
    {
        public VpxImageFormat fmt;

        public uint w;
        public uint h;
        public uint d_w;
        public uint d_h;

        public uint x_chroma_shift;
        public uint y_chroma_shift;

        public fixed int planes[4];
        public fixed int stride[4];

        public int bps;

        public IntPtr user_priv;
        public IntPtr img_data;

        public int img_data_owner;
        public int self_allocd;
    }
}

#pragma warning restore 1591