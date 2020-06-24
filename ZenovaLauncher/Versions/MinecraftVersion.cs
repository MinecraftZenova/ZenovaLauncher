using System;
using System.IO;

namespace ZenovaLauncher
{
    public class MinecraftVersion : NotifyPropertyChangedBase
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

        public MinecraftVersion(Version version, string uuid, bool isBeta = false)
        {
            Version = version;
            UUID = uuid;
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
        public Version Version { get; set; }
        public string UUID { get; set; }
        public bool Beta { get; set; }
        public string Name => Version.ToString();
        public bool Historical => Version.Major < 1;
        public bool Release => !Beta && !Historical;
        public bool LatestRelease => this == VersionManager.instance.LatestRelease;
        public bool LatestBeta => this == VersionManager.instance.LatestBeta;
        public string GameDirectory => Path.Combine(VersionManager.instance.VersionsDirectory, "Minecraft-" + Name);
        public bool IsInstalled => Directory.Exists(GameDirectory);
        public void UpdateInstallStatus()
        {
            OnPropertyChanged("IsInstalled");
        }

    }
}
