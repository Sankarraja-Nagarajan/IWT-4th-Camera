using AWS.Communication;
using AWS.Communication.Models;
using IWT.Admin_Pages;
using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using IWT.Views;
using MaterialDesignThemes.Wpf;
using Microsoft.Reporting.WinForms;
using MjpegProcessor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace IWT.TransactionPages
{
    public partial class FirstMulti : Page
    {
        public static MasterDBCall masterDBCall = new MasterDBCall();
        public static CommonFunction commonFunction = new CommonFunction();
        public static MainWindow mainWindow = new MainWindow();
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        private AdminDBCall _dbContext;
        //public int StableWeightArraySize = int.Parse(ConfigurationManager.AppSettings["StableWeightArraySize"]);
        //public int StableWeightArraySelectable = int.Parse(ConfigurationManager.AppSettings["StableWeightArraySelectable"]);
        public int StableWeightArraySize = 10;
        public int StableWeightArraySelectable = 1;
        public List<CustomFieldBuilder> CustomFieldsBuilder = new List<CustomFieldBuilder>();
        public List<Company_Details> company_Details = new List<Company_Details>();
        public static List<string> GainedWeightList = new List<string>(); private string _currentWeightment = "";
        AuthStatus authResult;
        public string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public string TransactionPath;
        List<ShiftMaster> shiftMasters = new List<ShiftMaster>();
        ShiftMaster CurrentShift = new ShiftMaster();
        private Transaction currentTransaction = new Transaction();
        Transaction CurrentTransaction = new Transaction();
        public static event EventHandler<TicketEventArgs> onFirstMultiTicketCompletion = delegate { };
        public static event EventHandler<TicketEventArgs> onFirstTicketCompletion = delegate { };        

        List<FieldDependency> fieldDependencies = new List<FieldDependency>();
        private bool handleChangeEvent = true;
        public List<CustomFieldBuilder> FormulaFields = new List<CustomFieldBuilder>();

        List<ImageSourcePath> CurrentTransactionImageSourcePath = new List<ImageSourcePath>();
        RolePriviliege rolePriviliege;
        UserHardwareProfile userHardwareProfile;
        DataTable CurrentTransactionDataTable = new DataTable();
        ERPFileLocation selectedERPFileLocation = new ERPFileLocation();
        CloudAppConfig selectedCloudAppConfig = new CloudAppConfig();
        public static event EventHandler<AwsCompletedEventArgs> awsOperationCompleted = delegate { };
        public static event EventHandler<TransLogEventArgs> createTransLog = delegate { };
        public List<string> TypeList = new List<string>();
        AWSTransaction AwsTransaction = null;
        AWSConfiguration awsConfiguration = new AWSConfiguration();
        public static Transaction_DBCall transaction_DBCall = new Transaction_DBCall();
        List<CamDetail> camDetail = new List<CamDetail>();
        public static BitmapImage Camera4Source { get; set; }
        public FirstMulti(AuthStatus _authResult, RolePriviliege _rolePriviliege, UserHardwareProfile _userHardwareProfile, AWSTransaction _transaction = null)
        {
            InitializeComponent();
            authResult = _authResult;
            this.rolePriviliege = _rolePriviliege;
            this.userHardwareProfile = _userHardwareProfile;
            AwsTransaction = _transaction;
            _dbContext = new AdminDBCall();
            toastViewModel = new ToastViewModel();
            CurrentTransaction = new Transaction();
            TransactionPath = System.IO.Path.Combine(BaseDirectory, "TransactionPages");
            Loaded += FirstMulti_Loaded;
            Unloaded += FirstMulti_Unloaded;
            GetShiftMasters();
            GetCompanyDetails();
            GetOtherSettings();
            GetFileLocation();
            GetERPFileLocation();
            GetCloudAppConfig();
            GetStableWeightConfiguration();
            fieldDependencies = GetAllFieldDependencies();
            ApplyOtherSettings();
            BuildCustomFields();
            InitializeTypeList();
            InitializeTypeComboBox();
            //commonFunction.RemoveDuplicateMaterials();
            //commonFunction.RemoveDuplicateSuppliers();
            //commonFunction.GetMaterialMasters();
            //commonFunction.GetSupplierMasters();
        }

        public void InitializeTypeList()
        {
            TypeList.Add("Inbound");
            TypeList.Add("Outbound");
        }

        public void InitializeTypeComboBox()
        {
            Types.ItemsSource = TypeList;
            Types.Items.Refresh();
        }

        private void MainWindow_onPlcReceived(object sender, PlcEventArgs e)
        {
            PlcValue = e.value;
        }

        private void GetERPFileLocation()
        {
            try
            {
                DataTable dt1 = _dbContext.GetAllData("SELECT * FROM ERPFile_Location");
                string JSONString = JsonConvert.SerializeObject(dt1);
                var ERPFileLocations = JsonConvert.DeserializeObject<List<ERPFileLocation>>(JSONString);
                selectedERPFileLocation = (ERPFileLocations != null && ERPFileLocations.Count > 0) ? ERPFileLocations.FirstOrDefault() : new ERPFileLocation();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("FirstMulti/GetERPFileLocation", ex);
            }
        }

        public void GetStableWeightConfiguration()
        {
            try
            {
                StableWeightConfiguration stableWeightConfiguration = commonFunction.GetStableWeightConfiguration();
                StableWeightArraySize = stableWeightConfiguration != null ? stableWeightConfiguration.MinimumWeightCount : 10;
                StableWeightArraySelectable = stableWeightConfiguration != null ? stableWeightConfiguration.StableWeightCount : 1;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("FirstMulti/GetStableWeightConfiguration", ex);
            }
        }
        private void GetCloudAppConfig()
        {
            try
            {
                DataTable dt1 = _dbContext.GetAllData("SELECT * FROM CloudApp_Config");
                string JSONString = JsonConvert.SerializeObject(dt1);
                var CloudAppConfigs = JsonConvert.DeserializeObject<List<CloudAppConfig>>(JSONString);
                selectedCloudAppConfig = (CloudAppConfigs != null && CloudAppConfigs.Count > 0) ? CloudAppConfigs.FirstOrDefault() : new CloudAppConfig();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("FirstMulti/GetCloudAppConfig", ex);
            }
        }
        public List<FieldDependency> GetAllFieldDependencies()
        {
            try
            {
                DataTable table = _dbContext.GetAllData($"select * from [Field_Dependency]");
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<FieldDependency>>(JSONString);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetTicktetDataTemplateData:" + ex.Message);
                return new List<FieldDependency>();
            }
        }
        public void BuildCustomFields()
        {
            try
            {
                List<TicketDataTemplate> ticketDataTemplates = GetTransactionCustomFields();
                foreach (var field in ticketDataTemplates)
                {
                    CustomFieldBuilder customFieldBuilder = new CustomFieldBuilder();
                    customFieldBuilder.FieldTable = field.F_Table;
                    customFieldBuilder.FieldName = field.F_FieldName;
                    customFieldBuilder.FieldCaption = field.F_Caption;
                    customFieldBuilder.FieldType = field.F_Type;
                    customFieldBuilder.ControlType = field.ControlType;
                    customFieldBuilder.ControlTable = field.ControlTable;
                    customFieldBuilder.SelectionBasis = field.SelectionBasis;
                    customFieldBuilder.RegName = field.F_FieldName;
                    customFieldBuilder.ControlTableRef = field.ControlTableRef;
                    customFieldBuilder.DisableStatus = JsonConvert.DeserializeObject<ControlStatus>(field.ControlLoadStatusDisable);
                    customFieldBuilder.MandatoryStatus = JsonConvert.DeserializeObject<ControlStatus>(field.MandatoryStatus);
                    customFieldBuilder.IsMandatory = customFieldBuilder.MandatoryStatus != null ? customFieldBuilder.MandatoryStatus.FTE : false;
                    int size = 0;
                    int.TryParse(field.F_Size, out size);
                    customFieldBuilder.FieldSize = size;
                    CustomFieldsBuilder.Add(customFieldBuilder);
                }
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
                    else if (template.ControlType == "Formula")
                    {
                        dynamicWrapPanel.Children.Add(CreateFormulaField(template));
                    }
                    else if (template.ControlType == "DataDependancy")
                    {
                        dynamicWrapPanel.Children.Add(CreateDataDependencyField(template));
                    }
                    DisableCustomeField(template, "FTE");
                }
                FormulaFields = CustomFieldsBuilder.Where(t => t.ControlType == "Formula").ToList();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("BuildCustomFields:" + ex.Message);
            }
        }
        public List<TicketDataTemplate> GetTransactionCustomFields()
        {
            try
            {
                DataTable table = _dbContext.GetAllData($"select * from Ticket_Data_Template where F_Table='Transaction'");
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<TicketDataTemplate>>(JSONString);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetTicktetDataTemplateData:" + ex.Message);
                return new List<TicketDataTemplate>();
            }
        }
        private Binding CreateMandatoryBinding()
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
            return binding;
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
                Source = new BitmapImage(new Uri(@"/Assets/Icons/Custom_Field.png", UriKind.Relative)),
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
                Tag = template.RegName,
                Style = style
            };
            if (template.IsMandatory)
            {
                Binding binding = CreateMandatoryBinding();
                customFieldTextBox.SetBinding(TextBox.TextProperty, binding);
            }
            if (template.FieldType == "FLOAT")
            {
                customFieldTextBox.PreviewTextInput += Constant_PreviewTextInput;
            }
            else
            {
                customFieldTextBox.MaxLength = template.FieldSize;
            }
            customFieldTextBox.TextChanged += CustomFieldTextBox_TextChanged;
            RegisterName(template.RegName, customFieldTextBox);
            HintAssist.SetHint(customFieldTextBox, template.FieldCaption);
            inputContainer.Children.Add(customFieldIcon);
            inputContainer.Children.Add(customFieldTextBox);
            container.Child = inputContainer;
            return container;
        }
        private void Constant_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //Regex regex = new Regex("[^0-9]+");
            //e.Handled = regex.IsMatch(e.Text);
            TextBox txtBox = sender as TextBox;
            if (txtBox != null)
            {
                var textBoxValue = txtBox.Text;
                textBoxValue += e.Text;
                Regex regex = new Regex("^[0-9]*([.]{0,1}[0-9]{0,2})?$");
                e.Handled = !regex.IsMatch(textBoxValue);
            }
            else
            {
                e.Handled = true;
            }
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
                Source = new BitmapImage(new Uri(@"/Assets/Icons/Custom_Field.png", UriKind.Relative)),
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
                Style = style
            };
            if (template.IsMandatory)
            {
                Binding binding = CreateMandatoryBinding();
                customFieldDatePicker.SetBinding(TextBox.TextProperty, binding);
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
                Source = new BitmapImage(new Uri(@"/Assets/Icons/Custom_Field.png", UriKind.Relative)),
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
                Tag = template.RegName,
                Style = style
            };
            if (template.IsMandatory)
            {
                Binding binding = CreateMandatoryBinding();
                customFieldComboBox.SetBinding(TextBox.TextProperty, binding);
            }
            customFieldComboBox.SelectionChanged += CustomFieldComboBox_SelectionChanged;
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

        private Border CreateFormulaField(CustomFieldBuilder template)
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
                Source = new BitmapImage(new Uri(@"/Assets/Icons/Custom_Field.png", UriKind.Relative)),
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
                IsEnabled = false,
                Style = style
            };
            RegisterName(template.RegName, customFieldTextBox);
            HintAssist.SetHint(customFieldTextBox, template.FieldCaption);
            inputContainer.Children.Add(customFieldIcon);
            inputContainer.Children.Add(customFieldTextBox);
            container.Child = inputContainer;
            var formulaTemplate = GetFormulaTemplate(template.FieldCaption);
            template.Formula = DecodeFormula(formulaTemplate.FormulaList);
            return container;
        }
        private Border CreateDataDependencyField(CustomFieldBuilder template)
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
                Source = new BitmapImage(new Uri(@"/Assets/Icons/Custom_Field.png", UriKind.Relative)),
                Width = 50,
                Margin = new Thickness(10)
            };
            var fd = fieldDependencies.FirstOrDefault(t => t.CustomName == template.FieldName && t.CustomcType == template.FieldType);
            if (fd != null)
            {
                Style style = this.FindResource("MaterialDesignFloatingHintTextBox") as Style;
                TextBox customFieldTextBox = new TextBox
                {
                    Width = 180,
                    Margin = new Thickness(10, -15, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                    FontFamily = new FontFamily("Segoe UI Semibold"),
                    IsEnabled = false,
                    Style = style
                };
                RegisterName(template.RegName, customFieldTextBox);
                HintAssist.SetHint(customFieldTextBox, template.FieldCaption);
                inputContainer.Children.Add(customFieldIcon);
                inputContainer.Children.Add(customFieldTextBox);
            }
            else
            {
                Style style = this.FindResource("MaterialDesignFloatingHintTextBox") as Style;
                TextBox customFieldTextBox = new TextBox
                {
                    Width = 90,
                    Margin = new Thickness(10, -15, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                    FontFamily = new FontFamily("Segoe UI Semibold"),
                    IsEnabled = false,
                    Style = style
                };
                RegisterName(template.RegName, customFieldTextBox);
                Button linkBtn = new Button
                {
                    Content = "Link"
                };
                linkBtn.Click += LinkBtn_Click;
                RegisterName("link_" + template.RegName, linkBtn);
                linkBtn.Tag = template.RegName;
                HintAssist.SetHint(customFieldTextBox, template.FieldCaption);
                inputContainer.Children.Add(customFieldIcon);
                inputContainer.Children.Add(customFieldTextBox);
                inputContainer.Children.Add(linkBtn);
            }
            container.Child = inputContainer;
            return container;
        }

        private void LinkBtn_Click(object sender, RoutedEventArgs e)
        {
            Button linkBtn = sender as Button;
            string field = linkBtn.Tag.ToString();
            FieldDependency fd = new FieldDependency();
            var cCustomField = CustomFieldsBuilder.FirstOrDefault(t => t.FieldName == field);
            if (cCustomField != null)
            {
                fd.CustomName = cCustomField.FieldName;
                fd.CustomcType = cCustomField.FieldType;
                fd.ControlTable = cCustomField.ControlTable;
                fd.ControlTableRef = cCustomField.ControlTableRef;
                fd.SelectionBasis = cCustomField.SelectionBasis;
            }
            OpenTableFieldDialog(fd);
        }
        private async void OpenTableFieldDialog(FieldDependency fd)
        {
            List<string> trTables = new List<string> { "VehicleNumber", "MaterialName", "SupplierName" };
            foreach (var field in CustomFieldsBuilder)
            {
                trTables.Add(field.FieldName);
            }
            var view = new TableFieldsDialog(fd, trTables);
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            if (result != null)
            {
                var field = result as FieldDependency;
                var component = FindName(field.LinkedName);
                if (component.GetType() == typeof(TextBox))
                {
                    field.LinkedType = "Textbox";
                    field.LinkedEvent = "CustomFieldTextBox_KeyUp";
                }
                else if (component.GetType() == typeof(ComboBox))
                {
                    field.LinkedType = "Combobox";
                    field.LinkedEvent = "CustomFieldComboBox_SelectionChanged";
                }
                CreateFieldDependency(field);
            }
        }
        private void CreateFieldDependency(FieldDependency fd)
        {
            string insertQuery = $@"INSERT INTO [Field_Dependency] (LinkedName,LinkedType,LinkedEvent,CustomName,CustomcType,ControlTable,ControlTableRef,SelectionBasis) VALUES ('{fd.LinkedName}','{fd.LinkedType}','{fd.LinkedEvent}','{fd.CustomName}','{fd.CustomcType}','{fd.ControlTable}','{fd.ControlTableRef}','{fd.SelectionBasis}')";
            var res = _dbContext.ExecuteQuery(insertQuery);
            if (res)
            {
                var component = FindName(fd.CustomName) as TextBox;
                component.Width = 180;
                var parent = component.Parent as StackPanel;
                var btn = FindName("link_" + fd.CustomName) as Button;
                parent.Children.Remove(btn);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Linked Successfully");
                fieldDependencies.Add(fd);
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong!!");
            }
        }
        private string[] DecodeFormula(string formula)
        {
            var result = formula.Split(' ');
            return result;
        }
        private FormulaTemplate GetFormulaTemplate(string FieldName)
        {
            DataTable data = _dbContext.GetAllData($"SELECT * FROM Formula_Table where FormulaName='{FieldName}'");
            string JSONString = JsonConvert.SerializeObject(data);
            var result = JsonConvert.DeserializeObject<List<FormulaTemplate>>(JSONString);
            return result.FirstOrDefault();
        }
        private void CustomFieldComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (handleChangeEvent)
            {
                string tag = ((ComboBox)sender).Tag.ToString();
                string value = (((ComboBox)sender).SelectedItem as ComboBoxItem).Content.ToString();
                var depFields = fieldDependencies.Where(t => t.LinkedName == tag).ToList();
                foreach (var depField in depFields)
                {
                    if (depField != null)
                    {
                        string getQuery = $@"select {depField.SelectionBasis} from [{depField.ControlTable}] where {depField.ControlTableRef}='{value}' and IsDeleted='0'";
                        string res = GetDependencyValue(getQuery, depField.SelectionBasis);
                        TextBox field = (TextBox)FindName(depField.CustomName);
                        if (field != null)
                            field.Text = res;
                    }
                }
            }
        }
        private void CustomFieldTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (handleChangeEvent)
            {
                string tag = ((TextBox)sender).Tag.ToString();
                string value = ((TextBox)sender).Text;
                string val = ((TextBox)sender).Text;
                var depFields = fieldDependencies.Where(t => t.LinkedName == tag).ToList();
                foreach (var depField in depFields)
                {
                    if (depField != null)
                    {
                        if (tag == "MaterialName")
                        {
                            string MatName = "";
                            string MatCode = "";
                            var mat = value?.Split('/');
                            if (mat?.Length > 1)
                            {
                                string[] strings = new string[mat.Length - 1];
                                for (var i = 1; i < mat?.Length; i++)
                                {
                                    strings[i - 1] = mat[i];
                                }

                                MatName = string.Join("/", strings);
                                MatCode = mat[0];
                            }
                            else
                            {
                                MatName = value;
                                MatCode = value;
                            }

                            if (depField.ControlTableRef == "MaterialCode")
                            {
                                value = MatCode;
                            }
                            if (depField.ControlTableRef == "MaterialName")
                            {
                                value = MatName;
                            }
                        }
                        //else if (depField.ControlTable == "Supplier_Master" && tag == "SupplierName")
                        else if (tag == "SupplierName")
                        {
                            string SupName = "";
                            string SupCode = "";
                            var sup = value?.Split('/');
                            if (sup?.Length > 1)
                            {
                                string[] strings = new string[sup.Length - 1];
                                for (var i = 1; i < sup?.Length; i++)
                                {
                                    strings[i - 1] = sup[i];
                                }

                                SupName = string.Join("/", strings);
                                SupCode = sup[0];
                            }
                            else
                            {
                                SupName = value;
                                SupCode = value;
                            }

                            if (depField.ControlTableRef == "SupplierCode")
                            {
                                value = SupCode;
                            }
                            if (depField.ControlTableRef == "SupplierName")
                            {
                                value = SupName;
                            }
                        }
                        string getQuery = $@"select {depField.SelectionBasis} from [{depField.ControlTable}] where {depField.ControlTableRef}='{value}' and IsDeleted='0'";
                        string res = GetDependencyValue(getQuery, depField.SelectionBasis);
                        TextBox field = (TextBox)FindName(depField.CustomName);
                        if (field != null)
                            field.Text = res;
                        value = val;
                    }
                }
            }
        }
        private string GetDependencyValue(string query, string column)
        {
            string result = "";
            DataTable data = _dbContext.GetAllData(query);
            if (data != null && data.Rows.Count > 0)
            {
                result = data.Rows[0][column].ToString();
            }
            return result;
        }
        public void SetFormulaFieldValues(int EmptyWeight, int LoadWeight, int Netweight)
        {
            foreach (var field in FormulaFields)
            {
                TextBox textBox = (TextBox)FindName(field.RegName);
                DataTable dt = new DataTable();
                string formula = "";
                for (int i = 0; i < field.Formula.Count() - 1; i++)
                {
                    double d;
                    bool isValid;
                    isValid = double.TryParse(field.Formula[i], out d);
                    if (field.Formula[i] == "EmptyWeight")
                    {
                        formula += EmptyWeight;
                    }
                    else if (field.Formula[i] == "LoadWeight")
                    {
                        formula += LoadWeight;
                    }
                    else if (field.Formula[i] == "NetWeight")
                    {
                        formula += Netweight;
                    }
                    else if (field.Formula[i] == "*" || field.Formula[i] == "/" || field.Formula[i] == "+" || field.Formula[i] == "-" || field.Formula[i] == "%" || field.Formula[i] == "(" || field.Formula[i] == ")")
                    {
                        formula += field.Formula[i];
                    }
                    else if (isValid)
                    {
                        formula += field.Formula[i];
                    }
                    else
                    {
                        var component = FindName(field.Formula[i]);
                        if (component.GetType() == typeof(TextBox))
                        {
                            string val = ((TextBox)component).Text;
                            if (val != "")
                            {
                                formula += val;
                            }
                        }
                        else if (component.GetType() == typeof(ComboBox))
                        {
                            ComboBoxItem selected = ((ComboBox)component).SelectedItem as ComboBoxItem;
                            if (selected != null)
                            {
                                string val = selected.Content.ToString();
                                formula += val;
                            }
                        }
                    }
                }
                try
                {
                    //var v = dt.Compute(formula, "");
                    //textBox.Text = v.ToString();
                    var v = dt.Compute(formula, "");
                    double d = 0;
                    double.TryParse(v?.ToString(), out d);
                    var vv = Math.Round(d, 2);
                    textBox.Text = vv.ToString();
                }
                catch (Exception)
                {
                    textBox.Text = "";
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
                        DatePicker datePicker = (DatePicker)FindName(field.RegName);
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
                        TextBox textBox = (TextBox)FindName(field.RegName);
                        field.Value = textBox.Text;
                    }
                }
                else if (field.ControlType == "Dropdown")
                {
                    ComboBox comboBox = (ComboBox)this.FindName(field.RegName);
                    ComboBoxItem selected = comboBox.SelectedItem as ComboBoxItem;
                    if (selected != null)
                    {
                        field.Value = selected.Content.ToString();
                    }
                }
                else if (field.ControlType == "Formula")
                {
                    TextBox textBox = (TextBox)FindName(field.RegName);
                    field.Value = textBox.Text;
                }
                else if (field.ControlType == "DataDependancy")
                {
                    TextBox textBox = (TextBox)FindName(field.RegName);
                    field.Value = textBox.Text;
                }
                if (field.IsMandatory && (field.Value == null || field.Value.ToString() == ""))
                {
                    throw new Exception("Please fill mandatory fields");
                }
            }
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
        private void FirstMulti_Loaded(object sender, RoutedEventArgs e)
        {
            WriteLog.WriteToFile("FirstMulti/FirstMulti_Loaded:- LoadedEvent Invoked");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            onFirstMultiTicketCompletion.Invoke(this, new TicketEventArgs("1"));
            MainWindow.onWeighmentReceived += Single_onWeighmentReceived;
            MainWindow.onImage1Recieved += MainWindow_onImage1Recieved;
            //MainWindow.onImage2Recieved += MainWindow_onImage2Recieved;
            MainWindow.onImage3Recieved += MainWindow_onImage3Recieved;
            MainWindow.cam4Img += MainWindow_cam4Img;
            MainWindow.cam2Img += MainWindow_cam2Img;
            cCTVSettings = commonFunction.GetCCTVSettings(MainWindow.systemConfig.HardwareProfile);
            awsConfiguration = commonFunction.GetAWSConfiguration(MainWindow.systemConfig.HardwareProfile);
            MainWindow.onPlcReceived += MainWindow_onPlcReceived;
            Task.Run(() => StartAwsSequence(AwsTransaction));

        }

        private void FirstMulti_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.onWeighmentReceived -= Single_onWeighmentReceived;
            MainWindow.onImage1Recieved -= MainWindow_onImage1Recieved;
            //MainWindow.onImage2Recieved -= MainWindow_onImage2Recieved;
            MainWindow.onImage3Recieved -= MainWindow_onImage3Recieved;
            MainWindow.cam4Img -= MainWindow_cam4Img;
            MainWindow.cam2Img -= MainWindow_cam2Img;
            MainWindow.onPlcReceived -= MainWindow_onPlcReceived;
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)ToggleButton.IsChecked)
            {
                EmptyLoadedLabel.Text = "Loaded";
                UpdateDisableMandatoryFields("FTL");
            }
            else
            {
                EmptyLoadedLabel.Text = "Empty";
                UpdateDisableMandatoryFields("FTE");
            }
        }
        //public void ShowMessage(Action<string> message, string name)
        //{
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        LastMessage = $"{name}";
        //        message(LastMessage);
        //    });
        //}
        private void Vehicle_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenVehicleDialog();
        }
        private async void OpenVehicleDialog()
        {
            var view = new Addvehicledialog();
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            if (result != null)
            {
                VehicleNumber.Text = result as string;
            }
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            ////Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }
        private void ClearTransaction()
        {
            EmptyWeight1.Text = "0";
            LoadedWeight.Text = "0";
            NetWeight1.Text = "0";
            VehicleNumber.Text = "";
            NumberOfMaterials.Text = "";
            Auto_MobileNumber.Text = "";
            Auto_Email.Text = "";
            Types.Text = "";
            DocumentNumber.Text = "";
            GatePassNumber.Text = "";
            TokenNumber.Text = "";
            CurrentTransaction = new Transaction();
            //_reportTemplate = "";
            _reportTemplate = _fileLocation.Default_Temp;
            ResetFields();
        }
        private void ResetFields()
        {
            handleChangeEvent = false;
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
                else if (field.ControlType == "Formula")
                {
                    TextBox textBox = (TextBox)FindName(field.RegName);
                    textBox.Text = null;
                }
                else if (field.ControlType == "DataDependancy")
                {
                    TextBox textBox = (TextBox)FindName(field.RegName);
                    textBox.Text = null;
                }
            }
            handleChangeEvent = true;
        }
        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            ClearTransaction();
        }
        private void Single_onWeighmentReceived(object sender, WeighmentEventArgs e)
        {
            var we = e._weight as string;
            AddToWeightmentList(we);
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
                    if (!string.IsNullOrEmpty(VehicleNumber.Text))
                    {
                        WeighButton.IsEnabled = true;
                    }
                    else
                    {
                        WeighButton.IsEnabled = false;
                    }
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    WeighButton.IsEnabled = false;
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
                        if ((bool)awsConfiguration.VPSEnable)
                        {
                            if (!MainWindow.isPlcConnected)
                            {
                                return false;
                            }
                            if (tempList.Length > 0 && PlcValue.Contains("22"))
                            {
                                return true;
                            }
                        }
                        else if (tempList.Length > 0)
                        {
                            return true;
                        }

                        return false;
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
        public void GetShiftMasters()
        {
            try
            {
                shiftMasters = commonFunction.GetShiftMasters();
                FindCurrentShift();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("FirstTransaction/GetShiftMasters/Exception:- " + ex.Message, ex);
            }
        }

        public void FindCurrentShift()
        {
            try
            {
                DateTime today = DateTime.Now;
                CurrentShift = new ShiftMaster();
                if (shiftMasters != null)
                {
                    shiftMasters.ForEach(x =>
                    {
                        var dateTime1 = DateTime.ParseExact(x.FromShift, "h:mm tt", null);
                        var dateTime2 = DateTime.ParseExact(x.ToShift, "h:mm tt", null);
                        if (dateTime1 > dateTime2)
                        {
                            dateTime2 = dateTime2.AddDays(1);
                        }
                        if (today >= dateTime1 && today <= dateTime2)
                        {
                            CurrentShift = x;
                            return;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("FirstMulti/FindCurrentShift/Exception:- " + ex.Message, ex);
            }
        }

        private void GetCompanyDetails()
        {
            company_Details = commonFunction.GetCompanyDetails();
        }

        private async void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(VehicleNumber.Text) && !string.IsNullOrEmpty(NumberOfMaterials.Text))
            {
                if (VehicleNumber.Text.Length <= 10)
                {
                    var Transac = commonFunction.GetVehicleDetails(VehicleNumber.Text);
                    if (Transac == null || Transac.Closed)
                    {

                        SaveButton.IsEnabled = false;

                        GetValuesFromFields();
                        AdminDBCall db = new AdminDBCall();
                        CurrentTransaction.VehicleNo = VehicleNumber.Text;
                        CurrentTransaction.NoOfMaterial = string.IsNullOrEmpty(NumberOfMaterials.Text) ? 0 : int.Parse(NumberOfMaterials.Text);
                        CurrentTransaction.Date = DateTime.Now;
                        if ((bool)ToggleButton.IsChecked)
                        {
                            CurrentTransaction.LoadWeight = string.IsNullOrEmpty(_currentWeightment) ? 0 : Convert.ToInt32(_currentWeightment);
                        }
                        else
                        {
                            CurrentTransaction.EmptyWeight = string.IsNullOrEmpty(_currentWeightment) ? 0 : Convert.ToInt32(_currentWeightment);
                        }
                        CurrentTransaction.EmptyWeightDate = DateTime.Now;
                        CurrentTransaction.LoadWeightDate = DateTime.Now;
                        CurrentTransaction.NetWeight = 0;
                        CurrentTransaction.Pending = true;
                        CurrentTransaction.Closed = true;
                        CurrentTransaction.MultiWeight = true;
                        CurrentTransaction.MultiWeightTransPending = true;
                        CurrentTransaction.UserName = authResult.UserName;
                        CurrentTransaction.ShiftName = CurrentShift?.ShiftName;
                        CurrentTransaction.State = "FMT";
                        CurrentTransaction.TransactionType = "FirstMulti";

                        CurrentTransaction.LoadStatus = EmptyLoadedLabel.Text;
                        if (CurrentTransaction.LoadStatus == "Loaded")
                        {
                            CurrentTransaction.LoadWeightDate = DateTime.Now;
                            CurrentTransaction.LoadWeightTime = DateTime.Now.ToString("hh:mm:ss tt");
                            CurrentTransaction.EmptyWeightDate = (DateTime?)null;
                            CurrentTransaction.EmptyWeightTime = "";

                        }
                        else
                        {
                            CurrentTransaction.EmptyWeightDate = DateTime.Now;
                            CurrentTransaction.EmptyWeightTime = DateTime.Now.ToString("hh:mm:ss tt");
                            CurrentTransaction.LoadWeightDate = (DateTime?)null;
                            CurrentTransaction.LoadWeightTime = "";
                        }

                        string insertQuery = BuildTransactionInsertQuery(CurrentTransaction, CustomFieldsBuilder);

                        SqlCommand command = new SqlCommand(insertQuery);

                        if (CurrentTransaction.LoadWeightDate.HasValue)
                        {
                            command.Parameters.AddWithValue("@LoadWeightDate", CurrentTransaction.LoadWeightDate.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@LoadWeightDate", DBNull.Value);
                        }

                        if (CurrentTransaction.EmptyWeightDate.HasValue)
                        {
                            command.Parameters.AddWithValue("@EmptyWeightDate", CurrentTransaction.EmptyWeightDate.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@EmptyWeightDate", DBNull.Value);
                        }


                        var res = _dbContext.ExecuteQuery(command);
                        if (res)
                        {
                            bool isTemplateSelected = true;
                            //if (_otherSettings != null && _otherSettings.AutoCopies)
                            //{
                            //    await OpenCopiesDialog();
                            //}
                            if (_otherSettings == null || (_otherSettings != null && !_otherSettings.AutoPrintPreview))
                            {
                                isTemplateSelected = await OpenTemplateDialog();
                            }
                            GetTransactionData();
                            transaction_DBCall.SaveCloudConfigTransaction(CurrentTransaction.TicketNo);
                            //if (isTemplateSelected && _otherSettings != null && _otherSettings.AutoFtPrint)
                            //{
                            //    if (_otherSettings != null && _otherSettings.AutoCopies)
                            //    {
                            //        await OpenCopiesDialog();
                            //    }
                            //    ReportViewerDemo1.LocalReport.PrintToPrinter(_noOfCopies);
                            //}
                            //else if (isTemplateSelected)
                            //{

                            //}
                            if (isTemplateSelected && !string.IsNullOrEmpty(_reportTemplate) && File.Exists(_reportTemplate))
                            {
                                if (_otherSettings != null && _otherSettings.AutoFtPrint)
                                {
                                    TicketDialog.IsOpen = false;
                                    if (_otherSettings != null && _otherSettings.AutoCopies)
                                    {
                                        await OpenCopiesDialog();
                                    }
                                    ReportViewerDemo1.LocalReport.PrintToPrinter(_noOfCopies);
                                }
                                else
                                {
                                    TicketDialog.IsOpen = true;
                                }
                            }
                            await ExecuteAutoMethods();
                            if (selectedERPFileLocation != null && selectedERPFileLocation.ID != 0 && selectedERPFileLocation.IsEnabled)
                            {
                                SendFileToERP();
                            }
                            if (selectedCloudAppConfig != null && selectedCloudAppConfig.ID != 0 && selectedCloudAppConfig.IsEnabled)
                            {
                                SendFileToAPI();
                            }
                            if (!TicketDialog.IsOpen)
                            {
                                ClearTransaction();
                            }
                            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Saved Successfully");
                            onFirstMultiTicketCompletion.Invoke(this, new TicketEventArgs("1"));
                        }
                        else
                        {
                            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong!!!");
                        }
                    }
                    else
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Vehicle has already been pending in a transaction");
                    }
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Maxmimum allowed character for vehicle number is 10");
                    //SaveButton.IsEnabled = false;
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the Vehicle no and Number of materials");
                SaveButton.IsEnabled = false;
            }
        }
        private void GetTransactionData()
        {
            AdminDBCall db = new AdminDBCall();
            DataTable dt = db.GetAllData("select * from [Transaction] where TicketNo = (select max(TicketNo) from [Transaction])");
            string JSONString = JsonConvert.SerializeObject(dt);
            var result = JsonConvert.DeserializeObject<List<Transaction>>(JSONString);
            if (result != null && result.Count > 0)
            {
                CurrentTransaction.TicketNo = result[0].TicketNo;
                CaptureCameraImage(CurrentTransaction);
            }

            if (CurrentTransactionDataTable.Rows.Count > 0)
            {
                if (EmptyLoadedLabel.Text == "Loaded")
                {
                    CurrentTransactionDataTable.Rows[0]["Date"] = CurrentTransactionDataTable.Rows[0]["LoadWeightDate"];
                    CurrentTransactionDataTable.Rows[0]["Time"] = CurrentTransactionDataTable.Rows[0]["LoadWeightTime"];
                }
                else
                {
                    CurrentTransactionDataTable.Rows[0]["Date"] = CurrentTransactionDataTable.Rows[0]["EmptyWeightDate"];
                    CurrentTransactionDataTable.Rows[0]["Time"] = CurrentTransactionDataTable.Rows[0]["EmptyWeightTime"];

                }
            }

            ReportDataSource rds = new ReportDataSource("DataSet1", dt);
            ReportDataSource rds1 = new ReportDataSource("DataSet2", company_Details);
            ReportDataSource rds3 = new ReportDataSource("DataSet3", CurrentTransactionImageSourcePath);
            ReportViewerDemo1.LocalReport.DataSources.Clear();
            ReportViewerDemo1.LocalReport.DataSources.Add(rds);
            ReportViewerDemo1.LocalReport.DataSources.Add(rds1);
            ReportViewerDemo1.LocalReport.DataSources.Add(rds3);
            ReportViewerDemo1.ShowExportButton = false;
            ReportViewerDemo1.ShowFindControls = false;
            ReportViewerDemo1.ShowStopButton = false;
            ReportViewerDemo1.LocalReport.ReportPath = _reportTemplate;
            ReportViewerDemo1.RefreshReport();
        }
        private void SendFileToERP()
        {
            if (selectedERPFileLocation.IsXML)
            {
                SendXMLFileToERP();
            }
            if (selectedERPFileLocation.IsCSV)
            {
                SendCSVFileToERP();
            }
        }
        private void SendXMLFileToERP()
        {
            try
            {
                var ERPFilePath = selectedERPFileLocation.ERPFilePath;
                var FileName = System.IO.Path.Combine(ERPFilePath, $"{CurrentTransaction.TicketNo.ToString()}.xml");
                CurrentTransactionDataTable.TableName = "Transaction";

                //string result;
                //using (StringWriter sw = new StringWriter())
                //{
                //    CurrentTransactionDataTable.WriteXml(sw);
                //    result = sw.ToString();
                //}
                //File.WriteAllText(FileName, result);
                DataSet ds = new DataSet();
                ds.Tables.Add(CurrentTransactionDataTable); // Table 1
                string dsXml = ds.GetXml();

                using (StreamWriter fs = new StreamWriter(FileName)) // XML File Path
                {
                    ds.WriteXml(fs);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SendXMLFileToERP:" + ex.Message);
            }
        }
        private void SendCSVFileToERP()
        {
            try
            {
                var ERPFilePath = selectedERPFileLocation.ERPFilePath;
                var FileName = System.IO.Path.Combine(ERPFilePath, $"{CurrentTransaction.TicketNo.ToString()}.csv");
                StringBuilder sb = new StringBuilder();

                IEnumerable<string> columnNames = CurrentTransactionDataTable.Columns.Cast<DataColumn>().
                                                  Select(column => column.ColumnName);
                sb.AppendLine(string.Join(",", columnNames));

                foreach (DataRow row in CurrentTransactionDataTable.Rows)
                {
                    IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                    sb.AppendLine(string.Join(",", fields));
                }

                File.WriteAllText(FileName, sb.ToString());
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SendCSVFileToERP:" + ex.Message);
            }
        }
        public void SendFileToAPI()
        {
            try
            {
                commonFunction.SendTransactionDetailsToCloudApp(selectedCloudAppConfig, CurrentTransactionDataTable, CurrentTransaction.TicketNo);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SendFileToAPI:" + ex.Message);
            }
        }


        public string BuildTransactionInsertQuery(Transaction transaction, List<CustomFieldBuilder> customFieldBuilders)
        {
            string InsertQuery = "INSERT INTO [Transaction] (TransactionType,VehicleNo,EmptyWeight,LoadWeight,EmptyWeightDate,LoadWeightDate,EmptyWeightTime,LoadWeightTime,NetWeight,Pending,Closed,ShiftName,MaterialName,SupplierName,MultiWeight,MultiWeightTransPending,LoadStatus,SystemID,UserName,NoOfMaterial,State,CreatedOn,RFIDAllocation,TransType,DocNumber,GatePassNumber,TokenNumber,IsSapBased,";
            foreach (var field in customFieldBuilders)
            {
                InsertQuery += $"{field.FieldName},";
            }
            var SName = string.IsNullOrEmpty(transaction.ShiftName) ? "Shift" : transaction.ShiftName;
            InsertQuery += $"Date,Time) VALUES ('{transaction.TransactionType}','{transaction.VehicleNo}','{transaction.EmptyWeight}','{transaction.LoadWeight}',@EmptyWeightDate,@LoadWeightDate,'{transaction.EmptyWeightTime}','{transaction.LoadWeightTime}','{transaction.NetWeight}','{transaction.Pending}','{transaction.Closed}','{SName}','{transaction.MaterialName}','{transaction.SupplierName}','{transaction.MultiWeight}','{transaction.MultiWeightTransPending}','{transaction.LoadStatus}','{MainWindow.SystemId}','{transaction.UserName}','{transaction.NoOfMaterial}','{transaction.State}','{DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss")}','{transaction.RFIDAllocation}','{transaction.TransType}','{transaction.DocNumber}','{transaction.GatePassNumber}','{transaction.TokenNumber}','{transaction.IsSapBased}',";
            foreach (var field in customFieldBuilders)
            {
                InsertQuery += $"'{field.Value}',";
            }
            InsertQuery += $"'{DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss")}','{DateTime.Now.ToString("hh:mm:ss tt")}')";
            return InsertQuery;
        }

        private void Weigh_Button_Click(object sender, RoutedEventArgs e)
        {
            int emptyWeight = 0;
            int loadWeight = 0;
            int netWeight = 0;
            if (!string.IsNullOrEmpty(VehicleNumber.Text) && !string.IsNullOrEmpty(NumberOfMaterials.Text))
            {
                if (VehicleNumber.Text.Length <= 10)
                {
                    _currentWeightment = GetWighment();
                    if ((bool)ToggleButton.IsChecked)
                    {
                        LoadedWeight.Text = _currentWeightment;
                        loadWeight = string.IsNullOrEmpty(LoadedWeight.Text) ? 0 : Convert.ToInt32(LoadedWeight.Text);

                        if (loadWeight <= 0)
                        {
                            SaveButton.IsEnabled = false;
                            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Weight should be a positive value and greater than zero");
                        }
                        else
                        {
                            SaveButton.IsEnabled = true;
                        }
                    }
                    else
                    {
                        EmptyWeight1.Text = _currentWeightment;
                        emptyWeight = string.IsNullOrEmpty(EmptyWeight1.Text) ? 0 : Convert.ToInt32(EmptyWeight1.Text);
                        if (emptyWeight <= 0)
                        {
                            SaveButton.IsEnabled = false;
                            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Weight should be a positive value and greater than zero");
                        }
                        else
                        {
                            SaveButton.IsEnabled = true;
                        }
                    }
                    SetFormulaFieldValues(emptyWeight, loadWeight, netWeight);
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Maxmimum allowed character for vehicle number is 10");
                    SaveButton.IsEnabled = false;
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the Vehicle no and Number of materials");
                SaveButton.IsEnabled = false;
            }
        }

        public void OpenTicketDialog()
        {
            if (!TicketDialog.IsOpen)
            {
                TicketDialog.Visibility = System.Windows.Visibility.Visible;
                TicketDialog.IsOpen = true;
                GetTicketData();
            }
            else TicketDialog.IsOpen = false;
        }
        private void GetTicketData()
        {
            AdminDBCall db = new AdminDBCall();
            CurrentTransactionDataTable = db.GetAllData("select * from [Transaction] where TicketNo = (select max(TicketNo) from [Transaction])");

            string JSONString = JsonConvert.SerializeObject(CurrentTransactionDataTable);
            var result = JsonConvert.DeserializeObject<List<Transaction>>(JSONString);
            if (result != null && result.Count > 0)
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(async () =>
                {
                    CurrentTransaction = result[0];
                    CaptureCameraImage(CurrentTransaction);
                    //CaptureCameraImage(result[0]);
                }));
            }




        }

        public void GetData()
        {
            AdminDBCall db = new AdminDBCall();
            DataTable dt = db.GetAllData("select * from [Transaction] where TicketNo = (select max(TicketNo) from [Transaction])");
            ReportDataSource rds = new ReportDataSource("DataSet1", dt);
            ReportDataSource rds1 = new ReportDataSource("DataSet2", company_Details);
            ReportDataSource rds3 = new ReportDataSource("DataSet3", CurrentTransactionImageSourcePath);
            //
            ReportViewerDemo1.LocalReport.DataSources.Clear();
            ReportViewerDemo1.LocalReport.DataSources.Add(rds);
            ReportViewerDemo1.LocalReport.DataSources.Add(rds1);
            ReportViewerDemo1.LocalReport.DataSources.Add(rds3);
            ReportViewerDemo1.ShowExportButton = false;
            ReportViewerDemo1.ShowFindControls = false;
            ReportViewerDemo1.ShowStopButton = false;
            //ReportViewerDemo1.LocalReport.ReportEmbeddedResource = "IWT.TransactionPages.TransactionReport.rdlc";

            // ReportParameter parameter = new ReportParameter("ImagePath1", imagePath);
            ReportViewerDemo1.LocalReport.ReportPath = System.IO.Path.Combine(TransactionPath, "TransactionReport.rdlc");

            //var imagePath =new Uri(@"C:\Users\iadmin\Pictures\Sunrise.jpg").AbsolutePath;
            //var r = new ReportParameter("ImagePath1", imagePath);
            //ReportViewerDemo1.LocalReport.EnableExternalImages = true;
            //ReportViewerDemo1.LocalReport.SetParameters(new ReportParameter[] {r});

            ReportViewerDemo1.RefreshReport();

            TicketDialog.IsOpen = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            TicketDialog.IsOpen = false;
            ClearTransaction();
        }

        private async void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            //popup.IsOpen = false;
            //ReportViewerDemo1.PrintDialog();
            //ReportViewerDemo1.LocalReport.PrintToPrinter(1);
            if (_otherSettings != null && _otherSettings.AutoCopies)
            {
                await OpenCopiesDialog();
            }
            ReportViewerDemo1.LocalReport.PrintToPrinter(_noOfCopies);
        }

        //public void ShowMessage(Action<string> message, string name)
        //{
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        LastMessage = name;
        //        message(LastMessage);
        //    });
        //}

        private async void SMSBtn_Click(object sender, RoutedEventArgs e)
        {
            var message = commonFunction.BuildSMS(CurrentTransaction);
            await commonFunction.CheckAndSendSMS(message);
        }

        private async void EmailBtn_Click(object sender, RoutedEventArgs e)
        {
            var message = commonFunction.BuildEmail(CurrentTransaction);
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
            await commonFunction.CheckAndSendEmail(CurrentTransaction, message, bytes, fileName);
        }
        private void VehicleNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex(@"[^a-zA-Z0-9\s]");
            e.Handled = regex.IsMatch(e.Text);
        }

        #region Camera

        DispatcherTimer CameraTimer;
        List<CCTVSettings> cCTVSettings = new List<CCTVSettings>();
        private MjpegDecoder _mjpeg1;
        private MjpegDecoder _mjpeg2;
        private MjpegDecoder _mjpeg3;
        private List<bool> cameraStatus = new List<bool> { false, false, false };

        public void IntializeCamera()
        {
            CameraTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            CameraTimer.Tick += CameraTimer_Tick;
            GetCCTVSettings();
        }

        private async void CameraTimer_Tick(object sender, EventArgs e)
        {
            foreach (var setting in cCTVSettings)
            {
                if (setting.CameraType == "Hikvision Camera")
                {
                    await GetSnapshotFromHikVision(setting);
                }
            }
        }
        private void _mjpeg1_FrameReady(object sender, FrameReadyEventArgs e)
        {
            var imgBytes = e.FrameBuffer;
            MemoryStream memstream = new MemoryStream(imgBytes);
            System.Drawing.Bitmap img = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memstream);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                image1.Source = BitmapToImageSource(img);
            }));
        }
        private void _mjpeg2_FrameReady(object sender, FrameReadyEventArgs e)
        {
            var imgBytes = e.FrameBuffer;
            MemoryStream memstream = new MemoryStream(imgBytes);
            System.Drawing.Bitmap img = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memstream);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                image2.Source = BitmapToImageSource(img);
            }));
        }
        private void _mjpeg3_FrameReady(object sender, FrameReadyEventArgs e)
        {
            var imgBytes = e.FrameBuffer;
            MemoryStream memstream = new MemoryStream(imgBytes);
            System.Drawing.Bitmap img = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memstream);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                image3.Source = BitmapToImageSource(img);
            }));
        }
        public void GetCCTVSettings()
        {
            try
            {
                cCTVSettings = commonFunction.GetCCTVSettings(MainWindow.systemConfig.HardwareProfile);
                foreach (var cam in cCTVSettings)
                {
                    PingCameraIPAddress(cam);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("FirstTransaction/GetShiftMasters/Exception:- " + ex.Message, ex);
            }
        }
        private async void PingCameraIPAddress(CCTVSettings settings)
        {
            try
            {
                if (!string.IsNullOrEmpty(settings.IPAddress))
                {
                    var url = settings.IPAddress;
                    Uri myUri = new Uri(url);
                    var ip = Dns.GetHostAddresses(myUri.Host)[0];
                    Ping pingSender = new Ping();
                    var reply = await pingSender.SendPingAsync(ip, 100);

                    if (reply.Status == IPStatus.Success)
                    {
                        if (settings.RecordID == 1)
                        {
                            cameraStatus[0] = true;
                        }
                        else if (settings.RecordID == 2)
                        {
                            cameraStatus[1] = true;
                        }
                        else
                        {
                            cameraStatus[2] = true;
                        }
                        WriteLog.WriteToFile($"SingleTransaction/PingCameraIPAddress:- Ping successfull for Cam{settings.RecordID}");
                        Task.Run(() =>
                        {
                            StartCameraStreaming(settings);
                        });
                        //this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                        //{
                        //    StartCameraStreaming(settings);
                        //}));
                    }
                }
            }
            catch (Exception ex1)
            {
                string LineNumber = "";
                try
                {
                    //LineNumber = new StackTrace(ex1, true).GetFrame(new StackTrace(ex1, true).FrameCount - 1).GetFileLineNumber().ToString();
                    WriteLog.WriteToFile("PingCameraIPAddress:-:- LineNumber: " + LineNumber + " Error : " + ex1.Message);
                }
                catch (Exception)
                {
                    //LineNumber = "NotFound";
                    WriteLog.WriteToFile("PingCameraIPAddress:- LineNumber: " + LineNumber + " Error : " + ex1.Message);
                }
            }
        }
        public void StartCameraStreaming(CCTVSettings settings)
        {
            try
            {
                Regex r = new Regex(@"[a-z]?\d+(\.\d+)+(\.\d+)?", RegexOptions.None, TimeSpan.FromMilliseconds(150));
                Match m = r.Match(settings.IPAddress);
                string ipaddress = m.ToString();
                if (settings.RecordID == 1)
                {
                    if (settings.CameraType == "Hikvision Camera")
                    {
                        if (!CameraTimer.IsEnabled)
                        {
                            CameraTimer.Start();
                        }
                    }
                    else
                    {
                        _mjpeg1.ParseStream(new Uri(settings.IPAddress), settings.CameraUserName, settings.CameraPassword);
                    }
                }
                else if (settings.RecordID == 2)
                {
                    if (settings.CameraType == "Hikvision Camera")
                    {
                        if (!CameraTimer.IsEnabled)
                        {
                            CameraTimer.Start();
                        }
                    }
                    else
                    {
                        _mjpeg2.ParseStream(new Uri(settings.IPAddress), settings.CameraUserName, settings.CameraPassword);
                    }
                }
                else if (settings.RecordID == 3)
                {
                    if (settings.CameraType == "Hikvision Camera")
                    {
                        if (!CameraTimer.IsEnabled)
                        {
                            CameraTimer.Start();
                        }
                    }
                    else
                    {
                        _mjpeg3.ParseStream(new Uri(settings.IPAddress), settings.CameraUserName, settings.CameraPassword);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("StartCameraStreaming:- " + ex.Message);
            }
        }
        public async void CaptureCameraImage(Transaction transaction)
        {
            WriteLog.WriteToFile("FirstMulti/CaptureCameraImage : CaptureCameraImage starts.");
            WriteLog.WriteToFile("FirstMulti/CaptureCameraImage : TicketNo :- " + transaction.TicketNo);
            ImageSourcePath imageSourcePath = new ImageSourcePath();
            foreach (var camera in cCTVSettings)
            {
                if (camera.Enable)
                {
                    WriteLog.WriteToFile("FirstMulti/CaptureCameraImage :- camera.RecordID : " + camera.RecordID);
                    string imagePath = $"{camera.LogFolder}\\{transaction.TicketNo}_{transaction.State}_cam{camera.RecordID.ToString()}_{DateTime.Now:ddMMyyyyhhmmss}.jpeg";
                    ImageSource imageSource = null;
                    if (camera.RecordID == 1)
                    {
                        WriteLog.WriteToFile("FirstMulti/CaptureCameraImage 1:- imageSource : " + imageSource);
                        WriteLog.WriteToFile("FirstMulti/CaptureCameraImage 1:- imagePath : " + imagePath);
                        imageSource = image1.Source;
                        imageSourcePath.Image1Path = commonFunction.SaveCameraImage(imageSource, imagePath);
                        WriteLog.WriteToFile("FirstMulti/CaptureCameraImage 1:- imageSourcePath.Image1Path : " + imageSourcePath.Image1Path);
                        FileInfo imageInfo = new FileInfo(imagePath);
                        long sizeInBytes = imageInfo.Length;
                        WriteLog.WriteToFile("Camera1/sizeInBytes :- " + sizeInBytes.ToString());
                        if (sizeInBytes < 500)
                        {
                            camDetail.Add(new CamDetail
                            {
                                CamId = 1,
                                FilePath = imagePath
                            });
                        }
                    }
                    else if (camera.RecordID == 2)
                    {
                        WriteLog.WriteToFile("FirstMulti/CaptureCameraImage 2:- imageSource : " + imageSource);
                        WriteLog.WriteToFile("FirstMulti/CaptureCameraImage 2:- imagePath : " + imagePath);
                        imageSource = image2.Source;
                        imageSourcePath.Image2Path = commonFunction.SaveCameraImage(imageSource, imagePath);
                        WriteLog.WriteToFile("FirstMulti/CaptureCameraImage 2:- imageSourcePath.Image1Path : " + imageSourcePath.Image1Path);
                        FileInfo imageInfo = new FileInfo(imagePath);
                        long sizeInBytes = imageInfo.Length;
                        WriteLog.WriteToFile("Camera2/sizeInBytes :- " + sizeInBytes.ToString());
                        if (sizeInBytes < 500)
                        {
                            camDetail.Add(new CamDetail
                            {
                                CamId = 2,
                                FilePath = imagePath
                            });
                        }
                    }
                    else if (camera.RecordID == 3)
                    {
                        WriteLog.WriteToFile("FirstMulti/CaptureCameraImage 3:- imageSource : " + imageSource);
                        WriteLog.WriteToFile("FirstMulti/CaptureCameraImage 3:- imagePath : " + imagePath);
                        imageSource = image3.Source;
                        imageSourcePath.Image3Path = commonFunction.SaveCameraImage(imageSource, imagePath);
                        WriteLog.WriteToFile("FirstMulti/CaptureCameraImage 3:- imageSourcePath.Image1Path : " + imageSourcePath.Image1Path);
                        FileInfo imageInfo = new FileInfo(imagePath);
                        long sizeInBytes = imageInfo.Length;
                        WriteLog.WriteToFile("Camera3/sizeInBytes :- " + sizeInBytes.ToString());
                        if (sizeInBytes < 500)
                        {
                            camDetail.Add(new CamDetail
                            {
                                CamId = 3,
                                FilePath = imagePath
                            });
                        }
                    }
                    else if (camera.RecordID == 4)
                    {
                        WriteLog.WriteToFile("FirstMulti/CaptureCameraImage 4:- imageSource : " + imageSource);
                        WriteLog.WriteToFile("FirstMulti/CaptureCameraImage 4:- imagePath : " + imagePath);
                        imageSource = image4.Source;
                        imageSourcePath.Img4Path = commonFunction.SaveCameraImage(imageSource, imagePath);
                        WriteLog.WriteToFile("FirstMulti/CaptureCameraImage 4:- imageSourcePath.Img4Path : " + imageSourcePath.Img4Path);
                        FileInfo imageInfo = new FileInfo(imagePath);
                        long sizeInBytes = imageInfo.Length;
                        WriteLog.WriteToFile("Camera4/sizeInBytes :- " + sizeInBytes.ToString());
                        if (sizeInBytes < 500)
                        {
                            camDetail.Add(new CamDetail
                            {
                                CamId = 4,
                                FilePath = imagePath
                            });
                        }
                    }
                }
            }
            CurrentTransactionImageSourcePath = new List<ImageSourcePath>();
            WriteLog.WriteToFile("FirstMulti/CaptureCameraImage:- imageSourcePath : " + imageSourcePath);
            CurrentTransactionImageSourcePath.Add(imageSourcePath);
            if (camDetail.Count > 0)
            {
                await OpenCamConfirmationDialog(camDetail);
            }
        }

        public async Task OpenCamConfirmationDialog(List<CamDetail> camera)
        {
            foreach (CamDetail cam in camera)
            {
                await ReCaptureCameraImage(cam);
                //var view = new ConfirmationDialog(null, false, $"Camera {cam.CamId} is not Captured. Do you want to Recapture?");
                //var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
                //if ((bool)result)
                //{
                //    // recapture code
                //    await ReCaptureCameraImage(cam);
                //}
            }
        }

        public async Task ReCaptureCameraImage(CamDetail cam)
        {
            ImageSource imageSource = null;
            ImageSourcePath imageSourcePath = new ImageSourcePath();
            if (cam.CamId == 1)
            {
                imageSource = image1.Source;
                File.Delete(cam.FilePath);
                WriteLog.WriteToFile("FirstMulti/ReCaptureCameraImage 1:- imageSource : " + imageSource);
                WriteLog.WriteToFile("FirstMulti/ReCaptureCameraImage 1:- imagePath : " + cam.FilePath);
                imageSourcePath.Image1Path = commonFunction.SaveCameraImage(imageSource, cam.FilePath);
                WriteLog.WriteToFile("FirstMulti/ReCaptureCameraImage 1:- imageSourcePath.Image1Path : " + imageSourcePath.Image1Path);
                CurrentTransactionImageSourcePath[0].Image1Path = imageSourcePath.Image1Path;
                FileInfo imageInfo = new FileInfo(cam.FilePath);
                long sizeInBytes = imageInfo.Length;
                WriteLog.WriteToFile("Camera1/sizeInBytes :- " + sizeInBytes.ToString());
                camDetail.RemoveAll(x => x.CamId == cam.CamId);
                if (sizeInBytes < 500)
                {
                    camDetail.Add(new CamDetail
                    {
                        CamId = 1,
                        FilePath = cam.FilePath
                    });
                }
            }
            else if (cam.CamId == 2)
            {
                imageSource = image2.Source;
                File.Delete(cam.FilePath);
                WriteLog.WriteToFile("FirstMulti/ReCaptureCameraImage 2:- imageSource : " + imageSource);
                WriteLog.WriteToFile("FirstMulti/ReCaptureCameraImage 2:- imagePath : " + cam.FilePath);
                imageSourcePath.Image2Path = commonFunction.SaveCameraImage(imageSource, cam.FilePath);
                WriteLog.WriteToFile("FirstMulti/ReCaptureCameraImage 2:- imageSourcePath.Image2Path : " + imageSourcePath.Image2Path);
                CurrentTransactionImageSourcePath[0].Image2Path = imageSourcePath.Image2Path;
                FileInfo imageInfo = new FileInfo(cam.FilePath);
                long sizeInBytes = imageInfo.Length;
                WriteLog.WriteToFile("Camera2/sizeInBytes :- " + sizeInBytes.ToString());
                camDetail.RemoveAll(x => x.CamId == cam.CamId);
                if (sizeInBytes < 500)
                {
                    camDetail.Add(new CamDetail
                    {
                        CamId = 2,
                        FilePath = cam.FilePath
                    });
                }
            }
            else if (cam.CamId == 3)
            {
                imageSource = image3.Source;
                File.Delete(cam.FilePath);
                WriteLog.WriteToFile("FirstMulti/ReCaptureCameraImage 3:- imageSource : " + imageSource);
                WriteLog.WriteToFile("FirstMulti/ReCaptureCameraImage 3:- imagePath : " + cam.FilePath);
                imageSourcePath.Image3Path = commonFunction.SaveCameraImage(imageSource, cam.FilePath);
                WriteLog.WriteToFile("FirstMulti/ReCaptureCameraImage 3:- imageSourcePath.Image3Path : " + imageSourcePath.Image3Path);
                CurrentTransactionImageSourcePath[0].Image3Path = imageSourcePath.Image3Path;
                FileInfo imageInfo = new FileInfo(cam.FilePath);
                long sizeInBytes = imageInfo.Length;
                WriteLog.WriteToFile("Camera3/sizeInBytes :- " + sizeInBytes.ToString());
                camDetail.RemoveAll(x => x.CamId == cam.CamId);
                if (sizeInBytes < 500)
                {
                    camDetail.Add(new CamDetail
                    {
                        CamId = 3,
                        FilePath = cam.FilePath
                    });
                }
            }
            else if (cam.CamId == 4)
            {
                imageSource = image4.Source;
                File.Delete(cam.FilePath);
                WriteLog.WriteToFile("FirstMulti/ReCaptureCameraImage 4:- imageSource : " + imageSource);
                WriteLog.WriteToFile("FirstMulti/ReCaptureCameraImage 4:- imagePath : " + cam.FilePath);
                imageSourcePath.Img4Path = commonFunction.SaveCameraImage(imageSource, cam.FilePath);
                WriteLog.WriteToFile("FirstMulti/ReCaptureCameraImage 4:- imageSourcePath.Image4Path : " + imageSourcePath.Image4Path);
                CurrentTransactionImageSourcePath[0].Img4Path = imageSourcePath.Img4Path;
                FileInfo imageInfo = new FileInfo(cam.FilePath);
                long sizeInBytes = imageInfo.Length;
                WriteLog.WriteToFile("Camera4/sizeInBytes :- " + sizeInBytes.ToString());
                camDetail.RemoveAll(x => x.CamId == cam.CamId);

                if (sizeInBytes < 500)
                {
                    camDetail.Add(new CamDetail
                    {
                        CamId = 4,
                        FilePath = cam.FilePath
                    });
                }
            }

            if (camDetail.Count > 0)
            {
                await OpenCamConfirmationDialog(camDetail);
            }
        }

        public async Task GetSnapshotFromHikVision(CCTVSettings ccTV)
        {
            byte[] buffer = new byte[300000];
            int read, total = 0;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(ccTV.IPAddress);
            req.Credentials = new NetworkCredential(ccTV.CameraUserName, ccTV.CameraPassword);
            try
            {
                WebResponse resp = await req.GetResponseAsync();
                Stream stream = resp.GetResponseStream();
                while ((read = stream.Read(buffer, total, 1000)) != 0)
                {
                    total += read;
                }
                MemoryStream memstream = new MemoryStream(buffer, 0, total);
                try
                {
                    System.Drawing.Bitmap img = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memstream);
                    if (ccTV.RecordID == 1)
                    {
                        image1.Source = BitmapToImageSource(img);
                    }
                    else if (ccTV.RecordID == 2)
                    {
                        image2.Source = BitmapToImageSource(img);
                    }
                    else if (ccTV.RecordID == 3)
                    {
                        image3.Source = BitmapToImageSource(img);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetSnapshotFromHikVision:- " + ex.Message);
            }
        }
        BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private void MainWindow_cam4Img(object sender, CameraEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                image4.Source = e.bitmap;
            }));
        }

        private void MainWindow_cam2Img(object sender, CameraEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                image2.Source = e.bitmap;
            }));
        }

        private void MainWindow_onImage3Recieved(object sender, CameraEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                image3.Source = e.bitmap;
            }));
        }

        //private void MainWindow_onImage2Recieved(object sender, CameraEventArgs e)
        //{
        //    Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
        //    {
        //        image2.Source = e.bitmap;
        //    }));
        //}

        private void MainWindow_onImage1Recieved(object sender, CameraEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                image1.Source = e.bitmap;
            }));
        }
        #endregion
        #region OtherSettings
        private string _reportTemplate;
        private int _noOfCopies = 1;
        private string _defaultTemplateFolder;
        private OtherSettings _otherSettings = new OtherSettings();
        private FileLocation _fileLocation = new FileLocation();
        public void GetOtherSettings()
        {
            try
            {
                DataTable table = _dbContext.GetAllData($"SELECT * FROM Other_Settings where HardwareProfile='{MainWindow.systemConfig.HardwareProfile}'");
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<OtherSettings>>(JSONString);
                if (result != null)
                    _otherSettings = result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetOtherSettings:- " + ex.Message);
            }
        }
        public void GetFileLocation()
        {
            try
            {
                DataTable table = _dbContext.GetAllData("select * from [FileLocation_Setting]");
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<FileLocation>>(JSONString);
                if (result != null)
                {
                    var flc = result.FirstOrDefault();
                    _fileLocation = flc;
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetFileLocation:- " + ex.Message);
            }
        }
        public void ApplyOtherSettings()
        {
            try
            {
                if (_otherSettings != null)
                {
                    if (_otherSettings != null && _otherSettings.AutoFtMail)
                    {
                        AutoMail_Container.Visibility = Visibility.Visible;
                        EmailBtn.Visibility = Visibility.Collapsed;
                    }
                    if (_otherSettings != null && _otherSettings.SMSAlerts)
                    {
                        AutoSMS_Container.Visibility = Visibility.Visible;
                        SMSBtn.Visibility = Visibility.Collapsed;
                    }
                    if (_otherSettings != null && _otherSettings.AutoFtPrint)
                    {
                        PrintBtn.Visibility = Visibility.Collapsed;
                    }
                    _noOfCopies = _otherSettings != null ? _otherSettings.NoOfCopies : 1;
                    _reportTemplate = _fileLocation.Default_Temp;
                    _defaultTemplateFolder = _fileLocation.Default_Ticket;
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("ApplyOtherSettings:- " + ex.Message);
            }

        }
        public async Task ExecuteAutoMethods()
        {

            try
            {
                if (_otherSettings != null && _otherSettings.SMSAlerts)
                {
                    var message = commonFunction.BuildSMS(CurrentTransaction);
                    await commonFunction.CheckAndSendSMS(message, Auto_MobileNumber.Text);
                }
                if (_otherSettings != null && _otherSettings.AutoFtMail)
                {
                    var message = commonFunction.BuildEmail(CurrentTransaction);
                    Warning[] warnings;
                    string[] streamids;
                    string mimeType;
                    string encoding;
                    string filenameExtension;

                    byte[] bytes = null;

                    if (!string.IsNullOrEmpty(_reportTemplate) && File.Exists(_reportTemplate))
                    {
                        bytes = ReportViewerDemo1.LocalReport.Render(
                        "PDF", null, out mimeType, out encoding, out filenameExtension,
                        out streamids, out warnings);
                    }


                    //using (FileStream fs = new FileStream("output.pdf", FileMode.Create))
                    //{
                    //    fs.Write(bytes, 0, bytes.Length);
                    //}

                    string fileName = $"report{DateTime.Now.ToString("ddMMyyyyHHmmss")}.pdf";
                    await commonFunction.CheckAndSendEmail(CurrentTransaction, message, bytes, fileName, Auto_Email.Text);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("ExecuteAutoMethods:- " + ex.Message);
            }


        }
        #endregion
        #region Dialogs
        public async Task OpenCopiesDialog()
        {
            try
            {
                var view = new NoOfCopiesDialog(_otherSettings != null ? _otherSettings.NoOfCopies : 1);
                var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
                if (result != null)
                {
                    _noOfCopies = int.Parse(result as string);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("OpenCopiesDialog:- " + ex.Message);
            }
        }
        public async Task<bool> OpenTemplateDialog()
        {
            try
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
            catch (Exception ex)
            {
                WriteLog.WriteToFile("OpenTemplateDialog:- " + ex.Message);
                return false;
            }
        }
        #endregion

        private void VehicleNumber_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        public void SetLoadStatus(bool status)
        {
            if (status)
            {
                EmptyLoadedLabel.Text = "Loaded";
                NumberOfMaterials.IsEnabled = true;
                VehicleNumber.IsEnabled = true;                
            }
            else
            {
                EmptyLoadedLabel.Text = "Empty";
                NumberOfMaterials.IsEnabled = false;
                VehicleNumber.IsEnabled = false;

            }
            ToggleButton.IsChecked = status;
        }
        #region Disable/Mandatory Status
        private void UpdateDisableMandatoryFields(string page)
        {
            foreach (var field in CustomFieldsBuilder)
            {
                DisableCustomeField(field, page);
            }
        }
        private void DisableCustomeField(CustomFieldBuilder fieldBuilder, string page)
        {
            try
            {
                bool IsDisable = false;
                bool IsMandatory = false;
                if (page == "SGT")
                {
                    IsDisable = fieldBuilder.DisableStatus != null ? !fieldBuilder.DisableStatus.SGT : true;
                    IsMandatory = fieldBuilder.MandatoryStatus != null ? fieldBuilder.MandatoryStatus.SGT : false;
                }
                else if (page == "FTE")
                {
                    IsDisable = fieldBuilder.DisableStatus != null ? !fieldBuilder.DisableStatus.FTE : true;
                    IsMandatory = fieldBuilder.MandatoryStatus != null ? fieldBuilder.MandatoryStatus.FTE : false;
                }
                else if (page == "FTL")
                {
                    IsDisable = fieldBuilder.DisableStatus != null ? !fieldBuilder.DisableStatus.FTL : true;
                    IsMandatory = fieldBuilder.MandatoryStatus != null ? fieldBuilder.MandatoryStatus.FTL : false;
                }
                else if (page == "STE")
                {
                    IsDisable = fieldBuilder.DisableStatus != null ? !fieldBuilder.DisableStatus.STE : true;
                    IsMandatory = fieldBuilder.MandatoryStatus != null ? fieldBuilder.MandatoryStatus.STE : false;
                }
                else if (page == "STL")
                {
                    IsDisable = fieldBuilder.DisableStatus != null ? !fieldBuilder.DisableStatus.STL : true;
                    IsMandatory = fieldBuilder.MandatoryStatus != null ? fieldBuilder.MandatoryStatus.STL : false;
                }
                fieldBuilder.IsMandatory = IsMandatory;
                if (fieldBuilder.ControlType == "TextBox")
                {
                    if (fieldBuilder.FieldType == "DATETIME")
                    {
                        var field = (DatePicker)FindName(fieldBuilder.FieldName);
                        field.IsEnabled = IsDisable;
                        if (IsMandatory)
                        {
                            field.SetBinding(TextBox.TextProperty, CreateMandatoryBinding());
                        }
                        else
                        {
                            field.ClearValue(TextBox.TextProperty);
                        }
                    }
                    else
                    {
                        var field = (TextBox)FindName(fieldBuilder.FieldName);
                        field.IsEnabled = IsDisable;
                        if (IsMandatory)
                        {
                            field.SetBinding(TextBox.TextProperty, CreateMandatoryBinding());
                        }
                        else
                        {
                            field.ClearValue(TextBox.TextProperty);
                        }
                    }
                }
                else if (fieldBuilder.ControlType == "Dropdown")
                {
                    var field = (ComboBox)FindName(fieldBuilder.FieldName);
                    field.IsEnabled = IsDisable;
                    if (IsMandatory)
                    {
                        field.SetBinding(TextBox.TextProperty, CreateMandatoryBinding());
                    }
                    else
                    {
                        field.ClearValue(TextBox.TextProperty);
                    }
                }
                else if (fieldBuilder.ControlType == "Formula")
                {
                    var field = (TextBox)FindName(fieldBuilder.FieldName);
                    field.IsEnabled = IsDisable;
                }
                else if (fieldBuilder.ControlType == "DataDependancy")
                {
                    var field = (TextBox)FindName(fieldBuilder.FieldName);
                    field.IsEnabled = IsDisable;
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DisableCustomeField: " + ex.Message);
            }
        }
        #endregion

        #region AWS
        public static string PlcValue = "";
        public bool IsAwsStarted { get; set; } = false;

        private RFIDAllocation currentAllocation = new RFIDAllocation();

        private void StartAwsSequence(AWSTransaction transaction)
        {
            MainWindow.onPlcReceived += MainWindow_onPlcReceived;
            try
            {
                if (transaction != null)
                {
                    var task = Task.Run(() => ExecuteAWS(transaction));
                    if (task.Wait(TimeSpan.FromSeconds(awsConfiguration.SequenceTimeOut)))
                        return;
                    else
                        throw new Exception("Operation timed out!!!");
                }
            }
            catch (Exception ex)
            {
                var exception = "";
                if (ex.InnerException.Message != exception)
                {
                    WriteLog.WriteAWSLog($"ex.InnerException.Message :- {ex.InnerException.Message}");
                    if (ex.InnerException != null)
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, ex.InnerException.Message);
                    else
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, ex.Message);
                    exception = ex.InnerException.Message;
                    WriteLog.WriteAWSLog($"exception :- {exception}");
                    WriteLog.WriteAWSLog($"ex.Message :- {ex.Message}");
                }

                WriteLog.WriteAWSLog("Exception:-", ex);
                if (ex.InnerException != null)
                    CreateLog($"Exception:- {ex.InnerException.Message}");
                else
                    CreateLog($"Exception:- {ex.Message}");
                IsAwsStarted = false;
                awsOperationCompleted.Invoke("first multi", new AwsCompletedEventArgs());
                this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                {
                    ClearTransaction();
                }));
                AwsTransaction = null;
                WriteLog.WriteAWSLog($"<========================AWS Sequence Completed With Exception!==========================>");
                CreateLog("55 Command to PLC sent");
                CreateLog("<============ AWS sequence completed with exception ! ============>");
                commonFunction.SendCommandToPLC("55");
            }
            finally
            {
                MainWindow.onPlcReceived -= MainWindow_onPlcReceived;
            }
        }
        private void ExecuteAWS(AWSTransaction transaction)
        {
            WriteLog.WriteAWSLog($"<========================AWS Sequence Started(First Multi Transaction)({transaction.AllocationData.RFIDTag})==========================>");
            CreateLog($"<========= AWS sequence started(First Multi Transaction)({transaction.AllocationData.RFIDTag}) ==============>");
            IsAwsStarted = true;
            currentTransaction = transaction.TransactionData;
            currentAllocation = transaction.AllocationData;
            this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                SetLoadStatus(currentTransaction.LoadStatus == "Loaded");
                NumberOfMaterials.Text = currentTransaction.NoOfMaterial.ToString();
                VehicleNumber.Text = currentTransaction.VehicleNo;
                if (transaction.AllocationData.IsSapBased)
                {
                    Type_Container.Visibility = Visibility.Visible;
                    document.Visibility = Visibility.Visible;
                    gatePassNumber.Visibility = Visibility.Visible;
                    tokenNumber.Visibility = Visibility.Visible;
                    Types.Text = currentTransaction.TransType;
                    DocumentNumber.Text = currentTransaction.DocNumber;
                    GatePassNumber.Text = currentTransaction.GatePassNumber;
                    TokenNumber.Text = currentTransaction.TokenNumber;
                }
                SetValuesToFields((GetCustomFieldValuesFromAllocation((int)currentTransaction.RFIDAllocation)));
            }));
            WriteLog.WriteAWSLog("Gate entry data patched");
            CreateLog("Gate entry data patched");
            PlcValue = "";
            string LastPlcCmd = "";
            if (transaction.IsSecondReader)
            {
                commonFunction.SendCommandToPLC("77");
                CreateLog("77 Command to PLC sent");
                WriteLog.WriteAWSLog($"77 Command to PLC sent");
                LastPlcCmd = "77";
            }
            else
            {
                commonFunction.SendCommandToPLC("11");
                CreateLog("11 Command to PLC sent");
                WriteLog.WriteAWSLog($"11 Command to PLC sent");
                LastPlcCmd = "11";
            }
            while (PlcValue == "") { commonFunction.RetryPlc(LastPlcCmd); }
            WriteLog.WriteAWSLog($"{PlcValue} recieved from PLC");
            CreateLog($"{PlcValue} recieved from PLC");
            if (PlcValue.Contains("99"))
            {
                throw new Exception("Plc error");
            }
            //Weighment
            var weighStatus = CaptureWeight();
            if (!weighStatus)
            {
                throw new Exception("Weight capture failed");
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Weight Captured");
                WriteLog.WriteAWSLog($"Weight captured");
                CreateLog($"Weight captured");
            }
            //Save transaction
            var saveStatus = SaveTransaction();
            if (!saveStatus)
            {
                throw new Exception("Save transaction failed");
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Transaction Saved");
                WriteLog.WriteAWSLog($"Transaction saved");
                CreateLog($"Transaction saved");
            }

            //SAP Transaction
            if (currentAllocation.IsSapBased)
            {
                Task.Run(async () => await SendDataToSAP());
                WriteLog.WriteAWSLog($"Sending data to SAP initiated");
                CreateLog($"Sending data to SAP initiated");
            }

            PlcValue = "";
            commonFunction.SendCommandToPLC("33");
            WriteLog.WriteAWSLog($"33 Command to PLC sent");
            CreateLog("33 Command to PLC sent");
            LastPlcCmd = "33";
            while (PlcValue == "") { commonFunction.RetryPlc(LastPlcCmd); }
            WriteLog.WriteAWSLog($"{PlcValue} recieved from PLC");
            CreateLog($"{PlcValue} recieved from PLC");
            if (PlcValue.Contains("99"))
            {
                throw new Exception("Plc error");
            }
            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "AWS Operation Competed");
            this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                ClearTransaction();
                onFirstTicketCompletion.Invoke(this, new TicketEventArgs("1"));
            }));
            IsAwsStarted = false;
            AwsTransaction = null;
            WriteLog.WriteAWSLog($"<========================AWS Sequence Completed==========================>");
            CreateLog("<============ AWS sequence completed ============>");
            awsOperationCompleted.Invoke("first multi", new AwsCompletedEventArgs());
        }
        private bool CaptureWeight()
        {
            while (!CheckIsStable()) { }
            _currentWeightment = GetWighment();
            var EmptyWeight = 0;
            var LoadWeight = 0;
            var Netweight = 0;
            var LoadStatus = true;
            this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                if ((bool)ToggleButton.IsChecked)
                {
                    LoadedWeight.Text = _currentWeightment;
                    LoadStatus = true;
                }
                else
                {
                    EmptyWeight1.Text = _currentWeightment;
                    LoadStatus = false;
                }
                EmptyWeight = string.IsNullOrEmpty(EmptyWeight1.Text) ? 0 : Convert.ToInt32(EmptyWeight1.Text);
                LoadWeight = string.IsNullOrEmpty(LoadedWeight.Text) ? 0 : Convert.ToInt32(LoadedWeight.Text);
                Netweight = 0;
            }));
            if (LoadStatus)
            {
                if (LoadWeight <= 0)
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Weight should be a positive value and greater than zero");
                    return false;
                }
                //else if (Netweight <= 0)
                //{
                //    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Empty weight should be less than loaded weight");
                //    return false;
                //}
            }
            else
            {
                if (EmptyWeight <= 0)
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Weight should be a positive value and greater than zero");
                    return false;
                }
            }

            this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                NetWeight1.Text = Netweight.ToString();
                SetFormulaFieldValues(EmptyWeight, LoadWeight, Netweight);
            }));
            currentTransaction.EmptyWeight = EmptyWeight;
            currentTransaction.LoadWeight = LoadWeight;
            currentTransaction.NetWeight = Netweight;
            return true;
        }
        private bool SaveTransaction()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                if ((bool)ToggleButton.IsChecked)
                {
                    currentTransaction.LoadStatus = "Loaded";
                    currentTransaction.LoadWeightDate = DateTime.Now;
                    currentTransaction.LoadWeightTime = DateTime.Now.ToString("hh:mm:ss tt");
                    currentTransaction.EmptyWeightDate = (DateTime?)null;
                    currentTransaction.EmptyWeightTime = "";
                }
                else
                {
                    currentTransaction.LoadStatus = "Empty";
                    currentTransaction.EmptyWeightDate = DateTime.Now;
                    currentTransaction.EmptyWeightTime = DateTime.Now.ToString("hh:mm:ss tt");
                    currentTransaction.LoadWeightDate = (DateTime?)null;
                    currentTransaction.LoadWeightTime = "";
                }
                GetValuesFromFields();
            }));
            currentTransaction.TransactionType = "First Multi";
            currentTransaction.MultiWeight = true;
            currentTransaction.Pending = true;
            currentTransaction.Closed = false;
            currentTransaction.MultiWeightTransPending = true;
            currentTransaction.SystemID = "";
            currentTransaction.UserName = authResult.UserName;
            currentTransaction.NoOfMaterial = (int)currentAllocation.NoOfMaterial;
            currentTransaction.ShiftName = CurrentShift?.ShiftName;
            currentTransaction.State = "FMT";

            SqlCommand cmd = new SqlCommand(BuildTransactionInsertQuery(currentTransaction, CustomFieldsBuilder));
            if (currentTransaction.LoadWeightDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@LoadWeightDate", currentTransaction.LoadWeightDate.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue("@LoadWeightDate", DBNull.Value);
            }

            if (currentTransaction.EmptyWeightDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@EmptyWeightDate", currentTransaction.EmptyWeightDate.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue("@EmptyWeightDate", DBNull.Value);
            }
            var res = _dbContext.ExecuteQuery(cmd);
            if (res)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(async () =>
                {
                    GetTransactionData();
                    transaction_DBCall.SaveCloudConfigTransaction(CurrentTransaction.TicketNo);
                    if (!string.IsNullOrEmpty(_reportTemplate) && File.Exists(_reportTemplate))
                    {
                        if (_otherSettings != null && awsConfiguration.FTPrintEnable.HasValue && awsConfiguration.FTPrintEnable.Value)
                        {
                            ReportViewerDemo1.LocalReport.PrintToPrinter(_noOfCopies);
                        }
                    }
                    await ExecuteAutoMethods();
                    if (selectedERPFileLocation != null && selectedERPFileLocation.ID != 0 && selectedERPFileLocation.IsEnabled)
                    {
                        SendFileToERP();
                    }
                    if (selectedCloudAppConfig != null && selectedCloudAppConfig.ID != 0 && selectedCloudAppConfig.IsEnabled)
                    {
                        SendFileToAPI();
                    }
                }));
                return true;
            }
            return false;
        }

        private async Task SendDataToSAP()
        {
            try
            {
                DataTable table = _dbContext.GetAllData($"SELECT * FROM [GatePasses] WHERE [Id] = {currentAllocation.GatePassId}");
                string gatePassJSON = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<GatePasses>>(gatePassJSON);
                var sapDataBackUp = new SAPDataBackUp();
                var currentGatePass = result.FirstOrDefault();
                if (currentGatePass != null)
                {
                    string docNumber = "";

                    string tk_gpNo = "";
                    if (currentGatePass.InOut.ToLower() == "in")
                    {
                        docNumber = currentGatePass.PoNumber;
                        tk_gpNo = currentGatePass.GatePassNumber;
                    }
                    else
                    {
                        docNumber = currentGatePass.SoNumber;
                        tk_gpNo = currentGatePass.TokenNumber;
                    }
                    SAPInterfaceCall interfaceCalls = new SAPInterfaceCall();
                    if (currentTransaction.LoadStatus == "Loaded")
                    {
                        var grossWeight = new GrossWeight
                        {
                            TokenNoGpNo = tk_gpNo,
                            CompPlant = currentGatePass.Plant,
                            SrNo = "1",
                            InOut = currentGatePass.InOut,
                            VbelnEbeln = docNumber,
                            MatNr = "",
                            PosNr = "",
                            GrossWt = currentTransaction.LoadWeight.ToString(),
                            GrossUom = "KG",
                            GrossDt = currentTransaction.LoadWeightDate.Value.ToString("yyyyMMdd"),
                            GrossTime = commonFunction.FormatTime(currentTransaction.LoadWeightTime),
                            LodeEnd = ""
                        };
                        var grossWeightResponse = await interfaceCalls.PostGrossWeight(grossWeight);
                        WriteLog.WriteErrorLog($"FirstMultiTransaction/SendDataToSAP/GROSS/Response:- {grossWeightResponse}");
                        char[] seperator = { '-' };
                        string[] grossWeightResponsearr = null;
                        grossWeightResponsearr = grossWeightResponse.Split(seperator);
                        var status = grossWeightResponsearr[0];
                        var response = grossWeightResponsearr[1];
                        sapDataBackUp.Payload = JsonConvert.SerializeObject(grossWeight);
                        sapDataBackUp.Type = "gross";
                        sapDataBackUp.Trans = "FMT";
                        sapDataBackUp.NoOfRetry = 0;
                        sapDataBackUp.Date = DateTime.Now;
                        sapDataBackUp.Response = response;
                        sapDataBackUp.CompletedTrans = 1;
                        sapDataBackUp.TransId = currentTransaction.TicketNo;
                        sapDataBackUp.TransType = "First Multi";
                        if (status == "failed")
                        {
                            sapDataBackUp.Status = "failed";
                        }
                        else if (status == "success")
                        {
                            sapDataBackUp.Status = "success";
                        }
                        commonFunction.InsertSAPDataBackUpDetails(sapDataBackUp);
                    }
                    else
                    {
                        var tareWeight = new TareWeight
                        {
                            TokenNoGpNo = tk_gpNo,
                            CompPlant = currentGatePass.Plant,
                            SrNo = "1",
                            InOut = currentGatePass.InOut,
                            VbelnEbeln = docNumber,
                            MatNr = "",
                            PosNr = "",
                            TareWt = currentTransaction.EmptyWeight.ToString(),
                            TareUom = "KG",
                            TareDt = currentTransaction.EmptyWeightDate.Value.ToString("yyyyMMdd"),
                            TareTime = commonFunction.FormatTime(currentTransaction.EmptyWeightTime),
                            UnlodeEnd = ""
                        };
                        var tareWeightResponse = await interfaceCalls.PostTareWeight(tareWeight);
                        WriteLog.WriteErrorLog($"FirstMultiTransaction/SendDataToSAP/TARE/Response:- {tareWeightResponse}");
                        char[] seperator = { '-' };
                        string[] tareWeightResponsearr = null;
                        tareWeightResponsearr = tareWeightResponse.Split(seperator);
                        var status = tareWeightResponsearr[0];
                        var response = tareWeightResponsearr[1];
                        sapDataBackUp.Payload = JsonConvert.SerializeObject(tareWeight);
                        sapDataBackUp.Type = "tare";
                        sapDataBackUp.Trans = "FMT";
                        sapDataBackUp.NoOfRetry = 0;
                        sapDataBackUp.Date = DateTime.Now;
                        sapDataBackUp.Response = response;
                        sapDataBackUp.CompletedTrans = 1;
                        sapDataBackUp.TransId = currentTransaction.TicketNo;
                        sapDataBackUp.TransType = "First Multi";
                        if (status == "failed")
                        {
                            sapDataBackUp.Status = "failed";
                        }
                        else if (status == "success")
                        {
                            sapDataBackUp.Status = "success";
                        }
                        commonFunction.InsertSAPDataBackUpDetails(sapDataBackUp);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("FirstMultiTransaction/SendDataToSAP/Exception:- " + ex.Message, ex);
            }
        }

        private List<CustomFieldBuilder> GetCustomFieldValuesFromAllocation(int allocationId)
        {
            string query = $@"select CustomFieldValues from [RFID_Allocations] where AllocationId={allocationId}";
            SqlCommand cmd = new SqlCommand(query);
            DataTable table = masterDBCall.GetData(cmd, CommandType.Text);
            foreach (DataRow row in table.Rows)
            {
                var JsonObj = JsonConvert.DeserializeObject<List<CustomFieldBuilder>>((string)row[0]);
                return JsonObj;
            }
            return new List<CustomFieldBuilder>();
        }

        private void SetValuesToFields(List<CustomFieldBuilder> customFieldBuilders)
        {
            if (customFieldBuilders != null)
            {
                foreach (var field in customFieldBuilders)
                {

                    if (field.ControlType == "TextBox")
                    {
                        if (field.FieldType == "DATETIME")
                        {
                            DatePicker datePicker = (DatePicker)FindName(field.RegName);
                            datePicker.SelectedDate = !string.IsNullOrEmpty(field.Value.ToString()) ? Convert.ToDateTime(field.Value) : (DateTime?)null;
                        }
                        else
                        {
                            TextBox textBox = (TextBox)FindName(field.RegName);
                            textBox.Text = field.Value.ToString();
                        }
                    }
                    else if (field.ControlType == "Dropdown")
                    {
                        ComboBox comboBox = (ComboBox)this.FindName(field.RegName);
                        foreach (var cmbi in comboBox.Items.Cast<ComboBoxItem>().Where(cmbi => (string)cmbi.Content == field.Value.ToString()))
                        {
                            cmbi.IsSelected = true;
                        }
                    }
                    else
                    {
                        TextBox textBox = (TextBox)FindName(field.RegName);
                        textBox.Text = field.Value.ToString();
                    }
                }
            }
        }
        #endregion
        #region Logging
        public void CreateLog(string message)
        {
            createTransLog.Invoke("single", new TransLogEventArgs(message));
        }
        #endregion
    }
}
