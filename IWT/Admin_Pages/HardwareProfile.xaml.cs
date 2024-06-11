using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.Views;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

namespace IWT.Admin_Pages
{
    /// <summary>
    /// Interaction logic for HardwareProfile.xaml
    /// </summary>
    public partial class HardwareProfile : Page
    {
        List<UserHardwareProfile> hardwareProfiles = new List<UserHardwareProfile>();
        UserHardwareProfile selectedHardwareProfile = new UserHardwareProfile();
        AdminDBCall adminDBCall = new AdminDBCall();
        MasterDBCall masterDBCall = new MasterDBCall();
        public HardwareProfile()
        {
            InitializeComponent();
            Loaded += HardwareProfile_Loaded;
        }

        private void HardwareProfile_Loaded(object sender, RoutedEventArgs e)
        {
            HardwareProfileGrid.SelectionChanged += HardwareProfileGrid_SelectionChanged;
            GetHardwareProfiles();
        }

        private void HardwareProfileGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedHardwareProfile = HardwareProfileGrid.SelectedItem as UserHardwareProfile;
            if (selectedHardwareProfile != null)
            {
                HardwareProfileName.Text = selectedHardwareProfile.HardwareProfileName;
                CameraAccess.IsChecked = selectedHardwareProfile.CameraAccess;
                RFIDReader1.IsChecked = selectedHardwareProfile.RFIDReader1;
                RFIDReader2.IsChecked = selectedHardwareProfile.RFIDReader2;
                RFIDReader3.IsChecked = selectedHardwareProfile.RFIDReader3;
                PLC.IsChecked = selectedHardwareProfile.PLC;
            }
        }

        public void GetHardwareProfiles()
        {
            try
            {
                DataTable dt1 = adminDBCall.GetAllData("SELECT * FROM User_HardwareProfiles");
                string JSONString = JsonConvert.SerializeObject(dt1);
                hardwareProfiles = JsonConvert.DeserializeObject<List<UserHardwareProfile>>(JSONString);
                HardwareProfileGrid.ItemsSource = hardwareProfiles;
                HardwareProfileGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Role/GetHardwareProfiles", ex);
            }

        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            ClearField();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(HardwareProfileName.Text))
            {
                GetFieldsValue();
                if (selectedHardwareProfile != null && selectedHardwareProfile.ID != 0)
                {
                    adminDBCall.UpdateSystemConfig(selectedHardwareProfile.HardwareProfileName, selectedHardwareProfile.ID);
                    UpdateHardwareProfile();
                }
                else
                {
                    adminDBCall.InsertSystemConfig(selectedHardwareProfile.HardwareProfileName);
                    InsertHardwareProfile();
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please enter the role name");
            }

        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearField();

        }

        public void InsertHardwareProfile()
        {
            try
            {
                string Query = "INSERT INTO [User_HardwareProfiles] (HardwareProfileName,CameraAccess,RFIDReader1,RFIDReader2,RFIDReader3,PLC)" +
                                                       "VALUES (@HardwareProfileName,@CameraAccess,@RFIDReader1,@RFIDReader2,@RFIDReader3,@PLC)";

                SqlCommand cmd = new SqlCommand(Query);
                cmd = AddParameters(cmd);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Hardware profile created successfully");
                ClearField();
                GetHardwareProfiles();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("HardwareProfile/InsertHardwareProfile", ex);

            }
        }

        public SqlCommand AddParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@HardwareProfileName", selectedHardwareProfile.HardwareProfileName);
            cmd.Parameters.AddWithValue("@CameraAccess", selectedHardwareProfile.CameraAccess);
            cmd.Parameters.AddWithValue("@RFIDReader1", selectedHardwareProfile.RFIDReader1);
            cmd.Parameters.AddWithValue("@RFIDReader2", selectedHardwareProfile.RFIDReader2);
            cmd.Parameters.AddWithValue("@RFIDReader3", selectedHardwareProfile.RFIDReader3);
            cmd.Parameters.AddWithValue("@PLC", selectedHardwareProfile.PLC);
            return cmd;
        }

        public void UpdateHardwareProfile()
        {
            try
            {
                string Query = "UPDATE [User_HardwareProfiles] SET HardwareProfileName=@HardwareProfileName,CameraAccess=@CameraAccess,RFIDReader1=@RFIDReader1,RFIDReader2=@RFIDReader2,RFIDReader3=@RFIDReader3,PLC=@PLC WHERE ID=@ID";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@ID", selectedHardwareProfile.ID);
                cmd = AddParameters(cmd);
                masterDBCall.InsertData(cmd, CommandType.Text);
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Hardware profile updated successfully");
                ClearField();
                GetHardwareProfiles();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("HardwareProfile/UpdateHardwareProfile", ex);
            }
        }


        public void GetFieldsValue()
        {
            if (selectedHardwareProfile == null)
            {
                selectedHardwareProfile = new UserHardwareProfile();
            }

            selectedHardwareProfile.HardwareProfileName = HardwareProfileName.Text;
            selectedHardwareProfile.CameraAccess = (bool)CameraAccess.IsChecked;
            selectedHardwareProfile.RFIDReader1 = (bool)RFIDReader1.IsChecked;
            selectedHardwareProfile.RFIDReader2 = (bool)RFIDReader2.IsChecked;
            selectedHardwareProfile.RFIDReader3 = (bool)RFIDReader3.IsChecked;
            selectedHardwareProfile.PLC = (bool)PLC.IsChecked;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedHardwareProfile != null && selectedHardwareProfile.ID != 0)
                {
                    var result = await OpenConfirmationDialog();
                    if (result)
                    {
                        string Query = "DELETE FROM User_HardwareProfiles WHERE ID=@ID";
                        SqlCommand cmd = new SqlCommand(Query);
                        cmd.Parameters.AddWithValue("@ID", selectedHardwareProfile.ID);
                        //cmd.CommandType = CommandType.Text;
                        masterDBCall.InsertData(cmd, CommandType.Text);
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Hardware profile deleted successfully");
                        ClearField();
                        GetHardwareProfiles();
                    }
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a Hardware profile to delete");
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("HardwareProfile/DeleteButton_Click", ex);
            }
        }

        public async Task<bool> OpenConfirmationDialog()
        {
            var view = new ConfirmationDialog($"Delete the hardware profile");

            //    //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            return (bool)result;
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
        }

        private void ClearField()
        {
            HardwareProfileGrid.SelectedItem = null;
            HardwareProfileName.Text = "";
            CameraAccess.IsChecked = false;
            RFIDReader1.IsChecked = false;
            RFIDReader2.IsChecked = false;
            RFIDReader3.IsChecked = false;
            PLC.IsChecked = false;
        }
    }
}
