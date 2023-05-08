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

namespace IWT.Views
{
    /// <summary>
    /// Interaction logic for RFIDAllocationUserTableControl.xaml
    /// </summary>
    public partial class RFIDAllocationUserTableControl : UserControl, INotifyPropertyChanged
    {
        public List<CustomFieldBuilder> CustomFieldsBuilder = new List<CustomFieldBuilder>();
        public MasterDBCall masterDBCall = new MasterDBCall();
        public MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        List<TransactionTypeMaster> transactionTypeMasters = new List<TransactionTypeMaster>();
        public static CommonFunction commonFunction = new CommonFunction();
        WeighbridgeSettings weighbridgeSetting = new WeighbridgeSettings();
        private AdminDBCall _dbContext;
        private string res;
        public string GotoScreen;
        private RFIDAllocationWithTrans SelectedRow;
        public string TCPServerAddress = "127.0.0.1";
        public int TCPServerPort = 4002;
        public int TotalRecords = 0;
        List<int> ItemPerPagesList = new List<int> { 5, 10, 25, 50, 100, 250, 500 };
        private string roleName;
        public event PropertyChangedEventHandler PropertyChanged;
        private int _CurrentPage;
        private int _NumberOfPages;
        private int _SelectedRecord;
        private bool _IsFirstEnable;
        private bool _IsPreviousEnable;
        private bool _IsNextEnable;
        private bool _IsLastEnable;
        int RecordStartFrom = 0;

        List<RFIDAllocationWithTrans> Result = new List<RFIDAllocationWithTrans>();
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

        public RFIDAllocationUserTableControl(string _Rolename, RolePriviliege _rolePriviliege)
        {
            InitializeComponent();
            _dbContext = new AdminDBCall();
            DataContext = this;
            roleName = _Rolename;
            FromDateValue.SelectedDate = DateTime.Now.AddDays(-30);
            ToDateValue.SelectedDate = DateTime.Now;
            Loaded += RFIDAllocationUserTableControl_Loaded;
        }

        private void RFIDAllocationUserTableControl_Loaded(object sender, RoutedEventArgs e)
        {
            SelectedRecord = 10;
            NumberOfPages = 1;
            GetRFIDAllocationsUserList(FromDateValue.SelectedDate, ToDateValue.SelectedDate);
            PaginatorComboBox.ItemsSource = ItemPerPagesList;
            GetWeighbridgeSettings();
            CloseBtn.IsEnabled = false;
            InitializeTransactionTypeMaster();
            InitializeTransactionTypeComboBox();
            //TransactionMode.SelectedIndex = -1;
            SelectedRow = null;
        }

        public void InitializeTransactionTypeMaster()
        {
            transactionTypeMasters = commonFunction.GetTransactionTypeMastersForAWSTransactions();
        }
        public void InitializeTransactionTypeComboBox()
        {
            //TransactionMode.ItemsSource = transactionTypeMasters;
            //TransactionMode.SelectedValuePath = "ShortCode";
            //TransactionMode.DisplayMemberPath = "Description";
            //TransactionMode.Items.Refresh();
        }

