using IWT.DBCall;
using IWT.Shared;
using IWT.ViewModel;
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

namespace IWT.Setting_Pages
{
    /// <summary>
    /// Interaction logic for SMTP_setting.xaml
    /// </summary>
    public partial class SMTP_setting : Page
    {
        string LastMessage;
        private readonly ToastViewModel toastViewModel;
        public SMTP_setting()
        {
            toastViewModel = new ToastViewModel();
            Setting_DBCall db = new Setting_DBCall();
            InitializeComponent();
            DataTable dt1 = db.GetSMTPData("SELECT * FROM SMTP_Settings");
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                foreach (DataRow row in dt1.Rows)
                {
                    Host.Text = (row["Host"].ToString());
                    Port.Text = (row["Port"].ToString());
                    WriteLog.WriteToFile("InitializeComponent:- InitializeComponentData - Inserted Successfully ");
                }
               
                // RFIDno.ItemsSource = items;
            }
            else
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For SMTP !!");
        }
        public class SMTP_Config
        {
            public string Host { get; set; }
            public string Port { get; set; }
        }

        private void SMTPSave_Click(object sender, RoutedEventArgs e)
        {
            Setting_DBCall db = new Setting_DBCall();
            SMTP_Config data = new SMTP_Config();
            data.Host = Host.Text;
            data.Port = Port.Text;
            DataTable dt1 = db.GetSMTPData("SELECT * FROM SMTP_Settings");
            if (data.Host != "" || data.Port != "")
            {
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    db.UpdateSMTPData(data);
                    WriteLog.WriteToFile("SMTPSave_Click:- UpdateSMTPData - Updated Successfully ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Updated Successsfully !!");
                    // RFIDno.ItemsSource = items;
                }
                else
                {
                    db.InsertSMTPData(data);
                    WriteLog.WriteToFile("SMTPSave_Click:- InsertSMTPData - Inserted Successfully ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "SMTP Data Inserted Successsfully !!");
                    // RFIDno.ItemsSource = items;
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Enter SMTP Fields !!");
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
