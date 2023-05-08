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
using System.Windows.Shapes;


namespace IWT.Admin_Pages
{
    /// <summary>
    /// Interaction logic for viewer.xaml
    /// </summary>
    public partial class viewer : Window
    {
        System.Windows.Controls.WebBrowser browser = new System.Windows.Controls.WebBrowser();
        public viewer()
        {
            InitializeComponent();
            try
            {
                //browser.Navigate("C:\\...\\sample.pdf");
                    browserHost.Children.Add(browser);

                //browser.Visible = true;
                browser.Navigate("C:\\Users\\User\\Downloads\\98f4728_2246smart.pdf");
                browserHost.Opacity = 200;
            }
            catch (Exception e)
            {
                Console.WriteLine("browser is visible/ not: " + browserHost.Visibility);
            }
        }
        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            /*Create the interop host control */
            //System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowFormsHost();

            /*Create the MaskedTextBox control */
            //browser.Navigate("C:\\...\\sample.pdf");
            //host.Child = browser;
            browserHost.Children.Add(browser);
        }
    }
}
