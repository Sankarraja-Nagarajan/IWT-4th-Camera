using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IWT
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

        public static void WriteAWSLog(string ControllerAction, Exception ex=null)
        {
            StreamWriter sw = null;
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AWSLogFiles");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                DateTime dt = DateTime.Today;
                DateTime ystrdy = DateTime.Today.AddDays(-45);//keep 45 days backup
                string yday = ystrdy.ToString("yyyyMMdd");
                string today = dt.ToString("yyyyMMdd");
                string Log = today + ".txt";
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\AWSLogFiles\\Log_" + yday + ".txt"))
                {
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\AWSLogFiles\\Log_" + yday + ".txt");
                }
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\AWSLogFiles\\Log_" + Log, true);
                if(ex != null)
                {
                    sw.WriteLine($"{DateTime.Now.ToString()} : {ControllerAction} :- {ex.Message}");
                    if (ex.Message.Contains("inner exception") && ex.InnerException != null)
                    {
                        sw.WriteLine($"{DateTime.Now.ToString()} : {ControllerAction} Inner :- {ex.InnerException.Message}");
                    }
                }
                else
                {
                    sw.WriteLine($"{DateTime.Now.ToString()} : {ControllerAction}");
                }
                sw.Flush();
                sw.Close();
            }
            catch
            {
            }
        }

        public static void WriteErrorLog(string Message)
        {
            StreamWriter sw = null;
            try
            {
                lock (WriteErrorLogLock)
                {
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ErrorFiles");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    DateTime dt = DateTime.Today;
                    DateTime ystrdy = DateTime.Today.AddDays(-50);//keep 50 days backup
                    string yday = ystrdy.ToString("yyyyMMdd");
                    string today = dt.ToString("yyyyMMdd");
                    string Log = today + ".txt";
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\ErrorFiles\\Log_" + yday + ".txt"))
                    {
                        System.GC.Collect();
                        System.GC.WaitForPendingFinalizers();
                        File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\ErrorFiles\\Log_" + yday + ".txt");
                    }
                    sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\ErrorFiles\\Log_" + Log, true);
                    sw.WriteLine(string.Format(DateTime.Now.ToString()) + ":" + Message);
                    sw.Flush();
                    sw.Close();
                    //WriteLogStation(Message);
                }
            }
            catch (Exception ex)
            {

            }

        }

        public static void WriteLogStation(string Message)
        {
            try
            {
                string URL = ConfigurationManager.AppSettings["BaseURL"].ToString();
                ConnectToApi(URL + "api/Queue/WriteLogStationWise?Message=" + Message);
            }
            catch (Exception e)
            {
            }

        }

        public static void WriteLogPLC(string Message, string station)
        {
            try
            {
                string URL = ConfigurationManager.AppSettings["BaseURL"].ToString();
                ConnectToApi1(URL + "api/Queue/WriteLogPlcService?Message=" + Message + "&Station=" + station);
            }
            catch (Exception e)
            {
            }

        }

        public static string ConnectToApi1(string FullAddress)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(FullAddress);
                request.Method = "GET";
                var response = (HttpWebResponse)request.GetResponse();
                string result = string.Empty;
                using (var stream = response.GetResponseStream())
                {
                    using (var sr = new StreamReader(stream))
                    {
                        result = sr.ReadToEnd();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteErrorLogForWebReqError("WriteLogPLC:-" + ex.Message);
                //WriteLog.WriteToFile("ConnectToApi1 :-WriteLogPLC : " + ex.Message);
                return null;
            }
        }

        public static void WriteHistoryLog(string Message)
        {
            StreamWriter sw = null;
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HistoryFiles");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                DateTime dt = DateTime.Today;
                DateTime ystrdy = DateTime.Today.AddDays(-15);//keep 15 days backup
                string yday = ystrdy.ToString("yyyyMMdd");
                string today = dt.ToString("yyyyMMdd");
                string Log = today + ".txt";
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\HistoryFiles\\Log_" + yday + ".txt"))
                {
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\HistoryFiles\\Log_" + yday + ".txt");
                }
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\HistoryFiles\\Log_" + Log, true);
                sw.WriteLine(string.Format(DateTime.Now.ToString()) + ":" + Message);
                sw.Flush();
                sw.Close();
            }
            catch
            {

            }
        }

        public static string ConnectToApi(string FullAddress)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(FullAddress);
                request.Method = "GET";
                request.Timeout = 500;
                var response = (HttpWebResponse)request.GetResponse();
                string result = string.Empty;
                using (var stream = response.GetResponseStream())
                {
                    using (var sr = new StreamReader(stream))
                    {
                        result = sr.ReadToEnd();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteErrorLogForWebReqError("WriteLogStation:-" + ex.Message);
                //WriteLog.WriteToFile("ConnectToApi :-WriteLogStation: " + ex.Message);
                return null;
            }
        }

        public static void WriteErrorLogForWebReqError(string Message)
        {
            StreamWriter sw = null;
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ErrorFiles");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                DateTime dt = DateTime.Today;
                DateTime ystrdy = DateTime.Today.AddDays(-50);//keep 50 days backup
                string yday = ystrdy.ToString("yyyyMMdd");
                string today = dt.ToString("yyyyMMdd");
                string Log = today + ".txt";
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\ErrorFiles\\Log_" + yday + ".txt"))
                {
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\ErrorFiles\\Log_" + yday + ".txt");
                }
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\ErrorFiles\\Log_" + Log, true);
                sw.WriteLine(string.Format(DateTime.Now.ToString()) + ":" + Message);
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
