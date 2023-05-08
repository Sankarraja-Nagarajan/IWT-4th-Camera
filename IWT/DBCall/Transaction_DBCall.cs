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
    class Transaction_DBCall
    {
        private string ConnectionString { get; set; }
        public Transaction_DBCall()
        {
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

    }
}
