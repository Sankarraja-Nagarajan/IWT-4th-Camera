using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using MaterialDesignThemes.Wpf;
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

namespace IWT.Views
{
    /// <summary>
    /// Interaction logic for ManageMaterialDialog.xaml
    /// </summary>
    public partial class ManageMaterialDialog : UserControl
    {
        private RFIDAllocation CurrentAllocation = null;
        private AdminDBCall _dbContext;
        private List<MaterialMaster> AllMaterials = new List<MaterialMaster>();
        private List<SupplierMaster> AllSuppliers = new List<SupplierMaster>();
        public static CommonFunction commonFunction = new CommonFunction();
        private List<StoreMaterialManagement> CurrentStoreMaterials = new List<StoreMaterialManagement>();
        private List<StoreSupplierManagement> CurrentStoreSuppliers = new List<StoreSupplierManagement>();
        private StoreMaterialManagement SelectedStoreMaterial = null;
        private StoreSupplierManagement SelectedStoreSupplier = null;
        private MaterialMaster SelectedMaterialMaster = null;
        private SupplierMaster SelectedSupplierMaster = null;
        private int? noOfMaterialCount;
        private string Source;
        private int maxClosedIndex = 0;

        public ManageMaterialDialog(RFIDAllocation allocation, string source, StoreManagement storeManagement=null)
        {
            InitializeComponent();
            CurrentAllocation = allocation;
            if (storeManagement != null)
            {
                CurrentStoreMaterials = storeManagement.Materials;
                CurrentStoreSuppliers = storeManagement.Suppliers;
            }
            Source = source;
            _dbContext = new AdminDBCall();
            Loaded += ManageMaterialDialog_Loaded; 
        }

