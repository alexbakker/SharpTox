using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTox.Av;
using SharpTox.Core;

namespace SharpTox.Test
{
    [TestFixture]
    public class AvSelfTests
    {
        [Test]
        public void TestToxAvCallAndAnswer()
        {
            var options = new ToxOptions(true, true);
            var tox1 = new Tox(options);
            var tox2 = new Tox(options);

            var toxAv1 = new ToxAv(tox1);
            var toxAv2 = new ToxAv(tox2);

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

            tox1.AddFriend(tox2.Id, "hey");
            tox2.AddFriend(tox1.Id, "hey");

            while (tox1.GetFriendConnectionStatus(0) == ToxConnectionStatus.None) { Thread.Sleep(10); }

            bool answered = false;
            toxAv1.Call(0, 48, 30000);

            toxAv2.OnCallRequestReceived += (sender, e) =>
            {
                var error2 = ToxAvErrorAnswer.Ok;
                bool result2 = toxAv2.Answer(e.FriendNumber, 48, 30000, out error2);
            };

            toxAv1.OnCallStateChanged += (sender, e) =>
            {
                answered = true;
            };

            while (!answered) { Thread.Sleep(10); }

            testFinished = true;
            toxAv1.Dispose();
            toxAv2.Dispose();
            tox1.Dispose();
            tox2.Dispose();
        }
    }
}
