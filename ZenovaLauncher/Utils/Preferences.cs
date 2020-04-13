using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.IO;

namespace ZenovaLauncher
{
    public class Preferences : NotifyPropertyChangedBase
    {
        public static Preferences instance;

        private static string _preferencesFile = "preferences.json";
        private static JsonSerializerSettings camelCaseSerialization;

        public bool EnableReleases { get; set; } = true;
        public bool EnableBetas { get; set; } = false;
        public bool EnableHistorical { get; set; } = false;
        [JsonConverter(typeof(StringEnumConverter))]
        public Profile.ProfileSortType ProfileSorting { get; set; } = Profile.ProfileSortType.ByLastPlayed;
        [JsonIgnore]
        public int ProfileSortingId
        {
            get { return (int)ProfileSorting; }
            set { ProfileSorting = (Profile.ProfileSortType)value; }
        }


        public static void LoadPreferences(string dataDir)
        {
            _preferencesFile = Path.Combine(dataDir, _preferencesFile);
            camelCaseSerialization = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            if (File.Exists(_preferencesFile))
                instance = JsonConvert.DeserializeObject<Preferences>(File.ReadAllText(_preferencesFile), camelCaseSerialization);
            else
                instance = new Preferences();
        }

        public static void SavePreferences()
        {
            File.WriteAllText(_preferencesFile, JsonConvert.SerializeObject(instance, Formatting.Indented, camelCaseSerialization));
        }
    }
}
