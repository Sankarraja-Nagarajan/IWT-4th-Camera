using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using IWT.Views;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace IWT.Admin_Pages
{
    /// <summary>
    /// Interaction logic for ManageUser.xaml
    /// </summary>
    public partial class ManageUser : Page
    {
        string LastMessage;
        private readonly ToastViewModel toastViewModel;
        List<UserHardwareProfile> hardwareProfiles = new List<UserHardwareProfile>();
        UserHardwareProfile selectedHardwareProfile = new UserHardwareProfile();
        AdminDBCall adminDBCall = new AdminDBCall();
        MasterDBCall masterDBCall = new MasterDBCall();

        public ManageUser()
        {
            toastViewModel = new ToastViewModel();
            InitializeComponent();
            UpdateUser.IsEnabled = false;
            DeleteUser.IsEnabled = false;
            SaveButton.IsEnabled = true;
            MaterialGrid5.ItemsSource = LoadCollectionData1();
            GetRoleData();
            GetHardwareProfiles();
        }

        public void GetHardwareProfiles()
        {
            try
            {
                DataTable dt1 = adminDBCall.GetAllData("SELECT * FROM User_HardwareProfiles");
                string JSONString = JsonConvert.SerializeObject(dt1);
                hardwareProfiles = JsonConvert.DeserializeObject<List<UserHardwareProfile>>(JSONString);
                Profile.ItemsSource = hardwareProfiles;
                Profile.Items.Refresh();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetHardwareProfiles", ex);
            }

        }

        private void GetRoleData()
        {
            AdminDBCall db = new AdminDBCall();
            Usermanage list = new Usermanage();
            DataTable dt1 = db.GetAllData("SELECT * FROM User_Previledges");
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                foreach (DataRow row in dt1.Rows)
                {
                    list.Role = (row["Role"].ToString());
                    Role.Items.Add(list.Role);
                }
            }
        }

        private List<Usermanage> LoadCollectionData1()
        {
            AdminDBCall db = new AdminDBCall();
            Usermanage list = new Usermanage();

            List<Usermanage> authors1 = new List<Usermanage>();
            DataTable dt1 = db.GetAllData("SELECT * FROM User_Management");
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                foreach (DataRow row in dt1.Rows)
                {
                    list.ID = Convert.ToInt32(row["ID"].ToString());
                    list.Name = (row["UserName"].ToString());
                    var password = (row["Password"].ToString());
                    list.Password = Decrypt(password, true);
                    list.Email = (row["EmailID"].ToString());
                    list.Role = (row["Role"].ToString());
                    list.Profile = (row["HardwareProfile"].ToString());
                    var a = (row["IsLocked"].ToString());
                    if (a == "True")
                        list.Locked = true;
                    else
                        list.Locked = false;
                    authors1.Add(new Usermanage()
                    {
                        ID = list.ID,
                        Name = list.Name,
                        //Password = list.Password,
                        Email = list.Email,
                        Role = list.Role,
                        Profile = list.Profile,
                        Locked = list.Locked,
                    });
                }

            }

            return authors1;
        }


        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Name.IsEnabled = true;
            Email.IsEnabled = true;
            password.IsEnabled = true;
            confirmPassword.IsEnabled = true;
            Profile.IsEnabled = true;
            Locked.IsEnabled = true;
            Role.IsEnabled = true;
            ID.Text = "";
            Name.Text = "";
            Email.Text = "";
            password.Password = "";
            confirmPassword.Password = "";
            Profile.Text = "";
            Locked.IsChecked = false;
            Role.Text = "";
            UpdateUser.IsEnabled = false;
            DeleteUser.IsEnabled = false;
            SaveButton.IsEnabled = true;
            MaterialGrid5.ItemsSource = LoadCollectionData1();

        }
        public void ShowMessage(Action<string> message, string name)
        {
            this.Dispatcher.Invoke(() =>
            {
                LastMessage = $"{name}";
                message(LastMessage);
            });
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            AdminDBCall db = new AdminDBCall();
            Usermanage data = new Usermanage();
            data.Name = Name.Text;
            data.Email = Email.Text;
            var passwrd = password.Password;
            data.Password = Encrypt(passwrd, true);
            data.Profile = Profile.SelectedValue?.ToString();
            data.Locked = Locked.IsChecked;
            data.Role = Role.Text;
            var confirmpswrd = Encrypt(confirmPassword.Password, true);


            if (data.Name != "" || data.Email != "" || data.Password != "" || data.Profile != "" || data.Role != "")
            {
                if (data.Password == confirmpswrd)
                {
                    db.InsertUserManageData(data);
                    WriteLog.WriteToFile("SMSSave_Click:- InsertSMSData - Inserted Successfully ");
                    // RFIDno.ItemsSource = items;
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "User Manage Inserted Successsfully!!");
                    MaterialGrid5.ItemsSource = LoadCollectionData1();
                    ClearField();
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Confirm Password Not Matched!!");
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Enter Users Fields!!");
            }

        }
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                AdminDBCall db = new AdminDBCall();
                DataGrid dg = (DataGrid)sender;

                Usermanage selectedRow = dg.SelectedItem as Usermanage;
                if (selectedRow != null)
                {
                    DataTable dt1 = db.GetAllData("SELECT * FROM User_Management Where ID=" + selectedRow.ID);
                    if (dt1 != null && dt1.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt1.Rows)
                        {
                            ID.Text = (row["ID"].ToString());
                            Name.Text = (row["UserName"].ToString());
                            password.Password = Decrypt((row["Password"].ToString()), true);
                            confirmPassword.Password = Decrypt((row["Password"].ToString()), true);
                            Email.Text = (row["EmailID"].ToString());
                            Role.Text = (row["Role"].ToString());
                            Profile.SelectedValue = (row["HardwareProfile"].ToString());
                            var a = (row["IsLocked"].ToString());
                            if (a == "True")
                                Locked.IsChecked = true;
                            else
                                Locked.IsChecked = false;
                        }

                    }
                    //    ID.Text = selectedRow.ID.ToString();
                    //Name.Text = selectedRow.Name;
                    //Email.Text = selectedRow.Email;
                    //password.Password = selectedRow.Password;
                    //confirmPassword.Password = selectedRow.Password;
                    //Profile.Text = selectedRow.Profile;
                    //Locked.IsChecked = selectedRow.Locked;
                    //Role.Text = selectedRow.Role;
                    ID.IsEnabled = false;
                    Name.IsEnabled = false;
                    Email.IsEnabled = false;
                    password.IsEnabled = false;
                    confirmPassword.IsEnabled = false;
                    Profile.IsEnabled = false;
                    Locked.IsEnabled = false;
                    Role.IsEnabled = false;
                    UpdateUser.IsEnabled = true;
                    DeleteUser.IsEnabled = true;
                    SaveButton.IsEnabled = false;
                }


            }
            catch (Exception)
            {
            }
        }

        private void UpdateUser_Click(object sender, RoutedEventArgs e)
        {
            AdminDBCall db = new AdminDBCall();
            Usermanage data = new Usermanage();
            data.ID = Convert.ToInt32(ID.Text);
            data.Name = Name.Text;
            data.Email = Email.Text;
            data.Password = Encrypt(password.Password, true);
            data.Profile = Profile.Text;
            data.Locked = Locked.IsChecked;
            data.Role = Role.Text;
            var confirmpasswrd = Encrypt(confirmPassword.Password, true);

            if (data.Name != "" || data.Email != "" || data.Password != "" || data.Profile != "" || data.Role != "")
            {
                if (data.Password == confirmpasswrd)
                {
                    db.UpdateUsermanageData(data);
                    WriteLog.WriteToFile("UpdateUser_Click:- UpdateUsermanageData - Updated Successfully ");
                    // RFIDno.ItemsSource = items;
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "User Manage Updated Successsfully!!");
                    MaterialGrid5.ItemsSource = LoadCollectionData1();
                    ClearField();
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Confirm Password Not Matched!!");
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Enter Users Fields!!");
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            // ID.IsEnabled = true;
            Name.IsEnabled = true;
            Email.IsEnabled = true;
            password.IsEnabled = true;
            confirmPassword.IsEnabled = true;
            Profile.IsEnabled = true;
            Locked.IsEnabled = true;
            Role.IsEnabled = true;
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            AdminDBCall db = new AdminDBCall();
            Usermanage data = new Usermanage();
            data.ID = Convert.ToInt32(ID.Text);
            data.Name = Name.Text;
            data.Email = Email.Text;
            data.Password = Encrypt(password.Password, true);
            data.Profile = Profile.Text;
            data.Locked = Locked.IsChecked;
            data.Role = Role.Text;
            var confirmpasswrd = Encrypt(confirmPassword.Password, true);

            if (data.Name != "" || data.Email != "" || data.Password != "" || data.Profile != "" || data.Role != "")
            {
                if (data.Password == confirmpasswrd)
                {
                    var res = await OpenConfirmationDialog();
                    if (res)
                    {
                        db.DeleteUserManageData(data);
                        WriteLog.WriteToFile("DeleteUser_Click:- DeleteUserManageData - Deleted Successfully ");
                        // RFIDno.ItemsSource = items;
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "User Manage Deleted Successsfully!!");
                        MaterialGrid5.ItemsSource = LoadCollectionData1();
                        ClearField();
                    }
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Confirm Password Not Matched!!");
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Enter Users Fields!!");
            }
        }

        public async Task<bool> OpenConfirmationDialog()
        {
            var view = new ConfirmationDialog("Delete the user");

            //    //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            return (bool)result;
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {

        }
        private void ClearField()
        {
            ID.Text = "";
            Name.Text = "";
            Email.Text = "";
            password.Password = "";
            confirmPassword.Password = "";
            Profile.Text = "";
            Locked.IsChecked = false;
            Role.Text = "";
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