        private void ManageMaterialDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentAllocation.IsSapBased)
            {
                SupplierMasterContainer.Visibility = Visibility.Collapsed;
                SupplierMasterActionsContainer.Visibility = Visibility.Collapsed;
                MaterialMasterContainer.Visibility = Visibility.Collapsed;
                MaterialMasterActionsContainer.Visibility = Visibility.Collapsed;
                SelectedSuppliersContainer.Visibility = Visibility.Collapsed;
                OrderSupplierContainer.Visibility = Visibility.Collapsed;
            }
            else if (Source == "Store")
            {
                SupplierMasterContainer.Visibility = Visibility.Collapsed;
                SupplierMasterActionsContainer.Visibility = Visibility.Collapsed;
                MaterialMasterContainer.Visibility = Visibility.Collapsed;
                MaterialMasterActionsContainer.Visibility = Visibility.Collapsed;
                OrderSupplierContainer.Visibility = Visibility.Collapsed;
                if(!CurrentAllocation.IsSapBased)
                    SapItemNumberContainer.Visibility = Visibility.Collapsed;
            }
            else
            {
                GetAllMaterials();
                GetAllSuppliers();
                SapItemNumberContainer.Visibility = Visibility.Collapsed;
            }
            SetStoreItemsValues();            
        }                

        private void SetStoreItemsValues()
        {
            if (Source == "Store")
            {
                CurrentStoreMaterials = commonFunction.GetStoreMaterials(CurrentAllocation.AllocationId);
                CurrentStoreSuppliers = commonFunction.GetStoreSuppliers(CurrentAllocation.AllocationId);
                CurrentStoreMaterials = CurrentStoreMaterials.OrderBy(t => t.Order).ToList();
                CurrentStoreSuppliers = CurrentStoreSuppliers.OrderBy(t => t.Order).ToList();
            }
            maxClosedIndex = CurrentStoreMaterials.Where(t => t.Closed.HasValue && t.Closed.Value).Count();
            SelectedMaterialsDataGrid.ItemsSource = CurrentStoreMaterials;
            SelectedSuppliersDataGrid.ItemsSource = CurrentStoreSuppliers;
            SapItemNumberDataGrid.ItemsSource = CurrentStoreMaterials;
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
            MaterialMasterDataGrid.ItemsSource = AllMaterials;
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
            SupplierMasterDataGrid.ItemsSource = AllSuppliers;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CurrentAllocation.IsSapBased && CurrentStoreMaterials.Count != CurrentStoreSuppliers.Count)
                {
                    throw new Exception("Material Count and Supplier Count not matching");
                }
                if (CurrentStoreMaterials.Count < 1)
                {
                    throw new Exception("Please add Materials");
                }
                var order = 1;
                foreach (var item in CurrentStoreMaterials)
                {
                    item.Order = order++;
                }
                order = 1;
                foreach (var item in CurrentStoreSuppliers)
                {
                    item.Order = order++;
                }
                DialogHost.CloseDialogCommand.Execute(new StoreManagement { Materials = CurrentStoreMaterials, Suppliers = CurrentStoreSuppliers }, null);
            }
            catch (Exception ex)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, ex.Message);
            }
        }

        private void MaterialSelect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedMaterialMaster != null)
            {
                var material = CurrentStoreMaterials.FirstOrDefault(t => t.MaterialCode == SelectedMaterialMaster.MaterialCode && t.MaterialName == SelectedMaterialMaster.MaterialName);
                if (material == null)
                {
                    if (CurrentAllocation.IsSapBased == false)
                    {
                        noOfMaterialCount = CurrentAllocation.NoOfMaterial;
                        if (noOfMaterialCount >= CurrentStoreMaterials.Count)
                        {
                            if (noOfMaterialCount == CurrentStoreMaterials.Count)
                            {
                                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "You cannot select more Materials than No.Of Material!!!");
                            }
                            else if(noOfMaterialCount != CurrentStoreMaterials.Count)
                            {
                                CurrentStoreMaterials.Add(new StoreMaterialManagement { AllocationId = CurrentAllocation.AllocationId, MaterialCode = SelectedMaterialMaster.MaterialCode, MaterialName = SelectedMaterialMaster.MaterialName });
                                SelectedMaterialsDataGrid.ItemsSource = CurrentStoreMaterials;
                                SelectedMaterialsDataGrid.Items.Refresh();
                            }
                            
                        }                        
                    }
                    else
                    {
                        CurrentStoreMaterials.Add(new StoreMaterialManagement { AllocationId = CurrentAllocation.AllocationId, MaterialCode = SelectedMaterialMaster.MaterialCode, MaterialName = SelectedMaterialMaster.MaterialName });
                        SelectedMaterialsDataGrid.ItemsSource = CurrentStoreMaterials;
                        SelectedMaterialsDataGrid.Items.Refresh();
                    }
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Material already exists!!!");
                }                    
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a material from master!!!");
            }
        }

        private void MaterialUnselect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedStoreMaterial != null)
            {
                CurrentStoreMaterials.Remove(SelectedStoreMaterial);
                SelectedMaterialsDataGrid.ItemsSource = CurrentStoreMaterials;
                SelectedMaterialsDataGrid.Items.Refresh();
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a material!!!");
            }
        }

        private void MaterialClear_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CurrentStoreMaterials = new List<StoreMaterialManagement>();
            SelectedMaterialsDataGrid.ItemsSource = CurrentStoreMaterials;
            SelectedMaterialsDataGrid.Items.Refresh();
        }

        private void MaterialUp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!CurrentAllocation.IsSapBased)
            {
                if (SelectedStoreMaterial != null)
                {
                    if (SelectedStoreMaterial.Closed.HasValue && SelectedStoreMaterial.Closed.Value)
                    {                        
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Selected Material Closed");
                    }
                    else
                    {
                        var indexForStoreMaterials = CurrentStoreMaterials.IndexOf(SelectedStoreMaterial);
                        SelectedStoreSupplier = CurrentStoreSuppliers[indexForStoreMaterials];
                        var indexForStoreSuppliers = indexForStoreMaterials;
                        if (indexForStoreMaterials >= 0)
                        {
                            if (indexForStoreMaterials == 0)
                                return;
                            else if (indexForStoreMaterials <= maxClosedIndex)
                            {
                                return;
                                //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Action not allowed");
                            }
                            else
                            {
                                var fElement = CurrentStoreMaterials[indexForStoreMaterials - 1];
                                CurrentStoreMaterials[indexForStoreMaterials - 1] = SelectedStoreMaterial;
                                CurrentStoreMaterials[indexForStoreMaterials] = fElement;

                                var gElement = CurrentStoreSuppliers[indexForStoreSuppliers - 1];
                                CurrentStoreSuppliers[indexForStoreSuppliers - 1] = SelectedStoreSupplier;
                                CurrentStoreSuppliers[indexForStoreSuppliers] = gElement;
                            }
                        }
                        SelectedMaterialsDataGrid.ItemsSource = CurrentStoreMaterials;
                        SelectedMaterialsDataGrid.Items.Refresh();
                        SelectedSuppliersDataGrid.ItemsSource = CurrentStoreSuppliers;
                        SelectedSuppliersDataGrid.Items.Refresh();
                    }                    
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a Material!!!");
                }
            }
            else
            {
                if (SelectedStoreMaterial != null)
                {
                    if (SelectedStoreMaterial.Closed.HasValue && SelectedStoreMaterial.Closed.Value)
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Selected Material Closed");
                    }
                    else
                    {
                        var index = CurrentStoreMaterials.IndexOf(SelectedStoreMaterial);
                        if (index >= 0)
                        {
                            if (index == 0)
                                return;
                            else if (index <= maxClosedIndex)
                            {
                                return;
                                //CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Action not allowed");
                            }
                            else
                            {
                                var fElement = CurrentStoreMaterials[index - 1];
                                CurrentStoreMaterials[index - 1] = SelectedStoreMaterial;
                                CurrentStoreMaterials[index] = fElement;
                            }
                        }
                        SelectedMaterialsDataGrid.ItemsSource = CurrentStoreMaterials;
                        SelectedMaterialsDataGrid.Items.Refresh();
                        SapItemNumberDataGrid.ItemsSource = CurrentStoreMaterials;
                        SapItemNumberDataGrid.Items.Refresh();
                    }
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a Material!!!");
                }
            }            
        }

        private void MaterialDown_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!CurrentAllocation.IsSapBased)
            {
                if (SelectedStoreMaterial != null)
                {
                    if (SelectedStoreMaterial.Closed.HasValue && SelectedStoreMaterial.Closed.Value)
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Selected Material already Closed");
                    }
                    else
                    {
                        var indexForStoreMaterials = CurrentStoreMaterials.IndexOf(SelectedStoreMaterial);
                        SelectedStoreSupplier = CurrentStoreSuppliers[indexForStoreMaterials];
                        var indexForStoreSuppliers = indexForStoreMaterials;
                        if (indexForStoreMaterials >= 0)
                        {
                            if (indexForStoreMaterials == CurrentStoreMaterials.Count - 1)
                                return;
                            else
                            {
                                var fElement = CurrentStoreMaterials[indexForStoreMaterials + 1];
                                CurrentStoreMaterials[indexForStoreMaterials + 1] = SelectedStoreMaterial;
                                CurrentStoreMaterials[indexForStoreMaterials] = fElement;
                            }
                        }
                        SelectedMaterialsDataGrid.ItemsSource = CurrentStoreMaterials;
                        SelectedMaterialsDataGrid.Items.Refresh();
                        if (indexForStoreSuppliers >= 0)
                        {
                            if (indexForStoreSuppliers == CurrentStoreSuppliers.Count - 1)
                                return;
                            else
                            {
                                var fElement = CurrentStoreSuppliers[indexForStoreSuppliers + 1];
                                CurrentStoreSuppliers[indexForStoreSuppliers + 1] = SelectedStoreSupplier;
                                CurrentStoreSuppliers[indexForStoreSuppliers] = fElement;
                            }
                        }
                        SelectedSuppliersDataGrid.ItemsSource = CurrentStoreSuppliers;
                        SelectedSuppliersDataGrid.Items.Refresh();
                    }                        
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a Material!!!");
                }
            }
            else
            {
                if (SelectedStoreMaterial != null)
                {
                    if (SelectedStoreMaterial.Closed.HasValue && SelectedStoreMaterial.Closed.Value)
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Selected Material Closed");
                    }
                    else
                    {
                        var index = CurrentStoreMaterials.IndexOf(SelectedStoreMaterial);
                        if (index >= 0)
                        {
                            if (index == CurrentStoreMaterials.Count - 1)
                                return;
                            else
                            {
                                var fElement = CurrentStoreMaterials[index + 1];
                                CurrentStoreMaterials[index + 1] = SelectedStoreMaterial;
                                CurrentStoreMaterials[index] = fElement;
                            }
                        }
                        SelectedMaterialsDataGrid.ItemsSource = CurrentStoreMaterials;
                        SelectedMaterialsDataGrid.Items.Refresh();
                        SapItemNumberDataGrid.ItemsSource = CurrentStoreMaterials;
                        SapItemNumberDataGrid.Items.Refresh();
                    }
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a Material!!!");
                }
            }            
        }

        private void SupplierSelect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedSupplierMaster != null)
            {
                //var Supplier = CurrentStoreSuppliers.FirstOrDefault(t => t.SupplierCode == SelectedSupplierMaster.SupplierCode && t.SupplierName == SelectedSupplierMaster.SupplierName);
                //if (Supplier == null)
                //{
                //    CurrentStoreSuppliers.Add(new StoreSupplierManagement { AllocationId=CurrentAllocation.AllocationId,SupplierCode= SelectedSupplierMaster.SupplierCode,SupplierName= SelectedSupplierMaster.SupplierName});
                //    SelectedSuppliersDataGrid.ItemsSource = CurrentStoreSuppliers;
                //    SelectedSuppliersDataGrid.Items.Refresh();
                //}
                //else
                //    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Supplier already exists!!!");
                if (CurrentAllocation.IsSapBased == false)
                {                    
                    if (noOfMaterialCount >= CurrentStoreSuppliers.Count)
                    {
                        if (noOfMaterialCount == CurrentStoreSuppliers.Count)
                        {
                            CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowError, "Material Count and Supplier Count not matching");
                        }
                        else if (noOfMaterialCount != CurrentStoreSuppliers.Count)
                        {
                            CurrentStoreSuppliers.Add(new StoreSupplierManagement { AllocationId = CurrentAllocation.AllocationId, SupplierCode = SelectedSupplierMaster.SupplierCode, SupplierName = SelectedSupplierMaster.SupplierName });
                            SelectedSuppliersDataGrid.ItemsSource = CurrentStoreSuppliers;
                            SelectedSuppliersDataGrid.Items.Refresh();
                        }
                    }
                }
                else
                {
                    CurrentStoreSuppliers.Add(new StoreSupplierManagement { AllocationId = CurrentAllocation.AllocationId, SupplierCode = SelectedSupplierMaster.SupplierCode, SupplierName = SelectedSupplierMaster.SupplierName });
                    SelectedSuppliersDataGrid.ItemsSource = CurrentStoreSuppliers;
                    SelectedSuppliersDataGrid.Items.Refresh();
                }
                
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a supplier from master!!!");
            }
        }

        private void SupplierUnselect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedStoreSupplier != null)
            {
                CurrentStoreSuppliers.Remove(SelectedStoreSupplier);
                SelectedSuppliersDataGrid.ItemsSource = CurrentStoreMaterials;
                SelectedSuppliersDataGrid.Items.Refresh();
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a supplier!!!");
            }
        }

        private void SupplierClear_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CurrentStoreSuppliers = new List<StoreSupplierManagement>();
            SelectedSuppliersDataGrid.ItemsSource = CurrentStoreMaterials;
            SelectedSuppliersDataGrid.Items.Refresh();
        }

        private void SupplierUp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CurrentAllocation.IsSapBased == false)
            {
                if (SelectedStoreSupplier != null)
                {
                    if (SelectedStoreSupplier.Closed == true)
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Selected Supplier already Closed");
                    }
                    else if (SelectedStoreSupplier.Closed == false)
                    {
                        var indexForStoreSuppliers = CurrentStoreSuppliers.IndexOf(SelectedStoreSupplier);
                        SelectedStoreMaterial = CurrentStoreMaterials[indexForStoreSuppliers];
                        var indexForStoreMaterials = indexForStoreSuppliers;
                        if (indexForStoreSuppliers >= 0)
                        {
                            if (indexForStoreSuppliers == 0)
                                return;
                            else
                            {
                                var fElement = CurrentStoreSuppliers[indexForStoreSuppliers - 1];
                                CurrentStoreSuppliers[indexForStoreSuppliers - 1] = SelectedStoreSupplier;
                                CurrentStoreSuppliers[indexForStoreSuppliers] = fElement;
                            }
                        }
                        SelectedSuppliersDataGrid.ItemsSource = CurrentStoreSuppliers;
                        SelectedSuppliersDataGrid.Items.Refresh();

                        if (indexForStoreMaterials >= 0)
                        {
                            if (indexForStoreMaterials == 0)
                                return;
                            else
                            {
                                var fElement = CurrentStoreMaterials[indexForStoreMaterials - 1];
                                CurrentStoreMaterials[indexForStoreMaterials - 1] = SelectedStoreMaterial;
                                CurrentStoreMaterials[indexForStoreMaterials] = fElement;
                            }
                        }
                        SelectedMaterialsDataGrid.ItemsSource = CurrentStoreMaterials;
                        SelectedMaterialsDataGrid.Items.Refresh();
                    }                        
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a Supplier!!!");
                }
            }
            else if (CurrentAllocation.IsSapBased == true)
            {
                if (SelectedStoreSupplier != null)
                {
                    var index = CurrentStoreSuppliers.IndexOf(SelectedStoreSupplier);
                    if (index >= 0)
                    {
                        if (index == 0)
                            return;
                        else
                        {
                            var fElement = CurrentStoreSuppliers[index - 1];
                            CurrentStoreSuppliers[index - 1] = SelectedStoreSupplier;
                            CurrentStoreSuppliers[index] = fElement;
                        }
                    }
                    SelectedSuppliersDataGrid.ItemsSource = CurrentStoreSuppliers;
                    SelectedSuppliersDataGrid.Items.Refresh();
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a Supplier!!!");
                }
            }            
        }

        private void SupplierDown_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CurrentAllocation.IsSapBased == false)
            {
                if (SelectedStoreSupplier != null)
                {
                    if (SelectedStoreSupplier.Closed == true)
                    {
                        CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Selected Supplier already Closed");
                    }
                    else if (SelectedStoreSupplier.Closed == false)
                    {
                        var indexForStoreSuppliers = CurrentStoreSuppliers.IndexOf(SelectedStoreSupplier);
                        SelectedStoreMaterial = CurrentStoreMaterials[indexForStoreSuppliers];
                        var indexForStoreMaterials = indexForStoreSuppliers;
                        if (indexForStoreSuppliers >= 0)
                        {
                            if (indexForStoreSuppliers == CurrentStoreSuppliers.Count - 1)
                                return;
                            else
                            {
                                var fElement = CurrentStoreSuppliers[indexForStoreSuppliers + 1];
                                CurrentStoreSuppliers[indexForStoreSuppliers + 1] = SelectedStoreSupplier;
                                CurrentStoreSuppliers[indexForStoreSuppliers] = fElement;
                            }
                        }
                        SelectedSuppliersDataGrid.ItemsSource = CurrentStoreSuppliers;
                        SelectedSuppliersDataGrid.Items.Refresh();
                        if (indexForStoreMaterials >= 0)
                        {
                            if (indexForStoreMaterials == CurrentStoreMaterials.Count - 1)
                                return;
                            else
                            {
                                var fElement = CurrentStoreMaterials[indexForStoreMaterials + 1];
                                CurrentStoreMaterials[indexForStoreMaterials + 1] = SelectedStoreMaterial;
                                CurrentStoreMaterials[indexForStoreMaterials] = fElement;
                            }
                        }
                        SelectedMaterialsDataGrid.ItemsSource = CurrentStoreMaterials;
                        SelectedMaterialsDataGrid.Items.Refresh();
                    }                        
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a Supplier!!!");
                }
            }
            else if (CurrentAllocation.IsSapBased == true)
            {
                if (SelectedStoreSupplier != null)
                {
                    var index = CurrentStoreSuppliers.IndexOf(SelectedStoreSupplier);
                    if (index >= 0)
                    {
                        if (index == CurrentStoreSuppliers.Count - 1)
                            return;
                        else
                        {
                            var fElement = CurrentStoreSuppliers[index + 1];
                            CurrentStoreSuppliers[index + 1] = SelectedStoreSupplier;
                            CurrentStoreSuppliers[index] = fElement;
                        }
                    }
                    SelectedSuppliersDataGrid.ItemsSource = CurrentStoreSuppliers;
                    SelectedSuppliersDataGrid.Items.Refresh();
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a Supplier!!!");
                }
            }            
        }

        private void MaterialMasterDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedMaterialMaster = ((DataGrid)sender).SelectedItem as MaterialMaster;
        }

        private void SelectedMaterialsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedStoreMaterial = ((DataGrid)sender).SelectedItem as StoreMaterialManagement;            
            var row = SelectedMaterialsDataGrid.ItemContainerGenerator.ContainerFromItem(SelectedStoreMaterial) as DataGridRow;            
            if (SelectedStoreMaterial.Closed == true)
            {
                row.Background = Brushes.GreenYellow;
            }
            else if (SelectedStoreMaterial.Closed == false)
            {
                row.Background = Brushes.White;
            }
        }        

        private void SupplierMasterDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedSupplierMaster = ((DataGrid)sender).SelectedItem as SupplierMaster;
        }

        private void SelectedSuppliersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedStoreSupplier = ((DataGrid)sender).SelectedItem as StoreSupplierManagement;
        }        
    }
}
