using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;

namespace ZenovaLauncher
{
    class WUTokenHelper
    {
        public static async Task<string[]> GetWUToken(string[] accountIds)
        {
            var tokens = new List<string>();

            foreach (string accountId in accountIds)
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

                tokens.Add(tokenBase64);
            }
            return tokens.ToArray();
        }
    }
}
