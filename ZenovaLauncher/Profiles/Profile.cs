using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Diagnostics;

namespace ZenovaLauncher
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Profile : NotifyPropertyChangedBase
    {
        public static Predicate<object> releaseFilter = (object item) =>
        {
            return (item as Profile).Release == true;
        };
        public static Predicate<object> betaFilter = (object item) =>
        {
            return (item as Profile).Beta == true;
        };
        public static Predicate<object> historicalFilter = (object item) =>
        {
            return (item as Profile).Historical == true;
        };

        public Profile(string name, MinecraftVersion version, DateTime lastUsed = default, DateTime created = default, ProfileType type = ProfileType.Custom)
        {
            Name = string.IsNullOrEmpty(name) ? "<unnamed profile>" : name;
            Version = version;
            LastUsed = lastUsed;
            Created = created;
            Type = type;
        }

        public Profile(Profile profile) : this(profile.Name, profile.Version, profile.LastUsed, profile.Created, profile.Type) { }

        [JsonConstructor]
        public Profile(DateTime created, DateTime lastUsed, string versionId, string name, ProfileType type) :
            this(name, VersionManager.instance.GetVersionFromString(versionId), lastUsed, created, type)
        { }

        [JsonProperty]
        public DateTime Created { get; set; }
        [JsonProperty]
        public DateTime LastUsed { get; set; }
        [JsonProperty]
        public string VersionId => Version.InternalName;
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public ProfileType Type { get; set; }
        public MinecraftVersion Version { get; set; }
        public string Hash => Utils.ComputeHash(this);
        public string VersionName => Version.Name;
        public bool Beta => Version.Beta;
        public bool Historical => Version.Historical;
        public bool Release => Version.Release;
        public bool Editable => Type == ProfileType.Custom;
        public bool Launching => ProfileLauncher.instance.LaunchedProfile == this;

        public void UpdateLaunchStatus()
        {
            OnPropertyChanged("Launching");
        }

        public enum ProfileType
        {
            Custom,
            LatestRelease,
            LatestBeta
        }

        public enum ProfileSortType
        {
            ByLastPlayed,
            ByName
        }
    }
}
