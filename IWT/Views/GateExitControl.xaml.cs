using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using Microsoft.Reporting.WinForms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using IWT.TransactionPages;
using System.IO;
using System.Windows.Threading;
using System.Threading;

namespace IWT.Views
{
    /// <summary>
    /// Interaction logic for GateExitControl.xaml
    /// </summary>
    public partial class GateExitControl : UserControl, INotifyPropertyChanged
    {
        public List<CustomFieldBuilder> CustomFieldsBuilder = new List<CustomFieldBuilder>();
        public MasterDBCall masterDBCall = new MasterDBCall();
        public CommonFunction commonFunction = new CommonFunction();
        private readonly ToastViewModel toastViewModel;
        List<RFIDAllocationWithTrans> RFIDAllocation = new List<RFIDAllocationWithTrans>();
        private OtherSettings _otherSettings = new OtherSettings();
        private AdminDBCall _dbContext;
        private string _reportTemplate;
        private int _noOfCopies;
        private string _defaultTemplateFolder;
        private FileLocation _fileLocation = new FileLocation();
        Transaction SelectedTransaction = new Transaction();
        List<ImageSourcePath> CurrentTransactionImageSourcePath = new List<ImageSourcePath>();
        public List<Company_Details> company_Details = new List<Company_Details>();

        List<CCTVSettings> cCTVSettings = new List<CCTVSettings>();
        private RFIDAllocationWithTrans SelectedRow;
        private RFIDAllocationWithTrans tempSelectedRow;

        public int TotalRecords = 0;
        List<int> ItemPerPagesList = new List<int> { 5, 10, 25, 50, 100, 250, 500 };

        public event PropertyChangedEventHandler PropertyChanged;
        private int _CurrentPage;
        private int _NumberOfPages;
        private int _SelectedRecord;
        private bool _IsFirstEnable;
        private bool _IsPreviousEnable;
        private bool _IsNextEnable;
        private bool _IsLastEnable;
        int RecordStartFrom = 0;

        List<RFIDAllocationWithTrans> Result1 = new List<RFIDAllocationWithTrans>();

        public int CurrentPage
        {
            get { return _CurrentPage; }
            set
            {
                _CurrentPage = value;
                OnPropertyChanged();
                UpdateEnableStatus();
            }
        }

        public int NumberOfPages
        {
            get { return _NumberOfPages; }
            set
            {
                _NumberOfPages = value;
                OnPropertyChanged();
                UpdateEnableStatus();
            }
        }

        //public new GateExitControl DataContext { get; }

        public int SelectedRecord
        {
            get { return _SelectedRecord; }
            set
            {
                _SelectedRecord = value;
                OnPropertyChanged();
                UpdateRecordCount();
            }
        }


        public bool IsFirstEnable
        {
            get { return _IsFirstEnable; }
            set
            {
                _IsFirstEnable = value;
                OnPropertyChanged();
            }
        }

        public bool IsPreviousEnable
        {
            get { return _IsPreviousEnable; }
            set
            {
                _IsPreviousEnable = value;
                OnPropertyChanged();
            }
        }

        public bool IsNextEnable
        {
            get { return _IsNextEnable; }
            set
            {
                _IsNextEnable = value;
                OnPropertyChanged();
            }
        }
        public bool IsLastEnable
        {
            get { return _IsLastEnable; }
            set
            {
                _IsLastEnable = value;
                OnPropertyChanged();
            }
        }
        public GateExitControl(string _Rolename, RolePriviliege _rolePriviliege)
        {
            InitializeComponent();
            Loaded += GateExitControl_Loaded;
            Unloaded += GateExitControl_Unloaded;
            _dbContext = new AdminDBCall();
            toastViewModel = new ToastViewModel();
            DataContext = this;
        }

        private void GateExitControl_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.onRfid1Received -= MainWindow_onRfid1Received;
        }

