using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using AWS.Communication.Models;
using AWS.Communication;
using System.Threading.Tasks;

namespace IWT.Admin_Pages
{
    /// <summary>
    /// Interaction logic for SAPSync.xaml
    /// </summary>
    public partial class SAPSync : Page, INotifyPropertyChanged
    {
        public MasterDBCall masterDBCall = new MasterDBCall();
        SAPInterfaceCall interfaceCalls = new SAPInterfaceCall();
        public CommonFunction commonFunction = new CommonFunction();
        private readonly ToastViewModel toastViewModel;
        List<SAPDataBackUp> SAPDataBackUp = new List<SAPDataBackUp>();
        List<SAPDataBackUp> selectedRFIDAllocations;
        List<GatePasses> selectedGatePasses;
        private OtherSettings _otherSettings = new OtherSettings();
        private AdminDBCall _dbContext;
        private SAPDataBackUp SelectedRow;
        private GatePasses SelectedRowGatePass;
        RolePriviliege rolePriviliege;
        string selectedTab="1";

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

        //List<RFIDAllocation> Result = new List<RFIDAllocation>();
        List<SAPDataBackUp> Result1 = new List<SAPDataBackUp>();
        List<GatePasses> Result2 = new List<GatePasses>();
        List<GatePasses> GatePasses = new List<GatePasses>();

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

        public SAPSync()
        {
            InitializeComponent();
            Loaded += SAPDataBackUps;
            FromDateValue.SelectedDate = DateTime.Now.AddDays(-30);
            ToDateValue.SelectedDate = DateTime.Now;
            gpFromDate.SelectedDate = DateTime.Now.AddDays(-30);
            gpToDate.SelectedDate = DateTime.Now;
            _dbContext = new AdminDBCall();
            toastViewModel = new ToastViewModel();            
            DataContext = this;
        }       

        private void SAPDataBackUps(object sender, RoutedEventArgs e)
        {
            _SelectedRecord = 10;
            NumberOfPages = 1;
            PaginatorComboBox.SelectedIndex = ItemPerPagesList.IndexOf(_SelectedRecord);
            gpPaginatorComboBox.SelectedIndex = ItemPerPagesList.IndexOf(_SelectedRecord);
            GetSAPDataBackUpsList();
            //UpdateRecordCount();
            PaginatorComboBox.ItemsSource = ItemPerPagesList;
            gpPaginatorComboBox.ItemsSource = ItemPerPagesList;
            CheckAndEnableButton();
        }

        public void GetSAPDataBackUpsList()
        {
            DataTable table = _dbContext.GetAllData($"SELECT * FROM SAP_Data_BackUp WHERE CAST(Date as DATE) BETWEEN '{FromDateValue.SelectedDate.Value.Date.ToString("yyyy-MM-dd HH:mm:ss")}' AND '{ToDateValue.SelectedDate.Value.Date.ToString("yyyy-MM-dd HH:mm:ss")}'");

            var JsonString = JsonConvert.SerializeObject(table);
            SAPDataBackUp = JsonConvert.DeserializeObject<List<SAPDataBackUp>>(JsonString);
            if (SAPDataBackUp != null)
            {
                SAPDataBackUp = SAPDataBackUp.OrderByDescending(x => x.Id).ToList();
                TotalRecords = SAPDataBackUp.Count;
                //_SelectedRecord = 10;
                //UpdateRecordCount();
                UpdateCollection(SAPDataBackUp.Take(_SelectedRecord));
                SetDynamicTable();
                CheckAndEnableButton();
            }
            else
            {
                List<SAPDataBackUp> SAPDataBackUp = new List<SAPDataBackUp>();
            }
        }

