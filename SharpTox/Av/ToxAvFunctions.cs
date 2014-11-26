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
        public static extern void Kill(IntPtr toxAv);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_new")]
        public static extern ToxAvHandle New(ToxHandle tox, int maxCalls);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_prepare_audio_frame")]
        public static extern int PrepareAudioFrame(ToxAvHandle toxAv, int callIndex, byte[] dest, int destMax, ushort[] frame, int frameSize);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_prepare_video_frame")]
        public static extern int PrepareVideoFrame(ToxAvHandle toxAv, int callIndex, byte[] dest, int destMax, IntPtr image);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_call")]
        public static extern ToxAvError Call(ToxAvHandle toxAv, ref int callIndex, int friend_number, ref ToxAvCodecSettings settings, int ringingSeconds);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_hangup")]
        public static extern ToxAvError Hangup(ToxAvHandle toxAv, int callIndex);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_answer")]
        public static extern ToxAvError Answer(ToxAvHandle toxAv, int callIndex, ref ToxAvCodecSettings settings);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_reject")]
        public static extern ToxAvError Reject(ToxAvHandle toxAv, int callIndex, string reason);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_cancel")]
        public static extern ToxAvError Cancel(ToxAvHandle toxAv, int callIndex, int peerId, string reason);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_change_settings")]
        public static extern ToxAvError ChangeSettings(ToxAvHandle toxAv, int callIndex, ref ToxAvCodecSettings settings);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_stop_call")]
        public static extern ToxAvError StopCall(ToxAvHandle toxAv, int callIndex);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_prepare_transmission")]
        public static extern ToxAvError PrepareTransmission(ToxAvHandle toxAv, int callIndex, uint jitterBufferSize, uint vadTreshold, int videoSupported);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_kill_transmission")]
        public static extern ToxAvError KillTransmission(ToxAvHandle toxAv, int callIndex);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_send_video")]
        public static extern ToxAvError SendVideo(ToxAvHandle toxAv, int callIndex, byte[] frame, uint frameSize);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_send_audio")]
        public static extern ToxAvError SendAudio(ToxAvHandle toxAv, int callIndex, byte[] frame, uint frame_size);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_get_peer_id")]
        public static extern int GetPeerID(ToxAvHandle toxAv, int callIndex, int peer);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_capability_supported")]
        public static extern int CapabilitySupported(ToxAvHandle toxAv, int callIndex, ToxAvCapabilities capability);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_get_tox")]
        public static extern IntPtr GetTox(ToxAvHandle toxAv);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_has_activity")]
        public static extern int HasActivity(ToxAvHandle toxAv, int callIndex, short[] pcm, ushort frameSize, float refEnergy);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_get_call_state")]
        public static extern ToxAvCallState GetCallState(ToxAvHandle toxAv, int callIndex);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_get_peer_csettings")]
        public static extern int GetPeerCodecSettings(ToxAvHandle toxAv, int callIndex, int peer, ref ToxAvCodecSettings settings);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_add_av_groupchat")]
        public static extern int AddAvGroupchat(ToxHandle tox, ToxAvDelegates.GroupAudioReceiveCallback callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_join_av_groupchat")]
        public static extern int JoinAvGroupchat(ToxHandle tox, int friendNumber, byte[] data, ushort length, ToxAvDelegates.GroupAudioReceiveCallback callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_group_send_audio")]
        public static extern int GroupSendAudio(ToxHandle tox, int groupNumber, short[] pcm, uint sampleCount, byte channels, uint sampleRate);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_do_interval")]
        public static extern uint DoInterval(ToxAvHandle toxav);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_do")]
        public static extern void Do(ToxAvHandle toxav);

        #endregion

        #region Callbacks
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_register_callstate_callback")]
        public static extern void RegisterCallstateCallback(ToxAvHandle toxAv, ToxAvDelegates.CallstateCallback callback, ToxAvCallbackID id, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_register_audio_recv_callback")]
        public static extern void RegisterAudioReceiveCallback(ToxAvHandle toxAv, ToxAvDelegates.AudioReceiveCallback callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_register_video_recv_callback")]
        public static extern void RegisterVideoReceiveCallback(ToxAvHandle toxAv, ToxAvDelegates.VideoReceiveCallback callback, IntPtr userData);

        #endregion
    }
}

#pragma warning restore 1591