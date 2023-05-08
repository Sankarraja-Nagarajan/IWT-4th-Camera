using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWTMultiFile.Console
{
    public class WriteLog
    {

        static readonly object WriteErrorLogLock = new object();
        public static void WriteToFile(string Message)
        {
            StreamWriter sw = null;
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogFiles");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                DateTime dt = DateTime.Today;
                DateTime ystrdy = DateTime.Today.AddDays(-45);//keep 45 days backup
                string yday = ystrdy.ToString("yyyyMMdd");
                string today = dt.ToString("yyyyMMdd");
                string Log = today + ".txt";
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\LogFiles\\Log_" + yday + ".txt"))
                {
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\LogFiles\\Log_" + yday + ".txt");
                }
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\LogFiles\\Log_" + Log, true);
                sw.WriteLine($"{DateTime.Now.ToString()} : {Message}");
                sw.Flush();
                sw.Close();
            }
            catch
            {



            }



        }
        public static void WriteToFile(string ControllerAction, Exception ex)
        {
            StreamWriter sw = null;
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogFiles");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                DateTime dt = DateTime.Today;
                DateTime ystrdy = DateTime.Today.AddDays(-45);//keep 45 days backup
                string yday = ystrdy.ToString("yyyyMMdd");
                string today = dt.ToString("yyyyMMdd");
                string Log = today + ".txt";
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\LogFiles\\Log_" + yday + ".txt"))
                {
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\LogFiles\\Log_" + yday + ".txt");
                }
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\LogFiles\\Log_" + Log, true);
                sw.WriteLine($"{DateTime.Now.ToString()} : {ControllerAction} :- {ex.Message}");
                if (ex.Message.Contains("inner exception") && ex.InnerException != null)
                {
                    sw.WriteLine($"{DateTime.Now.ToString()} : {ControllerAction} Inner :- {ex.InnerException.Message}");
                }
                sw.Flush();
                sw.Close();
            }
            catch
            {



            }



        }
        
    }
}
