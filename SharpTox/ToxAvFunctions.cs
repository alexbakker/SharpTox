#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;

namespace SharpTox
{
    public static class ToxAvFunctions
    {
		#region Functions
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

        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_answer(IntPtr toxav, ToxAvCallType call_type);
        public static ToxAvError Answer(IntPtr toxav, ToxAvCallType call_type)
        {
            return toxav_answer(toxav, call_type);
        }

        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_reject(IntPtr toxav, string reason);
        public static ToxAvError Reject(IntPtr toxav, string reason)
        {
            return toxav_reject(toxav, reason);
        }

        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_cancel(IntPtr toxav, int friend_number, string reason);
        public static ToxAvError Cancel(IntPtr toxav, int friend_number, string reason)
        {
            return toxav_cancel(toxav, friend_number, reason);
        }

        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_stop_call(IntPtr toxav);
        public static ToxAvError StopCall(IntPtr toxav)
        {
            return toxav_stop_call(toxav);
        }

        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_prepare_transmission(IntPtr toxav, int video_supported);
        public static ToxAvError PrepareTransmission(IntPtr toxav, bool video_supported)
        {
            return toxav_prepare_transmission(toxav, video_supported ? 1 : 0);
        }

        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_kill_transmission(IntPtr toxav);
        public static ToxAvError KillTransmission(IntPtr toxav)
        {
            return toxav_kill_transmission(toxav);
        }

        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_recv_video(IntPtr toxav, IntPtr output);
        public static ToxAvError ReceiveVideo(IntPtr toxav, ref IntPtr output)
        {
            return toxav_recv_video(toxav, output);
        }

        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_recv_audio(IntPtr toxav, int frame_size, ushort dest);
        public static ToxAvError ReceiveAudio(IntPtr toxav, int frame_size, ushort dest)
        {
            return toxav_recv_audio(toxav, frame_size, dest);
        }

        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_send_video(IntPtr toxav, IntPtr input);
        public static ToxAvError SendVideo(IntPtr toxav, IntPtr input)
        {
            return toxav_send_video(toxav, input);
        }

        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_send_audio(IntPtr toxav, ushort frame, int frame_size);
        public static ToxAvError SendAudio(IntPtr toxav, ushort frame, int frame_size)
        {
            return toxav_send_audio(toxav, frame, frame_size);
        }

        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvCallType toxav_get_peer_transmission_type(IntPtr toxav, int friend_number);
        public static ToxAvCallType GetPeerTransmissionType(IntPtr toxav, int friend_number)
        {
            return toxav_get_peer_transmission_type(toxav, friend_number);
        }

        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern int toxav_get_peer_id(IntPtr toxav, int friend_number);
        public static int GetPeerID(IntPtr toxav, int peer)
        {
            return toxav_get_peer_id(toxav, peer);
        }

        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern int toxav_capability_supported(IntPtr toxav, ToxAvCapabilities capability);
        public static bool CapabilitySupported(IntPtr toxav, ToxAvCapabilities capability)
        {
            return toxav_capability_supported(toxav, capability) == 1;
        }

        [DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr toxav_get_tox(IntPtr toxav);
        public static IntPtr GetTox(IntPtr toxav)
        {
            return toxav_get_tox(toxav);
        }
		#endregion

		#region Callbacks
		[DllImport("libtoxav", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr toxav_register_callstate_callback(ToxAvDelegates.CallstateCallback callback, ToxAvCallbackID id, IntPtr userdata);
		public static IntPtr RegisterCallstateCallback(ToxAvDelegates.CallstateCallback callback, ToxAvCallbackID id)
		{
			return toxav_register_callstate_callback(callback, id, IntPtr.Zero);
		}
		#endregion
    }
}

#pragma warning restore 1591