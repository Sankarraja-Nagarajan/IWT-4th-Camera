using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TcpClientService.Helpers
{
    public class DataConversionHelper
    {
        public static string ToRfidValue(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var data in bytes)
            {
                sb.Append(data.ToString("X2"));
            }
            string rfId = "";
            if (sb.Length > 24)
            {
                string raw = sb.ToString();
                rfId = raw.Substring(19, 5);
            }
            //string rfId = Encoding.UTF8.GetString(bytes);
            return rfId;
        }
        public static string ToWeighValue(byte[] bytes)
        {
            var weight = Encoding.UTF8.GetString(bytes);
            return Regex.Match(weight, @"-?\d+").Value;
        }
        public static string ToPlcValue(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
