using IWT.Admin_Pages;
using IWT.DBCall;
using IWT.Models;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace IWT.Views
{
    /// <summary>
    /// Interaction logic for Admin.xaml
    /// </summary>
    public partial class Admin : UserControl
    {
        string Rolename;
        RolePriviliege rolePriviliege;
        Button previousBtn;

        public static event EventHandler<UserControlEventArgs> onAdminWindowLoaded = delegate { };
        public Admin(string _Rolename, RolePriviliege _rolePriviliege)
        {
            InitializeComponent();
            Rolename = _Rolename;
            rolePriviliege = _rolePriviliege;
            previousBtn = new Button();
            Loaded += Admin_Loaded;
            GetInitialData();
        }

        private void Admin_Loaded(object sender, RoutedEventArgs e)
        {
            onAdminWindowLoaded.Invoke(this, new UserControlEventArgs());
        }

        public void GetInitialData()
        {
            if (rolePriviliege.UserManagementAccess.HasValue && rolePriviliege.UserManagementAccess.Value)
            {
                Role.Visibility = Visibility.Visible;
            }
            else
            {
                Role.Visibility = Visibility.Collapsed;
            }
            if (rolePriviliege.TicketDataEntryAccess.HasValue && rolePriviliege.TicketDataEntryAccess.Value)
            {
                MAIL.Visibility = Visibility.Visible;
            }
            else
            {
                MAIL.Visibility = Visibility.Collapsed;
            }
            if (rolePriviliege.UserManagementAccess.HasValue && rolePriviliege.UserManagementAccess.Value)
            {
                Main.Content = new ManageUser();
                var bc = new BrushConverter();
                NAme.Background = (Brush)bc.ConvertFrom("#FFAA33");
                NAme.Foreground = new SolidColorBrush(Colors.White);
                previousBtn = NAme;
            }
            else
            {
                NAme.Visibility = Visibility.Collapsed;
            }
            if (rolePriviliege.DuplicateTicketsAccess.HasValue && rolePriviliege.DuplicateTicketsAccess.Value)
            {
                Report.Visibility = Visibility.Visible;
            }
            else
            {
                Report.Visibility = Visibility.Collapsed;
            }
            if (rolePriviliege.EditHardwareProfile.HasValue && rolePriviliege.EditHardwareProfile.Value)
            {
                HardwareProfile.Visibility = Visibility.Visible;
            }
            else
            {
                HardwareProfile.Visibility = Visibility.Collapsed;
            }
            if (rolePriviliege.SAPSyncAccess.HasValue && rolePriviliege.SAPSyncAccess.Value)
            {
                SAPSync.Visibility = Visibility.Visible;
            }
            else
            {
                SAPSync.Visibility = Visibility.Collapsed;
            }
            if (rolePriviliege.SystemConfigurationAccess.HasValue && rolePriviliege.SystemConfigurationAccess.Value)
            {
                SystemConfig.Visibility = Visibility.Visible;
            }
            else
            {
                SystemConfig.Visibility = Visibility.Collapsed;
            }
        }
        private void Weighing_Click(object sender, RoutedEventArgs e)
        {
            var senderBtn = sender as Button;
            var bc = new BrushConverter();
            senderBtn.Background = (Brush)bc.ConvertFrom("#FFAA33");
            senderBtn.Foreground = new SolidColorBrush(Colors.White);
            previousBtn.Background = (Brush)bc.ConvertFrom("#BBC1D1");
            previousBtn.Foreground = new SolidColorBrush(Colors.Black);
            previousBtn = senderBtn;
            if (senderBtn.Uid == "1")
            {
                Main.Content = new ManageUser();
            }
            else if (senderBtn.Uid == "2")
            {
                Main.Content = new Role();
            }
            else if (senderBtn.Uid == "3")
            {
                Main.Content = new HardwareProfile();
            }
            else if (senderBtn.Uid == "4")
            {
                Main.Content = new TicketDataEntry();
            }
            else if(senderBtn.Uid == "5")
            {
                Main.Content = new PrintDeleteTicket(rolePriviliege);
            }
            else if (senderBtn.Uid == "7")
            {
                Main.Content = new SAPSync();
            }
            else if (senderBtn.Uid == "8")
            {
                Main.Content = new SystemConfiguration();
            }
            else
            {
                Main.Content = new ReTriggerMailSMS();
            }
        }


        private List<MaterialData2> LoadCollectionData()
        {
            List<MaterialData2> authors = new List<MaterialData2>();
            authors.Add(new MaterialData2()
            {
                Id = 1,
                Role = "Admin",
                MasterAccess = "Yes",
                SettingAccess = "Yes",
                TransactionAccess = "Yes",
                Adminaccess = "Yes",
                ReportAccess = "Yes",
                TicketDataEntryAccess = "Yes"
            });
            return authors;
        }

        private List<MaterialData1> LoadCollectionData1()
        {
            List<MaterialData1> authors1 = new List<MaterialData1>();
            authors1.Add(new MaterialData1()
            {
                Action = 1,
                TicketNumber = 334,
                VehicleNumber = "ABC-123",
                Date = "2022-03-11",
                Time = "16:03:14",
                EmptyWeight = 629,
            });
            return authors1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void HardwareProfile_Click(object sender, RoutedEventArgs e)
        {
            //var senderBtn = sender as Button;
            //if (senderBtn.Uid == "1")
            //{


            //}
            Main.Content = new HardwareProfile();
            var bc = new BrushConverter();
            NAme.Background = (Brush)bc.ConvertFrom("#BBC1D1");
            NAme.Foreground = new SolidColorBrush(Colors.Black);
            MAIL.Background = (Brush)bc.ConvertFrom("#BBC1D1");
            MAIL.Foreground = new SolidColorBrush(Colors.Black);
            Report.Background = (Brush)bc.ConvertFrom("#BBC1D1");
            Report.Foreground = new SolidColorBrush(Colors.Black);
            Role.Background = (Brush)bc.ConvertFrom("#BBC1D1");
            Role.Foreground = new SolidColorBrush(Colors.Black);
            SAPSync.Background = (Brush)bc.ConvertFrom("#BBC1D1");
            SAPSync.Foreground = new SolidColorBrush(Colors.Black);
            SystemConfig.Background = (Brush)bc.ConvertFrom("#BBC1D1");
            SystemConfig.Foreground = new SolidColorBrush(Colors.Black);
            HardwareProfile.Background = (Brush)bc.ConvertFrom("#FFAA33");
            HardwareProfile.Foreground = new SolidColorBrush(Colors.White);
        }
    }
    public class MaterialData2
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string MasterAccess { get; set; }
        public string SettingAccess { get; set; }
        public string TransactionAccess { get; set; }
        public string Adminaccess { get; set; }
        public string ReportAccess { get; set; }
        public string TicketDataEntryAccess { get; set; }
    }

    public class MaterialData1
    {
        public int Action { get; set; }
        public int TicketNumber { get; set; }
        public string VehicleNumber { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public int EmptyWeight { get; set; }
    }
}
