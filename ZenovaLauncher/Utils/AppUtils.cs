using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZenovaLauncher.AppUtils
{
    public class AppDebugger
    {
        private IPackageDebugSettings DebugSettings;
        private ApplicationActivationManager AppManager;
        private string PackageFullName;
        public int StatusCode { get; set; }

        public AppDebugger(string packageFullName)
        {
            PackageFullName = packageFullName;
            StatusCode = 0;
            DebugSettings = new PackageDebugSettings() as IPackageDebugSettings;
            AppManager = new ApplicationActivationManager();
        }

        public void LaunchApp(string applicationId)
        {
            try
            {
                WindowOptions.CoAllowSetForegroundWindow(Marshal.GetIUnknownForObject(AppManager), IntPtr.Zero);
                StatusCode = (int) AppManager.ActivateApplication(applicationId, null, ActivateOptions.None, out uint pid);
            }
            catch (Exception e)
            {
                StatusCode = 1;
                Trace.WriteLine("LaunchApp failed: " + e.ToString());
            }
        }

        public void EnableDebugging(string commandLineArgs)
        {
            try
            {
                StatusCode = DebugSettings.EnableDebugging(PackageFullName, commandLineArgs, IntPtr.Zero);
            }
            catch (Exception e)
            {
                StatusCode = 1;
                Trace.WriteLine("EnableDebugging failed: " + e.ToString());
            }
        }

        public void DisableDebugging()
        {
            try
            {
                StatusCode = DebugSettings.DisableDebugging(PackageFullName);
            }
            catch (Exception e)
            {
                StatusCode = 1;
                Trace.WriteLine("DisableDebugging failed: " + e.ToString());
            }
        }

        public void Suspend()
        {
            try
            {
                StatusCode = DebugSettings.Suspend(PackageFullName);
            }
            catch (Exception e)
            {
                StatusCode = 1;
                Trace.WriteLine("Suspend failed: " + e.ToString());
            }
        }

        public void Resume()
        {
            try
            {
                StatusCode = DebugSettings.Resume(PackageFullName);
            }
            catch (Exception e)
            {
                StatusCode = 1;
                Trace.WriteLine("Resume failed: " + e.ToString());
            }
        }

        public void TerminateAllProcesses()
        {
            try
            {
                StatusCode = DebugSettings.TerminateAllProcesses(PackageFullName);
            }
            catch (Exception e)
            {
                StatusCode = 1;
                Trace.WriteLine("TerminateAllProcesses failed: " + e.ToString());
            }
        }

        public PACKAGE_EXECUTION_STATE GetPackageExecutionState()
        {
            PACKAGE_EXECUTION_STATE packageExecutionState = PACKAGE_EXECUTION_STATE.PES_UNKNOWN;
            try
            {
                StatusCode = DebugSettings.GetPackageExecutionState(PackageFullName, out packageExecutionState);
            }
            catch (Exception e)
            {
                StatusCode = 1;
                Trace.WriteLine("GetPackageExecutionState failed: " + e.ToString());
            }
            return packageExecutionState;
        }
    }
}
