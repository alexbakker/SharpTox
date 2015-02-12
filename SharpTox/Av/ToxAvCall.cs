using System;
using SharpTox.Core;

namespace SharpTox.Av
{
    public class ToxAvCall
    {
        public ToxAv ToxAv { get; private set; }
        public int Index { get; private set; }

        internal ToxAvCall(ToxAv toxAv, int callIndex)
        {
            ToxAv = toxAv;
            Index = callIndex;
        }

        /// <summary>
        /// Answers a call.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public void Answer(ToxAvCodecSettings settings)
        {
            ToxAv.CheckDisposed();
            ToxAvException.Check(ToxAvFunctions.Answer(ToxAv.Handle, Index, ref settings));
        }

        /// <summary>
        /// Hangs up an in-progress call.
        /// </summary>
        /// <returns></returns>
        public void Hangup()
        {
            ToxAv.CheckDisposed();
            ToxAvException.Check(ToxAvFunctions.Hangup(ToxAv.Handle, Index));
        }

        /// <summary>
        /// Rejects an incoming call.
        /// </summary>
        /// <returns></returns>
        public void Reject()
        {
            ToxAv.CheckDisposed();
            ToxAvException.Check(ToxAvFunctions.Reject(ToxAv.Handle, Index, string.Empty));
        }

        /// <summary>
        /// Stops a call and terminates the transmission without notifying the remote peer.
        /// </summary>
        /// <returns></returns>
        public void Stop()
        {
            ToxAv.CheckDisposed();
            ToxAvException.Check(ToxAvFunctions.StopCall(ToxAv.Handle, Index));
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
        public void SendAudio(byte[] frame, int frameSize)
        {
            ToxAv.CheckDisposed();
            ToxAvException.Check(ToxAvFunctions.SendAudio(ToxAv.Handle, Index, frame, (uint)frameSize));
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
        public void ChangeSettings(ToxAvCodecSettings settings)
        {
            ToxAv.CheckDisposed();
            ToxAvException.Check(ToxAvFunctions.ChangeSettings(ToxAv.Handle, Index, ref settings));
        }

        /// <summary>
        /// Retrieves a peer's codec settings.
        /// </summary>
        /// <param name="callIndex"></param>
        /// <param name="peer"></param>
        /// <returns></returns>
        public ToxAvCodecSettings GetPeerCodecSettings()
        {
            ToxAv.CheckDisposed();

            ToxAvCodecSettings settings = new ToxAvCodecSettings();
            ToxAvFunctions.GetPeerCodecSettings(ToxAv.Handle, Index, 0, ref settings);

            return settings;
        }

        /// <summary>
        /// Prepares transmission. Must be called before any transmission occurs.
        /// </summary>
        /// <param name="supportVideo"></param>
        /// <returns></returns>
        public void PrepareTransmission(bool supportVideo)
        {
            ToxAv.CheckDisposed();
            ToxAvException.Check(ToxAvFunctions.PrepareTransmission(ToxAv.Handle, Index, supportVideo ? 1 : 0));
        }

        /// <summary>
        /// Kills the transmission of a call. Should be called at the end of the transmission.
        /// </summary>
        /// <returns></returns>
        public void KillTransmission()
        {
            ToxAv.CheckDisposed();
            ToxAvException.Check(ToxAvFunctions.KillTransmission(ToxAv.Handle, Index));
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

        /// <summary>
        /// Returns the corresponding friend to this call.
        /// </summary>
        /// <value>The friend.</value>
        public ToxFriend Friend
        {
            get
            {
                return ToxAv.Tox.FriendFromFriendNumber(GetPeerID(0));
            }
        }

        /// <summary>
        /// Cancels a call.
        /// </summary>
        /// <returns></returns>
        public void Cancel()
        {
            ToxAvException.Check(ToxAvFunctions.Cancel(ToxAv.Handle, Index, Friend.Number, string.Empty));
        }
    }
}
