using System;
using System.Linq;
using System.Threading;
using SharpTox.Core;
using SharpTox.Encryption;
using System.Runtime.InteropServices;
using NUnit.Framework;
using System.Threading.Tasks;

namespace SharpTox.Test
{
    [TestFixture]
    public class CoreSelfTests
    {
        [Test]
        public void TestToxPortBind()
        {
            var tox1 = new Tox(new ToxOptions(true, false));
            var tox2 = new Tox(new ToxOptions(true, true));

            var error = ToxErrorGetPort.Ok;
            int port = tox1.GetUdpPort(out error);
            if (error != ToxErrorGetPort.NotBound)
                Assert.Fail("Tox bound to an udp port while it's not supposed to, port: {0}", port);

            port = tox2.GetUdpPort(out error);
            if (error != ToxErrorGetPort.Ok)
                Assert.Fail("Failed to bind to an udp port");

            tox1.Dispose();
            tox2.Dispose();
        }

        [Test]
        public void TestToxLoadData()
        {
            var tox1 = new Tox(ToxOptions.Default);
            tox1.Name = "Test";
            tox1.StatusMessage = "Hey";

            var data = tox1.GetData();
            var tox2 = new Tox(ToxOptions.Default, ToxData.FromBytes(data.Bytes));

            if (tox2.Id != tox1.Id)
                Assert.Fail("Failed to load tox data correctly, tox id's don't match");

            if (tox2.Name != tox1.Name)
                Assert.Fail("Failed to load tox data correctly, names don't match");

            if (tox2.StatusMessage != tox1.StatusMessage)
                Assert.Fail("Failed to load tox data correctly, status messages don't match");

            tox1.Dispose();
            tox2.Dispose();
        }

        [Test]
        public void TestToxLoadSecretKey()
        {
            var tox1 = new Tox(ToxOptions.Default);
            var key1 = tox1.GetPrivateKey();

            var tox2 = new Tox(ToxOptions.Default, key1);
            var key2 = tox2.GetPrivateKey();

            if (key1 != key2)
                Assert.Fail("Private keys do not match");
        }

        [Test]
        public void TestToxSelfName()
        {
            var tox = new Tox(ToxOptions.Default);
            string name = "Test name";
            tox.Name = name;

            if (tox.Name != name)
                Assert.Fail("Failed to set/retrieve name");

            tox.Dispose();
        }

        [Test]
        public void TestToxSelfStatusMessage()
        {
            var tox = new Tox(ToxOptions.Default);
            string statusMessage = "Test status message";
            tox.StatusMessage = statusMessage;

            if (tox.StatusMessage != statusMessage)
                Assert.Fail("Failed to set/retrieve status message");

            tox.Dispose();
        }

        [Test]
        public void TestToxSelfStatus()
        {
            var tox = new Tox(ToxOptions.Default);
            var status = ToxUserStatus.Away;
            tox.Status = status;

            if (tox.Status != status)
                Assert.Fail("Failed to set/retrieve status");

            tox.Dispose();
        }

        [Test]
        public void TestToxNospam()
        {
            var tox = new Tox(ToxOptions.Default);
            byte[] randomBytes = new byte[sizeof(uint)];
            new Random().NextBytes(randomBytes);

            int nospam = BitConverter.ToInt32(randomBytes, 0);
            tox.SetNospam(nospam);

            if (nospam != tox.GetNospam())
                Assert.Fail("Failed to set/get nospam correctly, values don't match");

            tox.Dispose();
        }

        [Test]
        public void TestToxId()
        {
            var tox = new Tox(ToxOptions.Default);
            var toxId = new ToxId(tox.Id.PublicKey.GetBytes(), tox.Id.Nospam);

            if (toxId != tox.Id)
                Assert.Fail("Tox id's are not equal");
        }

        [Test]
        public void TestToxEncryption()
        {
            string password = "heythisisatest";
            byte[] garbage = new byte[0xBEEF];
            new Random().NextBytes(garbage);

            byte[] encryptedData = ToxEncryption.EncryptData(garbage, password);
            Assert.IsNotNull(encryptedData, "Failed to encrypt the data");

            byte[] decryptedData = ToxEncryption.DecryptData(encryptedData, password);
            Assert.IsNotNull(decryptedData, "Failed to decrypt the data");

            if (!garbage.SequenceEqual(decryptedData))
                Assert.Fail("Original data is not equal to the decrypted data");
        }

