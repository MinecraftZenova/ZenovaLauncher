using Octokit;
using System.Diagnostics;

namespace ZenovaLauncher
{
    public class ZenovaUpdater
    {
        private const string updateURL = "https://github.com/MinecraftZenova/ZenovaLauncher/releases/latest/download/ZenovaLauncher.exe";
        public static ZenovaUpdater instance;

        public async void CheckUpdate()
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("ZenovaLauncher"));
            var releases = await client.Repository.Release.GetAll("MinecraftZenova", "ZenovaLauncher");
            var latest = releases[0];
            Trace.WriteLine(
                string.Format("The latest release is tagged at {0} and is named {1}",
                latest.TagName,
                latest.Name));
        }
    }
}
