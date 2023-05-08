using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace IWT.Views
{
    /// <summary>
    /// Interaction logic for RFIDTagMasterControl.xaml
    /// </summary>
    public partial class RFIDTagMasterControl : UserControl, INotifyPropertyChanged
    {
        public CommonFunction commonFunction = new CommonFunction();
        RFIDMaster SelectedRFIDMaster = new RFIDMaster();
        public MasterDBCall masterDBCall = new MasterDBCall();
        public List<CustomFieldBuilder> CustomFieldsBuilder = new List<CustomFieldBuilder>();
        private readonly ToastViewModel toastViewModel;
        private AdminDBCall _dbContext;
        private RFIDMaster SelectedRow;
        public List<KeyValuePair<string, bool>> AvailabilityList = new List<KeyValuePair<string, bool>> { };

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

        List<RFIDMaster> RFIDMasters = new List<RFIDMaster>();
        List<RFIDMaster> RFIDMasters1 = new List<RFIDMaster>();

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

        public RFIDTagMasterControl(string _Rolename, RolePriviliege _rolePriviliege)
        {
            InitializeComponent();
            DataContext = this;
            _dbContext = new AdminDBCall();
            toastViewModel = new ToastViewModel();
            Loaded += RFIDTagMasterControl_Loaded;
        }

        private void RFIDTagMasterControl_Loaded(object sender, RoutedEventArgs e)
        {
            SelectedRecord = 10;
            NumberOfPages = 1;
            AvailabilityList.Clear();
            AvailabilityList.Add(new KeyValuePair<string, bool>("Active", true));
            AvailabilityList.Add(new KeyValuePair<string, bool>("Inactive", false));
            SetRFIDComboBox();
            GetRFIDMasters();            
            PaginatorComboBox.ItemsSource = ItemPerPagesList;
        }

        private void New_Button_Click(object sender, RoutedEventArgs e)
        {
            ResetRFIDMaster();
            RFIDNo.IsReadOnly = false;
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedRow == null)
                {
                    if (!string.IsNullOrEmpty(RFIDNo.Text))
                    {
                        GetRFIDMasterValues();
                        CreateRFIDMaster();
                    }
                    else
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Please Enter the RFID No");
                    }
                }
                else
                {
                    GetRFIDMasterValues();
                    UpdateRFIDMaster();
                }               
            }
            catch (Exception ex)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, ex.Message);
            }
        }

        public void SetDynamicTable()
        {
            try
            {
                MaterialGrid5.ItemsSource = RFIDMasters1;
                TableContainer.Content = MaterialGrid5;
                MaterialGrid5.Items.Refresh();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetDynamicTable:" + ex.Message);
            }
        }

        public void GetRFIDMasters1()
        {
            RFIDMasters1 = RFIDMasters;
        }

        public void SetRFIDComboBox()
        {
            try
            {
                IsDeleted.ItemsSource = AvailabilityList;
                IsDeleted.Items.Refresh();
                IsDeleted.DisplayMemberPath = "Key";
                IsDeleted.SelectedValuePath = "Value";
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetRFIDComboBox", ex);
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var s = sender as DataGrid;
            SelectedRow = s.SelectedItem as RFIDMaster;
            if (SelectedRow != null)
            {
                GetRFIDMasters1();
                SetRFIDMasterValues();
                RFIDNo.IsReadOnly = true;
            }
        }

        private void SetRFIDMasterValues()
        {
            RFIDNo.Text = SelectedRow.Tag;
            Status.Text = SelectedRow.Status;
            IsDeleted.SelectedIndex = SelectedRow.IsActive ? 0 : 1;
            VehicleNo.Text= SelectedRow.VehicleNo;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void UpdateCollection(IEnumerable<RFIDMaster> enumerables)
        {
            RFIDMasters1.Clear();
            foreach (var enumera in enumerables)
            {
                RFIDMasters1.Add(enumera);
            }
            SetDynamicTable();
        }

        public void UpdateRecordCount()
        {
            NumberOfPages = (int)Math.Ceiling((double)RFIDMasters.Count / SelectedRecord);
            NumberOfPages = NumberOfPages == 0 ? 1 : NumberOfPages;
            UpdateCollection(RFIDMasters.Take(SelectedRecord));
            CurrentPage = 1;
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
            UpdateCollection(RFIDMasters.Take(SelectedRecord));
            CurrentPage = 1;
            UpdateEnableStatus();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
            //RecordStartFrom = Result.Count - SelectedRecord * (NumberOfPages - (CurrentPage - 1));
            RecordStartFrom = SelectedRecord * (CurrentPage - 1);
            var RecordToShow = RFIDMasters.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            UpdateEnableStatus();
        }
        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            RecordStartFrom = CurrentPage * SelectedRecord;
            var RecordToShow = RFIDMasters.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            CurrentPage++;
            UpdateEnableStatus();

        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            var reordsToSkip = SelectedRecord * (NumberOfPages - 1);
            UpdateCollection(RFIDMasters.Skip(reordsToSkip));
            CurrentPage = NumberOfPages;
            UpdateEnableStatus();
        }

        public void GetRFIDMasterValues()
        {
            try
            {
                SelectedRFIDMaster = new RFIDMaster();
                SelectedRFIDMaster.Tag = RFIDNo.Text;
                SelectedRFIDMaster.Status = Status.Text;
                SelectedRFIDMaster.IsActive = IsDeleted.SelectedIndex == 0 ? true:false;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetRFIDMasterValues", ex);
            }
        }

        public void GetRFIDMasters()
        {
            DataTable table = _dbContext.GetAllData($"select * from [RFID_Tag_Master]");
            var JsonString = JsonConvert.SerializeObject(table);
            RFIDMasters = JsonConvert.DeserializeObject<List<RFIDMaster>>(JsonString);
            TotalRecords = RFIDMasters.Count;
            SelectedRecord = 10;
            UpdateRecordCount();
            UpdateCollection(RFIDMasters.Take(SelectedRecord));
            SetDynamicTable();
        }

        private void ResetRFIDMaster()
        {
            RFIDNo.Text = "";            
            SelectedRow = null;
            GetRFIDMasters1();
        }

        public void CreateRFIDMaster()
        {
            try
            {
                string insertQuery = $@"INSERT INTO [RFID_Tag_Master] (Tag,Status,IsActive,VehicleNo) 
                                                Values (@Tag,@Status,@IsActive,@VehicleNo)";

                SqlCommand cmd = new SqlCommand(insertQuery);
                cmd = AddParameters(cmd);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Record updated successfully");
                GetRFIDMasters();
                ResetRFIDMaster();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateRFIDAllocation", ex);
            }
        }

        public void UpdateRFIDMaster()
        {
            try
            {
                string insertQuery = $@"update [RFID_Tag_Master] set IsActive={SelectedRFIDMaster.IsActive}";

                SqlCommand cmd = new SqlCommand(insertQuery);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Record updated successfully");
                GetRFIDMasters();
                ResetRFIDMaster();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateRFIDAllocation", ex);
            }
        }

        public SqlCommand AddParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@Tag", SelectedRFIDMaster.Tag);
            cmd.Parameters.AddWithValue("@Status", "Open");
            cmd.Parameters.AddWithValue("@IsActive", true);
            cmd.Parameters.AddWithValue("@VehicleNo", "");
            return cmd;
        }

        private void Release_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRow != null)
            {
                string query = $"select ge.Status from [RFID_Allocations] ge inner join [RFID_Tag_Master] rm on ge.RFIDTag=rm.Tag where ge.Status='In-Transit' and ge.VehicleNumber='{SelectedRow.VehicleNo}' and ge.[AllocationType]='Temporary'";
                var res = masterDBCall.GetData(new SqlCommand(query), CommandType.Text);
                if (res!=null && res.Rows.Count > 0)
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError , "Vehicle is in transist or temporary card!!");
                }
                else
                {
                    string longTermQuery = $@"update [RFID_Allocations] set status='Released-RFIDMaster' where RFIDTag='{SelectedRow.Tag}' and AllocationType in ('Long-term Same Material','Long-term Different Material')";
                    string updateQuery = $@"update [RFID_Tag_Master] set Status='Open',VehicleNo='' where Tag='{SelectedRow.Tag}'";
                    string vehicleQuery = $@"update [Vehicle_Master] set Status='' where VehicleNumber='{SelectedRow.VehicleNo}'";
                    SqlCommand cmd = new SqlCommand(updateQuery);
                    masterDBCall.InsertData(cmd, CommandType.Text);
                    SqlCommand cmd1 = new SqlCommand(longTermQuery);
                    masterDBCall.InsertData(cmd1, CommandType.Text);
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Record updated successfully");
                    GetRFIDMasters();
                    ResetRFIDMaster();
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Please select a record!!");
            }
        }
    }    
}
