using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWT.Models
{
    public class ChangePassword
    {
        public string UserName { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
    public class ChangePasswordResult
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
