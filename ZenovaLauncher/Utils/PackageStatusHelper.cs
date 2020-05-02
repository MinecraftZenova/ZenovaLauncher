using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ZenovaLauncher
{
    [ComImport, Guid("B1AEC16F-2383-4852-B0E9-8F0B1DC66B4D")]
    public class PackageDebugSettings
    {
    }
    public enum PACKAGE_EXECUTION_STATE
    {
        PES_UNKNOWN,
        PES_RUNNING,
        PES_SUSPENDING,
        PES_SUSPENDED,
        PES_TERMINATED
    }
    [ComImport, Guid("F27C3930-8029-4AD1-94E3-3DBA417810C1"),
                     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPackageDebugSettings
    {
        int EnableDebugging([MarshalAs(UnmanagedType.LPWStr)] string packageFullName, [MarshalAs(UnmanagedType.LPWStr)]
                                                              string debuggerCommandLine, IntPtr environment);
        int DisableDebugging([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
        int Suspend([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
        int Resume([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
        int TerminateAllProcesses([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
        int SetTargetSessionId(int sessionId);
        int EnumerateBackgroundTasks([MarshalAs(UnmanagedType.LPWStr)] string packageFullName,
                                                      out uint taskCount, out int intPtr, [Out] string[] array);
        int ActivateBackgroundTask(IntPtr something);
        int StartServicing([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
        int StopServicing([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
        int StartSessionRedirection([MarshalAs(UnmanagedType.LPWStr)] string packageFullName, uint sessionId);
        int StopSessionRedirection([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
        int GetPackageExecutionState([MarshalAs(UnmanagedType.LPWStr)] string packageFullName,
                                            out PACKAGE_EXECUTION_STATE packageExecutionState);
        int RegisterForPackageStateChanges([MarshalAs(UnmanagedType.LPWStr)] string packageFullName,
                               IntPtr pPackageExecutionStateChangeNotification, out uint pdwCookie);
        int UnregisterForPackageStateChanges(uint dwCookie);
    }
}
