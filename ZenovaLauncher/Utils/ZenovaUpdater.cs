using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ZenovaLauncher
{
    public class ZenovaUpdater
    {
        public static ZenovaUpdater instance;

        public async Task CheckUpdate()
        {
            try
            {
                GitHubClient client = new GitHubClient(new ProductHeaderValue("ZenovaLauncher"));
                var releases = await client.Repository.Release.GetAll("MinecraftZenova", "ZenovaLauncher");
                var latest = releases[0];

                // get version from tag name, if greater than application version, start update
                Version installedVersion = Assembly.GetEntryAssembly().GetName().Version;
                Version latestVersion = new Version(latest.TagName.Trim());
                Trace.WriteLine("Installed Version: " + installedVersion);
                Trace.WriteLine("Latest Available Version: " + latestVersion);
                if (latestVersion > installedVersion)
                {
                    var response = await client.Connection.Get<object>(new Uri(latest.Assets[0].Url), new Dictionary<string, string>(), "application/octet-stream");
                    byte[] bytes = Encoding.ASCII.GetBytes(response.HttpResponse.Body.ToString());
                    string path = Path.GetTempFileName();
                    string dlPath = path.Replace(".tmp", "_" + latest.Assets[0].Name);
                    File.Move(path, dlPath);
                    File.WriteAllBytes(dlPath, bytes);
                    ProcessStartInfo psi = new ProcessStartInfo(dlPath, "/verysilent");
                    psi.CreateNoWindow = true;
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    Process.Start(psi);
                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate ()
                    {
                        System.Windows.Application.Current.MainWindow.Close();
                    });
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("Check for update failed:\n" + e.ToString());
            }
        }

        public void DeleteInstaller(string path)
        {
            Trace.WriteLine("Attempting to delete: " + path);
            File.Delete(path);
        }
    }
}
