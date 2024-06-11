using IWT.DBCall;
using IWT.Models;
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
    /// Interaction logic for Email.xaml
    /// </summary>
    public partial class Email : Page
    {
        string LastMessage;
        private readonly ToastViewModel toastViewModel;
        string radiovalue;
        Setting_DBCall db = null;
        public Email()
        {
            toastViewModel = new ToastViewModel();
            db = new Setting_DBCall();
            InitializeComponent();
            GetEmailData();
        }

        public void GetEmailData()
        {
            try
            {
                var send = "SMTP";
                DataTable dt1 = db.GetEmailData("SELECT * FROM Mail_Settings Where SendType=" + "'" + send + "'");
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    foreach (DataRow row in dt1.Rows)
                    {
                        To.Text = (row["ToID"].ToString());
                        CC.Text = (row["CCList"].ToString());
                        MailId.Text = (row["FromID"].ToString());
                        Password.Password = (row["Password"].ToString());
                        Subject.Text = (row["Subject"].ToString());
                        Message.Text = (row["Message"].ToString());
                        radiovalue = (row["SendType"].ToString());
                        if (radiovalue == "SMTP")
                        {
                            smtpValue.IsChecked = true;
                        }
                        else
                        {
                            sendgrid.IsChecked = true;
                        }
                    }
                    // RFIDno.ItemsSource = items;
                }
                else
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For " + send + " !!");
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Email/GetEmailData :- "+ex.Message);
            }
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
                // save_value(loadvalue);
                if (radiovalue != null)
                {
                    DataTable dt1 = db.GetEmailData("SELECT * FROM Mail_Settings WHERE SendType=" + "'" + radiovalue + "'");
                    if (dt1 != null && dt1.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt1.Rows)
                        {
                            To.Text = (row["ToID"].ToString());
                            CC.Text = (row["CCList"].ToString());
                            MailId.Text = (row["FromID"].ToString());
                            Password.Password = (row["Password"].ToString());
                            Subject.Text = (row["Subject"].ToString());
                            Message.Text = (row["Message"].ToString());
                            radiovalue = (row["SendType"].ToString());
                            if (radiovalue == "SMTP")
                            {
                                smtpValue.IsChecked = true;
                            }
                            else
                            {
                                sendgrid.IsChecked = true;
                            }
                        }
                        WriteLog.WriteToFile("InitializeComponent:- InitializeComponentData - Inserted Successfully ");
                        // RFIDno.ItemsSource = items;
                    }
                    else
                    {
                        To.Text = "";
                        CC.Text = "";
                        MailId.Text = "";
                        Subject.Text = "";
                        Message.Text = "";
                        Password.Password = "";
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For " + radiovalue);
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void EmailSave_Click(object sender, RoutedEventArgs e)
        {
            Setting_DBCall db = new Setting_DBCall();
            Mail data = new Mail();
            data.To = To.Text;
            data.Cc = CC.Text;
            data.MailID = MailId.Text;
            data.Subject = Subject.Text;
            data.Message = Message.Text;
            data.Password = Password.Password;
            data.SendType = radiovalue;
            if (data.To != "" || data.Cc != "" || data.MailID != "" || data.Subject != "" || data.Message != "" || data.Password != "" || data.SendType != null)
            {
                bool c = true;
                if (data.Cc != "")
                {
                    List<string> temp = data.Cc.Split(',').ToList();
                    foreach (var cc in temp)
                    {
                        if (!(cc.Contains("@")))
                        {
                            c = false;
                            break;
                        }
                    }
                }
                if (data.To.Contains("@") && data.MailID.Contains("@") && c)
                {
                    DataTable dt1 = db.GetEmailData("SELECT * FROM Mail_Settings Where SendType=" + "'" + data.SendType + "'");
                    if (dt1 != null && dt1.Rows.Count > 0)
                    {
                        db.UpdateEmailData(data);
                        WriteLog.WriteToFile("EmailSave_Click:- UpdateEmailData - Updated Successfully ");
                        // RFIDno.ItemsSource = items;
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Email Details Updated Successsfully For " + data.SendType + " !!");
                    }
                    else
                    {
                        db.InsertEmailData(data);
                        WriteLog.WriteToFile("EmailSave_Click:- InsertEmailData - Inserted Successfully ");
                        // RFIDno.ItemsSource = items;
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Email Details Inserted Successsfully For " + data.SendType + " !!");
                    }

                    db.DisableEmailData(data.SendType == "SMTP" ? "Send Grid" : "SMTP");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Invalid Mail !!");
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Enter EMail Fields !!");
            }


        }

       

        private async void DeleteMail_Click(object sender, RoutedEventArgs e)
        {
            var res = await OpenConfirmationDialog();
            if (res)
            {
                Setting_DBCall db = new Setting_DBCall();
                Mail data = new Mail();
                data.To = To.Text;
                data.Cc = CC.Text;
                data.MailID = MailId.Text;
                data.Subject = Subject.Text;
                data.Message = Message.Text;
                data.Password = Password.Password;
                data.SendType = radiovalue;
                DataTable dt1 = db.GetEmailData("SELECT * FROM Mail_Settings Where SendType=" + "'" + data.SendType + "'");
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    db.DeleteEmailData(data);
                    WriteLog.WriteToFile("DeleteMail_Click:- DeleteEmailData - Deleated Successfully ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Email Details Deleted Successsfully For " + data.SendType + " !!");
                    ClearFields();
                    WriteLog.WriteToFile("DeleteMail_Click:- ClearFields - All Fields Cleared ");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Enter Mail Details !!");
                }
            }
            //ShowMessage(toastViewModel.ClearMessages, "All Fields Cleared Successsfully!!");
        }

        public async Task<bool> OpenConfirmationDialog()
        {
            var view = new ConfirmationDialog("Delete the email setting");

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
            To.Text = "";
            CC.Text = "";
            MailId.Text = "";
            Subject.Text = "";
            Message.Text = "";
            Password.Password = "";
            smtpValue.IsChecked = false;
            sendgrid.IsChecked = false;
        }

        private async void EmailDesignBtn_Click(object sender, RoutedEventArgs e)
        {
            var view = new Email_Design();

            //    //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
        }
    }
}
