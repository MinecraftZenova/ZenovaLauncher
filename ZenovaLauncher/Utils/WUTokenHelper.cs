using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace ZenovaLauncher
{
    class WUTokenHelper
    {
        public static string GetWUToken(string accountId)
        {
            int status = GetWUToken(out string token, in accountId);
            if (status >= WU_ERRORS_START && status <= WU_ERRORS_END)
                throw new WUTokenException(status);
            else if (status != 0)
                Marshal.ThrowExceptionForHR(status);
            return token;
        }

        public static IEnumerable<Tuple<string, string>> GetMSAccounts()
        {
            int status = GetMSAccounts(out string[] accountNames, out string[] accountIds, out int accountCount);
            if (status >= WU_ERRORS_START && status <= WU_ERRORS_END)
                throw new WUTokenException(status);
            else if (status != 0)
                Marshal.ThrowExceptionForHR(status);
            return accountNames.Zip(accountIds, Tuple.Create);
        }

        private const int WU_ERRORS_START = 0x7ffc0200;
        private const int WU_NO_ACCOUNT = 0x7ffc0200;
        private const int WU_ERRORS_END = 0x7ffc0200;

        [DllImport("WUTokenHelper.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GetWUToken([MarshalAs(UnmanagedType.LPWStr)] out string token, [MarshalAs(UnmanagedType.LPWStr)] in string accountId);

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
            private static String GetExceptionText(int e)
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
