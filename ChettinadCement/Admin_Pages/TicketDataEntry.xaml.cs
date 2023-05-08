using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using IWT.Views;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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

namespace IWT.Admin_Pages
{
    /// <summary>
    /// Interaction logic for TicketDataEntry.xaml
    /// </summary>
    public partial class TicketDataEntry : Page
    {
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        private AdminDBCall _dbContext;
        private TicketDataEntryViewModel _context;
        private ObservableCollection<TableList> _tableList= new ObservableCollection<TableList>(); 
        private ObservableCollection<TableList> _customTableList= new ObservableCollection<TableList>();
        public TicketDataEntry()
        {
            InitializeComponent();
            _dbContext = new AdminDBCall();
            toastViewModel = new ToastViewModel();
            DataContext = new TicketDataEntryViewModel();
            _context = DataContext as TicketDataEntryViewModel;
            populateLists();
        }
        public void populateLists()
        {
            getAllCustomMasters();
            _tableList.Clear();
            addTableList();
            _context.TableList = _tableList;
        }

        public void addTableList()
        {
            _tableList.Add(new TableList { TableName = "Transaction", Type = "0" });
            _tableList.Add(new TableList { TableName = "Material_Master", Type = "1" });
            _tableList.Add(new TableList { TableName = "Supplier_Master", Type = "1" });
            _tableList.Add(new TableList { TableName = "Shift_Master", Type = "1" });
            _tableList.Add(new TableList { TableName = "Vehicle_Master", Type = "1" });
            foreach (var item in _customTableList)
            {
                _tableList.Add(new TableList { TableName = item.TableName, Type = item.Type });
            }
            _context.AllTableList = new ObservableCollection<TableList>(_tableList);
        }

        public void createCustomTable(string tableName)
        {
            try
            {
                if (_dbContext.CreateCustomTable(tableName))
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Table Created Successsfully !!");
                    TableName.Text = "";
                    populateLists();
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Failed to create table");
                }
            }
            catch (Exception ex)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, ex.Message);
            }
        }
        public void deleteCustomTable(string tableName)
        {
            try
            {
                if (_dbContext.DeleteCustomTable(tableName))
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Table Deleted Successsfully !!");
                    populateLists();
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Failed to delete table");
                }
            }
            catch (Exception ex)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, ex.Message);
            }
        }
        public void getAllCustomMasters()
        {
            try
            {
                _customTableList.Clear();
                DataTable data = _dbContext.GetAllData("SELECT * FROM Custom_Master_List");
                if (data != null && data.Rows.Count > 0)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        _customTableList.Add(new TableList { TableName = row["CutomMasterName"].ToString(), Type = "1" });
                    }
                    delTableName.ItemsSource = _customTableList;
                }
            }
            catch (Exception ex)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, ex.Message);
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

        private void CreateTable_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TableName.Text))
            {
                createCustomTable(TableName.Text);
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Please enter table name");
            }
        }
        private async void DeleteTable_Click(object sender, RoutedEventArgs e)
        {
            var t = delTableName.SelectedItem as TableList;
            if (t!=null)
            {
                DialogHost.Close("RootDialog");
                var res = await OpenConfirmationDialog("delete", "RootDialog");
                if (res)
                {
                    deleteCustomTable(t.TableName);
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Please select a table to delete");
            }
        }
        private void InsertField_Click(object sender, RoutedEventArgs e)
        {
            //if (!string.IsNullOrWhiteSpace(TableName.Text))
            //{
            //    createCustomTable(TableName.Text);
            //}
            //else
            //{
            //    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Please enter table name");
            //}
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var selectedType = _context.SelectedMode;
            if(selectedType==2 || selectedType == 3)
            {
                _tableList.Clear();
                _tableList.Add(new TableList { TableName = "Transaction", Type = "0" });
                _context.TableList=_tableList;
            }
            else
            {
                populateLists();
            }
        }

        private void TableSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TableList selectedTable =TableSelector.SelectedItem as TableList;
            //bool isEnable = false;
            //if (selectedTable != null)
            //{
            //    isEnable= true;
            //}
            //_context.IsNextEnable = true;
            var test = _context.IsNextEnabled;
            //Debug.WriteLine(test);
        }
        private async Task<bool> OpenConfirmationDialog(string action,string dialogHost)
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new ConfirmationDialog(action);

            //show the dialog
            var result = await DialogHost.Show(view, dialogHost, ClosingEventHandler);

            return (bool)result;
            //check the result...
            ////Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            //Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }

        private void TableName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
    }
}
