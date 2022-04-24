using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;

namespace ZenovaLauncher
{
    public class AccountManager : ObservableCollection<MSAccount>
    {
        public static AccountManager instance;

        public MSAccount SelectedAccount { get; set; }

        public async Task AddAccounts()
        {
            Trace.WriteLine("AddAccounts"); 
            WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.microsoft.com", "consumers");
            RegistryKey accountIdsReg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\IdentityCRL\\UserTileData");
            if (accountIdsReg != null)
            {
                string[] accountsIds = Array.FindAll(accountIdsReg.GetValueNames(), s => !s.EndsWith("_ETAG"));
                foreach (var accountsId in accountsIds)
                {
                    var account = await WebAuthenticationCoreManager.FindAccountAsync(provider, accountsId);
                    Add(new MSAccount(account.UserName, account.Id));
                }
            }
            SelectedAccount = this.First();
            Trace.WriteLine("AddAccounts finished");
        }
    }

    public class MSAccount : NotifyPropertyChangedBase
    {
        public MSAccount(string accountName, string accountId)
        {
            AccountName = accountName;
            AccountId = accountId;
        }

        public string AccountName { get; set; }
        public string AccountId { get; set; }
    }
}
