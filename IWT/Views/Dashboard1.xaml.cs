using IWT.DashboardPages;
using IWT.Models;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for Dashboard1.xaml
    /// </summary>
    public partial class Dashboard1 : UserControl
    {
        public Dashboard1()
        {
            InitializeComponent();
        }

        private async void ChangePasswordPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
         {
            var view = new ChangePasswordDialog();

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);

        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            var result = eventArgs?.Parameter as ChangePasswordResult;
            //Console.WriteLine(result);
            ////Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }

        private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var view = new ChangePasswordDialog();

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
        }
    }
}
