using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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

        private volatile bool _hasLaunchTask = false;

        public bool IsDownloading => DownloadInfo != null;
        public bool IsNotDownloading => !IsDownloading;

        private VersionDownloadInfo _downloadInfo;
        public VersionDownloadInfo DownloadInfo
        {
            get { return _downloadInfo; }
            set { _downloadInfo = value; OnPropertyChanged("DownloadInfo"); OnPropertyChanged("IsDownloading"); OnPropertyChanged("IsNotDownloading"); }
        }

        public Profile LaunchedProfile { get; set; }

        public void LaunchProfile(Profile p)
        {
            if (!IsDownloading)
            {
                if (!p.Version.IsInstalled)
                {
                    Task.Run(async () =>
                    {
                        Download(p);
                        Launch(p);
                    });
                }
                else
                {
                    Launch(p);
                }
            }
        }

        private void Launch(Profile p)
        {
            if (_hasLaunchTask)
                return;
            _hasLaunchTask = true;
            Task.Run(async () =>
            {
                string gameDir = Path.GetFullPath(p.Version.GameDirectory);
                try
                {
                    await ReRegisterPackage(gameDir);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("App re-register failed:\n" + e.ToString());
                    MessageBox.Show("App re-register failed:\n" + e.ToString());
                    _hasLaunchTask = false;
                    return;
                }

                try
                {
                    var pkg = await AppDiagnosticInfo.RequestInfoForPackageAsync(MINECRAFT_PACKAGE_FAMILY);
                    if (pkg.Count > 0)
                        await pkg[0].LaunchAsync();
                    Debug.WriteLine("App launch finished!");
                    _hasLaunchTask = false;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("App launch failed:\n" + e.ToString());
                    MessageBox.Show("App launch failed:\n" + e.ToString());
                    _hasLaunchTask = false;
                    return;
                }
            });
        }

        private async Task DeploymentProgressWrapper(IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> t)
        {
            TaskCompletionSource<int> src = new TaskCompletionSource<int>();
            t.Progress += (v, p) =>
            {
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
                    await DeploymentProgressWrapper(new PackageManager().RemovePackageAsync(pkg.Id.FullName, 0));
                }
                else
                {
                    Debug.WriteLine("Package is in development mode");
                    await DeploymentProgressWrapper(new PackageManager().RemovePackageAsync(pkg.Id.FullName, RemovalOptions.PreserveApplicationData));
                }
                Debug.WriteLine("Removal of package done: " + pkg.Id.FullName);
                break;
            }
            Debug.WriteLine("Registering package");
            string manifestPath = Path.Combine(gameDir, "AppxManifest.xml");
            await DeploymentProgressWrapper(new PackageManager().RegisterPackageAsync(new Uri(manifestPath), null, DeploymentOptions.DevelopmentMode));
            Debug.WriteLine("App re-register done!");
            RestoreMinecraftDataFromReinstall();
        }

        private async void Download(Profile p)
        {
            LaunchedProfile = p;
            p.UpdateLaunchStatus();
            MinecraftVersion v = p.Version;
            if (v.Beta)
                return;

            CancellationTokenSource cancelSource = new CancellationTokenSource();
            DownloadInfo = new VersionDownloadInfo
            {
                IsInitializing = true,
                CancelCommand = new RelayCommand((o) => cancelSource.Cancel())
            };

            Debug.WriteLine("Download start");
            string dlPath = Path.Combine(VersionManager.instance.VersionsDirectory, "Minecraft-" + v.Name + ".Appx");
            VersionDownloader downloader = VersionDownloader.standard;
            if (v.Beta)
            {
                /*downloader = VersionDownloader.user;
                if (Interlocked.CompareExchange(ref _userVersionDownloaderLoginTaskStarted, 1, 0) == 0)
                {
                    _userVersionDownloaderLoginTask.Start();
                }
                await _userVersionDownloaderLoginTask;*/
            }
            try
            {
                await downloader.Download(v.UUID, "1", dlPath, (current, total) =>
                {
                    if (DownloadInfo.IsInitializing)
                    {
                        Debug.WriteLine("Actual download started");
                        DownloadInfo.IsInitializing = false;
                        if (total.HasValue)
                            DownloadInfo.TotalSize = total.Value;
                    }
                    DownloadInfo.DownloadedBytes = current;
                }, cancelSource.Token);
                Debug.WriteLine("Download complete");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Download failed:\n" + e.ToString());
                if (!(e is TaskCanceledException))
                    MessageBox.Show("Download failed:\n" + e.ToString());
                DownloadInfo = null;
                return;
            }
            try
            {
                DownloadInfo.IsExtracting = true;
                string dirPath = v.GameDirectory;
                if (Directory.Exists(dirPath))
                    Directory.Delete(dirPath, true);
                ZipFile.ExtractToDirectory(dlPath, dirPath);
                DownloadInfo = null;
                File.Delete(Path.Combine(dirPath, "AppxSignature.p7x"));
            }
            catch (Exception e)
            {
                Debug.WriteLine("Extraction failed:\n" + e.ToString());
                MessageBox.Show("Extraction failed:\n" + e.ToString());
                DownloadInfo = null;
                return;
            }
            DownloadInfo = null;
            LaunchedProfile = null;
            v.UpdateInstallStatus();
            p.UpdateLaunchStatus();
        }
    }
}
