using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using Microsoft.Reporting.WinForms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using IWT.TransactionPages;
using System.IO;
using IWT.Views;

namespace IWT.AWS.Views
{
    /// <summary>
    /// Interaction logic for PrintAndDeleteTicketControl.xaml
    /// </summary>
    public partial class PrintAndDeleteTicketControl : UserControl, INotifyPropertyChanged
    {
        public static MasterDBCall masterDBCall = new MasterDBCall();
        public static AdminDBCall adminDBCall = new AdminDBCall();

        public static CommonFunction commonFunction = new CommonFunction();
        List<OperationDetail> operationDetails = new List<OperationDetail>();
        public List<Company_Details> company_Details = new List<Company_Details>();

        //printdetails list = new printdetails();
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        Transaction SelectedTransaction = new Transaction();
        public string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public string TransactionPath;
        string Rolename;
        List<Transaction> transactions = new List<Transaction>();
        //List<Transaction> selectedTransactions = new List<Transaction>();
        List<ImageSourcePath> CurrentTransactionImageSourcePath = new List<ImageSourcePath>();

        List<CCTVSettings> cCTVSettings = new List<CCTVSettings>();
        RolePriviliege rolePriviliege;

        private string _reportTemplate;
        private int _noOfCopies;
        private string _defaultTemplateFolder;
        private OtherSettings _otherSettings = new OtherSettings();
        private FileLocation _fileLocation = new FileLocation();

        public int TotalRecords = 0;
        List<int> ItemPerPagesList = new List<int> { 5, 10, 25, 50, 100, 250, 500 };
        public event PropertyChangedEventHandler PropertyChanged;
        private int _CurrentPage;
        private int _NumberOfPages;
        private int _SelectedRecord;
        private bool _IsFirstEnable;
        private bool _IsPreviousEnable;
        private bool _IsNextEnable;
        private bool _IsLastEnable;
        int RecordStartFrom = 0;

        //List<dynamic> Result = new List<dynamic>();
        List<Transaction> Result1 = new List<Transaction>();


        public int CurrentPage
        {
            get { return _CurrentPage; }
            set
            {
                _CurrentPage = value;
                OnPropertyChanged();
                UpdateEnableStatus();
            }
        }

        public int NumberOfPages
        {
            get { return _NumberOfPages; }
            set
            {
                _NumberOfPages = value;
                OnPropertyChanged();
                UpdateEnableStatus();
            }
        }
        public int SelectedRecord
        {
            get { return _SelectedRecord; }
            set
            {
                _SelectedRecord = value;
                OnPropertyChanged();
                UpdateRecordCount();
            }
        }


        public bool IsFirstEnable
        {
            get { return _IsFirstEnable; }
            set
            {
                _IsFirstEnable = value;
                OnPropertyChanged();
            }
        }

        public bool IsPreviousEnable
        {
            get { return _IsPreviousEnable; }
            set
            {
                _IsPreviousEnable = value;
                OnPropertyChanged();
            }
        }

        public bool IsNextEnable
        {
            get { return _IsNextEnable; }
            set
            {
                _IsNextEnable = value;
                OnPropertyChanged();
            }
        }
        public bool IsLastEnable
        {
            get { return _IsLastEnable; }
            set
            {
                _IsLastEnable = value;
                OnPropertyChanged();
            }
        }
        public PrintAndDeleteTicketControl(string _Rolename, RolePriviliege _rolePriviliege)
        {
            toastViewModel = new ToastViewModel();
            SelectedTransaction = new Transaction();
            TransactionPath = System.IO.Path.Combine(BaseDirectory, "TransactionPages");
            //view.Show();
            InitializeComponent();
            this.rolePriviliege = _rolePriviliege;
            Rolename = _Rolename;
            DataContext = this;
            SelectedRecord = 5;
            NumberOfPages = 1;
            //GetCompanyDetails();
            PaginatorComboBox.ItemsSource = ItemPerPagesList;
            Loaded += PrintDeleteTicket_Loaded;
            Unloaded += PrintDeleteTicket_Unloaded;
            //MaterialGrid5.ItemsSource = LoadCollectionData1();
        }

