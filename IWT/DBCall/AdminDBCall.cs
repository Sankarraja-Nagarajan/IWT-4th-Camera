using IWT.Admin_Pages;
using IWT.Models;
using IWT.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using static IWT.Admin_Pages.ManageUser;
using static IWT.Admin_Pages.Role;
using static IWT.TransactionPages.Addmaterial;
using static IWT.TransactionPages.Addsupplier;
using static IWT.TransactionPages.FirstVehicle;

using static IWT.Saved_Template.CreateTemplate;

namespace IWT.DBCall
{
    public class AdminDBCall
    {
        private string ConnectionString { get; set; }
        public Viewvehicle DataContext { get; }

        public AdminDBCall()
        {
            //ConnectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
            ConnectionString = GetDecryptedConnectionStringDB();
            DataContext = new Viewvehicle();
        }
        public string GetDecryptedConnectionStringDB()
        {
            try
            {
                //var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString);
                //var x= System.Convert.ToBase64String(plainTextBytes);
                byte[] b = Convert.FromBase64String(ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString);
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
        //public void InsertMaterialData(material_details1 data)
        //{
        //    try
        //    {
        //        SqlConnection con = new SqlConnection(ConnectionString);
        //        string sql = "INSERT INTO [Material_Master] (MaterialCode,MaterialName,IsDeleted) " +
        //            "VALUES(@MaterialCode,@MaterialName,@IsDeleted)";
        //        SqlCommand cmd = new SqlCommand(sql, con);
        //        cmd.Parameters.Add("@MaterialCode", SqlDbType.VarChar).Value = data.MaterialCode;
        //        cmd.Parameters.Add("@MaterialName", SqlDbType.VarChar).Value = data.MaterialName;
        //        cmd.Parameters.Add("@IsDeleted", SqlDbType.VarChar).Value = data.IsDeleted;

        //        con.Open();
        //        cmd.ExecuteNonQuery();
        //        cmd.Dispose();
        //        con.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog.WriteToFile("InsertMaterialData:" + ex.Message);
        //    }
        //}
        public void InsertsingletransactionData(transaction_details data)
        {
            try
            {

                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "INSERT INTO [Transaction] (VehicleNo,MaterialName,SupplierName,NoOfMaterial,Date,EmptyWeight,LoadWeight,EmptyWeightDate,LoadWeightDate,EmptyWeightTime,LoadWeightTime,NetWeight,Pending,Closed,MultiWeight,MultiWeightTransPending,State,TransactionType,LoadStatus,CreatedOn) " +
                "VALUES(@VehicleNo,@MaterialName,@SupplierName,@NoOfMaterial,@Date,@EmptyWeight,@LoadWeight,@EmptyWeightDate,@LoadWeightDate,@EmptyWeightTime,@LoadWeightTime,@Netweight,@Pending,@Closed,@Multiweight,@MultiweightTransPending,@State,@TransactionType,@LoadStatus,@CreatedOn)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@VehicleNo", SqlDbType.VarChar).Value = data.VehicleNo;
                cmd.Parameters.AddWithValue("@MaterialName", data.MaterialName as object ?? DBNull.Value);
                cmd.Parameters.Add("@SupplierName", SqlDbType.VarChar).Value = data.SupplierName as object ?? DBNull.Value;
                cmd.Parameters.Add("@NoOfMaterial", SqlDbType.VarChar).Value = data.NoOfMaterial;
                cmd.Parameters.Add("@Date", SqlDbType.DateTime).Value = data.Date;
                cmd.Parameters.Add("@EmptyWeight", SqlDbType.Int).Value = data.EmptyWeight;
                cmd.Parameters.Add("@LoadWeight", SqlDbType.Int).Value = data.LoadWeight;
                cmd.Parameters.Add("@EmptyWeightDate", SqlDbType.DateTime).Value = data.EmptyWeightDate;
                cmd.Parameters.Add("@LoadWeightDate", SqlDbType.DateTime).Value = data.LoadWeightDate;
                cmd.Parameters.Add("@EmptyWeightTime", SqlDbType.DateTime).Value = data.EmptyWeightTime;
                cmd.Parameters.Add("@LoadWeightTime", SqlDbType.DateTime).Value = data.LoadWeightTime;
                cmd.Parameters.Add("@Netweight", SqlDbType.Int).Value = data.Netweight;
                cmd.Parameters.Add("@Pending", SqlDbType.Bit).Value = data.Pending;
                cmd.Parameters.Add("@Closed", SqlDbType.Bit).Value = data.Closed;
                cmd.Parameters.Add("@Multiweight", SqlDbType.Bit).Value = data.Multiweight;
                cmd.Parameters.Add("@MultiweightTransPending", SqlDbType.Bit).Value = data.MultiweightTransPending;
                cmd.Parameters.Add("@State", SqlDbType.NVarChar).Value = data.State;
                cmd.Parameters.Add("@TransactionType", SqlDbType.NVarChar).Value = data.TransactionType;
                cmd.Parameters.Add("@LoadStatus", SqlDbType.NVarChar).Value = data.LoadStatus;
                cmd.Parameters.AddWithValue("@CreatedOn", DateTime.Now);
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InsertMaterialData:" + ex.Message);
            }
        }
        public DataTable GetAllData(string SQL)
        {
            try
            {
                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(GetDecryptedConnectionStringDB());
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
                WriteLog.WriteToFile("GetAllData:" + ex.Message);
                return null;
            }
        }

        public void InsertsecondtransactionData(transaction_details data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "INSERT INTO [Transaction] (VehicleNo,MaterialName,SupplierName,NoOfMaterial,Date,EmptyWeight,LoadWeight,EmptyWeightDate,LoadWeightDate,NetWeight,Pending,Closed,MultiWeight,MultiWeightTransPending) " +
                "VALUES(@VehicleNo,@MaterialName,@SupplierName,@NoOfMaterial,@Date,@EmptyWeight,@LoadWeight,@EmptyWeightDate,@LoadWeightDate,@Netweight,@Pending,@Closed,@Multiweight,@MultiweightTransPending)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@VehicleNo", SqlDbType.VarChar).Value = data.VehicleNo;
                cmd.Parameters.Add("@MaterialName", SqlDbType.VarChar).Value = data.MaterialName;
                cmd.Parameters.Add("@SupplierName", SqlDbType.VarChar).Value = data.SupplierName;
                cmd.Parameters.Add("@NoOfMaterial", SqlDbType.VarChar).Value = data.NoOfMaterial;
                cmd.Parameters.Add("@Date", SqlDbType.DateTime).Value = data.Date;
                cmd.Parameters.Add("@EmptyWeight", SqlDbType.Int).Value = data.EmptyWeight;
                cmd.Parameters.Add("@LoadWeight", SqlDbType.Int).Value = data.LoadWeight;
                cmd.Parameters.Add("@EmptyWeightDate", SqlDbType.DateTime).Value = data.EmptyWeightDate;
                cmd.Parameters.Add("@LoadWeightDate", SqlDbType.DateTime).Value = data.LoadWeightDate;
                cmd.Parameters.Add("@Netweight", SqlDbType.Int).Value = data.Netweight;
                cmd.Parameters.Add("@Pending", SqlDbType.Bit).Value = data.Pending;
                cmd.Parameters.Add("@Closed", SqlDbType.Bit).Value = data.Closed;
                cmd.Parameters.Add("@Multiweight", SqlDbType.Bit).Value = data.Multiweight;
                cmd.Parameters.Add("@MultiweightTransPending", SqlDbType.Bit).Value = data.MultiweightTransPending;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InsertMaterialData:" + ex.Message);
            }
        }

