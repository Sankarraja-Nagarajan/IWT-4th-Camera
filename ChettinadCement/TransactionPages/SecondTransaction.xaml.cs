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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// <summary>
    /// Interaction logic for SecondTransaction.xaml
    /// </summary>
    public partial class SecondTransaction : Page
    {
        public static MasterDBCall masterDBCall = new MasterDBCall();
        public static CommonFunction commonFunction = new CommonFunction();
        public static MainWindow mainWindow = new MainWindow();
        private AdminDBCall _dbContext;
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        //public int StableWeightArraySize = int.Parse(ConfigurationManager.AppSettings["StableWeightArraySize"]);
        //public int StableWeightArraySelectable = int.Parse(ConfigurationManager.AppSettings["StableWeightArraySelectable"]);
        public int StableWeightArraySize = 10;
        public int StableWeightArraySelectable = 1;
        public List<CustomFieldBuilder> CustomFieldsBuilder = new List<CustomFieldBuilder>();
        public List<Company_Details> company_Details = new List<Company_Details>();
        public static List<string> GainedWeightList = new List<string>();
        string currentWeightment = "";
        private Transaction currentTransaction = new Transaction();
        public List<string> TypeList = new List<string>();
        private string _currentWeightment = "";
        SqlCommand cmd;
        int TicketNo = 0;
        AuthStatus authResult;
        Transaction pendingTicketsTransaction;
        public string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public string TransactionPath;
        List<ShiftMaster> shiftMasters = new List<ShiftMaster>();
        ShiftMaster CurrentShift = new ShiftMaster();

        public static event EventHandler<SelectTicketEventArgs> onSecondTransactionTicketSelected = delegate { };
        public static event EventHandler<TicketEventArgs> onSecondTicketCompletion = delegate { };
        List<FieldDependency> fieldDependencies = new List<FieldDependency>();
        private bool handleChangeEvent = true;
        public List<CustomFieldBuilder> FormulaFields = new List<CustomFieldBuilder>();
        List<ImageSourcePath> CurrentTransactionImageSourcePath = new List<ImageSourcePath>();
        ImageSourcePath FirstTransactionImageSourcePath = new ImageSourcePath();
        RolePriviliege rolePriviliege;
        UserHardwareProfile userHardwareProfile;
        Transaction CurrentTransaction;
        DataTable CurrentTransactionDataTable = new DataTable();
        ERPFileLocation selectedERPFileLocation = new ERPFileLocation();
        CloudAppConfig selectedCloudAppConfig = new CloudAppConfig();
        public static event EventHandler<AwsCompletedEventArgs> awsOperationCompleted = delegate { };
        AWSTransaction AwsTransaction = null;
        AWSConfiguration awsConfiguration = new AWSConfiguration();
        public static event EventHandler<TransLogEventArgs> createTransLog = delegate { };

        public SecondTransaction(AuthStatus _authResult, RolePriviliege _rolePriviliege, UserHardwareProfile _userHardwareProfile, AWSTransaction _transaction = null)
        {
            InitializeComponent();
            toastViewModel = new ToastViewModel();
            authResult = _authResult;
            this.rolePriviliege = _rolePriviliege;
            this.userHardwareProfile = _userHardwareProfile;
            AwsTransaction = _transaction;
            Loaded += SecondTransaction_Loaded;
            Unloaded += SecondTransaction_Unloaded;
            _dbContext = new AdminDBCall();
            TransactionPath = System.IO.Path.Combine(BaseDirectory, "TransactionPages");
            GetShiftMasters();
            fieldDependencies = GetAllFieldDependencies();
            GetCompanyDetails();
            GetOtherSettings();
            GetFileLocation();
            GetERPFileLocation();
            GetCloudAppConfig();
            GetStableWeightConfiguration();
            ApplyOtherSettings();
            BuildCustomFields();
            InitializeTypeList();
            InitializeTypeComboBox();
            if (_transaction == null)
                OpenPendingDialog();
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
                WriteLog.WriteToFile("Second/GetERPFileLocation", ex);
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
                WriteLog.WriteToFile("Second/GetCloudAppConfig", ex);
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
        private void SecondTransaction_Loaded(object sender, RoutedEventArgs e)
        {
            WriteLog.WriteToFile("SecondTransaction/SecondTransaction_Loaded:- LoadedEvent Invoked");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            onSecondTicketCompletion.Invoke(this, new TicketEventArgs("1"));
            MainWindow.onWeighmentReceived += Single_onWeighmentReceived;
            MainWindow.onSecondTransactionTicketSelection += MainWindow_onSecondTransactionTicketSelection;
            MainWindow.onImage1Recieved += MainWindow_onImage1Recieved;
            MainWindow.onImage2Recieved += MainWindow_onImage2Recieved;
            MainWindow.onImage3Recieved += MainWindow_onImage3Recieved;
            cCTVSettings = commonFunction.GetCCTVSettings(MainWindow.systemConfig.HardwareProfile);
            awsConfiguration = commonFunction.GetAWSConfiguration(MainWindow.systemConfig.HardwareProfile);
            Task.Run(() => StartAwsSequence(AwsTransaction));
        }

        private void MainWindow_onPlcReceived(object sender, PlcEventArgs e)
        {
            PlcValue = e.value;
        }

        private void SecondTransaction_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.onWeighmentReceived -= Single_onWeighmentReceived;
            MainWindow.onSecondTransactionTicketSelection -= MainWindow_onSecondTransactionTicketSelection;
            MainWindow.onImage1Recieved -= MainWindow_onImage1Recieved;
            MainWindow.onImage2Recieved -= MainWindow_onImage2Recieved;
            MainWindow.onImage3Recieved -= MainWindow_onImage3Recieved;
            MainWindow.onPlcReceived -= MainWindow_onPlcReceived;
        }

        private void MainWindow_onSecondTransactionTicketSelection(object sender, SelectTicketEventArgs e)
        {
            //Dispatcher.Invoke(() =>
            //{
            //    OpenPendingDialog();
            //});
            OpenPendingDialog();
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
                    customFieldBuilder.DisableStatus = JsonConvert.DeserializeObject<ControlStatus>(field.ControlLoadStatusDisable);
                    customFieldBuilder.MandatoryStatus = JsonConvert.DeserializeObject<ControlStatus>(field.MandatoryStatus);
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
                Tag = template.RegName,
                Style=style
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
                Style=style
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
                Tag = template.RegName,
                Style=style
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
                Style=style
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
                    Style=style
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
                    Style=style
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
                        WeighBtn.IsEnabled = true;
                    }
                    else
                    {
                        WeighBtn.IsEnabled = false;
                    }
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    WeighBtn.IsEnabled = false;
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
                        if ((bool)mainWindow.VPSEnable)
                        {
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
        private void LoadStatusToggleBtn_Click(object sender, RoutedEventArgs e)
        {
            var tb = (sender as ToggleButton);
            if (tb.IsChecked.HasValue && tb.IsChecked.Value)
            {
                MaterialName.IsEnabled = true;
                Types.IsEnabled = true;
                DocumentNumber.IsEnabled = true;
                GatePassNumber.IsEnabled = true;
                TokenNumber.IsEnabled = true;
                SupplierName.IsEnabled = true;
                SelectMaterialBtn.IsEnabled = true;
                SelectSupplierBtn.IsEnabled = true;
                AddMaterialBtn.IsEnabled = true;
                AddSupplierBtn.IsEnabled = true;
                LoadStatusBlock.Text = "Loaded";
                UpdateDisableMandatoryFields("STL");
            }
            else
            {
                MaterialName.IsEnabled = false;
                Types.IsEnabled = false;
                DocumentNumber.IsEnabled = false;
                GatePassNumber.IsEnabled = false;
                TokenNumber.IsEnabled = false;
                SupplierName.IsEnabled = false;
                SelectMaterialBtn.IsEnabled = false;
                SelectSupplierBtn.IsEnabled = false;
                AddMaterialBtn.IsEnabled = false;
                AddSupplierBtn.IsEnabled = false;
                LoadStatusBlock.Text = "Empty";
                UpdateDisableMandatoryFields("STE");
            }
        }
        public void OpenPendingDialog()
        {
            //_context = DataContext as ViewPendingTickets;
            //_context.ExecutePendingvehicleDialog();
            ExecutePendingvehicleDialog();
        }
        public async void ExecutePendingvehicleDialog()
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new PendingVehicleDialog();

            if (!DialogHost.IsDialogOpen("RootDialog"))
            {
                var result1 = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);

                DataTable dt1 = result1 as DataTable;
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    string JSONString = JsonConvert.SerializeObject(dt1);
                    var result = JsonConvert.DeserializeObject<List<Transaction>>(JSONString);
                    if (result.Count > 0)
                    {
                        pendingTicketsTransaction = result[0];
                        if (pendingTicketsTransaction != null)
                        {
                            onSecondTransactionTicketSelected.Invoke(this, new SelectTicketEventArgs(pendingTicketsTransaction.TicketNo.ToString()));

                            this.Dispatcher.Invoke(() =>
                            {
                                TicketNum.Text = pendingTicketsTransaction.TicketNo.ToString();
                                //TicketNum.Visibility = Visibility.Collapsed;
                                TicketNo = pendingTicketsTransaction.TicketNo;
                                VehicleNumber.Text = pendingTicketsTransaction.VehicleNo;
                                Types.Text = pendingTicketsTransaction.TransType;
                                DocumentNumber.Text = pendingTicketsTransaction.DocNumber;
                                GatePassNumber.Text = pendingTicketsTransaction.GatePassNumber;
                                TokenNumber.Text = pendingTicketsTransaction.TokenNumber;
                                MaterialName.Text = $@"{pendingTicketsTransaction.MaterialCode}/{pendingTicketsTransaction.MaterialName}";
                                SupplierName.Text = $@"{pendingTicketsTransaction.SupplierCode}/{pendingTicketsTransaction.SupplierName}";
                                //Date.Text = result.Date.ToShortDateString();
                                //Weight.Text = result.LoadWeight.ToString();
                                //Time.Text = result.Time;
                                // Loaded.Text = result.LoadStatus;
                                TareWeightBlock.Text = pendingTicketsTransaction.EmptyWeight.ToString();
                                LoadedWeightBlock.Text = pendingTicketsTransaction.LoadWeight.ToString();
                                NetWeightBlock.Text = pendingTicketsTransaction.NetWeight.ToString();
                                LoadStatusToggleBtn.IsEnabled = false;
                                if (pendingTicketsTransaction.LoadStatus == "Empty")
                                {
                                    //MaterialName.IsReadOnly = false;
                                    //SupplierName.IsReadOnly = false;
                                    MaterialName.IsEnabled = true;
                                    Types.IsEnabled = true;
                                    DocumentNumber.IsEnabled = true;
                                    GatePassNumber.IsEnabled = true;
                                    TokenNumber.IsEnabled = true;
                                    SupplierName.IsEnabled = true;
                                    SelectMaterialBtn.IsEnabled = true;
                                    SelectSupplierBtn.IsEnabled = true;
                                    AddMaterialBtn.IsEnabled = true;
                                    AddSupplierBtn.IsEnabled = true;
                                    LoadStatusToggleBtn.IsChecked = true;
                                    LoadStatusBlock.Text = "Loaded";
                                    UpdateDisableMandatoryFields("STL");
                                }
                                if (pendingTicketsTransaction.LoadStatus == "Loaded")
                                {
                                    //MaterialName.IsReadOnly = true;
                                    //SupplierName.IsReadOnly = true;
                                    MaterialName.IsEnabled = false;
                                    Types.IsEnabled = false;
                                    DocumentNumber.IsEnabled = false;
                                    GatePassNumber.IsEnabled = false;
                                    TokenNumber.IsEnabled = false;
                                    SupplierName.IsEnabled = false;
                                    SelectMaterialBtn.IsEnabled = false;
                                    SelectSupplierBtn.IsEnabled = false;
                                    AddMaterialBtn.IsEnabled = false;
                                    AddSupplierBtn.IsEnabled = false;
                                    LoadStatusToggleBtn.IsChecked = false;
                                    LoadStatusBlock.Text = "Empty";
                                    UpdateDisableMandatoryFields("STE");
                                }
                            });
                            GetFirstTransactionCapturedImages();
                        }
                        else
                        {
                            //vehicleNO = "";
                        }
                    }
                    foreach (DataRow row in dt1.Rows)
                    {
                        SetValuesToFields(row);
                    }
                }

            }

            //show the dialog

            //check the result...
            // //Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        //private async void SelectVehicleBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    await OpenvehicleDialog();
        //}

        //private async Task OpenvehicleDialog()
        //{
        //    //let's set up a little MVVM, cos that's what the cool kids are doing:
        //    var view = new Addvehicledialog();

        //    //show the dialog
        //    var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);

        //    //check the result...
        //    // //Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        //}

        public void GetFirstTransactionCapturedImages()
        {
            try
            {
                FirstTransactionImageSourcePath = new ImageSourcePath();
                var i = 1;
                foreach (var ccTV in cCTVSettings)
                {
                    var folder = ccTV.LogFolder;
                    FileInfo fi;
                    DirectoryInfo di = new DirectoryInfo(ccTV.LogFolder);

                    fi = di.GetFiles("*.jpeg").Where(file => file.Name.Contains($"{pendingTicketsTransaction.TicketNo}_") &&
                    file.Name.Contains($"cam{ccTV.RecordID}_")).FirstOrDefault();
                    if (fi != null)
                    {
                        var bytes = File.ReadAllBytes(fi.FullName);
                        if (i == 1)
                        {
                            FirstTransactionImageSourcePath.Image1Path = bytes;
                        }
                        if (i == 2)
                        {
                            FirstTransactionImageSourcePath.Image2Path = bytes;
                        }
                        if (i == 3)
                        {
                            FirstTransactionImageSourcePath.Image3Path = bytes;
                        }
                    }
                    i++;
                }
            }
            catch (Exception ex)
            {

            }

        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            // //Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }

        private void SetValuesToFields(DataRow row)
        {
            foreach (var field in CustomFieldsBuilder)
            {

                if (field.ControlType == "TextBox")
                {
                    if (field.FieldType == "DATETIME")
                    {
                        DatePicker datePicker = (DatePicker)FindName(field.RegName);
                        var dt = row[field.RegName]?.ToString();
                        datePicker.SelectedDate = !string.IsNullOrEmpty(dt) ? Convert.ToDateTime(dt) : (DateTime?)null;
                    }
                    else
                    {
                        TextBox textBox = (TextBox)FindName(field.RegName);
                        textBox.Text = row[field.RegName]?.ToString();
                    }
                }
                else if (field.ControlType == "Dropdown")
                {
                    ComboBox comboBox = (ComboBox)this.FindName(field.RegName);
                    var val = row[field.RegName]?.ToString().Trim();
                    foreach (var cmbi in comboBox.Items.Cast<ComboBoxItem>().Where(cmbi => (string)cmbi.Content == val))
                    {
                        cmbi.IsSelected = true;
                    }
                }
                else
                {
                    TextBox textBox = (TextBox)FindName(field.RegName);
                    textBox.Text = row[field.RegName]?.ToString();
                }
            }
        }


        private void SelectMaterialBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(VehicleNumber.Text))
            {
                OpenMaterialDialog();
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the vehicle number");
            }
        }

        private void AddMaterialBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(VehicleNumber.Text))
            {
                OpenMaterialDialog(1);
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the vehicle number");
            }
        }

        private async void OpenMaterialDialog(int selectedIndex = 0)
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new Addmaterial(selectedIndex);

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler1);

            if (result != null)
            {
                MaterialMaster material = result as MaterialMaster;
                if (material.MaterialID > 0)
                {
                    OpenSupplierDialog();
                }
                MaterialName.Text = $@"{material.MaterialCode}/{material.MaterialName}"; ;
            }

            //check the result...
            // //Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        private void ClosingEventHandler1(object sender, DialogClosingEventArgs eventArgs)
        {
            var result = eventArgs?.Parameter as MaterialMaster;
            if (result != null)
            {
                MaterialName.Text = $@"{result.MaterialCode}/{result.MaterialName}"; ;
                //MaterialCode.Text = Material_name;
            }
            else
            {
                //Material_name = "";
                //MaterialCode.Text = Material_name;
            }

            // //Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }

        private void SelectSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(VehicleNumber.Text))
            {
                OpenSupplierDialog();
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the vehicle number");
            }
        }
        private void AddSupplierBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(VehicleNumber.Text))
            {
                OpenSupplierDialog(1);
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the vehicle number");
            }
        }
        private async void OpenSupplierDialog(int selectedIndex = 0)
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new Addsupplier(selectedIndex);

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler2);

            if (result != null)
            {
                SupplierMaster supplier = result as SupplierMaster;
                SupplierName.Text = $@"{supplier.SupplierCode}/{supplier.SupplierName}";
            }

            //check the result...
            // //Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        private void ClosingEventHandler2(object sender, DialogClosingEventArgs eventArgs)
        {
            var result = eventArgs?.Parameter as SupplierMaster;
            if (result != null)
            {
                SupplierName.Text = $@"{result.SupplierCode}/{result.SupplierName}";
                //VehicleTareWeight.Text = Supplier_name;
            }
            else
            {
                //Supplier_name = "";
                //VehicleTareWeight.Text = Supplier_name;
            }

            // //Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }

        private void WeighBtn_Click(object sender, RoutedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(VehicleNumber.Text) && !string.IsNullOrEmpty(MaterialCode.Text) && !string.IsNullOrEmpty(SupplierCode.Text))
            //{
            //    currentWeightment = GetWighment();
            //    if (LoadStatusToggleBtn.IsChecked.HasValue && LoadStatusToggleBtn.IsChecked.Value)
            //    {
            //        LoadedWeightBlock.Text = currentWeightment;
            //    }
            //    else
            //    {
            //        TareWeightBlock.Text = currentWeightment;
            //    }
            //    SaveBtn.IsEnabled = true;
            //}
            //else
            //{
            //    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the required Fields");
            //    SaveBtn.IsEnabled = false;
            //}
            int EmptyWeight = 0;
            int LoadWeight = 0;
            int NetWeight = 0;
            if (!string.IsNullOrEmpty(VehicleNumber.Text))
            {
                if (VehicleNumber.Text.Length <= 10)
                {
                    if (LoadStatusBlock.Text == "Loaded")
                    {
                        if (!string.IsNullOrEmpty(MaterialName.Text) && MaterialName.Text != "/" && !string.IsNullOrEmpty(SupplierName.Text) && SupplierName.Text != "/")
                        {
                            currentWeightment = GetWighment();
                            if (LoadStatusToggleBtn.IsChecked.HasValue && LoadStatusToggleBtn.IsChecked.Value)
                            {
                                LoadedWeightBlock.Text = currentWeightment;
                            }
                            else
                            {
                                TareWeightBlock.Text = currentWeightment;
                            }
                            EmptyWeight = string.IsNullOrEmpty(TareWeightBlock.Text) ? 0 : Convert.ToInt32(TareWeightBlock.Text);
                            LoadWeight = string.IsNullOrEmpty(LoadedWeightBlock.Text) ? 0 : Convert.ToInt32(LoadedWeightBlock.Text);
                            NetWeight = LoadWeight - EmptyWeight;
                            NetWeightBlock.Text = NetWeight.ToString();
                            if (EmptyWeight <= 0 || LoadWeight <= 0)
                            {
                                SaveBtn.IsEnabled = false;
                                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Weight should be a positive value and greater than zero");
                            }
                            else if (NetWeight <= 0)
                            {
                                SaveBtn.IsEnabled = false;
                                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Loaded weight should be greater than empty weight");
                            }
                            else
                            {
                                SaveBtn.IsEnabled = true;
                            }

                        }
                        else
                        {
                            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the material and supplier details");
                            SaveBtn.IsEnabled = false;
                        }

                    }
                    else
                    {
                        currentWeightment = GetWighment();
                        if (LoadStatusToggleBtn.IsChecked.HasValue && LoadStatusToggleBtn.IsChecked.Value)
                        {
                            LoadedWeightBlock.Text = currentWeightment;
                        }
                        else
                        {
                            TareWeightBlock.Text = currentWeightment;
                        }

                        EmptyWeight = string.IsNullOrEmpty(TareWeightBlock.Text) ? 0 : Convert.ToInt32(TareWeightBlock.Text);
                        LoadWeight = string.IsNullOrEmpty(LoadedWeightBlock.Text) ? 0 : Convert.ToInt32(LoadedWeightBlock.Text);
                        NetWeight = LoadWeight - EmptyWeight;
                        NetWeightBlock.Text = NetWeight.ToString();
                        if (EmptyWeight <= 0 || LoadWeight <= 0)
                        {
                            SaveBtn.IsEnabled = false;
                            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Weight should be a positive value and greater than zero");
                        }
                        else if (NetWeight <= 0)
                        {
                            SaveBtn.IsEnabled = false;
                            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Empty weight should be less than loaded weight");
                        }
                        else
                        {
                            SaveBtn.IsEnabled = true;
                        }
                    }
                    SetFormulaFieldValues(EmptyWeight, LoadWeight, NetWeight);
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Maxmimum allowed character for vehicle number is 10");
                    SaveBtn.IsEnabled = false;
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the vehicle number");
                SaveBtn.IsEnabled = false;
            }
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

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var vehicle_number = VehicleNumber.Text;
            var material_name = MaterialName.Text;
            var supplier_name = SupplierName.Text;

            if (!string.IsNullOrEmpty(vehicle_number))
            {
                if (VehicleNumber.Text.Length <= 10)
                {
                    if (LoadStatusBlock.Text == "Loaded")
                    {
                        if (!string.IsNullOrEmpty(MaterialName.Text) && !string.IsNullOrEmpty(SupplierName.Text))
                        {
                            CheckAndSaveTransactionData();
                        }
                        else
                        {
                            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the material and supplier details");
                        }
                    }
                    else
                    {
                        CheckAndSaveTransactionData();
                    }
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Maxmimum allowed character for vehicle number is 10");
                    //SaveBtn.IsEnabled = false;
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the vehicle number");
            }
        }

        public async void CheckAndSaveTransactionData()
        {
            try
            {
                var vehicle_number = VehicleNumber.Text;
                var material_name = MaterialName.Text;
                var supplier_name = SupplierName.Text;
                CurrentTransaction = new Transaction();
                AdminDBCall db = new AdminDBCall();
                CurrentTransaction.TicketNo = pendingTicketsTransaction.TicketNo;
                CurrentTransaction.VehicleNo = pendingTicketsTransaction.VehicleNo = vehicle_number;
                CurrentTransaction.TransType = pendingTicketsTransaction.TransType;
                CurrentTransaction.DocNumber = pendingTicketsTransaction.DocNumber;
                CurrentTransaction.GatePassNumber = pendingTicketsTransaction.GatePassNumber;
                CurrentTransaction.TokenNumber = pendingTicketsTransaction.TokenNumber;
                //CurrentTransaction.MaterialName = pendingTicketsTransaction.MaterialName = material_name;
                SaveBtn.IsEnabled = false;

                var mat = MaterialName.Text?.Split('/');
                if (mat?.Length > 1)
                {

                    string[] strings = new string[mat.Length - 1];
                    for (var i = 1; i < mat?.Length; i++)
                    {
                        strings[i - 1] = mat[i];
                    }

                    CurrentTransaction.MaterialName = pendingTicketsTransaction.MaterialName = string.Join("/", strings);

                    //CurrentTransaction.MaterialName = pendingTicketsTransaction.MaterialName = mat[1];
                    CurrentTransaction.MaterialCode = pendingTicketsTransaction.MaterialCode = mat[0];
                }
                else
                {
                    CurrentTransaction.MaterialName = pendingTicketsTransaction.MaterialName = MaterialName.Text;
                    CurrentTransaction.MaterialCode = pendingTicketsTransaction.MaterialCode = "";
                }

                //CurrentTransaction.SupplierName = pendingTicketsTransaction.SupplierName = supplier_name;

                var sup = SupplierName.Text?.Split('/');
                if (sup?.Length > 1)
                {
                    string[] strings = new string[sup.Length - 1];
                    for (var i = 1; i < sup?.Length; i++)
                    {
                        strings[i - 1] = sup[i];
                    }

                    CurrentTransaction.SupplierName = pendingTicketsTransaction.SupplierName = string.Join("/", strings);

                    //CurrentTransaction.SupplierName = pendingTicketsTransaction.SupplierName = sup[1];
                    CurrentTransaction.SupplierCode = pendingTicketsTransaction.SupplierCode = sup[0];
                }
                else
                {
                    CurrentTransaction.SupplierName = pendingTicketsTransaction.SupplierName = SupplierName.Text;
                    CurrentTransaction.SupplierCode = pendingTicketsTransaction.SupplierCode = "";
                }

                CurrentTransaction.NoOfMaterial = pendingTicketsTransaction.NoOfMaterial = 1;
                CurrentTransaction.Date = pendingTicketsTransaction.Date = DateTime.Now;
                CurrentTransaction.EmptyWeight = pendingTicketsTransaction.EmptyWeight = string.IsNullOrEmpty(TareWeightBlock.Text) ? 0 : Convert.ToInt32(TareWeightBlock.Text);
                CurrentTransaction.LoadWeight = pendingTicketsTransaction.LoadWeight = string.IsNullOrEmpty(LoadedWeightBlock.Text) ? 0 : Convert.ToInt32(LoadedWeightBlock.Text);
                //data.EmptyWeightDate = DateTime.Now;
                //data.LoadWeightDate = DateTime.Now;
                CurrentTransaction.NetWeight = pendingTicketsTransaction.NetWeight = CurrentTransaction.LoadWeight - CurrentTransaction.EmptyWeight;
                CurrentTransaction.Pending = pendingTicketsTransaction.Pending = false;
                CurrentTransaction.Closed = pendingTicketsTransaction.Closed = true;
                CurrentTransaction.MultiWeight = pendingTicketsTransaction.MultiWeight = false;
                CurrentTransaction.MultiWeightTransPending = pendingTicketsTransaction.MultiWeightTransPending = false;
                CurrentTransaction.ShiftName = pendingTicketsTransaction.ShiftName = CurrentShift?.ShiftName;
                CurrentTransaction.LoadStatus = pendingTicketsTransaction.LoadStatus = LoadStatusBlock.Text;
                if (CurrentTransaction.LoadStatus == "Loaded")
                {
                    CurrentTransaction.LoadWeightDate = pendingTicketsTransaction.LoadWeightDate = DateTime.Now;
                    CurrentTransaction.LoadWeightTime = pendingTicketsTransaction.LoadWeightTime = DateTime.Now.ToString("hh:mm:ss tt");
                    CurrentTransaction.EmptyWeightDate = pendingTicketsTransaction.EmptyWeightDate;
                    CurrentTransaction.EmptyWeightTime = pendingTicketsTransaction.EmptyWeightTime;
                }
                else
                {
                    CurrentTransaction.EmptyWeightDate = pendingTicketsTransaction.EmptyWeightDate = DateTime.Now;
                    CurrentTransaction.EmptyWeightTime = pendingTicketsTransaction.EmptyWeightTime = DateTime.Now.ToString("hh:mm:ss tt");
                    CurrentTransaction.LoadWeightDate = pendingTicketsTransaction.LoadWeightDate;
                    CurrentTransaction.LoadWeightTime = pendingTicketsTransaction.LoadWeightTime;
                }
                pendingTicketsTransaction.State = "ST";
                pendingTicketsTransaction.TransactionType = "Second";
                GetValuesFromFields();
                bool res = BuildTransactionInsertQuery(CurrentTransaction, CustomFieldsBuilder);
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

                    //else if (isTemplateSelected)
                    //{

                    //}
                    if (isTemplateSelected && !string.IsNullOrEmpty(_reportTemplate) && File.Exists(_reportTemplate))
                    {
                        if (_otherSettings != null && _otherSettings.AutoFtPrint)
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
                    //if (isTemplateSelected && _otherSettings != null && _otherSettings.AutoPrint)
                    //{
                    //    if (_otherSettings != null && _otherSettings.AutoCopies)
                    //    {
                    //        await OpenCopiesDialog();
                    //    }
                    //    ReportViewerDemo1.LocalReport.PrintToPrinter(_noOfCopies);
                    //}
                    await ExecuteAutoMethods();
                    if (selectedERPFileLocation != null && selectedERPFileLocation.ID != 0 && selectedERPFileLocation.IsEnabled)
                    {
                        SendFileToERP();
                    }
                    if (selectedCloudAppConfig != null && selectedCloudAppConfig.ID != 0 && selectedCloudAppConfig.IsEnabled)
                    {
                        SendFileToAPI();
                    }
                    if (!popup.IsOpen)
                    {
                        ClearTransaction();
                    }
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Saved Successfully");
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong!!!");
                }
            }
            catch (Exception ex)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, ex.Message);
                WriteLog.WriteToFile("CheckAndSaveTransactionData:" + ex.Message);
            }
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
                //commonFunction.SendTransactionDetailsToCloudApp(selectedCloudAppConfig, CurrentTransactionDataTable, CurrentTransaction.TicketNo);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SendFileToAPI:" + ex.Message);
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
        public bool BuildTransactionInsertQuery(Transaction data, List<CustomFieldBuilder> customFieldBuilders)
        {
            try
            {
                string Query = "UPDATE [Transaction] SET EmptyWeight=@EmptyWeight,EmptyWeightDate=@EmptyWeightDate,EmptyWeightTime=@EmptyWeightTime,ShiftName=@ShiftName," +
                                                        "LoadWeight=@LoadWeight,LoadWeightDate=@LoadWeightDate,LoadWeightTime=@LoadWeightTime,NetWeight=@NetWeight," +
                                                        "Pending=@Pending,Closed=@Closed,LoadStatus=@LoadStatus,State=@State," +
                                                        "MaterialCode=@MaterialCode,MaterialName=@MaterialName,SupplierCode=@SupplierCode,SupplierName=@SupplierName,TransType=@TransType,DocNumber=@DocNumber,GatePassNumber=@GatePassNumber,TokenNumber=@TokenNumber,IsSapBased=@IsSapBased,";
                foreach (var field in customFieldBuilders)
                {
                    Query += $"{field.FieldName}=@{field.FieldName},";
                }
                Query += "TransactionType = @TransactionType WHERE TicketNo = @TicketNo";
                cmd = new SqlCommand(Query);
                cmd.Parameters.Add("@TicketNo", SqlDbType.VarChar).Value = TicketNo;
                cmd.Parameters.Add("@EmptyWeight", SqlDbType.Int).Value = data.EmptyWeight;
                cmd.Parameters.Add("@EmptyWeightDate", SqlDbType.DateTime2).Value = data.EmptyWeightDate;
                cmd.Parameters.Add("@EmptyWeightTime", SqlDbType.VarChar).Value = data.EmptyWeightTime;
                cmd.Parameters.Add("@LoadWeight", SqlDbType.Int).Value = data.LoadWeight;
                cmd.Parameters.Add("@LoadWeightDate", SqlDbType.DateTime2).Value = data.LoadWeightDate;
                cmd.Parameters.Add("@LoadWeightTime", SqlDbType.VarChar).Value = data.LoadWeightTime;
                cmd.Parameters.Add("@Netweight", SqlDbType.Int).Value = data.NetWeight;
                cmd.Parameters.Add("@Pending", SqlDbType.Bit).Value = data.Pending;
                cmd.Parameters.Add("@Closed", SqlDbType.Bit).Value = data.Closed;
                cmd.Parameters.Add("@Multiweight", SqlDbType.Bit).Value = data.MultiWeight;
                cmd.Parameters.Add("@MultiweightTransPending", SqlDbType.Bit).Value = data.MultiWeightTransPending;
                cmd.Parameters.Add("@LoadStatus", SqlDbType.VarChar).Value = data.LoadStatus;
                cmd.Parameters.Add("@ShiftName", SqlDbType.VarChar).Value = string.IsNullOrEmpty(data.ShiftName) ? "Shift" : data.ShiftName;
                cmd.Parameters.Add("@MaterialCode", SqlDbType.VarChar).Value = data.MaterialCode;
                cmd.Parameters.Add("@MaterialName", SqlDbType.VarChar).Value = data.MaterialName;
                cmd.Parameters.Add("@SupplierCode", SqlDbType.VarChar).Value = data.SupplierCode;
                cmd.Parameters.Add("@SupplierName", SqlDbType.VarChar).Value = data.SupplierName;
                cmd.Parameters.Add("@TransType", SqlDbType.VarChar).Value = data.TransType;
                cmd.Parameters.Add("@DocNumber", SqlDbType.VarChar).Value = data.DocNumber;
                cmd.Parameters.Add("@GatePassNumber", SqlDbType.VarChar).Value = data.GatePassNumber;
                cmd.Parameters.Add("@TokenNumber", SqlDbType.VarChar).Value = data.TokenNumber;
                cmd.Parameters.Add("@IsSapBased", SqlDbType.Bit).Value = data.IsSapBased;
                cmd.Parameters.Add("@State", SqlDbType.VarChar).Value = "ST";
                cmd.Parameters.Add("@TransactionType", SqlDbType.VarChar).Value = "Second";
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = authResult?.UserName;
                foreach (var field in customFieldBuilders)
                {
                    cmd.Parameters.AddWithValue($"@{field.FieldName}", field.Value ?? DBNull.Value);
                }
                masterDBCall.InsertData(cmd, CommandType.Text);
                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("BuildTransactionInsertQuery:" + ex.Message);
                return false;
            }
        }

        private void ClearTransaction()
        {
            TicketNum.Text = "";
            TareWeightBlock.Text = "0";
            LoadedWeightBlock.Text = "0";
            NetWeightBlock.Text = "0";
            VehicleNumber.Text = "";
            MaterialName.Text = "";
            SupplierName.Text = "";
            Types.Text = "";
            DocumentNumber.Text = "";
            GatePassNumber.Text = "";
            TokenNumber.Text = "";
            currentWeightment = "";
            WeighBtn.IsEnabled = false;
            SaveBtn.IsEnabled = false;
            Auto_MobileNumber.Text = "";
            Auto_Email.Text = "";
            ResetFields();
            CurrentTransaction = new Transaction();
            //_reportTemplate = "";
            _reportTemplate = _fileLocation.Default_Temp;
            onSecondTransactionTicketSelected.Invoke(this, new SelectTicketEventArgs(""));
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

        public void openDialog()
        {
            if (!popup.IsOpen)
            {
                popup.IsOpen = true;
                //GetData();
            }// Open it if it's not open
            else popup.IsOpen = false;
        }
        private void GetTransactionData()
        {
            AdminDBCall db = new AdminDBCall();
            CurrentTransactionDataTable = db.GetAllData("select * from [Transaction] where TicketNo = " + TicketNo);
            string JSONString = JsonConvert.SerializeObject(CurrentTransactionDataTable);
            var result = JsonConvert.DeserializeObject<List<Transaction>>(JSONString);
            if (result != null && result.Count > 0)
            {
                //CurrentTransaction.TicketNo = result[0].TicketNo;
                CaptureCameraImage(result[0]);
            }


            if (CurrentTransactionDataTable.Rows.Count > 0)
            {
                if (LoadStatusBlock.Text == "Loaded")
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

            ReportDataSource rds = new ReportDataSource("DataSet1", CurrentTransactionDataTable);
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
                WriteLog.WriteToFile("FirstTransaction/FindCurrentShift/Exception:- " + ex.Message, ex);
            }
        }

        private void GetCompanyDetails()
        {
            company_Details = commonFunction.GetCompanyDetails();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
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
            var message = commonFunction.BuildSMS(pendingTicketsTransaction);
            await commonFunction.CheckAndSendSMS(message);
        }

        private async void EmailBtn_Click(object sender, RoutedEventArgs e)
        {
            var message = commonFunction.BuildEmail(pendingTicketsTransaction);
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
            await commonFunction.CheckAndSendEmail(pendingTicketsTransaction, message, bytes, fileName);
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
        public void CaptureCameraImage(Transaction transaction)
        {
            ImageSourcePath imageSourcePath = new ImageSourcePath();
            foreach (var camera in cCTVSettings)
            {
                if (camera.Enable)
                {
                    string imagePath = $"{camera.LogFolder}\\{transaction.TicketNo}_{transaction.State}_cam{camera.RecordID.ToString()}_{DateTime.Now:ddMMyyyyhhmmss}.jpeg";
                    ImageSource imageSource = null;
                    if (camera.RecordID == 1)
                    {
                        imageSource = image1.Source;
                        imageSourcePath.Image4Path = commonFunction.SaveCameraImage(imageSource, imagePath);
                    }
                    else if (camera.RecordID == 2)
                    {
                        imageSource = image2.Source;
                        imageSourcePath.Image5Path = commonFunction.SaveCameraImage(imageSource, imagePath);
                    }
                    else if (camera.RecordID == 3)
                    {
                        imageSource = image3.Source;
                        imageSourcePath.Image6Path = commonFunction.SaveCameraImage(imageSource, imagePath);
                    }
                }
            }
            imageSourcePath.Image1Path = FirstTransactionImageSourcePath.Image1Path;
            imageSourcePath.Image2Path = FirstTransactionImageSourcePath.Image2Path;
            imageSourcePath.Image3Path = FirstTransactionImageSourcePath.Image3Path;
            CurrentTransactionImageSourcePath = new List<ImageSourcePath>();
            CurrentTransactionImageSourcePath.Add(imageSourcePath);
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
        private void MainWindow_onImage3Recieved(object sender, CameraEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                image3.Source = e.bitmap;
            }));
        }

        private void MainWindow_onImage2Recieved(object sender, CameraEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                image2.Source = e.bitmap;
            }));
        }

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
                    if (_otherSettings != null && _otherSettings.AutoMail)
                    {
                        AutoMail_Container.Visibility = Visibility.Visible;
                        EmailBtn.Visibility = Visibility.Collapsed;
                    }
                    if (_otherSettings != null && _otherSettings.SMSAlerts)
                    {
                        AutoSMS_Container.Visibility = Visibility.Visible;
                        SMSBtn.Visibility = Visibility.Collapsed;
                    }
                    if (_otherSettings != null && _otherSettings.AutoPrint)
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
                if (_otherSettings != null && _otherSettings.AutoMail)
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

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearTransaction();
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

        public void SetLoadStatus(bool status)
        {
            if (status)
            {
                LoadStatusBlock.Text = "Loaded";
                if (AddMaterialBtn != null && AddSupplierBtn != null)
                {
                    SelectMaterialBtn.IsEnabled = true;
                    SelectSupplierBtn.IsEnabled = true;
                    AddMaterialBtn.IsEnabled = true;
                    AddSupplierBtn.IsEnabled = true;
                    MaterialName.IsEnabled = true;
                    SupplierName.IsEnabled = true;
                    Types.IsEnabled = true;
                    DocumentNumber.IsEnabled = true;
                    GatePassNumber.IsEnabled = true;
                    TokenNumber.IsEnabled = true;
                }
            }
            else
            {
                LoadStatusBlock.Text = "Empty";
                if (AddMaterialBtn != null && AddSupplierBtn != null)
                {
                    SelectMaterialBtn.IsEnabled = false;
                    SelectSupplierBtn.IsEnabled = false;
                    AddMaterialBtn.IsEnabled = false;
                    AddSupplierBtn.IsEnabled = false;
                    MaterialName.IsEnabled = false;
                    SupplierName.IsEnabled = false;
                    Types.IsEnabled = true;
                    DocumentNumber.IsEnabled = true;
                    GatePassNumber.IsEnabled = true;
                    TokenNumber.IsEnabled = true;
                }
            }
            LoadStatusToggleBtn.IsChecked = status;
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
                if (ex.InnerException != null)
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, ex.InnerException.Message);
                else
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, ex.Message);
                WriteLog.WriteAWSLog("Exception:-", ex);
                if (ex.InnerException != null)
                    CreateLog($"Exception:- {ex.InnerException.Message}");
                else
                    CreateLog($"Exception:- {ex.Message}");
                IsAwsStarted = false;
                awsOperationCompleted.Invoke("second", new AwsCompletedEventArgs());
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
            WriteLog.WriteAWSLog($"<========================AWS Sequence Started(Second Transaction)({transaction.AllocationData.RFIDTag})(TicketNo:-{transaction.TransactionData.TicketNo})==========================>");
            CreateLog($"<========= AWS sequence started(Second Transaction)({transaction.AllocationData.RFIDTag})(TicketNo:-{transaction.TransactionData.TicketNo}) ==============>");
            IsAwsStarted = true;
            currentTransaction = transaction.TransactionData;
            currentAllocation = transaction.AllocationData;
            this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                SetLoadStatus(currentTransaction.LoadStatus == "Empty");
                MaterialName.Text = $@"{currentTransaction.MaterialCode}/{currentTransaction.MaterialName}";
                SupplierName.Text = $@"{currentTransaction.SupplierCode}/{currentTransaction.SupplierName}";
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
                TareWeightBlock.Text = currentTransaction.EmptyWeight.ToString();
                LoadedWeightBlock.Text = currentTransaction.LoadWeight.ToString();
                NetWeightBlock.Text = currentTransaction.NetWeight.ToString();
                onSecondTransactionTicketSelected.Invoke(this, new SelectTicketEventArgs(currentTransaction.TicketNo.ToString()));
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
            CreateLog($"{PlcValue} recieved from PLC");
            WriteLog.WriteAWSLog($"{PlcValue} recieved from PLC");
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

            //Auto gate exit (Non SAP)
            if (!transaction.AllocationData.IsSapBased && transaction.AllocationData.AllocationType == "Temporary" && awsConfiguration.AutoGateExit.HasValue && awsConfiguration.AutoGateExit.Value)
            {
                commonFunction.AutoGateExit(transaction.AllocationData);
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
            }));
            IsAwsStarted = false;
            AwsTransaction = null;
            WriteLog.WriteAWSLog($"<========================AWS Sequence Completed==========================>");
            CreateLog("<============ AWS sequence completed ============>");
            awsOperationCompleted.Invoke("second", new AwsCompletedEventArgs());
        }
        private bool CaptureWeight()
        {
            while (!CheckIsStable()) { }
            currentWeightment = GetWighment();
            int EmptyWeight = 0;
            int LoadWeight = 0;
            int NetWeight = 0;
            if (currentTransaction.LoadStatus == "Empty")
            {
                this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => {
                    LoadedWeightBlock.Text = currentWeightment;
                    EmptyWeight = string.IsNullOrEmpty(TareWeightBlock.Text) ? 0 : Convert.ToInt32(TareWeightBlock.Text);
                    LoadWeight = string.IsNullOrEmpty(LoadedWeightBlock.Text) ? 0 : Convert.ToInt32(LoadedWeightBlock.Text);
                }));
                NetWeight = LoadWeight - EmptyWeight;
                this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                {
                    NetWeightBlock.Text = NetWeight.ToString();
                    SetFormulaFieldValues(EmptyWeight, LoadWeight, NetWeight);
                }));
                if (EmptyWeight <= 0 || LoadWeight <= 0)
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Weight should be a positive value and greater than zero");
                    return false;
                }
                else if (NetWeight <= 0)
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Loaded weight should be greater than empty weight");
                    return false;
                }
            }
            else
            {
                this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                {
                    TareWeightBlock.Text = currentWeightment;
                    EmptyWeight = string.IsNullOrEmpty(TareWeightBlock.Text) ? 0 : Convert.ToInt32(TareWeightBlock.Text);
                    LoadWeight = string.IsNullOrEmpty(LoadedWeightBlock.Text) ? 0 : Convert.ToInt32(LoadedWeightBlock.Text);
                }));

                NetWeight = LoadWeight - EmptyWeight;
                this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                {
                    NetWeightBlock.Text = NetWeight.ToString();
                    SetFormulaFieldValues(EmptyWeight, LoadWeight, NetWeight);
                }));
                if (EmptyWeight <= 0 || LoadWeight <= 0)
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Weight should be a positive value and greater than zero");
                    return false;
                }
                else if (NetWeight <= 0)
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Empty weight should be less than loaded weight");
                    return false;
                }
            }
            currentTransaction.EmptyWeight = EmptyWeight;
            currentTransaction.LoadWeight = LoadWeight;
            currentTransaction.NetWeight = NetWeight;
            return true;
        }
        private bool SaveTransaction()
        {
            TicketNo = currentTransaction.TicketNo;
            this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                if (currentTransaction.LoadStatus=="Loaded")
                {
                    currentTransaction.EmptyWeightDate = DateTime.Now;
                    currentTransaction.EmptyWeightTime = DateTime.Now.ToString("hh:mm:ss tt");
                }
                else
                {
                    currentTransaction.LoadWeightDate = DateTime.Now;
                    currentTransaction.LoadWeightTime = DateTime.Now.ToString("hh:mm:ss tt");
                }
                GetValuesFromFields();
            }));
            currentTransaction.TransactionType = "Second";
            currentTransaction.MultiWeight = false;
            currentTransaction.Pending = false;
            currentTransaction.Closed = true;
            currentTransaction.MultiWeightTransPending = false;
            currentTransaction.SystemID = "";
            currentTransaction.UserName = authResult.UserName;
            currentTransaction.NoOfMaterial = 1;
            currentTransaction.ShiftName = CurrentShift?.ShiftName;
            currentTransaction.State = "ST";

            var res=BuildTransactionInsertQuery(currentTransaction, CustomFieldsBuilder);
            if (res)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(async () =>
                {
                    GetTransactionData();
                    if (!string.IsNullOrEmpty(_reportTemplate) && File.Exists(_reportTemplate))
                    {
                        if (_otherSettings != null && awsConfiguration.STPrintEnable.HasValue && awsConfiguration.STPrintEnable.Value)
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

                    DataTable table1 = _dbContext.GetAllData($"SELECT * FROM [GatePassItems] WHERE [GatePassId] = {currentAllocation.GatePassId}");
                    string itemsJson = JsonConvert.SerializeObject(table1);
                    var items = JsonConvert.DeserializeObject<List<GatePassItems>>(itemsJson);
                    string itemNo = "";
                    string tk_gpNo = "";
                    if (currentGatePass.InOut.ToLower() == "in")
                    {
                        docNumber = currentGatePass.PoNumber;
                        tk_gpNo = currentGatePass.GatePassNumber;
                        var item = items.FirstOrDefault(t => t.ItemType == "PO");
                        if (item != null)
                            itemNo = item.ItemNumber;
                    }
                    else
                    {
                        docNumber = currentGatePass.SoNumber;
                        tk_gpNo = currentGatePass.TokenNumber;
                        var item = items.FirstOrDefault(t => t.ItemType == "SO");
                        if (item != null)
                            itemNo = item.ItemNumber;
                    }
                    SAPInterfaceCall interfaceCalls = new SAPInterfaceCall();
                    if (currentTransaction.LoadStatus == "Empty")
                    {
                        var grossWeight = new GrossWeight
                        {
                            TokenNoGpNo = tk_gpNo,
                            CompPlant = currentGatePass.Plant,
                            SrNo = "1",
                            InOut = currentGatePass.InOut,
                            VbelnEbeln = docNumber,
                            MatNr = currentTransaction.MaterialCode,
                            PosNr = itemNo,
                            GrossWt = currentTransaction.LoadWeight.ToString(),
                            GrossUom = "KG",
                            GrossDt = currentTransaction.LoadWeightDate.Value.ToString("yyyyMMdd"),
                            GrossTime = commonFunction.FormatTime(currentTransaction.LoadWeightTime),
                            LodeEnd = "X"
                        };
                        var grossWeightResponse = await interfaceCalls.PostGrossWeight(grossWeight);
                        WriteLog.WriteErrorLog($"SecondTransaction/SendDataToSAP/GROSS/Response:- {grossWeightResponse}");
                        char[] seperator = { '-' };
                        string[] grossWeightResponsearr = null;
                        grossWeightResponsearr = grossWeightResponse.Split(seperator);
                        var status = grossWeightResponsearr[0];
                        var response = grossWeightResponsearr[1];
                        sapDataBackUp.Payload = JsonConvert.SerializeObject(grossWeight);
                        sapDataBackUp.Type = "gross";
                        sapDataBackUp.Trans = "ST";
                        sapDataBackUp.NoOfRetry = 0;
                        sapDataBackUp.Date = DateTime.Now;
                        sapDataBackUp.Response = response;
                        sapDataBackUp.CompletedTrans = 1;
                        sapDataBackUp.TransId = currentTransaction.TicketNo;
                        sapDataBackUp.TransType = "Second";
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
                            MatNr = currentTransaction.MaterialCode,
                            PosNr = itemNo,
                            TareWt = currentTransaction.EmptyWeight.ToString(),
                            TareUom = "KG",
                            TareDt = currentTransaction.EmptyWeightDate.Value.ToString("yyyyMMdd"),
                            TareTime = commonFunction.FormatTime(currentTransaction.EmptyWeightTime),
                            UnlodeEnd = "X"
                        };
                        var tareWeightResponse = await interfaceCalls.PostTareWeight(tareWeight);
                        WriteLog.WriteErrorLog($"SecondTransaction/SendDataToSAP/TARE/Response:- {tareWeightResponse}");
                        char[] seperator = { '-' };
                        string[] tareWeightResponsearr = null;
                        tareWeightResponsearr = tareWeightResponse.Split(seperator);
                        var status = tareWeightResponsearr[0];
                        var response = tareWeightResponsearr[1];
                        sapDataBackUp.Payload = JsonConvert.SerializeObject(tareWeight);
                        sapDataBackUp.Type = "tare";
                        sapDataBackUp.Trans = "ST";
                        sapDataBackUp.NoOfRetry = 0;
                        sapDataBackUp.Date = DateTime.Now;
                        sapDataBackUp.Response = response;
                        sapDataBackUp.CompletedTrans = 1;
                        sapDataBackUp.TransId = currentTransaction.TicketNo;
                        sapDataBackUp.TransType = "Second";
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
                WriteLog.WriteToFile("SecondTransaction/SendDataToSAP/Exception:- " + ex.Message, ex);
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
