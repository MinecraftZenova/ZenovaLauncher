using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

namespace ZenovaLauncher
{
    public class ZenovaUpdater : NotifyPropertyChangedBase
    {
        public static ZenovaUpdater instance;
        public static AssemblyType InstallerAssembly;
        public static AssemblyType ApiAssembly;

        private GitHubClient Client { get; set; }
        private HttpClient DownloadClient { get; set; }

        public CancellationTokenSource cancelSource = new CancellationTokenSource();

        private bool _isDownloading = false;
        private long _downloadedBytes;
        private long _downloadSize;
        private string _displayText;

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

        public string DisplayText
        {
            get { return _displayText; }
            set { _displayText = value; OnPropertyChanged("DisplayText"); }
        }

        public ICommand CancelCommand { get; set; }

        public ZenovaUpdater()
        {
            Client = new GitHubClient(new ProductHeaderValue("ZenovaLauncher"));
            DownloadClient = new HttpClient();
            if (InstallerAssembly == null)
                InitializeAssemblyTypes();
        }

        public void InitializeAssemblyTypes()
        {
            try
            {
                InstallerAssembly = new AssemblyType("ZenovaLauncher", (type, assetNumber) =>
                {
                    string path = Path.GetTempFileName();
                    string dlPath = path.Replace(".tmp", "_" + type.LatestRelease.Assets[0].Name);
                    File.Move(path, dlPath);
                    return dlPath;
                }, async (dlPath) =>
                {
                    ProcessStartInfo psi = new ProcessStartInfo(dlPath, "/verysilent");
                    psi.CreateNoWindow = true;
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    Process.Start(psi);
                    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate () { App.Current.Shutdown(); });
                }, Assembly.GetEntryAssembly().GetName().Version);
                ApiAssembly = new AssemblyType("ZenovaAPI", (type, assetNumber) => Path.Combine(App.DataDirectory, type.LatestRelease.Assets[assetNumber].Name),
                async (dlPath) =>
                {
                    if (dlPath.EndsWith(".zip"))
                    {
                        string devPath = Path.Combine(App.DataDirectory, "dev");
                        if (Directory.Exists(devPath))
                            Utils.Empty(devPath);
                        else
                            Directory.CreateDirectory(devPath);
                        await Task.Run(() => 
                        { 
                            ZipFile.ExtractToDirectory(dlPath, devPath);
                            Utils.SetModifiedTimeUtcRecursive(devPath, DateTime.UtcNow);
                        });
                        File.Delete(dlPath);
                    }
                    else if (dlPath.EndsWith(".dll"))
                    {
                        // Ensure ZenovaAPI.dll has ALL_APPLICATION_PACKAGES security
                        Utils.AddSecurityToFile(dlPath);
                    }
                }, numberOfAssets: 2);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Failed to Initialize AssemblyTypes:\n" + e.ToString());
            }
        }

        public async Task<List<AssemblyType>> CheckUpdates()
        {
            List<AssemblyType> updateTypes = new List<AssemblyType>();
            try
            {
                if (InstallerAssembly != null && await CheckUpdate(InstallerAssembly))
                    updateTypes.Add(InstallerAssembly);
                else if (ApiAssembly != null && await CheckUpdate(ApiAssembly))
                    updateTypes.Add(ApiAssembly);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Failed to Check Updates:\n" + e.ToString());
            }
            return updateTypes;
        }

        public async Task<bool> CheckUpdate(AssemblyType type)
        {
            try
            {
                type.LatestRelease = await Client.Repository.Release.GetLatest("MinecraftZenova", type.RepositoryName);
                type.TagInfo = (await Client.Repository.GetAllTags("MinecraftZenova", type.RepositoryName)).FirstOrDefault(x => x.Name == type.LatestRelease.TagName);

                if (type.InstalledVersion == null)
                    return true;

                Version latestVersion = new Version(type.LatestRelease.TagName.Trim());
                Trace.WriteLine(type.RepositoryName + " Installed Version: " + type.InstalledVersion);
                Trace.WriteLine(type.RepositoryName + " Latest Version: " + latestVersion);
                if (latestVersion > type.InstalledVersion && type.InstalledVersion > new Version(0, 0, 0, 0))
                    return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine(type.RepositoryName + " Check for update failed:\n" + e.ToString());
            }
            return false;
        }

        public async Task<bool> DoUpdate(AssemblyType type)
        {
            for (int asset = 0; asset < type.AssetsCount; asset++)
            {
                string dlPath = type.DownloadPath(type, asset);
                try
                {
                    DisplayText = type.RepositoryName;
                    await DownloadFile(type.LatestRelease.Assets[asset].BrowserDownloadUrl, dlPath, (current, total) =>
                    {
                        if (!IsDownloading)
                        {
                            Trace.WriteLine(type.RepositoryName + " download started");
                            IsDownloading = true;
                            if (total.HasValue)
                                DownloadSize = total.Value;
                        }
                        DownloadedBytes += current;
                    }, cancelSource.Token);
                    Trace.WriteLine(type.RepositoryName + " download finished");
                    IsDownloading = false;
                }
                catch (Exception e)
                {
                    Trace.WriteLine(type.RepositoryName + " download failed:\n" + e.ToString());
                    return false;
                }
                await type.PostDownloadTask(dlPath);
            }
            return true;
        }

        private async Task DownloadFile(string url, string to, DownloadProgress progress, CancellationToken cancelToken)
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

        public class AssemblyType
        {
            public delegate string GetDLPath(AssemblyType type, int assetNumber);
            public delegate Task PostDownload(string dlPath);

            private Version _installedVersion;
            public int AssetsCount { get; set; }
            public string RepositoryName { get; set; }
            public Release LatestRelease { get; set; }
            public RepositoryTag TagInfo { get; set; }
            public GetDLPath DownloadPath { get; set; }
            public PostDownload PostDownloadTask { get; set; }
            public Version InstalledVersion => _installedVersion ?? GetVersionFromPath(DownloadPath(this, AssetsCount - 1));

            public AssemblyType(string repositoryName, GetDLPath downloadPath, PostDownload postDownloadTask = default, Version installedVersion = null, int numberOfAssets = 1)
            {
                RepositoryName = repositoryName;
                DownloadPath = downloadPath;
                PostDownloadTask = postDownloadTask;
                _installedVersion = installedVersion;
                AssetsCount = numberOfAssets;
            }

            public static Version GetVersionFromPath(string path)
            {
                try
                {
                    if (File.Exists(path))
                        return new Version(FileVersionInfo.GetVersionInfo(path).FileVersion);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Failed to find version at: " + path + "\n" + e.ToString());
                }
                return null;
            }


            public string InstalledVersionString => InstalledVersion.ToString();
            public string PublishDateString => LatestRelease?.PublishedAt.Value.DateTime.ToString("dddd, MMMM dd, yyyy, HH:mm:ss");
            public string CommitHashString => TagInfo.Commit.Sha;
        }
    }
}
