using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWTMultiFile.Console
{
    internal class Program
    {
        public static string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static void Main(string[] args)
        {
            InstallNetFramework();
            InstallCRedistribution();
            InstallReportBuilder();
            InstallSQLServer();
            InstallSSMS();
            InstallIWT();
            InstallIWTUninstallar();
            RunSQLScript();
            //RunKioskMode();
        }

        public static void InstallNetFramework()
        {
            var FilePath = Path.Combine(BaseDirectory, "Setup", "ndp48-x86-x64-allos-enu.exe");
            try
            {
                if (File.Exists(FilePath))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = true;
                    startInfo.FileName = FilePath;
                    //startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        exeProcess.WaitForExit();
                        if (exeProcess.HasExited)
                        {
                            // exeProcess.Kill();
                            WriteLog.WriteToFile("InstallNetFramework Completed");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InstallNetFramework", ex);
            }
        }
        public static void InstallCRedistribution()
        {
            var FilePath = Path.Combine(BaseDirectory, "Setup", "VC_redist.x64.exe");
            try
            {
                if (File.Exists(FilePath))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = false;
                    startInfo.FileName = FilePath;
                    //startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        exeProcess.WaitForExit();
                        if (exeProcess.HasExited)
                        {
                            // exeProcess.Kill();
                            WriteLog.WriteToFile("InstallCRedistribution Completed");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InstallCRedistribution", ex);
            }
        }
        public static void InstallReportBuilder()
        {
            Process myProcess = new Process();
            var FilePath = Path.Combine(BaseDirectory, "Setup", "ReportBuilder.msi");
            try
            {
                if (File.Exists(FilePath))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = true;
                    startInfo.FileName = FilePath;
                    //startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        exeProcess.WaitForExit();
                        if (exeProcess.HasExited)
                        {
                            // exeProcess.Kill();
                            WriteLog.WriteToFile("InstallReportBuilder Completed");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InstallReportBuilder", ex);
            }
        }

        public static void InstallSQLServer()
        {
            Process myProcess = new Process();
            var FilePath = Path.Combine(BaseDirectory, "Setup", "SQL2019", "SETUP.EXE");
            try
            {
                if (File.Exists(FilePath))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = true;
                    startInfo.FileName = FilePath;
                    //startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        exeProcess.WaitForExit();
                        if (exeProcess.HasExited)
                        {
                            // exeProcess.Kill();
                            WriteLog.WriteToFile("InstallSQLServer Completed");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InstallSQLServer", ex);
            }
        }

        public static void InstallSSMS()
        {
            Process myProcess = new Process();
            var FilePath = Path.Combine(BaseDirectory, "Setup", "SSMS-Setup-ENU.exe");
            try
            {
                if (File.Exists(FilePath))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = true;
                    startInfo.FileName = FilePath;
                    //startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        exeProcess.WaitForExit();
                        if (exeProcess.HasExited)
                        {
                            // exeProcess.Kill();
                            WriteLog.WriteToFile("InstallSSMS Completed");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InstallSSMS", ex);
            }
        }


        public static void InstallIWT()
        {
            Process myProcess = new Process();
            var FilePath = Path.Combine(BaseDirectory, "Setup", "IWT-AWS", "IWT.Setup.msi");
            try
            {
                if (File.Exists(FilePath))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = true;
                    startInfo.FileName = FilePath;
                    //startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        exeProcess.WaitForExit();
                        if (exeProcess.HasExited)
                        {
                            // exeProcess.Kill();
                            WriteLog.WriteToFile("InstallIWT Completed");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InstallIWT", ex);
            }
        }

        public static void InstallIWTUninstallar()
        {
            Process myProcess = new Process();
            var FilePath = Path.Combine(BaseDirectory, "Setup", "IWTUninstaller", "IWT.Uninstaller.Setup.msi");
            try
            {
                if (File.Exists(FilePath))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = true;
                    startInfo.FileName = FilePath;
                    //startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        exeProcess.WaitForExit();
                        if (exeProcess.HasExited)
                        {
                            // exeProcess.Kill();
                            WriteLog.WriteToFile("InstallIWTUninstallar Completed");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InstallIWTUninstallar", ex);
            }
        }

        public static void RunSQLScript()
        {
            Process myProcess = new Process();
            var FilePath = Path.Combine(BaseDirectory, "Setup", "iwt-aws-script.sql");
            try
            {
                if (File.Exists(FilePath))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = true;
                    startInfo.FileName = FilePath;
                    //startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        //exeProcess.WaitForExit();
                        //if (exeProcess.HasExited)
                        //{
                        //    // exeProcess.Kill();
                        //    WriteLog.WriteToFile("RunSQLScript Completed");
                        //}
                        WriteLog.WriteToFile("RunSQLScript Completed");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InstallSQLServer", ex);
            }
        }

        public static void RunKioskMode()
        {
            Process myProcess = new Process();
            var BatFilePath = Path.Combine(BaseDirectory, "Setup", "kioskMode.bat");
            var FilePath= @"cmd.exe";
            try
            {
                if (File.Exists(BatFilePath))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = true;
                    //startInfo.UseShellExecute = true;
                    startInfo.FileName = FilePath;
                    //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.Verb = "runas";
                    startInfo.Arguments = "/C " + BatFilePath;

                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        exeProcess.WaitForExit();
                        if (exeProcess.HasExited)
                        {
                            // exeProcess.Kill();
                            WriteLog.WriteToFile("RunKioskMode Completed");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("RunKioskMode", ex);
            }
        }
    }
}
