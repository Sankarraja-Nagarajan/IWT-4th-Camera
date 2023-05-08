using IWT.Admin_Pages;
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
    /// Interaction logic for SystemConfiguration.xaml
    /// </summary>
    public partial class SystemConfiguration : UserControl, INotifyPropertyChanged
    {

        List<SystemConfigurations> systemConfiguration = new List<SystemConfigurations>();
        SystemConfigurations systemConfig = new SystemConfigurations();
        public CommonFunction commonFunction = new CommonFunction();
        public ManageUser manageUser = new ManageUser();
        public MasterDBCall masterDBCall = new MasterDBCall();
        public List<string> RFIDAllocationTypeList = new List<string>();


        private readonly ToastViewModel toastViewModel;
        private AdminDBCall _dbContext;
        private SystemConfigurations SelectedRow;

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

        List<SystemConfigurations> Result1 = new List<SystemConfigurations>();
        List<Usermanage> UserManage = new List<Usermanage>();

        Setting_DBCall db = new Setting_DBCall();
        List<HardwareProfileModel> HardwareProfile = new List<HardwareProfileModel>();

        public int CurrentPage
        {
            get { return _CurrentPage; }
            set
            {
                _CurrentPage = value;
                OnPropertyChanged();
            }
        }

        public int NumberOfPages
        {
            get { return _NumberOfPages; }
            set
            {
                _NumberOfPages = value;
                OnPropertyChanged();
            }
        }

        public int SelectedRecord
        {
            get { return _SelectedRecord; }
            set
            {
                _SelectedRecord = value;
                OnPropertyChanged();
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
        public SystemConfiguration()
        {
            InitializeComponent();
            DataContext = this;
            _dbContext = new AdminDBCall();
            toastViewModel = new ToastViewModel();
            Loaded += SystemConfigurationUserControl_Loaded;
        }

        private void SystemConfigurationUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SelectedRecord = 10;
            NumberOfPages = 1;
            PaginatorComboBox.ItemsSource = ItemPerPagesList;
            GetHardwareProfile();
            GetSystemConfigurationList();            
            DisableFields();
        }

        public void GetHardwareProfile()
        {
            try
            {
                DataTable table = db.GetCCTVData("SELECT * FROM Hardware_Profile");
                if (table != null && table.Rows.Count > 0)
                {
                    string JSONString = JsonConvert.SerializeObject(table);
                    HardwareProfile = JsonConvert.DeserializeObject<List<HardwareProfileModel>>(JSONString);
                    SetProfileComboBox();
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For HardWare Profile!!");
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetRFIDMasters", ex);
            }
        }

        public void SetProfileComboBox()
        {
            try
            {
                HardwareProfileName.ItemsSource = HardwareProfile;
                HardwareProfileName.Items.Refresh();
                HardwareProfileName.DisplayMemberPath = "ProfileName";
                HardwareProfileName.SelectedValuePath = "ProfileName";
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetProfileComboBox", ex);
            }
        }        

        public void GetSystemConfigurationList()
        {
            DataTable table = _dbContext.GetAllData($"select * from [Sytem_Configuration]");
            var JsonString = JsonConvert.SerializeObject(table);
            systemConfiguration = JsonConvert.DeserializeObject<List<SystemConfigurations>>(JsonString);
            if (systemConfiguration != null)
            {
                TotalRecords = systemConfiguration.Count;
                SelectedRecord = 10;
                UpdateRecordCount();
                UpdateCollection(systemConfiguration.Take(SelectedRecord));
                SetDynamicTable();
            }
        }        

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var s = sender as DataGrid;
            SelectedRow = s.SelectedItem as SystemConfigurations;
            if (SelectedRow != null)
            {
                SetValuesOnFields();
            }
        }

        public void SetValuesOnFields()
        {
            Name.Text = SelectedRow.Name;            
            HardwareProfileName.Text = SelectedRow.HardwareProfile;
            EnableFields();
        }

        public void DisableFields()
        {
            HardwareProfileName.IsEnabled = false;
            SaveButton.IsEnabled = false;
            HardwareProfileName.SelectedIndex = -1;
            Name.Text = "";
            SelectedRow = null;
        }

        public void EnableFields()
        {            
            HardwareProfileName.IsEnabled = true;
            SaveButton.IsEnabled = true;
        }

        public void GetValueFromFields()
        {
            systemConfig.Name = Name.Text;            
            systemConfig.HardwareProfile = HardwareProfileName.Text;            
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetValueFromFields();
                string Query = $@"UPDATE [Sytem_Configuration] SET Name='{systemConfig.Name}',HardwareProfile='{systemConfig.HardwareProfile}' where Id='{SelectedRow.Id}'";
                SqlCommand cmd = new SqlCommand(Query);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Selected Row Updated Successfully!");
                GetSystemConfigurationList();
                DisableFields();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Save_Button_Click" + ex.Message);
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

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void UpdateCollection(IEnumerable<SystemConfigurations> enumerables)
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
            NumberOfPages = (int)Math.Ceiling((double)systemConfiguration.Count / SelectedRecord);
            NumberOfPages = NumberOfPages == 0 ? 1 : NumberOfPages;
            UpdateCollection(systemConfiguration.Take(SelectedRecord));
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
            UpdateCollection(systemConfiguration.Take(SelectedRecord));
            CurrentPage = 1;
            UpdateEnableStatus();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
            //RecordStartFrom = Result.Count - SelectedRecord * (NumberOfPages - (CurrentPage - 1));
            RecordStartFrom = SelectedRecord * (CurrentPage - 1);
            var RecordToShow = systemConfiguration.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            UpdateEnableStatus();
        }
        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            RecordStartFrom = CurrentPage * SelectedRecord;
            var RecordToShow = systemConfiguration.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            CurrentPage++;
            UpdateEnableStatus();

        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            var reordsToSkip = SelectedRecord * (NumberOfPages - 1);
            UpdateCollection(systemConfiguration.Skip(reordsToSkip));
            CurrentPage = NumberOfPages;
            UpdateEnableStatus();
        }
    }
}
