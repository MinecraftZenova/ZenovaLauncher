using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace ZenovaLauncher
{
    public class Mod
    {
        public string NameId { get; set; }
        public string Name { get; set; }
        public string Author { get; set; } = "unknown";
        public string Description { get; set; }
        public string DescriptionFile { get; set; } = null;
        public Version Version { get; set; }
        public string MinVersion
        {
            get { return MinMCVersion.InternalName; }
            set { MinMCVersion = VersionManager.instance.GetVersionFromString(value); }
        }
        public string MaxVersion
        {
            get { return MaxMCVersion.InternalName; }
            set { MaxMCVersion = VersionManager.instance.GetVersionFromString(value); }
        }
        [JsonIgnore]
        public string ModDirectory { get; set; }
        [JsonIgnore]
        public MinecraftVersion MinMCVersion { get; set; }
        [JsonIgnore]
        public MinecraftVersion MaxMCVersion { get; set; }
        [JsonIgnore]
        public Version LatestSupported => MaxMCVersion.Version;
        [JsonIgnore]
        public string ModVersion => Version.ToString();
        [JsonIgnore]
        public List<Profile> LinkedProfiles { get; set; } = new List<Profile>();

        public bool SupportsVersion(MinecraftVersion version)
        {
            return version.Version >= MinMCVersion.Version && version.Version <= MaxMCVersion.Version;
        }

        public void SetDescriptionTextBlock(TextBlock textBlock)
        {
            if (!string.IsNullOrEmpty(DescriptionFile))
            {
                string descriptionFilePath = Path.Combine(ModManager.instance.ModsDirectory, ModDirectory, DescriptionFile);
                if (File.Exists(descriptionFilePath))
                {
                    textBlock.Inlines.Clear();
                    textBlock.Inlines.Add(XamlReader.Parse(File.ReadAllText(descriptionFilePath)) as Inline);
                }
            }
            else
            {
                textBlock.Text = Description;
            }
        }

        public enum ModSortType
        {
            ByLatestSupported,
            ByName
        }
    }
}
