using IWT.AWS.ViewModel;
using IWT.DBCall;
using IWT.Models;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using static IWT.ViewModel.Viewvehicle;

namespace IWT.TransactionPages
{
    /// <summary>
    /// Interaction logic for FirstVehicle.xaml
    /// </summary>
    public partial class FirstVehicle : UserControl
    {
        public FirstVehicle()
        {
            InitializeComponent();
            GetAllVehicleMasters();
            MainWindow.isSAPBased = false;
            MainWindow.multiTrans = "";
            Loaded += FirstVehicle_Loaded;
        }

        private void FirstVehicle_Loaded(object sender, RoutedEventArgs e)
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
            }
            catch(Exception ex)
            {
                WriteLog.WriteToFile("FirstVehicle/FirstVehicle_Loaded/DataContext_PropertyChanged/Exception :- "+ ex.Message);
            }
        }

        private void GetAllVehicleMasters()
        {
            List<VehicleMaster> vehicleMasters = new List<VehicleMaster>();
            AdminDBCall db = new AdminDBCall();
            DataTable dt = db.GetAllData("SELECT * FROM [dbo].[Vehicle_Master] WHERE IsDeleted='FALSE'");
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    var item = new VehicleMaster();
                    item.VehicleID = (int)dr["VehicleID"];
                    item.VehicleNumber = (string)dr["VehicleNumber"];
                    item.VehicleTareWeight = (int)dr["VehicleTareWeight"];
                    vehicleMasters.Add(item);
                }
                //VehicleName.ItemsSource = vehicleMasters;
            }
        }
        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {
            var selected = VehicleNumber.SelectedItem;
            DialogHost.CloseDialogCommand.Execute(selected, null);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
