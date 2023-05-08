using IWT.DBCall;
using IWT.Shared;
using IWT.ViewModel;
using IWT.Views;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace IWT.Setting_Pages
{
    /// <summary>
    /// Interaction logic for Weighing.xaml
    /// </summary>
    public partial class Weighing : Page
    {
        string LastMessage;
        private readonly ToastViewModel toastViewModel;
        Setting_DBCall db = null;
        public MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        string hardwareProfile;
        public Weighing()
        {
            toastViewModel = new ToastViewModel();
            Setting_DBCall db = new Setting_DBCall();
            InitializeComponent();
            hardwareProfile = mainWindow.HardwareProfile;
            if (hardwareProfile != null)
            {
                DataTable dt1 = db.GetWeighBridgeData($"SELECT * FROM Weighbridge_Settings where HardwareProfile='{hardwareProfile}'");
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    foreach (DataRow row in dt1.Rows)
                    {
                        Host.Text = (row["Host"].ToString());
                        Port.Text = (row["Port"].ToString());
                    }
                    WriteLog.WriteToFile("InitializeComponent:- InitializeComponentData - Inserted Successfully ");
                    // RFIDno.ItemsSource = items;
                }
                else
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For WeighBridge !!");
            }          

        }

        private void Weight_Launch_Click(object sender, RoutedEventArgs e)
        {
            Setting_DBCall db = new Setting_DBCall();
            Weight_Data data = new Weight_Data();
            data.Host = Host.Text;
            data.Port = Port.Text;
            data.HardwareProfile = hardwareProfile;
            DataTable dt1 = db.GetWeighBridgeData($"SELECT * FROM Weighbridge_Settings where HardwareProfile='{hardwareProfile}'");
            if (data.Host != "" || data.Port != "")
            {
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    db.UpdateWeighBridgeData(data);
                    WriteLog.WriteToFile("Weight_Launch_Click:- UpdateWeighBridgeData - Updated Successfully ");
                    // RFIDno.ItemsSource = items;
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Updated Successsfully !!");
                }
                else
                {
                    db.InsertWeighBridgeData(data);
                    WriteLog.WriteToFile("Weight_Launch_Click:- InsertWeighBridgeData - Inserted Successfully ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "WeightBridge Data Inserted Successsfully !!");
                    // RFIDno.ItemsSource = items;
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Enter WeighBridge Fields !!");
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
        public class Weight_Data
        {
            public string Host { get; set; }
            public string Port { get; set; }
            public string HardwareProfile { get; set; }
        }

    }
}
