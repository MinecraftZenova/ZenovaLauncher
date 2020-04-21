using System.Security.Principal;

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
    }
}
