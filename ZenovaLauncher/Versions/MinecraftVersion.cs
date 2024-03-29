﻿using System;
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

        // todo: generate these from the AppxManifest and use this for the PublisherId
        // https://gchq.github.io/CyberChef/#recipe=Encode_text('UTF-16LE%20(1200)')SHA2('256',64,160)Drop_bytes(16,48,false)From_Hex('Auto')To_Base32('0-9a-hjkmnp-tv-z')
        private static readonly string MICROSOFT_PUBLISHER_ID = "8wekyb3d8bbwe";
        private static readonly string MINECRAFT_PACKAGE_FAMILY = "Microsoft.MinecraftUWP_" + MICROSOFT_PUBLISHER_ID;
        private static readonly string MINECRAFT_PREVIEW_PACKAGE_FAMILY = "Microsoft.MinecraftWindowsBeta_" + MICROSOFT_PUBLISHER_ID;

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
                if (Preferences.instance.AnyVerMods)
                {
                    return true;
                }

                if (Beta)
                {
                    return false;
                }

                // probably should be changed to a list but not gonna bother till we support more versions
                var SupportedVersions = new Version(1, 14, 60, 5);

                if (Version != SupportedVersions)
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
            Null = -1,
            Release = 0,
            Beta = 1,
            Preview = 2,
            Imported = 100
        }

    }
}
