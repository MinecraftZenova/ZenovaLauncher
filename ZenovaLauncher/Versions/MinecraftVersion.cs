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

        private static readonly string MINECRAFT_PACKAGE_FAMILY = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";
        private static readonly string MINECRAFT_PREVIEW_PACKAGE_FAMILY = "Microsoft.MinecraftWindowsBeta_8wekyb3d8bbwe";

        public MinecraftVersion(Version version, string uuid, VersionType versionType)
        {
            Version = version;
            UUID = uuid;
            Type = versionType;
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
                return (Release ? "release " : (Preview ? "preview " : (Beta ? "beta " : "old_beta "))) + Name;
            }
        }
        public Version Version { get; set; }
        public VersionType Type { get; set; }
        public string UUID { get; set; }
        public string Name => Version.ToString();
        public bool Beta => Type == VersionType.Beta || Type == VersionType.Preview;
        public bool Preview => Type == VersionType.Preview;
        public bool Historical => Version.Major < 1;
        public bool Release => Type == VersionType.Release;
        public bool LatestRelease => this == VersionManager.instance.LatestRelease;
        public bool LatestBeta => this == VersionManager.instance.LatestBeta;
        public string VersionName => (Preview ? "MinecraftPreview-" : "Minecraft-") + Name;
        public string GameDirectory => Path.Combine(VersionManager.instance.VersionsDirectory, VersionName);
        public string PackageFamily => Preview ? MINECRAFT_PREVIEW_PACKAGE_FAMILY : MINECRAFT_PACKAGE_FAMILY;
        public bool IsInstalled => Directory.Exists(GameDirectory);
        public bool ModSupported
        {
            get
            {
                // probably should be changed to a list but not gonna bother till we support more versions
                var SupportedVersions = new System.Version(1, 14, 60, 5);

                if (Beta || Version != SupportedVersions)
                {
                    return false;
                }

                return true;
            }
        }

        public void UpdateInstallStatus()
        {
            OnPropertyChanged("IsInstalled");
        }

        public enum VersionType
        {
            Release = 0,
            Beta = 1,
            Preview = 2,
            Imported = 100
        }

    }
}
