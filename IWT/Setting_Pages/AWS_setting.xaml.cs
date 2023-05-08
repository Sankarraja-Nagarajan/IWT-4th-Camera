using IWT.Admin_Pages;
using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IWT.Setting_Pages
{
    /// <summary>
    /// Interaction logic for AWS_setting.xaml
    /// </summary>
    public partial class AWS_setting : Page
    {
        string LastMessage;
        private readonly ToastViewModel toastViewModel;
        Setting_DBCall db = null;
        MasterDBCall masterDBCall = new MasterDBCall();
        CommonFunction commonFunction = new CommonFunction();
        RFIDReaderMaster readerMaster1 = new RFIDReaderMaster();
        RFIDReaderMaster readerMaster2 = new RFIDReaderMaster();
        RFIDReaderMaster readerMaster3 = new RFIDReaderMaster();
        PLCMaster pLCMaster = new PLCMaster();
        SerialCommunicationSetting serialComSettings = new SerialCommunicationSetting();
        AWSConfiguration awsConfigurations = new AWSConfiguration();
        public static event EventHandler<GeneralEventHandler> onCompletion = delegate { };
        public MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        string hardwareProfile;
        private AdminDBCall _dbContext;
        List<Usermanage> UserManage = new List<Usermanage>();
        public ManageUser manageUser = new ManageUser();
        public AWS_setting()
        {
            toastViewModel = new ToastViewModel();
            Setting_DBCall db = new Setting_DBCall();
            InitializeComponent();
            _dbContext = new AdminDBCall();
            hardwareProfile = mainWindow.HardwareProfile;
            GetUsersManagement();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem ti = Tabcontrol.SelectedItem as TabItem;
            var selectedtab = ti.Uid;

            if (selectedtab == "1")
            {
                awsConfigurations = commonFunction.GetAWSConfiguration(hardwareProfile);
                if (awsConfigurations != null && TimeOut.Text == "")
                {
                    if (awsConfigurations.IsAutoLogin == false)
                    {
                        AutoLoginUserComboBox.Visibility = Visibility.Collapsed;
                        AutoLoginUserImage.Visibility = Visibility.Collapsed;
                        AutoLoginUserText.Visibility = Visibility.Collapsed;
                    }
                    SetAWSConfigurationValue();
                }
                else if (awsConfigurations == null && TimeOut.Text == "")
                {
                    AutoLoginUserComboBox.Visibility = Visibility.Collapsed;
                    AutoLoginUserImage.Visibility = Visibility.Collapsed;
                    AutoLoginUserText.Visibility = Visibility.Collapsed;
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For Configuration !!");
                }
            }

            if (selectedtab == "2")
            {
                readerMaster1 = commonFunction.GetRFIDReaderByReader("1", hardwareProfile);
                if (readerMaster1 != null)
                {
                    SetRFIDReaderMaster1Values();
                }
                else
                {
                    Location1.IsEnabled = true;
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For RFID Reader 1 !!");
                }
            }
            else if (selectedtab == "3")
            {
                readerMaster2 = commonFunction.GetRFIDReaderByReader("2", hardwareProfile);
                if (readerMaster2 != null)
                {
                    SetRFIDReaderMaster2Values();
                }
                else
                {
                    Location2.IsEnabled = true;
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For RFID Reader 2 !!");
                }

            }
            else if (selectedtab == "4")
            {
                readerMaster3 = commonFunction.GetRFIDReaderByReader("3", hardwareProfile);
                if (readerMaster3 != null)
                {
                    SetRFIDReaderMaster3Values();
                }
                else
                {
                    Location3.IsEnabled = true;
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For RFID Reader 3 !!");
                }

            }
            else if (selectedtab == "5")
            {
                pLCMaster = commonFunction.GetPLCMaster(hardwareProfile);
                if (pLCMaster != null)
                {
                    SetPLCMasterValues();
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For PLC !!");
                }
            }
            else if (selectedtab == "6")
            {
                serialComSettings = commonFunction.GetSerialCommunicationSetting(hardwareProfile);
                if (serialComSettings != null)
                {
                    SetSerialCommunicationSetting();
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "No Data For Serial Communication Settings !!");
                }
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

        private void IsAutoLoginCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as System.Windows.Controls.CheckBox;
            if (checkBox.IsChecked == true)
            {
                AutoLoginUserComboBox.Visibility = Visibility.Visible;
                AutoLoginUserImage.Visibility = Visibility.Visible;
                AutoLoginUserText.Visibility = Visibility.Visible;
            }
            else if (checkBox.IsChecked == false)
            {
                AutoLoginUserComboBox.Visibility = Visibility.Collapsed;
                AutoLoginUserImage.Visibility = Visibility.Collapsed;
                AutoLoginUserText.Visibility = Visibility.Collapsed;
                AutoLoginUserComboBox.Text = "";
            }
        }

        public void GetUsersManagement()
        {
            try
            {
                Usermanage list = new Usermanage();
                DataTable table = _dbContext.GetAllData($"SELECT * FROM [User_Management] WHERE [IsLocked]=0");
                if (table != null && table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        list.ID = Convert.ToInt32(row["ID"].ToString());
                        list.Name = (row["UserName"].ToString());
                        var password = (row["Password"].ToString());
                        list.Password = manageUser.Decrypt(password, true);
                        list.Email = (row["EmailID"].ToString());
                        list.Role = (row["Role"].ToString());
                        list.Profile = (row["HardwareProfile"].ToString());
                        var a = (row["IsLocked"].ToString());
                        if (a == "True")
                            list.Locked = true;
                        else
                            list.Locked = false;
                        UserManage.Add(new Usermanage()
                        {
                            ID = list.ID,
                            Name = list.Name,
                            //Password = list.Password,
                            Email = list.Email,
                            Role = list.Role,
                            Profile = list.Profile,
                            Locked = list.Locked,
                        });
                    }
                }

                List<Usermanage> UserManagement = new List<Usermanage>();
                SetAutoLoginUserComboBox();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetUsersManagement", ex);
            }
        }

        public void SetAutoLoginUserComboBox()
        {
            try
            {
                AutoLoginUserComboBox.ItemsSource = UserManage;
                AutoLoginUserComboBox.Items.Refresh();
                AutoLoginUserComboBox.DisplayMemberPath = "Name";
                AutoLoginUserComboBox.SelectedValuePath = "Name";
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetAutoLoginUserComboBox", ex);
            }
        }

        private void SaveBtn1_Click(object sender, RoutedEventArgs e)
        {
            RFIDReaderMaster readerMaster1 = new RFIDReaderMaster();
            readerMaster1.Reader = "1";
            readerMaster1.IP = IP1.Text;
            readerMaster1.Port = int.Parse(Port1.Text);
            readerMaster1.Location = Location1.Text;
            readerMaster1.IsEnable = Reader1.IsChecked;
            readerMaster1.GateExitEnable = GateExitEnable.IsChecked;
            readerMaster1.HardwareProfile = hardwareProfile;
            DataTable dt1 = _dbContext.GetAllData($"SELECT * FROM RFID_Reader_Master where HardwareProfile='{readerMaster1.HardwareProfile}' and Reader='{readerMaster1.Reader}'");
            if (readerMaster1.Reader != null)
            {
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    UpdateRFIDReaderMaster(readerMaster1);
                }
                else
                {
                    CreateRFIDReaderMaster(readerMaster1);
                    Location1.IsEnabled = false;
                }
            }
            onCompletion.Invoke(this, new GeneralEventHandler());
        }

        private void SaveBtn2_Click(object sender, RoutedEventArgs e)
        {
            RFIDReaderMaster readerMaster2 = new RFIDReaderMaster();
            readerMaster2.Reader = "2";
            readerMaster2.IP = IP2.Text;
            readerMaster2.Port = int.Parse(Port2.Text);
            readerMaster2.Location = Location2.Text;
            readerMaster2.IsEnable = Reader2.IsChecked;
            readerMaster2.GateExitEnable = false;
            readerMaster2.HardwareProfile = hardwareProfile;
            DataTable dt1 = _dbContext.GetAllData($"SELECT * FROM RFID_Reader_Master where HardwareProfile='{readerMaster2.HardwareProfile}' and Reader='{readerMaster2.Reader}'");
            if (readerMaster2.Reader != null)
            {
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    UpdateRFIDReaderMaster(readerMaster2);
                }
                else
                {
                    CreateRFIDReaderMaster(readerMaster2);
                    Location2.IsEnabled = false;
                }
            }
            onCompletion.Invoke(this, new GeneralEventHandler());
        }

        private void SaveBtn3_Click(object sender, RoutedEventArgs e)
        {
            RFIDReaderMaster readerMaster3 = new RFIDReaderMaster();
            readerMaster3.Reader = "3";
            readerMaster3.IP = IP3.Text;
            readerMaster3.Port = int.Parse(Port3.Text);
            readerMaster3.Location = Location3.Text;
            readerMaster3.IsEnable = Reader3.IsChecked;
            readerMaster3.GateExitEnable = false;
            readerMaster3.HardwareProfile = hardwareProfile;
            DataTable dt1 = _dbContext.GetAllData($"SELECT * FROM RFID_Reader_Master where HardwareProfile='{readerMaster3.HardwareProfile}' and Reader='{readerMaster3.Reader}'");
            if (readerMaster3.Reader != null)
            {
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    UpdateRFIDReaderMaster(readerMaster3);
                }
                else
                {
                    CreateRFIDReaderMaster(readerMaster3);
                    Location3.IsEnabled = false;
                }
            }
            onCompletion.Invoke(this, new GeneralEventHandler());
        }

        private void SaveBtn6_Click(object sender, RoutedEventArgs e)
        {
            SerialCommunicationSetting serialComSettings = new SerialCommunicationSetting();
            if (Port5.Text == "" || Port5.Text == null)
            {
                serialComSettings.Port = "";
            }
            else
            {
                serialComSettings.Port = Port5.Text;
            }
            if (BaudRate.Text == "" || BaudRate.Text == null)
            {
                serialComSettings.BaudRate = 0;
            }
            else
            {
                serialComSettings.BaudRate = int.Parse(BaudRate.Text);
            }
            if (Parity.Text == "" || Parity.Text == null)
            {
                serialComSettings.Parity = 0;
            }
            else
            {
                serialComSettings.Parity = int.Parse(Parity.Text);
            }
            if (StopBit.Text == "" || StopBit.Text == null)
            {
                serialComSettings.StopBit = 0;
            }
            else
            {
                serialComSettings.StopBit = int.Parse(StopBit.Text);
            }
            if (DataBits.Text == "" || DataBits.Text == null)
            {
                serialComSettings.DataBits = 0;
            }
            else
            {
                serialComSettings.DataBits = int.Parse(DataBits.Text);
            }
            if (DataLength.Text == "" || DataLength.Text == null)
            {
                serialComSettings.DataLength = 0;
            }
            else
            {
                serialComSettings.DataLength = int.Parse(DataLength.Text);
            }
            if (ImmediateDelay.Text == "" || ImmediateDelay.Text == null)
            {
                serialComSettings.ImmediateDelay = 0;
            }
            else
            {
                serialComSettings.ImmediateDelay = int.Parse(ImmediateDelay.Text);
            }
            if (StartChar.Text == "" || StartChar.Text == null)
            {
                serialComSettings.StartChar = "";
            }
            else
            {
                serialComSettings.StartChar = StartChar.Text;
            }
            if (EndChar.Text == "" || EndChar.Text == null)
            {
                serialComSettings.EndChar = "";
            }
            else
            {
                serialComSettings.EndChar = EndChar.Text;
            }
            serialComSettings.HardwareProfile = hardwareProfile;
            serialComSettings.Enable = EnableSerial.IsChecked;
            DataTable dt1 = _dbContext.GetAllData($"SELECT * FROM Serial_COM_Setting where HardwareProfile='{serialComSettings.HardwareProfile}'");
            if (serialComSettings.Port != null)
            {
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    UpdateSerialCommunicationSettings(serialComSettings);
                }
                else
                {
                    CreateSerialCommunicationSettings(serialComSettings);
                }
            }
            onCompletion.Invoke(this, new GeneralEventHandler());
        }

        public void SetRFIDReaderMaster1Values()
        {
            try
            {
                IP1.Text = readerMaster1.IP;
                Port1.Text = readerMaster1.Port.ToString();
                Location1.Text = readerMaster1.Location;
                Reader1.IsChecked = readerMaster1.IsEnable;
                GateExitEnable.IsChecked = readerMaster1.GateExitEnable;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetRFIDReaderMaster1Values", ex);
            }
        }

        public void ResetRFIDReaderMaster1Values()
        {
            try
            {
                readerMaster1 = new RFIDReaderMaster();
                IP1.Text = "";
                Port1.Text = "";
                Location1.Text = "";
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("ResetRFIDReaderMaster1Values", ex);
            }
        }

        public void SetRFIDReaderMaster2Values()
        {
            try
            {
                IP2.Text = readerMaster2.IP;
                Port2.Text = readerMaster2.Port.ToString();
                Location2.Text = readerMaster2.Location;
                Reader2.IsChecked = readerMaster2.IsEnable;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetRFIDReaderMaster2Values", ex);
            }
        }

        public void SetRFIDReaderMaster3Values()
        {
            try
            {
                IP3.Text = readerMaster3.IP;
                Port3.Text = readerMaster3.Port.ToString();
                Location3.Text = readerMaster3.Location;
                Reader3.IsChecked = readerMaster3.IsEnable;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetRFIDReaderMaster3Values", ex);
            }
        }

        public void ResetRFIDReaderMaster3Values()
        {
            try
            {
                readerMaster3 = new RFIDReaderMaster();
                IP3.Text = "";
                Port3.Text = "";
                Location3.Text = "";
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("ResetRFIDReaderMaster3Values", ex);
            }
        }
        public void GetPLCMasterValues()
        {
            try
            {
                pLCMaster.IP = IP4.Text;
                pLCMaster.Port = int.Parse(Port4.Text);
                pLCMaster.IsEnable = (bool)PlcEnable.IsChecked;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetPLCMasterValues", ex);
            }
        }

        public void SetPLCMasterValues()
        {
            try
            {
                //pLCMaster.Tag = "3";
                IP4.Text = pLCMaster.IP;
                Port4.Text = pLCMaster.Port.ToString();
                PlcEnable.IsChecked = (bool)pLCMaster.IsEnable;
                //AntennaID3.Text= pLCMaster.AntennaID;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetPLCMasterValues", ex);
            }
        }

        public void SetSerialCommunicationSetting()
        {
            try
            {
                Port5.Text = serialComSettings.Port;
                BaudRate.Text = serialComSettings.BaudRate.ToString();
                Parity.Text = serialComSettings.Parity.ToString();
                StopBit.Text = serialComSettings.StopBit.ToString();
                DataBits.Text = serialComSettings.DataBits.ToString();
                DataLength.Text = serialComSettings.DataLength.ToString();
                ImmediateDelay.Text = serialComSettings.ImmediateDelay.ToString();
                StartChar.Text = serialComSettings.StartChar;
                EndChar.Text = serialComSettings.EndChar;
                EnableSerial.IsChecked = serialComSettings.Enable;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetSerialCommunicationSetting", ex);
            }
        }

        public void SetAWSConfigurationValue()
        {
            try
            {
                TimeOut.Text = awsConfigurations.SequenceTimeOut.ToString();
                SAP.IsChecked = awsConfigurations.SAP;
                SemiAutomatic.IsChecked = awsConfigurations.SemiAutomatic;
                VPS.IsChecked = awsConfigurations.VPSEnable;
                IsAutoLogin.IsChecked = awsConfigurations.IsAutoLogin;
                AutoLoginUserComboBox.Text = awsConfigurations.AutoLoginUser;
                FTPrintEnable.IsChecked = awsConfigurations.FTPrintEnable;
                STPrintEnable.IsChecked = awsConfigurations.STPrintEnable;
                AutoGateExitEnable.IsChecked = awsConfigurations.AutoGateExit;
                SGPrintEnable.IsChecked = awsConfigurations.SGPrintEnable;
                if ((bool)awsConfigurations.IsAutoLogin)
                {
                    AutoLoginUserComboBox.Visibility = Visibility.Visible;
                    AutoLoginUserImage.Visibility = Visibility.Visible;
                    AutoLoginUserText.Visibility = Visibility.Visible;
                }
                WeightCommunication.Text = awsConfigurations.WeightCommunication;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetAWSConfigurationValue", ex);
            }
        }

        //public void GetAWSConfiguration()
        //{
        //    try
        //    {
        //        awsConfiguration.SequenceTimeOut = int.Parse(TimeOut.Text);
        //        awsConfiguration.SAP = SAP.IsChecked;
        //        awsConfiguration.SemiAutomatic = SemiAutomatic.IsChecked;


        //        awsConfiguration.VPSEnable = VPS.IsChecked;
        //        awsConfiguration.IsAutoLogin = IsAutoLogin.IsChecked;
        //        if (AutoLoginUserComboBox.Text == "" || AutoLoginUserComboBox.Text == null)
        //        {
        //            awsConfiguration.AutoLoginUser = "";
        //        }
        //        else
        //        {
        //            awsConfiguration.AutoLoginUser = AutoLoginUserComboBox.Text;
        //        }
        //        if (WeightCommunication.Text == "" || WeightCommunication.Text == null)
        //        {
        //            awsConfiguration.WeightCommunication = "";
        //        }
        //        else
        //        {
        //            awsConfiguration.WeightCommunication = WeightCommunication.Text;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog.WriteToFile("GetAWSConfiguration", ex);
        //    }
        //}

        public void ResetPLCMasterValues()
        {
            try
            {
                pLCMaster = new PLCMaster();
                //pLCMaster.Tag = "3";
                IP4.Text = "";
                Port4.Text = "";
                PlcEnable.IsChecked = false;
                //AntennaID3.Text = "";
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("ResetPLCMasterValues", ex);
            }
        }


        public void CreateRFIDReaderMaster(RFIDReaderMaster readerMaster)
        {
            try
            {
                string insertQuery = $@"INSERT INTO [RFID_Reader_Master] (Reader,IP,Port,Location,IsEnable,GateExitEnable,HardwareProfile) 
                                                Values (@Reader,@IP,@Port,@Location,@IsEnable,@GateExitEnable,@HardwareProfile)";

                SqlCommand cmd = new SqlCommand(insertQuery);
                cmd = AddParameters(cmd, readerMaster);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Record created successfully");


            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateRFIDAllocation", ex);
            }
        }

        public void UpdateRFIDReaderMaster(RFIDReaderMaster readerMaster)
        {
            try
            {
                string updateQuery = $@"UPDATE [RFID_Reader_Master] SET IP=@IP,Port=@Port,Location=@Location,IsEnable=@IsEnable,GateExitEnable=@GateExitEnable WHERE Reader='{readerMaster.Reader}' and HardwareProfile='{readerMaster.HardwareProfile}'";

                SqlCommand cmd = new SqlCommand(updateQuery);
                cmd.Parameters.AddWithValue("@IP", readerMaster.IP);
                cmd.Parameters.AddWithValue("@Port", readerMaster.Port);
                cmd.Parameters.AddWithValue("@Location", readerMaster.Location);
                cmd.Parameters.AddWithValue("@IsEnable", readerMaster.IsEnable);
                cmd.Parameters.AddWithValue("@GateExitEnable", readerMaster.GateExitEnable);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Record updated successfully");
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdateRFIDAllocation", ex);
            }
        }

        public void CreateSerialCommunicationSettings(SerialCommunicationSetting serialCommunicationSetting)
        {
            try
            {
                string insertQuery = $@"INSERT INTO [Serial_COM_Setting] (Port,BaudRate,Parity,StopBit,DataBits,DataLength,ImmediateDelay,StartChar,EndChar,Enable,HardwareProfile) 
                                                Values (@Port,@BaudRate,@Parity,@StopBit,@DataBits,@DataLength,@ImmediateDelay,@StartChar,@EndChar,@Enable,@HardwareProfile)";

                SqlCommand cmd = new SqlCommand(insertQuery);
                cmd.Parameters.AddWithValue("@Port", serialCommunicationSetting.Port);
                cmd.Parameters.AddWithValue("@BaudRate", serialCommunicationSetting.BaudRate);
                cmd.Parameters.AddWithValue("@Parity", serialCommunicationSetting.Parity);
                cmd.Parameters.AddWithValue("@StopBit", serialCommunicationSetting.StopBit);
                cmd.Parameters.AddWithValue("@DataBits", serialCommunicationSetting.DataBits);
                cmd.Parameters.AddWithValue("@DataLength", serialCommunicationSetting.DataLength);
                cmd.Parameters.AddWithValue("@ImmediateDelay", serialCommunicationSetting.ImmediateDelay);
                cmd.Parameters.AddWithValue("@StartChar", serialCommunicationSetting.StartChar);
                cmd.Parameters.AddWithValue("@EndChar", serialCommunicationSetting.EndChar);
                cmd.Parameters.AddWithValue("@Enable", serialCommunicationSetting.Enable);
                cmd.Parameters.AddWithValue("@HardwareProfile", serialCommunicationSetting.HardwareProfile);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Record created successfully");


            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateRFIDAllocation", ex);
            }
        }

        public void UpdateSerialCommunicationSettings(SerialCommunicationSetting serialCommunicationSetting)
        {
            try
            {
                string updateQuery = $@"UPDATE [Serial_COM_Setting] SET Port=@Port,BaudRate=@BaudRate,Parity=@Parity,StopBit=@StopBit,DataBits=@DataBits,
                                      DataLength=@DataLength,ImmediateDelay=@ImmediateDelay,StartChar=@StartChar,EndChar=@EndChar,Enable=@Enable WHERE Id=@Id and HardwareProfile=@HardwareProfile";

                SqlCommand cmd = new SqlCommand(updateQuery);
                cmd.Parameters.AddWithValue("@Id", serialComSettings.Id);
                cmd.Parameters.AddWithValue("@HardwareProfile", serialCommunicationSetting.HardwareProfile);
                cmd = AddParametersForSerialComSetting(cmd, serialCommunicationSetting);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Record updated successfully");
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdateSerialCommunicationSettings", ex);
            }
        }

        public SqlCommand AddParameters(SqlCommand cmd, RFIDReaderMaster readerMaster)
        {
            cmd.Parameters.AddWithValue("@Reader", readerMaster.Reader);
            cmd.Parameters.AddWithValue("@IP", readerMaster.IP);
            cmd.Parameters.AddWithValue("@Port", readerMaster.Port);
            cmd.Parameters.AddWithValue("@Location", readerMaster.Location);
            cmd.Parameters.AddWithValue("@IsEnable", readerMaster.IsEnable);
            cmd.Parameters.AddWithValue("@GateExitEnable", readerMaster.GateExitEnable);
            cmd.Parameters.AddWithValue("@HardwareProfile", readerMaster.HardwareProfile);

            return cmd;
        }

        public SqlCommand AddParametersForSerialComSetting(SqlCommand cmd, SerialCommunicationSetting serialCommunicationSetting)
        {
            cmd.Parameters.AddWithValue("@Port", serialCommunicationSetting.Port);
            cmd.Parameters.AddWithValue("@BaudRate", serialCommunicationSetting.BaudRate);
            cmd.Parameters.AddWithValue("@Parity", serialCommunicationSetting.Parity);
            cmd.Parameters.AddWithValue("@StopBit", serialCommunicationSetting.StopBit);
            cmd.Parameters.AddWithValue("@DataBits", serialCommunicationSetting.DataBits);
            cmd.Parameters.AddWithValue("@DataLength", serialCommunicationSetting.DataLength);
            cmd.Parameters.AddWithValue("@ImmediateDelay", serialCommunicationSetting.ImmediateDelay);
            cmd.Parameters.AddWithValue("@StartChar", serialCommunicationSetting.StartChar);
            cmd.Parameters.AddWithValue("@EndChar", serialCommunicationSetting.EndChar);
            cmd.Parameters.AddWithValue("@Enable", serialCommunicationSetting.Enable);
            return cmd;
        }

        public void CreatePLCMaster(PLCMaster readerMaster)
        {
            try
            {
                string insertQuery = $@"INSERT INTO [PLC_Settings] (IP,Port,HardwareProfile,IsEnable) 
                                                Values (@IP,@Port,@HardwareProfile,@IsEnable)";

                SqlCommand cmd = new SqlCommand(insertQuery);
                cmd = AddPLCParameters(cmd, readerMaster);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Record created successfully");
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreatePLCMaster", ex);
            }
        }

        public void CreateAWSConfiguration(AWSConfiguration awsConfiguration)
        {
            try
            {
                string insertQuery = $@"INSERT INTO [AWS_Configurations] (SequenceTimeOut,SAP,SemiAutomatic,VPSEnable,IsAutoLogin,AutoLoginUser,WeightCommunication,HardwareProfile,SGPrintEnable,FTPrintEnable,STPrintEnable,AutoGateExit) 
                                                Values (@SequenceTimeOut,@SAP,@SemiAutomatic,@VPSEnable,@IsAutoLogin,@AutoLoginUser,@WeightCommunication,@HardwareProfile,@SGPrintEnable,@FTPrintEnable,@STPrintEnable,@AutoGateExit)";

                SqlCommand cmd = new SqlCommand(insertQuery);
                cmd.Parameters.AddWithValue("@SequenceTimeOut", awsConfiguration.SequenceTimeOut);
                cmd.Parameters.AddWithValue("@SAP", awsConfiguration.SAP);
                cmd.Parameters.AddWithValue("@SemiAutomatic", awsConfiguration.SemiAutomatic);
                cmd.Parameters.AddWithValue("@VPSEnable", awsConfiguration.VPSEnable);
                cmd.Parameters.AddWithValue("@IsAutoLogin", awsConfiguration.IsAutoLogin);
                cmd.Parameters.AddWithValue("@AutoLoginUser", awsConfiguration.AutoLoginUser);
                cmd.Parameters.AddWithValue("@WeightCommunication", awsConfiguration.WeightCommunication);
                cmd.Parameters.AddWithValue("@HardwareProfile", awsConfiguration.HardwareProfile);
                cmd.Parameters.AddWithValue("@SGPrintEnable", awsConfiguration.SGPrintEnable ?? false);
                cmd.Parameters.AddWithValue("@FTPrintEnable", awsConfiguration.FTPrintEnable ?? false);
                cmd.Parameters.AddWithValue("@STPrintEnable", awsConfiguration.STPrintEnable ?? false);
                cmd.Parameters.AddWithValue("@AutoGateExit", awsConfiguration.AutoGateExit ?? false);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Record created successfully");
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreatePLCMaster", ex);
            }
        }

        public void UpdatePLCMaster(PLCMaster readerMaster)
        {
            try
            {
                string updateQuery = $@"UPDATE [PLC_Settings] SET IP=@IP,Port=@Port,IsEnable=@IsEnable WHERE HardwareProfile='{readerMaster.HardwareProfile}'";

                SqlCommand cmd = new SqlCommand(updateQuery);
                cmd.Parameters.AddWithValue("@IP", readerMaster.IP);
                cmd.Parameters.AddWithValue("@Port", readerMaster.Port);
                cmd.Parameters.AddWithValue("@IsEnable", readerMaster.IsEnable);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Record updated successfully");
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdatePLCMaster", ex);
            }
        }

        public void UpdateAWSConfiguration(AWSConfiguration awsConfiguration)
        {
            try
            {
                string updateQuery = $@"UPDATE [AWS_Configurations] SET SequenceTimeOut=@SequenceTimeOut,SAP=@SAP,SemiAutomatic=@SemiAutomatic,VPSEnable=@VPSEnable,IsAutoLogin=@IsAutoLogin,
                                     AutoLoginUser=@AutoLoginUser,WeightCommunication=@WeightCommunication,FTPrintEnable=@FTPrintEnable,STPrintEnable=@STPrintEnable,AutoGateExit=@AutoGateExit,SGPrintEnable=@SGPrintEnable WHERE Id=@Id and HardwareProfile=@HardwareProfile";

                SqlCommand cmd = new SqlCommand(updateQuery);
                cmd.Parameters.AddWithValue("@Id", awsConfigurations.Id);
                cmd.Parameters.AddWithValue("@HardwareProfile", awsConfiguration.HardwareProfile);
                cmd = AddAWSConfigurationParameters(cmd, awsConfiguration);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Record updated successfully");
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdatePLCMaster", ex);
            }
        }

        public SqlCommand AddPLCParameters(SqlCommand cmd, PLCMaster readerMaster)
        {
            cmd.Parameters.AddWithValue("@IP", readerMaster.IP);
            cmd.Parameters.AddWithValue("@Port", readerMaster.Port);
            cmd.Parameters.AddWithValue("@HardwareProfile", readerMaster.HardwareProfile);
            cmd.Parameters.AddWithValue("@IsEnable", readerMaster.IsEnable);
            return cmd;
        }

        public SqlCommand AddAWSConfigurationParameters(SqlCommand cmd, AWSConfiguration awsConfiguration)
        {
            cmd.Parameters.AddWithValue("@SequenceTimeOut", awsConfiguration.SequenceTimeOut);
            cmd.Parameters.AddWithValue("@SAP", awsConfiguration.SAP);
            cmd.Parameters.AddWithValue("@SemiAutomatic", awsConfiguration.SemiAutomatic);
            cmd.Parameters.AddWithValue("@VPSEnable", awsConfiguration.VPSEnable);
            cmd.Parameters.AddWithValue("@IsAutoLogin", awsConfiguration.IsAutoLogin);
            cmd.Parameters.AddWithValue("@AutoLoginUser", awsConfiguration.AutoLoginUser);
            cmd.Parameters.AddWithValue("@WeightCommunication", awsConfiguration.WeightCommunication);
            cmd.Parameters.AddWithValue("@FTPrintEnable", awsConfiguration.FTPrintEnable ?? false);
            cmd.Parameters.AddWithValue("@STPrintEnable", awsConfiguration.STPrintEnable ?? false);
            cmd.Parameters.AddWithValue("@AutoGateExit", awsConfiguration.AutoGateExit ?? false);
            cmd.Parameters.AddWithValue("@SGPrintEnable", awsConfiguration.SGPrintEnable ?? false);
            return cmd;
        }

        private void SaveBtn4_Click(object sender, RoutedEventArgs e)
        {
            PLCMaster pLCMaster = new PLCMaster();
            pLCMaster.IP = IP4.Text;
            pLCMaster.Port = int.Parse(Port4.Text);
            pLCMaster.HardwareProfile = hardwareProfile;
            pLCMaster.IsEnable = (bool)PlcEnable.IsChecked;
            DataTable dt1 = _dbContext.GetAllData($"SELECT * FROM PLC_Settings where HardwareProfile='{pLCMaster.HardwareProfile}'");
            if (pLCMaster.Port != null || pLCMaster.Port > 0)
            {
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    UpdatePLCMaster(pLCMaster);
                }
                else
                {
                    CreatePLCMaster(pLCMaster);
                }
            }
            onCompletion.Invoke(this, new GeneralEventHandler());
        }

        private void SaveBtn5_Click(object sender, RoutedEventArgs e)
        {
            AWSConfiguration awsConfiguration = new AWSConfiguration();
            awsConfiguration.SequenceTimeOut = int.Parse(TimeOut.Text);
            awsConfiguration.SAP = SAP.IsChecked;
            awsConfiguration.SemiAutomatic = SemiAutomatic.IsChecked;
            awsConfiguration.VPSEnable = VPS.IsChecked;
            awsConfiguration.IsAutoLogin = IsAutoLogin.IsChecked;
            awsConfiguration.SGPrintEnable = SGPrintEnable.IsChecked;
            awsConfiguration.FTPrintEnable = FTPrintEnable.IsChecked;
            awsConfiguration.STPrintEnable = STPrintEnable.IsChecked;
            awsConfiguration.AutoGateExit = AutoGateExitEnable.IsChecked;
            if (AutoLoginUserComboBox.Text == "" || AutoLoginUserComboBox.Text == null)
            {
                awsConfiguration.AutoLoginUser = "";
            }
            else
            {
                awsConfiguration.AutoLoginUser = AutoLoginUserComboBox.Text;
            }
            if (WeightCommunication.Text == "" || WeightCommunication.Text == null)
            {
                awsConfiguration.WeightCommunication = "";
            }
            else
            {
                awsConfiguration.WeightCommunication = WeightCommunication.Text;
            }
            awsConfiguration.HardwareProfile = hardwareProfile;
            DataTable dt1 = _dbContext.GetAllData($"SELECT * FROM AWS_Configurations where HardwareProfile='{awsConfiguration.HardwareProfile}'");
            if (awsConfiguration != null)
            {
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    UpdateAWSConfiguration(awsConfiguration);
                }
                else
                {
                    CreateAWSConfiguration(awsConfiguration);
                }
            }
            onCompletion.Invoke(this, new GeneralEventHandler());
        }

        private void Page_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }
        private void Constant_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
