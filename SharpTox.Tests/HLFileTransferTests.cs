using System;
using NUnit.Framework;
using SharpTox.HL;
using SharpTox.HL.Transfers;
using SharpTox.Core;
using System.IO;
using System.Linq;
using System.Threading;

namespace SharpTox.Tests
{
    [TestFixture]
    public class HLFileTransferTests
    {
        private ToxHL _tox1, _tox2;
        private readonly byte[] _dataToSend = new byte[1 << 26];
        private MemoryStream _sentData;
        private MemoryStream _receivedData;

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
                    (s, e) => e.Transfer.Accept(_receivedData = new MemoryStream(new byte[e.Transfer.Size]));
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

            var transfer = _tox1.Friends[0].SendFile(_sentData = new MemoryStream(_dataToSend), "test.dat", ToxFileKind.Data);

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

            Assert.IsTrue(_dataToSend.SequenceEqual(_receivedData.ToArray()), "Received data is not equal to sent data!");
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

            var transfer = _tox1.Friends[0].SendFile(_sentData = new MemoryStream(_dataToSend), "test.dat", ToxFileKind.Data);

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

            Assert.IsTrue(_dataToSend.SequenceEqual(_receivedData.ToArray()), "Received data is not equal to sent data!");
        }

        /// <summary>
        /// Restart the client which was receiving the file (tox2).
        /// </summary>
        [Test]
        public void HLTestToxFileTransferResumeIncoming()
        {
            var errored = false;
            var errorMessage = string.Empty;
            var finished = false;
            var restartedCount = 0;

            var transfer = _tox1.Friends[0].SendFile(_sentData = new MemoryStream(_dataToSend), "test.dat", ToxFileKind.Data);

            Console.WriteLine(transfer.Progress.ToString("P"));

            transfer.ProgressChanged += (sender, args) =>
            {
                if (0.1 <= args.Progress && args.Progress <= 0.11 && restartedCount == 0)
                {
                    RestartReceiver();
                    restartedCount++;
                }

                if (0.4 <= args.Progress && args.Progress <= 0.41 && restartedCount == 1)
                {
                    RestartReceiver();
                    restartedCount++;
                }

                Console.WriteLine(args.Progress.ToString("P"));
            };

            Console.WriteLine(transfer.State);

            transfer.StateChanged += (sender, args) =>
            {
                Console.WriteLine(args.State);

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

            Assert.IsTrue(_dataToSend.SequenceEqual(_receivedData.ToArray()), "Received data is not equal to sent data!");
        }

        private void RestartReceiver()
        {
            var transferResumeData = _tox2.Friends[0].Transfers[0].GetResumeData();

            var tox2Data = _tox2.GetData();
            _tox2.Dispose();
            _tox2 = new ToxHL(ToxOptions.Default, tox2Data);
            _tox2.Start();

            _tox2.Friends[0].ResumeBrokenTransfer(transferResumeData, _receivedData);
        }
        
        bool _errored;
        string _errorMessage;
        bool _finished;
        int _restartedCount;

        /// <summary>
        /// Restart the client which was sending the file (tox2).
        /// </summary>
        [Test]
        public void HLTestToxFileTransferResumeOutgoing()
        {
            _tox1.Friends[0].TransferRequestReceived +=
                (s, e) => e.Transfer.Accept(_receivedData = new MemoryStream(new byte[e.Transfer.Size]));

            while (!_tox2.Friends[0].IsOnline)
            {
                Thread.Sleep(100);
            }

            var transfer = _tox2.Friends[0].SendFile(_sentData = new MemoryStream(_dataToSend), "test.dat", ToxFileKind.Data);

            Console.WriteLine(transfer.Progress.ToString("P"));

            transfer.ProgressChanged += (sender, args) =>
            {
                if (0.1 <= args.Progress && args.Progress <= 0.11 && _restartedCount == 0)
                {
                    RestartSender();
                    _restartedCount++;
                }
                
                Console.WriteLine(args.Progress.ToString("P"));
            };

            Console.WriteLine(transfer.State);

            transfer.StateChanged += (sender, args) =>
            {
                Console.WriteLine(args.State);

                if (args.State == ToxTransferState.Finished || args.State == ToxTransferState.Canceled)
                {
                    _finished = true;
                }
            };

            transfer.Errored += (sender, args) =>
            {
                _errored = true;
                _errorMessage = args.Error.Message;
            };

            while (!_finished)
            {
                Assert.IsTrue(!_errored, _errorMessage);
                Thread.Sleep(100);
            }

            Assert.IsTrue(_dataToSend.SequenceEqual(_receivedData.ToArray()), "Received data is not equal to sent data!");
        }

        private void RestartSender()
        {
            var transferResumeData = _tox2.Friends[0].Transfers[0].GetResumeData();

            var tox2Data = _tox2.GetData();
            _tox2.Dispose();
            _tox2 = new ToxHL(ToxOptions.Default, tox2Data);
            _tox2.Start();
            
            _tox2.Friends[0].ResumeBrokenTransfer(transferResumeData, _sentData);

            RegisterCallbacks(_tox2.Friends[0].Transfers[0]);
        }

        private void RegisterCallbacks(ToxTransfer transfer)
        {
            transfer.ProgressChanged += (sender, args) =>
            {
                Console.WriteLine(args.Progress.ToString("P"));

                if (0.4 <= args.Progress && args.Progress <= 0.41 && _restartedCount == 1)
                {
                    RestartSender();
                    _restartedCount++;
                }
            };

            Console.WriteLine(transfer.State);

            transfer.StateChanged += (sender, args) =>
            {
                Console.WriteLine(args.State);

                if (args.State == ToxTransferState.Finished || args.State == ToxTransferState.Canceled)
                {
                    _finished = true;
                }
            };

            transfer.Errored += (sender, args) =>
            {
                _errored = true;
                _errorMessage = args.Error.Message;
            };
        }
    }
}