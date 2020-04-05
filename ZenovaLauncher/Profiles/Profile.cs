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
            Add(new Profile("This is a very long profile name that could potentially cause problems", new MinecraftVersion("1.14.30.2", "uuid", false)));
            Add(new Profile("Profile1", new MinecraftVersion("1.14.30.2", "uuid", false)));
            Add(new Profile("Profile2", new MinecraftVersion("1.14.30.2", "uuid", false)));
            Add(new Profile("Profile3", new MinecraftVersion("1.14.30.2", "uuid", false)));
            Add(new Profile("Profile4", new MinecraftVersion("1.14.30.2", "uuid", false)));
            Add(new Profile("Profile5", new MinecraftVersion("1.14.30.2", "uuid", false)));
            Add(new Profile("Profile6", new MinecraftVersion("1.14.30.2", "uuid", false)));
            Add(new Profile("Profile7", new MinecraftVersion("1.14.30.2", "uuid", false)));
            Add(new Profile("Profile8", new MinecraftVersion("1.14.30.2", "uuid", false)));
            Add(new Profile("Profile9", new MinecraftVersion("1.14.30.2", "uuid", false)));
        }
    }
}
