using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class Addsupplier : UserControl
    {
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        private AdminDBCall _dbContext;
        private List<SupplierMaster> AllSuppliers = new List<SupplierMaster>();
        int selectedIndex = 0;
        public Addsupplier(int _selectedIndex = 0)
        {
            InitializeComponent();
            selectedIndex = _selectedIndex;
            _dbContext = new AdminDBCall();
            MainTabControl.SelectedIndex = selectedIndex;
            GetAllSuppliers();
            toastViewModel = new ToastViewModel();
        }
        private void GetAllSuppliers()
        {
            DataTable dt = _dbContext.GetAllData("SELECT * FROM [dbo].[Supplier_Master] WHERE IsDeleted='FALSE'");
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    var row = new SupplierMaster();
                    row.SupplierID = (int)item["SupplierID"];
                    row.SupplierCode = (string)item["SupplierCode"];
                    row.SupplierName = (string)item["SupplierName"];
                    AllSuppliers.Add(row);
                }
            }
            SupplierCode.ItemsSource = AllSuppliers;
            SupplierName.ItemsSource = AllSuppliers;
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            string materialCode = NewSupplierCode.Text;
            string materialName = NewSupplierName.Text;
            SupplierMaster supplierMaster = new SupplierMaster();
            supplierMaster.SupplierCode = materialCode;
            supplierMaster.SupplierName = materialName;
            if (materialCode != "" && materialName != "")
            {
                var previousValue = AllSuppliers.FirstOrDefault(t => t.SupplierCode == supplierMaster.SupplierCode || t.SupplierName == supplierMaster.SupplierName);
                if (previousValue == null)
                {
                    bool res = _dbContext.ExecuteQuery($"INSERT INTO [Supplier_Master] (SupplierCode,SupplierName,IsDeleted) VALUES ('{materialCode}','{materialName}','False')");
                    if (res)
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Supplier Created Successsfully !!");
                        DialogHost.CloseDialogCommand.Execute(supplierMaster, null);
                    }
                    else
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !!");
                    }
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Record with same SupplierCode/SupplierName already exists !!");
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the details !!");
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
        private void SupplierCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SupplierName.SelectedIndex = SupplierCode.SelectedIndex;
        }

        private void SupplierName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SupplierCode.SelectedIndex = SupplierName.SelectedIndex;
        }
        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            SupplierMaster materialMaster = SupplierCode.SelectedItem as SupplierMaster;
            if (materialMaster != null)
            {
                DialogHost.CloseDialogCommand.Execute(materialMaster, null);
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a supplier !!");
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }

}
