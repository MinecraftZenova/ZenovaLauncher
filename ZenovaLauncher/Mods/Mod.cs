using Newtonsoft.Json;
using System;

namespace ZenovaLauncher
{
    public class Mod
    {
        public string NameId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Version Version { get; set; }
        public string MinVersion
        {
            get { return MinMCVersion.InternalName; }
            set { MinMCVersion = VersionManager.instance.GetVersionFromString(value); }
        }
        public string MaxVersion
        {
            get { return MaxMCVersion.InternalName; }
            set { MaxMCVersion = VersionManager.instance.GetVersionFromString(value); }
        }
        [JsonIgnore]
        public string ModDirectory { get; set; }
        [JsonIgnore]
        public MinecraftVersion MinMCVersion { get; set; }
        [JsonIgnore]
        public MinecraftVersion MaxMCVersion { get; set; }
        [JsonIgnore]
        public Version LatestSupported => MaxMCVersion.Version;

        public enum ModSortType
        {
            ByLatestSupported,
            ByName
        }
    }
}