        [Test]
        public void TestToxEncryptionLoad()
        {
            var tox1 = new Tox(ToxOptions.Default);
            tox1.Name = "Test";
            tox1.StatusMessage = "Hey";

            string password = "heythisisatest";
            var data = tox1.GetData(password);

            Assert.IsNotNull(data, "Failed to encrypt the Tox data");
            Assert.IsTrue(data.IsEncrypted, "We encrypted the data, but toxencryptsave thinks we didn't");

            var tox2 = new Tox(ToxOptions.Default, ToxData.FromBytes(data.Bytes), password);

            if (tox2.Id != tox1.Id)
                Assert.Fail("Failed to load tox data correctly, tox id's don't match");

            if (tox2.Name != tox1.Name)
                Assert.Fail("Failed to load tox data correctly, names don't match");

            if (tox2.StatusMessage != tox1.StatusMessage)
                Assert.Fail("Failed to load tox data correctly, status messages don't match");

            tox1.Dispose();
            tox2.Dispose();
        }

        [Test, Timeout(120000), Ignore("requires a running socks proxy")]
        public void TestToxProxySocks5()
        {
            var options = new ToxOptions(true, ToxProxyType.Socks5, "127.0.0.1", 9050);
            var tox = new Tox(options);
            var error = ToxErrorBootstrap.Ok;

            foreach (var node in Globals.TcpRelays)
            {
                bool result = tox.AddTcpRelay(node, out error);
                if (!result || error != ToxErrorBootstrap.Ok)
                    Assert.Fail("Failed to bootstrap, error: {0}, result: {1}", error, result);
            }

            tox.Start();
            while (!tox.IsConnected) { Thread.Sleep(10); }

            Console.WriteLine("Tox connected!");
            tox.Dispose();
        }

        [Test]
        public void TestToxFriendRequest()
        {
            var options = new ToxOptions(true, true);
            var tox1 = new Tox(options);
            var tox2 = new Tox(options);
            var error = ToxErrorFriendAdd.Ok;
            string message = "Hey, this is a test friend request.";
            bool testFinished = false;

            tox1.AddFriend(tox2.Id, message, out error);
            if (error != ToxErrorFriendAdd.Ok)
                Assert.Fail("Failed to add friend: {0}", error);

            tox2.OnFriendRequestReceived += (sender, args) =>
            {
                if (args.Message != message)
                    Assert.Fail("Message received in the friend request is not the same as the one that was sent");

                tox2.AddFriendNoRequest(args.PublicKey, out error);
                if (error != ToxErrorFriendAdd.Ok)
                    Assert.Fail("Failed to add friend (no request): {0}", error);

                if (!tox2.FriendExists(0))
                    Assert.Fail("Friend doesn't exist according to core");

                testFinished = true;
            };

            while (!testFinished && tox1.GetFriendConnectionStatus(0) == ToxConnectionStatus.None)
            {
                int time1 = tox1.Iterate();
                int time2 = tox2.Iterate();

                Thread.Sleep(Math.Min(time1, time2));
            }

            tox1.Dispose();
            tox2.Dispose();
        }

        [Test]
        public void TestToxDataParsing()
        {
            var tox = new Tox(ToxOptions.Default);
            tox.Name = "Test";
            tox.StatusMessage = "Status";
            tox.Status = ToxUserStatus.Away;

            var data = tox.GetData();
            ToxDataInfo info = null;

            if (data == null || !data.TryParse(out info))
                Assert.Fail("Parsing the data file failed");

            if (info.Id != tox.Id || info.Name != tox.Name || info.SecretKey != tox.GetPrivateKey() || info.Status != tox.Status || info.StatusMessage != tox.StatusMessage)
                Assert.Fail("Parsing the data file failed");

            tox.Dispose();
        }

        [Test]
        public void TestToxIsDataEncrypted()
        {
            var tox = new Tox(ToxOptions.Default);
            var data = tox.GetData();

            Assert.IsFalse(data.IsEncrypted);
        }
    }
}
