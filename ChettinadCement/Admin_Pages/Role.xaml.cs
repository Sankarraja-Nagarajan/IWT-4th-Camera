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

namespace IWT.Admin_Pages
{
    /// <summary>
    /// Interaction logic for Role.xaml
    /// </summary>
    public partial class Role : Page
    {
        string LastMessage;
        List<RolePriviliege> rolePrivilieges = new List<RolePriviliege>();
        RolePriviliege SelectedRolePriviliege = new RolePriviliege();
        AdminDBCall adminDBCall = new AdminDBCall();
        MasterDBCall masterDBCall = new MasterDBCall();
        private readonly ToastViewModel toastViewModel;
        public Role()
        {
            toastViewModel = new ToastViewModel();
            InitializeComponent();
            GetRolesAndPreviledges();
            //MaterialGrid4.ItemsSource = LoadCollectionData();
            //MaterialGrid3.ItemsSource = LoadCollectionData();
            //MaterialGrid2.ItemsSource = LoadCollectionData();
            //MaterialGrid1.ItemsSource = LoadCollectionData();
            //MaterialGrid.ItemsSource = LoadCollectionData();
        }
        //public void ShowMessage(Action<string> message, string name)
        //{
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        LastMessage = $"{name}";
        //        message(LastMessage);
        //    });
        //}

        public void GetRolesAndPreviledges()
        {
            try
            {

                DataTable dt1 = adminDBCall.GetAllData("SELECT * FROM User_Previledges");

                string JSONString = JsonConvert.SerializeObject(dt1);
                rolePrivilieges = JsonConvert.DeserializeObject<List<RolePriviliege>>(JSONString);
                MaterialGrid4.ItemsSource = rolePrivilieges;
                MaterialGrid4.Items.Refresh();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetRolesAndPreviledges", ex);
            }

        }

