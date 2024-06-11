using AWS.Communication;
using AWS.Communication.Models;
using IWT.Admin_Pages;
using IWT.AWS.Views;
using IWT.DashboardPages;
using IWT.DBCall;
using IWT.FactorySetupPages;
using IWT.Models;
using IWT.Setting_Pages;
using IWT.Shared;
using IWT.TransactionPages;
using IWT.ViewModel;
using IWT.Views;
using MaterialDesignThemes.Wpf;
using MjpegProcessor;
using Newtonsoft.Json;
using SuperSimpleTcp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TcpClientService;

namespace IWT
{
    public partial class MainWindow : Window
    {
        public static MasterDBCall masterDBCall = new MasterDBCall();
        public static CommonFunction commonFunction = new CommonFunction();
        public static Hardware_profile hardwareProfileClass = new Hardware_profile();
        public static Weighing weightingClass = new Weighing();
        SAPInterfaceCall interfaceCalls = new SAPInterfaceCall();
        private List<TicketDataTemplate> AllCustomFields = new List<TicketDataTemplate>();
        private List<CustomMastereBuilder> CustomMasterCollection = new List<CustomMastereBuilder>();
        private AdminDBCall _dbContext;
        private readonly ToastViewModel toastViewModel;
        public string role;
        public string GotoScreen;
        private MainWindowViewModel _dataContext;
        AuthStatus authResult;
        RolePriviliege rolePriviliege;
        UserHardwareProfile userHardwareProfile;
        string ProcessName = "";
        string LastMessage = "";
        WeighbridgeSettings weighbridgeSetting = new WeighbridgeSettings();
        public string CurrentTransaction;
        List<ShiftMaster> shiftMasters = new List<ShiftMaster>();
        List<SAPDataBackUp> sapDataBackUps = new List<SAPDataBackUp>();
        ShiftMaster CurrentShift = new ShiftMaster();
        DispatcherTimer LiveTime;
        DispatcherTimer MailSMSRetryTimer;
        public static string SystemId;
        Setting_DBCall db = new Setting_DBCall();
        public string HardwareProfile { get; set; }
        public bool VPSEnable { get; set; } = false;


        public static event EventHandler<SelectTicketEventArgs> onSecondTransactionTicketSelection = delegate { };
        public static event EventHandler<SelectTicketEventArgs> onSecondMultiTransactionTicketSelection = delegate { };
        public static event EventHandler<MainWindowButtonEventArgs> onMainWindowButtonClick = delegate { };
        private BrushConverter _bc = new BrushConverter();

        public MainWindow()
        {
            InitializeComponent();
            SystemId = ConfigurationManager.AppSettings["SystemId"];
            this.HardwareProfile = "Admin";
            _dbContext = new AdminDBCall();
            toastViewModel = new ToastViewModel();
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Version.Text = "Version - " + version;
            //Loaded += MainWindow_Loaded;
            //Unloaded += MainWindow_Unloaded;
            DataContext = new MainWindowViewModel();
            _dataContext = DataContext as MainWindowViewModel;
            CurrentTransaction = "Single";
        }

        private void InitializeSystem()
        {
            try
            {
                //Initialization
                systemConfig = commonFunction.GetSystemConfiguration(SystemId);
                if (systemConfig == null)
                {
                    throw new Exception("System Configuration Not Found !!!");
                }
                CheckAutoLogin();
                GetNextTicketNumber();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MainWindow/InitializeSystem/Exception:- ", ex);
            }
        }

        public void InitializeAuthResult()
        {
            Username.Text = authResult.UserName;
            Username1.Text = authResult.UserName;
            role = authResult.Role;
            RfidIndicator1.Visibility = Visibility.Collapsed;
            RfidIndicator2.Visibility = Visibility.Collapsed;
            RfidIndicator3.Visibility = Visibility.Collapsed;
            PLCIndicator.Visibility = Visibility.Collapsed;
            if (systemConfig != null)
                InitializeWeighCommunication();
            if (userHardwareProfile != null && userHardwareProfile.CameraAccess)
            {
                IntializeCamera();
            }
            InitializeRfid();
            if (userHardwareProfile != null && userHardwareProfile.PLC)
            {
                PLCIndicator.Visibility = Visibility.Visible;
                InitializePlc();
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                WriteLog.WriteToFile("MainWindow/MainWindow_Loaded");
                await InitializeDataBaseDetails();
                await GetFactorySetup();

                LiveTime = new DispatcherTimer();
                LiveTime.Interval = TimeSpan.FromSeconds(1);
                LiveTime.Tick += LiveTime_Tick;
                LiveTime.Start();
                MailSMSRetryTimer = new DispatcherTimer();
                MailSMSRetryTimer.Interval = TimeSpan.FromMinutes(10); // Failed Mail,SMS auto retriggering interval
                MailSMSRetryTimer.Tick += MailSMSRetryTimer_Tick; ;
                MailSMSRetryTimer.Start();

                SingleTransaction.onSingleTicketCompletion += SingleTransaction_onSingleTicketCompletion;
                FirstTransaction.onFirstTicketCompletion += FirstTransaction_onFirstTicketCompletion;
                SecondTransaction.onSecondTicketCompletion += SecondTransaction_onSecondTicketCompletion;
                SecondTransaction.onSecondTransactionTicketSelected += SecondTransaction_onSecondTransactionTicketSelected;
                FirstMulti.onFirstMultiTicketCompletion += FirstMulti_onFirstMultiTicketCompletion;
                SecondMulti.onSecondMultiTicketCompletion += SecondMulti_onSecondMultiTicketCompletion;
                SecondMulti.onSecondMultiTicketSelected += SecondMulti_onSecondMultiTicketSelected;
                Master.onMasterWindowLoaded += Master_onMasterWindowLoaded;
                Settings.onSettingWindowLoaded += Settings_onSettingWindowLoaded;
                Admin.onAdminWindowLoaded += Admin_onAdminWindowLoaded;
                SummaryReports.onSummaryReportLoaded += SummaryReports_onSummaryReportLoaded;
                Camera_setting.onCompletion += Camera_setting_onCompletion;
                AWS_setting.onCompletion += AWS_setting_onCompletion;

                InitializeSystem();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MainWindow/MainWindow_Loaded/Exception:- " + ex.Message, ex);
            }
        }

        private void AWS_setting_onCompletion(object sender, GeneralEventHandler e)
        {
            RemovePLC();
            if (userHardwareProfile != null && userHardwareProfile.PLC)
                InitializePlc();
            RemoveRFIDReaders();
            InitializeRfid();
        }

        private void Camera_setting_onCompletion(object sender, GeneralEventHandler e)
        {
            if (CameraTimer != null && CameraTimer.IsEnabled)
            {
                CameraTimer.Tick -= CameraTimer_Tick;
                CameraTimer.Stop();
                CameraTimer = null;
            }
            if (CameraRetryTimer != null && CameraRetryTimer.IsEnabled)
            {
                CameraRetryTimer.Tick -= CameraRetryTimer_Tick;
                CameraRetryTimer.Stop();
                CameraRetryTimer = null;
            }
            if (_mjpeg1 != null)
            {
                _mjpeg1.FrameReady -= _mjpeg1_FrameReady;
                _mjpeg1.Error -= _mjpeg1_Error;
                _mjpeg1.StopStream();
                _mjpeg1 = null;
            }
            if (_mjpeg2 != null)
            {
                _mjpeg2.FrameReady -= _mjpeg2_FrameReady;
                _mjpeg2.Error -= _mjpeg2_Error;
                _mjpeg2.StopStream();
                _mjpeg2 = null;
            }
            if (_mjpeg3 != null)
            {
                _mjpeg3.FrameReady -= _mjpeg3_FrameReady;
                _mjpeg3.Error -= _mjpeg3_Error;
                _mjpeg3.StopStream();
                _mjpeg3 = null;
            }

            IntializeCamera();
        }

        private void MailSMSRetryTimer_Tick(object sender, EventArgs e)
        {
            List<FailedSMS> failedSMSs = commonFunction.GetFailedSMS();
            List<FailedMailSMS> failedMailSMS = commonFunction.GetFailedMailSMS();
            if (failedSMSs.Count <= 5 && failedSMSs.Count > 0)
            {
                SMSTemplate template = commonFunction.GetSMSTemplate();
                foreach (var failedSMS in failedSMSs)
                {
                    Task.Run(async () =>
                    {
                        await commonFunction.RetrySMS(template, failedSMS.Message, failedSMS.ID, failedSMS.SMSRoute);
                    });
                }
            }
            if (failedMailSMS.Count <= 5 && failedMailSMS.Count > 0)
            {
                SMTPSetting setting = commonFunction.GetSMTPDetails();
                MailSetting mailSetting = commonFunction.GetMailDetails();
                foreach (var failedMail in failedMailSMS)
                {
                    Task.Run(async () =>
                    {
                        mailSetting.ToID = failedMail.ToID;
                        await commonFunction.RetryMail(setting, mailSetting, failedMail);
                    });
                }
            }

            Task.Run(() =>
            {
                ClosePendingTickets();
                SendPendingSAPData();
            });
        }

