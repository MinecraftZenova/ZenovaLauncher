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
        private static JsonSerializerSettings camelCaseSerialization;

        private readonly string _profilesFile = "profiles.json";
        private readonly string _profilesDir;

        public ProfileManager(string profileDir)
        {
            _profilesDir = profileDir;
        }

        public Profile SelectedProfile { get; set; }

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

        public void AddProfiles()
        {
            LoadProfiles();
            AddDefaultProfiles();
        }

        public void AddDefaultProfiles()
        {
            LatestRelease = new Profile("Latest release", VersionManager.instance.LatestRelease, type: Profile.ProfileType.LatestRelease);
            LatestBeta = new Profile("Latest beta", VersionManager.instance.LatestBeta, type: Profile.ProfileType.LatestBeta);
            SelectedProfile = this.First();
        }

        public void LoadProfiles()
        {
            //string[] profileFiles = Directory.GetFiles(_profilesDir, "*.json", SearchOption.AllDirectories);
            //foreach (string file in profileFiles)
            //{
            camelCaseSerialization = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            if (File.Exists(Path.Combine(_profilesDir, _profilesFile)))
            {
                try
                {
                    Dictionary<string, Profile> profileList = JsonConvert.DeserializeObject<Dictionary<string, Profile>>(File.ReadAllText(Path.Combine(_profilesDir, _profilesFile)), camelCaseSerialization);
                    foreach (var p in profileList)
                    {
                        if (p.Key == p.Value.Hash)
                        {
                            switch (p.Value.Type)
                            {
                                case Profile.ProfileType.Custom:
                                    Add(p.Value);
                                    break;
                                case Profile.ProfileType.LatestBeta:
                                    LatestBeta = p.Value;
                                    break;
                                case Profile.ProfileType.LatestRelease:
                                    LatestRelease = p.Value;
                                    break;
                            }
                        }
                        else
                        {
                            Trace.WriteLine("Failed to load profile due to incorrect hash: " + p.Key);
                        }
                    }
                }
                catch(Exception e)
                {
                    Trace.WriteLine("Profile JSON Deserialize Failed: " + e.ToString());
                    MessageBox.Show("Profile JSON Deserialize Failed: " + e.ToString());
                }
            }
            //}
        }

        public void SaveProfiles()
        {
            //DirectoryInfo di = new DirectoryInfo(_profilesDir);
            //foreach (FileInfo file in di.EnumerateFiles()) file.Delete();
            //foreach (DirectoryInfo dir in di.EnumerateDirectories()) dir.Delete(true);
            Dictionary<string, Profile> profilesDic = this.ToDictionary(x => x.Hash, x => x);
            File.WriteAllText(Path.Combine(_profilesDir, _profilesFile), JsonConvert.SerializeObject(profilesDic, Formatting.Indented, camelCaseSerialization));
        }
    }
}
