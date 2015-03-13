using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SharpTox.Core;

namespace SharpTox.Test
{
    [TestClass]
    public class CoreSelfTests
    {
        [TestMethod]
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
    }
}
