using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IWT.DashboardPages
{
    /// <summary>
    /// Interaction logic for ChangePasswordDialog.xaml
    /// </summary>
    public partial class ChangePasswordDialog : UserControl
    {
        public static MasterDBCall masterDBCall = new MasterDBCall();
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        public ChangePasswordDialog()
        {
            InitializeComponent();
            DataContext = new ChangePasswordViewModel();
            toastViewModel = new ToastViewModel();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValid(OldPassword.Password) || !IsValid(NewPassword.Password) || !IsValid(ConfirmPassword.Password))
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Please fill value for all the fields");
            }
            else
            {
                var OldPassword1 = OldPassword.Password;
                var NewPassword1 = NewPassword.Password;
                var ConfirmPassword1 = ConfirmPassword.Password;
                if (OldPassword1 == NewPassword1)
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "New password should be different from old password");
                }
                else if (ConfirmPassword1 != NewPassword1)
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Confirm password should be same as New password");
                }
                else
                {
                    ChangePassword changePassword = new ChangePassword();
                    changePassword.UserName = "Admin";
                    changePassword.OldPassword = Encrypt(OldPassword.Password,true);
                    changePassword.NewPassword = Encrypt(NewPassword.Password, true);
                    ChangeUserPassword(changePassword);
                }
            }
        }

        public void ChangeUserPassword(ChangePassword changePassword)
        {
            try
            {
                string Query = "ChangePassword_Proc";
                SqlCommand command = new SqlCommand(Query);
                command.Parameters.AddWithValue("@UserName", changePassword.UserName);
                command.Parameters.AddWithValue("@OldPassword", changePassword.OldPassword);
                command.Parameters.AddWithValue("@NewPassword", changePassword.NewPassword);
                DataTable table = masterDBCall.GetData(command);
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<ChangePasswordResult>>(JSONString);
                if (result.Count > 0)
                {
                    var res = result[0];
                    if (res.Status == "Success")
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Password changed successfully!!");
                        DialogHost.CloseDialogCommand.Execute(res, null);
                    }
                    else
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, res.Message);
                    }
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong");
                }
               // DialogHost.CloseDialogCommand.Execute(null, null);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("ChangeUserPassword :" + ex.Message);
            }
        }

        public bool IsValid(string Value)
        {
            return !string.IsNullOrEmpty(Value);
        }
        public void ShowMessage(Action<string> message, string name)
        {
            this.Dispatcher.Invoke(() =>
            {
                LastMessage = name;
                message(LastMessage);
            });
        }

        private void PackIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
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
}
