using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ZenovaLauncher
{
    public class AccountManager : ObservableCollection<XboxAccount>
    {
        public static AccountManager instance;

        public XboxAccount CurrentXboxAccount { get; set; }

        public async Task AddAccounts()
        {
            Trace.WriteLine("AddAccounts");
            RegistryKey currentXboxReg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\XboxLive");
            if (currentXboxReg != null)
            {
                var gamertag = currentXboxReg.GetValue("Gamertag");
                var xuid = currentXboxReg.GetValue("Xuid");
                if (gamertag != null && xuid != null)
                    Add(new XboxAccount(gamertag.ToString(), xuid.ToString()));
            }
            CurrentXboxAccount = this.First();
            Trace.WriteLine("AddAccounts finished");
        }
    }

    public class XboxAccount : NotifyPropertyChangedBase
    {
        public XboxAccount(string gamertag, string xuid)
        {
            Gamertag = gamertag;
            Xuid = xuid;
        }

        public string Gamertag { get; set; }
        public string Xuid { get; set; }
    }
}