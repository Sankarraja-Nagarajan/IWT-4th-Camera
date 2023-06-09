using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWT.Models
{
    public class MaterialMaster
    {
        public int MaterialID { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class SupplierMaster
    {
        public int SupplierID { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class ShiftMaster
    {
        public int ShiftID { get; set; }
        public string ShiftName { get; set; }
        public string FromShift { get; set; }
        public string ToShift { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class VehicleMaster
    {
        public int VehicleID { get; set; }
        public string VehicleNumber { get; set; }
        public int VehicleTareWeight { get; set; }
        public DateTime? Expiry { get; set; }
        public TimeSpan? TaredTime { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? CreatedOn { get; set; }

    }
    public class CustomMasterList
    {
        public int ID { get; set; }
        public string CutomMasterName { get; set; }
        public string TableImage { get; set; }
        public string TableText { get; set; }
    }
    public class FactorySetup
    {
        public int ID { get; set; }
        public bool FactoryInstall { get; set; }
    }
    public class SoftwareConfigure
    {
        public int ID { get; set; }
        public bool Single_Transaction { get; set; }
        public bool First_Transaction { get; set; }
        public bool Second_Transaction { get; set; }
        public bool First_MultiTransaction { get; set; }
        public bool Second_MultiTransaction { get; set; }
        public bool Single_Axle { get; set; }
        public bool First_Axle { get; set; }
        public bool Second_Axle { get; set; }
        public bool Loading { get; set; }
        public bool Unloading { get; set; }
    }
    public class MailSettings
    {
        public int ID { get; set; }
        public string FromID { get; set; }
        public string ToID { get; set; }
        public bool Enable { get; set; }
        public string Password { get; set; }
        public string CCList { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string UserName { get; set; }
        public string SendType { get; set; }
    }
    public class WeighbridgeSettings
    {
        public int ID { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string HardwareProfile { get; set; }
    }
    public class OtherSettings
    {
        public int ID { get; set; }
        public string DefaultTransaction { get; set; }
        public bool DualScaleSet { get; set; }
        public bool SMSAlerts { get; set; }
        public bool SaveTransTxt { get; set; }
        public int TareExpirePeriod { get; set; }
        public bool AutoPrint { get; set; }
        public int NoOfCopies { get; set; }
        public bool DosPrint { get; set; }
        public string UserName { get; set; }
        public bool AutoMail { get; set; }
        public bool AutoDelete { get; set; }
        public bool AutoFtMail { get; set; }
        public bool AutoFtPrint { get; set; }
        public bool AutoFtSMS { get; set; }
        public string StableSensorConfigForVPS { get; set; }
        public bool AutoPrintPreview { get; set; }
        public string BaseURL { get; set; }
        public string DeviceName { get; set; }
        public string PortNumber { get; set; }
        public string DeviceName_CCID { get; set; }
        public string PortNumber_CCID { get; set; }
        public bool AutoCopies { get; set; }
        public int StableWeightCount { get; set; }
        public int MinimumWeightCount { get; set; }
        public int StablePLCCount { get; set; }
        public int ExpiryDays { get; set; }
        public string HardwareProfile { get; set; }
    }
    public class FileLocation
    {
        public int ID { get; set; }
        public string Log_Path { get; set; }
        public string IReport_Path { get; set; }
        public string IReport { get; set; }
        public string Report_Temp { get; set; }
        public string Transaction_Log { get; set; }
        public string Ticket_Temp { get; set; }
        public string Default_Ticket { get; set; }
        public string Default_Temp { get; set; }
        public string Weigh { get; set; }
        public string WeighIndicator { get; set; }
    }
    public class Mail
    {
        public string To { get; set; }
        public string Cc { get; set; }
        public string MailID { get; set; }
        public string Password { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string SendType { get; set; }

    }

    public interface IInitialization
    {
        void GetInitialData();
    }

    public class RolePriviliege
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public bool? MasterAccess { get; set; }
        public bool? SettingAccess { get; set; }
        public bool? TransactionAccess { get; set; }
        public bool? AdminAccess { get; set; }
        public bool? ReportAccess { get; set; }
        public bool? TicketDataEntryAccess { get; set; }
        public bool? HardwareSettingAccess { get; set; }
        public bool? SoftwareConfigurationAccess { get; set; }
        public bool? EditHardwareProfile { get; set; }
        public bool? ALPRSettingAccess { get; set; }
        public bool? PLCPortSettingAccess { get; set; }
        public bool? DongleSettingAccess { get; set; }
        public bool? DBPswdChangeAccess { get; set; }
        public bool? RFIDWriterAccess { get; set; }
        public bool? RemoteDisplaySettingAccess { get; set; }
        public bool? WeighBridgeSetting { get; set; }
        public bool? BioMetricSettingsAccess { get; set; }
        public bool? CCTVSettings { get; set; }
        public bool? EmailSettingsAccess { get; set; }
        public bool? SMTPAccess { get; set; }
        public bool? SummaryReportAccess { get; set; }
        public bool? SMSAdminAccess { get; set; }
        public bool? GSMModemSettingAccess { get; set; }
        public bool? FileLocationSettingsAccess { get; set; }
        public bool? RFIDPortSettingAccess { get; set; }
        public bool? PLCEnableAccess { get; set; }
        public bool? OtherSettingsAccess { get; set; }
        public bool? ImportExportAccess { get; set; }
        public bool? SendMailAccess { get; set; }
        public bool? RecordDeletionAccess { get; set; }
        public bool? ReachUsAccess { get; set; }
        public bool? ContactsAccess { get; set; }
        public bool? DeleteAccess { get; set; }
        public bool? BackupDBAccess { get; set; }
        public bool? SalesForceSettingsAccess { get; set; }
        public bool? DuplicateTicketsAccess { get; set; }
        public bool? DBMigrationAccess { get; set; }
        public bool? UserManagementAccess { get; set; }
        public bool? CustomFieldAccess { get; set; }
        public bool? MaterialMasterAccess { get; set; }
        public bool? SupllierMasterAccess { get; set; }
        public bool? ShiftMasterAccess { get; set; }
        public bool? VehicleMasterAccess { get; set; }
        public bool? CustomMasterAccess { get; set; }
        public bool? RFIDMasterAccess { get; set; }
        public bool? CloseTickets { get; set; }
        public bool? RFIDAllocationAccess { get; set; }
        public bool? RFIDUserTableAccess { get; set; }
        public bool? GateExitAccess { get; set; }
        public bool? AWSAccess { get; set; }
        public bool? StoreAccess { get; set; }
        public bool? SystemConfigurationAccess { get; set; }
        public bool? SAPSyncAccess { get; set; }
        public bool? PrintAndDeleteAccess { get; set; }
        public bool? TransErrorLogs { get; set; }
    }
    public class RolePriviliege1
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string MasterAccess { get; set; }
        public string SettingAccess { get; set; }
        public string TransactionAccess { get; set; }
        public string Adminaccess { get; set; }
        public string ReportAccess { get; set; }
        public string TicketDataEntryAccess { get; set; }

        public string HardwareSettingAccess { get; set; }
        public string SoftwareConfigurationAccess { get; set; }
        public string EditHardwareProfile { get; set; }
        public string ALPRSettingAccess { get; set; }
        public string PLCPortSettingAccess { get; set; }
        public string DongleSettingAccess { get; set; }
        public string DBPswdChangeAccess { get; set; }
        public string RFIDWriterAccess { get; set; }
        public string RemoteDisplaySettingAccess { get; set; }
        public string WeighBridgeSetting { get; set; }
        public string BioMetricSettingsAccess { get; set; }
        public string CCTVSettings { get; set; }
        public string EmailSettingsAccess { get; set; }
        public string SMTPAccess { get; set; }
        public string SummaryReportAccess { get; set; }
        public string SMSAdminAccess { get; set; }
        public string GSMModemSettingAccess { get; set; }
        public string FileLocationSettingsAccess { get; set; }
        public string RFIDPortSettingAccess { get; set; }
        public string PLCEnableAccess { get; set; }
        public string OtherSettingsAccess { get; set; }
        public string ImportExportAccess { get; set; }
        public string SendMailAccess { get; set; }
        public string RecordDeletionAccess { get; set; }
        public string ReachUsAccess { get; set; }
        public string ContactsAccess { get; set; }
        public string DeleteAccess { get; set; }
        public string BackupDBAccess { get; set; }
        public string SalesForceSettingsAccess { get; set; }
        public string DuplicateTicketsAccess { get; set; }
        public string DBMigrationAccess { get; set; }
        public string UserManagementAccess { get; set; }
        public string CustomFieldAccess { get; set; }
        public string MaterialMasterAccess { get; set; }
        public string SupllierMasterAccess { get; set; }
        public string ShiftMasterAccess { get; set; }
        public string VehicleMasterAccess { get; set; }
        public string RFIDMasterAccess { get; set; }
        public string CloseTickets { get; set; }
    }

    public class UserHardwareProfile
    {
        public int ID { get; set; }
        public string HardwareProfileName { get; set; }
        public bool CameraAccess { get; set; }
        public bool RFIDReader1 { get; set; }
        public bool RFIDReader2 { get; set; }
        public bool RFIDReader3 { get; set; }
        public bool PLC { get; set; }

    }

    public class OracleModel
    {
        public int TransId { get; set; }
        public string RFIDTAGUID { get; set; }
        public string FIRSTWTDT { get; set; }
        public string FIRSTWTTM { get; set; }
        public string FIRSTWT { get; set; }
        public string SECONDWTDT { get; set; }
        public string SECONDWTTM { get; set; }
        public string SECONDWT { get; set; }
        public string NETWT { get; set; }
        public int? WBTOLLMIN { get; set; }
        public int? WBTOLLMAX { get; set; }
        public string WBNO_F { get; set; }
        public string WBNO_S { get; set; }
        public string IMAGENO1 { get; set; }
        public string IMAGENO2 { get; set; }
        public string STATUS_FLAG { get; set; }
        public string GINDT { get; set; }
        public string CFLAG { get; set; }
        public string AFLAG { get; set; }
        public string VEHICLE_NUMBER { get; set; }
        public string MATERIAL_CODE { get; set; }
        public string MATERIAL_DESCRIPTION { get; set; }
        public string SUPPLIER_CODE { get; set; }
        public string SUPPLIER_DESCRIPTION { get; set; }
        public int? PARTYWT { get; set; }
    }

    public class ERPFileLocation
    {
        public int ID { get; set; }
        public bool IsEnabled { get; set; }
        public string ERPFilePath { get; set; }
        public bool IsXML { get; set; }
        public bool IsCSV { get; set; }
    }

    public class StableWeightConfiguration
    {
        public int ID { get; set; }
        public int StableWeightCount { get; set; }
        public int MinimumWeightCount { get; set; }
        public int StablePLCCount { get; set; }
    }

    public class MSG91Master
    {
        public int ID { get; set; }
        public string AuthKey { get; set; }
        public string SenderID { get; set; }
        public string DLT_TE_ID { get; set; }
        public string PE_ID { get; set; }
    }
    public class SendGridMaster
    {
        public int ID { get; set; }
        public string APIKey { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class CloudAppConfig
    {
        public int ID { get; set; }
        public bool IsEnabled { get; set; }
        public string SystemID { get; set; }
        public string WeighBridgeID { get; set; }
        public string BaseURL { get; set; }
    }

    public class Usermanage
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Profile { get; set; }
        public bool? Locked { get; set; }
        public string Role { get; set; }
    }

    public class MaterialData2
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string MasterAccess { get; set; }
        public string SettingAccess { get; set; }
        public string TransactionAccess { get; set; }
        public string Adminaccess { get; set; }
        public string ReportAccess { get; set; }
        public string TicketDataEntryAccess { get; set; }
    }

    public class MaterialData1
    {
        public int Action { get; set; }
        public int TicketNumber { get; set; }
        public string VehicleNumber { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public int EmptyWeight { get; set; }
    }

}
