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
        public Profile(string name, MinecraftVersion version, DateTime lastPlayed = default)
        {
            ProfileName = name;
            Version = version;
            LastPlayed = lastPlayed;
        }

        public Profile(Profile profile) : this(profile.ProfileName, profile.Version, profile.LastPlayed) { }

        public string ProfileName { get; set; }
        public DateTime LastPlayed { get; set; }
        public MinecraftVersion Version { get; set; }
        public string VersionName { get { return Version.Name; } }
        public bool Beta { get { return Version.Beta; } }
        public bool Historical { get { return Version.Historical; } }
        public bool Release { get { return Version.Release; } }
    }
}
