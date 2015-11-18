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

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_version_major")]
        internal static extern uint VersionMajor();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_version_minor")]
        internal static extern uint VersionMinor();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_version_patch")]
        internal static extern uint VersionPatch();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_version_is_compatible")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool VersionIsCompatible(uint major, uint minor, uint patch);

        #region Functions
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_new")]
        internal static extern ToxAvHandle New(ToxHandle tox, ref ToxAvErrorNew error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_kill")]
        internal static extern void Kill(IntPtr toxAv);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_get_tox")]
        internal static extern IntPtr GetTox(ToxAvHandle toxAv);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_iteration_interval")]
        internal static extern uint IterationInterval(ToxAvHandle toxAv);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_iterate")]
        internal static extern void Iterate(ToxAvHandle toxAv);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_call")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool Call(ToxAvHandle toxAv, uint friendNumber, uint audioBitrate, uint videoBitrate, ref ToxAvErrorCall error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_answer")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool Answer(ToxAvHandle toxAv, uint friendNumber, uint audioBitrate, uint videoBitrate, ref ToxAvErrorAnswer error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_call_control")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool CallControl(ToxAvHandle toxAv, uint friendNumber, ToxAvCallControl control, ref ToxAvErrorCallControl error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_bit_rate_set")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool BitrateSet(ToxAvHandle toxAv, uint friendNumber, int audioBitrate, int videoBitrate, ref ToxAvErrorSetBitrate error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_video_send_frame")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool VideoSendFrame(ToxAvHandle toxAv, uint friendNumber, ushort width, ushort height, byte[] y, byte[] u, byte[] v, ref ToxAvErrorSendFrame error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_audio_send_frame")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool AudioSendFrame(ToxAvHandle toxAv, uint friendNumber, short[] pcm, uint sampleCount, byte channels, uint samplingRate, ref ToxAvErrorSendFrame error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_add_av_groupchat")]
        internal static extern int AddAvGroupchat(ToxHandle tox, ToxAvDelegates.GroupAudioReceiveCallback callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_join_av_groupchat")]
        internal static extern int JoinAvGroupchat(ToxHandle tox, int friendNumber, byte[] data, ushort length, ToxAvDelegates.GroupAudioReceiveCallback callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_group_send_audio")]
        internal static extern int GroupSendAudio(ToxHandle tox, int groupNumber, short[] pcm, uint sampleCount, byte channels, uint sampleRate);

        #endregion

        #region Callbacks
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_callback_call")]
        internal static extern void RegisterCallCallback(ToxAvHandle toxAv, ToxAvDelegates.CallCallback callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_callback_call_state")]
        internal static extern void RegisterCallStateCallback(ToxAvHandle toxAv, ToxAvDelegates.CallStateCallback callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_callback_bit_rate_status")]
        internal static extern void RegisterBitrateStatusCallback(ToxAvHandle toxAv, ToxAvDelegates.BitrateStatusCallback callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_callback_video_receive_frame")]
        internal static extern void RegisterVideoReceiveFrameCallback(ToxAvHandle toxAv, ToxAvDelegates.VideoReceiveFrameCallback callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_callback_audio_receive_frame")]
        internal static extern void RegisterAudioReceiveFrameCallback(ToxAvHandle toxAv, ToxAvDelegates.AudioReceiveFrameCallback callback, IntPtr userData);

        #endregion
    }
}
