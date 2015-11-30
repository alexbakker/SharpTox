using System;
using SharpTox.Core;
using System.IO;

namespace SharpTox.HL
{
    public class ToxHL
    {
        public readonly ToxOptions Options;
        internal readonly Tox Core;

        public ToxHL(ToxOptions options)
        {
            Core = new Tox(options);
            Options = options;
        }
    }
}
