using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWT.Models
{
    public class SMTPSetting
    {
        public int ID { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
    }
    public class MailSetting
    {
        public int ID { get; set; }
        public string FromID { get; set; }
        public string Password { get; set; }
        public string ToID { get; set; }
        public string CCList { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string UserName { get; set; }
        public string SendType { get; set; }
    }

    public class SMSTemplate
    {
        public int TemplateNo { get; set; }
        public string Content { get; set; }
        public string PhoneNo1 { get; set; }
        public string PhoneNo2 { get; set; }
        public string PhoneNo3 { get; set; }
        public string ProviderUserName { get; set; }
        public string Password { get; set; }
        public bool UseGSM { get; set; }
        public string UserName { get; set; }
        public string AutoSMSNumber { get; set; }
    }

    public class CCTVSettings
    {
        public int ID { get; set; }
        public bool Enable { get; set; }
        public string IPAddress { get; set; }
        public string CaptureURL { get; set; }
        public string Port { get; set; }
        public string LogFolder { get; set; }
        public string HardwareProfile { get; set; }
        public int RecordID { get; set; }
        public string CameraUserName { get; set; }
        public string CameraPassword { get; set; }
        public string CameraType { get; set; }
        public string UserName { get; set; }

    }

    public class PendingVehicle
    {
        public string VehicleNumber;
        public int TicketNo;
    }
    public class TableDetails
    {
        public string TableName { get; set; }
    }
    public class TableColumnDetails
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
    }

    public class WhereTableColumnDetails
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public string MatchedColumnName { get; set; }
    }

    public class SMSDesign
    {
        public int ID { get; set; }
        public string DesignedContent { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class EmailDesign
    {
        public int ID { get; set; }
        public string DesignedContent { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class OperationDetail
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public enum OperationTypes
    {
        All, Common
    }
    public class SavedReportTemplate
    {
        public int TemplateID { get; set; }
        public string ReportName { get; set; }
        public string TableName { get; set; }
        public string Query { get; set; }
        public bool WhereEnabled { get; set; }
        public string WhereJSON { get; set; }
        public string WhereCriteria { get; set; }
    }
    public class SavedReportTemplateFields
    {
        public int FieldID { get; set; }
        public int TemplateID { get; set; }
        public string FieldName { get; set; }
        public string DataType { get; set; }
        public string TableName { get; set; }
    }

    public class SavedReportTemplateWhereFields
    {
        public int WhereFieldID { get; set; }
        public int TemplateID { get; set; }
        public string FieldName { get; set; }
        public string DataType { get; set; }
        public string TableName { get; set; }
        public string MatchedColumnName { get; set; }
    }

    public class TransactionTypeMaster
    {
        public string TransactionType { get; set; }
        public string ShortCode { get; set; }
        public string Description { get; set; }
    }

    public class IsSapBased
    {
        public bool ShortCode { get; set; }
        public string Description { get; set; }
    }

    public class IsLoaded
    {
        public bool ShortCode { get; set; }
        public string Description { get; set; }
    }

    public class SavedReportTemplate1
    {
        public double TemplateID { get; set; }
    }

    public class Caption
    {
        public int CaptionID { get; set; }
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public string CaptionName { get; set; }
        public bool IsDeleted { get; set; }
        public int Width { get; set; }

    }

    public class GroupByClass
    {
        public string Column1 { get; set; }
        public string Column2 { get; set; }
        public string Column3 { get; set; }
    }

    public class ImageSourcePath
    {
        public byte[] Image1Path { get; set; }
        public byte[] Image2Path { get; set; }
        public byte[] Image3Path { get; set; }
        public byte[] Image4Path { get; set; }
        public byte[] Image5Path { get; set; }
        public byte[] Image6Path { get; set; }
        public byte[] Img4Path { get; set; }
        public byte[] Img4Path2 { get; set; }
        public byte[] WaterMarkImagePath { get; set; }
    }
    public class FailedMailSMS
    {
        public int ID { get; set; }
        public string FromID { get; set; }
        public string ToID { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string CCList { get; set; }
        public bool MailFlag { get; set; }
        public string UserName { get; set; }
    }
    public class FailedSMS
    {
        public int ID { get; set; }
        public string MobileNo1 { get; set; }
        public string MobileNo2 { get; set; }
        public string MobileNo3 { get; set; }
        public string Message { get; set; }
        public string SMSRoute { get; set; }
        public bool SMSFlag { get; set; }
    }

    public class SoftwareSettingConfig
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

    public class RFIDAllocation
    {
        public int AllocationId { get; set; }
        public int? GatePassId { get; set; }
        public string TransType { get; set; }
        public bool IsSapBased { get; set; }
        public string DocNumber { get; set; }
        public string TransMode { get; set; }
        public bool? IsLoaded { get; set; }
        public string VehicleNumber { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public int TareWeight { get; set; }
        public string AllocationType { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string RFIDTag { get; set; }
        public string Status { get; set; }
        public string CustomFieldValues { get; set; }
        public string GatePassNumber { get; set; }
        public string TokenNumber { get; set; }
        public bool IsSelected { get; set; }
        public int? NoOfMaterial { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class RFIDAllocationWithTrans
    {
        public int AllocationId { get; set; }
        public int? GatePassId { get; set; }
        public string TransType { get; set; }
        public bool IsSapBased { get; set; }
        public string DocNumber { get; set; }
        public string TransMode { get; set; }
        public bool? IsLoaded { get; set; }
        public string VehicleNumber { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public int? TareWeight { get; set; }
        public string AllocationType { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string RFIDTag { get; set; }
        public string Status { get; set; }
        public string CustomFieldValues { get; set; }
        public string GatePassNumber { get; set; }
        public string TokenNumber { get; set; }
        public bool IsSelected { get; set; }
        public int? NoOfMaterial { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? TicketNo { get; set; }
        public int? EmptyWeight { get; set; }
        public int? LoadWeight { get; set; }
        public DateTime? EmptyWeightDate { get; set; }
        public string EmptyWeightTime { get; set; }
        public DateTime? LoadWeightDate { get; set; }
        public string LoadWeightTime { get; set; }
        public int? NetWeight { get; set; }
        public bool? Closed { get; set; }
        public string SystemID { get; set; }
    }

    public class SystemConfigurations
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //public bool IsAutoLogin { get; set; }
        //public string AutoLoginUser { get; set; }
        public string HardwareProfile { get; set; }
    }


    public class GatePasses
    {
        public int Id { get; set; }
        public string TokenNumber { get; set; }
        public string VehicleNumber { get; set; }
        public string InOut { get; set; }
        public string OutwardType { get; set; }
        public string Plant { get; set; }
        public string MaterialNumber { get; set; }
        public string GatePassNumber { get; set; }
        public string PoNumber { get; set; }
        public string PoItemNumber { get; set; }
        public string SoNumber { get; set; }
        public string SoItemNumber { get; set; }
        public string InwardType { get; set; }
        public string Status { get; set; }
        public bool IsSelected { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    public class SAPDataBackUp
    {
        public int Id { get; set; }
        public string Trans { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public string Payload { get; set; }
        public string Response { get; set; }
        public int NoOfRetry { get; set; }
        public string Status { get; set; }
        public bool IsSelected { get; set; }
        public int? TransId { get; set; }
        public string TransType { get; set; }
        public int? CompletedTrans { get; set; }
    }

    public class GatePassItems
    {
        public int Id { get; set; }
        public string ItemNumber { get; set; }
        public string ItemType { get; set; }
        public int GatePassId { get; set; }
        public bool IsSelected { get; set; }
    }

    public class OperationType
    {
        public int ID { get; set; }
        public string Type { get; set; }
    }

    public class RFIDMaster
    {
        public string Tag { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public string VehicleNo { get; set; }
    }

    public class RFIDReaderMaster
    {
        public string Location { get; set; }
        public string Reader { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public bool? IsEnable { get; set; }
        public bool? GateExitEnable { get; set; }
        public string HardwareProfile { get; set; }
    }

    public class PLCMaster
    {
        public int Id { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public bool IsEnable { get; set; }
        public string HardwareProfile { get; set; }
    }

    public class SerialCommunicationSetting 
    {
        public int Id { get; set; }
        public string Port { get; set; }
        public int BaudRate { get; set; }
        public int Parity { get; set; }
        public int StopBit { get; set; }
        public int DataBits { get; set; }
        public int DataLength { get; set; }
        public int ImmediateDelay { get; set; }
        public string StartChar { get; set; }
        public string EndChar { get; set; }
        public bool? Enable { get; set; }
        public string HardwareProfile { get; set; }
    }


    public class AWSConfiguration
    {
        public int Id { get; set; }
        public int SequenceTimeOut { get; set; }
        public bool? SAP  { get; set;}
        public bool? SemiAutomatic { get; set;}
        public bool? VPSEnable { get; set; }
        public bool? IsAutoLogin { get; set; }
        public string AutoLoginUser { get; set; }
        public string WeightCommunication { get; set; }
        public string HardwareProfile { get; set; }
        public bool? FTPrintEnable { get; set; }
        public bool? STPrintEnable { get; set; }
        public bool? AutoGateExit { get; set; }
        public bool? SGPrintEnable { get; set; }
    }

    public class HardwareProfileModel
    {
        public int ID { get; set; }
        public string ProfileName { get; set; } 
    }
}
