using IWT.Admin_Pages;
using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.TransactionPages;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace IWT.Views
{
    /// <summary>
    /// Interaction logic for RFIDAllocationUserControl.xaml
    /// </summary>
    public partial class RFIDAllocationUserControl : UserControl, INotifyPropertyChanged
    {
        string ExpiryDateForRFID;
        MaterialMaster material;
        SupplierMaster supplier;
        MaterialMaster result;
        private string ConnectionString { get; set; }
        //public List<RFIDAllocation> RFIDAllocations = new List<RFIDAllocation>();
        RFIDAllocation SelectedRFIDAllocation = new RFIDAllocation();
        AWSConfiguration awsConfiguration = new AWSConfiguration();
        VehicleMaster VehicleDetails = new VehicleMaster();
        Transaction transaction = new Transaction();
        GatePasses gatePasses = new GatePasses();
        GatePasses getGatePassesData = new GatePasses();
        public List<IsLoaded> LoadStatusList = new List<IsLoaded>();
        public List<string> GateList = new List<string>();
        List<TransactionTypeMaster> transactionTypeMasters = new List<TransactionTypeMaster>();
        public CommonFunction commonFunction = new CommonFunction();
        public MasterDBCall masterDBCall = new MasterDBCall();
        public List<string> RFIDAllocationTypeList = new List<string>();
        public List<IsSapBased> SAPTypeList = new List<IsSapBased>();
        public List<string> TypeList = new List<string>();
        public List<string> DocumentNumberList = new List<string>();
        AuthStatus authResult;
        ShiftMaster CurrentShift = new ShiftMaster();

        public List<CustomFieldBuilder> CustomFieldsBuilder = new List<CustomFieldBuilder>();
        public List<CustomFieldBuilder> FormulaFields = new List<CustomFieldBuilder>();
        List<FieldDependency> fieldDependencies = new List<FieldDependency>();
        OperationType selectedOperationType = new OperationType();
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        private RFIDAllocation SelectedRow;
        private bool _isRowSelected = false;
        private AdminDBCall _dbContext;

        private List<StoreMaterialManagement> CurrentStoreMaterials = new List<StoreMaterialManagement>();
        private List<StoreSupplierManagement> CurrentStoreSuppliers = new List<StoreSupplierManagement>();

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
        private bool handleChangeEvent = true;

        List<RFIDAllocationWithTrans> Result = new List<RFIDAllocationWithTrans>();

        List<RFIDMaster> RFIDMasters = new List<RFIDMaster>();

        List<GatePasses> GatePasses = new List<GatePasses>();
        public int CurrentPage
        {
            get { return _CurrentPage; }
            set
            {
                _CurrentPage = value;
                OnPropertyChanged();
            }
        }

        public int NumberOfPages
        {
            get { return _NumberOfPages; }
            set
            {
                _NumberOfPages = value;
                OnPropertyChanged();
            }
        }
        public int SelectedRecord
        {
            get { return _SelectedRecord; }
            set
            {
                _SelectedRecord = value;
                OnPropertyChanged();
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

        public RFIDAllocationUserControl(string _Rolename, RolePriviliege _rolePriviliege)
        {
            InitializeComponent();
            DataContext = this;
            SelectedRecord = 10;
            NumberOfPages = 1;
            _dbContext = new AdminDBCall();
            ConnectionString = _dbContext.GetDecryptedConnectionStringDB();
            toastViewModel = new ToastViewModel();
            Loaded += RFIDAllocationUserControl_Loaded;
            Unloaded += RFIDAllocationUserControl_Unloaded;
        }

        private void RFIDAllocationUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.onRfid1Received -= MainWindow_onRfid1Received;
        }

        private void RFIDAllocationUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeLoadStatusList();
            InitializeLoadStatusComboBox();
            InitializeTransactionTypeMaster();
            InitializeTransactionTypeComboBox();
            InitializeRFIDAllocationTypeList();
            InitializeRFIDAllocationTypeComboBox();
            InitializeSAPTypeList();
            InitializeSAPTypeComboBox();
            InitializeTypeList();
            InitializeTypeComboBox();
            fieldDependencies = GetAllFieldDependencies();
            BuildCustomFields();
            MainWindow.onRfid1Received += MainWindow_onRfid1Received;
            awsConfiguration = commonFunction.GetAWSConfiguration(MainWindow.systemConfig.HardwareProfile);
            GetRFIDMasters();
            GetRFIDAllocationRecentTransaction();
            ResetFields();
            ResetRFIDAllocations();
            if ((bool)awsConfiguration.SAP)
            {
                SAPType.SelectedIndex = 0;
                document.Visibility = Visibility.Visible;
                gatePassNumber.Visibility = Visibility.Visible;
                tokenNumber.Visibility = Visibility.Visible;
            }
            else
            {
                SAPType.SelectedIndex = 1;
                document.Visibility = Visibility.Collapsed;
                gatePassNumber.Visibility = Visibility.Collapsed;
                tokenNumber.Visibility = Visibility.Collapsed;
            }
            noOfMaterial.Visibility = Visibility.Collapsed;
            ExpiryDate.Text = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd HH:mm:ss");
            RFIDAllocationType.Text = "Temporary";

        }

        bool rfidDetect = false;
        bool isPermanent = false;
        private void MainWindow_onRfid1Received(object sender, RfidEventArgs e)
        {
            if (!rfidDetect)
            {
                rfidDetect = true;
                Task.Run(() => InvokeRfId(e));
            }
        }

        private void InvokeRfId(RfidEventArgs e)
        {
            if (!isPermanent)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                {
                    CheckAndSaveRfid(e.tag);

                    var item = RFIDNo.Items.Cast<RFIDMaster>().FirstOrDefault(t => t.Tag == e.tag);
                    RFIDNo.SelectedItem = item;
                }));
            }

            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowInformation, $"RFID detected - {e.tag}");
            Thread.Sleep(5000);
            rfidDetect = false;
        }

        private void CheckAndSaveRfid(string rfid)
        {
            DataTable table = _dbContext.GetAllData($"SELECT * FROM [RFID_Tag_Master] where Tag='{rfid}'");
            string JSONString = JsonConvert.SerializeObject(table);
            var rfids = JsonConvert.DeserializeObject<List<RFIDMaster>>(JSONString);
            if (rfids != null && rfids.Count() > 0)
            {
                GetRFIDMasters();
            }
            else
            {
                string insertQuery = $"insert into [RFID_Tag_Master] (Tag,Status,IsActive) values ('{rfid}','Open',1)";
                var res = _dbContext.ExecuteQuery(insertQuery);
                if (res)
                {
                    WriteLog.WriteToFile($"CheckAndSaveRfid:- new RFID tag inserted!!  <----- {rfid} ----->");
                    GetRFIDMasters();
                }
            }
        }

        public void GetRFIDMasters()
        {
            try
            {
                DataTable table = _dbContext.GetAllData($"SELECT * FROM [RFID_Tag_Master] WHERE [Status] = 'Open' and IsActive=1");
                string JSONString = JsonConvert.SerializeObject(table);
                RFIDMasters = JsonConvert.DeserializeObject<List<RFIDMaster>>(JSONString);
                SetRFIDComboBox();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetRFIDMasters", ex);
            }
        }

        public void SetRFIDComboBox()
        {
            try
            {
                RFIDNo.ItemsSource = RFIDMasters;
                RFIDNo.Items.Refresh();
                RFIDNo.DisplayMemberPath = "Tag";
                RFIDNo.SelectedValuePath = "Tag";
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetRFIDComboBox", ex);
            }
        }

        public List<GatePassItems> GetGatePasseItemByGatePassId(int GatePassId, string type = "Material")
        {
            try
            {
                string Query = $"SELECT * FROM GatePassItems WHERE GatePassId=@GatePassId and ItemType='{type}'";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@GatePassId", GatePassId);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var Result = JsonConvert.DeserializeObject<List<GatePassItems>>(JSONString);
                if (Result != null)
                {
                    return Result;
                }
                return new List<GatePassItems>();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetGatePasseItemByGatePassId", ex);
                return new List<GatePassItems>();
            }
        }

        public void SAPComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = SAPType.SelectedIndex;
            if (index == 0)
            {
                RFIDNo.Text = "";
                VehicleNumber.Text = "";
                MaterialName.Text = "";
                SupplierName.Text = "";
                DefaultTareWeight.Text = "";
                LoadStatus.SelectedIndex = -1;
                TransactionType.SelectedIndex = -1;
                RFIDAllocationType.Text = "Temporary";
                Types.SelectedIndex = -1;
                DocumentNumber.Text = "";
                GatePassNumber.Text = "";
                TokenNumber.Text = "";
                NoOfMaterial.Text = "";
                ExpiryDate.Text = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd HH:mm:ss");
                SelectedRow = null;
                ResetFields();
                document.Visibility = Visibility.Visible;
                gatePassNumber.Visibility = Visibility.Visible;
                tokenNumber.Visibility = Visibility.Visible;
                MaterialDesignThemes.Wpf.HintAssist.SetHint(DocumentNumber, "Document Number");
            }
            else if (index == 1)
            {
                RFIDNo.Text = "";
                VehicleNumber.Text = "";
                MaterialName.Text = "";
                SupplierName.Text = "";
                DefaultTareWeight.Text = "";
                LoadStatus.SelectedIndex = -1;
                TransactionType.SelectedIndex = -1;
                RFIDAllocationType.Text = "Temporary";
                Types.SelectedIndex = -1;
                DocumentNumber.Text = "";
                GatePassNumber.Text = "";
                TokenNumber.Text = "";
                NoOfMaterial.Text = "";
                ExpiryDate.Text = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd HH:mm:ss");
                SelectedRow = null;
                ResetFields();
                document.Visibility = Visibility.Collapsed;
                gatePassNumber.Visibility = Visibility.Collapsed;
                tokenNumber.Visibility = Visibility.Collapsed;
                noOfMaterial.Visibility = Visibility.Collapsed;
                materialField.Visibility = Visibility.Visible;
                supplierField.Visibility = Visibility.Visible;
                MaterialDesignThemes.Wpf.HintAssist.SetHint(DocumentNumber, "Document Number");
            }
        }

        public void NoOfMaterial_SelectionChanged(object sender, RoutedEventHandler e)
        {
            var text = sender;
        }

        public void TransactionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LoadStatus.Text == "" || LoadStatus.Text == null)
            {
                if (TransactionType.SelectedIndex == 0)
                {
                    LoadStatus.Text = "Loaded";
                }
                else if (TransactionType.SelectedIndex != 0)
                {
                    LoadStatus.Text = "";
                }
            }
            if (TransactionType.SelectedIndex == 3)
            {
                noOfMaterial.Visibility = Visibility.Visible;
                materialField.Visibility = Visibility.Collapsed;
                supplierField.Visibility = Visibility.Collapsed;
            }
            else
            {
                noOfMaterial.Visibility = Visibility.Collapsed;
                materialField.Visibility = Visibility.Visible;
                supplierField.Visibility = Visibility.Visible;
            }

            if (TransactionType.SelectedIndex == 3 && SAPType.SelectedIndex==1)
            {
                NoOfMaterial.IsEnabled = true;
            }

            if (SAPType.SelectedIndex==0)
            {
                if (TransactionType.SelectedIndex == 3 && SAPType.SelectedIndex==1)
                {
                    NoOfMaterial.IsEnabled = true;
                }
                else
                {
                    NoOfMaterial.IsEnabled = false;
                }
            }
        }

        public void DocumentNumberOnDropDownOpened(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Types.Text))
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select the Type");
            }
        }

        public void TypesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Types.SelectedIndex == 0)
            {
                LoadStatus.Text = "Loaded";
            }
            else if (Types.SelectedIndex == 1)
            {
                LoadStatus.Text = "Empty";
            }
        }

        public void GetRFIDAllocationRecentTransaction()
        {
            DataTable table = _dbContext.GetAllData($@"select top(25)
tr.[TicketNo], ge.[AllocationId], ge.[VehicleNumber], ge.[RFIDTag], ge.[TransMode], ge.[Status], ge.[MaterialCode], ge.[MaterialName], ge.[SupplierCode], ge.[SupplierName], ge.[ExpiryDate], ge.[TareWeight], ge.[IsLoaded], ge.[TransType], ge.[AllocationType], ge.[IsSapBased], ge.[DocNumber], ge.[GatePassNumber], ge.[TokenNumber], ge.[NoOfMaterial], ge.[CreatedOn]
from[RFID_Allocations] ge left join [Transaction] tr on ge.AllocationId = tr.RFIDAllocation order by ge.AllocationId desc");
            var JsonString = JsonConvert.SerializeObject(table);
            Result = JsonConvert.DeserializeObject<List<RFIDAllocationWithTrans>>(JsonString);
            SetDynamicTable();
        }

        public void SetDynamicTable()
        {
            try
            {
                MaterialGrid5.ItemsSource = Result;
                //TableContainer.Content = MaterialGrid5;
                MaterialGrid5.Items.Refresh();
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
                LastMessage = $"{name}";
                message(LastMessage);
            });
        }

        private void New_Button_Click(object sender, RoutedEventArgs e)
        {
            _isRowSelected = false;
            ResetRFIDAllocations();
            ResetFields();
            document.Visibility = Visibility.Collapsed;
            gatePassNumber.Visibility = Visibility.Collapsed;
            tokenNumber.Visibility = Visibility.Collapsed;
            noOfMaterial.Visibility = Visibility.Collapsed;
            materialField.Visibility = Visibility.Visible;
            supplierField.Visibility = Visibility.Visible;
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlTransaction transaction;
                con.Open();
                transaction = con.BeginTransaction();
                try
                {
                    GetValuesFromFields();          //Save custom field values to Allocation
                    GetRFIDAllocationValues();
                    var getGatePass = commonFunction.GetGatePassesByVehicleNumber(SelectedRFIDAllocation.VehicleNumber);
                    SelectedRFIDAllocation.GatePassId = getGatePass != null ? getGatePass.Id : 0;
                    if ((SelectedRFIDAllocation.IsSapBased == false || SelectedRFIDAllocation.IsSapBased == true)
                            && (!string.IsNullOrEmpty(SelectedRFIDAllocation.VehicleNumber)) && (!string.IsNullOrEmpty(SelectedRFIDAllocation.TransMode))
                            && (SelectedRFIDAllocation.IsLoaded == false || SelectedRFIDAllocation.IsLoaded == true)
                            && !string.IsNullOrEmpty(SelectedRFIDAllocation.RFIDTag) && !string.IsNullOrEmpty(SelectedRFIDAllocation.AllocationType))
                    {
                        if (DefaultTareWeight.Text != "0")
                        {
                            VehicleDetails = commonFunction.GetVehicleNumberDetail(VehicleNumber.Text);

                            if (VehicleDetails.Expiry < DateTime.Now)
                            {
                                throw new Exception("Tare weight expired!!");
                            }
                        }
                        if (DefaultTareWeight.Text == "0" && TransactionType.SelectedIndex == 0 && !SelectedRFIDAllocation.IsSapBased)
                            throw new Exception("Invalid tare found!!");
                        if (SelectedRFIDAllocation.TransMode == "FMT" && string.IsNullOrEmpty(NoOfMaterial.Text))
                            throw new Exception("Please add Materials!!");
                        if (SelectedRFIDAllocation.TransMode != "FMT" && (string.IsNullOrEmpty(SupplierName.Text) || string.IsNullOrEmpty(MaterialName.Text)))
                            throw new Exception("Please fill all fields!!");
                        int allocationId = CreateRFIDAllocation(transaction, con);
                        if (SelectedRFIDAllocation.TransMode == "FMT")
                        {
                            foreach (var item in CurrentStoreMaterials)
                            {
                                item.AllocationId = allocationId;
                            }
                            foreach (var item in CurrentStoreSuppliers)
                            {
                                item.AllocationId = allocationId;
                            }
                            commonFunction.SaveStoreManagement(new StoreManagement { Materials = CurrentStoreMaterials, Suppliers = CurrentStoreSuppliers }, transaction, con);
                        }
                        if (SelectedRFIDAllocation.AllocationType.ToLower().Contains("long-term"))
                        {
                            UpdateRFIDStatus(SelectedRFIDAllocation.VehicleNumber, SelectedRFIDAllocation.RFIDTag, transaction, con);
                        }
                        else
                        {
                            UpdateRFIDStatus("", SelectedRFIDAllocation.RFIDTag, transaction, con);
                        }
                        if (SelectedRFIDAllocation.IsSapBased)
                            commonFunction.UpdateGatePassStatus("In-Transit", getGatePass.Id, transaction, con);
                        commonFunction.UpdateVehicleStatus("In-Transit", SelectedRFIDAllocation.VehicleNumber, transaction, con);
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Gate Entry happened successfully");
                        ResetRFIDAllocations();
                        ResetFields();
                        document.Visibility = Visibility.Collapsed;
                        gatePassNumber.Visibility = Visibility.Collapsed;
                        tokenNumber.Visibility = Visibility.Collapsed;
                        noOfMaterial.Visibility = Visibility.Collapsed;
                        materialField.Visibility = Visibility.Visible;
                        supplierField.Visibility = Visibility.Visible;
                        transaction.Commit();
                        isPermanent = false;
                        RFIDNo.IsEnabled = true;
                        RFIDAllocationType.IsEnabled = true;
                        GetRFIDMasters();
                    }
                    else
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Please Fill the fields");
                    }
                }
                catch (Exception exp)
                {
                    transaction.Rollback();
                    throw exp;
                }
                finally
                {
                    con.Close();
                    GetRFIDAllocationRecentTransaction();
                }
            }
            catch (Exception ex)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, ex.Message);
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

        public void ResetFields()
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
                    ComboBox comboBox = (ComboBox)FindName(field.RegName);
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

        public int CreateRFIDAllocation(SqlTransaction transaction, SqlConnection con)
        {
            try
            {
                string insertQuery = $@"INSERT INTO [RFID_Allocations] (TransType,IsSapBased,DocNumber,TransMode,IsLoaded,VehicleNumber,MaterialCode,MaterialName,SupplierCode,SupplierName,TareWeight,AllocationType,ExpiryDate,RFIDTag,Status,CustomFieldValues,GatePassNumber,TokenNumber,NoOfMaterial,GatePassId,CreatedOn) 
                                                Values ('{SelectedRFIDAllocation.TransType}','{SelectedRFIDAllocation.IsSapBased}','{SelectedRFIDAllocation.DocNumber}','{SelectedRFIDAllocation.TransMode}','{SelectedRFIDAllocation.IsLoaded}','{SelectedRFIDAllocation.VehicleNumber}','{SelectedRFIDAllocation.MaterialCode}','{SelectedRFIDAllocation.MaterialName}','{SelectedRFIDAllocation.SupplierCode}','{SelectedRFIDAllocation.SupplierName}','{SelectedRFIDAllocation.TareWeight}','{SelectedRFIDAllocation.AllocationType}','{ExpiryDateForRFID}','{SelectedRFIDAllocation.RFIDTag}','{SelectedRFIDAllocation.Status}','{SelectedRFIDAllocation.CustomFieldValues}','{SelectedRFIDAllocation.GatePassNumber}','{SelectedRFIDAllocation.TokenNumber}','{SelectedRFIDAllocation.NoOfMaterial}','{SelectedRFIDAllocation.GatePassId}','{DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss")}');SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(insertQuery, con, transaction);
                cmd.CommandType = CommandType.Text;
                //cmd.ExecuteNonQuery();
                var newId = cmd.ExecuteScalar();
                return int.Parse(newId.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateRFIDStatus(string vehicleNo, string rfid, SqlTransaction transaction, SqlConnection con)
        {
            try
            {
                string updateQuery = $@"UPDATE [RFID_Tag_Master] SET VehicleNo=@VehicleNo,Status='Allocated' WHERE Tag=@Tag";
                SqlCommand cmd = new SqlCommand(updateQuery, con, transaction);
                cmd.Parameters.AddWithValue("@VehicleNo", vehicleNo);
                cmd.Parameters.AddWithValue("@Tag", rfid);
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdateRFIDStatus", ex);
                throw ex;
            }
        }

        public int SetTareWeightValue(string VehicleNumber)
        {
            if (VehicleNumber != null && VehicleNumber != "")
            {
                string Query = "SELECT * FROM Vehicle_Master WHERE VehicleNumber=@VehicleNumber";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@VehicleNumber", VehicleNumber);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var Result = JsonConvert.DeserializeObject<List<VehicleMaster>>(JSONString);
                if (Result.Count > 0)
                {
                    return Result.Find(x => x.VehicleNumber == VehicleNumber).VehicleTareWeight;
                }
                return 0;
            }
            else
            {
                return 0;
            }
        }

        public void GetRFIDAllocationValues()
        {
            try
            {
                SelectedRFIDAllocation.RFIDTag = RFIDNo.Text;
                SelectedRFIDAllocation.VehicleNumber = VehicleNumber.Text;
                DateTime getValueFromField = (DateTime)ExpiryDate.SelectedDate;
                ExpiryDateForRFID = getValueFromField.ToString("yyyy-MM-dd HH:mm:ss");
                SelectedRFIDAllocation.ExpiryDate = Convert.ToDateTime(ExpiryDateForRFID);
                SelectedRFIDAllocation.TareWeight = string.IsNullOrEmpty(DefaultTareWeight.Text) ? 0 : Convert.ToInt16(DefaultTareWeight.Text);

                if (LoadStatus.SelectedValue != null)
                    SelectedRFIDAllocation.IsLoaded = (bool)LoadStatus.SelectedValue;

                SelectedRFIDAllocation.TransMode = TransactionType.SelectedValue?.ToString();
                SelectedRFIDAllocation.AllocationType = RFIDAllocationType.SelectedValue?.ToString();

                if (SAPType.SelectedIndex == 0)
                    SelectedRFIDAllocation.IsSapBased = true;
                else
                    SelectedRFIDAllocation.IsSapBased = false;

                SelectedRFIDAllocation.TransType = Types.Text;
                SelectedRFIDAllocation.GatePassNumber = GatePassNumber.Text;
                SelectedRFIDAllocation.TokenNumber = TokenNumber.Text;
                int noOfMaterial = 0;
                int.TryParse(NoOfMaterial.Text, out noOfMaterial);
                SelectedRFIDAllocation.NoOfMaterial = noOfMaterial;

                if (SelectedRFIDAllocation.IsSapBased == true)
                    SelectedRFIDAllocation.DocNumber = DocumentNumber.Text;
                else
                    SelectedRFIDAllocation.DocNumber = null;

                SelectedRFIDAllocation.Status = "In-Transit";
                SelectedRFIDAllocation.CustomFieldValues = JsonConvert.SerializeObject(CustomFieldsBuilder);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetRFIDAllocationValues", ex);
            }
        }

        public void ResetRFIDAllocations()
        {
            RFIDNo.Text = "";
            VehicleNumber.Text = "";
            MaterialName.Text = "";
            SupplierName.Text = "";
            DefaultTareWeight.Text = "";
            //ExpiryDate.SelectedDate = (DateTime?)null;
            LoadStatus.SelectedIndex = -1;
            TransactionType.SelectedIndex = -1;
            RFIDAllocationType.Text = "Temporary";
            //SAPType.SelectedIndex = -1;
            Types.SelectedIndex = -1;
            //DocumentNumber.SelectedIndex = -1;
            DocumentNumber.Text = "";
            GatePassNumber.Text = "";
            TokenNumber.Text = "";
            NoOfMaterial.Text = "";
            ExpiryDate.Text = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd HH:mm:ss");
            SelectedRow = null;
            RFIDNo.IsEnabled = true;
            RFIDAllocationType.IsEnabled = true;
            SelectedRFIDAllocation = new RFIDAllocation();
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

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void InitializeLoadStatusList()
        {
            LoadStatusList = commonFunction.GetIsLoaded();
        }
        public void InitializeLoadStatusComboBox()
        {
            LoadStatus.ItemsSource = LoadStatusList;
            LoadStatus.SelectedValuePath = "ShortCode";
            LoadStatus.DisplayMemberPath = "Description";
            LoadStatus.Items.Refresh();
        }

        public void InitializeTransactionTypeMaster()
        {
            transactionTypeMasters = commonFunction.GetTransactionTypeMasters();
        }
        public void InitializeTransactionTypeComboBox()
        {
            TransactionType.ItemsSource = transactionTypeMasters;
            TransactionType.SelectedValuePath = "ShortCode";
            TransactionType.DisplayMemberPath = "Description";
            TransactionType.Items.Refresh();
        }

        public void InitializeRFIDAllocationTypeList()
        {
            RFIDAllocationTypeList.Clear();
            RFIDAllocationTypeList.Add("Temporary");
            RFIDAllocationTypeList.Add("Long-term Same Material");
            RFIDAllocationTypeList.Add("Long-term Different Material");
        }
        public void InitializeRFIDAllocationTypeComboBox()
        {
            RFIDAllocationType.ItemsSource = RFIDAllocationTypeList;
            RFIDAllocationType.Items.Refresh();
        }

        public void InitializeSAPTypeList()
        {
            SAPTypeList = commonFunction.GetIsSapBasedInRFIDAllocation();
        }
        public void InitializeSAPTypeComboBox()
        {
            SAPType.ItemsSource = SAPTypeList;
            SAPType.SelectedValuePath = "ShortCode";
            SAPType.DisplayMemberPath = "Description";
            SAPType.Items.Refresh();
        }

        public void InitializeTypeList()
        {
            TypeList.Clear();
            TypeList.Add("Inbound");
            TypeList.Add("Outbound");
        }

        public void InitializeTypeComboBox()
        {
            Types.ItemsSource = TypeList;
            Types.Items.Refresh();
        }

        private void SelectRFIDBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void SelectVehicleBtn_Click(object sender, RoutedEventArgs e)
        {
            await OpenvehicleDialog();
        }

        private async void SelectMaterialBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(VehicleNumber.Text))
            {
                await OpenMaterialDialog();
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the vehicle number");
            }
        }

        private async void SelectSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(VehicleNumber.Text))
            {
                await OpenSupplierDialog();
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the vehicle number");
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


        private void VehicleNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void VehicleNumber_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private async Task OpenvehicleDialog()
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new Addvehicledialog(0, SAPType.SelectedIndex == 0);

            if (SAPType.SelectedIndex == 0)
                view.AddVehicle.Visibility = Visibility.Collapsed;
            else
                view.AddVehicle.Visibility = Visibility.Visible;
            CurrentStoreMaterials = new List<StoreMaterialManagement>();
            CurrentStoreSuppliers = new List<StoreSupplierManagement>();
            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            if (result != null)
            {
                VehicleNumber.Text = result.ToString();
                RFIDMaster rfid = commonFunction.GetRFIDByVehicleNo(VehicleNumber.Text);
                if (rfid != null)
                {
                    var temp = RFIDMasters.FirstOrDefault(t => t.Tag == rfid.Tag);
                    if (temp == null)
                    {
                        RFIDMasters.Add(rfid);
                        RFIDNo.ItemsSource = RFIDMasters;
                        RFIDNo.Items.Refresh();
                    }
                    isPermanent = true;
                    RFIDNo.SelectedItem = rfid;
                    RFIDNo.IsEnabled = false;
                    RFIDAllocationType.SelectedIndex = 2;
                    RFIDAllocationType.IsEnabled = false;
                }
                DefaultTareWeight.Text = SetTareWeightValue(VehicleNumber.Text).ToString();
                if (SAPType.SelectedIndex == 0)
                {
                    getGatePassesData = commonFunction.GetGatePassesByVehicleNumber(VehicleNumber.Text);
                    if (getGatePassesData != null)
                    {
                        if (getGatePassesData.InOut.ToUpper() == "OUT")
                        {
                            Types.Text = "Outbound";
                            LoadStatus.Text = "Empty";
                            MaterialDesignThemes.Wpf.HintAssist.SetHint(DocumentNumber, "Document Number (SO)");
                            DocumentNumber.Text = getGatePassesData.SoNumber;
                        }
                        else if (getGatePassesData.InOut.ToUpper() == "IN")
                        {
                            Types.Text = "Inbound";
                            LoadStatus.Text = "Loaded";
                            MaterialDesignThemes.Wpf.HintAssist.SetHint(DocumentNumber, "Document Number (PO)");
                            DocumentNumber.Text = getGatePassesData.PoNumber;
                        }
                        var gatePassItems = GetGatePasseItemByGatePassId(getGatePassesData.Id);
                        TransactionType.Text = "First and Second Transaction";
                        SupplierName.Text = $@"{"SAP"}/{"-"}";
                        SelectedRFIDAllocation.SupplierCode = "SAP";
                        SelectedRFIDAllocation.SupplierName = "-";
                        GatePassNumber.Text = getGatePassesData.GatePassNumber;
                        TokenNumber.Text = getGatePassesData.TokenNumber;
                        if (gatePassItems.Count == 1)
                        {
                            MaterialName.Text = gatePassItems[0].ItemNumber;
                            string query = $"select * from [Material_Master] where MaterialCode='{gatePassItems[0].ItemNumber}';";
                            var dt = _dbContext.GetAllData(query);
                            string jsonString = JsonConvert.SerializeObject(dt);
                            var sapMaterial = JsonConvert.DeserializeObject<List<MaterialMaster>>(jsonString);
                            if (sapMaterial != null && sapMaterial.Count > 0)
                            {
                                SelectedRFIDAllocation.MaterialCode = sapMaterial[0].MaterialCode;
                                SelectedRFIDAllocation.MaterialName = sapMaterial[0].MaterialName;
                                MaterialName.Text = $@"{sapMaterial[0].MaterialCode}/{sapMaterial[0].MaterialName}";
                            }
                            materialField.Visibility = Visibility.Visible;
                            supplierField.Visibility = Visibility.Visible;
                            noOfMaterial.Visibility = Visibility.Collapsed;
                        }
                        else if (gatePassItems.Count > 1)
                        {
                            TransactionType.Text = "Multi Transaction";
                            materialField.Visibility = Visibility.Collapsed;
                            supplierField.Visibility = Visibility.Collapsed;
                            noOfMaterial.Visibility = Visibility.Visible;
                            NoOfMaterial.Text = gatePassItems.Count.ToString();
                            string query = "select * from [Material_Master] where MaterialCode in ({0});";
                            string formatted = string.Format(query, string.Join(",", gatePassItems.Select(t => $"'{t.ItemNumber}'").ToList()));
                            var dt = _dbContext.GetAllData(formatted);
                            string jsonString = JsonConvert.SerializeObject(dt);
                            var sapMaterials = JsonConvert.DeserializeObject<List<MaterialMaster>>(jsonString);
                            CurrentStoreMaterials = new List<StoreMaterialManagement>();
                            int order = 1;
                            var gatePassItemNos = GetGatePasseItemByGatePassId(getGatePassesData.Id, getGatePassesData.InOut.ToUpper() == "OUT" ? "SO" : "PO");
                            if (gatePassItemNos.Count == sapMaterials.Count)
                            {
                                for (int i = 0; i < sapMaterials.Count; i++)
                                {
                                    var storeMaterial = new StoreMaterialManagement { MaterialCode = sapMaterials[i].MaterialCode, MaterialName = sapMaterials[i].MaterialName, Order = order++, ItemNo = gatePassItemNos[i].ItemNumber };
                                    CurrentStoreMaterials.Add(storeMaterial);
                                }
                            }
                            else
                            {
                                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Invalid SAP data, Materials and Item numbers not matched!");
                            }
                        }
                        else
                        {
                            //SAPType.Text = "";
                            Types.Text = "";
                            LoadStatus.Text = "";
                            DocumentNumber.Text = "";
                            MaterialName.Text = "";
                            TransactionType.Text = "";
                            SupplierName.Text = "";
                            NoOfMaterial.Text = "";
                            GatePassNumber.Text = "";
                            TokenNumber.Text = "";
                            document.Visibility = Visibility.Collapsed;
                            gatePassNumber.Visibility = Visibility.Collapsed;
                            tokenNumber.Visibility = Visibility.Collapsed;
                            noOfMaterial.Visibility = Visibility.Collapsed;
                            materialField.Visibility = Visibility.Visible;
                            supplierField.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            var result = eventArgs?.Parameter;
            if (result != null)
            {
                //VehicleNumber.Text = result.ToString();
            }
            else
            {
                //vehicleNO = "";
            }

            // //Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }

        private async Task OpenMaterialDialog()
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new Addmaterial();

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler1);

            if (result != null)
            {
                material = result as MaterialMaster;
                MaterialName.Text = $@"{material.MaterialCode}/{material.MaterialName}";
                SelectedRFIDAllocation.MaterialName = material.MaterialName;
                SelectedRFIDAllocation.MaterialCode = material.MaterialCode;
                if (material.MaterialID > 0)
                {
                    await OpenSupplierDialog();
                }
            }

            //check the result...
            // //Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        private void ClosingEventHandler1(object sender, DialogClosingEventArgs eventArgs)
        {
            // //Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }

        private async Task OpenSupplierDialog()
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new Addsupplier();

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler2);

            if (result != null)
            {
                supplier = result as SupplierMaster;
                SupplierName.Text = $@"{supplier.SupplierCode}/{supplier.SupplierName}";
                SelectedRFIDAllocation.SupplierName = supplier.SupplierName;
                SelectedRFIDAllocation.SupplierCode = supplier.SupplierCode;
            }

            //check the result...
            // //Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        private void ClosingEventHandler2(object sender, DialogClosingEventArgs eventArgs)
        {

            // //Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }

        #region Disable/Mandatory Status
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

        #region Custom Fields
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

        private Border CreateFormulaField(CustomFieldBuilder template)
        {
            Border container = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1)
            };
            StackPanel inputContainer = new StackPanel
            {
                Height = 70,
                Orientation = Orientation.Horizontal
            };
            Image customFieldIcon = new Image
            {
                Source = new BitmapImage(new Uri(@"/Assets/Icons/formula.png", UriKind.Relative)),
                Width = 50,
                Margin = new Thickness(10)
            };
            Style style = this.FindResource("MaterialDesignFloatingHintTextBox") as Style;
            TextBox customFieldTextBox = new TextBox
            {
                Width = 290,
                Margin = new Thickness(3, -15, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
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

        private Border CreateDataDependencyField(CustomFieldBuilder template)
        {
            Border container = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1)
            };
            StackPanel inputContainer = new StackPanel
            {
                Height = 70,
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
                    Width = 290,
                    Margin = new Thickness(3, -15, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
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
                    customFieldBuilder.IsMandatory = customFieldBuilder.MandatoryStatus != null ? customFieldBuilder.MandatoryStatus.SGT : false;
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
                    DisableCustomeField(template, "SGT");
                }
                FormulaFields = CustomFieldsBuilder.Where(t => t.ControlType == "Formula").ToList();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("BuildCustomFields:" + ex.Message);
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

        private Border CreateInputField(CustomFieldBuilder template)
        {
            Border container = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1)
            };
            StackPanel inputContainer = new StackPanel
            {
                Height = 70,
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
                Width = 290,
                Margin = new Thickness(3, -15, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
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
                Height = 70,
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
                Width = 290,
                Margin = new Thickness(10, -15, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
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
                Height = 70,
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
                Width = 290,
                Margin = new Thickness(3, -15, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
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

        private void CustomFieldComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (handleChangeEvent)
            {
                string tag = ((ComboBox)sender).Tag.ToString();
                string value = (((ComboBox)sender).SelectedItem as ComboBoxItem).Content.ToString();
                //var depField = fieldDependencies.FirstOrDefault(t => t.LinkedName == tag);
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

        private void NoOfMaterialTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            NoOfMaterial.Text = ((TextBox)sender).Text;
        }

        private void CustomFieldTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (handleChangeEvent)
            {
                string tag = ((TextBox)sender).Tag.ToString();
                string value = ((TextBox)sender).Text;
                string val = ((TextBox)sender).Text;
                //var depField = fieldDependencies.FirstOrDefault(t => t.LinkedName == tag);
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
                        //else if (tag == "VehicleNumber")
                        //{
                        //    DefaultTareWeight.Text = SetTareWeightValue(value);
                        //}
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
        #endregion

        private async Task OpenStoreDialog(RFIDAllocation allocation)
        {
            var view = new Object();
            if (allocation.IsSapBased)
                view = new ManageMaterialDialog(allocation, "GateEntry", new StoreManagement { Materials = CurrentStoreMaterials });
            else
                view = new ManageMaterialDialog(allocation, "GateEntry", new StoreManagement { Materials = CurrentStoreMaterials, Suppliers = CurrentStoreSuppliers });
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            if (result != null)
            {
                CurrentStoreMaterials = ((StoreManagement)result).Materials;
                CurrentStoreSuppliers = ((StoreManagement)result).Suppliers;
                NoOfMaterial.Text = CurrentStoreMaterials.Count.ToString();
            }
        }

        private async void NoOfMaterialBtn_Click(object sender, RoutedEventArgs e)
        {
            GetRFIDAllocationValues();
            if (SelectedRFIDAllocation.IsSapBased == false)
            {
                if (SelectedRFIDAllocation.NoOfMaterial != 0)
                {
                    await OpenStoreDialog(SelectedRFIDAllocation);
                }
                else if (SelectedRFIDAllocation.NoOfMaterial == 0)
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Please fill the No Of Material");
                }
            }
            else
            {
                await OpenStoreDialog(SelectedRFIDAllocation);
            }
        }
    }
}
