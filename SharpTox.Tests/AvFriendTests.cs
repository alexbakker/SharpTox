using System;
using SharpTox.Core;
using SharpTox.Av;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SharpTox.Test
{
    [TestFixture]
    public class AvFriendTests
    {
        private bool _running = true;
        private Tox _tox1;
        private Tox _tox2;
        private ToxAv _toxAv1;
        private ToxAv _toxAv2;

        [TestFixtureSetUp]
        public void Init()
        {
            var options = new ToxOptions(true, true);
            _tox1 = new Tox(options);
            _tox2 = new Tox(options);

            _toxAv1 = new ToxAv(_tox1);
            _toxAv2 = new ToxAv(_tox2);

            _tox1.AddFriend(_tox2.Id, "hey");
            _tox2.AddFriend(_tox1.Id, "hey");

            while (_tox1.GetFriendConnectionStatus(0) == ToxConnectionStatus.None) { DoIterate(); }

            bool answered = false;
            _toxAv1.Call(0, 48, 30000);

            _toxAv2.OnCallRequestReceived += (sender, e) =>
            {
                var error2 = ToxAvErrorAnswer.Ok;
                bool result2 = _toxAv2.Answer(e.FriendNumber, 48, 30000, out error2);
            };

            _toxAv1.OnCallStateChanged += (sender, e) =>
            {
                answered = true;
            };

            while (!answered) { DoIterate(); }
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            _running = false;

            _toxAv1.Dispose();
            _toxAv2.Dispose();

            _tox1.Dispose();
            _tox2.Dispose();
        }

        private void DoIterate()
        {
            int time1 = _tox1.Iterate();
            int time2 = _tox2.Iterate();

            Thread.Sleep(Math.Min(time1, time2));
        }

        [Test]
        public void TestToxAvAudioBitrateChange()
        {
            int bitrate = 16;
            var error = ToxAvErrorSetBitrate.Ok;
            bool result = _toxAv1.SetAudioBitrate(0, bitrate, false, out error);

            if (!result || error != ToxAvErrorSetBitrate.Ok)
                Assert.Fail("Failed to set audio bitrate, error: {0}, result: {1}", error, result);
        }

        [Test]
        public void TestToxAvVideoBitrateChange()
        {
            int bitrate = 2000;
            var error = ToxAvErrorSetBitrate.Ok;
            bool result = _toxAv1.SetVideoBitrate(0, bitrate, false, out error);

            if (!result || error != ToxAvErrorSetBitrate.Ok)
                Assert.Fail("Failed to set video bitrate, error: {0}, result: {1}", error, result);
        }

        [Test]
        public void TestToxAvSendControl()
        {
            var control = ToxAvCallControl.Pause;
            var error = ToxAvErrorCallControl.Ok;
            bool testFinished = false;

            _toxAv2.OnCallStateChanged += (sender, e) =>
            {
                if (!e.State.HasFlag(ToxAvCallState.Paused))
                    Assert.Fail("Tried to pause a call but the call state didn't change correctly, call state: {0}", e.State);

                testFinished = true;
            };

            bool result = _toxAv1.SendControl(0, control, out error);
            if (!result || error != ToxAvErrorCallControl.Ok)
                Assert.Fail("Could not send call control, error: {0}, result: {1}", error, result);

            while (!testFinished) { DoIterate(); }
        }

        [Test]
        public void TestToxAvSendAudio()
        {
            _toxAv2.OnAudioFrameReceived += (sender, e) =>
            {
                Console.WriteLine("Received frame, length: {0}, sampling rate: {1}", e.Frame.Data.Length, e.Frame.SamplingRate);
            };

            for (int i = 0; i < 100; i++)
            {
                short[] frame = new short[1920];
                RandomShorts(frame);

                var error = ToxAvErrorSendFrame.Ok;
                bool result = _toxAv1.SendAudioFrame(0, new ToxAvAudioFrame(frame, 48000, 2), out error);

                if (!result || error != ToxAvErrorSendFrame.Ok)
                    Assert.Fail("Failed to send audio frame, error: {0}, result: {1}", error, result);

                DoIterate();
            }
        }

        [Test]
        public void TestToxAvSendVideo()
        {
            _toxAv2.OnVideoFrameReceived += (sender, e) =>
            {
                Console.WriteLine("Received frame, width: {0}, height: {1}", e.Frame.Width, e.Frame.Height);
            };

            for (int i = 0; i < 100; i++)
            {
                var random = new Random();
                int width = 800;
                int height = 600;

                byte[] y = new byte[width * height];
                byte[] u = new byte[(height / 2) * (width / 2)];
                byte[] v = new byte[(height / 2) * (width / 2)];

                var frame = new ToxAvVideoFrame(800, 600, y, u, v);

                var error = ToxAvErrorSendFrame.Ok;
                bool result = _toxAv1.SendVideoFrame(0, frame, out error);

                if (!result || error != ToxAvErrorSendFrame.Ok)
                    Assert.Fail("Failed to send video frame, error: {0}, result: {1}", error, result);

                DoIterate();
            }
        }

        private static void RandomShorts(short[] shorts)
        {
            var random = new Random();
            for (int i = 0; i < shorts.Length; i++)
            {
                byte[] s = new byte[sizeof(short)];
                random.NextBytes(s);

                shorts[i] = BitConverter.ToInt16(s, 0);
            }
        }
    }
}
