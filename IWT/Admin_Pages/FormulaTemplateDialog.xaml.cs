using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
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

namespace IWT.Admin_Pages
{
    /// <summary>
    /// Interaction logic for FormulaTemplateDialog.xaml
    /// </summary>
    public partial class FormulaTemplateDialog : UserControl
    {
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        private AdminDBCall _dbContext;
        public FormulaTemplateDialog()
        {
            InitializeComponent();
            _dbContext = new AdminDBCall();
            toastViewModel = new ToastViewModel();
            getAllFormulaTemplates();
        }
        public void getAllFormulaTemplates()
        {
            DataTable data = _dbContext.GetAllData("SELECT * FROM Formula_Table");
            string JSONString = JsonConvert.SerializeObject(data);
            var result = JsonConvert.DeserializeObject<List<FormulaTemplate>>(JSONString);
            FormulaTemplateCB.ItemsSource = result;
        }

        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {
            var selected=FormulaTemplateCB.SelectedItem;
            if (selected != null)
            {
                DialogHost.CloseDialogCommand.Execute(selected, null);
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a template!!");
            }
        }
        public void ShowMessage(Action<string> message, string name)
        {
            this.Dispatcher.Invoke(() =>
            {
                LastMessage = $"{name}";
                message(LastMessage);
            });
        }
    }
}
