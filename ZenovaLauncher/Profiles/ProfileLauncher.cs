using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Windows.Foundation;
using Windows.Management.Core;
using Windows.Management.Deployment;
using Windows.System;
using ZenovaLauncher.AppUtils;

namespace ZenovaLauncher
{
    public class ProfileLauncher : NotifyPropertyChangedBase
    {
        public static ProfileLauncher instance;

        public bool IsLaunching => LaunchInfo != null;
        public bool IsNotLaunching => !IsLaunching;

        private ProfileLaunchInfo _launchInfo;
        public ProfileLaunchInfo LaunchInfo
        {
            get { return _launchInfo; }
            set { _launchInfo = value; OnPropertyChanged("LaunchInfo"); OnPropertyChanged("IsLaunching"); OnPropertyChanged("IsNotLaunching"); }
        }

        private Profile _launchedProfile;
        public Profile LaunchedProfile
        {
            get { return _launchedProfile; }
            set
            {
                Profile p = value ?? _launchedProfile;
                _launchedProfile = value;
                OnPropertyChanged("LaunchedProfile");
                p.UpdateLaunchStatus();
            }
        }

        public void LaunchProfile(Profile p)
        {
            if (!IsLaunching)
            {
                Task.Run(async () =>
                {
                    LaunchInfo = new ProfileLaunchInfo { Status = LaunchStatus.InitializingDownload, WillDownload = false };
                    LaunchedProfile = p;
                    bool installStatus = true;

                    if (!p.Version.IsInstalled)
                    {
                        LaunchInfo.WillDownload = true;
                        installStatus = await Download(p);
                    }

                    LaunchInfo.Status = LaunchStatus.InitializingLaunch;
                    LaunchInfo.LaunchCurrent = 0;
                    bool launchStatus = false;
                    if (installStatus == true)
                        launchStatus = await Launch(p);

                    LaunchInfo = null;
                    LaunchedProfile.LastUsed = DateTime.Now;
                    LaunchedProfile = null;
                    if (launchStatus == true)
                    {
                        if (!Preferences.instance.KeepLauncherOpen)
                            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate ()
                            {
                                Application.Current.MainWindow.Close();
                            });
                    }
                });
            }
        }

        private async Task<bool> Launch(Profile p)
        {
            try
            {
                await ReRegisterPackage(p.Version);
            }
            catch (Exception e)
            {
                Trace.WriteLine("App re-register failed:\n" + e.ToString());
                Utils.ShowErrorDialog("Launch failed", "An error occured which prevented Zenova from re-registering Minecraft. Ensure that Developer Mode is enabled in Windows Settings.");
                return false;
            }

            try
            {
                LaunchInfo.Status = LaunchStatus.Launching;
                App.WriteFiles();
                var pkgs = await AppDiagnosticInfo.RequestInfoForPackageAsync(p.Version.PackageFamily);
                var pkg = pkgs.Count > 0 ? pkgs[0] : null;
                if (pkg == null)
                {
                    Utils.ShowErrorDialog("Launch failed", "An error occured which prevented Zenova from launching Minecraft. Ensure that Minecraft is installed.");
                    return false;
                }
                if (p.Modded)
                {
                    // Last attempt to get ZenovaAPI.dll the ALL_APPLICATION_PACKAGES security
                    try
                    {
                        Utils.AddSecurityToFile(Path.Combine(App.DataDirectory, "ZenovaAPI.dll"));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Utils.ShowErrorDialog("Launch failed", "An error occured which prevented Zenova from launching Minecraft. Unable to add necessary permissions to ZenovaAPI.");
                        return false;
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    AppDebugger app = new AppDebugger(Utils.FindPackages(p.Version.PackageFamily).ToList()[0].Id.FullName);
                    if (app.GetPackageExecutionState() != PACKAGE_EXECUTION_STATE.PES_UNKNOWN)
                    {
                        app.TerminateAllProcesses();
                        if (app.StatusCode != 0)
                            return false;
                    }
                    app.EnableDebugging(AppDomain.CurrentDomain.BaseDirectory + "ZenovaLoader -d " + (Preferences.instance.DebugMode ? "1" : "0"));
                    if (app.StatusCode != 0)
                        return false;
                    app.LaunchApp(pkg.AppInfo.AppUserModelId);
                    if (app.StatusCode != 0)
                        return false;
                    app.DisableDebugging();
                    if (app.StatusCode != 0)
                        return false;
                }
                else
                {
                    await pkg.LaunchAsync();
                }
                Trace.WriteLine("App launch finished!");
                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine("App launch failed:\n" + e.ToString());
                Utils.ShowErrorDialog("Launch failed", "An error occured which prevented Zenova from launching Minecraft.");
                return false;
            }
        }

        private async Task<DeploymentResult> DeploymentProgressWrapper(IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> t, LaunchStatus status)
        {
            TaskCompletionSource<int> src = new TaskCompletionSource<int>();
            t.Progress += (v, p) =>
            {
                if (LaunchInfo.Status != status)
                    LaunchInfo.Status = status;
                LaunchInfo.LaunchCurrent = p.percentage;
                Trace.WriteLine("Deployment progress: " + p.state + " " + p.percentage + "%");
            };
            t.Completed += (v, p) =>
            {
                Trace.WriteLine("Deployment done: " + p);
                src.SetResult(1);
            };
            await src.Task;
            return t.GetResults();
        }

        private string GetBackupMinecraftDataDir()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string tmpDir = Path.Combine(localAppData, "TmpMinecraftLocalState");
            return tmpDir;
        }

