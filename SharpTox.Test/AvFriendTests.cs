using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTox.Core;
using SharpTox.Av;
using System.Threading;
using System.Threading.Tasks;

namespace SharpTox.Test
{
    [TestClass]
    public class AvFriendTests : ExtendedTestClass
    {
        private static bool _running = true;
        private static Tox _tox1;
        private static Tox _tox2;
        private static ToxAv _toxAv1;
        private static ToxAv _toxAv2;

        [ClassInitialize()]
        public static void InitClass(TestContext context)
        {
            var options = new ToxOptions(true, true);
            _tox1 = new Tox(options);
            _tox2 = new Tox(options);

            _toxAv1 = new ToxAv(_tox1);
            _toxAv2 = new ToxAv(_tox2);

            DoLoop();

            _tox1.AddFriend(_tox2.Id, "hey");
            _tox2.AddFriend(_tox1.Id, "hey");

            while (_tox1.GetFriendConnectionStatus(0) == ToxConnectionStatus.None) { Thread.Sleep(10); }

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

            while (!answered) { Thread.Sleep(10); }
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            _running = false;

            _toxAv1.Dispose();
            _toxAv2.Dispose();

            _tox1.Dispose();
            _tox2.Dispose();
        }

        private static void DoLoop()
        {
            Task.Run(async () =>
            {
                while (_running)
                {
                    int time1 = Math.Min(_tox1.Iterate(), _tox2.Iterate());
                    int time2 = Math.Min(_toxAv1.Iterate(), _toxAv2.Iterate());

                    await Task.Delay(Math.Min(time1, time2));
                }
            });
        }

        [TestMethod]
        public void TestToxAvAudioBitrateChange()
        {
            int bitrate = 16;
            var error = ToxAvErrorBitrate.Ok;
            bool result = _toxAv1.SetAudioBitrate(0, bitrate, false, out error);

            if (!result || error != ToxAvErrorBitrate.Ok)
                Assert.Fail("Failed to set audio bitrate, error: {0}, result: {1}", error, result);
        }

        [TestMethod]
        public void TestToxAvVideoBitrateChange()
        {
            int bitrate = 2000;
            var error = ToxAvErrorBitrate.Ok;
            bool result = _toxAv1.SetVideoBitrate(0, bitrate, false, out error);

            if (!result || error != ToxAvErrorBitrate.Ok)
                Assert.Fail("Failed to set video bitrate, error: {0}, result: {1}", error, result);
        }

        [TestMethod]
        public void TestToxAvSendControl()
        {
            var control = ToxAvCallControl.Pause;
            var error = ToxAvErrorCallControl.Ok;
            bool result = _toxAv1.SendControl(0, control, out error);

            if (!result || error != ToxAvErrorCallControl.Ok)
                Assert.Fail("Could not send call control, error: {0}, result: {1}", error, result);

            _toxAv2.OnCallStateChanged += (sender, e) =>
            {
                if (!e.State.HasFlag(ToxAvCallState.Paused))
                    Fail("Tried to pause a call but the call state didn't change correctly, call state: {0}", e.State);

                _wait = false;
            };

            while (_wait) { Thread.Sleep(10); }
            CheckFailed();
        }

        [TestMethod]
        public void TestToxAvSendAudio()
        {
            _toxAv2.OnAudioFrameReceived += (sender, e) =>
            {
                Console.WriteLine("Received frame, length: {0}, sampling rate: {1}", e.Frame.Data.Length, e.Frame.SamplingRate);
            };

            for (int i = 0; i < 100; i++)
            {
                short[] frame = new short[960];
                RandomShorts(frame);

                var error = ToxAvErrorSendFrame.Ok;
                bool result = _toxAv1.SendAudioFrame(0, new ToxAvAudioFrame(frame, 48000, 1), out error);

                if (!result || error != ToxAvErrorSendFrame.Ok)
                    Assert.Fail("Failed to send audio frame, error: {0}, result: {1}", error, result);

                Thread.Sleep(30);
            }
        }

        [TestMethod]
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

                Thread.Sleep(30);
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
