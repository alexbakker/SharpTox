using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpTox.Core
{
    public class ToxVersion
    {
        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Patch { get; private set; }

        public static ToxVersion Current
        {
            get
            {
                return new ToxVersion(
                    (int)ToxFunctions.VersionMajor(),
                    (int)ToxFunctions.VersionMinor(),
                    (int)ToxFunctions.VersionPatch());
            }
        }

        public bool IsCompatible()
        {
            return ToxFunctions.VersionIsCompatible((uint)Major, (uint)Minor, (uint)Patch);
        }

        public ToxVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }
    }
}
