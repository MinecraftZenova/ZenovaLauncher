using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;

namespace ZenovaLauncher
{
    class WUTokenHelper
    {
        public static async Task<string> GetWUToken(string accountId)
        {
            WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.microsoft.com", "consumers");
            WebAccount account = await WebAuthenticationCoreManager.FindAccountAsync(provider, accountId);

            WebTokenRequest request = new WebTokenRequest(provider, "service::dcat.update.microsoft.com::MBI_SSL", "{28520974-CE92-4F36-A219-3F255AF7E61E}");

            WebTokenRequestResult result = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(request, account);
            string token = result.ResponseData[0].Token;
            var tokenBinary = CryptographicBuffer.ConvertStringToBinary(token, BinaryStringEncoding.Utf16LE);
            var tokenBase64 = CryptographicBuffer.EncodeToBase64String(tokenBinary);
            Trace.WriteLine("Token = " + token);
            Trace.WriteLine("TokenBase64 = " + tokenBase64);
            return tokenBase64;
        }

        public static async Task<List<Tuple<string, string>>> GetMSAccounts()
        {
            WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.microsoft.com", "consumers");

            int status = GetMSAccounts(out string[] accountNames, out string[] accountIds, out int accountCount);
            if (status >= WU_ERRORS_START && status <= WU_ERRORS_END)
                throw new WUTokenException(status);
            else if (status != 0)
                Marshal.ThrowExceptionForHR(status);
            var accounts = accountNames.Zip(accountIds, Tuple.Create).ToList();
            foreach (var account in accounts.ToList())
                if (await WebAuthenticationCoreManager.FindAccountAsync(provider, account.Item2) == null)
                    accounts.Remove(account);
            return accounts;
        }

        private const int WU_ERRORS_START = 0x7ffc0200;
        private const int WU_NO_ACCOUNT = 0x7ffc0200;
        private const int WU_ERRORS_END = 0x7ffc0200;

        [DllImport("WUTokenHelper.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GetMSAccounts(
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2)] out string[] accountNames, 
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2)] out string[] accountIds, 
            out int accountCount);

        public class WUTokenException : Exception
        {
            public WUTokenException(int exception) : base(GetExceptionText(exception))
            {
                HResult = exception;
            }
            private static string GetExceptionText(int e)
            {
                switch (e)
                {
                    case WU_NO_ACCOUNT: return "No account";
                    default: return "Unknown " + e;
                }
            }
        }

    }
}