        private async void SendPendingSAPData()
        {
            try
            {
                sapDataBackUps = commonFunction.GetSAPDataBackUpByNoOfRetryAndStatus();
                foreach (SAPDataBackUp sapDataBackUp in sapDataBackUps)
                {
                    if (sapDataBackUp.Type == "gross")
                    {
                        GrossWeight grossWeight = JsonConvert.DeserializeObject<GrossWeight>(sapDataBackUp.Payload);
                        var grossWeightResponse = await interfaceCalls.PostGrossWeight(grossWeight);
                        char[] seperator = { '-' };
                        string[] grossWeightResponsearr = null;
                        grossWeightResponsearr = grossWeightResponse.Split(seperator);
                        var status = grossWeightResponsearr[0];
                        var response = grossWeightResponsearr[1];
                        var sap = new SAPDataBackUp();
                        sap.Id = sapDataBackUp.Id;
                        sap.Trans = sapDataBackUp.Trans;
                        sap.Type = sapDataBackUp.Type;
                        sap.Payload = sapDataBackUp.Payload;
                        sap.Response = response;
                        sap.NoOfRetry = sapDataBackUp.NoOfRetry + 1;
                        if (status == "failed")
                        {
                            sap.Status = "failed";
                        }
                        else if (status == "success")
                        {
                            sap.Status = "success";
                        }
                        commonFunction.UpdateSAPDataBackUpById(sap);
                    }
                    else if (sapDataBackUp.Type == "tare")
                    {
                        TareWeight tareWeight = JsonConvert.DeserializeObject<TareWeight>(sapDataBackUp.Payload);
                        var tareWeightResponse = await interfaceCalls.PostTareWeight(tareWeight);
                        char[] seperator = { '-' };
                        string[] tareWeightResponsearr = null;
                        tareWeightResponsearr = tareWeightResponse.Split(seperator);
                        var status = tareWeightResponsearr[0];
                        var response = tareWeightResponsearr[1];
                        var sap = new SAPDataBackUp();
                        sap.Id = sapDataBackUp.Id;
                        sap.Trans = sapDataBackUp.Trans;
                        sap.Type = sapDataBackUp.Type;
                        sap.Payload = sapDataBackUp.Payload;
                        sap.Response = response;
                        sap.NoOfRetry = sapDataBackUp.NoOfRetry + 1;
                        if (status == "failed")
                        {
                            sap.Status = "failed";
                        }
                        else if (status == "success")
                        {
                            sap.Status = "success";
                        }
                        commonFunction.UpdateSAPDataBackUpById(sap);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SendPendingSAPData", ex);
            }
        }

        private void ClosePendingTickets()
        {
            try
            {
                AdminDBCall db = new AdminDBCall();
                DataTable dt1 = db.GetAllData($"SELECT * FROM Other_Settings where HardwareProfile='{systemConfig.HardwareProfile}'");
                string JSONString = JsonConvert.SerializeObject(dt1);
                var result = JsonConvert.DeserializeObject<List<OtherSettings>>(JSONString);
                if (result != null && result.Count > 0 && result[0] != null)
                {
                    OtherSettings otherSettings = result[0];
                    if (otherSettings.ExpiryDays > 0)
                    {
                        WriteLog.WriteToFile("ClosePendingTickets method called");
                        MasterDBCall masterDBCall = new MasterDBCall();
                        string Query = "ClosePendingTickets";
                        SqlCommand cmd = new SqlCommand(Query);
                        cmd.Parameters.AddWithValue("@ExpiryDays", otherSettings.ExpiryDays);
                        masterDBCall.InsertData(cmd, CommandType.StoredProcedure);
                        WriteLog.WriteToFile($"ClosePendingTickets : - All the expired ({otherSettings.ExpiryDays} days) pending tickets are closed");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("ClosePendingTickets", ex);
            }
        }

        private void LiveTime_Tick(object sender, EventArgs e)
        {
            //LiveDateTime1.Text = "Welcome | " + DateTime.Now.ToString("yyyy MMMM dd") + " | " + DateTime.Now.ToString("HH:mm:ss");
            //LiveDateTime11.Text = DateTime.Now.ToString("yyyy MMMM dd") + " | " + DateTime.Now.ToString("HH:mm:ss");
            //LiveDateTime2.Text = DateTime.Now.ToString("yyyy MMMM dd") + " | " + DateTime.Now.ToString("HH:mm:ss");
            LiveDateTime1.Text = "Welcome | " + DateTime.Now.ToString("MM/dd/yyyy") + " | " + DateTime.Now.ToString("HH:mm:ss");
            LiveDateTime11.Text = DateTime.Now.ToString("MM/dd/yyyy") + " | " + DateTime.Now.ToString("HH:mm:ss");
            LiveDateTime2.Text = DateTime.Now.ToString("MM/dd/yyyy") + " | " + DateTime.Now.ToString("HH:mm:ss");
            if (CurrentShift != null && !string.IsNullOrEmpty(CurrentShift.ToShift))
            {
                //LiveDateTime.Text = CurrentShift.ShiftName + " | " + DateTime.Now.ToString("yyyy MMMM dd") + " | " + DateTime.Now.ToString("HH:mm:ss");
                LiveDateTime.Text = CurrentShift.ShiftName + " | " + DateTime.Now.ToString("MM/dd/yyyy") + " | " + DateTime.Now.ToString("HH:mm:ss");
                var dateTime2 = DateTime.ParseExact(CurrentShift.ToShift, "h:mm tt", null);
                var today = DateTime.Now;
                if (dateTime2 < today)
                {
                    dateTime2 = dateTime2.AddDays(1);
                }
                TimeSpan ts = dateTime2.Subtract(today);
                CurrentShiftEndTimeLabel.Content = ts.ToString("hh':'mm':'ss");
                //CurrentShiftEndTimeLabel.Content = ts.ToString("h' Hours 'm' Minutes 's' Seconds'");
                //CurrentShiftEndTimeLabel.Content = ts.ToString($"hmmss");
            }
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //System.GC.Collect();
                //System.GC.WaitForPendingFinalizers();
                WriteLog.WriteToFile("MainWindow/MainWindow_Unloaded");
                SingleTransaction.onSingleTicketCompletion -= SingleTransaction_onSingleTicketCompletion;
                FirstTransaction.onFirstTicketCompletion -= FirstTransaction_onFirstTicketCompletion;
                SecondTransaction.onSecondTicketCompletion -= SecondTransaction_onSecondTicketCompletion;
                SecondTransaction.onSecondTransactionTicketSelected -= SecondTransaction_onSecondTransactionTicketSelected;
                FirstMulti.onFirstMultiTicketCompletion -= FirstMulti_onFirstMultiTicketCompletion;
                SecondMulti.onSecondMultiTicketCompletion -= SecondMulti_onSecondMultiTicketCompletion;
                SecondMulti.onSecondMultiTicketSelected -= SecondMulti_onSecondMultiTicketSelected;
                Master.onMasterWindowLoaded -= Master_onMasterWindowLoaded;
                Settings.onSettingWindowLoaded -= Settings_onSettingWindowLoaded;
                Admin.onAdminWindowLoaded -= Admin_onAdminWindowLoaded;
                SummaryReports.onSummaryReportLoaded -= SummaryReports_onSummaryReportLoaded;
                AWS_setting.onCompletion -= AWS_setting_onCompletion;
                Camera_setting.onCompletion -= Camera_setting_onCompletion;
                //GC.Collect();
                //GC.WaitForPendingFinalizers();
                LiveTime.Tick -= LiveTime_Tick;
                MailSMSRetryTimer.Tick -= MailSMSRetryTimer_Tick;
                LiveTime.Stop();
                MailSMSRetryTimer.Stop();

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

                DisposeSerialCommunication();


                if (CameraTimer != null && CameraTimer.IsEnabled)
                {
                    CameraTimer.Tick -= CameraTimer_Tick;
                    CameraTimer.Stop();
                }
                if (CameraRetryTimer != null && CameraRetryTimer.IsEnabled)
                {
                    CameraRetryTimer.Tick -= CameraRetryTimer_Tick;
                    CameraRetryTimer.Stop();
                }
                if (_mjpeg1 != null)
                {
                    _mjpeg1.FrameReady -= _mjpeg1_FrameReady;
                    _mjpeg1.Error -= _mjpeg1_Error;
                    _mjpeg1.StopStream();
                }
                if (_mjpeg2 != null)
                {
                    _mjpeg2.FrameReady -= _mjpeg2_FrameReady;
                    _mjpeg2.Error -= _mjpeg2_Error;
                    _mjpeg2.StopStream();
                }
                if (_mjpeg3 != null)
                {
                    _mjpeg3.FrameReady -= _mjpeg3_FrameReady;
                    _mjpeg3.Error -= _mjpeg3_Error;
                    _mjpeg3.StopStream();
                }

            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MainWindow/MainWindow_Unloaded/Exception:- " + ex.Message, ex);
            }
        }


        private void SingleTransaction_onSingleTicketCompletion(object sender, TicketEventArgs e)
        {
            CurrentTransaction = "Single";
            SelectedTicketLabel.Content = "";
            GetNextTicketNumber();
            DisableSecondTransactionLabel();
        }
        private void FirstTransaction_onFirstTicketCompletion(object sender, TicketEventArgs e)
        {
            CurrentTransaction = "First";
            SelectedTicketLabel.Content = "";
            GetNextTicketNumber();
            DisableSecondTransactionLabel();
        }

        private void SecondTransaction_onSecondTicketCompletion(object sender, TicketEventArgs e)
        {
            CurrentTransaction = "Second";
            SelectedTicketLabel.Content = "";
            GetNextTicketNumber();
            EnableSecondTransactionLabel();
        }

        private void SecondTransaction_onSecondTransactionTicketSelected(object sender, SelectTicketEventArgs e)
        {
            //if (!string.IsNullOrEmpty(e.currentTransaction))
            SelectedTicketLabel.Content = e.currentTransaction;
        }


        private void FirstMulti_onFirstMultiTicketCompletion(object sender, TicketEventArgs e)
        {
            CurrentTransaction = "FirstMulti";
            SelectedTicketLabel.Content = "";
            GetNextTicketNumber();
            DisableSecondTransactionLabel();
        }

        private void SecondMulti_onSecondMultiTicketCompletion(object sender, TicketEventArgs e)
        {
            CurrentTransaction = "SecondMulti";
            SelectedTicketLabel.Content = "";
            GetNextTicketNumber();
            EnableSecondTransactionLabel();
        }
        private void SecondMulti_onSecondMultiTicketSelected(object sender, SelectTicketEventArgs e)
        {
            SelectedTicketLabel.Content = e.currentTransaction;
        }


        private void Master_onMasterWindowLoaded(object sender, UserControlEventArgs e)
        {
            DisableTransactionLabel();
        }
        private void Settings_onSettingWindowLoaded(object sender, UserControlEventArgs e)
        {
            DisableTransactionLabel();
        }
        private void Admin_onAdminWindowLoaded(object sender, UserControlEventArgs e)
        {
            DisableTransactionLabel();
        }
        private void SummaryReports_onSummaryReportLoaded(object sender, UserControlEventArgs e)
        {
            DisableTransactionLabel();
        }

        #region CustomMaster
        public void ConstructCustomMasters()
        {
            List<CustomMasterList> customMasterLists = GetAllCustomMasters();
            AllCustomFields = GetAllTicktetDataTemplates();
            foreach (var table in customMasterLists)
            {
                CustomMastereBuilder customMasterBuilder = new CustomMastereBuilder
                {
                    TableName = table.CutomMasterName,
                    Fields = new List<CustomFieldBuilder>()
                };
                List<TicketDataTemplate> customFieldTemplates = AllCustomFields.Where(t => t.F_Table == table.CutomMasterName).ToList();
                foreach (var field in customFieldTemplates)
                {
                    CustomFieldBuilder customFieldBuilder = new CustomFieldBuilder();
                    customFieldBuilder.FieldTable = field.F_Table;
                    customFieldBuilder.FieldName = field.F_FieldName;
                    customFieldBuilder.FieldCaption = field.F_Caption;
                    customFieldBuilder.FieldType = field.F_Type;
                    customFieldBuilder.ControlType = field.ControlType;
                    customFieldBuilder.ControlTableRef = field.ControlTableRef;
                    customFieldBuilder.ControlTable = field.ControlTable;
                    customFieldBuilder.SelectionBasis = field.SelectionBasis;
                    customFieldBuilder.RegName = table.CutomMasterName + "_" + field.F_FieldName;
                    customMasterBuilder.Fields.Add(customFieldBuilder);
                }
                CustomMasterCollection.Add(customMasterBuilder);
            }
        }
        public List<CustomMasterList> GetAllCustomMasters()
        {
            try
            {
                DataTable table = _dbContext.GetAllData($"SELECT * FROM Custom_Master_List");
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<CustomMasterList>>(JSONString);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetAllCustomMasters:" + ex.Message);
                return new List<CustomMasterList>();
            }
        }
        public List<TicketDataTemplate> GetAllTicktetDataTemplates()
        {
            try
            {
                AdminDBCall _dbContext = new AdminDBCall();
                DataTable table = _dbContext.GetAllData($"select * from Ticket_Data_Template");
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<TicketDataTemplate>>(JSONString);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetTicktetDataTemplateData:" + ex.Message);
                return new List<TicketDataTemplate>();
            }
        }
        #endregion

        internal void SwitchScreen(object sender)
        {
            try
            {
                var screen = (UserControl)(((ItemMenu)sender).Screen);
                var header = (string)(((ItemMenu)sender).Header);
                var menuItems = Menu.Children;
                foreach (UserControlMenuItem item in menuItems)
                {
                    var context = item.DataContext as ItemMenu;
                    if (context.Header == header)
                    {
                        var bc = new BrushConverter();
                        var border = item.Content as Border;
                        border.Background = (Brush)bc.ConvertFrom("#FFCE1921");
                        var textBlock = (TextBlock)((StackPanel)border.Child).Children[1];
                        textBlock.Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF");
                    }
                    else
                    {
                        var bc = new BrushConverter();
                        var border = item.Content as Border;
                        border.Background = (Brush)bc.ConvertFrom("#FFFFFFFF");
                        var textBlock = (TextBlock)((StackPanel)border.Child).Children[1];
                        textBlock.Foreground = Brushes.Black;
                    }
                }
                if (screen is Master)
                {
                    screen = new Master(role, rolePriviliege);
                }
                else if (screen is Settings)
                {
                    screen = new Settings(role, rolePriviliege);
                }
                if (screen != null)
                {
                    StackPanelMain.Children.Clear();
                    StackPanelMain.Children.Add(screen);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MainWindow/SwitchScreen/Exception:- " + ex.Message, ex);
            }

        }

        public void EnableSecondTransactionLabel()
        {
            try
            {
                TicketNumberPanel.Visibility = Visibility.Collapsed;
                SelecrTicketPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {

            }
        }
        public void DisableSecondTransactionLabel()
        {
            try
            {
                TicketNumberPanel.Visibility = Visibility.Visible;
                SelecrTicketPanel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {

            }
        }

        public void DisableTransactionLabel()
        {
            try
            {
                TicketNumberPanel.Visibility = Visibility.Collapsed;
                SelecrTicketPanel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {

            }
        }

        public void GetWeighbridgeSettings()
        {
            try
            {
                weighbridgeSetting = commonFunction.GetWeighbridgeSettings();
                if (weighbridgeSetting != null)
                {
                    TCPServerAddress = weighbridgeSetting.Host;
                    TCPServerPort = weighbridgeSetting.Port;
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MainWindow/GetWeighbridgeSettings/Exception:- " + ex.Message, ex);
            }
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
                WriteLog.WriteToFile("MainWindow/GetShiftMasters/Exception:- " + ex.Message, ex);
            }
        }

        public void FindCurrentShift()
        {
            try
            {
                DateTime today = DateTime.Now;
                CurrentShift = new ShiftMaster();
                CurrentShiftTimings.Content = "";
                if (shiftMasters != null)
                {
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
                            CurrentShiftTimings.Content = (x.ShiftName?.ToLower().Contains("shift")).Value ? $"{x.ShiftName} ends in " : $"{x.ShiftName} Shift ends in ";
                            //CurrentShiftTimings.Content = $"{x.FromShift} - {x.ToShift} ends in ";
                            //CurrentShiftEndTimeLabel.Content = x.ToShift;
                            return;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MainWindow/FindCurrentShift/Exception:- " + ex.Message, ex);
            }
        }
        public static double CurrentTicketNumber { get; set; }
        public void GetNextTicketNumber()
        {
            try
            {
                string Query = "select max(TicketNo) as CurrentTicketNo from [Transaction]";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd, CommandType.Text);
                if (table != null)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        var tc = row["CurrentTicketNo"]?.ToString();
                        var CurrentTicketNo = string.IsNullOrEmpty(tc) ? 0 : double.Parse(tc);
                        Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                        {
                            TicketLabel.Content = CurrentTicketNo + 1;
                            CurrentTicketNumber = CurrentTicketNo;
                        }));
                    }
                }

            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MainWindow/GetNextTicketNumber/Exception:- " + ex.Message, ex);
            }

        }
        public void GetRoleData(AuthStatus authResult, string GotoScreen = null)
        {
            Menu.Children?.Clear();
            if (rolePriviliege.TransactionAccess.HasValue && rolePriviliege.TransactionAccess.Value)
            {
                try
                {
                    var item0 = new ItemMenu("Transaction", new Dashboard(authResult, rolePriviliege, userHardwareProfile, weighbridgeSetting), "/Assets/Icons/Transactions.png");
                    Menu.Children.Add(new UserControlMenuItem(item0, this));
                }
                catch (Exception ex)
                {
                    WriteLog.WriteToFile("MainWindow/GetShiftMasters/Exception:- " + ex.Message, ex);
                }
            }
            if (rolePriviliege.MasterAccess.HasValue && rolePriviliege.MasterAccess.Value)
            {
                var item10 = new ItemMenu("Master", new Master(authResult.Role, rolePriviliege), "/Assets/Icons/Master.png");
                Menu.Children.Add(new UserControlMenuItem(item10, this));
            }
            if (rolePriviliege.ReportAccess.HasValue && rolePriviliege.ReportAccess.Value)
            {
                var item13 = new ItemMenu("Reports", new SummaryReports(), "/Assets/Icons/Reports.png");
                Menu.Children.Add(new UserControlMenuItem(item13, this));
            }
            if (rolePriviliege.SettingAccess.HasValue && rolePriviliege.SettingAccess.Value)
            {
                var item11 = new ItemMenu("Settings", new Settings(authResult.Role, rolePriviliege), "/Assets/Icons/Settings.png");
                Menu.Children.Add(new UserControlMenuItem(item11, this));
            }
            if (rolePriviliege.AdminAccess.HasValue && rolePriviliege.AdminAccess.Value)
            {
                var item12 = new ItemMenu("Admin", new Admin(authResult.Role, rolePriviliege), "/Assets/Icons/admin.png");
                Menu.Children.Add(new UserControlMenuItem(item12, this));
            }
            if (rolePriviliege.RFIDAllocationAccess.HasValue && rolePriviliege.RFIDAllocationAccess.Value)
            {
                var rfidAllocationUser = new ItemMenu("Gate Entry", new RFIDAllocationUserControl(authResult.Role, rolePriviliege), "/Assets/Icons/gateEntry.png");
                Menu.Children.Add(new UserControlMenuItem(rfidAllocationUser, this));
            }
            if (rolePriviliege.RFIDUserTableAccess.HasValue && rolePriviliege.RFIDUserTableAccess.Value)
            {
                var rfidAllocationUserTable = new ItemMenu("AWS Transactions", new RFIDAllocationUserTableControl(authResult.Role, rolePriviliege), "/Assets/Icons/databaseImage.png");
                Menu.Children.Add(new UserControlMenuItem(rfidAllocationUserTable, this));
            }
            if (rolePriviliege.GateExitAccess.HasValue && rolePriviliege.GateExitAccess.Value)
            {
                var gateExit = new ItemMenu("Gate Exit", new GateExitControl(authResult.Role, rolePriviliege), "/Assets/Icons/gateExit.png");
                Menu.Children.Add(new UserControlMenuItem(gateExit, this));
            }
            if (rolePriviliege.StoreAccess.HasValue && rolePriviliege.StoreAccess.Value)
            {
                var gateExit = new ItemMenu("Store", new StoreModule(authResult.Role, rolePriviliege), "/Assets/Icons/manage-material.png");
                Menu.Children.Add(new UserControlMenuItem(gateExit, this));
            }
            if (rolePriviliege.PrintAndDeleteAccess.HasValue && rolePriviliege.PrintAndDeleteAccess.Value)
            {
                var duplicateTickets = new ItemMenu("Print / Delete", new PrintAndDeleteTicketControl(authResult.Role, rolePriviliege), "/Assets/Icons/print1.png");
                Menu.Children.Add(new UserControlMenuItem(duplicateTickets, this));
            }
            SwitchScreenOnCondition(authResult, GotoScreen);
        }

        public void SwitchScreenOnCondition(AuthStatus authResult, string GotoScreen = null)
        {
            if (!string.IsNullOrEmpty(GotoScreen))
            {
                switch (GotoScreen)
                {
                    case "Settings":
                        SwitchScreen(new ItemMenu("Settings", new Settings(authResult.Role, rolePriviliege), "/Assets/Icons/Settings.png"));
                        break;
                    case "Admin":
                        SwitchScreen(new ItemMenu("Admin", new Admin(authResult.Role, rolePriviliege), "/Assets/Icons/admin.png"));
                        break;
                    case "Reports":
                        SwitchScreen(new ItemMenu("Reports", new SummaryReports(), "/Assets/Icons/Reports.png"));
                        break;
                    case "Masters":
                        //var materialCustomMaterialFields = AllCustomFields.Where(t => t.F_Table == "Material_Master").ToList();
                        //SwitchScreen(new MaterialMastereUserControl(materialCustomMaterialFields));
                        SwitchScreen(new ItemMenu("Master", new Master(authResult.Role, rolePriviliege), "/Assets/Icons/Master.png"));
                        break;
                    case "Transaction":
                        SwitchScreen(new ItemMenu("Transaction", new Dashboard(authResult, rolePriviliege, userHardwareProfile, weighbridgeSetting), "/Assets/Icons/Transactions.png"));
                        break;
                    case "Gate Entry":
                        SwitchScreen(new ItemMenu("Gate Entry", new RFIDAllocationUserControl(authResult.Role, rolePriviliege), "/Assets/Icons/gateEntry.png"));
                        break;
                    case "AWS Transactions":
                        SwitchScreen(new ItemMenu("AWS Transactions", new RFIDAllocationUserTableControl(authResult.Role, rolePriviliege), "/Assets/Icons/AWS Database.png"));
                        break;
                    case "Gate Exit":
                        SwitchScreen(new ItemMenu("Gate Exit", new GateExitControl(authResult.Role, rolePriviliege), "/Assets/Icons/gateExit.png"));
                        break;
                    case "Store":
                        SwitchScreen(new ItemMenu("Store", new StoreModule(authResult.Role, rolePriviliege), "/Assets/Icons/manage-material.png"));
                        break;
                    case "Print / Delete":
                        SwitchScreen(new ItemMenu("Print / Delete", new PrintAndDeleteTicketControl(authResult.Role, rolePriviliege), "/Assets/Icons/print1.png"));
                        break;
                    default:
                        break;
                }
            }

        }

        private void PackIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            //LogInWindow logInWindow = new LogInWindow();
            //logInWindow.Show();
            //Close();
            RemovePLC();
            RemoveRFIDReaders();
            GoToLoginWindow();
            InitializeSystem();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //LogInWindow logInWindow = new LogInWindow();
            //logInWindow.Show();
            //Close();
            GoToLoginWindow();
        }

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
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);

            return (bool)result;
            //check the result...
            ////Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            ////Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }

        private async void ChangePassword_Button_Click(object sender, RoutedEventArgs e)
        {
            var view = new ChangePasswordDialog();

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);

        }

