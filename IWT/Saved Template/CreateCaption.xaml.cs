using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using IWT.Views;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
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

namespace IWT.Saved_Template
{
    /// <summary>
    /// Interaction logic for CreateCaption.xaml
    /// </summary>
    /// 
    public partial class CreateCaption : UserControl
    {
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        public static AdminDBCall adminDBCall = new AdminDBCall();
        public static CommonFunction commonFunction = new CommonFunction();
        List<TableDetails> tableDetails = new List<TableDetails>();
        TableDetails SelectedTableDetails = new TableDetails();
        List<TableColumnDetails> tableColumnDetails = new List<TableColumnDetails>();
        List<Caption> captions = new List<Caption>();

        public CreateCaption()
        {
            InitializeComponent();
            toastViewModel = new ToastViewModel();
            Loaded += CreateCaption_Loaded;
            Unloaded += CreateCaption_Unloaded;
            //BindComboBox(Combobox);
        }

        private void CreateCaption_Loaded(object sender, RoutedEventArgs e)
        {
            GetTableDetails();
        }
        private void CreateCaption_Unloaded(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public void GetTableDetails()
        {
            try
            {
                tableDetails = commonFunction.GetTableDetails();
                tableDetails = tableDetails.OrderBy(t => t.TableName).ToList();
                TableDetailsComboBox.ItemsSource = tableDetails;
                TableDetailsComboBox.Items.Refresh();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateCaption/GetTableDetails/Exception:- " + ex.Message, ex);
            }
        }

        public void TableDetailsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedTableDetails = TableDetailsComboBox.SelectedItem as TableDetails;
            if (SelectedTableDetails != null)
            {
                if (!string.IsNullOrEmpty(SelectedTableDetails.TableName))
                {
                    GetCaptions(SelectedTableDetails.TableName);
                    GetTableColumnDetails();
                    AddNonExistsCaptions();
                    LoadCaptionDataGrid();
                }
            }
        }

        public List<Caption> GetCaptions(string TableName)
        {
            captions = new List<Caption>();
            try
            {
                captions = commonFunction.GetCaptions(TableName);
                return captions;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetCaptions : " + ex.Message);
                return captions;
            }
        }

        public void GetTableColumnDetails()
        {
            try
            {
                tableColumnDetails = commonFunction.GetTableColumnDetails(SelectedTableDetails.TableName);
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("CreateTemplate/GetTableDetails/Exception:- " + ex.Message, ex);
            }
        }

        public void AddNonExistsCaptions()
        {
            var NonExistColumns = tableColumnDetails.Where(x => !captions.Any(y => y.FieldName == x.ColumnName)).ToList();
            foreach (var column in NonExistColumns)
            {
                Caption caption = new Caption();
                caption.TableName = column.TableName;
                caption.FieldName = column.ColumnName;
                caption.CaptionName = "";
                caption.Width = 0;
                captions.Add(caption);
            }
        }

        public void LoadCaptionDataGrid()
        {
            CaptionDataGrid.ItemsSource = captions.Where(x => !x.IsDeleted).ToList();
            CaptionDataGrid.Items.Refresh();
        }

        private void BindComboBox()
        {

        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(true, null);
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TableDetailsComboBox.SelectedIndex == -1)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a table to add captions");
            }
            else
            {
                //captions=JsonConvert.DeserializeObject<List<Caption>>(CaptionDataGrid.ItemsSource);
                captions = CaptionDataGrid.ItemsSource as List<Caption>;
                if (commonFunction.CreateCaptions(captions))
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Captions created successfully");
                    DialogHost.CloseDialogCommand.Execute(true, null);
                }
            }
        }

        private async void Delete_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
                var captionToDelete = image.DataContext as Caption;
                if (captionToDelete != null && captionToDelete.CaptionID != 0)
                {
                    var res = await OpenConfirmationDialog();
                    if (res)
                    {
                        var result = commonFunction.DeleteCaption(captionToDelete);
                        if (result)
                        {
                            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Caption deleted successfully");
                            var c=captions.Where(x => x.CaptionID == captionToDelete.CaptionID).FirstOrDefault();
                            if (c != null)
                            {
                                c.IsDeleted = true;
                            }
                            LoadCaptionDataGrid();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SummaryReport/Delete_MouseLeftButtonDown/Exception:- " + ex.Message, ex);
            }
        }
        public async Task<bool> OpenConfirmationDialog()
        {
            var view = new ConfirmationDialog("Delete the caption");

            //    //show the dialog
            var result = await DialogHost.Show(view, "CaptionDialog", ClosingEventHandler);
            return (bool)result;
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {

        }
        public void ShowMessage(Action<string> message, string name)
        {
            this.Dispatcher.Invoke(() =>
            {
                LastMessage = name;
                message(LastMessage);
            });
        }

    }
}
