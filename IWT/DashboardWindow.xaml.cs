using IWT.DashboardPages;
using IWT.DBCall;
using IWT.Models;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using SuperSimpleTcp;
using System.Diagnostics;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Data;
using Newtonsoft.Json;
using IWT.FactorySetupPages;
using IWT.ViewModel;
using IWT.Views;
using IWT.Shared;

namespace IWT
{
    /// <summary>
    /// Interaction logic for DashboardWindow.xaml
    /// </summary>
    public partial class DashboardWindow : Window
    {
        public static MasterDBCall masterDBCall = new MasterDBCall();
        public static CommonFunction commonFunction = new CommonFunction();

        AuthStatus authResult;
        RolePriviliege rolePriviliege;
        UserHardwareProfile userHardwareProfile;
        public string TCPServerAddress = "127.0.0.1";
        public int TCPServerPort = 4002;
        public SimpleTcpClient TCP_Client;
        DispatcherTimer LiveTime = new DispatcherTimer();
        List<ShiftMaster> shiftMasters = new List<ShiftMaster>();
        ShiftMaster CurrentShift = new ShiftMaster();
        string weghsoftApp = ConfigurationManager.AppSettings["WeighmentApplicationPath"].ToString();
        string ProcessName = "";

        private readonly ToastViewModel toastViewModel;
        string LastMessage;

        public DashboardWindow(AuthStatus _authResult, RolePriviliege _rolePriviliege, UserHardwareProfile _userHardwareProfile)
        {
            InitializeComponent();
            TCP_Client = new SimpleTcpClient(TCPServerAddress, TCPServerPort);
            toastViewModel = new ToastViewModel();
            authResult = _authResult;
            rolePriviliege = _rolePriviliege;
            userHardwareProfile = _userHardwareProfile;
            Loaded += DashboardWindow_Loaded;
            Unloaded += DashboardWindow_Unloaded;
        }

