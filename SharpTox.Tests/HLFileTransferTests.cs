using System;
using NUnit.Framework;
using SharpTox.HL;
using SharpTox.HL.Transfers;
using SharpTox.Core;
using System.IO;
using System.Threading;

namespace SharpTox.Tests
{
    [TestFixture]
    public class HLFileTransferTests
    {
        private ToxHL _tox1, _tox2;
        private readonly byte[] _dataToSend = new byte[1 << 16];

        [TestFixtureSetUp]
        public void SetUp()
        {
            new Random().NextBytes(_dataToSend);

            _tox1 = new ToxHL(ToxOptions.Default);
            _tox2 = new ToxHL(ToxOptions.Default);

            _tox1.Start();
            _tox2.Start();

            _tox1.AddFriend(_tox2.Id, "test");

            _tox2.FriendRequestReceived += (sender, args) =>
            {
                var friend = _tox2.AddFriendNoRequest(args.PublicKey);

                friend.TransferRequestReceived +=
                    (s, e) => e.Transfer.Accept(new MemoryStream(new byte[e.Transfer.Size]));
            };

            while (!_tox1.Friends[0].IsOnline)
            {
                Thread.Sleep(100);
            }
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _tox1.Dispose();
            _tox2.Dispose();
        }

        [Test]
        public void HLTestToxFileTransferInfo()
        {
            var errored = false;
            var errorMessage = string.Empty;
            var finished = false;

            var transfer = _tox1.Friends[0].SendFile(new MemoryStream(_dataToSend), "test.dat", ToxFileKind.Data);

            Console.WriteLine(transfer.Progress.ToString("P"));

            Console.WriteLine("elapsed:\t" + transfer.ElapsedTime.ToString("hh\\:mm\\:ss\\:ff"));
            Console.WriteLine("remaining:\t" +
                              (transfer.RemainingTime.HasValue
                                  ? transfer.RemainingTime.Value.ToString("hh\\:mm\\:ss\\:ff")
                                  : "unknown"));

            transfer.ProgressChanged += (sender, args) =>
            {
                Console.WriteLine(args.Progress.ToString("P"));

                Console.WriteLine("elapsed:\t" + transfer.ElapsedTime.ToString("hh\\:mm\\:ss\\:ff"));
                Console.WriteLine("remaining:\t" +
                                  (transfer.RemainingTime.HasValue
                                      ? transfer.RemainingTime.Value.ToString("hh\\:mm\\:ss\\:ff")
                                      : "unknown"));
            };

            Console.WriteLine(">> " + (transfer.Speed/1000).ToString("F") + " kByte/sec");
            transfer.SpeedChanged +=
                (sender, args) => { Console.WriteLine(">> " + (args.Speed/1000).ToString("F") + " kByte/sec"); };

            transfer.StateChanged += (sender, args) =>
            {
                if (args.State == ToxTransferState.Finished)
                {
                    finished = true;
                }
                else if (args.State == ToxTransferState.Canceled)
                {
                    errored = true;
                    errorMessage = "Transfer is canceled unexpectedly!";
                }
            };

            transfer.Errored += (sender, args) =>
            {
                errored = true;
                errorMessage = args.Error.Message;
            };

            while (!finished)
            {
                Assert.IsTrue(!errored, errorMessage);
                Thread.Sleep(100);
            }
        }

        [Test]
        public void HLTestToxFileTransferConsecutively()
        {
            SendOne();
            SendOne();
        }

        private void SendOne()
        {
            var errored = false;
            var errorMessage = string.Empty;
            var finished = false;

            var transfer = _tox1.Friends[0].SendFile(new MemoryStream(_dataToSend), "test.dat", ToxFileKind.Data);

            transfer.StateChanged += (sender, args) =>
            {
                if (args.State == ToxTransferState.Finished || args.State == ToxTransferState.Canceled)
                {
                    finished = true;
                }
            };

            transfer.Errored += (sender, args) =>
            {
                errored = true;
                errorMessage = args.Error.Message;
            };

            while (!finished)
            {
                Assert.IsTrue(!errored, errorMessage);
                Thread.Sleep(100);
            }
        }
    }
}