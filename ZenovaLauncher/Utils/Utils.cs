using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace ZenovaLauncher
{
    public class Utils
    {
        public static bool IsElevated
        {
            get
            {
                return WindowsIdentity.GetCurrent().Owner
                  .IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);
            }
        }

        public static string ComputeHash(object objectToHash)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            try
            {
                byte[] result = md5.ComputeHash(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(objectToHash)));

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < result.Length; i++)
                {
                    sb.Append(result[i].ToString("x2"));
                }

                return sb.ToString();
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Hash has not been generated.");
                return null;
            }
        }
    }
}