        private void BackupMinecraftDataForRemoval(MinecraftVersion version)
        {
            var data = ApplicationDataManager.CreateForPackageFamily(version.PackageFamily);
            string tmpDir = GetBackupMinecraftDataDir();
            if (Directory.Exists(tmpDir))
            {
                Trace.WriteLine("BackupMinecraftDataForRemoval error: " + tmpDir + " already exists");
                Process.Start("explorer.exe", tmpDir);
                MessageBox.Show("The temporary directory for backing up MC data already exists. This probably means that we failed last time backing up the data. Please back the directory up manually.");
                throw new Exception("Temporary dir exists");
            }
            Trace.WriteLine("Moving Minecraft data to: " + tmpDir);
            Directory.Move(data.LocalFolder.Path, tmpDir);
        }

        private void RestoreMove(string from, string to)
        {
            foreach (var f in Directory.EnumerateFiles(from))
            {
                string ft = Path.Combine(to, Path.GetFileName(f));
                if (File.Exists(ft))
                {
                    if (MessageBox.Show("The file " + ft + " already exists in the destination.\nDo you want to replace it? The old file will be lost otherwise.", "Restoring data directory from previous installation", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                        continue;
                    File.Delete(ft);
                }
                File.Move(f, ft);
            }
            foreach (var f in Directory.EnumerateDirectories(from))
            {
                string tp = Path.Combine(to, Path.GetFileName(f));
                if (!Directory.Exists(tp))
                {
                    if (File.Exists(tp) && MessageBox.Show("The file " + tp + " is not a directory. Do you want to remove it? The data from the old directory will be lost otherwise.", "Restoring data directory from previous installation", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                        continue;
                    Directory.CreateDirectory(tp);
                }
                RestoreMove(f, tp);
            }
        }

        private void RestoreMinecraftDataFromReinstall(MinecraftVersion version)
        {
            string tmpDir = GetBackupMinecraftDataDir();
            if (!Directory.Exists(tmpDir))
                return;
            var data = ApplicationDataManager.CreateForPackageFamily(version.PackageFamily);
            Trace.WriteLine("Moving backup Minecraft data to: " + data.LocalFolder.Path);
            RestoreMove(tmpDir, data.LocalFolder.Path);
            Directory.Delete(tmpDir, true);
        }

        private async Task ReRegisterPackage(MinecraftVersion version)
        {
            string gameDir = Path.GetFullPath(version.GameDirectory);
            DeploymentResult results;
            foreach (var pkg in Utils.FindPackages(version.PackageFamily))
            {
                try
                {
                    if (pkg.InstalledLocation.Path == gameDir)
                    {
                        Trace.WriteLine("Skipping package removal - same path: " + pkg.Id.FullName + " " + pkg.InstalledLocation.Path);
                        return;
                    }
                    Trace.WriteLine("Removing package: " + pkg.Id.FullName + " " + pkg.InstalledLocation.Path);
                }
                catch (FileNotFoundException) { } // This will throw if the InstalledLocation no longer exists. In this case, continue as normal, and remove previous package
                if (!pkg.IsDevelopmentMode)
                {
                    BackupMinecraftDataForRemoval(version);
                    results = await DeploymentProgressWrapper(new PackageManager().RemovePackageAsync(pkg.Id.FullName, 0), LaunchStatus.LaunchRemovePackage);
                    if (!string.IsNullOrEmpty(results.ErrorText))
                        throw new Exception("Unable to remove original package:\n" + results.ErrorText + "\n");
                }
                else
                {
                    Trace.WriteLine("Package is in development mode");
                    results = await DeploymentProgressWrapper(new PackageManager().RemovePackageAsync(pkg.Id.FullName, RemovalOptions.PreserveApplicationData), LaunchStatus.LaunchRemovePackage);
                    if (!string.IsNullOrEmpty(results.ErrorText))
                        throw new Exception("Unable to remove development package:\n" + results.ErrorText + "\n");
                }
                Trace.WriteLine("Removal of package done: " + pkg.Id.FullName);
                break;
            }
            Trace.WriteLine("Registering package");
            string manifestPath = Path.Combine(gameDir, "AppxManifest.xml");
            results = await DeploymentProgressWrapper(new PackageManager().RegisterPackageAsync(new Uri(manifestPath), null, DeploymentOptions.DevelopmentMode), LaunchStatus.LaunchRegisterPackage);
            if (!string.IsNullOrEmpty(results.ErrorText))
                throw new Exception("Unable to re-register package:\n" + results.ErrorText + "\n");
            Trace.WriteLine("App re-register done!");
            RestoreMinecraftDataFromReinstall(version);
        }

        private async Task<bool> Download(Profile p)
        {
            MinecraftVersion v = p.Version;
            CancellationTokenSource cancelSource = new CancellationTokenSource();
            LaunchInfo.CancelCommand = new RelayCommand((o) => cancelSource.Cancel());

            Trace.WriteLine("Download start");
            string dlPath = v.GameDirectory + ".Appx";
            VersionDownloader downloader = v.Beta ? VersionDownloader.user : VersionDownloader.standard;
            try
            {
                Trace.WriteLine("Initializing Download");
                await downloader.Download(v.UUID, "1", dlPath, (current, total) =>
                {
                    if (LaunchInfo.Status == LaunchStatus.InitializingDownload)
                    {
                        Trace.WriteLine("Actual download started");
                        LaunchInfo.Status = LaunchStatus.Downloading;
                        if (total.HasValue)
                            LaunchInfo.DownloadSize = total.Value;
                    }
                    LaunchInfo.DownloadedBytes += current;
                }, cancelSource.Token);
                Trace.WriteLine("Download complete");
            }
            catch (Exception e)
            {
                Trace.WriteLine("Download failed:\n" + e.ToString());
                if (!(e is TaskCanceledException))
                    Utils.ShowErrorDialog("Download failed", string.Format("An error occured while downloading Minecraft {0}.{1}", v.Name, v.Beta ? " Ensure the selected account is the one registered for beta versions in the Xbox Insider app." : ""));
                return false;
            }
            try
            {
                string dirPath = v.GameDirectory;
                if (Directory.Exists(dirPath))
                    Directory.Delete(dirPath, true);
                Progress<ZipProgress> progress = new Progress<ZipProgress>();
                progress.ProgressChanged += (sender, zipProgress) =>
                {
                    if (LaunchInfo.Status != LaunchStatus.Extracting)
                    {
                        Trace.WriteLine("Extraction started");
                        LaunchInfo.Status = LaunchStatus.Extracting;
                        LaunchInfo.ZipTotal = zipProgress.Total;
                    }
                    LaunchInfo.ZipProcessed = zipProgress.Processed;
                    LaunchInfo.ZipCurrentItem = zipProgress.CurrentItem;
                };
                using (ZipArchive zipFile = new ZipArchive(new FileStream(dlPath, FileMode.Open)))
                {
                    zipFile.ExtractToDirectory(dirPath, progress);
                }
                File.Delete(Path.Combine(dirPath, "AppxSignature.p7x"));
                File.Delete(dlPath);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Extraction failed:\n" + e.ToString());
                Utils.ShowErrorDialog("Extraction failed", string.Format("An error occured during extraction to directory:\n{0}", v.GameDirectory));
                return false;
            }
            v.UpdateInstallStatus();
            return true;
        }

        public class ProfileLaunchInfo : NotifyPropertyChangedBase
        {

            private LaunchStatus _status;
            private long _downloadedBytes;
            private long _downloadSize;
            private long _zipProcessed;
            private long _zipTotal;
            private string _zipCurrentItem;
            private long _launchCurrent;

            public bool WillDownload { get; set; }

            public LaunchStatus Status
            {
                get { return _status; }
                set { _status = value; OnPropertyChanged("Status"); OnPropertyChanged("IsProgressIndeterminate"); OnPropertyChanged("DisplayStatus"); OnPropertyChanged("AnimateTime"); OnPropertyChanged("ProgressMax"); }
            }

            public bool IsProgressIndeterminate => Status == LaunchStatus.InitializingDownload;

            public long DownloadedBytes
            {
                get { return _downloadedBytes; }
                set { _downloadedBytes = value; OnPropertyChanged("DownloadedBytes"); OnPropertyChanged("DisplayStatus"); OnPropertyChanged("ProgressCurrent"); }
            }

            public long DownloadSize
            {
                get { return _downloadSize; }
                set { _downloadSize = value; OnPropertyChanged("DownloadSize"); OnPropertyChanged("DisplayStatus"); OnPropertyChanged("ProgressMax"); }
            }

            public long ZipProcessed
            {
                get { return _zipProcessed; }
                set { _zipProcessed = value; OnPropertyChanged("ZipProcessed"); OnPropertyChanged("DisplayStatus"); OnPropertyChanged("ProgressCurrent"); }
            }

            public long ZipTotal
            {
                get { return _zipTotal; }
                set { _zipTotal = value; OnPropertyChanged("ZipTotal"); OnPropertyChanged("DisplayStatus"); OnPropertyChanged("ProgressMax"); }
            }

            public string ZipCurrentItem
            {
                get { return _zipCurrentItem; }
                set { _zipCurrentItem = value; OnPropertyChanged("ZipCurrentItem"); OnPropertyChanged("DisplayStatus"); }
            }

            public long LaunchCurrent
            {
                get { return _launchCurrent; }
                set { _launchCurrent = value; OnPropertyChanged("LaunchCurrent"); OnPropertyChanged("DisplayStatus"); OnPropertyChanged("ProgressCurrent"); }
            }

            public double ProgressCurrent
            {
                get
                {
                    if (WillDownload)
                    {
                        if (Status == LaunchStatus.Downloading)
                            return 400.0 * DownloadedBytes / DownloadSize;
                        if (Status == LaunchStatus.Extracting)
                            return (400.0 * ZipProcessed / ZipTotal) + 400.0;
                        if (Status == LaunchStatus.LaunchRemovePackage || Status == LaunchStatus.InitializingLaunch)
                            return LaunchCurrent + 800.0;
                        if (Status == LaunchStatus.LaunchRegisterPackage || Status == LaunchStatus.Launching)
                            return LaunchCurrent + 900.0;
                    }
                    else
                    {
                        if (Status == LaunchStatus.LaunchRemovePackage || Status == LaunchStatus.InitializingLaunch)
                            return LaunchCurrent;
                        if (Status == LaunchStatus.LaunchRegisterPackage || Status == LaunchStatus.Launching)
                            return LaunchCurrent + 100.0;
                    }
                    return 0;
                }
            }

            public double ProgressMax => WillDownload ? 400 + 400 + 100 + 100 : 100 + 100;

            public string DisplayStatus
            {
                get
                {
                    if (Status == LaunchStatus.InitializingDownload)
                        return "Initializing";
                    if (Status == LaunchStatus.Downloading)
                        return "Downloading " + ((double)DownloadedBytes / 1024 / 1024).ToString("N2") + " MB / " + ((double)DownloadSize / 1024 / 1024).ToString("N2") + " MB";
                    if (Status == LaunchStatus.Extracting)
                        return "Extracting " + ZipCurrentItem;
                    if (Status == LaunchStatus.Launching)
                        return "Finalizing";
                    return "Preparing";
                }
            }

            public TimeSpan AnimateTime
            {
                get
                {
                    if (Status == LaunchStatus.LaunchRemovePackage || Status == LaunchStatus.LaunchRegisterPackage || Status == LaunchStatus.Launching)
                        return new TimeSpan(0, 0, 0, 0, 500);
                    return new TimeSpan(0, 0, 0, 0, 100);
                }
            }

            public ICommand CancelCommand { get; set; }
        }

        public enum LaunchStatus
        {
            InitializingDownload,
            Downloading,
            Extracting,
            InitializingLaunch,
            LaunchRemovePackage,
            LaunchRegisterPackage,
            Launching
        }
    }
}
