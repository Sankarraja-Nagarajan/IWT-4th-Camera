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
    /// Interaction logic for TableFieldsDialog.xaml
    /// </summary>
    public partial class TableFieldsDialog : UserControl
    {
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        private FieldDependency fieldDependency=new FieldDependency();
        public TableFieldsDialog(FieldDependency fd, List<string> trTable)
        {
            InitializeComponent();
            toastViewModel = new ToastViewModel();
            TableColumns.ItemsSource=trTable;
            //TableColumns.ItemsSource = GetTableColumns("Transaction");
            fieldDependency = fd;
        }
        public List<TabelColumns> GetTableColumns(string tableName)
        {
            try
            {
                AdminDBCall _dbContext = new AdminDBCall();
                DataTable table = _dbContext.GetAllData($"select column_name from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='{tableName}'");
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<TabelColumns>>(JSONString);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetTicktetDataTemplateData:" + ex.Message);
                return new List<TabelColumns>();
            }
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            var selectedColumn = TableColumns.SelectedItem as string;
            if (selectedColumn != null)
            {
                fieldDependency.LinkedName = selectedColumn;
                DialogHost.CloseDialogCommand.Execute(fieldDependency, null);
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a material !!");
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
