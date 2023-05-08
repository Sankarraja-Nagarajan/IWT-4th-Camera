using IWT.Admin_Pages;
using IWT.DBCall;
using IWT.Models;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IWT.ViewModel
{
    public class TicketDataEntryViewModel : ViewBaseModel
    {
        private AdminDBCall _dbContext;
        public TicketDataEntryViewModel()
        {
            _dbContext = new AdminDBCall();
        }
        private TableList _selectedTable;
        public ICommand NextDialogCommand => new AnotherCommandImplementation(ExecuteInsertTextFieldDialog, IsNextEnabled => _selectedTable!=null);
        public ICommand ViewFieldDialogCommand => new AnotherCommandImplementation(ExecuteViewFieldDialog, IsNextEnabled => _selectedTable != null);

        public TableList SelectedTable { get => _selectedTable; set => _selectedTable = value; }
        private bool[] _fieldType = new bool[] { true, false, false, false };
        public int SelectedMode
        {
            get { return Array.IndexOf(FieldType, true); }
        }

        public bool[] FieldType { get => _fieldType; set => _fieldType = value; }

        private ObservableCollection<TableList> tableList = new ObservableCollection<TableList>();
        private ObservableCollection<TableList> allTableList = new ObservableCollection<TableList>();
        public ObservableCollection<TableList> TableList { get => tableList; set => tableList = value; }
        public bool IsNextEnabled => SelectedTable != null;

        public ObservableCollection<TableList> AllTableList { get => allTableList; set => allTableList = value; }

        private async void ExecuteInsertTextFieldDialog(object _)
        {
            if (SelectedMode == 0 || SelectedMode == 1 || SelectedMode == 2)
            {
                //let's set up a little MVVM, cos that's what the cool kids are doing:
                var view = new InsertTextFieldDialog()
                {
                    DataContext = new InsertTextFieldViewModel(_selectedTable, SelectedMode,allTableList)
                };
                //show the dialog
                var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
                //check the result...
                //Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
            }
            else
            {
                var trColumns = GetTableColumns("Transaction");
                //let's set up a little MVVM, cos that's what the cool kids are doing:
                var view = new FormulaFieldDialog(trColumns);

                //show the dialog
                var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);

                //check the result...
                //Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
            }
        }
        private async void ExecuteViewFieldDialog(object _)
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new ViewFieldDialog()
            {
                DataContext = new ViewFieldDialogViewModel(GetTicktetDataTemplateData(_selectedTable.TableName),_selectedTable.TableName)
            };

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);

            //check the result...
            //Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            //Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }
        public List<TicketDataTemplate> GetTicktetDataTemplateData(string tableName)
        {
            try
            {
                DataTable table = _dbContext.GetAllData($"select * from Ticket_Data_Template where F_Table='{tableName}'");
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<TicketDataTemplate>>(JSONString);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetTicktetDataTemplateData:" + ex.Message);
                return new List<TicketDataTemplate>();
            }
        }
        public List<TabelColumns> GetTableColumns(string tableName)
        {
            try
            {
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
    }
}
