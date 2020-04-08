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

        public ProfileManager()
        {
            Add(new Profile("Latest release", new MinecraftVersion("1.14.30.2", "uuid")));
            Add(new Profile("Latest beta", new MinecraftVersion("1.16.0.55", "uuid", isBeta: true)));
        }
    }
}