        private void OpenWeighSoftApplication()
        {
            try
            {
                GetWeighbridgeSettings();

                if (string.IsNullOrEmpty(ProcessName))
                {
                    startWeighApplication();
                }
                else
                {
                    Process[] pname = Process.GetProcessesByName(ProcessName);
                    if (pname.Length == 0)
                    {
                        startWeighApplication();
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
                WriteLog.WriteToFile("MainWindow/OpenWeighSoftApplication/Exception:- " + ex.Message, ex);
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
                    //ProcessName = p.ProcessName;
                    if (TCP_Client != null && TCP_Client.IsConnected)
                    {
                        HideWeighApp();
                    }
                    else
                    {
                        SetupTCPWeighment();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MainWindow/startWeighApplication/Exception:- " + ex.Message, ex);
            }

        }


        private void SelectTicketBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentTransaction == "Second")
            {
                onSecondTransactionTicketSelection.Invoke(this, new SelectTicketEventArgs(CurrentTransaction));
            }
            else if (CurrentTransaction == "SecondMulti")
            {
                onSecondMultiTransactionTicketSelection.Invoke(this, new SelectTicketEventArgs(CurrentTransaction));
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //DashboardWindow mainWindow = new DashboardWindow(authResult, rolePriviliege, userHardwareProfile);
            //TCP_Client.Disconnect();
            //mainWindow.Show();
            //Close();
            //DashboardWindowHost.Visibility = Visibility.Visible;
            //MainWindowHost.Visibility = Visibility.Collapsed;
            //LogInWindowHost.Visibility = Visibility.Collapsed;
            GoToDashboardWindow();
        }

        public void GoToDashboardWindow()
        {
            DashboardWindowHost.Visibility = Visibility.Visible;
            MainWindowHost.Visibility = Visibility.Collapsed;
            LogInWindowHost.Visibility = Visibility.Collapsed;
        }

        public void GoToLoginWindow()
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
            DashboardWindowHost.Visibility = Visibility.Collapsed;
            MainWindowHost.Visibility = Visibility.Collapsed;
            LogInWindowHost.Visibility = Visibility.Visible;
        }

        private void Zero_Button_Click(object sender, RoutedEventArgs e)
        {
            RezeroWeight();
        }
        private void Show_Button_Click(object sender, RoutedEventArgs e)
        {
            ShowWeighApp();
        }
        private void ConnectWeighSoft_Button_Click(object sender, RoutedEventArgs e)
        {
            InitializeWeighCommunication();
        }

        #region Weighment
        public static event EventHandler<WeighmentEventArgs> onWeighmentReceived = delegate { };
        public string TCPServerAddress = "127.0.0.1";
        public int TCPServerPort = 4002;
        public SimpleTcpClient TCP_Client;
        string weghsoftApp = ConfigurationManager.AppSettings["WeighmentApplicationPath"].ToString();
        public void SetupTCPWeighment()
        {
            if (TCP_Client != null)
            {
                TCP_Client.Dispose();
                TCP_Client = null;
            }
            TCP_Client = new SimpleTcpClient(TCPServerAddress, TCPServerPort);
            TCP_Client.Events.Connected += Connected;
            TCP_Client.Events.Disconnected += Disconnected;
            TCP_Client.Events.DataReceived += DataReceived;
            Task.Run(() =>
            {
                InitializeTCP();
            });
        }
        private void InitializeTCP()
        {
            try
            {
                WriteLog.WriteToFile("MainWindow/InitializeTCP started");
                if (TCP_Client != null && !TCP_Client.IsConnected)
                    TCP_Client.Connect();
                HideWeighApp();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MainWindow/InitializeTCP :- " + ex.Message);
            }
        }
        public void Connected(object sender, ConnectionEventArgs e)
        {
            WriteLog.WriteToFile($"MainWindow/InitializeTCP :- Server {e.IpPort} connected!!");
        }

        public void Disconnected(object sender, ConnectionEventArgs e)
        {
            onWeighmentReceived.Invoke(sender, new WeighmentEventArgs("Error"));
            WriteLog.WriteToFile($"MainWindow/InitializeTCP :- Server {e.IpPort} disconnected!!");
        }

        public void DataReceived(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            var weight = Encoding.UTF8.GetString(e.Data);
            string result = Regex.Match(weight, @"-?\d+").Value;

            this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                WeighmentLabel.Text = result;
                WeighmentLabelUnit.Text = "kg";
                //onWeighmentReceived.Invoke(sender, new WeighmentEventArgs(result));
            }));

