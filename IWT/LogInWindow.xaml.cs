using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.Views;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace IWT
{
    /// <summary>
    /// Interaction logic for LogInWindow.xaml
    /// </summary>
    public partial class LogInWindow : Window
    {
        DispatcherTimer LiveTime = new DispatcherTimer();
        CommonFunction commonFunction = new CommonFunction();
        public LogInWindow()
        {
            InitializeComponent();
            Version.Text = "Version-" + ConfigurationManager.AppSettings["Version"].ToString();
            Loaded += LogInWindow_Loaded;
            Unloaded += LogInWindow_Unloaded;
        }

        private void LogInWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            LiveTime.Interval = TimeSpan.FromSeconds(1);
            LiveTime.Tick += timer_Tick;
            LiveTime.Start();
            //username.Text = "Admin";
            //password.Password = "Exalca@123";
            //ShowKeyboard();
        }
        private static void ShowKeyboard()
        {
            //var path64 = @"C:\Windows\winsxs\amd64_microsoft-windows-osk_31bf3856ad364e35_6.1.7600.16385_none_06b1c513739fb828\osk.exe";
            //var path32 = @"C:\windows\system32\osk.exe";
            //var path = (Environment.Is64BitOperatingSystem) ? path64 : path32;
            //Process.Start(path);
            //var path64 = System.IO.Path.Combine(Directory.GetDirectories(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "winsxs"), "amd64_microsoft-windows-osk_*")[0], "osk.exe");
            //var path32 = @"C:\windows\system32\osk.exe";
            //var path = (Environment.Is64BitOperatingSystem) ? path64 : path32;
            //if (File.Exists(path))
            //{
            //    Process.Start(path);
            //}
        }
        private void LogInWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            LiveTime.Stop();
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            LiveDateTime.Text = "Welcome | " + DateTime.Now.ToString("yyyy MMMM dd") + " | " + DateTime.Now.ToString("HH:mm:ss");
        }
        private async void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            var userName = username.Text;
            var passWord = password.Password;
            Authentication auth = new Authentication();
            AuthStatus authResult = await auth.authenticateUser(userName, passWord);
            if (authResult.Status)
            {
                //MainWindow mainWindow = new MainWindow(authResult.Role, authResult.UserName);
                //mainWindow.Show();
                //Close();
                List<RolePriviliege> rolePrivilieges = GetRolesAndPreviledgesByRole(authResult.Role);
                List<UserHardwareProfile> userHardwareProfiles = GetUserHardwareProfileByProfile(authResult.HardwareProfileName);
                if (rolePrivilieges != null && rolePrivilieges.Count > 0)
                {
                    var rolePriviliege = rolePrivilieges[0];
                    UserHardwareProfile hardwareProf = (userHardwareProfiles != null && userHardwareProfiles.Count > 0) ? userHardwareProfiles[0] : null;
                    //DashboardWindow dashboardWindow = new DashboardWindow(authResult, rolePriviliege, hardwareProf);
                    //dashboardWindow.Show();
                    //MainWindow mainWindow = new MainWindow(authResult, rolePriviliege, hardwareProf);
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();

                    WriteLog.WriteToFile("LoginWindow:- Logged in successfully!!");
                    Close();
                }
            }
            else
            {
                showSnackbar(authResult.Message);
            }
        }

        public List<RolePriviliege> GetRolesAndPreviledgesByRole(string Role)
        {
            try
            {
                return commonFunction.GetRolesAndPreviledgesByRole(Role);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Login/GetRolesAndPreviledgesByRole", ex);
                return null;
            }

        }

        public List<UserHardwareProfile> GetUserHardwareProfileByProfile(string HardwareProfile)
        {
            try
            {
                return commonFunction.GetUserHardwareProfileByProfile(HardwareProfile);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Login/GetUserHardwareProfileByProfile", ex);
                return null;
            }

        }

        public void showSnackbar(string message)
        {
            snackbar.MessageQueue?.Enqueue(
                message,
                null,
                null,
                null,
                false,
                true,
                TimeSpan.FromSeconds(3));
        }

        private async void Shutdown_Button_Click(object sender, RoutedEventArgs e)
        {
            var res = await OpenConfirmationDialog();
            if (res)
            {
                var psi = new ProcessStartInfo("shutdown", "/s /t 0");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi);
            }
        }
        private async Task<bool> OpenConfirmationDialog()
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new ConfirmationDialog("shutdown");

            //show the dialog
            var result = await DialogHost.Show(view, "LoginDialogHost", ClosingEventHandler);

            return (bool)result;
            //check the result...
            ////Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            ////Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }
    }
}
