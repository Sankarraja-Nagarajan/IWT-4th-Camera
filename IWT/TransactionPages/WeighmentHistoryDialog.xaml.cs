using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for WeighmentHistoryDialog.xaml
    /// </summary>
    public partial class WeighmentHistoryDialog : UserControl
    {
        public static MasterDBCall masterDBCall = new MasterDBCall();
        public static CommonFunction commonFunction = new CommonFunction();
        List<TransactionDetails> transactionDetails=new List<TransactionDetails>();
        int TicketNo = 0;
        public WeighmentHistoryDialog(int _TicketNo)
        {
            InitializeComponent();
            TicketNo = _TicketNo;
            GetTransactionDetailsByTicket(TicketNo);
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        public List<TransactionDetails> GetTransactionDetailsByTicket(int TicketNo)
        {
            try
            {
                transactionDetails = commonFunction.GetTransactionDetailsByTicket(TicketNo);
                WeighmentDataGrid.ItemsSource= transactionDetails;
                WeighmentDataGrid.Items.Refresh();
                return transactionDetails;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetTransactionDetailsByTicket:" + ex.Message);
                return null;
            }
        }
    }
}