        private void PrintDeleteTicket_Unloaded(object sender, RoutedEventArgs e)
        {
            FieldComboBox.SelectedIndex = -1;
            OperationComboBox.SelectedIndex = -1;
            SelectedFieldValue.Text = "";
            FromFieldValue.Text = "";
            ToFieldValue.Text = "";
        }

        private void PrintDeleteTicket_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                GetCompanyDetails();
                GetTransactions();
                GetCCTVSettings();
                GetOtherSettings();
                GetFileLocation();
            });

            GetTableColumnDetails("Transaction");
            SetOperations();
        }


        public void GetOtherSettings()
        {
            DataTable table = adminDBCall.GetAllData("SELECT * FROM Other_Settings");
            string JSONString = JsonConvert.SerializeObject(table);
            var result = JsonConvert.DeserializeObject<List<OtherSettings>>(JSONString);
            _otherSettings = result.FirstOrDefault();
            ApplyOtherSettings();
        }
        public void GetFileLocation()
        {
            DataTable table = adminDBCall.GetAllData("select * from [FileLocation_Setting]");
            string JSONString = JsonConvert.SerializeObject(table);
            var result = JsonConvert.DeserializeObject<List<FileLocation>>(JSONString);
            var flc = result.FirstOrDefault();
            _fileLocation = flc;
        }
        public void ApplyOtherSettings()
        {
            if (_otherSettings != null)
            {
                _noOfCopies = _otherSettings != null ? _otherSettings.NoOfCopies : 1;
                _reportTemplate = _fileLocation.Default_Temp;
                _defaultTemplateFolder = _fileLocation.Default_Ticket;
            }
        }

        public async Task OpenCopiesDialog()
        {
            var view = new NoOfCopiesDialog(_otherSettings != null ? _otherSettings.NoOfCopies : 1);
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            if (result != null)
            {
                _noOfCopies = int.Parse(result as string);
            }
        }
        public async Task<bool> OpenTemplateDialog()
        {
            var view = new TemplateSelectionDialog(_defaultTemplateFolder);
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            if (result != null)
            {
                _reportTemplate = ((FileInfo)result).FullName;
                return true;
            }
            return false;
        }

        public void GetCCTVSettings()
        {
            try
            {
                cCTVSettings = commonFunction.GetCCTVSettings(MainWindow.systemConfig.HardwareProfile);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("PrintDeleteTicket/GetCCTVSettings/Exception:- " + ex.Message, ex);
            }
        }
        private void OperationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedOperation = OperationComboBox.SelectedValue?.ToString();
            var selectedFieldValue = FieldComboBox.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(selectedOperation) && selectedOperation == "BETWEEN")
            {
                if (selectedFieldValue == "TicketNo")
                {
                    FromFieldValue.Visibility = Visibility.Visible;
                    ToFieldValue.Visibility = Visibility.Visible;
                    SelectedFieldValue.Visibility = Visibility.Collapsed;
                    SelectedFieldValue.Text = "";
                    FromDateValue.Visibility = Visibility.Collapsed;
                    ToDateValue.Visibility = Visibility.Collapsed;
                    SelectedDateValue.Visibility = Visibility.Collapsed;
                    SelectedDateValue.Text = "";
                }
                else
                {
                    FromFieldValue.Visibility = Visibility.Collapsed;
                    ToFieldValue.Visibility = Visibility.Collapsed;
                    SelectedFieldValue.Visibility = Visibility.Collapsed;
                    SelectedFieldValue.Text = "";
                    FromDateValue.Visibility = Visibility.Visible;
                    ToDateValue.Visibility = Visibility.Visible;
                    SelectedDateValue.Visibility = Visibility.Collapsed;
                    SelectedDateValue.Text = "";
                }

            }
            else
            {
                if (!string.IsNullOrEmpty(selectedFieldValue) && selectedFieldValue.ToLower().Contains("date"))
                {
                    SelectedDateValue.Visibility = Visibility.Visible;
                    SelectedFieldValue.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SelectedDateValue.Visibility = Visibility.Collapsed;
                    SelectedFieldValue.Visibility = Visibility.Visible;
                }
                FromFieldValue.Visibility = Visibility.Collapsed;
                ToFieldValue.Visibility = Visibility.Collapsed;
                //SelectedFieldValue.Text = "";
                FromDateValue.Visibility = Visibility.Collapsed;
                ToDateValue.Visibility = Visibility.Collapsed;
                //SelectedDateValue.Text = "";
            }
        }

        private void GetCompanyDetails()
        {
            company_Details = commonFunction.GetCompanyDetails();
        }

        public void GetAllOperations()
        {
            GetCommonOperations();
            //operationDetails = new List<OperationDetail>();
            OperationDetail operation1 = new OperationDetail() { Name = "greater than", Value = ">" };
            operationDetails.Add(operation1);
            OperationDetail operation2 = new OperationDetail() { Name = "greater than or equal", Value = ">=" };
            operationDetails.Add(operation2);
            OperationDetail operation3 = new OperationDetail() { Name = "less than", Value = "<" };
            operationDetails.Add(operation3);
            OperationDetail operation4 = new OperationDetail() { Name = "less than or equal", Value = "<=" };
            operationDetails.Add(operation4);
            OperationDetail operation5 = new OperationDetail() { Name = "between", Value = "BETWEEN" };
            operationDetails.Add(operation5);
        }
        public void GetCommonOperations()
        {
            operationDetails = new List<OperationDetail>();
            OperationDetail operation1 = new OperationDetail() { Name = "equal", Value = "=" };
            operationDetails.Add(operation1);
            OperationDetail operation2 = new OperationDetail() { Name = "not equal", Value = "!=" };
            operationDetails.Add(operation2);
            OperationDetail operation3 = new OperationDetail() { Name = "like", Value = "LIKE" };
            operationDetails.Add(operation3);
            OperationDetail operation4 = new OperationDetail() { Name = "not like", Value = "NOT LIKE" };
            operationDetails.Add(operation4);
        }

        public void SetOperations(OperationTypes operationType = OperationTypes.Common)
        {
            if (operationType == OperationTypes.Common)
            {
                GetCommonOperations();

            }
            else if (operationType == OperationTypes.All)
            {
                GetAllOperations();
            }
            OperationComboBox.ItemsSource = operationDetails;
            OperationComboBox.Items.Refresh();
        }

        private void GetTransactions()
        {
            AdminDBCall db = new AdminDBCall();

            DataTable dt1 = db.GetAllData("SELECT top 1000 * FROM [Transaction] WHERE IsDeleted=0 order by TicketNo desc");
            string JSONString = JsonConvert.SerializeObject(dt1);
            transactions = JsonConvert.DeserializeObject<List<Transaction>>(JSONString);
            transactions = transactions.OrderByDescending(x => x.TicketNo).ToList();
            TotalRecords = transactions.Count;
            SelectedRecord = 10;
            UpdateRecordCount();
            UpdateCollection(transactions.Take(SelectedRecord));
            SetDynamicTable();
            CheckAndEnableButtons();

        }
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                System.Windows.Controls.DataGrid dg = (System.Windows.Controls.DataGrid)sender;
                SelectedTransaction = dg.SelectedItem as Transaction;
            }
            catch (Exception)
            {
            }
        }

        private async void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedTransaction = transactions.FirstOrDefault(x => x.IsSelected);
            if (SelectedTransaction != null && SelectedTransaction.TicketNo != 0)
            {
                bool isTemplateSelected = true;
                if (_otherSettings == null || (_otherSettings != null && !_otherSettings.AutoPrintPreview))
                {
                    isTemplateSelected = await OpenTemplateDialog();
                }
                GetData();
                if (isTemplateSelected && !string.IsNullOrEmpty(_reportTemplate) && File.Exists(_reportTemplate))
                {
                    if (_otherSettings != null && _otherSettings.AutoPrint)
                    {
                        popup.IsOpen = false;
                        if (_otherSettings != null && _otherSettings.AutoCopies)
                        {
                            await OpenCopiesDialog();
                        }
                        ReportViewerDemo1.LocalReport.PrintToPrinter(_noOfCopies);
                    }
                    else
                    {
                        popup.IsOpen = true;
                    }
                }


            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a transaction to print");
            }
        }

        public void openDialog()
        {
            if (!popup.IsOpen)
            {
                popup.IsOpen = true;
                GetData();
            }// Open it if it's not open
            else popup.IsOpen = false;
        }

        private void GetData()
        {
            AdminDBCall db = new AdminDBCall();
            DataTable dt = db.GetAllData("select * from [Transaction] where TicketNo = " + SelectedTransaction.TicketNo);
            DataTable dt1 = new DataTable();
            if (SelectedTransaction.TransactionType == "SecondMulti")
            {
                dt1 = db.GetAllData("select * from [Transaction_Details] where TicketNo = " + SelectedTransaction.TicketNo);
            }
            GetCapturedImages();


            var Path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Duplicate.gif");
            var bytes1 = File.Exists(Path) ? File.ReadAllBytes(Path) : null;

            if (CurrentTransactionImageSourcePath.Count > 0)
            {
                CurrentTransactionImageSourcePath[0].WaterMarkImagePath = bytes1;
            }
            else
            {
                ImageSourcePath imageSourcePath = new ImageSourcePath();
                imageSourcePath.WaterMarkImagePath = bytes1;
                CurrentTransactionImageSourcePath.Add(imageSourcePath);
            }

            ReportDataSource rds = new ReportDataSource("DataSet1", dt);
            ReportDataSource rds1 = new ReportDataSource("DataSet2", company_Details);
            ReportDataSource rds3 = new ReportDataSource("DataSet3", CurrentTransactionImageSourcePath);

            ReportViewerDemo1.LocalReport.DataSources.Clear();
            ReportViewerDemo1.LocalReport.DataSources.Add(rds);
            ReportViewerDemo1.LocalReport.DataSources.Add(rds1);
            ReportViewerDemo1.LocalReport.DataSources.Add(rds3);
            if (SelectedTransaction.TransactionType == "SecondMulti")
            {
                ReportDataSource rds10 = new ReportDataSource("DataSet10", dt1);
                ReportViewerDemo1.LocalReport.DataSources.Add(rds10);
            }
            else
            {
            }
            ReportViewerDemo1.LocalReport.ReportPath = _reportTemplate;
            ReportViewerDemo1.ShowExportButton = false;
            ReportViewerDemo1.ShowFindControls = false;
            ReportViewerDemo1.ShowStopButton = false;
            ReportViewerDemo1.RefreshReport();
        }

        private void GetCapturedImages()
        {
            try
            {
                CurrentTransactionImageSourcePath = new List<ImageSourcePath>();
                ImageSourcePath imageSourcePath = new ImageSourcePath();
                var i = 1;
                foreach (var ccTV in cCTVSettings)
                {
                    var folder = ccTV.LogFolder;
                    FileInfo fi;
                    List<FileInfo> fis;
                    DirectoryInfo di = new DirectoryInfo(ccTV.LogFolder);

                    fis = di.GetFiles("*.jpeg").Where(file => file.Name.Contains($"{SelectedTransaction.TicketNo}_") &&
                    file.Name.Contains($"cam{ccTV.RecordID}_")).OrderByDescending(p => p.CreationTime).Take(2).ToList();
                    int j = 1;
                    foreach (var f in fis)
                    {
                        fi = f;
                        if (fi != null)
                        {
                            var bytes = File.ReadAllBytes(fi.FullName);
                            if (i == 1)
                            {
                                if (j == 1)
                                {
                                    imageSourcePath.Image1Path = bytes;
                                }
                                if (j == 2)
                                {
                                    imageSourcePath.Image4Path = imageSourcePath.Image1Path;
                                    imageSourcePath.Image1Path = bytes;
                                }
                            }
                            if (i == 2)
                            {
                                if (j == 1)
                                {
                                    imageSourcePath.Image2Path = bytes;
                                }
                                if (j == 2)
                                {
                                    imageSourcePath.Image5Path = imageSourcePath.Image2Path;
                                    imageSourcePath.Image2Path = bytes;
                                }
                            }
                            if (i == 3)
                            {
                                if (j == 1)
                                {
                                    imageSourcePath.Image3Path = bytes;
                                }
                                if (j == 2)
                                {
                                    imageSourcePath.Image6Path = imageSourcePath.Image3Path;
                                    imageSourcePath.Image3Path = bytes;
                                }
                            }
                            //if (i == 4)
                            //{
                            //    imageSourcePath.Image4Path = bytes;
                            //}
                            //if (i == 5)
                            //{
                            //    imageSourcePath.Image5Path = bytes;
                            //}
                            //if (i == 6)
                            //{
                            //    imageSourcePath.Image6Path = bytes;
                            //}
                        }
                        j++;
                    }

                    i++;
                }

                CurrentTransactionImageSourcePath.Add(imageSourcePath);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("PrintDeleteTicket/GetCapturedImages/Exception :- " + ex.Message);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
        }
        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            //popup.IsOpen = false;
            //ReportViewerDemo1.PrintDialog();
            ReportViewerDemo1.LocalReport.PrintToPrinter(1);
        }

        private async void SMSBtn_Click(object sender, RoutedEventArgs e)
        {
            var message = commonFunction.BuildSMS(SelectedTransaction);
            await commonFunction.CheckAndSendSMS(message);
        }

        //public string BuildSMS()
        //{
        //    try
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        sb.AppendFormat("Tkno : {0}\n", CurrentTransaction.TicketNo);
        //        sb.AppendFormat("Vehno : {0}\n", CurrentTransaction.VehicleNo);
        //        sb.AppendFormat("Mat : {0}\n", CurrentTransaction.MaterialName);
        //        sb.AppendFormat("Sup : {0}\n", CurrentTransaction.SupplierName);
        //        sb.AppendFormat("Emt wt : {0}\n", CurrentTransaction.EmptyWeight);
        //        sb.AppendFormat("Load wt : {0}\n", CurrentTransaction.LoadWeight);
        //        sb.AppendFormat("Net wt : {0}\n", CurrentTransaction.NetWeight);
        //        sb.AppendFormat("Regards Essae");
        //        return sb.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}

        private async void EmailBtn_Click(object sender, RoutedEventArgs e)
        {
            var message = commonFunction.BuildEmail(SelectedTransaction);

            Warning[] warnings;
            string[] streamids;
            string mimeType;
            string encoding;
            string filenameExtension;


            byte[] bytes = ReportViewerDemo1.LocalReport.Render(
                "PDF", null, out mimeType, out encoding, out filenameExtension,
                out streamids, out warnings);

            //using (FileStream fs = new FileStream("output.pdf", FileMode.Create))
            //{
            //    fs.Write(bytes, 0, bytes.Length);
            //}

            string fileName = $"report{DateTime.Now.ToString("ddMMyyyyHHmmss")}.pdf";
            //string fileName = $"report{DateTime.Now.ToString("ddMMyyyyHHmmss")}.pdf";

            await commonFunction.CheckAndSendEmail(SelectedTransaction, message, bytes, fileName);
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTransactions = transactions.Where(x => x.IsSelected).ToList();
            if (selectedTransactions != null && selectedTransactions.Count > 0)
            {
                var res = await OpenConfirmationDialog();
                if (res)
                {
                    new AdminDBCall().DeletetticketData(selectedTransactions);
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Transaction details deleted successfully");
                    SelectedTransaction = new Transaction();
                    GetTransactions();
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select atleast a transaction to delete");
            }
        }

        public async Task<bool> OpenConfirmationDialog()
        {
            var view = new ConfirmationDialog("Delete the ticket");

            //    //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            return (bool)result;
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {

        }
        public void showSnackbar(string message)
        {
            snackbar.MessageQueue?.Enqueue(
                message,
                null,
                null,
                null,
                false,
                true,
                TimeSpan.FromSeconds(3));
        }

        public List<TableColumnDetails> GetTableColumnDetails(string TableName)
        {
            try
            {
                List<TableColumnDetails> tableColumnDetails = commonFunction.GetTableColumnDetails(TableName);
                TableColumnDetails tableColumnDetails1 = new TableColumnDetails();
                tableColumnDetails1.ColumnName = "";
                tableColumnDetails.Add(tableColumnDetails1);
                FieldComboBox.ItemsSource = tableColumnDetails;
                FieldComboBox.Items.Refresh();
                FieldComboBox.SelectionChanged += FieldComboBox_SelectionChanged;
                return tableColumnDetails;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetTableColumnDetails/Exception:- " + ex.Message, ex);
                return null;
            }
        }

        private void FieldComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedValue = FieldComboBox.SelectedItem as TableColumnDetails;
            if (selectedValue == null || string.IsNullOrEmpty(selectedValue.ColumnName))
            {
                OperationComboBox.SelectedIndex = -1;
                OperationComboBox.IsEnabled = false;
                //SelectedFieldValue.Text = "";
                //SelectedFieldValue.IsEnabled = false;
            }
            else
            {
                OperationComboBox.IsEnabled = true;

                var columnName = selectedValue.ColumnName;
                if (selectedValue != null && (columnName == "TicketNo" || columnName.ToLower().Contains("date")))
                {
                    if (columnName == "TicketNo")
                    {
                        SelectedFieldValue.Visibility = Visibility.Visible;
                        SelectedDateValue.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        SelectedFieldValue.Visibility = Visibility.Collapsed;
                        SelectedDateValue.Visibility = Visibility.Visible;
                    }
                    SetOperations(OperationTypes.All);
                }
                else
                {
                    SetOperations();
                }
            }

            //throw new NotImplementedException();
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedValue = FieldComboBox.SelectedItem as TableColumnDetails;
            if (selectedValue == null || string.IsNullOrEmpty(selectedValue.ColumnName))
            {
                GetTransactions();
            }
            else
            {
                if (selectedValue != null)
                {
                    var selectedOperation = OperationComboBox.SelectedValue?.ToString();
                    var SelectedFieldVal = SelectedFieldValue.Text;
                    var SelectedDateVal = SelectedDateValue.SelectedDate;
                    var FromFieldVal = FromFieldValue.Text;
                    var ToFieldVal = ToFieldValue.Text;
                    var FromDateVal = FromDateValue.SelectedDate;
                    var ToDateVal = ToDateValue.SelectedDate;
                    if (string.IsNullOrEmpty(selectedOperation))
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the operation details");
                    }
                    else
                    {
                        if (selectedOperation != "BETWEEN")
                        {
                            if (selectedValue.ColumnName.ToLower().Contains("date"))
                            {
                                if (!SelectedDateVal.HasValue)
                                {
                                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the date");
                                }
                                else
                                {
                                    BuildAndExecuteQuery(selectedValue.ColumnName, selectedOperation, SelectedFieldVal, FromFieldVal, ToFieldVal, SelectedDateVal, FromDateVal, ToDateVal);
                                }

                            }
                            else if (string.IsNullOrEmpty(SelectedFieldVal))
                            {
                                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the value");
                            }
                            else
                            {
                                BuildAndExecuteQuery(selectedValue.ColumnName, selectedOperation, SelectedFieldVal, FromFieldVal, ToFieldVal, SelectedDateVal, FromDateVal, ToDateVal);
                            }

                        }
                        else if (selectedOperation == "BETWEEN")
                        {
                            if (selectedValue.ColumnName.ToLower().Contains("date"))
                            {
                                if (!FromDateVal.HasValue || !ToDateVal.HasValue)
                                {
                                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill from and to date details");
                                }
                                else
                                {
                                    BuildAndExecuteQuery(selectedValue.ColumnName, selectedOperation, SelectedFieldVal, FromFieldVal, ToFieldVal, SelectedDateVal, FromDateVal, ToDateVal);
                                }

                            }
                            else if (string.IsNullOrEmpty(FromFieldVal) || string.IsNullOrEmpty(ToFieldVal))
                            {
                                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill from and to value details");
                            }
                            else
                            {
                                BuildAndExecuteQuery(selectedValue.ColumnName, selectedOperation, SelectedFieldVal, FromFieldVal, ToFieldVal, SelectedDateVal, FromDateVal, ToDateVal);
                            }
                        }
                        else
                        {
                            BuildAndExecuteQuery(selectedValue.ColumnName, selectedOperation, SelectedFieldVal, FromFieldVal, ToFieldVal, SelectedDateVal, FromDateVal, ToDateVal);
                        }
                    }


                }
            }
        }

        public void BuildAndExecuteQuery(string SelectedField, string selectedOperation, string SelectedFieldVal, string FromFieldVal, string ToFieldVal, DateTime? SelectedDateVal, DateTime? FromDateVal, DateTime? ToDateVal)
        {
            string Query = "SELECT * FROM [Transaction] WHERE IsDeleted=0 AND ";

            if (SelectedField.ToLower().Contains("date"))
            {
                SelectedFieldVal = SelectedDateVal.HasValue ? SelectedDateVal.Value.Date.ToString("MM-dd-yyyy HH:mm:ss") : string.Empty;
                FromFieldVal = FromDateVal.HasValue ? FromDateVal.Value.Date.ToString("MM-dd-yyyy HH:mm:ss") : string.Empty;
                ToFieldVal = ToDateVal.HasValue ? ToDateVal.Value.Date.ToString("MM-dd-yyyy HH:mm:ss") : string.Empty;

                Query += $"CAST({SelectedField} as date) ";
            }
            else
            {
                Query += $"{SelectedField} ";
            }
            //if (selectedOperation.Contains("LIKE"))
            //{
            //    Query += $"{selectedOperation} " + "'%" + "@SelectedFieldVal" + "%'";
            //}
            //else
            //{
            //    Query += $"{selectedOperation} @SelectedFieldVal";
            //}

            SqlCommand cmd = new SqlCommand();

            if (selectedOperation.Contains("LIKE"))
            {
                Query += $"{selectedOperation} @SelectedFieldVal";
                cmd.CommandText = Query;
                cmd.Parameters.AddWithValue("@SelectedFieldVal", $"%" + SelectedFieldVal + $"%");
            }
            else if (selectedOperation == "BETWEEN")
            {
                //if (SelectedField.ToLower().Contains("date"))
                //{

                //}
                //else
                //{
                //    Query += $"{selectedOperation} @FromFieldVal AND @ToFieldVal";
                //}
                Query += $"{selectedOperation} @FromFieldVal AND @ToFieldVal";
                cmd.CommandText = Query;
                cmd.Parameters.AddWithValue("@FromFieldVal", FromFieldVal);
                cmd.Parameters.AddWithValue("@ToFieldVal", ToFieldVal);
            }
            else
            {
                Query += $"{selectedOperation} @SelectedFieldVal";
                cmd.CommandText = Query;
                cmd.Parameters.AddWithValue("@SelectedFieldVal", SelectedFieldVal);
            }

            DataTable table = masterDBCall.GetData(cmd, CommandType.Text);
            string JSONString = JsonConvert.SerializeObject(table);
            transactions = JsonConvert.DeserializeObject<List<Transaction>>(JSONString);
            transactions = transactions.OrderByDescending(x => x.TicketNo).ToList();
            TotalRecords = transactions.Count;
            SelectedRecord = 10;
            UpdateRecordCount();
            UpdateCollection(transactions.Take(SelectedRecord));
            SetDynamicTable();
        }

        public void SetDynamicTable()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    MaterialGrid5.ItemsSource = Result1;
                    MaterialGrid5.Items.Refresh();
                });
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetDynamicTable:" + ex.Message);
            }
        }

        public void ShowMessage(Action<string> message, string name)
        {
            this.Dispatcher.Invoke(() =>
            {
                LastMessage = name;
                message(LastMessage);
            });
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearControls();
        }
        public void ClearControls()
        {
            FieldComboBox.SelectedIndex = -1;
            OperationComboBox.SelectedIndex = -1;
            SelectedFieldValue.Text = "";
            SelectedDateValue.Text = "";
            FromFieldValue.Text = "";
            ToFieldValue.Text = "";
            FromDateValue.Text = "";
            ToDateValue.Text = "";
        }


        private void HeaderCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as System.Windows.Controls.CheckBox;

            if (HeaderCheckbox.IsChecked.HasValue && HeaderCheckbox.IsChecked.Value)
            {
                Result1.ForEach(x => x.IsSelected = true);
                //MaterialGrid5.ItemsSource = transactions;
                //MaterialGrid5.Items.Refresh();
                //UpdateCollection(transactions.Take(SelectedRecord));
                SetDynamicTable();
            }
            else
            {
                Result1.ForEach(x => x.IsSelected = false);
                //MaterialGrid5.ItemsSource = transactions;
                //MaterialGrid5.Items.Refresh();
                //UpdateCollection(transactions.Take(SelectedRecord));
                SetDynamicTable();
            }
            CheckAndEnableButtons();
        }

        private void ColumnCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //var checkBox = sender as System.Windows.Controls.CheckBox;
            //if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
            //{
            //    HeaderCheckbox.IsChecked = transactions.TrueForAll(x => x.IsSelected);
            //}
            //else
            //{
            //    HeaderCheckbox.IsChecked = false;
            //}

            CheckAndEnableButtons();
        }

        private void CheckAndEnableButtons()
        {
            var selectedTransactions = transactions.Where(x => x.IsSelected).ToList();
            Dispatcher.Invoke(() =>
            {
                if (selectedTransactions.Count > 0)
                {
                    DeleteButton.IsEnabled = rolePriviliege.PrintAndDeleteAccess.HasValue && rolePriviliege.PrintAndDeleteAccess.Value;
                    PrintButton.IsEnabled = rolePriviliege.PrintAndDeleteAccess.HasValue && rolePriviliege.PrintAndDeleteAccess.Value && selectedTransactions.Count == 1;
                }
                else
                {
                    DeleteButton.IsEnabled = false;
                    PrintButton.IsEnabled = false;
                }
            });

        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void UpdateCollection(IEnumerable<dynamic> enumerables)
        {
            Result1.Clear();
            foreach (var enumera in enumerables)
            {
                Result1.Add(enumera);
            }
            SetDynamicTable();
        }

        public void UpdateRecordCount()
        {
            NumberOfPages = (int)Math.Ceiling((double)transactions.Count / SelectedRecord);
            NumberOfPages = NumberOfPages == 0 ? 1 : NumberOfPages;
            UpdateCollection(transactions.Take(SelectedRecord));
            CurrentPage = 1;
        }
        public void UpdateEnableStatus()
        {
            IsFirstEnable = CurrentPage > 1;
            IsPreviousEnable = CurrentPage > 1;
            IsNextEnable = CurrentPage < NumberOfPages;
            IsLastEnable = CurrentPage < NumberOfPages;
            //FirstPage.IsEnabled = CurrentPage > 1;
            //PreviousPage.IsEnabled = CurrentPage > 1;
            //NextPage.IsEnabled= CurrentPage < NumberOfPages;
            //LastPage.IsEnabled=CurrentPage< NumberOfPages;
        }
        private void PaginatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(PaginatorComboBox.SelectedValue?.ToString()))
            {
                SelectedRecord = Convert.ToInt32(PaginatorComboBox.SelectedValue.ToString());
            }
            UpdateRecordCount();
        }


        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            UpdateCollection(transactions.Take(SelectedRecord));
            CurrentPage = 1;
            UpdateEnableStatus();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
            //RecordStartFrom = transactions.Count - SelectedRecord * (NumberOfPages - (CurrentPage - 1));
            RecordStartFrom = SelectedRecord * (CurrentPage - 1);
            var RecordToShow = transactions.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            UpdateEnableStatus();
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            RecordStartFrom = CurrentPage * SelectedRecord;
            var RecordToShow = transactions.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            CurrentPage++;
            UpdateEnableStatus();

        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            var reordsToSkip = SelectedRecord * (NumberOfPages - 1);
            UpdateCollection(transactions.Skip(reordsToSkip));
            CurrentPage = NumberOfPages;
            UpdateEnableStatus();
        }
    }
}
