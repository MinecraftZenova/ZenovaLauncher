using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ZenovaLauncher
{
    public class AccountManager : ObservableCollection<MSAccount>
    {
        public static AccountManager instance;

        public async Task AddAccounts()
        {
            await Task.Run(async () =>
            {
                Trace.WriteLine("AddAccounts");
                var accounts = await WUTokenHelper.GetMSAccounts();
                foreach (Tuple<string, string> account in accounts)
                    Add(new MSAccount(account.Item1, account.Item2));
                Trace.WriteLine("AddAccounts finished");
            });
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