        private void MaterialGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                SelectedRolePriviliege = MaterialGrid4.SelectedItem as RolePriviliege;
                if (SelectedRolePriviliege != null)
                {
                    Name6.Text = SelectedRolePriviliege.Role;

                    Admin.IsChecked = SelectedRolePriviliege.AdminAccess;
                    Transaction.IsChecked = SelectedRolePriviliege.TransactionAccess;
                    Report.IsChecked = SelectedRolePriviliege.ReportAccess;
                    Master.IsChecked = SelectedRolePriviliege.MasterAccess;
                    Setting.IsChecked = SelectedRolePriviliege.SettingAccess;
                    GateEntry.IsChecked = SelectedRolePriviliege.RFIDAllocationAccess;
                    AWSTransactions.IsChecked = SelectedRolePriviliege.RFIDUserTableAccess;
                    GateExit.IsChecked = SelectedRolePriviliege.GateExitAccess;
                    SystemConfig.IsChecked = SelectedRolePriviliege.SystemConfigurationAccess;
                    SapSync.IsChecked = SelectedRolePriviliege.SAPSyncAccess;
                    PrintDelete.IsChecked = SelectedRolePriviliege.PrintAndDeleteAccess;

                    SoftWareConfiguration.IsChecked = SelectedRolePriviliege.SoftwareConfigurationAccess;
                    EditHardware.IsChecked = SelectedRolePriviliege.EditHardwareProfile;
                    dbpassword.IsChecked = SelectedRolePriviliege.DBPswdChangeAccess;
                    smtp.IsChecked = SelectedRolePriviliege.SMTPAccess;
                    weight.IsChecked = SelectedRolePriviliege.WeighBridgeSetting;
                    cctv.IsChecked = SelectedRolePriviliege.CCTVSettings;
                    email.IsChecked = SelectedRolePriviliege.EmailSettingsAccess;
                    summary.IsChecked = SelectedRolePriviliege.SummaryReportAccess;
                    file.IsChecked = SelectedRolePriviliege.FileLocationSettingsAccess;
                    others.IsChecked = SelectedRolePriviliege.OtherSettingsAccess;
                    import.IsChecked = SelectedRolePriviliege.ImportExportAccess;
                    sms_setting.IsChecked = SelectedRolePriviliege.SMSAdminAccess;
                    aws_setting.IsChecked = SelectedRolePriviliege.AWSAccess;

                    usermanage.IsChecked = SelectedRolePriviliege.UserManagementAccess;
                    customefield.IsChecked = SelectedRolePriviliege.CustomFieldAccess;
                    ticketDataEntry.IsChecked = SelectedRolePriviliege.TicketDataEntryAccess;
                    duplicateticket.IsChecked = SelectedRolePriviliege.DuplicateTicketsAccess;
                    deleterecord.IsChecked = SelectedRolePriviliege.RecordDeletionAccess;

                    vehicle.IsChecked = SelectedRolePriviliege.VehicleMasterAccess;
                    material.IsChecked = SelectedRolePriviliege.MaterialMasterAccess;
                    supplier.IsChecked = SelectedRolePriviliege.SupllierMasterAccess;
                    shift.IsChecked = SelectedRolePriviliege.ShiftMasterAccess;
                    rfid.IsChecked = SelectedRolePriviliege.RFIDMasterAccess;
                    customMaster.IsChecked = SelectedRolePriviliege.CustomMasterAccess;
                    Store.IsChecked = SelectedRolePriviliege.StoreAccess;
                    CloseTicket.IsChecked = SelectedRolePriviliege.CloseTickets;
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/MaterialGrid_SelectionChanged", ex);
            }
        }


        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem ti = Tabcontrol.SelectedItem as TabItem;
            string selectedtab = ti.Header.ToString();
        }

        #region General
        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            ClearField();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Name6.Text))
            {
                GetFieldsValue();
                if (SelectedRolePriviliege != null && SelectedRolePriviliege.Id != 0)
                {
                    UpdateRolePrivilages();
                }
                else
                {
                    InsertRolePrivilages();
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please enter the role name");
            }
        }

        public void InsertRolePrivilages()
        {
            try
            {
                string Query = "INSERT INTO [User_Previledges] (Role,TransactionAccess,MasterAccess,ReportAccess,AdminAccess,SettingAccess," +
                    "SoftwareConfigurationAccess,EditHardwareProfile,DBPswdChangeAccess,SMTPAccess,WeighBridgeSetting,CCTVSettings," +
                    "EmailSettingsAccess,SummaryReportAccess,FileLocationSettingsAccess,OtherSettingsAccess,ImportExportAccess,SMSAdminAccess," +
                    "UserManagementAccess,CustomFieldAccess,TicketDataEntryAccess,DuplicateTicketsAccess,RecordDeletionAccess," +
                    "VehicleMasterAccess,MaterialMasterAccess,SupllierMasterAccess,ShiftMasterAccess,RFIDMasterAccess,CustomMasterAccess,RFIDAllocationAccess,RFIDUserTableAccess,AWSAccess,GateExitAccess," +
                    "CloseTickets,StoreAccess,SystemConfigurationAccess,SAPSyncAccess,PrintAndDeleteAccess) " +

                    "VALUES (@Role,@TransactionAccess,@MasterAccess,@ReportAccess,@AdminAccess,@SettingAccess," +
                    "@SoftwareConfigurationAccess,@EditHardwareProfile,@DBPswdChangeAccess,@SMTPAccess,@WeighBridgeSetting,@CCTVSettings," +
                    "@EmailSettingsAccess,@SummaryReportAccess,@FileLocationSettingsAccess,@OtherSettingsAccess,@ImportExportAccess,@SMSAdminAccess," +
                    "@UserManagementAccess,@CustomFieldAccess,@TicketDataEntryAccess,@DuplicateTicketsAccess,@RecordDeletionAccess," +
                    "@VehicleMasterAccess,@MaterialMasterAccess,@SupllierMasterAccess,@ShiftMasterAccess,@RFIDMasterAccess,@CustomMasterAccess,@RFIDAllocationAccess,@RFIDUserTableAccess,@AWSAccess,@GateExitAccess," +
                    "@CloseTickets,@StoreAccess,@SystemConfigurationAccess,@SAPSyncAccess,@PrintAndDeleteAccess)";

                SqlCommand cmd = new SqlCommand(Query);
                cmd = AddParameters(cmd);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Role created successfully");
                ClearField();
                GetRolesAndPreviledges();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/InsertRolePrivilages", ex);
            }

        }

        public SqlCommand AddParameters(SqlCommand cmd)
        {
            cmd.Parameters.Add("@Role", SqlDbType.VarChar).Value = SelectedRolePriviliege.Role;
            cmd.Parameters.AddWithValue("@TransactionAccess", SelectedRolePriviliege.TransactionAccess);
            cmd.Parameters.AddWithValue("@MasterAccess", SelectedRolePriviliege.MasterAccess);
            cmd.Parameters.AddWithValue("@ReportAccess", SelectedRolePriviliege.ReportAccess);
            cmd.Parameters.AddWithValue("@AdminAccess", SelectedRolePriviliege.AdminAccess);
            cmd.Parameters.AddWithValue("@SettingAccess", SelectedRolePriviliege.SettingAccess);
            cmd.Parameters.AddWithValue("@RFIDAllocationAccess", SelectedRolePriviliege.RFIDAllocationAccess);
            cmd.Parameters.AddWithValue("@RFIDUserTableAccess", SelectedRolePriviliege.RFIDUserTableAccess);
            cmd.Parameters.AddWithValue("@GateExitAccess", SelectedRolePriviliege.GateExitAccess);
            cmd.Parameters.AddWithValue("@SystemConfigurationAccess", SelectedRolePriviliege.SystemConfigurationAccess);
            cmd.Parameters.AddWithValue("@SAPSyncAccess", SelectedRolePriviliege.SAPSyncAccess);

            cmd.Parameters.AddWithValue("@SoftwareConfigurationAccess", SelectedRolePriviliege.SoftwareConfigurationAccess);
            cmd.Parameters.AddWithValue("@EditHardwareProfile", SelectedRolePriviliege.EditHardwareProfile);
            cmd.Parameters.AddWithValue("@DBPswdChangeAccess", SelectedRolePriviliege.DBPswdChangeAccess);
            cmd.Parameters.AddWithValue("@SMTPAccess", SelectedRolePriviliege.SMTPAccess);
            cmd.Parameters.AddWithValue("@WeighBridgeSetting", SelectedRolePriviliege.WeighBridgeSetting);
            cmd.Parameters.AddWithValue("@CCTVSettings", SelectedRolePriviliege.CCTVSettings);
            cmd.Parameters.AddWithValue("@EmailSettingsAccess", SelectedRolePriviliege.EmailSettingsAccess);
            cmd.Parameters.AddWithValue("@SummaryReportAccess", SelectedRolePriviliege.SummaryReportAccess);
            cmd.Parameters.AddWithValue("@FileLocationSettingsAccess", SelectedRolePriviliege.FileLocationSettingsAccess);
            cmd.Parameters.AddWithValue("@OtherSettingsAccess", SelectedRolePriviliege.OtherSettingsAccess);
            cmd.Parameters.AddWithValue("@ImportExportAccess", SelectedRolePriviliege.ImportExportAccess);
            cmd.Parameters.AddWithValue("@SMSAdminAccess", SelectedRolePriviliege.SMSAdminAccess);
            cmd.Parameters.AddWithValue("@AWSAccess", SelectedRolePriviliege.AWSAccess);

            cmd.Parameters.AddWithValue("@UserManagementAccess", SelectedRolePriviliege.UserManagementAccess);
            cmd.Parameters.AddWithValue("@CustomFieldAccess", SelectedRolePriviliege.CustomFieldAccess) ;
            cmd.Parameters.AddWithValue("@TicketDataEntryAccess", SelectedRolePriviliege.TicketDataEntryAccess);
            cmd.Parameters.AddWithValue("@DuplicateTicketsAccess", SelectedRolePriviliege.DuplicateTicketsAccess);
            cmd.Parameters.AddWithValue("@RecordDeletionAccess", SelectedRolePriviliege.RecordDeletionAccess);

            cmd.Parameters.AddWithValue("@VehicleMasterAccess", SelectedRolePriviliege.VehicleMasterAccess);
            cmd.Parameters.AddWithValue("@MaterialMasterAccess", SelectedRolePriviliege.MaterialMasterAccess);
            cmd.Parameters.AddWithValue("@SupllierMasterAccess", SelectedRolePriviliege.SupllierMasterAccess);
            cmd.Parameters.AddWithValue("@ShiftMasterAccess", SelectedRolePriviliege.ShiftMasterAccess);
            cmd.Parameters.AddWithValue("@RFIDMasterAccess", SelectedRolePriviliege.RFIDMasterAccess);
            cmd.Parameters.AddWithValue("@CustomMasterAccess", SelectedRolePriviliege.CustomMasterAccess);
            cmd.Parameters.AddWithValue("@StoreAccess", SelectedRolePriviliege.StoreAccess);
            cmd.Parameters.AddWithValue("@PrintAndDeleteAccess", SelectedRolePriviliege.PrintAndDeleteAccess);

            cmd.Parameters.AddWithValue("@CloseTickets", SelectedRolePriviliege.CloseTickets);

            return cmd;
        }

        public void UpdateRolePrivilages()
        {
            try
            {
                string Query = "UPDATE [User_Previledges] SET Role=@Role,TransactionAccess=@TransactionAccess,MasterAccess=@MasterAccess,ReportAccess=@ReportAccess," +
                    "AdminAccess=@AdminAccess,SettingAccess=@SettingAccess," +
                    "SoftwareConfigurationAccess=@SoftwareConfigurationAccess,EditHardwareProfile=@EditHardwareProfile,DBPswdChangeAccess=@DBPswdChangeAccess," +
                    "SMTPAccess=@SMTPAccess,WeighBridgeSetting=@WeighBridgeSetting,CCTVSettings=@CCTVSettings," +
                    "EmailSettingsAccess=@EmailSettingsAccess,SummaryReportAccess=@SummaryReportAccess,FileLocationSettingsAccess=@FileLocationSettingsAccess," +
                    "OtherSettingsAccess=@OtherSettingsAccess,ImportExportAccess=@ImportExportAccess,SMSAdminAccess=@SMSAdminAccess," +
                    "UserManagementAccess=@UserManagementAccess,CustomFieldAccess=@CustomFieldAccess,TicketDataEntryAccess=@TicketDataEntryAccess,DuplicateTicketsAccess=@DuplicateTicketsAccess,RecordDeletionAccess=@RecordDeletionAccess," +
                    "VehicleMasterAccess=@VehicleMasterAccess,MaterialMasterAccess=@MaterialMasterAccess,SupllierMasterAccess=@SupllierMasterAccess," +
                    "ShiftMasterAccess=@ShiftMasterAccess,RFIDMasterAccess=@RFIDMasterAccess,CustomMasterAccess=@CustomMasterAccess,RFIDAllocationAccess=@RFIDAllocationAccess,RFIDUserTableAccess=@RFIDUserTableAccess,AWSAccess=@AWSAccess,GateExitAccess=@GateExitAccess," +
                    "CloseTickets=@CloseTickets,StoreAccess=@StoreAccess,SystemConfigurationAccess=@SystemConfigurationAccess,SAPSyncAccess=@SAPSyncAccess,PrintAndDeleteAccess=@PrintAndDeleteAccess WHERE Id=@Id"; ;

                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@Id", SelectedRolePriviliege.Id);
                cmd = AddParameters(cmd);
                masterDBCall.InsertData(cmd, CommandType.Text);

                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Role updated successfully");
                ClearField();
                GetRolesAndPreviledges();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/UpdateRolePrivilages", ex);

            }

        }

        public void GetFieldsValue()
        {
            try
            {
                if (SelectedRolePriviliege == null)
                {
                    SelectedRolePriviliege = new RolePriviliege();
                }
                SelectedRolePriviliege.Role = Name6.Text;

                SelectedRolePriviliege.AdminAccess = Admin.IsChecked;
                SelectedRolePriviliege.MasterAccess = Master.IsChecked;
                SelectedRolePriviliege.TransactionAccess = Transaction.IsChecked;
                SelectedRolePriviliege.SettingAccess = Setting.IsChecked;
                SelectedRolePriviliege.ReportAccess = Report.IsChecked;
                SelectedRolePriviliege.RFIDAllocationAccess = GateEntry.IsChecked;
                SelectedRolePriviliege.RFIDUserTableAccess = AWSTransactions.IsChecked;
                SelectedRolePriviliege.GateExitAccess = GateExit.IsChecked;
                SelectedRolePriviliege.SystemConfigurationAccess = SystemConfig.IsChecked;
                SelectedRolePriviliege.SAPSyncAccess = SapSync.IsChecked;
                SelectedRolePriviliege.PrintAndDeleteAccess = PrintDelete.IsChecked;

                SelectedRolePriviliege.SoftwareConfigurationAccess = SoftWareConfiguration.IsChecked;
                SelectedRolePriviliege.EditHardwareProfile = EditHardware.IsChecked;
                SelectedRolePriviliege.DBPswdChangeAccess = dbpassword.IsChecked;
                SelectedRolePriviliege.SMTPAccess = smtp.IsChecked;
                SelectedRolePriviliege.WeighBridgeSetting = weight.IsChecked;
                SelectedRolePriviliege.CCTVSettings = cctv.IsChecked;
                SelectedRolePriviliege.EmailSettingsAccess = email.IsChecked;
                SelectedRolePriviliege.SummaryReportAccess = summary.IsChecked;
                SelectedRolePriviliege.FileLocationSettingsAccess = file.IsChecked;
                SelectedRolePriviliege.OtherSettingsAccess = others.IsChecked;
                SelectedRolePriviliege.ImportExportAccess = import.IsChecked;
                SelectedRolePriviliege.SMSAdminAccess = sms_setting.IsChecked;
                SelectedRolePriviliege.AWSAccess = aws_setting.IsChecked;

                SelectedRolePriviliege.UserManagementAccess = usermanage.IsChecked;
                SelectedRolePriviliege.CustomFieldAccess = customefield.IsChecked;
                SelectedRolePriviliege.TicketDataEntryAccess = ticketDataEntry.IsChecked;
                SelectedRolePriviliege.DuplicateTicketsAccess = duplicateticket.IsChecked;
                SelectedRolePriviliege.RecordDeletionAccess = deleterecord.IsChecked;
                SelectedRolePriviliege.MaterialMasterAccess = material.IsChecked;
                SelectedRolePriviliege.SupllierMasterAccess = supplier.IsChecked;
                SelectedRolePriviliege.VehicleMasterAccess = vehicle.IsChecked;
                SelectedRolePriviliege.ShiftMasterAccess = shift.IsChecked;
                SelectedRolePriviliege.RFIDMasterAccess = rfid.IsChecked;
                SelectedRolePriviliege.CustomMasterAccess = customMaster.IsChecked;
                SelectedRolePriviliege.CloseTickets = CloseTicket.IsChecked;


                SelectedRolePriviliege.GSMModemSettingAccess = false;
                SelectedRolePriviliege.RFIDPortSettingAccess = false;
                SelectedRolePriviliege.PLCEnableAccess = false;
                SelectedRolePriviliege.ALPRSettingAccess = false;
                SelectedRolePriviliege.PLCPortSettingAccess = false;
                SelectedRolePriviliege.DongleSettingAccess = false;
                SelectedRolePriviliege.RFIDWriterAccess = false;
                SelectedRolePriviliege.RemoteDisplaySettingAccess = false;
                SelectedRolePriviliege.BioMetricSettingsAccess = false;
                SelectedRolePriviliege.HardwareSettingAccess = false;
                SelectedRolePriviliege.ContactsAccess = false;
                SelectedRolePriviliege.DeleteAccess = false;
                SelectedRolePriviliege.BackupDBAccess = false;
                SelectedRolePriviliege.SalesForceSettingsAccess = false;
                SelectedRolePriviliege.DBMigrationAccess = false;
                SelectedRolePriviliege.StoreAccess = Store.IsChecked;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetFieldsValue", ex);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearField();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedRolePriviliege != null && SelectedRolePriviliege.Id != 0)
                {
                    var result = await OpenConfirmationDialog();
                    if (result)
                    {
                        string Query = "DELETE FROM User_Previledges WHERE Id=@Id";
                        SqlCommand cmd = new SqlCommand(Query);
                        cmd.Parameters.AddWithValue("@Id", SelectedRolePriviliege.Id);
                        //cmd.CommandType = CommandType.Text;
                        masterDBCall.InsertData(cmd, CommandType.Text);
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Role deleted successfully");
                        ClearField();
                        GetRolesAndPreviledges();
                    }
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a role to delete");
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/DeleteButton_Click", ex);
            }

        }

        public async Task<bool> OpenConfirmationDialog()
        {
            var view = new ConfirmationDialog($"Delete the role");

            //    //show the dialog
            var result = await DialogHost.Show(view, "ReportDialog", ClosingEventHandler);
            return (bool)result;
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
        }
        private void ClearField()
        {
            MaterialGrid4.SelectedItem = null;
            Name6.Text = "";

            Admin.IsChecked = false;
            Transaction.IsChecked = false;
            Report.IsChecked = false;
            Master.IsChecked = false;
            Setting.IsChecked = false;
            GateEntry.IsChecked = false;
            AWSTransactions.IsChecked = false;
            GateExit.IsChecked = false;
            SystemConfig.IsChecked = false;
            SapSync.IsChecked = false;
            PrintDelete.IsChecked = false;

            SoftWareConfiguration.IsChecked = false;
            EditHardware.IsChecked = false;
            dbpassword.IsChecked = false;
            smtp.IsChecked = false;
            weight.IsChecked = false;
            cctv.IsChecked = false;
            email.IsChecked = false;
            summary.IsChecked = false;
            file.IsChecked = false;
            others.IsChecked = false;
            import.IsChecked = false;
            sms_setting.IsChecked = false;
            aws_setting.IsChecked = false;

            usermanage.IsChecked = false;
            customefield.IsChecked = false;
            ticketDataEntry.IsChecked = false;
            duplicateticket.IsChecked = false;
            deleterecord.IsChecked = false;

            vehicle.IsChecked = false;
            material.IsChecked = false;
            supplier.IsChecked = false;
            shift.IsChecked = false;
            rfid.IsChecked = false;
            customMaster.IsChecked = false;

            CloseTicket.IsChecked = false;
        }
        #endregion

    }
}
