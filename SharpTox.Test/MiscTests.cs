using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SharpTox.Core;

namespace SharpTox.Test
{
    [TestClass]
    public class MiscTests
    {
        private ToxOptions _options = new ToxOptions(true, true);

        [TestMethod]
        public void TestToxBootstrapAndConnect()
        {
            var tox = new Tox(_options);
            var error = ToxErrorBootstrap.Ok;

            foreach (var node in Globals.Nodes)
            {
                bool result = tox.Bootstrap(node, out error);
                if (!result || error != ToxErrorBootstrap.Ok)
                    Assert.Fail("Failed to bootstrap error: {0}, result: {1}", error, result);
            }

            tox.Start();
            while (!tox.IsConnected) { }

            Console.WriteLine("Tox connected!");
            tox.Dispose();
        }
    }
}
