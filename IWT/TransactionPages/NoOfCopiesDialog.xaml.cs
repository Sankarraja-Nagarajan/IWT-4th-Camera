using IWT.Shared;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace IWT.TransactionPages
{
    /// <summary>
    /// Interaction logic for NoOfCopiesDialog.xaml
    /// </summary>
    public partial class NoOfCopiesDialog : UserControl
    {
        public NoOfCopiesDialog(int copies)
        {
            InitializeComponent();
            Copies.Text = copies.ToString();
        }
        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {
            string result = Copies.Text;
            if (result != "")
            {
                DialogHost.CloseDialogCommand.Execute(result, null);
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please enter number of copies!!");
            }
        }
        private void Constant_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
