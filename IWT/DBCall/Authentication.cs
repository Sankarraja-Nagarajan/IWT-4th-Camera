using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IWT.DBCall
{
    public class Authentication
    {
        private string connectionString;
        public Authentication()
        {
            connectionString= GetDecryptedConnectionStringDB();
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

        public async Task<AuthStatus> authenticateUser(string username,string password)
        {
            AuthStatus Result = new AuthStatus();
            try
            {
                Result.Status = false;
                string decryptedPassword;
                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(connectionString);
                string sql = "select * from [User_Management] where UserName=@username";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@username", SqlDbType.NVarChar).Value = username;
                await con.OpenAsync();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
                cmd.Dispose();
                con.Close();
                if (dt != null && dt.Rows.Count > 0)
                {
                    string lPassword=dt.Rows[0]["Password"].ToString();
                    string role = dt.Rows[0]["Role"].ToString();
                    string name = dt.Rows[0]["UserName"].ToString();
                    string hardware = dt.Rows[0]["HardwareProfile"].ToString();
                    decryptedPassword = Decrypt(lPassword,true);
                    if (decryptedPassword == password)
                    {
                        WriteLog.WriteToFile($"authenticateUser:- {username} - Logged in successfully ");
                        Result.Status = true;
                        Result.Message = "Successful login";
                        Result.Role = role;
                        Result.UserName = name;
                        Result.HardwareProfileName= hardware;
                        return Result;
                    }
                    else
                    {
                        WriteLog.WriteToFile($"authenticateUser:- {username} - Incorrect password ");
                        Result.Status = false;
                        Result.Message = "Invalid User name or password";
                        return Result;
                    }
                }
                else
                {
                    WriteLog.WriteToFile($"authenticateUser:- {username} - User not found");
                    Result.Status = false;
                    //Result.Message = "User not found";
                    Result.Message = "Invalid User name or password";
                    return Result;
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("authenticateUser:" + ex.Message);
                Result.Status = false;
                Result.Message = ex.Message;
                return Result;
            }
        }
        #region EncryptAndDecrypt
        public string Decrypt(string Password, bool UseHashing)
        {
            try
            {
                //string EncryptionKey = ConfigurationManager.AppSettings["EncryptionKey"];
                string EncryptionKey = "Exalca";
                byte[] KeyArray;
                byte[] ToEncryptArray = Convert.FromBase64String(Password);
                if (UseHashing)
                {
                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                    KeyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(EncryptionKey));
                    hashmd5.Clear();
                }
                else
                {
                    KeyArray = UTF8Encoding.UTF8.GetBytes(EncryptionKey);
                }

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                tdes.Key = KeyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = tdes.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(
                                     ToEncryptArray, 0, ToEncryptArray.Length);
                tdes.Clear();
                return UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("PortalRepository/Decrypt :- " + ex);
                return null;
            }

        }

        public string Encrypt(string Password, bool useHashing)
        {
            try
            {
                //string EncryptionKey = ConfigurationManager.AppSettings["EncryptionKey"];
                string EncryptionKey = "Exalca";
                byte[] KeyArray;
                byte[] ToEncryptArray = UTF8Encoding.UTF8.GetBytes(Password);
                if (useHashing)
                {
                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                    KeyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(EncryptionKey));
                    hashmd5.Clear();
                }
                else
                    KeyArray = UTF8Encoding.UTF8.GetBytes(EncryptionKey);

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                tdes.Key = KeyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = tdes.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(ToEncryptArray, 0,
                  ToEncryptArray.Length);
                tdes.Clear();
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("PortalRepository/Encrypt :- " + ex);
                return null;
            }
        }

        #endregion
    }
    public class AuthStatus
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string Role { get; set; }
        public string HardwareProfileName { get; set; }
        public string UserName { get; set; }
    }

    public class AuthStatusForTransactions
    {
        public bool Status = true;
        public string Message = "Logged in successfully";
        public string Role = "Admin";
        public string HardwareProfileName = "Store";
        public string UserName = "store";
    }
}
