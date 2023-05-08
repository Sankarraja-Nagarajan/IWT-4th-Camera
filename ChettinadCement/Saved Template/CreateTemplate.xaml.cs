using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
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
using DataGridTextColumn = System.Windows.Controls.DataGridTextColumn;

namespace IWT.Saved_Template
{
    /// <summary>
    /// Interaction logic for CreateTemplate.xaml
    /// </summary>
    public partial class CreateTemplate : UserControl
    {
        public string Tabledata;
        public string FieldData;
        public string combinedString = "";
        public string result;
        public string b;
        public bool check;
        List<string> list = new List<string>();
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        public static AdminDBCall adminDBCall = new AdminDBCall();
        public static MasterDBCall masterDBCall = new MasterDBCall();
        public static CommonFunction commonFunction = new CommonFunction();
        List<TableDetails> tableDetails = new List<TableDetails>();
        TableDetails SelectedTableDetails = new TableDetails();
        TableDetails WhereSelectedTableDetails = new TableDetails();
        List<TableColumnDetails> tableColumnDetails = new List<TableColumnDetails>();
        List<TableColumnDetails> WhereTableColumnDetails = new List<TableColumnDetails>();
        List<TableColumnDetails> SelectedFieldDetails = new List<TableColumnDetails>();
        List<WhereTableColumnDetails> WhereSelectedFieldDetails = new List<WhereTableColumnDetails>();
        TableColumnDetails SelectedTableColumnDetails = new TableColumnDetails();
        TableColumnDetails WhereSelectedTableColumnDetails = new TableColumnDetails();
        TableColumnDetails SelectedFiledDetail = new TableColumnDetails();
        WhereTableColumnDetails WhereSelectedFiledDetail = new WhereTableColumnDetails();


        SavedReportTemplate selectedSavedReportTemplate = new SavedReportTemplate();
        SavedReportTemplate selectedEditSavedReportTemplate = new SavedReportTemplate();
        List<SavedReportTemplateFields> savedReportTemplateFields = new List<SavedReportTemplateFields>();
        List<SavedReportTemplateWhereFields> savedReportTemplateWhereFields = new List<SavedReportTemplateWhereFields>();
        public CreateTemplate(SavedReportTemplate _selectedEditSavedReportTemplate = null)
        {
            InitializeComponent();
            toastViewModel = new ToastViewModel();
            Loaded += CreateTemplate_Loaded;
            Unloaded += CreateTemplate_Unloaded;
            this.selectedEditSavedReportTemplate = _selectedEditSavedReportTemplate;
        }


        private void CreateTemplate_Loaded(object sender, RoutedEventArgs e)
        {
            GetTableDetails();
            TemplateName.IsEnabled = true;
            TablesDataGrid.SelectionChanged += TablesDataGrid_SelectionChanged;
            ColumnsDataGrid.SelectionChanged += ColumnsDataGrid_SelectionChanged;
            SelectedFiledsDataGrid.SelectionChanged += SelectedFiledsDataGrid_SelectionChanged;
            WhereTablesDataGrid.SelectionChanged += WhereTablesDataGrid_SelectionChanged;
            WhereColumnsDataGrid.SelectionChanged += WhereColumnsDataGrid_SelectionChanged;
            WhereSelectedFiledsDataGrid.SelectionChanged += WhereSelectedFiledsDataGrid_SelectionChanged;
            CheckEditSavedReportTemplate();
        }

        private void CheckEditSavedReportTemplate()
        {
            if (this.selectedEditSavedReportTemplate != null)
            {
                SelectedTableDetails = new TableDetails();
                TemplateName.Text = selectedEditSavedReportTemplate.ReportName;
                TemplateName.IsEnabled = false;
                SelectedTableDetails.TableName = this.selectedEditSavedReportTemplate.TableName;
                GetSavedTemplateFields();
                WhereRadioButton.IsChecked = selectedEditSavedReportTemplate.WhereEnabled;

                if (this.selectedEditSavedReportTemplate.WhereEnabled)
                {
                    GetSavedTemplateWhereFields();
                }
            }
        }

        private void GetSavedTemplateFields()
        {
            try
            {
                string Query = $"SELECT * FROM Saved_ReportTemplateFields WHERE TemplateID={selectedEditSavedReportTemplate.TemplateID}";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                savedReportTemplateFields = JsonConvert.DeserializeObject<List<SavedReportTemplateFields>>(JSONString);

                GetTableColumnDetails();

                SelectedFieldDetails = new List<TableColumnDetails>();
                foreach (var sa in savedReportTemplateFields)
                {
                    TableColumnDetails tableColumnDetails = new TableColumnDetails();
                    tableColumnDetails.TableName = sa.TableName;
                    tableColumnDetails.ColumnName = sa.FieldName;
                    tableColumnDetails.DataType = sa.DataType;
                    SelectedFieldDetails.Add(tableColumnDetails);
                }

                SelectedFiledsDataGrid.ItemsSource = SelectedFieldDetails;
                SelectedFiledsDataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/GetSavedTemplateFields/Exception:- " + ex.Message, ex);
            }
        }

