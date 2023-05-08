using IWT.DBCall;
using IWT.Models;
using IWT.Saved_Template;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using Microsoft.Reporting.WinForms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Linq.Dynamic;
using DataGridTextColumn = System.Windows.Controls.DataGridTextColumn;
using System.Diagnostics;
using System.Configuration;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IWT.Views
{
    /// <summary>
    /// Interaction logic for SummaryReports.xaml
    /// </summary>
    public partial class SummaryReports : UserControl, INotifyPropertyChanged
    {
        public string RangePicker;
        public string TransactionType;
        public string Tabledata;
        public string FieldData = "";
        public string combinedString;
        public string Query;
        public string form1;
        public string TemplateName;
        public string QueryName;
        List<string> stringList = new List<string>();
        public string[] fieldset = new string[] { };
        List<string> list = new List<string>();
        List<string> list1 = new List<string>();

        public static MasterDBCall masterDBCall = new MasterDBCall();
        public static AdminDBCall adminDBCall = new AdminDBCall();

        public static CommonFunction commonFunction = new CommonFunction();
        List<TransactionTypeMaster> AllTransactionTypes = new List<TransactionTypeMaster>();
        public List<MaterialMaster> AllMaterials = new List<MaterialMaster>();
        public List<SupplierMaster> AllSuppliers = new List<SupplierMaster>();
        public List<ShiftMaster> shiftMasters = new List<ShiftMaster>();
        string LastMessage;
        private readonly ToastViewModel toastViewModel;
        TabItem selectedTabItem;
        string selectedTabItemName;
        List<SavedReportTemplate> savedReportTemplates = new List<SavedReportTemplate>();
        SavedReportTemplate selectedSavedReportTemplate = new SavedReportTemplate();
        List<SavedReportTemplateFields> savedReportTemplateFields = new List<SavedReportTemplateFields>();
        List<SavedReportTemplateWhereFields> savedReportTemplateWhereFields = new List<SavedReportTemplateWhereFields>();
        public List<CustomFieldBuilder> CustomFieldsBuilder = new List<CustomFieldBuilder>();
        List<string> RangeList = new List<string>();
        List<Transaction> transactions = new List<Transaction>();
        Dictionary<string, string> columnNames = new Dictionary<string, string>();
        List<Caption> captions = new List<Caption>();

        public static event EventHandler<UserControlEventArgs> onSummaryReportLoaded = delegate { };
        List<ImageSourcePath> CurrentTransactionImageSourcePath = new List<ImageSourcePath>();
        public string reportApp = ConfigurationManager.AppSettings["ReportApplicationPath"].ToString();

        List<OperationDetail> operationDetails = new List<OperationDetail>();
        List<string> RegisteredNames = new List<string>();

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

        List<SavedReportTemplate> Result = new List<SavedReportTemplate>();
        List<SavedReportTemplate> Result1 = new List<SavedReportTemplate>();

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

        public SummaryReports()
        {
            InitializeComponent();
            //BindComboBox(ComboBoxZone);
            //BindComboBox1(ComboBoxZone1);
            //BindComboBox2(ComboBoxZone2);
            //BindComboBox3(ComboBoxZone3);
            //BindComboBox4(ComboBoxZone4);
            //BindComboBox5(cbo);
            //BindComboBox6(Template);
            //BindComboBox7(l2);
            //GetTemplate();
            //DataContext = new ViewTemplate();
            toastViewModel = new ToastViewModel();
            DataContext = this;
            SelectedRecord = 10;
            NumberOfPages = 1;
            //GetCompanyDetails();
            PaginatorComboBox.ItemsSource = ItemPerPagesList;
            Loaded += SummaryReports_Loaded;
            Unloaded += SummaryReports_Unloaded;
            RegisteredNames = new List<string>();
        }

        private void SummaryReports_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (var temp in CustomFieldsBuilder)
            {
                if (FindName($"c_{temp.RegName}") != null)
                    UnregisterName($"c_{temp.RegName}");
            }

            foreach (var temp in RegisteredNames)
            {
                if (FindName($"c_{temp}") != null)
                    UnregisterName($"c_{temp}");
            }
            RegisteredNames = new List<string>();
        }

        private void SummaryReports_Loaded(object sender, RoutedEventArgs e)
        {
            onSummaryReportLoaded.Invoke(this, new UserControlEventArgs());
            SavedTemplateGrid.SelectionChanged += SavedTemplateGrid_SelectionChanged;
            GetSavedTemplates();
            SetSavedReportTemplateComboBox();
            GetAllRanges();
            GetAllTransactionTypes();
            GetAllMaterials();
            GetAllSuppliers();
            GetShiftMasters();
            ShowGeneralFilter();
            HideCustomizedFilter();
        }

        private void SavedTemplateGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //e.Handled = true;
        }

        public void GetAllRanges()
        {
            RangeList = new List<string>() { "Week", "Month", "Quarter", "Year" };
            RangeComboBox.ItemsSource = RangeList;
            RangeComboBox.Items.Refresh();
        }

        public void GetAllTransactionTypes()
        {
            try
            {
                AllTransactionTypes = commonFunction.GetTransactionTypeMasters();
                TransactionTypeComboBox.ItemsSource = AllTransactionTypes;
                TransactionTypeComboBox.Items.Refresh();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/GetAllMaterials/Exception:- " + ex.Message, ex);
            }
        }
        private void GetAllMaterials()
        {
            try
            {
                AllMaterials = commonFunction.GetMaterialMasters();
                MaterialComboBox.ItemsSource = AllMaterials;
                MaterialComboBox.Items.Refresh();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/GetAllMaterials/Exception:- " + ex.Message, ex);
            }
        }

        private void GetAllSuppliers()
        {
            try
            {
                AllSuppliers = commonFunction.GetSupplierMasters();
                SupplierComboBox.ItemsSource = AllSuppliers;
                SupplierComboBox.Items.Refresh();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/GetAllSuppliers/Exception:- " + ex.Message, ex);
            }
        }
        public void GetShiftMasters()
        {
            try
            {
                shiftMasters = commonFunction.GetShiftMasters();
                ShiftComboBox.ItemsSource = shiftMasters;
                ShiftComboBox.Items.Refresh();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/GetShiftMasters/Exception:- " + ex.Message, ex);
            }
        }
        private void ReportTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl) //if this event fired from TabControl then enter
            {
                selectedTabItem = ReportTabControl.SelectedItem as TabItem;
                var selectedTabItemName = selectedTabItem?.Name;
                GenerateReportLabel.Foreground = selectedTabItemName == "GenerateReportTabItem" ? Brushes.White : Brushes.Black;
                SavedTemplateLabel.Foreground = selectedTabItemName == "SavedTemplateTabItem" ? Brushes.White : Brushes.Black;
                //SavedCriteriaLabel.Foreground = selectedTabItemName == "SavedCriteriaTabItem" ? Brushes.White : Brushes.Black;

                switch (selectedTabItemName)
                {
                    case "GenerateReportTabItem":
                        GetSavedTemplates();
                        SetSavedReportTemplateComboBox();
                        //ShowGeneralFilter();
                        //HideCustomizedFilter();
                        ClearForms();
                        break;
                    case "SavedTemplateTabItem":
                        GetSavedTemplates();
                        SetSavedTemplateTable();
                        break;
                    case "SavedCriteriaTabItem":
                        break;
                    default:
                        break;

                }
            }

        }
        private void GetSavedTemplates()
        {
            try
            {
                string Query = "SELECT * FROM Saved_ReportTemplate";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                savedReportTemplates = JsonConvert.DeserializeObject<List<SavedReportTemplate>>(JSONString);
                Result = JsonConvert.DeserializeObject<List<SavedReportTemplate>>(JSONString);

            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/GetSavedTemplates/Exception:- " + ex.Message, ex);
            }
        }

        public void SetSavedReportTemplateComboBox()
        {
            TemplateComboBox.ItemsSource = savedReportTemplates;
            TemplateComboBox.Items.Refresh();
        }

        public void SetSavedTemplateTable()
        {
            //SavedTemplateGrid.ItemsSource = savedReportTemplates;
            //SavedTemplateGrid.Items.Refresh();
            TotalRecords = Result.Count;
            UpdateRecordCount();
            UpdateCollection(Result.Take(SelectedRecord));
            SetDynamicTable();
        }
        public void SetDynamicTable()
        {
            try
            {
                SavedTemplateGrid.ItemsSource = Result1;
                SavedTemplateGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetTableData:" + ex.Message);
            }
        }
        private void GetSavedTemplateFields()
        {
            try
            {
                string Query = $"SELECT * FROM Saved_ReportTemplateFields WHERE TemplateID={selectedSavedReportTemplate.TemplateID}";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                savedReportTemplateFields = JsonConvert.DeserializeObject<List<SavedReportTemplateFields>>(JSONString);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/GetSavedTemplateFields/Exception:- " + ex.Message, ex);
            }
        }
        public List<Caption> GetCaptions(string TableName)
        {
            captions = new List<Caption>();
            try
            {
                captions = commonFunction.GetCaptions(TableName);
                return captions;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetCaptions : " + ex.Message);
                return captions;
            }
        }

        private void GetSavedTemplateWhereFields()
        {
            try
            {
                string Query = $"SELECT * FROM Saved_ReportTemplateWhereFields WHERE TemplateID={selectedSavedReportTemplate.TemplateID}";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                savedReportTemplateWhereFields = JsonConvert.DeserializeObject<List<SavedReportTemplateWhereFields>>(JSONString);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/GetSavedTemplateWhereFields/Exception:- " + ex.Message, ex);
            }
        }

        public void BuildCustomFields()
        {
            foreach (var temp in CustomFieldsBuilder)
            {
                if (FindName($"c_{temp.RegName}") != null)
                    UnregisterName($"c_{temp.RegName}");
            }
            foreach (var temp in RegisteredNames)
            {
                if (FindName($"c_{temp}") != null)
                    UnregisterName($"c_{temp}");
            }
            RegisteredNames = new List<string>();

            CustomFieldsBuilder = new List<CustomFieldBuilder>();
            foreach (var field in savedReportTemplateWhereFields)
            {
                CustomFieldBuilder customFieldBuilder = new CustomFieldBuilder();
                customFieldBuilder.FieldTable = field.TableName;
                customFieldBuilder.FieldName = field.FieldName;
                customFieldBuilder.FieldCaption = field.FieldName;
                customFieldBuilder.FieldType = field.DataType;

                if (customFieldBuilder.FieldName?.ToLower() == "suppliercode" || customFieldBuilder.FieldName?.ToLower() == "suppliername" ||
                   customFieldBuilder.FieldName?.ToLower() == "materialcode" || customFieldBuilder.FieldName?.ToLower() == "materialname" ||
                   customFieldBuilder.FieldName?.ToLower() == "transactiontype" || customFieldBuilder.FieldName?.ToLower() == "shiftname" ||
                   customFieldBuilder.FieldName?.ToLower() == "ticketnocriteria")
                {
                    customFieldBuilder.ControlType = "Dropdown";

                }
                else
                {
                    customFieldBuilder.ControlType = "TextBox";
                }

                customFieldBuilder.ControlTable = field.TableName;
                customFieldBuilder.SelectionBasis = field.TableName;
                customFieldBuilder.RegName = field.FieldName;
                customFieldBuilder.ControlTableRef = field.TableName;
                CustomFieldsBuilder.Add(customFieldBuilder);
            }
            foreach (CustomFieldBuilder template in CustomFieldsBuilder)
            {

                //var intTypeFileds = savedReportTemplateWhereFields.Where(x => x.DataType == "int").ToList();
                //foreach (var x in intTypeFileds)
                //{
                //    SavedReportTemplateWhereFields fields = new SavedReportTemplateWhereFields();
                //    fields.TemplateID = x.TemplateID;
                //    fields.FieldName = x.FieldName + "To";
                //    fields.DataType = x.DataType;
                //    fields.TableName = x.TableName;
                //    fields.MatchedColumnName = x.MatchedColumnName;
                //    savedReportTemplateWhereFields.Add(fields);
                //    x.FieldName = x.FieldName + "From";
                //}

                if (template.FieldName == "TicketNoCriteria")
                {
                    GetAllOperations();
                    dynamicWrapPanel.Children.Add(CreateCriteriaComboboxField(template));
                }
                else
                {
                    if (template.ControlType == "TextBox")
                    {
                        if (template.FieldType == "datetime2")
                        {
                            dynamicWrapPanel.Children.Add(CreateDateField(template));
                        }
                        else
                        {
                            dynamicWrapPanel.Children.Add(CreateInputField(template));
                        }
                    }
                    else if (template.ControlType == "Dropdown")
                    {
                        dynamicWrapPanel.Children.Add(CreateComboboxField(template));
                    }
                }

            }
        }

        private void GetValuesFromFields()
        {
            foreach (var field in CustomFieldsBuilder)
            {
                if (field.ControlType == "TextBox")
                {
                    if (field.FieldType == "DATETIME")
                    {
                        DatePicker datePicker = (DatePicker)FindName($"c_{field.RegName}");
                        if (datePicker.SelectedDate.HasValue)
                        {
                            var date = (DateTime)datePicker.SelectedDate;
                            field.Value = date.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            field.Value = null;
                        }
                        field.Value = datePicker.SelectedDate;
                    }
                    else
                    {
                        TextBox textBox = (TextBox)FindName($"c_{field.RegName}");
                        field.Value = textBox.Text;
                    }
                }
                else if (field.ControlType == "Dropdown")
                {
                    ComboBox comboBox = (ComboBox)this.FindName($"c_{field.RegName}");
                    //ComboBoxItem selected = comboBox.SelectedItem as ComboBoxItem;
                    //if (selected != null)
                    //{
                    //    field.Value = selected.Content.ToString();
                    //}
                    field.Value = comboBox.SelectedValue?.ToString();
                }
                else if (field.ControlType == "Formula")
                {
                    TextBox textBox = (TextBox)FindName($"c_{field.RegName}");
                    field.Value = textBox.Text;
                }
                else if (field.ControlType == "DataDependancy")
                {
                    TextBox textBox = (TextBox)FindName($"c_{field.RegName}");
                    field.Value = textBox.Text;
                }
            }
        }


        private void TemplateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //e.Handled = true;
            GenerateBtn.IsEnabled = !string.IsNullOrEmpty(TemplateComboBox.SelectedValue?.ToString());
            DateBorder.IsEnabled = !string.IsNullOrEmpty(TemplateComboBox.SelectedValue?.ToString());
            DateRadioButton.IsEnabled = !string.IsNullOrEmpty(TemplateComboBox.SelectedValue?.ToString());
            RangeRadioButton.IsEnabled = !string.IsNullOrEmpty(TemplateComboBox.SelectedValue?.ToString());
            GroupbyRadioBtn.IsEnabled = !string.IsNullOrEmpty(TemplateComboBox.SelectedValue?.ToString());
            selectedSavedReportTemplate = TemplateComboBox.SelectedItem as SavedReportTemplate;
            if (selectedSavedReportTemplate != null && selectedSavedReportTemplate.TemplateID != 0)
            {
                if (selectedSavedReportTemplate.WhereEnabled)
                {
                    dynamicWrapPanel.Children.Clear();
                    HideGeneralFilter();
                    ShowCustomizedFilter();
                    GetSavedTemplateWhereFields();
                    if (savedReportTemplateWhereFields.Count > 0)
                    {

                        var intTypeFileds = savedReportTemplateWhereFields.Where(x => x.DataType == "int").ToList();
                        foreach (var x in intTypeFileds)
                        {
                            if (x.FieldName == "TicketNo")
                            {
                                SavedReportTemplateWhereFields fields1 = new SavedReportTemplateWhereFields();
                                fields1.TemplateID = x.TemplateID;
                                fields1.FieldName = x.FieldName + "Criteria";
                                fields1.DataType = "nvarchar";
                                fields1.TableName = x.TableName;
                                fields1.MatchedColumnName = x.MatchedColumnName;
                                savedReportTemplateWhereFields.Add(fields1);

                                SavedReportTemplateWhereFields field = new SavedReportTemplateWhereFields();
                                field.TemplateID = x.TemplateID;
                                field.FieldName = x.FieldName + "From";
                                field.DataType = x.DataType;
                                field.TableName = x.TableName;
                                field.MatchedColumnName = x.MatchedColumnName;
                                savedReportTemplateWhereFields.Add(field);

                                SavedReportTemplateWhereFields fields = new SavedReportTemplateWhereFields();
                                fields.TemplateID = x.TemplateID;
                                fields.FieldName = x.FieldName + "To";
                                fields.DataType = x.DataType;
                                fields.TableName = x.TableName;
                                fields.MatchedColumnName = x.MatchedColumnName;
                                savedReportTemplateWhereFields.Add(fields);

                                x.FieldName = x.FieldName + "Value";
                            }
                            else
                            {
                                SavedReportTemplateWhereFields fields = new SavedReportTemplateWhereFields();
                                fields.TemplateID = x.TemplateID;
                                fields.FieldName = x.FieldName + "To";
                                fields.DataType = x.DataType;
                                fields.TableName = x.TableName;
                                fields.MatchedColumnName = x.MatchedColumnName;
                                savedReportTemplateWhereFields.Add(fields);
                                x.FieldName = x.FieldName + "From";
                            }


                        }

                        //var intTypeFileds = savedReportTemplateWhereFields.Where(x => x.FieldName == "TicketNo" && x.DataType == "int").ToList();
                        //foreach (var x in intTypeFileds)
                        //{


                        //}
                        savedReportTemplateWhereFields = savedReportTemplateWhereFields.OrderBy(x => x.FieldName).ToList();
                        BuildCustomFields();
                    }
                    else
                    {
                        ShowGeneralFilter();
                        HideCustomizedFilter();
                    }
                    GetSavedTemplateFields();
                    GetCaptions(selectedSavedReportTemplate.TableName);
                }
                else
                {
                    ShowGeneralFilter();
                    HideCustomizedFilter();
                    GetSavedTemplateFields();
                    GetCaptions(selectedSavedReportTemplate.TableName);

                }
            }
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

        //public void SetOperations(OperationType operationType = OperationType.Common)
        //{
        //    if (operationType == OperationType.Common)
        //    {
        //        GetCommonOperations();

        //    }
        //    else if (operationType == OperationType.All)
        //    {
        //        GetAllOperations();
        //    }
        //    //OperationComboBox.ItemsSource = operationDetails;
        //    //OperationComboBox.Items.Refresh();
        //}




        private void ResetFields()
        {
            foreach (var field in CustomFieldsBuilder)
            {
                if (field.ControlType == "TextBox")
                {
                    if (field.FieldType == "datetime2")
                    {
                        DatePicker datePicker = (DatePicker)FindName($"c_{field.RegName}");
                        if (datePicker != null)
                            datePicker.SelectedDate = null;
                    }
                    else
                    {
                        TextBox textBox = (TextBox)FindName($"c_{field.RegName}");
                        if (textBox != null)
                            textBox.Text = null;
                    }
                }
            }
        }


        public void ShowGeneralFilter()
        {
            DateRadioButton.IsChecked = false;
            RangeRadioButton.IsChecked = false;
            RangeRadioButton.Visibility = Visibility.Visible;
            RangeBorder.Visibility = Visibility.Visible;
            TransactionBorder.Visibility = Visibility.Visible;
            MaterialBorder.Visibility = Visibility.Visible;
            SupplierBorder.Visibility = Visibility.Visible;
            ShiftBorder.Visibility = Visibility.Visible;
            VehicleBorder.Visibility = Visibility.Visible;
        }

        public void HideGeneralFilter()
        {
            DateRadioButton.IsChecked = true;
            RangeRadioButton.IsChecked = false;
            RangeRadioButton.Visibility = Visibility.Collapsed;
            RangeBorder.Visibility = Visibility.Collapsed;
            TransactionBorder.Visibility = Visibility.Collapsed;
            MaterialBorder.Visibility = Visibility.Collapsed;
            SupplierBorder.Visibility = Visibility.Collapsed;
            ShiftBorder.Visibility = Visibility.Collapsed;
            VehicleBorder.Visibility = Visibility.Collapsed;
        }

        public void ShowCustomizedFilter()
        {
            dynamicWrapPanelContainer.Visibility = Visibility.Visible;
            dynamicWrapPanel.Visibility = Visibility.Visible;
        }

        public void HideCustomizedFilter()
        {
            dynamicWrapPanelContainer.Visibility = Visibility.Collapsed;
            dynamicWrapPanel.Visibility = Visibility.Collapsed;
        }


        private Border CreateInputField(CustomFieldBuilder template)
        {
            Border container = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1)
            };
            container.Name = $"{template.FieldName}Container";
            RegisterName($"c_{container.Name}", container);
            RegisteredNames.Add($"{container.Name}");
            StackPanel inputContainer = new StackPanel
            {
                Height = 50,
                Orientation = Orientation.Horizontal
            };
            inputContainer.Name = $"{template.FieldName}Panel";
            Image customFieldIcon = new Image
            {
                Source = new BitmapImage(new Uri(@"/Assets/Icons/Custom_Field.png", UriKind.Relative)),
                Width = 50,
                Margin = new Thickness(10)
            };
            Style style = this.FindResource("MaterialDesignFloatingHintTextBox") as Style;
            TextBox customFieldTextBox = new TextBox
            {
                Width = 200,
                Margin = new Thickness(10, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14,
                FontFamily = new FontFamily("Segoe UI Semibold"),
                IsEnabled = template.IsEnabled,
                Tag = template.RegName,
                Style = style
            };
            customFieldTextBox.Name = $"{template.FieldName}";
            //customFieldTextBox.TextChanged += CustomFieldTextBox_TextChanged;
            RegisterName($"c_{template.RegName}", customFieldTextBox);

            HintAssist.SetHint(customFieldTextBox, template.FieldCaption);

            inputContainer.Children.Add(customFieldIcon);
            inputContainer.Children.Add(customFieldTextBox);
            container.Child = inputContainer;
            if (template.FieldName == "TicketNoFrom" || template.FieldName == "TicketNoTo")
            {
                //customFieldTextBox.Visibility = Visibility.Hidden;
                //customFieldIcon.Visibility = Visibility.Hidden;
                container.Visibility = Visibility.Collapsed;
            }
            return container;
        }

        private Border CreateComboboxField(CustomFieldBuilder template)
        {
            Border container = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1)
            };
            StackPanel inputContainer = new StackPanel
            {
                Height = 50,
                Orientation = Orientation.Horizontal
            };
            Image customFieldIcon = new Image
            {
                Source = new BitmapImage(new Uri(@"/Assets/Icons/Custom_Field.png", UriKind.Relative)),
                Width = 50,
                Margin = new Thickness(10)
            };
            Style style = this.FindResource("MaterialDesignFloatingHintComboBox") as Style;
            ComboBox customFieldComboBox = new ComboBox
            {
                Width = 200,
                Margin = new Thickness(10, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14,
                FontFamily = new FontFamily("Segoe UI Semibold"),
                IsEnabled = template.IsEnabled,
                Tag = template.RegName,
                Style = style,
            };
            //customFieldComboBox.SetResourceReference(Control.StyleProperty, "MaterialDesignFloatingHintComboBox");
            customFieldComboBox.SelectionChanged += CustomFieldComboBox_SelectionChanged;
            RegisterName($"c_{template.RegName}", customFieldComboBox);
            //List<string> comboboxItems = GetItemsBySelection(template.ControlTable, template.SelectionBasis);

            if (template.FieldName?.ToLower() == "suppliercode" || template.FieldName?.ToLower() == "suppliername")
            {
                customFieldComboBox.ItemsSource = AllSuppliers;
                customFieldComboBox.Items.Refresh();
                customFieldComboBox.DisplayMemberPath = template.FieldName?.ToLower() == "suppliercode" ? "SupplierCode" : "SupplierName";
                customFieldComboBox.SelectedValuePath = "SupplierName";
            }
            else if (template.FieldName?.ToLower() == "materialcode" || template.FieldName?.ToLower() == "materialname")
            {
                customFieldComboBox.ItemsSource = AllMaterials;
                customFieldComboBox.Items.Refresh();
                customFieldComboBox.DisplayMemberPath = template.FieldName?.ToLower() == "materialcode" ? "MaterialCode" : "MaterialName";
                customFieldComboBox.SelectedValuePath = "MaterialName";
            }
            else if (template.FieldName?.ToLower() == "transactiontype")
            {
                customFieldComboBox.ItemsSource = AllTransactionTypes;
                customFieldComboBox.Items.Refresh();
                customFieldComboBox.DisplayMemberPath = "TransactionType";
                customFieldComboBox.SelectedValuePath = "TransactionType";
            }
            if (template.FieldName?.ToLower() == "shiftname")
            {
                customFieldComboBox.ItemsSource = shiftMasters;
                customFieldComboBox.Items.Refresh();
                customFieldComboBox.DisplayMemberPath = "ShiftName";
                customFieldComboBox.SelectedValuePath = "ShiftName";
            }

            //foreach (var item in comboboxItems)
            //{
            //    customFieldComboBox.Items.Add(new ComboBoxItem { Content = item });
            //}
            HintAssist.SetHint(customFieldComboBox, template.FieldCaption);
            inputContainer.Children.Add(customFieldIcon);
            inputContainer.Children.Add(customFieldComboBox);
            container.Child = inputContainer;
            return container;
        }

        private Border CreateCriteriaComboboxField(CustomFieldBuilder template)
        {
            Border container = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1)
            };
            StackPanel inputContainer = new StackPanel
            {
                Height = 50,
                Orientation = Orientation.Horizontal
            };
            Image customFieldIcon = new Image
            {
                Source = new BitmapImage(new Uri(@"/Assets/Icons/Custom_Field.png", UriKind.Relative)),
                Width = 50,
                Margin = new Thickness(10)
            };
            Style style = this.FindResource("MaterialDesignFloatingHintComboBox") as Style;
            ComboBox customFieldComboBox1 = new ComboBox
            {
                Width = 200,
                Margin = new Thickness(10, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14,
                FontFamily = new FontFamily("Segoe UI Semibold"),
                IsEnabled = template.IsEnabled,
                Tag = template.RegName,
                Style = style,
            };
            //customFieldComboBox.SetResourceReference(Control.StyleProperty, "MaterialDesignFloatingHintComboBox");
            customFieldComboBox1.SelectionChanged += CustomFieldComboBox1_SelectionChanged;
            RegisterName($"c_{template.RegName}", customFieldComboBox1);
            //List<string> comboboxItems = GetItemsBySelection(template.ControlTable, template.SelectionBasis);
            customFieldComboBox1.ItemsSource = operationDetails;
            customFieldComboBox1.Items.Refresh();
            customFieldComboBox1.DisplayMemberPath = "Name";
            customFieldComboBox1.SelectedValuePath = "Value";
            HintAssist.SetHint(customFieldComboBox1, template.FieldCaption);
            inputContainer.Children.Add(customFieldIcon);
            inputContainer.Children.Add(customFieldComboBox1);
            container.Child = inputContainer;
            return container;
        }

        private void CustomFieldComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //e.Handled = true;
            var cb = (ComboBox)sender;
            var selectedOperation = cb.SelectedValue?.ToString();
            var ticketNo = (Border)this.FindName("c_TicketNoValueContainer");
            var ticketNoFrom = (Border)this.FindName("c_TicketNoFromContainer");
            var ticketNoTo = (Border)this.FindName("c_TicketNoToContainer");

            if (!string.IsNullOrEmpty(selectedOperation) && selectedOperation == "BETWEEN")
            {
                ticketNo.Visibility = Visibility.Collapsed;
                ticketNoFrom.Visibility = Visibility.Visible;
                ticketNoTo.Visibility = Visibility.Visible;
            }
            else
            {
                ticketNo.Visibility = Visibility.Visible;
                ticketNoFrom.Visibility = Visibility.Collapsed;
                ticketNoTo.Visibility = Visibility.Collapsed;
            }
        }

        private void CustomFieldComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //e.Handled = true;
        }

        private Border CreateDateField(CustomFieldBuilder template)
        {
            Border container = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1)
            };
            StackPanel inputContainer = new StackPanel
            {
                Height = 50,
                Orientation = Orientation.Horizontal
            };
            Image customFieldIcon = new Image
            {
                Source = new BitmapImage(new Uri(@"/Assets/Icons/Custom_Field.png", UriKind.Relative)),
                Width = 50,
                Margin = new Thickness(10)
            };
            DatePicker customFieldDatePicker = new DatePicker
            {
                Width = 200,
                Margin = new Thickness(10, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14,
                FontFamily = new FontFamily("Segoe UI Semibold"),
                IsEnabled = template.IsEnabled
            };
            RegisterName($"c_{template.RegName}", customFieldDatePicker);
            HintAssist.SetHint(customFieldDatePicker, template.FieldCaption);
            inputContainer.Children.Add(customFieldIcon);
            inputContainer.Children.Add(customFieldDatePicker);
            container.Child = inputContainer;
            return container;
        }

        //private Border CreateComboboxField(CustomFieldBuilder template)
        //{
        //    Border container = new Border
        //    {
        //        BorderBrush = Brushes.LightGray,
        //        BorderThickness = new Thickness(1)
        //    };
        //    StackPanel inputContainer = new StackPanel
        //    {
        //        Height = 50,
        //        Orientation = Orientation.Horizontal
        //    };
        //    Image customFieldIcon = new Image
        //    {
        //        Source = new BitmapImage(new Uri(@"/Assets/Icons/Custom_Field.png", UriKind.Relative)),
        //        Width = 50,
        //        Margin = new Thickness(10)
        //    };
        //    ComboBox customFieldComboBox = new ComboBox
        //    {
        //        Width = 180,
        //        Margin = new Thickness(10, 0, 10, 0),
        //        VerticalAlignment = VerticalAlignment.Center,
        //        FontSize = 14,
        //        FontFamily = new FontFamily("Segoe UI Semibold"),
        //        IsEnabled = template.IsEnabled,
        //        Tag = template.RegName
        //    };
        //    customFieldComboBox.SelectionChanged += CustomFieldComboBox_SelectionChanged;
        //    RegisterName(template.RegName, customFieldComboBox);
        //    List<string> comboboxItems = GetItemsBySelection(template.ControlTable, template.SelectionBasis);
        //    foreach (var item in comboboxItems)
        //    {
        //        customFieldComboBox.Items.Add(new ComboBoxItem { Content = item });
        //    }
        //    HintAssist.SetHint(customFieldComboBox, template.FieldCaption);
        //    inputContainer.Children.Add(customFieldIcon);
        //    inputContainer.Children.Add(customFieldComboBox);
        //    container.Child = inputContainer;
        //    return container;
        //}
        //private Border CreateFormulaField(CustomFieldBuilder template)
        //{
        //    Border container = new Border
        //    {
        //        BorderBrush = Brushes.LightGray,
        //        BorderThickness = new Thickness(1)
        //    };
        //    StackPanel inputContainer = new StackPanel
        //    {
        //        Height = 50,
        //        Orientation = Orientation.Horizontal
        //    };
        //    Image customFieldIcon = new Image
        //    {
        //        Source = new BitmapImage(new Uri(@"/Assets/Icons/Custom_Field.png", UriKind.Relative)),
        //        Width = 50,
        //        Margin = new Thickness(10)
        //    };
        //    TextBox customFieldTextBox = new TextBox
        //    {
        //        Width = 180,
        //        Margin = new Thickness(10, 0, 10, 0),
        //        VerticalAlignment = VerticalAlignment.Center,
        //        FontSize = 14,
        //        FontFamily = new FontFamily("Segoe UI Semibold"),
        //        IsEnabled = false
        //    };
        //    RegisterName(template.RegName, customFieldTextBox);
        //    HintAssist.SetHint(customFieldTextBox, template.FieldCaption);
        //    inputContainer.Children.Add(customFieldIcon);
        //    inputContainer.Children.Add(customFieldTextBox);
        //    container.Child = inputContainer;
        //    var formulaTemplate = GetFormulaTemplate(template.FieldCaption);
        //    template.Formula = DecodeFormula(formulaTemplate.FormulaList);
        //    return container;
        //}
        //private Border CreateDataDependencyField(CustomFieldBuilder template)
        //{
        //    Border container = new Border
        //    {
        //        BorderBrush = Brushes.LightGray,
        //        BorderThickness = new Thickness(1)
        //    };
        //    StackPanel inputContainer = new StackPanel
        //    {
        //        Height = 50,
        //        Orientation = Orientation.Horizontal
        //    };
        //    Image customFieldIcon = new Image
        //    {
        //        Source = new BitmapImage(new Uri(@"/Assets/Icons/Custom_Field.png", UriKind.Relative)),
        //        Width = 50,
        //        Margin = new Thickness(10)
        //    };
        //    var fd = fieldDependencies.FirstOrDefault(t => t.CustomName == template.FieldName && t.CustomcType == template.FieldType);
        //    if (fd != null)
        //    {
        //        TextBox customFieldTextBox = new TextBox
        //        {
        //            Width = 180,
        //            Margin = new Thickness(10, 0, 10, 0),
        //            VerticalAlignment = VerticalAlignment.Center,
        //            FontSize = 14,
        //            FontFamily = new FontFamily("Segoe UI Semibold"),
        //            IsEnabled = false
        //        };
        //        RegisterName(template.RegName, customFieldTextBox);
        //        HintAssist.SetHint(customFieldTextBox, template.FieldCaption);
        //        inputContainer.Children.Add(customFieldIcon);
        //        inputContainer.Children.Add(customFieldTextBox);
        //    }
        //    else
        //    {
        //        TextBox customFieldTextBox = new TextBox
        //        {
        //            Width = 90,
        //            Margin = new Thickness(10, 0, 10, 0),
        //            VerticalAlignment = VerticalAlignment.Center,
        //            FontSize = 14,
        //            FontFamily = new FontFamily("Segoe UI Semibold"),
        //            IsEnabled = false
        //        };
        //        RegisterName(template.RegName, customFieldTextBox);
        //        Button linkBtn = new Button
        //        {
        //            Content = "Link"
        //        };
        //        linkBtn.Click += LinkBtn_Click;
        //        RegisterName("link_" + template.RegName, linkBtn);
        //        linkBtn.Tag = template.RegName;
        //        HintAssist.SetHint(customFieldTextBox, template.FieldCaption);
        //        inputContainer.Children.Add(customFieldIcon);
        //        inputContainer.Children.Add(customFieldTextBox);
        //        inputContainer.Children.Add(linkBtn);
        //    }
        //    container.Child = inputContainer;
        //    return container;
        //}



        private void DateRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (DateRadioButton.IsChecked.HasValue && DateRadioButton.IsChecked.Value)
            {
                ShowDatePickers();
                HideRangeComboBox();
                RangeRadioButton.IsChecked = false;
            }
            else
            {
                HideDatePickers();
                //ShowRangeComboBox();
            }
        }
        private void RangeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (RangeRadioButton.IsChecked.HasValue && RangeRadioButton.IsChecked.Value)
            {
                HideDatePickers();
                ShowRangeComboBox();
                DateRadioButton.IsChecked = false;
            }
            else
            {
                //ShowDatePickers();
                HideRangeComboBox();
            }
        }

        private void ShowDatePickers()
        {
            FromDatePicker.Visibility = Visibility.Visible;
            ToDatePicker.Visibility = Visibility.Visible;
            FromDateTextBlock.Visibility = Visibility.Collapsed;
            ToDateTextBlock.Visibility = Visibility.Collapsed;
        }

        private void HideDatePickers()
        {
            FromDatePicker.Visibility = Visibility.Collapsed;
            ToDatePicker.Visibility = Visibility.Collapsed;
            FromDateTextBlock.Visibility = Visibility.Visible;
            ToDateTextBlock.Visibility = Visibility.Visible;
        }
        private void ShowRangeComboBox()
        {
            RangeComboBox.Visibility = Visibility.Visible;
            RangeTextBlock.Visibility = Visibility.Collapsed;
        }
        private void HideRangeComboBox()
        {
            RangeComboBox.Visibility = Visibility.Collapsed;
            RangeTextBlock.Visibility = Visibility.Visible;
        }


        public void BuildAndExecuteQuery(SavedReportTemplate SelectedSavedReportTemplate)
        {
            string Query = "";
            if (SelectedSavedReportTemplate.TableName == "Transaction")
            {
                Query = $"{SelectedSavedReportTemplate.Query} WHERE [{SelectedSavedReportTemplate.TableName}].IsDeleted=0";
            }
            else
            {
                Query = $"{SelectedSavedReportTemplate.Query} WHERE";
            }
            if (DateRadioButton.IsChecked.HasValue && DateRadioButton.IsChecked.Value)
            {

                var DatePickerQuery = GetDatePickerQuery(SelectedSavedReportTemplate);
                if (!string.IsNullOrEmpty(DatePickerQuery))
                {
                    Query = Query + " AND " + DatePickerQuery;
                }
            }
            else if (RangeRadioButton.IsChecked.HasValue && RangeRadioButton.IsChecked.Value)
            {
                var RangeQuery = GetRangeQuery();
                if (!string.IsNullOrEmpty(RangeQuery))
                {
                    Query = Query + " AND " + RangeQuery;
                }
            }

            var TransactionTypeQuery = GetTransactionTypeQuery();
            if (!string.IsNullOrEmpty(TransactionTypeQuery))
            {
                Query = Query + " AND " + TransactionTypeQuery;
            }

            var MaterialQuery = GetMaterialQuery();
            if (!string.IsNullOrEmpty(MaterialQuery))
            {
                Query = Query + " AND " + MaterialQuery;
            }

            var SupplierQuery = GetSupplierQuery();
            if (!string.IsNullOrEmpty(SupplierQuery))
            {
                Query = Query + " AND " + SupplierQuery;
            }

            var ShiftQuery = GetShiftQuery();
            if (!string.IsNullOrEmpty(ShiftQuery))
            {
                Query = Query + " AND " + ShiftQuery;
            }

            var VehicleNoQuery = GetVehicleNoQuery();
            if (!string.IsNullOrEmpty(VehicleNoQuery))
            {
                Query = Query + " AND " + VehicleNoQuery;
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
            var Queries = Query.Split(' ');
            if (Queries.Length > 0 && Queries[Queries.Length - 1] == "WHERE")
            {
                Query = Query.Replace("WHERE", "");
            }
            cmd.CommandText = Query;

            //if (selectedOperation.Contains("LIKE"))
            //{
            //    Query += $"{selectedOperation} @SelectedFieldVal";
            //    cmd.CommandText = Query;
            //    cmd.Parameters.AddWithValue("@SelectedFieldVal", $"%" + SelectedFieldVal + $"%");
            //}
            //else if (selectedOperation == "BETWEEN")
            //{
            //    //if (SelectedField.ToLower().Contains("date"))
            //    //{

            //    //}
            //    //else
            //    //{
            //    //    Query += $"{selectedOperation} @FromFieldVal AND @ToFieldVal";
            //    //}
            //    Query += $"{selectedOperation} @FromFieldVal AND @ToFieldVal";
            //    cmd.CommandText = Query;
            //    cmd.Parameters.AddWithValue("@FromFieldVal", FromFieldVal);
            //    cmd.Parameters.AddWithValue("@ToFieldVal", ToFieldVal);
            //}
            //else
            //{
            //    Query += $"{selectedOperation} @SelectedFieldVal";
            //    cmd.CommandText = Query;
            //    cmd.Parameters.AddWithValue("@SelectedFieldVal", SelectedFieldVal);
            //}

            DataTable table = masterDBCall.GetData(cmd, CommandType.Text);
            string JSONString = JsonConvert.SerializeObject(table);
            //transactions = JsonConvert.DeserializeObject<List<Transaction>>(JSONString);
            if (table != null && table.Rows.Count > 0)
            {
                columnNames = new Dictionary<string, string>();
                if(savedReportTemplateFields!=null && savedReportTemplateFields.Count > 0)
                {
                    foreach (var tempFields in savedReportTemplateFields)
                    {
                        if (captions != null)
                        {
                            var cap = captions.FirstOrDefault(x => x.TableName == tempFields.TableName && x.FieldName == tempFields.FieldName);
                            if (cap != null && !string.IsNullOrEmpty(cap.CaptionName))
                            {
                                columnNames.Add(tempFields.FieldName, cap.CaptionName);
                            }
                            else
                            {
                                columnNames.Add(tempFields.FieldName, tempFields.FieldName);
                            }
                        }
                        else
                        {
                            columnNames.Add(tempFields.FieldName, tempFields.FieldName);
                        }
                       
                    }

                    ExecuteDynamicDialog(table);
                }
                
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No records found");
            }
            //MaterialGrid5.ItemsSource = transactions;
            //MaterialGrid5.Items.Refresh();
        }


        public void BuildAndExecuteCustomizedQuery(SavedReportTemplate SelectedSavedReportTemplate)
        {
            //string Query = "SELECT * FROM [dbo].[Transaction] WHERE IsDeleted=0";
            string Query = "";
            if (SelectedSavedReportTemplate.TableName == "Transaction")
            {
                Query = $"{SelectedSavedReportTemplate.Query} WHERE [{SelectedSavedReportTemplate.TableName}].IsDeleted=0";
            }
            else
            {
                Query = $"{SelectedSavedReportTemplate.Query} WHERE";
            }

            if (DateRadioButton.IsChecked.HasValue && DateRadioButton.IsChecked.Value)
            {

                var DatePickerQuery = GetDatePickerQuery(SelectedSavedReportTemplate);
                if (!string.IsNullOrEmpty(DatePickerQuery))
                {
                    Query = Query + " AND " + DatePickerQuery;
                }
            }
            else if (RangeRadioButton.IsChecked.HasValue && RangeRadioButton.IsChecked.Value)
            {
                var RangeQuery = GetRangeQuery();
                if (!string.IsNullOrEmpty(RangeQuery))
                {
                    Query = Query + " AND " + RangeQuery;
                }
            }
            GetValuesFromFields();

            foreach (var field in CustomFieldsBuilder)
            {
                if (field.FieldName.Contains("TicketNo"))
                {
                    var CommonName = "TicketNo";
                    var field1 = CustomFieldsBuilder.FirstOrDefault(x => x.FieldName == $"{CommonName}Criteria");
                    var field2 = CustomFieldsBuilder.FirstOrDefault(x => x.FieldName == $"{CommonName}Value");
                    var field3 = CustomFieldsBuilder.FirstOrDefault(x => x.FieldName == $"{CommonName}From");
                    var field4 = CustomFieldsBuilder.FirstOrDefault(x => x.FieldName == $"{CommonName}To");

                    if (!field1.IsUsed)
                    {
                        var selectedOperation = field1.Value?.ToString();
                        var selectedValue = field2.Value?.ToString();
                        var selectedFromValue = field3.Value?.ToString();
                        var selectedToValue = field4.Value?.ToString();
                        if (!string.IsNullOrEmpty(selectedOperation))
                        {
                            if (selectedOperation.Contains("LIKE") && !string.IsNullOrEmpty(selectedValue))
                            {
                                Query += $" AND [{field.FieldTable}].[{CommonName}] {selectedOperation} '%{selectedValue}%'";
                            }
                            else if (selectedOperation == "BETWEEN")
                            {
                                if (!string.IsNullOrEmpty(selectedFromValue) && !string.IsNullOrEmpty(selectedToValue))
                                {
                                    Query = Query + " AND " + $"[{field.FieldTable}].[{CommonName}] BETWEEN {selectedFromValue} AND {selectedToValue}";
                                }
                                else if (!string.IsNullOrEmpty(selectedFromValue))
                                {
                                    Query = Query + " AND " + $"[{field.FieldTable}].[{CommonName}]>={selectedFromValue}";
                                }
                                else if (!string.IsNullOrEmpty(selectedToValue))
                                {
                                    Query = Query + " AND " + $"[{field.FieldTable}].[{CommonName}]<={selectedToValue}";
                                }

                            }
                            else if (!string.IsNullOrEmpty(selectedValue))
                            {
                                Query += $" AND [{field.FieldTable}].[{CommonName}] {selectedOperation} {selectedValue}";
                            }
                        }
                    }
                    field1.IsUsed = true;
                    field2.IsUsed = true;
                    field3.IsUsed = true;
                    field4.IsUsed = true;
                }
                else
                {

                    if (field.FieldType == "int" && (field.FieldName.Contains("From") || field.FieldName.Contains("To")))
                    {
                        if (!string.IsNullOrEmpty(field.Value?.ToString()))
                        {
                            var FieldName = "";
                            if (field.FieldName.Contains("From") && !field.IsUsed)
                            {
                                FieldName = field.FieldName.Replace("From", "");
                                var field1 = CustomFieldsBuilder.FirstOrDefault(x => x.FieldName == $"{FieldName}To");
                                if (field1 != null && !string.IsNullOrEmpty(field1.Value?.ToString()))
                                {
                                    field1.IsUsed = true;
                                    Query = Query + " AND " + $"[{field.FieldTable}].[{FieldName}] BETWEEN {field.Value} AND {field1.Value}";
                                }
                                else
                                {
                                    Query = Query + " AND " + $"[{field.FieldTable}].[{FieldName}]>='{field.Value}'";
                                }
                                field.IsUsed = true;
                            }
                            else if (field.FieldName.Contains("To") && !field.IsUsed)
                            {
                                FieldName = field.FieldName.Replace("To", "");
                                var field1 = CustomFieldsBuilder.FirstOrDefault(x => x.FieldName == $"{FieldName}From");
                                if (field1 != null && !string.IsNullOrEmpty(field1.Value?.ToString()))
                                {
                                    field1.IsUsed = true;
                                    Query = Query + " AND " + $"[{field.FieldTable}].[{FieldName}] BETWEEN {field1.Value} AND {field.Value}";
                                }
                                else
                                {
                                    Query = Query + " AND " + $"[{field.FieldTable}].[{FieldName}]<='{field.Value}'";
                                }
                                field.IsUsed = true;
                            }
                        }

                    }

                    else if (!string.IsNullOrEmpty(field.Value?.ToString()))
                    {
                        Query = Query + " AND " + $"[{field.FieldTable}].[{field.FieldName}]='{field.Value}'";
                    }
                }
            }


            SqlCommand cmd = new SqlCommand();

            var Queries = Query.Split(' ');
            if (Queries.Length > 0 && Queries[Queries.Length - 1] == "WHERE")
            {
                Query = Query.Replace("WHERE", "");
            }

            cmd.CommandText = Query;
            DataTable table = masterDBCall.GetData(cmd, CommandType.Text);
            string JSONString = JsonConvert.SerializeObject(table);
            //transactions = JsonConvert.DeserializeObject<List<Transaction>>(JSONString);
            if (table != null && table.Rows.Count > 0)
            {
                columnNames = new Dictionary<string, string>();
                foreach (var tempFields in savedReportTemplateFields)
                {
                    var cap = captions.FirstOrDefault(x => x.TableName == tempFields.TableName && x.FieldName == tempFields.FieldName);
                    if (cap != null && !string.IsNullOrEmpty(cap.CaptionName))
                    {
                        columnNames.Add(tempFields.FieldName, cap.CaptionName);
                    }
                    else
                    {
                        columnNames.Add(tempFields.FieldName, tempFields.FieldName);
                    }
                }

                ExecuteDynamicDialog(table);
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No records found");
                CustomFieldsBuilder.ForEach(x => x.IsUsed = false);
            }
        }

        private async void ExecuteDynamicDialog(DataTable dt)
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            //var rows = dt.AsEnumerable().Distinct(System.Data.DataRowComparer.Default).ToList();
            //dt.Rows.Clear();
            //foreach (var row in rows)
            //{
            //    dt.Rows.Add(row);
            //}
            //if (rows.Any())
            //{
            //    DataTable dt1 = rows.CopyToDataTable();
            //}
            dt = dt.DefaultView.ToTable(true);
            bool IsGroupByChecked = false;
            string GroupByColumn1 = null, GroupByColumn2 = null, GroupByColumn3 = null;
            if (GroupbyRadioBtn.IsChecked.HasValue && GroupbyRadioBtn.IsChecked.Value)
            {
                IsGroupByChecked = true;
                GroupByColumn1 = GroupCondtion1.SelectedValue?.ToString();
                GroupByColumn2 = GroupCondtion2.SelectedValue?.ToString();
                GroupByColumn3 = GroupCondtion3.SelectedValue?.ToString();

                //string JSONString = JsonConvert.SerializeObject(dt);
                //var lis = JsonConvert.DeserializeObject<List<dynamic>>(JSONString);
                //lis = lis.Distinct().ToList();
                //CreateDynamicGroups(dt, lis, GroupByColumn1, GroupByColumn2, GroupByColumn3);
            }
            var view = new DynamicReport(dt, columnNames, IsGroupByChecked, GroupByColumn1, GroupByColumn2, GroupByColumn3);

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);

            ClearForms();
        }


        public void CreateDynamicGroups(DataTable dt, List<dynamic> lis, string GroupByColumn1 = null, string GroupByColumn2 = null, string GroupByColumn3 = null)
        {
            IEnumerable<IGrouping<object, dynamic>> list = new List<IGrouping<object, dynamic>>();
            if (!string.IsNullOrEmpty(GroupByColumn1))
            {
                if (!string.IsNullOrEmpty(GroupByColumn2))
                {
                    if (!string.IsNullOrEmpty(GroupByColumn3))
                    {
                        var dt1 = dt.AsEnumerable()
                       .GroupBy(r => new { Column1 = r[$"{GroupByColumn1}"], Column2 = r[$"{GroupByColumn2}"], Column3 = r[$"{GroupByColumn3}"] })
                       .Select(g => g).ToList();
                        CreateAndRegisterDynamicGroups(dt1);

                    }
                    else
                    {
                        var dt1 = dt.AsEnumerable()
                       .GroupBy(r => new { Column1 = r[$"{GroupByColumn1}"], Column2 = r[$"{GroupByColumn2}"] })
                       .Select(g => g).ToList();
                        CreateAndRegisterDynamicGroups(dt1);
                    }
                }
                else
                {
                    var dt1 = dt.AsEnumerable()
                      .GroupBy(r => new { Column1 = r[$"{GroupByColumn1}"] })
                      .Select(g => g).ToList();
                    CreateAndRegisterDynamicGroups(dt1);
                }
                //CreateAndRegisterDynamicGroups(list);
            }
        }

        public void CreateAndRegisterDynamicGroups(IEnumerable<IGrouping<dynamic, dynamic>> groupings)
        {
            foreach (var grouping in groupings)
            {
                CreateDynamicDataGrid(grouping);
            }
        }

        public void CreateDynamicDataGrid(IGrouping<object, dynamic> group)
        {
            var datagrid = new DataGrid();
            DataTable dt1 = new DataTable();
            datagrid.CanUserAddRows = false;
            datagrid.AutoGenerateColumns = false;
            datagrid.Margin = new Thickness(10);
            datagrid.IsReadOnly = true;
            foreach (var colName in columnNames)
            {
                datagrid.Columns.Add(new MaterialDesignThemes.Wpf.DataGridTextColumn
                {
                    // bind to a dictionary property
                    Binding = new Binding(colName.Key),
                    Header = colName.Value,
                });
                dt1.Columns.Add(colName.Key);
            }
            string heading = "";
            var x = group.ToList();
            var key = group.Key?.ToString();
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

            foreach (dynamic d in x)
            {
                var y = d as DataRow;
                dt1.Rows.Add(y.ItemArray);
            }

            Random r = new Random();
            var rNumber = "text_" + r.Next(1000, 9999).ToString();

            RegisterName($"{rNumber}", datagrid);
            //DynamicReportPanel.Children.Add(datagrid);

        }

        //private static object GetPropertyValue(object obj, string propertyName)
        //{
        //    return obj.GetType().GetProperty(propertyName).GetValue(obj, null);
        //}
        //public static object GetPropertyValue(dynamic obj, string propName)
        //{
        //    return obj.GetType().GetProperty(propName).GetValue(obj, null);
        //}

        public void ClearForms()
        {
            TemplateComboBox.SelectedIndex = -1;
            FromDatePicker.Text = "";
            ToDatePicker.Text = "";
            RangeComboBox.SelectedIndex = -1;
            TransactionTypeComboBox.SelectedIndex = -1;
            MaterialComboBox.SelectedIndex = -1;
            SupplierComboBox.SelectedIndex = -1;
            ShiftComboBox.SelectedIndex = -1;
            TruckNoText.Text = "";
            ResetFields();
            HideCustomizedFilter();
            ShowGeneralFilter();
            GroupbyRadioBtn.IsEnabled = false;
            GroupbyRadioBtn.IsChecked = false;
            HideGroupByButton();
            ClearGroupByButton();
            foreach (var temp in CustomFieldsBuilder)
            {
                if (FindName($"c_{temp.RegName}") != null)
                    UnregisterName($"c_{temp.RegName}");
            }
            foreach (var temp in RegisteredNames)
            {
                if (FindName($"c_{temp}") != null)
                    UnregisterName($"c_{temp}");
            }
            RegisteredNames = new List<string>();
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            var a = eventArgs?.Parameter;
            if (a != null)
            {
                FieldData = a.ToString();
                //.ItemsSource = FieldData;

            }

        }

        public string GetDatePickerQuery(SavedReportTemplate SelectedSavedReportTemplate)
        {
            string Query = "";
            try
            {
                if (FromDatePicker.SelectedDate.HasValue && ToDatePicker.SelectedDate.HasValue)
                {
                    var FromDateValue = FromDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
                    var ToDateValue = ToDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
                    Query += $"CAST([{SelectedSavedReportTemplate.TableName}].[Date] as date) BETWEEN '{FromDateValue}' AND '{ToDateValue}'";
                }
                else if (FromDatePicker.SelectedDate.HasValue)
                {
                    var FromDateValue = FromDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
                    Query += $"CAST([{SelectedSavedReportTemplate.TableName}].[Date] as date) >='{FromDateValue}'";
                }
                else if (ToDatePicker.SelectedDate.HasValue)
                {
                    var ToDateValue = ToDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
                    Query += $"CAST([{SelectedSavedReportTemplate.TableName}].[Date] as date) <='{ToDateValue}'";
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/GetSavedTemplates/Exception:- " + ex.Message, ex);
            }
            return Query;
        }
        public string GetRangeQuery()
        {
            string Query = "";
            try
            {
                var SelectedRange = RangeComboBox.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(SelectedRange))
                {
                    switch (SelectedRange)
                    {
                        case "Week":
                            Query += "[Date] >= dateadd(day, 1-datepart(dw, GETDATE()), CONVERT(date,GETDATE())) AND WorkDate <  dateadd(day, 8-datepart(dw, GETDATE()), CONVERT(date,GETDATE()))";
                            break;
                        case "Month":
                            Query += "MONTH([Date]) = MONTH(GETDATE()) AND YEAR([Date]) = YEAR(GETDATE())";
                            break;
                        case "Quarter":
                            Query += "[Date] >= DATEADD(QUARTER, DATEDIFF(QUARTER, 0, GETDATE()-1), 0) AND [Date] <=  CAST(GETDATE() AS DATE)";
                            break;
                        case "Year":
                            Query += "YEAR([Date]) = YEAR(GETDATE())";
                            break;
                        default:
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/GetRangeQuery/Exception:- " + ex.Message, ex);
            }
            return Query;
        }

        public string GetTransactionTypeQuery()
        {
            string Query = "";
            try
            {
                var selectedVal = TransactionTypeComboBox.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(selectedVal))
                {
                    Query += $"[TransactionType] = '{selectedVal}'";
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/GetTransactionTypeQuery/Exception:- " + ex.Message, ex);
            }
            return Query;
        }

        public string GetMaterialQuery()
        {
            string Query = "";
            try
            {
                var selectedVal = MaterialComboBox.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(selectedVal))
                {
                    Query += $"MaterialName = '{selectedVal}'";
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/GetMaterialQuery/Exception:- " + ex.Message, ex);
            }
            return Query;
        }
        public string GetSupplierQuery()
        {
            string Query = "";
            try
            {
                var selectedVal = SupplierComboBox.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(selectedVal))
                {
                    Query += $"SupplierName = '{selectedVal}'";
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/GetSupplierQuery/Exception:- " + ex.Message, ex);
            }
            return Query;
        }
        public string GetShiftQuery()
        {
            string Query = "";
            try
            {
                var selectedVal = ShiftComboBox.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(selectedVal))
                {
                    Query += $"Shift = '{selectedVal}'";
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/GetShiftQuery/Exception:- " + ex.Message, ex);
            }
            return Query;
        }
        public string GetVehicleNoQuery()
        {
            string Query = "";
            try
            {
                var selectedVal = TruckNoText.Text;
                if (!string.IsNullOrEmpty(selectedVal))
                {
                    Query += $"VehicleNo = '{selectedVal}'";
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/GetVehicleNoQuery/Exception:- " + ex.Message, ex);
            }
            return Query;
        }

        private void GenerateBtn_Click(object sender, RoutedEventArgs e)
        {
            selectedSavedReportTemplate = TemplateComboBox.SelectedItem as SavedReportTemplate;
            if (selectedSavedReportTemplate != null && !string.IsNullOrEmpty(selectedSavedReportTemplate.Query))
            {
                if (selectedSavedReportTemplate.WhereEnabled)
                {
                    BuildAndExecuteCustomizedQuery(selectedSavedReportTemplate);
                }
                else
                {
                    BuildAndExecuteQuery(selectedSavedReportTemplate);
                }
            }

        }

        private void RangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //e.Handled = true;
        }

        private void TransactionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // e.Handled = true;
        }

        private void MaterialComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //e.Handled = true;
        }

        private void SupplierComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //e.Handled = true;
        }

        private void ShiftComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //e.Handled = true;
        }

        private async void CreateTemplateBtn_Click(object sender, RoutedEventArgs e)
        {
            await OpenCreateTemplateDialog();
        }

        private async void CreateCaptionBtn_Click(object sender, RoutedEventArgs e)
        {
            await OpenCreateCaptionDialog();
        }

        private async Task OpenCreateTemplateDialog(SavedReportTemplate savedReportTemplate = null)
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new CreateTemplate(savedReportTemplate);

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            if (result != null)
            {
                if ((bool)result)
                {
                    GetSavedTemplates();
                    SetSavedTemplateTable();
                }
            }
            

        }
        private async Task OpenCreateCaptionDialog()
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new CreateCaption();

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);


        }

        private async void Delete_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
                var TemplateToDelete = image.DataContext as SavedReportTemplate;
                if (TemplateToDelete != null && TemplateToDelete.TemplateID != 0)
                {
                    var res = await OpenConfirmationDialog(TemplateToDelete);
                    if (res)
                    {
                        var result = commonFunction.DeleteTemplate(TemplateToDelete);
                        if (result)
                        {
                            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Templated deleted successfully");
                            GetSavedTemplates();
                            SetSavedTemplateTable();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/Delete_MouseLeftButtonDown/Exception:- " + ex.Message, ex);
            }
        }
        public async Task<bool> OpenConfirmationDialog(SavedReportTemplate savedReportTemplateToDelete)
        {
            var view = new ConfirmationDialog($"Delete the template {savedReportTemplateToDelete.ReportName}");

            //    //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            return (bool)result;
        }

        //public void ShowMessage(Action<string> message, string name)
        //{
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        LastMessage = name;
        //        message(LastMessage);
        //    });
        //}

        private void DateRadioButton1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DateRadioButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DateBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DateRadioButton.IsChecked = true;
        }

        private void GroupbyRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (GroupbyRadioBtn.IsChecked.HasValue && GroupbyRadioBtn.IsChecked.Value)
            {
                ShowGroupByButton();
                LoadGroupByButtopnOptions();
            }
            else
            {
                HideGroupByButton();
                ClearGroupByButton();
            }
        }

        private void GroupCondtion1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //e.Handled = true;

            if (!string.IsNullOrEmpty(GroupCondtion1.SelectedValue?.ToString()))
            {
                GroupCondtion2.IsEnabled = true;
            }
            else
            {
                GroupCondtion2.IsEnabled = false;

            }
        }

        private void GroupCondtion2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //e.Handled = true;

            if (!string.IsNullOrEmpty(GroupCondtion2.SelectedValue?.ToString()))
            {
                GroupCondtion3.IsEnabled = true;
            }
            else
            {
                GroupCondtion3.IsEnabled = false;
            }
        }

        private void GroupCondtion3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //e.Handled = true;

        }

        public void ShowGroupByButton()
        {
            GroupCondtion1.Visibility = Visibility.Visible;
            GroupCondtion2.Visibility = Visibility.Visible;
            GroupCondtion3.Visibility = Visibility.Visible;
        }

        public void LoadGroupByButtopnOptions()
        {
            GroupCondtion1.ItemsSource = savedReportTemplateFields;
            GroupCondtion1.Items.Refresh();
            GroupCondtion2.ItemsSource = savedReportTemplateFields;
            GroupCondtion2.Items.Refresh();
            GroupCondtion3.ItemsSource = savedReportTemplateFields;
            GroupCondtion3.Items.Refresh();

        }

        public void HideGroupByButton()
        {
            GroupCondtion1.Visibility = Visibility.Collapsed;
            GroupCondtion2.Visibility = Visibility.Collapsed;
            GroupCondtion3.Visibility = Visibility.Collapsed;
        }

        public void ClearGroupByButton()
        {
            GroupCondtion1.SelectedIndex = -1;
            GroupCondtion2.SelectedIndex = -1;
            GroupCondtion3.SelectedIndex = -1;
            GroupCondtion2.IsEnabled = false;
            GroupCondtion3.IsEnabled = false;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearForms();
        }
        private async void EditTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
                //var TemplateToEdit = image.DataContext as SavedReportTemplate;

                //SavedReportTemplate obj = ((FrameworkElement)sender).DataContext as SavedReportTemplate;
                //DataRowView dataRowView = (DataRowView)((Button)e.Source).DataContext;
                //var TemplateToEdit = SavedTemplateGrid.SelectedItem as SavedReportTemplate;
                //e.Handled = true;
                System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
                var TemplateToEdit = button.CommandParameter as SavedReportTemplate;

                if (TemplateToEdit != null && TemplateToEdit.TemplateID != 0)
                {
                    await OpenCreateTemplateDialog(TemplateToEdit);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/DeleteTemplate_Click/Exception:- " + ex.Message, ex);
            }


        }

        private async void DeleteTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
                //var TemplateToDelete = image.DataContext as SavedReportTemplate;
                //var TemplateToDelete = SavedTemplateGrid.SelectedItem as SavedReportTemplate;
                System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
                var TemplateToDelete = button.CommandParameter as SavedReportTemplate;
                if (TemplateToDelete != null && TemplateToDelete.TemplateID != 0)
                {
                    var res = await OpenConfirmationDialog(TemplateToDelete);
                    if (res)
                    {
                        var result = commonFunction.DeleteTemplate(TemplateToDelete);
                        if (result)
                        {
                            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Templated deleted successfully");
                            GetSavedTemplates();
                            SetSavedTemplateTable();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/DeleteTemplate_Click/Exception:- " + ex.Message, ex);
            }
        }


        private void LaunchBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenReportApplication();
        }

        private void OpenReportApplication()
        {
            Process _process = new Process();
            ProcessStartInfo psi = new ProcessStartInfo(reportApp);
            _process = Process.Start(psi);
            //ReportDesignerWindow reportWindow = new ReportDesignerWindow();
            //reportWindow.Show();
            //try
            //{
            //    Process p = new Process();
            //    ProcessStartInfo s = new ProcessStartInfo(reportApp)
            //    {
            //        UseShellExecute = true
            //    };
            //    p.StartInfo = s;
            //    s.WindowStyle = ProcessWindowStyle.Minimized;
            //    s.CreateNoWindow = true;
            //    p.Start();
            //}
            //catch (Exception ex)
            //{
            //    WriteLog.WriteToFile("MainWindow/OpenReportApplication/Exception:- " + ex.Message, ex);
            //}
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
            //e.Handled = true;
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


        //        private void BindComboBox5(ComboBox cbo)
        //        {
        //            AdminDBCall db = new AdminDBCall();
        //            PendingVehicle list = new PendingVehicle();
        //            List<PendingVehicle> authors = new List<PendingVehicle>();
        //            DataTable dt1 = db.GetAllData("select COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'Transaction'");
        //            if (dt1 != null && dt1.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt1.Rows)
        //                {
        //                    list.VehicleNumber = (row["COLUMN_NAME"].ToString());
        //                    cbo.Items.Add(list.VehicleNumber);
        //                    authors.Add(new PendingVehicle()
        //                    {
        //                        VehicleNumber = list.VehicleNumber,
        //                    });
        //                }
        //            }

        //        }
        //        private void BindComboBox7(ComboBox l2)
        //        {
        //            AdminDBCall db = new AdminDBCall();
        //            PendingVehicle list = new PendingVehicle();
        //            List<PendingVehicle> authors = new List<PendingVehicle>();
        //            DataTable dt1 = db.GetAllData("select COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'Transaction'");
        //            if (dt1 != null && dt1.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt1.Rows)
        //                {
        //                    list.VehicleNumber = (row["COLUMN_NAME"].ToString());
        //                    cbo.Items.Add(list.VehicleNumber);
        //                    authors.Add(new PendingVehicle()
        //                    {
        //                        VehicleNumber = list.VehicleNumber,
        //                    });
        //                }
        //            }

        //            //int count = l2.Items.Count - 1;
        //            //for (int i = count; i > 0; i--)
        //            //    l2.Items.RemoveAt(i);
        //            //BindComboBox5(cbo);

        //        }
        //        private void BindComboBox4(ComboBox comboBoxZone4)
        //        {
        //            AdminDBCall db = new AdminDBCall();
        //            PendingVehicle list = new PendingVehicle();
        //            List<PendingVehicle> authors = new List<PendingVehicle>();
        //            DataTable dt1 = db.GetAllData("SELECT * FROM [dbo].[Supplier_Master]");
        //            if (dt1 != null && dt1.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt1.Rows)
        //                {
        //                    list.VehicleNumber = (row["SupplierCode"].ToString());
        //                    l2.Items.Add(list.VehicleNumber);
        //                    authors.Add(new PendingVehicle()
        //                    {
        //                        VehicleNumber = list.VehicleNumber,
        //                    });
        //                }
        //            }
        //        }

        //        private void BindComboBox3(ComboBox comboBoxZone3)
        //        {
        //            AdminDBCall db = new AdminDBCall();
        //            PendingVehicle list = new PendingVehicle();
        //            List<PendingVehicle> authors = new List<PendingVehicle>();
        //            DataTable dt1 = db.GetAllData("SELECT * FROM [dbo].[Vehicle_Master]");
        //            if (dt1 != null && dt1.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt1.Rows)
        //                {
        //                    list.VehicleNumber = (row["VehicleNumber"].ToString());
        //                    comboBoxZone3.Items.Add(list.VehicleNumber);
        //                    authors.Add(new PendingVehicle()
        //                    {
        //                        VehicleNumber = list.VehicleNumber,
        //                    });
        //                }
        //            }
        //        }

        //        private void BindComboBox2(ComboBox comboBoxZone2)
        //        {
        //            AdminDBCall db = new AdminDBCall();
        //            PendingVehicle list = new PendingVehicle();
        //            List<PendingVehicle> authors = new List<PendingVehicle>();
        //            DataTable dt1 = db.GetAllData("SELECT * FROM Material_Master");
        //            if (dt1 != null && dt1.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt1.Rows)
        //                {
        //                    list.VehicleNumber = (row["MaterialCode"].ToString());
        //                    comboBoxZone2.Items.Add(list.VehicleNumber);
        //                    authors.Add(new PendingVehicle()
        //                    {
        //                        VehicleNumber = list.VehicleNumber,
        //                    });
        //                }
        //            }
        //        }

        //        private void BindComboBox1(ComboBox comboBoxZone1)
        //        {
        //            AdminDBCall db = new AdminDBCall();
        //            PendingVehicle list = new PendingVehicle();
        //            List<PendingVehicle> authors = new List<PendingVehicle>();
        //            DataTable dt1 = db.GetAllData("SELECT * FROM [dbo].[Transaction]");
        //            if (dt1 != null && dt1.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt1.Rows)
        //                {
        //                    list.VehicleNumber = (row["Shift"].ToString());
        //                    comboBoxZone1.Items.Add(list.VehicleNumber);
        //                    authors.Add(new PendingVehicle()
        //                    {
        //                        VehicleNumber = list.VehicleNumber,
        //                    });
        //                }
        //            }
        //        }

        //        private void BindComboBox(ComboBox ComboBoxZone)
        //        {
        //            if (ComboBoxZone.SelectedItem != null)
        //            {

        //                ComboBoxItem cbi = (ComboBoxItem)ComboBoxZone.SelectedItem;
        //                string selectedText = cbi.Content.ToString();
        //                TransactionType = selectedText;
        //                Console.WriteLine(TransactionType);

        //            }
        //        }
        //        private void BindComboBox6(ComboBox Template)
        //        {
        //            AdminDBCall db = new AdminDBCall();
        //            PendingVehicle list = new PendingVehicle();
        //            List<PendingVehicle> authors = new List<PendingVehicle>();
        //            DataTable dt1 = db.GetAllData("SELECT * FROM Save_Reports");
        //            if (dt1 != null && dt1.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt1.Rows)
        //                {
        //                    list.VehicleNumber = (row["ReportName"].ToString());
        //                    Template.Items.Add(list.VehicleNumber);
        //                    authors.Add(new PendingVehicle()
        //                    {
        //                        VehicleNumber = list.VehicleNumber,
        //                    });
        //                }
        //            }
        //            if (Template.Text != "")
        //            {
        //                showGrid.Visibility = Visibility.Visible;
        //            }
        //        }
        //public class PendingVehicle
        //        {
        //            public string VehicleNumber;
        //        }

        //        private void Button_Click(object sender, RoutedEventArgs e)
        //        {

        //            SqlConnection cn = new SqlConnection(@"Data Source=192.168.0.28,1434;Initial Catalog=IWT;Persist Security Info=True;User ID=essae;Password=essae@123");
        //            cn.Open();
        //            SqlCommand check_User_Name = new SqlCommand("SELECT * FROM [Save_FieldName] WHERE ReportName='" + Template.Text + "'", cn);
        //            check_User_Name.Parameters.AddWithValue("@ReportName", Template.Text);
        //            //SqlDataReader reader = check_User_Name.ExecuteReader();
        //            SqlDataAdapter da = new SqlDataAdapter(check_User_Name);
        //            DataTable dt = new DataTable();
        //            da.Fill(dt);


        //                foreach (DataRow row in dt.Rows)
        //                {

        //                 QueryName = row.ItemArray[0].ToString();
        //                stringList.Add(QueryName);
        //                DataGrid d1 = new DataGrid();
        //                DataGridTextColumn t1 = new DataGridTextColumn();

        //                t1.Header = QueryName;
        //                d1.Columns.Add(t1);
        //                GetData();
        //                // grid.Children.Add(d1);




        //            }


        //        }
        //        private void GetData()
        //        {
        //            SqlConnection cn = new SqlConnection(@"Data Source=192.168.0.28,1434;Initial Catalog=IWT;Persist Security Info=True;User ID=essae;Password=essae@123");
        //            AdminDBCall db = new AdminDBCall();

        //            SqlCommand cmd = new SqlCommand("SELECT * FROM [Save_Reports] WHERE ReportName='" + Template.Text + "'", cn);
        //            cmd.Parameters.AddWithValue("@ReportName", Template.Text);


        //            SqlDataAdapter da = new SqlDataAdapter(cmd);
        //            DataTable dt = new DataTable();
        //            da.Fill(dt);
        //            SqlCommand cmd1 = new SqlCommand();
        //            SqlCommand select = new SqlCommand();
        //            TextBox dynamicTextBox = new TextBox();
        //            dynamicTextBox.Text = "TN 21 SE 1234";
        //            foreach (DataRow row in dt.Rows)
        //            {

        //                TemplateName = row.ItemArray[1].ToString();
        //                string Queryname1 = row.ItemArray[2].ToString();
        //                form1 = row.ItemArray[3].ToString();

        //                if (stringList.Count == 1)
        //                {

        //                    if (RangePicker == "Week")
        //                    {
        //                        cmd = new SqlCommand("select * from [dbo].[Transaction] where Date >= DATEADD(day, -5, GETDATE())", cn);
        //                    }
        //                    if (RangePicker == "Month")
        //                    {

        //                        cmd = new SqlCommand("select * from [dbo].[Transaction] where Date >= datefromparts(year(getdate()), month(getdate()), 1)", cn);
        //                    }
        //                    if (RangePicker == "Year")
        //                    {

        //                        cmd = new SqlCommand("SELECT * FROM [dbo].[Transaction] where YEAR(date) = YEAR(getdate())", cn);
        //                    }
        //                    if (RangePicker == "Quater")
        //                    {

        //                        cmd = new SqlCommand("SELECT * FROM [dbo].[Transaction] WHERE Date >= DATEADD(DAY, -90, GETDATE())", cn);
        //                    }
        //                    if (ComboBoxZone2.Text != "")
        //                    {
        //                        select = new SqlCommand(Queryname1 + "where MaterialName='" + ComboBoxZone2.Text + "'", cn);
        //                        // cmd = new SqlCommand("select * from [dbo].[Transaction] where MaterialName='" + ComboBoxZone2.Text + "'", cn);
        //                    }
        //                    if (ComboBoxZone4.Text != "")
        //                    {
        //                        select = new SqlCommand("select * from [dbo].[Transaction] where SupplierName='" + ComboBoxZone4.Text + "'", cn);
        //                    }
        //                    if (ComboBoxZone3.Text != "")
        //                    {
        //                        select = new SqlCommand(Queryname1 + " where VehicleNo='" + ComboBoxZone3.Text + "'", cn);
        //                        // cmd = new SqlCommand("select * from [dbo].[Transaction] where VehicleNo='" + ComboBoxZone3.Text + "'", cn);
        //                    }
        //                    if (tbSettingText.Text != "" && text1.Text != "")
        //                    {
        //                        cmd = new SqlCommand("select * from [dbo].[Transaction] where Date between  '" + tbSettingText.Text + "' and '" + text1.Text + "'", cn);
        //                    }
        //                    if (ComboBoxZone.Text != "")
        //                    {
        //                        select = new SqlCommand(Queryname1 + "where TransactionType='" + ComboBoxZone.Text + "'", cn);
        //                    }

        //                    if (dynamicTextBox.Text != "")
        //                    {
        //                        select = new SqlCommand(Queryname1 + "where VehicleNo='" + dynamicTextBox.Text + "'", cn);

        //                    }
        //                }
        //                else
        //                {
        //                   select = new SqlCommand(Queryname1 + "where TransactionType='" + ComboBoxZone.Text + "' and  VehicleNo='" + ComboBoxZone3.Text + "' and MaterialName='" + ComboBoxZone2.Text + "'", cn);
        //                   // cmd = new SqlCommand(Queryname1 + "where TransactionType='" + ComboBoxZone.Text + "' and  VehicleNo='" + ComboBoxZone3.Text + "'", cn);
        //                }
        //            }

        //            SqlDataAdapter da1 = new SqlDataAdapter(select);
        //            DataTable dt1 = new DataTable();
        //            da1.Fill(dt1);
        //            ExecuteDynamicDialog(dt1);
        //             var dataGrid = new DataGrid { ItemsSource = dt1.DefaultView };
        //            //DataGridContainer.Items.Add(dataGrid);


        //            Console.WriteLine(cmd);




        //        }




        //        private void ComboBoxZone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //        {
        //            if (ComboBoxZone.SelectedItem != null)
        //            {

        //                ComboBoxItem cbi = (ComboBoxItem)ComboBoxZone.SelectedItem;
        //                string selectedText = cbi.Content.ToString();
        //                TransactionType = selectedText;
        //                Console.WriteLine(TransactionType);

        //            }
        //        }


        //        private void ComboBoxZone2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //        {
        //            AdminDBCall db = new AdminDBCall();
        //            PendingVehicle list = new PendingVehicle();
        //            List<PendingVehicle> authors = new List<PendingVehicle>();
        //            DataTable dt1 = db.GetAllData("SELECT * FROM Material_Master");
        //            if (dt1 != null && dt1.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt1.Rows)
        //                {
        //                    list.VehicleNumber = (row["MaterialCode"].ToString());
        //                    ComboBoxZone2.Items.Add(list.VehicleNumber);
        //                    authors.Add(new PendingVehicle()
        //                    {
        //                        VehicleNumber = list.VehicleNumber,
        //                    });
        //                }
        //            }

        //        }

        //        private void ComboBoxZone4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //        {
        //            AdminDBCall db = new AdminDBCall();
        //            PendingVehicle list = new PendingVehicle();
        //            List<PendingVehicle> authors = new List<PendingVehicle>();
        //            DataTable dt1 = db.GetAllData("SELECT * FROM [dbo].[Supplier_Master]");
        //            if (dt1 != null && dt1.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt1.Rows)
        //                {
        //                    list.VehicleNumber = (row["SupplierCode"].ToString());
        //                    ComboBoxZone4.Items.Add(list.VehicleNumber);
        //                    authors.Add(new PendingVehicle()
        //                    {
        //                        VehicleNumber = list.VehicleNumber,
        //                    });
        //                }
        //            }
        //        }

        //        private void ComboBoxZone1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //        {
        //            AdminDBCall db = new AdminDBCall();
        //            PendingVehicle list = new PendingVehicle();
        //            List<PendingVehicle> authors = new List<PendingVehicle>();
        //            DataTable dt1 = db.GetAllData("SELECT * FROM [dbo].[Transaction]");
        //            if (dt1 != null && dt1.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt1.Rows)
        //                {
        //                    list.VehicleNumber = (row["Shift"].ToString());
        //                    ComboBoxZone1.Items.Add(list.VehicleNumber);
        //                    authors.Add(new PendingVehicle()
        //                    {
        //                        VehicleNumber = list.VehicleNumber,
        //                    });
        //                }
        //            }
        //        }

        //        private void ComboBoxZone3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //        {
        //            AdminDBCall db = new AdminDBCall();
        //            PendingVehicle list = new PendingVehicle();
        //            List<PendingVehicle> authors = new List<PendingVehicle>();
        //            DataTable dt1 = db.GetAllData("SELECT * FROM [dbo].[Vehicle_Master]");
        //            if (dt1 != null && dt1.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt1.Rows)
        //                {
        //                    list.VehicleNumber = (row["VehicleNumber"].ToString());
        //                    ComboBoxZone3.Items.Add(list.VehicleNumber);
        //                    authors.Add(new PendingVehicle()
        //                    {
        //                        VehicleNumber = list.VehicleNumber,
        //                    });
        //                }

        //            }

        //        }



        //        private void Range_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //        {
        //            if (Range.SelectedItem != null)
        //            {

        //                ComboBoxItem cbi = (ComboBoxItem)Range.SelectedItem;
        //                string selectedText = cbi.Content.ToString();
        //                RangePicker = selectedText;
        //                Console.WriteLine(RangePicker);

        //            }


        //        }

        //        private void Weighing_Click(object sender, RoutedEventArgs e)
        //        {

        //        }

        //        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //        {
        //            AdminDBCall db = new AdminDBCall();
        //            PendingVehicle list = new PendingVehicle();
        //            List<PendingVehicle> authors = new List<PendingVehicle>();
        //            DataTable dt1 = db.GetAllData("select COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'Transaction'");
        //            if (dt1 != null && dt1.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt1.Rows)
        //                {
        //                    list.VehicleNumber = (row["COLUMN_NAME"].ToString());
        //                    cbo.Items.Add(list.VehicleNumber);
        //                    authors.Add(new PendingVehicle()
        //                    {
        //                        VehicleNumber = list.VehicleNumber,
        //                    });
        //                }
        //            }
        //        }

        //        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //        {
        //            SqlConnection cn = new SqlConnection(@"Data Source=192.168.0.28,1434;Initial Catalog=IWT;Persist Security Info=True;User ID=essae;Password=essae@123");

        //            cn.Open();
        //            //AdminDBCall db = new AdminDBCall();
        //            //var tkno = _context.TicketNoSELECT IDENT_CURRENT('TableName')
        //            //  and SupplierName = '" + ComboBoxZone4.Text + "' and VehicleNo = '" + ComboBoxZone3.Text+ "'   Date between  '" + tbSettingText.Text + "' and '" + text1.Text + "' or Date = '" + RangePicker + "'
        //            SqlCommand cmd = new SqlCommand("select TABLE_NAME FROM IWT.INFORMATION_SCHEMA.TABLE", cn);
        //            SqlDataAdapter da = new SqlDataAdapter(cmd);
        //            DataTable dt = new DataTable();

        //        }
        //        private void Template_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //        {
        //            AdminDBCall db = new AdminDBCall();
        //            PendingVehicle list = new PendingVehicle();
        //            List<PendingVehicle> authors = new List<PendingVehicle>();
        //            DataTable dt1 = db.GetAllData("SELECT * FROM Save_Reports");
        //            if (dt1 != null && dt1.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt1.Rows)
        //                {

        //                        TemplateName = row.ItemArray[1].ToString();
        //                        string Queryname1 = row.ItemArray[2].ToString();
        //                        form1 = row.ItemArray[3].ToString();
        //                        list.VehicleNumber = (row["ReportName"].ToString());
        //                    Template.Items.Add(list.VehicleNumber);
        //                    authors.Add(new PendingVehicle()
        //                    {
        //                        VehicleNumber = list.VehicleNumber,
        //                    });
        //                }
        //            }
        //            if(Template.Text != "")
        //            {
        //                showGrid.Visibility = Visibility.Visible;
        //            }
        //            if (TemplateName != "" && form1 == "True")
        //            {
        //                 showGrid.Visibility = Visibility.Visible;
        //                showGrid_Loaded();
        //            }

        //        }








        //        //dynamic table


        //        //save
        //        private void Button_Click_1(object sender, RoutedEventArgs e)
        //        {
        //            ExecuteCreateTemplateDialog();
        //        }
        //        private async void ExecuteCreateTemplateDialog()
        //        {
        //            //let's set up a little MVVM, cos that's what the cool kids are doing:
        //            var view = new CreateTemplate();

        //            //show the dialog
        //            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);


        //        }
        //        private async void ExecutecaptionDialog()
        //        {
        //            //let's set up a little MVVM, cos that's what the cool kids are doing:
        //            var view = new CreateCaption();

        //            //show the dialog
        //            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);


        //        }
        //        private async void ExecuteDynamicDialog(DataTable dt)
        //        {
        //            //let's set up a little MVVM, cos that's what the cool kids are doing:
        //            var view = new DynamicReport(dt);

        //            //show the dialog
        //            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);


        //        }
        //        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        //        {
        //            var a = eventArgs?.Parameter;
        //            if (a != null)
        //            {
        //                FieldData = a.ToString();
        //                //.ItemsSource = FieldData;

        //            }

        //        }




        //        private void Template_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        //        {
        //            cmb1Border.BorderBrush = Brushes.Red;
        //        }

        //        private void Button_Click_2(object sender, RoutedEventArgs e)
        //        {
        //            ExecutecaptionDialog();
        //        }

        //        private void button_Click(object sender, RoutedEventArgs e)
        //        {
        //            //if (form1=="true")
        //            //{
        //            //    TextBox dynamicTextBox = new TextBox();
        //            //    dynamicTextBox.Text = "Type Partnumber";
        //            //    Grid.SetRow(dynamicTextBox, 1);
        //            //    Grid.SetColumn(dynamicTextBox, 0);
        //            //    this.canContainer.Children.Add(dynamicTextBox);
        //            //}
        //        }

        //        private void showGrid_Loaded()
        //        {
        //            SqlConnection cn = new SqlConnection(@"Data Source=192.168.0.28,1434;Initial Catalog=IWT;Persist Security Info=True;User ID=essae;Password=essae@123");
        //            cn.Open();
        //            SqlCommand check_User_Name = new SqlCommand("SELECT * FROM [Save_FieldName] WHERE ReportName='" + Template.Text + "'", cn);

        //            SqlDataAdapter da = new SqlDataAdapter(check_User_Name);
        //            DataTable dt = new DataTable();
        //            da.Fill(dt);


        //            foreach (DataRow row in dt.Rows)
        //            {

        //                QueryName = row.ItemArray[0].ToString();

        //            }

        //            if (TemplateName != "" && QueryName != "")
        //            {
        //                TextBox dynamicTextBox = new TextBox();
        //                dynamicTextBox.Text = "TN 21 SE 1234";

        //                Grid.SetRow(dynamicTextBox, 1);
        //                Grid.SetColumn(dynamicTextBox, 0);
        //                this.canContainer.Children.Add(dynamicTextBox);
        //            }

        //        }

        //        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        //        {

        //        }

        //        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        //        {
        //            AdminDBCall db = new AdminDBCall();
        //            PendingVehicle list = new PendingVehicle();
        //            List<PendingVehicle> authors = new List<PendingVehicle>();
        //            DataTable dt1 = db.GetAllData("select COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'Transaction'");
        //            if (dt1 != null && dt1.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt1.Rows)
        //                {
        //                    list.VehicleNumber = (row["COLUMN_NAME"].ToString());
        //                    l2.Items.Add(list.VehicleNumber);
        //                    authors.Add(new PendingVehicle()
        //                    {
        //                        VehicleNumber = list.VehicleNumber,
        //                    });
        //                }
        //            }
        //        }

        //        private void GetTemplate()
        //        {

        //            AdminDBCall db = new AdminDBCall();
        //            DataTable dt1 = db.GetAllData("SELECT ReportName FROM Save_Reports");
        //            if (dt1 != null && dt1.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt1.Rows)
        //                {
        //                    TemplateName = row.ItemArray[0].ToString();
        //                    list.Add(TemplateName);
        //                    TemplateList.ItemsSource = list;
        //                }

        //            }


        //        }


    }
}


