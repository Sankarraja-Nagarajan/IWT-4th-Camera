using System;
using System.Collections.Generic;
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
    /// Interaction logic for Setting_Screen.xaml
    /// </summary>
    public partial class Setting_Screen : Page
    {
        public Setting_Screen()
        {
            InitializeComponent();
        }
        private void ButtonFechar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        

        private void Weighing_Click(object sender, RoutedEventArgs e)
        {
            var result = false;
            var senderBtn = sender as Button;
            if (senderBtn.Uid == "1")
            {
                Main.Content = new Weighing();
                //result = true;
                //NAme.Background = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                //NAme.BorderBrush = Brushes.White;
                //NAme.BorderThickness = new Thickness(3);
                //if(senderBtn.Uid != "1")
                //{

                //        senderBtn.Background = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                //        senderBtn.BorderBrush = Brushes.Black;
                //        senderBtn.BorderThickness = new Thickness(3);

                //}
            }

            if (senderBtn.Uid == "2")
            {
                Main.Content = new Email();
                //result = true;
                //MAIL.Background = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                //senderBtn.BorderBrush = Brushes.White;
                //senderBtn.BorderThickness = new Thickness(3);
                //if (!(senderBtn.Uid != "2"))
                //{

                //    NAme.Background = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                //    NAme.BorderBrush = Brushes.Black;
                //    senderBtn.BorderThickness = new Thickness(3);

                //}
            }

            else if (senderBtn.Uid == "3")
                Main.Content = new Summary_Report();
            else if (senderBtn.Uid == "4")
                Main.Content = new Camera_setting();
            else if (senderBtn.Uid == "5")
                Main.Content = new File_location();
            else if (senderBtn.Uid == "6")
                Main.Content = new Software();
            else if (senderBtn.Uid == "7")
                Main.Content = new Hardware_profile();
            else if (senderBtn.Uid == "8")
                Main.Content = new SMTP_setting();
            else if (senderBtn.Uid == "9")
                Main.Content = new Other_setting();
            else if (senderBtn.Uid == "10")
                Main.Content = new Export_Import();
            else if (senderBtn.Uid == "11")
                Main.Content = new DB_Page();
            else if (senderBtn.Uid == "12")
                Main.Content = new SMS_setting();
            if (result)
            {
            }
        }

        
    }
}
