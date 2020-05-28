using Octokit;
using System.Diagnostics;
using System.Reflection;

namespace ZenovaLauncher
{
    public class ZenovaUpdater
    {
        public static ZenovaUpdater instance;

        public async void CheckUpdate()
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("ZenovaLauncher"));
            var releases = await client.Repository.Release.GetAll("MinecraftZenova", "ZenovaLauncher");
            var latest = releases[0];
            // get version from tag name, if greater than application version, start update
            Trace.WriteLine("Installed Version: " + Assembly.GetEntryAssembly().GetName().Version);
            Trace.WriteLine(
                string.Format("The latest release is tagged at {0} and is named {1}",
                latest.TagName,
                latest.Name));
        }
    }
}
