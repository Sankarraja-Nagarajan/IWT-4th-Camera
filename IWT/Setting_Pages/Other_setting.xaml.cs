using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

namespace IWT.Setting_Pages
{
    /// <summary>
    /// Interaction logic for Other_setting.xaml
    /// </summary>

    public partial class Other_setting : Page
    {
        string LastMessage;
        private readonly ToastViewModel toastViewModel;
        ERPFileLocation selectedERPFileLocation = new ERPFileLocation();
        CloudAppConfig selectedCloudAppConfig = new CloudAppConfig();
        AdminDBCall adminDBCall = new AdminDBCall();
        MasterDBCall masterDBCall = new MasterDBCall();
        public MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        string hardwareProfile;
        public Other_setting()
        {
            toastViewModel = new ToastViewModel();
            InitializeComponent();
            hardwareProfile = mainWindow.HardwareProfile;
            if (hardwareProfile != null)
            {
                AdminDBCall db = new AdminDBCall();
                DataTable dt1 = db.GetAllData($"SELECT * FROM Other_Settings where HardwareProfile='{hardwareProfile}'");
                if (dt1 == null || dt1.Rows.Count == 0)
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For Other Settings !!");
                }
            }
            Loaded += Other_setting_Loaded;
        }

        private void Other_setting_Loaded(object sender, RoutedEventArgs e)
        {            
            GetERPFileLocation();
            GetCloudAppConfig();
        }

