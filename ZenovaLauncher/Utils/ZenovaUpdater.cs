using Octokit;
using System;
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
            Version latestVersion = new Version(latest.TagName.Trim());
            // get version from tag name, if greater than application version, start update
            Trace.WriteLine("Installed Version: " + Assembly.GetEntryAssembly().GetName().Version);
            Trace.WriteLine("Latest Available Version: " + latestVersion);
        }
    }
}
