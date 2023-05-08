using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWT.Uninstaller
{
    public static class UninstallPrograms
    {
        public static void StartUninstallation()
        {
            var programsToRemove = new List<string>();
            programsToRemove.Add("IWT-AWS");
            programsToRemove.Add(".NET Framework 4.8");
            programsToRemove.Add("Microsoft Visual C++ 2015-2022 Redistributable");
            programsToRemove.Add("Microsoft Report Builder");
            programsToRemove.Add("Microsoft SQL Server Management Studio");
            //programsToRemove.Add("Microsoft SQL Server");
            
            var programs = ListPrograms();
            var matched = programs.Where(x => programsToRemove.Any(y => x.DisplayName.ToLower().Contains(y.ToLower()))).
                         GroupBy(x => x.DisplayName).Select(y => y.FirstOrDefault()).ToList();

            RemoveSQLServer();
            foreach (var mat in matched)
            {
                if (!string.IsNullOrEmpty(mat.UninstallString) && mat.DisplayName != "IWTUninstaller" && !mat.DisplayName.Contains("IWT-186"))
                {
                    WriteLog.WriteToFile($"Trying to uninstall {mat.DisplayName}");
                    UninstallApplication(mat.UninstallString, mat.DisplayName);
                }
            }
        }

        public static void UninstallApplication(string uninstallString, string DisplayName)
        {
            try
            {

                ProcessStartInfo startInfo = new ProcessStartInfo();

                int indexofexe = uninstallString.IndexOf(".exe");
                //Check for executable existence 
                if (indexofexe > 0)
                {
                    uninstallString = uninstallString.Replace(@"""", string.Empty);

                    //Get exe path 
                    string uninstallerPath = uninstallString.Substring(0, indexofexe + 4);
                    startInfo.FileName = uninstallerPath;

                    //Check for arguments
                    if (uninstallerPath.Length != uninstallString.Length)
                    {
                        string args = uninstallString.Substring(uninstallerPath.Length);
                        if (!string.IsNullOrEmpty(args))
                        {

                            /*If not set to false You will get InvalidOperationException :
                             *The Process object must have the UseShellExecute property set to false in order to use environment variables.*/
                            startInfo.UseShellExecute = false;

                            startInfo.Arguments = args;
                        }
                    }
                }
                //Not tested 
                else
                {
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = "/c " + uninstallString;
                }

                //Start the process
                //Process.Start(startInfo).WaitForExit();
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                    if (exeProcess.HasExited)
                    {
                        //WriteLog.WriteToFile($"{DisplayName} uninstalled sucessfully using UseShellExecute false");
                        WriteLog.WriteToFile($"Uninstall {DisplayName} has exited using UseShellExecute false");
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The requested operation requires elevation"))
                {
                    UninstallApplication1(uninstallString, DisplayName);
                }
                else
                {
                    WriteLog.WriteToFile($"UninstallApplication of {DisplayName} sing UseShellExecute false has error : - " + ex.Message);
                }

            }

        }

        public static void UninstallApplication1(string uninstallString, string DisplayName)
        {
            try
            {

                ProcessStartInfo startInfo = new ProcessStartInfo();

                int indexofexe = uninstallString.IndexOf(".exe");
                //Check for executable existence 
                if (indexofexe > 0)
                {
                    uninstallString = uninstallString.Replace(@"""", string.Empty);

                    //Get exe path 
                    string uninstallerPath = uninstallString.Substring(0, indexofexe + 4);
                    startInfo.FileName = uninstallerPath;

                    //Check for arguments
                    if (uninstallerPath.Length != uninstallString.Length)
                    {
                        string args = uninstallString.Substring(uninstallerPath.Length);
                        if (!string.IsNullOrEmpty(args))
                        {

                            /*If not set to false You will get InvalidOperationException :
                             *The Process object must have the UseShellExecute property set to false in order to use environment variables.*/
                            startInfo.UseShellExecute = true;

                            startInfo.Arguments = args;
                        }
                    }
                }
                //Not tested 
                else
                {
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = "/c " + uninstallString;
                }

                //Start the process
                //Process.Start(startInfo).WaitForExit();

                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                    if (exeProcess.HasExited)
                    {
                        //WriteLog.WriteToFile($"{DisplayName} uninstalled sucessfully using UseShellExecute true");
                        WriteLog.WriteToFile($"Uninstall {DisplayName} has exited using UseShellExecute true");
                    }
                }


            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile($"UninstallApplication of {DisplayName} sing UseShellExecute true has error : - " + ex.Message);
            }

        }

        public static List<InstalledProgram> ListPrograms()
        {
            List<InstalledProgram> programs = new List<InstalledProgram>();

            try
            {
                //ManagementObjectSearcher mos =
                //  new ManagementObjectSearcher("SELECT * FROM Win32_Product WHERE Name='IWT'");
                //foreach (ManagementObject mo in mos.Get())
                //{
                //    try
                //    {
                //        //more properties:
                //        //http://msdn.microsoft.com/en-us/library/windows/desktop/aa394378(v=vs.85).aspx
                //        programs.Add(mo["Name"].ToString());

                //    }
                //    catch (Exception ex)
                //    {
                //        //this program may not have a name property
                //    }
                //}

                //return programs;

                string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

                using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key))
                {
                    if (key != null)
                    {
                        foreach (string subkey_name in key.GetSubKeyNames())
                        {
                            using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                            {
                                InstalledProgram installedProgram = new InstalledProgram();
                                if (!string.IsNullOrEmpty(subkey.GetValue("DisplayName")?.ToString()) && !string.IsNullOrEmpty(subkey.GetValue("UninstallString")?.ToString()))
                                {
                                    installedProgram.DisplayName = subkey.GetValue("DisplayName")?.ToString();
                                    installedProgram.UninstallString = subkey.GetValue("UninstallString")?.ToString();
                                    programs.Add(installedProgram);
                                }

                                //Console.WriteLine(subkey.GetValue("DisplayName"));
                            }
                        }
                    }
                }

                registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

                using (Microsoft.Win32.RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(registry_key))
                {
                    if (key != null)
                    {
                        foreach (string subkey_name in key.GetSubKeyNames())
                        {
                            using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                            {
                                InstalledProgram installedProgram = new InstalledProgram();
                                if (!string.IsNullOrEmpty(subkey.GetValue("DisplayName")?.ToString()) && !string.IsNullOrEmpty(subkey.GetValue("UninstallString")?.ToString()))
                                {
                                    installedProgram.DisplayName = subkey.GetValue("DisplayName")?.ToString();
                                    installedProgram.UninstallString = subkey.GetValue("UninstallString")?.ToString();
                                    programs.Add(installedProgram);
                                }

                                //Console.WriteLine(subkey.GetValue("DisplayName"));
                            }
                        }
                    }
                }

                registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

                using (Microsoft.Win32.RegistryKey key = Registry.CurrentUser.OpenSubKey(registry_key))
                {
                    if (key != null)
                    {
                        foreach (string subkey_name in key.GetSubKeyNames())
                        {
                            using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                            {
                                InstalledProgram installedProgram = new InstalledProgram();
                                if (!string.IsNullOrEmpty(subkey.GetValue("DisplayName")?.ToString()) && !string.IsNullOrEmpty(subkey.GetValue("UninstallString")?.ToString()))
                                {
                                    installedProgram.DisplayName = subkey.GetValue("DisplayName")?.ToString();
                                    installedProgram.UninstallString = subkey.GetValue("UninstallString")?.ToString();
                                    programs.Add(installedProgram);
                                }

                                //Console.WriteLine(subkey.GetValue("DisplayName"));
                            }
                        }
                    }
                }

                registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

                using (Microsoft.Win32.RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).OpenSubKey(registry_key))
                {
                    if (key != null)
                    {
                        foreach (string subkey_name in key.GetSubKeyNames())
                        {
                            using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                            {
                                InstalledProgram installedProgram = new InstalledProgram();
                                if (!string.IsNullOrEmpty(subkey.GetValue("DisplayName")?.ToString()) && !string.IsNullOrEmpty(subkey.GetValue("UninstallString")?.ToString()))
                                {
                                    installedProgram.DisplayName = subkey.GetValue("DisplayName")?.ToString();
                                    installedProgram.UninstallString = subkey.GetValue("UninstallString")?.ToString();
                                    programs.Add(installedProgram);
                                }

                                //Console.WriteLine(subkey.GetValue("DisplayName"));
                            }
                        }
                    }
                }

                //using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MICROSOFT\Microsoft SQL Server"))
                //{
                //    if (key != null)
                //    {
                //        foreach (string subkey_name in key.GetSubKeyNames())
                //        {
                //            using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                //            {
                //                InstalledProgram installedProgram = new InstalledProgram();
                //                if (!string.IsNullOrEmpty(subkey.GetValue("DisplayName")?.ToString()) && !string.IsNullOrEmpty(subkey.GetValue("UninstallString")?.ToString()))
                //                {
                //                    installedProgram.DisplayName = subkey.GetValue("DisplayName")?.ToString();
                //                    installedProgram.UninstallString = subkey.GetValue("UninstallString")?.ToString();
                //                    programs.Add(installedProgram);
                //                }

                //                //Console.WriteLine(subkey.GetValue("DisplayName"));
                //            }
                //        }
                //    }
                //}

                //using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\MICROSOFT\Microsoft SQL Server"))
                //{
                //    if (key != null)
                //    {
                //        foreach (string subkey_name in key.GetSubKeyNames())
                //        {
                //            using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                //            {
                //                InstalledProgram installedProgram = new InstalledProgram();
                //                if (!string.IsNullOrEmpty(subkey.GetValue("DisplayName")?.ToString()) && !string.IsNullOrEmpty(subkey.GetValue("UninstallString")?.ToString()))
                //                {
                //                    installedProgram.DisplayName = subkey.GetValue("DisplayName")?.ToString();
                //                    installedProgram.UninstallString = subkey.GetValue("UninstallString")?.ToString();
                //                    programs.Add(installedProgram);
                //                }

                //                //Console.WriteLine(subkey.GetValue("DisplayName"));
                //            }
                //        }
                //    }
                //}

                return programs;

            }
            catch (Exception ex)
            {
                return programs;
            }
        }

        public static void RemoveSQLServer()
        {
            try
            {
                var SQLServerMediaPath = ConfigurationManager.AppSettings["SQLServerMediaPath"];
                var InstanceName = ConfigurationManager.AppSettings["InstanceName"];
                var FilePath = Path.Combine(SQLServerMediaPath, "setup.exe");
                var Arguments = Path.Combine($" /qs /ACTION=Uninstall /FEATURES=SQLEngine /INSTANCENAME={InstanceName}");
                try
                {
                    if (Directory.Exists(SQLServerMediaPath))
                    {
                        WriteLog.WriteToFile($"Trying to uninstall SQLServer");
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.CreateNoWindow = false;
                        startInfo.UseShellExecute = true;
                        //startInfo.Domain = SQLServerMediaPath;
                        startInfo.FileName = FilePath;
                        startInfo.Arguments = Arguments;
                        //startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                        using (Process exeProcess = Process.Start(startInfo))
                        {
                            exeProcess.WaitForExit();
                            if (exeProcess.HasExited)
                            {
                                // exeProcess.Kill();
                                WriteLog.WriteToFile("Remove SQLServer has exited");
                            }
                        }
                    }
                    else
                    {
                        WriteLog.WriteToFile("SQLServer media file does not exist");
                    }
                }
                catch (Exception ex)
                {
                    WriteLog.WriteToFile("RemoveSQLServer has error :", ex);
                }

            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile($"RemoveSQLServer has error : - " + ex.Message);
            }
        }
    }
    public class InstalledProgram
    {
        public string DisplayName { get; set; }
        public string ApplicationName { get; set; }
        public string UninstallString { get; set; }
    }
}
