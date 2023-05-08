using IWT.Shared;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace IWT.TransactionPages
{
    /// <summary>
    /// Interaction logic for TemplateSelectionDialog.xaml
    /// </summary>
    public partial class TemplateSelectionDialog : UserControl
    {
        private FileInfo[] templateFiles;
        public TemplateSelectionDialog(string templateFolder)
        {
            InitializeComponent();
            if (Directory.Exists(templateFolder))
            {
                templateFiles = GetTemplatesFromLocation(templateFolder);
            }
            else
            {
                templateFiles = GetTemplatesFromLocation(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TransactionPages"));
            }
            TemplateComboBox.ItemsSource = templateFiles;
            TemplateComboBox.Items.Refresh();
        }
        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {
            var selected = TemplateComboBox.SelectedItem;
            if (selected != null)
            {
                DialogHost.CloseDialogCommand.Execute(selected, null);
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a template!!");
            }
        }
        private FileInfo[] GetTemplatesFromLocation(string templateFolder)
        {
            if (Directory.Exists(templateFolder))
            {
                DirectoryInfo Folder = new DirectoryInfo(templateFolder);
                FileInfo[] Files = Folder.GetFiles("*.rdl*");
                return Files;
            }
            else
            {
                return new FileInfo[0];
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