        private void GateExitControl_Loaded(object sender, RoutedEventArgs e)
        {
            GetRFIDAllocationsUserList();
            GetCompanyDetails();
            GetOtherSettings();
            GetFileLocation();
            SelectedRecord = 10;
            NumberOfPages = 1;
            PaginatorComboBox.ItemsSource = ItemPerPagesList;
            CheckAndEnableButtons();
            MainWindow.onRfid1Received += MainWindow_onRfid1Received;
        }
        bool rfidDetect = false;
        private void MainWindow_onRfid1Received(object sender, RfidEventArgs e)
        {
            if (!rfidDetect)
            {
                rfidDetect = true;
                Task.Run(() => InvokeRfId(e));
            }
        }

        private void InvokeRfId(RfidEventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                SelectedRow = MaterialGrid5.SelectedItem as RFIDAllocationWithTrans;
            }));
            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowInformation, $"RFID detected - {e.tag}");
            Thread.Sleep(5000);
            rfidDetect = false;
        }

        private void GetCompanyDetails()
        {
            company_Details = commonFunction.GetCompanyDetails();
        }

        public void GetOtherSettings()
        {
            DataTable table = _dbContext.GetAllData($"SELECT * FROM Other_Settings where HardwareProfile='{MainWindow.systemConfig.HardwareProfile}'");
            string JSONString = JsonConvert.SerializeObject(table);
            var result = JsonConvert.DeserializeObject<List<OtherSettings>>(JSONString);
            _otherSettings = result.FirstOrDefault();
            ApplyOtherSettings();
        }

        public void GetFileLocation()
        {
            DataTable table = _dbContext.GetAllData("select * from [FileLocation_Setting]");
            string JSONString = JsonConvert.SerializeObject(table);
            var result = JsonConvert.DeserializeObject<List<FileLocation>>(JSONString);
            var flc = result.FirstOrDefault();
            _fileLocation = flc;
        }

        public void ApplyOtherSettings()
        {
            if (_otherSettings != null)
            {
                _noOfCopies = _otherSettings != null ? _otherSettings.NoOfCopies : 1;
                _reportTemplate = _fileLocation.Default_Temp;
                _defaultTemplateFolder = _fileLocation.Default_Ticket;
            }
        }

        public void GetRFIDAllocationsUserList()
        {
            DataTable table = _dbContext.GetAllData($@"select ge.[AllocationId],tr.[TicketNo],ge.[VehicleNumber],ge.[RFIDTag],ge.[TransMode],ge.[Status],ge.[MaterialCode],ge.[MaterialName],ge.[SupplierCode],ge.[SupplierName],ge.[ExpiryDate],ge.[TareWeight],ge.[IsLoaded],ge.[TransType],ge.[AllocationType],ge.[IsSapBased],ge.[DocNumber],ge.[GatePassNumber],ge.[TokenNumber],ge.[NoOfMaterial],ge.[CreatedOn],
tr.[EmptyWeight],tr.[LoadWeight],tr.[EmptyWeightDate],tr.[EmptyWeightTime],tr.[LoadWeightDate],tr.[LoadWeightTime],tr.[NetWeight],tr.[Closed],tr.[SystemID]
from [RFID_Allocations] ge inner join [Transaction] tr on ge.AllocationId=tr.RFIDAllocation WHERE ge.Status in ('In-Transit','GateExit-SAP') and tr.Closed=1 order by tr.TicketNo desc");
            var JsonString = JsonConvert.SerializeObject(table);
            RFIDAllocation = JsonConvert.DeserializeObject<List<RFIDAllocationWithTrans>>(JsonString);
            if (RFIDAllocation != null)
            {
                RFIDAllocation = RFIDAllocation.OrderByDescending(x => x.AllocationId).ToList();
                TotalRecords = RFIDAllocation.Count;
                SelectedRecord = 10;
                UpdateRecordCount();
                UpdateCollection(RFIDAllocation.Take(SelectedRecord));
                SetDynamicTable();
                CheckAndEnableButtons();
            }
            else
            {
                RFIDAllocation = new List<RFIDAllocationWithTrans>();
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var s = sender as DataGrid;
            SelectedRow = s.SelectedItem as RFIDAllocationWithTrans;
            CheckAndEnableButtons();
        }


        private void ColumnCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckAndEnableButtons();
        }

        private async void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRow != null)
            {
                tempSelectedRow = SelectedRow;
                var isPreview = await OpenPreviewDialog();
                GetData((int)SelectedRow.TicketNo);
                if (isPreview)
                    popup.IsOpen = true;
                else
                {
                    ReportViewerDemo1.LocalReport.PrintToPrinter(_noOfCopies);
                    PerformGateExit("GateExit-Printed");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Gate exit performed successfully");
                }
                GetRFIDAllocationsUserList();
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a transaction to print");
            }
        }

        private void GetData(int ticketNo)
        {
            try
            {
                AdminDBCall db = new AdminDBCall();
                DataTable dt = db.GetAllData("select * from [Transaction] where TicketNo = " + ticketNo);
                DataTable dt1 = new DataTable();
                if (tempSelectedRow.TransMode == "FMT")
                {
                    dt1 = db.GetAllData("select * from [Transaction_Details] where TicketNo = " + ticketNo);
                }
                GetCapturedImages();

                if (CurrentTransactionImageSourcePath.Count > 0)
                {
                    CurrentTransactionImageSourcePath[0].WaterMarkImagePath = null;
                }
                else
                {
                    ImageSourcePath imageSourcePath = new ImageSourcePath();
                    imageSourcePath.WaterMarkImagePath = null;
                    CurrentTransactionImageSourcePath.Add(imageSourcePath);
                }

                ReportDataSource rds = new ReportDataSource("DataSet1", dt);
                ReportDataSource rds1 = new ReportDataSource("DataSet2", company_Details);
                ReportDataSource rds3 = new ReportDataSource("DataSet3", CurrentTransactionImageSourcePath);

                ReportViewerDemo1.LocalReport.DataSources.Clear();
                ReportViewerDemo1.LocalReport.DataSources.Add(rds);
                ReportViewerDemo1.LocalReport.DataSources.Add(rds1);
                ReportViewerDemo1.LocalReport.DataSources.Add(rds3);
                if (tempSelectedRow.TransMode == "FMT")
                {
                    ReportDataSource rds10 = new ReportDataSource("DataSet10", dt1);
                    ReportViewerDemo1.LocalReport.DataSources.Add(rds10);
                    ReportViewerDemo1.LocalReport.ReportPath = System.IO.Path.Combine(_fileLocation.Default_Ticket, "MultiTransaction.rdl");
                }
                else
                {
                    ReportViewerDemo1.LocalReport.ReportPath = System.IO.Path.Combine(_fileLocation.Default_Ticket, "Transaction.rdl");
                }
                ReportViewerDemo1.ShowExportButton = false;
                ReportViewerDemo1.ShowFindControls = false;
                ReportViewerDemo1.ShowStopButton = false;
                ReportViewerDemo1.RefreshReport();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GateExit/GetData/Exception :- " + ex.Message);
            }
        }

        private void GetCapturedImages()
        {
            try
            {
                CurrentTransactionImageSourcePath = new List<ImageSourcePath>();
                ImageSourcePath imageSourcePath = new ImageSourcePath();
                var i = 1;
                foreach (var ccTV in cCTVSettings)
                {
                    var folder = ccTV.LogFolder;
                    FileInfo fi;
                    List<FileInfo> fis;
                    DirectoryInfo di = new DirectoryInfo(ccTV.LogFolder);

                    fis = di.GetFiles("*.jpeg").Where(file => file.Name.Contains($"{SelectedTransaction.TicketNo}_") &&
                    file.Name.Contains($"cam{ccTV.RecordID}_")).OrderByDescending(p => p.CreationTime).Take(2).ToList();
                    int j = 1;
                    foreach (var f in fis)
                    {
                        fi = f;
                        if (fi != null)
                        {
                            var bytes = File.ReadAllBytes(fi.FullName);
                            if (i == 1)
                            {
                                if (j == 1)
                                {
                                    imageSourcePath.Image1Path = bytes;
                                }
                                if (j == 2)
                                {
                                    imageSourcePath.Image4Path = imageSourcePath.Image1Path;
                                    imageSourcePath.Image1Path = bytes;
                                }
                            }
                            if (i == 2)
                            {
                                if (j == 1)
                                {
                                    imageSourcePath.Image2Path = bytes;
                                }
                                if (j == 2)
                                {
                                    imageSourcePath.Image5Path = imageSourcePath.Image2Path;
                                    imageSourcePath.Image2Path = bytes;
                                }
                            }
                            if (i == 3)
                            {
                                if (j == 1)
                                {
                                    imageSourcePath.Image3Path = bytes;
                                }
                                if (j == 2)
                                {
                                    imageSourcePath.Image6Path = imageSourcePath.Image3Path;
                                    imageSourcePath.Image3Path = bytes;
                                }
                            }
                        }
                        j++;
                    }

                    i++;
                }

                CurrentTransactionImageSourcePath.Add(imageSourcePath);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GateExit/GetCapturedImages/Exception :- " + ex.Message);
            }
        }

        public async Task OpenCopiesDialog()
        {
            var view = new NoOfCopiesDialog(_otherSettings != null ? _otherSettings.NoOfCopies : 1);
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            if (result != null)
            {
                _noOfCopies = int.Parse(result as string);
            }
        }
        public async Task<bool> OpenTemplateDialog()
        {
            var view = new TemplateSelectionDialog(_defaultTemplateFolder);
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            if (result != null)
            {
                _reportTemplate = ((FileInfo)result).FullName;
                return true;
            }
            return false;
        }

        private void CheckAndEnableButtons()
        {
            Dispatcher.Invoke(() =>
            {
                if (SelectedRow != null)
                {
                    CloseButton.IsEnabled = true;
                    PrintButton.IsEnabled = true;
                }
                else
                {
                    CloseButton.IsEnabled = false;
                    PrintButton.IsEnabled = false;
                }
            });
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            ReportViewerDemo1.LocalReport.PrintToPrinter(1);
            PerformGateExit("GateExit-Printed");
            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Gate exit performed successfully");
            GetRFIDAllocationsUserList();
        }

        private async void SMSBtn_Click(object sender, RoutedEventArgs e)
        {
            var message = commonFunction.BuildSMS(SelectedTransaction);
            await commonFunction.CheckAndSendSMS(message);
        }

        private async void EmailBtn_Click(object sender, RoutedEventArgs e)
        {
            var message = commonFunction.BuildEmail(SelectedTransaction);

            Warning[] warnings;
            string[] streamids;
            string mimeType;
            string encoding;
            string filenameExtension;


            byte[] bytes = ReportViewerDemo1.LocalReport.Render(
                "PDF", null, out mimeType, out encoding, out filenameExtension,
                out streamids, out warnings);

            //using (FileStream fs = new FileStream("output.pdf", FileMode.Create))
            //{
            //    fs.Write(bytes, 0, bytes.Length);
            //}

            string fileName = $"report{DateTime.Now.ToString("ddMMyyyyHHmmss")}.pdf";
            //string fileName = $"report{DateTime.Now.ToString("ddMMyyyyHHmmss")}.pdf";

            await commonFunction.CheckAndSendEmail(SelectedTransaction, message, bytes, fileName);
        }

        public async void ClickClose_Button(object sender, RoutedEventArgs e)
        {
            if (SelectedRow != null)
            {
                tempSelectedRow = SelectedRow;
                var res = await OpenConfirmationDialog();
                if (res)
                {
                    PerformGateExit("GateExit");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Gate exit happened successfully!!");
                    GetRFIDAllocationsUserList();
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select record!!");
            }
        }

        public void PerformGateExit(string status)
        {
            try
            {
                if (tempSelectedRow.IsSapBased)
                {
                    string gatePassQuery = $"UPDATE [GatePasses] SET Status='{status}' WHERE Id='{tempSelectedRow.GatePassId}'";
                    _dbContext.ExecuteQuery(gatePassQuery);
                }
                
                string vehicleQuery = $"UPDATE [Vehicle_Master] SET Status='' WHERE VehicleNumber='{tempSelectedRow.VehicleNumber}'";
                var res = _dbContext.ExecuteQuery(vehicleQuery);
                if (tempSelectedRow.AllocationType == "Temporary")
                {
                    string allocationQuery = $"UPDATE [RFID_Allocations] SET Status='{status}' WHERE AllocationId='{tempSelectedRow.AllocationId}'";
                    _dbContext.ExecuteQuery(allocationQuery);
                    string rfidquery = $"UPDATE [RFID_Tag_Master] SET Status='Open',VehicleNo='' WHERE Tag='{tempSelectedRow.RFIDTag}'";
                    _dbContext.ExecuteQuery(rfidquery);
                }
                else if(tempSelectedRow.AllocationType == "Long-term Different Material")
                {
                    string allocationQuery = $"UPDATE [RFID_Allocations] SET Status='{status}' WHERE AllocationId='{tempSelectedRow.AllocationId}'";
                    _dbContext.ExecuteQuery(allocationQuery);
                }
                else
                {
                    string allocationQuery = $"UPDATE [RFID_Allocations] SET Status='LTSM' WHERE AllocationId='{tempSelectedRow.AllocationId}'";
                    _dbContext.ExecuteQuery(allocationQuery);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("PerformGateExit : " + ex.Message);
            }
        }

        public async Task<bool> OpenConfirmationDialog()
        {
            var view = new ConfirmationDialog("do gate exit");

            //    //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            return (bool)result;
        }

        public async Task<bool> OpenPreviewDialog()
        {
            var view = new ConfirmationDialog("", false, "Do you want to preview?");

            //    //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            return (bool)result;
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {

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


        public void SetDynamicTable()
        {
            try
            {
                //MaterialGrid5.ItemsSource = Result1;
                ////TableContainer.Content = MaterialGrid5;
                //MaterialGrid5.Items.Refresh();

                Dispatcher.Invoke(() =>
                {
                    MaterialGrid5.ItemsSource = Result1;
                    MaterialGrid5.Items.Refresh();
                });
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetDynamicTable:" + ex.Message);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void UpdateCollection(IEnumerable<RFIDAllocationWithTrans> enumerables)
        {
            Result1.Clear();
            foreach (var enumera in enumerables)
            {
                Result1.Add(enumera);
            }
            SetDynamicTable();
        }

        public void UpdateRecordCount()
        {
            NumberOfPages = (int)Math.Ceiling((double)RFIDAllocation.Count / SelectedRecord);
            NumberOfPages = NumberOfPages == 0 ? 1 : NumberOfPages;
            UpdateCollection(RFIDAllocation.Take(SelectedRecord));
            CurrentPage = 1;
        }
        public void UpdateEnableStatus()
        {
            IsFirstEnable = CurrentPage > 1;
            IsPreviousEnable = CurrentPage > 1;
            IsNextEnable = CurrentPage < NumberOfPages;
            IsLastEnable = CurrentPage < NumberOfPages;
        }

        private void PaginatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(PaginatorComboBox.SelectedValue?.ToString()))
            {
                SelectedRecord = Convert.ToInt32(PaginatorComboBox.SelectedValue.ToString());
            }
            UpdateRecordCount();
        }

        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            UpdateCollection(RFIDAllocation.Take(SelectedRecord));
            CurrentPage = 1;
            UpdateEnableStatus();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
            //RecordStartFrom = Result.Count - SelectedRecord * (NumberOfPages - (CurrentPage - 1));
            RecordStartFrom = SelectedRecord * (CurrentPage - 1);
            var RecordToShow = RFIDAllocation.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            UpdateEnableStatus();
        }
        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            RecordStartFrom = CurrentPage * SelectedRecord;
            var RecordToShow = RFIDAllocation.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            CurrentPage++;
            UpdateEnableStatus();

        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            var reordsToSkip = SelectedRecord * (NumberOfPages - 1);
            UpdateCollection(RFIDAllocation.Skip(reordsToSkip));
            CurrentPage = NumberOfPages;
            UpdateEnableStatus();
        }

        private void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {
            GetRFIDAllocationsUserList();
        }
    }
}
