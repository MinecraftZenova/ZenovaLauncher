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

        public ProfileManager() { }

        public void AddDefaultProfiles()
        {
            Add(new Profile("Latest release", VersionManager.instance.LatestRelease));
            Add(new Profile("Latest beta", VersionManager.instance.LatestBeta));
        }
    }
}
