using IWT.DBCall;
using IWT.Shared;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace IWT.FactorySetupPages
{
    /// <summary>
    /// Interaction logic for DataBaseDetailsDialog.xaml
    /// </summary>
    public partial class DataBaseDetailsDialog : UserControl
    {
        public static MasterDBCall masterDBCall = new MasterDBCall();
        public DataBaseDetailsDialog()
        {
            InitializeComponent();
        }

        private void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var DataSourceValue = DataSource.Text;
                var DatabaseValue = Database.Text;
                var UserIDValue = UserID.Text;
                var PasswordValue = Password.Password;
                if (string.IsNullOrEmpty(DataSourceValue) || string.IsNullOrEmpty(DatabaseValue) || string.IsNullOrEmpty(UserIDValue) || string.IsNullOrEmpty(PasswordValue))
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the mandatory fields");
                }
                else
                {
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(masterDBCall.GetDecryptedConnectionStringDB());
                    builder.DataSource = DataSourceValue;
                    builder.InitialCatalog = DatabaseValue;
                    builder.UserID = UserIDValue;
                    builder.Password = PasswordValue;

                    Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    var connectionStringsSection = (ConnectionStringsSection)configuration.GetSection("connectionStrings");
                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(builder.ConnectionString);
                    var EncryptedConnectionString = System.Convert.ToBase64String(plainTextBytes);
                    connectionStringsSection.ConnectionStrings["AuthContext"].ConnectionString = EncryptedConnectionString;
                    configuration.Save(ConfigurationSaveMode.Modified, true);
                    ConfigurationManager.RefreshSection("connectionStrings");

                    configuration.AppSettings.Settings["IsDataBaseDetailsAdded"].Value = "true";
                    configuration.Save(ConfigurationSaveMode.Modified,true);
                    ConfigurationManager.RefreshSection("appSettings");

                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Database details added successfully!!");
                    DialogHost.CloseDialogCommand.Execute("system", null);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SubmitBtn_Click:" + ex.Message);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong");
            }

        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Password.Password))
            {
                PasswordError.Visibility = Visibility.Visible;
            }
            else
            {
                PasswordError.Visibility = Visibility.Hidden;
            }
        }

    }
}
