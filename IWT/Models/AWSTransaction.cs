using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWT.Models
{
    public class AWSTransaction
    {
        public Transaction TransactionData { get; set; }
        public RFIDAllocation AllocationData { get; set; }
        public bool IsSecondReader { get; set; }
    }
}
