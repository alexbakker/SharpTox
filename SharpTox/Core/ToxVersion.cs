using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpTox.Core
{
    /// <summary>
    /// Represents a version of Tox.
    /// </summary>
    public class ToxVersion
    {
        /// <summary>
        /// The major version number. Incremented when the API or ABI changes in an incompatible way.
        /// </summary>
        public int Major { get; private set; }

        /// <summary>
        /// The minor version number. Incremented when functionality is added without breaking the API or ABI. 
        /// Set to 0 when the major version number is incremented.
        /// </summary>
        public int Minor { get; private set; }

        /// <summary>
        /// The patch or revision number. Incremented when bugfixes are applied without changing any functionality or API or ABI.
        /// </summary>
        public int Patch { get; private set; }

        /// <summary>
        /// The current version of Tox. Assuming there's a libtox.dll/libtoxcore.so in our PATH.
        /// </summary>
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

        /// <summary>
        /// Checks whether or not this version is compatible with the version of Tox that we're using.
        /// </summary>
        /// <returns>True if this version is compatible, false if it's not.</returns>
        public bool IsCompatible()
        {
            return ToxFunctions.VersionIsCompatible((uint)Major, (uint)Minor, (uint)Patch);
        }

        /// <summary>
        /// Initializes a new instance of the ToxVersion class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch or revision number.</param>
        public ToxVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", Major, Minor, Patch);
        }
    }
}
