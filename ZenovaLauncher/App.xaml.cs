using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private readonly string _environmentKey = "ZENOVA_DATA";
        private readonly string _directoryMods = "mods";
        private readonly string _directoryVersions = "versions";
        private string _dataDirectory;

        private const string AppID = "ZenovaApplicationID";

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(AppID))
            {
                var application = new App();
                application.InitializeComponent();
                application.Run();
                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // handle command line arguments of second instance
            ReadCommandArgs(args);
            return true;
        }

        public void ReadCommandArgs(IList<string> args)
        {
            if (args.Count > 1)
            {
                if (args[1].EndsWith(".zmp"))
                {
                    ModManager.instance.ImportMod(args[1]);
                }
            }
        }

        public void AppStart(object sender, StartupEventArgs e)
        {
            SetupEnvironment();
            Trace.Listeners.Add(new TextWriterTraceListener(new FileStream(Path.Combine(DataDirectory, "log.txt"), FileMode.Create)));
            Trace.AutoFlush = true;
            Trace.WriteLine("AppStart");
            VersionDownloader.standard = new VersionDownloader();
            VersionDownloader.user = new VersionDownloader();
            VersionManager.instance = new VersionManager(VersionsDirectory);
            ProfileManager.instance = new ProfileManager(DataDirectory);
            ProfileLauncher.instance = new ProfileLauncher();
            AccountManager.instance = new AccountManager();
            ModManager.instance = new ModManager(ModsDirectory);
            Task loadTask = Task.Run(async () =>
            {
                await AccountManager.instance.AddAccounts();
                await VersionManager.instance.LoadMinecraftVersions();
                ModManager.instance.LoadMods();
                ProfileManager.instance.AddProfiles();
                Preferences.LoadPreferences(DataDirectory);
                VersionManager.instance.RemoveUnusedVersions();
            });
            loadTask.Wait();
            ReadCommandArgs(Environment.GetCommandLineArgs());
            Trace.WriteLine("AppStart Finished");
        }

        public void AppExit(object sender, ExitEventArgs e)
        {
            ProfileManager.instance.SaveProfiles();
            Preferences.SavePreferences();
            Trace.Flush();
        }

        private void SetupEnvironment()
        {
            // check user and then machine environments to see if ZENOVA_DATA is present
            string value = Environment.GetEnvironmentVariable(_environmentKey, EnvironmentVariableTarget.User);
            if (value != null)
            {
                _dataDirectory = value;
            }
            else
            {
                value = Environment.GetEnvironmentVariable(_environmentKey, EnvironmentVariableTarget.Machine);
                if (value != null)
                {
                    _dataDirectory = value;
                }
                else
                {
                    // if not, create user environment variable at default location
                    Environment.SetEnvironmentVariable(_environmentKey, DataDirectory, EnvironmentVariableTarget.User);
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
