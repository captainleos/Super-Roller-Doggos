using System;

namespace TerraUnity.Edittime
{ 
    public class TVersionController
    {
        private static int _majorVersion = 2;
        public static int MajorVersion { get => _majorVersion; }

        private static int _minorVersion = 303;
        public static int MinorVersion { get => _minorVersion; }
        public static int Version { get { return _majorVersion * 1000 + _minorVersion; } }

        public static string VersionStr
        {
            get
            {
                string version = "";
                float mv = _minorVersion;
                if (mv < 100 && mv > 9) mv = mv * 10;
                if (mv < 10) mv = mv * 100;

                if (Math.Round((mv / 10)) == (mv / 10f))
                {
                    mv = mv / 10f;
                    version = _majorVersion.ToString() + "." + mv + ".0";
                }
                else
                {
                    mv = mv / 10f;
                    version = _majorVersion.ToString() + "." + mv;
                }

                return version;
            }
        }
    }
}
