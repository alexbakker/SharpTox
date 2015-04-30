using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTox.Core;

namespace SharpTox.Test
{
    [TestClass]
    public class CoreFriendTests : ExtendedTestClass
    {
        private static bool _running = true;
        private static Tox _tox1;
        private static Tox _tox2;

        [ClassInitialize()]
        public static void InitClass(TestContext context)
        {
            var options = new ToxOptions(true, true);
            _tox1 = new Tox(options);
            _tox2 = new Tox(options);

            DoLoop();

            _tox1.AddFriend(_tox2.Id, "hey");
            _tox2.AddFriend(_tox1.Id, "hey");

            while (_tox1.GetFriendConnectionStatus(0) == ToxConnectionStatus.None) { Thread.Sleep(10); }
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            _running = false;

            _tox1.Dispose();
            _tox2.Dispose();
        }
       
        private static void DoLoop()
        {
            Task.Run(async () =>
            {
                while (_running)
                {
                    int time1 = _tox1.Iterate();
                    int time2 = _tox2.Iterate();

                    await Task.Delay(Math.Min(time1, time2));
                }
            });
        }

        [TestMethod]
        public void TestToxMessage()
        {
            string messageFormat = "Hey! This is test message number ";
            int messageCount = 100;
            int receivedMessageCount = 0;

            for (int i = 0; i < messageCount; i++)
            {
                var sendError = ToxErrorSendMessage.Ok;
                _tox1.SendMessage(0, messageFormat + i.ToString(), ToxMessageType.Message, out sendError);
                if (sendError != ToxErrorSendMessage.Ok)
                    Assert.Fail("Failed to send message to friend: {0}", sendError);
            }

            _tox2.OnFriendMessageReceived += (object sender, ToxEventArgs.FriendMessageEventArgs args) =>
            {
                if (args.MessageType != ToxMessageType.Message || args.Message != messageFormat + receivedMessageCount)
                {
                    Fail("Message arrived got garbled");
                    return;
                }

                receivedMessageCount++;
            };

            while (receivedMessageCount != messageCount && _wait) { Thread.Sleep(10); }
            Console.WriteLine("Received all messages without errors");
        }

        [TestMethod]
        public void TestToxAction()
        {
            string actionFormat = "Hey! This is test action number ";
            int actionCount = 100;
            int receivedActionCount = 0;

            for (int i = 0; i < actionCount; i++)
            {
                var sendError = ToxErrorSendMessage.Ok;
                _tox1.SendMessage(0, actionFormat + i.ToString(), ToxMessageType.Action, out sendError);
                if (sendError != ToxErrorSendMessage.Ok)
                    Fail("Failed to send action to friend: {0}", sendError);
            }

            _tox2.OnFriendMessageReceived += (object sender, ToxEventArgs.FriendMessageEventArgs args) =>
            {
                if (args.MessageType != ToxMessageType.Action || args.Message != actionFormat + receivedActionCount)
                {
                    Fail("Action arrived got garbled");
                    return;
                }

                receivedActionCount++;
            };

            while (receivedActionCount != actionCount && _wait) { Thread.Sleep(10); }
            Console.WriteLine("Received all actions without errors");
        }

        [TestMethod]
        public void TestToxName()
        {
            string name = "Test, test and test";

            _tox1.Name = name;
            _tox2.OnFriendNameChanged += (object sender, ToxEventArgs.NameChangeEventArgs args) =>
            {
                if (args.Name != name)
                    Fail("Name received is not equal to the name we set");

                _wait = false;
            };

            while (_wait) { Thread.Sleep(10); }
            CheckFailed();
        }

        [TestMethod]
        public void TestToxStatus()
        {
            var status = ToxUserStatus.Busy;

            _tox1.Status = status;
            _tox2.OnFriendStatusChanged += (object sender, ToxEventArgs.StatusEventArgs args) =>
            {
                if (args.Status != status)
                    Fail("Status received is not equal to the status we set");

                _wait = false;
            };

            while (_wait) { Thread.Sleep(10); }
            CheckFailed();
        }

        [TestMethod]
        public void TestToxStatusMessage()
        {
            string message = "Test, test and test";

            _tox1.StatusMessage = message;
            _tox2.OnFriendStatusMessageChanged += (object sender, ToxEventArgs.StatusMessageEventArgs args) =>
            {
                if (args.StatusMessage != message)
                    Fail("Status message received is not equal to the status message we set");

                _wait = false;
            };

            while (_wait) { Thread.Sleep(10); }
            CheckFailed();
        }

        [TestMethod]
        public void TestToxTyping()
        {
            bool isTyping = true;

            _tox2.OnFriendTypingChanged += (object sender, ToxEventArgs.TypingStatusEventArgs args) =>
            {
                if (args.IsTyping != isTyping)
                {
                    Fail("IsTyping value received does not equal the one we set");
                    return;
                }
                
                var error = ToxErrorFriendQuery.Ok;
                bool result = _tox2.GetFriendTypingStatus(0, out error);
                if (!result || error != ToxErrorFriendQuery.Ok)
                    Fail("Failed to get typing status, error: {0}, result: {1}", error, result);

                _wait = false;
            };
            {
                var error = ToxErrorSetTyping.Ok;
                bool result = _tox1.SetTypingStatus(0, isTyping, out error);
                if (!result || error != ToxErrorSetTyping.Ok)
                    Assert.Fail("Failed to set typing status, error: {0}, result: {1}", error, result);

                while (_wait) { Thread.Sleep(10); }
                CheckFailed();
            }
        }

