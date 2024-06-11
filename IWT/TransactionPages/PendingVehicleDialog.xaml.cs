using IWT.AWS.ViewModel;
using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace IWT.TransactionPages
{
    /// <summary>
    /// Interaction logic for PendingVehicleDialog.xaml
    /// </summary>
    public partial class PendingVehicleDialog : UserControl
    {
        string TransactionType;
        List<Transaction> transactions = new List<Transaction>();
        public PendingVehicleDialog(string _TransactionType = "Second")
        {
            TransactionType = _TransactionType;
            MainWindow.multiTrans = TransactionType;
            MainWindow.isSAPBased = false;
            InitializeComponent();
            GetPendingTickets();
            Loaded += PendingVehicleDialog_Loaded;
        }

        private void PendingVehicleDialog_Loaded(object sender, RoutedEventArgs e)
        {
            WriteLog.WriteToFile("PendingVehicleDialog_Loaded invoked");
            //autocomplete
            TransactionViewModel dataContext = this.DataContext as TransactionViewModel;
            dataContext.PropertyChanged += DataContext_PropertyChanged;
        }
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                WriteLog.WriteToFile("DataContext_PropertyChanged invoked");
                TransactionViewModel dataContext = this.DataContext as TransactionViewModel;
                if (e.PropertyName == "Transaction" && dataContext.Transaction != null)
                {
                    VehicleNo.Text = dataContext.Transaction.VehicleNo;
                }
                //VehicleNumber.Text = VehicleNumber.SelectedItem.ToString();

            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("PendingVehicleDialog/PendingVehicleDialog_Loaded/DataContext_PropertyChanged/Exception :- " + ex.Message);
            }
        }

        public void GetPendingTickets()
        {
            AdminDBCall db = new AdminDBCall();

            //List<PendingVehicle> authors = new List<PendingVehicle>();
            DataTable dt1 = null;
            if (TransactionType == "Second")
            {
                dt1 = db.GetAllData("SELECT * FROM [dbo].[Transaction] WHERE Pending=1 AND MultiWeight=0");
            }
            else if (TransactionType == "Second Multi")
            {
                dt1 = db.GetAllData("SELECT * FROM [dbo].[Transaction] WHERE Pending=1 AND MultiWeight=1");
            }

            if (dt1 != null && dt1.Rows.Count > 0)
            {
                //foreach (DataRow row in dt1.Rows)
                //{
                //    PendingVehicle list = new PendingVehicle();
                //    list.VehicleNumber = (row["VehicleNo"].ToString());
                //    var ticketno = (row["TicketNo"].ToString());
                //    list.TicketNo = int.Parse(ticketno);
                //    //combobox.Items.Add(list.VehicleNumber);
                //    //combobox1.Items.Add(list.TicketNo);
                //    authors.Add(new PendingVehicle()
                //    {
                //        VehicleNumber = list.VehicleNumber,
                //        TicketNo = list.TicketNo
                //    });
                //}

                string JSONString = JsonConvert.SerializeObject(dt1);
                transactions = JsonConvert.DeserializeObject<List<Transaction>>(JSONString);
                transactions = transactions.OrderByDescending(x => x.TicketNo).ToList();
                //combobox.ItemsSource = transactions;
                //combobox.Items.Refresh();
                combobox1.ItemsSource = transactions;
                combobox1.Items.Refresh();

            }
        }
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {

            AdminDBCall db = new AdminDBCall();
            if (combobox1.SelectedItem != null)
            {
                var Transac = combobox1.SelectedItem as Transaction;
                //string selectedText = combobox1.SelectedValue.ToString();
                if (Transac != null && Transac.TicketNo != 0)
                {
                    DataTable dt1 = db.GetAllData("SELECT * FROM [dbo].[Transaction] Where TicketNo=" + "'" + Transac.TicketNo + "'");
                    //if (dt1 != null && dt1.Rows.Count > 0)
                    //{
                    //    foreach (DataRow row in dt1.Rows)
                    //    {
                    //        string JSONString = JsonConvert.SerializeObject(dt1);
                    //        var result = JsonConvert.DeserializeObject<List<PendingTicketsTransaction>>(JSONString);
                    //        if (result.Count > 0)
                    //        {
                    //            var res = result[0];
                    //            DialogHost.CloseDialogCommand.Execute(res, null);
                    //        }

                    //    }
                    //}
                    DialogHost.CloseDialogCommand.Execute(dt1, null);
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a ticket / vehicle number");
            }

        }
        private void SubmitButton_Click1(object sender, RoutedEventArgs e)
        {

            AdminDBCall db = new AdminDBCall();
            if (VehicleNo.SelectedItem != null)
            {
                var Transac = VehicleNo.SelectedItem as Transaction;
                //string selectedText = combobox.SelectedValue.ToString();
                if (Transac != null && Transac.TicketNo != 0)
                {
                    DataTable dt1 = db.GetAllData("SELECT * FROM [dbo].[Transaction] Where TicketNo=" + "'" + Transac.TicketNo + "'");
                    //if (dt1 != null && dt1.Rows.Count > 0)
                    //{
                    //    foreach (DataRow row in dt1.Rows)
                    //    {
                    //        string JSONString = JsonConvert.SerializeObject(dt1);
                    //        var result = JsonConvert.DeserializeObject<List<PendingTicketsTransaction>>(JSONString);
                    //        if (result.Count > 0)
                    //        {
                    //            var res = result[0];
                    //            DialogHost.CloseDialogCommand.Execute(res, null);
                    //        }

                    //    }
                    //}
                    DialogHost.CloseDialogCommand.Execute(dt1, null);
                }
            }

        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
