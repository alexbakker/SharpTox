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
        internal static extern bool Call(ToxAvHandle toxAv, uint friendNumber, uint audioBitrate, uint videoBitrate, ref ToxAvErrorCall error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_answer")]
        internal static extern bool Answer(ToxAvHandle toxAv, uint friendNumber, uint audioBitrate, uint videoBitrate, ref ToxAvErrorAnswer error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_call_control")]
        internal static extern bool CallControl(ToxAvHandle toxAv, uint friendNumber, ToxAvCallControl control, ref ToxAvErrorCallControl error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_set_audio_bit_rate")]
        internal static extern bool SetAudioBitrate(ToxAvHandle toxAv, uint friendNumber, uint audioBitrate, ref ToxAvErrorBitrate error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_set_video_bit_rate")]
        internal static extern bool SetVideoBitrate(ToxAvHandle toxAv, uint friendNumber, uint videoBitrate, ref ToxAvErrorBitrate error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_send_video_frame")]
        internal static extern bool SendVideoFrame(ToxAvHandle toxAv, uint friendNumber, ushort width, ushort height, byte[] y, byte[] u, byte[] v, ref ToxAvErrorSendFrame error);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_send_audio_frame")]
        internal static extern bool SendAudioFrame(ToxAvHandle toxAv, uint friendNumber, short[] pcm, uint sampleCount, byte channels, uint samplingRate, ref ToxAvErrorSendFrame error);

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

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_callback_video_frame_request")]
        internal static extern void RegisterVideoFrameRequestCallback(ToxAvHandle toxAv, ToxAvDelegates.FrameRequestCallback callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_callback_audio_frame_request")]
        internal static extern void RegisterAudioFrameRequestCallback(ToxAvHandle toxAv, ToxAvDelegates.FrameRequestCallback callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_callback_receive_video_frame")]
        internal static extern void RegisterReceiveVideoFrameCallback(ToxAvHandle toxAv, ToxAvDelegates.ReceiveVideoFrameCallback callback, IntPtr userData);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "toxav_callback_receive_audio_frame")]
        internal static extern void RegisterReceiveAudioFrameCallback(ToxAvHandle toxAv, ToxAvDelegates.ReceiveAudioFrameCallback callback, IntPtr userData);

        #endregion
    }
}

#pragma warning restore 1591