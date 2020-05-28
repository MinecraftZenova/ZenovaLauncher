using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace ZenovaLauncher
{
    public class ZenovaUpdater : NotifyPropertyChangedBase
    {
        public static ZenovaUpdater instance;

        private GitHubClient Client { get; set; }
        private HttpClient DownloadClient { get; set; }
        private Release LatestRelease { get; set; }

        public CancellationTokenSource cancelSource = new CancellationTokenSource();

        private bool _isDownloading = false;
        private long _downloadedBytes;
        private long _downloadSize;

        public bool IsDownloading
        {
            get { return _isDownloading; }
            set { _isDownloading = value; OnPropertyChanged("IsDownloading"); OnPropertyChanged("IsNotDownloading"); }
        }

        public bool IsNotDownloading => !IsDownloading;

        public long DownloadedBytes
        {
            get { return _downloadedBytes; }
            set { _downloadedBytes = value; OnPropertyChanged("DownloadedBytes"); }
        }

        public long DownloadSize
        {
            get { return _downloadSize; }
            set { _downloadSize = value; OnPropertyChanged("DownloadSize"); }
        }

        public ICommand CancelCommand { get; set; }

        public ZenovaUpdater()
        {
            Client = new GitHubClient(new ProductHeaderValue("ZenovaLauncher"));
            DownloadClient = new HttpClient();
        }

        public async Task<bool> CheckUpdate()
        {
            try
            {
                var releases = await Client.Repository.Release.GetAll("MinecraftZenova", "ZenovaLauncher");
                LatestRelease = releases[0];

                // get version from tag name, if greater than application version, start update
                Version installedVersion = Assembly.GetEntryAssembly().GetName().Version;
                Version latestVersion = new Version(LatestRelease.TagName.Trim());
                Trace.WriteLine("Installed Version: " + installedVersion);
                Trace.WriteLine("Latest Available Version: " + latestVersion);
                if (latestVersion > installedVersion)
                    return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine("Check for update failed:\n" + e.ToString());
            }
            return false;
        }

        public async Task<bool> DoUpdate()
        {
            string path = Path.GetTempFileName();
            string dlPath = path.Replace(".tmp", "_" + LatestRelease.Assets[0].Name);
            File.Move(path, dlPath);
            try
            {
                await DownloadInstaller(LatestRelease.Assets[0].BrowserDownloadUrl, dlPath, (current, total) =>
                {
                    if (!IsDownloading)
                    {
                        Trace.WriteLine("Actual download started");
                        IsDownloading = true;
                        if (total.HasValue)
                            DownloadSize = total.Value;
                    }
                    DownloadedBytes += current;
                }, cancelSource.Token);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Download failed:\n" + e.ToString());
                return false;
            }
            ProcessStartInfo psi = new ProcessStartInfo(dlPath, "/verysilent");
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(psi);
            return true;
        }

        private async Task DownloadInstaller(string url, string to, DownloadProgress progress, CancellationToken cancelToken)
        {
            using (var resp = await DownloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancelToken))
            {
                using (var inStream = await resp.Content.ReadAsStreamAsync())
                using (var outStream = new FileStream(to, System.IO.FileMode.Create))
                {
                    long? totalSize = resp.Content.Headers.ContentLength;
                    progress(0, totalSize);
                    long transferred = 0;
                    byte[] buf = new byte[1024 * 1024];
                    while (true)
                    {
                        int n = await inStream.ReadAsync(buf, 0, buf.Length, cancelToken);
                        if (n == 0)
                            break;
                        await outStream.WriteAsync(buf, 0, n, cancelToken);
                        transferred += n;
                        progress(transferred, totalSize);
                    }
                }
            }
        }

        public void DeleteInstaller(string path)
        {
            Trace.WriteLine("Attempting to delete: " + path);
            File.Delete(path);
        }

        public delegate void DownloadProgress(long current, long? total);
    }
}