        public void GetGatePassData()
        {
            DataTable table = _dbContext.GetAllData($@"select gp.Id,gp.TokenNumber,gp.VehicleNumber,gp.InOut,gp.OutwardType,gp.Plant,
(select cast(item.ItemNumber as nvarchar(50))+',' from GatePassItems item where item.GatePassId=gp.Id and item.ItemType='Material' FOR XML PATH('')) as MaterialNumber,
gp.GatePassNumber,gp.PoNumber,
(select cast(item.ItemNumber as nvarchar(50))+',' from GatePassItems item where item.GatePassId=gp.Id and item.ItemType='PO' FOR XML PATH('')) as PoItemNumber,
gp.SoNumber,
(select cast(item.ItemNumber as nvarchar(50))+',' from GatePassItems item where item.GatePassId=gp.Id and item.ItemType='SO' FOR XML PATH('')) as SoItemNumber,gp.InwardType,gp.Status,gp.CreatedOn from GatePasses gp where cast(gp.CreatedOn as date) between '{gpFromDate.SelectedDate.Value.Date.ToString("yyyy-MM-dd HH:mm:ss")}' and '{gpToDate.SelectedDate.Value.Date.ToString("yyyy-MM-dd HH:mm:ss")}'");
            var JsonString = JsonConvert.SerializeObject(table);
            GatePasses = JsonConvert.DeserializeObject<List<GatePasses>>(JsonString);
            if (GatePasses != null)
            {
                GatePasses = GatePasses.OrderByDescending(x => x.Id).ToList();
                TotalRecords = GatePasses.Count;
                //_SelectedRecord = 10;
                //UpdateRecordCount();
                UpdateCollection(GatePasses.Take(_SelectedRecord));
                SetGPDynamicTable();
                CheckAndEnableButtonForGatePass();
            }
            else
            {
                List<GatePasses> GatePasses = new List<GatePasses>();
            }
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

        public void SetGPDynamicTable()
        {
            try
            {
                GatePassGrid.ItemsSource = Result2;
                //TableContainer1.Content = GatePassGrid;
                GatePassGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetGPDynamicTable:" + ex.Message);
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var s = sender as DataGrid;
            SelectedRow = s.SelectedItem as SAPDataBackUp;
            if (SelectedRow != null)
            {
                SAPDataBackUp.FirstOrDefault(t => t.Id == SelectedRow.Id).IsSelected = !SAPDataBackUp.FirstOrDefault(t => t.Id == SelectedRow.Id).IsSelected;
                CheckAndEnableButton();
            }            
        }

        private void DataGrid_SelectionChanged1(object sender, SelectionChangedEventArgs e)
        {
            var s = sender as DataGrid;
            SelectedRowGatePass = s.SelectedItem as GatePasses;
            if (SelectedRowGatePass != null)
            {
                GatePasses.FirstOrDefault(t => t.Id == SelectedRowGatePass.Id).IsSelected = !GatePasses.FirstOrDefault(t => t.Id == SelectedRowGatePass.Id).IsSelected;
                CheckAndEnableButtonForGatePass();
                NumberOfPages = 1;
                UpdateRecordCount();
            }
        }



        private void HeaderCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as System.Windows.Controls.CheckBox;

            if (HeaderCheckbox.IsChecked.HasValue && HeaderCheckbox.IsChecked.Value)
            {
                Result1.ForEach(x => x.IsSelected = true);
                SetDynamicTable();
            }
            else
            {
                Result1.ForEach(x => x.IsSelected = false);
                SetDynamicTable();
            }

            CheckAndEnableButton();
        }

        private void ColumnCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckAndEnableButton();
        }

        private void ColumnCheckBox_Checked1(object sender, RoutedEventArgs e)
        {
            CheckAndEnableButtonForGatePass();
        }

        private void CheckAndEnableButton()
        {
            selectedRFIDAllocations = SAPDataBackUp.Where(x => x.IsSelected).ToList();
            Dispatcher.Invoke(() =>
            {
                if (selectedRFIDAllocations.Count > 0)
                {
                    SyncButton.IsEnabled = true;
                }
                else
                {
                    SyncButton.IsEnabled = false;
                }
            });
        }

        private void CheckAndEnableButtonForGatePass()
        {
            selectedGatePasses = GatePasses.Where(x => x.IsSelected).ToList();
            Dispatcher.Invoke(() =>
            {
                if (selectedGatePasses.Count > 0)
                {
                    closeGpBtn.IsEnabled = true;
                }
                else
                {
                    closeGpBtn.IsEnabled = false;
                }
            });
        }

        public void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRFIDAllocations != null)
            {
                Task.Run(() => SyncData());
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Sync Initiated");
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select record!!");
            }
        }

