using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
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
        private readonly string _environmentKey = "ZENOVA_DATA";
        private readonly string _directoryMods = "Mods";
        private readonly string _directoryProfiles = "Profiles";
        private readonly string _directoryVersions = "Versions";
        private string _dataDirectory;
        private EnvironmentVariableTarget _environmentType;


        public void AppStart(object sender, StartupEventArgs e)
        {
            SetupEnvironment();
            VersionManager.instance = new VersionManager(Path.Combine(VersionsDirectory, "versions.json"));
            ProfileManager.instance = new ProfileManager(ProfilesDirectory);
            Preferences.LoadPreferences(DataDirectory);
            Dispatcher.Invoke(async () =>
            {
                await VersionManager.instance.LoadMinecraftVersions();
                ProfileManager.instance.AddProfiles();
            });
        }

        public void AppExit(object sender, ExitEventArgs e)
        {
            ProfileManager.instance.SaveProfiles();
            Preferences.SavePreferences();
        }

        private void SetupEnvironment()
        {
            // check user and then machine environments to see if ZENOVA_DATA is present
            string value = Environment.GetEnvironmentVariable(_environmentKey, EnvironmentVariableTarget.User);
            if (value != null)
            {
                _dataDirectory = value;
                _environmentType = EnvironmentVariableTarget.User;
            }
            else
            {
                value = Environment.GetEnvironmentVariable(_environmentKey, EnvironmentVariableTarget.Machine);
                if (value != null)
                {
                    _dataDirectory = value;
                    _environmentType = EnvironmentVariableTarget.Machine;
                }
                else
                {
                    // if not, create user environment variable at default location
                    Environment.SetEnvironmentVariable(_environmentKey, DataDirectory, EnvironmentVariableTarget.User);
                    _environmentType = EnvironmentVariableTarget.User;
                }
            }
        }

        public string DataDirectory
        {
            get
            {
                if (_dataDirectory == null)
                    _dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Zenova");
                return _dataDirectory;
            }
        }

        public string ModsDirectory
        {
            get
            {
                string path = Path.Combine(DataDirectory, _directoryMods);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        public string ProfilesDirectory
        {
            get
            {
                string path = Path.Combine(DataDirectory, _directoryProfiles);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        public string VersionsDirectory
        {
            get
            {
                string path = Path.Combine(DataDirectory, _directoryVersions);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }
    }
}
