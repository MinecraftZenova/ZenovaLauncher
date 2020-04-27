using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
            jsonSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
        }

        public Mod GetModFromDirectory(string dir)
        {
            return this.FirstOrDefault(m => m.ModDirectory == dir);
        }

        public void RemoveMod(Mod mod)
        {
            Directory.Delete(mod.ModDirectory, true);
            Remove(mod);
        }

        public void LoadMods()
        {
            foreach (string dir in Directory.GetDirectories(ModsDirectory))
                LoadMod(dir);
        }

        public void LoadMod(string modDir)
        {
            try
            {
                if (File.Exists(Path.Combine(modDir, _modsFileName)))
                {
                    Mod oldMod = GetModFromDirectory(new DirectoryInfo(modDir).Name);
                    if (oldMod != null)
                        Remove(oldMod);

                    Mod mod = JsonConvert.DeserializeObject<Mod>(File.ReadAllText(Path.Combine(modDir, _modsFileName)), jsonSettings);
                    mod.ModDirectory = new DirectoryInfo(modDir).Name;
                    Add(mod);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("Mods JSON Deserialize Failed: " + e.ToString());
                MessageBox.Show("Mods JSON Deserialize Failed: " + e.ToString());
            }
        }

        public void ImportMod(string modFile)
        {
            // make sure modFile exists
            if (File.Exists(modFile))
            {
                List<string> modDirs = new List<string>();
                string profileText = string.Empty;
                // extract directories to mods folder
                using (ZipArchive archive = ZipFile.OpenRead(modFile))
                {
                    var result = from entry in archive.Entries
                                 where !string.IsNullOrEmpty(Path.GetDirectoryName(entry.FullName))
                                 where !string.IsNullOrEmpty(entry.Name)
                                 select entry;

                    foreach (ZipArchiveEntry entry in result)
                    {
                        string path = Path.Combine(ModsDirectory, entry.FullName);

                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(Path.GetDirectoryName(path));

                        if (entry.Name == _modsFileName)
                            modDirs.Add(Path.GetDirectoryName(path));

                        // the second parameter specifies to replace file in destination
                        entry.ExtractToFile(path, true);
                    }
                    try
                    {
                        ZipArchiveEntry profileFile = archive.GetEntry("profiles.json");
                        if (profileFile != null)
                        {
                            using (StreamReader reader = new StreamReader(profileFile.Open()))
                            {
                                profileText = reader.ReadToEnd();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine("profiles.json not found in Mod: " + e.ToString());
                    }
                }
                // load mod for each directory
                foreach (string dir in modDirs)
                    LoadMod(dir);

                // load profiles.json if it exists
                if (!string.IsNullOrEmpty(profileText))
                    ProfileManager.instance.LoadProfiles(profileText);
            }
        }

    }
}
