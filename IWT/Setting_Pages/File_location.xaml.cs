using IWT.DBCall;
using IWT.Shared;
using IWT.ViewModel;
using IWT.Views;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
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
    /// 
    /// Interaction logic for File_location.xaml
    /// </summary>
    public partial class File_location : Page
    {
        string LastMessage;
        private readonly ToastViewModel toastViewModel;
        public File_location()
        {
            toastViewModel = new ToastViewModel();
            Setting_DBCall db = new Setting_DBCall();
            InitializeComponent();
            DataTable dt1 = db.GetFileData("SELECT * FROM FileLocation_Setting");
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                foreach (DataRow row in dt1.Rows)
                {
                    Log.Text = (row["Log_Path"].ToString());
                    Ireport.Text = (row["IReport_Path"].ToString());
                    IReport_file.Text = (row["IReport"].ToString());
                    report.Text = (row["Report_Temp"].ToString());
                    Transaction.Text = (row["Transaction_Log"].ToString());
                    ticket.Text = (row["Ticket_Temp"].ToString());
                    default_ticket.Text = (row["Default_Ticket"].ToString());
                    default_temp.Text = (row["Default_Temp"].ToString());
                    Weigh.Text = (row["Weigh"].ToString());
                    Weight_indicator.Text = (row["WeighIndicator"].ToString());

                }
                WriteLog.WriteToFile("InitializeComponent:- InitializeComponentData - Inserted Successfully ");
                // RFIDno.ItemsSource = items;
            }
            else
            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For File Location!!");
        }
        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFolderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var result = openFolderDialog.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                Log.Text = openFolderDialog.SelectedPath;
            }
        }

        private void btnOpenFile1_Click(object sender, RoutedEventArgs e)
        {
            var openFolderDialog1 = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var result = openFolderDialog1.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                Ireport.Text = openFolderDialog1.SelectedPath;
            }
            
        }

        private void btnOpenFile2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                IReport_file.Text = openFileDialog.FileName;
        }

        private void btnOpenFile3_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                report.Text = openFileDialog.FileName;
        }

        private void btnOpenFile4_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                Transaction.Text = openFileDialog.FileName;
        }

        private void btnOpenFile5_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                ticket.Text = openFileDialog.FileName;
        }

        private void btnOpenFile6_Click(object sender, RoutedEventArgs e)
        {
            var openFolderDialog2 = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var result = openFolderDialog2.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                default_ticket.Text = openFolderDialog2.SelectedPath;
            }
        }

        private void btnOpenFile7_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                default_temp.Text = openFileDialog.FileName;
        }

        private void btnOpenFile8_Click(object sender, RoutedEventArgs e)
        {
            var openFolderDialog3 = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var result = openFolderDialog3.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                Weigh.Text = openFolderDialog3.SelectedPath;
            }
            
        }

        private void btnOpenFile9_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                Weight_indicator.Text = openFileDialog.FileName;
        }

        private void FileSave_Click(object sender, RoutedEventArgs e)
        {
            Setting_DBCall db = new Setting_DBCall();
            File_config data = new File_config();
            data.Log = Log.Text;
            data.IReport_Path = Ireport.Text;
            data.IReport = IReport_file.Text;
            data.Report = report.Text;
            data.Trans_Log = Transaction.Text;
            data.Ticket_temp = ticket.Text;
            data.Default_ticket = default_ticket.Text;
            data.Default_temp = default_temp.Text;
            data.Weigh = Weigh.Text;
            data.WeighIndicator = Weight_indicator.Text;
            if (data.Log !="" || data.IReport_Path != "" || data.IReport !="" || data.Report !="" || data.Trans_Log !="" || data.Ticket_temp !="" || data.Default_ticket !="" || data.Default_temp !="" || data.Weigh !="" || data.WeighIndicator !="")
            {
                DataTable dt1 = db.GetFileData("SELECT * FROM FileLocation_Setting");
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    db.UpdateFileData(data);
                    WriteLog.WriteToFile("FileSave_Click:- UpdateFileData - Updated Successfully ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "File Location Data Updated Successsfully!!");
                    // RFIDno.ItemsSource = items;
                }
                else
                {
                    db.InsertFileData(data);
                    WriteLog.WriteToFile("FileSave_Click:- InsertFileData - Inserted Successfully ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "File Location Data Inserted Successsfully!!");
                    // RFIDno.ItemsSource = items;
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Enter File Location Fields !!");
            }
            
        }

        private async void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            var res = await OpenConfirmationDialog();
            if (res)
            {
                Setting_DBCall db = new Setting_DBCall();
                File_config data = new File_config();
                data.Log = Log.Text;
                data.IReport_Path = IReport_file.Text;
                data.IReport = Ireport.Text;
                data.Report = report.Text;
                data.Trans_Log = Transaction.Text;
                data.Ticket_temp = ticket.Text;
                data.Default_ticket = default_ticket.Text;
                data.Default_temp = default_temp.Text;
                data.Weigh = Weigh.Text;
                data.WeighIndicator = Weight_indicator.Text;
                DataTable dt1 = db.GetFileData("SELECT * FROM FileLocation_Setting");
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    db.DeleteFileData(data);
                    WriteLog.WriteToFile("DeleteMail_Click:- DeleteEmailData - Deleated Successfully ");
                    ClearFields();
                    WriteLog.WriteToFile("DeleteMail_Click:- ClearFields - All Fields Cleared ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "File Location Data Deleted Successsfully!!");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Enter FIle Location Details !!");
                }
            }
        }

        public async Task<bool> OpenConfirmationDialog()
        {
            var view = new ConfirmationDialog("Delete file location details");

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
            Log.Text = "";
            Ireport.Text = "";
            IReport_file.Text = "";
            report.Text = "";
            Transaction.Text = "";
            ticket.Text = "";
            default_ticket.Text ="";
            default_temp.Text = "";
            Weigh.Text = "";
            Weight_indicator.Text = "";

        }
        public class File_config
        {
            public string Log { get; set; }
            public string IReport_Path { get; set; }
            public string IReport { get; set; }
            public string Report { get; set; }
            public string Trans_Log { get; set; }
            public string Ticket_temp { get; set; }
            public string Default_ticket { get; set; }
            public string Default_temp { get; set; }
            public string Weigh { get; set; }
            public string WeighIndicator { get; set; }
        }
    }
}
