using IWT.AWS.ViewModel;
using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

namespace IWT.TransactionPages
{
    public partial class Addvehicledialog : UserControl
    {
        private ToastViewModel toastViewModel;
        string LastMessage;
        private AdminDBCall _dbContext;
        public MasterDBCall masterDBCall = new MasterDBCall();
        private List<VehicleMaster> AllVehicles = new List<VehicleMaster>();
        private List<GatePasses> vehiclesInGatePasses = new List<GatePasses>();
        private bool SapBased;
        int selectedIndex = 0;
        public Addvehicledialog(int _selectedIndex = 0, bool isSapBased = false)
        {
            InitializeComponent();
            _dbContext = new AdminDBCall();
            selectedIndex = _selectedIndex;
            MainTabControl.SelectedIndex = selectedIndex;
            SapBased = isSapBased;
            MainWindow.isSAPBased = SapBased;
            MainWindow.multiTrans = "";
            if (SapBased)
                GetAllVehiclesFromGatePasses();
            else
                GetAllVehicleFromVehicleMaster();
            toastViewModel = new ToastViewModel();
            Loaded += Addvehicledialog_Loaded;
        }

        private void Addvehicledialog_Loaded(object sender, RoutedEventArgs e)
        {
            //autocomplete
            TransactionViewModel dataContext = this.DataContext as TransactionViewModel;
            dataContext.PropertyChanged += DataContext_PropertyChanged;
        }

        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                TransactionViewModel dataContext = this.DataContext as TransactionViewModel;
                if (e.PropertyName == "Vehicle" && dataContext.Vehicle != null)
                {
                    VehicleNumber.Text = dataContext.Vehicle.VehicleNumber;
                }
                if(e.PropertyName =="GatePasses" &&  dataContext.GatePasses != null)
                {
                    VehicleNumber.Text = dataContext.GatePasses.VehicleNumber;
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("AddVehicleDialog/Addvehicledialog_Loaded/DataContext_PropertyChanged/Exception :- " + ex.Message);
            }
        }

        private void GetAllVehiclesFromGatePasses()
        {
            DataTable dt = _dbContext.GetAllData("SELECT * FROM [GatePasses] WHERE [Status] in ('','OGI')");
            string JSONString = JsonConvert.SerializeObject(dt);
            vehiclesInGatePasses = JsonConvert.DeserializeObject<List<GatePasses>>(JSONString);
            //VehicleNumber.ItemsSource = vehiclesInGatePasses.OrderByDescending(t=>t.Id).ToList();
        }
        private void GetAllVehicleFromVehicleMaster()
        {
            try
            {
                string Query = "SELECT * FROM [Vehicle_Master] WHERE IsDeleted='0' and (Status is null or Status = '')";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                AllVehicles = JsonConvert.DeserializeObject<List<VehicleMaster>>(JSONString);
                //VehicleNumber.ItemsSource = AllVehicles.OrderByDescending(t=>t.VehicleID).ToList();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetAllVehicleFromVehicleMaster:- " + ex.Message, ex);
            }
        }
        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            string vehicleNumber = NewVehicleNumber.Text;
            VehicleMaster vehicleMaster = new VehicleMaster();
            vehicleMaster.VehicleNumber = vehicleNumber;
            if (vehicleNumber != "")
            {
                var previousValue = AllVehicles.FirstOrDefault(t => t.VehicleNumber == vehicleMaster.VehicleNumber);
                if (previousValue == null)
                {
                    bool res = _dbContext.ExecuteQuery($"INSERT INTO [Vehicle_Master] (VehicleNumber,Expiry,VehicleTareWeight,TaredTime,IsDeleted,ModifiedOn,CreatedOn) VALUES ('{vehicleNumber}','{DateTime.Now.AddDays(2).ToString("yyyy-MM-dd HH:mm:ss")}','0','{DateTime.Now.ToString("hh:mm:ss tt")}','False','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}')");
                    if (res)
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "VehicleNumber Created Successsfully !!");
                        DialogHost.CloseDialogCommand.Execute(vehicleMaster.VehicleNumber, null);
                    }
                    else
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !!");
                    }
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Record with same VehicleNumber already exists !!");
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please enter vehicle number!!");
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

        private void Name6_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            var vehileNumber = VehicleNumber.Text;
            if (vehileNumber != null)
            {
                DialogHost.CloseDialogCommand.Execute(vehileNumber, null);
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a vehicle number !!");
            }
        }
        private void VehicleNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex(@"[^a-zA-Z0-9\s]");
            e.Handled = regex.IsMatch(e.Text);
            //VehicleNumber.IsDropDownOpen = true;
            //VehicleNumber.ItemsSource = vehiclesInGatePasses.Where(p => p.VehicleNumber.Contains(e.Text)).ToList();
        }

        private void VehicleNumber_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
