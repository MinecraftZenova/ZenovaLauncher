using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

        public Profile(string name, MinecraftVersion version, DateTime lastUsed = default, DateTime created = default, ProfileType type = ProfileType.Custom, ObservableCollection<Mod> modsList = null)
        {
            Name = string.IsNullOrEmpty(name) ? "<unnamed profile>" : name;
            Version = version;
            LastUsed = lastUsed;
            Created = created;
            Type = type;
            ModsList = modsList;
        }

        public Profile(Profile profile) : this(profile.Name, profile.Version, profile.LastUsed, profile.Created, profile.Type, profile.ModsList) { }

        public Profile() { }

        [JsonProperty]
        public DateTime Created { get; set; }
        [JsonProperty]
        public DateTime LastUsed { get; set; }
        [JsonProperty]
        public string VersionId
        {
            get { return Version.InternalName; }
            set { Version = VersionManager.instance.GetVersionFromString(value); }
        }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public ProfileType Type { get; set; }
        [JsonProperty]
        public List<string> Mods
        {
            get { return ModsList?.Select(m => m.ModDirectory).ToList(); }
            set { ModsList = new ObservableCollection<Mod>(value?.Select(m => ModManager.instance.GetModFromDirectory(m))); }
        }
        public ObservableCollection<Mod> ModsList { get; set; }
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
