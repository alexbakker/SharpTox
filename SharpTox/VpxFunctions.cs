using System;
using System.Runtime.InteropServices;

namespace SharpTox
{
    public static class VpxFunctions
    {
        [DllImport("cygvpx-1.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern IntPtr vpx_img_alloc(vpx_image* image, VpxImageFormat format, uint d_w, uint d_h, uint align);
        public unsafe static IntPtr ImageAlloc(vpx_image* image, VpxImageFormat format, uint d_w, uint d_h, uint align)
        {
            return vpx_img_alloc(image, format, d_w, d_h, align);
        }

        [DllImport("cygvpx-1.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern IntPtr vpx_img_free(vpx_image* image);
        public unsafe static IntPtr ImageFree(vpx_image* image)
        {
            return vpx_img_free(image);
        }
    }
}