        private async void SyncData()
        {
            try
            {
                foreach (SAPDataBackUp sapDataBackUp in selectedRFIDAllocations)
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

                    GetSAPDataBackUpsList();
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SAPSync/SyncData/Exception:- " + ex.Message, ex);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void UpdateCollection(IEnumerable<Object> enumerables)
        {
            if (selectedTab=="1")
            {
                Result1.Clear();
                foreach (var enumera in enumerables)
                {
                    Result1.Add((SAPDataBackUp)enumera);
                }
                SetDynamicTable();
            }
            else
            {
                Result2.Clear();
                foreach (var enumera in enumerables)
                {
                    Result2.Add((GatePasses)enumera);
                }
                SetGPDynamicTable();
            }
        }

        public void UpdateRecordCount()
        {
            if (selectedTab == "1") {
                NumberOfPages = (int)Math.Ceiling((double)SAPDataBackUp.Count / _SelectedRecord);
                NumberOfPages = NumberOfPages == 0 ? 1 : NumberOfPages;
                CurrentPage = 1;
                UpdateCollection(SAPDataBackUp.Take(_SelectedRecord));
            }
            else
            {
                NumberOfPages = (int)Math.Ceiling((double)GatePasses.Count / _SelectedRecord);
                NumberOfPages = NumberOfPages == 0 ? 1 : NumberOfPages;
                CurrentPage = 1;
                UpdateCollection(GatePasses.Take(_SelectedRecord));
            }
        }
        public void UpdateEnableStatus()
        {
            IsFirstEnable = CurrentPage > 1;
            IsPreviousEnable = CurrentPage > 1;
            IsNextEnable = CurrentPage < NumberOfPages;
            IsLastEnable = CurrentPage < NumberOfPages;
        }

        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            UpdateCollection(SAPDataBackUp.Take(_SelectedRecord));
            CurrentPage = 1;
            UpdateEnableStatus();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
            RecordStartFrom = _SelectedRecord * (CurrentPage - 1);
            var RecordToShow = SAPDataBackUp.Skip(RecordStartFrom).Take(_SelectedRecord);
            UpdateCollection(RecordToShow);
            UpdateEnableStatus();
        }
        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            RecordStartFrom = CurrentPage * _SelectedRecord;
            var RecordToShow = SAPDataBackUp.Skip(RecordStartFrom).Take(_SelectedRecord);
            UpdateCollection(RecordToShow);
            CurrentPage++;
            UpdateEnableStatus();

        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            var reordsToSkip = _SelectedRecord * (NumberOfPages - 1);
            UpdateCollection(SAPDataBackUp.Skip(reordsToSkip));
            CurrentPage = NumberOfPages;
            UpdateEnableStatus();
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            GetSAPDataBackUpsList();
        }

        private void gpFirstPage_Click(object sender, RoutedEventArgs e)
        {
            UpdateCollection(GatePasses.Take(_SelectedRecord));
            CurrentPage = 1;
            UpdateEnableStatus();
        }

        private void gpPreviousPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
            //RecordStartFrom = Result.Count - _SelectedRecord * (NumberOfPages - (CurrentPage - 1));
            RecordStartFrom = _SelectedRecord * (CurrentPage - 1);
            var RecordToShow = GatePasses.Skip(RecordStartFrom).Take(_SelectedRecord);
            UpdateCollection(RecordToShow);
            UpdateEnableStatus();
        }

        private void gpNextPage_Click(object sender, RoutedEventArgs e)
        {
            RecordStartFrom = CurrentPage * _SelectedRecord;
            var RecordToShow = GatePasses.Skip(RecordStartFrom).Take(_SelectedRecord);
            UpdateCollection(RecordToShow);
            CurrentPage++;
            UpdateEnableStatus();
        }

        private void gpLastPage_Click(object sender, RoutedEventArgs e)
        {
            var reordsToSkip = _SelectedRecord * (NumberOfPages - 1);
            UpdateCollection(GatePasses.Skip(reordsToSkip));
            CurrentPage = NumberOfPages;
            UpdateEnableStatus();
        }

        private void gpReload_Click(object sender, RoutedEventArgs e)
        {
            GetGatePassData();
        }

        private void Tabcontrol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem ti = Tabcontrol.SelectedItem as TabItem;
            selectedTab = ti.Uid;
            if (selectedTab == "1")
            {
                //_SelectedRecord = 10;
                NumberOfPages = 1;
                //GetSAPDataBackUpsList();
                UpdateRecordCount();
            }
            else
            {
                if (selectedGatePasses == null)
                {
                    //_SelectedRecord = 10;
                    NumberOfPages = 1;
                    GetGatePassData();
                    UpdateRecordCount();
                }                
            }
        }

        private void PaginatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _SelectedRecord = Convert.ToInt32(PaginatorComboBox.SelectedValue.ToString());
            UpdateRecordCount();
        }

        private void gpPaginatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _SelectedRecord = Convert.ToInt32(gpPaginatorComboBox.SelectedValue.ToString());
            UpdateRecordCount();
        }

        private void closeGpBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedGatePasses != null)
            {
                foreach (var item in selectedGatePasses)
                {
                    this._dbContext.ExecuteQuery($"update [GatePasses] set [Status]='Closed-Manual' where Id='{item.Id}'");
                }
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Gatepasses Closed!!!");
                GetGatePassData();
            }
        }
    }
}
