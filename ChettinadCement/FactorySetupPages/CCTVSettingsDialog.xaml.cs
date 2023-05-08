using IWT.DBCall;
using IWT.Models;
using IWT.Setting_Pages;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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

namespace IWT.FactorySetupPages
{
    /// <summary>
    /// Interaction logic for CCTVSettingsDialog.xaml
    /// </summary>
    public partial class CCTVSettingsDialog : UserControl
    {
        AdminDBCall dbCall = new AdminDBCall();
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        private CCTVSettings _cctvSettings1 = new CCTVSettings();
        private CCTVSettings _cctvSettings2 = new CCTVSettings();
        private CCTVSettings _cctvSettings3 = new CCTVSettings();
        public MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        string hardwareProfile;
        public CCTVSettingsDialog()
        {
            InitializeComponent();
            hardwareProfile = mainWindow.HardwareProfile;
            toastViewModel = new ToastViewModel();
            Loaded += CCTVSettingsDialog_Loaded;
        }

        private void CCTVSettingsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            hardwareProfile = mainWindow.HardwareProfile;
            GetCCTVConfig();
        }
        public void GetCCTVConfig()
        {
            try
            {
                DataTable table = dbCall.GetAllData($"select * from [CCTV_Settings] WHERE [HarwareProfile]='{hardwareProfile}'");
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<CCTVSettings>>(JSONString);
                if (result != null)
                {
                    var cctv1 = result.FirstOrDefault(t => t.RecordID == 1);
                    var cctv2 = result.FirstOrDefault(t => t.RecordID == 2);
                    var cctv3 = result.FirstOrDefault(t => t.RecordID == 3);
                    if (cctv1 != null)
                    {
                        _cctvSettings1 = cctv1;
                        SetCCTVConfig1();
                    }
                    if (cctv2 != null)
                    {
                        _cctvSettings2 = cctv2;
                        SetCCTVConfig2();
                    }
                    if (cctv3 != null)
                    {
                        _cctvSettings3 = cctv3;
                        SetCCTVConfig3();
                    }
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For CCTV !!");
                }
                
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MailSettingsDialog/GetSoftwareConfig/Exception:- " + ex.Message, ex);
            }
        }
        public void SetCCTVConfig1()
        {
            captureUrl1.Text = _cctvSettings1.CaptureURL;
            cameraType1.Text = _cctvSettings1.CameraType;
            streamUrl1.Text = _cctvSettings1.IPAddress;
            cameraUsername1.Text = _cctvSettings1.CameraUserName;
            cameraPassword1.Password = _cctvSettings1.CameraPassword;
            logFilePath1.Text = _cctvSettings1.LogFolder;
            cameraEnable1.IsChecked = _cctvSettings1.Enable;
        }
        public void SetCCTVConfig2()
        {
            captureUrl2.Text = _cctvSettings1.CaptureURL;
            cameraType2.Text = _cctvSettings1.CameraType;
            streamUrl2.Text = _cctvSettings1.IPAddress;
            cameraUsername2.Text = _cctvSettings1.CameraUserName;
            cameraPassword2.Password = _cctvSettings1.CameraPassword;
            logFilePath2.Text = _cctvSettings1.LogFolder;
            cameraEnable2.IsChecked = _cctvSettings1.Enable;
        }
        public void SetCCTVConfig3()
        {
            captureUrl3.Text = _cctvSettings1.CaptureURL;
            cameraType3.Text = _cctvSettings1.CameraType;
            streamUrl3.Text = _cctvSettings1.IPAddress;
            cameraUsername3.Text = _cctvSettings1.CameraUserName;
            cameraPassword3.Password = _cctvSettings1.CameraPassword;
            logFilePath3.Text = _cctvSettings1.LogFolder;
            cameraEnable3.IsChecked = _cctvSettings1.Enable;
        }
        private void Previous_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute("other", null);
        }
        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute("file", null);
        }
        public void ShowMessage(Action<string> message, string name)
        {
            this.Dispatcher.Invoke(() =>
            {
                LastMessage = $"{name}";
                message(LastMessage);
            });
        }
        public string GetSaveQuery(CCTVSettings settings)
        {
            string saveQuery = "";
            if (_cctvSettings1.ID > 0)
            {
                saveQuery = $@"UPDATE [CCTV_Settings] SET RecordID='{settings.RecordID}',CaptureURL='{settings.CaptureURL}',CameraType='{settings.CameraType}',IPAddress='{settings.IPAddress}',CameraUserName='{settings.CameraUserName}',CameraPassword='{settings.CameraPassword}',LogFolder='{settings.LogFolder}',Enable='{settings.Enable}' WHERE ID='{settings.ID}' and HarwareProfile='{settings.HardwareProfile}'";
            }
            else
            {
                saveQuery = $@"INSERT INTO [CCTV_Settings] (RecordID,CaptureURL,CameraType,IPAddress,CameraUserName,CameraPassword,LogFolder,Enable,HarwareProfile) VALUES ('{settings.RecordID}','{settings.CaptureURL}','{settings.CameraType}','{settings.IPAddress}','{settings.CameraUserName}','{settings.CameraPassword}','{settings.LogFolder}','{settings.Enable}','{settings.HardwareProfile}')";
            }
            return saveQuery;
        }
        private void SaveButton1_Click(object sender, RoutedEventArgs e)
        {
            _cctvSettings1.RecordID = 1;
            _cctvSettings1.CaptureURL = captureUrl1.Text;
            _cctvSettings1.CameraType = cameraType1.Text;
            _cctvSettings1.IPAddress = streamUrl1.Text;
            _cctvSettings1.CameraUserName = cameraUsername1.Text;
            _cctvSettings1.CameraPassword = cameraPassword1.Password;
            _cctvSettings1.LogFolder = logFilePath1.Text;
            _cctvSettings1.Enable = (bool)cameraEnable1.IsChecked;

            var res = dbCall.ExecuteQuery(GetSaveQuery(_cctvSettings1));
            if (res)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "CCTV1 configuration saved successfully !!");
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !!");
            }
        }

        private void SaveButton2_Click(object sender, RoutedEventArgs e)
        {
            _cctvSettings2.RecordID = 2;
            _cctvSettings2.CaptureURL = captureUrl2.Text;
            _cctvSettings2.CameraType = cameraType2.Text;
            _cctvSettings2.IPAddress = streamUrl2.Text;
            _cctvSettings2.CameraUserName = cameraUsername2.Text;
            _cctvSettings2.CameraPassword = cameraPassword2.Password;
            _cctvSettings2.LogFolder = logFilePath2.Text;
            _cctvSettings2.Enable = (bool)cameraEnable2.IsChecked;
            var res = dbCall.ExecuteQuery(GetSaveQuery(_cctvSettings2));
            if (res)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "CCTV2 configuration saved successfully !!");
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !!");
            }
        }

        private void SaveButton3_Click(object sender, RoutedEventArgs e)
        {
            _cctvSettings3.RecordID = 3;
            _cctvSettings3.CaptureURL = captureUrl3.Text;
            _cctvSettings3.CameraType = cameraType3.Text;
            _cctvSettings3.IPAddress = streamUrl3.Text;
            _cctvSettings3.CameraUserName = cameraUsername3.Text;
            _cctvSettings3.CameraPassword = cameraPassword3.Password;
            _cctvSettings3.LogFolder = logFilePath3.Text;
            _cctvSettings3.Enable = (bool)cameraEnable3.IsChecked;
            var res = dbCall.ExecuteQuery(GetSaveQuery(_cctvSettings3));
            if (res)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "CCTV3 configuration saved successfully !!");
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !!");
            }
        }

        private void ChooseFolder1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            logFilePath1.Text = GetFolderLocation();
        }
        private void ChooseFolder2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            logFilePath2.Text = GetFolderLocation();
        }
        private void ChooseFolder3_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            logFilePath3.Text = GetFolderLocation();
        }
        public string GetFolderLocation()
        {
            var browseFolder = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var result = browseFolder.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                return browseFolder.SelectedPath;
            }
            else
            {
                return "";
            }
        }
    }
}
