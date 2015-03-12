using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTox.Core;

namespace SharpTox.Test
{
    [TestClass]
    public class CoreTests
    {
        private ToxOptions _options = new ToxOptions(true, true);

        [TestMethod]
        public void TestToxBootstrapAndConnect()
        {
            var tox = new Tox(_options);
            var error = ToxErrorBootstrap.Ok;
            bool result = tox.Bootstrap(new ToxNode("impy.me", 33445, new ToxKey(ToxKeyType.Public, "788236D34978D1D5BD822F0A5BEBD2C53C64CC31CD3149350EE27D4D9A2F9B6B")), out error);
            if (!result || error != ToxErrorBootstrap.Ok)
                Assert.Fail("Failed to bootstrap error: {0}, result: {1}", error, result);

            tox.Start();

            while (!tox.IsConnected) { }

            Console.WriteLine("Tox connected!");
            tox.Dispose();
        }

        [TestMethod]
        public void TestToxMessage()
        {
            var tox1 = new Tox(_options);
            var tox2 = new Tox(_options);
            var addError = ToxErrorFriendAdd.Ok;
            var messages = new List<string>(10);
            bool testFinished = false;

            Task.Run(async() =>
            {
                while (!testFinished)
                {
                    int time1 = tox1.Iterate();
                    int time2 = tox2.Iterate();

                    await Task.Delay(Math.Max(time1, time2));
                }
            });

            tox2.AddFriend(tox1.Id, "hey", out addError);
            if (addError != ToxErrorFriendAdd.Ok)
                Assert.Fail("Failed to add friend: {0}", addError);

            tox1.OnFriendRequest += (object sender, ToxEventArgs.FriendRequestEventArgs args) =>
            {
                tox1.AddFriendNoRequest(args.PublicKey, out addError);
                Assert.AreEqual(addError, ToxErrorFriendAdd.Ok);
                if (addError != ToxErrorFriendAdd.Ok)
                    Assert.Fail("Failed to add friend (no request): {0}", addError);
            };

            while (tox1.GetFriendConnectionStatus(0) == ToxConnectionStatus.None) { }

            for (int i = 0; i < messages.Count; i++)
            {
                var sendError = ToxErrorSendMessage.Ok;
                tox1.SendMessage(0, "Hey! This is test message number " + i.ToString());
                if (sendError != ToxErrorSendMessage.Ok)
                    Assert.Fail("Failed to send message to friend: {0}", addError);
            }

            tox1.Dispose();
            tox2.Dispose();
            testFinished = true;
        }
    }
}
