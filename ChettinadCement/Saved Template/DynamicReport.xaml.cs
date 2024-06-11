using iTextSharp.text;
using iTextSharp.text.pdf;
using IWT.Models;
using IWT.Shared;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using Rectangle = iTextSharp.text.Rectangle;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Windows.Documents.Serialization;
using NPOI.XWPF.UserModel;
using System.IO.Packaging;
using IWT.DBCall;

namespace IWT.Saved_Template
{
    /// <summary>
    /// Interaction logic for DynamicReport.xaml
    /// </summary>
    public partial class DynamicReport : UserControl, INotifyPropertyChanged
    {
        DataTable dt;
        List<DataTable> dataTables = new List<DataTable>();
        List<string> headings = new List<string>();
        public static CommonFunction commonFunction = new CommonFunction();
        public List<Company_Details> company_Details = new List<Company_Details>();
        public List<CompanySummaryReportData> companySummaryReportDatas = new List<CompanySummaryReportData>();
        List<Transaction> transactions = new List<Transaction>();
        Dictionary<string, string> columnNames = new Dictionary<string, string>();
        bool IsGroupByChecked = false;
        string GroupByColumn1 = null, GroupByColumn2 = null, GroupByColumn3 = null;

        List<string> RegisteredNames = new List<string>();
        string header_title;
        string company_name;
        string companyphone_address;
        string name;
        string companyLogoPath = "";

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

        List<dynamic> Result = new List<dynamic>();
        List<dynamic> Result1 = new List<dynamic>();
        DataRow newrow;
        private MasterDBCall masterDB = new MasterDBCall();

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


        public DynamicReport(DataTable _dt, Dictionary<string, string> _columnNames, bool _IsGroupByChecked = false,
            string _GroupByColumn1 = null, string _GroupByColumn2 = null, string _GroupByColumn3 = null)
        {
            InitializeComponent();
            dt = _dt;
            DataRow row = dt.NewRow();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                var type = dt.Columns[i].DataType;
                if ((type == typeof(Double) || type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64)) && dt.Columns[i].ColumnName != "TicketNo")
                {
                    row[dt.Columns[i].ColumnName] = dt.Compute($"Sum({dt.Columns[i].ColumnName})", "");
                }
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string query1 = "select LoadStatus,TransactionType from [Transaction] where TicketNo=@TicketNo";
                System.Data.SqlClient.SqlCommand cmd1 = new System.Data.SqlClient.SqlCommand(query1);
                cmd1.Parameters.AddWithValue("@TicketNo", dt.Rows[i]["TicketNo"].ToString());
                DataTable trans = masterDB.GetData(cmd1, System.Data.CommandType.Text);
                string JSONString1 = JsonConvert.SerializeObject(trans);
                var result1 = JsonConvert.DeserializeObject<List<Transaction>>(JSONString1);
                for (int j = 0; j < result1.Count; j++)
                {
                    if (result1[j].TransactionType == "SecondMulti")
                    {
                        string query = "select * from [Transaction_Details] where TicketNo=@TicketNo";
                        System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(query);
                        cmd.Parameters.AddWithValue("@TicketNo", dt.Rows[i]["TicketNo"].ToString());
                        DataTable trans_details = masterDB.GetData(cmd, System.Data.CommandType.Text);

                        string JSONString = JsonConvert.SerializeObject(trans_details);
                        var result = JsonConvert.DeserializeObject<List<TransactionDetails>>(JSONString);

                        if (result1[j].LoadStatus == "Empty" && result.Count > 0)
                        {
                            dt.Rows[i]["EmptyWeight"] = result[result.Count - 1].TDEmptyWeight.ToString();
                            dt.Rows[i]["LoadWeight"] = result[0].TDLoadWeight.ToString();
                            //int value =int.Parse(dt.Rows[i]["EmptyWeight"]) - int.Parse
                            // dt.Rows[i]["NetWeight"] = (Int32.Parse(dt.Rows[i]["EmptyWeight"].ToString()) - Int32.Parse(dt.Rows[i]["EmptyWeight"])).ToString());
                            dt.Rows[i]["NetWeight"] = Convert.ToInt32(dt.Rows[i]["LoadWeight"]) - Convert.ToInt32(dt.Rows[i]["EmptyWeight"]);
                        }
                        if (result1[j].LoadStatus == "Loaded")
                        {
                            dt.Rows[i]["EmptyWeight"] = result[0].TDEmptyWeight.ToString();
                            dt.Rows[i]["LoadWeight"] = result[result.Count - 1].TDLoadWeight.ToString();
                            //int value =int.Parse(dt.Rows[i]["EmptyWeight"]) - int.Parse
                            // dt.Rows[i]["NetWeight"] = (Int32.Parse(dt.Rows[i]["EmptyWeight"].ToString()) - Int32.Parse(dt.Rows[i]["EmptyWeight"])).ToString());
                            dt.Rows[i]["NetWeight"] = Convert.ToInt32(dt.Rows[i]["LoadWeight"]) - Convert.ToInt32(dt.Rows[i]["EmptyWeight"]);

                        }
                    }
                }
            }

