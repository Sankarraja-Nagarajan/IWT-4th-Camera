using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

namespace IWT.Admin_Pages
{
    /// <summary>
    /// Interaction logic for ReTriggerMailSMS.xaml
    /// </summary>
    public partial class ReTriggerMailSMS : UserControl
    {
        public static CommonFunction commonFunction = new CommonFunction();
        public ReTriggerMailSMS()
        {
            InitializeComponent();
        }

        private void SMS_Button_Click(object sender, RoutedEventArgs e)
        {
            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "SMS sending has been initialized");
            commonFunction.RetriggerFailedSMS();
        }

        private void Mail_Button_Click(object sender, RoutedEventArgs e)
        {
            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Mail sending has been initialized");
            commonFunction.RetriggerFailedMails();
        }

        private void CloseTransactionBtn_Click(object sender, RoutedEventArgs e)
        {
            ClosePendingTickets();
        }

        private void ClosePendingTickets()
        {
            try
            {
                AdminDBCall db = new AdminDBCall();
                DataTable dt1 = db.GetAllData("SELECT * FROM Other_Settings");
                string JSONString = JsonConvert.SerializeObject(dt1);
                var result = JsonConvert.DeserializeObject<List<OtherSettings>>(JSONString);
                if (result != null && result.Count > 0 && result[0] != null)
                {
                    OtherSettings otherSettings = result[0];
                    if (otherSettings.ExpiryDays > 0)
                    {
                        WriteLog.WriteToFile("ClosePendingTickets method called");
                        MasterDBCall masterDBCall = new MasterDBCall();
                        string Query = "ClosePendingTickets";
                        SqlCommand cmd = new SqlCommand(Query);
                        cmd.Parameters.AddWithValue("@ExpiryDays", otherSettings.ExpiryDays);
                        masterDBCall.InsertData(cmd, CommandType.StoredProcedure);
                        WriteLog.WriteToFile($"ClosePendingTickets : - All the expired ({otherSettings.ExpiryDays} days) pending tickets are closed");
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "All the expired pending tickets are closed");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("ClosePendingTickets", ex);
            }
        }
    }
}
