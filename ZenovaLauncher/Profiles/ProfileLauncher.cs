using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ZenovaLauncher
{
    public class ProfileLauncher : NotifyPropertyChangedBase
    {
        public static ProfileLauncher instance;

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
            if (!IsDownloading && !p.Version.IsInstalled)
            {
                Download(p);
            }
        }

        public void Download(Profile p)
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
            Task.Run(async () =>
            {
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
            });
        }
    }
}
