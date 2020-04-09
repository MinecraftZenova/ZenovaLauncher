﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            AddDefaultProfiles();
        }

        public void AddDefaultProfiles()
        {
            Add(new Profile("Latest release", VersionManager.instance.LatestRelease, type: Profile.ProfileType.LatestRelease));
            Add(new Profile("Latest beta", VersionManager.instance.LatestBeta, type: Profile.ProfileType.LatestBeta));
        }

        public void SaveProfiles()
        {

        }
    }
}
