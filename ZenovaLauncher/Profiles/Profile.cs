using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenovaLauncher
{
    class Profile
    {
        private MinecraftVersion _version;
        
        public Profile(string name, MinecraftVersion version)
        {
            ProfileName = name;
            _version = version;
        }

        public string ProfileName { get; set; }

        public string VersionName
        {
            get { return _version.Name; }
        }
    }

    class Profiles : ObservableCollection<Profile>
    {
        public Profiles()
        {
            Add(new Profile("Latest release", new MinecraftVersion("1.14.30.2", "uuid", false)));
            Add(new Profile("Latest beta", new MinecraftVersion("1.16.0.55", "uuid", true)));
        }
    }
}
