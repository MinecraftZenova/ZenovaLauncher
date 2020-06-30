using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        [JsonProperty("mcversion")]
        public List<MinecraftVersion> MCVersionList { get; set; }
        [JsonIgnore]
        public string ModDirectory { get; set; }
        [JsonIgnore]
        public MinecraftVersion LatestSupported => MCVersionList.OrderBy(x => x.Version).Reverse().ElementAt(0);
        [JsonIgnore]
        public Version LatestSupportedVersion => LatestSupported.Version;
        [JsonIgnore]
        public string ModVersion => Version.ToString();
        [JsonIgnore]
        public List<Profile> LinkedProfiles { get; set; } = new List<Profile>();

        public bool SupportsVersion(MinecraftVersion version)
        {
            return MCVersionList.Contains(version);
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

        public class MinecraftVersionConverter : JsonConverter<List<MinecraftVersion>>
        {
            public override void WriteJson(JsonWriter writer, List<MinecraftVersion> value, JsonSerializer serializer)
            {
                //writer.WriteValue(value.ToString());
            }

            public override List<MinecraftVersion> ReadJson(JsonReader reader, Type objectType, List<MinecraftVersion> existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.StartArray)
                {
                    var l = new List<MinecraftVersion>();
                    reader.Read();
                    while (reader.TokenType != JsonToken.EndArray)
                    {
                        l.Add(VersionManager.instance.GetVersionFromString(reader.Value as string));

                        reader.Read();
                    }
                    return l;
                }
                else
                {
                    return new List<MinecraftVersion> { VersionManager.instance.GetVersionFromString(reader.Value as string) };
                }
            }
        }
    }
}
