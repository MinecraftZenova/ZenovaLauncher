using Microsoft.Shell;
using System;
using System.Collections.Generic;
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
    public partial class App : Application, ISingleInstanceApp
    {
        private readonly string _environmentKey = "ZENOVA_DATA";
        private readonly string _directoryMods = "mods";
        private readonly string _directoryVersions = "versions";
        private string _dataDirectory;

        private const string AppID = "ZenovaApplicationID";

        public static Stopwatch sw;
        private SplashScreen splash;

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(AppID))
            {
                var application = new App();
                application.splash = new SplashScreen("Assets/zenova_splash.png");
                application.splash.Show(false);
                application.InitializeComponent();
                application.Run();
                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // handle command line arguments of second instance
            MainWindow.Activate();
            ReadCommandArgs(args);
            return true;
        }

        public void ReadCommandArgs(IList<string> args)
        {
            if (args.Count > 1)
            {
                if (args[1] == "/delete" && args.Count > 2)
                {
                    ZenovaUpdater.instance.DeleteInstaller(args[2]);
                }
                if (args[1].EndsWith(".zmp"))
                {
                    ModManager.instance.TryImportMods(new List<string>() { args[1] });
                }
            }
        }

        public void AppStart(object sender, StartupEventArgs e)
        {
            List<ZenovaUpdater.AssemblyType> availableUpdates = new List<ZenovaUpdater.AssemblyType>();
            Task startTask = Task.Run(async () =>
            {
                sw = Stopwatch.StartNew();
                Trace.WriteLine("AppStart " + sw.ElapsedMilliseconds + " ms");
                SetupEnvironment();
                Trace.Listeners.Add(new TextWriterTraceListener(new FileStream(Path.Combine(DataDirectory, "log.txt"), FileMode.Create)));
                Trace.AutoFlush = true;
                ZenovaUpdater.instance = new ZenovaUpdater(DataDirectory);
                Trace.WriteLine("ZenovaUpdater.instance " + sw.ElapsedMilliseconds + " ms");
                VersionDownloader.standard = new VersionDownloader();
                Trace.WriteLine("VersionDownloader.standard " + sw.ElapsedMilliseconds + " ms");
                VersionDownloader.user = new VersionDownloader();
                Trace.WriteLine("VersionDownloader.user " + sw.ElapsedMilliseconds + " ms");
                VersionManager.instance = new VersionManager(VersionsDirectory);
                Trace.WriteLine("VersionManager.instance " + sw.ElapsedMilliseconds + " ms");
                ProfileManager.instance = new ProfileManager(DataDirectory);
                Trace.WriteLine("ProfileManager.instance " + sw.ElapsedMilliseconds + " ms");
                ProfileLauncher.instance = new ProfileLauncher();
                Trace.WriteLine("ProfileLauncher.instance " + sw.ElapsedMilliseconds + " ms");
                AccountManager.instance = new AccountManager();
                Trace.WriteLine("AccountManager.instance " + sw.ElapsedMilliseconds + " ms");
                ModManager.instance = new ModManager(ModsDirectory);
                Trace.WriteLine("ModManager.instance " + sw.ElapsedMilliseconds + " ms");
                await AccountManager.instance.AddAccounts();
                Trace.WriteLine("AccountManager.AddAccounts " + sw.ElapsedMilliseconds + " ms");
                await VersionManager.instance.LoadMinecraftVersions();
                Trace.WriteLine("VersionManager.LoadMinecraftVersions " + sw.ElapsedMilliseconds + " ms");
                ModManager.instance.LoadMods();
                Trace.WriteLine("ModManager.LoadMods " + sw.ElapsedMilliseconds + " ms");
                ProfileManager.instance.ImportProfiles();
                Trace.WriteLine("ProfileManager.ImportProfiles " + sw.ElapsedMilliseconds + " ms");
                Preferences.LoadPreferences(DataDirectory);
                Trace.WriteLine("Preferences.LoadPreferences " + sw.ElapsedMilliseconds + " ms");
                VersionManager.instance.RemoveUnusedVersions();
                Trace.WriteLine("VersionManager.RemoveUnusedVersions " + sw.ElapsedMilliseconds + " ms");
                availableUpdates = await ZenovaUpdater.instance.CheckUpdates();
                Trace.WriteLine("ZenovaUpdater.CheckUpdate " + sw.ElapsedMilliseconds + " ms");
            });
            startTask.Wait();
            splash.Close(TimeSpan.FromSeconds(1));
            if (availableUpdates.Count > 0 && Preferences.instance.AutoUpdate)
            {
                UpdateWindow updateWindow = new UpdateWindow(availableUpdates);
                Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                Current.MainWindow = updateWindow;
                updateWindow.Show();
            }
            else
            {
                StartMainWindow();
            }
        }

        public void StartMainWindow()
        {
            var mainWindow = new MainWindow();
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            Current.MainWindow = mainWindow;
            mainWindow.Show();
        }

        public void AppExit(object sender, ExitEventArgs e)
        {
            WriteFiles();
            Trace.Flush();
        }

        public static void WriteFiles()
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
                {
                    _dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Zenova");
                    foreach (string path in Directory.EnumerateFiles(_dataDirectory))
                        Utils.AddSecurityToFile(path);
                }
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
                Utils.AddSecurityToDirectory(path);
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
