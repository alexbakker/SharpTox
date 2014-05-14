#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;

namespace SharpTox
{
    public static class ToxAvFunctions
    {
        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern void toxav_kill(IntPtr toxav);
        public static void Kill(IntPtr toxav)
        {
            toxav_kill(toxav);
        }

        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr toxav_new(IntPtr tox, ToxAvCodecSettings codec_settings);
        public static IntPtr New(IntPtr tox, ToxAvCodecSettings codec_settings)
        {
            return toxav_new(tox, codec_settings);
        }

        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_call(IntPtr toxav, int friend_number, ToxAvCallType call_type, int ringing_seconds);
        public static ToxAvError Call(IntPtr toxav, int friend_number, ToxAvCallType call_type, int ringing_seconds)
        {
            return toxav_call(toxav, friend_number, call_type, ringing_seconds);
        }

        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_hangup(IntPtr toxav);
        public static ToxAvError Hangup(IntPtr toxav)
        {
            return toxav_hangup(toxav);
        }
    }
}

#pragma warning restore 1591