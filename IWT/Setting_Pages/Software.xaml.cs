using IWT.DBCall;
using IWT.Shared;
using IWT.ViewModel;
using System;
using System.Collections.Generic;
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

namespace IWT.Setting_Pages
{
    /// <summary>
    /// Interaction logic for Software.xaml
    /// </summary>
    public partial class Software : Page
    {
        string LastMessage;
        private readonly ToastViewModel toastViewModel;
        Setting_DBCall db = new Setting_DBCall();
        public Software()
        {
            toastViewModel = new ToastViewModel();
            InitializeComponent();
            GetSoftwareConfig();
        }

        public void GetSoftwareConfig()
        {
            try
            {
                DataTable dt1 = db.GetSoftwareData("SELECT * FROM Software_SettingConfig");
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    foreach (DataRow row in dt1.Rows)
                    {
                        var a = (row["Single_Transaction"].ToString());
                        var b = (row["First_Transaction"].ToString());
                        var c = (row["Second_Transaction"].ToString());
                        var d = (row["First_MultiTransaction"].ToString());
                        var e = (row["Second_MultiTransaction"].ToString());
                        var f = (row["Single_Axle"].ToString());
                        var g = (row["First_Axle"].ToString());
                        var h = (row["Second_Axle"].ToString());
                        var i = (row["Loading"].ToString());
                        var j = (row["Unloading"].ToString());
                        if (a == "True")
                        {
                            single.IsChecked = true;
                        }
                        if (b == "True")
                        {
                            first.IsChecked = true;
                        }
                        if (c == "True")
                            second.IsChecked = true;
                        if (d == "True")
                            ftmulti.IsChecked = true;
                        if (e == "True")
                            secmulti.IsChecked = true;
                        if (f == "True")
                            singleaxle.IsChecked = true;
                        if (g == "True")
                            ftaxle.IsChecked = true;
                        if (h == "True")
                            secaxle.IsChecked = true;
                        if (i == "True")
                            loading.IsChecked = true;
                        if (j == "True")
                            first.IsChecked = true;
                    }
                   // RFIDno.ItemsSource = items;
                }
                else
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For Software Config!!");
            }
            catch (Exception ex)
            {

                WriteLog.WriteToFile("Software:- GetSoftwareConfig - " + ex.Message);
            }
        }

        private void SoftwareSave_Click(object sender, RoutedEventArgs e)
        {
            Software_config data = new Software_config();
            Setting_DBCall db = new Setting_DBCall();
            data.First = first.IsChecked;
            data.Second = second.IsChecked;
            data.Single = single.IsChecked;
            data.First_multi = ftmulti.IsChecked;
            data.Second_multi = secmulti.IsChecked;
            data.Single_axle = singleaxle.IsChecked;
            data.First_axle = ftaxle.IsChecked;
            data.Second_axle = secaxle.IsChecked;
            data.Loading = loading.IsChecked;
            data.Unloading = unloading.IsChecked;
            if (data.First_axle != false || data.First != false || data.Second != false || data.Single != false || data.First_multi != false || data.Second_multi != false || data.Second_axle != false || data.Single_axle != false || data.Second_axle != false || data.Loading != false || data.Unloading != false)
            {
                DataTable dt1 = db.GetSoftwareData("SELECT * FROM Software_SettingConfig");
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    db.UpdateSoftwareData(data);
                    WriteLog.WriteToFile("SoftwareSave_Click:- UpdateSoftwateData - Updated Successfully ");
                    // RFIDno.ItemsSource = items;
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Data Updated Successsfully!!");
                }
                else
                {
                    db.InsertSoftwareData(data);
                    WriteLog.WriteToFile("SoftwareSave_Click:- UpdateSoftwateData - Inserted Successfully ");
                    // RFIDno.ItemsSource = items;
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Data Inserted Successsfully!!");
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Please Select Fields!!");
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
        public class Software_config
        {
            public bool? Single { get; set; }
            public bool? First { get; set; }
            public bool? Second { get; set; }
            public bool? First_multi { get; set; }
            public bool? Second_multi { get; set; }
            public bool? Single_axle { get; set; }
            public bool? First_axle { get; set; }
            public bool? Second_axle { get; set; }
            public bool? Loading { get; set; }
            public bool? Unloading { get; set; }

        }
    }
}