            onWeighmentReceived.Invoke(sender, new WeighmentEventArgs(result));
        }

        //private async void ChangePassword_Button_Click(object sender, RoutedEventArgs e)
        //{
        //    var view = new ChangePasswordDialog();

        //    //show the dialog
        //    var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);

        //}

        public async void RezeroWeight()
        {
            if (TCP_Client.IsConnected)
            {
                await TCP_Client.SendAsync("<ZERO>");
            }
        }
        public async void ShowWeighApp()
        {
            if (TCP_Client.IsConnected)
            {
                await TCP_Client.SendAsync("<VISIBLE>");
            }
        }
        public async void HideWeighApp()
        {
            if (TCP_Client.IsConnected)
            {
                await TCP_Client.SendAsync("<HIDE>");
            }
        }
        #endregion

        #region Camera
        public static event EventHandler<CameraEventArgs> onImage1Recieved = delegate { };
        public static event EventHandler<CameraEventArgs> onImage2Recieved = delegate { };
        public static event EventHandler<CameraEventArgs> onImage3Recieved = delegate { };
        DispatcherTimer CameraTimer;
        DispatcherTimer CameraRetryTimer;
        List<CCTVSettings> cCTVSettings = new List<CCTVSettings>();
        private MjpegDecoder _mjpeg1;
        private MjpegDecoder _mjpeg2;
        private MjpegDecoder _mjpeg3;
        private List<bool> _cameraStatus = new List<bool> { true, true, true };
        private List<DateTime> _frameTime = new List<DateTime> { DateTime.Now, DateTime.Now, DateTime.Now };
        private List<CancellationTokenSource> cts = new List<CancellationTokenSource> { new CancellationTokenSource(), new CancellationTokenSource(), new CancellationTokenSource() };

        public void IntializeCamera()
        {
            _cameraStatus = new List<bool> { true, true, true };
            _frameTime = new List<DateTime> { DateTime.Now, DateTime.Now, DateTime.Now };
            //WriteLog.WriteToFile("MainWindow/IntializeCamera called");
            CameraTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            CameraTimer.Tick += CameraTimer_Tick;
            CameraRetryTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            CameraRetryTimer.Tick += CameraRetryTimer_Tick;
            CameraRetryTimer.Start();
            _mjpeg1 = new MjpegDecoder();
            _mjpeg2 = new MjpegDecoder();
            _mjpeg3 = new MjpegDecoder();
            _mjpeg1.FrameReady += _mjpeg1_FrameReady;
            _mjpeg2.FrameReady += _mjpeg2_FrameReady;
            _mjpeg3.FrameReady += _mjpeg3_FrameReady;
            _mjpeg1.Error += _mjpeg1_Error;
            _mjpeg2.Error += _mjpeg2_Error;
            _mjpeg3.Error += _mjpeg3_Error;
            GetCCTVSettings();
        }

        private void CameraRetryTimer_Tick(object sender, EventArgs e)
        {
            //RetryCameraStream();
        }

        private void _mjpeg3_Error(object sender, MjpegProcessor.ErrorEventArgs e)
        {
            _cameraStatus[2] = false;
            int width = 128;
            int height = width;
            int stride = width / 8;
            byte[] pixels = new byte[height * stride];
            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
            colors.Add(System.Windows.Media.Colors.Azure);
            BitmapPalette myPalette = new BitmapPalette(colors);
            BitmapSource img = BitmapSource.Create(
                                     width, height,
                                     96, 96,
                                     PixelFormats.Indexed1,
                                     myPalette,
                                     pixels,
                                     stride);
            onImage3Recieved.Invoke(sender, new CameraEventArgs(img));
            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Camera 3 is offline!!");
            _mjpeg3.StopStream();
        }

        private void _mjpeg2_Error(object sender, MjpegProcessor.ErrorEventArgs e)
        {
            _cameraStatus[1] = false;
            int width = 128;
            int height = width;
            int stride = width / 8;
            byte[] pixels = new byte[height * stride];
            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
            colors.Add(System.Windows.Media.Colors.Azure);
            BitmapPalette myPalette = new BitmapPalette(colors);
            BitmapSource img = BitmapSource.Create(
                                     width, height,
                                     96, 96,
                                     PixelFormats.Indexed1,
                                     myPalette,
                                     pixels,
                                     stride);
            onImage2Recieved.Invoke(sender, new CameraEventArgs(img));
            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Camera 2 is offline!!");
            _mjpeg2.StopStream();
        }

        private void _mjpeg1_Error(object sender, MjpegProcessor.ErrorEventArgs e)
        {
            _cameraStatus[0] = false;
            int width = 128;
            int height = width;
            int stride = width / 8;
            byte[] pixels = new byte[height * stride];
            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
            colors.Add(System.Windows.Media.Colors.Azure);
            BitmapPalette myPalette = new BitmapPalette(colors);
            BitmapSource img = BitmapSource.Create(
                                     width, height,
                                     96, 96,
                                     PixelFormats.Indexed1,
                                     myPalette,
                                     pixels,
                                     stride);
            onImage1Recieved.Invoke(sender, new CameraEventArgs(img));
            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Camera 1 is offline!!");
            _mjpeg1.StopStream();
        }

        private async void CameraTimer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < cCTVSettings.Count; i++)
            {
                if (cCTVSettings[i].CameraType == "Hikvision Camera" && cCTVSettings[i].Enable)
                {
                    await CaptureSnap(cCTVSettings[i], cts[i].Token);
                    cts[i].CancelAfter(5000);
                }
            }
        }

        private async Task CaptureSnap(CCTVSettings setting, CancellationToken token)
        {
            if (await PingIP(setting.CaptureURL) == IPStatus.Success)
            {
                await GetSnapshotFromHikVision(setting, token);
            }
            else
            {
                SetEmptyImage(setting);
            }
        }

        private void _mjpeg1_FrameReady(object sender, FrameReadyEventArgs e)
        {
            var imgBytes = e.FrameBuffer;
            MemoryStream memstream = new MemoryStream(imgBytes);
            System.Drawing.Bitmap img = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memstream);
            onImage1Recieved.Invoke(sender, new CameraEventArgs(Bitmap2BitmapImage(img)));
            _cameraStatus[0] = true;
            _frameTime[0] = DateTime.Now;
        }
        private void _mjpeg2_FrameReady(object sender, FrameReadyEventArgs e)
        {
            var imgBytes = e.FrameBuffer;
            MemoryStream memstream = new MemoryStream(imgBytes);
            System.Drawing.Bitmap img = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memstream);
            onImage2Recieved.Invoke(sender, new CameraEventArgs(Bitmap2BitmapImage(img)));
            _cameraStatus[1] = true;
            _frameTime[1] = DateTime.Now;
        }
        private void _mjpeg3_FrameReady(object sender, FrameReadyEventArgs e)
        {
            var imgBytes = e.FrameBuffer;
            MemoryStream memstream = new MemoryStream(imgBytes);
            System.Drawing.Bitmap img = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memstream);
            onImage3Recieved.Invoke(sender, new CameraEventArgs(Bitmap2BitmapImage(img)));
            _cameraStatus[2] = true;
            _frameTime[2] = DateTime.Now;
        }
        public void GetCCTVSettings()
        {
            try
            {
                cCTVSettings = commonFunction.GetCCTVSettings(systemConfig.HardwareProfile);
                foreach (var cam in cCTVSettings)
                {
                    if (cam.Enable)
                    {
                        PingCameraIPAddress(cam);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("FirstTransaction/GetShiftMasters/Exception:- " + ex.Message, ex);
            }
        }
        private async void PingCameraIPAddress(CCTVSettings settings)
        {
            try
            {
                if (!string.IsNullOrEmpty(settings.IPAddress))
                {
                    var url = settings.IPAddress;
                    Uri myUri = new Uri(url);
                    var ip = Dns.GetHostAddresses(myUri.Host)[0];
                    Ping pingSender = new Ping();
                    var reply = await pingSender.SendPingAsync(ip, 100);

                    if (reply.Status == IPStatus.Success)
                    {
                        WriteLog.WriteToFile($"SingleTransaction/PingCameraIPAddress:- Ping successfull for Cam{settings.RecordID}");
                        StartCameraStreaming(settings);
                    }
                    else
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, $"Camera {settings.RecordID} is not pinging!");
                    }
                }
                else
                {
                    WriteLog.WriteToFile($"SingleTransaction/PingCameraIPAddress:- Invalid ip for Cam{settings.RecordID}");
                }
            }
            catch (Exception ex1)
            {
                string LineNumber = "";
                try
                {
                    WriteLog.WriteToFile("PingCameraIPAddress:-:- LineNumber: " + LineNumber + " Error : " + ex1.Message);
                }
                catch (Exception)
                {
                    WriteLog.WriteToFile("PingCameraIPAddress:- LineNumber: " + LineNumber + " Error : " + ex1.Message);
                }
            }
        }
        public void StartCameraStreaming(CCTVSettings settings)
        {
            try
            {
                if (settings.CameraType == "Hikvision Camera" && settings.Enable)
                {
                    if (!CameraTimer.IsEnabled)
                    {
                        CameraTimer.Start();
                    }
                }
                else
                {
                    if (settings.RecordID == 1 && settings.Enable)
                    {
                        _mjpeg1.ParseStream(new Uri(settings.IPAddress), settings.CameraUserName, settings.CameraPassword);
                        WriteLog.WriteToFile($"MainWindow/StartCameraStreaming/Cam{settings.RecordID}:-stream started");
                    }
                    else if (settings.RecordID == 2 && settings.Enable)
                    {
                        _mjpeg2.ParseStream(new Uri(settings.IPAddress), settings.CameraUserName, settings.CameraPassword);
                        WriteLog.WriteToFile($"MainWindow/StartCameraStreaming/Cam{settings.RecordID}:-stream started");
                    }
                    else if (settings.RecordID == 3 && settings.Enable)
                    {
                        _mjpeg3.ParseStream(new Uri(settings.IPAddress), settings.CameraUserName, settings.CameraPassword);
                        WriteLog.WriteToFile($"MainWindow/StartCameraStreaming/Cam{settings.RecordID}:-stream started");
                    }

                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("StartCameraStreaming:- " + ex.Message);
            }
        }

        public async Task GetSnapshotFromHikVision(CCTVSettings ccTV, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                byte[] buffer = new byte[300000];
                int read, total = 0;
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(ccTV.CaptureURL);
                req.Credentials = new NetworkCredential(ccTV.CameraUserName, ccTV.CameraPassword);
                WebResponse resp = await req.GetResponseAsync();
                Stream stream = resp.GetResponseStream();
                while ((read = stream.Read(buffer, total, 1000)) != 0)
                {
                    total += read;
                }
                MemoryStream memstream = new MemoryStream(buffer, 0, total);
                try
                {
                    System.Drawing.Bitmap img = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memstream);
                    if (ccTV.RecordID == 1)
                    {
                        onImage1Recieved.Invoke("MainWindow", new CameraEventArgs(Bitmap2BitmapImage(img)));
                    }
                    else if (ccTV.RecordID == 2)
                    {
                        onImage2Recieved.Invoke("MainWindow", new CameraEventArgs(Bitmap2BitmapImage(img)));
                    }
                    else if (ccTV.RecordID == 3)
                    {
                        onImage3Recieved.Invoke("MainWindow", new CameraEventArgs(Bitmap2BitmapImage(img)));
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (OperationCanceledException)
            {
                if (ccTV.RecordID == 1)
                    cts[0] = new CancellationTokenSource();
                else if (ccTV.RecordID == 2)
                    cts[1] = new CancellationTokenSource();
                else
                    cts[2] = new CancellationTokenSource();
                SetEmptyImage(ccTV);
            }
            catch (Exception ex)
            {
                SetEmptyImage(ccTV);
                WriteLog.WriteToFile("GetSnapshotFromHikVision:- " + ex.Message);
            }
        }

        private void SetEmptyImage(CCTVSettings ccTV)
        {
            int width = 128;
            int height = width;
            int stride = width / 8;
            byte[] pixels = new byte[height * stride];
            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
            colors.Add(System.Windows.Media.Colors.Azure);
            BitmapPalette myPalette = new BitmapPalette(colors);
            BitmapSource img = BitmapSource.Create(
                                     width, height,
                                     96, 96,
                                     PixelFormats.Indexed1,
                                     myPalette,
                                     pixels,
                                     stride);
            if (ccTV.RecordID == 1)
            {
                onImage1Recieved.Invoke("MainWindow", new CameraEventArgs(img));
            }
            else if (ccTV.RecordID == 2)
            {
                onImage2Recieved.Invoke("MainWindow", new CameraEventArgs(img));
            }
            else if (ccTV.RecordID == 3)
            {
                onImage3Recieved.Invoke("MainWindow", new CameraEventArgs(img));
            }
        }

        public async void RetryCameraStream()
        {
            for (int i = 0; i < cCTVSettings.Count; i++)
            {
                if (cCTVSettings[i].CameraType != "Hikvision Camera" && _cameraStatus[i] && !IsFrameUpdating(i) && cCTVSettings[i].Enable)
                {
                    _cameraStatus[i] = false;
                    SetEmptyImage(cCTVSettings[i]);
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, $"Camera {cCTVSettings[i].RecordID} is offline!!");
                    WriteLog.WriteToFile($"MainWindow/RetryCameraStream/Cam{cCTVSettings[i].RecordID}:-stream is offline");
                }
                if (!CameraTimer.IsEnabled && cCTVSettings[i].Enable)
                {
                    CameraTimer.Start();
                }
            }
            if (!_cameraStatus[0] && cCTVSettings[0].Enable && await PingIP(cCTVSettings[0].IPAddress) == IPStatus.Success)
            {
                _mjpeg1.ParseStream(new Uri(cCTVSettings[0].IPAddress), cCTVSettings[0].CameraUserName, cCTVSettings[0].CameraPassword);
                WriteLog.WriteToFile($"MainWindow/RetryCameraStream/Cam{cCTVSettings[0].RecordID}:-stream started");
            }
            if (!_cameraStatus[1] && cCTVSettings[1].Enable && await PingIP(cCTVSettings[1].IPAddress) == IPStatus.Success)
            {
                _mjpeg2.ParseStream(new Uri(cCTVSettings[1].IPAddress), cCTVSettings[1].CameraUserName, cCTVSettings[1].CameraPassword);
                WriteLog.WriteToFile($"MainWindow/RetryCameraStream/Cam{cCTVSettings[1].RecordID}:-stream started");
            }
            if (!_cameraStatus[2] && cCTVSettings[2].Enable && await PingIP(cCTVSettings[2].IPAddress) == IPStatus.Success)
            {
                _mjpeg3.ParseStream(new Uri(cCTVSettings[2].IPAddress), cCTVSettings[2].CameraUserName, cCTVSettings[2].CameraPassword);
                WriteLog.WriteToFile($"MainWindow/RetryCameraStream/Cam{cCTVSettings[2].RecordID}:-stream started");
            }
        }

        private bool IsFrameUpdating(int i)
        {
            if (DateTime.Now.Subtract(_frameTime[i]).TotalSeconds > 3)
            {
                return false;
            }
            return true;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private BitmapSource Bitmap2BitmapImage(System.Drawing.Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapSource retval;

            try
            {
                retval = Imaging.CreateBitmapSourceFromHBitmap(
                             hBitmap,
                             IntPtr.Zero,
                             Int32Rect.Empty,
                             BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return retval;
        }
        private async Task<IPStatus> PingIP(string ipAddress)
        {
            var url = ipAddress;
            Uri myUri = new Uri(url);
            var ip = Dns.GetHostAddresses(myUri.Host)[0];
            Ping pingSender = new Ping();
            var reply = await pingSender.SendPingAsync(ip);
            return reply.Status;
        }
        #endregion


        #region DashboardWindow

        private async void ChangePasswordPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var view = new ChangePasswordDialog();

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);

        }
        private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var view = new ChangePasswordDialog();

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
        }

        public void GoToMainWindow(string GotoScreen = null)
        {
            DashboardWindowHost.Visibility = Visibility.Collapsed;
            MainWindowHost.Visibility = Visibility.Visible;
            LogInWindowHost.Visibility = Visibility.Collapsed;

            SwitchScreenOnCondition(authResult, GotoScreen);
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

        private void RFIDAllocationUserPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                e.Handled = true;
            }
            else
            {
                if (rolePriviliege.RFIDAllocationAccess.HasValue && rolePriviliege.RFIDAllocationAccess.Value)
                {
                    GoToMainWindow("Gate Entry");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "You do not have the permission for the page");
                }
            }
        }

        private void RFIDAllocationTablePanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                e.Handled = true;
            }
            else
            {
                if (rolePriviliege.RFIDUserTableAccess.HasValue && rolePriviliege.RFIDUserTableAccess.Value)
                {
                    GoToMainWindow("AWS Transactions");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "You do not have the permission for the page");
                }
            }
        }

        private void GateExitPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                e.Handled = true;
            }
            else
            {
                if (rolePriviliege.GateExitAccess.HasValue && rolePriviliege.GateExitAccess.Value)
                {
                    GoToMainWindow("Gate Exit");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "You do not have the permission for the page");
                }
            }
        }


        #endregion

        #region LogInWindow


        public async void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            if(systemConfig==null)
                systemConfig = commonFunction.GetSystemConfiguration(SystemId);
            await ExcuteLogin();
        }

        public async Task ExcuteLogin(bool autoLogin = false, string user = null, string pass = null)
        {
            try
            {
                var userName = username.Text;
                var passWord = password.Password;
                if (autoLogin)
                {
                    userName = user;
                    passWord = pass;
                }

                //CloseOnScreenKeyboard();
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passWord))
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill user name and password");
                }
                else
                {
                    Authentication auth = new Authentication();
                    authResult = await auth.authenticateUser(userName, passWord);
                    if (authResult.Status)
                    {
                        TouchKeyboardEnableCheck();
                        List<RolePriviliege> rolePrivilieges = GetRolesAndPreviledgesByRole(authResult.Role);
                        List<UserHardwareProfile> userHardwareProfiles = GetUserHardwareProfileByProfile(authResult.HardwareProfileName);
                        if (rolePrivilieges != null && rolePrivilieges.Count > 0)
                        {
                            rolePriviliege = rolePrivilieges[0];
                            userHardwareProfile = (userHardwareProfiles != null && userHardwareProfiles.Count > 0) ? userHardwareProfiles[0] : null;
                            WriteLog.WriteToFile("LoginWindow:- Logged in successfully!!");
                            username.Text = "";
                            password.Password = "";
                            InitializeAuthResult();
                            if (rolePrivilieges[0].Role == "AWS")
                            {
                                GotoScreen = "Transaction";
                                GoToMainWindow(GotoScreen);
                            }
                            else if (rolePrivilieges[0].Role == "Gate")
                            {
                                GotoScreen = "Gate Entry";
                                GoToMainWindow(GotoScreen);
                            }
                            else if (rolePrivilieges[0].Role == "Store")
                            {
                                GotoScreen = "Store";
                                GoToMainWindow(GotoScreen);
                            }
                            else
                            {
                                GoToDashboardWindow();
                            }
                            GetRoleData(authResult, GotoScreen);
                            GetShiftMasters();
                            commonFunction.RemoveDuplicateMaterials();
                            commonFunction.RemoveDuplicateSuppliers();
                            commonFunction.GetMaterialMasters();
                            commonFunction.GetSupplierMasters();
                        }
                    }
                    else
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, authResult.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Login/GetRolesAndPreviledgesByRole", ex);
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
            var res = await OpenConfirmationDialog1();
            if (res)
            {
                var psi = new ProcessStartInfo("shutdown", "/s /t 0");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi);
            }
        }
        private async Task<bool> OpenConfirmationDialog1()
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new ConfirmationDialog("shutdown");

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);

            return (bool)result;
            //check the result...
            ////Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }



        #endregion


        #region FactorySetup

        private async Task InitializeDataBaseDetails()
        {
            try
            {
                var IsDataBaseDetailsAdded = ConfigurationManager.AppSettings["IsDataBaseDetailsAdded"];
                if (IsDataBaseDetailsAdded == "false")
                {
                    var view = new DataBaseDetailsDialog();
                    var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DashboardWindow/InitializeDataBaseDetails/Exception:- " + ex.Message, ex);
            }
        }

        private async Task GetFactorySetup()
        {
            try
            {
                if (string.IsNullOrEmpty(SystemId))
                {
                    await OpenFactorySetupDialogs("database");
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DashboardWindow/GetFactorySetup/Exception:- " + ex.Message, ex);
            }
        }
        private async Task OpenFactorySetupDialogs(string dialog)
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
            else if (dialog == "system")
            {
                view = new SystemConfigureDialog();
            }
            else if (dialog == "database")
            {
                view = new DataBaseDetailsDialog();
            }
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            if (result != null && (string)result != "completed")
            {
                await OpenFactorySetupDialogs((string)result);
            }
            else if ((string)result == "completed")
            {
                //Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                //configuration.AppSettings.Settings["IsFactorySetupCompleted"].Value = "true";
                //configuration.Save(ConfigurationSaveMode.Modified, true);
                //ConfigurationManager.RefreshSection("appSettings");
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Factory setup completed !!");
                SystemId = ConfigurationManager.AppSettings["SystemId"];
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

        public void InitializeWeighCommunication()
        {
            var awsConfig = commonFunction.GetAWSConfiguration(systemConfig.HardwareProfile);
            if (awsConfig != null)
            {
                if (awsConfig.WeightCommunication == "Serial")
                {
                    Task.Run(() =>
                    {
                        InitializeSerialCommunication();
                    });
                }
                else
                    OpenWeighSoftApplication();
            }
            else
            {
                OpenWeighSoftApplication();
            }
        }

        public static SystemConfigurations systemConfig;
        private void CheckAutoLogin()
        {
            if (systemConfig != null)
            {
                var awsConfig = commonFunction.GetAWSConfiguration(systemConfig.HardwareProfile);
                if (awsConfig != null)
                {
                    if ((bool)awsConfig.IsAutoLogin)
                    {
                        string password = GetAutoLoginPassword(awsConfig.AutoLoginUser);
                        this.Dispatcher.BeginInvoke(new Action(async () => { await ExcuteLogin(true, awsConfig.AutoLoginUser, password); }));
                    }
                    VPSEnable = awsConfig.VPSEnable ?? false;
                }
            }
        }

        private string GetAutoLoginPassword(string username)
        {
            ManageUser manageUser = new ManageUser();
            DataTable table = _dbContext.GetAllData($"SELECT * FROM [User_Management] WHERE UserName='{username}'");
            if (table != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    var password = (row["Password"].ToString());
                    return manageUser.Decrypt(password, true);
                }
            }
            return "";
        }

        public void CloseOnScreenKeyboard()
        {
            try
            {
                foreach (System.Diagnostics.Process process in System.Diagnostics.Process.GetProcessesByName("osk"))
                {
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile($"MainWindow/CloseOnScreenKeyboard", ex);
            }
        }

        private void TouchKeyboardToggleBtn_Click(object sender, RoutedEventArgs e)
        {
            App.IsTouchKeyBoardEnabled = !App.IsTouchKeyBoardEnabled;

            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (App.IsTouchKeyBoardEnabled)
            {
                configuration.AppSettings.Settings["TouchKeyBoardEnabled"].Value = "True";
            }
            else
            {
                configuration.AppSettings.Settings["TouchKeyBoardEnabled"].Value = "False";
            }

            configuration.Save(ConfigurationSaveMode.Modified, true);
            ConfigurationManager.RefreshSection("appSettings");

            TouchKeyboardEnableCheck();
        }

        private void TouchKeyboardEnableCheck()
        {
            var TouchKeyBoardEnabled = ConfigurationManager.AppSettings["TouchKeyBoardEnabled"];
            if (TouchKeyBoardEnabled == "True")
            {
                App.IsTouchKeyBoardEnabled = true;
                TouchKeyboardToggleBtn.Content = "Disable TouchKeyboard";
            }
            else
            {
                App.IsTouchKeyBoardEnabled = false;
                TouchKeyboardToggleBtn.Content = "Enable TouchKeyboard";
            }

        }

        #region RFID
        public TcpClient rfidClient1;
        public TcpClient rfidClient2;
        public TcpClient rfidClient3;
        public static event EventHandler<RfidEventArgs> onRfid1Received = delegate { };
        public static event EventHandler<RfidEventArgs> onRfid2Received = delegate { };
        public static event EventHandler<RfidEventArgs> onRfid3Received = delegate { };
        public void InitializeRfid()
        {
            try
            {
                if (userHardwareProfile != null && userHardwareProfile.RFIDReader1)
                {
                    RfidIndicator1.Visibility = Visibility.Visible;
                    //var RFIDReader1 = commonFunction.GetRFIDReaderMasterById("1");
                    var RFIDReader1 = commonFunction.GetRFIDReaderMasterByIdAndHardwareProfile("1", systemConfig.HardwareProfile);
                    if (RFIDReader1 != null && (bool)RFIDReader1.IsEnable)
                    {
                        if (rfidClient1 == null)
                        {
                            rfidClient1 = new TcpClient(RFIDReader1.IP, Convert.ToInt32(RFIDReader1.Port), "RFID");
                            rfidClient1.Initialize();

                            rfidClient1.OnValueRecieved += RfidClient1_OnValueRecieved;
                            rfidClient1.OnConnected += RfidClient1_OnConnected;
                            rfidClient1.OnDisconnected += RfidClient1_OnDisconnected;
                        }
                    }
                }
                if (userHardwareProfile != null && userHardwareProfile.RFIDReader2)
                {
                    RfidIndicator2.Visibility = Visibility.Visible;
                    //var RFIDReader2 = commonFunction.GetRFIDReaderMasterById("2");
                    var RFIDReader2 = commonFunction.GetRFIDReaderMasterByIdAndHardwareProfile("2", systemConfig.HardwareProfile);
                    if (RFIDReader2 != null && (bool)RFIDReader2.IsEnable)
                    {
                        if (rfidClient2 == null)
                        {
                            rfidClient2 = new TcpClient(RFIDReader2.IP, Convert.ToInt32(RFIDReader2.Port), "RFID");
                            rfidClient2.Initialize();
                            rfidClient2.OnValueRecieved += RfidClient2_OnValueRecieved;
                            rfidClient2.OnConnected += RfidClient2_OnConnected;
                            rfidClient2.OnDisconnected += RfidClient2_OnDisconnected;
                        }
                    }
                }
                if (userHardwareProfile != null && userHardwareProfile.RFIDReader3)
                {
                    RfidIndicator3.Visibility = Visibility.Visible;
                    //var RFIDReader3 = commonFunction.GetRFIDReaderMasterById("3");
                    var RFIDReader3 = commonFunction.GetRFIDReaderMasterByIdAndHardwareProfile("3", systemConfig.HardwareProfile);
                    if (RFIDReader3 != null)
                    {
                        if (rfidClient3 == null && (bool)RFIDReader3.IsEnable)
                        {
                            rfidClient3 = new TcpClient(RFIDReader3.IP, Convert.ToInt32(RFIDReader3.Port), "RFID");
                            rfidClient3.Initialize();
                            rfidClient3.OnValueRecieved += RfidClient3_OnValueRecieved;
                            rfidClient3.OnConnected += RfidClient3_OnConnected;
                            rfidClient3.OnDisconnected += RfidClient3_OnDisconnected;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MainWindow/InitializeRfid/Exception:- " + ex.Message, ex);
            }
        }

        private void RfidClient1_OnConnected(object sender, TcpClient.TcpEventArgs e)
        {
            this.Dispatcher.Invoke(() => RfidIndicator1.Background = (Brush)_bc.ConvertFrom("#FF83F528"), DispatcherPriority.Render);
        }
        private void RfidClient1_OnDisconnected(object sender, TcpClient.TcpEventArgs e)
        {
            this.Dispatcher.Invoke(() => RfidIndicator1.Background = (Brush)_bc.ConvertFrom("#FFE1E4DF"), DispatcherPriority.Render);
        }
        private void RfidClient2_OnConnected(object sender, TcpClient.TcpEventArgs e)
        {
            this.Dispatcher.Invoke(() => RfidIndicator2.Background = (Brush)_bc.ConvertFrom("#FF83F528"), DispatcherPriority.Render);
        }
        private void RfidClient2_OnDisconnected(object sender, TcpClient.TcpEventArgs e)
        {
            this.Dispatcher.Invoke(() => RfidIndicator2.Background = (Brush)_bc.ConvertFrom("#FFE1E4DF"), DispatcherPriority.Render);
        }
        private void RfidClient3_OnConnected(object sender, TcpClient.TcpEventArgs e)
        {
            this.Dispatcher.Invoke(() => RfidIndicator3.Background = (Brush)_bc.ConvertFrom("#FF83F528"), DispatcherPriority.Render);
        }
        private void RfidClient3_OnDisconnected(object sender, TcpClient.TcpEventArgs e)
        {
            this.Dispatcher.Invoke(() => RfidIndicator3.Background = (Brush)_bc.ConvertFrom("#FFE1E4DF"), DispatcherPriority.Render);
        }

        private void RemoveRFIDReaders()
        {
            if (rfidClient1 != null)
            {
                rfidClient1.OnValueRecieved -= RfidClient1_OnValueRecieved;
                rfidClient1.OnConnected -= RfidClient1_OnConnected;
                rfidClient1.OnDisconnected -= RfidClient1_OnDisconnected;
                rfidClient1.Dispose();
            }
            if (rfidClient2 != null)
            {
                rfidClient2.OnValueRecieved -= RfidClient2_OnValueRecieved;
                rfidClient2.OnConnected -= RfidClient2_OnConnected;
                rfidClient2.OnDisconnected -= RfidClient2_OnDisconnected;
                rfidClient2.Dispose();
            }
            if (rfidClient3 != null)
            {
                rfidClient3.OnValueRecieved -= RfidClient3_OnValueRecieved;
                rfidClient3.OnConnected -= RfidClient3_OnConnected;
                rfidClient3.OnDisconnected -= RfidClient3_OnDisconnected;
                rfidClient3.Dispose();
            }
            rfidClient1 = null;
            rfidClient2 = null;
            rfidClient3 = null;
        }

        private void RfidClient1_OnValueRecieved(object sender, TcpClient.TcpValueArgs e)
        {
            onRfid1Received.Invoke(sender, new RfidEventArgs(e.value));
        }

        private void RfidClient2_OnValueRecieved(object sender, TcpClient.TcpValueArgs e)
        {
            onRfid2Received.Invoke(sender, new RfidEventArgs(e.value));
        }

        private void RfidClient3_OnValueRecieved(object sender, TcpClient.TcpValueArgs e)
        {
            onRfid3Received.Invoke(sender, new RfidEventArgs(e.value));
        }

        #endregion

        #region PLC
        public static TcpClient plcClient;
        public static event EventHandler<PlcEventArgs> onPlcReceived = delegate { };
        public void InitializePlc()
        {
            try
            {
                PLCMaster plcMaster = commonFunction.GetPLCMaster(systemConfig.HardwareProfile);
                if (plcMaster != null && plcMaster.IsEnable)
                {
                    if (plcClient == null)
                    {
                        plcClient = new TcpClient(plcMaster.IP, Convert.ToInt32(plcMaster.Port), "PLC");
                        plcClient.Initialize();
                        plcClient.OnValueRecieved += PlcClient_OnValueRecieved;
                        plcClient.OnConnected += PlcClient_OnConnected;
                        plcClient.OnDisconnected += PlcClient_OnDisconnected;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MainWindow/InitializePlc/Exception:- " + ex.Message, ex);
            }
        }

        private void PlcClient_OnConnected(object sender, TcpClient.TcpEventArgs e)
        {
            this.Dispatcher.Invoke(() => PLCIndicator.Background = (Brush)_bc.ConvertFrom("#FF83F528"), DispatcherPriority.Render);
            //WriteLog.WriteAWSLog($"PLC Connected!!!");
        }

        private void PlcClient_OnDisconnected(object sender, TcpClient.TcpEventArgs e)
        {
            this.Dispatcher.Invoke(() => PLCIndicator.Background = (Brush)_bc.ConvertFrom("#FFE1E4DF"), DispatcherPriority.Render);
            //WriteLog.WriteAWSLog($"PLC Disconnected!!!");
        }

        private void PlcClient_OnValueRecieved(object sender, TcpClient.TcpValueArgs e)
        {
            onPlcReceived.Invoke(sender, new PlcEventArgs(e.value));
            WriteLog.WriteAWSLog($"PLC value recieved :- {e.value}");
        }

        private void RemovePLC()
        {
            if (plcClient != null)
            {
                plcClient.OnValueRecieved -= PlcClient_OnValueRecieved;
                plcClient.OnConnected -= PlcClient_OnConnected;
                plcClient.OnDisconnected -= PlcClient_OnDisconnected;
                plcClient.Dispose();
            }
            plcClient = null;
        }
        #endregion

        private void StorePanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                e.Handled = true;
            }
            else
            {
                if (rolePriviliege.StoreAccess.HasValue && rolePriviliege.StoreAccess.Value)
                {
                    GoToMainWindow("Store");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "You do not have the permission for the page");
                }
            }
        }

        #region SerialCommunication
        SerialPort serialPort;
        public SerialCommunicationSetting serialCommunicationSetting = new SerialCommunicationSetting();
        DateTime lastRecievedTime;
        private void InitializeSerialCommunication()
        {
            try
            {
                serialCommunicationSetting = commonFunction.GetSerialCommunicationSetting(systemConfig.HardwareProfile);
                if (serialCommunicationSetting != null)
                {
                    if (serialPort != null)
                    {
                        serialPort.Dispose();
                        serialPort = null;
                    }
                    serialPort = new SerialPort
                    {
                        PortName = serialCommunicationSetting.Port,
                        BaudRate = serialCommunicationSetting.BaudRate,
                        Parity = (Parity)serialCommunicationSetting.Parity,
                        StopBits = (StopBits)serialCommunicationSetting.StopBit,
                        DataBits = serialCommunicationSetting.DataBits,
                        Handshake = Handshake.None,
                        Encoding = Encoding.ASCII,
                        ReadTimeout = 200,
                    };
                    serialPort.DataReceived += SerialPort_DataReceived;
                    serialPort.ErrorReceived += SerialPort_ErrorReceived;
                    serialPort.PinChanged += SerialPort_PinChanged;
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                        serialPort.Open();
                        WriteLog.WriteToFile($"MainWindow/InitializeSerialCommunication:- Serial Port {serialCommunicationSetting.Port} Connected!!");
                    }
                    else
                    {
                        serialPort.Open();
                        WriteLog.WriteToFile($"MainWindow/InitializeSerialCommunication:- Serial Port {serialCommunicationSetting.Port} Connected!!");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MainWindow/InitializeSerialCommunication/Exception:- " + ex.Message, ex);
            }
        }

        private void SerialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            onWeighmentReceived.Invoke(sender, new WeighmentEventArgs("Error"));
        }

        private void DisposeSerialCommunication()
        {
            if (serialPort != null)
            {
                serialPort.Close();
                serialPort.Dispose();
                serialPort.DataReceived -= SerialPort_DataReceived;
                serialPort.ErrorReceived -= SerialPort_ErrorReceived;
                serialPort.PinChanged -= SerialPort_PinChanged;
                serialPort = null;
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            onWeighmentReceived.Invoke(sender, new WeighmentEventArgs("Error"));
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                lastRecievedTime = DateTime.Now;
                SerialPort sp = (SerialPort)sender;
                string result = sp.ReadLine();
                result = Regex.Match(result, @"-?\d+").Value;
                this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                {
                    WeighmentLabel.Text = result;
                    WeighmentLabelUnit.Text = "kg";
                }));
                onWeighmentReceived.Invoke(sender, new WeighmentEventArgs(result));
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MainWindow/SerialPort_DataReceived/Exception:- " + ex.Message, ex);
            }
        }
        #endregion

        private void DialogHost_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
    public class TicketEventArgs : EventArgs
    {
        public string currentTicketNo { get; set; }
        public TicketEventArgs(string _currentTicketNo) : base()
        {
            this.currentTicketNo = _currentTicketNo;
        }
    }
    public class SelectTicketEventArgs : EventArgs
    {
        public string currentTransaction { get; set; }
        public SelectTicketEventArgs(string _currentTransaction) : base()
        {
            this.currentTransaction = _currentTransaction;
        }
    }

    public class MainWindowButtonEventArgs : EventArgs
    {
        public string ButtonName { get; set; }
        public MainWindowButtonEventArgs(string _btn) : base()
        {
            this.ButtonName = _btn;
        }
    }

    public class AwsCompletedEventArgs : EventArgs
    {

    }

    public class UserControlEventArgs : EventArgs
    {

    }
    public class WeighmentEventArgs : EventArgs
    {
        public string _weight { get; set; }
        public WeighmentEventArgs(string weight) : base()
        {
            this._weight = weight;
        }
    }

    public class CameraEventArgs : EventArgs
    {
        public BitmapSource bitmap { get; set; }
        public CameraEventArgs(BitmapSource image = null) : base()
        {
            this.bitmap = image;
        }
    }

    public class GeneralEventHandler : EventArgs
    {

    }

    public class RfidEventArgs : EventArgs
    {
        public string tag { get; set; }
        public RfidEventArgs(string tag) : base()
        {
            this.tag = tag;
        }
    }

    public class PlcEventArgs : EventArgs
    {
        public string value { get; set; }
        public PlcEventArgs(string value) : base()
        {
            this.value = value;
        }
    }

    public class TransLogEventArgs : EventArgs
    {
        public string log { get; set; }
        public TransLogEventArgs(string _log) : base()
        {
            this.log = _log;
        }
    }
}
