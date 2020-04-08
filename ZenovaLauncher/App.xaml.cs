using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public async void AppStart(object sender, StartupEventArgs e)
        {
            VersionManager.instance = new VersionManager();
            ProfileManager.instance = new ProfileManager();
            await VersionManager.instance.LoadMinecraftVersions();
            ProfileManager.instance.AddDefaultProfiles();
        }
    }
}
