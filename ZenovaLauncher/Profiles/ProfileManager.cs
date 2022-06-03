using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ZenovaLauncher
{
    public class ProfileManager : ObservableCollection<Profile>
    {
        public static ProfileManager instance;
        private static JsonSerializerSettings jsonSettings;

        private readonly string _profilesFileName = "profiles.json";
        private string ProfilesFile;

        public Action Refresh { get; set; }
        public string ProfilesDir { get; }
        public string DefaultProfileDir { get; }
        public ProfileSelected SelectedProfile { get; set; }
        public Dictionary<string, Profile> InternalDictionary => this.ToDictionary(x => x.Hash, x => x);

        public ProfileManager(string profileDir)
        {
            ProfilesDir = profileDir;
            DefaultProfileDir = Path.Combine(ProfilesDir, "Default\\com.mojang\\");
            if (!Directory.Exists(DefaultProfileDir))
                Directory.CreateDirectory(DefaultProfileDir);
            ProfilesFile = Path.Combine(App.DataDirectory, _profilesFileName);
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

        public void RemoveProfile(Profile profile)
        {
            foreach (Mod m in profile.ModsList)
                m?.LinkedProfiles.Remove(profile);
            Remove(profile);
            VersionManager.instance.RemoveUnusedVersions();
        }

        public void AddProfiles(List<Profile> profiles)
        {
            foreach (Profile profile in profiles)
            {
                if (!InternalDictionary.ContainsKey(profile.Hash))
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
            if (File.Exists(ProfilesFile))
                AddProfiles(LoadProfiles(File.ReadAllText(ProfilesFile)));
            AddDefaultProfiles();
        }

        public void AddDefaultProfiles()
        {
            LatestRelease = new Profile("Latest release", VersionManager.instance.LatestRelease, "", type: Profile.ProfileType.LatestRelease);
            LatestBeta = new Profile("Latest beta", VersionManager.instance.LatestBeta, "", type: Profile.ProfileType.LatestBeta);
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
                Utils.ShowErrorDialog("Failed to load profile", "Error occured while parsing profiles.json. Make sure profiles.json is formatted correctly.");
            }
            return new List<Profile>();
        }

        public void SaveProfiles()
        {
            //DirectoryInfo di = new DirectoryInfo(_profilesDir);
            //foreach (FileInfo file in di.EnumerateFiles()) file.Delete();
            //foreach (DirectoryInfo dir in di.EnumerateDirectories()) dir.Delete(true);
            File.WriteAllText(ProfilesFile, JsonConvert.SerializeObject(InternalDictionary, Formatting.Indented, jsonSettings));
            Utils.AddSecurityToFile(ProfilesFile);
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
