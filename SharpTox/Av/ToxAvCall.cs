using System;

namespace SharpTox.Av
{
    public class ToxAvCall
    {
        public ToxAv ToxAv { get; private set; }
        public int Index { get; private set; }

        public ToxAvCall(ToxAv toxAv, int callIndex)
        {
            ToxAv = toxAv;
            Index = callIndex;
        }

        /// <summary>
        /// Answers a call.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public ToxAvError Answer(ToxAvCodecSettings settings)
        {
            ToxAv.CheckDisposed();

            return ToxAvFunctions.Answer(ToxAv.Handle, Index, ref settings);
        }

        /// <summary>
        /// Hangs up an in-progress call.
        /// </summary>
        /// <returns></returns>
        public ToxAvError Hangup()
        {
            ToxAv.CheckDisposed();

            return ToxAvFunctions.Hangup(ToxAv.Handle, Index);
        }

        /// <summary>
        /// Rejects an incoming call.
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public ToxAvError Reject(string reason)
        {
            ToxAv.CheckDisposed();

            return ToxAvFunctions.Reject(ToxAv.Handle, Index, reason);
        }

        /// <summary>
        /// Stops a call and terminates the transmission without notifying the remote peer.
        /// </summary>
        /// <returns></returns>
        public ToxAvError Stop()
        {
            ToxAv.CheckDisposed();

            return ToxAvFunctions.StopCall(ToxAv.Handle, Index);
        }

        /// <summary>
        /// Checks whether a certain capability is supported.
        /// </summary>
        /// <param name="capability"></param>
        /// <returns></returns>
        public bool CapabilitySupported(ToxAvCapabilities capability)
        {
            ToxAv.CheckDisposed();

            return ToxAvFunctions.CapabilitySupported(ToxAv.Handle, Index, capability) == 1;
        }

        /// <summary>
        /// Sends an encoded audio frame.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="frameSize"></param>
        /// <returns></returns>
        public ToxAvError SendAudio(byte[] frame, int frameSize)
        {
            ToxAv.CheckDisposed();

            return ToxAvFunctions.SendAudio(ToxAv.Handle, Index, frame, (uint)frameSize);
        }

        /// <summary>
        /// Encodes an audio frame.
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="destMax"></param>
        /// <param name="frames"></param>
        /// <param name="perframe"></param>
        /// <returns></returns>
        public int PrepareAudioFrame(byte[] dest, int destMax, short[] frames, int perframe) //TODO: use 'out' keyword to get the encoded frame
        {
            ToxAv.CheckDisposed();

            return ToxAvFunctions.PrepareAudioFrame(ToxAv.Handle, Index, dest, destMax, frames, perframe);
        }

        /// <summary>
        /// Retrieves the state of a call.
        /// </summary>
        /// <returns></returns>
        public ToxAvCallState State
        {
            get
            {
                ToxAv.CheckDisposed();

                return ToxAvFunctions.GetCallState(ToxAv.Handle, Index);
            }
        }

        /// <summary>
        /// Changes the type of an in-progress call
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public ToxAvError ChangeSettings(ToxAvCodecSettings settings)
        {
            ToxAv.CheckDisposed();

            return ToxAvFunctions.ChangeSettings(ToxAv.Handle, Index, ref settings);
        }

        /// <summary>
        /// Prepares transmission. Must be called before any transmission occurs.
        /// </summary>
        /// <param name="supportVideo"></param>
        /// <returns></returns>
        public ToxAvError PrepareTransmission(bool supportVideo)
        {
            ToxAv.CheckDisposed();

            return ToxAvFunctions.PrepareTransmission(ToxAv.Handle, Index, supportVideo ? 1 : 0);
        }

        /// <summary>
        /// Kills the transmission of a call. Should be called at the end of the transmission.
        /// </summary>
        /// <returns></returns>
        public ToxAvError KillTransmission()
        {
            ToxAv.CheckDisposed();

            return ToxAvFunctions.KillTransmission(ToxAv.Handle, Index);
        }

        /// <summary>
        /// Get the friend_number of peer participating in conversation
        /// </summary>
        /// <param name="peer"></param>
        /// <returns></returns>
        public int GetPeerID(int peer)
        {
            ToxAv.CheckDisposed();

            return ToxAvFunctions.GetPeerID(ToxAv.Handle, Index, peer);
        }

    }
}

