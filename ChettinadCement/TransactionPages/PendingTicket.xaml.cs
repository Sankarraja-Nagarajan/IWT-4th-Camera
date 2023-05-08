using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using IWT.Views;
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

namespace IWT.TransactionPages
{
    /// <summary>
    /// Interaction logic for PendingTicket.xaml
    /// </summary>
    public partial class PendingTicket : UserControl
    {
        string TransactionType;
        string LastMessage;
        List<Transaction> transactions = new List<Transaction>();

        private readonly ToastViewModel toastViewModel;

        public PendingTicket(string transactionType = "Second")
        {
            InitializeComponent();
            toastViewModel = new ToastViewModel();
            TransactionType = transactionType;
            GetPendingTickets();
        }

        public void GetPendingTickets()
        {
            AdminDBCall db = new AdminDBCall();
            PendingVehicle list = new PendingVehicle();
            List<PendingVehicle> authors = new List<PendingVehicle>();
            DataTable dt1 = null;
            if (TransactionType == "Second")
            {
                dt1 = db.GetAllData("SELECT * FROM [dbo].[Transaction] WHERE Pending=1 AND MultiWeight=0");
            }
            else if (TransactionType == "SecondMulti")
            {
                dt1 = db.GetAllData("SELECT * FROM [dbo].[Transaction] WHERE Pending=1 AND MultiWeight=1");
            }

            if (dt1 != null && dt1.Rows.Count > 0)
            {
                //foreach (DataRow row in dt1.Rows)
                //{
                //    list.VehicleNumber = (row["VehicleNo"].ToString());
                //    var ticketno = (row["TicketNo"].ToString());
                //    list.TicketNo = int.Parse(ticketno);
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

                combobox1.ItemsSource = transactions;
                combobox1.Items.Refresh();

                combobox2.ItemsSource = transactions;
                combobox2.Items.Refresh();
            }
        }


        private async void CloseTicketButton_Click(object sender, RoutedEventArgs e)
        {
            if (combobox1.SelectedItem != null)
            {
                string selectedTicket = combobox1.SelectedValue?.ToString();
                //var result = CloseTheSelectedTicket(selectedTicket);
                //if (result)
                //{
                //    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select the ticket");
                //}
                var res = await OpenConfirmationDialog(selectedTicket);
                if (res)
                {
                    var res1 = CloseTheSelectedTicket(selectedTicket);
                    if (res1)
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Ticket closed successfully");
                        DialogHost.CloseDialogCommand.Execute(null, null);
                        //SetBtnIsEnable(false);
                        //ClearTransaction();
                    }
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select the ticket");
            }
        }

        private async void CloseTicketButton2_Click(object sender, RoutedEventArgs e)
        {
            if (combobox2.SelectedItem != null)
            {
                string selectedTicket = combobox2.SelectedValue?.ToString();
                string vehicle = (combobox2.SelectedItem as Transaction)?.VehicleNo;
                //var result = CloseTheSelectedTicket(selectedTicket);
                //if (result)
                //{
                //    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select the ticket");
                //}
                var res = await OpenConfirmationDialog2(selectedTicket, vehicle);
                if (res)
                {
                    var res1 = CloseTheSelectedTicket(selectedTicket);
                    if (res1)
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Ticket closed successfully");
                        DialogHost.CloseDialogCommand.Execute(null, null);
                        //SetBtnIsEnable(false);
                        //ClearTransaction();
                    }
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select the ticket");
            }
        }

        private async void CloseTicketBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async Task<bool> OpenConfirmationDialog(string selectedTicket)
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new ConfirmationDialog($"Close the ticket - {selectedTicket}");

            //show the dialog
            var result = await DialogHost.Show(view, "TicketDialogHost", ClosingEventHandler);

            return (bool)result;
            //check the result...
            ////Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        private async Task<bool> OpenConfirmationDialog2(string selectedTicket, string vehicle)
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new ConfirmationDialog($"close the ticket - {selectedTicket} ({vehicle})");

            //show the dialog
            var result = await DialogHost.Show(view, "TicketDialogHost", ClosingEventHandler);

            return (bool)result;
            //check the result...
            ////Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            // //Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }

        public bool CloseTheSelectedTicket(string selectedTicket)
        {
            try
            {
                string Query = "UPDATE [Transaction] SET MultiWeightTransPending=@MultiWeightTransPending,Pending=@Pending,Closed=@Closed WHERE TicketNo=@TicketNo";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.Add("@TicketNo", SqlDbType.VarChar).Value = selectedTicket;
                cmd.Parameters.Add("@MultiWeightTransPending", SqlDbType.Bit).Value = false;
                cmd.Parameters.Add("@Pending", SqlDbType.Bit).Value = false;
                cmd.Parameters.Add("@Closed", SqlDbType.Bit).Value = true;
                new MasterDBCall().InsertData(cmd, CommandType.Text);
                return true;

            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetTransactionDetailsByTicket:" + ex.Message);
                return false;
            }
        }

        //public bool CloseTheSelectedTicket(string selectedTicket)
        //{
        //    try
        //    {
        //        string Query = "UPDATE [Transaction] SET MultiWeightTransPending=@MultiWeightTransPending,Pending=@Pending,Closed=@Closed WHERE TicketNo=@TicketNo";
        //        SqlCommand cmd = new SqlCommand(Query);
        //        cmd.Parameters.Add("@TicketNo", SqlDbType.VarChar).Value = selectedTicket;
        //        cmd.Parameters.Add("@MultiWeightTransPending", SqlDbType.Bit).Value = false;
        //        cmd.Parameters.Add("@Pending", SqlDbType.Bit).Value = false;
        //        cmd.Parameters.Add("@Closed", SqlDbType.Bit).Value = true;
        //        new MasterDBCall().InsertData(cmd, CommandType.Text);
        //        return true;

        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog.WriteToFile("GetTransactionDetailsByTicket:" + ex.Message);
        //        return false;
        //    }
        //}

        public void ShowMessage(Action<string> message, string name)
        {
            this.Dispatcher.Invoke(() =>
            {
                LastMessage = name;
                message(LastMessage);
            });
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }


    }
}
