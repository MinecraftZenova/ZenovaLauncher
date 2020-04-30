using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace ZenovaLauncher
{
    public class ProfileManager : ObservableCollection<Profile>
    {
        public static ProfileManager instance;
        private static JsonSerializerSettings jsonSettings;

        private readonly string _profilesFile = "profiles.json";

        public Action Refresh { get; set; }
        public string ProfilesDir { get; }
        public ProfileSelected SelectedProfile { get; set; }
        public Dictionary<string, Profile> internalDictionary => this.ToDictionary(x => x.Hash, x => x);

        public ProfileManager(string profileDir)
        {
            ProfilesDir = profileDir;
            SelectedProfile = new ProfileSelected();
            jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public Profile LatestRelease
        {
            get
            {
                return this.SingleOrDefault(p => p.Type == Profile.ProfileType.LatestRelease);
            }
            set
            {
                Profile ret = this.SingleOrDefault(p => p.Type == Profile.ProfileType.LatestRelease);
                if (ret == null)
                    Add(value);
            }
        }

        public Profile LatestBeta
        {
            get
            {
                return this.SingleOrDefault(p => p.Type == Profile.ProfileType.LatestBeta);
            }
            set
            {
                Profile ret = this.SingleOrDefault(p => p.Type == Profile.ProfileType.LatestBeta);
                if (ret == null)
                    Add(value);
            }
        }

        public void AddProfiles(List<Profile> profiles)
        {
            foreach (Profile profile in profiles)
            {
                if (!internalDictionary.ContainsKey(profile.Hash))
                {
                    switch (profile.Type)
                    {
                        case Profile.ProfileType.Custom:
                            Add(profile);
                            break;
                        case Profile.ProfileType.LatestBeta:
                            LatestBeta = profile;
                            break;
                        case Profile.ProfileType.LatestRelease:
                            LatestRelease = profile;
                            break;
                    }
                }
            }
            Refresh?.Invoke();
        }

        public void ImportProfiles()
        {
            if (File.Exists(Path.Combine(ProfilesDir, _profilesFile)))
                AddProfiles(LoadProfiles(File.ReadAllText(Path.Combine(ProfilesDir, _profilesFile))));
            AddDefaultProfiles();
        }

        public void AddDefaultProfiles()
        {
            LatestRelease = new Profile("Latest release", VersionManager.instance.LatestRelease, type: Profile.ProfileType.LatestRelease);
            LatestBeta = new Profile("Latest beta", VersionManager.instance.LatestBeta, type: Profile.ProfileType.LatestBeta);
            SelectedProfile.SelectedProfile = this.First();
        }

        public List<Profile> LoadProfiles(string profileText)
        {
            try
            {
                Dictionary<string, Profile> profileList = JsonConvert.DeserializeObject<Dictionary<string, Profile>>(profileText, jsonSettings);
                return profileList.Values.ToList();
            }
            catch (Exception e)
            {
                Trace.WriteLine("Profile JSON Deserialize Failed: " + e.ToString());
                MessageBox.Show("Profile JSON Deserialize Failed: " + e.ToString());
            }
            return new List<Profile>();
        }

        public void SaveProfiles()
        {
            //DirectoryInfo di = new DirectoryInfo(_profilesDir);
            //foreach (FileInfo file in di.EnumerateFiles()) file.Delete();
            //foreach (DirectoryInfo dir in di.EnumerateDirectories()) dir.Delete(true);
            File.WriteAllText(Path.Combine(ProfilesDir, _profilesFile), JsonConvert.SerializeObject(internalDictionary, Formatting.Indented, jsonSettings));
        }

        public class ProfileSelected : NotifyPropertyChangedBase
        {
            private Profile _selectedProfile;

            public Profile SelectedProfile
            {
                get { return _selectedProfile; }
                set { _selectedProfile = value; OnPropertyChanged("SelectedProfile"); }
            }
        }
    }
}
