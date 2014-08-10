#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;
using SharpTox.Core;

namespace SharpTox.Av
{
    public static class ToxAvFunctions
    {
        const string dll = "libtox";

        #region Functions
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_kill")]
        public static extern void Kill(IntPtr toxav);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_new")]
        public static extern ToxAvHandle New(ToxHandle tox, int max_calls);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_prepare_audio_frame")]
        public static extern int PrepareAudioFrame(ToxAvHandle toxav, int call_index, byte[] dest, int dest_max, ushort[] frame, int frame_size);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_prepare_video_frame")]
        public static extern int PrepareVideoFrame(ToxAvHandle toxav, int call_index, byte[] dest, int dest_max, IntPtr image);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_call")]
        public static extern ToxAvError Call(ToxAvHandle toxav, ref int call_index, int friend_number, ref ToxAvCodecSettings settings, int ringing_seconds);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_hangup")]
        public static extern ToxAvError Hangup(ToxAvHandle toxav, int call_index);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_answer")]
        public static extern ToxAvError Answer(ToxAvHandle toxav, int call_index, ref ToxAvCodecSettings settings);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_reject")]
        public static extern ToxAvError Reject(ToxAvHandle toxav, int call_index, string reason);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_cancel")]
        public static extern ToxAvError Cancel(ToxAvHandle toxav, int call_index, int peer_id, string reason);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_change_settings")]
        public static extern ToxAvError ChangeSettings(ToxAvHandle toxav, int call_index, ref ToxAvCodecSettings settings);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_stop_call")]
        public static extern ToxAvError StopCall(ToxAvHandle toxav, int call_index);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_prepare_transmission")]
        public static extern ToxAvError PrepareTransmission(ToxAvHandle toxav, int call_index, uint jbuf_size, uint VAD_treshold, int video_supported);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_kill_transmission")]
        public static extern ToxAvError KillTransmission(ToxAvHandle toxav, int call_index);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_send_video")]
        public static extern ToxAvError SendVideo(ToxAvHandle toxav, int call_index, byte[] frame, uint frame_size);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_send_audio")]
        public static extern ToxAvError SendAudio(ToxAvHandle toxav, int call_index, byte[] frame, uint frame_size);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_get_peer_id")]
        public static extern int GetPeerID(ToxAvHandle toxav, int call_index, int peer);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_capability_supported")]
        public static extern int CapabilitySupported(ToxAvHandle toxav, int call_index, ToxAvCapabilities capability);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_get_tox")]
        public static extern IntPtr GetTox(ToxAvHandle toxav);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_has_activity")]
        public static extern int HasActivity(ToxAvHandle toxav, int call_index, short[] pcm, ushort frame_size, float ref_energy);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_get_call_state")]
        public static extern ToxAvCallState GetCallState(ToxAvHandle toxav, int call_index);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_get_peer_csettings")]
        public static extern int GetPeerCodecSettings(ToxAvHandle toxav, int call_index, int peer, ref ToxAvCodecSettings settings);

        #endregion

        #region Callbacks
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_register_callstate_callback")]
        public static extern IntPtr RegisterCallstateCallback(ToxAvHandle toxav, ToxAvDelegates.CallstateCallback callback, ToxAvCallbackID id, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_register_audio_recv_callback")]
        public static extern IntPtr RegisterAudioReceiveCallback(ToxAvHandle toxav, ToxAvDelegates.AudioReceiveCallback callback, IntPtr userdata);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_register_video_recv_callback")]
        public static extern IntPtr RegisterVideoReceiveCallback(ToxAvHandle toxav, ToxAvDelegates.VideoReceiveCallback callback, IntPtr userdata);

        #endregion
    }
}

#pragma warning restore 1591