using ModernWpf.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace ZenovaLauncher
{
    public class ModManager : ObservableCollection<Mod>
    {
        public static ModManager instance;
        private static JsonSerializerSettings jsonSettings;

        private readonly string _modsFileName = "modinfo.json";

        public Action Refresh { get; set; }
        public string ModsDirectory { get; set; }
        public bool WindowLoaded { get; set; }
        public List<string> ModLoadQueue = new List<string>();

        public ModManager(string modsDir)
        {
            ModsDirectory = modsDir;
            jsonSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
        }

        public Mod GetModFromDirectory(string dir)
        {
            return this.FirstOrDefault(m => m.ModDirectory == dir);
        }

        public void AddMod(Mod mod)
        {
            Mod oldMod = GetModFromDirectory(mod.ModDirectory);
            if (oldMod != null)
                Remove(oldMod);
            Add(mod);
            Refresh?.Invoke();
        }

        public void RemoveMod(Mod mod)
        {
            foreach (Profile p in mod.LinkedProfiles)
                p.ModsList.Remove(mod);
            Directory.Delete(Path.Combine(ModsDirectory, mod.ModDirectory), true);
            Remove(mod);
        }

        public void LoadMods()
        {
            foreach (string dir in Directory.GetDirectories(ModsDirectory))
                AddMod(LoadModFromDir(dir));
        }

        public Mod LoadModFromDir(string modDir)
        {
            try
            {
                if (File.Exists(Path.Combine(modDir, _modsFileName)))
                    return LoadMod(File.ReadAllText(Path.Combine(modDir, _modsFileName)), new DirectoryInfo(modDir).Name);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Load Mod from directory Failed: " + e.ToString());
                Utils.ShowErrorDialog("Failed to load mod", string.Format("Error occured while loading mod from directory:\n{0}\nMake sure directory exists and try again.", modDir));
            }
            return null;
        }

        public Mod LoadMod(string modText, string dirName)
        {
            try
            {
                Mod mod = JsonConvert.DeserializeObject<Mod>(modText, jsonSettings);
                mod.ModDirectory = dirName;
                return mod;
            }
            catch (Exception e)
            {
                Trace.WriteLine("Mods JSON Deserialize Failed: " + e.ToString());
                Utils.ShowErrorDialog("Failed to load mod", string.Format("Error occured while parsing modinfo.json in directory:\n{0}\nMake sure modinfo.json exists and is formatted correctly.", dirName));
            }
            return null;
        }

        public (List<Mod>, List<Profile>) ParseModPackage(string modFile)
        {
            var (modsList, profilesList) = (new List<Mod>(), new List<Profile>());
            // make sure modFile exists
            if (File.Exists(modFile))
            {
                // extract directories to mods folder
                using (ZipArchive archive = ZipFile.OpenRead(modFile))
                {
                    var result = from entry in archive.Entries
                                 where !string.IsNullOrEmpty(entry.Name)
                                 select entry;

                    foreach (ZipArchiveEntry entry in result)
                    {
                        if (entry.Name == _modsFileName)
                        {
                            using (StreamReader reader = new StreamReader(entry.Open()))
                            {
                                modsList.Add(LoadMod(reader.ReadToEnd(), Path.GetDirectoryName(entry.FullName)));
                            }

                        }
                        else if (entry.Name == "profiles.json")
                        {
                            using (StreamReader reader = new StreamReader(entry.Open()))
                            {
                                profilesList.AddRange(ProfileManager.instance.LoadProfiles(reader.ReadToEnd()));
                            }
                        }
                    }
                }
            }
            return (modsList, profilesList);
        }

        public void ImportModPackage(string modFile)
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
                        if (entry.Name == "profiles.json")
                        {
                            using (StreamReader reader = new StreamReader(entry.Open()))
                            {
                                profileText = reader.ReadToEnd();
                            }
                        }
                        else
                        {
                            entry.ExtractToFile(path, true);
                        }
                    }
                }
                // load mod for each directory
                foreach (string dir in modDirs)
                    AddMod(LoadModFromDir(dir));

                if (!string.IsNullOrEmpty(profileText))
                    ProfileManager.instance.AddProfiles(ProfileManager.instance.LoadProfiles(profileText));
            }
        }

        public async void ImportModsConfirmation(List<string> modsToImport)
        {
            Trace.WriteLine("Trying to show ImportModConfirmation");
            List<string> modPaths = modsToImport.ToList();
            var (modsList, profilesList) = (new List<Mod>(), new List<Profile>());
            foreach (string file in modPaths)
            {
                var (mods, profiles) = ParseModPackage(file);
                modsList.AddRange(mods);
                profilesList.AddRange(profiles);
            }
            ImportModDialog confirmDialog = new ImportModDialog(modsList, profilesList);
            var result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                foreach (string file in modPaths)
                    ImportModPackage(file);
            }
        }

        public void TryImportMods(List<string> modPaths)
        {
            ModLoadQueue.AddRange(modPaths);
            if (WindowLoaded && ModLoadQueue.Count > 0)
            {
                ImportModsConfirmation(ModLoadQueue);
                ModLoadQueue.Clear();
            }
        }
    }
}
