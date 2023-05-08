using Microsoft.Reporting.WinForms;
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
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        public ReportView()
        {
            InitializeComponent();
            GetData();
        }

        private void GetData()
        {
            SqlConnection cn = new SqlConnection(@"Data Source=192.168.0.28,1434;Initial Catalog=IWT;Persist Security Info=True;User ID=essae;Password=essae@123");
            cn.Open();
            SqlCommand cmd = new SqlCommand("select * from Transaction_Details", cn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            ReportDataSource rds = new ReportDataSource("DataSet1", dt);
            ReportViewerDemo.LocalReport.ReportPath = @"D:\IWT\IWT 5\IWT\TransactionReport\Transaction_Report.rdlc";
            ReportViewerDemo.LocalReport.DataSources.Clear();
            ReportViewerDemo.LocalReport.DataSources.Add(rds);
            ReportViewerDemo.RefreshReport();
        }

        private void PCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
