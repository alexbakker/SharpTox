using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpTox.Test
{
    public class ExtendedTestClass
    {
        protected volatile string _failReason = string.Empty;
        protected volatile bool _failed = false;
        protected volatile bool _wait = true;

        /// <summary>
        /// Stores a fail reason for later use with CheckFailed()
        /// </summary>
        protected void Fail(string reason, params object[] objects)
        {
            _failed = true;
            _failReason = string.Format(reason, objects);
            _wait = false;
        }

        /// <summary>
        /// Checks whether or not Fail() was called, if so, Assert.Fail is called with the saved message.
        /// Can only be called from the test's main thread.
        /// </summary>
        protected void CheckFailed()
        {
            if (_failed)
                Assert.Fail(_failReason);
        }
    }
}
