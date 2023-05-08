namespace AWS.Communication.Models
{
    public class GatePass
    {
        public int Id { get; set; }
        public string TokenNumber { get; set; }
        public string VehicleNumber { get; set; }
        public string InOut { get; set; }
        public string OutwardType { get; set; }
        public string Plant { get; set; }
        public string[] MaterialNumber { get; set; }
        public string GatePassNumber { get; set; }
        public string PoNumber { get; set; }
        public string[] PoItemNumber { get; set; }
        public string SoNumber { get; set; }
        public string[] SoItemNumber { get; set; }
        public string InwardType { get; set; }
        public string Status { get; set; }
    }
}