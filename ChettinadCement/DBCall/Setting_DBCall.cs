using IWT.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IWT.Setting_Pages.Camera_setting;
using static IWT.Setting_Pages.Email;
using static IWT.Setting_Pages.File_location;
using static IWT.Setting_Pages.Hardware_profile;
using static IWT.Setting_Pages.Other_setting;
using static IWT.Setting_Pages.SMS_setting;
using static IWT.Setting_Pages.SMTP_setting;
using static IWT.Setting_Pages.Software;
using static IWT.Setting_Pages.Summary_Report;
using static IWT.Setting_Pages.Weighing;

namespace IWT.DBCall
{
    class Setting_DBCall
    {
        private string ConnectionString { get; set; }
        public Setting_DBCall()
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
        #region WeightBridge

        public DataTable GetWeighBridgeData(string SQL)
        {
            try
            {
                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlCommand cmd = new SqlCommand(SQL);
                cmd.Connection = con;
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
                WriteLog.WriteToFile("GetWeighBridgeData:" + ex.Message);
                return null;
            }
        }
        public void InsertWeighBridgeData(Weight_Data data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "INSERT INTO [Weighbridge_Settings] (Host,Port,HardwareProfile) VALUES(@Host,@Port,@HardwareProfile)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Host", SqlDbType.VarChar).Value = data.Host;
                cmd.Parameters.Add("@Port", SqlDbType.VarChar).Value = data.Port;
                cmd.Parameters.Add("@HardwareProfile", SqlDbType.VarChar).Value = data.HardwareProfile;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InsertWeighBridgeData : " + ex.Message);
            }
        }
        public void UpdateWeighBridgeData(Weight_Data data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = $"Update Weighbridge_Settings SET Host=@Host,Port=@Port WHERE HardwareProfile='{data.HardwareProfile}'";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Host", SqlDbType.VarChar).Value = data.Host;
                cmd.Parameters.Add("@Port", SqlDbType.VarChar).Value = data.Port;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdateWeighBridgeData : " + ex.Message);
            }
        }
        #endregion

        #region Email
        public DataTable GetEmailData(string SQL)
        {
            try
            {
                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlCommand cmd = new SqlCommand(SQL);
                cmd.Connection = con;
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
                WriteLog.WriteToFile("GetEmailData:" + ex.Message);
                return null;
            }
        }
        public void InsertEmailData(Mail data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "INSERT INTO [Mail_Settings] (FromID,ToID,Enable,Password,CCList,Subject,Message,SendType) VALUES(@FromID,@ToID,@Enable,@Password,@CCList,@Subject,@Message,@SendType)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@FromID", SqlDbType.VarChar).Value = data.MailID;
                cmd.Parameters.Add("@ToID", SqlDbType.VarChar).Value = data.To;
                cmd.Parameters.Add("@Enable", SqlDbType.Bit).Value = true;
                cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = data.Password;
                cmd.Parameters.Add("@CCList", SqlDbType.VarChar).Value = data.Cc;
                cmd.Parameters.Add("@Subject", SqlDbType.VarChar).Value = data.Subject;
                cmd.Parameters.Add("@Message", SqlDbType.VarChar).Value = data.Message;

                cmd.Parameters.Add("@SendType", SqlDbType.VarChar).Value = data.SendType;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InsertEmailData : " + ex.Message);
            }
        }
        public void UpdateEmailData(Mail data)
        {
            try
            {

                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "Update Mail_Settings SET ToID=@ToID,Enable=@Enable,Password=@Password,CCList=@CCList,Subject=@Subject,Message=@Message,FromID=@FromID WHERE SendType=@SendType";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@ToID", SqlDbType.VarChar).Value = data.To;
                cmd.Parameters.Add("@Enable", SqlDbType.Bit).Value = true;
                cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = data.Password;
                cmd.Parameters.Add("@CCList", SqlDbType.VarChar).Value = data.Cc;
                cmd.Parameters.Add("@Subject", SqlDbType.VarChar).Value = data.Subject;
                cmd.Parameters.Add("@Message", SqlDbType.VarChar).Value = data.Message;
                cmd.Parameters.Add("@FromID", SqlDbType.VarChar).Value = data.MailID;
                cmd.Parameters.Add("@SendType", SqlDbType.VarChar).Value = data.SendType;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdateEmailData : " + ex.Message);
            }
        }

        public void DisableEmailData(string SendType)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "DisableEmailData";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Enable", SqlDbType.Bit).Value = false;
                cmd.Parameters.Add("@SendType", SqlDbType.VarChar).Value = SendType;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdateEmailData : " + ex.Message);
            }
        }

        public void DeleteEmailData(Mail data)
        {
            try
            {

                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "DELETE FROM Mail_Settings WHERE ToID=@ToID AND Password=@Password AND CCList=@CCList AND Subject=@Subject AND Message=@Message AND FromID=@FromID AND SendType=@SendType";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@ToID", SqlDbType.VarChar).Value = data.To;
                cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = data.Password;
                cmd.Parameters.Add("@CCList", SqlDbType.VarChar).Value = data.Cc;
                cmd.Parameters.Add("@Subject", SqlDbType.VarChar).Value = data.Subject;
                cmd.Parameters.Add("@Message", SqlDbType.VarChar).Value = data.Message;
                cmd.Parameters.Add("@FromID", SqlDbType.VarChar).Value = data.MailID;
                cmd.Parameters.Add("@SendType", SqlDbType.VarChar).Value = data.SendType;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DeleteEmailData : " + ex.Message);
            }
        }

        #endregion

        #region Camera_setting
        public DataTable GetCCTVData(string SQL)
        {
            try
            {
                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlCommand cmd = new SqlCommand(SQL);
                cmd.Connection = con;
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
                WriteLog.WriteToFile("GetCCTVData:" + ex.Message);
                return null;
            }
        }
        public void InsertCCTVData(Camera_Config data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "INSERT INTO [CCTV_Settings] (Enable,IPAddress,CaptureURL,LogFolder,RecordID,CameraUserName,CameraPassword,CameraType,HarwareProfile) VALUES(@Enable,@IPAddress,@CaptureURL,@LogFolder,@RecordID,@CameraUserName,@CameraPassword,@CameraType,@HarwareProfile)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Enable", SqlDbType.VarChar).Value = data.enable;
                cmd.Parameters.Add("@IPAddress", SqlDbType.VarChar).Value = data.stream;
                cmd.Parameters.Add("@CaptureURL", SqlDbType.VarChar).Value = data.capture;
                cmd.Parameters.Add("@LogFolder", SqlDbType.VarChar).Value = data.log;
                cmd.Parameters.Add("@RecordID", SqlDbType.VarChar).Value = data.recordID;
                cmd.Parameters.Add("@CameraUserName", SqlDbType.VarChar).Value = data.username;
                cmd.Parameters.Add("@CameraPassword", SqlDbType.VarChar).Value = data.password;
                cmd.Parameters.Add("@CameraType", SqlDbType.VarChar).Value = data.type;
                cmd.Parameters.Add("@HarwareProfile", SqlDbType.VarChar).Value = data.HarwareProfile;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InsertCCTVData:" + ex.Message);
            }
        }
        public void UpdateCCTVData(Camera_Config data)
        {
            try
            {

                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = $"Update CCTV_Settings SET  Enable=@Enable,IPAddress=@IPAddress,CaptureURL=@CaptureURL,LogFolder=@LogFolder,CameraUserName=@CameraUserName,CameraPassword=@CameraPassword,CameraType=@CameraType WHERE RecordID='{data.recordID}' and HarwareProfile='{data.HarwareProfile}'";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Enable", SqlDbType.VarChar).Value = data.enable;
                cmd.Parameters.Add("@IPAddress", SqlDbType.VarChar).Value = data.stream;
                cmd.Parameters.Add("@CaptureURL", SqlDbType.VarChar).Value = data.capture;
                cmd.Parameters.Add("@LogFolder", SqlDbType.VarChar).Value = data.log;
                cmd.Parameters.Add("@CameraUserName", SqlDbType.VarChar).Value = data.username;
                cmd.Parameters.Add("@CameraPassword", SqlDbType.VarChar).Value = data.password;
                cmd.Parameters.Add("@CameraType", SqlDbType.VarChar).Value = data.type;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdateCCTVData:" + ex.Message);
            }
        }
        #endregion

        #region SMTP_Setting

        public DataTable GetSMTPData(string SQL)
        {
            try
            {
                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlCommand cmd = new SqlCommand(SQL);
                cmd.Connection = con;
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
                WriteLog.WriteToFile("GetSMTPData:" + ex.Message);
                return null;
            }
        }
        public void InsertSMTPData(SMTP_Config data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "INSERT INTO [SMTP_Settings] (Host,Port) VALUES(@Host,@Port)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Host", SqlDbType.VarChar).Value = data.Host;
                cmd.Parameters.Add("@Port", SqlDbType.VarChar).Value = data.Port;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InsertSMTPData:" + ex.Message);
            }
        }
        public void UpdateSMTPData(SMTP_Config data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "Update SMTP_Settings SET Host=@Host,Port=@Port";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Host", SqlDbType.VarChar).Value = data.Host;
                cmd.Parameters.Add("@Port", SqlDbType.VarChar).Value = data.Port;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdateSMTPData:" + ex.Message);
            }
        }
        #endregion

        #region Summary_report

        public DataTable GetSummaryData(string SQL)
        {
            try
            {
                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlCommand cmd = new SqlCommand(SQL);
                cmd.Connection = con;
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
                WriteLog.WriteToFile("InsertSummaryData:" + ex.Message);
                return null;
            }
        }

        public void InsertSummaryData(CompanySummaryReportData data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "INSERT INTO [Company_SummaryReport_Data] (LogoPath,CompanyName,CompanyHeaderTitle,CompanyPhoneAddress,CompanyFooter) VALUES(@LogoPath,@CompanyName,@CompanyHeaderTitle,@CompanyPoneAddress,@CompanyFooter)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@LogoPath", SqlDbType.VarChar).Value = data.LogoPath;
                cmd.Parameters.Add("@CompanyName", SqlDbType.VarChar).Value = data.CompanyName;
                cmd.Parameters.Add("@CompanyHeaderTitle", SqlDbType.VarChar).Value = data.CompanyHeaderTitle;
                cmd.Parameters.Add("@CompanyPoneAddress", SqlDbType.VarChar).Value = data.CompanyPhoneAddress;
                cmd.Parameters.Add("@CompanyFooter", SqlDbType.VarChar).Value = data.CompanyFooter;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InsertSummaryData:" + ex.Message);
            }
        }
        public void UpdateSummaryData(CompanySummaryReportData data)
        {
            try
            {

                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "Update Company_SummaryReport_Data SET LogoPath=@LogoPath,CompanyName=@CompanyName,CompanyHeaderTitle=@CompanyHeaderTitle,CompanyPhoneAddress=@CompanyPhoneAddress,CompanyFooter=@CompanyFooter";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@LogoPath", SqlDbType.VarChar).Value = data.LogoPath;
                cmd.Parameters.Add("@CompanyName", SqlDbType.VarChar).Value = data.CompanyName;
                cmd.Parameters.Add("@CompanyHeaderTitle", SqlDbType.VarChar).Value = data.CompanyHeaderTitle;
                cmd.Parameters.Add("@CompanyPhoneAddress", SqlDbType.VarChar).Value = data.CompanyPhoneAddress;
                cmd.Parameters.Add("@CompanyFooter", SqlDbType.VarChar).Value = data.CompanyFooter;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdateSummaryData : " + ex.Message);
            }
        }

        public void DeleteSummaryData(CompanySummaryReportData data)
        {
            try
            {

                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "DELETE FROM Company_SummaryReport_Data WHERE LogoPath=@LogoPath AND CompanyName=@CompanyName AND CompanyHeaderTitle=@CompanyHeaderTitle AND CompanyPhoneAddress=@CompanyPhoneAddress AND CompanyFooter=@CompanyFooter";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@LogoPath", SqlDbType.VarChar).Value = data.LogoPath;
                cmd.Parameters.Add("@CompanyName", SqlDbType.VarChar).Value = data.CompanyName;
                cmd.Parameters.Add("@CompanyHeaderTitle", SqlDbType.VarChar).Value = data.CompanyHeaderTitle;
                cmd.Parameters.Add("@CompanyPhoneAddress", SqlDbType.VarChar).Value = data.CompanyPhoneAddress;
                cmd.Parameters.Add("@CompanyFooter", SqlDbType.VarChar).Value = data.CompanyFooter;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DeleteSummaryData : " + ex.Message);
            }
        }
        #endregion

        #region Hardware
        public void InsertProfileData(Hardware_config data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "INSERT INTO [Hardware_Profile] (ProfileName) VALUES(@profile)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@profile", SqlDbType.VarChar).Value = data.profile;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InsertProfileData : " + ex.Message);
            }
        }
        public void UpdateProfileData(Hardware_config data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "Update Hardware_Profile SET ProfileName=@profile";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@profile", SqlDbType.VarChar).Value = data.profile;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InsertProfileData : " + ex.Message);
            }
        }
        public void DeleteProfileData(Hardware_config data)
        {
            try
            {

                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "DELETE FROM Hardware_Profile WHERE ProfileName=@profile";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@profile", SqlDbType.VarChar).Value = data.profile;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DeleteSummaryData : " + ex.Message);
            }
        }
        #endregion

        #region SMS
        public DataTable GetSMSData(string SQL)
        {
            try
            {
                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlCommand cmd = new SqlCommand(SQL);
                cmd.Connection = con;
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
                WriteLog.WriteToFile("GetSMSData:" + ex.Message);
                return null;
            }
        }
        public void InsertSMSData(SMS_config data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "INSERT INTO [SMS_Template] (Content,PhoneNo1,PhoneNo2,PhoneNo3,ProviderUserName,Password,UseGSM) VALUES(@Content,@PhoneNo1,@PhoneNo2,@PhoneNo3,@ProviderUserName,@Password,@UseGSM)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Content", SqlDbType.VarChar).Value = data.content;
                cmd.Parameters.Add("@PhoneNo1", SqlDbType.VarChar).Value = data.phone1;
                cmd.Parameters.Add("@PhoneNo2", SqlDbType.VarChar).Value = data.phone2;
                cmd.Parameters.Add("@PhoneNo3", SqlDbType.VarChar).Value = data.phone3;
                cmd.Parameters.Add("@ProviderUserName", SqlDbType.VarChar).Value = data.username;
                cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = data.password;
                cmd.Parameters.Add("@UseGSM", SqlDbType.VarChar).Value = data.GSM;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InsertSMSData:" + ex.Message);
            }
        }
        public void UpdateSMSData(SMS_config data)
        {
            try
            {

                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "Update SMS_Template SET  Content=@Content,PhoneNo1=@PhoneNo1,PhoneNo2=@PhoneNo2,PhoneNo3=@PhoneNo3,ProviderUserName=@ProviderUserName,Password=@Password,UseGSM=@UseGSM ";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Content", SqlDbType.VarChar).Value = data.content;
                cmd.Parameters.Add("@PhoneNo1", SqlDbType.VarChar).Value = data.phone1;
                cmd.Parameters.Add("@PhoneNo2", SqlDbType.VarChar).Value = data.phone2;
                cmd.Parameters.Add("@PhoneNo3", SqlDbType.VarChar).Value = data.phone3;
                cmd.Parameters.Add("@ProviderUserName", SqlDbType.VarChar).Value = data.username;
                cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = data.password;
                cmd.Parameters.Add("@UseGSM", SqlDbType.VarChar).Value = data.GSM;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdateSMSData:" + ex.Message);
            }
        }
        public void DeleteSMSData(SMS_config data)
        {
            try
            {

                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "DELETE FROM SMS_Template WHERE Content=@Content AND PhoneNo1=@PhoneNo1 AND PhoneNo2=@PhoneNo2 AND PhoneNo3=@PhoneNo3 AND ProviderUserName=@ProviderUserName AND Password=@Password AND UseGSM=@UseGSM";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Content", SqlDbType.VarChar).Value = data.content;
                cmd.Parameters.Add("@PhoneNo1", SqlDbType.VarChar).Value = data.phone1;
                cmd.Parameters.Add("@PhoneNo2", SqlDbType.VarChar).Value = data.phone2;
                cmd.Parameters.Add("@PhoneNo3", SqlDbType.VarChar).Value = data.phone3;
                cmd.Parameters.Add("@ProviderUserName", SqlDbType.VarChar).Value = data.username;
                cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = data.password;
                cmd.Parameters.Add("@UseGSM", SqlDbType.VarChar).Value = data.GSM;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DeleteSummaryData : " + ex.Message);
            }
        }
        #endregion

        #region Others_setting

        public DataTable GetOthersData(string SQL)
        {
            try
            {
                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlCommand cmd = new SqlCommand(SQL);
                cmd.Connection = con;
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
                WriteLog.WriteToFile("GetOthersData:" + ex.Message);
                return null;
            }
        }
        public void InsertOthersData(OtherSettings data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "INSERT INTO [Other_Settings] (DefaultTransaction,DualScaleSet,SMSAlerts,SaveTransTxt,TareExpirePeriod,AutoPrint,NoOfCopies,DosPrint,AutoMail,AutoDelete,AutoFtMail,AutoFtPrint,AutoFtSMS,StableSensorConfigForVPS,StableWeightCount,MinimumWeightCount,StablePLCCount,AutoPrintPreview) VALUES(@DefaultTransaction,@DualScaleSet,@SMSAlerts,@SaveTransTxt,@TareExpirePeriod,@AutoPrint,@NoOfCopies,@DosPrint,@AutoMail,@AutoDelete,@AutoFtMail,@AutoFtPrint,@AutoFtSMS,@StableSensorConfigForVPS,@StableWeightCount,@MinimumWeightCount,@StablePLCCount,@AutoPrintPreview,@AutoCopies)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@DefaultTransaction", SqlDbType.VarChar).Value = data.DefaultTransaction;
                cmd.Parameters.Add("@DualScaleSet", SqlDbType.VarChar).Value = false;
                cmd.Parameters.Add("@SMSAlerts", SqlDbType.VarChar).Value = data.SMSAlerts;
                cmd.Parameters.Add("@SaveTransTxt", SqlDbType.VarChar).Value = false;
                cmd.Parameters.Add("@TareExpirePeriod", SqlDbType.VarChar).Value = data.TareExpirePeriod;
                cmd.Parameters.Add("@AutoPrint", SqlDbType.VarChar).Value = data.AutoPrint;
                cmd.Parameters.Add("@NoOfCopies", SqlDbType.VarChar).Value = data.NoOfCopies;
                cmd.Parameters.Add("@DosPrint", SqlDbType.VarChar).Value = data.DosPrint;
                cmd.Parameters.Add("@AutoMail", SqlDbType.VarChar).Value = data.AutoMail;
                cmd.Parameters.Add("@AutoDelete", SqlDbType.VarChar).Value = false;
                cmd.Parameters.Add("@AutoFtMail", SqlDbType.VarChar).Value = data.AutoFtMail;
                cmd.Parameters.Add("@AutoFtPrint", SqlDbType.VarChar).Value = data.AutoFtPrint;
                cmd.Parameters.Add("@AutoFtSMS", SqlDbType.VarChar).Value = data.AutoFtSMS;
                cmd.Parameters.Add("@StableSensorConfigForVPS", SqlDbType.VarChar).Value = data.StableSensorConfigForVPS;
                cmd.Parameters.Add("@StableWeightCount", SqlDbType.VarChar).Value = data.StableWeightCount;
                cmd.Parameters.Add("@MinimumWeightCount", SqlDbType.VarChar).Value = data.MinimumWeightCount;
                cmd.Parameters.Add("@StablePLCCount", SqlDbType.VarChar).Value = data.StablePLCCount;
                cmd.Parameters.Add("@AutoPrintPreview", SqlDbType.VarChar).Value = data.AutoPrintPreview;
                cmd.Parameters.Add("@AutoCopies", SqlDbType.VarChar).Value = data.AutoCopies;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InsertOthersData:" + ex.Message);
            }
        }
        public void UpdateOthersData(OtherSettings data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "Update Other_Settings SET  DefaultTransaction=@DefaultTransaction,DualScaleSet=@DualScaleSet,SMSAlerts=@SMSAlerts,SaveTransTxt=@SaveTransTxt,TareExpirePeriod=@TareExpirePeriod,AutoPrint=@AutoPrint,NoOfCopies=@NoOfCopies,DosPrint=@DosPrint,AutoMail=@AutoMail,AutoDelete=@AutoDelete,AutoFtMail=@AutoFtMail,AutoFtPrint=@AutoFtPrint,AutoFtSMS=@AutoFtSMS,StableSensorConfigForVPS=@StableSensorConfigForVPS,StableWeightCount=@StableWeightCount,MinimumWeightCount=@MinimumWeightCount,StablePLCCount=@StablePLCCount,AutoPrintPreview=@AutoPrintPreview";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@DefaultTransaction", SqlDbType.VarChar).Value = data.DefaultTransaction;
                cmd.Parameters.Add("@DualScaleSet", SqlDbType.Bit).Value = false;
                cmd.Parameters.Add("@SMSAlerts", SqlDbType.VarChar).Value = data.SMSAlerts;
                cmd.Parameters.Add("@SaveTransTxt", SqlDbType.Bit).Value = false;
                cmd.Parameters.Add("@TareExpirePeriod", SqlDbType.VarChar).Value = data.TareExpirePeriod;
                cmd.Parameters.Add("@AutoPrint", SqlDbType.Bit).Value = data.AutoPrint;
                cmd.Parameters.Add("@NoOfCopies", SqlDbType.VarChar).Value = data.NoOfCopies;
                cmd.Parameters.Add("@DosPrint", SqlDbType.Bit).Value = data.DosPrint;
                cmd.Parameters.Add("@AutoMail", SqlDbType.Bit).Value = data.AutoMail;
                cmd.Parameters.Add("@AutoDelete", SqlDbType.Bit).Value = false;
                cmd.Parameters.Add("@AutoFtMail", SqlDbType.Bit).Value = data.AutoFtMail;
                cmd.Parameters.Add("@AutoFtPrint", SqlDbType.Bit).Value = data.AutoFtPrint;
                cmd.Parameters.Add("@AutoFtSMS", SqlDbType.Bit).Value = data.AutoFtSMS;
                cmd.Parameters.Add("@StableSensorConfigForVPS", SqlDbType.VarChar).Value = data.StableSensorConfigForVPS;
                cmd.Parameters.Add("@StableWeightCount", SqlDbType.VarChar).Value = data.StableWeightCount;
                cmd.Parameters.Add("@MinimumWeightCount", SqlDbType.VarChar).Value = data.MinimumWeightCount;
                cmd.Parameters.Add("@StablePLCCount", SqlDbType.VarChar).Value = data.StablePLCCount;
                cmd.Parameters.Add("@AutoPrintPreview", SqlDbType.VarChar).Value = data.AutoPrintPreview;
                cmd.Parameters.Add("@AutoCopies", SqlDbType.Bit).Value = data.AutoCopies;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdateOthersData:" + ex.Message);
            }
        }
        #endregion

        #region Software
        public DataTable GetSoftwareData(string SQL)
        {
            try
            {
                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlCommand cmd = new SqlCommand(SQL);
                cmd.Connection = con;
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
                WriteLog.WriteToFile("GetOthersData:" + ex.Message);
                return null;
            }
        }
        public void InsertSoftwareData(Software_config data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "INSERT INTO [Software_SettingConfig] (Single_Transaction,First_Transaction,Second_Transaction,First_MultiTransaction,Second_MultiTransaction,Single_Axle,First_Axle,Second_Axle,Loading,Unloading) VALUES(@Single_Transaction,@First_Transaction,@Second_Transaction,@First_MultiTransaction,@Second_MultiTransaction,@Single_Axle,@First_Axle,@Second_Axle,@Loading,@Unloading)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Single_Transaction", SqlDbType.VarChar).Value = data.Single;
                cmd.Parameters.Add("@First_Transaction", SqlDbType.VarChar).Value = data.First;
                cmd.Parameters.Add("@Second_Transaction", SqlDbType.VarChar).Value = data.Second;
                cmd.Parameters.Add("@First_MultiTransaction", SqlDbType.VarChar).Value = data.First_multi;
                cmd.Parameters.Add("@Second_MultiTransaction", SqlDbType.VarChar).Value = data.Second_multi;
                cmd.Parameters.Add("@Single_Axle", SqlDbType.VarChar).Value = data.Single_axle;
                cmd.Parameters.Add("@First_Axle", SqlDbType.VarChar).Value = data.First_axle;
                cmd.Parameters.Add("@Second_Axle", SqlDbType.VarChar).Value = data.Second_axle;
                cmd.Parameters.Add("@Loading", SqlDbType.VarChar).Value = data.Loading;
                cmd.Parameters.Add("@Unloading", SqlDbType.VarChar).Value = data.Unloading;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InsertOthersData:" + ex.Message);
            }
        }
        public void UpdateSoftwareData(Software_config data)
        {
            try
            {

                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "Update Software_SettingConfig SET  Single_Transaction=@Single_Transaction,First_Transaction=@First_Transaction,Second_Transaction=@Second_Transaction,First_MultiTransaction=@First_MultiTransaction,Second_MultiTransaction=@Second_MultiTransaction,Single_Axle=@Single_Axle,First_Axle=@First_Axle,Second_Axle=@Second_Axle,Loading=@Loading,Unloading=@Unloading";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Single_Transaction", SqlDbType.VarChar).Value = data.Single;
                cmd.Parameters.Add("@First_Transaction", SqlDbType.VarChar).Value = data.First;
                cmd.Parameters.Add("@Second_Transaction", SqlDbType.VarChar).Value = data.Second;
                cmd.Parameters.Add("@First_MultiTransaction", SqlDbType.VarChar).Value = data.First_multi;
                cmd.Parameters.Add("@Second_MultiTransaction", SqlDbType.VarChar).Value = data.Second_multi;
                cmd.Parameters.Add("@Single_Axle", SqlDbType.VarChar).Value = data.Single_axle;
                cmd.Parameters.Add("@First_Axle", SqlDbType.VarChar).Value = data.First_axle;
                cmd.Parameters.Add("@Second_Axle", SqlDbType.VarChar).Value = data.Second_axle;
                cmd.Parameters.Add("@Loading", SqlDbType.VarChar).Value = data.Loading;
                cmd.Parameters.Add("@Unloading", SqlDbType.VarChar).Value = data.Unloading;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdateOthersData:" + ex.Message);
            }
        }
        #endregion

        #region File_setting
        public DataTable GetFileData(string SQL)
        {
            try
            {
                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlCommand cmd = new SqlCommand(SQL);
                cmd.Connection = con;
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
                WriteLog.WriteToFile("GetFileData:" + ex.Message);
                return null;
            }
        }
        public void InsertFileData(File_config data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "INSERT INTO [FileLocation_Setting] (Log_Path,IReport_Path,IReport,Report_Temp,Transaction_Log,Ticket_Temp,Default_Ticket,Default_Temp,Weigh,WeighIndicator) VALUES(@Log_Path,@IReport_Path,@IReport,@Report_Temp,@Transaction_Log,@Ticket_Temp,@Default_Ticket,@Default_Temp,@Weigh,@WeighIndicator)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Log_Path", SqlDbType.VarChar).Value = data.Log;
                cmd.Parameters.Add("@IReport_Path", SqlDbType.VarChar).Value = data.IReport_Path;
                cmd.Parameters.Add("@IReport", SqlDbType.VarChar).Value = data.IReport;
                cmd.Parameters.Add("@Report_Temp", SqlDbType.VarChar).Value = data.Report;
                cmd.Parameters.Add("@Transaction_Log", SqlDbType.VarChar).Value = data.Trans_Log;
                cmd.Parameters.Add("@Ticket_Temp", SqlDbType.VarChar).Value = data.Ticket_temp;
                cmd.Parameters.Add("@Default_Ticket", SqlDbType.VarChar).Value = data.Default_ticket;
                cmd.Parameters.Add("@Default_Temp", SqlDbType.VarChar).Value = data.Default_temp;
                cmd.Parameters.Add("@Weigh", SqlDbType.VarChar).Value = data.Weigh;
                cmd.Parameters.Add("@WeighIndicator", SqlDbType.VarChar).Value = data.WeighIndicator;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InsertFileData:" + ex.Message);
            }
        }
        public void UpdateFileData(File_config data)
        {
            try
            {

                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "Update FileLocation_Setting SET  Log_Path=@Log_Path,IReport_Path=@IReport_Path,IReport=@IReport,Report_Temp=@Report_Temp,Transaction_Log=@Transaction_Log,Ticket_Temp=@Ticket_Temp,Default_Ticket=@Default_Ticket,Default_Temp=@Default_Temp,Weigh=@Weigh,WeighIndicator=@WeighIndicator";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Log_Path", SqlDbType.VarChar).Value = data.Log;
                cmd.Parameters.Add("@IReport_Path", SqlDbType.VarChar).Value = data.IReport_Path;
                cmd.Parameters.Add("@IReport", SqlDbType.VarChar).Value = data.IReport;
                cmd.Parameters.Add("@Report_Temp", SqlDbType.VarChar).Value = data.Report;
                cmd.Parameters.Add("@Transaction_Log", SqlDbType.VarChar).Value = data.Trans_Log;
                cmd.Parameters.Add("@Ticket_Temp", SqlDbType.VarChar).Value = data.Ticket_temp;
                cmd.Parameters.Add("@Default_Ticket", SqlDbType.VarChar).Value = data.Default_ticket;
                cmd.Parameters.Add("@Default_Temp", SqlDbType.VarChar).Value = data.Default_temp;
                cmd.Parameters.Add("@Weigh", SqlDbType.VarChar).Value = data.Weigh;
                cmd.Parameters.Add("@WeighIndicator", SqlDbType.VarChar).Value = data.WeighIndicator;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdateFileData:" + ex.Message);
            }
        }
        public void DeleteFileData(File_config data)
        {
            try
            {

                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "DELETE FROM FileLocation_Setting WHERE  Log_Path=@Log_Path AND IReport_Path=@IReport_Path AND IReport=@IReport AND Report_Temp=@Report_Temp AND Transaction_Log=@Transaction_Log AND Ticket_Temp=@Ticket_Temp AND Default_Ticket=@Default_Ticket AND Default_Temp=@Default_Temp AND Weigh=@Weigh AND WeighIndicator=@WeighIndicator";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Log_Path", SqlDbType.VarChar).Value = data.Log;
                cmd.Parameters.Add("@IReport_Path", SqlDbType.VarChar).Value = data.IReport_Path;
                cmd.Parameters.Add("@IReport", SqlDbType.VarChar).Value = data.IReport;
                cmd.Parameters.Add("@Report_Temp", SqlDbType.VarChar).Value = data.Report;
                cmd.Parameters.Add("@Transaction_Log", SqlDbType.VarChar).Value = data.Trans_Log;
                cmd.Parameters.Add("@Ticket_Temp", SqlDbType.VarChar).Value = data.Ticket_temp;
                cmd.Parameters.Add("@Default_Ticket", SqlDbType.VarChar).Value = data.Default_ticket;
                cmd.Parameters.Add("@Default_Temp", SqlDbType.VarChar).Value = data.Default_temp;
                cmd.Parameters.Add("@Weigh", SqlDbType.VarChar).Value = data.Weigh;
                cmd.Parameters.Add("@WeighIndicator", SqlDbType.VarChar).Value = data.WeighIndicator;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DeleteFileData:" + ex.Message);
            }
        }
        #endregion


    }

}
