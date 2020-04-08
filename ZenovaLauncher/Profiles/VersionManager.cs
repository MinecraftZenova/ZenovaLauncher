﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ZenovaLauncher
{
    class VersionManager : ObservableCollection<MinecraftVersion>
    {
        public static VersionManager instance;

        private readonly string _cacheFile = "versions.json";
        private readonly HttpClient _client = new HttpClient();

        public VersionManager()
        {

        }

        private void ParseList(JArray data)
        {
            Clear();
            // ([name, uuid, isBeta])[]
            foreach (JArray o in data.AsEnumerable().Reverse())
            {
                Add(new MinecraftVersion(o[0].Value<string>(), o[1].Value<string>(), o[2].Value<int>() == 1));
            }
        }

        public async Task LoadFromCache()
        {
            try
            {
                using (var reader = File.OpenText(_cacheFile))
                {
                    var data = await reader.ReadToEndAsync();
                    ParseList(JArray.Parse(data));
                }
            }
            catch (FileNotFoundException)
            { // ignore
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
