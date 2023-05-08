using IWT.Models;
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

namespace IWT.Admin_Pages
{
    /// <summary>
    /// Interaction logic for ControlStatusDialog.xaml
    /// </summary>
    public partial class ControlStatusDialog : UserControl
    {
        private ControlStatus _controlStatus = new ControlStatus();
        public ControlStatusDialog(string header,ControlStatus controlStatus)
        {
            InitializeComponent();
            DialogHeader.Text = header;
            SetControlStatus(controlStatus);
        }
        private void GetControlStatus()
        {
            if (string.IsNullOrEmpty(Single.Text) || Single.Text=="None")
            {
                _controlStatus.SGT = false;
            }
            if(string.IsNullOrEmpty(First.Text) || First.Text == "None")
            {
                _controlStatus.FTE = false;
                _controlStatus.FTL = false;
            }
            if(First.Text == "Empty")
            {
                _controlStatus.FTL = false;
            }
            if (First.Text == "Loaded")
            {
                _controlStatus.FTE = false;
            }
            if (string.IsNullOrEmpty(Second.Text) || Second.Text == "None")
            {
                _controlStatus.STE = false;
                _controlStatus.STL = false;
            }
            if (Second.Text == "Empty")
            {
                _controlStatus.STL = false;
            }
            if (Second.Text == "Loaded")
            {
                _controlStatus.STE = false;
            }
        }
        private void SetControlStatus(ControlStatus controlStatus)
        {
            if (controlStatus.SGT)
            {
                Single.Text = "Both";
            }
            else
            {
                Single.Text = "None";
            }
            if(controlStatus.FTE && controlStatus.FTL)
            {
                First.Text = "Both";
            }
            else if (controlStatus.FTE)
            {
                First.Text = "Empty";
            }
            else if (controlStatus.FTL)
            {
                First.Text = "Loaded";
            }
            else
            {
                First.Text = "None";
            }
            if (controlStatus.STE && controlStatus.STL)
            {
                Second.Text = "Both";
            }
            else if (controlStatus.STE)
            {
                Second.Text = "Empty";
            }
            else if (controlStatus.STL)
            {
                Second.Text = "Loaded";
            }
            else
            {
                Second.Text = "None";
            }
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            GetControlStatus();
            DialogHost.Close("ViewFieldDialogHost", _controlStatus);
        }
    }
}
