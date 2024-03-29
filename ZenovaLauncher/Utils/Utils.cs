﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Windows;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

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

        public static bool WindowLoaded { get; set; } = false;
        public static List<Tuple<string, string>> ErrorQueue = new List<Tuple<string, string>>();

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

        public static void ShowErrorDialog(string title = default, string message = default)
        {
            if (title != null && message != null)
                ErrorQueue.Add(new Tuple<string, string>(title, message));
            if (WindowLoaded && ErrorQueue.Count > 0)
            {
                Application.Current.Dispatcher.Invoke((Action)async delegate
                {
                    foreach (var error in ErrorQueue)
                    {
                        ErrorDialog errorDialog = new ErrorDialog(error.Item1, error.Item2);
                        await errorDialog.ShowAsync();
                    }
                    ErrorQueue.Clear();
                });
            }
        }

        public static IEnumerable<Package> FindPackages(string familyName)
        {
            return IsElevated ? new PackageManager().FindPackages(familyName) : new PackageManager().FindPackagesForUser(string.Empty, familyName);
        }

        private static readonly SecurityIdentifier ALL_APP_PACKAGES_SID = new SecurityIdentifier("S-1-15-2-1");
        private static readonly FileSystemRights DefaultRights = FileSystemRights.FullControl;

        public static void AddSecurityToFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                FileSecurity fileSecurity = fileInfo.GetAccessControl();
                fileSecurity.AddAccessRule(new FileSystemAccessRule(ALL_APP_PACKAGES_SID, DefaultRights, AccessControlType.Allow));
                fileInfo.SetAccessControl(fileSecurity);
            }
        }

        public static void AddSecurityToDirectory(string dirPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            if (dirInfo.Exists)
            {
                DirectorySecurity dirSecurity = dirInfo.GetAccessControl();
                dirSecurity.AddAccessRule(new FileSystemAccessRule(
                    ALL_APP_PACKAGES_SID,
                    DefaultRights,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));
                dirInfo.SetAccessControl(dirSecurity);
            }
        }

        public static void Empty(string directoryString)
        {
            DirectoryInfo directory = new DirectoryInfo(directoryString);
            foreach (var file in directory.GetFiles()) file.Delete();
            foreach (var dir in directory.GetDirectories()) dir.Delete(true);
        }

        public static void SetModifiedTimeUtcFile(string filePath, DateTime time)
        {
            File.SetCreationTimeUtc(filePath, time);
            File.SetLastWriteTimeUtc(filePath, time);
            File.SetLastWriteTimeUtc(filePath, time);
        }

        public static void SetModifiedTimeUtcRecursive(string dirPath, DateTime time)
        {
            foreach (var file in Directory.GetFiles(dirPath, "*.*", SearchOption.AllDirectories))
            {
                SetModifiedTimeUtcFile(file, time);
            }
        }
    }
}
