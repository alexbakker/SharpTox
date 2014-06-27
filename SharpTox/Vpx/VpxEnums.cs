using System;

namespace SharpTox.Vpx
{
    public enum VpxImageFormat
    {
        NONE,
        RGB24,
        RGB32,
        RGB565,
        RGB555,
        UYVY,
        YUY2,
        YVYU,
        BGR24,
        RGB32_LE,
        ARGB,
        ARGB_LE,
        RGB565_LE,
        RGB555_LE,
        YV12 = VpxConstants.VPX_IMG_FMT_PLANAR | VpxConstants.VPX_IMG_FMT_UV_FLIP | 1,
        I420 = VpxConstants.VPX_IMG_FMT_PLANAR | 2,
        VPXYV12 = VpxConstants.VPX_IMG_FMT_PLANAR | VpxConstants.VPX_IMG_FMT_UV_FLIP | 3,
        VPXI420 = VpxConstants.VPX_IMG_FMT_PLANAR | 4
    }
}