            newrow = row;
            dt.Rows.Add(newrow);
            //transactions = _transactions;
            columnNames = _columnNames;
            IsGroupByChecked = _IsGroupByChecked;
            GroupByColumn1 = _GroupByColumn1;
            GroupByColumn2 = _GroupByColumn2;
            GroupByColumn3 = _GroupByColumn3;
            RegisteredNames = new List<string>();
            DataContext = this;
            SelectedRecord = 10;
            NumberOfPages = 1;
            //GetCompanyDetails();
            PaginatorComboBox.ItemsSource = ItemPerPagesList;
            dataTables = new List<DataTable>();
            headings = new List<string>();
            GetCompanySummaryReportData();
            LoadDataGrid();
        }
        //private void GetCompanyDetails()
        //{
        //    try
        //    {
        //        company_Details = commonFunction.GetCompanyDetails();
        //        if (company_Details != null && company_Details.Count > 0)
        //        {
        //            var company = company_Details[0];
        //            CompanyNameTextBlock.Text = company.Name;
        //            AddressLine1TextBlock.Text = company.Address1;
        //            AddressLine2TextBlock.Text = company.Address2;
        //            CityTextBlock.Text = $"{company.City} , {company.State}";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog.WriteToFile("DynamicReport/GetCompanyDetails/Exception:- " + ex.Message, ex);
        //    }
        //}
        private void GetCompanySummaryReportData()
        {
            try
            {
                companySummaryReportDatas = commonFunction.GetCompanySummaryReportData();
                if (companySummaryReportDatas != null && companySummaryReportDatas.Count > 0)
                {
                    var company = companySummaryReportDatas[0];
                    CompanyNameTextBlock.Text = company.CompanyHeaderTitle;
                    CompanyNameTextBlock1.Text = company.CompanyHeaderTitle;
                    header_title = company.CompanyHeaderTitle;
                    AddressLine1TextBlock.Text = company.CompanyName;
                    AddressLine1TextBlock1.Text = company.CompanyName;
                    company_name = company.CompanyName;
                    AddressLine2TextBlock.Text = company.CompanyPhoneAddress;
                    AddressLine2TextBlock1.Text = company.CompanyPhoneAddress;
                    companyphone_address = company.CompanyPhoneAddress;
                    name = company_name + "\n" + companyphone_address;

                    if (!string.IsNullOrEmpty(company.LogoPath) && File.Exists(company.LogoPath))
                    {
                        companyLogoPath = company.LogoPath;
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(company.LogoPath);
                        bitmap.EndInit();
                        CompanyLogoImage.Source = bitmap;
                        CompanyLogoImage1.Source = bitmap;
                    }
                    //CityTextBlock.Text = $"{company.City} , {company.State}";
                    FooterTextBlock.Text = company.CompanyFooter;
                    FooterTextBlock2.Text = company.CompanyFooter;
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DynamicReport/GetCompanyDetails/Exception:- " + ex.Message, ex);
            }
        }
        public void DynamicTable()
        {

            //DataGrid d1 = new DataGrid();
            //DataGridTextColumn t1 = new DataGridTextColumn();
            //t1.Header = ;
            //d1.Columns.Add(t1);
            // grid.Children.Add(d1);
        }

        public void LoadDataGrid()
        {

            //ReportDataGrid.DataContext = dt.DefaultView;
            string JSONString = JsonConvert.SerializeObject(dt);
            Result = JsonConvert.DeserializeObject<List<dynamic>>(JSONString);
            Result = Result.Distinct().ToList();
            if (!IsGroupByChecked)
            {
                PaginatorPanel.Visibility = Visibility.Visible;
                ReportDataGrid.Columns.Clear();
                foreach (var colName in columnNames)
                {
                    ReportDataGrid.Columns.Add(new MaterialDesignThemes.Wpf.DataGridTextColumn
                    {
                        // bind to a dictionary property
                        Binding = new Binding(colName.Key),
                        Header = colName.Value,
                    });
                }
                //ReportDataGrid.ItemsSource = lis;
                //ReportDataGrid.Items.Refresh();
                TotalRecords = Result.Count;
                UpdateRecordCount();
                UpdateCollection(Result.Take(SelectedRecord));
                SetDynamicTable();
            }
            else
            {
                PaginatorPanel.Visibility = Visibility.Collapsed;
                ReportDataGrid.Visibility = Visibility.Collapsed;
                DynamicReportPanel.Visibility = Visibility.Visible;
                CreateDynamicGroups(dt, false);
            }
        }

        public void SetDynamicTable()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    ReportDataGrid.ItemsSource = Result1;
                    ReportDataGrid.Items.Refresh();
                });
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetTableData:" + ex.Message);
            }
        }

        public void CreateDynamicGroups(DataTable dt, bool IsLandscape)
        {
            try
            {
                DynamicReportPanel.Children.Clear();
                dataTables = new List<DataTable>();
                IEnumerable<IGrouping<object, dynamic>> list = new List<IGrouping<object, dynamic>>();
                if (!string.IsNullOrEmpty(GroupByColumn1))
                {
                    if (!string.IsNullOrEmpty(GroupByColumn2))
                    {
                        if (!string.IsNullOrEmpty(GroupByColumn3))
                        {
                            var dt1 = dt.AsEnumerable()
                           .GroupBy(r => new { Column1 = GroupByColumn1.ToLower().Contains("date") ? Convert.ToDateTime(r[$"{GroupByColumn1}"]).ToString("yyyy/MM/dd") : r[$"{GroupByColumn1}"], 
                               Column2 = GroupByColumn2.ToLower().Contains("date") ? Convert.ToDateTime(r[$"{GroupByColumn2}"]).ToString("yyyy/MM/dd") : r[$"{GroupByColumn2}"], 
                               Column3 = GroupByColumn3.ToLower().Contains("date") ? Convert.ToDateTime(r[$"{GroupByColumn3}"]).ToString("yyyy/MM/dd") : r[$"{GroupByColumn3}"]
                           })
                           .Select(g => g).ToList();
                            CreateAndRegisterDynamicGroups(dt1, IsLandscape);

                        }
                        else
                        {
                            var dt1 = dt.AsEnumerable()
                           .GroupBy(r => new { Column1 = GroupByColumn1.ToLower().Contains("date") ? Convert.ToDateTime(r[$"{GroupByColumn1}"]).ToString("yyyy/MM/dd") : r[$"{GroupByColumn1}"], 
                               Column2 = GroupByColumn2.ToLower().Contains("date") ? Convert.ToDateTime(r[$"{GroupByColumn2}"]).ToString("yyyy/MM/dd") : r[$"{GroupByColumn2}"]
                           })
                           .Select(g => g).ToList();
                            CreateAndRegisterDynamicGroups(dt1, IsLandscape);
                        }
                    }
                    else
                    {

                        //Convert.ToDateTime(row["Date"]).ToString("dd/MM/yyyy");

                        var dt1 = dt.AsEnumerable()
                          .GroupBy(r => new { Column1 = GroupByColumn1.ToLower().Contains("date") ? Convert.ToDateTime(r[$"{GroupByColumn1}"]).ToString("yyyy/MM/dd") : r[$"{GroupByColumn1}"] })
                          .Select(g => g).ToList();
                        CreateAndRegisterDynamicGroups(dt1, IsLandscape);
                    }
                    //CreateAndRegisterDynamicGroups(list);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DynamicReport/CreateDynamicGroups/Exception:- " + ex.Message, ex);

            }
        }

        public void CreateAndRegisterDynamicGroups(IEnumerable<IGrouping<dynamic, dynamic>> groupings, bool IsLandscape)
        {
            foreach (var grouping in groupings)
            {
                int No = 100000;
                AddChilderenToDynamicReportPanel(grouping, No, IsLandscape);
                No++;
            }
        }

        public void AddChilderenToDynamicReportPanel(IGrouping<object, dynamic> group, int No, bool IsLandscape)
        {
            string heading = "";
            var key = group.Key?.ToString();
            heading = GetHeading(key);
            Random r = new Random();
            var rNumber = "c_text_" + No.ToString();
            //var gridName = string.IsNullOrEmpty(heading) ? "rNumber" : heading;
            var gridName = rNumber;
            var lableName = gridName + "_" + "TextBlock";

            TextBlock TextBlock = CreateDynamicTextBlock(heading);
            //RegisterName($"{lableName}", TextBlock);
            //RegisteredNames.Add(lableName);
            DynamicReportPanel.Children.Add(TextBlock);

            DataGrid dataGrid = CreateDynamicDataGrid(group, heading, IsLandscape);
            //RegisterName($"{gridName}", dataGrid);
            //RegisteredNames.Add(gridName);
            DynamicReportPanel.Children.Add(dataGrid);
        }

        public string GetHeading(string key)
        {
            string heading = "";
            try
            {
                var v = key.Replace("=", ":'").Replace(",", "',").Replace("}", "'}");
                GroupByClass groupByClass = JsonConvert.DeserializeObject<GroupByClass>(v?.ToString());
                if (groupByClass != null)
                {
                    if (!string.IsNullOrEmpty(groupByClass.Column1))
                    {
                        heading = groupByClass.Column1;
                        if (!string.IsNullOrEmpty(groupByClass.Column2))
                        {
                            heading = heading + " - " + groupByClass.Column2;
                            if (!string.IsNullOrEmpty(groupByClass.Column3))
                            {
                                heading = heading + " - " + groupByClass.Column3;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DynamicReport/GetHeading/Exception:- " + ex.Message, ex);
            }
            return heading;
        }

        public TextBlock CreateDynamicTextBlock(string heading)
        {
            TextBlock TextBlock = new TextBlock();
            try
            {
                TextBlock.Margin = new Thickness(10);
                TextBlock.FontSize = 14;
                TextBlock.FontWeight = FontWeights.Bold;
                TextBlock.Text = heading;
                TextBlock.TextDecorations = System.Windows.TextDecorations.Underline;

                //TextDecoration myUnderline = new TextDecoration();
                //// Create a linear gradient pen for the text decoration.
                //Pen myPen = new Pen();
                //myPen.Brush = new LinearGradientBrush(Colors.Yellow, Colors.Red, new Point(0, 0.5), new Point(1, 0.5));
                //myPen.Brush.Opacity = 0.5;
                //myPen.Thickness = 1.5;
                //myPen.DashStyle = DashStyles.Dash;
                //myUnderline.Pen = myPen;
                //myUnderline.PenThicknessUnit = TextDecorationUnit.FontRecommended;

                //// Set the underline decoration to a TextDecorationCollection and add it to the text block.
                //TextDecorationCollection myCollection = new TextDecorationCollection();
                //myCollection.Add(myUnderline);

                //TextBlock.TextDecorations = myCollection;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DynamicReport/CreateDynamicTextBlock/Exception:- " + ex.Message, ex);
            }
            return TextBlock;
        }

        public DataGrid CreateDynamicDataGrid(IGrouping<object, dynamic> group, string heading, bool IsLandscape)
        {
            var datagrid = new DataGrid();

            try
            {
                datagrid.CanUserAddRows = false;
                datagrid.AutoGenerateColumns = false;
                datagrid.Margin = new Thickness(10, 0, 10, 10);
                datagrid.IsReadOnly = true;
                datagrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                foreach (var colName in columnNames)
                {
                    datagrid.Columns.Add(new MaterialDesignThemes.Wpf.DataGridTextColumn
                    {
                        // bind to a dictionary property
                        Binding = new Binding(colName.Key),
                        Header = colName.Value,
                    });
                }

                DataTable dt1 = new DataTable();
                dt1 = CreateDynamicDataTable(group, IsLandscape);
                dataTables.Add(dt1);
                headings.Add(heading);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var lis = JsonConvert.DeserializeObject<List<dynamic>>(JSONString);
                lis = lis.Distinct().ToList();
                datagrid.ItemsSource = lis;
                datagrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DynamicReport/CreateDynamicDataGrid/Exception:- " + ex.Message, ex);
            }

            return datagrid;
        }

        public DataTable CreateDynamicDataTable(IGrouping<object, dynamic> group, bool IsLandscape)
        {
            DataTable dt1 = new DataTable();
            var x = group.ToList();
            try
            {
                foreach (var colName in columnNames)
                {
                    if (dt.Columns[colName.Key].DataType == typeof(DateTime) || dt.Columns[colName.Key].DataType == typeof(DateTime?) ||
                        dt.Columns[colName.Key].DataType == typeof(Boolean) || dt.Columns[colName.Key].DataType == typeof(Boolean?) ||
                        dt.Columns[colName.Key].DataType == typeof(Byte[]))
                    {
                        dt1.Columns.Add(colName.Key);
                    }
                    else
                        dt1.Columns.Add(colName.Key, dt.Columns[colName.Key].DataType);
                }
                foreach (dynamic d in x)
                {
                    var y = d as DataRow;
                    ////dt1.Rows.Add(y.ItemArray);
                    dt1.ImportRow(y);
                }

                if (IsLandscape)
                {
                    DataRow row = dt1.NewRow();
                    for (int i = 0; i < dt1.Columns.Count; i++)
                    {
                        //dt1.Columns[i].DataType = dt.Columns[i].DataType;
                        var type = dt1.Columns[i].DataType;
                        if ((type == typeof(Double) || type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64)) && dt1.Columns[i].ColumnName != "TicketNo")
                        {
                            row[dt1.Columns[i].ColumnName] = dt1.Compute($"Sum({dt1.Columns[i].ColumnName})", "");
                        }
                        else
                        {
                            if (dt1.Columns[i].ColumnName == "TicketNo")
                            {
                                row[dt1.Columns[i].ColumnName] = 0;
                            }
                            else
                            {
                                //row[dt1.Columns[i].ColumnName] = "";
                                if (type == typeof(DateTime) || type == typeof(DateTime?))
                                {
                                    if (IsLandscape)
                                        row[dt1.Columns[i].ColumnName] = (DateTime?)null;
                                    else
                                        row[dt1.Columns[i].ColumnName] = DateTime.Now;
                                }
                                else if (type == typeof(Boolean) || type == typeof(Boolean?))
                                {
                                    if (IsLandscape)
                                        row[dt1.Columns[i].ColumnName] = false;
                                    else
                                        row[dt1.Columns[i].ColumnName] = false;
                                }
                                else if (type == typeof(Byte[]))
                                {
                                    if (IsLandscape)
                                        row[dt1.Columns[i].ColumnName] = null;
                                    else
                                        row[dt1.Columns[i].ColumnName] = null;
                                }
                                else
                                {
                                    row[dt1.Columns[i].ColumnName] = "";
                                }
                            }
                        }
                    }
                    dt1.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DynamicReport/CreateDynamicDataTable/Exception:- " + ex.Message, ex);
            }
            return dt1;
        }

        private void EmailBtn_Click(object sender, RoutedEventArgs e)
        {
            //var bytearray = this.ExportToPdf(dt);
            WriteLog.WriteToFile("DynamicReport/EmailBtn_Click clicked");
            var bytearray1 = this.ExportToPdf1(dt);
            var message = "Please find the summary report";
            var FileName = "report.pdf";
            if (commonFunction.CheckAndSendEmail1(message, bytearray1, FileName))
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Mail has been sent the successfully");
                //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Mail has been sent the successfully");
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !");
                //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !");
            }
        }
        private void EmailAsPdf_Click(object sender, RoutedEventArgs e)
        {
            byte[] bytearray1 = null;
            if (IsGroupByChecked)
            {
                bytearray1 = this.ExportToPdfs();
            }
            else
            {
                bytearray1 = this.ExportToPdf1(dt);
            }
            if (bytearray1 != null)
            {
                var message = "Please find the summary report";
                var FileName = "report.pdf";
                if (commonFunction.CheckAndSendEmail1(message, bytearray1, FileName))
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Mail has been sent the successfully");
                    //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Mail has been sent the successfully");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !");
                    //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !");
                }
            }

        }

        private void EmailAsExcel_Click(object sender, RoutedEventArgs e)
        {
            byte[] bytearray1 = null;
            if (IsGroupByChecked)
            {
                bytearray1 = this.ExportToExcels();
            }
            else
            {
                bytearray1 = this.ExportToExcel(dt);
            }
            if (bytearray1 != null)
            {
                var message = "Please find the summary report";
                var FileName = "report.xlsx";
                if (commonFunction.CheckAndSendEmail1(message, bytearray1, FileName))
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Mail has been sent the successfully");
                    //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Mail has been sent the successfully");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !");
                    //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !");
                }
            }
        }

        private void EmailAsWord_Click(object sender, RoutedEventArgs e)
        {
            byte[] bytearray1 = null;
            if (IsGroupByChecked)
            {
                bytearray1 = this.ExportToWords();
            }
            else
            {
                bytearray1 = this.ExportToWord(dt);
            }
            if (bytearray1 != null)
            {
                var message = "Please find the summary report";
                var FileName = "report.docx";
                if (commonFunction.CheckAndSendEmail1(message, bytearray1, FileName))
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Mail has been sent the successfully");
                    //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Mail has been sent the successfully");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !");
                    //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !");
                }
            }
        }

        //private static object GetPropertyValue(object obj, string propertyName)
        //{
        //    return obj.GetType().GetProperty(propertyName).GetValue(obj, null);
        //}
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            UnRegisterDynamicGrids();
            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        public void UnRegisterDynamicGrids()
        {
            foreach (string name in RegisteredNames)
            {
                UnregisterName(name);
            }
        }

        public void CreateTempFolder()
        {
            string TempFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
            if (!Directory.Exists(TempFolder))
            {
                Directory.CreateDirectory(TempFolder);
            }
        }

        public byte[] ExportToPdf1(DataTable dt)
        {
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {

                DataRow row = dt.NewRow();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var type = dt.Columns[i].DataType;
                    if ((type == typeof(Double) || type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64)) && dt.Columns[i].ColumnName != "TicketNo")
                    {
                        row[dt.Columns[i].ColumnName] = dt.Compute($"Sum({dt.Columns[i].ColumnName})", "");
                    }
                    else
                    {
                        if (dt.Columns[i].ColumnName == "TicketNo")
                        {
                            row[dt.Columns[i].ColumnName] = 0;
                        }
                        else
                        {
                            //row[dt.Columns[i].ColumnName] = "";
                            if (type == typeof(DateTime) || type == typeof(DateTime?))
                            {
                                row[dt.Columns[i].ColumnName] = DateTime.Now;
                            }
                            //else
                            //{
                            //    row[dt.Columns[i].ColumnName] = "";
                            //}
                            else if (type == typeof(Boolean) || type == typeof(Boolean?))
                            {
                                row[dt.Columns[i].ColumnName] = false;
                            }
                            else if (type == typeof(Byte[]))
                            {
                                row[dt.Columns[i].ColumnName] = null;
                            }
                            else
                            {
                                row[dt.Columns[i].ColumnName] = "";
                            }
                        }
                    }
                }
                //dt.Rows.Add(row);
                iTextSharp.text.Document document = new iTextSharp.text.Document(PageSize.A4, 10, 10, 10, 10);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                PdfContentByte cb = new PdfContentByte(writer);
                document.Open();
                var imagePath = "";

                Font font5 = FontFactory.GetFont(FontFactory.HELVETICA, 5);
                var titleFont = FontFactory.GetFont("Arial", 10, Font.BOLD);
                var normalFont = FontFactory.GetFont("Arial", 8, Font.NORMAL);


                PdfPTable POHeaderTable = new PdfPTable(3) { WidthPercentage = 100 };
                POHeaderTable.SetWidths(new float[] { 25f, 45f, 30f });

                PdfPCell cell1 = new PdfPCell();

                if (!string.IsNullOrEmpty(companyLogoPath) && File.Exists(companyLogoPath))
                {
                    imagePath = companyLogoPath;
                }
                else
                {
                    imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "download.png");
                }
                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    iTextSharp.text.Image img1 = iTextSharp.text.Image.GetInstance(imagePath);
                    img1.ScaleAbsolute(15f, 12f);
                    PdfPCell Imgcell = new PdfPCell(img1);
                    Imgcell.Padding = 5f;
                    Imgcell.PaddingBottom = 12f;
                    Imgcell.Border = Rectangle.NO_BORDER;
                    Imgcell.VerticalAlignment = Element.ALIGN_BOTTOM;
                    POHeaderTable.AddCell(Imgcell);
                }
                else
                {
                    PdfPCell cell11 = new PdfPCell(new Phrase("", normalFont));
                    cell11.Border = Rectangle.NO_BORDER;
                    POHeaderTable.AddCell(cell11);
                }

                PdfPCell cell = new PdfPCell(new Phrase(header_title, normalFont));
                cell.Border = Rectangle.NO_BORDER;
                POHeaderTable.AddCell(cell);

                cell = new PdfPCell(new Phrase(name, normalFont));
                cell.Border = Rectangle.NO_BORDER;
                POHeaderTable.AddCell(cell);

                PdfPTable table = new PdfPTable(dt.Columns.Count);


                Array floatArray = Array.CreateInstance(typeof(float), dt.Columns.Count);
                for (int i = 0; i < dt.Columns.Count; i++)
                    floatArray.SetValue(4f, i);

                table.SetWidths((float[])floatArray);

                table.WidthPercentage = 100;
                //PdfPCell cell1 = new PdfPCell(new Phrase("Products"));

                cell.Colspan = dt.Columns.Count;

                foreach (DataColumn c in dt.Columns)
                {
                    table.AddCell(new Phrase(c.ColumnName, font5));
                }
                int it = 0;
                foreach (DataRow r in dt.Rows)
                {
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            //table.AddCell(new Phrase(r[i].ToString(), font5));
                            if (it == dt.Rows.Count - 1)
                            {
                                if (dt.Columns[i].DataType == typeof(DateTime) || dt.Columns[i].DataType == typeof(DateTime?))
                                {
                                    table.AddCell(new Phrase("", font5));
                                }
                                else if (dt.Columns[i].DataType == typeof(Boolean) || dt.Columns[i].DataType == typeof(Boolean?))
                                {
                                    table.AddCell(new Phrase("", font5));
                                }
                                else if (dt.Columns[i].DataType == typeof(Byte[]))
                                {
                                    table.AddCell(new Phrase("", font5));
                                }
                                else
                                {
                                    table.AddCell(new Phrase(r[i].ToString(), font5));
                                }
                            }
                            else
                            {
                                table.AddCell(new Phrase(r[i].ToString(), font5));
                            }
                        }
                    }
                    it++;
                }
                document.Add(POHeaderTable);
                table.SpacingBefore = 5f;
                document.Add(table);
                document.Close();
                //var FileName = $"PRDetails.pdf";
                //string TempFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
                ////var FileName = "NoticeDetails.xls";
                //var FileFullPath = System.IO.Path.Combine(TempFolder, FileName);
                //if (System.IO.File.Exists(FileFullPath))
                //{
                //    System.GC.Collect();
                //    System.GC.WaitForPendingFinalizers();
                //    System.IO.File.Delete(FileFullPath);
                //}
                //byte[] fileByteArray = System.IO.File.ReadAllBytes("C:\\Users\\User.EA1180\\Downloads\\GRN_PRINT.pdf");
                byte[] fileByteArray = memoryStream.ToArray();
                memoryStream.Close();


                //PDDocument doc = null;
                //doc = PDDocument.load(@"C:\Users\User.EA1180\Downloads\GRN_PRINT.pdf");
                //PDFTextStripper textStrip = new PDFTextStripper();
                //string strPDFText = textStrip.getText(doc);
                //doc.close();
                //string fn = @"C:\Users\User.EA1180\Downloads\sample.docx";
                //var wordDoc = DocX.Create(fn);
                //wordDoc.InsertParagraph(strPDFText);
                //wordDoc.Save();

                //if (dt.Rows.Count > 0)
                //{
                //    dt.Rows.RemoveAt(dt.Rows.Count - 1);
                //}


                return fileByteArray;
            }

        }

        public byte[] ExportToPdfs()
        {
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                iTextSharp.text.Document document = new iTextSharp.text.Document(PageSize.A4, 10, 10, 10, 10);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                PdfContentByte cb = new PdfContentByte(writer);
                document.Open();
                var imagePath = "";

                Font font5 = FontFactory.GetFont(FontFactory.HELVETICA, 5);
                var titleFont = FontFactory.GetFont("Arial", 10, Font.BOLD);
                var normalFont = FontFactory.GetFont("Arial", 8, Font.NORMAL);


                PdfPTable POHeaderTable = new PdfPTable(3) { WidthPercentage = 100 };
                POHeaderTable.SetWidths(new float[] { 25f, 45f, 30f });

                PdfPCell cell1 = new PdfPCell();

                if (!string.IsNullOrEmpty(companyLogoPath) && File.Exists(companyLogoPath))
                {
                    imagePath = companyLogoPath;
                }
                else
                {
                    imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "download.png");
                }
                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    iTextSharp.text.Image img1 = iTextSharp.text.Image.GetInstance(imagePath);
                    img1.ScaleAbsolute(15f, 12f);
                    PdfPCell Imgcell = new PdfPCell(img1);
                    Imgcell.Padding = 5f;
                    Imgcell.PaddingBottom = 12f;
                    Imgcell.Border = Rectangle.NO_BORDER;
                    Imgcell.VerticalAlignment = Element.ALIGN_BOTTOM;
                    POHeaderTable.AddCell(Imgcell);
                }
                else
                {
                    PdfPCell cell11 = new PdfPCell(new Phrase("", normalFont));
                    cell11.Border = Rectangle.NO_BORDER;
                    POHeaderTable.AddCell(cell11);
                }

                PdfPCell cell = new PdfPCell(new Phrase(header_title, normalFont));
                cell.Border = Rectangle.NO_BORDER;
                POHeaderTable.AddCell(cell);

                cell = new PdfPCell(new Phrase(name, normalFont));
                cell.Border = Rectangle.NO_BORDER;
                POHeaderTable.AddCell(cell);

                document.Add(POHeaderTable);

                int hi = 0;
                foreach (var dt in dataTables)
                {

                    DataRow row = dt.NewRow();
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        var type = dt.Columns[i].DataType;
                        if ((type == typeof(Double) || type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64)) && dt.Columns[i].ColumnName != "TicketNo")
                        {
                            row[dt.Columns[i].ColumnName] = dt.Compute($"Sum({dt.Columns[i].ColumnName})", "");
                        }
                        else
                        {
                            if (dt.Columns[i].ColumnName == "TicketNo")
                            {
                                row[dt.Columns[i].ColumnName] = 0;
                            }
                            else
                            {
                                if (type == typeof(DateTime) || type == typeof(DateTime?))
                                {
                                    row[dt.Columns[i].ColumnName] = DateTime.Now;
                                }
                                //else
                                //{
                                //    row[dt.Columns[i].ColumnName] = "";
                                //}
                                else if (type == typeof(Boolean) || type == typeof(Boolean?))
                                {
                                    row[dt.Columns[i].ColumnName] = false;
                                }
                                else if (type == typeof(Byte[]))
                                {
                                    row[dt.Columns[i].ColumnName] = null;
                                }
                                else
                                {
                                    row[dt.Columns[i].ColumnName] = "";
                                }
                            }
                        }
                    }
                    //dt.Rows.Add(row);

                    var heading = "";
                    if (headings?.Count > hi)
                    {
                        heading = headings[hi];
                    }

                    iTextSharp.text.Paragraph p = new iTextSharp.text.Paragraph();
                    Chunk chunk = new Chunk(heading, titleFont);
                    chunk.SetUnderline(0.5f, -1.5f);
                    p.Add(chunk);
                    p.PaddingTop = 2f;
                    document.Add(p);

                    PdfPTable table = new PdfPTable(dt.Columns.Count);

                    Array floatArray = Array.CreateInstance(typeof(float), dt.Columns.Count);
                    for (int i = 0; i < dt.Columns.Count; i++)
                        floatArray.SetValue(4f, i);

                    table.SetWidths((float[])floatArray);

                    table.WidthPercentage = 100;
                    //PdfPCell cell1 = new PdfPCell(new Phrase("Products"));

                    cell.Colspan = dt.Columns.Count;

                    foreach (DataColumn c in dt.Columns)
                    {
                        table.AddCell(new Phrase(c.ColumnName, font5));
                    }

                    int it = 0;

                    foreach (DataRow r in dt.Rows)
                    {
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                if (it == dt.Rows.Count - 1)
                                {
                                    if (dt.Columns[i].DataType == typeof(DateTime) || dt.Columns[i].DataType == typeof(DateTime?))
                                    {
                                        table.AddCell(new Phrase("", font5));
                                    }
                                    //else
                                    //{
                                    //    table.AddCell(new Phrase(r[i].ToString(), font5));
                                    //}
                                    else if (dt.Columns[i].DataType == typeof(Boolean) || dt.Columns[i].DataType == typeof(Boolean?))
                                    {
                                        table.AddCell(new Phrase("", font5));
                                    }
                                    else if (dt.Columns[i].DataType == typeof(Byte[]))
                                    {
                                        table.AddCell(new Phrase("", font5));
                                    }
                                    else
                                    {
                                        table.AddCell(new Phrase(r[i].ToString(), font5));
                                    }
                                }
                                else
                                {
                                    table.AddCell(new Phrase(r[i].ToString(), font5));
                                }
                            }
                        }
                        it++;
                    }

                    table.SpacingBefore = 7f;
                    document.Add(table);

                    //if (dt.Rows.Count > 0)
                    //{
                    //    dt.Rows.RemoveAt(dt.Rows.Count - 1);
                    //}

                    hi++;
                }


                document.Close();
                //var FileName = $"PRDetails.pdf";
                //string TempFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
                ////var FileName = "NoticeDetails.xls";
                //var FileFullPath = System.IO.Path.Combine(TempFolder, FileName);
                //if (System.IO.File.Exists(FileFullPath))
                //{
                //    System.GC.Collect();
                //    System.GC.WaitForPendingFinalizers();
                //    System.IO.File.Delete(FileFullPath);
                //}
                //byte[] fileByteArray = System.IO.File.ReadAllBytes("C:\\Users\\User.EA1180\\Downloads\\GRN_PRINT.pdf");
                byte[] fileByteArray = memoryStream.ToArray();
                memoryStream.Close();


                //PDDocument doc = null;
                //doc = PDDocument.load(@"C:\Users\User.EA1180\Downloads\GRN_PRINT.pdf");
                //PDFTextStripper textStrip = new PDFTextStripper();
                //string strPDFText = textStrip.getText(doc);
                //doc.close();
                //string fn = @"C:\Users\User.EA1180\Downloads\sample.docx";
                //var wordDoc = DocX.Create(fn);
                //wordDoc.InsertParagraph(strPDFText);
                //wordDoc.Save();


                return fileByteArray;
            }

        }

        private byte[] ExportToExcel(DataTable dt)
        {
            try
            {
                //excel = new Microsoft.Office.Interop.Excel.Application();
                //excel.DisplayAlerts = false;
                //excel.Visible = false;
                //workBook = excel.Workbooks.Add(Type.Missing);
                //workSheet = (Microsoft.Office.Interop.Excel.Worksheet)workBook.ActiveSheet;
                //workSheet.Name = "LearningExcel";
                //System.Data.DataTable tempDt = dt;
                ////dgExcel.ItemsSource = tempDt.DefaultView;
                //workSheet.Cells.Font.Size = 11;
                //int rowcount = 1;
                //for (int i = 1; i <= tempDt.Columns.Count; i++) //taking care of Headers.  
                //{
                //    workSheet.Cells[1, i] = tempDt.Columns[i - 1].ColumnName;
                //}
                //foreach (System.Data.DataRow row in tempDt.Rows) //taking care of each Row  
                //{
                //    rowcount += 1;
                //    for (int i = 0; i < tempDt.Columns.Count; i++) //taking care of each column  
                //    {
                //        workSheet.Cells[rowcount, i + 1] = row[i].ToString();
                //    }
                //}
                //cellRange = workSheet.Range[workSheet.Cells[1, 1], workSheet.Cells[rowcount, tempDt.Columns.Count]];
                //cellRange.EntireColumn.AutoFit();
                //workBook.SaveAs(System.IO.Path.Combine("C:\\Users\\User.EA1180\\Downloads\\", "Summary3"));
                //byte[] fileByteArray = System.IO.File.ReadAllBytes("C:\\Users\\User.EA1180\\Downloads\\Summary3.xlsx");
                //workBook.Close();
                //excel.Quit();
                //return fileByteArray;

                DataRow roww = dt.NewRow();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var type = dt.Columns[i].DataType;
                    if ((type == typeof(Double) || type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64)) && dt.Columns[i].ColumnName != "TicketNo")
                    {
                        roww[dt.Columns[i].ColumnName] = dt.Compute($"Sum({dt.Columns[i].ColumnName})", "");
                    }
                    else
                    {
                        if (dt.Columns[i].ColumnName == "TicketNo")
                        {
                            roww[dt.Columns[i].ColumnName] = 0;
                        }
                        else
                        {
                            //roww[dt.Columns[i].ColumnName] = "";
                            if (type == typeof(DateTime) || type == typeof(DateTime?))
                            {
                                //roww[dt.Columns[i].ColumnName] = DateTime.Now;
                                roww[dt.Columns[i].ColumnName] = "";
                            }
                            //else
                            //{
                            //    roww[dt.Columns[i].ColumnName] = "";
                            //}
                            else if (type == typeof(Boolean) || type == typeof(Boolean?))
                            {
                                //roww[dt.Columns[i].ColumnName] = DateTime.Now;
                                roww[dt.Columns[i].ColumnName] = "";
                            }
                            else if (type == typeof(Byte[]))
                            {
                                //roww[dt.Columns[i].ColumnName] = DateTime.Now;
                                roww[dt.Columns[i].ColumnName] = "";
                            }
                            else
                            {
                                roww[dt.Columns[i].ColumnName] = "";
                            }
                        }
                    }
                }
                //dt.Rows.Add(roww);

                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {
                    IWorkbook workbook = new XSSFWorkbook();
                    ISheet excelSheet = workbook.CreateSheet("report");
                    List<string> columns = new List<string>();
                    IRow row = excelSheet.CreateRow(0);
                    int columnIndex = 0;

                    foreach (System.Data.DataColumn column in dt.Columns)
                    {
                        columns.Add(column.ColumnName);
                        row.CreateCell(columnIndex).SetCellValue(column.ColumnName);
                        columnIndex++;
                    }

                    int rowIndex = 1;
                    foreach (DataRow dsrow in dt.Rows)
                    {
                        row = excelSheet.CreateRow(rowIndex);
                        int cellIndex = 0;
                        foreach (String col in columns)
                        {
                            row.CreateCell(cellIndex).SetCellValue(dsrow[col].ToString());
                            cellIndex++;
                        }

                        rowIndex++;
                    }
                    workbook.Write(memoryStream);
                    byte[] fileByteArray = memoryStream.ToArray();
                    memoryStream.Close();
                    //if (dt.Rows.Count > 0)
                    //{
                    //    dt.Rows.RemoveAt(dt.Rows.Count - 1);
                    //}

                    return fileByteArray;
                }


            }
            catch (Exception)
            {
                throw;
            }
        }

        private byte[] ExportToExcels()
        {
            try
            {
                //excel = new Microsoft.Office.Interop.Excel.Application();
                //excel.DisplayAlerts = false;
                //excel.Visible = false;
                //workBook = excel.Workbooks.Add(Type.Missing);
                //workSheet = (Microsoft.Office.Interop.Excel.Worksheet)workBook.ActiveSheet;
                //workSheet.Name = "LearningExcel";
                //System.Data.DataTable tempDt = dt;
                ////dgExcel.ItemsSource = tempDt.DefaultView;
                //workSheet.Cells.Font.Size = 11;
                //int rowcount = 1;
                //for (int i = 1; i <= tempDt.Columns.Count; i++) //taking care of Headers.  
                //{
                //    workSheet.Cells[1, i] = tempDt.Columns[i - 1].ColumnName;
                //}
                //foreach (System.Data.DataRow row in tempDt.Rows) //taking care of each Row  
                //{
                //    rowcount += 1;
                //    for (int i = 0; i < tempDt.Columns.Count; i++) //taking care of each column  
                //    {
                //        workSheet.Cells[rowcount, i + 1] = row[i].ToString();
                //    }
                //}
                //cellRange = workSheet.Range[workSheet.Cells[1, 1], workSheet.Cells[rowcount, tempDt.Columns.Count]];
                //cellRange.EntireColumn.AutoFit();
                //workBook.SaveAs(System.IO.Path.Combine("C:\\Users\\User.EA1180\\Downloads\\", "Summary3"));
                //byte[] fileByteArray = System.IO.File.ReadAllBytes("C:\\Users\\User.EA1180\\Downloads\\Summary3.xlsx");
                //workBook.Close();
                //excel.Quit();
                //return fileByteArray;
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {
                    IWorkbook workbook = new XSSFWorkbook();
                    ISheet excelSheet = workbook.CreateSheet("report");
                    int hi = 0;
                    int rowIndex = 0;
                    foreach (var dt in dataTables)
                    {
                        DataRow roww = dt.NewRow();
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            var type = dt.Columns[i].DataType;
                            if ((type == typeof(Double) || type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64)) && dt.Columns[i].ColumnName != "TicketNo")
                            {
                                roww[dt.Columns[i].ColumnName] = dt.Compute($"Sum({dt.Columns[i].ColumnName})", "");
                            }
                            else
                            {
                                if (dt.Columns[i].ColumnName == "TicketNo")
                                {
                                    roww[dt.Columns[i].ColumnName] = 0;
                                }
                                else
                                {
                                    //roww[dt.Columns[i].ColumnName] = "";
                                    if (type == typeof(DateTime) || type == typeof(DateTime?))
                                    {
                                        //roww[dt.Columns[i].ColumnName] = DateTime.Now;
                                        roww[dt.Columns[i].ColumnName] = "";
                                    }
                                    //else
                                    //{
                                    //    roww[dt.Columns[i].ColumnName] = "";
                                    //}
                                    else if (type == typeof(Boolean) || type == typeof(Boolean?))
                                    {
                                        //roww[dt.Columns[i].ColumnName] = DateTime.Now;
                                        roww[dt.Columns[i].ColumnName] = "";
                                    }
                                    else if (type == typeof(Byte[]))
                                    {
                                        //roww[dt.Columns[i].ColumnName] = DateTime.Now;
                                        roww[dt.Columns[i].ColumnName] = "";
                                    }
                                    else
                                    {
                                        roww[dt.Columns[i].ColumnName] = "";
                                    }
                                }
                            }
                        }
                        //dt.Rows.Add(roww);




                        var heading = "";
                        if (headings?.Count > hi)
                        {
                            heading = headings[hi];
                        }

                        IRow row = excelSheet.CreateRow(rowIndex);
                        int cellIndex1 = 0;
                        row.CreateCell(cellIndex1).SetCellValue(heading);

                        rowIndex++;

                        List<string> columns = new List<string>();
                        row = excelSheet.CreateRow(rowIndex);
                        int columnIndex = 0;

                        foreach (System.Data.DataColumn column in dt.Columns)
                        {
                            columns.Add(column.ColumnName);
                            row.CreateCell(columnIndex).SetCellValue(column.ColumnName);
                            columnIndex++;
                        }


                        foreach (DataRow dsrow in dt.Rows)
                        {
                            row = excelSheet.CreateRow(rowIndex);
                            int cellIndex = 0;
                            foreach (String col in columns)
                            {
                                row.CreateCell(cellIndex).SetCellValue(dsrow[col].ToString());
                                cellIndex++;
                            }

                            rowIndex++;
                        }



                        //if (dt.Rows.Count > 0)
                        //{
                        //    dt.Rows.RemoveAt(dt.Rows.Count - 1);
                        //}

                        hi++;
                    }
                    workbook.Write(memoryStream);
                    byte[] fileByteArray = memoryStream.ToArray();
                    memoryStream.Close();
                    return fileByteArray;
                }


            }
            catch (Exception)
            {
                throw;
            }
        }
        private byte[] ExportToWord(DataTable dt)
        {
            try
            {
                //excel = new Microsoft.Office.Interop.Excel.Application();
                //excel.DisplayAlerts = false;
                //excel.Visible = false;
                //workBook = excel.Workbooks.Add(Type.Missing);
                //workSheet = (Microsoft.Office.Interop.Excel.Worksheet)workBook.ActiveSheet;
                //workSheet.Name = "LearningExcel";
                //System.Data.DataTable tempDt = dt;
                ////dgExcel.ItemsSource = tempDt.DefaultView;
                //workSheet.Cells.Font.Size = 11;
                //int rowcount = 1;
                //for (int i = 1; i <= tempDt.Columns.Count; i++) //taking care of Headers.  
                //{
                //    workSheet.Cells[1, i] = tempDt.Columns[i - 1].ColumnName;
                //}
                //foreach (System.Data.DataRow row in tempDt.Rows) //taking care of each Row  
                //{
                //    rowcount += 1;
                //    for (int i = 0; i < tempDt.Columns.Count; i++) //taking care of each column  
                //    {
                //        workSheet.Cells[rowcount, i + 1] = row[i].ToString();
                //    }
                //}
                //cellRange = workSheet.Range[workSheet.Cells[1, 1], workSheet.Cells[rowcount, tempDt.Columns.Count]];
                //cellRange.EntireColumn.AutoFit();
                //workBook.SaveAs(System.IO.Path.Combine("C:\\Users\\User.EA1180\\Downloads\\", "Summary3"));
                //byte[] fileByteArray = System.IO.File.ReadAllBytes("C:\\Users\\User.EA1180\\Downloads\\Summary3.xlsx");
                //workBook.Close();
                //excel.Quit();
                //return fileByteArray;



                //using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                //{
                //    XWPFDocument document = new XWPFDocument();

                //    XWPFTable table = document.CreateTable();

                //    //create first row
                //    XWPFTableRow tableRowOne = table.GetRow(0);
                //    tableRowOne.GetCell(0).SetText("col one, row one");
                //    tableRowOne.AddNewTableCell().SetText("col two, row one");
                //    tableRowOne.AddNewTableCell().SetText("col three, row one");

                //    //create second row
                //    XWPFTableRow tableRowTwo = table.CreateRow();
                //    tableRowTwo.GetCell(0).SetText("col one, row two");
                //    tableRowTwo.GetCell(1).SetText("col two, row two");
                //    tableRowTwo.GetCell(2).SetText("col three, row two");

                //    //create third row
                //    XWPFTableRow tableRowThree = table.CreateRow();
                //    tableRowThree.GetCell(0).SetText("col one, row three");
                //    tableRowThree.GetCell(1).SetText("col two, row three");
                //    tableRowThree.GetCell(2).SetText("col three, row three");

                //    document.Write(memoryStream);
                //    byte[] fileByteArray = memoryStream.ToArray();
                //    memoryStream.Close();
                //    return fileByteArray;
                //}





                DataRow roww = dt.NewRow();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var type = dt.Columns[i].DataType;
                    if ((type == typeof(Double) || type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64)) && dt.Columns[i].ColumnName != "TicketNo")
                    {
                        roww[dt.Columns[i].ColumnName] = dt.Compute($"Sum({dt.Columns[i].ColumnName})", "");
                    }
                    else
                    {
                        if (dt.Columns[i].ColumnName == "TicketNo")
                        {
                            roww[dt.Columns[i].ColumnName] = 0;
                        }
                        else
                        {
                            //roww[dt.Columns[i].ColumnName] = "";
                            if (type == typeof(DateTime) || type == typeof(DateTime?))
                            {
                                //roww[dt.Columns[i].ColumnName] = DateTime.Now;
                                roww[dt.Columns[i].ColumnName] = "";
                            }
                            //else
                            //{
                            //    roww[dt.Columns[i].ColumnName] = "";
                            //}
                            else if (type == typeof(Boolean) || type == typeof(Boolean?))
                            {
                                //roww[dt.Columns[i].ColumnName] = DateTime.Now;
                                roww[dt.Columns[i].ColumnName] = "";
                            }
                            else if (type == typeof(Byte[]))
                            {
                                //roww[dt.Columns[i].ColumnName] = DateTime.Now;
                                roww[dt.Columns[i].ColumnName] = "";
                            }
                            else
                            {
                                roww[dt.Columns[i].ColumnName] = "";
                            }
                        }
                    }
                }
                //dt.Rows.Add(roww);

                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {
                    //IWorkbook workbook = new XSSFWorkbook();
                    //ISheet excelSheet = workbook.CreateSheet("report");
                    XWPFDocument doc = new XWPFDocument();
                    List<string> columns = new List<string>();
                    XWPFTable table = doc.CreateTable();

                    //XWPFTableRow row = table.CreateRow();
                    XWPFTableRow row = table.GetRow(0);
                    int columnIndex = 0;

                    foreach (System.Data.DataColumn column in dt.Columns)
                    {
                        columns.Add(column.ColumnName);
                        if (columnIndex == 0)
                            row.GetCell(columnIndex).SetText(column.ColumnName);
                        else row.AddNewTableCell().SetText(column.ColumnName);
                        columnIndex++;
                    }

                    int rowIndex = 1;
                    foreach (DataRow dsrow in dt.Rows)
                    {
                        row = table.CreateRow();
                        int cellIndex = 0;
                        foreach (String col in columns)
                        {
                            row.GetCell(cellIndex).SetText(dsrow[col].ToString());
                            cellIndex++;
                        }

                        rowIndex++;
                    }
                    doc.Write(memoryStream);
                    byte[] fileByteArray = memoryStream.ToArray();
                    memoryStream.Close();
                    //File.WriteAllBytes(@"C:\Users\iadmin\Documents\text.docx", fileByteArray);
                    //if (dt.Rows.Count > 0)
                    //{
                    //    dt.Rows.RemoveAt(dt.Rows.Count - 1);
                    //}

                    return fileByteArray;
                }


            }
            catch (Exception)
            {
                throw;
            }
        }

        private byte[] ExportToWords()
        {
            try
            {
                //excel = new Microsoft.Office.Interop.Excel.Application();
                //excel.DisplayAlerts = false;
                //excel.Visible = false;
                //workBook = excel.Workbooks.Add(Type.Missing);
                //workSheet = (Microsoft.Office.Interop.Excel.Worksheet)workBook.ActiveSheet;
                //workSheet.Name = "LearningExcel";
                //System.Data.DataTable tempDt = dt;
                ////dgExcel.ItemsSource = tempDt.DefaultView;
                //workSheet.Cells.Font.Size = 11;
                //int rowcount = 1;
                //for (int i = 1; i <= tempDt.Columns.Count; i++) //taking care of Headers.  
                //{
                //    workSheet.Cells[1, i] = tempDt.Columns[i - 1].ColumnName;
                //}
                //foreach (System.Data.DataRow row in tempDt.Rows) //taking care of each Row  
                //{
                //    rowcount += 1;
                //    for (int i = 0; i < tempDt.Columns.Count; i++) //taking care of each column  
                //    {
                //        workSheet.Cells[rowcount, i + 1] = row[i].ToString();
                //    }
                //}
                //cellRange = workSheet.Range[workSheet.Cells[1, 1], workSheet.Cells[rowcount, tempDt.Columns.Count]];
                //cellRange.EntireColumn.AutoFit();
                //workBook.SaveAs(System.IO.Path.Combine("C:\\Users\\User.EA1180\\Downloads\\", "Summary3"));
                //byte[] fileByteArray = System.IO.File.ReadAllBytes("C:\\Users\\User.EA1180\\Downloads\\Summary3.xlsx");
                //workBook.Close();
                //excel.Quit();
                //return fileByteArray;
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {
                    //IWorkbook workbook = new XSSFWorkbook();
                    //ISheet excelSheet = workbook.CreateSheet("report");
                    XWPFDocument doc = new XWPFDocument();
                    int hi = 0;
                    foreach (var dt in dataTables)
                    {
                        DataRow roww = dt.NewRow();
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            var type = dt.Columns[i].DataType;
                            if ((type == typeof(Double) || type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64)) && dt.Columns[i].ColumnName != "TicketNo")
                            {
                                roww[dt.Columns[i].ColumnName] = dt.Compute($"Sum({dt.Columns[i].ColumnName})", "");
                            }
                            else
                            {
                                if (dt.Columns[i].ColumnName == "TicketNo")
                                {
                                    roww[dt.Columns[i].ColumnName] = 0;
                                }
                                else
                                {
                                    //roww[dt.Columns[i].ColumnName] = "";
                                    if (type == typeof(DateTime) || type == typeof(DateTime?))
                                    {
                                        //roww[dt.Columns[i].ColumnName] = DateTime.Now;
                                        roww[dt.Columns[i].ColumnName] = "";
                                    }
                                    //else
                                    //{
                                    //    roww[dt.Columns[i].ColumnName] = "";
                                    //}
                                    else if (type == typeof(Boolean) || type == typeof(Boolean?))
                                    {
                                        //roww[dt.Columns[i].ColumnName] = DateTime.Now;
                                        roww[dt.Columns[i].ColumnName] = "";
                                    }
                                    else if (type == typeof(Byte[]))
                                    {
                                        //roww[dt.Columns[i].ColumnName] = DateTime.Now;
                                        roww[dt.Columns[i].ColumnName] = "";
                                    }
                                    else
                                    {
                                        roww[dt.Columns[i].ColumnName] = "";
                                    }
                                }
                            }
                        }
                        //dt.Rows.Add(roww);

                        var heading = "";
                        if (headings?.Count > hi)
                        {
                            heading = headings[hi];
                        }

                        XWPFParagraph paragraph = doc.CreateParagraph();
                        XWPFRun run = paragraph.CreateRun();
                        run.IsBold = true;
                        run.SetUnderline(UnderlinePatterns.Single);
                        run.SetText(heading);

                        List<string> columns = new List<string>();
                        XWPFTable table = doc.CreateTable();


                        XWPFTableRow row = table.GetRow(0);
                        int columnIndex = 0;

                        foreach (System.Data.DataColumn column in dt.Columns)
                        {
                            //columns.Add(column.ColumnName);
                            //row.CreateCell().SetText(column.ColumnName);
                            columns.Add(column.ColumnName);
                            if (columnIndex == 0)
                                row.GetCell(columnIndex).SetText(column.ColumnName);
                            else row.AddNewTableCell().SetText(column.ColumnName);
                            columnIndex++;
                        }

                        int rowIndex = 1;
                        foreach (DataRow dsrow in dt.Rows)
                        {
                            row = table.CreateRow();
                            int cellIndex = 0;
                            foreach (String col in columns)
                            {
                                row.GetCell(cellIndex).SetText(dsrow[col].ToString());
                                cellIndex++;
                            }

                            rowIndex++;
                        }

                        //File.WriteAllBytes(@"C:\Users\iadmin\Documents\text.docx", fileByteArray);
                        //if (dt.Rows.Count > 0)
                        //{
                        //    dt.Rows.RemoveAt(dt.Rows.Count - 1);
                        //}
                        hi++;
                    }
                    doc.Write(memoryStream);
                    byte[] fileByteArray = memoryStream.ToArray();
                    memoryStream.Close();
                    return fileByteArray;
                }


            }
            catch (Exception)
            {
                throw;
            }
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {

            var bytearray1 = this.ExportToPdf1(dt);
            var dat = DateTime.Now.ToString("ddMMyyyyhhmmss");
            var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{dat}print.pdf");
            File.WriteAllBytes(filePath, bytearray1);
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            //ProcessStartInfo info = new ProcessStartInfo();
            //info.Verb = "print";
            //info.FileName = filePath;
            //info.CreateNoWindow = true;
            //info.WindowStyle = ProcessWindowStyle.Hidden;

            //Process p = new Process();
            //p.StartInfo = info;
            //p.Start();

            //p.WaitForInputIdle();
            //System.Threading.Thread.Sleep(3000);
            //if (false == p.CloseMainWindow())
            //    p.Kill();


            //string filePath = "C:\\Users\\User.EA1180\\Downloads\\GRN_PRINT.pdf";
            //ProcessStartInfo info = new ProcessStartInfo(filePath);
            //info.Verb = "Print";
            //info.CreateNoWindow = true;
            //info.WindowStyle = ProcessWindowStyle.Hidden;
            //Process.Start(info);

            //BitmapImage myBitmapImage = new BitmapImage();
            //myBitmapImage.BeginInit();
            //myBitmapImage.UriSource = new Uri(@"C:\\Users\\User.EA1180\\Downloads\\GRN_PRINT.pdf");
            //myBitmapImage.EndInit();
            //DataGrid.Source = myBitmapImage;
            //PrintDialog myPrintDialog = new PrintDialog();

            //if (myPrintDialog.ShowDialog() == true)
            //{
            //    //myPrintDialog.PrintVisual(myImage, "Image Print");
            //}

            //PrinterSettings printerSettings = new PrinterSettings();
            //var printer = printerSettings.PrinterName;
            //printerSettings.DefaultPageSettings.Landscape = false;

            //Process p = new Process();
            //p.StartInfo = new ProcessStartInfo()
            //{
            //    CreateNoWindow = true,
            //    Verb = "print",
            //    FileName = filePath //put the correct path here
            //};
            //p.Start();

            //streamToPrint = new StreamReader(filePath);
            //try
            //{
            //    printFont = new Font("Arial", 10);
            //    PrintDocument pd = new PrintDocument();
            //    pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
            //    pd.PrinterSettings.PrinterName = printer;
            //    // Set the page orientation to landscape.
            //    pd.DefaultPageSettings.Landscape = true;
            //    pd.Print();
            //}
            //finally
            //{
            //    streamToPrint.Close();
            //}


            //var file = File.ReadAllBytes(filePath);
            //var printQueue = LocalPrintServer.GetDefaultPrintQueue();

            //using (var job = printQueue.AddJob())
            //using (var stream = job.JobStream)
            //{
            //    stream.Write(file, 0, file.Length);
            //}

            //using (System.Windows.Forms.PrintDialog Dialog = new System.Windows.Forms.PrintDialog())
            //{
            //    Dialog.ShowDialog();

            //    ProcessStartInfo printProcessInfo = new ProcessStartInfo()
            //    {
            //        Verb = "print",
            //        CreateNoWindow = true,
            //        FileName = filePath,
            //        WindowStyle = ProcessWindowStyle.Hidden
            //    };

            //    Process printProcess = new Process();
            //    printProcess.StartInfo = printProcessInfo;
            //    printProcess.Start();

            //    printProcess.WaitForInputIdle();

            //    Thread.Sleep(300);

            //    if (false == printProcess.CloseMainWindow())
            //    {
            //        printProcess.Kill();
            //    }
            //}

            //StreamReader streamToPrint = new StreamReader(filePath);

            //System.Drawing.Font printFont = new System.Drawing.Font("Arial", 10);
            //PrintDocument pd = new PrintDocument();
            ////pd.PrintPage += (object sender1, PrintPageEventArgs ev) =>
            ////{
            ////    float linesPerPage = 0;
            ////    float yPos = 0;
            ////    int count = 0;
            ////    float leftMargin = ev.MarginBounds.Left;
            ////    float topMargin = ev.MarginBounds.Top;
            ////    string line = null;

            ////    // Calculate the number of lines per page.
            ////    linesPerPage = ev.MarginBounds.Height /
            ////       printFont.GetHeight(ev.Graphics);

            ////    // Print each line of the file.
            ////    while (count < linesPerPage &&
            ////       ((line = streamToPrint.ReadLine()) != null))
            ////    {
            ////        yPos = topMargin + (count *
            ////           printFont.GetHeight(ev.Graphics));
            ////        ev.Graphics.DrawString(line, printFont, System.Drawing.Brushes.Black,
            ////           leftMargin, yPos, new System.Drawing.StringFormat());
            ////        count++;
            ////    }

            ////    // If more lines exist, print another page.
            ////    if (line != null)
            ////        ev.HasMorePages = true;
            ////    else
            ////        ev.HasMorePages = false;
            ////};


            //pd.Print();

            //streamToPrint.Close();

            //PrinterSettings printerSettings = new PrinterSettings();
            //var printer = printerSettings.PrinterName;
            //printerSettings.DefaultPageSettings.Landscape = true;

            //RawPrint.Printer.PrintFile(printer, filePath, $"{dat}print");


            //var p = @"C:\Users\iadmin\Downloads\test.pdf";
            //RawPrinterHelper.SendFileToPrinter(p, printer);


            //PrintDialog printDialog = new PrintDialog();
            //printDialog.PageRangeSelection = PageRangeSelection.AllPages;
            //printDialog.UserPageRangeEnabled = true;
            //bool? doPrint = printDialog.ShowDialog();
            //if (doPrint != true)
            //{
            //    return;
            //}

            ////open the pdf file
            //FixedDocument fixedDocument;
            //using (FileStream pdfFile = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            //{
            //    Document document = new Document(pdfFile);
            //    RenderSettings renderSettings = new RenderSettings();
            //    ConvertToWpfOptions renderOptions = new ConvertToWpfOptions { ConvertToImages = false };
            //    renderSettings.RenderPurpose = RenderPurpose.Print;
            //    renderSettings.ColorSettings.TransformationMode = ColorTransformationMode.HighQuality;
            //    //convert the pdf with the rendersettings and options to a fixed-size document which can be printed
            //    fixedDocument = document.ConvertToWpf(renderSettings, renderOptions);
            //}
            //printDialog.PrintDocument(fixedDocument.DocumentPaginator, "Print");

            //PrintDialog printDialog = new PrintDialog();

            //printDialog.PrintQueue = LocalPrintServer.GetDefaultPrintQueue();

            //printDialog.PrintTicket = printDialog.PrintQueue.DefaultPrintTicket;

            ////----</ Get_Print_Dialog_and_Printer >----



            ////*set the default page orientation based on the desired output.

            //printDialog.PrintTicket.PageOrientation = PageOrientation.Landscape;

            //printDialog.PrintTicket.PageScalingFactor = 90;

            //printDialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);

            ////printDialog.PrintableAreaHeight ; //*get

            ////printDialog.PrintableAreaWidth;   //get

            ////printDialog.PrintDocument.

            //printDialog.PrintTicket.PageBorderless = PageBorderless.None;

            //string PrintFileName = "testPrint";
            //if (File.Exists(PrintFileName)) File.Delete(PrintFileName);
            //XpsDocument xpsDocument = new XpsDocument(PrintFileName, FileAccess.ReadWrite);
            //XpsDocumentWriter writer1 = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
            //SerializerWriterCollator serializerWriterCollator = writer1.CreateVisualsCollator();
            //serializerWriterCollator.BeginBatchWrite();
            //serializerWriterCollator.Write(MainGrid);
            //serializerWriterCollator.EndBatchWrite();
            //FixedDocumentSequence fixedDocumentSequence=xpsDocument.GetFixedDocumentSequence();

            //if (printDialog.ShowDialog() == true)

            //{

            //    //----< print >----   

            //    // set the print ticket for the document sequence and write it to the printer.

            //    fixedDocumentSequence.PrintTicket = printDialog.PrintTicket;



            //    //-< send_document_to_printer >-

            //    XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(printDialog.PrintQueue);

            //    writer.WriteAsync(fixedDocumentSequence, printDialog.PrintTicket);

            //    //-</ send_document_to_printer >-

            //    //----</ print >----   

            //}

            ////----------------</ Print_Document() >-----------------------

            //xpsDocument.Close();
            //writer1 = null;
            //serializerWriterCollator = null;
            //xpsDocument = null;



            var datagrid = new DataGrid();

            try
            {
                datagrid.CanUserAddRows = false;
                datagrid.AutoGenerateColumns = false;
                datagrid.Margin = new Thickness(10, 0, 10, 10);
                datagrid.IsReadOnly = true;
                datagrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                foreach (var colName in columnNames)
                {
                    datagrid.Columns.Add(new MaterialDesignThemes.Wpf.DataGridTextColumn
                    {
                        // bind to a dictionary property
                        Binding = new Binding(colName.Key),
                        Header = colName.Value,
                    });
                }

                //DataTable dt1 = new DataTable();
                //dt1 = CreateDynamicDataTable(group);
                string JSONString = JsonConvert.SerializeObject(dt);
                var lis = JsonConvert.DeserializeObject<List<dynamic>>(JSONString);
                lis = lis.Distinct().ToList();
                datagrid.ItemsSource = lis;
                datagrid.Items.Refresh();
                DynamicReportPanel.Children.Add(datagrid);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DynamicReport/CreateDynamicDataGrid/Exception:- " + ex.Message, ex);
            }


            DynamicReportPanel.Visibility = Visibility.Visible;

            PrintDialog printDialog = new PrintDialog();
            printDialog.PrintTicket.PageOrientation = PageOrientation.Landscape;
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(DynamicReportPanel, "invoice");
            }

            //using (System.Windows.Forms.PrintDialog Dialog = new System.Windows.Forms.PrintDialog())
            //{
            try
            {
                // Create the printer settings for our printer
                var printerSettings = new PrinterSettings
                {
                    //PrinterName = printer,
                    Copies = 1,

                };
                printerSettings.DefaultPageSettings.Landscape = true;
                // Create our page settings for the paper size selected
                var pageSettings = new PageSettings(printerSettings)
                {
                    Margins = new Margins(0, 0, 0, 0),
                };
                foreach (System.Drawing.Printing.PaperSize paperSize in printerSettings.PaperSizes)
                {
                    if (paperSize.PaperName == "A4")
                    {
                        pageSettings.PaperSize = paperSize;
                        break;
                    }
                }

                // Now print the PDF document
                using (var document = PdfiumViewer.PdfDocument.Load(filePath))
                {
                    using (var printDocument = document.CreatePrintDocument())
                    {
                        printDocument.DefaultPageSettings.Landscape = true;
                        //printDocument.DefaultPageSettings = pageSettings;
                        printDocument.PrintController = new StandardPrintController();
                        System.Windows.Forms.PrintDialog printdlg = new System.Windows.Forms.PrintDialog();
                        //System.Windows.Forms.PrintPreviewDialog printPrvDlg = new System.Windows.Forms.PrintPreviewDialog();
                        //printPrvDlg.Document = printDocument;
                        //printPrvDlg.ShowDialog();
                        printdlg.Document = printDocument;
                        if (printdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            printDocument.Print();
                        }

                    }
                }
                //return true;
            }
            catch
            {
                //return false;
            }
            //}

        }

        private void PrintAsPortrait_Click(object sender, RoutedEventArgs e)
        {
            if (!IsGroupByChecked)
            {
                if (PrintPDF(false))
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Printed successfully");
                }
            }
            else
            {
                if (PrintPDFS(false))
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Printed successfully");
                }
            }
        }

        private void PrintAsLandscape_Click(object sender, RoutedEventArgs e)
        {
            if (!IsGroupByChecked)
            {
                if (PrintPDF1(true))
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Printed successfully");
                }
            }
            else
            {
                if (PrintPDF2(true))
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Printed successfully");
                }
            }

        }


        public bool PrintPDF(bool IsLandscape)
        {
            try
            {
                var bytearray1 = this.ExportToPdf1(dt);
                var dat = DateTime.Now.ToString("ddMMyyyyhhmmss");
                var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{dat}print.pdf");
                var filePath1 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{dat}print1.pdf");
                File.WriteAllBytes(filePath, bytearray1);
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();

                PdfReader reader = new PdfReader(filePath);
                int pagesCount = reader.NumberOfPages;
                for (int n = 1; n <= pagesCount; n++)
                {
                    PdfDictionary page = reader.GetPageN(n);
                    PdfNumber rotate = page.GetAsNumber(PdfName.ROTATE);
                    page.Put(PdfName.ROTATE, new PdfNumber(IsLandscape ? 270 : 0));
                }

                FileStream fs = new FileStream(filePath1, FileMode.Create,
                FileAccess.Write, FileShare.None);
                PdfStamper stamper = new PdfStamper(reader, fs);
                stamper.Close();
                reader.Close();


                // we create a reader for a certain document
                //PdfReader reader = new PdfReader(filePath);
                //// we retrieve the total number of pages
                //int n = reader.NumberOfPages;
                //Console.WriteLine("There are " + n + " pages in the original file.");

                //// step 1: creation of a document-object
                //Document document1 = new Document(reader.GetPageSizeWithRotation(1));
                //// step 2: we create a writer that listens to the document
                //PdfWriter writer = PdfWriter.GetInstance(document1, new FileStream(filePath1, FileMode.Create));
                //// step 3: we open the document
                //document1.Open();
                //PdfContentByte cb = writer.DirectContent;
                //PdfImportedPage page;
                //int rotation;
                //// step 4: we add content
                ////while (f < args.Length)
                ////{
                //    int i = 0;
                //    while (i < n)
                //    {
                //        i++;
                //        document1.SetPageSize(reader.GetPageSizeWithRotation(i));
                //        document1.NewPage();
                //        page = writer.GetImportedPage(reader, i);
                //        rotation = reader.GetPageRotation(i);
                //        if (rotation == 90 || rotation == 270)
                //        {
                //            cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                //        }
                //        else
                //        {
                //            cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                //        }
                //        Console.WriteLine("Processed page " + i);
                //    }
                //    //f++;
                //    //if (f < args.Length)
                //    //{
                //    //    reader = new PdfReader(args[f]);
                //    //    // we retrieve the total number of pages
                //    //    n = reader.NumberOfPages;
                //    //    Console.WriteLine("There are " + n + " pages in the original file.");
                //    //}
                ////}
                //// step 5: we close the document
                //document1.Close();


                // Create the printer settings for our printer
                //var printerSettings = new PrinterSettings
                //{
                //    //PrinterName = printer,
                //    Copies = 1,

                //};
                //var pageSettings = new PageSettings(printerSettings)
                //{
                //    Margins = new Margins(0, 0, 0, 0),
                //};
                //foreach (System.Drawing.Printing.PaperSize paperSize in printerSettings.PaperSizes)
                //{
                //    if (paperSize.PaperName == "A4")
                //    {
                //        pageSettings.PaperSize = paperSize;
                //        break;
                //    }
                //}


                // Now print the PDF document
                using (var document = PdfiumViewer.PdfDocument.Load(filePath1))
                {
                    using (var printDocument = document.CreatePrintDocument())
                    {
                        printDocument.PrinterSettings.DefaultPageSettings.Landscape = IsLandscape;
                        printDocument.DefaultPageSettings.Landscape = IsLandscape;
                        //printDocument.DefaultPageSettings = pageSettings;
                        //printDocument.PrintController = new StandardPrintController();
                        System.Windows.Forms.PrintDialog printdlg = new System.Windows.Forms.PrintDialog();
                        //System.Windows.Forms.PrintPreviewDialog printPrvDlg = new System.Windows.Forms.PrintPreviewDialog();
                        //printPrvDlg.Document = printDocument;
                        //printPrvDlg.ShowDialog();
                        printdlg.Document = printDocument;
                        if (printdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            printDocument.Print();
                        }

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DynamicReport/PrintPDF/Exception:- " + ex.Message, ex);
                return false;
            }
        }

        public bool PrintPDFS(bool IsLandscape)
        {
            try
            {
                var bytearray1 = this.ExportToPdfs();
                var dat = DateTime.Now.ToString("ddMMyyyyhhmmss");
                var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{dat}print.pdf");
                var filePath1 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{dat}print1.pdf");
                File.WriteAllBytes(filePath, bytearray1);
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();

                PdfReader reader = new PdfReader(filePath);
                int pagesCount = reader.NumberOfPages;
                for (int n = 1; n <= pagesCount; n++)
                {
                    PdfDictionary page = reader.GetPageN(n);
                    PdfNumber rotate = page.GetAsNumber(PdfName.ROTATE);
                    page.Put(PdfName.ROTATE, new PdfNumber(IsLandscape ? 270 : 0));
                }

                FileStream fs = new FileStream(filePath1, FileMode.Create,
                FileAccess.Write, FileShare.None);
                PdfStamper stamper = new PdfStamper(reader, fs);
                stamper.Close();
                reader.Close();


                // we create a reader for a certain document
                //PdfReader reader = new PdfReader(filePath);
                //// we retrieve the total number of pages
                //int n = reader.NumberOfPages;
                //Console.WriteLine("There are " + n + " pages in the original file.");

                //// step 1: creation of a document-object
                //Document document1 = new Document(reader.GetPageSizeWithRotation(1));
                //// step 2: we create a writer that listens to the document
                //PdfWriter writer = PdfWriter.GetInstance(document1, new FileStream(filePath1, FileMode.Create));
                //// step 3: we open the document
                //document1.Open();
                //PdfContentByte cb = writer.DirectContent;
                //PdfImportedPage page;
                //int rotation;
                //// step 4: we add content
                ////while (f < args.Length)
                ////{
                //    int i = 0;
                //    while (i < n)
                //    {
                //        i++;
                //        document1.SetPageSize(reader.GetPageSizeWithRotation(i));
                //        document1.NewPage();
                //        page = writer.GetImportedPage(reader, i);
                //        rotation = reader.GetPageRotation(i);
                //        if (rotation == 90 || rotation == 270)
                //        {
                //            cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                //        }
                //        else
                //        {
                //            cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                //        }
                //        Console.WriteLine("Processed page " + i);
                //    }
                //    //f++;
                //    //if (f < args.Length)
                //    //{
                //    //    reader = new PdfReader(args[f]);
                //    //    // we retrieve the total number of pages
                //    //    n = reader.NumberOfPages;
                //    //    Console.WriteLine("There are " + n + " pages in the original file.");
                //    //}
                ////}
                //// step 5: we close the document
                //document1.Close();


                // Create the printer settings for our printer
                //var printerSettings = new PrinterSettings
                //{
                //    //PrinterName = printer,
                //    Copies = 1,

                //};
                //var pageSettings = new PageSettings(printerSettings)
                //{
                //    Margins = new Margins(0, 0, 0, 0),
                //};
                //foreach (System.Drawing.Printing.PaperSize paperSize in printerSettings.PaperSizes)
                //{
                //    if (paperSize.PaperName == "A4")
                //    {
                //        pageSettings.PaperSize = paperSize;
                //        break;
                //    }
                //}


                // Now print the PDF document
                using (var document = PdfiumViewer.PdfDocument.Load(filePath1))
                {
                    using (var printDocument = document.CreatePrintDocument())
                    {
                        printDocument.PrinterSettings.DefaultPageSettings.Landscape = IsLandscape;
                        printDocument.DefaultPageSettings.Landscape = IsLandscape;
                        //printDocument.DefaultPageSettings = pageSettings;
                        //printDocument.PrintController = new StandardPrintController();
                        System.Windows.Forms.PrintDialog printdlg = new System.Windows.Forms.PrintDialog();
                        //System.Windows.Forms.PrintPreviewDialog printPrvDlg = new System.Windows.Forms.PrintPreviewDialog();
                        //printPrvDlg.Document = printDocument;
                        //printPrvDlg.ShowDialog();
                        printdlg.Document = printDocument;
                        if (printdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            printDocument.Print();
                        }

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DynamicReport/PrintPDF/Exception:- " + ex.Message, ex);
                return false;
            }
        }

        public bool PrintPDF1(bool IsLandscape)
        {
            try
            {
                var datagrid = new DataGrid();
                DynamicReportPanel.Children.Clear();
                try
                {
                    datagrid.CanUserAddRows = false;
                    datagrid.AutoGenerateColumns = false;
                    datagrid.Margin = new Thickness(10, 0, 10, 10);
                    datagrid.IsReadOnly = true;
                    datagrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    foreach (var colName in columnNames)
                    {
                        datagrid.Columns.Add(new MaterialDesignThemes.Wpf.DataGridTextColumn
                        {
                            // bind to a dictionary property
                            Binding = new Binding(colName.Key),
                            Header = colName.Value,
                        });
                    }

                    //DataTable dt1 = new DataTable();
                    //dt1 = CreateDynamicDataTable(group);

                    DataTable dt1 = new DataTable();

                    foreach (var colName in columnNames)
                    {
                        if (dt.Columns[colName.Key].DataType == typeof(DateTime) || dt.Columns[colName.Key].DataType == typeof(DateTime?))
                        {
                            dt1.Columns.Add(colName.Key);
                        }
                        else if (dt.Columns[colName.Key].DataType == typeof(Boolean) || dt.Columns[colName.Key].DataType == typeof(Boolean?))
                        {
                            dt1.Columns.Add(colName.Key);
                        }
                        else if (dt.Columns[colName.Key].DataType == typeof(Byte[]))
                        {
                            //roww[dt.Columns[i].ColumnName] = DateTime.Now;
                            dt1.Columns.Add(colName.Key);
                        }
                        else
                            dt1.Columns.Add(colName.Key, dt.Columns[colName.Key].DataType);
                    }
                    foreach (var d in dt.Rows)
                    {
                        var y = d as DataRow;
                        ////dt1.Rows.Add(y.ItemArray);
                        dt1.ImportRow(y);
                    }



                    DataRow row = dt1.NewRow();
                    for (int i = 0; i < dt1.Columns.Count; i++)
                    {
                        var type = dt1.Columns[i].DataType;
                        if ((type == typeof(Double) || type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64)) && dt1.Columns[i].ColumnName != "TicketNo")
                        {
                            row[dt1.Columns[i].ColumnName] = dt.Compute($"Sum({dt1.Columns[i].ColumnName})", "");
                        }
                        else
                        {
                            if (dt1.Columns[i].ColumnName == "TicketNo")
                            {
                                row[dt1.Columns[i].ColumnName] = 0;
                            }
                            else
                            {
                                //row[dt.Columns[i].ColumnName] = "";
                                if (type == typeof(DateTime) || type == typeof(DateTime?))
                                {
                                    //row[dt.Columns[i].ColumnName] = DateTime.Now;
                                    row[dt1.Columns[i].ColumnName] = "";
                                }
                                //else
                                //{
                                //    row[dt1.Columns[i].ColumnName] = "";
                                //}
                                else if (type == typeof(Boolean) || type == typeof(Boolean?))
                                {
                                    //row[dt.Columns[i].ColumnName] = DateTime.Now;
                                    row[dt1.Columns[i].ColumnName] = "";
                                }
                                else if (type == typeof(Byte[]))
                                {
                                    //row[dt.Columns[i].ColumnName] = DateTime.Now;
                                    row[dt1.Columns[i].ColumnName] = "";
                                }
                                else
                                {
                                    row[dt1.Columns[i].ColumnName] = "";
                                }
                            }
                        }
                    }
                    //dt1.Rows.Add(row);

                    string JSONString = JsonConvert.SerializeObject(dt1);
                    var lis = JsonConvert.DeserializeObject<List<dynamic>>(JSONString);
                    lis = lis.Distinct().ToList();
                    datagrid.ItemsSource = lis;
                    datagrid.Items.Refresh();
                    datagrid.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    DynamicReportPanel.Children.Add(datagrid);
                }
                catch (Exception ex)
                {
                    WriteLog.WriteToFile("DynamicReport/CreateDynamicDataGrid/Exception:- " + ex.Message, ex);
                }

                ScrollViewer1.ScrollToHome();
                ScrollViewer1.ScrollToLeftEnd();

                CompanyGrid1.Visibility = Visibility.Collapsed;
                Separator1.Visibility = Visibility.Collapsed;
                ReportDataGrid.Visibility = Visibility.Collapsed;
                //PaginatorPanel.Visibility = Visibility.Collapsed;
                //ActionPanel.Visibility = Visibility.Collapsed;

                CompanyGrid2.Visibility = Visibility.Visible;
                Separator2.Visibility = Visibility.Collapsed;
                Footer2.Visibility = Visibility.Collapsed;
                FooterTextBlock2.Visibility = Visibility.Visible;
                DynamicReportPanel.Visibility = Visibility.Visible;


                PrintDialog printDialog = new PrintDialog();
                printDialog.PrintTicket.PageOrientation = IsLandscape ? PageOrientation.Landscape : PageOrientation.Portrait;
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(ReportPanel2, "report");

                    CompanyGrid1.Visibility = Visibility.Visible;
                    Separator1.Visibility = Visibility.Visible;
                    ReportDataGrid.Visibility = Visibility.Visible;
                    //PaginatorPanel.Visibility = Visibility.Visible;
                    //ActionPanel.Visibility = Visibility.Visible;

                    CompanyGrid2.Visibility = Visibility.Collapsed;
                    Separator2.Visibility = Visibility.Collapsed;
                    Footer2.Visibility = Visibility.Collapsed;
                    FooterTextBlock2.Visibility = Visibility.Collapsed;
                    DynamicReportPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    CompanyGrid1.Visibility = Visibility.Visible;
                    Separator1.Visibility = Visibility.Visible;
                    ReportDataGrid.Visibility = Visibility.Visible;
                    //PaginatorPanel.Visibility = Visibility.Visible;
                    //ActionPanel.Visibility = Visibility.Visible;

                    CompanyGrid2.Visibility = Visibility.Collapsed;
                    Separator2.Visibility = Visibility.Collapsed;
                    Footer2.Visibility = Visibility.Collapsed;
                    FooterTextBlock2.Visibility = Visibility.Collapsed;
                    DynamicReportPanel.Visibility = Visibility.Collapsed;
                }

                //if (dt.Rows.Count > 0)
                //{
                //    dt.Rows.RemoveAt(dt.Rows.Count - 1);
                //}

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool PrintPDF2(bool IsLandscape)
        {
            try
            {
                DynamicReportPanel.Children.Clear();
                CreateDynamicGroups(dt, IsLandscape);
                //var datagrid = new DataGrid();
                //DynamicReportPanel.Children.Clear();
                //try
                //{
                //    datagrid.CanUserAddRows = false;
                //    datagrid.AutoGenerateColumns = false;
                //    datagrid.Margin = new Thickness(10, 0, 10, 10);
                //    datagrid.IsReadOnly = true;
                //    datagrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                //    foreach (var colName in columnNames)
                //    {
                //        datagrid.Columns.Add(new MaterialDesignThemes.Wpf.DataGridTextColumn
                //        {
                //            // bind to a dictionary property
                //            Binding = new Binding(colName.Key),
                //            Header = colName.Value,
                //        });
                //    }

                //    //DataTable dt1 = new DataTable();
                //    //dt1 = CreateDynamicDataTable(group);
                //    string JSONString = JsonConvert.SerializeObject(dt);
                //    var lis = JsonConvert.DeserializeObject<List<dynamic>>(JSONString);
                //    lis = lis.Distinct().ToList();
                //    datagrid.ItemsSource = lis;
                //    datagrid.Items.Refresh();
                //    datagrid.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                //    DynamicReportPanel.Children.Add(datagrid);
                //}
                //catch (Exception ex)
                //{
                //    WriteLog.WriteToFile("DynamicReport/CreateDynamicDataGrid/Exception:- " + ex.Message, ex);
                //}

                ScrollViewer1.ScrollToHome();
                ScrollViewer1.ScrollToLeftEnd();

                CompanyGrid1.Visibility = Visibility.Collapsed;
                Separator1.Visibility = Visibility.Collapsed;
                ReportDataGrid.Visibility = Visibility.Collapsed;
                //PaginatorPanel.Visibility = Visibility.Collapsed;
                //ActionPanel.Visibility = Visibility.Collapsed;

                CompanyGrid2.Visibility = Visibility.Visible;
                Separator2.Visibility = Visibility.Collapsed;
                Footer2.Visibility = Visibility.Collapsed;
                FooterTextBlock2.Visibility = Visibility.Visible;
                DynamicReportPanel.Visibility = Visibility.Visible;


                PrintDialog printDialog = new PrintDialog();
                printDialog.PrintTicket.PageOrientation = IsLandscape ? PageOrientation.Landscape : PageOrientation.Portrait;
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(ReportPanel2, "report");

                    CompanyGrid1.Visibility = Visibility.Visible;
                    Separator1.Visibility = Visibility.Visible;
                    ReportDataGrid.Visibility = Visibility.Visible;
                    //PaginatorPanel.Visibility = Visibility.Visible;
                    //ActionPanel.Visibility = Visibility.Visible;

                    CompanyGrid2.Visibility = Visibility.Collapsed;
                    Separator2.Visibility = Visibility.Collapsed;
                    Footer2.Visibility = Visibility.Collapsed;
                    FooterTextBlock2.Visibility = Visibility.Collapsed;
                    //DynamicReportPanel.Visibility = Visibility.Collapsed;
                    CreateDynamicGroups(dt, false);
                }
                else
                {
                    CompanyGrid1.Visibility = Visibility.Visible;
                    Separator1.Visibility = Visibility.Visible;
                    ReportDataGrid.Visibility = Visibility.Visible;
                    //PaginatorPanel.Visibility = Visibility.Visible;
                    //ActionPanel.Visibility = Visibility.Visible;

                    CompanyGrid2.Visibility = Visibility.Collapsed;
                    Separator2.Visibility = Visibility.Collapsed;
                    Footer2.Visibility = Visibility.Collapsed;
                    FooterTextBlock2.Visibility = Visibility.Collapsed;
                    //DynamicReportPanel.Visibility = Visibility.Collapsed;
                    CreateDynamicGroups(dt, false);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //private void PrintDocument_BeginPrint(object sender, PrintEventArgs e)
        //{
        //    e.
        //}

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
            NumberOfPages = (int)Math.Ceiling((double)Result.Count / SelectedRecord);
            NumberOfPages = NumberOfPages == 0 ? 1 : NumberOfPages;
            UpdateCollection(Result.Take(SelectedRecord));
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
            UpdateCollection(Result.Take(SelectedRecord));
            CurrentPage = 1;
            UpdateEnableStatus();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
            //RecordStartFrom = Result.Count - SelectedRecord * (NumberOfPages - (CurrentPage - 1));
            RecordStartFrom = SelectedRecord * (CurrentPage - 1);
            var RecordToShow = Result.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            UpdateEnableStatus();
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            RecordStartFrom = CurrentPage * SelectedRecord;
            var RecordToShow = Result.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            CurrentPage++;
            UpdateEnableStatus();

        }



        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            var reordsToSkip = SelectedRecord * (NumberOfPages - 1);
            UpdateCollection(Result.Skip(reordsToSkip));
            CurrentPage = NumberOfPages;
            UpdateEnableStatus();
        }
        private void SaveToBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFolderDialog2 = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                var result = openFolderDialog2.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    var destinationFolder = openFolderDialog2.SelectedPath;
                    string TempFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
                    CreateTempFolder();
                    var byteArray1 = ExportToPdf1(dt);
                    if (byteArray1 != null && byteArray1.Length > 0)
                    {
                        var FileName = $"{DateTime.Now.ToString("ddMMMyyyyhhmmss")}Report.pdf";
                        var FilePath = System.IO.Path.Combine(TempFolder, FileName);
                        var destinationPath = System.IO.Path.Combine(destinationFolder, FileName);
                        File.WriteAllBytes(FilePath, byteArray1);
                        File.Move(FilePath, destinationPath);
                        System.GC.Collect();
                        System.GC.WaitForPendingFinalizers();
                        if (File.Exists(FilePath))
                        {
                            File.Delete(FilePath);
                        }
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "File has been saved to the selected path");
                    }

                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DynamicReport/SaveToBtn_Click/Exception:- " + ex.Message, ex);
            }

        }


    }
    public class PageViewModel
    {
        Page Page { get; set; }
        PageOrientation? PageOrientation { get; set; }
    }
}
