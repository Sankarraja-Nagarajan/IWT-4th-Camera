using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWT.Models
{
    public class Transaction
    {
        public int TicketNo { get; set; }
        public string VehicleNo { get; set; }
        public string TransactionType { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int EmptyWeight { get; set; }
        public int LoadWeight { get; set; }
        public DateTime? EmptyWeightDate { get; set; }
        public string EmptyWeightTime { get; set; }
        public DateTime? LoadWeightDate { get; set; }
        public string LoadWeightTime { get; set; }
        public int NetWeight { get; set; }
        public bool Pending { get; set; }
        public bool Closed { get; set; }
        public string ShiftName { get; set; }
        public string State { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public bool MultiWeight { get; set; }
        public bool MultiWeightTransPending { get; set; }
        public string LoadStatus { get; set; }
        public string SystemID { get; set; }
        public string UserName { get; set; }
        public int NoOfMaterial { get; set; }
        public int ProcessedMaterial { get; set; }
        public bool IsSelected { get; set; }
        public int RFIDAllocation { get; set; }
        public string TransType { get; set; }
        public string DocNumber { get; set; }
        public string GatePassNumber { get; set; }
        public string TokenNumber { get; set; }
        public bool IsSapBased { get; set; }
    }

    public class TransactionDetails
    {
        public int TransactionDetID { get; set; }
        public int TicketNo { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public int Weight { get; set; }
        public int TDLoadWeight { get; set; }
        public int TDEmptyWeight { get; set; }
    }

    public class PendingTicketsTransaction
    {
        public int TicketNo { get; set; }
        public string VehicleNo { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int EmptyWeight { get; set; }
        public int LoadWeight { get; set; }
        public DateTime EmptyWeightDate { get; set; }
        public string EmptyWeightTime { get; set; }
        public DateTime LoadWeightDate { get; set; }
        public string LoadWeightTime { get; set; }
        public int NetWeight { get; set; }
        public bool Pending { get; set; }
        public bool Closed { get; set; }
        public string MaterialName { get; set; }
        public string SupplierName { get; set; }
        public string State { get; set; }
        public bool MultiWeight { get; set; }
        public bool MultiWeightTransPending { get; set; }
        public string LoadStatus { get; set; }
        public string SystemID { get; set; }
        public string UserName { get; set; }
        public string DriverName { get; set; }
        public string NoOfMaterial { get; set; }
        public string TrCustomerName { get; set; }
        public string TrOperatorName { get; set; }
    }
    public class TransactionResult
    {
        public double TicketNo { get; set; }
    }

    public class Company_Details
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Pin { get; set; }
        public string State { get; set; }
        public string Phone { get; set; }
        public string CompanyLogo { get; set; }
    }

    public class CompanySummaryReportData
    {
        public int ID { get; set; }
        public string LogoPath { get; set; }
        public string CompanyName { get; set; }
        public string CompanyHeaderTitle { get; set; }
        public string CompanyPhoneAddress { get; set; }
        public string CompanyFooter { get; set; }
        public string UserName { get; set; }
    }

    public class ImageData
    {
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public string Image3 { get; set; }
    }

}