        private void DashboardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteLog.WriteToFile($"DashboardWindow/DashboardWindow_Loaded");
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                LiveTime.Interval = TimeSpan.FromSeconds(1);
                LiveTime.Tick += timer_Tick;
                LiveTime.Start();
                Task.Run(() =>
                {
                    OpenWeighSoftApplication();
                });
                SetupTCPWeighment();
                GetShiftMasters();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DashboardWindow/DashboardWindow_Loaded/Exception:- " + ex.Message, ex);
            }
        }

        private void DashboardWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteLog.WriteToFile("DashboardWindow/DashboardWindow_Unloaded");
                GC.Collect();
                System.GC.WaitForPendingFinalizers();
                LiveTime.Tick -= timer_Tick;
                if (TCP_Client != null)
                {
                    TCP_Client.Events.Connected -= Connected;
                    TCP_Client.Events.Disconnected -= Disconnected;
                    TCP_Client.Events.DataReceived -= DataReceived;
                    if (TCP_Client.IsConnected)
                    {
                        TCP_Client.Disconnect();
                        TCP_Client.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DashboardWindow/DashboardWindow_Unloaded/Exception:- " + ex.Message, ex);
            }
        }


        private void OpenWeighSoftApplication()
        {
            try
            {

                //Process[] pname = Process.GetProcessesByName(weghsoftApp);
                //if (pname.Length == 0)
                //    MessageBox.Show("nothing");
                //else
                //    MessageBox.Show("run");

                if (string.IsNullOrEmpty(ProcessName))
                {
                    //startWeighApplication();
                }
                else
                {
                    Process[] pname = Process.GetProcessesByName(ProcessName);
                    if (pname.Length == 0)
                    {
                        //startWeighApplication();
                    }
                    else
                    {
                        WriteLog.WriteToFile($"DashboardWindow/OpenWeighSoftApplication/Process {ProcessName} already running");
                    }
                }


            }
            catch (Exception ex)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, ex.Message);
                WriteLog.WriteToFile("DashboardWindow/OpenWeighSoftApplication/Exception:- " + ex.Message, ex);
            }
        }

        private void startWeighApplication()
        {
            try
            {
                WriteLog.WriteToFile("DashboardWindow/startWeighApplication called");
                if (System.IO.File.Exists(weghsoftApp))
                {
                    Process p = new Process();
                    ProcessStartInfo s = new ProcessStartInfo(weghsoftApp)
                    {
                        UseShellExecute = true
                    };
                    p.StartInfo = s;
                    s.CreateNoWindow = true;
                    s.WindowStyle = ProcessWindowStyle.Minimized;
                    p.Start();
                    ProcessName = p.ProcessName;
                    //Process[] pname = Process.GetProcessesByName(ProcessName);
                    //if (pname.Length == 0)
                    //{
                    //    p.Start();
                    //}
                    //else
                    //{
                    //    WriteLog.WriteToFile($"DashboardWindow/startWeighApplication/Process {ProcessName} already running");
                    //}
                    if (TCP_Client.IsConnected)
                    {
                        //TCP_Client.Disconnect();
                        //TCP_Client.Dispose();
                        HideWeighApp();
                    }
                    else
                    {
                        InitializeTCP();
                    }
                }

                //Thread.Sleep(2000);
                //InitializeTCP();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DashboardWindow/startWeighApplication/Exception:- " + ex.Message, ex);
            }

        }


        public void SetupTCPWeighment()
        {
            TCP_Client = new SimpleTcpClient(TCPServerAddress, TCPServerPort);
            TCP_Client.Events.Connected += Connected;
            TCP_Client.Events.Disconnected += Disconnected;
            TCP_Client.Events.DataReceived += DataReceived;
            Task.Run(() =>
            {
                InitializeTCP();
            });
        }


        void timer_Tick(object sender, EventArgs e)
        {
            LiveDateTime.Text = CurrentShift.ShiftName + " | " + DateTime.Now.ToString("yyyy MMMM dd") + " | " + DateTime.Now.ToString("HH:mm:ss");
        }

        public void GetShiftMasters()
        {
            try
            {
                shiftMasters = commonFunction.GetShiftMasters();
                FindCurrentShift();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DashboardWindow/GetShiftMasters/Exception:- " + ex.Message, ex);
            }
        }

        public void FindCurrentShift()
        {
            try
            {
                DateTime today = DateTime.Now;
                CurrentShift = new ShiftMaster();
                shiftMasters.ForEach(x =>
                {
                    var dateTime1 = DateTime.ParseExact(x.FromShift, "h:mm tt", null);
                    var dateTime2 = DateTime.ParseExact(x.ToShift, "h:mm tt", null);
                    if (dateTime1 > dateTime2)
                    {
                        dateTime2 = dateTime2.AddDays(1);
                    }
                    if (today >= dateTime1 && today <= dateTime2)
                    {
                        CurrentShift = x;
                        //CurrentShiftTimings.Content = $"{x.FromShift} - {x.ToShift} ends in ";
                        //CurrentShiftEndTimeLabel.Content = x.ToShift;
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DashboardWindow/FindCurrentShift/Exception:- " + ex.Message, ex);
            }
        }


        private void InitializeTCP()
        {
            try
            {
                Ping pinger = new Ping();
                PingReply reply = pinger.Send($"{TCPServerAddress}");
                if (reply.Status == IPStatus.Success)
                {
                    WriteLog.WriteToFile("DashboardWindow/InitializeTCP :- Ping to IP successfully!!");
                    if (!TCP_Client.IsConnected)
                        TCP_Client.Connect();
                    HideWeighApp();
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DashboardWindow/InitializeTCP :- " + ex.Message);
            }

        }

        public void Connected(object sender, ConnectionEventArgs e)
        {
            //Debug.WriteLine($"*** Server {e.IpPort} connected");
            //this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
            //    WeighmentLabel.Text = "Connected";
            //}));
            WriteLog.WriteToFile($"DashboardWindow/InitializeTCP :- Server {e.IpPort} connected!!");
        }

        public void Disconnected(object sender, ConnectionEventArgs e)
        {
            //Debug.WriteLine($"*** Server {e.IpPort} disconnected");
            this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                WeighmentLabel.Text = "Error";
            }));
            WriteLog.WriteToFile($"DashboardWindow/InitializeTCP :- Server {e.IpPort} disconnected!!");
        }

        public void DataReceived(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            //Debug.WriteLine($"[{e.IpPort}] {Encoding.UTF8.GetString(e.Data)}");
            this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                var weight = Encoding.UTF8.GetString(e.Data);
                string result = Regex.Match(weight, @"-?\d+").Value;
                WeighmentLabel.Text = result;
                WeighmentLabelUnit.Text = "kg";
                //onWeighmentReceived.Invoke(sender, new WeighmentEventArgs(result));
            }));
        }

        //private void Timer1_Tick(object sender, EventArgs e)
        //{
        //    WeighmentLabel.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
        //    {
        //        updateWeight();
        //    }));
        //}

        //public void ScheduleService() //schdule timing
        //{
        //    try
        //    {
        //        //int appMode = 0;
        //        updateWeight();

        //        timer = new Timer(new TimerCallback(SchedularCallback));
        //        //string mode = ConfigurationManager.AppSettings["Mode"].ToUpper(); //details from App config file---"INTERVAL" OR "DAILY"

        //        //Set the Default Time.
        //        DateTime scheduledTime = DateTime.MinValue;

        //        //Get the Interval in Minutes from AppSettings.
        //        int interval = 1;

        //        //Set the Scheduled Time by adding the Interval to Current Time.
        //        scheduledTime = DateTime.Now.AddMinutes(interval);
        //        if (DateTime.Now > scheduledTime)
        //        {
        //            //If Scheduled Time is passed set Schedule for the next Interval.
        //            scheduledTime = scheduledTime.AddSeconds(interval);
        //        }

        //        TimeSpan timeSpan = scheduledTime.Subtract(DateTime.Now);
        //        string schedule = string.Format("{0} day(s) {1} hour(s) {2} minute(s) {3} seconds(s)", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

        //        //ErrorLog.WriteLog("Exalca Invoice Signer Service scheduled to run after: " + schedule);
        //        //Get the difference in Minutes between the Scheduled and Current Time.
        //        int dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);

        //        //Change the Timer's Due Time.
        //        timer.Change(dueTime, Timeout.Infinite);
        //    }
        //    catch (Exception ex)
        //    {
        //        //ErrorLog.WriteLog(ex.Message);

        //        //Stop the Windows Service.
        //        using (System.ServiceProcess.ServiceController serviceController = new System.ServiceProcess.ServiceController("SimpleService"))
        //        {
        //            serviceController.Stop();
        //        }
        //    }
        //}
        //private void SchedularCallback(object e)
        //{
        //   // Starter = true;
        //    this.ScheduleService();
        //}

        private async void Restart_Button_Click(object sender, RoutedEventArgs e)
        {
            var res = await OpenConfirmationDialog();
            if (res)
            {
                var psi = new ProcessStartInfo("shutdown", "/r /t 0");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi);
            }
        }
        private async Task<bool> OpenConfirmationDialog()
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new ConfirmationDialog("restart");

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog1", ClosingEventHandler);

            return (bool)result;
            //check the result...
            ////Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        //private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        //{
        //    ////Debug.WriteLine("You can intercept the closing event, and cancel here.");
        //}

        private async void ChangePassword_Button_Click(object sender, RoutedEventArgs e)
        {
            var view = new ChangePasswordDialog();

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog1", ClosingEventHandler);

        }

        private void ConnectWeighSoft_Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                OpenWeighSoftApplication();
            });
        }
        private async void Zero_Button_Click(object sender, RoutedEventArgs e)
        {
            if (TCP_Client.IsConnected)
            {
                await TCP_Client.SendAsync("<ZERO>");
            }
        }
        public async void HideWeighApp()
        {
            if (TCP_Client.IsConnected)
            {
                await TCP_Client.SendAsync("<HIDE>");
            }
        }

        private async void ChangePasswordPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var view = new ChangePasswordDialog();

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog1", ClosingEventHandler);

        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            var result = eventArgs?.Parameter as ChangePasswordResult;
            //Console.WriteLine(result);
            ////Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }
        private void PackIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            LogInWindow logInWindow = new LogInWindow();
            logInWindow.Show();
            Close();
        }
        private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var view = new ChangePasswordDialog();

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog1", ClosingEventHandler);
        }

        public void GoToMainWindow(string GotoScreen = null)
        {
            //MainWindow mainWindow = new MainWindow(authResult, rolePriviliege, userHardwareProfile, GotoScreen);
            MainWindow mainWindow = new MainWindow();
            //TCP_Client.Disconnect();
            mainWindow.Show();
            Close();
        }
        private void SettingsPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                e.Handled = true;
            }
            else
            {
                if (rolePriviliege.SettingAccess.HasValue && rolePriviliege.SettingAccess.Value)
                {
                    GoToMainWindow("Settings");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "You do not have the permission for the page");
                }
            }

        }

        private void AdminPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                e.Handled = true;
            }
            else
            {
                if (rolePriviliege.AdminAccess.HasValue && rolePriviliege.AdminAccess.Value)
                {
                    GoToMainWindow("Admin");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "You do not have the permission for the page");
                }
            }
        }

        private void ReportsPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                e.Handled = true;
            }
            else
            {
                if (rolePriviliege.ReportAccess.HasValue && rolePriviliege.ReportAccess.Value)
                {
                    GoToMainWindow("Reports");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "You do not have the permission for the page");
                }
            }
        }

        private void MastersPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                e.Handled = true;
            }
            else
            {
                if (rolePriviliege.MasterAccess.HasValue && rolePriviliege.MasterAccess.Value)
                {
                    GoToMainWindow("Masters");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "You do not have the permission for the page");
                }
            }
        }

        private void TransactionPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                e.Handled = true;
            }
            else
            {
                if (rolePriviliege.TransactionAccess.HasValue && rolePriviliege.TransactionAccess.Value)
                {
                    GoToMainWindow("Transaction");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "You do not have the permission for the page");
                }
            }
        }

        //MainWindow mainWindow = new MainWindow(authResult.Role, authResult.UserName);
        //mainWindow.Show();
        //        Close();

        #region FactorySetup
        private void GetFactorySetup()
        {
            AdminDBCall dbCall = new AdminDBCall();
            try
            {
                DataTable table = dbCall.GetAllData("select * from [Factory_Setup]");
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<FactorySetup>>(JSONString);
                if (result.Count > 0 && !result[0].FactoryInstall)
                {
                    OpenFactorySetupDialogs("software");
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DashboardWindow/GetFactorySetup/Exception:- " + ex.Message, ex);
            }
        }
        private async void OpenFactorySetupDialogs(string dialog)
        {
            var view = new object();
            if (dialog == "software")
            {
                view = new SoftwareConfigureDialog();
            }
            else if (dialog == "email")
            {
                view = new EmailSettingsDialog();
            }
            else if (dialog == "weighbridge")
            {
                view = new WeighBridgeDialog();
            }
            else if (dialog == "other")
            {
                view = new OtherSettingsDialog();
            }
            else if (dialog == "cctv")
            {
                view = new CCTVSettingsDialog();
            }
            else if (dialog == "file")
            {
                view = new FileLocationDialog();
            }
            var result = await DialogHost.Show(view, "RootDialog1", ClosingEventHandler);
            if (result != null && (string)result != "completed")
            {
                OpenFactorySetupDialogs((string)result);
            }
            else if ((string)result == "completed")
            {
                var dbCall = new AdminDBCall();
                string query = $@"update [Factory_Setup] set FactoryInstall='TRUE'";
                var res = dbCall.ExecuteQuery(query);
                if (res)
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Factory setup completed !!");
                    DialogHost.CloseDialogCommand.Execute("email", null);
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !!");
                }
            }
        }
        public void ShowMessage(Action<string> message, string name)
        {
            this.Dispatcher.Invoke(() =>
            {
                LastMessage = $"{name}";
                message(LastMessage);
            });
        }
        #endregion

        private void DialogHost_Loaded(object sender, RoutedEventArgs e)
        {
            GetFactorySetup();
        }

    }
}
