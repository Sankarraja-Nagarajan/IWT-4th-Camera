using IWT.Models;
using IWT.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWT.DBCall
{
    public class Transaction_DBCall
    {
        private string ConnectionString { get; set; }
        public static AdminDBCall adminDBCall = new AdminDBCall();
        public static CommonFunction commonFunction = new CommonFunction();
        CloudAppConfig selectedCloudAppConfig = new CloudAppConfig();
        public Transaction_DBCall()
        {
            GetCloudAppConfig();
            //ConnectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
            ConnectionString = GetDecryptedConnectionStringDB();
        }
        public string GetDecryptedConnectionStringDB()
        {
            try
            {
                Byte[] b = Convert.FromBase64String(ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString);
                string decryptedConnectionString = System.Text.ASCIIEncoding.ASCII.GetString(b);
                return decryptedConnectionString;
            }
            catch (Exception ex)
            {
                //WriteLog.WriteToFile("GetDecryptedConnectionStringDB:" + ex.Message);
                Byte[] b1 = System.Text.ASCIIEncoding.ASCII.GetBytes(ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString);
                string encryptedConnectionString = Convert.ToBase64String(b1);
                var decrypted = System.Text.ASCIIEncoding.ASCII.GetString(b1);
                return decrypted;
            }
        }

        public DataTable GetData(SqlCommand cmd)
        {
            try
            {
                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(ConnectionString);
                cmd.Connection = con;
                cmd.CommandType = CommandType.StoredProcedure;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
                cmd.Dispose();
                con.Close();
                return dt;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetData:" + ex.Message);
                return null;
            }
        }

        public void ReSendCloudSync()
        {
            try
            {
                if (selectedCloudAppConfig != null && selectedCloudAppConfig.ID != 0 && selectedCloudAppConfig.IsEnabled)
                {
                    WriteLog.WriteToFile("--------------------------------- ReSend CloudSync called ---------------------------------");
                    DataTable dt1 = adminDBCall.GetAllData("select * from Cloud_Transaction where IsSynced='false'");
                    string JSONString = JsonConvert.SerializeObject(dt1);
                    List<CloudConfigTransaction> transactions = JsonConvert.DeserializeObject<List<CloudConfigTransaction>>(JSONString);
                    if (transactions.Count > 0 && selectedCloudAppConfig.IsEnabled)
                    {
                        foreach (CloudConfigTransaction trans in transactions)
                        {
                            DataTable dt = adminDBCall.GetAllData($"select * from [Transaction] where TicketNo ={trans.TicketNo}");
                            commonFunction.SendTransactionDetailsToCloudApp(selectedCloudAppConfig, dt, trans.TicketNo);
                        }
                    }
                    WriteLog.WriteToFile("--------------------------------- ReSend CloudSync end ---------------------------------");
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Transaction_DBCall/ReSendCloudSync/Exception:- "+ex.Message);
            }
        }

        public void SaveCloudConfigTransaction(int ticketNo)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                con.Open();
                string InsertIntoTransactionQuery = $"INSERT INTO [Cloud_Transaction] (TicketNo,IsSynced) VALUES ({ticketNo},'false')";
                new SqlCommand(InsertIntoTransactionQuery, con).ExecuteNonQuery();
                con.Close();
            }
            catch(Exception ex)
            {
                WriteLog.WriteToFile("Transaction_DBCall/SaveCloudConfigTransaction/Exception :- " + ex.Message);
            }
        }

        public void UpdateCloudTransaction(int ticketNo, bool isSynced=true)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                con.Open();
                string InsertIntoTransactionQuery = $" UPDATE [Cloud_Transaction] SET IsSynced = '{isSynced}' WHERE TicketNo='{ticketNo}'";
                new SqlCommand(InsertIntoTransactionQuery, con).ExecuteNonQuery();
                con.Close();
            }
            catch(Exception ex)
            {
                WriteLog.WriteToFile("Transaction_DBCall/UpdateCloudTransaction/Exception :- " + ex.Message);
            }
        }

        public void GetCloudAppConfig()
        {
            try
            {
                DataTable dt1 = adminDBCall.GetAllData("SELECT * FROM CloudApp_Config");
                string JSONString = JsonConvert.SerializeObject(dt1);
                var CloudAppConfigs = JsonConvert.DeserializeObject<List<CloudAppConfig>>(JSONString);
                selectedCloudAppConfig = (CloudAppConfigs != null && CloudAppConfigs.Count > 0) ? CloudAppConfigs.FirstOrDefault() : new CloudAppConfig();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Transaction_DBCall/GetCloudAppConfig", ex);
            }
        }
    }
}
