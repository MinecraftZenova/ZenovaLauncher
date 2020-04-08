using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenovaLauncher
{
    public class MinecraftVersion
    {
        private string _uuid;

        public MinecraftVersion(Version version, string uuid, bool isBeta = false, bool isHistorical = false)
        {
            Version = version;
            _uuid = uuid;
            Beta = isBeta;
            Historical = isHistorical;
        }
        public int SortOrder
        {
            get
            {
                if (this == VersionManager.instance.LatestRelease)
                    return 0;
                else if (this == VersionManager.instance.LatestBeta)
                    return 1;
                return 2;
            }
        }
        public string FullName
        {
            get
            {
                if (this == VersionManager.instance.LatestRelease)
                    return "Latest release (" + Name + ")";
                else if (this == VersionManager.instance.LatestBeta)
                    return "Latest beta (" + Name + ")";
                return (Release ? "release " : (Beta ? "beta " : "old_beta ")) + Name;
            }
        }
        public string Name { get { return Version.ToString(); } }
        public Version Version { get; set; }
        public bool Beta { get; set; }
        public bool Historical { get; set; }
        public bool Release { get { return !Beta && !Historical; } }
    }
}
