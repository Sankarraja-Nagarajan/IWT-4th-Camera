using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace IWT.FactorySetupPages
{
    /// <summary>
    /// Interaction logic for SoftwareConfigureDialog.xaml
    /// </summary>
    public partial class SoftwareConfigureDialog : UserControl
    {
        AdminDBCall dbCall = new AdminDBCall();
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        private SoftwareConfigure _softwareConfigure=new SoftwareConfigure();
        public SoftwareConfigureDialog()
        {
            InitializeComponent();
            toastViewModel = new ToastViewModel();
            Loaded += SoftwareConfigureDialog_Loaded;
        }

        private void SoftwareConfigureDialog_Loaded(object sender, RoutedEventArgs e)
        {
            GetSoftwareConfig();
        }

        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveSoftwareConfig();
        }
        public void GetSoftwareConfig()
        {
            try
            {
                DataTable table = dbCall.GetAllData("select * from [Software_SettingConfig]");
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<SoftwareConfigure>>(JSONString);
                var sc = result.FirstOrDefault();
                if (sc != null)
                {
                    _softwareConfigure = sc;
                    SetSoftwareConfig();
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("SoftwareConfigureDialog/GetSoftwareConfig/Exception:- " + ex.Message, ex);
            }
        }
        public void SetSoftwareConfig()
        {
            single.IsChecked = _softwareConfigure.Single_Transaction;
            first.IsChecked = _softwareConfigure.First_Transaction;
            second.IsChecked = _softwareConfigure.Second_Transaction;
            firstMulti.IsChecked = _softwareConfigure.First_MultiTransaction;
            secondMulti.IsChecked = _softwareConfigure.Second_MultiTransaction;
            singleAxle.IsChecked = _softwareConfigure.Single_Axle;
            firstAxle.IsChecked = _softwareConfigure.First_Axle;
            secondAxle.IsChecked = _softwareConfigure.Second_Axle;
            loadingAxle.IsChecked = _softwareConfigure.Loading;
            unloadingAxle.IsChecked = _softwareConfigure.Unloading;
        }
        public void SaveSoftwareConfig()
        {
            _softwareConfigure.Single_Transaction = (bool)single.IsChecked;
            _softwareConfigure.First_Transaction = (bool)first.IsChecked;
            _softwareConfigure.Second_Transaction = (bool)second.IsChecked;
            _softwareConfigure.First_MultiTransaction = (bool)firstMulti.IsChecked;
            _softwareConfigure.Second_MultiTransaction = (bool)secondMulti.IsChecked;
            _softwareConfigure.Single_Axle = (bool)singleAxle.IsChecked;
            _softwareConfigure.First_Axle = (bool)firstAxle.IsChecked;
            _softwareConfigure.Second_Axle = (bool)secondAxle.IsChecked;
            _softwareConfigure.Loading = (bool)loadingAxle.IsChecked;
            _softwareConfigure.Unloading = (bool)unloadingAxle.IsChecked;
            string saveQuery = "";
            if (_softwareConfigure.ID > 0)
            {
                saveQuery = $@"UPDATE [Software_SettingConfig] SET Single_Transaction='{_softwareConfigure.Single_Transaction}',First_Transaction='{_softwareConfigure.First_Transaction}',Second_Transaction='{_softwareConfigure.Second_Transaction}',First_MultiTransaction='{_softwareConfigure.First_MultiTransaction}',Second_MultiTransaction='{_softwareConfigure.Second_MultiTransaction}',Single_Axle='{_softwareConfigure.Single_Axle}',First_Axle='{_softwareConfigure.First_Axle}',Second_Axle='{_softwareConfigure.Second_Axle}',Loading='{_softwareConfigure.Loading}',Unloading='{_softwareConfigure.Unloading}' WHERE ID='{_softwareConfigure.ID}'";
            }
            else
            {
                saveQuery = $@"INSERT INTO [Software_SettingConfig] (Single_Transaction,First_Transaction,Second_Transaction,First_MultiTransaction,Second_MultiTransaction,Single_Axle,First_Axle,Second_Axle,Loading,Unloading) VALUES ('{_softwareConfigure.Single_Transaction}','{_softwareConfigure.First_Transaction}','{_softwareConfigure.Second_Transaction}','{_softwareConfigure.First_MultiTransaction}','{_softwareConfigure.Second_MultiTransaction}','{_softwareConfigure.Single_Axle}','{_softwareConfigure.First_Axle}','{_softwareConfigure.Second_Axle}','{_softwareConfigure.Loading}','{_softwareConfigure.Unloading}')";
            }
            var res=dbCall.ExecuteQuery(saveQuery);
            if (res)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Software configuration saved successfully !!");
                DialogHost.CloseDialogCommand.Execute("email", null);
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !!");
            }
        }

        private void Previous_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute("system", null);
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