        private void GetSavedTemplateWhereFields()
        {
            try
            {
                string Query = $"SELECT * FROM Saved_ReportTemplateWhereFields WHERE TemplateID={selectedEditSavedReportTemplate.TemplateID}";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                savedReportTemplateWhereFields = JsonConvert.DeserializeObject<List<SavedReportTemplateWhereFields>>(JSONString);

                WhereSelectedFieldDetails = new List<WhereTableColumnDetails>();
                foreach (var wf in savedReportTemplateWhereFields)
                {
                    WhereTableColumnDetails tableColumnDetails = new WhereTableColumnDetails();
                    tableColumnDetails.TableName = wf.TableName;
                    tableColumnDetails.ColumnName = wf.FieldName;
                    tableColumnDetails.MatchedColumnName = wf.MatchedColumnName;
                    tableColumnDetails.DataType = wf.DataType;
                    WhereSelectedFieldDetails.Add(tableColumnDetails);

                    WhereSelectedTableDetails = new TableDetails();
                    WhereSelectedTableDetails.TableName = wf.TableName;
                }

                GetWhereTableColumnDetails();

                WhereSelectedFiledsDataGrid.ItemsSource = WhereSelectedFieldDetails;
                WhereSelectedFiledsDataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/GetSavedTemplateWhereFields/Exception:- " + ex.Message, ex);
            }
        }

        private void CreateTemplate_Unloaded(object sender, RoutedEventArgs e)
        {
            TablesDataGrid.SelectionChanged -= TablesDataGrid_SelectionChanged;
            ColumnsDataGrid.SelectionChanged -= ColumnsDataGrid_SelectionChanged;
            WhereTablesDataGrid.SelectionChanged -= WhereTablesDataGrid_SelectionChanged;
            WhereColumnsDataGrid.SelectionChanged -= WhereColumnsDataGrid_SelectionChanged;
        }

