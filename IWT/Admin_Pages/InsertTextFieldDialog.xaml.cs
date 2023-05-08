using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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

namespace IWT.Admin_Pages
{
    /// <summary>
    /// Interaction logic for InsertTextFieldDialog.xaml
    /// </summary>
    public partial class InsertTextFieldDialog : UserControl
    {
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        private AdminDBCall _dbContext;
        private InsertTextFieldViewModel _dataContext;
        public static CommonFunction commonFunction = new CommonFunction();
        List<TableColumnDetails> tableColumnDetails = new List<TableColumnDetails>();
        public InsertTextFieldDialog()
        {
            InitializeComponent();
            _dbContext = new AdminDBCall();
            toastViewModel = new ToastViewModel();
        }

        private async void Insert_Button_Click(object sender, RoutedEventArgs e)
        {
            _dataContext = DataContext as InsertTextFieldViewModel;
            TicketDataTemplate ticketData = new TicketDataTemplate();
            //ticketData.ControlTable = _dataContext.TableName;
            ticketData.ControlType = _dataContext.ControlType;
            ticketData.F_FieldName = FieldName.Text;
            var type = CustomFieldType.SelectedItem as FieldType;
            ticketData.F_Type = type.Type;
            ticketData.F_Size = FieldSize.Text;
            ticketData.F_Table = FieldTableName.Text;
            ticketData.F_Caption = FieldCaption.Text;
            ticketData.Mandatory = IsMandatory.IsChecked ?? false;
            ticketData.ControlDisableFirst = false;
            ticketData.ControlDisableSecond = false;
            ticketData.ControlDisableSingle = false;
            if (ticketData.F_Table != null)
            {
                GetTableColumnDetails(ticketData.F_Table);
                bool IsAlreadyExists = false;

                if (tableColumnDetails.Count > 0)
                {
                    var tableColumn = tableColumnDetails.FirstOrDefault(x => x.ColumnName == ticketData.F_FieldName);
                    IsAlreadyExists = tableColumn != null;
                }

                if (!IsAlreadyExists)
                {
                    if (_dataContext.TableName == "Transaction")
                    {
                        ticketData.ControlLoadStatusDisable = JsonConvert.SerializeObject(new ControlStatus(false, false, false, false,false));
                        if ((bool)IsMandatory.IsChecked)
                        {
                            ticketData.MandatoryStatus = JsonConvert.SerializeObject(new ControlStatus());
                        }
                        else
                        {
                            ticketData.MandatoryStatus = JsonConvert.SerializeObject(new ControlStatus(false, false, false, false,false));
                        }
                        if(ticketData.ControlType == "DataDependancy")
                        {
                            ticketData.MandatoryStatus = JsonConvert.SerializeObject(new ControlStatus(false, false, false, false, false));
                            ticketData.ControlLoadStatusDisable = JsonConvert.SerializeObject(new ControlStatus());
                        }
                    }
                    ticketData.Dependent = false;
                    var selectedField = selectionFieldCombobox.SelectedItem as TabelColumns;
                    if (selectedField != null)
                    {
                        ticketData.SelectionBasis = selectedField.column_name;
                    }
                    var selectedSTable = selctionTableCombobox.SelectedItem as TableList;
                    if (selectedSTable != null)
                    {
                        ticketData.ControlTable = selectedSTable.TableName;
                    }
                    var selectedRef = SelectionBasis.SelectedItem as TabelColumns;
                    if (selectedRef != null)
                    {
                        ticketData.ControlTableRef = selectedRef.column_name;
                    }
                    var res = _dbContext.CreateCustomField(ticketData);
                    if (res)
                    {
                        DialogHost.Close("RootDialog");
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Field Inserted Successsfully !!");

                        var view = new ViewFieldDialog()
                        {
                            DataContext = new ViewFieldDialogViewModel(GetTicktetDataTemplateData(ticketData.F_Table), ticketData.F_Table)
                        };

                        //show the dialog
                        var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
                    }
                    else
                    {
                        DialogHost.Close("RootDialog");
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something Went Wrong");
                    }
                }
                else
                {
                    //DialogHost.Close("RootDialog");
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, $"{ticketData.F_FieldName} Field already present in table");
                }
                

            }

        }

        public void GetTableColumnDetails(string TableName)
        {
            tableColumnDetails = new List<TableColumnDetails>();
            try
            {
                if (!string.IsNullOrEmpty(TableName))
                {
                    tableColumnDetails = commonFunction.GetTableColumnDetails(TableName);
                }

            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateTemplate/GetTableDetails/Exception:- " + ex.Message, ex);
            }
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
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            ////Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }
        public void ShowMessage(Action<string> message, string name)
        {
            this.Dispatcher.Invoke(() =>
            {
                LastMessage = $"{name}";
                message(LastMessage);
            });
        }

        private void FieldSize_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void selectionTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _dataContext = DataContext as InsertTextFieldViewModel;
            ComboBox table = (ComboBox)sender;
            ////Debug.WriteLine("selected table", table.SelectedValue);
            var tbColumns = GetTableColumns(table.SelectedValue as string);
            _dataContext.TabelColumns = tbColumns;
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

        private void CustomFieldType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var val = CustomFieldType.SelectedValue as FieldType;
            if (val.DisplayType != "CHARACTER")
            {
                FieldSizeContainer.Visibility = Visibility.Collapsed;
            }
            else
            {
                FieldSizeContainer.Visibility = Visibility.Visible;
            }
        }
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }
    }
}
