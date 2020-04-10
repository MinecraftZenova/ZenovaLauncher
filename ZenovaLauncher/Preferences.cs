using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenovaLauncher
{
    public class Preferences : NotifyPropertyChangedBase
    {
        public static Preferences instance;

        private static string _preferencesFile = "preferences.json";
        private static JsonSerializerSettings camelCaseSerialization;

        public bool EnableReleases { get; set; }
        public bool EnableBetas { get; set; }
        public bool EnableHistorical { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Profile.ProfileSortType ProfileSorting { get; set; }
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
            {
                instance = JsonConvert.DeserializeObject<Preferences>(File.ReadAllText(_preferencesFile), camelCaseSerialization);
            }
            else
            {
                instance = new Preferences();
                instance.SetDefaultPreferences();
            }
        }

        public static void SavePreferences()
        {
            File.WriteAllText(_preferencesFile, JsonConvert.SerializeObject(instance, Formatting.Indented, camelCaseSerialization));
        }

        public void SetDefaultPreferences()
        {
            EnableReleases = true;
            EnableBetas = false;
            EnableHistorical = false;
            ProfileSorting = Profile.ProfileSortType.ByLastPlayed;
        }
    }
}
