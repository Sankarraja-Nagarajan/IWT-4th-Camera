using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
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
    /// Interaction logic for FileLocationDialog.xaml
    /// </summary>
    public partial class FileLocationDialog : UserControl
    {
        AdminDBCall dbCall = new AdminDBCall();
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        private FileLocation _flSettings = new FileLocation();
        public MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        public FileLocationDialog()
        {
            InitializeComponent();
            toastViewModel = new ToastViewModel();
        }

        private void FileLocationDialog_Loaded(object sender, RoutedEventArgs e)
        {
            GetFLConfig();
        }
        public void GetFLConfig()
        {
            try
            {
                DataTable table = dbCall.GetAllData("select * from [FileLocation_Setting]");
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<FileLocation>>(JSONString);
                var flc = result.FirstOrDefault();
                if (flc != null)
                {
                    _flSettings = flc;
                    SetFLConfig();
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("MailSettingsDialog/GetSoftwareConfig/Exception:- " + ex.Message, ex);
            }
        }
        public void SetFLConfig()
        {
            logFilePath.Text = _flSettings.Log_Path;
            reportFilePath.Text = _flSettings.IReport_Path;
            reportFile.Text = _flSettings.IReport;
            reportTemp.Text = _flSettings.Report_Temp;
            transactionLog.Text = _flSettings.Transaction_Log;
            ticketTemp.Text = _flSettings.Ticket_Temp;
            defaultTicketTemplate.Text = _flSettings.Default_Ticket;
            DefaultTemplateFile.Text = _flSettings.Default_Temp;
            weighIndicator.Text = _flSettings.Weigh;
            weighIndicatorFile.Text = _flSettings.WeighIndicator;
        }
        public void SaveFLConfig()
        {
            _flSettings.Log_Path = logFilePath.Text;
            _flSettings.IReport_Path = reportFilePath.Text;
            _flSettings.IReport = reportFile.Text;
            _flSettings.Report_Temp = reportTemp.Text;
            _flSettings.Transaction_Log = transactionLog.Text;
            _flSettings.Ticket_Temp = ticketTemp.Text;
            _flSettings.Default_Ticket = defaultTicketTemplate.Text;
            _flSettings.Default_Temp = DefaultTemplateFile.Text;
            _flSettings.Weigh = weighIndicator.Text;
            _flSettings.WeighIndicator = weighIndicatorFile.Text;
            string saveQuery = "";
            if (_flSettings.ID > 0)
            {
                saveQuery = $@"UPDATE [FileLocation_Setting] SET Log_Path='{_flSettings.Log_Path}',IReport_Path='{_flSettings.IReport_Path}',IReport='{_flSettings.IReport}',Report_Temp='{_flSettings.Report_Temp}',Transaction_Log='{_flSettings.Transaction_Log}',Ticket_Temp='{_flSettings.Ticket_Temp}',Default_Ticket='{_flSettings.Default_Ticket}',Default_Temp='{_flSettings.Default_Temp}',Weigh='{_flSettings.Weigh}',WeighIndicator='{_flSettings.WeighIndicator}' WHERE ID='{_flSettings.ID}'";
            }
            else
            {
                saveQuery = $@"INSERT INTO [FileLocation_Setting] (Log_Path,IReport_Path,IReport,Report_Temp,Transaction_Log,Ticket_Temp,Default_Ticket,Default_Temp,Weigh,WeighIndicator) VALUES ('{_flSettings.Log_Path}','{_flSettings.IReport_Path}','{_flSettings.IReport}','{_flSettings.Report_Temp}','{_flSettings.Transaction_Log}','{_flSettings.Ticket_Temp}','{_flSettings.Default_Ticket}','{_flSettings.Default_Temp}','{_flSettings.Weigh}','{_flSettings.WeighIndicator}')";
            }
            var res = dbCall.ExecuteQuery(saveQuery);
            if (res)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "File location configuration saved successfully !!");
                DialogHost.CloseDialogCommand.Execute("completed", null);
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !!");
            }
        }

        private void Previous_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute("cctv", null);
        }
        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFLConfig();
        }
        public void ShowMessage(Action<string> message, string name)
        {
            this.Dispatcher.Invoke(() =>
            {
                LastMessage = $"{name}";
                message(LastMessage);
            });
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
        public string GetFileLocation()
        {
            var browseFile = new OpenFileDialog();
            if (browseFile.ShowDialog() == true)
            {
                return browseFile.FileName;
            }
            else
            {
                return "";
            }
        }

        private void LogFilePath_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            logFilePath.Text = GetFolderLocation();
        }
        private void ReportFilePath_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            reportFilePath.Text = GetFolderLocation();
        }
        private void ReportFile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            reportFile.Text = GetFileLocation();
        }
        private void ReportTemp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            reportTemp.Text = GetFileLocation();
        }
        private void TransactionLog_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            transactionLog.Text = GetFolderLocation();
        }
        private void TicketTemplate_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ticketTemp.Text = GetFileLocation();
        }
        private void DefaultTicketTemplateFolder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            defaultTicketTemplate.Text = GetFolderLocation();
        }
        private void DefaultTemplateFile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DefaultTemplateFile.Text = GetFileLocation();
        }
        private void WeighIndicator_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            weighIndicator.Text = GetFolderLocation();
        }
        private void WeighIndicatorFile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            weighIndicatorFile.Text = GetFileLocation();
        }
    }
}
