using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using IWT.Views;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IWT.Setting_Pages
{
    /// <summary>
    /// Interaction logic for Hardware_profile.xaml
    /// </summary>
    public partial class Hardware_profile : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private int _CurrentPage;
        private int _NumberOfPages;
        private int _SelectedRecord;
        private bool _IsFirstEnable;
        private bool _IsPreviousEnable;
        private bool _IsNextEnable;
        private bool _IsLastEnable;
        int RecordStartFrom = 0;
        public int TotalRecords = 0;
        List<int> ItemPerPagesList = new List<int> { 5, 10, 25, 50, 100, 250, 500 };
        string LastMessage;
        private readonly ToastViewModel toastViewModel;
        Setting_DBCall db = new Setting_DBCall();
        List<HardwareProfileModel> HardwareProfile = new List<HardwareProfileModel>();
        List<HardwareProfileModel> Result1 = new List<HardwareProfileModel>();
        public static MainWindow mainWindow = new MainWindow();
        public static Weighing weightingClass = new Weighing();
        string hardwareProfileValue;
        private AdminDBCall _dbContext;
        public Hardware_profile()
        {
            toastViewModel = new ToastViewModel();
            InitializeComponent();
            Loaded += Hardware_profile_Loaded;
            GetHardwareProfile();
            if (!string.IsNullOrEmpty(mainWindow.HardwareProfile))
            {
                Profile.Text = mainWindow.HardwareProfile;
                hardwareProfileValue = Profile.Text;
            }
            else
            {
                Profile.Text = "";
            }
            DataContext = this;
            _dbContext = new AdminDBCall();
        }

        private void Hardware_profile_Loaded(object sender, RoutedEventArgs e)
        {
            SelectedRecord = 10;
            NumberOfPages = 1;
            PaginatorComboBox.ItemsSource = ItemPerPagesList;
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
                    if (HardwareProfile != null)
                    {
                        TotalRecords = HardwareProfile.Count;
                        SelectedRecord = 10;
                        UpdateRecordCount();
                        UpdateCollection(HardwareProfile.Take(SelectedRecord));
                        SetDynamicTable();
                    }
                    else
                    {
                        HardwareProfile = new List<HardwareProfileModel>();
                    }
                    SetProfileComboBox();

                    //if (mainWindow.HardwareProfile == null || mainWindow.HardwareProfile == "")
                    //{
                    //    mainWindow.HardwareProfile = HardwareProfile[0].ProfileName;
                    //    Profile.Text = HardwareProfile[0].ProfileName;
                    //}                                        
                    WriteLog.WriteToFile("InitializeComponent:- InitializeComponentData - Inserted Successfully ");
                }
                else
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For HardWare Profile!!");

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
                Profile.ItemsSource = HardwareProfile;
                Profile.Items.Refresh();
                Profile.DisplayMemberPath = "ProfileName";
                Profile.SelectedValuePath = "ProfileName";
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetProfileComboBox", ex);
            }
        }

        public void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mainWindow.HardwareProfile = (string)Profile.SelectedValue;
        }

        public void SetHardwareProfileFromWindow()
        {
            Profile.Text = hardwareProfileValue;
            CreateProfile.Text = "";
        }

        private void HardwareSave_Click(object sender, RoutedEventArgs e)
        {
            Setting_DBCall db = new Setting_DBCall();
            Hardware_config data = new Hardware_config();
            data.profile = CreateProfile.Text;
            if (data.profile != "")
            {
                DataTable dt1 = db.GetCCTVData($"SELECT * FROM Hardware_Profile where ProfileName='{data.profile}'");
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Profile Name Already Exist!!");
                    //db.UpdateProfileData(data);
                    //WriteLog.WriteToFile("HardwareSave_Click:- UpdateProfileData - Updated Successfully ");
                    //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Updated Successsfully !!");
                    // RFIDno.ItemsSource = items;
                }
                else
                {
                    db.InsertProfileData(data);
                    WriteLog.WriteToFile("HardwareSave_Click:- InsertProfileData - Inserted Successfully ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Profile Data Inserted Successsfully !!");
                    GetHardwareProfile();
                    SetHardwareProfileFromWindow();
                    // RFIDno.ItemsSource = items;
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Enter Profile Field!!");
            }
        }
        public class Hardware_config
        {
            public string profile { get; set; }
        }

        private async void HardwareDelete_Click(object sender, RoutedEventArgs e)
        {
            var res = await OpenConfirmationDialog();
            if (res)
            {
                Setting_DBCall db = new Setting_DBCall();
                Hardware_config data = new Hardware_config();
                DataTable dt1 = db.GetCCTVData("SELECT * FROM Hardware_Profile");
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    db.DeleteProfileData(data);
                    WriteLog.WriteToFile("HardwareDelete_Click:- DeleteProfileData - Deleated Successfully ");
                    ClearFields();
                    WriteLog.WriteToFile("HardwareDelete_Click:- ClearFields - All Fields Cleared ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Profile Data Deleted Successsfully !!");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Insert Profile Data  !!");
                }
            }

        }

        public async Task<bool> OpenConfirmationDialog()
        {
            var view = new ConfirmationDialog("Delete the hardware profile");

            //    //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            return (bool)result;
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {

        }
        public void ClearFields()
        {
            Profile.SelectedIndex = -1;
        }
        public void ShowMessage(Action<string> message, string name)
        {
            this.Dispatcher.Invoke(() =>
            {
                LastMessage = $"{name}";
                message(LastMessage);
            });
        }

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

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void UpdateCollection(IEnumerable<HardwareProfileModel> enumerables)
        {
            Result1.Clear();
            foreach (var enumera in enumerables)
            {
                Result1.Add(enumera);
            }
            SetDynamicTable();
        }

        public void SetDynamicTable()
        {
            try
            {
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

        public void UpdateRecordCount()
        {
            NumberOfPages = (int)Math.Ceiling((double)HardwareProfile.Count / SelectedRecord);
            NumberOfPages = NumberOfPages == 0 ? 1 : NumberOfPages;
            UpdateCollection(HardwareProfile.Take(SelectedRecord));
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
            UpdateCollection(HardwareProfile.Take(SelectedRecord));
            CurrentPage = 1;
            UpdateEnableStatus();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
            RecordStartFrom = SelectedRecord * (CurrentPage - 1);
            var RecordToShow = HardwareProfile.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            UpdateEnableStatus();
        }
        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            RecordStartFrom = CurrentPage * SelectedRecord;
            var RecordToShow = HardwareProfile.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            CurrentPage++;
            UpdateEnableStatus();

        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            var reordsToSkip = SelectedRecord * (NumberOfPages - 1);
            UpdateCollection(HardwareProfile.Skip(reordsToSkip));
            CurrentPage = NumberOfPages;
            UpdateEnableStatus();
        }
    }
}
