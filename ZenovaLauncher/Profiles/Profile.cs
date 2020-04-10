using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenovaLauncher
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Profile
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

        public Profile(string name, MinecraftVersion version, DateTime lastPlayed = default, ProfileType type = ProfileType.Custom)
        {
            Name = string.IsNullOrEmpty(name) ? "<unnamed profile>" : name;
            Version = version;
            LastPlayed = lastPlayed;
            Type = type;
        }

        public Profile(Profile profile) : this(profile.Name, profile.Version, profile.LastPlayed) { }

        [JsonConstructor]
        public Profile(string name, DateTime lastPlayed, ProfileType type, string versionId) : 
            this(name, VersionManager.instance.GetVersionFromString(versionId), lastPlayed, type) { }

        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public DateTime LastPlayed { get; set; }
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public ProfileType Type { get; set; }
        public MinecraftVersion Version { get; set; }
        [JsonProperty]
        public string VersionId { get { return Version.InternalName; } }
        public string VersionName { get { return Version.Name; } }
        public bool Beta { get { return Version.Beta; } }
        public bool Historical { get { return Version.Historical; } }
        public bool Release { get { return Version.Release; } }
        public bool Editable { get { return Type == ProfileType.Custom; } }

        public enum ProfileType
        {
            Custom,
            LatestRelease,
            LatestBeta
        }
    }
}
