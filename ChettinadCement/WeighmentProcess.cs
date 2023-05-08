using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace IWT
{
    public static class WeighmentProcess
    {
        public static int StableWeightArraySize = int.Parse(ConfigurationManager.AppSettings["StableWeightArraySize"]);
        public static int StableWeightArraySelectable = int.Parse(ConfigurationManager.AppSettings["StableWeightArraySelectable"]);
        public static List<string> GainedWeightList = new List<string>();
        public static string server = "127.0.0.1";
        public static int port = 4002;
        public static TcpClient client = new TcpClient();
        private static StreamWriter SwSender;
        private static StreamReader SrReciever;
        private static Thread thrMessaging;
        private delegate void UpdateLogCallBack(string strMessage);
        public static void ConnectTCpClient()
        {
            try
            {
                Ping pinger = new Ping();
                PingReply reply = pinger.Send("127.0.0.1");
                if(reply.Status == IPStatus.Success)
                {
                    client = new TcpClient(server, port);
                    if (client.Connected)
                    {

                    }
                    else
                    {
                        client.Connect(server, port);
                        //Byte[] data = System.Text.Encoding.ASCII.GetBytes("");
                        //NetworkStream stream = client.GetStream();
                        //stream.Write(data, 0, data.Length);
                    }
                }
                
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetWighment :-" + ex.Message);
            }
        }

        public static string AddWighments()
        {
            try
            {
                if (client.Connected)
                {
                    //client.ReceiveTimeout = 2000;
                    if (GainedWeightList.Count < StableWeightArraySize)
                    {
                        var data1 = new Byte[256];
                        String responseData = String.Empty;
                        Int32 bytes = 0;
                        NetworkStream stream = client.GetStream();
                        bytes = stream.Read(data1, 0, data1.Length);
                        responseData = System.Text.Encoding.ASCII.GetString(data1, 0, bytes);
                        GainedWeightList.Add(responseData);
                    }
                    else
                    {
                        int extra = GainedWeightList.Count - StableWeightArraySize;
                        GainedWeightList.RemoveRange(0, extra);
                        var data1 = new Byte[256];
                        String responseData = String.Empty;
                        Int32 bytes = 0;
                        NetworkStream stream = client.GetStream();
                        bytes = stream.Read(data1, 0, data1.Length);
                        responseData = System.Text.Encoding.ASCII.GetString(data1, 0, bytes);
                        GainedWeightList.Add(responseData);
                    }
                    return GainedWeightList.LastOrDefault();
                }
                else
                {
                    ConnectTCpClient();
                    return null;
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("AddWighments :-" + ex.Message);
                return null;
            }

        }

        private static void ReceiveMessages()
        {
            SrReciever = new StreamReader(client.GetStream());
            while (true)
            {
                string response = SrReciever.ReadLine();
                //Dispatcher.Invoke(new UpdateLogCallBack(UpdateLog), new object[] { response });
            }
        }
        private static void UpdateLog(string strMessage)
        {
           // txt_Log.AppendText(strMessage);
        }

        public static string GetWighment()
        {

            try
            {
                return GainedWeightList.LastOrDefault();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetWighment :-" + ex.Message);
                return null;
            }
        }

        public static bool CheckIsStable()
        {
            try
            {
                var Copylist = new List<string>();
                Copylist = GainedWeightList;
                var tempList = Copylist.Skip(Math.Max(0, Copylist.Count() - StableWeightArraySelectable)).Take(StableWeightArraySelectable).ToArray();
                if (Array.TrueForAll(tempList, y => y == tempList[0]))
                {
                    if (tempList.Length > 0)
                    {
                        return true;
                    }

                }

            }
            catch (Exception ex)
            {
                //WriteLog.WriteToFile("CheckIsStable :-" + ex.Message);
                return false;
            }
            return false;
        }

    }
}