        private void TablesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedTableDetails = TablesDataGrid.SelectedItem as TableDetails;
            if (SelectedTableDetails != null)
            {
                if (WhereRadioButton.IsChecked.HasValue && WhereRadioButton.IsChecked.Value && SelectedTableDetails.TableName == "Transaction_Details")
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Transaction Details selection not applicable when where condition selected");
                }
                else
                {
                    GetTableColumnDetails();
                    //SetColumnsDataGrid();
                    SelectedFieldDetails = new List<TableColumnDetails>();
                    SelectedFiledsDataGrid.ItemsSource = SelectedFieldDetails;
                    SelectedFiledsDataGrid.Items.Refresh();
                }
            }
        }

        private void ColumnsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedTableColumnDetails = ColumnsDataGrid.SelectedItem as TableColumnDetails;
            if (SelectedTableColumnDetails != null)
            {
                if (SelectedFieldDetails.IndexOf(SelectedTableColumnDetails) < 0)
                {
                    SelectedFieldDetails.Add(SelectedTableColumnDetails);
                    SelectedFiledsDataGrid.ItemsSource = SelectedFieldDetails;
                    SelectedFiledsDataGrid.Items.Refresh();

                }
            }
        }
        private void SelectedFiledsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedFiledDetail = SelectedFiledsDataGrid.SelectedItem as TableColumnDetails;
        }



        private void WhereTablesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WhereSelectedTableDetails = WhereTablesDataGrid.SelectedItem as TableDetails;
            if (WhereSelectedTableDetails != null)
            {
                if (WhereRadioButton.IsChecked.HasValue && WhereRadioButton.IsChecked.Value && WhereSelectedTableDetails.TableName == "Transaction_Details")
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Transaction Details selection not applicable when where condition selected");
                }
                else
                {
                    GetWhereTableColumnDetails();

                }
            }
        }


        private void WhereColumnsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WhereSelectedTableColumnDetails = WhereColumnsDataGrid.SelectedItem as TableColumnDetails;
            if (WhereSelectedTableColumnDetails != null)
            {
                var matched = tableColumnDetails.Where(x => WhereTableColumnDetails.Any(y => x.ColumnName == y.ColumnName &&
                x.ColumnName.ToLower() != "createdon" && x.ColumnName.ToLower() != "modifiedon" && x.ColumnName.ToLower() != "isdeleted")).ToList();
                if (matched.Count <= 0)
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, $"No relationship found between {SelectedTableDetails.TableName} and {WhereSelectedTableDetails.TableName}");
                }
                else
                {
                    var c = new WhereTableColumnDetails();
                    c.TableName = WhereSelectedTableColumnDetails.TableName;
                    c.ColumnName = WhereSelectedTableColumnDetails.ColumnName;
                    c.DataType = WhereSelectedTableColumnDetails.DataType;
                    c.MatchedColumnName = matched.Select(x => x.ColumnName).FirstOrDefault();
                    if (WhereSelectedFieldDetails.IndexOf(c) < 0)
                    {
                        WhereSelectedFieldDetails.Add(c);
                        WhereSelectedFiledsDataGrid.ItemsSource = WhereSelectedFieldDetails;
                        WhereSelectedFiledsDataGrid.Items.Refresh();
                    }
                }



            }
        }

        private void WhereSelectedFiledsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WhereSelectedFiledDetail = WhereSelectedFiledsDataGrid.SelectedItem as WhereTableColumnDetails;
        }


        public void ShowGrids()
        {
            TablesGrid.Visibility = Visibility.Visible;
            ColumnsGrid.Visibility = Visibility.Visible;
            SelectedFiledsGrid.Visibility = Visibility.Visible;
            ColumnActionGrid.Visibility = Visibility.Visible;
            ColumnOrderGrid.Visibility = Visibility.Visible;
        }
        public void HideGrids()
        {
            TablesGrid.Visibility = Visibility.Collapsed;
            ColumnsGrid.Visibility = Visibility.Collapsed;
            SelectedFiledsGrid.Visibility = Visibility.Collapsed;
            ColumnActionGrid.Visibility = Visibility.Collapsed;
            ColumnOrderGrid.Visibility = Visibility.Collapsed;
        }

        public void ShowWhereGrids()
        {
            WhereTablesGrid.Visibility = Visibility.Visible;
            WhereColumnsGrid.Visibility = Visibility.Visible;
            WhereSelectedFiledsGrid.Visibility = Visibility.Visible;
            WhereColumnActionGrid.Visibility = Visibility.Visible;
            WhereColumnOrderGrid.Visibility = Visibility.Visible;
        }
        public void HideWhereGrids()
        {
            WhereTablesGrid.Visibility = Visibility.Collapsed;
            WhereColumnsGrid.Visibility = Visibility.Collapsed;
            WhereSelectedFiledsGrid.Visibility = Visibility.Collapsed;
            WhereColumnActionGrid.Visibility = Visibility.Collapsed;
            WhereColumnOrderGrid.Visibility = Visibility.Collapsed;

        }

        public void GetTableDetails()
        {
            try
            {
                tableDetails = commonFunction.GetTableDetails();
                tableDetails = tableDetails.OrderBy(t => t.TableName).ToList();
                TablesDataGrid.ItemsSource = tableDetails;
                TablesDataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateTemplate/GetTableDetails/Exception:- " + ex.Message, ex);
            }
        }
        public void GetTableColumnDetails()
        {
            try
            {
                if (SelectedTableDetails != null && !string.IsNullOrEmpty(SelectedTableDetails.TableName))
                {
                    tableColumnDetails = commonFunction.GetTableColumnDetails(SelectedTableDetails.TableName);
                    SetColumnsDataGrid();
                }

            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateTemplate/GetTableDetails/Exception:- " + ex.Message, ex);
            }
        }

        public void SetColumnsDataGrid()
        {
            if (SelectedTableDetails != null && !string.IsNullOrEmpty(SelectedTableDetails.TableName))
            {
                ColumnsDataGrid.ItemsSource = tableColumnDetails;
                ColumnsDataGrid.Items.Refresh();
            }
        }

        public void GetWhereTableColumnDetails()
        {
            try
            {
                if (WhereSelectedTableDetails != null && !string.IsNullOrEmpty(WhereSelectedTableDetails.TableName))
                {
                    WhereTableColumnDetails = commonFunction.GetTableColumnDetails(WhereSelectedTableDetails.TableName);

                    var matched = tableColumnDetails.Where(x => WhereTableColumnDetails.Any(y => x.ColumnName == y.ColumnName)).ToList();
                    if (matched.Count <= 0)
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, $"No relationship found between {SelectedTableDetails.TableName} and {WhereSelectedTableDetails.TableName}");
                    }
                    else
                    {
                        SetWhereColumnsDataGrid();
                    }
                }

            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateTemplate/GetWhereTableColumnDetails/Exception:- " + ex.Message, ex);
            }
        }

        public void SetWhereColumnsDataGrid()
        {
            if (WhereSelectedTableDetails != null && !string.IsNullOrEmpty(WhereSelectedTableDetails.TableName))
            {
                WhereColumnsDataGrid.ItemsSource = WhereTableColumnDetails;
                WhereColumnsDataGrid.Items.Refresh();
            }
        }



        // #start region
        private void dataGrid1_Loaded(object sender, RoutedEventArgs e)
        {
            //SqlConnection cn = new SqlConnection(@"Data Source=192.168.0.28,1434;Initial Catalog=IWT;Persist Security Info=True;User ID=essae;Password=essae@123");

            //string queryString = "select '[' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']' FROM IWT.INFORMATION_SCHEMA.TABLES";
            //cn.Open();

            //DataTable table = new DataTable();
            //SqlDataAdapter a = new SqlDataAdapter(queryString, cn);
            //a.Fill(table);
            //datagird1.DataContext = table.DefaultView;
            ////  datagird1.ItemsSource = table.DefaultView;
        }

        private void WhereCheckbox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void WhereRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (WhereRadioButton.IsChecked.HasValue && WhereRadioButton.IsChecked.Value)
            {
                SaveBtn.Visibility = Visibility.Collapsed;
                NextBtn.Visibility = Visibility.Visible;
            }
            else
            {
                SaveBtn.Visibility = Visibility.Visible;
                NextBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var tempName = TemplateName.Text;
            if (!string.IsNullOrEmpty(tempName))
            {


                if (SelectedFieldDetails != null && SelectedFieldDetails.Count > 0)
                {

                    if (WhereRadioButton.IsChecked.HasValue && WhereRadioButton.IsChecked.Value)
                    {
                        if (WhereSelectedFieldDetails != null && WhereSelectedFieldDetails.Count > 0)
                        {
                            selectedSavedReportTemplate = new SavedReportTemplate();
                            savedReportTemplateFields = new List<SavedReportTemplateFields>();
                            savedReportTemplateWhereFields = new List<SavedReportTemplateWhereFields>();

                            selectedSavedReportTemplate.ReportName = TemplateName.Text;
                            selectedSavedReportTemplate.TableName = SelectedTableDetails.TableName;
                            selectedSavedReportTemplate.WhereEnabled = true;
                            selectedSavedReportTemplate.Query = BuidWhereQuery(true);

                            SavedReportTemplate result = null;
                            if (selectedEditSavedReportTemplate != null && selectedEditSavedReportTemplate.TemplateID != 0)
                            {
                                selectedSavedReportTemplate.TemplateID = selectedEditSavedReportTemplate.TemplateID;
                                result = commonFunction.UpdateSavedReportTemplate(selectedSavedReportTemplate);
                            }
                            else
                            {
                                result = commonFunction.CreateSavedReportTemplate(selectedSavedReportTemplate);
                            }

                            if (result != null)
                            {
                                var result1 = commonFunction.CreateSavedReportTemplateFields(savedReportTemplateFields, result);

                                if (result1)
                                {
                                    var result2 = commonFunction.CreateSavedReportTemplateWhereFields(savedReportTemplateWhereFields, result);
                                    if (result2)
                                    {
                                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Template saved successfully");
                                        DialogHost.CloseDialogCommand.Execute(true, null);
                                    }
                                }
                            }
                        }
                        else
                        {
                            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select fields for the where condition");
                        }
                    }
                    else
                    {
                        selectedSavedReportTemplate = new SavedReportTemplate();
                        savedReportTemplateFields = new List<SavedReportTemplateFields>();
                        selectedSavedReportTemplate.ReportName = TemplateName.Text;
                        selectedSavedReportTemplate.TableName = SelectedTableDetails.TableName;
                        if (WhereRadioButton.IsChecked.HasValue && WhereRadioButton.IsChecked.Value)
                        {
                            selectedSavedReportTemplate.WhereEnabled = true;
                            selectedSavedReportTemplate.Query = BuidQuery(true);
                            //var result = commonFunction.CreateSavedReportTemplate(selectedSavedReportTemplate);
                            SavedReportTemplate result = null;
                            if (selectedEditSavedReportTemplate != null && selectedEditSavedReportTemplate.TemplateID != 0)
                            {
                                selectedSavedReportTemplate.TemplateID = selectedEditSavedReportTemplate.TemplateID;
                                result = commonFunction.UpdateSavedReportTemplate(selectedSavedReportTemplate);
                            }
                            else
                            {
                                result = commonFunction.CreateSavedReportTemplate(selectedSavedReportTemplate);
                            }
                            if (result != null)
                            {
                                var result1 = commonFunction.CreateSavedReportTemplateFields(savedReportTemplateFields, result);
                                if (result1)
                                {
                                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Template saved successfully");
                                    DialogHost.CloseDialogCommand.Execute(true, null);

                                }
                            }
                        }
                        else
                        {
                            selectedSavedReportTemplate.WhereEnabled = false;
                            selectedSavedReportTemplate.Query = BuidQuery(false);
                            //var result = commonFunction.CreateSavedReportTemplate(selectedSavedReportTemplate);
                            SavedReportTemplate result = null;
                            if (selectedEditSavedReportTemplate != null && selectedEditSavedReportTemplate.TemplateID != 0)
                            {
                                selectedSavedReportTemplate.TemplateID = selectedEditSavedReportTemplate.TemplateID;
                                result = commonFunction.UpdateSavedReportTemplate(selectedSavedReportTemplate);
                            }
                            else
                            {
                                result = commonFunction.CreateSavedReportTemplate(selectedSavedReportTemplate);
                            }
                            if (result != null)
                            {
                                var result1 = commonFunction.CreateSavedReportTemplateFields(savedReportTemplateFields, result);
                                if (result1)
                                {
                                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Template saved successfully");
                                    DialogHost.CloseDialogCommand.Execute(true, null);

                                }
                            }
                        }
                    }

                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select fields for the template");
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill out template name");
            }
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            var tempName = TemplateName.Text;
            if (!string.IsNullOrEmpty(tempName))
            {
                if (SelectedFieldDetails != null && SelectedFieldDetails.Count > 0)
                {
                    NextBtn.Visibility = Visibility.Collapsed;
                    SaveBtn.Visibility = Visibility.Visible;
                    WhereTablesDataGrid.ItemsSource = tableDetails;
                    WhereTablesDataGrid.Items.Refresh();
                    HideGrids();
                    ShowWhereGrids();

                    //SelectedFieldsLabel.Content = "Selected Where Fields";

                    //selectedSavedReportTemplate = new SavedReportTemplate();
                    //savedReportTemplateFields = new List<SavedReportTemplateFields>();
                    //selectedSavedReportTemplate.ReportName = TemplateName.Text;
                    //if (WhereRadioButton.IsChecked.HasValue && WhereRadioButton.IsChecked.Value)
                    //{
                    //    selectedSavedReportTemplate.WhereEnabled = true;
                    //    selectedSavedReportTemplate.Query = BuidQuery(true);
                    //}
                    //else
                    //{
                    //    selectedSavedReportTemplate.WhereEnabled = false;
                    //    selectedSavedReportTemplate.Query = BuidQuery(false);
                    //    var result = commonFunction.CreateSavedReportTemplate(selectedSavedReportTemplate);
                    //    if (result != null)
                    //    {
                    //        var result1 = commonFunction.CreateSavedReportTemplateFields(savedReportTemplateFields, result);
                    //        if (result1)
                    //        {
                    //            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Template saved successfully");
                    //        }
                    //    }
                    //}
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select fields for the template");
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill out template name");
            }
        }



        public string BuidQuery(bool IsWhereEnabled)
        {
            string Query = "SELECT";
            var SelectedFieldsQuery = BuildSelectedFieldsQuery();
            if (!string.IsNullOrEmpty(SelectedFieldsQuery))
            {
                Query = Query + SelectedFieldsQuery;
                Query = Query + $" FROM [{SelectedTableDetails?.TableName}]";
            }
            else
            {
                Query = "";
            }

            return Query;
        }



        public string BuildSelectedFieldsQuery()
        {
            string Query = "";

            try
            {
                foreach (var fd in SelectedFieldDetails)
                {
                    SavedReportTemplateFields templateFields = new SavedReportTemplateFields();
                    templateFields.FieldName = fd.ColumnName;
                    templateFields.DataType = fd.DataType;
                    templateFields.TableName = SelectedTableDetails?.TableName;
                    savedReportTemplateFields.Add(templateFields);
                    Query = Query + $" [{fd.ColumnName}],";
                }
                Query = Query.Length > 0 ? Query.Remove(Query.Length - 1) : Query;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateTemplate/BuildSelectedFieldQuery/Exception:- " + ex.Message, ex);
            }
            return Query;
        }



        public string BuidWhereQuery(bool IsWhereEnabled)
        {
            string Query = "SELECT";

            var GroupedFields = WhereSelectedFieldDetails.GroupBy(x => x.TableName);

            if (GroupedFields != null && GroupedFields.Count() > 0)
            {
                if (GroupedFields.Count() == 1 && SelectedTableDetails?.TableName == GroupedFields.Select(x => x.Key).FirstOrDefault())
                {
                    var SelectedFieldsQuery = BuildSelectedFieldsQuery1();
                    if (!string.IsNullOrEmpty(SelectedFieldsQuery))
                    {
                        Query = Query + SelectedFieldsQuery;
                        Query = Query + $" FROM [{SelectedTableDetails?.TableName}]";
                    }
                    else
                    {
                        Query = "";
                    }
                }
                else
                {
                    var SelectedWhereFieldsQuery = BuildWhereSelectedFieldsQuery();
                    if (!string.IsNullOrEmpty(SelectedWhereFieldsQuery))
                    {
                        Query = Query + SelectedWhereFieldsQuery;
                        Query = Query + $" FROM [{SelectedTableDetails?.TableName}]";
                        foreach (var group in GroupedFields)
                        {
                            var tableName = group.Key;
                            var commonColumn = group.Select(y => y.MatchedColumnName).FirstOrDefault();
                            if (SelectedTableDetails?.TableName == tableName)
                            {
                                Query = Query + $" JOIN [{tableName}] AS A ON [{SelectedTableDetails?.TableName}].[{commonColumn}]=A.[{commonColumn}]";

                            }
                            else
                            {
                                Query = Query + $" JOIN [{tableName}] ON [{SelectedTableDetails?.TableName}].[{commonColumn}]=[{tableName}].[{commonColumn}]";
                            }
                        }
                    }
                    else
                    {
                        Query = "";
                    }
                }
            }
            else
            {
                var SelectedFieldsQuery = BuildSelectedFieldsQuery();
                if (!string.IsNullOrEmpty(SelectedFieldsQuery))
                {
                    Query = Query + SelectedFieldsQuery;
                    Query = Query + $" FROM [{SelectedTableDetails?.TableName}]";

                }
                else
                {
                    Query = "";
                }
            }


            return Query;
        }

        public string BuildSelectedFieldsQuery1()
        {
            var GroupedFields = WhereSelectedFieldDetails.GroupBy(x => x.TableName);
            string Query = "";

            try
            {
                foreach (var fd in SelectedFieldDetails)
                {
                    SavedReportTemplateFields templateFields = new SavedReportTemplateFields();
                    templateFields.FieldName = fd.ColumnName;
                    templateFields.DataType = fd.DataType;
                    templateFields.TableName = SelectedTableDetails?.TableName;
                    savedReportTemplateFields.Add(templateFields);

                    Query = Query + $" [{fd.ColumnName}],";
                }

                foreach (var group in GroupedFields)
                {
                    var tableName = group.Key;
                    //var commonColumn = group.Select(y => y.MatchedColumnName).FirstOrDefault();

                    foreach (var col in group)
                    {
                        //SavedReportTemplateFields templateFields = new SavedReportTemplateFields();
                        //templateFields.FieldName = col.ColumnName;
                        //templateFields.TableName = col.TableName;
                        //savedReportTemplateFields.Add(templateFields);
                        //Query = Query + $"[{col.TableName}].[{col.ColumnName}],";

                        SavedReportTemplateWhereFields whereFields = new SavedReportTemplateWhereFields();
                        whereFields.TableName = col.TableName;
                        whereFields.FieldName = col.ColumnName;
                        whereFields.DataType = col.DataType;
                        whereFields.MatchedColumnName = col.MatchedColumnName;
                        savedReportTemplateWhereFields.Add(whereFields);
                    }
                }

                Query = Query.Length > 0 ? Query.Remove(Query.Length - 1) : Query;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateTemplate/BuildSelectedFieldsQuery1/Exception:- " + ex.Message, ex);
            }
            return Query;
        }

        public string BuildWhereSelectedFieldsQuery()
        {
            var GroupedFields = WhereSelectedFieldDetails.GroupBy(x => x.TableName);
            string Query = "";

            try
            {
                foreach (var fd in SelectedFieldDetails)
                {
                    SavedReportTemplateFields templateFields = new SavedReportTemplateFields();
                    templateFields.FieldName = fd.ColumnName;
                    templateFields.DataType = fd.DataType;
                    templateFields.TableName = SelectedTableDetails?.TableName;
                    savedReportTemplateFields.Add(templateFields);

                    Query = Query + $" [{SelectedTableDetails?.TableName}].[{fd.ColumnName}],";
                }

                foreach (var group in GroupedFields)
                {
                    var tableName = group.Key;
                    //var commonColumn = group.Select(y => y.MatchedColumnName).FirstOrDefault();

                    foreach (var col in group)
                    {
                        //SavedReportTemplateFields templateFields = new SavedReportTemplateFields();
                        //templateFields.FieldName = col.ColumnName;
                        //templateFields.TableName = col.TableName;
                        //savedReportTemplateFields.Add(templateFields);
                        //Query = Query + $"[{col.TableName}].[{col.ColumnName}],";

                        SavedReportTemplateWhereFields whereFields = new SavedReportTemplateWhereFields();
                        whereFields.TableName = col.TableName;
                        whereFields.FieldName = col.ColumnName;
                        whereFields.DataType = col.DataType;
                        whereFields.MatchedColumnName = col.MatchedColumnName;
                        savedReportTemplateWhereFields.Add(whereFields);
                    }
                }

                Query = Query.Length > 0 ? Query.Remove(Query.Length - 1) : Query;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateTemplate/BuildWhereSelectedFieldsQuery/Exception:- " + ex.Message, ex);
            }
            return Query;
        }


        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }


        public void ShowMessage(Action<string> message, string name)
        {
            this.Dispatcher.Invoke(() =>
            {
                LastMessage = $"{name}";
                message(LastMessage);
            });
        }
        private void ColumnSelectAll_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (tableColumnDetails != null && tableColumnDetails.Count > 0)
            {
                foreach (var tbc in tableColumnDetails)
                {
                    if (SelectedFieldDetails.IndexOf(tbc) < 0)
                    {
                        SelectedFieldDetails.Add(tbc);
                    }
                    SelectedFiledsDataGrid.ItemsSource = SelectedFieldDetails;
                    SelectedFiledsDataGrid.Items.Refresh();
                }

            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No column found to select");
            }
        }

        private void ColumnRemoveAll_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectedFieldDetails.Clear();
            SelectedFiledsDataGrid.ItemsSource = SelectedFieldDetails;
            SelectedFiledsDataGrid.Items.Refresh();
        }

        private void ColumnRemove_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedFiledDetail != null)
            {
                SelectedFieldDetails.Remove(SelectedFiledDetail);
                SelectedFiledsDataGrid.ItemsSource = SelectedFieldDetails;
                SelectedFiledsDataGrid.Items.Refresh();
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No column selected to remove");
            }
        }
        private void ColumnUp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedFiledDetail != null)
            {
                var index = SelectedFieldDetails.IndexOf(SelectedFiledDetail);
                if (index >= 0)
                {
                    if (index == 0)
                    {

                    }
                    else
                    {
                        var fElement = SelectedFieldDetails[index - 1];
                        SelectedFieldDetails[index - 1] = SelectedFiledDetail;
                        SelectedFieldDetails[index] = fElement;
                    }
                }

                SelectedFiledsDataGrid.ItemsSource = SelectedFieldDetails;
                SelectedFiledsDataGrid.Items.Refresh();
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No column selected to move the order");
            }
        }

        private void ColumnDown_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedFiledDetail != null)
            {
                var index = SelectedFieldDetails.IndexOf(SelectedFiledDetail);
                if (index >= 0)
                {
                    if (index == SelectedFieldDetails.Count - 1)
                    {

                    }
                    else
                    {
                        var nElement = SelectedFieldDetails[index + 1];
                        SelectedFieldDetails[index + 1] = SelectedFiledDetail;
                        SelectedFieldDetails[index] = nElement;
                    }
                }
                SelectedFiledsDataGrid.ItemsSource = SelectedFieldDetails;
                SelectedFiledsDataGrid.Items.Refresh();
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No column selected to move the order");
            }
        }

        private void WhereColumnSelectAll_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (WhereTableColumnDetails != null && WhereTableColumnDetails.Count > 0)
            {
                var matched = tableColumnDetails.Where(x => WhereTableColumnDetails.Any(y => x.ColumnName == y.ColumnName &&
                             x.ColumnName.ToLower() != "createdon" && x.ColumnName.ToLower() != "modifiedon" && x.ColumnName.ToLower() != "isdeleted")).ToList();

                foreach (var tbc in WhereTableColumnDetails)
                {
                    //if (WhereSelectedFieldDetails.IndexOf(tbc) < 0)
                    //{
                    //    WhereSelectedFieldDetails.Add(tbc);
                    //}

                    var c = new WhereTableColumnDetails();
                    c.TableName = tbc.TableName;
                    c.ColumnName = tbc.ColumnName;
                    c.DataType = tbc.DataType;
                    c.MatchedColumnName = matched.Select(x => x.ColumnName).FirstOrDefault();
                    if (WhereSelectedFieldDetails.IndexOf(c) < 0)
                    {
                        WhereSelectedFieldDetails.Add(c);
                        WhereSelectedFiledsDataGrid.ItemsSource = WhereSelectedFieldDetails;
                        WhereSelectedFiledsDataGrid.Items.Refresh();
                    }

                    WhereSelectedFiledsDataGrid.ItemsSource = WhereSelectedFieldDetails;
                    WhereSelectedFiledsDataGrid.Items.Refresh();
                }

            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No column found to select");
            }
        }

        private void WhereColumnRemoveAll_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WhereSelectedFieldDetails.Clear();
            WhereSelectedFiledsDataGrid.ItemsSource = WhereSelectedFieldDetails;
            WhereSelectedFiledsDataGrid.Items.Refresh();
        }

        private void WhereColumnRemove_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (WhereSelectedFiledDetail != null)
            {
                WhereSelectedFieldDetails.Remove(WhereSelectedFiledDetail);
                WhereSelectedFiledsDataGrid.ItemsSource = WhereSelectedFieldDetails;
                WhereSelectedFiledsDataGrid.Items.Refresh();
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No column selected to remove");
            }
        }
        private void WhereColumnUp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (WhereSelectedFiledDetail != null)
            {
                var index = WhereSelectedFieldDetails.IndexOf(WhereSelectedFiledDetail);
                if (index >= 0)
                {
                    if (index == 0)
                    {

                    }
                    else
                    {
                        var fElement = WhereSelectedFieldDetails[index - 1];
                        WhereSelectedFieldDetails[index - 1] = WhereSelectedFiledDetail;
                        WhereSelectedFieldDetails[index] = fElement;
                    }
                }

                WhereSelectedFiledsDataGrid.ItemsSource = WhereSelectedFieldDetails;
                WhereSelectedFiledsDataGrid.Items.Refresh();
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No column selected to move the order");
            }
        }

        private void WhereColumnDown_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (WhereSelectedFiledDetail != null)
            {
                var index = WhereSelectedFieldDetails.IndexOf(WhereSelectedFiledDetail);
                if (index >= 0)
                {
                    if (index == WhereSelectedFieldDetails.Count - 1)
                    {

                    }
                    else
                    {
                        var nElement = WhereSelectedFieldDetails[index + 1];
                        WhereSelectedFieldDetails[index + 1] = WhereSelectedFiledDetail;
                        WhereSelectedFieldDetails[index] = nElement;
                    }
                }
                WhereSelectedFiledsDataGrid.ItemsSource = WhereSelectedFieldDetails;
                WhereSelectedFiledsDataGrid.Items.Refresh();
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No column selected to move the order");
            }
        }





        //private void selectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        //{


        //    foreach (DataRowView row in datagird1.SelectedItems)
        //    {
        //        string text = row.Row.ItemArray[0].ToString();
        //        Tabledata = text;
        //        DataGrid_Loaded(text, null);

        //    }
        //    // DataGrid_Loaded(e);

        //}

        //private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        //{
        //    SqlConnection cn = new SqlConnection(@"Data Source=192.168.0.28,1434;Initial Catalog=IWT;Persist Security Info=True;User ID=essae;Password=essae@123");
        //    if (Tabledata == "[dbo].[Transaction]")
        //    {
        //        string queryString = "select COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'Transaction'";
        //        //"SELECT COLUMN_NAME,* FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Transaction' AND TABLE_SCHEMA = 'dbo'";
        //        //"select COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE '[' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']'  = N'Transaction'";
        //        cn.Open();

        //        DataTable table = new DataTable();
        //        SqlDataAdapter a = new SqlDataAdapter(queryString, cn);
        //        a.Fill(table);
        //        datagird2.DataContext = table.DefaultView;
        //    }
        //    if (Tabledata == "Material_Master")
        //    {
        //        string queryString = "select COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'Material_Master'";
        //        cn.Open();

        //        DataTable table = new DataTable();
        //        SqlDataAdapter a = new SqlDataAdapter(queryString, cn);
        //        a.Fill(table);
        //        datagird2.DataContext = table.DefaultView;
        //    }
        //    if (Tabledata == "Supplier_Master")
        //    {
        //        string queryString = "select COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'Supplier_Master'";
        //        cn.Open();

        //        DataTable table = new DataTable();
        //        SqlDataAdapter a = new SqlDataAdapter(queryString, cn);
        //        a.Fill(table);
        //        datagird2.DataContext = table.DefaultView;
        //    }
        //    if (Tabledata == "Vehicle_Master")
        //    {
        //        string queryString = "select COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'Vehicle_Master'";
        //        cn.Open();

        //        DataTable table = new DataTable();
        //        SqlDataAdapter a = new SqlDataAdapter(queryString, cn);
        //        a.Fill(table);
        //        datagird2.DataContext = table.DefaultView;
        //    }
        //}

        //private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    //datagrid3.ItemsSource = FieldData;
        //    //datagrid3.DisplayMemberPath = FieldData;
        //}
        //private void datagrid3_Loaded(object sender, RoutedEventArgs e)
        //{

        //}


        //public class query_details
        //{
        //    public string ReportName { get; set; }
        //    public string Query { get; set; }
        //    public bool WhereEnabled { get; set; }
        //}
        //public class selectedFields
        //{
        //    public string FieldName { get; set; }
        //    public string Name { get; set; }
        //}
        //private void datagird2_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        //{


        //    Console.WriteLine(datagird2.SelectedItems);
        //    foreach (DataRowView row in datagird2.SelectedItems)
        //    {
        //        string text = row.Row.ItemArray[0].ToString();
        //        FieldData = text;
        //        list.Add(FieldData);
        //    }

        //    var a = list;


        //    string combinedString = string.Join(",", list.ToArray()); 

        //    string query = "SELECT ";
        //    string query1 = " from ";
        //    string query2 = Tabledata;
        //    query += combinedString + query1 + query2;
        //    result = query;


        //    Console.WriteLine(combinedString);

        //    ListViewImages_Loaded(FieldData, null);

        //}

        //    private void ListViewImages_Loaded(object sender, RoutedEventArgs e)
        //{

        //    string a = string.Join(",", list.ToArray());

        //     b = a;
        //    ListViewImages.ItemsSource = list;
        //    ListViewImages.Items.Refresh();




        //}
        //private void addcolumn(string FieldData)
        //{
        //    DataGrid d1 = new DataGrid();
        //    DataGridTextColumn t1 = new DataGridTextColumn();
        //    t1.Header = combinedString;
        //    d1.Columns.Add(t1);
        //   // grid.Children.Add(d1);
        //}
        ////multi cell click
        //private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    DataGrid grid = (DataGrid)sender;
        //    var test = grid.SelectedItems; //Count == 1 (always)
        //}

        ////# end
        //private void CheckBox_Checked(object sender, RoutedEventArgs e)
        //{
        //    //if (myCheckBox.IsChecked == true)
        //    //{

        //    //}
        //    //else
        //    //{

        //    //}
        //    var radioButton = sender as RadioButton;
        //    if (radioButton == null)
        //        return;
        //    int intIndex = Convert.ToInt32(radioButton.Content.ToString());
        //}

        ////        if (reader.HasRows)
        ////            {
        ////                DataRow match = null;
        ////                foreach (DataRow row in dt.Rows)
        ////                {

        ////                        foreach (var item in row.ItemArray)
        ////                        {
        ////                            console.Write("Value:" + item);
        ////                        }
        ////}
        ////            }
        //private void SaveButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!popup1.IsOpen)
        //    {
        //        popup1.IsOpen = true;
        //        addcolumn(FieldData);
        //    }// Open it if it's not open
        //    else popup1.IsOpen = false;
        //    String s = Header.Text;
        //    query_details data = new query_details();
        //    selectedFields data1 = new selectedFields();
        //    AdminDBCall db = new AdminDBCall();
        //    data.Query = result;
        //    data.WhereEnabled = check;
        //    data.ReportName = Header.Text;
        //    data1.FieldName = b;
        //    data1.Name = Header.Text;
        //    db.InserSaveReport(data);
        //    db.InserSaveField(data1);
        //    Console.WriteLine(data1);
        //    Console.WriteLine(ListViewImages.ItemsSource);
        //    // string result1 = s;
        //    DialogHost.CloseDialogCommand.Execute(s, null);
        //    Console.WriteLine(result);


        //}

        //private void grid1_Loaded(object sender, RoutedEventArgs e)
        //{

        //}

        //private void dataGrid_Loaded_1(object sender, RoutedEventArgs e)
        //{

        //}

        //private void lvDataBinding_Loaded(object sender, RoutedEventArgs e)
        //{

        //}

        //private void myCheckBox_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (myCheckBox.IsChecked == true)
        //    {
        //        check = myCheckBox.IsEnabled;
        //    }
        //    else
        //    {

        //    }
        //}

        //    private void CreateATextBox()
        //    {

        //}

        //private void datagird1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{

        //}
    }
}