        private void ToDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ToDateValue.SelectedDate < FromDateValue.SelectedDate)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please Select date greater than from date");
                ToDateValue.Text = "";
            }
            else if (ToDateValue.SelectedDate >= FromDateValue.SelectedDate)
            {
                return;
            }
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            GetRFIDAllocationsUserList(FromDateValue.SelectedDate, ToDateValue.SelectedDate);
        }

        public void GetRFIDAllocationsUserList(DateTime? FromDateVal, DateTime? ToDateVal)
        {
            string Query = $@"select
tr.[TicketNo],ge.[AllocationId],ge.[VehicleNumber],ge.[RFIDTag],ge.[TransMode],ge.[Status],ge.[MaterialCode],ge.[MaterialName],ge.[SupplierCode],ge.[SupplierName],ge.[ExpiryDate],ge.[TareWeight],ge.[IsLoaded],ge.[TransType],ge.[AllocationType],ge.[IsSapBased],ge.[DocNumber],ge.[GatePassNumber],ge.[TokenNumber],ge.[NoOfMaterial],ge.[CreatedOn],
tr.[EmptyWeight],tr.[LoadWeight],tr.[EmptyWeightDate],tr.[EmptyWeightTime],tr.[LoadWeightDate],tr.[LoadWeightTime],tr.[NetWeight],tr.[Closed],tr.[SystemID]
from [RFID_Allocations] ge left join [Transaction] tr on ge.AllocationId=tr.RFIDAllocation WHERE ";
            var FromDate = FromDateVal.HasValue ? FromDateVal.Value.Date.ToString("MM-dd-yyyy") : string.Empty;
            var ToDate = ToDateVal.HasValue ? ToDateVal.Value.Date.ToString("MM-dd-yyyy") : string.Empty;
            Query += $"CAST(ge.[CreatedOn] as date) BETWEEN";
            Query += $"'{FromDate}' AND '{ToDate}' order by tr.TicketNo desc";
            SqlCommand cmd = new SqlCommand(Query);
            DataTable table = masterDBCall.GetData(cmd, CommandType.Text);
            string JSONString = JsonConvert.SerializeObject(table);
            Result = JsonConvert.DeserializeObject<List<RFIDAllocationWithTrans>>(JSONString);
            Result = Result.OrderByDescending(x => x.AllocationId).ToList();
            TotalRecords = Result.Count;
            SelectedRecord = 10;
            UpdateRecordCount();
            UpdateCollection(Result.Take(SelectedRecord));
            SetDynamicTable();
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            //TransactionMode.SelectedIndex = -1;
            //FromDateValue.Text = "";
            //ToDateValue.Text = "";
            GetRFIDAllocationsUserList(FromDateValue.SelectedDate, ToDateValue.SelectedDate);
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

        private async void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var s = sender as DataGrid;
            SelectedRow = s.SelectedItem as RFIDAllocationWithTrans;
            if (roleName.ToLower() == "admin")
                CloseBtn.IsEnabled = true;
        }

        public async void ClickClose_Button(object sender, RoutedEventArgs e)
        {
            if (SelectedRow != null)
            {
                if (SelectedRow.Status.ToLower().Contains("gateexit") || SelectedRow.Status.ToLower().Contains("printed"))
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Transaction already closed!!");
                }
                else
                {
                    res = await OpenConfirmationDialog();

                    PerformGateExit();
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Transaction closed successfully!!");
                    GetRFIDAllocationsUserList(FromDateValue.SelectedDate, ToDateValue.SelectedDate);
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select record!!");
            }
        }

        public void PerformGateExit()
        {
            try
            {
                string transCloseQuery = $"update [transaction] set Closed=1 where TicketNo='{SelectedRow.TicketNo}'";
                _dbContext.ExecuteQuery(transCloseQuery);
                if (SelectedRow.IsSapBased)
                {
                    string gatePassQuery = $"UPDATE [GatePasses] SET Status='Closed-Manual' WHERE Id='{SelectedRow.GatePassId}'";
                    _dbContext.ExecuteQuery(gatePassQuery);
                }

                string vehicleQuery = $"UPDATE [Vehicle_Master] SET Status='' WHERE VehicleNumber='{SelectedRow.VehicleNumber}'";
                var res = _dbContext.ExecuteQuery(vehicleQuery);

                if (SelectedRow.AllocationType == "Temporary")
                {
                    string allocationQuery = $"UPDATE [RFID_Allocations] SET Status='Closed-Manual',Remarks='{this.res}' WHERE AllocationId='{SelectedRow.AllocationId}'";
                    _dbContext.ExecuteQuery(allocationQuery);
                    string query = $"UPDATE [RFID_Tag_Master] SET Status='Open',VehicleNo='' WHERE Tag='{SelectedRow.RFIDTag}'";
                    _dbContext.ExecuteQuery(query);
                }
                else if (SelectedRow.AllocationType == "Long-term Different Material")
                {
                    string allocationQuery = $"UPDATE [RFID_Allocations] SET Status='Closed-Manual' WHERE AllocationId='{SelectedRow.AllocationId}'";
                    _dbContext.ExecuteQuery(allocationQuery);
                }
                else
                {
                    string allocationQuery = $"UPDATE [RFID_Allocations] SET Status='LTSM' WHERE AllocationId='{SelectedRow.AllocationId}'";
                    _dbContext.ExecuteQuery(allocationQuery);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("PerformGateExit : " + ex.Message);
            }
        }

        public async Task<string> OpenConfirmationDialog()
        {
            var view = new ConfirmationDialog("Close the ticket", true);

            //    //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            return (string)result;
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
                MaterialGrid5.ItemsSource = Result1;
                //TableContainer.Content = MaterialGrid5;
                MaterialGrid5.Items.Refresh();
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
            if (Result != null)
            {
                NumberOfPages = (int)Math.Ceiling((double)Result.Count / SelectedRecord);
                NumberOfPages = NumberOfPages == 0 ? 1 : NumberOfPages;
                UpdateCollection(Result.Take(SelectedRecord));
                CurrentPage = 1;
            }
        }
        public void UpdateEnableStatus()
        {
            IsFirstEnable = CurrentPage > 1;
            IsPreviousEnable = CurrentPage > 1;
            IsNextEnable = CurrentPage < NumberOfPages;
            IsLastEnable = CurrentPage < NumberOfPages;
            //FirstPage.IsEnabled = CurrentPage > 1;
            //PreviousPage.IsEnabled = CurrentPage > 1;
            //NextPage.IsEnabled= CurrentPage < NumberOfPages;
            //LastPage.IsEnabled=CurrentPage< NumberOfPages;
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
            UpdateCollection(Result.Take(SelectedRecord));
            CurrentPage = 1;
            UpdateEnableStatus();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
            //RecordStartFrom = Result.Count - SelectedRecord * (NumberOfPages - (CurrentPage - 1));
            RecordStartFrom = SelectedRecord * (CurrentPage - 1);
            var RecordToShow = Result.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            UpdateEnableStatus();
        }
        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            RecordStartFrom = CurrentPage * SelectedRecord;
            var RecordToShow = Result.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            CurrentPage++;
            UpdateEnableStatus();

        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            var reordsToSkip = SelectedRecord * (NumberOfPages - 1);
            UpdateCollection(Result.Skip(reordsToSkip));
            CurrentPage = NumberOfPages;
            UpdateEnableStatus();
        }
    }
}
