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
            return this.SingleOrDefault(v => v.Name == versionName);
        }

        public string VersionsDirectory { get; }
        public MinecraftVersion LatestRelease => this.FirstOrDefault(v => v.Release);
        public MinecraftVersion LatestBeta => this.FirstOrDefault(v => v.Beta);

        private void ParseList(JArray data)
        {
            Clear();
            // ([name, uuid, isBeta])[]
            foreach (JArray o in data.AsEnumerable().Reverse())
            {
                Add(new MinecraftVersion(new Version(o[0].Value<string>()), o[1].Value<string>(), o[2].Value<int>() == 1));
            }
        }

        public async Task LoadMinecraftVersions()
        {
            try
            {
                await LoadFromCache();
            }
            catch (Exception e)
            {
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
            }
        }

        public async Task LoadFromCache()
        {
            using (var reader = File.OpenText(_cacheFile))
            {
                var data = await reader.ReadToEndAsync();
                ParseList(JArray.Parse(data));
            }
        }

        public async Task DownloadList()
        {
            var resp = await _client.GetAsync("https://mrarm.io/r/w10-vdb");
            resp.EnsureSuccessStatusCode();
            var data = await resp.Content.ReadAsStringAsync();
            File.WriteAllText(_cacheFile, data);
            ParseList(JArray.Parse(data));
        }
    }
}
