#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;

namespace SharpTox.Av
{
    public static class ToxAvFunctions
    {
        const string dll = "libtox";

        #region Functions
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void toxav_kill(IntPtr toxav);
        public static void Kill(IntPtr toxav)
        {
            toxav_kill(toxav);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr toxav_new(IntPtr tox, int max_calls);
        public static IntPtr New(IntPtr tox, int max_calls)
        {
            return toxav_new(tox, max_calls);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int toxav_prepare_audio_frame(IntPtr toxav, int call_index, byte[] dest, int dest_max, ushort[] frame, int frame_size);
        public static int PrepareAudioFrame(IntPtr tox, int call_index, byte[] dest, int dest_max, ushort[] frame, int frame_size)
        {
            return toxav_prepare_audio_frame(tox, call_index, dest, dest_max, frame, frame_size);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int toxav_prepare_video_frame(IntPtr toxav, int call_index, byte[] dest, int dest_max, IntPtr image);
        public unsafe static int PrepareVideoFrame(IntPtr tox, int call_index, byte[] dest, int dest_max, IntPtr image)
        {
            return toxav_prepare_video_frame(tox, call_index, dest, dest_max, image);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_call(IntPtr toxav, ref int call_index, int friend_number, ref ToxAvCodecSettings settings, int ringing_seconds);
        public static ToxAvError Call(IntPtr toxav, int friend_number, ToxAvCodecSettings settings, int ringing_seconds, out int call_index)
        {
            int index = new int();
            ToxAvError result = toxav_call(toxav, ref index, friend_number, ref settings, ringing_seconds);

            call_index = index;
            return result;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_hangup(IntPtr toxav, int call_index);
        public static ToxAvError Hangup(IntPtr toxav, int call_index)
        {
            return toxav_hangup(toxav, call_index);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_answer(IntPtr toxav, int call_index, ref ToxAvCodecSettings settings);
        public static ToxAvError Answer(IntPtr toxav, int call_index, ToxAvCodecSettings settings)
        {
            return toxav_answer(toxav, call_index, ref settings);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_reject(IntPtr toxav, int call_index, string reason);
        public static ToxAvError Reject(IntPtr toxav, int call_index, string reason)
        {
            return toxav_reject(toxav, call_index, reason);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_cancel(IntPtr toxav, int call_index, int peer_id, string reason);
        public static ToxAvError Cancel(IntPtr toxav, int call_index, int peer_id, string reason)
        {
            return toxav_cancel(toxav, call_index, peer_id, reason);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_change_settings(IntPtr toxav, int call_index, ref ToxAvCodecSettings settings);
        public static ToxAvError ChangeSettings(IntPtr toxav, int call_index, int peer_id, ToxAvCodecSettings settings)
        {
            return toxav_change_settings(toxav, call_index, ref settings);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_stop_call(IntPtr toxav, int call_index);
        public static ToxAvError StopCall(IntPtr toxav, int call_index)
        {
            return toxav_stop_call(toxav, call_index);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_prepare_transmission(IntPtr toxav, int call_index, uint jbuf_size, uint VAD_treshold, int video_supported);
        public static ToxAvError PrepareTransmission(IntPtr toxav, int call_index, uint jbuf_size, uint VAD_treshold, bool video_supported)
        {
            return toxav_prepare_transmission(toxav, call_index, jbuf_size, VAD_treshold, video_supported ? 1 : 0);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_kill_transmission(IntPtr toxav, int call_index);
        public static ToxAvError KillTransmission(IntPtr toxav, int call_index)
        {
            return toxav_kill_transmission(toxav, call_index);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_send_video(IntPtr toxav, int call_index, byte[] frame, uint frame_size);
        public static ToxAvError SendVideo(IntPtr toxav, int call_index, byte[] frame, uint frame_size)
        {
            return toxav_send_video(toxav, call_index, frame, frame_size);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvError toxav_send_audio(IntPtr toxav, int call_index, byte[] frame, uint frame_size);
        public static ToxAvError SendAudio(IntPtr toxav, int call_index, ref byte[] frame, uint frame_size)
        {
            return toxav_send_audio(toxav, call_index, frame, frame_size);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int toxav_get_peer_id(IntPtr toxav, int call_index, int peer);
        public static int GetPeerID(IntPtr toxav, int call_index, int peer)
        {
            return toxav_get_peer_id(toxav, call_index, peer);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int toxav_capability_supported(IntPtr toxav, int call_index, ToxAvCapabilities capability);
        public static bool CapabilitySupported(IntPtr toxav, int call_index, ToxAvCapabilities capability)
        {
            return toxav_capability_supported(toxav, call_index, capability) == 1;
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr toxav_get_tox(IntPtr toxav);
        public static IntPtr GetTox(IntPtr toxav)
        {
            return toxav_get_tox(toxav);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int toxav_has_activity(IntPtr toxav, int call_index, short[] pcm, ushort frame_size, float ref_energy);
        public static int HasActivity(IntPtr toxav, int call_index, short[] pcm, ushort frame_size, float ref_energy)
        {
            return toxav_has_activity(toxav, call_index, pcm, frame_size, ref_energy);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvCallState toxav_get_call_state(IntPtr toxav, int call_index);
        public static ToxAvCallState GetCallState(IntPtr toxav, int call_index)
        {
            return toxav_get_call_state(toxav, call_index);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern ToxAvCallState toxav_get_peer_csettings(IntPtr toxav, int call_index, int peer, ref ToxAvCodecSettings settings);
        public static ToxAvCodecSettings GetPeerCodecSettings(IntPtr toxav, int call_index, int peer)
        {
            ToxAvCodecSettings settings = new ToxAvCodecSettings();
            toxav_get_peer_csettings(toxav, call_index, peer, ref settings);

            return settings;
        }

        #endregion

        #region Callbacks
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr toxav_register_callstate_callback(IntPtr toxav, ToxAvDelegates.CallstateCallback callback, ToxAvCallbackID id, IntPtr userdata);
        public static IntPtr RegisterCallstateCallback(IntPtr toxav, ToxAvDelegates.CallstateCallback callback, ToxAvCallbackID id)
        {
            return toxav_register_callstate_callback(toxav, callback, id, IntPtr.Zero);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr toxav_register_audio_recv_callback(IntPtr toxav, ToxAvDelegates.AudioReceiveCallback callback, IntPtr userdata);
        public static IntPtr RegisterAudioReceiveCallback(IntPtr toxav, ToxAvDelegates.AudioReceiveCallback callback, IntPtr userdata)
        {
            return toxav_register_audio_recv_callback(toxav, callback, userdata);
        }

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr toxav_register_video_recv_callback(IntPtr toxav, ToxAvDelegates.VideoReceiveCallback callback, IntPtr userdata);
        public static IntPtr RegisterVideoReceiveCallback(IntPtr toxav, ToxAvDelegates.VideoReceiveCallback callback, IntPtr userdata)
        {
            return toxav_register_video_recv_callback(toxav, callback, userdata);
        }

        #endregion
    }
}

#pragma warning restore 1591