        private void Other_settingSave_Click(object sender, RoutedEventArgs e)
        {
            OtherSettings data = new OtherSettings();
            AdminDBCall db = new AdminDBCall();
            data.NoOfCopies = int.Parse(Copies.Text);
            int expirydays = 0;
            int.TryParse(ExpiryDays.Text, out expirydays);
            //data.SystemID = SystemID.Text;
            data.BaseURL = BaseURL.Text;
            data.SMSAlerts = (bool)SMS_alert.IsChecked;
            data.DosPrint = (bool)Dos_print.IsChecked;
            data.AutoPrint = (bool)Auto_print.IsChecked;
            data.AutoMail = (bool)Auto_mail.IsChecked;
            data.AutoFtMail = (bool)Auto_ft_mail.IsChecked;
            data.AutoFtPrint = (bool)Auto_ft_print.IsChecked;
            data.AutoFtSMS = (bool)Auto_ft_sms.IsChecked;
            data.AutoCopies = (bool)Auto_copies.IsChecked;
            data.DefaultTransaction = Ticket.Text;
            data.ExpiryDays = expirydays;
            data.StableWeightCount = string.IsNullOrEmpty(Stable_weight.Text) ? 0 : Convert.ToInt32(Stable_weight.Text);
            data.MinimumWeightCount = string.IsNullOrEmpty(Min_weight.Text) ? 0 : Convert.ToInt32(Min_weight.Text);
            data.StablePLCCount = string.IsNullOrEmpty(Stable_PLC.Text) ? 0 : Convert.ToInt32(Stable_PLC.Text);
            data.DeviceName = Device_name.Text;
            data.PortNumber = Port_number.Text;
            data.DeviceName_CCID = Device_name1.Text;
            data.PortNumber_CCID = Port_number1.Text;
            data.AutoPrintPreview = (bool)Auto_Print_Preview.IsChecked;
            data.HardwareProfile = hardwareProfile;
            DataTable dt1 = db.GetAllData($"SELECT * FROM Other_Settings where HardwareProfile='{data.HardwareProfile}'");
            string saveQuery = "";
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                saveQuery = $@"UPDATE [Other_Settings] SET NoOfCopies='{data.NoOfCopies}',BaseURL='{data.BaseURL}',SMSAlerts='{data.SMSAlerts}',DosPrint='{data.DosPrint}',AutoPrint='{data.AutoPrint}',AutoMail='{data.AutoMail}',AutoFtMail='{data.AutoFtMail}',AutoFtSMS='{data.AutoFtSMS}',AutoFtPrint='{data.AutoFtPrint}',AutoCopies='{data.AutoCopies}',DefaultTransaction='{data.DefaultTransaction}',StableWeightCount='{data.StableWeightCount}',MinimumWeightCount='{data.MinimumWeightCount}',StablePLCCount='{data.StablePLCCount}',DeviceName='{data.DeviceName}',PortNumber='{data.PortNumber}',DeviceName_CCID='{data.DeviceName_CCID}',PortNumber_CCID='{data.PortNumber_CCID}',AutoPrintPreview='{data.AutoPrintPreview}',ExpiryDays='{data.ExpiryDays}' WHERE ID='{dt1.Rows[0]["ID"]}' and HardwareProfile='{data.HardwareProfile}'";
                //db.UpdateOthersData(data);
                //WriteLog.WriteToFile("Other_settingSave_Click:- UpdateOthersData - Updated Successfully ");
                //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Updated Successfully!!");
            }
            else
            {
                saveQuery = $@"INSERT INTO [Other_Settings] (NoOfCopies,BaseURL,SMSAlerts,DosPrint,AutoPrint,AutoMail,AutoFtMail,AutoFtSMS,AutoCopies,DefaultTransaction,StableWeightCount,MinimumWeightCount,StablePLCCount,DeviceName,PortNumber,DeviceName_CCID,PortNumber_CCID,AutoPrintPreview,AutoFtPrint,ExpiryDays,HardwareProfile,DualScaleSet,SaveTransTxt,TareExpirePeriod,AutoDelete) VALUES ('{data.NoOfCopies}','{data.BaseURL}','{data.SMSAlerts}','{data.DosPrint}','{data.AutoPrint}','{data.AutoMail}','{data.AutoFtMail}','{data.AutoFtSMS}','{data.AutoCopies}','{data.DefaultTransaction}','{data.StableWeightCount}','{data.MinimumWeightCount}','{data.StablePLCCount}','{data.DeviceName}','{data.PortNumber}','{data.DeviceName_CCID}','{data.PortNumber_CCID}','{data.AutoPrintPreview}','{data.AutoFtPrint}','{data.ExpiryDays}','{data.HardwareProfile}','{data.DualScaleSet}','{data.SaveTransTxt}','{data.TareExpirePeriod}','{data.AutoDelete}')";
                //db.InsertOthersData(data);
                //WriteLog.WriteToFile("Other_settingSave_Click:- InsertOthersData - Inserted Successfully ");
                //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Inserted Successfully!!");
            }
            var res = db.ExecuteQuery(saveQuery);
            if (res)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Saved Successfully!!");
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !!");
            }

        }
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AdminDBCall db = new AdminDBCall();
            TabItem ti = Tabcontrol.SelectedItem as TabItem;
            var selectedtab = ti.Uid;
            DataTable dt1 = db.GetAllData($"SELECT * FROM Other_Settings where HardwareProfile='{hardwareProfile}'");
            string JSONString = JsonConvert.SerializeObject(dt1);
            var result = JsonConvert.DeserializeObject<List<OtherSettings>>(JSONString);
            Other_settingSave.Visibility = Visibility.Visible;

            if (selectedtab == "1")
            {
                if (result.Count > 0)
                {
                    Copies.Text = result[0].NoOfCopies.ToString();
                    SMS_alert.IsChecked = result[0].SMSAlerts;
                    Dos_print.IsChecked = result[0].DosPrint;
                    Auto_print.IsChecked = result[0].AutoPrint;
                    Auto_mail.IsChecked = result[0].AutoMail;
                    Auto_Print_Preview.IsChecked = result[0].AutoPrintPreview;
                    Auto_ft_sms.IsChecked = result[0].AutoFtSMS;
                    Auto_ft_print.IsChecked = result[0].AutoFtPrint;
                    Auto_ft_mail.IsChecked = result[0].AutoFtMail;
                    Auto_copies.IsChecked = result[0].AutoCopies;
                    Copies.Text = result[0].NoOfCopies.ToString();
                    BaseURL.Text = result[0].BaseURL;
                    Stable_weight.Text = result[0].StableWeightCount.ToString();
                    Min_weight.Text = result[0].MinimumWeightCount.ToString();
                    Stable_PLC.Text = result[0].StablePLCCount.ToString();
                }

            }
            else if (selectedtab == "2")
            {
                //if (dt1 != null && dt1.Rows.Count > 0)
                //{
                //    foreach (DataRow row in dt1.Rows)
                //    {
                //        Ticket.Text = (row[1].ToString());
                //    }
                //}
                //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For File Location!!");
                if (result.Count > 0)
                {
                    Ticket.Text = result[0].DefaultTransaction;
                    ExpiryDays.Text = result[0].ExpiryDays.ToString();
                }
            }
            else if (selectedtab == "3")
            {
                //if (dt1 != null && dt1.Rows.Count > 0)
                //{
                //    foreach (DataRow row in dt1.Rows)
                //    {
                //        Stable_weight.Text = (row[15].ToString());
                //        Min_weight.Text = (row[17].ToString());
                //        Stable_PLC.Text = (row[20].ToString());

                //    }
                //}
                //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For File Location!!");
                if (result.Count > 0)
                {
                    Stable_weight.Text = result[0].StableWeightCount.ToString();
                    Min_weight.Text = result[0].MinimumWeightCount.ToString();
                    Stable_PLC.Text = result[0].StablePLCCount.ToString();
                }
            }
            else if (selectedtab == "4")
            {
                //if (dt1 != null && dt1.Rows.Count > 0)
                //{
                //    foreach (DataRow row in dt1.Rows)
                //    {
                //        Device_name.Text = (row[21].ToString());
                //        Port_number.Text = (row[22].ToString());
                //    }
                //}
                //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For File Location!!");
                if (result.Count > 0)
                {
                    Device_name.Text = result[0].DeviceName;
                    Port_number.Text = result[0].PortNumber;
                }
            }
            else if (selectedtab == "5")
            {
                //if (dt1 != null && dt1.Rows.Count > 0)
                //{
                //    foreach (DataRow row in dt1.Rows)
                //    {
                //        Device_name1.Text = (row[23].ToString());
                //        Port_number1.Text = (row[24].ToString());
                //    }
                //}
                //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For File Location!!");
                if (result.Count > 0)
                {
                    Device_name1.Text = result[0].DeviceName_CCID;
                    Port_number1.Text = result[0].PortNumber_CCID;
                }
            }
            else if (selectedtab == "6")
            {
                Other_settingSave.Visibility = Visibility.Collapsed;
            }
            else if (selectedtab == "7")
            {
                Other_settingSave.Visibility = Visibility.Collapsed;
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
        public class Other_config
        {
            public bool? SMS_alert { get; set; }
            public bool? Dos { get; set; }
            public bool? Print { get; set; }
            public bool? Mail { get; set; }
            public bool? First_SMS { get; set; }
            public bool? First_Print { get; set; }
            public bool? First_Mail { get; set; }
            public string Copies { get; set; }
            public string TareWeight { get; set; }
            public string BaseURL { get; set; }
            public string Ticket { get; set; }
            public string Stableweight { get; set; }
            public string Minweight { get; set; }
            public string Stableplc { get; set; }
            public string DeviceName { get; set; }
            public string PortNumber { get; set; }
            public string DeviceName1 { get; set; }
            public string PortNumber1 { get; set; }
            public int ExpiryDays { get; set; }
        }

        private void SMS_alert_Changed(object sender, RoutedEventArgs e)
        {
            if ((bool)SMS_alert.IsChecked)
            {
                Auto_ft_sms.IsEnabled = true;
            }
            else
            {
                Auto_ft_sms.IsChecked = false;
                Auto_ft_sms.IsEnabled = false;
            }
        }

        private void Auto_print_Changed(object sender, RoutedEventArgs e)
        {
            if ((bool)Auto_print.IsChecked)
            {
                Auto_ft_print.IsEnabled = true;
            }
            else
            {
                Auto_ft_print.IsChecked = false;
                Auto_ft_print.IsEnabled = false;
            }
        }
        private void Auto_mail_Changed(object sender, RoutedEventArgs e)
        {
            if ((bool)Auto_mail.IsChecked)
            {
                Auto_ft_mail.IsEnabled = true;
            }
            else
            {
                Auto_ft_mail.IsChecked = false;
                Auto_ft_mail.IsEnabled = false;
            }
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFolderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var result = openFolderDialog.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                ERPFilePath.Text = openFolderDialog.SelectedPath;
            }
        }

        private void GetERPFileLocation()
        {
            try
            {
                DataTable dt1 = adminDBCall.GetAllData("SELECT * FROM ERPFile_Location");
                string JSONString = JsonConvert.SerializeObject(dt1);
                var ERPFileLocations = JsonConvert.DeserializeObject<List<ERPFileLocation>>(JSONString);
                selectedERPFileLocation = (ERPFileLocations != null && ERPFileLocations.Count > 0) ? ERPFileLocations.FirstOrDefault() : new ERPFileLocation();
                if (selectedERPFileLocation.ID != 0)
                {
                    SetFieldsValue();
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("OtherSetting/GetERPFileLocation", ex);
            }
        }

        private void SaveERPBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ERPFilePath.Text))
            {
                GetFieldsValue();
                if (selectedERPFileLocation != null && selectedERPFileLocation.ID != 0)
                {
                    UpdateERPFileLocation();
                }
                else
                {
                    InsertERPFileLocation();
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please enter ERP File path");
            }
        }


        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearField();

        }

        public void InsertERPFileLocation()
        {
            try
            {
                string Query = "INSERT INTO [ERPFile_Location] (IsEnabled,ERPFilePath,IsXML,IsCSV)" +
                                                       "VALUES (@IsEnabled,@ERPFilePath,@IsXML,@IsCSV)";

                SqlCommand cmd = new SqlCommand(Query);
                cmd = AddParameters(cmd);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "ERP File location created successfully");
                ClearField();
                GetERPFileLocation();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("OtherSetting/InsertERPFileLocation", ex);

            }
        }

        public SqlCommand AddParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@IsEnabled", selectedERPFileLocation.IsEnabled);
            cmd.Parameters.AddWithValue("@ERPFilePath", selectedERPFileLocation.ERPFilePath);
            cmd.Parameters.AddWithValue("@IsXML", selectedERPFileLocation.IsXML);
            cmd.Parameters.AddWithValue("@IsCSV", selectedERPFileLocation.IsCSV);
            return cmd;
        }

        public void UpdateERPFileLocation()
        {
            try
            {
                string Query = "UPDATE [ERPFile_Location] SET IsEnabled=@IsEnabled,ERPFilePath=@ERPFilePath,IsXML=@IsXML,IsCSV=@IsCSV WHERE ID=@ID";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@ID", selectedERPFileLocation.ID);
                cmd = AddParameters(cmd);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "ERP File location updated successfully");
                ClearField();
                GetERPFileLocation();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("OtherSetting/UpdateERPFileLocation", ex);
            }
        }
        public void SetFieldsValue()
        {
            IsEnabled.IsChecked = selectedERPFileLocation.IsEnabled;
            ERPFilePath.Text = selectedERPFileLocation.ERPFilePath;
            IsXML.IsChecked = selectedERPFileLocation.IsXML;
            IsCSV.IsChecked = selectedERPFileLocation.IsCSV;
        }

        public void GetFieldsValue()
        {
            if (selectedERPFileLocation == null)
            {
                selectedERPFileLocation = new ERPFileLocation();
            }
            selectedERPFileLocation.IsEnabled = IsEnabled.IsChecked.HasValue ? IsEnabled.IsChecked.Value : false;
            selectedERPFileLocation.ERPFilePath = ERPFilePath.Text;
            selectedERPFileLocation.IsXML = IsXML.IsChecked.HasValue ? IsXML.IsChecked.Value : false;
            selectedERPFileLocation.IsCSV = IsCSV.IsChecked.HasValue ? IsCSV.IsChecked.Value : false;

        }
        private void ClearField()
        {
            selectedERPFileLocation = new ERPFileLocation();
            IsEnabled.IsChecked = false;
            ERPFilePath.Text = "";
            IsXML.IsChecked = false;
            IsXML.IsChecked = false;
        }



        private void GetCloudAppConfig()
        {
            try
            {
                DataTable dt1 = adminDBCall.GetAllData("SELECT * FROM CloudApp_Config");
                string JSONString = JsonConvert.SerializeObject(dt1);
                var CloudAppConfigs = JsonConvert.DeserializeObject<List<CloudAppConfig>>(JSONString);
                selectedCloudAppConfig = (CloudAppConfigs != null && CloudAppConfigs.Count > 0) ? CloudAppConfigs.FirstOrDefault() : new CloudAppConfig();
                if (selectedCloudAppConfig.ID != 0)
                {
                    SetCloudAppConfigFieldsValue();
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("OtherSetting/GetCloudAppConfig", ex);
            }
        }

        private void SaveCloudAppBtn_Click(object sender, RoutedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(ERPFilePath.Text))
            //{
            GetCloudAppConfigFieldsValue();
            if (selectedCloudAppConfig != null && selectedCloudAppConfig.ID != 0)
            {
                UpdateCloudAppConfig();
            }
            else
            {
                InsertCloudAppConfig();
            }
            //}
            //else
            //{
            //    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please enter the role name");
            //}
        }


        private void ClearCloudAppConfigButton_Click(object sender, RoutedEventArgs e)
        {
            ClearCloudAppConfigField();

        }

        public void InsertCloudAppConfig()
        {
            try
            {
                string Query = "INSERT INTO [CloudApp_Config] (IsEnabled,SystemID,WeighBridgeID,BaseURL)" +
                                                       "VALUES (@IsEnabled,@SystemID,@WeighBridgeID,@BaseURL)";

                SqlCommand cmd = new SqlCommand(Query);
                cmd = AddCloudAppConfigParameters(cmd);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "CloudApp Config created successfully");
                ClearCloudAppConfigField();
                GetCloudAppConfig();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("OtherSetting/InsertCloudAppConfig", ex);

            }
        }

        public SqlCommand AddCloudAppConfigParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@IsEnabled", selectedCloudAppConfig.IsEnabled);
            cmd.Parameters.AddWithValue("@SystemID", selectedCloudAppConfig.SystemID);
            cmd.Parameters.AddWithValue("@WeighBridgeID", selectedCloudAppConfig.WeighBridgeID);
            cmd.Parameters.AddWithValue("@BaseURL", selectedCloudAppConfig.BaseURL);
            return cmd;
        }

        public void UpdateCloudAppConfig()
        {
            try
            {
                string Query = "UPDATE [CloudApp_Config] SET IsEnabled=@IsEnabled,SystemID=@SystemID,WeighBridgeID=@WeighBridgeID,BaseURL=@BaseURL WHERE ID=@ID";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@ID", selectedCloudAppConfig.ID);
                cmd = AddCloudAppConfigParameters(cmd);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "CloudApp Config updated successfully");
                ClearCloudAppConfigField();
                GetCloudAppConfig();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("OtherSetting/UpdateCloudAppConfig", ex);
            }
        }
        public void SetCloudAppConfigFieldsValue()
        {
            IsCloudAppEnabled.IsChecked = selectedCloudAppConfig.IsEnabled;
            SystemId.Text = selectedCloudAppConfig.SystemID;
            WeighBridgeID.Text = selectedCloudAppConfig.WeighBridgeID;
            BaseURI.Text = selectedCloudAppConfig.BaseURL;
        }

        public void GetCloudAppConfigFieldsValue()
        {
            if (selectedCloudAppConfig == null)
            {
                selectedCloudAppConfig = new CloudAppConfig();
            }
            selectedCloudAppConfig.IsEnabled = IsCloudAppEnabled.IsChecked.HasValue ? IsCloudAppEnabled.IsChecked.Value : false;
            selectedCloudAppConfig.SystemID = SystemId.Text;
            selectedCloudAppConfig.WeighBridgeID = WeighBridgeID.Text;
            selectedCloudAppConfig.BaseURL = BaseURI.Text;
        }
        private void ClearCloudAppConfigField()
        {
            selectedCloudAppConfig = new CloudAppConfig();
            IsCloudAppEnabled.IsChecked = false;
            //SystemID.Text = "";
            WeighBridgeID.Text = "";
            BaseURI.Text = "";
        }

        private void ExpiryDays_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
