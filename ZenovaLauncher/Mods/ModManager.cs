using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace ZenovaLauncher
{
    public class ModManager : ObservableCollection<Mod>
    {
        public static ModManager instance;
        private static JsonSerializerSettings jsonSettings;

        private readonly string _modsFileName = "modinfo.json";

        public string ModsDirectory { get; set; }

        public ModManager(string modsDir)
        {
            ModsDirectory = modsDir;
        }

        public Mod GetModFromDirectory(string dir)
        {
            return this.FirstOrDefault(m => m.ModDirectory == dir);
        }

        public void LoadMods()
        {
            jsonSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            foreach (string dir in Directory.GetDirectories(ModsDirectory))
            {
                try
                {
                    if (File.Exists(Path.Combine(dir, _modsFileName)))
                    {
                        Mod mod = JsonConvert.DeserializeObject<Mod>(File.ReadAllText(Path.Combine(dir, _modsFileName)), jsonSettings);
                        mod.ModDirectory = dir;
                        Add(mod);
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Mods JSON Deserialize Failed: " + e.ToString());
                    MessageBox.Show("Mods JSON Deserialize Failed: " + e.ToString());
                }
            }
        }

    }
}
