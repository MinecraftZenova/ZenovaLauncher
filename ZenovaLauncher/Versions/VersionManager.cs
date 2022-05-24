using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ZenovaLauncher
{
    class VersionManager : ObservableCollection<MinecraftVersion>
    {
        public static VersionManager instance;

        private readonly string _cacheFile = "versions.json";
        private readonly HttpClient _client = new HttpClient();

        public VersionManager(string versionsDir)
        {
            _cacheFile = Path.Combine(versionsDir, _cacheFile);
            VersionsDirectory = versionsDir;
            DefaultVersion = new MinecraftVersion(new Version("0.0.0.0"), "00000000-0000-0000-0000-000000000000", MinecraftVersion.VersionType.Null);
        }

        public void RemoveUnusedVersions()
        {
            if(Preferences.instance.RemoveUnusedVersions)
                Task.Run(() =>
                {
                    try
                    {
                        Parallel.ForEach(Directory.GetDirectories(VersionsDirectory).Except(ProfileManager.instance.Select(p => p.Version.GameDirectory).Distinct()), (dir => Directory.Delete(dir, true)));
                    }
                    catch (Exception)
                    {
                        return;
                    }
                });
        }

        public MinecraftVersion GetVersionFromString(string versionName)
        {
            if (versionName == "latest-release")
                return LatestRelease;
            if (versionName == "latest-beta")
                return LatestBeta;
            return this.SingleOrDefault(v => v.Name == versionName) ?? DefaultVersion;
        }

        public string VersionsDirectory { get; }
        public MinecraftVersion LatestRelease => this.FirstOrDefault(v => v.Release) ?? DefaultVersion;
        public MinecraftVersion LatestBeta => this.FirstOrDefault(v => v.Beta) ?? DefaultVersion;
        public MinecraftVersion DefaultVersion { get; }

        private void ParseList(JArray data)
        {
            Clear();
            // ([name, uuid, isBeta])[]
            foreach (JArray o in data.AsEnumerable().Reverse())
            {
                Add(new MinecraftVersion(new Version(o[0].Value<string>()), o[1].Value<string>(), (MinecraftVersion.VersionType) o[2].Value<int>()));
            }
        }

        public async Task LoadMinecraftVersions()
        {
            bool loadedFromCache = true;
            try
            {
                await LoadFromCache();
            }
            catch (Exception e)
            {
                loadedFromCache = false;
                if (!(e is FileNotFoundException))
                    Trace.WriteLine("List cache load failed:\n" + e.ToString());
            }
            try
            {
                await DownloadList();
            }
            catch (Exception e2)
            {
                Trace.WriteLine("List download failed:\n" + e2.ToString());
                if (!loadedFromCache)
                    Utils.ShowErrorDialog("Download Version List Failed", "Failed to download Minecraft version list and no cached version list was found. Please check your internet connection and relaunch.");
            }
        }

        public async Task LoadFromCache()
        {
            using (var reader = File.OpenText(_cacheFile))
            {
                string data = await reader.ReadToEndAsync();
                ParseList(JArray.Parse(data));
            }
        }

        public async Task DownloadList()
        {
            var resp = await _client.GetAsync("https://mrarm.io/r/w10-vdb");
            resp.EnsureSuccessStatusCode();
            string data = await resp.Content.ReadAsStringAsync();
            File.WriteAllText(_cacheFile, data);
            ParseList(JArray.Parse(data));
        }
    }
}
