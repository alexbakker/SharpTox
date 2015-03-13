using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTox.Core;

namespace SharpTox.Test
{
    [TestClass]
    public class CoreFriendTests
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

            while (_tox1.GetFriendConnectionStatus(0) == ToxConnectionStatus.None) { }
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
                _tox1.SendMessage(0, messageFormat + i.ToString(), out sendError);
                if (sendError != ToxErrorSendMessage.Ok)
                    Assert.Fail("Failed to send message to friend: {0}", sendError);
            }

            _tox2.OnFriendMessage += (object sender, ToxEventArgs.FriendMessageEventArgs args) =>
            {
                if (args.Message != messageFormat + receivedMessageCount)
                    Assert.Fail("Message arrived got garbled");

                receivedMessageCount++;
            };

            while (receivedMessageCount != messageCount) { }
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
                _tox1.SendAction(0, actionFormat + i.ToString(), out sendError);
                if (sendError != ToxErrorSendMessage.Ok)
                    Assert.Fail("Failed to send action to friend: {0}", sendError);
            }

            _tox2.OnFriendAction += (object sender, ToxEventArgs.FriendActionEventArgs args) =>
            {
                if (args.Action != actionFormat + receivedActionCount)
                    Assert.Fail("Action arrived got garbled");

                receivedActionCount++;
            };

            while (receivedActionCount != actionCount) { }
            Console.WriteLine("Received all actions without errors");
        }

        [TestMethod]
        public void TestToxName()
        {
            string name = "Test, test and test";
            bool wait = true;

            _tox1.Name = name;
            _tox2.OnFriendNameChanged += (object sender, ToxEventArgs.NameChangeEventArgs args) =>
            {
                if (args.Name != name)
                    Assert.Fail("Name received is not equal to the name we set");

                wait = false;
            };

            while (wait) { }
        }

        [TestMethod]
        public void TestToxStatus()
        {
            var status = ToxStatus.Busy;
            bool wait = true;

            _tox1.Status = status;
            _tox2.OnFriendStatusChanged += (object sender, ToxEventArgs.StatusEventArgs args) =>
            {
                if (args.Status != status)
                    Assert.Fail("Status received is not equal to the status we set");

                wait = false;
            };

            while (wait) { }
        }

        [TestMethod]
        public void TestToxStatusMessage()
        {
            string message = "Test, test and test";
            bool wait = true;

            _tox1.StatusMessage = message;
            _tox2.OnFriendStatusMessageChanged += (object sender, ToxEventArgs.StatusMessageEventArgs args) =>
            {
                if (args.StatusMessage != message)
                    Assert.Fail("Status message received is not equal to the status message we set");

                wait = false;
            };

            while (wait) { }
        }

        [TestMethod]
        public void TestToxFriendRequest()
        {
            var options = new ToxOptions(true, true);
            var tox1 = new Tox(options);
            var tox2 = new Tox(options);
            var error = ToxErrorFriendAdd.Ok;
            string message = "Hey, this is a test friend request.";
            bool testFinished = false;

            Task.Run(async () =>
            {
                while (!testFinished)
                {
                    int time1 = tox1.Iterate();
                    int time2 = tox2.Iterate();

                    await Task.Delay(Math.Min(time1, time2));
                }
            });

            tox1.AddFriend(tox2.Id, message, out error);
            if (error != ToxErrorFriendAdd.Ok)
                Assert.Fail("Failed to add friend: {0}", error);

            tox2.OnFriendRequest += (object sender, ToxEventArgs.FriendRequestEventArgs args) =>
            {
                if (args.Message != message)
                    Assert.Fail("Message received in the friend request is not the same as the one that was sent");

                tox2.AddFriendNoRequest(args.PublicKey, out error);
                if (error != ToxErrorFriendAdd.Ok)
                    Assert.Fail("Failed to add friend (no request): {0}", error);
            };

            while (tox1.GetFriendConnectionStatus(0) == ToxConnectionStatus.None) { }

            testFinished = true;
            tox1.Dispose();
            tox2.Dispose();
        }
    }
}
