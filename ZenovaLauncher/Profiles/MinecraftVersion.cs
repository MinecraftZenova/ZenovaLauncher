using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenovaLauncher
{
    public class MinecraftVersion
    {
        public static Predicate<object> releaseFilter = (object item) =>
        {
            return (item as MinecraftVersion).Release == true;
        };
        public static Predicate<object> betaFilter = (object item) =>
        {
            return (item as MinecraftVersion).Beta == true;
        };
        public static Predicate<object> historicalFilter = (object item) =>
        {
            return (item as MinecraftVersion).Historical == true;
        };

        private string _uuid;

        public MinecraftVersion(Version version, string uuid, bool isBeta = false)
        {
            Version = version;
            _uuid = uuid;
            Beta = isBeta;
        }
        public int SortOrder
        {
            get
            {
                if (LatestRelease)
                    return 0;
                if (LatestBeta)
                    return 1;
                return 2;
            }
        }
        public string FullName
        {
            get
            {
                if (LatestRelease)
                    return "Latest release (" + Name + ")";
                if (LatestBeta)
                    return "Latest beta (" + Name + ")";
                return (Release ? "release " : (Beta ? "beta " : "old_beta ")) + Name;
            }
        }
        public string InternalName
        {
            get
            {
                if (LatestRelease)
                    return "latest-release";
                if (LatestBeta)
                    return "latest-beta";
                return Name;
            }
        }
        public string Name { get { return Version.ToString(); } }
        public Version Version { get; set; }
        public bool Beta { get; set; }
        public bool Historical { get { return Version.Major < 1; } }
        public bool Release { get { return !Beta && !Historical; } }
        public bool LatestRelease { get { return this == VersionManager.instance.LatestRelease; } }
        public bool LatestBeta { get { return this == VersionManager.instance.LatestBeta; } }
    }
}
