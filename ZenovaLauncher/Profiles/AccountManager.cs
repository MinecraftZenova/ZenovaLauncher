using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenovaLauncher
{
    public class AccountManager : ObservableCollection<MSAccount>
    {
        public static AccountManager instance;

        public void AddAccounts()
        {
            var accounts = WUTokenHelper.GetMSAccounts();
            foreach (Tuple<string, string> account in accounts)
                Add(new MSAccount(account.Item1, account.Item2));
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
