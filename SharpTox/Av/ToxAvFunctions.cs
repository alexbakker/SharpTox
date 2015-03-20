#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;
using SharpTox.Core;

namespace SharpTox.Av
{
    internal static class ToxAvFunctions
    {
#if POSIX
		const string dll = "libtoxav.so";
#else 
		const string dll = "libtox";
#endif

        #region Functions
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_kill")]
        internal static extern void Kill(IntPtr toxAv);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_new")]
        internal static extern ToxAvHandle New(ToxHandle tox, int maxCalls);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_prepare_audio_frame")]
        internal static extern int PrepareAudioFrame(ToxAvHandle toxAv, int callIndex, byte[] dest, int destMax, short[] frame, int frameSize);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_prepare_video_frame")]
        internal static extern int PrepareVideoFrame(ToxAvHandle toxAv, int callIndex, byte[] dest, int destMax, IntPtr image);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_call")]
        internal static extern ToxAvError Call(ToxAvHandle toxAv, ref int callIndex, int friend_number, ref ToxAvCodecSettings settings, int ringingSeconds);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_hangup")]
        internal static extern ToxAvError Hangup(ToxAvHandle toxAv, int callIndex);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_answer")]
        internal static extern ToxAvError Answer(ToxAvHandle toxAv, int callIndex, ref ToxAvCodecSettings settings);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_reject")]
        internal static extern ToxAvError Reject(ToxAvHandle toxAv, int callIndex, string reason);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_cancel")]
        internal static extern ToxAvError Cancel(ToxAvHandle toxAv, int callIndex, int peerId, string reason);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_change_settings")]
        internal static extern ToxAvError ChangeSettings(ToxAvHandle toxAv, int callIndex, ref ToxAvCodecSettings settings);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_stop_call")]
        internal static extern ToxAvError StopCall(ToxAvHandle toxAv, int callIndex);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_prepare_transmission")]
        internal static extern ToxAvError PrepareTransmission(ToxAvHandle toxAv, int callIndex, int videoSupported);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_kill_transmission")]
        internal static extern ToxAvError KillTransmission(ToxAvHandle toxAv, int callIndex);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_send_video")]
        internal static extern ToxAvError SendVideo(ToxAvHandle toxAv, int callIndex, byte[] frame, uint frameSize);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_send_audio")]
        internal static extern ToxAvError SendAudio(ToxAvHandle toxAv, int callIndex, byte[] frame, uint frame_size);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_get_peer_id")]
        internal static extern int GetPeerID(ToxAvHandle toxAv, int callIndex, int peer);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_capability_supported")]
        internal static extern int CapabilitySupported(ToxAvHandle toxAv, int callIndex, ToxAvCapabilities capability);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_get_tox")]
        internal static extern IntPtr GetTox(ToxAvHandle toxAv);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_get_call_state")]
        internal static extern ToxAvCallState GetCallState(ToxAvHandle toxAv, int callIndex);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_get_peer_csettings")]
        internal static extern int GetPeerCodecSettings(ToxAvHandle toxAv, int callIndex, int peer, ref ToxAvCodecSettings settings);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_add_av_groupchat")]
        internal static extern int AddAvGroupchat(ToxHandle tox, ToxAvDelegates.GroupAudioReceiveCallback callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_join_av_groupchat")]
        internal static extern int JoinAvGroupchat(ToxHandle tox, int friendNumber, byte[] data, ushort length, ToxAvDelegates.GroupAudioReceiveCallback callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_group_send_audio")]
        internal static extern int GroupSendAudio(ToxHandle tox, int groupNumber, short[] pcm, uint sampleCount, byte channels, uint sampleRate);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_do_interval")]
        internal static extern uint DoInterval(ToxAvHandle toxAv);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_do")]
        internal static extern void Do(ToxAvHandle toxAv);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_get_active_count")]
        internal static extern int GetActiveCount(ToxAvHandle toxav);

        #endregion

        #region Callbacks
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_register_callstate_callback")]
        internal static extern void RegisterCallstateCallback(ToxAvHandle toxAv, ToxAvDelegates.CallstateCallback callback, ToxAvCallbackID id, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_register_audio_callback")]
        internal static extern void RegisterAudioReceiveCallback(ToxAvHandle toxAv, ToxAvDelegates.AudioReceiveCallback callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_register_video_callback")]
        internal static extern void RegisterVideoReceiveCallback(ToxAvHandle toxAv, ToxAvDelegates.VideoReceiveCallback callback, IntPtr userData);

        #endregion
    }
}

#pragma warning restore 1591