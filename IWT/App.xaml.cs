using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
//using VirtualKeyboard.Wpf;
//using WPFTabTipMixedHardware;
//using WPFTabTipMixedHardware.Models;

namespace IWT
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex = null;
        public static bool IsTouchKeyBoardEnabled = true;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            
            //StartUpManager.AddApplicationToCurrentUserStartup();
            MainWindow window = new MainWindow();
            window.Show();
            window.Closed += Window_Closed;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            WriteLog.WriteToFile("App - Application exit window closed");
            Application.Current.Shutdown();
        }

        private void TabTipAutomation_ExceptionCatched(Exception obj)
        {
            WriteLog.WriteToFile("App - TabTipAutomation_ExceptionCatched :- " + obj.Message);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "IWT-AWS";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                //app is already running! Exiting the application  
                WriteLog.WriteToFile("App - application already exists");
                Application.Current.Shutdown();
            }

            var TouchKeyBoardEnabled = ConfigurationManager.AppSettings["TouchKeyBoardEnabled"];
            if (TouchKeyBoardEnabled == "True")
            {
                IsTouchKeyBoardEnabled = true;
            }
            else
            {
                IsTouchKeyBoardEnabled = false;
            }

            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler(TextBox_GotFocus));
            EventManager.RegisterClassHandler(typeof(PasswordBox), PasswordBox.GotFocusEvent, new RoutedEventHandler(PasswordBox_GotFocus));
            EventManager.RegisterClassHandler(typeof(Button), Button.ClickEvent, new RoutedEventHandler(Button_Click));

            base.OnStartup(e);
            Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);
            DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            //TabTipAutomation.BindTo<TextBox>();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (IsTouchKeyBoardEnabled)
                System.Diagnostics.Process.Start("osk.exe");
        }
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (IsTouchKeyBoardEnabled)
                System.Diagnostics.Process.Start("osk.exe");
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (System.Diagnostics.Process process in System.Diagnostics.Process.GetProcessesByName("osk"))
            {
                process.Kill();
            }
        }

        void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                WriteLog.WriteToFile("App - Current_DispatcherUnhandledException :- " + e.Exception.Message);
            }
            e.Handled = true;
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                WriteLog.WriteToFile("App - App_DispatcherUnhandledException :- " + e.Exception.Message);
            }
            e.Handled = true;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // we cannot handle this, but not to worry, I have not encountered this exception yet.  
            // However, you can show/log the exception message and show a message that if the application is terminating or not.  
            WriteLog.WriteToFile("App - CurrentDomain_UnhandledException :- " + e?.ExceptionObject?.ToString());
            var isTerminating = e.IsTerminating;
        }
        protected override void OnExit(ExitEventArgs e)
        {
            foreach (System.Diagnostics.Process process in System.Diagnostics.Process.GetProcessesByName("osk"))
            {
                process.Kill();
            }
            base.OnExit(e);
        }
        private void SetAddRemoveProgramsIcon()
        {
            if (ApplicationDeployment.IsNetworkDeployed && ApplicationDeployment.CurrentDeployment.IsFirstRun)
            {
                try
                {
                    var iconSourcePath = Path.Combine(System.Windows.Forms.Application.StartupPath, "favicon.ico");

                    if (!File.Exists(iconSourcePath)) return;

                    var myUninstallKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
                    if (myUninstallKey == null) return;

                    var mySubKeyNames = myUninstallKey.GetSubKeyNames();
                    foreach (var subkeyName in mySubKeyNames)
                    {
                        var myKey = myUninstallKey.OpenSubKey(subkeyName, true);
                        var myValue = myKey.GetValue("DisplayName");
                        if (myValue != null && myValue.ToString() == "MyProductName") // same as in 'Product name:' field
                        {
                            myKey.SetValue("DisplayIcon", iconSourcePath);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteLog.WriteToFile("App:- " + ex.Message);
                }
            }
        }
    }

    //public static class Initialization
    //{
    //    public static void GetInitialData(this UserControl ob)
    //    {

    //    }
    //}
}
