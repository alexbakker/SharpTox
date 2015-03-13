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
        private static bool _running;
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
    }
}
