using System;
using System.Threading;
using SharpTox.Core;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace SharpTox.Test
{
    [TestFixture]
    public class MiscTests
    {
        private ToxOptions _options = new ToxOptions(true, true);

        [Test, Timeout(30000)]
        public void TestToxBootstrapAndConnect()
        {
            var tox = new Tox(_options);
            var error = ToxErrorBootstrap.Ok;

            foreach (var node in Globals.Nodes)
            {
                bool result = tox.Bootstrap(node, out error);
                if (!result || error != ToxErrorBootstrap.Ok)
                    Assert.Fail("Failed to bootstrap, error: {0}, result: {1}", error, result);
            }

            tox.Start();
            while (!tox.IsConnected) { Thread.Sleep(10); }

            Console.WriteLine("Tox connected!");
            tox.Dispose();
        }

        [Test, Timeout(120000)]
        public void TestToxBootstrapAndConnectTcp()
        {
            var tox = new Tox(new ToxOptions(true, false));
            var error = ToxErrorBootstrap.Ok;

            foreach (var node in Globals.TcpRelays)
            {
                bool result = tox.AddTcpRelay(node, out error);
                if (!result || error != ToxErrorBootstrap.Ok)
                    Assert.Fail("Failed to bootstrap error: {0}, result: {1}", error, result);
            }

            tox.Start();
            while (!tox.IsConnected) { Thread.Sleep(10); }

            Console.WriteLine("Tox connected!");
            tox.Dispose();
        }

        [Test]
        public void TestToxHash()
        {
            byte[] data = new byte[0xBEEF];
            new Random().NextBytes(data);

            byte[] hash = ToxTools.Hash(data);
        }

        /*[TestMethod]
        public void TestToxOptions()
        {
            var error = ToxErrorOptionsNew.Ok;
            var ptr = ToxFunctions.OptionsNew(ref error);

            if (error != ToxErrorOptionsNew.Ok)
                Assert.Fail("Failed to allocate a new instance of Tox_Options, error: {0}", error);

            var options = (ToxOptions)Marshal.PtrToStructure(ptr, typeof(ToxOptions));
            if (options != ToxOptions.Default)
                Assert.Fail("Failed, result of OptionsNew does not have the same values as the default tox options");

            Marshal.StructureToPtr(options, ptr, true);
            ToxFunctions.OptionsFree(ptr);
        }*/
    }
}
