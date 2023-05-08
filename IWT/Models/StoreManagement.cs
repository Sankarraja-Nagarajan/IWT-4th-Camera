using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWT.Models
{
    public class StoreMaterialManagement
    {
        public int Id { get; set; }
        public int AllocationId { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public int Order { get; set; }
        public bool? Closed { get; set; }
        public string ItemNo { get; set; }
    }
    public class StoreSupplierManagement
    {
        public int Id { get; set; }
        public int AllocationId { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public int Order { get; set; }
        public bool? Closed { get; set; }
    }

    public class StoreManagement
    {
        public List<StoreMaterialManagement> Materials { get; set; }
        public List<StoreSupplierManagement> Suppliers { get; set; }
        public StoreManagement()
        {
            this.Materials = new List<StoreMaterialManagement>();
            this.Suppliers= new List<StoreSupplierManagement>();
        }
    }
}
