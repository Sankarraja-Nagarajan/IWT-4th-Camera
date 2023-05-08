using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for OtherSettingsDialog.xaml
    /// </summary>
    public partial class OtherSettingsDialog : UserControl
    {
        AdminDBCall dbCall = new AdminDBCall();
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        private OtherSettings _otherSettings = new OtherSettings();
        public MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        string hardwareProfile;
        public OtherSettingsDialog()
        {
            InitializeComponent();
            toastViewModel = new ToastViewModel();
            hardwareProfile = mainWindow.HardwareProfile;
            Loaded += OtherSettingsDialog_Loaded;
        }

        private void OtherSettingsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            hardwareProfile = mainWindow.HardwareProfile;
            GetOtherConfig();
        }
        public void GetOtherConfig()
        {
            try
            {
                DataTable table = dbCall.GetAllData($"SELECT * FROM Other_Settings where HardwareProfile='{hardwareProfile}'");
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<OtherSettings>>(JSONString);
                var oc = result.FirstOrDefault();
                if (oc != null)
                {
                    _otherSettings = oc;
                    SetOtherConfig();
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For Other Settings !!");
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MailSettingsDialog/GetOtherConfig/Exception:- " + ex.Message, ex);
            }
        }
        public void SetOtherConfig()
        {
            smsAlert.IsChecked = _otherSettings.SMSAlerts;
            dosPrint.IsChecked = _otherSettings.DosPrint;
            autoPrint.IsChecked = _otherSettings.AutoPrint;
            autoMail.IsChecked = _otherSettings.AutoMail;
            aftSMS.IsChecked = _otherSettings.AutoFtSMS;
            aftTP.IsChecked = _otherSettings.AutoFtPrint;
            aftMail.IsChecked = _otherSettings.AutoFtMail;
            noOfCopies.Text = _otherSettings.NoOfCopies.ToString();
            tareExpiry.Text = _otherSettings.TareExpirePeriod.ToString();
        }
        public void SaveOtherConfig()
        {
            _otherSettings.SMSAlerts = (bool)smsAlert.IsChecked;
            _otherSettings.DosPrint = (bool)dosPrint.IsChecked;
            _otherSettings.AutoPrint = (bool)autoPrint.IsChecked;
            _otherSettings.AutoMail = (bool)autoMail.IsChecked;
            _otherSettings.AutoFtSMS = (bool)aftSMS.IsChecked;
            _otherSettings.AutoFtPrint = (bool)aftTP.IsChecked;
            _otherSettings.AutoFtMail = (bool)aftMail.IsChecked;
            _otherSettings.NoOfCopies = int.Parse(noOfCopies.Text);
            _otherSettings.TareExpirePeriod = int.Parse(tareExpiry.Text);
            string saveQuery = "";
            if (_otherSettings.ID > 0)
            {
                saveQuery = $@"UPDATE [Other_Settings] SET SMSAlerts='{_otherSettings.SMSAlerts}',DosPrint='{_otherSettings.DosPrint}',AutoPrint='{_otherSettings.AutoPrint}',AutoMail='{_otherSettings.AutoMail}',AutoFtSMS='{_otherSettings.AutoFtSMS}',AutoFtPrint='{_otherSettings.AutoFtPrint}',AutoFtMail='{_otherSettings.AutoFtMail}',NoOfCopies='{_otherSettings.NoOfCopies}',TareExpirePeriod='{_otherSettings.TareExpirePeriod}' WHERE ID='{_otherSettings.ID}' and HardwareProfile='{_otherSettings.HardwareProfile}'";
            }
            else
            {
                saveQuery = $@"INSERT INTO [Other_Settings] (SMSAlerts,DosPrint,AutoPrint,AutoMail,AutoFtSMS,AutoFtPrint,AutoFtMail,NoOfCopies,TareExpirePeriod,HardwareProfile) VALUES ('{_otherSettings.SMSAlerts}','{_otherSettings.DosPrint}','{_otherSettings.AutoPrint}','{_otherSettings.AutoMail}','{_otherSettings.AutoFtSMS}','{_otherSettings.AutoFtPrint}','{_otherSettings.AutoFtMail}','{_otherSettings.NoOfCopies}','{_otherSettings.TareExpirePeriod}','{_otherSettings.HardwareProfile}')";
            }
            var res = dbCall.ExecuteQuery(saveQuery);
            if (res)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Other configuration saved successfully !!");
                DialogHost.CloseDialogCommand.Execute("cctv", null);
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !!");
            }
        }
        private void Previous_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute("weighbridge", null);
        }
        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveOtherConfig();
        }
        public void ShowMessage(Action<string> message, string name)
        {
            this.Dispatcher.Invoke(() =>
            {
                LastMessage = $"{name}";
                message(LastMessage);
            });
        }
        private void Constant_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
