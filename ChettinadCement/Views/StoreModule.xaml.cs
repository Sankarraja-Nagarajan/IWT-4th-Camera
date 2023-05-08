using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
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

namespace IWT.Views
{
    /// <summary>
    /// Interaction logic for StoreModule.xaml
    /// </summary>
    public partial class StoreModule : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private int _CurrentPage;
        private int _NumberOfPages;
        private int _SelectedRecord;
        private bool _IsFirstEnable;
        private bool _IsPreviousEnable;
        private bool _IsNextEnable;
        private bool _IsLastEnable;
        int RecordStartFrom = 0;
        public int TotalRecords = 0;
        List<int> ItemPerPagesList = new List<int> { 5, 10, 25, 50, 100, 250, 500 };
        public CommonFunction commonFunction = new CommonFunction();

        List<Transaction> Transaction = new List<Transaction>();
        Transaction SelectedTransaction = null;
        List<Transaction> Result1 = new List<Transaction>();
        RFIDAllocation RFIDAllocation = new RFIDAllocation();

        private AdminDBCall _dbContext;
        RolePriviliege rolePriviliege;
        string Rolename;
        public StoreModule(string _rolename, RolePriviliege _rolePriviliege)
        {
            InitializeComponent();
            Loaded += StoreModule_Loaded;
            _dbContext = new AdminDBCall();
            Rolename = _rolename;
            rolePriviliege = _rolePriviliege;
            DataContext = this;
        }

        private void StoreModule_Loaded(object sender, RoutedEventArgs e)
        {
            SelectedRecord = 10;
            NumberOfPages = 1;
            GetTransactionsList();
            PaginatorComboBox.ItemsSource = ItemPerPagesList;
        }
        public void GetTransactionsList()
        {
            DataTable table = _dbContext.GetAllData($"SELECT * FROM [Transaction] WHERE Pending=1 AND MultiWeight=1");
            var JsonString = JsonConvert.SerializeObject(table);
            Transaction = JsonConvert.DeserializeObject<List<Transaction>>(JsonString);
            if (Transaction != null)
            {
                Transaction = Transaction.OrderByDescending(x => x.RFIDAllocation).ToList();
                TotalRecords = Transaction.Count;
                SelectedRecord = 10;
                UpdateRecordCount();
                UpdateCollection(Transaction.Take(SelectedRecord));
                SetDynamicTable();
                CheckAndEnableButtons();
            }
            else
            {
                Transaction = new List<Transaction>();
            }
        }

        private void CheckAndEnableButtons()
        {
            Dispatcher.Invoke(() =>
            {
                if (SelectedTransaction != null)
                {
                    ManageMaterial.IsEnabled = rolePriviliege.StoreAccess.HasValue && rolePriviliege.StoreAccess.Value;
                    PreviewTransaction.IsEnabled = rolePriviliege.StoreAccess.HasValue && rolePriviliege.StoreAccess.Value;
                }
                else
                {
                    ManageMaterial.IsEnabled = false;
                    PreviewTransaction.IsEnabled = false;
                }
            });
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var s = sender as DataGrid;
            SelectedTransaction = s.SelectedItem as Transaction;
            if (SelectedTransaction != null)
            {
                RFIDAllocation = commonFunction.GetRFIDAllocationById(SelectedTransaction.RFIDAllocation);
            }            
            CheckAndEnableButtons();
        }

        public int CurrentPage
        {
            get { return _CurrentPage; }
            set
            {
                _CurrentPage = value;
                OnPropertyChanged();
                UpdateEnableStatus();
            }
        }

        public int NumberOfPages
        {
            get { return _NumberOfPages; }
            set
            {
                _NumberOfPages = value;
                OnPropertyChanged();
                UpdateEnableStatus();
            }
        }

        public int SelectedRecord
        {
            get { return _SelectedRecord; }
            set
            {
                _SelectedRecord = value;
                OnPropertyChanged();
                UpdateRecordCount();
            }
        }


