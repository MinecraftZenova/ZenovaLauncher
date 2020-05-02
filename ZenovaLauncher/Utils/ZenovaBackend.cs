using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;

namespace ZenovaLauncher
{
    class ZenovaBackend
    {
        public static async Task<List<Tuple<string, string>>> GetMSAccounts()
        {
            WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.microsoft.com", "consumers");

            int status = GetMSAccounts(out string[] accountNames, out string[] accountIds, out int accountCount);
            if (status >= ZL_ERRORS_START && status <= ZL_ERRORS_END)
                throw new BackendException(status);
            else if (status != 0)
                Marshal.ThrowExceptionForHR(status);
            var accounts = accountNames.Zip(accountIds, Tuple.Create).ToList();
            foreach (var account in accounts.ToList())
                if (await WebAuthenticationCoreManager.FindAccountAsync(provider, account.Item2) == null)
                    accounts.Remove(account);
            return accounts;
        }

        private const int ZL_ERRORS_START = 0x7ffc0200;
        private const int ZL_NO_ACCOUNT = 0x7ffc0200;
        private const int ZL_ERRORS_END = 0x7ffc0200;

        [DllImport("ZenovaBackend.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GetMSAccounts(
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2)] out string[] accountNames, 
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2)] out string[] accountIds, 
            out int accountCount);

        public class BackendException : Exception
        {
            public BackendException(int exception) : base(GetExceptionText(exception))
            {
                HResult = exception;
            }
            private static string GetExceptionText(int e)
            {
                switch (e)
                {
                    case ZL_NO_ACCOUNT: return "No account";
                    default: return "Unknown " + e;
                }
            }
        }

    }
}
