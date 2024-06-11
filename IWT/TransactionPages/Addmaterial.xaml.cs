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
using static IWT.ViewModel.Viewvehicle;

namespace IWT.TransactionPages
{
    /// <summary>
    /// Interaction logic for Addmaterial.xaml
    /// </summary>
    public partial class Addmaterial : UserControl
    {
        private readonly ToastViewModel toastViewModel;
        string LastMessage;
        private AdminDBCall _dbContext;
        private List<MaterialMaster> AllMaterials = new List<MaterialMaster>();
        int selectedIndex = 0;
        public Addmaterial(int _selectedIndex = 0)
        {
            InitializeComponent();
            selectedIndex = _selectedIndex;
            _dbContext = new AdminDBCall();
            MainTabControl.SelectedIndex = selectedIndex;
            GetAllMaterials();
            toastViewModel = new ToastViewModel();
        }

        private void GetAllMaterials()
        {
            DataTable dt = _dbContext.GetAllData("SELECT * FROM [dbo].[Material_Master] WHERE IsDeleted='FALSE'");
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    var row = new MaterialMaster();
                    row.MaterialID = (int)item["MaterialID"];
                    row.MaterialCode = (string)item["MaterialCode"];
                    row.MaterialName = (string)item["MaterialName"];
                    AllMaterials.Add(row);
                }
            }
            MaterialCode.ItemsSource = AllMaterials;
            MaterialName.ItemsSource = AllMaterials;
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            string materialCode = NewMaterialCode.Text;
            string materialName = NewMaterialName.Text;
            MaterialMaster materialMaster = new MaterialMaster();
            materialMaster.MaterialCode = materialCode;
            materialMaster.MaterialName = materialName;
            if (materialCode != "" && materialName != "")
            {
                var previousValue = AllMaterials.FirstOrDefault(t => t.MaterialCode == materialMaster.MaterialCode || t.MaterialName.ToLower() == materialMaster.MaterialName.ToLower());
                if (previousValue == null)
                {
                    bool res = _dbContext.ExecuteQuery($"INSERT INTO [Material_Master] (MaterialCode,MaterialName,IsDeleted) VALUES ('{materialCode}','{materialName}','False')");
                    if (res)
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Material Created Successsfully !!");
                        DialogHost.CloseDialogCommand.Execute(materialMaster, null);
                    }
                    else
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Something went wrong !!");
                    }
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Record with same MaterialCode/MaterialName already exists !!");
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
        private void MaterialCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MaterialName.SelectedIndex = MaterialCode.SelectedIndex;
        }

        private void MaterialName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MaterialCode.SelectedIndex = MaterialName.SelectedIndex;
        }
        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            MaterialMaster materialMaster = MaterialCode.SelectedItem as MaterialMaster;
            if (materialMaster != null)
            {
                DialogHost.CloseDialogCommand.Execute(materialMaster, null);
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a material !!");
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