        public bool IsFirstEnable
        {
            get { return _IsFirstEnable; }
            set
            {
                _IsFirstEnable = value;
                OnPropertyChanged();
            }
        }

        public bool IsPreviousEnable
        {
            get { return _IsPreviousEnable; }
            set
            {
                _IsPreviousEnable = value;
                OnPropertyChanged();
            }
        }

        public bool IsNextEnable
        {
            get { return _IsNextEnable; }
            set
            {
                _IsNextEnable = value;
                OnPropertyChanged();
            }
        }
        public bool IsLastEnable
        {
            get { return _IsLastEnable; }
            set
            {
                _IsLastEnable = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void UpdateCollection(IEnumerable<Transaction> enumerables)
        {
            Result1.Clear();
            foreach (var enumera in enumerables)
            {
                Result1.Add(enumera);
            }
            SetDynamicTable();
        }

        public void SetDynamicTable()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    MaterialGrid5.ItemsSource = Result1;
                    MaterialGrid5.Items.Refresh();
                });
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetDynamicTable:" + ex.Message);
            }
        }

        public void UpdateRecordCount()
        {
            NumberOfPages = (int)Math.Ceiling((double)Transaction.Count / SelectedRecord);
            NumberOfPages = NumberOfPages == 0 ? 1 : NumberOfPages;
            UpdateCollection(Transaction.Take(SelectedRecord));
            CurrentPage = 1;
        }
        public void UpdateEnableStatus()
        {
            IsFirstEnable = CurrentPage > 1;
            IsPreviousEnable = CurrentPage > 1;
            IsNextEnable = CurrentPage < NumberOfPages;
            IsLastEnable = CurrentPage < NumberOfPages;
        }

        private void PaginatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(PaginatorComboBox.SelectedValue?.ToString()))
            {
                SelectedRecord = Convert.ToInt32(PaginatorComboBox.SelectedValue.ToString());
            }
            UpdateRecordCount();
        }

        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            UpdateCollection(Transaction.Take(SelectedRecord));
            CurrentPage = 1;
            UpdateEnableStatus();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
            RecordStartFrom = SelectedRecord * (CurrentPage - 1);
            var RecordToShow = Transaction.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            UpdateEnableStatus();
        }
        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            RecordStartFrom = CurrentPage * SelectedRecord;
            var RecordToShow = Transaction.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            CurrentPage++;
            UpdateEnableStatus();

        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            var reordsToSkip = SelectedRecord * (NumberOfPages - 1);
            UpdateCollection(Transaction.Skip(reordsToSkip));
            CurrentPage = NumberOfPages;
            UpdateEnableStatus();
        }

        private async void ManageMaterial_Click(object sender, RoutedEventArgs e)
        {
            await OpenDialog(RFIDAllocation);
        }

        private async void PreviewTransaction_Click(object sender, RoutedEventArgs e)
        {
            await OpenDialogForPreviewTransaction(SelectedTransaction);
        }

        private async Task OpenDialogForPreviewTransaction(Transaction transaction)
        {
            var view = new PreviewTransactionDialog(transaction);
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);            
        }

        private async Task OpenDialog(RFIDAllocation allocation)
        {
            var view = new ManageMaterialDialog(allocation, "Store");
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            if (result != null)
            {
                var res = result as StoreManagement;
                SaveStoreManagement(res);
            }
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            //Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }

        private void SaveStoreManagement(StoreManagement storeManagement)
        {
            SqlConnection con = new SqlConnection(_dbContext.GetDecryptedConnectionStringDB());
            SqlTransaction transaction;
            con.Open();
            transaction = con.BeginTransaction();
            try
            {
                commonFunction.SaveStoreManagement(storeManagement, transaction, con);
                transaction.Commit();
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Saved Successfully !!!");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                WriteLog.WriteToFile("SaveStoreManagement:" + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            GetTransactionsList();
        }
    }
}
