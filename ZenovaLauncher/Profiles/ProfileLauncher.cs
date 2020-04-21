using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Management.Core;
using Windows.Management.Deployment;
using Windows.System;

namespace ZenovaLauncher
{
    public class ProfileLauncher : NotifyPropertyChangedBase
    {
        public static ProfileLauncher instance;

        private static readonly string MINECRAFT_PACKAGE_FAMILY = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

        private Task UserLoginTask = new Task(() => 
        {
            VersionDownloader.user.EnableUserAuthorization();
        });

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
                Profile p = value != null ? value : _launchedProfile;
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
                    LaunchInfo = new ProfileLaunchInfo { Status = LaunchStatus.InitializingDownload };
                    LaunchedProfile = p;
                    bool installStatus = true;

                    if (!p.Version.IsInstalled)
                        installStatus = await Download(p);

                    LaunchInfo.Status = LaunchStatus.InitializingLaunch;
                    if (installStatus)
                        await Launch(p);

                    LaunchInfo = null;
                    LaunchedProfile.LastPlayed = DateTime.Now;
                    LaunchedProfile = null;
                });
            }
        }

        private async Task Launch(Profile p)
        {
            try
            {
                await ReRegisterPackage(Path.GetFullPath(p.Version.GameDirectory));
            }
            catch (Exception e)
            {
                Debug.WriteLine("App re-register failed:\n" + e.ToString());
                MessageBox.Show("App re-register failed:\n" + e.ToString());
                return;
            }

            try
            {
                LaunchInfo.Status = LaunchStatus.Launching;
                var pkg = await AppDiagnosticInfo.RequestInfoForPackageAsync(MINECRAFT_PACKAGE_FAMILY);
                if (pkg.Count > 0)
                    await pkg[0].LaunchAsync();
                Debug.WriteLine("App launch finished!");
            }
            catch (Exception e)
            {
                Debug.WriteLine("App launch failed:\n" + e.ToString());
                MessageBox.Show("App launch failed:\n" + e.ToString());
                return;
            }
        }

        private async Task DeploymentProgressWrapper(IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> t, LaunchStatus status)
        {
            TaskCompletionSource<int> src = new TaskCompletionSource<int>();
            t.Progress += (v, p) =>
            {
                if (LaunchInfo.Status != status)
                    LaunchInfo.Status = status;
                LaunchInfo.LaunchCurrent = p.percentage;
                Debug.WriteLine("Deployment progress: " + p.state + " " + p.percentage + "%");
            };
            t.Completed += (v, p) =>
            {
                Debug.WriteLine("Deployment done: " + p);
                src.SetResult(1);
            };
            await src.Task;
        }

        private string GetBackupMinecraftDataDir()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string tmpDir = Path.Combine(localAppData, "TmpMinecraftLocalState");
            return tmpDir;
        }

        private void BackupMinecraftDataForRemoval()
        {
            var data = ApplicationDataManager.CreateForPackageFamily(MINECRAFT_PACKAGE_FAMILY);
            string tmpDir = GetBackupMinecraftDataDir();
            if (Directory.Exists(tmpDir))
            {
                Debug.WriteLine("BackupMinecraftDataForRemoval error: " + tmpDir + " already exists");
                Process.Start("explorer.exe", tmpDir);
                MessageBox.Show("The temporary directory for backing up MC data already exists. This probably means that we failed last time backing up the data. Please back the directory up manually.");
                throw new Exception("Temporary dir exists");
            }
            Debug.WriteLine("Moving Minecraft data to: " + tmpDir);
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

        private void RestoreMinecraftDataFromReinstall()
        {
            string tmpDir = GetBackupMinecraftDataDir();
            if (!Directory.Exists(tmpDir))
                return;
            var data = ApplicationDataManager.CreateForPackageFamily(MINECRAFT_PACKAGE_FAMILY);
            Debug.WriteLine("Moving backup Minecraft data to: " + data.LocalFolder.Path);
            RestoreMove(tmpDir, data.LocalFolder.Path);
            Directory.Delete(tmpDir, true);
        }

        private async Task ReRegisterPackage(string gameDir)
        {
            foreach (var pkg in new PackageManager().FindPackages(MINECRAFT_PACKAGE_FAMILY))
            {
                if (pkg.InstalledLocation.Path == gameDir)
                {
                    Debug.WriteLine("Skipping package removal - same path: " + pkg.Id.FullName + " " + pkg.InstalledLocation.Path);
                    return;
                }
                Debug.WriteLine("Removing package: " + pkg.Id.FullName + " " + pkg.InstalledLocation.Path);
                if (!pkg.IsDevelopmentMode)
                {
                    BackupMinecraftDataForRemoval();
                    await DeploymentProgressWrapper(new PackageManager().RemovePackageAsync(pkg.Id.FullName, 0), LaunchStatus.LaunchRemovePackage);
                }
                else
                {
                    Debug.WriteLine("Package is in development mode");
                    await DeploymentProgressWrapper(new PackageManager().RemovePackageAsync(pkg.Id.FullName, RemovalOptions.PreserveApplicationData), LaunchStatus.LaunchRemovePackage);
                }
                Debug.WriteLine("Removal of package done: " + pkg.Id.FullName);
                break;
            }
            Debug.WriteLine("Registering package");
            string manifestPath = Path.Combine(gameDir, "AppxManifest.xml");
            await DeploymentProgressWrapper(new PackageManager().RegisterPackageAsync(new Uri(manifestPath), null, DeploymentOptions.DevelopmentMode), LaunchStatus.LaunchRegisterPackage);
            Debug.WriteLine("App re-register done!");
            RestoreMinecraftDataFromReinstall();
        }

        private async Task<bool> Download(Profile p)
        {
            MinecraftVersion v = p.Version;
            CancellationTokenSource cancelSource = new CancellationTokenSource();
            LaunchInfo.CancelCommand = new RelayCommand((o) => cancelSource.Cancel());

            Debug.WriteLine("Download start");
            string dlPath = Path.Combine(VersionManager.instance.VersionsDirectory, "Minecraft-" + v.Name + ".Appx");
            VersionDownloader downloader = VersionDownloader.standard;
            if (v.Beta)
            {
                downloader = VersionDownloader.user;
                if (UserLoginTask.Status == TaskStatus.Created)
                    UserLoginTask.Start();
                await UserLoginTask;
            }
            try
            {
                await downloader.Download(v.UUID, "1", dlPath, (current, total) =>
                {
                    if (LaunchInfo.Status == LaunchStatus.InitializingDownload)
                    {
                        Debug.WriteLine("Actual download started");
                        LaunchInfo.Status = LaunchStatus.Downloading;
                        if (total.HasValue)
                            LaunchInfo.DownloadSize = total.Value;
                    }
                    LaunchInfo.DownloadedBytes += current;
                }, cancelSource.Token);
                Debug.WriteLine("Download complete");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Download failed:\n" + e.ToString());
                if (!(e is TaskCanceledException))
                    MessageBox.Show("Download failed:\n" + e.ToString());
                return false;
            }
            try
            {
                LaunchInfo.Status = LaunchStatus.InitializingExtraction;
                string dirPath = v.GameDirectory;
                if (Directory.Exists(dirPath))
                    Directory.Delete(dirPath, true);
                Progress<ZipProgress> progress = new Progress<ZipProgress>();
                progress.ProgressChanged += (sender, zipProgress) =>
                {
                    if (LaunchInfo.Status == LaunchStatus.InitializingExtraction)
                    {
                        Debug.WriteLine("Extraction started");
                        LaunchInfo.Status = LaunchStatus.Extracting;
                        LaunchInfo.ZipTotal = zipProgress.Total;
                    }
                    LaunchInfo.ZipProcessed = zipProgress.Processed;
                    LaunchInfo.ZipCurrentItem = zipProgress.CurrentItem;
                };
                ZipArchive zipFile = new ZipArchive(new FileStream(dlPath, FileMode.Open));
                zipFile.ExtractToDirectory(dirPath, progress);
                File.Delete(Path.Combine(dirPath, "AppxSignature.p7x"));
            }
            catch (Exception e)
            {
                Debug.WriteLine("Extraction failed:\n" + e.ToString());
                MessageBox.Show("Extraction failed:\n" + e.ToString());
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

            public LaunchStatus Status
            {
                get { return _status; }
                set { _status = value; OnPropertyChanged("Status"); OnPropertyChanged("IsProgressIndeterminate"); OnPropertyChanged("DisplayStatus"); OnPropertyChanged("AnimateTime"); OnPropertyChanged("ProgressMax"); }
            }

            public bool IsProgressIndeterminate
            {
                get { return Status == LaunchStatus.InitializingDownload || Status == LaunchStatus.InitializingExtraction || Status == LaunchStatus.InitializingLaunch || Status == LaunchStatus.Launching; }
            }

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

            public long LaunchTotal => 100 * 2;

            public long ProgressCurrent
            {
                get
                {
                    if (Status == LaunchStatus.Extracting)
                        return ZipProcessed;
                    if (Status == LaunchStatus.Downloading)
                        return DownloadedBytes;
                    if (Status == LaunchStatus.LaunchRemovePackage)
                        return LaunchCurrent;
                    if (Status == LaunchStatus.LaunchRegisterPackage)
                        return 100 + LaunchCurrent;
                    return 0;
                }
            }

            public long ProgressMax
            {
                get
                {
                    if (Status == LaunchStatus.Extracting)
                        return ZipTotal;
                    if (Status == LaunchStatus.Downloading)
                        return DownloadSize;
                    if (Status == LaunchStatus.LaunchRegisterPackage || Status == LaunchStatus.LaunchRemovePackage)
                        return LaunchTotal;
                    return 1;
                }
            }

            public string DisplayStatus
            {
                get
                {
                    if (Status == LaunchStatus.Downloading)
                        return "Downloading " + ((double)DownloadedBytes / 1024 / 1024).ToString("N2") + " MB / " + ((double)DownloadSize / 1024 / 1024).ToString("N2") + " MB";
                    if (Status == LaunchStatus.InitializingExtraction)
                        return "Extracting";
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
            InitializingExtraction,
            Extracting,
            InitializingLaunch,
            LaunchRemovePackage,
            LaunchRegisterPackage,
            Launching
        }
    }
}
