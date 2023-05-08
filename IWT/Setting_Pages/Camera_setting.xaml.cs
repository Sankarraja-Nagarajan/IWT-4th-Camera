using IWT.DBCall;
using IWT.Shared;
using IWT.ViewModel;
using Microsoft.Win32;
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
    /// Interaction logic for Camera_setting.xaml
    /// </summary>
    public partial class Camera_setting : Page
    {
        string radiovalue;
        string LastMessage;
        private readonly ToastViewModel toastViewModel;
        public static event EventHandler<GeneralEventHandler> onCompletion = delegate { };
        public MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        string hardwareProfile;
        public Camera_setting()
        {
            toastViewModel = new ToastViewModel();
            InitializeComponent();
            hardwareProfile = mainWindow.HardwareProfile;

        }
        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var result = openFileDialog.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                Log.Text = openFileDialog.SelectedPath;
            }
        }
        private void btnOpenFile_Click1(object sender, RoutedEventArgs e)
        {
            var openFileDialog1 = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var result = openFileDialog1.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                Log1.Text = openFileDialog1.SelectedPath;
            }
        }
        private void btnOpenFile_Click2(object sender, RoutedEventArgs e)
        {
            var openFileDialog2 = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var result = openFileDialog2.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                Log2.Text = openFileDialog2.SelectedPath;
            }
        }

        public class Camera_Config
        {
            public string capture { get; set; }
            public string stream { get; set; }
            public string type { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string log { get; set; }
            public bool enable { get; set; }
            public int recordID { get; set; }
            public string HarwareProfile { get; set; }
        }

        private void CCTV1Save_Click(object sender, RoutedEventArgs e)
        {
            Setting_DBCall db = new Setting_DBCall();
            Camera_Config data = new Camera_Config();
            data.capture = Camera_Capture.Text;
            data.stream = Camera_Stream.Text;
            data.type = Camera_Type.Text;
            data.username = Camera_User.Text;
            data.password = Camera_password.Password;
            data.log = Log.Text;
            if((bool)radio1.IsChecked)
            data.enable = true;
            else
            data.enable = false;
            data.HarwareProfile = hardwareProfile;
            data.recordID = 1;
            if (data.capture !="" || data.stream != "" || data.type !="" ||data.username !="" || data.password != "" || data.log !="" )
            {
                DataTable dt1 = db.GetCCTVData($"SELECT * FROM CCTV_Settings Where RecordID='{data.recordID}' and HarwareProfile='{data.HarwareProfile}'");
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    db.UpdateCCTVData(data);
                    WriteLog.WriteToFile("CCTV1Save_Click:- UpdateCCTVData -CCVT1 Updated Successfully ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "CCTV1 Updated Successsfully !!");
                    // RFIDno.ItemsSource = items;
                }
                else
                {
                    db.InsertCCTVData(data);
                    WriteLog.WriteToFile("CCTV1Save_Click:- InsertCCTVData -CCTV1 Inserted Successfully ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "CCTV1 Inserted Successsfully !!");
                    // RFIDno.ItemsSource = items;
                }
                onCompletion.Invoke(this, new GeneralEventHandler());
            }

            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Enter CCTV1 Fields !!");
            }
        }
        public void RadioButtonChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                var radioButton = sender as RadioButton;
                if (radioButton == null)
                    return;
                radiovalue = radioButton.Name.ToString();

                // save_value(loadvalue);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CCTV2_Click(object sender, RoutedEventArgs e)
        {
            Setting_DBCall db = new Setting_DBCall();
            Camera_Config data = new Camera_Config();
            data.capture = Camera_Capture1.Text;
            data.stream = Camera_Stream1.Text;
            data.type = Camera_Type1.Text;
            data.username = Camera_User1.Text;
            data.password = Camera_password1.Password;
            data.log = Log1.Text;
            if ((bool)radio2.IsChecked)
                data.enable = true;
            else
                data.enable = false;
            data.HarwareProfile = hardwareProfile;
            data.recordID = 2;
            if (data.capture != "" || data.stream != "" || data.type != "" || data.username != "" || data.password != "" || data.log != "")
            {
                DataTable dt1 = db.GetCCTVData($"SELECT * FROM CCTV_Settings Where RecordID='{data.recordID}' and HarwareProfile='{data.HarwareProfile}'");
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    db.UpdateCCTVData(data);
                    WriteLog.WriteToFile("CCTV2_Click:- UpdateCCTVData -CCTV2 Updated Successfully ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "CCTV2 Updated Successsfully !!");
                    // RFIDno.ItemsSource = items;
                }
                else
                {
                    db.InsertCCTVData(data);
                    WriteLog.WriteToFile("CCTV2_Click:- InsertCCTVData -  CCTV2 Inserted Successfully ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "CCTV2 Inserted Successsfully !!");
                    // RFIDno.ItemsSource = items;
                }
                onCompletion.Invoke(this, new GeneralEventHandler());
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Enter CCTV2 Fields !!");
            }
        }

        private void CCTV3_Click(object sender, RoutedEventArgs e)
        {
            Setting_DBCall db = new Setting_DBCall();
            Camera_Config data = new Camera_Config();
            data.capture = Camera_Capture2.Text;
            data.stream = Camera_Stream2.Text;
            data.type = Camera_Type2.Text;
            data.username = Camera_User2.Text;
            data.password = Camera_password2.Password;
            data.log = Log2.Text;
            if ((bool)radio3.IsChecked)
                data.enable = true;
            else
                data.enable = false;
            data.HarwareProfile = hardwareProfile;
            data.recordID = 3;
            if (data.capture != "" || data.stream != "" || data.type != "" || data.username != "" || data.password != "" || data.log != "")
            {
                DataTable dt1 = db.GetCCTVData($"SELECT * FROM CCTV_Settings Where RecordID='{data.recordID}' and HarwareProfile='{data.HarwareProfile}'");
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    db.UpdateCCTVData(data);
                    WriteLog.WriteToFile("CCTV3_Click:- UpdateCCTVData - Updated Successfully ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "CCTV3 Updated Successsfully !!");
                    // RFIDno.ItemsSource = items;
                }
                else
                {
                    db.InsertCCTVData(data);
                    WriteLog.WriteToFile("CCTV3_Click:- InsertCCTVData - Inserted Successfully ");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "CCTV3 Inserted Successsfully !!");
                    // RFIDno.ItemsSource = items;
                }
                onCompletion.Invoke(this, new GeneralEventHandler());
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Enter CCTV3 Fields !!");
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
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Setting_DBCall db = new Setting_DBCall();
            TabItem ti = Tabcontrol.SelectedItem as TabItem;
            var selectedtab = ti.Uid;
            DataTable dt1 = db.GetCCTVData($"SELECT * FROM CCTV_Settings Where RecordID='{selectedtab}' and HarwareProfile='{hardwareProfile}'");

            if (selectedtab == "1")
            {
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    foreach (DataRow row in dt1.Rows)
                    {
                        Camera_Stream.Text = (row[2].ToString());
                        Camera_Capture.Text = (row[3].ToString());
                        Camera_Type.Text = (row[10].ToString());
                        Camera_User.Text = (row[8].ToString());
                        Camera_password.Password = (row[9].ToString());
                        Log.Text = (row[5].ToString());
                        radiovalue = (row[1].ToString());
                        if (radiovalue == "True")
                        {
                            radio1.IsChecked = true;
                        }
                        else
                        {
                            radio1.IsChecked = false;
                        }
                    }
                    WriteLog.WriteToFile("InitializeComponent:- InitializeComponentData - Inserted Successfully ");
                    // RFIDno.ItemsSource = items;
                }
                else if ((dt1 == null || dt1.Rows.Count == 0) && Camera_Stream.Text == "")
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For CCTV1 !!");

                }
            }
            else if(selectedtab == "2")
            {
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    foreach (DataRow row in dt1.Rows)
                    {
                        Camera_Stream1.Text = (row[2].ToString());
                        Camera_Capture1.Text = (row[3].ToString());
                        Camera_Type1.Text = (row[10].ToString());
                        Camera_User1.Text = (row[8].ToString());
                        Camera_password1.Password = (row[9].ToString());
                        Log1.Text = (row[5].ToString());
                        radiovalue = (row[1].ToString());
                        if (radiovalue == "True")
                        {
                            radio2.IsChecked = true;
                        }
                        else
                        {
                            radio2.IsChecked = false;
                        }
                    }
                    WriteLog.WriteToFile("InitializeComponent:- InitializeComponentData - Inserted Successfully ");
                    // RFIDno.ItemsSource = items;
                }
                else if (dt1 == null || dt1.Rows.Count == 0 && Camera_Stream1.Text == "")
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For CCTV2 !!");

                }
            }
            else
            {
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    foreach (DataRow row in dt1.Rows)
                    {
                        Camera_Stream2.Text = (row[2].ToString());
                        Camera_Capture2.Text = (row[3].ToString());
                        Camera_Type2.Text = (row[10].ToString());
                        Camera_User2.Text = (row[8].ToString());
                        Camera_password2.Password = (row[9].ToString());
                        Log2.Text = (row[5].ToString());
                        radiovalue = (row[1].ToString());
                        if (radiovalue == "True")
                        {
                            radio3.IsChecked = true;
                        }
                        else
                        {
                            radio3.IsChecked = false;
                        }
                    }
                    WriteLog.WriteToFile("InitializeComponent:- InitializeComponentData - Inserted Successfully ");
                    // RFIDno.ItemsSource = items;
                }
                else if (dt1 == null || dt1.Rows.Count == 0 && Camera_Stream2.Text == "")
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For CCTV3 !!");

                }
            }
        }
    }
}
