using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenovaLauncher
{
    public class ProfileManager : ObservableCollection<Profile>
    {
        public static ProfileManager instance;

        private readonly string _profilesFile = "profiles.json";
        private readonly string _profilesDir;

        public ProfileManager(string profileDir)
        {
            _profilesDir = profileDir;
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

        public void AddProfiles()
        {
            LoadProfiles();
            AddDefaultProfiles();
        }

        public void AddDefaultProfiles()
        {
            LatestRelease = new Profile("Latest release", VersionManager.instance.LatestRelease, type: Profile.ProfileType.LatestRelease);
            LatestBeta = new Profile("Latest beta", VersionManager.instance.LatestBeta, type: Profile.ProfileType.LatestBeta); ;
        }

        public void LoadProfiles()
        {
            string[] profileFiles = Directory.GetFiles(_profilesDir, "*.json", SearchOption.AllDirectories);
            foreach(string file in profileFiles)
            {
                List<Profile> profileList = JsonConvert.DeserializeObject<List<Profile>>(File.ReadAllText(file));
                foreach (Profile p in profileList)
                {
                    switch(p.Type)
                    {
                        case Profile.ProfileType.Custom:
                            Add(p);
                            break;
                        case Profile.ProfileType.LatestBeta:
                            LatestBeta = p;
                            break;
                        case Profile.ProfileType.LatestRelease:
                            LatestRelease = p;
                            break;
                    }
                }
            }
        }

        public void SaveProfiles()
        {
            DirectoryInfo di = new DirectoryInfo(_profilesDir);
            foreach (FileInfo file in di.EnumerateFiles()) file.Delete();
            foreach (DirectoryInfo dir in di.EnumerateDirectories()) dir.Delete(true);
            File.WriteAllText(Path.Combine(_profilesDir, _profilesFile), JsonConvert.SerializeObject(this));
        }
    }
}
