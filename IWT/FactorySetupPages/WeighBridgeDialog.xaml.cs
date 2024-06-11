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
    /// Interaction logic for WeighBridgeDialog.xaml
    /// </summary>
    public partial class WeighBridgeDialog : UserControl
    {
        AdminDBCall dbCall = new AdminDBCall();
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        private WeighbridgeSettings _wbSettings = new WeighbridgeSettings();
        public MainWindow mainWindow = new MainWindow();
        string hardwareProfile;
        public WeighBridgeDialog()
        {
            InitializeComponent();
            toastViewModel = new ToastViewModel();
            hardwareProfile = mainWindow.HardwareProfile;
            Loaded += WeighBridgeDialog_Loaded;
        }

        private void WeighBridgeDialog_Loaded(object sender, RoutedEventArgs e)
        {
            hardwareProfile = mainWindow.HardwareProfile;
            GetWBConfig();
        }
        public void GetWBConfig()
        {
            try
            {
                DataTable table = dbCall.GetAllData($"SELECT * FROM Weighbridge_Settings where HardwareProfile='{hardwareProfile}'");
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<WeighbridgeSettings>>(JSONString);
                var wbc = result.FirstOrDefault();
                if (wbc != null)
                {
                    _wbSettings = wbc;
                    SetWBConfig();
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For Other Weighbridge !!");
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MailSettingsDialog/GetSoftwareConfig/Exception:- " + ex.Message, ex);
            }
        }
        public void SetWBConfig()
        {
            Host.Text = _wbSettings.Host;
            Port.Text = _wbSettings.Port.ToString();
        }
        public void SaveWBConfig()
        {
            _wbSettings.Host = Host.Text;
            var port1 = 4002;
            int.TryParse(Port.Text, out port1);
            _wbSettings.Port = port1;
            string saveQuery = "";
            if (_wbSettings.ID > 0)
            {
                saveQuery = $@"UPDATE [Weighbridge_Settings] SET Host='{_wbSettings.Host}',Port='{_wbSettings.Port}' WHERE ID='{_wbSettings.ID}' and HardwareProfile='{_wbSettings.HardwareProfile}'";
            }
            else
            {
                saveQuery = $@"INSERT INTO [Weighbridge_Settings] (Host,Port,HardwareProfile) VALUES ('{_wbSettings.Host}','{_wbSettings.Port}','{_wbSettings.HardwareProfile}')";
            }
            var res = dbCall.ExecuteQuery(saveQuery);
            if (res)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Weigh bridge configuration saved successfully !!");
                DialogHost.CloseDialogCommand.Execute("other", null);
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !!");
            }
        }

        private void Previous_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute("email", null);
        }
        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveWBConfig();
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
