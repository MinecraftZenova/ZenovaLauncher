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

        public Profile(string name, MinecraftVersion version, DateTime lastPlayed = default)
        {
            ProfileName = name;
            _version = version;
            LastPlayed = lastPlayed;
        }

        public string ProfileName { get; set; }
        public DateTime LastPlayed { get; set; }
        public string VersionName { get { return _version.Name; } }
        public bool Beta { get { return _version.Beta; } }
        public bool Historical { get { return _version.Historical; } }
        public bool Release { get { return _version.Release; } }

    }

    class Profiles : ObservableCollection<Profile>
    {
        public Profiles()
        {
            Add(new Profile("Latest release", new MinecraftVersion("1.14.30.2", "uuid", false, false), new DateTime(2019, 10, 2)));
            Add(new Profile("Latest beta", new MinecraftVersion("1.16.0.55", "uuid", true, false), new DateTime(2020, 1, 20)));
            Add(new Profile("This is a very long profile name that could potentially cause problems", new MinecraftVersion("1.14.30.2", "uuid", false, false)));
            Add(new Profile("Profile1", new MinecraftVersion("1.14.30.2", "uuid", false, false), new DateTime(2019, 11, 2)));
            Add(new Profile("Profile2", new MinecraftVersion("1.14.30.2", "uuid", false, false), new DateTime(2019, 1, 2)));
            Add(new Profile("Profile3", new MinecraftVersion("1.14.30.2", "uuid", false, false), new DateTime(2019, 3, 2)));
            Add(new Profile("Profile4", new MinecraftVersion("1.14.30.2", "uuid", false, false), new DateTime(2019, 10, 5)));
            Add(new Profile("Profile5", new MinecraftVersion("1.14.30.2", "uuid", false, false), new DateTime(2019, 11, 12)));
            Add(new Profile("Profile6", new MinecraftVersion("1.14.30.2", "uuid", false, false), new DateTime(2019, 10, 12)));
            Add(new Profile("Profile7", new MinecraftVersion("1.14.30.2", "uuid", false, false), new DateTime(2019, 1, 21)));
            Add(new Profile("Profile8", new MinecraftVersion("1.14.30.2", "uuid", false, false), new DateTime(2019, 10, 21)));
            Add(new Profile("Profile9", new MinecraftVersion("1.14.30.2", "uuid", false, false), new DateTime(2019, 1, 23)));
        }
    }
}
