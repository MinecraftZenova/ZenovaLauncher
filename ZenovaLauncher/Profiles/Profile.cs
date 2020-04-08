using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenovaLauncher
{
    public class Profile
    {
        private MinecraftVersion _version;

        public Profile(string name, MinecraftVersion version, DateTime lastPlayed = default)
        {
            ProfileName = name;
            _version = version;
            LastPlayed = lastPlayed;
        }

        public Profile(Profile profile) : this(profile.ProfileName, profile._version, profile.LastPlayed) { }

        public string ProfileName { get; set; }
        public DateTime LastPlayed { get; set; }
        public string VersionName { get { return _version.Name; } }
        public bool Beta { get { return _version.Beta; } }
        public bool Historical { get { return _version.Historical; } }
        public bool Release { get { return _version.Release; } }
    }
}
