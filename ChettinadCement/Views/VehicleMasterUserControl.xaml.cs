using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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

namespace IWT.Views
{
    /// <summary>
    /// Interaction logic for VehicleMasterUserControl.xaml
    /// </summary>
    public partial class VehicleMasterUserControl : UserControl, INotifyPropertyChanged
    {
        public List<CustomFieldBuilder> CustomFieldsBuilder = new List<CustomFieldBuilder>();
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        private IDictionary<string, object> SelectedRow;
        private bool _isRowSelected = false;
        private AdminDBCall _dbContext;
        public static CommonFunction commonFunction = new CommonFunction();
        //public int StableWeightArraySize = int.Parse(ConfigurationManager.AppSettings["StableWeightArraySize"]);
        //public int StableWeightArraySelectable = int.Parse(ConfigurationManager.AppSettings["StableWeightArraySelectable"]);
        public int StableWeightArraySize = 10;
        public int StableWeightArraySelectable = 1;
        public List<string> GainedWeightList = new List<string>();
        private string _currentWeightment = "";

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
        public VehicleMasterUserControl(List<CustomFieldBuilder> fields)
        {
            InitializeComponent();
            _dbContext = new AdminDBCall();
            toastViewModel = new ToastViewModel();
            DataContext = this;
            SelectedRecord = 10;
            NumberOfPages = 1;
            GetStableWeightConfiguration();
            CustomFieldsBuilder = fields;
            BuildFields();
            Loaded += VehicleMasterUserControl_Loaded;
            Unloaded += VehicleMasterUserControl_Unloaded;
            PaginatorComboBox.ItemsSource = ItemPerPagesList;

        }

        private void VehicleMasterUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.onWeighmentReceived += Single_onWeighmentReceived;
        }

