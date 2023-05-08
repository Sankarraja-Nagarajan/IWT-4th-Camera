using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using IWT.Views;
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
    /// Interaction logic for FormulaFieldDialog.xaml
    /// </summary>
    public partial class FormulaFieldDialog : UserControl
    {
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        private AdminDBCall _dbContext;
        private FormulaTemplate selectedFormula=new FormulaTemplate();
        private List<string> operators = new List<string> { "*", "+", "-", "/", "%", "(", ")" };
        public FormulaFieldDialog(List<TabelColumns> tabelColumns)
        {
            InitializeComponent();
            _dbContext = new AdminDBCall();
            toastViewModel = new ToastViewModel();
            FieldName.ItemsSource = tabelColumns;
            Operator.ItemsSource = operators;
        }

        private async void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            TicketDataTemplate ticketData = new TicketDataTemplate();
            ticketData.F_Table = "Transaction";
            ticketData.F_Type = "NVARCHAR";
            ticketData.F_Size = "200";
            ticketData.F_FieldName = FormulaName.Text;
            ticketData.F_Caption = FormulaName.Text;
            ticketData.ControlDisableFirst = false;
            ticketData.ControlDisableSecond = false;
            ticketData.ControlDisableSingle = false;
            ticketData.Dependent = false;
            ticketData.ControlType = "Formula";
            ticketData.Mandatory = false;
            ticketData.MandatoryStatus = JsonConvert.SerializeObject(new ControlStatus(false, false, false, false, false));
            ticketData.ControlLoadStatusDisable = JsonConvert.SerializeObject(new ControlStatus());
            selectedFormula.FormulaList = Formula.Text;
            selectedFormula.FormulaName=FormulaName.Text;

            var res = _dbContext.SaveFormulaField(selectedFormula,ticketData);
            if (res)
            {
                DialogHost.Close("RootDialog");
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Field Inserted Successsfully !!");

                var view = new ViewFieldDialog()
                {
                    DataContext = new ViewFieldDialogViewModel(GetTicktetDataTemplateData(ticketData.F_Table), ticketData.F_Table)
                };

                var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            }
            else
            {
                DialogHost.Close("RootDialog");
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something Went Wrong");
            }
        }
        private void Constant_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private async void Formula_Template_Button_Click(object sender, RoutedEventArgs e)
        {
            var res = await OpenFormulaTemplateDialog();
            if (res != null)
            {
                selectedFormula = res;
                FormulaName.Text = res.FormulaName;
                Formula.Text = res.FormulaList;
                SaveBtn.IsEnabled = true;
                FormulaName.IsReadOnly = true;
                //DataTable dt = new DataTable();
                //var v = dt.Compute("40/(4+4)", "");
            }
        }

        private async Task<FormulaTemplate> OpenFormulaTemplateDialog()
        {
            var view = new FormulaTemplateDialog();
            var result = await DialogHost.Show(view, "FormulaDialogHost", ClosingEventHandler);
            return (FormulaTemplate)result;
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            ////Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }

        private void Field_Button_Click(object sender, RoutedEventArgs e)
        {
            TabelColumns field = FieldName.SelectedItem as TabelColumns;
            if (field != null)
            {
                FieldName.SelectedIndex = -1;
                FieldBtn.IsEnabled = false;
                Formula.Text += field.column_name+" ";
            }
        }

        private void Operator_Button_Click(object sender, RoutedEventArgs e)
        {
            string item = Operator.SelectedValue as string;
            Formula.Text += item + " ";
            Operator.SelectedIndex = -1;
            OperatorBtn.IsEnabled = false;
        }

        private void Constant_Button_Click(object sender, RoutedEventArgs e)
        {
            Formula.Text += Constant.Text + " ";
            Constant.Text = "";
            ConstantBtn.IsEnabled = false;
        }

        private void Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            //FormulaName.Text = "";
            //FieldName.SelectedIndex = -1;
            //Operator.SelectedIndex = -1;
            //Constant.Text = "";
            Formula.Text = "";
            //FieldBtn.IsEnabled = false;
            //OperatorBtn.IsEnabled = false;
            //ConstantBtn.IsEnabled = false;

        }

        private void FieldName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FieldBtn.IsEnabled = true;
        }

        private void Operator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OperatorBtn.IsEnabled = true;
        }

        private void Constant_KeyUp(object sender, KeyEventArgs e)
        {
            if (Constant.Text != "")
            {
                ConstantBtn.IsEnabled = true;
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
