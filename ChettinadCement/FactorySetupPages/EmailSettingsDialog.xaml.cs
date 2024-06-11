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
    /// Interaction logic for EmailSettingsDialog.xaml
    /// </summary>
    public partial class EmailSettingsDialog : UserControl
    {
        AdminDBCall dbCall = new AdminDBCall();
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        private MailSettings _mailSettings = new MailSettings();
        public EmailSettingsDialog()
        {
            InitializeComponent();
            toastViewModel = new ToastViewModel();
            Loaded += EmailSettingsDialog_Loaded;
        }

        private void EmailSettingsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            GetMailConfig();
        }
        public void GetMailConfig()
        {
            try
            {
                DataTable table = dbCall.GetAllData("select * from [Mail_Settings]");
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<MailSettings>>(JSONString);
                var mc = result.FirstOrDefault();
                if (mc != null)
                {
                    _mailSettings = mc;
                    SetMailConfig();
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MailSettingsDialog/GetSoftwareConfig/Exception:- " + ex.Message, ex);
            }
        }
        public void SetMailConfig()
        {
            ToMail.Text = _mailSettings.ToID;
            CCMail.Text = _mailSettings.CCList;
            FromMail.Text = _mailSettings.FromID;
            Password.Password = _mailSettings.Password;
            Subject.Text = _mailSettings.Subject;
            Message.Text = _mailSettings.Message;
        }
        public void SaveMailConfig()
        {
            _mailSettings.ToID = ToMail.Text;
            _mailSettings.CCList = CCMail.Text;
            _mailSettings.FromID = FromMail.Text;
            _mailSettings.Password = Password.Password;
            _mailSettings.Subject = Subject.Text;
            _mailSettings.Message = Message.Text;
            string saveQuery = "";
            if (_mailSettings.ID > 0)
            {
                saveQuery = $@"UPDATE [Mail_Settings] SET ToID='{_mailSettings.ToID}',CCList='{_mailSettings.CCList}',FromID='{_mailSettings.FromID}',Password='{_mailSettings.Password}',Subject='{_mailSettings.Subject}',Message='{_mailSettings.Message}' WHERE ID='{_mailSettings.ID}'";
            }
            else
            {
                saveQuery = $@"INSERT INTO [Mail_Settings] (ToID,CCList,FromID,Password,Subject,Message) VALUES ('{_mailSettings.ToID}','{_mailSettings.CCList}','{_mailSettings.FromID}','{_mailSettings.Password}','{_mailSettings.Subject}','{_mailSettings.Message}')";
            }
            if (_mailSettings.ToID.Contains("@") && _mailSettings.FromID.Contains("@"))
            {
                var res = dbCall.ExecuteQuery(saveQuery);
                if (res)
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Mail configuration saved successfully !!");
                    DialogHost.CloseDialogCommand.Execute("weighbridge", null);
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !!");
                }
            }
            DialogHost.CloseDialogCommand.Execute("weighbridge", null);
        }
        private void Previous_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute("software", null);
        }
        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveMailConfig();
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
