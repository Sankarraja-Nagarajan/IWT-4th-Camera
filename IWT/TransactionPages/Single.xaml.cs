using IWT.DBCall;
using IWT.Models;
using IWT.ViewModel;
using IWT.Views;
using MaterialDesignThemes.Wpf;
using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
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
    /// Interaction logic for Single.xaml
    /// </summary>
    public partial class Single : Page
    {
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        //private string name = "";
        private string vehicle_number = "";
        public string Vehicle = "";

        public String Name2;
        public int tareweight;
        //private Viewvehicle _context;
        private string vehicleNO = "";
        private string Material_name = "";
        private string Supplier_name = "";
        private string _Hello = "";

        public int StableWeightArraySize = int.Parse(ConfigurationManager.AppSettings["StableWeightArraySize"]);
        public int StableWeightArraySelectable = int.Parse(ConfigurationManager.AppSettings["StableWeightArraySelectable"]);
        public List<string> GainedWeightList = new List<string>();

        Timer timer;
        DispatcherTimer timer1 = new DispatcherTimer();
        string currentWeightment = "";
        public Single()
        {
            InitializeComponent();
            DataContext = new Viewvehicle();
            toastViewModel = new ToastViewModel();
            // new Dashboard().onWeighmentReceived += Single_onWeighmentReceived;
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
                MaterialSaveButton.IsEnabled = true;
            }
            else
            {
                MaterialSaveButton.IsEnabled = false;
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
                WriteLog.WriteToFile("CheckIsStable :-" + ex.Message);
                return false;
            }
            return false;
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            ExecuteaddfirstvehicleDialog();
        }
        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            if (vehicleNO != "")
            {
                ExecuteaddmaterialDialog();

            }

        }
        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            ExecuteaddsupplierDialog();
        }



        private async void ExecuteaddfirstvehicleDialog()
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new FirstVehicle();

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);

            //check the result...
            // Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        private async void ExecuteaddmaterialDialog()
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new Addmaterial();

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler1);

            //check the result...
            // Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        private async void ExecuteaddsupplierDialog()
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new Addsupplier();

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler2);

            //check the result...
            // Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            var result = eventArgs?.Parameter;
            if (result != null)
            {
                vehicleNO = result.ToString();
                vehicle.Text = vehicleNO;
                AdminDBCall db = new AdminDBCall();
                DataTable dt1 = db.GetAllData("SELECT * FROM Vehicle_Master Where VehicleNumber=" + "'" + vehicleNO + "'");
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    foreach (DataRow row in dt1.Rows)
                    {
                        tareweight = Convert.ToInt32(row["VehicleTareWeight"]);

                        empty_weight.Text = tareweight.ToString();

                    }
                }

            }
            else
            {
                vehicleNO = "";
            }

            // Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }
        private void ClosingEventHandler1(object sender, DialogClosingEventArgs eventArgs)
        {
            var result = eventArgs?.Parameter;
            if (result != null)
            {
                Material_name = result.ToString();
                code2.Text = Material_name;
            }
            else
            {
                Material_name = "";
                code2.Text = Material_name;
            }

            // Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }
        private void ClosingEventHandler2(object sender, DialogClosingEventArgs eventArgs)
        {
            var result = eventArgs?.Parameter;
            if (result != null)
            {
                Supplier_name = result.ToString();
                supplier1.Text = Supplier_name;
            }
            else
            {
                Supplier_name = "";
                supplier1.Text = Supplier_name;
            }

            // Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }
        public string Hello
        {
            get { return _Hello; }
            set
            {
                _Hello = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
        private void GetData()
        {
            AdminDBCall db = new AdminDBCall();
            SqlConnection cn = new SqlConnection(db.GetDecryptedConnectionStringDB());
            //var tkno = _context.TicketNoSELECT IDENT_CURRENT('TableName')
            SqlCommand cmd = new SqlCommand("select * from[dbo].[Transaction] where TicketNo = (select max(TicketNo) from[dbo].[Transaction])", cn);
            //"SELECT MAX(TicketNo) FROM [dbo].[Transaction] where MaterialName = '" + obj1.materialname + "'", cn);
            Console.WriteLine(cmd);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            ReportDataSource rds = new ReportDataSource("DataSet1", dt);
            //ReportViewerDemo.LocalReport.ReportPath = @"D:\Exalca\Projects\IWT\IWT\TransactionPages\SingleTransaction.rdlc";
            ReportViewerDemo.LocalReport.DataSources.Clear();
            ReportViewerDemo.LocalReport.DataSources.Add(rds);
            ReportViewerDemo.LocalReport.ReportEmbeddedResource = "IWT.TransactionPages.SingleTransaction.rdlc";
            ReportViewerDemo.RefreshReport();
        }
        public void openDialog()
        {
            if (!popup.IsOpen)
            {
                //var Materialname =obj1.materialname;

                popup.IsOpen = true;
                GetData();
            }// Open it if it's not open
            else popup.IsOpen = false;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //string vehicli_mannual = obj.LogedInPerson;
            //if (obj.LogedInPerson == null)
            //{
            //    vehicle_number = vehicle.Text.ToString();
            //}
            //else
            //{
            //    vehicle_number = obj.LogedInPerson;
            //}
            vehicle_number = vehicle.Text.ToString();
            string material_name = code2.Text;
            string material_code = code2.Text;
            string supplier_name = supplier1.Text;
            string supplier_code = supplier1.Text;
            transaction_details data = new transaction_details();
            AdminDBCall db = new AdminDBCall();
            data.VehicleNo = vehicle_number;
            data.MaterialName = material_name;
            data.SupplierName = supplier_name;
            data.NoOfMaterial = 1;
            data.Date = DateTime.Now;
            data.EmptyWeight = tareweight;
            data.LoadWeight = string.IsNullOrEmpty(currentWeightment) ? 0 : Convert.ToInt32(currentWeightment);
            data.EmptyWeightDate = DateTime.Now;
            data.LoadWeightDate = DateTime.Now;
            data.Netweight = data.LoadWeight - data.EmptyWeight;
            data.Pending = true;
            data.Closed = true;
            data.Multiweight = true;
            data.MultiweightTransPending = true;
            data.TransactionType = "Single";
            db.InsertsingletransactionData(data);
            if (!string.IsNullOrEmpty(vehicle_number) && !string.IsNullOrEmpty(material_name) && !string.IsNullOrEmpty(supplier_name))
            {
                openDialog();
                ShowMessage(toastViewModel.ShowSuccess, "Saved Successfully");
                //obj.LogedInPerson = null;
                //obj1.materialname = null;
                //obj1.materialcode = null;
                //obj3.suppliercode = null;
                //obj3.suppliername = null;
                vehicle.Text = "";
                code2.Text = "";
                supplier1.Text = "";
                material_name = "";
                material_code = "";
                supplier_name = "";
                supplier_code = "";
                vehicle_number = "";
                vehicleNO = "";
                empty_weight.Text = "";
                tareweight = 0;
                currentWeightment = "";
                MaterialSaveButton.IsEnabled = false;
                SaveButton.IsEnabled = false;
            }
            else
            {
                ShowMessage(toastViewModel.ShowWarning, "Please fill the required Fields");

            }
            //name = code1.Text;
        }

        public void ShowMessage(Action<string> message, string name)
        {
            this.Dispatcher.Invoke(() =>
            {
                LastMessage = name;
                message(LastMessage);
            });
        }

        public void test()
        {
            //string selected_dept = obj.LogedInPerson;
            //Vehicle = selected_dept;
            _Hello = Vehicle;
            if (Vehicle != null)
            {
                //code1.Text = selected_dept;
            }
            //string material_name = obj1.materialname;
            //string material_code = obj1.materialcode;
        }
       
        private void MaterialSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(vehicle.Text) && !string.IsNullOrEmpty(Material_name) && !string.IsNullOrEmpty(Supplier_name))
            {
                currentWeightment = GetWighment();
                SaveButton.IsEnabled = true;
            }
            else
            {
                ShowMessage(toastViewModel.ShowWarning, "Please fill the required Fields");
                SaveButton.IsEnabled = false;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
        }
    }


}
