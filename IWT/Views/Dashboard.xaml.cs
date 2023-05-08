using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.TransactionPages;
using IWT.ViewModel;
using Newtonsoft.Json;
using SuperSimpleTcp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace IWT.Views
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        public static CommonFunction commonFunction = new CommonFunction();
        public static MasterDBCall masterDBCall = new MasterDBCall();
        private Button currentTab = new Button();
        private Button previousTab = new Button();
        //public static event EventHandler<WeighmentEventArgs> onWeighmentReceived = delegate { };
        public string TCPServerAddress = "127.0.0.1";
        public int TCPServerPort = 4002;
        public SimpleTcpClient TCP_Client;
        AuthStatus authResult;
        RolePriviliege rolePriviliege;
        UserHardwareProfile userHardwareProfile;
        string weghsoftApp = ConfigurationManager.AppSettings["WeighmentApplicationPath"].ToString();
        Setting_DBCall db = new Setting_DBCall();
        WeighbridgeSettings weighbridgeSetting = new WeighbridgeSettings();
        SoftwareSettingConfig softwareSettingConfig = new SoftwareSettingConfig();
        AWSConfiguration awsConfiguration = new AWSConfiguration();
        public Dashboard(AuthStatus _authResult, RolePriviliege _rolePriviliege, UserHardwareProfile _userHardwareProfile, WeighbridgeSettings _weighbridgeSetting)
        {
            InitializeComponent();
            authResult = _authResult;
            this.rolePriviliege = _rolePriviliege;
            this.userHardwareProfile = _userHardwareProfile;
            weighbridgeSetting = _weighbridgeSetting;
            Loaded += Dashboard_Loaded;
            Unloaded += Dashboard_Unloaded;
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
        public void GetSoftwareConfig()
        {
            try
            {
                DataTable dt1 = db.GetSoftwareData("SELECT * FROM Software_SettingConfig");
                var JSONString = JsonConvert.SerializeObject(dt1);
                var res = JsonConvert.DeserializeObject<List<SoftwareSettingConfig>>(JSONString);
                if (res != null)
                    softwareSettingConfig = res.Count > 0 ? res[0] : null;

            }
            catch (Exception ex)
            {

            }
        }
        private void Dashboard_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.onWeighmentReceived += Single_onWeighmentReceived;
            MainWindow.onRfid2Received += MainWindow_onRfid2Received;
            MainWindow.onRfid3Received += MainWindow_onRfid3Received;
            SingleTransaction.awsOperationCompleted += AwsOperationCompleted;
            FirstTransaction.awsOperationCompleted += AwsOperationCompleted;
            SecondTransaction.awsOperationCompleted += AwsOperationCompleted;
            FirstMulti.awsOperationCompleted+=AwsOperationCompleted;
            SecondMulti.awsOperationCompleted += AwsOperationCompleted;
            SingleTransaction.createTransLog += createTransLog;
            FirstTransaction.createTransLog += createTransLog;
            SecondTransaction.createTransLog += createTransLog;
            FirstMulti.createTransLog += createTransLog;
            SecondMulti.createTransLog += createTransLog;
            awsConfiguration = commonFunction.GetAWSConfiguration(MainWindow.systemConfig.HardwareProfile);
            GetSoftwareConfig();
            if (softwareSettingConfig != null)
            {
                ShowTransactionByConfig();
            }
            else
            {
                Main.Content = new SingleTransaction(authResult, rolePriviliege, userHardwareProfile);
                var bc = new BrushConverter();
                Tab_Single.Background = (Brush)bc.ConvertFrom("#FFAA33");
                Tab_Single.Foreground = new SolidColorBrush(Colors.White);
                currentTab = Tab_Single;
                (currentTab.Template.FindName("border_color", currentTab) as Border).BorderThickness = new Thickness(2);
                ChangeZIndex();
            }
        }

        private void createTransLog(object sender, TransLogEventArgs e)
        {
            CreateLog(e.log);
        }

        private void AwsOperationCompleted(object sender, AwsCompletedEventArgs e)
        {
            IsAwsStarted = false;
        }

        private void Dashboard_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.onWeighmentReceived -= Single_onWeighmentReceived;
            MainWindow.onRfid2Received -= MainWindow_onRfid2Received;
            MainWindow.onRfid3Received -= MainWindow_onRfid3Received;
            SingleTransaction.awsOperationCompleted -= AwsOperationCompleted;
            FirstTransaction.awsOperationCompleted -= AwsOperationCompleted;
            SecondTransaction.awsOperationCompleted -= AwsOperationCompleted;
            FirstMulti.awsOperationCompleted -= AwsOperationCompleted;
            SecondMulti.awsOperationCompleted -= AwsOperationCompleted;
            SingleTransaction.createTransLog -= createTransLog;
            FirstTransaction.createTransLog -= createTransLog;
            SecondTransaction.createTransLog -= createTransLog;
            FirstMulti.createTransLog -= createTransLog;
            SecondMulti.createTransLog -= createTransLog;
            ClearTabSelection();
        }

        public void ShowTransactionByConfig()
        {
            try
            {

                Tab_Single.Visibility = softwareSettingConfig.Single_Transaction ? Visibility.Visible : Visibility.Collapsed;
                Tab_First.Visibility = softwareSettingConfig.First_Transaction ? Visibility.Visible : Visibility.Collapsed;
                Tab_Second.Visibility = softwareSettingConfig.Second_Transaction ? Visibility.Visible : Visibility.Collapsed;
                Tab_FirstMulti.Visibility = softwareSettingConfig.First_MultiTransaction ? Visibility.Visible : Visibility.Collapsed;
                Tab_SecondMulti.Visibility = softwareSettingConfig.Second_MultiTransaction ? Visibility.Visible : Visibility.Collapsed;
                if (softwareSettingConfig.Single_Transaction)
                {
                    Main.Content = new SingleTransaction(authResult, rolePriviliege, userHardwareProfile);
                    var bc = new BrushConverter();
                    Tab_Single.Background = (Brush)bc.ConvertFrom("#FFAA33");
                    Tab_Single.Foreground = new SolidColorBrush(Colors.White);
                    currentTab = Tab_Single;
                    if ((currentTab.Template?.FindName("border_color", currentTab) as Border) != null)
                    {
                        (currentTab.Template?.FindName("border_color", currentTab) as Border).BorderThickness = new Thickness(2);
                    }

                }
                else if (softwareSettingConfig.First_Transaction)
                {
                    Main.Content = new FirstTransaction(authResult, rolePriviliege, userHardwareProfile);
                    var bc = new BrushConverter();
                    Tab_First.Background = (Brush)bc.ConvertFrom("#FFAA33");
                    Tab_First.Foreground = new SolidColorBrush(Colors.White);
                    currentTab = Tab_First;
                    if ((currentTab.Template?.FindName("border_color", currentTab) as Border) != null)
                    {
                        (currentTab.Template?.FindName("border_color", currentTab) as Border).BorderThickness = new Thickness(2);
                    }
                }
                else if (softwareSettingConfig.Second_Transaction)
                {
                    Main.Content = new SecondTransaction(authResult, rolePriviliege, userHardwareProfile);
                    var bc = new BrushConverter();
                    Tab_Second.Background = (Brush)bc.ConvertFrom("#FFAA33");
                    Tab_Second.Foreground = new SolidColorBrush(Colors.White);
                    currentTab = Tab_Second;
                    if ((currentTab.Template?.FindName("border_color", currentTab) as Border) != null)
                    {
                        (currentTab.Template?.FindName("border_color", currentTab) as Border).BorderThickness = new Thickness(2);
                    }
                }
                else if (softwareSettingConfig.First_MultiTransaction)
                {
                    Main.Content = new FirstMulti(authResult, rolePriviliege, userHardwareProfile);
                    var bc = new BrushConverter();
                    Tab_FirstMulti.Background = (Brush)bc.ConvertFrom("#FFAA33");
                    Tab_FirstMulti.Foreground = new SolidColorBrush(Colors.White);
                    currentTab = Tab_FirstMulti;
                    if ((currentTab.Template?.FindName("border_color", currentTab) as Border) != null)
                    {
                        (currentTab.Template?.FindName("border_color", currentTab) as Border).BorderThickness = new Thickness(2);
                    }
                }
                else if (softwareSettingConfig.Second_MultiTransaction)
                {
                    Main.Content = new SecondMulti(authResult, rolePriviliege, userHardwareProfile);
                    var bc = new BrushConverter();
                    Tab_SecondMulti.Background = (Brush)bc.ConvertFrom("#FFAA33");
                    Tab_SecondMulti.Foreground = new SolidColorBrush(Colors.White);
                    currentTab = Tab_SecondMulti;
                    if ((currentTab.Template?.FindName("border_color", currentTab) as Border) != null)
                    {
                        (currentTab.Template?.FindName("border_color", currentTab) as Border).BorderThickness = new Thickness(2);
                    }
                }
                ChangeZIndex();
            }
            catch (Exception ex)
            {

            }
        }
        private void Single_onWeighmentReceived(object sender, WeighmentEventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                var we = e._weight as string;
                WeighmentLabel.Text = we;
                WeighmentLabelUnit.Text = "kg";
            }));
        }
        private void Tab_Click(object sender, RoutedEventArgs e)
        {
            SwitchTab(sender as Button);
        }

        private void SwitchTab(Button sender, AWSTransaction transaction = null)
        {
            var bc = new BrushConverter();
            currentTab = sender;
            ClearTabSelection();
            previousTab = currentTab;
            currentTab.Background = (Brush)bc.ConvertFrom("#FFAA33");
            currentTab.Foreground = new SolidColorBrush(Colors.White);
            (currentTab.Template.FindName("border_color", currentTab) as Border).BorderThickness = new Thickness(2);
            ChangeZIndex(transaction);
        }

        public void ChangeZIndex(AWSTransaction transaction = null)
        {
            Panel.SetZIndex(currentTab, 6);
            if (currentTab.Uid == "1")
            {
                SetZIndex(new List<int> { 5, 4, 3, 2, 1 });
                try
                {
                    Main.Content = new SingleTransaction(authResult, rolePriviliege, userHardwareProfile, transaction);
                }
                catch (Exception ex)
                {
                    WriteLog.WriteToFile("Dashboard/Tab_Click:SingleTransaction :- " + ex.Message);
                }
            }
            else if (currentTab.Uid == "2")
            {
                SetZIndex(new List<int> { 4, 5, 3, 2, 1 });
                Main.Content = new FirstTransaction(authResult, rolePriviliege, userHardwareProfile, transaction);
            }
            else if (currentTab.Uid == "3")
            {
                SetZIndex(new List<int> { 3, 4, 5, 2, 1 });
                Main.Content = new SecondTransaction(authResult, rolePriviliege, userHardwareProfile, transaction);
            }
            else if (currentTab.Uid == "4")
            {
                SetZIndex(new List<int> { 2, 3, 4, 5, 1 });
                Main.Content = new FirstMulti(authResult, rolePriviliege, userHardwareProfile, transaction);
            }
            else if (currentTab.Uid == "5")
            {
                SetZIndex(new List<int> { 1, 2, 3, 4, 5 });
                Main.Content = new SecondMulti(authResult, rolePriviliege, userHardwareProfile, transaction);
            }
        }

        private void ClearTabSelection()
        {
            try
            {
                var bc = new BrushConverter();

                Tab_Single.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Tab_Single.Foreground = new SolidColorBrush(Colors.Black);
                if ((Tab_Single.Template?.FindName("border_color", Tab_Single) as Border) != null)
                {
                    (Tab_Single.Template?.FindName("border_color", Tab_Single) as Border).BorderThickness = new Thickness(0);
                }

                Tab_First.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Tab_First.Foreground = new SolidColorBrush(Colors.Black);
                if ((Tab_First.Template?.FindName("border_color", Tab_First) as Border) != null)
                {
                    (Tab_First.Template?.FindName("border_color", Tab_First) as Border).BorderThickness = new Thickness(0);
                }

                Tab_Second.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Tab_Second.Foreground = new SolidColorBrush(Colors.Black);
                if ((Tab_Second.Template?.FindName("border_color", Tab_Second) as Border) != null)
                {
                    (Tab_Second.Template?.FindName("border_color", Tab_Second) as Border).BorderThickness = new Thickness(0);
                }

                Tab_FirstMulti.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Tab_FirstMulti.Foreground = new SolidColorBrush(Colors.Black);
                if ((Tab_FirstMulti.Template?.FindName("border_color", Tab_FirstMulti) as Border) != null)
                {
                    (Tab_FirstMulti.Template?.FindName("border_color", Tab_FirstMulti) as Border).BorderThickness = new Thickness(0);
                }

                Tab_SecondMulti.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Tab_SecondMulti.Foreground = new SolidColorBrush(Colors.Black);
                if ((Tab_SecondMulti.Template?.FindName("border_color", Tab_SecondMulti) as Border) != null)
                {
                    (Tab_SecondMulti.Template?.FindName("border_color", Tab_SecondMulti) as Border).BorderThickness = new Thickness(0);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void SetZIndex(List<int> arr)
        {
            Panel.SetZIndex(Tab_Single, arr[0]);
            Panel.SetZIndex(Tab_First, arr[1]);
            Panel.SetZIndex(Tab_Second, arr[2]);
            Panel.SetZIndex(Tab_FirstMulti, arr[3]);
            Panel.SetZIndex(Tab_SecondMulti, arr[4]);
        }

        #region AWS
        public bool IsAwsStarted = false;

        private void MainWindow_onRfid2Received(object sender, RfidEventArgs e)
        {
            if (awsConfiguration!=null && !IsAwsStarted)
            {
                Task.Run(() => StartAwsSequence(e.tag, true));
            }
        }
        private void MainWindow_onRfid3Received(object sender, RfidEventArgs e)
        {
            if (awsConfiguration != null && !IsAwsStarted)
            {
                Task.Run(() => StartAwsSequence(e.tag));
            }
        }
        public void StartAwsSequence(string rfid, bool isSecondReader = false)
        {
            try
            {
                CreateLog($"RFID-{rfid} detected!! Initiating AWS process...");
                IsAwsStarted = true;
                this.Dispatcher.Invoke(() => Panel.SetZIndex(DashboardContainer, 10));
                if (!MainWindow.plcClient.Client.IsConnected)
                    throw new Exception("PLC not connected");
                else
                {
                    MainWindow.plcClient.Client.Send("88");
                }
                string Query = $@"SELECT * FROM [RFID_Allocations] WHERE RFIDTag='{rfid}' and Status in ('In-Transit','LTSM')";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var Result = JsonConvert.DeserializeObject<List<RFIDAllocation>>(JSONString);
                var Allocation = Result.FirstOrDefault();
                if (Allocation != null)
                {
                    var currentTransaction = new Transaction
                    {
                        VehicleNo = Allocation.VehicleNumber,
                        MaterialCode = Allocation.MaterialCode,
                        MaterialName = Allocation.MaterialName,
                        SupplierCode = Allocation.SupplierCode,
                        SupplierName = Allocation.SupplierName,
                        LoadStatus = (bool)Allocation.IsLoaded ? "Loaded" : "Empty",
                        RFIDAllocation = Allocation.AllocationId,
                        DocNumber = Allocation.DocNumber,
                        TokenNumber = Allocation.TokenNumber,
                        GatePassNumber = Allocation.GatePassNumber,
                        TransType = Allocation.TransType,
                        IsSapBased = Allocation.IsSapBased,
                        NoOfMaterial = (int)Allocation.NoOfMaterial,
                        EmptyWeight = Allocation.TareWeight,
                        TicketNo = (int)MainWindow.CurrentTicketNumber+1
                    };
                    string transQuery = $@"SELECT * FROM [Transaction] WHERE VehicleNo='{Allocation.VehicleNumber}' and RFIDAllocation='{Allocation.AllocationId}'";
                    SqlCommand cmd1 = new SqlCommand(transQuery);
                    DataTable dt = masterDBCall.GetData(cmd1, CommandType.Text);
                    string JSONString1 = JsonConvert.SerializeObject(dt);
                    var Result1 = JsonConvert.DeserializeObject<List<Transaction>>(JSONString1);
                    var closedTransaction = Result1.OrderByDescending(t => t.TicketNo).FirstOrDefault(t => t.Closed);
                    var pendingTransaction = Result1.OrderByDescending(t => t.TicketNo).FirstOrDefault(t => t.Pending);
                    if ((Allocation.AllocationType == "Temporary" && closedTransaction != null) || (Allocation.AllocationType == "Long-term Different Material" && closedTransaction != null))
                        throw new Exception("Transaction closed !!! Please do gate entry...");

                    if (Allocation.ExpiryDate < DateTime.Now  && Allocation.AllocationType!= "Long-term Same Material")
                        throw new Exception("Allocation expired !!! Please do gate entry...");

                    this.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                    {
                        if (Allocation.TransMode == "SGT")
                        {
                            SwitchTab(Tab_Single, new AWSTransaction { TransactionData = currentTransaction, AllocationData = Allocation, IsSecondReader = isSecondReader });
                        }
                        else if (Allocation.TransMode == "FT")
                        {
                            if (pendingTransaction != null)
                            {
                                SwitchTab(Tab_Second, new AWSTransaction { TransactionData = pendingTransaction, AllocationData = Allocation, IsSecondReader = isSecondReader });
                            }
                            else
                            {
                                SwitchTab(Tab_First, new AWSTransaction { TransactionData = currentTransaction, AllocationData = Allocation, IsSecondReader = isSecondReader });
                            }
                        }
                        else if (Allocation.TransMode == "FMT")
                        {
                            if (pendingTransaction != null)
                            {
                                SwitchTab(Tab_SecondMulti, new AWSTransaction { TransactionData = pendingTransaction, AllocationData = Allocation, IsSecondReader = isSecondReader });
                            }
                            else
                            {
                                SwitchTab(Tab_FirstMulti, new AWSTransaction { TransactionData = currentTransaction, AllocationData = Allocation, IsSecondReader = isSecondReader });
                            }
                        }
                    }));
                    while (IsAwsStarted) { }
                    this.Dispatcher.Invoke(() => Panel.SetZIndex(DashboardContainer, 0));
                }
                else
                {
                    throw new Exception("Transaction with RFID not found!!");
                }
            }
            catch (Exception ex)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, ex.Message);
                WriteLog.WriteToFile("StartAwsSequence/Exception:- ", ex);
                CreateLog($"Exception:-{ex.Message}");
                //commonFunction.SendCommandToPLC("55");
                Thread.Sleep(5000);
                IsAwsStarted = false;
                this.Dispatcher.Invoke(() => Panel.SetZIndex(DashboardContainer, 0));
                return;
            }
        }
        #endregion
        #region Logging
        private int logThresholdCount = 1000;        //Backup log lines
        private int logCount = 0;
        public void CreateLog(string message)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (logCount <= logThresholdCount)
                {
                    var log = new TextBlock { Text = $"{DateTime.Now}:- {message}" };
                    LogTerminal.Children.Add(log);
                }
                else
                {
                    LogTerminal.Children.RemoveAt(0);
                    var log = new TextBlock { Text = $"{DateTime.Now}:- {message}" };
                    LogTerminal.Children.Add(log);
                }
                logCount++;
                LogContainer.ScrollToBottom();
            }), DispatcherPriority.Render);
        }
        #endregion
    }
}
