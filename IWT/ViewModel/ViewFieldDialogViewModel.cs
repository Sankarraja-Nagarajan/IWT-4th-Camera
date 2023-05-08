using IWT.Admin_Pages;
using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IWT.ViewModel
{
    public class ViewFieldDialogViewModel:ViewBaseModel
    {
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        private AdminDBCall _dbContext;
        private string _tableName;
        public ViewFieldDialogViewModel(List<TicketDataTemplate> ticketData,string tableName)
        {
            _tableData=ticketData;
            _tableName = tableName;
            _dbContext=new AdminDBCall();
            toastViewModel=new ToastViewModel();
        }
        private List<TicketDataTemplate> _tableData;
        public List<TicketDataTemplate> TableData { get => _tableData; set => SetProperty(ref _tableData, value); }
        public ICommand DeleteCommand => new AnotherCommandImplementation(DeleteTicketData,r=>TableData.Where(t=>t.IsSelected).Count()>0);
        public ICommand DisableCommand => new AnotherCommandImplementation(OpenDisableControlDialog, r => TableData.Where(t => t.IsSelected).Count() == 1 && _tableName=="Transaction" && TableData.FirstOrDefault(t=>t.IsSelected).ControlType!= "DataDependancy" && TableData.FirstOrDefault(t => t.IsSelected).ControlType != "Formula");
        public ICommand MandatoryCommand => new AnotherCommandImplementation(OpenMandatoryControlDialog, r => TableData.Where(t => t.IsSelected).Count() == 1 && _tableName == "Transaction" && TableData.FirstOrDefault(t => t.IsSelected).ControlType != "DataDependancy" && TableData.FirstOrDefault(t => t.IsSelected).ControlType != "Formula");
        private void DeleteTicketData(object _)
        {
            var selected = TableData.Where(t => t.IsSelected).ToList();
            var res=_dbContext.DropCustomFields(selected);
            if (res)
            {
                var result = GetTicktetDataTemplateData(_tableName);
                TableData = result;
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Fields Deleted Successsfully !!");
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something Went Wrong");
            }
        }
        private async void OpenDisableControlDialog(object _)
        {
            var selected = TableData.Where(t => t.IsSelected).First();
            ControlStatus controlStatus = JsonConvert.DeserializeObject<ControlStatus>(selected.ControlLoadStatusDisable);
            var view = new ControlStatusDialog("Control Disable", controlStatus);
            var result = await DialogHost.Show(view, "ViewFieldDialogHost", ClosingEventHandler);
            if (result != null)
            {
                ControlStatus cs = result as ControlStatus;
                SaveControlStatusDisable(JsonConvert.SerializeObject(cs), selected.ControlID);
            }
        }
        private async void OpenMandatoryControlDialog(object _)
        {
            var selected = TableData.Where(t => t.IsSelected).First();
            ControlStatus controlStatus = JsonConvert.DeserializeObject<ControlStatus>(selected.MandatoryStatus);
            var view = new ControlStatusDialog("Control Mandatory", controlStatus);
            var result = await DialogHost.Show(view, "ViewFieldDialogHost", ClosingEventHandler);
            if (result != null)
            {
                ControlStatus cs = result as ControlStatus;
                SaveControlStatusMandatory(JsonConvert.SerializeObject(cs), selected.ControlID);
            }
        }
        private void SaveControlStatusDisable(string controlStatus,int ControlID)
        {
            string query = $@"UPDATE [Ticket_Data_Template] SET ControlLoadStatusDisable='{controlStatus}' WHERE ControlID='{ControlID}'";
            var res = _dbContext.ExecuteQuery(query);
            if (res)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Disable status updated");
                var result = GetTicktetDataTemplateData(_tableName);
                TableData = result;
            }
        }
        private void SaveControlStatusMandatory(string controlStatus, int ControlID)
        {
            string query = $@"UPDATE [Ticket_Data_Template] SET MandatoryStatus='{controlStatus}' WHERE ControlID='{ControlID}'";
            var res = _dbContext.ExecuteQuery(query);
            if (res)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Mandatory status updated");
                var result = GetTicktetDataTemplateData(_tableName);
                TableData = result;
            }
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
        public void ShowMessage(Action<string> message, string name)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LastMessage = $"{name}";
                message(LastMessage);
            });
        }
    }
}
