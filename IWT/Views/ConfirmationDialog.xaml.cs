using MaterialDesignThemes.Wpf;
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

namespace IWT.Views
{
    /// <summary>
    /// Interaction logic for ConfirmationDialog.xaml
    /// </summary>
    public partial class ConfirmationDialog : UserControl
    {
        private string _message;
        private bool _remarksEnable;
        public ConfirmationDialog(string action, bool remarksEnable = false, string customMessage = null)
        {
            InitializeComponent();
            if (customMessage == null)
                _message = "Are you sure want to " + action + " ?";
            else
                _message = customMessage;
            Message.Text = _message;
            _remarksEnable = remarksEnable;
            if (remarksEnable == true)
            {
                RemarksField.Visibility = Visibility.Visible;
            }
            else if (remarksEnable == false)
            {
                RemarksField.Visibility = Visibility.Collapsed;
            }
        }

        private void NoBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }

        private void YesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!_remarksEnable)
                DialogHost.CloseDialogCommand.Execute(true, null);
            else
            {
                DialogHost.CloseDialogCommand.Execute(Remarks.Text, null);
            }
        }
    }
}
