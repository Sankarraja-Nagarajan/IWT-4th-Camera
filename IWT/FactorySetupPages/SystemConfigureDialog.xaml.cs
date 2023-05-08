using IWT.Admin_Pages;
using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace IWT.FactorySetupPages
{
    /// <summary>
    /// Interaction logic for SystemConfigureDialog.xaml
    /// </summary>
    public partial class SystemConfigureDialog : UserControl
    {
        private AdminDBCall _dbContext;
        private readonly ToastViewModel toastViewModel;
        public CommonFunction commonFunction = new CommonFunction();
        public ManageUser manageUser = new ManageUser();
        SystemConfigurations systemConfig = new SystemConfigurations();
        List<Usermanage> UserManage = new List<Usermanage>();
        string LastMessage;
        Setting_DBCall db = new Setting_DBCall();
        List<HardwareProfileModel> HardwareProfile = new List<HardwareProfileModel>();
        public MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        string hardwareProfile;
        public SystemConfigureDialog()
        {
            InitializeComponent();
            toastViewModel = new ToastViewModel();
            _dbContext = new AdminDBCall();
            Loaded += SystemConfigurationUserControl_Loaded;
        }

        private void SystemConfigurationUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GetHardwareProfile();
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

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveSystemConfiguration();
        }

        public void GetValueFromFields()
        {
            systemConfig.Name = Name.Text;     
            systemConfig.HardwareProfile = HardwareProfileName.Text;           
        }

        public void SaveSystemConfiguration()
        {
            try
            {
                GetValueFromFields();
                if (systemConfig.Name != "" && systemConfig.HardwareProfile != "")
                {
                    commonFunction.InsertSystemConfigurationDetails(systemConfig);
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "System configuration saved successfully !!");
                    SetSystemId(systemConfig.Name);
                    DialogHost.CloseDialogCommand.Execute("software", null);                    
                }
                else
                {                    
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please Fill the Fields !!");
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SaveSystemConfiguration" + ex.Message, ex);
            }
        }

        private void SetSystemId(string systemId)
        {
            try
            {
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configuration.AppSettings.Settings["SystemId"].Value = systemId;
                configuration.Save(ConfigurationSaveMode.Modified, true);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, $"Exception:- {ex.Message}");
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
    }
}
