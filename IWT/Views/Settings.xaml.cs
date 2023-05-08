using IWT.DBCall;
using IWT.Models;
using IWT.Setting_Pages;
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
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl, IInitialization
    {
        public string role;
        RolePriviliege rolePriviliege;

        public static event EventHandler<UserControlEventArgs> onSettingWindowLoaded = delegate { };
        public Settings(string Role, RolePriviliege _rolePriviliege)
        {
            InitializeComponent();
            role = Role;
            this.rolePriviliege = _rolePriviliege;
            Loaded += Settings_Loaded;
            GetInitialData();
        }

        private void Settings_Loaded(object sender, RoutedEventArgs e)
        {
            onSettingWindowLoaded.Invoke(this, new UserControlEventArgs());
        }

        public void GetInitialData()
        {
            //BuildFields();
            var bc = new BrushConverter();
            GetSettingsOptions(role);
            Main.Content = new Weighing();
            NAme.Visibility = Visibility.Visible;
            NAme.Background = (Brush)bc.ConvertFrom("#FFAA33");
            NAme.Foreground = new SolidColorBrush(Colors.White);
        }

        public void GetSettingsOptions(string Role)
        {
            if (rolePriviliege.WeighBridgeSetting.HasValue && rolePriviliege.WeighBridgeSetting.Value)
            {
                NAme.Visibility = Visibility.Visible;
                Main.Content = new Weighing(); var bc = new BrushConverter();
                NAme.Background = (Brush)bc.ConvertFrom("#FFAA33");
                NAme.Foreground = new SolidColorBrush(Colors.White);
                MAIL.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                MAIL.Foreground = new SolidColorBrush(Colors.Black);
                Report.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Report.Foreground = new SolidColorBrush(Colors.Black);
                Camera.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Camera.Foreground = new SolidColorBrush(Colors.Black);
                File.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                File.Foreground = new SolidColorBrush(Colors.Black);
                software.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                software.Foreground = new SolidColorBrush(Colors.Black);
                hardware.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                hardware.Foreground = new SolidColorBrush(Colors.Black);
                smtp.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                smtp.Foreground = new SolidColorBrush(Colors.Black);
                other.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                other.Foreground = new SolidColorBrush(Colors.Black);
                export.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                export.Foreground = new SolidColorBrush(Colors.Black);
                DB.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                DB.Foreground = new SolidColorBrush(Colors.Black);
                sms.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                sms.Foreground = new SolidColorBrush(Colors.Black);
                aws.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                aws.Foreground = new SolidColorBrush(Colors.Black);
            }

            else
                NAme.Visibility = Visibility.Collapsed;
            if (rolePriviliege.EmailSettingsAccess.HasValue && rolePriviliege.EmailSettingsAccess.Value)
                MAIL.Visibility = Visibility.Visible;
            else
                MAIL.Visibility = Visibility.Collapsed;
            if (rolePriviliege.SummaryReportAccess.HasValue && rolePriviliege.SummaryReportAccess.Value)
                Report.Visibility = Visibility.Visible;
            else
                Report.Visibility = Visibility.Collapsed;
            if (rolePriviliege.CCTVSettings.HasValue && rolePriviliege.CCTVSettings.Value)
                Camera.Visibility = Visibility.Visible;
            else
                Camera.Visibility = Visibility.Collapsed;
            if (rolePriviliege.FileLocationSettingsAccess.HasValue && rolePriviliege.FileLocationSettingsAccess.Value)
                File.Visibility = Visibility.Visible;
            else
                File.Visibility = Visibility.Collapsed;
            if (rolePriviliege.SoftwareConfigurationAccess.HasValue && rolePriviliege.SoftwareConfigurationAccess.Value)
                software.Visibility = Visibility.Visible;
            else
                software.Visibility = Visibility.Collapsed;
            if (rolePriviliege.EditHardwareProfile.HasValue && rolePriviliege.EditHardwareProfile.Value)
                hardware.Visibility = Visibility.Visible;
            else
                hardware.Visibility = Visibility.Collapsed;
            if (rolePriviliege.SMTPAccess.HasValue && rolePriviliege.SMTPAccess.Value)
                smtp.Visibility = Visibility.Visible;
            else
                smtp.Visibility = Visibility.Collapsed;
            if (rolePriviliege.OtherSettingsAccess.HasValue && rolePriviliege.OtherSettingsAccess.Value)
                other.Visibility = Visibility.Visible;
            else
                other.Visibility = Visibility.Collapsed;
            if (rolePriviliege.ImportExportAccess.HasValue && rolePriviliege.ImportExportAccess.Value)
                export.Visibility = Visibility.Visible;
            else
                export.Visibility = Visibility.Collapsed;
            if (rolePriviliege.DBPswdChangeAccess.HasValue && rolePriviliege.DBPswdChangeAccess.Value)
                DB.Visibility = Visibility.Visible;
            else
                DB.Visibility = Visibility.Collapsed;
            if (rolePriviliege.SMSAdminAccess.HasValue && rolePriviliege.SMSAdminAccess.Value)
                sms.Visibility = Visibility.Visible;
            else
                sms.Visibility = Visibility.Collapsed;
            if (rolePriviliege.AWSAccess.HasValue && rolePriviliege.AWSAccess.Value)
                aws.Visibility = Visibility.Visible;
            else
                aws.Visibility = Visibility.Collapsed;
        }

        private void Weighing_Click(object sender, RoutedEventArgs e)
        {
            var senderBtn = sender as Button;
            if (senderBtn.Uid == "1")
            {
                Main.Content = new Weighing();
                var bc = new BrushConverter();
                NAme.Background = (Brush)bc.ConvertFrom("#FFAA33");
                NAme.Foreground = new SolidColorBrush(Colors.White);
                MAIL.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                MAIL.Foreground = new SolidColorBrush(Colors.Black);
                Report.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Report.Foreground = new SolidColorBrush(Colors.Black);
                Camera.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Camera.Foreground = new SolidColorBrush(Colors.Black);
                File.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                File.Foreground = new SolidColorBrush(Colors.Black);
                software.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                software.Foreground = new SolidColorBrush(Colors.Black);
                hardware.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                hardware.Foreground = new SolidColorBrush(Colors.Black);
                smtp.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                smtp.Foreground = new SolidColorBrush(Colors.Black);
                other.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                other.Foreground = new SolidColorBrush(Colors.Black);
                export.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                export.Foreground = new SolidColorBrush(Colors.Black);
                DB.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                DB.Foreground = new SolidColorBrush(Colors.Black);
                sms.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                sms.Foreground = new SolidColorBrush(Colors.Black);
                aws.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                aws.Foreground = new SolidColorBrush(Colors.Black);
            }
            if (senderBtn.Uid == "2")
            {
                var bc = new BrushConverter();
                NAme.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                NAme.Foreground = new SolidColorBrush(Colors.Black);
                MAIL.Background = (Brush)bc.ConvertFrom("#FFAA33");
                MAIL.Foreground = new SolidColorBrush(Colors.White);
                Report.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Report.Foreground = new SolidColorBrush(Colors.Black);
                Camera.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Camera.Foreground = new SolidColorBrush(Colors.Black);
                File.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                File.Foreground = new SolidColorBrush(Colors.Black);
                software.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                software.Foreground = new SolidColorBrush(Colors.Black);
                hardware.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                hardware.Foreground = new SolidColorBrush(Colors.Black);
                smtp.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                smtp.Foreground = new SolidColorBrush(Colors.Black);
                other.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                other.Foreground = new SolidColorBrush(Colors.Black);
                export.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                export.Foreground = new SolidColorBrush(Colors.Black);
                DB.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                DB.Foreground = new SolidColorBrush(Colors.Black);
                sms.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                sms.Foreground = new SolidColorBrush(Colors.Black);
                aws.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                aws.Foreground = new SolidColorBrush(Colors.Black);
                Main.Content = new Email();

            }
            else if (senderBtn.Uid == "3")
            {
                var bc = new BrushConverter();
                Report.Background = (Brush)bc.ConvertFrom("#FFAA33");
                Report.Foreground = new SolidColorBrush(Colors.White);
                NAme.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                NAme.Foreground = new SolidColorBrush(Colors.Black);
                MAIL.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                MAIL.Foreground = new SolidColorBrush(Colors.Black);
                Camera.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Camera.Foreground = new SolidColorBrush(Colors.Black);
                File.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                File.Foreground = new SolidColorBrush(Colors.Black);
                software.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                software.Foreground = new SolidColorBrush(Colors.Black);
                hardware.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                hardware.Foreground = new SolidColorBrush(Colors.Black);
                smtp.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                smtp.Foreground = new SolidColorBrush(Colors.Black);
                other.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                other.Foreground = new SolidColorBrush(Colors.Black);
                export.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                export.Foreground = new SolidColorBrush(Colors.Black);
                DB.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                DB.Foreground = new SolidColorBrush(Colors.Black);
                sms.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                sms.Foreground = new SolidColorBrush(Colors.Black);
                aws.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                aws.Foreground = new SolidColorBrush(Colors.Black);
                Main.Content = new Summary_Report();

            }
            else if (senderBtn.Uid == "4")
            {
                var bc = new BrushConverter();
                Camera.Background = (Brush)bc.ConvertFrom("#FFAA33");
                Camera.Foreground = new SolidColorBrush(Colors.White);
                NAme.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                NAme.Foreground = new SolidColorBrush(Colors.Black);
                MAIL.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                MAIL.Foreground = new SolidColorBrush(Colors.Black);
                Report.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Report.Foreground = new SolidColorBrush(Colors.Black);

                File.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                File.Foreground = new SolidColorBrush(Colors.Black);
                software.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                software.Foreground = new SolidColorBrush(Colors.Black);
                hardware.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                hardware.Foreground = new SolidColorBrush(Colors.Black);
                smtp.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                smtp.Foreground = new SolidColorBrush(Colors.Black);
                other.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                other.Foreground = new SolidColorBrush(Colors.Black);
                export.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                export.Foreground = new SolidColorBrush(Colors.Black);
                DB.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                DB.Foreground = new SolidColorBrush(Colors.Black);
                sms.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                sms.Foreground = new SolidColorBrush(Colors.Black);
                aws.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                aws.Foreground = new SolidColorBrush(Colors.Black);
                Main.Content = new Camera_setting();
            }
            else if (senderBtn.Uid == "5")
            {
                var bc = new BrushConverter();
                NAme.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                NAme.Foreground = new SolidColorBrush(Colors.Black);
                MAIL.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                MAIL.Foreground = new SolidColorBrush(Colors.Black);
                Report.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Report.Foreground = new SolidColorBrush(Colors.Black);
                Camera.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Camera.Foreground = new SolidColorBrush(Colors.Black);
                File.Background = (Brush)bc.ConvertFrom("#FFAA33");
                File.Foreground = new SolidColorBrush(Colors.White);
                software.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                software.Foreground = new SolidColorBrush(Colors.Black);
                hardware.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                hardware.Foreground = new SolidColorBrush(Colors.Black);
                smtp.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                smtp.Foreground = new SolidColorBrush(Colors.Black);
                other.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                other.Foreground = new SolidColorBrush(Colors.Black);
                export.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                export.Foreground = new SolidColorBrush(Colors.Black);
                DB.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                DB.Foreground = new SolidColorBrush(Colors.Black);
                sms.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                sms.Foreground = new SolidColorBrush(Colors.Black);
                aws.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                aws.Foreground = new SolidColorBrush(Colors.Black);
                Main.Content = new File_location();
            }
            else if (senderBtn.Uid == "6")
            {
                var bc = new BrushConverter();
                NAme.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                NAme.Foreground = new SolidColorBrush(Colors.Black);
                MAIL.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                MAIL.Foreground = new SolidColorBrush(Colors.Black);
                Report.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Report.Foreground = new SolidColorBrush(Colors.Black);
                Camera.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Camera.Foreground = new SolidColorBrush(Colors.Black);
                File.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                File.Foreground = new SolidColorBrush(Colors.Black);
                software.Background = (Brush)bc.ConvertFrom("#FFAA33");
                software.Foreground = new SolidColorBrush(Colors.White);
                hardware.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                hardware.Foreground = new SolidColorBrush(Colors.Black);
                smtp.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                smtp.Foreground = new SolidColorBrush(Colors.Black);
                other.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                other.Foreground = new SolidColorBrush(Colors.Black);
                export.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                export.Foreground = new SolidColorBrush(Colors.Black);
                DB.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                DB.Foreground = new SolidColorBrush(Colors.Black);
                sms.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                sms.Foreground = new SolidColorBrush(Colors.Black);
                aws.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                aws.Foreground = new SolidColorBrush(Colors.Black);
                Main.Content = new Software();
            }

            else if (senderBtn.Uid == "7")
            {
                var bc = new BrushConverter();
                NAme.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                NAme.Foreground = new SolidColorBrush(Colors.Black);
                MAIL.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                MAIL.Foreground = new SolidColorBrush(Colors.Black);
                Report.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Report.Foreground = new SolidColorBrush(Colors.Black);
                Camera.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Camera.Foreground = new SolidColorBrush(Colors.Black);
                File.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                File.Foreground = new SolidColorBrush(Colors.Black);
                software.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                software.Foreground = new SolidColorBrush(Colors.Black);
                hardware.Background = (Brush)bc.ConvertFrom("#FFAA33");
                hardware.Foreground = new SolidColorBrush(Colors.White);
                smtp.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                smtp.Foreground = new SolidColorBrush(Colors.Black);
                other.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                other.Foreground = new SolidColorBrush(Colors.Black);
                export.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                export.Foreground = new SolidColorBrush(Colors.Black);
                DB.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                DB.Foreground = new SolidColorBrush(Colors.Black);
                sms.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                sms.Foreground = new SolidColorBrush(Colors.Black);
                aws.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                aws.Foreground = new SolidColorBrush(Colors.Black);
                Main.Content = new Hardware_profile();
            }

            else if (senderBtn.Uid == "8")
            {
                var bc = new BrushConverter();
                NAme.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                NAme.Foreground = new SolidColorBrush(Colors.Black);
                MAIL.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                MAIL.Foreground = new SolidColorBrush(Colors.Black);
                Report.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Report.Foreground = new SolidColorBrush(Colors.Black);
                Camera.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Camera.Foreground = new SolidColorBrush(Colors.Black);
                File.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                File.Foreground = new SolidColorBrush(Colors.Black);
                software.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                software.Foreground = new SolidColorBrush(Colors.Black);
                hardware.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                hardware.Foreground = new SolidColorBrush(Colors.Black);
                smtp.Background = (Brush)bc.ConvertFrom("#FFAA33");
                smtp.Foreground = new SolidColorBrush(Colors.White);
                other.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                other.Foreground = new SolidColorBrush(Colors.Black);
                export.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                export.Foreground = new SolidColorBrush(Colors.Black);
                DB.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                DB.Foreground = new SolidColorBrush(Colors.Black);
                sms.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                sms.Foreground = new SolidColorBrush(Colors.Black);
                aws.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                aws.Foreground = new SolidColorBrush(Colors.Black);
                Main.Content = new SMTP_setting();
            }

            else if (senderBtn.Uid == "12")
            {
                var bc = new BrushConverter();
                NAme.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                NAme.Foreground = new SolidColorBrush(Colors.Black);
                MAIL.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                MAIL.Foreground = new SolidColorBrush(Colors.Black);
                Report.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Report.Foreground = new SolidColorBrush(Colors.Black);
                Camera.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Camera.Foreground = new SolidColorBrush(Colors.Black);
                File.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                File.Foreground = new SolidColorBrush(Colors.Black);
                software.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                software.Foreground = new SolidColorBrush(Colors.Black);
                hardware.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                hardware.Foreground = new SolidColorBrush(Colors.Black);
                smtp.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                smtp.Foreground = new SolidColorBrush(Colors.Black);
                other.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                other.Foreground = new SolidColorBrush(Colors.Black);
                export.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                export.Foreground = new SolidColorBrush(Colors.Black);
                DB.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                DB.Foreground = new SolidColorBrush(Colors.Black);
                sms.Background = (Brush)bc.ConvertFrom("#FFAA33");
                sms.Foreground = new SolidColorBrush(Colors.White);
                aws.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                aws.Foreground = new SolidColorBrush(Colors.Black);
                Main.Content = new SMS_setting();
            }
            else if (senderBtn.Uid == "10")
            {
                var bc = new BrushConverter();
                NAme.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                NAme.Foreground = new SolidColorBrush(Colors.Black);
                MAIL.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                MAIL.Foreground = new SolidColorBrush(Colors.Black);
                Report.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Report.Foreground = new SolidColorBrush(Colors.Black);
                File.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                File.Foreground = new SolidColorBrush(Colors.Black);
                software.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                software.Foreground = new SolidColorBrush(Colors.Black);
                hardware.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                hardware.Foreground = new SolidColorBrush(Colors.Black);
                Camera.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Camera.Foreground = new SolidColorBrush(Colors.Black);
                smtp.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                smtp.Foreground = new SolidColorBrush(Colors.Black);
                other.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                other.Foreground = new SolidColorBrush(Colors.Black);
                export.Background = (Brush)bc.ConvertFrom("#FFAA33");
                export.Foreground = new SolidColorBrush(Colors.White);
                DB.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                DB.Foreground = new SolidColorBrush(Colors.Black);
                sms.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                sms.Foreground = new SolidColorBrush(Colors.Black);
                aws.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                aws.Foreground = new SolidColorBrush(Colors.Black);
                Main.Content = new Export_Import();
            }
            else if (senderBtn.Uid == "11")
            {
                var bc = new BrushConverter();
                NAme.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                NAme.Foreground = new SolidColorBrush(Colors.Black);
                MAIL.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                MAIL.Foreground = new SolidColorBrush(Colors.Black);
                Report.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Report.Foreground = new SolidColorBrush(Colors.Black);
                Camera.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Camera.Foreground = new SolidColorBrush(Colors.Black);
                File.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                File.Foreground = new SolidColorBrush(Colors.Black);
                software.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                software.Foreground = new SolidColorBrush(Colors.Black);
                hardware.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                hardware.Foreground = new SolidColorBrush(Colors.Black);
                smtp.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                smtp.Foreground = new SolidColorBrush(Colors.Black);
                other.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                other.Foreground = new SolidColorBrush(Colors.Black);
                export.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                export.Foreground = new SolidColorBrush(Colors.Black);
                DB.Background = (Brush)bc.ConvertFrom("#FFAA33");
                DB.Foreground = new SolidColorBrush(Colors.White);
                sms.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                sms.Foreground = new SolidColorBrush(Colors.Black);
                aws.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                aws.Foreground = new SolidColorBrush(Colors.Black);
                Main.Content = new DB_Page();
            }
            else if (senderBtn.Uid == "9")
            {
                var bc = new BrushConverter();
                NAme.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                NAme.Foreground = new SolidColorBrush(Colors.Black);
                MAIL.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                MAIL.Foreground = new SolidColorBrush(Colors.Black);
                Report.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Report.Foreground = new SolidColorBrush(Colors.Black);
                Camera.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Camera.Foreground = new SolidColorBrush(Colors.Black);
                File.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                File.Foreground = new SolidColorBrush(Colors.Black);
                software.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                software.Foreground = new SolidColorBrush(Colors.Black);
                hardware.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                hardware.Foreground = new SolidColorBrush(Colors.Black);
                smtp.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                smtp.Foreground = new SolidColorBrush(Colors.Black);
                other.Background = (Brush)bc.ConvertFrom("#FFAA33");
                other.Foreground = new SolidColorBrush(Colors.White);
                export.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                export.Foreground = new SolidColorBrush(Colors.Black);
                DB.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                DB.Foreground = new SolidColorBrush(Colors.Black);
                sms.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                sms.Foreground = new SolidColorBrush(Colors.Black);
                aws.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                aws.Foreground = new SolidColorBrush(Colors.Black);
                Main.Content = new Other_setting();
            }
            else if (senderBtn.Uid == "13")
            {
                var bc = new BrushConverter();
                NAme.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                NAme.Foreground = new SolidColorBrush(Colors.Black);
                MAIL.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                MAIL.Foreground = new SolidColorBrush(Colors.Black);
                Report.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Report.Foreground = new SolidColorBrush(Colors.Black);
                Camera.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                Camera.Foreground = new SolidColorBrush(Colors.Black);
                File.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                File.Foreground = new SolidColorBrush(Colors.Black);
                software.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                software.Foreground = new SolidColorBrush(Colors.Black);
                hardware.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                hardware.Foreground = new SolidColorBrush(Colors.Black);
                smtp.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                smtp.Foreground = new SolidColorBrush(Colors.Black);
                other.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                other.Foreground = new SolidColorBrush(Colors.White);
                export.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                export.Foreground = new SolidColorBrush(Colors.Black);
                DB.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                DB.Foreground = new SolidColorBrush(Colors.Black);
                sms.Background = (Brush)bc.ConvertFrom("#BBC1D1");
                sms.Foreground = new SolidColorBrush(Colors.Black);
                aws.Background = (Brush)bc.ConvertFrom("#FFAA33");
                aws.Foreground = new SolidColorBrush(Colors.Black);
                Main.Content = new AWS_setting();
            }
        }
    }
}
