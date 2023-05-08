using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

namespace IWT.Setting_Pages
{
    /// <summary>
    /// Interaction logic for SMS_Design.xaml
    /// </summary>
    public partial class SMS_Design : UserControl
    {
        public static MasterDBCall masterDBCall = new MasterDBCall();
        public static CommonFunction commonFunction = new CommonFunction();
        private readonly ToastViewModel toastViewModel;
        public SMSDesign CurrentSMSDesign;
        string LastMessage;
        public SMS_Design()
        {
            InitializeComponent();
            toastViewModel = new ToastViewModel();
            CurrentSMSDesign = new SMSDesign();
            GetTableColumnDetails("Transaction");
            GetSMSDesigns();
        }

        public List<TableColumnDetails> GetTableColumnDetails(string TableName)
        {
            try
            {
                List<TableColumnDetails> tableColumnDetails = commonFunction.GetTableColumnDetails(TableName);
                TransactionListView.ItemsSource = tableColumnDetails;
                TransactionListView.Items.Refresh();
                TransactionListView.SelectionChanged += TransactionListView_SelectionChanged;
                return tableColumnDetails;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/GetTableColumnDetails/Exception:- " + ex.Message, ex);
                return null;
            }
        }

        private void TransactionListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //throw new NotImplementedException();
            if (TransactionListView.SelectedItems?.Count > 0)
            {
                var selectedItem = TransactionListView.SelectedItems[0] as TableColumnDetails;
                //MessageTextBox.Text = MessageTextBox.Text + "{" + selectedItem.ColumnName + "}";
                MessageTextBox.Text = MessageTextBox.Text + "[" + selectedItem.ColumnName + "]";
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

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null && item.IsSelected)
            {
                //Do your stuff
            }
        }


        private void SaveDesignButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(MessageTextBox.Text))
            {
                string DesignedContent = MessageTextBox.Text;
                bool result = SaveSMSDesign(DesignedContent);
                if (result)
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "SMS Design saved successfully");
                    DialogHost.CloseDialogCommand.Execute(null, null);
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the content");
            }
        }

        public List<SMSDesign> GetSMSDesigns()
        {
            try
            {
                var result = commonFunction.GetSMSDesigns();
                if (result.Count > 0)
                {
                    CurrentSMSDesign = result.FirstOrDefault();
                    if (CurrentSMSDesign != null)
                    {
                        MessageTextBox.Text = CurrentSMSDesign.DesignedContent;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SMS_Design/GetTableColumnDetails/Exception:- " + ex.Message, ex);
                return null;
            }
        }

        public bool SaveSMSDesign(string DesignedContent)
        {
            try
            {
                if (CurrentSMSDesign != null && CurrentSMSDesign.ID != 0)
                {
                    return UpdateSMSDesign(DesignedContent);
                }
                else
                {
                    return CreateSMSDesign(DesignedContent);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/SaveSMSDesign/Exception:- " + ex.Message, ex);
                return false;
            }

        }

        public bool CreateSMSDesign(string DesignedContent)
        {
            try
            {
                string Query = "INSERT INTO SMS_Design (DesignedContent,CreatedOn) values (@DesignedContent,@CreatedOn)";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@DesignedContent", DesignedContent);
                cmd.Parameters.AddWithValue("@CreatedOn", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                masterDBCall.InsertData(cmd, System.Data.CommandType.Text);
                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/SaveSMSDesign/Exception:- " + ex.Message, ex);
                return false;
            }
        }
        public bool UpdateSMSDesign(string DesignedContent)
        {
            try
            {
                string Query = "UPDATE SMS_Design SET DesignedContent=@DesignedContent,ModifiedOn=@ModifiedOn WHERE ID=@ID";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@ID", CurrentSMSDesign.ID);
                cmd.Parameters.AddWithValue("@DesignedContent", DesignedContent);
                cmd.Parameters.AddWithValue("@ModifiedOn", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                masterDBCall.InsertData(cmd, System.Data.CommandType.Text);
                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CommonFunction/SaveSMSDesign/Exception:- " + ex.Message, ex);
                return false;
            }
        }
    }
}