        [TestMethod]
        public void TestToxFriendPublicKey()
        {
            var error = ToxErrorFriendGetPublicKey.Ok;
            var publicKey = _tox2.GetFriendPublicKey(0, out error);
            if (error != ToxErrorFriendGetPublicKey.Ok)
                Assert.Fail("Could not get friend public key, error: {0}", error);

            var error2 = ToxErrorFriendByPublicKey.Ok;
            int friend = _tox2.GetFriendByPublicKey(publicKey, out error2);
            if (friend != 0 || error2 != ToxErrorFriendByPublicKey.Ok)
                Assert.Fail("Could not get friend by public key, error: {0}, friend: {1}", error2, friend);
        }

        [TestMethod]
        public void TestToxLossyPacket()
        {
            int receivedPackets = 0;
            byte[] data = new byte[ToxConstants.MaxCustomPacketSize];
            new Random().NextBytes(data);
            data[0] = 210;

            var error = ToxErrorFriendCustomPacket.Ok;
            bool result = _tox1.FriendSendLossyPacket(0, data, out error);
            if (!result || error != ToxErrorFriendCustomPacket.Ok)
                Assert.Fail("Failed to send lossy packet to friend, error: {0}, result: {1}", error, result);

            _tox2.OnFriendLossyPacketReceived += (object sender, ToxEventArgs.FriendPacketEventArgs args) =>
            {
                if (args.Data.Length != data.Length || data[0] != args.Data[0])
                {
                    Fail("Packet doesn't have the same length/identifier");
                    return;
                }
                else if (!data.SequenceEqual(args.Data))
                {
                    Fail("Packet contents don't match");
                    return;
                }

                receivedPackets++;
            };

            while (receivedPackets != 1 && _wait) { Thread.Sleep(10); }
            CheckFailed();
        }

        [TestMethod]
        public void TestToxLosslessPacket()
        {
            int receivedPackets = 0;
            byte[] data = new byte[ToxConstants.MaxCustomPacketSize];
            new Random().NextBytes(data);
            data[0] = 170;

            var error = ToxErrorFriendCustomPacket.Ok;
            bool result = _tox1.FriendSendLosslessPacket(0, data, out error);
            if (!result || error != ToxErrorFriendCustomPacket.Ok)
                Assert.Fail("Failed to send lossless packet to friend, error: {0}, result: {1}", error, result);

            _tox2.OnFriendLosslessPacketReceived += (object sender, ToxEventArgs.FriendPacketEventArgs args) =>
            {
                if (args.Data.Length != data.Length || data[0] != args.Data[0])
                {
                    Fail("Packet doesn't have the same length/identifier");
                    return;
                }
                else if (!data.SequenceEqual(args.Data))
                {
                    Fail("Packet contents don't match");
                    return;
                }

                receivedPackets++;
            };

            while (receivedPackets != 1 && _wait) { Thread.Sleep(10); }
            CheckFailed();
        }

        [TestMethod]
        public void TestToxFileTransfer()
        {
            byte[] fileData = new byte[0xBEEEF];
            byte[] receivedData = new byte[0xBEEEF];
            new Random().NextBytes(fileData);

            string fileName = "testing.dat";
            bool fileReceived = false;

            var error = ToxErrorFileSend.Ok;
            var fileInfo = _tox1.FileSend(0, ToxFileKind.Data, fileData.Length, fileName, out error);
            if (error != ToxErrorFileSend.Ok)
                Assert.Fail("Failed to send a file send request, error: {0}", error);

            _tox1.OnFileChunkRequested += (object sender, ToxEventArgs.FileRequestChunkEventArgs args) =>
            {
                byte[] data = new byte[args.Length];
                Array.Copy(fileData, args.Position, data, 0, args.Length);

                var error2 = ToxErrorFileSendChunk.Ok;
                bool result = _tox1.FileSendChunk(args.FriendNumber, args.FileNumber, args.Position, data, out error2);
                if (!result || error2 != ToxErrorFileSendChunk.Ok)
                {
                    Fail("Failed to send chunk, error: {0}, result: {1}", error2, result);
                    return;
                }
            };

            _tox2.OnFileSendRequestReceived += (object sender, ToxEventArgs.FileSendRequestEventArgs args) => 
            {
                if (fileName != args.FileName)
                {
                    Fail("Filenames do not match");
                    return;
                }

                if (args.FileSize != fileData.Length)
                {
                    Fail("File lengths do not match");
                    return;
                }

                var error2 = ToxErrorFileControl.Ok;
                bool result = _tox2.FileControl(args.FriendNumber, args.FileNumber, ToxFileControl.Resume);
                if (!result || error2 != ToxErrorFileControl.Ok)
                {
                    Fail("Failed to send file control, error: {0}, result: {1}", error2, result);
                    return;
                }
            };

            _tox2.OnFileChunkReceived += (object sender, ToxEventArgs.FileChunkEventArgs args) =>
            {
                if (args.Position == fileData.Length)
                    fileReceived = true;
                else
                    Array.Copy(args.Data, 0, receivedData, args.Position, args.Data.Length);
            };

            while (!fileReceived && _wait) { Thread.Sleep(10); }
            CheckFailed();

            if (!fileData.SequenceEqual(receivedData))
                Assert.Fail("Original data is not equal to the data we received");
        }
    }
}
