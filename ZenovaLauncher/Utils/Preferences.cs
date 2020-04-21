using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
        public string SelectedAccountName 
        { 
            get { return SelectedAccount.AccountName; }
            set { SelectedAccount = AccountManager.instance.FirstOrDefault(a => a.AccountName == value); }
        }
        [JsonIgnore]
        public MSAccount SelectedAccount { get; set; } = AccountManager.instance.First();


        public static void LoadPreferences(string dataDir)
        {
            Trace.WriteLine("Loading Preferences");
            _preferencesFile = Path.Combine(dataDir, _preferencesFile);
            camelCaseSerialization = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            if (File.Exists(_preferencesFile))
                instance = JsonConvert.DeserializeObject<Preferences>(File.ReadAllText(_preferencesFile), camelCaseSerialization);
            else
                instance = new Preferences();
            Trace.WriteLine("Loaded Preferences");
        }

        public static void SavePreferences()
        {
            File.WriteAllText(_preferencesFile, JsonConvert.SerializeObject(instance, Formatting.Indented, camelCaseSerialization));
        }
    }
}
