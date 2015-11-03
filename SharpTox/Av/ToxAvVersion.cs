using SharpTox.Core;

namespace SharpTox.Av
{
    /// <summary>
    /// Represents a version of ToxAv.
    /// </summary>
    public class ToxAvVersion
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
        /// The current version of Tox. Assuming there's a libtox.dll/libtoxav.so in our PATH.
        /// </summary>
        public static ToxAvVersion Current
        {
            get
            {
                return new ToxAvVersion(
                    (int)ToxAvFunctions.VersionMajor(),
                    (int)ToxAvFunctions.VersionMinor(),
                    (int)ToxAvFunctions.VersionPatch());
            }
        }

        /// <summary>
        /// Checks whether or not this version is compatible with the version of ToxAv that we're using.
        /// </summary>
        /// <returns>True if this version is compatible, false if it's not.</returns>
        public bool IsCompatible()
        {
            return ToxAvFunctions.VersionIsCompatible((uint)Major, (uint)Minor, (uint)Patch);
        }

        /// <summary>
        /// Initializes a new instance of the ToxAvVersion class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch or revision number.</param>
        public ToxAvVersion(int major, int minor, int patch)
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
