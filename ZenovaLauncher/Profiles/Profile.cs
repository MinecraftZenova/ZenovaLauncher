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

        public Profile(string name, MinecraftVersion version, DateTime lastUsed = default, DateTime created = default, ProfileType type = ProfileType.Custom, List<Mod> modsList = default)
        {
            Name = string.IsNullOrEmpty(name) ? "<unnamed profile>" : name;
            Version = version;
            LastUsed = lastUsed;
            Created = created;
            Type = type;
            modsList?.ForEach(m => AddMod(m));
        }

        public Profile(Profile profile) : this(profile.Name, profile.Version, profile.LastUsed, profile.Created, profile.Type, profile.ModsList.ToList()) { }

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
            get { return ModsList.Count > 0 ? ModsList.Select(m => m?.ModDirectory).ToList() : null; }
            set { value?.ForEach(m => AddMod(ModManager.instance.GetModFromDirectory(m))); }
        }
        public ObservableCollection<Mod> ModsList { get; set; } = new ObservableCollection<Mod>();
        public MinecraftVersion Version { get; set; }
        public string Hash => Utils.ComputeHash(this);
        public string VersionName => Version.Name;
        public bool Beta => Version.Beta;
        public bool Historical => Version.Historical;
        public bool Release => Version.Release;
        public bool Editable => Type == ProfileType.Custom;
        public bool Launching => ProfileLauncher.instance.LaunchedProfile == this;
        public bool Modded => ModsList != null ? ModsList.Count > 0 : false;

        public void UpdateLaunchStatus()
        {
            OnPropertyChanged("Launching");
        }

        public void AddMod(Mod m)
        {
            if (m != null)
            {
                ModsList.Add(m);
                m.LinkedProfiles.Add(this);
            }
        }

        public void RemoveMod(Mod m)
        {
            if (m != null)
            {
                ModsList.Remove(m);
                m.LinkedProfiles.Remove(this);
            }
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
