using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

namespace IWT.Setting_Pages
{
    /// <summary>
    /// Interaction logic for DB_Page.xaml
    /// </summary>
    public partial class DB_Page : Page
    {
        public static MasterDBCall masterDBCall = new MasterDBCall();
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        public DB_Page()
        {
            InitializeComponent();
            DataContext = new ChangePasswordViewModel();
            toastViewModel = new ToastViewModel();
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            bool IsOldPassword = CheckRequiredValidation(OldPassword.Password);
            bool IsNewPassword = CheckRequiredValidation(NewPassword.Password);
            if (IsOldPassword && IsNewPassword)
            {
                ChangePassword changePassword = new ChangePassword();
                changePassword.OldPassword = OldPassword.Password;
                changePassword.NewPassword = NewPassword.Password;
                if (changePassword.OldPassword != changePassword.NewPassword)
                {
                    await ChangeDBUserPassword(changePassword);
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "New password should be different from old password");
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill Old and New password");
            }
        }

        public async Task ChangeDBUserPassword(ChangePassword changePassword)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(masterDBCall.GetDecryptedConnectionStringDB());
                string user = builder.UserID;
                //string pass = builder.Password;

                string Query = "ChangeDBPassword_Proc";
                SqlCommand command = new SqlCommand(Query);
                command.Parameters.AddWithValue("@UserName", user);
                command.Parameters.AddWithValue("@OldPassword", changePassword.OldPassword);
                command.Parameters.AddWithValue("@NewPassword", changePassword.NewPassword);
                DataTable table = masterDBCall.GetData(command);
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<ChangePasswordResult>>(JSONString);
                if (result != null && result.Count > 0)
                {
                    var res = result[0];
                    if (res.Status == "Success")
                    {
                        builder.Password = changePassword.NewPassword;
                        Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        var connectionStringsSection = (ConnectionStringsSection)configuration.GetSection("connectionStrings");

                        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(builder.ConnectionString);
                        var EncryptedConnectionString = System.Convert.ToBase64String(plainTextBytes);

                        connectionStringsSection.ConnectionStrings["AuthContext"].ConnectionString = EncryptedConnectionString;
                        configuration.Save(ConfigurationSaveMode.Modified, true);
                        ConfigurationManager.RefreshSection("connectionStrings");
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "DB Password changed successfully!!");
                    }
                    else
                    {
                        WriteLog.WriteToFile("ChangeDBUserPassword:" + res.Message);
                        //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, res.Message);
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "You might not have permission to update the password , please check with administrator");
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
                WriteLog.WriteToFile("ChangeDBUserPassword:" + ex.Message);
            }
        }

        public bool CheckRequiredValidation(string Value)
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

    }
}
