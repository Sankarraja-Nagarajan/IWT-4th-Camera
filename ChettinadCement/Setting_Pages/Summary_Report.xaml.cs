using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using IWT.Views;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
    /// Interaction logic for Summary_Report.xaml
    /// </summary>
    public partial class Summary_Report : Page
    {
        string LastMessage;
        private readonly ToastViewModel toastViewModel;
        Setting_DBCall db;
        public Summary_Report()
        {
            toastViewModel = new ToastViewModel();
            db = new Setting_DBCall();
            InitializeComponent();
            GetSummaryReportData();
        }

        public void GetSummaryReportData()
        {
            DataTable dt1 = db.GetSummaryData("SELECT * FROM Company_SummaryReport_Data");
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                foreach (DataRow row in dt1.Rows)
                {
                    File_name.Text = (row["LogoPath"].ToString());
                    CompanyName.Text = (row["CompanyName"].ToString());
                    ReportTitle.Text = (row["CompanyHeaderTitle"].ToString());
                    Address1.Text = (row["CompanyPhoneAddress"].ToString());
                    Company_footer.Text = (row["CompanyFooter"].ToString());

                }
                WriteLog.WriteToFile("InitializeComponent:- InitializeComponentData - Inserted Successfully ");
                // RFIDno.ItemsSource = items;
            }
            else
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For Summary Reports !!");

        }


        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                File_name.Text = openFileDialog.FileName;
        }

        //public class Summary_report
        //{
        //    public string CompanyName { get; set; }
        //    public string Logo_path { get; set; }
        //    public string CompanyFooter { get; set; }
        //    public string Title { get; set; }
        //    public string Address { get; set; }
        //}

        private void SummarySave_Click(object sender, RoutedEventArgs e)
        {
            Setting_DBCall db = new Setting_DBCall();
            CompanySummaryReportData data = new CompanySummaryReportData();
            data.CompanyName = CompanyName.Text;
            data.CompanyHeaderTitle = ReportTitle.Text;
            data.CompanyFooter = Company_footer.Text;
            data.CompanyPhoneAddress = Address1.Text;
            data.LogoPath = File_name.Text;
            if (data.CompanyName !="" || data.CompanyFooter !="" || data.CompanyHeaderTitle !="" || data.CompanyPhoneAddress !="" 
                || data.LogoPath != "") {
                DataTable dt1 = db.GetEmailData("SELECT * FROM Company_SummaryReport_Data");
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    db.UpdateSummaryData(data);
                    WriteLog.WriteToFile("SummarySave_Click:- UpdateSummaryData - Updated Successfully ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Updated Successsfully !!");
                    // RFIDno.ItemsSource = items;
                }
                else
                {
                    db.InsertSummaryData(data);
                    WriteLog.WriteToFile("SummarySave_Click:- InsertSummaryData - Inserted Successfully ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Summary Data Inserted Successsfully !!");
                    // RFIDno.ItemsSource = items;
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Enter Summary Fields !!");
            }
        }

        private async void DeleteSummary_Click(object sender, RoutedEventArgs e)
        {
            var res = await OpenConfirmationDialog();
            if (res)
            {
                Setting_DBCall db = new Setting_DBCall();
                CompanySummaryReportData data = new CompanySummaryReportData();
                data.CompanyName = CompanyName.Text;
                data.CompanyHeaderTitle = ReportTitle.Text;
                data.CompanyFooter = Company_footer.Text;
                data.CompanyPhoneAddress = Address1.Text;
                data.LogoPath = File_name.Text;
                DataTable dt1 = db.GetEmailData("SELECT * FROM Company_SummaryReport_Data");
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    db.DeleteSummaryData(data);
                    WriteLog.WriteToFile("DeleteMail_Click:- DeleteSummaryData - Deleated Successfully ");
                    ClearFields();
                    WriteLog.WriteToFile("DeleteMail_Click:- ClearFields - All Fields Cleared ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Summary Data Deleted Successsfully !!");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Enter Summary Details !!");
                }
            }
        }

        public async Task<bool> OpenConfirmationDialog()
        {
            var view = new ConfirmationDialog("Delete report setting");

            //    //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            return (bool)result;
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {

        }
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
            CompanyName.Text = "";
            Company_footer.Text = "";
            File_name.Text = "";
            Address1.Text = "";
            ReportTitle.Text = "";
            
        }
    }
}
