﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ZenovaLauncher
{
    public class Preferences : NotifyPropertyChangedBase
    {
        public static Preferences instance;

        private static string _preferencesFile = "preferences.json";
        private static JsonSerializerSettings jsonSettings;

        public bool EnableReleases { get; set; } = true;
        public bool EnableBetas { get; set; } = false;
        public bool EnableHistorical { get; set; } = false;
        public bool KeepLauncherOpen { get; set; } = false;
        public bool RemoveUnusedVersions { get; set; } = false;
        public bool DebugMode { get; set; } = false;
        public bool AutoUpdate { get; set; } = true;
        public bool AnyVerMods { get; set; } = false;
        [JsonConverter(typeof(StringEnumConverter))]
        public Profile.ProfileSortType ProfileSorting { get; set; } = Profile.ProfileSortType.ByLastPlayed;
        [JsonConverter(typeof(StringEnumConverter))]
        public Mod.ModSortType ModSorting { get; set; } = Mod.ModSortType.ByLatestSupported;
        [JsonIgnore]
        public int ProfileSortingId
        {
            get { return (int)ProfileSorting; }
            set { ProfileSorting = (Profile.ProfileSortType)value; }
        }
        [JsonIgnore]
        public int ModSortingId
        {
            get { return (int)ModSorting; }
            set { ModSorting = (Mod.ModSortType)value; }
        }
        public string SelectedAccount
        {
            get { return AccountManager.instance.CurrentXboxAccount.Gamertag; }
            set
            {
                var manager = AccountManager.instance;
                var current = manager.FirstOrDefault(a => a.Gamertag == value);
                if (current != null)
                {
                    manager.CurrentXboxAccount = current;
                }
                else if (manager.First() != XboxAccount._null)
                {
                    manager.CurrentXboxAccount = manager.First();
                }
                else
                {
                    manager.CurrentXboxAccount = XboxAccount._null;
                }
            }
        }
        public string SelectedProfile
        {
            get { return ProfileManager.instance.SelectedProfile.SelectedProfile.Hash; }
            set {
                var selected = ProfileManager.instance.FirstOrDefault(p => p.Hash == value);
                ProfileManager.instance.SelectedProfile.SelectedProfile = selected ?? ProfileManager.instance.LatestRelease;
            }
        }


        public static void LoadPreferences()
        {
            Trace.WriteLine("Loading Preferences");
            _preferencesFile = Path.Combine(App.DataDirectory, _preferencesFile);
            jsonSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            if (File.Exists(_preferencesFile))
            {
                try
                {
                    instance = JsonConvert.DeserializeObject<Preferences>(File.ReadAllText(_preferencesFile), jsonSettings);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Preferences JSON Deserialize Failed: " + e.ToString());
                    Utils.ShowErrorDialog("Failed to load Preferences", "An error occured when loading Preferences from preferences.json. Ensure the file is formatted correctly.");
                    instance = new Preferences();
                }
            }
            else
            {
                instance = new Preferences();
            }

            Trace.WriteLine("Loaded Preferences");
        }

        public static void SavePreferences()
        {
            File.WriteAllText(_preferencesFile, JsonConvert.SerializeObject(instance, Formatting.Indented, jsonSettings));
            Utils.AddSecurityToFile(_preferencesFile);
        }
    }
}
