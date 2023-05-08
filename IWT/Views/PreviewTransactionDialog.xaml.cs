using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace IWT.Views
{
    /// <summary>
    /// Interaction logic for PreviewTransactionDialog.xaml
    /// </summary>
    public partial class PreviewTransactionDialog : UserControl, INotifyPropertyChanged
    {
        private Transaction OldTransaction = null;
        private AdminDBCall _dbContext;
        public static CommonFunction commonFunction = new CommonFunction();
        public static MasterDBCall masterDBCall = new MasterDBCall();
        private readonly ToastViewModel toastViewModel;
        public int TotalRecords = 0;
        List<int> ItemPerPagesList = new List<int> { 5, 10, 25, 50, 100, 250, 500 };

        public event PropertyChangedEventHandler PropertyChanged;
        private int _CurrentPage;
        private int _NumberOfPages;
        private int _SelectedRecord;
        private bool _IsFirstEnable;
        private bool _IsPreviousEnable;
        private bool _IsNextEnable;
        private bool _IsLastEnable;
        int RecordStartFrom = 0;

        List<TransactionDetails> Result = new List<TransactionDetails>();
        List<TransactionDetails> Result1 = new List<TransactionDetails>();

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

        public PreviewTransactionDialog(Transaction transaction)
        {
            InitializeComponent();
            _dbContext = new AdminDBCall();
            toastViewModel = new ToastViewModel();
            OldTransaction = transaction;
            DataContext = this;
            Loaded += ManagePreviewTransactionDialog_Loaded;
        }

        private void ManagePreviewTransactionDialog_Loaded(object sender, RoutedEventArgs e)
        {
            SelectedRecord = 10;
            NumberOfPages = 1;
            PaginatorComboBox.ItemsSource = ItemPerPagesList;
            if (OldTransaction != null)
            {
                GetTransactionDetailsListByTicketNo(OldTransaction.TicketNo);
            }
        }   

        public void GetTransactionDetailsListByTicketNo(int TicketNo)
        {
            try
            {
                string Query = "SELECT * FROM [Transaction_Details] WHERE TicketNo=@TicketNo";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@TicketNo", TicketNo);
                DataTable table = masterDBCall.GetData(cmd, System.Data.CommandType.Text);
                string JSONString = JsonConvert.SerializeObject(table);
                Result = JsonConvert.DeserializeObject<List<TransactionDetails>>(JSONString);
                if (Result != null)
                {
                    Result = Result.OrderByDescending(x => x.TicketNo).ToList();
                }
                TotalRecords = Result.Count;
                SelectedRecord = 10;
                UpdateRecordCount();
                UpdateCollection(Result.Take(SelectedRecord));
                SetDynamicTable();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetTransactionDetailsListByTicketNo" + ex.Message, ex);
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        public void SetDynamicTable()
        {
            try
            {
                MaterialGrid5.ItemsSource = Result1;
                MaterialGrid5.Items.Refresh();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SetDynamicTable:" + ex.Message);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void UpdateCollection(IEnumerable<TransactionDetails> enumerables)
        {
            Result1.Clear();
            foreach (var enumera in enumerables)
            {
                Result1.Add(enumera);
            }
            SetDynamicTable();
        }

        public void UpdateRecordCount()
        {
            if (Result != null)
            {
                NumberOfPages = (int)Math.Ceiling((double)Result.Count / SelectedRecord);
                NumberOfPages = NumberOfPages == 0 ? 1 : NumberOfPages;
                UpdateCollection(Result.Take(SelectedRecord));
                CurrentPage = 1;
            }
        }
        public void UpdateEnableStatus()
        {
            IsFirstEnable = CurrentPage > 1;
            IsPreviousEnable = CurrentPage > 1;
            IsNextEnable = CurrentPage < NumberOfPages;
            IsLastEnable = CurrentPage < NumberOfPages;
            //FirstPage.IsEnabled = CurrentPage > 1;
            //PreviousPage.IsEnabled = CurrentPage > 1;
            //NextPage.IsEnabled= CurrentPage < NumberOfPages;
            //LastPage.IsEnabled=CurrentPage< NumberOfPages;
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
            UpdateCollection(Result.Take(SelectedRecord));
            CurrentPage = 1;
            UpdateEnableStatus();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
            //RecordStartFrom = Result.Count - SelectedRecord * (NumberOfPages - (CurrentPage - 1));
            RecordStartFrom = SelectedRecord * (CurrentPage - 1);
            var RecordToShow = Result.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            UpdateEnableStatus();
        }
        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            RecordStartFrom = CurrentPage * SelectedRecord;
            var RecordToShow = Result.Skip(RecordStartFrom).Take(SelectedRecord);
            UpdateCollection(RecordToShow);
            CurrentPage++;
            UpdateEnableStatus();

        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            var reordsToSkip = SelectedRecord * (NumberOfPages - 1);
            UpdateCollection(Result.Skip(reordsToSkip));
            CurrentPage = NumberOfPages;
            UpdateEnableStatus();
        }
    }
}