        private void VehicleMasterUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.onWeighmentReceived -= Single_onWeighmentReceived;
        }
        public void GetStableWeightConfiguration()
        {
            try
            {
                StableWeightConfiguration stableWeightConfiguration = commonFunction.GetStableWeightConfiguration();
                StableWeightArraySize = stableWeightConfiguration != null ? stableWeightConfiguration.StablePLCCount : 10;
                StableWeightArraySelectable = stableWeightConfiguration != null ? stableWeightConfiguration.StableWeightCount : 1;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("VehicleMaster/GetStableWeightConfiguration", ex);
            }
        }
        public void BuildFields()
        {
            foreach (CustomFieldBuilder template in CustomFieldsBuilder)
            {
                if (template.ControlType == "TextBox")
                {
                    if (template.FieldType == "DATETIME")
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
            //var dataTable = GetCustomMasterTable();
            //Grid.SetRow(dataTable, 3);
            //Grid.SetColumn(dataTable, 1);
            //mainGrid.Children.Add(dataTable);
            GetTableData();
            SetDynamicTable();

        }
        private DataGrid GetCustomMasterTable()
        {
            try
            {
                DataGrid dynamicTable = new DataGrid
                {
                    CanUserAddRows = false,
                    AutoGenerateColumns = false,
                    FontFamily = new FontFamily("Segoe UI Semibold"),
                    FontSize = 14
                };
                RegisterName("table_VehicleMaster", dynamicTable);
                dynamicTable.SelectionChanged += dynamicDataGrid_SelectionChanged;
                foreach (var field in CustomFieldsBuilder)
                {
                    var col = new System.Windows.Controls.DataGridTextColumn
                    {
                        Header = field.FieldCaption,
                        Binding = new Binding(field.FieldName),
                        IsReadOnly = true
                    };
                    dynamicTable.Columns.Add(col);
                }
                dynamicTable.ItemsSource = GetTableData();
                return dynamicTable;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetTableData:" + ex.Message);
                return new DataGrid();
            }
        }
        private void dynamicDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var s = sender as DataGrid;
            SelectedRow = s.SelectedItem as IDictionary<string, object>;
            if (SelectedRow != null)
            {
                _isRowSelected = true;
                SetValuesToField(SelectedRow);
            }
        }
        private List<dynamic> GetTableData()
        {
            Result = new List<dynamic>();
            DataTable table = _dbContext.GetAllData($"select * from [Vehicle_Master] WHERE [IsDeleted]='FALSE'");
            if (table != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    dynamic expando = new ExpandoObject();
                    AddProperty(expando, "VehicleID", row["VehicleID"]);
                    foreach (var field in CustomFieldsBuilder)
                    {
                        AddProperty(expando, field.FieldName, row[field.FieldName].ToString());
                    }
                    Result.Add(expando);
                }
            }
            TotalRecords = Result.Count;
            UpdateRecordCount();
            UpdateCollection(Result.Take(SelectedRecord));
            return Result;
        }
        public void SetDynamicTable()
        {
            try
            {
                DataGrid dynamicTable = new DataGrid
                {
                    CanUserAddRows = false,
                    AutoGenerateColumns = false,
                    FontFamily = new FontFamily("Segoe UI Semibold"),
                    FontSize = 14
                };
                //RegisterName("table_MaterialMaster1", dynamicTable);
                dynamicTable.SelectionChanged += dynamicDataGrid_SelectionChanged;
                foreach (var field in CustomFieldsBuilder)
                {
                    var col = new System.Windows.Controls.DataGridTextColumn
                    {
                        Header = field.FieldCaption,
                        Binding = new Binding(field.FieldName),
                        IsReadOnly = true
                    };
                    dynamicTable.Columns.Add(col);
                }
                dynamicTable.ItemsSource = Result1;
                TableContainer.Content = dynamicTable;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetDynamicTable:" + ex.Message);
            }
        }

        public static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }
        public void ShowMessage(Action<string> message, string name)
        {
            this.Dispatcher.Invoke(() =>
            {
                LastMessage = $"{name}";
                message(LastMessage);
            });
        }
        private void SetValuesToField(IDictionary<string, object> selectedRow)
        {
            foreach (var field in CustomFieldsBuilder)
            {
                if (field.ControlType == "TextBox")
                {
                    if (field.FieldType == "DATETIME")
                    {
                        DatePicker datePicker = (DatePicker)FindName(field.RegName);
                        var date = selectedRow[field.FieldName].ToString();
                        if (date != "")
                        {
                            datePicker.SelectedDate = DateTime.Parse(date);
                        }
                        else
                        {
                            datePicker.SelectedDate = null;
                        }
                    }
                    else
                    {
                        TextBox textBox = (TextBox)FindName(field.RegName);
                        textBox.Text = selectedRow[field.FieldName].ToString();
                    }
                }
                else if (field.ControlType == "Dropdown")
                {
                    ComboBox comboBox = (ComboBox)this.FindName(field.RegName);
                    var items = comboBox.Items;
                    int index = -1;
                    for (int i = 0; i < items.Count; i++)
                    {
                        var comboBoxItem = items[i] as ComboBoxItem;
                        if (comboBoxItem.Content.ToString() == selectedRow[field.FieldName].ToString())
                        {
                            index = i;
                            break;
                        }
                    }
                    comboBox.SelectedIndex = index;
                }
            }
        }
        private void ResetFields()
        {
            foreach (var field in CustomFieldsBuilder)
            {
                if (field.ControlType == "TextBox")
                {
                    if (field.FieldType == "DATETIME")
                    {
                        DatePicker datePicker = (DatePicker)FindName(field.RegName);
                        datePicker.SelectedDate = null;
                    }
                    else
                    {
                        TextBox textBox = (TextBox)FindName(field.RegName);
                        textBox.Text = null;
                    }
                }
                else if (field.ControlType == "Dropdown")
                {
                    ComboBox comboBox = (ComboBox)this.FindName(field.RegName);
                    comboBox.SelectedIndex = -1;
                }
            }
            //DataGrid table = (DataGrid)FindName("table_VehicleMaster");
            //table.SelectedItem = null;
            SelectedRow = null;

        }
        private void New_Button_Click(object sender, RoutedEventArgs e)
        {
            ResetFields();
            _isRowSelected = false;
        }
        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentWeightment != "0")
                {
                    if (SelectedRow == null)
                    {
                        var fieldValues = GetValuesFromFields();
                        var previousValue = Result.FirstOrDefault(t => t.VehicleNumber == fieldValues.VehicleNumber);
                        if (previousValue == null)
                        {
                            string insertQuery = $@"INSERT INTO [Vehicle_Master] (";
                            var fields = CustomFieldsBuilder;
                            foreach (var field in fields)
                            {
                                insertQuery += $"{field.FieldName},";
                            }
                            insertQuery += $"IsDeleted) VALUES (";
                            foreach (var field in fields)
                            {
                                var dict = (IDictionary<string, object>)fieldValues;
                                string val = dict[field.FieldName].ToString();
                                insertQuery += $"'{val}',";
                            }
                            insertQuery += $"'FALSE')";
                            //Debug.WriteLine(insertQuery);
                            bool res = _dbContext.ExecuteQuery(insertQuery);
                            if (res)
                            {
                                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Record Inserted !!");
                                GetTableData();
                                ResetFields();
                            }
                            else
                            {
                                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !!");
                            }
                        }
                        else
                        {
                            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Record with same Vehicle Number already exists !!");
                        }
                    }
                    else
                    {
                        var fieldValues = GetValuesFromFields();
                        string updateQuery = $@"UPDATE [Vehicle_Master] SET ";
                        var fields = CustomFieldsBuilder;
                        for (int i = 0; i < fields.Count; i++)
                        {
                            if (i < fields.Count - 1)
                            {
                                var dict = (IDictionary<string, object>)fieldValues;
                                string val = dict[fields[i].FieldName].ToString();
                                updateQuery += $"{fields[i].FieldName}='{val}',";
                            }
                            else
                            {
                                var dict = (IDictionary<string, object>)fieldValues;
                                string val = dict[fields[i].FieldName].ToString();
                                updateQuery += $"{fields[i].FieldName}='{val}'";
                            }
                        }
                        updateQuery += $"WHERE VehicleID='{SelectedRow["VehicleID"].ToString()}'";
                        var res = _dbContext.ExecuteQuery(updateQuery);
                        if (res)
                        {
                            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Record Updated !!");
                            GetTableData();
                            ResetFields();
                        }
                        else
                        {
                            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !!");
                        }
                    }
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Weight should be a positive value and greater than zero");
                }


            }
            catch (Exception ex)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, ex.Message);
            }
        }
        //private void Update_Button_Click(object sender, RoutedEventArgs e)
        //{
        //    if (SelectedRow != null)
        //    {
        //        var fieldValues = GetValuesFromFields();
        //        string updateQuery = $@"UPDATE [Vehicle_Master] SET ";
        //        var fields = CustomFieldsBuilder;
        //        for (int i = 0; i < fields.Count; i++)
        //        {
        //            if (i < fields.Count - 1)
        //            {
        //                var dict = (IDictionary<string, object>)fieldValues;
        //                string val = dict[fields[i].FieldName].ToString();
        //                updateQuery += $"{fields[i].FieldName}='{val}',";
        //            }
        //            else
        //            {
        //                var dict = (IDictionary<string, object>)fieldValues;
        //                string val = dict[fields[i].FieldName].ToString();
        //                updateQuery += $"{fields[i].FieldName}='{val}'";
        //            }
        //        }
        //        updateQuery += $"WHERE VehicleID='{SelectedRow["VehicleID"].ToString()}'";
        //        var res = _dbContext.ExecuteQuery(updateQuery);
        //        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Record Updated !!");
        //        GetTableData();
        //        ResetFields();
        //    }
        //    else
        //    {
        //        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a record !!");
        //    }
        //}
        private async void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRow != null)
            {
                var res1 = await OpenConfirmationDialog();
                if (res1)
                {
                    var res = _dbContext.DeleteMasterRow("VehicleID", SelectedRow["VehicleID"].ToString(), "Vehicle_Master");
                    if (res)
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Record Deleted !!");
                        GetTableData();
                        ResetFields();
                    }
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a record !!");
            }
        }

        public async Task<bool> OpenConfirmationDialog()
        {
            var view = new ConfirmationDialog("Delete the vehicle record");

            //    //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            return (bool)result;
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {

        }

        private dynamic GetValuesFromFields()
        {
            dynamic expando = new ExpandoObject();
            foreach (var field in CustomFieldsBuilder)
            {

                if (field.ControlType == "TextBox")
                {
                    if (field.FieldType == "DATETIME")
                    {
                        DatePicker datePicker = (DatePicker)FindName(field.RegName);
                        if (datePicker.SelectedDate.HasValue)
                        {
                            var date = (DateTime)datePicker.SelectedDate;
                            AddProperty(expando, field.FieldName, date.ToString("yyyy-MM-dd"));
                            field.Value = date.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            AddProperty(expando, field.FieldName, null);
                            field.Value = null;
                        }
                    }
                    else
                    {
                        TextBox textBox = (TextBox)FindName(field.RegName);
                        AddProperty(expando, field.FieldName, textBox.Text);
                        field.Value = textBox.Text;
                    }
                }
                else if (field.ControlType == "Dropdown")
                {
                    ComboBox comboBox = (ComboBox)this.FindName(field.RegName);
                    ComboBoxItem selected = comboBox.SelectedItem as ComboBoxItem;
                    if (selected != null)
                    {
                        AddProperty(expando, field.FieldName, selected.Content.ToString());
                        field.Value = selected.Content.ToString();
                    }
                    else
                    {
                        AddProperty(expando, field.FieldName, "");
                        field.Value = "";
                    }
                }
                if (field.IsMandatory && (field.Value == null || field.Value.ToString() == ""))
                {
                    throw new Exception("Please fill mandatory fields");
                }
            }
            return expando;
        }
        private Border CreateInputField(CustomFieldBuilder template)
        {
            Border container = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1)
            };
            StackPanel inputContainer = new StackPanel
            {
                Height = 60,
                Orientation = Orientation.Horizontal
            };
            Image customFieldIcon = new Image
            {
                Source = new BitmapImage(new Uri(template.FieldImage, UriKind.Relative)),
                Width = 50,
                Margin = new Thickness(10)
            };
            Style style = this.FindResource("MaterialDesignFloatingHintTextBox") as Style;
            TextBox customFieldTextBox = new TextBox
            {
                Width = 180,
                Margin = new Thickness(10, -15, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14,
                FontFamily = new FontFamily("Segoe UI Semibold"),
                IsEnabled = template.IsEnabled,
                Style=style
            };
            if (template.IsMandatory)
            {
                Binding binding = new Binding
                {
                    RelativeSource = RelativeSource.Self,
                    Path = new PropertyPath("Text"),
                    UpdateSourceTrigger = UpdateSourceTrigger.LostFocus
                };
                ValidationRule rule = new Validators.NotEmptyValidationRule();
                rule.ValidatesOnTargetUpdated = true;
                binding.ValidationRules.Add(rule);
                customFieldTextBox.SetBinding(TextBox.TextProperty, binding);
            }
            if (template.FieldType == "FLOAT")
            {
                customFieldTextBox.PreviewTextInput += Constant_PreviewTextInput;
            }
            if (template.FieldName == "VehicleNumber")
            {
                customFieldTextBox.PreviewTextInput += VehicleNumber_PreviewTextInput;
                customFieldTextBox.PreviewKeyDown += CustomFieldTextBox_PreviewKeyDown;
                customFieldTextBox.CharacterCasing = CharacterCasing.Upper;
                customFieldTextBox.MaxLength = 10;
            }
            RegisterName(template.RegName, customFieldTextBox);
            HintAssist.SetHint(customFieldTextBox, template.FieldCaption);
            inputContainer.Children.Add(customFieldIcon);
            inputContainer.Children.Add(customFieldTextBox);
            container.Child = inputContainer;
            return container;
        }

        private void CustomFieldTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void Constant_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
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
                Height = 60,
                Orientation = Orientation.Horizontal
            };
            Image customFieldIcon = new Image
            {
                Source = new BitmapImage(new Uri(template.FieldImage, UriKind.Relative)),
                Width = 50,
                Margin = new Thickness(10)
            };
            Style style = this.FindResource("MaterialDesignFloatingHintDatePicker") as Style;
            DatePicker customFieldDatePicker = new DatePicker
            {
                Width = 180,
                Margin = new Thickness(10, -15, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14,
                FontFamily = new FontFamily("Segoe UI Semibold"),
                IsEnabled = template.IsEnabled,
                Style=style
            };
            if (template.IsMandatory)
            {
                Binding binding = new Binding
                {
                    RelativeSource = RelativeSource.Self,
                    Path = new PropertyPath("Text"),
                    UpdateSourceTrigger = UpdateSourceTrigger.LostFocus
                };
                ValidationRule rule = new Validators.NotEmptyValidationRule();
                rule.ValidatesOnTargetUpdated = true;
                binding.ValidationRules.Add(rule);
                customFieldDatePicker.SetBinding(TextBox.TextProperty, binding);
            }
            if (template?.RegName == "Expiry")
            {
                //customFieldDatePicker.DisplayDateStart = DateTime.Now;
                CalendarDateRange cdr = new CalendarDateRange(DateTime.MinValue,DateTime.Now.AddDays(-1));
                customFieldDatePicker.BlackoutDates.Add(cdr);
            }
            RegisterName(template.RegName, customFieldDatePicker);
            HintAssist.SetHint(customFieldDatePicker, template.FieldCaption);
            inputContainer.Children.Add(customFieldIcon);
            inputContainer.Children.Add(customFieldDatePicker);
            container.Child = inputContainer;
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
                Height = 60,
                Orientation = Orientation.Horizontal
            };
            Image customFieldIcon = new Image
            {
                Source = new BitmapImage(new Uri(template.FieldImage, UriKind.Relative)),
                Width = 50,
                Margin = new Thickness(10)
            };
            Style style = this.FindResource("MaterialDesignFloatingHintComboBox") as Style;
            ComboBox customFieldComboBox = new ComboBox
            {
                Width = 180,
                Margin = new Thickness(10, -15, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14,
                FontFamily = new FontFamily("Segoe UI Semibold"),
                IsEnabled = template.IsEnabled,
                Style=style
            };
            if (template.IsMandatory)
            {
                Binding binding = new Binding
                {
                    RelativeSource = RelativeSource.Self,
                    Path = new PropertyPath("Text"),
                    UpdateSourceTrigger = UpdateSourceTrigger.LostFocus
                };
                ValidationRule rule = new Validators.NotEmptyValidationRule();
                rule.ValidatesOnTargetUpdated = true;
                binding.ValidationRules.Add(rule);
                customFieldComboBox.SetBinding(TextBox.TextProperty, binding);
            }
            RegisterName(template.RegName, customFieldComboBox);
            List<string> comboboxItems = GetItemsBySelection(template.ControlTable, template.SelectionBasis);
            foreach (var item in comboboxItems)
            {
                customFieldComboBox.Items.Add(new ComboBoxItem { Content = item });
            }
            HintAssist.SetHint(customFieldComboBox, template.FieldCaption);
            inputContainer.Children.Add(customFieldIcon);
            inputContainer.Children.Add(customFieldComboBox);
            container.Child = inputContainer;
            return container;
        }
        private List<string> GetItemsBySelection(string tableName, string columnName)
        {
            AdminDBCall _dbContext = new AdminDBCall();
            List<string> Result = new List<string>();
            DataTable table = _dbContext.GetAllData($"select [{columnName}] from [{tableName}]");
            if (table != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    var item = (row[$"{columnName}"].ToString());
                    Result.Add(item);
                }
            }
            return Result;
        }
        private void Single_onWeighmentReceived(object sender, WeighmentEventArgs e)
        {
            var we = e._weight as string;
            string result = Regex.Replace(we, @"[^\d]", "");
            AddToWeightmentList(result);
        }
        private void AddToWeightmentList(string w)
        {
            if (GainedWeightList.Count < StableWeightArraySize)
            {
                GainedWeightList.Add(w);
            }
            else
            {
                int extra = GainedWeightList.Count - StableWeightArraySize;
                GainedWeightList.RemoveRange(0, extra);
                GainedWeightList.Add(w);
            }
            if (CheckIsStable())
            {
                Dispatcher.Invoke(() =>
                {
                    WeightButton.IsEnabled = true;
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    WeightButton.IsEnabled = false;
                });
            }
        }

        public string GetWighment()
        {

            try
            {
                return GainedWeightList.LastOrDefault();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetWighment :-" + ex.Message);
                return null;
            }
        }

        public bool CheckIsStable()
        {
            try
            {
                var Copylist = new List<string>();
                Copylist = GainedWeightList;
                if (Copylist.Count < StableWeightArraySelectable)
                {
                    return false;
                }
                else
                {
                    var tempList = Copylist.Skip(Math.Max(0, Copylist.Count() - StableWeightArraySelectable)).Take(StableWeightArraySelectable).ToArray();
                    if (Array.TrueForAll(tempList, y => y == tempList[0]))
                    {
                        if (tempList.Length > 0)
                        {
                            return true;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                //WriteLog.WriteToFile("CheckIsStable :-" + ex.Message);
                return false;
            }
            return false;
        }
        private void Weigh_Button_Click(object sender, RoutedEventArgs e)
        {
            _currentWeightment = GetWighment();
            TextBox tareWeight = (TextBox)FindName("VehicleTareWeight");
            tareWeight.Text = _currentWeightment;
            TextBox taredTime = (TextBox)FindName("TaredTime");
            taredTime.Text = DateTime.Now.ToString("t");
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
        private void VehicleNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex(@"[^a-zA-Z0-9\s]");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
