using IWT.DBCall;
using IWT.Models;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IWT.Shared
{
    public class CommonFunction
    {
        public static MasterDBCall masterDBCall = new MasterDBCall();
        public static AdminDBCall adminDB = new AdminDBCall();
        public async Task CheckAndSendEmail(Transaction CurrentTransaction, string Message, byte[] bytearray, string FileName, string autoEmail = null)
        {
            try
            {

                MailSetting mailSetting = GetMailDetails();
                if (mailSetting != null)
                {
                    if (!string.IsNullOrEmpty(autoEmail))
                    {
                        mailSetting.ToID = autoEmail;
                    }
                    if (mailSetting.SendType == "SMTP")
                    {
                        SMTPSetting setting = GetSMTPDetails();
                        if (setting != null)
                        {
                            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Mail sending has been initialized");
                            await SendMail(CurrentTransaction, setting, mailSetting, Message, bytearray, FileName);
                        }
                    }
                    else if (mailSetting.SendType == "Send Grid")
                    {
                        await SendMailUsingSendGrid(CurrentTransaction, mailSetting, Message, bytearray, FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/CheckAndSendEmail :- " + ex.Message);
            }
        }

        public Transaction GetVehicleDetails(string VehicleNo)
        {
            try
            {
                string Query = "SELECT * FROM [Transaction] WHERE VehicleNo=@VehicleNo";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@VehicleNo", VehicleNo);
                DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<Transaction>>(JSONString);
                if (result != null)
                {
                    result = result.OrderByDescending(x => x.TicketNo).ToList();
                }
                return (result != null && result.Count > 0) ? result[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetVehicleDetails/Exception:- " + ex.Message, ex);
                return null;
            }
        }

        public Transaction GetTransactionByRFIDAllocation(int RFIDAllocation)
        {
            try
            {
                string Query = "SELECT * FROM [Transaction] WHERE TicketNo=@Ticket";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@RFIDAllocation", RFIDAllocation);
                DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<Transaction>>(JSONString);
                if (result != null)
                {
                    result = result.OrderByDescending(x => x.TicketNo).ToList();
                }
                return (result != null && result.Count > 0) ? result[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetTransactionByRFIDAllocation/Exception:- " + ex.Message, ex);
                return null;
            }
        }

        public List<SMSDesign> GetSMSDesigns()
        {
            try
            {
                string Query = "SELECT * FROM SMS_Design";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<SMSDesign>>(JSONString);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetSMSDesigns/Exception:- " + ex.Message, ex);
                return null;
            }
        }

        public List<EmailDesign> GetEmailDesigns()
        {
            try
            {
                string Query = "SELECT * FROM Email_Design";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<EmailDesign>>(JSONString);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetSMSDesigns/Exception:- " + ex.Message, ex);
                return null;
            }
        }
        public DataTable GetSMSDesignData()
        {
            try
            {
                string Query = "SELECT * FROM SMS_Design";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
                return table;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetSMSDesigns/Exception:- " + ex.Message, ex);
                return null;
            }
        }

        public string BuildSMS(Transaction CurrentTransaction)
        {
            try
            {
                List<SMSDesign> designs = GetSMSDesigns();
                if (designs != null && designs.Count > 0)
                {
                    var DesignedContent = designs[0].DesignedContent;
                    var result = ReplacePropertyValues(DesignedContent, CurrentTransaction);
                    return string.IsNullOrEmpty(result) ? DefaultBuildedSMS(CurrentTransaction) : result;
                    //return DefaultBuildedSMS(CurrentTransaction);
                }
                else
                {
                    return DefaultBuildedSMS(CurrentTransaction);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string BuildSMSForGateExit(RFIDAllocation RFIDAllocation)
        {
            try
            {
                List<SMSDesign> designs = GetSMSDesigns();
                if (designs != null && designs.Count > 0)
                {
                    var DesignedContent = designs[0].DesignedContent;
                    var result = ReplacePropertyValues(DesignedContent, RFIDAllocation);
                    return string.IsNullOrEmpty(result) ? DefaultBuildedSMSForRFIDAllocation(RFIDAllocation) : result;
                    //return DefaultBuildedSMS(CurrentTransaction);
                }
                else
                {
                    return DefaultBuildedSMSForRFIDAllocation(RFIDAllocation);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string DefaultBuildedSMS(Transaction CurrentTransaction)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Tkno : {0}\n", CurrentTransaction.TicketNo);
            sb.AppendFormat("Vehno : {0}\n", CurrentTransaction.VehicleNo);
            sb.AppendFormat("Mat : {0}\n", CurrentTransaction.MaterialName);
            sb.AppendFormat("Sup : {0}\n", CurrentTransaction.SupplierName);
            sb.AppendFormat("Emt wt : {0}\n", CurrentTransaction.EmptyWeight);
            sb.AppendFormat("Load wt : {0}\n", CurrentTransaction.LoadWeight);
            sb.AppendFormat("Net wt : {0}\n", CurrentTransaction.NetWeight);
            sb.AppendFormat("Regards Essae");
            return sb.ToString();
        }

        public string DefaultBuildedSMSForRFIDAllocation(RFIDAllocation RFIDAllocation)
        {
            StringBuilder sb = new StringBuilder();
            //sb.AppendFormat("Tkno : {0}\n", CurrentTransaction.TicketNo);
            //sb.AppendFormat("Vehno : {0}\n", CurrentTransaction.VehicleNo);
            //sb.AppendFormat("Mat : {0}\n", CurrentTransaction.MaterialName);
            //sb.AppendFormat("Sup : {0}\n", CurrentTransaction.SupplierName);
            //sb.AppendFormat("Emt wt : {0}\n", CurrentTransaction.EmptyWeight);
            //sb.AppendFormat("Load wt : {0}\n", CurrentTransaction.LoadWeight);
            //sb.AppendFormat("Net wt : {0}\n", CurrentTransaction.NetWeight);
            //sb.AppendFormat("Regards Essae");
            return sb.ToString();
        }

        public string ReplacePropertyValues(string DesignedContent, Object obj)
        {
            try
            {
                Type t = obj.GetType();
                //Console.WriteLine("Type is: {0}", t.Name);
                PropertyInfo[] props = t.GetProperties();
                //Console.WriteLine("Properties (N = {0}):",
                //                  props.Length);
                foreach (var prop in props)
                {
                    if (prop.GetIndexParameters().Length == 0)
                    {
                        //Console.WriteLine("   {0} ({1}): {2}", prop.Name,
                        //                                     prop.PropertyType.Name, prop.GetValue(obj));
                        string str = "[" + prop.Name + "]";
                        DesignedContent = ReplaceFirstOccurrence(DesignedContent, str, prop.GetValue(obj)?.ToString() ?? "");
                    }

                    else
                    {
                        //  Console.WriteLine("   {0} ({1}): <Indexed>", prop.Name,
                        //prop.PropertyType.Name);

                    }
                }
                return DesignedContent;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/ReplacePropertyValues :- " + ex.Message);
                return null;
            }


        }

        private string ReplaceFirstOccurrence(string Source, string Find, string Replace)
        {
            int Place = Source.IndexOf(Find);
            if (Place > -1)
            {
                string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
                return result;
            }
            return Source;
        }

        public string BuildEmail(Transaction CurrentTransaction)
        {

            try
            {
                List<EmailDesign> designs = GetEmailDesigns();
                if (designs != null && designs.Count > 0)
                {
                    var DesignedContent = designs[0].DesignedContent;
                    var result = ReplacePropertyValues(DesignedContent, CurrentTransaction);
                    return string.IsNullOrEmpty(result) ? DefaultBuildedSMS(CurrentTransaction) : result;
                    //return DefaultBuildedSMS(CurrentTransaction);
                }
                else
                {
                    return DefaultBuildedEmail(CurrentTransaction);
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public string BuildEmailForGateExit(RFIDAllocation Allocation)
        {

            try
            {
                List<EmailDesign> designs = GetEmailDesigns();
                if (designs != null && designs.Count > 0)
                {
                    var DesignedContent = designs[0].DesignedContent;
                    var result = ReplacePropertyValues(DesignedContent, Allocation);
                    return string.IsNullOrEmpty(result) ? DefaultBuildedSMSForRFIDAllocation(Allocation) : result;
                    //return DefaultBuildedSMS(CurrentTransaction);
                }
                else
                {
                    return DefaultBuildedEmailForAllocation(Allocation);
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public string DefaultBuildedEmail(Transaction CurrentTransaction)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Dear Sir/Madam,<br/>"));
            sb.Append($"<p>Please find the transaction details</p>");
            sb.Append($"<p>Ticket Number : {CurrentTransaction.TicketNo}</p>");
            sb.Append($"<p>Vehicle Number : {CurrentTransaction.VehicleNo}</p>");
            sb.Append($"<p>Material Name  : {CurrentTransaction.MaterialName}</p>");
            sb.Append($"<p>Supplier Name : {CurrentTransaction.SupplierName}</p>");
            sb.Append($"<p>Load Status : {CurrentTransaction.LoadStatus}</p>");
            sb.Append($"<p>Empty Weight : {CurrentTransaction.EmptyWeight}</p>");
            sb.Append($"<p>Loaded Weight : {CurrentTransaction.LoadWeight}</p>");
            sb.Append($"<p>Netweight : {CurrentTransaction.NetWeight}</p>");
            sb.Append($"<p>User Name : {CurrentTransaction.UserName}</p>");
            sb.Append($"<p>State : {CurrentTransaction.State}</p><br/>");
            sb.AppendFormat("<p>Regards</p><p>Essae</p>");
            return sb.ToString();
        }

        public string DefaultBuildedEmailForAllocation(RFIDAllocation Allocation)
        {
            StringBuilder sb = new StringBuilder();
            //sb.Append(string.Format("Dear Sir/Madam,<br/>"));
            //sb.Append($"<p>Please find the transaction details</p>");
            //sb.Append($"<p>Ticket Number : {CurrentTransaction.TicketNo}</p>");
            //sb.Append($"<p>Vehicle Number : {CurrentTransaction.VehicleNo}</p>");
            //sb.Append($"<p>Material Name  : {CurrentTransaction.MaterialName}</p>");
            //sb.Append($"<p>Supplier Name : {CurrentTransaction.SupplierName}</p>");
            //sb.Append($"<p>Load Status : {CurrentTransaction.LoadStatus}</p>");
            //sb.Append($"<p>Empty Weight : {CurrentTransaction.EmptyWeight}</p>");
            //sb.Append($"<p>Loaded Weight : {CurrentTransaction.LoadWeight}</p>");
            //sb.Append($"<p>Netweight : {CurrentTransaction.NetWeight}</p>");
            //sb.Append($"<p>User Name : {CurrentTransaction.UserName}</p>");
            //sb.Append($"<p>State : {CurrentTransaction.State}</p><br/>");
            //sb.AppendFormat("<p>Regards</p><p>Essae</p>");
            return sb.ToString();
        }

        public async Task CheckAndSendSMS(string Message, string autoSMSNumber = null)
        {
            try
            {
                SMSTemplate template = GetSMSTemplate();
                if (template != null)
                {
                    if (autoSMSNumber != null)
                    {
                        template.AutoSMSNumber = autoSMSNumber;
                    }
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "SMS sending has been initialized");
                    await SendSMS(template, Message);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/CheckAndSendEmail :- " + ex.Message);
            }
        }
        public SMTPSetting GetSMTPDetails()
        {
            try
            {
                string Query = "Select * from SMTP_Settings";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<SMTPSetting>>(JSONString);
                return result.Count > 0 ? result[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetSMTPDetails/Exception:- " + ex.Message, ex);
                return null;
            }
        }
        public MailSetting GetMailDetails()
        {
            try
            {
                string Query = "Select * from Mail_Settings where Enable=1";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<MailSetting>>(JSONString);
                return result.Count > 0 ? result[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetMailDetails/Exception:- " + ex.Message, ex);
                return null;
            }
        }

        public SMSTemplate GetSMSTemplate()
        {
            try
            {
                string Query = "Select * from SMS_Template where UseGSM=0";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<SMSTemplate>>(JSONString);
                return result.Count > 0 ? result[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetSMSTemplate/Exception:- " + ex.Message, ex);
                return null;
            }

        }

        public async Task SendMail(Transaction CurrentTransaction, SMTPSetting setting, MailSetting mailSetting, string Message, byte[] bytearray, string FileName)
        {
            FailedMailSMS failedMail = new FailedMailSMS();
            try
            {
                if (!string.IsNullOrEmpty(mailSetting.ToID))
                {
                    string hostName = setting.Host;
                    string SMTPEmail = mailSetting.FromID;
                    string SMTPEmailPassword = mailSetting.Password;
                    string SMTPPort = setting.Port;
                    var message = new MailMessage();
                    string subject = "";
                    subject = $"Transaction Receipt Vehicle Number : {CurrentTransaction.VehicleNo}";
                    SmtpClient client = new SmtpClient();
                    client.Port = Convert.ToInt32(SMTPPort);
                    client.Host = hostName;
                    client.EnableSsl = true;
                    client.Timeout = 60000;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential(SMTPEmail, SMTPEmailPassword);
                    string deserializedMessage = System.Net.WebUtility.HtmlDecode(Message);
                    MailMessage reportEmail = new MailMessage(SMTPEmail, mailSetting.ToID, subject, deserializedMessage);
                    var CCs = mailSetting.CCList.Split(',');
                    foreach (var cc in CCs)
                    {
                        if (!string.IsNullOrEmpty(cc))
                            reportEmail.CC.Add(new MailAddress(cc));
                    }
                    reportEmail.BodyEncoding = UTF8Encoding.UTF8;
                    reportEmail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                    reportEmail.IsBodyHtml = true;
                    if (bytearray != null && bytearray.Length > 0)
                    {
                        System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(new MemoryStream(bytearray), MediaTypeNames.Application.Octet);
                        ContentDisposition disposition = attachment.ContentDisposition;
                        disposition.CreationDate = DateTime.Now;
                        disposition.ModificationDate = DateTime.Now;
                        disposition.ReadDate = DateTime.Now;
                        disposition.FileName = FileName;
                        disposition.Size = bytearray.Length;
                        disposition.DispositionType = DispositionTypeNames.Attachment;
                        reportEmail.Attachments.Add(attachment);
                    }

                    string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Report");
                    CreateReportDirectory(directoryPath);
                    string filePath = Path.Combine(directoryPath, FileName);
                    if (bytearray != null && bytearray.Length > 0)
                    {
                        File.WriteAllBytes(filePath, bytearray);
                    }

                    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    failedMail.FromID = mailSetting.FromID;
                    failedMail.ToID = mailSetting.ToID;
                    failedMail.Subject = subject;
                    failedMail.Text = deserializedMessage;
                    failedMail.CCList = mailSetting.CCList;
                    failedMail.MailFlag = false;
                    failedMail.FileName = FileName;
                    failedMail.FilePath = filePath;
                    //throw new Exception();
                    await client.SendMailAsync(reportEmail);
                    WriteLog.WriteToFile($"Mail has been sent successfully to {mailSetting.ToID}");
                    if (File.Exists(filePath))
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        File.Delete(filePath);
                    }
                }

                //return true;

            }
            catch (SmtpFailedRecipientsException ex)
            {
                for (int i = 0; i < ex.InnerExceptions.Length; i++)
                {
                    SmtpStatusCode status = ex.InnerExceptions[i].StatusCode;
                    if (status == SmtpStatusCode.MailboxBusy ||
                        status == SmtpStatusCode.MailboxUnavailable)
                    {
                        WriteLog.WriteToFile("CommonFunction/SendMail/MailboxBusy/MailboxUnavailable/SmtpFailedRecipientsException:Inner- " + ex.InnerExceptions[i].Message);
                    }
                    else
                    {
                        WriteLog.WriteToFile("CommonFunction/SendMail/SmtpFailedRecipientsException:Inner- " + ex.InnerExceptions[i].Message);
                    }
                }
                WriteLog.WriteToFile("CommonFunction/SendMail/SmtpFailedRecipientsException:- " + ex.Message, ex);
                CaptureFailedMail(failedMail);
                //return false;
            }
            catch (SmtpException ex)
            {
                WriteLog.WriteToFile("CommonFunction/SendMail/SmtpException:- " + ex.Message, ex);
                CaptureFailedMail(failedMail);
                //return false;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/SendMail/Exception:- " + ex.Message, ex);
                CaptureFailedMail(failedMail);
                //return false;
            }
        }

        public void CreateReportDirectory(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/CreateReportDirectory/Exception:- " + ex.Message, ex);
            }
        }

        public async Task SendMailUsingSendGrid(Transaction CurrentTransaction, MailSetting mailSetting, string Message, byte[] bytearray, string FileName)
        {
            try
            {
                //using (MailMessage mailMsg = new MailMessage())
                //{
                //    // API key
                //    string apiKey = ConfigurationManager.AppSettings["SendGridAPIKey"].ToString();
                //    string UserName = ConfigurationManager.AppSettings["SendGridUserName"].ToString();
                //    string Password = ConfigurationManager.AppSettings["SendGridPassword"].ToString();

                //    // From
                //    mailMsg.From = new MailAddress(mailSetting.FromID, mailSetting.FromID);

                //    // To
                //    mailMsg.To.Add(new MailAddress(mailSetting.ToID, mailSetting.ToID));

                //    var CCs = mailSetting.CCList.Split(',');
                //    foreach (var cc in CCs)
                //    {
                //        mailMsg.CC.Add(new MailAddress(cc));
                //    }

                //    // Subject and multipart/alternative Body
                //    mailMsg.Subject = $"Transaction Receipt Vehicle Number : {CurrentTransaction.VehicleNo}";
                //    string deserializedMessage = System.Net.WebUtility.HtmlDecode(Message);
                //    mailMsg.Body = deserializedMessage;
                //    mailMsg.BodyEncoding = UTF8Encoding.UTF8;
                //    mailMsg.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                //    mailMsg.IsBodyHtml = true;
                //    //mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
                //    //mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

                //    // Init SmtpClient and send
                //    using (SmtpClient smtpClient = new SmtpClient("smtp.sendgrid.net", 587))
                //    {
                //        smtpClient.Credentials = new NetworkCredential("apikey", apiKey);
                //        smtpClient.EnableSsl = true;
                //        smtpClient.Timeout = 60000;
                //        smtpClient.Send(mailMsg);
                //    }
                //}
                if (!string.IsNullOrEmpty(mailSetting.ToID))
                {
                    SendGridMaster sendGridMaster = GetSendGridMaster();
                    if (sendGridMaster != null)
                    {
                        var apiKey = sendGridMaster.APIKey;
                        var client = new SendGridClient(apiKey);
                        var from_email = new EmailAddress(mailSetting.FromID, mailSetting.FromID);
                        var subject = $"Transaction Receipt Vehicle Number : {CurrentTransaction.VehicleNo}";
                        var to_email = new EmailAddress(mailSetting.ToID, mailSetting.ToID);
                        string deserializedMessage = System.Net.WebUtility.HtmlDecode(Message);
                        var htmlContent = deserializedMessage;
                        //var msg = MailHelper.CreateSingleEmail(from_email, to_email, subject, "", htmlContent);

                        var msg = new SendGridMessage()
                        {
                            From = from_email,
                            Subject = subject,
                            HtmlContent = htmlContent,
                        };

                        msg.AddTo(to_email);

                        var CCs = mailSetting.CCList.Split(',');
                        foreach (var cc in CCs)
                        {
                            if (!string.IsNullOrEmpty(cc))
                                msg.AddCc(cc);
                        }
                        //msg.AddCcs(CCs);

                        var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
                        WriteLog.WriteToFile($"Mail has been sent successfully to {mailSetting.ToID}");
                    }
                }

            }
            catch (SmtpFailedRecipientsException ex)
            {
                for (int i = 0; i < ex.InnerExceptions.Length; i++)
                {
                    SmtpStatusCode status = ex.InnerExceptions[i].StatusCode;
                    if (status == SmtpStatusCode.MailboxBusy ||
                        status == SmtpStatusCode.MailboxUnavailable)
                    {
                        WriteLog.WriteToFile("CommonFunction/SendMailUsingSendGrid/MailboxBusy/MailboxUnavailable/SmtpFailedRecipientsException:Inner- " + ex.InnerExceptions[i].Message);
                    }
                    else
                    {
                        WriteLog.WriteToFile("CommonFunction/SendMailUsingSendGrid/SmtpFailedRecipientsException:Inner- " + ex.InnerExceptions[i].Message);
                    }
                }
                WriteLog.WriteToFile("CommonFunction/SendMailUsingSendGrid/SmtpFailedRecipientsException:- " + ex.Message, ex);
            }
            catch (SmtpException ex)
            {
                WriteLog.WriteToFile("CommonFunction/SendMailUsingSendGrid/SmtpException:- " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/SendMailUsingSendGrid/Exception:- " + ex.Message, ex);
            }

        }

        public async Task SendSMS(SMSTemplate template, string Message)
        {
            FailedSMS failed = new FailedSMS();
            try
            {
                MSG91Master mSG91Master = GetMSG91Master();
                if (mSG91Master != null)
                {
                    string authKey = mSG91Master.AuthKey;
                    //Multiple mobiles numbers separated by comma
                    string mobileNumber = template.PhoneNo1;
                    if (!string.IsNullOrEmpty(template.PhoneNo2))
                    {
                        mobileNumber += $",{template.PhoneNo2}";
                    }
                    if (!string.IsNullOrEmpty(template.PhoneNo3))
                    {
                        mobileNumber += $",{template.PhoneNo3}";
                    }
                    if (!string.IsNullOrEmpty(template.AutoSMSNumber))
                    {
                        mobileNumber += $",{template.AutoSMSNumber}";
                    }
                    //Sender ID,While using route4 sender id should be 6 characters long.
                    string senderId = mSG91Master.SenderID;
                    string DLT_TE_ID = mSG91Master.DLT_TE_ID;
                    string PE_ID = mSG91Master.PE_ID;
                    //Your message to send, Add URL encoding here.
                    string message = HttpUtility.UrlEncode($"{Message}");

                    failed.MobileNo1 = template.PhoneNo1;
                    failed.MobileNo2 = template.PhoneNo2;
                    failed.MobileNo3 = template.PhoneNo3;
                    failed.SMSRoute = template.AutoSMSNumber;
                    failed.Message = message;
                    failed.SMSFlag = false;
                    //Prepare you post parameters
                    StringBuilder sbPostData = new StringBuilder();
                    sbPostData.AppendFormat("authkey={0}", authKey);
                    sbPostData.AppendFormat("&mobiles={0}", mobileNumber);
                    sbPostData.AppendFormat("&message={0}", message);
                    sbPostData.AppendFormat("&sender={0}", senderId);
                    sbPostData.AppendFormat("&route={0}", "4");
                    sbPostData.AppendFormat("&DLT_TE_ID={0}", DLT_TE_ID);
                    sbPostData.AppendFormat("&PE_ID={0}", PE_ID);

                    string sendSMSUri = "http://api.msg91.com/api/sendhttp.php";
                    //Create HTTPWebrequest
                    HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(sendSMSUri);
                    //Prepare and Add URL Encoded data
                    UTF8Encoding encoding = new UTF8Encoding();
                    byte[] data = encoding.GetBytes(sbPostData.ToString());
                    //Specify post method
                    httpWReq.Method = "POST";
                    httpWReq.ContentType = "application/x-www-form-urlencoded";
                    httpWReq.ContentLength = data.Length;
                    using (Stream stream = httpWReq.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                    //Get the response
                    WebResponse response = await httpWReq.GetResponseAsync();
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseString = reader.ReadToEnd();

                    //Close the response
                    reader.Close();
                    response.Close();

                    //Prepare you post parameters


                    //string sendSMSUri = "http://api.msg91.com/api/sendhttp.php?";
                    //StringBuilder sbPostData = new StringBuilder(sendSMSUri);
                    //sbPostData.AppendFormat("authkey={0}", authKey);
                    //sbPostData.AppendFormat("&sender={0}", senderId);
                    //sbPostData.AppendFormat("&route={0}", "4");
                    //sbPostData.AppendFormat("&mobiles={0}", mobileNumber);
                    //sbPostData.AppendFormat("&message={0}", message);
                    //sbPostData.AppendFormat("&DLT_TE_ID={0}", DLT_TE_ID);
                    //sbPostData.AppendFormat("&PE_ID={0}", PE_ID);
                    //sendSMSUri = sbPostData.ToString();
                    ////Create HTTPWebrequest
                    //HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(sendSMSUri);
                    ////Prepare and Add URL Encoded data
                    ////UTF8Encoding encoding = new UTF8Encoding();
                    ////byte[] data = encoding.GetBytes(sbPostData.ToString());
                    ////Specify post method
                    //httpWReq.Method = "GET";
                    ////httpWReq.ContentType = "application/x-www-form-urlencoded";
                    ////httpWReq.ContentLength = data.Length;
                    ////using (Stream stream = httpWReq.GetRequestStream())
                    ////{
                    ////    stream.Write(data, 0, data.Length);
                    ////}
                    ////Get the response
                    //HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse();
                    //StreamReader reader = new StreamReader(response.GetResponseStream());
                    //string responseString = reader.ReadToEnd();

                    ////Close the response
                    //reader.Close();
                    //response.Close();


                    //return true;
                }
            }
            catch (SystemException ex)
            {
                WriteLog.WriteToFile("CommonFunction/SendSMS/SystemException:- " + ex.Message, ex);
                //return false;
                CaptureFailedSMS(failed);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/SendSMS/Exception:- " + ex.Message, ex);
                //return false;
                CaptureFailedSMS(failed);
            }
        }

        public List<CompanySummaryReportData> GetCompanySummaryReportData()
        {
            List<CompanySummaryReportData> companies = new List<CompanySummaryReportData>();
            try
            {
                var Query = "SELECT *  FROM [Company_SummaryReport_Data]";
                SqlCommand cmd1 = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd1, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                if (JSONString != null)
                {
                    companies = JsonConvert.DeserializeObject<List<CompanySummaryReportData>>(JSONString);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/CompanySummaryReportData/Exception:- " + ex.Message, ex);
            }
            return companies;
        }

        public List<Company_Details> GetCompanyDetails()
        {
            List<Company_Details> companies = new List<Company_Details>();
            try
            {
                var Query = "SELECT *  FROM [Company_Detail]";
                SqlCommand cmd1 = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd1, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                if (JSONString != null)
                {
                    companies = JsonConvert.DeserializeObject<List<Company_Details>>(JSONString);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetCompanyDetails/Exception:- " + ex.Message, ex);
            }
            return companies;
        }

        public RFIDAllocation GetRFIDAllocationByRFID(string RFIDTag)
        {
            try
            {
                string Query = "SELECT * FROM RFID_Allocations WHERE RFIDTag=@RFIDTag and Status='In-Transit'";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@RFIDTag", RFIDTag);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var res = JsonConvert.DeserializeObject<List<RFIDAllocation>>(JSONString);
                return res.Count > 0 ? res[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetRFIDAllocationByRFID", ex);
                return null;
            }
        }

        public RFIDAllocation GetRFIDAllocationById(int allocationId)
        {
            try
            {
                string Query = "SELECT * FROM RFID_Allocations WHERE AllocationId=@AllocationId";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@AllocationId", allocationId);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var res = JsonConvert.DeserializeObject<List<RFIDAllocation>>(JSONString);
                return res.Count > 0 ? res[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetRFIDAllocationByRFID", ex);
                return null;
            }
        }

        public VehicleMaster GetVehicleNumberDetail(string VehicleNumber)
        {
            try
            {
                string Query = "SELECT * FROM Vehicle_Master WHERE VehicleNumber=@VehicleNumber and IsDeleted=0";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@VehicleNumber", VehicleNumber);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var res = JsonConvert.DeserializeObject<List<VehicleMaster>>(JSONString);
                return res.Count > 0 ? res[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetVehicleNumberDetail", ex);
                return null;
            }
        }

        public void UpdateRFIDStatusForRFIDAllocation(string RFIDNo, string Status)
        {
            try
            {
                string updateQuery = $@"UPDATE [RFIDAllocation] SET Status=@Status WHERE RFIDNo=@RFIDNo";

                SqlCommand cmd = new SqlCommand(updateQuery);
                cmd.Parameters.AddWithValue("@Status", Status);
                cmd.Parameters.AddWithValue("@RFIDNo", RFIDNo);
                masterDBCall.InsertData(cmd, CommandType.Text);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/UpdateRFIDStatus", ex);
            }
        }

        public RFIDMaster GetRFIDMasterByRFID(string Tag)
        {
            try
            {
                string Query = "SELECT * FROM RFID_Tag_Master WHERE Tag=@Tag";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@Tag", Tag);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var res = JsonConvert.DeserializeObject<List<RFIDMaster>>(JSONString);
                return res.Count > 0 ? res[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetRFIDMasterByRFID", ex);
                return null;
            }
        }

        public void UpdateVehicleStatus(string status, string vehicleNumber, SqlTransaction transaction, SqlConnection con)
        {
            try
            {
                string updateQuery = $@"UPDATE [Vehicle_Master] SET Status=@Status WHERE VehicleNumber=@VehicleNumber";
                SqlCommand cmd = new SqlCommand(updateQuery, con, transaction);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@VehicleNumber", vehicleNumber);
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdateVehicleStatus", ex);
                throw ex;
            }
        }

        public void UpdateGatePassStatus(string status, int gatePassId, SqlTransaction transaction, SqlConnection con)
        {
            try
            {
                string updateQuery = $@"UPDATE [GatePasses] SET Status=@Status WHERE Id=@GatePassId";
                SqlCommand cmd = new SqlCommand(updateQuery, con, transaction);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@GatePassId", gatePassId);
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("UpdateGatePassStatus", ex);
                throw ex;
            }
        }

        public GatePasses GetGatePassesByVehicleNumber(string VehicleNumber)
        {
            try
            {
                string Query = "SELECT * FROM GatePasses WHERE VehicleNumber=@VehicleNumber and Status in ('','OGI')";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@VehicleNumber", VehicleNumber);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var res = JsonConvert.DeserializeObject<List<GatePasses>>(JSONString);
                return res.OrderByDescending(t => t.Id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetGatePassesByVehicleNumber", ex);
                return null;
            }
        }

        public List<TransactionTypeMaster> GetTransactionTypeMasters()
        {
            try
            {
                List<TransactionTypeMaster> transactionTypeMasters = new List<TransactionTypeMaster>() {
                    new TransactionTypeMaster()
                    {
                        TransactionType = "Single",
                        ShortCode = "SGT",
                        Description = "Single Transaction"
                    },
                    new TransactionTypeMaster()
                    {
                        TransactionType = "Single",
                        ShortCode = "SGT",
                        Description = "Single Transaction Gross"
                    },
                    new TransactionTypeMaster()
                    {
                        TransactionType = "Two way",
                        ShortCode = "FT",
                        Description = "First and Second Transaction"
                    },
                    new TransactionTypeMaster()
                    {
                        TransactionType = "Multi",
                        ShortCode = "FMT",
                        Description = "Multi Transaction"
                    }
                };
                return transactionTypeMasters;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetTransactionTypeMasters/Exception:- " + ex.Message, ex);
                return null;
            }
        }

        public List<TransactionTypeMaster> GetTransactionTypeMastersForAWSTransactions()
        {
            try
            {
                List<TransactionTypeMaster> transactionTypeMasters = new List<TransactionTypeMaster>() {
                    new TransactionTypeMaster()
                    {
                        TransactionType = "Single",
                        ShortCode = "SGT",
                        Description = "Single Transaction"
                    },
                    new TransactionTypeMaster()
                    {
                        TransactionType = "Two way",
                        ShortCode = "FT",
                        Description = "First and Second Transaction"
                    },
                    new TransactionTypeMaster()
                    {
                        TransactionType = "Multi",
                        ShortCode = "FMT",
                        Description = "Multi Transaction"
                    }
                };
                return transactionTypeMasters;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetTransactionTypeMasters/Exception:- " + ex.Message, ex);
                return null;
            }
        }

        public List<IsSapBased> GetIsSapBasedInRFIDAllocation()
        {
            try
            {
                List<IsSapBased> sapBased = new List<IsSapBased>() {
                    new IsSapBased()
                    {
                        ShortCode = true,
                        Description = "SAP"
                    },
                    new IsSapBased()
                    {
                        ShortCode = false,
                        Description = "Non SAP"
                    }
                };
                return sapBased;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetIsSapBasedInRFIDAllocation/Exception:- " + ex.Message, ex);
                return null;
            }
        }

        public List<IsLoaded> GetIsLoaded()
        {
            try
            {
                List<IsLoaded> loaded = new List<IsLoaded>() {
                    new IsLoaded()
                    {
                        ShortCode = true,
                        Description = "Loaded"
                    },
                    new IsLoaded()
                    {
                        ShortCode = false,
                        Description = "Empty"
                    }
                };
                return loaded;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetIsLoaded/Exception:- " + ex.Message, ex);
                return null;
            }
        }

        public List<MaterialMaster> GetMaterialMasters()
        {
            List<MaterialMaster> materialMasters = new List<MaterialMaster>();
            try
            {
                var Query = "SELECT * FROM [Material_Master] WHERE IsDeleted='FALSE'";
                SqlCommand cmd1 = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd1, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                if (JSONString != null)
                {
                    materialMasters = JsonConvert.DeserializeObject<List<MaterialMaster>>(JSONString);
                }
                return materialMasters;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetMaterialMasters/Exception:- " + ex.Message, ex);
                return null;
            }
        }

        public RFIDReaderMaster GetRFIDReaderByReader(string reader, string hardwareProfile)
        {
            try
            {
                string Query = $@"SELECT * FROM RFID_Reader_Master WHERE [Reader]='{reader}' and [HardwareProfile]='{hardwareProfile}'";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var res = JsonConvert.DeserializeObject<List<RFIDReaderMaster>>(JSONString);
                return res.Count > 0 ? res[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetRFIDReaderByTag", ex);
                return null;
            }
        }

        public PLCMaster GetPLCMaster(string hardwareProfile)
        {
            try
            {
                string Query = $"SELECT * FROM PLC_Settings WHERE [HardwareProfile]='{hardwareProfile}'";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var res = JsonConvert.DeserializeObject<List<PLCMaster>>(JSONString);
                return res.Count > 0 ? res[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetPLCMaster", ex);
                return null;
            }
        }

        public SerialCommunicationSetting GetSerialCommunicationSetting(string hardwareProfile)
        {
            try
            {
                string Query = $"SELECT * FROM Serial_COM_Setting where HardwareProfile='{hardwareProfile}'";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var res = JsonConvert.DeserializeObject<List<SerialCommunicationSetting>>(JSONString);
                return res.Count > 0 ? res[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetPLCMaster", ex);
                return null;
            }
        }

        public AWSConfiguration GetAWSConfiguration(string hardwareProfile)
        {
            try
            {
                string Query = $"SELECT * FROM AWS_Configurations where HardwareProfile='{hardwareProfile}'";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var res = JsonConvert.DeserializeObject<List<AWSConfiguration>>(JSONString);
                return res.Count > 0 ? res[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetAWSConfiguration", ex);
                return null;
            }
        }

        public List<SupplierMaster> GetSupplierMasters()
        {
            List<SupplierMaster> supplierMasters = new List<SupplierMaster>();
            try
            {
                var Query = "SELECT * FROM [Supplier_Master] WHERE IsDeleted='FALSE'";
                SqlCommand cmd1 = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd1, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                if (JSONString != null)
                {
                    supplierMasters = JsonConvert.DeserializeObject<List<SupplierMaster>>(JSONString);
                }
                return supplierMasters;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetSupplierMasters/Exception:- " + ex.Message, ex);
                return null;
            }
        }
        public List<ShiftMaster> GetShiftMasters()
        {
            List<ShiftMaster> shiftMasters = new List<ShiftMaster>();
            try
            {
                var Query = "SELECT * FROM [Shift_Master] WHERE IsDeleted=0";
                SqlCommand cmd1 = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd1, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                if (JSONString != null)
                {
                    shiftMasters = JsonConvert.DeserializeObject<List<ShiftMaster>>(JSONString);
                }
                return shiftMasters;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetShiftMasters/Exception:- " + ex.Message, ex);
                return null;
            }
        }


        public List<TableDetails> GetTableDetails()
        {
            try
            {
                string Query = "SELECT TABLE_NAME as TableName FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<TableDetails>>(JSONString);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetTableColumnDetails/Exception:- " + ex.Message, ex);
                return null;
            }
        }
        public List<TableColumnDetails> GetTableColumnDetails(string TableName)
        {
            try
            {
                //string Query = "SELECT TableName=@TableName, name as ColumnName FROM sys.columns WHERE object_id = OBJECT_ID(@TableName)";
                string Query = "SELECT TableName=@TableName, COLUMN_NAME as ColumnName, DATA_TYPE as DataType FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME =@TableName";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@TableName", TableName);
                DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<TableColumnDetails>>(JSONString);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetTableColumnDetails/Exception:- " + ex.Message, ex);
                return null;
            }
        }

        public void InsertSAPDataBackUpDetails(SAPDataBackUp sapDataBackUp)
        {
            try
            {
                string insertQuery = $@"INSERT INTO [SAP_Data_BackUp] (Trans,Type,Date,Payload,Response,NoOfRetry,Status,CompletedTrans,TransId,TransType) 
                                                Values ('{sapDataBackUp.Trans}','{sapDataBackUp.Type}','{sapDataBackUp.Date}','{sapDataBackUp.Payload}','{sapDataBackUp.Response}','{sapDataBackUp.NoOfRetry}','{sapDataBackUp.Status}','{sapDataBackUp.CompletedTrans}','{sapDataBackUp.TransId}','{sapDataBackUp.TransType}')";
                SqlCommand cmd = new SqlCommand(insertQuery);
                masterDBCall.InsertData(cmd, CommandType.Text);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void InsertSystemConfigurationDetails(SystemConfigurations systemConfiguration)
        {
            try
            {
                string insertQuery = $@"INSERT INTO [Sytem_Configuration] (Name,HardwareProfile) 
                                                Values ('{systemConfiguration.Name}','{systemConfiguration.HardwareProfile}')";
                SqlCommand cmd = new SqlCommand(insertQuery);
                masterDBCall.InsertData(cmd, CommandType.Text);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateSAPDataBackUpById(SAPDataBackUp sapDataBackUp)
        {
            try
            {
                string Query = $@"UPDATE [SAP_Data_BackUp] SET Trans='{sapDataBackUp.Trans}',Type='{sapDataBackUp.Type}',Date='{DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss")}',Payload='{sapDataBackUp.Payload}',
                               Response ='{sapDataBackUp.Response}',NoOfRetry='{sapDataBackUp.NoOfRetry}',Status='{sapDataBackUp.Status}' where Id='{sapDataBackUp.Id}'";
                SqlCommand cmd = new SqlCommand(Query);
                masterDBCall.InsertData(cmd, CommandType.Text);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/UpdateSAPDataBackUpById:" + ex.Message);
            }
        }

        public List<SAPDataBackUp> GetSAPDataBackUpByNoOfRetryAndStatus()
        {
            try
            {
                string Query = "SELECT * FROM SAP_Data_BackUp WHERE NoOfRetry < 5 and Status='failed'";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var res = JsonConvert.DeserializeObject<List<SAPDataBackUp>>(JSONString);
                return res;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetSAPDataBackUpByNoOfRetryAndStatus", ex);
                return null;
            }
        }

        public List<TransactionDetails> GetTransactionDetailsByTicket(int TicketNo)
        {
            try
            {
                string Query = $"Select * from Transaction_Details where TicketNo={TicketNo}";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<TransactionDetails>>(JSONString);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetTransactionDetailsByTicket:" + ex.Message);
                return null;
            }
        }

        public SavedReportTemplate CreateSavedReportTemplate(SavedReportTemplate data)
        {
            try
            {
                string Query = $"CreateSavedReportTemplate";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@ReportName", data.ReportName);
                cmd.Parameters.AddWithValue("@TableName", data.TableName);
                cmd.Parameters.AddWithValue("@Query", data.Query);
                cmd.Parameters.AddWithValue("@WhereEnabled", data.WhereEnabled);
                DataTable table = masterDBCall.GetData(cmd, CommandType.StoredProcedure);
                string JSONString = JsonConvert.SerializeObject(table);
                var des = JsonConvert.DeserializeObject<List<SavedReportTemplate1>>(JSONString);
                if (des != null && des.Count > 0)
                {
                    data.TemplateID = (Int32)des[0].TemplateID;
                }
                return data;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateSavedReportTemplate:" + ex.Message);
                return null;
            }
        }

        public SavedReportTemplate UpdateSavedReportTemplate(SavedReportTemplate data)
        {
            try
            {
                string Query = $"DeleteAndUpdateSavedReportTemplate";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@TemplateID", data.TemplateID);
                cmd.Parameters.AddWithValue("@ReportName", data.ReportName);
                cmd.Parameters.AddWithValue("@TableName", data.TableName);
                cmd.Parameters.AddWithValue("@Query", data.Query);
                cmd.Parameters.AddWithValue("@WhereEnabled", data.WhereEnabled);
                DataTable table = masterDBCall.GetData(cmd, CommandType.StoredProcedure);
                //string JSONString = JsonConvert.SerializeObject(table);
                //var des = JsonConvert.DeserializeObject<List<SavedReportTemplate1>>(JSONString);
                //if (des != null && des.Count > 0)
                //{
                //    data.TemplateID = (Int32)des[0].TemplateID;
                //}
                return data;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateSavedReportTemplate:" + ex.Message);
                return null;
            }
        }

        public bool CreateSavedReportTemplateFields(List<SavedReportTemplateFields> fields, SavedReportTemplate data)
        {
            try
            {
                foreach (SavedReportTemplateFields field in fields)
                {
                    string Query = "INSERT INTO [Saved_ReportTemplateFields] (TemplateID,FieldName,DataType,TableName) VALUES(@TemplateID,@FieldName,@DataType,@TableName)";
                    SqlCommand cmd = new SqlCommand(Query);
                    cmd.Parameters.AddWithValue("@TemplateID", data.TemplateID);
                    cmd.Parameters.AddWithValue("@FieldName", field.FieldName);
                    cmd.Parameters.AddWithValue("@DataType", field.DataType);
                    cmd.Parameters.AddWithValue("@TableName", field.TableName);
                    masterDBCall.InsertData(cmd, CommandType.Text);
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateSavedReportTemplateFields:" + ex.Message);
                return false;
            }
        }

        public bool CreateSavedReportTemplateWhereFields(List<SavedReportTemplateWhereFields> fields, SavedReportTemplate data)
        {
            try
            {
                foreach (SavedReportTemplateWhereFields field in fields)
                {
                    string Query = "INSERT INTO [Saved_ReportTemplateWhereFields] (TemplateID,FieldName,DataType,TableName,MatchedColumnName) VALUES(@TemplateID,@FieldName,@DataType,@TableName,@MatchedColumnName)";
                    SqlCommand cmd = new SqlCommand(Query);
                    cmd.Parameters.AddWithValue("@TemplateID", data.TemplateID);
                    cmd.Parameters.AddWithValue("@FieldName", field.FieldName);
                    cmd.Parameters.AddWithValue("@DataType", field.DataType);
                    cmd.Parameters.AddWithValue("@TableName", field.TableName);
                    cmd.Parameters.AddWithValue("@MatchedColumnName", field.MatchedColumnName);
                    masterDBCall.InsertData(cmd, CommandType.Text);
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateSavedReportTemplateWhereFields:" + ex.Message);
                return false;
            }
        }

        public bool DeleteTemplate(SavedReportTemplate template)
        {
            try
            {
                string Query = $"DELETE FROM Saved_ReportTemplate WHERE TemplateID = {template.TemplateID}";
                SqlCommand cmd = new SqlCommand(Query);
                masterDBCall.InsertData(cmd, CommandType.Text);
                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("DeleteTemplate : " + ex.Message);
                return false;
            }
        }


        #region Caption

        public List<Caption> GetCaptions(string TableName)
        {
            List<Caption> captions = new List<Caption>();
            try
            {
                string Query = "[GetCaptions_Proc]";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@TableName", TableName);
                DataTable dt = masterDBCall.GetData(cmd, CommandType.StoredProcedure);
                string JSONString = JsonConvert.SerializeObject(dt);
                captions = JsonConvert.DeserializeObject<List<Caption>>(JSONString);
                return captions;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetCaptions : " + ex.Message);
                return captions;
            }
        }

        public bool CreateCaptions(List<Caption> captions)
        {
            try
            {
                foreach (Caption cap in captions)
                {
                    string Query = "[CreateCaption_Proc]";
                    SqlCommand cmd = new SqlCommand(Query);
                    cmd.Parameters.AddWithValue("@TableName", cap.TableName);
                    cmd.Parameters.AddWithValue("@FieldName", cap.FieldName);
                    cmd.Parameters.AddWithValue("@CaptionName", cap.CaptionName);
                    cmd.Parameters.AddWithValue("@Width", cap.Width);
                    masterDBCall.InsertData(cmd, CommandType.StoredProcedure);
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateSavedReportTemplateWhereFields:" + ex.Message);
                return false;
            }
        }

        public bool DeleteCaption(Caption caption)
        {
            try
            {
                string Query = "[DeleteCaption_Proc]";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@CaptionID", caption.CaptionID);
                masterDBCall.InsertData(cmd, CommandType.StoredProcedure);

                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateSavedReportTemplateWhereFields:" + ex.Message);
                return false;
            }
        }

        #endregion


        public List<RolePriviliege> GetRolesAndPreviledgesByRole(string Role)
        {
            try
            {
                string Query = "SELECT * FROM User_Previledges WHERE Role=@Role";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@Role", Role);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                return JsonConvert.DeserializeObject<List<RolePriviliege>>(JSONString);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetRolesAndPreviledgesByRole", ex);
                return null;
            }

        }

        public List<UserHardwareProfile> GetUserHardwareProfileByProfile(string HardwareProfile)
        {
            try
            {
                string Query = "SELECT * FROM User_HardwareProfiles WHERE HardwareProfileName=@HardwareProfile";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@HardwareProfile", HardwareProfile);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                return JsonConvert.DeserializeObject<List<UserHardwareProfile>>(JSONString);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetRolesAndPreviledgesByRole", ex);
                return null;
            }

        }

        public WeighbridgeSettings GetWeighbridgeSettings()
        {
            try
            {
                string Query = "SELECT * FROM Weighbridge_Settings";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                List<WeighbridgeSettings> weighbridgeSettings = JsonConvert.DeserializeObject<List<WeighbridgeSettings>>(JSONString);
                return (weighbridgeSettings != null && weighbridgeSettings.Count > 0) ? weighbridgeSettings[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetWeighbridgeSettings", ex);
                return null;
            }
        }

        public StableWeightConfiguration GetStableWeightConfiguration()
        {
            try
            {
                string Query = "SELECT ID,StableWeightCount,MinimumWeightCount,StablePLCCount FROM Other_Settings";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                List<StableWeightConfiguration> stableWeightConfigurations = JsonConvert.DeserializeObject<List<StableWeightConfiguration>>(JSONString);
                return (stableWeightConfigurations != null && stableWeightConfigurations.Count > 0) ? stableWeightConfigurations[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetRolesAndPreviledgesByRole", ex);
                return null;
            }

        }

        public MSG91Master GetMSG91Master()
        {
            try
            {
                string Query = "SELECT * FROM MSG91_Master";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                List<MSG91Master> mSG91Masters = JsonConvert.DeserializeObject<List<MSG91Master>>(JSONString);
                return (mSG91Masters != null && mSG91Masters.Count > 0) ? mSG91Masters[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetMSG91Master", ex);
                return null;
            }

        }

        public SendGridMaster GetSendGridMaster()
        {
            try
            {
                string Query = "SELECT * FROM SendGrid_Master";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                List<SendGridMaster> sendGridMasters = JsonConvert.DeserializeObject<List<SendGridMaster>>(JSONString);
                return (sendGridMasters != null && sendGridMasters.Count > 0) ? sendGridMasters[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetSendGridMaster", ex);
                return null;
            }

        }

        public bool CheckAndSendEmail1(string Message, byte[] bytearray, string FileName)
        {
            try
            {
                SMTPSetting setting = GetSMTPDetails();
                MailSetting mailSetting = GetMailDetails();
                if (setting != null && mailSetting != null)
                {
                    return SendMail1(setting, mailSetting, Message, bytearray, FileName);
                }
                return false;

            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/CheckAndSendEmail :- " + ex.Message);
                return false;
            }
        }

        public bool SendMail1(SMTPSetting setting, MailSetting mailSetting, string Message, byte[] bytearray, string FileName)
        {
            try
            {
                if (!string.IsNullOrEmpty(mailSetting.ToID))
                {
                    Stream stream = new MemoryStream(bytearray);
                    var attachment1 = new System.Net.Mail.Attachment(stream, "summary", "application/pdf");
                    string hostName = setting.Host;
                    string SMTPEmail = mailSetting.FromID;
                    string SMTPEmailPassword = mailSetting.Password;
                    string SMTPPort = setting.Port;
                    var message = new MailMessage();
                    string subject = "";
                    //StringBuilder sb = new StringBuilder();
                    //sb.Append(string.Format("Dear Sir/Madam,<br/>"));
                    //sb.Append($"{mailSetting.Message}");

                    //sb.Append("You have invited to register in our BPCloud by Usha Martin Limited, Request you to proceed with registration");
                    //sb.Append("<p><a href=\"" + PortalAddress + "/#/register/vendor?token=" + code + "&Id=" + TransID + "&Email=" + toEmail + "\"" + ">Register</a></p>");
                    //sb.Append($"<i>Note: The verification link will expire in {_tokenTimespan} days.<i>");
                    //sb.Append("<p>Regards,</p><p>Admin</p>");

                    //sb.Append(@"<html><head></head><body><div style='border:1px solid #dbdbdb;'><div style='padding: 20px 20px; background-color: #fff06769;text-align: center;font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;'><p><h2>Usha Martin Vendor Onboadring</h2></p></div><div style='background-color: #f8f7f7;padding: 20px 20px;font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;'><div style='padding: 20px 20px;border:1px solid white;background-color: white !important;'><p>Dear concern</p><p>You have invited to register in our BPCloud by Usha Martin Limited, Request you to proceed with registration</p><div style='text-align: end;'>" + "<a href =\"" + PortalAddress + "/#/register/vendor?token=" + code + "&Id=" + TransID + "&Email=" + toEmail + "\"" + "><button style='width: 90px;height: 28px;backgroud-color:red;background-color: #008CBA;color: white'>Register</button></a>" + "</div></div></div></div></body></html>");

                    //sb.Append(@"<html><head></head><body> <div style='border:1px solid #dbdbdb;'> <div style='padding: 20px 20px; background-color: #fff06769;text-align: center;font-family: Segoe UI;'> <p> <h2>Usha Martin Vendor Onboadring</h2> </p> </div> <div style='background-color: #f8f7f7;padding: 20px 20px;font-family: Segoe UI'> <div style='padding: 20px 20px;border:1px solid white;background-color: white !important'> <p>Dear concern,</p> <p>You have invited to register in our BPCloud by Usha Martin Limited, Request you to proceed with registration.</p> <div style='text-align: end;'>" + "<a href=\"" + PortalAddress + "/#/register/vendor?token=" + code + "&Id=" + TransID + "&Email=" + toEmail + "\"" + "><button style='width: 90px;height: 28px; background-color: #039be5;color: white'>Register</button> </div> <p>Note: The verification link will expire in " + _tokenTimespan + " days.</p> <p>Regards,</p> <p>Admin</p> </div> </div> </div></body></html>");
                    subject = $"Summary Report : ";
                    SmtpClient client = new SmtpClient();
                    client.Port = Convert.ToInt32(SMTPPort);
                    client.Host = hostName;
                    client.EnableSsl = true;
                    client.Timeout = 60000;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential(SMTPEmail, SMTPEmailPassword);
                    string deserializedMessage = System.Net.WebUtility.HtmlDecode(Message);
                    MailMessage reportEmail = new MailMessage(SMTPEmail, mailSetting.ToID, subject, deserializedMessage);
                    var CCs = mailSetting.CCList.Split(',');
                    foreach (var cc in CCs)
                    {
                        if (!string.IsNullOrEmpty(cc))
                            reportEmail.CC.Add(new MailAddress(cc));
                    }
                    reportEmail.BodyEncoding = UTF8Encoding.UTF8;
                    reportEmail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                    reportEmail.IsBodyHtml = true;
                    //reportEmail.Attachments.Add(attachment1);
                    if (bytearray != null && bytearray.Length > 0)
                    {
                        System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(new MemoryStream(bytearray), MediaTypeNames.Application.Octet);
                        ContentDisposition disposition = attachment.ContentDisposition;
                        disposition.CreationDate = DateTime.Now;
                        disposition.ModificationDate = DateTime.Now;
                        disposition.ReadDate = DateTime.Now;
                        disposition.FileName = FileName;
                        disposition.Size = bytearray.Length;
                        disposition.DispositionType = DispositionTypeNames.Attachment;
                        reportEmail.Attachments.Add(attachment);
                    }

                    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    client.Send(reportEmail);
                    WriteLog.WriteToFile($"Mail has been sent successfully to {mailSetting.ToID}");
                    return true;
                }
                return false;
            }
            catch (SmtpFailedRecipientsException ex)
            {
                for (int i = 0; i < ex.InnerExceptions.Length; i++)
                {
                    SmtpStatusCode status = ex.InnerExceptions[i].StatusCode;
                    if (status == SmtpStatusCode.MailboxBusy ||
                        status == SmtpStatusCode.MailboxUnavailable)
                    {
                        WriteLog.WriteToFile("CommonFunction/SendMail/MailboxBusy/MailboxUnavailable/SmtpFailedRecipientsException:Inner- " + ex.InnerExceptions[i].Message);
                    }
                    else
                    {
                        WriteLog.WriteToFile("CommonFunction/SendMail/SmtpFailedRecipientsException:Inner- " + ex.InnerExceptions[i].Message);
                    }
                }
                WriteLog.WriteToFile("CommonFunction/SendMail/SmtpFailedRecipientsException:- " + ex.Message, ex);
                return false;
            }
            catch (SmtpException ex)
            {
                WriteLog.WriteToFile("CommonFunction/SendMail/SmtpException:- " + ex.Message, ex);
                return false;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/SendMail/Exception:- " + ex.Message, ex);
                return false;
            }

        }

        public bool SendTransactionDetailsToCloudApp(CloudAppConfig selectedCloudAppConfig, DataTable CurrentTransactionDataTable, int TicketNumber)
        {
            try
            {
                string responseFromServer = "";
                var url = selectedCloudAppConfig.BaseURL + "/save/transaction";
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                var JSONString = JsonConvert.SerializeObject(CurrentTransactionDataTable);
                List<dynamic> Transactionlist = JsonConvert.DeserializeObject<List<dynamic>>(JSONString);

                string postData = JSONString;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                request.ContentType = "application/json";
                request.ContentLength = byteArray.Length;

                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }


                WebResponse response = request.GetResponse();
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                //if(((HttpWebResponse)response).StatusCode==HttpStatusCode.OK)
                using (Stream dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                    responseFromServer = reader.ReadToEnd();
                }

                response.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/SendTransactionDetailsToCloudApp/Exception:- " + ex.Message, ex);
                return false;
            }
            return true;
        }

        #region Failed SMS,Mail
        public void CaptureFailedMail(FailedMailSMS failedMail)
        {
            string InsertQuery = $@"INSERT INTO [Failed_Mail_SMS] (FromID,ToID,Subject,Text,FileName,FilePath,CCList,MailFlag,UserName) VALUES('{failedMail.FromID}','{failedMail.ToID}','{failedMail.Subject}','{failedMail.Text}','{failedMail.FileName}','{failedMail.FilePath}','{failedMail.CCList}','{failedMail.MailFlag}','{failedMail.UserName}')";
            var res = adminDB.ExecuteQuery(InsertQuery);
            if (res)
            {
                WriteLog.WriteToFile("CommonFunction/CaptureFailedMail:- Failed Mail captured");
            }
        }
        public void CaptureFailedSMS(FailedSMS failedSMS)
        {
            string InsertQuery = $@"INSERT INTO [Failed_SMS] (MobileNo1,MobileNo2,MobileNo3,Message,SMSRoute,SMSFlag) VALUES('{failedSMS.MobileNo1}','{failedSMS.MobileNo2}','{failedSMS.MobileNo3}','{failedSMS.Message}','{failedSMS.SMSRoute}','{failedSMS.SMSFlag}')";
            var res = adminDB.ExecuteQuery(InsertQuery);
            if (res)
            {
                WriteLog.WriteToFile("CommonFunction/CaptureFailedSMS:- Failed SMS captured");
            }
        }
        public List<FailedSMS> GetFailedSMS()
        {
            string Query = "SELECT * FROM Failed_SMS WHERE SMSFlag='0'";
            SqlCommand cmd = new SqlCommand(Query);
            DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
            string JSONString = JsonConvert.SerializeObject(table);
            List<FailedSMS> failedSMSs = JsonConvert.DeserializeObject<List<FailedSMS>>(JSONString);
            return failedSMSs;
        }
        public async void RetriggerFailedSMS()
        {
            try
            {
                List<FailedSMS> failedSMSs = GetFailedSMS();
                SMSTemplate template = GetSMSTemplate();
                foreach (var failedSMS in failedSMSs)
                {
                    await RetrySMS(template, failedSMS.Message, failedSMS.ID, failedSMS.SMSRoute);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/RetriggerFailedSMS/Exception:- " + ex.Message, ex);
            }
        }
        public async Task RetrySMS(SMSTemplate template, string Message, int failID, string autoSMS)
        {
            try
            {
                MSG91Master mSG91Master = GetMSG91Master();
                if (mSG91Master != null)
                {
                    string authKey = mSG91Master.AuthKey;
                    string mobileNumber = template.PhoneNo1;
                    if (!string.IsNullOrEmpty(template.PhoneNo2))
                    {
                        mobileNumber += $",{template.PhoneNo2}";
                    }
                    if (!string.IsNullOrEmpty(template.PhoneNo3))
                    {
                        mobileNumber += $",{template.PhoneNo3}";
                    }
                    if (!string.IsNullOrEmpty(template.AutoSMSNumber))
                    {
                        mobileNumber += $",{autoSMS}";
                    }
                    string senderId = mSG91Master.SenderID;
                    string DLT_TE_ID = mSG91Master.DLT_TE_ID;
                    string PE_ID = mSG91Master.PE_ID;
                    string message = HttpUtility.UrlEncode($"{Message}");
                    StringBuilder sbPostData = new StringBuilder();
                    sbPostData.AppendFormat("authkey={0}", authKey);
                    sbPostData.AppendFormat("&mobiles={0}", mobileNumber);
                    sbPostData.AppendFormat("&message={0}", message);
                    sbPostData.AppendFormat("&sender={0}", senderId);
                    sbPostData.AppendFormat("&route={0}", "4");
                    sbPostData.AppendFormat("&DLT_TE_ID={0}", DLT_TE_ID);
                    sbPostData.AppendFormat("&PE_ID={0}", PE_ID);
                    string sendSMSUri = "http://api.msg91.com/api/sendhttp.php";
                    HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(sendSMSUri);
                    UTF8Encoding encoding = new UTF8Encoding();
                    byte[] data = encoding.GetBytes(sbPostData.ToString());
                    httpWReq.Method = "POST";
                    httpWReq.ContentType = "application/x-www-form-urlencoded";
                    httpWReq.ContentLength = data.Length;
                    using (Stream stream = httpWReq.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                    WebResponse response = await httpWReq.GetResponseAsync();
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseString = reader.ReadToEnd();
                    reader.Close();
                    response.Close();
                    UpdateSMSFlagStatus(failID, 1);
                }

            }
            catch (SystemException ex)
            {
                WriteLog.WriteToFile("CommonFunction/SendSMS/SystemException:- " + ex.Message, ex);
                UpdateSMSFlagStatus(failID, 0);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/SendSMS/Exception:- " + ex.Message, ex);
                UpdateSMSFlagStatus(failID, 0);
            }
        }
        public void UpdateSMSFlagStatus(int ID, int Status)
        {
            string updateQuery = $@"update [Failed_SMS] set SMSFlag='{Status}' where ID='{ID}'";
            var result = adminDB.ExecuteQuery(updateQuery);
            if (result)
            {
                WriteLog.WriteToFile($"CommonFunction/UpdateSMSFlagStatus:- FailedSMS {ID} flag set to {Status}");
            }
        }
        public List<FailedMailSMS> GetFailedMailSMS()
        {
            string Query = "SELECT * FROM Failed_Mail_SMS WHERE MailFlag='0'";
            SqlCommand cmd = new SqlCommand(Query);
            DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
            string JSONString = JsonConvert.SerializeObject(table);
            List<FailedMailSMS> failedMailSMS = JsonConvert.DeserializeObject<List<FailedMailSMS>>(JSONString);
            return failedMailSMS;
        }
        public async void RetriggerFailedMails()
        {
            try
            {
                List<FailedMailSMS> failedMailSMS = GetFailedMailSMS();
                SMTPSetting setting = GetSMTPDetails();
                MailSetting mailSetting = GetMailDetails();
                foreach (var failedMail in failedMailSMS)
                {
                    mailSetting.ToID = failedMail.ToID;
                    await RetryMail(setting, mailSetting, failedMail);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/RetriggerFailedMails/Exception:- " + ex.Message, ex);
            }
        }
        public async Task RetryMail(SMTPSetting setting, MailSetting mailSetting, FailedMailSMS failed)
        {
            try
            {
                if (!string.IsNullOrEmpty(failed.ToID))
                {
                    string hostName = setting.Host;
                    string SMTPEmail = mailSetting.FromID;
                    string SMTPEmailPassword = mailSetting.Password;
                    string SMTPPort = setting.Port;
                    var message = new MailMessage();
                    string subject = failed.Subject;
                    SmtpClient client = new SmtpClient();
                    client.Port = Convert.ToInt32(SMTPPort);
                    client.Host = hostName;
                    client.EnableSsl = true;
                    client.Timeout = 60000;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential(SMTPEmail, SMTPEmailPassword);
                    string deserializedMessage = System.Net.WebUtility.HtmlDecode(failed.Text);
                    MailMessage reportEmail = new MailMessage(SMTPEmail, failed.ToID, subject, deserializedMessage);
                    var CCs = mailSetting.CCList.Split(',');
                    foreach (var cc in CCs)
                    {
                        if (!string.IsNullOrEmpty(cc))
                            reportEmail.CC.Add(new MailAddress(cc));
                    }
                    reportEmail.BodyEncoding = UTF8Encoding.UTF8;
                    reportEmail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                    reportEmail.IsBodyHtml = true;
                    //reportEmail.Attachments.Add
                    byte[] bytearray = null;
                    if (File.Exists(failed.FilePath))
                    {
                        bytearray = File.ReadAllBytes(failed.FilePath);
                    }

                    if (bytearray != null && bytearray.Length > 0)
                    {
                        System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(new MemoryStream(bytearray), MediaTypeNames.Application.Octet);
                        ContentDisposition disposition = attachment.ContentDisposition;
                        disposition.CreationDate = DateTime.Now;
                        disposition.ModificationDate = DateTime.Now;
                        disposition.ReadDate = DateTime.Now;
                        disposition.FileName = failed.FileName;
                        disposition.Size = bytearray.Length;
                        disposition.DispositionType = DispositionTypeNames.Attachment;
                        reportEmail.Attachments.Add(attachment);
                    }

                    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    await client.SendMailAsync(reportEmail);
                    WriteLog.WriteToFile($"Mail has been sent successfully to {mailSetting.ToID}");
                    if (File.Exists(failed.FilePath))
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        File.Delete(failed.FilePath);
                    }
                    UpdateMailFlagStatus(failed.ID, 1);
                }
            }
            catch (SmtpFailedRecipientsException ex)
            {
                for (int i = 0; i < ex.InnerExceptions.Length; i++)
                {
                    SmtpStatusCode status = ex.InnerExceptions[i].StatusCode;
                    if (status == SmtpStatusCode.MailboxBusy ||
                        status == SmtpStatusCode.MailboxUnavailable)
                    {
                        WriteLog.WriteToFile("CommonFunction/SendMail/MailboxBusy/MailboxUnavailable/SmtpFailedRecipientsException:Inner- " + ex.InnerExceptions[i].Message);
                    }
                    else
                    {
                        WriteLog.WriteToFile("CommonFunction/SendMail/SmtpFailedRecipientsException:Inner- " + ex.InnerExceptions[i].Message);
                    }
                }
                WriteLog.WriteToFile("CommonFunction/SendMail/SmtpFailedRecipientsException:- " + ex.Message, ex);
                UpdateMailFlagStatus(failed.ID, 0);
            }
            catch (SmtpException ex)
            {
                WriteLog.WriteToFile("CommonFunction/SendMail/SmtpException:- " + ex.Message, ex);
                UpdateMailFlagStatus(failed.ID, 0);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/SendMail/Exception:- " + ex.Message, ex);
                UpdateMailFlagStatus(failed.ID, 0);
            }
        }
        public void UpdateMailFlagStatus(int ID, int Status)
        {
            string updateQuery = $@"update [Failed_Mail_SMS] set MailFlag='{Status}' where ID='{ID}'";
            var result = adminDB.ExecuteQuery(updateQuery);
            if (result)
            {
                WriteLog.WriteToFile($"CommonFunction/UpdateFlagStatus:- FailedMailSMS {ID} flag set to {Status}");
            }
        }
        #endregion

        #region Camera
        public List<CCTVSettings> GetCCTVSettings(string hardwareProfile)
        {
            try
            {
                string Query = $"Select * from CCTV_Settings where HarwareProfile='{hardwareProfile}'";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<CCTVSettings>>(JSONString);
                return result;

            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetCCTVSettings/Exception:- " + ex.Message, ex);
                return null;
            }
        }

        public byte[] SaveCameraImage(ImageSource imageSource, string imagePath)
        {
            byte[] bytes = null;
            var encoder = new PngBitmapEncoder();
            if (imageSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)imageSource));
                using (FileStream stream = new FileStream(imagePath, FileMode.Create))
                {
                    encoder.Save(stream);
                }
                bytes = File.ReadAllBytes(imagePath);
            }
            return bytes;
        }
        #endregion

        #region RFID
        public RFIDReaderMaster GetRFIDReaderMasterById(string Id)
        {
            try
            {
                string Query = $@"SELECT * FROM RFID_Reader_Master WHERE [Reader]='{Id}' and [IsEnable]=1";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var res = JsonConvert.DeserializeObject<List<RFIDReaderMaster>>(JSONString);
                return res.Count > 0 ? res[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetRFIDReaderByTag", ex);
                return null;
            }
        }

        public RFIDReaderMaster GetRFIDReaderMasterByIdAndHardwareProfile(string Id, string hardwareProfile)
        {
            try
            {
                string Query = $@"SELECT * FROM RFID_Reader_Master WHERE [Reader]='{Id}' and [IsEnable]=1 and [HardwareProfile]='{hardwareProfile}'";
                SqlCommand cmd = new SqlCommand(Query);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var res = JsonConvert.DeserializeObject<List<RFIDReaderMaster>>(JSONString);
                return res.Count > 0 ? res[0] : null;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetRFIDReaderByTag", ex);
                return null;
            }
        }
        #endregion

        #region PLC
        int PlcRetryCount=0;
        public void SendCommandToPLC(string cmd)
        {
            try
            {
                PlcRetryCount++;
                if (MainWindow.plcClient.Client.IsConnected)
                {
                    MainWindow.plcClient.Client.Send(cmd);
                    PlcRetryCount = 0;
                }
                else
                {
                    if (PlcRetryCount > 15)
                        throw new Exception("Not able to reach PLC!!!");
                    Thread.Sleep(1000);
                    SendCommandToPLC(cmd);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteAWSLog("CommonFunctions/SendCommandToPLC/Exception:- ", ex);
                PlcRetryCount = 0;
                throw ex;
            }
        }
        public void RetryPlc(string cmd)
        {
            try
            {
                if (!MainWindow.plcClient.Client.IsConnected)
                    SendCommandToPLC(cmd);
            }
            catch (Exception ex)
            {
                WriteLog.WriteAWSLog("CommonFunctions/RetryPlc/Exception:- ", ex);
                throw ex;
            }
        }
        #endregion

        #region Store Module
        public List<StoreMaterialManagement> GetStoreMaterials(int allocationId)
        {
            try
            {
                string Query = "SELECT * FROM Store_Material_Management WHERE AllocationId=@AllocationId";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@AllocationId", allocationId);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var res = JsonConvert.DeserializeObject<List<StoreMaterialManagement>>(JSONString);
                if (res == null)
                    return new List<StoreMaterialManagement>();
                return res;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Store/GetStoreMaterials", ex);
                return null;
            }
        }
        public List<StoreSupplierManagement> GetStoreSuppliers(int allocationId)
        {
            try
            {
                string Query = "SELECT * FROM Store_Supplier_Management WHERE AllocationId=@AllocationId";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@AllocationId", allocationId);
                DataTable dt1 = masterDBCall.GetData(cmd, CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(dt1);
                var res = JsonConvert.DeserializeObject<List<StoreSupplierManagement>>(JSONString);
                if (res == null)
                    return new List<StoreSupplierManagement>();
                return res;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Store/GetStoreSuppliers", ex);
                return null;
            }
        }

        public void SaveStoreManagement(StoreManagement storeManagement, SqlTransaction transaction, SqlConnection con)
        {
            try
            {
                SqlCommand materialDeleteCmd = new SqlCommand($"delete from [Store_Material_Management] where AllocationId='{storeManagement.Materials[0].AllocationId}'", con, transaction);
                materialDeleteCmd.CommandType = CommandType.Text;
                materialDeleteCmd.ExecuteNonQuery();
                string query = $@"insert into [Store_Material_Management] (AllocationId,MaterialCode,MaterialName,[Order],Closed,ItemNo) values ";
                foreach (var item in storeManagement.Materials)
                {
                    string value = $@"('{item.AllocationId}','{item.MaterialCode}','{item.MaterialName}','{item.Order}','{item.Closed}','{item.ItemNo}'),";
                    query += value;
                }
                query = query.Remove(query.Count() - 1, 1);
                SqlCommand materialSaveCmd = new SqlCommand(query, con, transaction);
                materialSaveCmd.CommandType = CommandType.Text;
                materialSaveCmd.ExecuteNonQuery();
                if (storeManagement.Suppliers.Count > 0)
                {
                    SqlCommand supplierDeleteCmd = new SqlCommand($"delete from [Store_Supplier_Management] where AllocationId='{storeManagement.Suppliers[0].AllocationId}'", con, transaction);
                    supplierDeleteCmd.CommandType = CommandType.Text;
                    supplierDeleteCmd.ExecuteNonQuery();
                    string query1 = $@"insert into [Store_Supplier_Management] (AllocationId,SupplierCode,SupplierName,[Order]) values ";
                    foreach (var item in storeManagement.Suppliers)
                    {
                        string value = $@"('{item.AllocationId}','{item.SupplierCode}','{item.SupplierName}','{item.Order}'),";
                        query1 += value;
                    }
                    query1 = query1.Remove(query1.Count() - 1, 1);
                    SqlCommand cmd1 = new SqlCommand(query1, con, transaction);
                    cmd1.CommandType = CommandType.Text;
                    cmd1.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Store/GetStoreSuppliers", ex);
                throw ex;
            }
        }
        #endregion

        public SystemConfigurations GetSystemConfiguration(string systemId)
        {
            try
            {
                DataTable table = adminDB.GetAllData($"select * from [Sytem_Configuration] where [Name]='{systemId}'");
                var JsonString = JsonConvert.SerializeObject(table);
                var systemConfigurations = JsonConvert.DeserializeObject<List<SystemConfigurations>>(JsonString);
                return systemConfigurations.FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }

        }

        public SystemConfigurations GetSystemConfigurationBySystemIdAndHardwareProfile(string systemId, string hardwareProfile)
        {
            try
            {
                DataTable table = adminDB.GetAllData($"select * from [Sytem_Configuration] where [Name]='{systemId}' and [HardwareProfile]='{hardwareProfile}'");
                var JsonString = JsonConvert.SerializeObject(table);
                var systemConfigurations = JsonConvert.DeserializeObject<List<SystemConfigurations>>(JsonString);
                if (systemConfigurations != null)
                {
                    return systemConfigurations.FirstOrDefault();
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void AutoGateExit(RFIDAllocation allocation)
        {
            try
            {
                string vehicleQuery = $"UPDATE [Vehicle_Master] SET Status='' WHERE VehicleNumber='{allocation.VehicleNumber}'";
                var res = adminDB.ExecuteQuery(vehicleQuery);
                if (allocation.AllocationType == "Temporary")
                {
                    string allocationQuery = $"UPDATE [RFID_Allocations] SET Status='GateExit-Auto' WHERE AllocationId='{allocation.AllocationId}'";
                    adminDB.ExecuteQuery(allocationQuery);
                    string rfidquery = $"UPDATE [RFID_Tag_Master] SET Status='Open',VehicleNo='' WHERE Tag='{allocation.RFIDTag}'";
                    adminDB.ExecuteQuery(rfidquery);
                }
                else if (allocation.AllocationType == "Long-term Different Material")
                {
                    string allocationQuery = $"UPDATE [RFID_Allocations] SET Status='GateExit-Auto' WHERE AllocationId='{allocation.AllocationId}'";
                    adminDB.ExecuteQuery(allocationQuery);
                }
                else
                {
                    string allocationQuery = $"UPDATE [RFID_Allocations] SET Status='LTSM' WHERE AllocationId='{allocation.AllocationId}'";
                    adminDB.ExecuteQuery(allocationQuery);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string FormatTime(string time)
        {
            //var timeVal = time.Split(' ');
            //var timeSpan = TimeSpan.Parse(time);
            //var dateTime = DateTime.Today.Add(timeSpan);
            DateTime dt;
            if (!DateTime.TryParseExact(time, "hh:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                // handle validation error
            }
            return dt.ToString("HHmmss");
        }

        public RFIDMaster GetRFIDByVehicleNo(string vehicleNo)
        {
            try
            {
                DataTable table = adminDB.GetAllData($"select * from [RFID_Tag_Master] where [VehicleNo]='{vehicleNo}' and IsActive=1");
                var JsonString = JsonConvert.SerializeObject(table);
                var rfids = JsonConvert.DeserializeObject<List<RFIDMaster>>(JsonString);
                return rfids.FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void UpdateRFIDAllocationStatus(int id,string status)
        {
            try
            {
                var res=adminDB.GetAllData($"update [RFID_Allocations] set Status='{status}' where [AllocationId]={id}");
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/UpdateRFIDAllocationStatus", ex);
            }
        }
    }
}
