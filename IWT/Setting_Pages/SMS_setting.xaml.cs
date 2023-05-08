using IWT.DBCall;
using IWT.Shared;
using IWT.ViewModel;
using IWT.Views;
using MaterialDesignThemes.Wpf;
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
    /// Interaction logic for SMS_setting.xaml
    /// </summary>
    public partial class SMS_setting : Page
    {
        string radiovalue;
        bool radio_value;
        string LastMessage;
        private readonly ToastViewModel toastViewModel;
        public TextBox FocusedTextBox { get; set; }
        public SMS_setting()
        {
            toastViewModel = new ToastViewModel();
            Setting_DBCall db = new Setting_DBCall();
            InitializeComponent();
            DataTable dt1 = db.GetSMSData("SELECT * FROM SMS_Template");
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                foreach (DataRow row in dt1.Rows)
                {
                    Content.Text = (row["Content"].ToString());
                    Phone1.Text = (row["PhoneNo1"].ToString());
                    Phone2.Text = (row["PhoneNo2"].ToString());
                    Phone3.Text = (row["PhoneNo3"].ToString());
                    Username.Text = (row["ProviderUserName"].ToString());
                    Password.Password = (row["Password"].ToString());
                    radiovalue = (row["UseGSM"].ToString());
                    if (radiovalue == "True")
                    {
                        GSM.IsChecked = true;
                    }
                    else
                    {
                        Message.IsChecked = true;
                    }
                }
                WriteLog.WriteToFile("InitializeComponent:- InitializeComponentData - Inserted Successfully ");
                // RFIDno.ItemsSource = items;
            }
            else
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For Both GSM And Message91");
        }
        public void RadioButtonChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                Setting_DBCall db = new Setting_DBCall();
                var radioButton = sender as RadioButton;
                if (radioButton == null)
                    return;
                radiovalue = radioButton.Content.ToString();
                if (radiovalue == "Message91")
                {
                    radio_value = false;
                }
                else
                {
                    radio_value = true;
                }
                if (radiovalue != null)
                {
                    DataTable dt1 = db.GetEmailData("SELECT * FROM SMS_Template Where UseGSM=" + "'" + radio_value + "'");
                    if (dt1 != null && dt1.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt1.Rows)
                        {
                            Content.Text = (row["Content"].ToString());
                            Phone1.Text = (row["PhoneNo1"].ToString());
                            Phone2.Text = (row["PhoneNo2"].ToString());
                            Phone3.Text = (row["PhoneNo3"].ToString());
                            Username.Text = (row["ProviderUserName"].ToString());
                            Password.Password = (row["Password"].ToString());
                            var radiovalue1 = (row["UseGSM"].ToString());
                            if (radiovalue1 == "True")
                            {
                                GSM.IsChecked = true;
                            }
                            else
                            {
                                Message.IsChecked = true;
                            }
                        }
                        WriteLog.WriteToFile("InitializeComponent:- InitializeComponentData - Inserted Successfully ");
                        // RFIDno.ItemsSource = items;
                    }
                    else
                    {
                        Content.Text = "";
                        Phone1.Text = "";
                        Phone2.Text = "";
                        Phone3.Text = "";
                        Username.Text = "";
                        Password.Password = "";
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For " + radiovalue);
                    }
                }
                // save_value(loadvalue);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SMSSave_Click(object sender, RoutedEventArgs e)
        {
            
            Setting_DBCall db = new Setting_DBCall();
            SMS_config data = new SMS_config();
            data.content = Content.Text;
            data.GSM = radio_value;
            data.phone1 = Phone1.Text;
            data.phone2 = Phone2.Text;
            data.phone3 = Phone3.Text;
            data.username = Username.Text;
            data.password = Password.Password;
            data.SMS_alert = SMS_alert.IsEnabled;
            if (data.content != "" || data.password != "" || data.phone1 != "" || data.phone2 != "" || data.phone3 != "" || data.username != "")
            {
                DataTable dt1 = db.GetEmailData("SELECT * FROM SMS_Template Where UseGSM=" + "'" + data.GSM + "'");
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    db.UpdateSMSData(data);
                    WriteLog.WriteToFile("SMSSave_Click:- UpdateSMSData - Updated Successfully ");
                    // RFIDno.ItemsSource = items;
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "SMS_Details Updated Successsfully For " + radiovalue + " !!");
                }
                else
                {
                    db.InsertSMSData(data);
                    WriteLog.WriteToFile("SMSSave_Click:- InsertSMSData - Inserted Successfully ");
                    // RFIDno.ItemsSource = items;
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "SMS_Details Inserted Successsfully For " + radiovalue + " !!");
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Enter SMS Fields For "+radiovalue+" !!");
            }
           

        }
        public class SMS_config
        {
            public bool GSM { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string content { get; set; }
            public string phone1 { get; set; }
            public string phone2 { get; set; }
            public string phone3 { get; set; }
            public bool SMS_alert { get; set; }
        }

        private async void DeleteSMS_Click(object sender, RoutedEventArgs e)
        {
            var res = await OpenConfirmationDialog();
            if (res)
            {
                Setting_DBCall db = new Setting_DBCall();
                SMS_config data = new SMS_config();
                data.content = Content.Text;
                data.GSM = radio_value;
                data.phone1 = Phone1.Text;
                data.phone2 = Phone2.Text;
                data.phone3 = Phone3.Text;
                data.username = Username.Text;
                data.password = Password.Password;
                data.SMS_alert = SMS_alert.IsEnabled;

                DataTable dt1 = db.GetEmailData("SELECT * FROM SMS_Template Where UseGSM=" + "'" + data.GSM + "'");
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    db.DeleteSMSData(data);
                    WriteLog.WriteToFile("DeleteSMS_Click:- DeleteSMSData - Deleated Successfully ");
                    ClearFields();
                    WriteLog.WriteToFile("DeleteSMS_Click:- ClearFields - All Fields Cleared ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "SMS_Details Deleted Successsfully For " + radiovalue + " !!");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Enter SMS Details For " + radiovalue + " !!");
                }
            }
        }
        public async Task<bool> OpenConfirmationDialog()
        {
            var view = new ConfirmationDialog("Delete the SMS setting");

            //    //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            return (bool)result;
        }
        //private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        //{

        //}
        public void ShowMessage(Action<string> message, string name)
        {
            this.Dispatcher.Invoke(() =>
            {
                LastMessage = $"{name}";
                message(LastMessage);
            });
        }
        public void ClearFields()
        {
            Content.Text = "";
            Phone1.Text = "";
            Phone2.Text = "";
            Phone3.Text = "";
            Username.Text = "";
            Password.Password = "";
            GSM.IsChecked = false;
            Message.IsChecked = false;
            SMS_alert.IsChecked = false;
        }

        private void Username_GotFocus(object sender, RoutedEventArgs e)
        {
            //this.VirtualKeyboard.FocusedTextBox = txtBox;
        }

        private async void SMSDesignBtn_Click(object sender, RoutedEventArgs e)
        {
            var view = new SMS_Design();

            //    //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
        }
    }
}
