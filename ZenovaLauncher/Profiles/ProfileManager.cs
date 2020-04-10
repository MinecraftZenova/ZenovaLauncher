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

        private readonly string _cacheFile;

        public ProfileManager(string cacheFile)
        {
            _cacheFile = cacheFile;
        }

        public void AddProfiles()
        {
            if(!LoadProfiles())
                AddDefaultProfiles();
        }

        public void AddDefaultProfiles()
        {
            Add(new Profile("Latest release", VersionManager.instance.LatestRelease, type: Profile.ProfileType.LatestRelease));
            Add(new Profile("Latest beta", VersionManager.instance.LatestBeta, type: Profile.ProfileType.LatestBeta));
        }

        public bool LoadProfiles()
        {
            if (!File.Exists(_cacheFile))
                return false;
            List<Profile> profileList = JsonConvert.DeserializeObject<List<Profile>>(File.ReadAllText(_cacheFile));
            foreach (Profile p in profileList)
                Add(p);
            return true;
        }

        public void SaveProfiles()
        {
            File.WriteAllText(_cacheFile, JsonConvert.SerializeObject(this));
        }
    }
}