        #region UserManage
        public void InsertUserManageData(Usermanage data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "INSERT INTO [User_Management] (UserName,Password,EmailID,Role,HardwareProfile,IsLocked) VALUES(@UserName,@Password,@EmailID,@Role,@HardwareProfile,@IsLocked)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = data.Name;
                cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = data.Password;
                cmd.Parameters.Add("@EmailID", SqlDbType.VarChar).Value = data.Email;
                cmd.Parameters.Add("@Role", SqlDbType.VarChar).Value = data.Role;
                cmd.Parameters.Add("@HardwareProfile", SqlDbType.VarChar).Value = data.Profile;
                cmd.Parameters.Add("@IsLocked", SqlDbType.VarChar).Value = data.Locked;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InsertUserManageData:" + ex.Message);
            }
        }
        public void UpdateUsermanageData(Usermanage data)
        {
            try
            {

                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "Update User_Management SET  UserName=@UserName,Password=@Password,EmailID=@EmailID,Role=@Role,HardwareProfile=@HardwareProfile,IsLocked=@IsLocked Where ID =" + data.ID;
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = data.Name;
                cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = data.Password;
                cmd.Parameters.Add("@EmailID", SqlDbType.VarChar).Value = data.Email;
                cmd.Parameters.Add("@Role", SqlDbType.VarChar).Value = data.Role;
                cmd.Parameters.Add("@HardwareProfile", SqlDbType.VarChar).Value = data.Profile;
                cmd.Parameters.Add("@IsLocked", SqlDbType.VarChar).Value = data.Locked;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdateUsermanageData:" + ex.Message);
            }
        }
        public void DeleteUserManageData(Usermanage data)
        {
            try
            {

                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "DELETE FROM User_Management WHERE ID= " + "'" + data.ID + "'";
                //"DELETE FROM SMS_Template WHERE Content=@Content AND PhoneNo1=@PhoneNo1 AND PhoneNo2=@PhoneNo2 AND PhoneNo3=@PhoneNo3 AND ProviderUserName=@ProviderUserName AND Password=@Password AND UseGSM=@UseGSM"
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = data.Name;
                cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = data.Password;
                cmd.Parameters.Add("@EmailID", SqlDbType.VarChar).Value = data.Email;
                cmd.Parameters.Add("@Role", SqlDbType.VarChar).Value = data.Role;
                cmd.Parameters.Add("@HardwareProfile", SqlDbType.VarChar).Value = data.Profile;
                cmd.Parameters.Add("@IsLocked", SqlDbType.VarChar).Value = data.Locked;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DeleteUserManageData : " + ex.Message);
            }
        }
        #endregion
        #region TicketEntry

        public bool ExecuteQuery(string query)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                con.Open();
                try
                {
                    new SqlCommand(query, con).ExecuteNonQuery();
                }
                catch (Exception exp)
                {
                    throw exp;
                }
                con.Close();
                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("ExecuteQuery : " + ex.Message);
                return false;
            }
        }

        public bool ExecuteQuery(SqlCommand command)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                con.Open();
                try
                {
                    command.Connection = con;
                    command.ExecuteNonQuery();
                }
                catch (Exception exp)
                {
                    throw exp;
                }
                con.Close();
                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("ExecuteQuery : " + ex.Message);
                return false;
            }
        }

        public bool CreateCustomTable(string tableName)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlTransaction transaction;
                string sql = $@"CREATE TABLE [{tableName}] (ID INT IDENTITY(1,1) PRIMARY KEY,IsDeleted BIT NOT NULL)";
                string sql1 = $@"INSERT INTO [Custom_Master_List] (CutomMasterName) VALUES ('{tableName}')";
                con.Open();
                transaction = con.BeginTransaction();
                try
                {
                    new SqlCommand(sql, con, transaction).ExecuteNonQuery();
                    new SqlCommand(sql1, con, transaction).ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception exp)
                {
                    transaction.Rollback();
                    throw exp;
                }
                con.Close();
                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DeleteUserManageData : " + ex.Message);
                return false;
            }
        }

        public bool DeleteCustomField(TicketDataTemplate ticketData)
        {
            return true;
        }
        public bool DeleteCustomTable(string tableName)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlTransaction transaction;
                string sql = $@"DROP TABLE [{tableName}]";
                string sql1 = $@"DELETE FROM [Custom_Master_List] WHERE CutomMasterName='{tableName}'";
                string sql2 = $@"DELETE FROM [Ticket_Data_Template] WHERE F_Table='{tableName}'";
                con.Open();
                transaction = con.BeginTransaction();
                try
                {
                    new SqlCommand(sql, con, transaction).ExecuteNonQuery();
                    new SqlCommand(sql1, con, transaction).ExecuteNonQuery();
                    new SqlCommand(sql2, con, transaction).ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception exp)
                {
                    transaction.Rollback();
                    throw exp;
                }
                con.Close();
                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DeleteUserManageData : " + ex.Message);
                return false;
            }
        }

        public bool DeleteMasterRow(string idField, string id, string tableName)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlTransaction transaction;
                string sql = $@"UPDATE [{tableName}] SET [IsDeleted]='TRUE' WHERE [{idField}]='{id}'";
                con.Open();
                transaction = con.BeginTransaction();
                try
                {
                    new SqlCommand(sql, con, transaction).ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception exp)
                {
                    transaction.Rollback();
                    throw exp;
                }
                con.Close();
                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DeleteMasterRow : " + ex.Message);
                return false;
            }
        }
        public bool CreateCustomField(TicketDataTemplate ticketData)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlTransaction transaction;
                string sql1 = $@"INSERT INTO [Ticket_Data_Template] (ControlType,ControlTable,ControlTableRef,ControlDisableFirst,ControlDisableSecond,ControlDisableSingle,ControlLoadStatusDisable,Dependent,F_Table,F_FieldName,F_Caption,F_Type,F_Size,SelectionBasis,Mandatory,MandatoryStatus) VALUES ('{ticketData.ControlType}','{ticketData.ControlTable}','{ticketData.ControlTableRef}','{ticketData.ControlDisableFirst}','{ticketData.ControlDisableSecond}','{ticketData.ControlDisableSingle}','{ticketData.ControlLoadStatusDisable}','{ticketData.Dependent}','{ticketData.F_Table}','{GetTablePrefix(ticketData.F_Table)}{ticketData.F_FieldName}','{ticketData.F_Caption}','{ticketData.F_Type}','{ticketData.F_Size}','{ticketData.SelectionBasis}','{ticketData.Mandatory}','{ticketData.MandatoryStatus}')";
                string sql2 = $@"ALTER TABLE [{ticketData.F_Table}] ADD [{GetTablePrefix(ticketData.F_Table)}{ticketData.F_FieldName}] {ticketData.F_Type}({ticketData.F_Size})";
                string sql3 = $@"ALTER TABLE [{ticketData.F_Table}] ADD [{GetTablePrefix(ticketData.F_Table)}{ticketData.F_FieldName}] {ticketData.F_Type} NULL";
                con.Open();
                transaction = con.BeginTransaction();
                try
                {
                    new SqlCommand(sql1, con, transaction).ExecuteNonQuery();
                    if (ticketData.F_Type == "NVARCHAR")
                    {
                        new SqlCommand(sql2, con, transaction).ExecuteNonQuery();
                    }
                    else
                    {
                        new SqlCommand(sql3, con, transaction).ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch (Exception exp)
                {
                    transaction.Rollback();
                    throw exp;
                }
                con.Close();
                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateCustomField : " + ex.Message);
                return false;
            }
        }
        public bool SaveFormulaField(FormulaTemplate formula, TicketDataTemplate ticketData)
        {
            string sql1 = $@"INSERT INTO [Ticket_Data_Template] (ControlType,ControlTable,ControlTableRef,ControlDisableFirst,ControlDisableSecond,ControlDisableSingle,ControlLoadStatusDisable,Dependent,F_Table,F_FieldName,F_Caption,F_Type,F_Size,SelectionBasis,Mandatory,MandatoryStatus) VALUES ('{ticketData.ControlType}','{ticketData.ControlTable}','{ticketData.ControlTableRef}','{ticketData.ControlDisableFirst}','{ticketData.ControlDisableSecond}','{ticketData.ControlDisableSingle}','{ticketData.ControlLoadStatusDisable}','{ticketData.Dependent}','{ticketData.F_Table}','{GetTablePrefix(ticketData.F_Table)}{ticketData.F_FieldName}','{ticketData.F_Caption}','{ticketData.F_Type}','{ticketData.F_Size}','{ticketData.SelectionBasis}','{ticketData.Mandatory}','{ticketData.MandatoryStatus}')";
            string sql2 = $@"ALTER TABLE [{ticketData.F_Table}] ADD [{GetTablePrefix(ticketData.F_Table)}{ticketData.F_FieldName}] {ticketData.F_Type}({ticketData.F_Size})";
            string sql3 = $@"INSERT INTO [Formula_Table] (FormulaName,FormulaList) VALUES('{formula.FormulaName}','{formula.FormulaList}')";
            string sql4 = $@"UPDATE [Formula_Table] SET FormulaList='{formula.FormulaList}' WHERE FormulaID='{formula.FormulaID}'";
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlTransaction transaction;
                con.Open();
                transaction = con.BeginTransaction();
                try
                {
                    if (formula.FormulaID > 0)
                    {
                        new SqlCommand(sql4, con, transaction).ExecuteNonQuery();
                    }
                    else
                    {
                        new SqlCommand(sql1, con, transaction).ExecuteNonQuery();
                        new SqlCommand(sql2, con, transaction).ExecuteNonQuery();
                        new SqlCommand(sql3, con, transaction).ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch (Exception exp)
                {
                    transaction.Rollback();
                    throw exp;
                }
                con.Close();
                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateCustomField : " + ex.Message);
                return false;
            }
        }
        public string GetTablePrefix(string tableName)
        {
            if (tableName == "Transaction")
            {
                return "tr_";
            }
            else if (tableName == "Material_Master")
            {
                return "mm_";
            }
            else if (tableName == "Supplier_Master")
            {
                return "sm_";
            }
            else if (tableName == "Shift_Master")
            {
                return "shm_";
            }
            else if (tableName == "Vehicle_Master")
            {
                return "vm_";
            }
            else
            {
                return "";
            }
        }
        public bool DropCustomFields(List<TicketDataTemplate> fields)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlTransaction transaction;
                con.Open();
                transaction = con.BeginTransaction();
                try
                {
                    foreach (TicketDataTemplate field in fields)
                    {
                        string dropQuery = $@"ALTER TABLE [{field.F_Table}] DROP COLUMN {field.F_FieldName};";
                        string delQuery = $@"DELETE FROM [Ticket_Data_Template] Where ControlID='{field.ControlID}'";
                        string delFormula = $@"DELETE FROM [Formula_Table] Where FormulaName='{field.F_Caption}'";
                        string delDependency = $@"DELETE FROM [Field_Dependency] Where CustomName='{field.F_FieldName}'";
                        new SqlCommand(dropQuery, con, transaction).ExecuteNonQuery();
                        new SqlCommand(delQuery, con, transaction).ExecuteNonQuery();
                        new SqlCommand(delFormula, con, transaction).ExecuteNonQuery();
                        new SqlCommand(delDependency, con, transaction).ExecuteNonQuery();
                    }
                    transaction.Commit();
                    return true;
                }
                catch (Exception exp)
                {
                    transaction.Rollback();
                    throw exp;
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateCustomField : " + ex.Message);
                return false;
            }
        }
        #endregion

        #region Role/Privilege - General
        public void InsertGeneralData(RolePriviliege data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "INSERT INTO [User_Previledges] (Role,TransactionAccess,MasterAccess,ReportAccess,HardwareSettingsAccess,EmailSettingsAccess,AdminAccess,SettingAccess,SoftwareConfigurationAccess,OtherSettingsAccess,BackupDBAccess,RecordDeletionAccess,ContactsAccess,UserManagementAccess,TicketDataEntryAccess,SendMailAccess,SMSAdminAccess,SalesForceSettingsAccess,DuplicateTicketsAccess,DBMigrationAccess,VehicleMasterAccess,FileLocationSettingsAccess,ReachUs,CCTVSettings,BioMetricSettings,GSMModemSetting,AlprSetting,PLCEnable,PLCPortSetting,WeighBridgeSetting,RFIDPortSetting,RFIDFieldMapping,RFIDWriter,DongleSetting,RemoteDisplaySetting,EditHardwareProfile,SMTPAccess,SummaryReportAccess,MaterialMasterAccess,SupllierMasterAccess,ShiftMasterAccess,DBPswdChangeAccess,DeleteAccess,ImportExportAccess,CloseTickets,RFIDAllocationAccess,RFIDUserTableAccess,GateExitAccess,AWSAccess,SystemConfigurationAccess,SAPSyncAccess,PrintAndDeleteAccess) " +
                    "VALUES(@Role,@TransactionAccess,@MasterAccess,@ReportAccess,@HardwareSettingsAccess,@EmailSettingsAccess,@AdminAccess,@SettingAccess,@SoftwareConfigurationAccess,@OtherSettingsAccess,@BackupDBAccess,@RecordDeletionAccess,@ContactsAccess,@UserManagementAccess,@TicketDataEntryAccess,@SendMailAccess,@SMSAdminAccess,@SalesForceSettingsAccess,@DuplicateTicketsAccess,@DBMigrationAccess,@VehicleMasterAccess,@FileLocationSettingsAccess,@ReachUs,@CCTVSettings,@BioMetricSettings,@GSMModemSetting,@AlprSetting,@PLCEnable,@PLCPortSetting,@WeighBridgeSetting,@RFIDPortSetting,@RFIDFieldMapping,@RFIDWriter,@DongleSetting,@RemoteDisplaySetting,@EditHardwareProfile,@SMTPAccess,@SummaryReportAccess,@MaterialMasterAccess,@SupllierMasterAccess,@ShiftMasterAccess,@DBPswdChangeAccess,@DeleteAccess,@ImportExportAccess,@CloseTickets,@RFIDAllocationAccess,@RFIDUserTableAccess,@GateExitAccess,@AWSAccess,@SystemConfigurationAccess,@SAPSyncAccess,@PrintAndDeleteAccess)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Role", SqlDbType.VarChar).Value = data.Role;
                cmd.Parameters.Add("@TransactionAccess", SqlDbType.VarChar).Value = data.TransactionAccess;
                cmd.Parameters.Add("@MasterAccess", SqlDbType.VarChar).Value = data.MasterAccess;
                cmd.Parameters.Add("@ReportAccess", SqlDbType.VarChar).Value = data.ReportAccess;
                cmd.Parameters.Add("@AdminAccess", SqlDbType.VarChar).Value = data.AdminAccess;
                cmd.Parameters.Add("@SettingAccess", SqlDbType.VarChar).Value = data.SettingAccess;
                cmd.Parameters.Add("@EmailSettingsAccess", SqlDbType.VarChar).Value = data.EmailSettingsAccess;
                cmd.Parameters.Add("@HardwareSettingsAccess", SqlDbType.VarChar).Value = data.HardwareSettingAccess;
                cmd.Parameters.Add("@SoftwareConfigurationAccess", SqlDbType.VarChar).Value = data.SoftwareConfigurationAccess;
                cmd.Parameters.Add("@OtherSettingsAccess", SqlDbType.VarChar).Value = data.OtherSettingsAccess;
                cmd.Parameters.Add("@BackupDBAccess", SqlDbType.VarChar).Value = data.BackupDBAccess;
                cmd.Parameters.Add("@RecordDeletionAccess", SqlDbType.VarChar).Value = data.RecordDeletionAccess;
                cmd.Parameters.Add("@ContactsAccess", SqlDbType.VarChar).Value = data.ContactsAccess;
                cmd.Parameters.Add("@UserManagementAccess", SqlDbType.VarChar).Value = data.UserManagementAccess;
                cmd.Parameters.Add("@TicketDataEntryAccess", SqlDbType.VarChar).Value = data.CustomFieldAccess;
                cmd.Parameters.Add("@SendMailAccess", SqlDbType.VarChar).Value = data.SendMailAccess;
                cmd.Parameters.Add("@SMSAdminAccess", SqlDbType.VarChar).Value = data.SMSAdminAccess;
                cmd.Parameters.Add("@SalesForceSettingsAccess", SqlDbType.VarChar).Value = data.SalesForceSettingsAccess;
                cmd.Parameters.Add("@DuplicateTicketsAccess", SqlDbType.VarChar).Value = data.DuplicateTicketsAccess;
                cmd.Parameters.Add("@DBMigrationAccess", SqlDbType.VarChar).Value = data.DBMigrationAccess;
                cmd.Parameters.Add("@VehicleMasterAccess", SqlDbType.VarChar).Value = data.VehicleMasterAccess;
                cmd.Parameters.Add("@FileLocationSettingsAccess", SqlDbType.VarChar).Value = data.FileLocationSettingsAccess;
                cmd.Parameters.Add("@ReachUs", SqlDbType.VarChar).Value = data.ReachUsAccess;
                cmd.Parameters.Add("@CCTVSettings", SqlDbType.VarChar).Value = data.CCTVSettings;
                cmd.Parameters.Add("@BioMetricSettings", SqlDbType.VarChar).Value = data.BioMetricSettingsAccess;
                cmd.Parameters.Add("@GSMModemSetting", SqlDbType.VarChar).Value = data.GSMModemSettingAccess;
                cmd.Parameters.Add("@AlprSetting", SqlDbType.VarChar).Value = data.ALPRSettingAccess;
                cmd.Parameters.Add("@PLCEnable", SqlDbType.VarChar).Value = data.PLCEnableAccess;
                cmd.Parameters.Add("@PLCPortSetting", SqlDbType.VarChar).Value = data.PLCPortSettingAccess;
                cmd.Parameters.Add("@WeighBridgeSetting", SqlDbType.VarChar).Value = data.WeighBridgeSetting;
                cmd.Parameters.Add("@RFIDPortSetting", SqlDbType.VarChar).Value = data.RFIDPortSettingAccess;
                cmd.Parameters.Add("@RFIDFieldMapping", SqlDbType.VarChar).Value = data.RFIDMasterAccess;
                cmd.Parameters.Add("@RFIDWriter", SqlDbType.VarChar).Value = data.RFIDWriterAccess;
                cmd.Parameters.Add("@DongleSetting", SqlDbType.VarChar).Value = data.DongleSettingAccess;
                cmd.Parameters.Add("@RemoteDisplaySetting", SqlDbType.VarChar).Value = data.RemoteDisplaySettingAccess;
                cmd.Parameters.Add("@EditHardwareProfile", SqlDbType.VarChar).Value = data.EditHardwareProfile;

                cmd.Parameters.Add("@SMTPAccess", SqlDbType.VarChar).Value = data.SMTPAccess;
                cmd.Parameters.Add("@SummaryReportAccess", SqlDbType.VarChar).Value = data.SummaryReportAccess;
                cmd.Parameters.Add("@MaterialMasterAccess", SqlDbType.VarChar).Value = data.MaterialMasterAccess;
                cmd.Parameters.Add("@SupllierMasterAccess", SqlDbType.VarChar).Value = data.SupllierMasterAccess;
                cmd.Parameters.Add("@ShiftMasterAccess", SqlDbType.VarChar).Value = data.ShiftMasterAccess;
                cmd.Parameters.Add("@DBPswdChangeAccess", SqlDbType.VarChar).Value = data.DBPswdChangeAccess;
                cmd.Parameters.Add("@DeleteAccess", SqlDbType.VarChar).Value = data.DeleteAccess;

                cmd.Parameters.Add("@ImportExportAccess", SqlDbType.VarChar).Value = data.ImportExportAccess;
                cmd.Parameters.Add("@CloseTickets", SqlDbType.VarChar).Value = data.CloseTickets;
                cmd.Parameters.Add("@RFIDAllocationAccess", SqlDbType.VarChar).Value = data.RFIDAllocationAccess;
                cmd.Parameters.Add("@RFIDUserTableAccess", SqlDbType.VarChar).Value = data.RFIDUserTableAccess;                
                cmd.Parameters.Add("@GateExitAccess", SqlDbType.VarChar).Value = data.GateExitAccess;
                cmd.Parameters.Add("@AWSAccess", SqlDbType.VarChar).Value = data.AWSAccess;
                cmd.Parameters.Add("@SystemConfigurationAccess", SqlDbType.VarChar).Value = data.SystemConfigurationAccess;
                cmd.Parameters.Add("@SAPSyncAccess", SqlDbType.VarChar).Value = data.SAPSyncAccess;
                cmd.Parameters.Add("@PrintAndDeleteAccess", SqlDbType.VarChar).Value = data.PrintAndDeleteAccess;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InsertUserManageData:" + ex.Message);
            }
        }
        //public void InserSaveReport(query_details data)
        //{
        //    try
        //    {
        //        SqlConnection con = new SqlConnection(ConnectionString);
        //        string sql = "INSERT INTO [Save_Reports] (Query,WhereEnabled,ReportName) " +
        //            "VALUES(@Query,@WhereEnabled,@ReportName)";
        //        SqlCommand cmd = new SqlCommand(sql, con);
        //        cmd.Parameters.Add("@Query", SqlDbType.VarChar).Value = data.Query;
        //        cmd.Parameters.Add("@WhereEnabled", SqlDbType.VarChar).Value = data.WhereEnabled;
        //        cmd.Parameters.Add("@ReportName", SqlDbType.VarChar).Value = data.ReportName;

        //        con.Open();
        //        cmd.ExecuteNonQuery();
        //        cmd.Dispose();
        //        con.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog.WriteToFile("InserSaveReport:" + ex.Message);
        //    }
        //}
        //public void InserSaveField(selectedFields data)
        //{
        //    try
        //    {
        //        SqlConnection con = new SqlConnection(ConnectionString);
        //        string sql = "INSERT INTO [Save_FieldName] (FieldName,ReportName) " +
        //            "VALUES(@FieldName,@ReportName)";
        //        SqlCommand cmd = new SqlCommand(sql, con);
        //        cmd.Parameters.Add("@FieldName", SqlDbType.VarChar).Value = data.FieldName;
        //        cmd.Parameters.Add("@ReportName", SqlDbType.VarChar).Value = data.Name;


        //        con.Open();
        //        cmd.ExecuteNonQuery();
        //        cmd.Dispose();
        //        con.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog.WriteToFile("InserSaveReport:" + ex.Message);
        //    }
        //}



        //public void InsertMaterialData(material_details1 data)
        //{
        //    try
        //    {
        //        SqlConnection con = new SqlConnection(ConnectionString);
        //        string sql = "INSERT INTO [Material_Master] (MaterialCode,MaterialName,IsDeleted) " +
        //            "VALUES(@MaterialCode,@MaterialName,@IsDeleted)";
        //        SqlCommand cmd = new SqlCommand(sql, con);
        //        cmd.Parameters.Add("@MaterialCode", SqlDbType.VarChar).Value = data.MaterialCode;
        //        cmd.Parameters.Add("@MaterialName", SqlDbType.VarChar).Value = data.MaterialName;
        //        cmd.Parameters.Add("@IsDeleted", SqlDbType.VarChar).Value = data.IsDeleted;

        //        con.Open();
        //        cmd.ExecuteNonQuery();
        //        cmd.Dispose();
        //        con.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog.WriteToFile("InsertMaterialData:" + ex.Message);
        //    }
        //}

        //public void InsertSupplierData(supplier_details1 data)
        //{
        //    try
        //    {
        //        SqlConnection con = new SqlConnection(ConnectionString);
        //        string sql = "INSERT INTO [Supplier_Master] (SupplierName,SupplierCode,IsDeleted) " +
        //            "VALUES(@SupplierName,@SupplierCode,@IsDeleted)";
        //        SqlCommand cmd = new SqlCommand(sql, con);
        //        cmd.Parameters.Add("@SupplierName", SqlDbType.VarChar).Value = data.SupplierName;
        //        cmd.Parameters.Add("@SupplierCode", SqlDbType.VarChar).Value = data.SupplierCode;
        //        cmd.Parameters.Add("@IsDeleted", SqlDbType.VarChar).Value = data.IsDeleted;

        //        con.Open();
        //        cmd.ExecuteNonQuery();
        //        cmd.Dispose();
        //        con.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog.WriteToFile("InsertMaterialData:" + ex.Message);
        //    }
        //}
        //public void InsertsingletransactionData(transaction_details data)
        //{
        //    try
        //    {
        //        SqlConnection con = new SqlConnection(ConnectionString);
        //        string sql = "INSERT INTO [Transaction] (VehicleNo,MaterialName,SupplierName,NoOfMaterial,Date,EmptyWeight,LoadWeight,EmptyWeightDate,LoadWeightDate,NetWeight,Pending,Closed,MultiWeight,MultiWeightTransPending) " +
        //            "VALUES(@VehicleNo,@MaterialName,@SupplierName,@NoOfMaterial,@Date,@EmptyWeight,@LoadWeight,@EmptyWeightDate,@LoadWeightDate,@Netweight,@Pending,@Closed,@Multiweight,@MultiweightTransPending)";
        //        SqlCommand cmd = new SqlCommand(sql, con);
        //        cmd.Parameters.Add("@VehicleNo", SqlDbType.VarChar).Value = data.VehicleNo;
        //        cmd.Parameters.Add("@MaterialName", SqlDbType.VarChar).Value = data.Materialname;
        //        cmd.Parameters.Add("@SupplierName", SqlDbType.VarChar).Value = data.Suppliername;
        //        cmd.Parameters.Add("@NoOfMaterial", SqlDbType.VarChar).Value = data.NoOfMaterial;
        //        cmd.Parameters.Add("@Date", SqlDbType.DateTime).Value = data.Date;
        //        cmd.Parameters.Add("@EmptyWeight", SqlDbType.Int).Value = data.EmptyWeight;
        //        cmd.Parameters.Add("@LoadWeight", SqlDbType.Int).Value = data.LoadWeight;
        //        cmd.Parameters.Add("@EmptyWeightDate", SqlDbType.DateTime).Value = data.EmptyWeightDate;
        //        cmd.Parameters.Add("@LoadWeightDate", SqlDbType.DateTime).Value = data.LoadWeightDate;
        //        cmd.Parameters.Add("@Netweight", SqlDbType.Int).Value = data.Netweight;
        //        cmd.Parameters.Add("@Pending", SqlDbType.Bit).Value = data.Pending;
        //        cmd.Parameters.Add("@Closed", SqlDbType.Bit).Value = data.Closed;
        //        cmd.Parameters.Add("@Multiweight", SqlDbType.Bit).Value = data.Multiweight;
        //        cmd.Parameters.Add("@MultiweightTransPending", SqlDbType.Bit).Value = data.MultiweightTransPending;

        //        con.Open();
        //        cmd.ExecuteNonQuery();
        //        cmd.Dispose();
        //        con.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog.WriteToFile("InsertMaterialData:" + ex.Message);
        //    }
        //}
        public void UpdateGeneralData(RolePriviliege data)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "Update User_Previledges SET TransactionAccess=@TransactionAccess,MasterAccess=@MasterAccess,ReportAccess=@ReportAccess,AdminAccess=@AdminAccess,SettingAccess=@SettingAccess,EmailSettingsAccess=@EmailSettingsAccess,HardwareSettingsAccess=@HardwareSettingsAccess,SoftwareConfigurationAccess=@SoftwareConfigurationAccess," +
                    "OtherSettingsAccess=@OtherSettingsAccess,BackupDBAccess=@BackupDBAccess,RecordDeletionAccess=@RecordDeletionAccess,ContactsAccess=@ContactsAccess,UserManagementAccess=@UserManagementAccess,TicketDataEntryAccess=@TicketDataEntryAccess,SendMailAccess=@SendMailAccess,SMSAdminAccess=@SMSAdminAccess,SalesForceSettingsAccess=@SalesForceSettingsAccess," +
                    "DuplicateTicketsAccess=@DuplicateTicketsAccess,DBMigrationAccess=@DBMigrationAccess,VehicleMasterAccess=@VehicleMasterAccess,FileLocationSettingsAccess=@FileLocationSettingsAccess,ReachUs=@ReachUs,CCTVSettings=@CCTVSettings,BioMetricSettings=@BioMetricSettings,GSMModemSetting=@GSMModemSetting,AlprSetting=@AlprSetting,PLCEnable=@PLCEnable,PLCPortSetting=@PLCPortSetting," +
                    "WeighBridgeSetting=@WeighBridgeSetting,RFIDPortSetting=@RFIDPortSetting,RFIDFieldMapping=@RFIDFieldMapping,RFIDWriter=@RFIDWriter,DongleSetting=@DongleSetting,RemoteDisplaySetting=@RemoteDisplaySetting,EditHardwareProfile=@EditHardwareProfile,SMTPAccess=@SMTPAccess,SummaryReportAccess=@SummaryReportAccess,MaterialMasterAccess=@MaterialMasterAccess,SupllierMasterAccess=@SupllierMasterAccess," +
                    "ShiftMasterAccess=@ShiftMasterAccess,DBPswdChangeAccess=@DBPswdChangeAccess,DeleteAccess=@DeleteAccess,ImportExportAccess=@ImportExportAccess,CloseTickets=@CloseTickets,RFIDAllocationAccess=@RFIDAllocationAccess,RFIDUserTableAccess=@RFIDUserTableAccess,GateExitAccess=@GateExitAccess,AWSAccess=@AWSAccess,SystemConfigurationAccess=@SystemConfigurationAccess,SAPSyncAccess=@SAPSyncAccess,PrintAndDeleteAccess=@PrintAndDeleteAccess Where Role=" + "'" + data.Role + "'";
                SqlCommand cmd = new SqlCommand(sql, con);
                //cmd.Parameters.Add("@Role", SqlDbType.VarChar).Value = data.Role;

                cmd.Parameters.Add("@TransactionAccess", SqlDbType.VarChar).Value = data.TransactionAccess;
                cmd.Parameters.Add("@MasterAccess", SqlDbType.VarChar).Value = data.MasterAccess;
                cmd.Parameters.Add("@ReportAccess", SqlDbType.VarChar).Value = data.ReportAccess;
                cmd.Parameters.Add("@AdminAccess", SqlDbType.VarChar).Value = data.AdminAccess;
                cmd.Parameters.Add("@SettingAccess", SqlDbType.VarChar).Value = data.SettingAccess;
                cmd.Parameters.Add("@EmailSettingsAccess", SqlDbType.VarChar).Value = data.EmailSettingsAccess;
                cmd.Parameters.Add("@HardwareSettingsAccess", SqlDbType.VarChar).Value = data.HardwareSettingAccess;
                cmd.Parameters.Add("@SoftwareConfigurationAccess", SqlDbType.VarChar).Value = data.SoftwareConfigurationAccess;
                cmd.Parameters.Add("@OtherSettingsAccess", SqlDbType.VarChar).Value = data.OtherSettingsAccess;
                cmd.Parameters.Add("@BackupDBAccess", SqlDbType.VarChar).Value = data.BackupDBAccess;
                cmd.Parameters.Add("@RecordDeletionAccess", SqlDbType.VarChar).Value = data.RecordDeletionAccess;
                cmd.Parameters.Add("@ContactsAccess", SqlDbType.VarChar).Value = data.ContactsAccess;
                cmd.Parameters.Add("@UserManagementAccess", SqlDbType.VarChar).Value = data.UserManagementAccess;
                cmd.Parameters.Add("@TicketDataEntryAccess", SqlDbType.VarChar).Value = data.CustomFieldAccess;
                cmd.Parameters.Add("@SendMailAccess", SqlDbType.VarChar).Value = data.SendMailAccess;
                cmd.Parameters.Add("@SMSAdminAccess", SqlDbType.VarChar).Value = data.SMSAdminAccess;
                cmd.Parameters.Add("@SalesForceSettingsAccess", SqlDbType.VarChar).Value = data.SalesForceSettingsAccess;
                cmd.Parameters.Add("@DuplicateTicketsAccess", SqlDbType.VarChar).Value = data.DuplicateTicketsAccess;
                cmd.Parameters.Add("@DBMigrationAccess", SqlDbType.VarChar).Value = data.DBMigrationAccess;
                cmd.Parameters.Add("@VehicleMasterAccess", SqlDbType.VarChar).Value = data.VehicleMasterAccess;
                cmd.Parameters.Add("@FileLocationSettingsAccess", SqlDbType.VarChar).Value = data.FileLocationSettingsAccess;
                cmd.Parameters.Add("@ReachUs", SqlDbType.VarChar).Value = data.ReachUsAccess;
                cmd.Parameters.Add("@CCTVSettings", SqlDbType.VarChar).Value = data.CCTVSettings;
                cmd.Parameters.Add("@BioMetricSettings", SqlDbType.VarChar).Value = data.BioMetricSettingsAccess;
                cmd.Parameters.Add("@GSMModemSetting", SqlDbType.VarChar).Value = data.GSMModemSettingAccess;
                cmd.Parameters.Add("@AlprSetting", SqlDbType.VarChar).Value = data.ALPRSettingAccess;
                cmd.Parameters.Add("@PLCEnable", SqlDbType.VarChar).Value = data.PLCEnableAccess;
                cmd.Parameters.Add("@PLCPortSetting", SqlDbType.VarChar).Value = data.PLCPortSettingAccess;
                cmd.Parameters.Add("@WeighBridgeSetting", SqlDbType.VarChar).Value = data.WeighBridgeSetting;
                cmd.Parameters.Add("@RFIDPortSetting", SqlDbType.VarChar).Value = data.RFIDPortSettingAccess;
                cmd.Parameters.Add("@RFIDFieldMapping", SqlDbType.VarChar).Value = data.RFIDMasterAccess;
                cmd.Parameters.Add("@RFIDWriter", SqlDbType.VarChar).Value = data.RFIDWriterAccess;
                cmd.Parameters.Add("@DongleSetting", SqlDbType.VarChar).Value = data.DongleSettingAccess;
                cmd.Parameters.Add("@RemoteDisplaySetting", SqlDbType.VarChar).Value = data.RemoteDisplaySettingAccess;
                cmd.Parameters.Add("@EditHardwareProfile", SqlDbType.VarChar).Value = data.EditHardwareProfile;

                cmd.Parameters.Add("@SMTPAccess", SqlDbType.VarChar).Value = data.SMTPAccess;
                cmd.Parameters.Add("@SummaryReportAccess", SqlDbType.VarChar).Value = data.SummaryReportAccess;
                cmd.Parameters.Add("@MaterialMasterAccess", SqlDbType.VarChar).Value = data.MaterialMasterAccess;
                cmd.Parameters.Add("@SupllierMasterAccess", SqlDbType.VarChar).Value = data.SupllierMasterAccess;
                cmd.Parameters.Add("@ShiftMasterAccess", SqlDbType.VarChar).Value = data.ShiftMasterAccess;
                cmd.Parameters.Add("@DBPswdChangeAccess", SqlDbType.VarChar).Value = data.DBPswdChangeAccess;
                cmd.Parameters.Add("@DeleteAccess", SqlDbType.VarChar).Value = data.DeleteAccess;

                cmd.Parameters.Add("@ImportExportAccess", SqlDbType.VarChar).Value = data.ImportExportAccess;
                cmd.Parameters.Add("@CloseTickets", SqlDbType.VarChar).Value = data.CloseTickets;
                cmd.Parameters.Add("@RFIDAllocationAccess", SqlDbType.VarChar).Value = data.RFIDAllocationAccess;
                cmd.Parameters.Add("@RFIDUserTableAccess", SqlDbType.VarChar).Value = data.RFIDUserTableAccess;
                cmd.Parameters.Add("@GateExitAccess", SqlDbType.VarChar).Value = data.GateExitAccess;
                cmd.Parameters.Add("@AWSAccess", SqlDbType.VarChar).Value = data.AWSAccess;
                cmd.Parameters.Add("@SystemConfigurationAccess", SqlDbType.VarChar).Value = data.SystemConfigurationAccess;
                cmd.Parameters.Add("@SAPSyncAccess", SqlDbType.VarChar).Value = data.SAPSyncAccess;
                cmd.Parameters.Add("@PrintAndDeleteAccess", SqlDbType.VarChar).Value = data.PrintAndDeleteAccess;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("InsertUserManageData:" + ex.Message);
            }
        }

        public void DeleteGeneralData(RolePriviliege data)
        {
            try
            {

                SqlConnection con = new SqlConnection(ConnectionString);
                string sql = "DELETE FROM User_Previledges WHERE Role= " + "'" + data.Role + "'";
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DeleteUserManageData : " + ex.Message);
            }
        }
        public void DeletetticketData(List<Transaction> transactions)
        {
            try
            {
                var ticketNos = transactions.Select(x => x.TicketNo).ToList();
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                string sql = "UPDATE [Transaction] SET IsDeleted=1 WHERE TicketNo IN ({0})";
                string[] paramArray = ticketNos.Select((x, i) => "@ticketnos" + i).ToArray();
                cmd.CommandText = string.Format(sql, string.Join(",", paramArray));

                for (int i = 0; i < ticketNos.Count; ++i)
                {
                    cmd.Parameters.Add(new SqlParameter("@ticketnos" + i, ticketNos[i]));
                }
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DeletetticketData : " + ex.Message);
            }
        }
        #endregion

        string[] tableNames = new string[]
        {
            "[ALPR_Settings]",
            "[Basic_Configuration]",
            "[Other_Settings]",
            "[Serial_COM_Setting]",
            "[Sytem_Configuration]",
            "[User_Management]",
            "[Weighbridge_Settings]"
        };
        public void UpdateHardwareProfileInAllTAbles(string oldName, string newName)
        {
            try
            {
                SqlConnection con1 = new SqlConnection(ConnectionString);
                con1.Open();
                foreach (var table in tableNames)
                {
                    string UpdateAllTableHPQuery = $" UPDATE {table} SET HardwareProfile = '{newName}' where HardwareProfile='{oldName}'";
                    new SqlCommand(UpdateAllTableHPQuery, con1).ExecuteNonQuery();
                }
                string UpdateCCTVSettingsQuery = $" UPDATE [CCTV_Settings] SET HarwareProfile = '{newName}' where HarwareProfile='{oldName}'";
                string UpdateFileLocationSettingQuery = $" UPDATE [FileLocation_Setting] SET HarwareProfile = '{newName}' where HarwareProfile='{oldName}'";
                new SqlCommand(UpdateCCTVSettingsQuery, con1).ExecuteNonQuery();
                new SqlCommand(UpdateFileLocationSettingQuery, con1).ExecuteNonQuery();
                con1.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("AdminDBCall/UpdateHardwareProfileInAllTAbles/Exception :- " + ex.Message);
            }
        }

        public void UpdateSystemConfig(string HardwareProfile, int id)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                con.Open();
                string getHardwareProfileById = $"select HardwareProfileName from [User_HardwareProfiles] where ID='{id}'";
                SqlCommand cmd = new SqlCommand(getHardwareProfileById, con);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string oldName = reader["HardwareProfileName"].ToString();
                        UpdateHardwareProfileInAllTAbles(oldName, HardwareProfile);
                    }
                }

                string UpdateSystemConfigQuery = $" UPDATE [Sytem_Configuration] SET HardwareProfile = '{HardwareProfile}',Name='{HardwareProfile}' where Id='{id}'";
                new SqlCommand(UpdateSystemConfigQuery, con).ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("AdminDBCall/UpdateSystemConfig/Exception :- " + ex.Message);
            }
        }

        public void InsertSystemConfig(string HardwareProfile)
        {
            try
            {
                DataTable dt1 = GetAllData($"SELECT * FROM [Sytem_Configuration] where HardwareProfile='{HardwareProfile}'");
                if (dt1 != null && dt1.Rows.Count > 0)
                {

                }
                else
                {
                    SqlConnection con = new SqlConnection(ConnectionString);
                    string insertSystemConfigQuery = "INSERT INTO [Sytem_Configuration] (Name,HardwareProfile) VALUES(@Name,@HardwareProfile)";
                    SqlCommand cmd = new SqlCommand(insertSystemConfigQuery, con);
                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = HardwareProfile;
                    cmd.Parameters.Add("@HardwareProfile", SqlDbType.NVarChar).Value = HardwareProfile;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("AdminDBCall/InsertSystemConfig/Exception :- " + ex.Message);
            }
        }
    }
    public class transaction_details
    {
        public string VehicleNo { get; set; }
        public string MaterialName { get; set; }
        public int NoOfMaterial { get; set; }
        public int ProcessedMaterial { get; set; }
        public string SupplierName { get; set; }
        public DateTime Date { get; set; }
        public int EmptyWeight { get; set; }
        public int LoadWeight { get; set; }
        public DateTime EmptyWeightDate { get; set; }
        public string EmptyWeightTime { get; set; }
        public DateTime LoadWeightDate { get; set; }
        public string LoadWeightTime { get; set; }
        public int Netweight { get; set; }
        public bool MultiWeightTransPending { get; set; }
        public bool Pending { get; set; }
        public bool Closed { get; set; }
        public bool Multiweight { get; set; }
        public bool MultiweightTransPending { get; set; }
        public string LoadStatus { get; set; }
        public string TransactionType { get; set; }
        public string ShiftName { get; set; }
        public string State { get; set; }
        public string SystemID { get; set; }
        public string UserName { get; set; }
    }
}
