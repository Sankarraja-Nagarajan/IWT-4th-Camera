using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperSimpleTcp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net.NetworkInformation;
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
using System.Windows.Threading;

namespace IWT.Views
{
    public partial class Master : UserControl
    {
        private readonly ToastViewModel toastViewModel;
        public static CommonFunction commonFunction = new CommonFunction();
        string LastMessage;
        AdminDBCall _dbContext;
        private List<TicketDataTemplate> AllCustomFields = new List<TicketDataTemplate>();
        private List<CustomMastereBuilder> CustomMasterCollection = new List<CustomMastereBuilder>();
        private Button currentTab = new Button();
        private Button previousTab = new Button();
        private List<int> zIndexArr = new List<int>();
        public static event EventHandler<WeighmentEventArgs> onWeighmentReceived = delegate { };
        WeighbridgeSettings weighbridgeSetting = new WeighbridgeSettings();
        public string TCPServerAddress = "127.0.0.1";
        public int TCPServerPort = 4002;
        public SimpleTcpClient TCP_Client;
        RolePriviliege rolePriviliege;
        string role;
        string weghsoftApp = ConfigurationManager.AppSettings["WeighmentApplicationPath"].ToString();
        public static event EventHandler<UserControlEventArgs> onMasterWindowLoaded = delegate { };
        public Master(string Role, RolePriviliege _rolePriviliege)
        {
            InitializeComponent();

            toastViewModel = new ToastViewModel();
            role = Role;
            rolePriviliege = _rolePriviliege;
            _dbContext = new AdminDBCall();
            GetWeighbridgeSettings();
            MainWindow.onWeighmentReceived += Single_onWeighmentReceived;
            //TCP_Client = new SimpleTcpClient(TCPServerAddress, TCPServerPort);
            //TCP_Client.Events.Connected += Connected;
            //TCP_Client.Events.Disconnected += Disconnected;
            //TCP_Client.Events.DataReceived += DataReceived;
            Loaded += Master_Loaded;
            Unloaded += Master_Unloaded;
            //MainWindow.onMainWindowButtonClick += MainWindow_onMainWindowButtonClick;
            //InitializeTCP();
        }

        //private void MainWindow_onMainWindowButtonClick(object sender, MainWindowButtonEventArgs e)
        //{
        //    if (e.ButtonName == "ZERO")
        //    {
        //        RezeroWeight();
        //    }
        //    else if (e.ButtonName == "SHOW")
        //    {
        //        ShowWeighApp();
        //    }
        //    else if (e.ButtonName == "CONNECT")
        //    {
        //        new Task(() => {
        //            OpenWeighSoftApplication();
        //        }).Start();
        //    }
        //}
        //public async void RezeroWeight()
        //{
        //    if (TCP_Client.IsConnected)
        //    {
        //        await TCP_Client.SendAsync("<ZERO>");
        //    }
        //}
        //public async void ShowWeighApp()
        //{
        //    if (TCP_Client.IsConnected)
        //    {
        //        await TCP_Client.SendAsync("<VISIBLE>");
        //    }
        //}
        //public async void HideWeighApp()
        //{
        //    if (TCP_Client.IsConnected)
        //    {
        //        await TCP_Client.SendAsync("<HIDE>");
        //    }
        //}
        public void GetWeighbridgeSettings()
        {
            try
            {
                weighbridgeSetting = commonFunction.GetWeighbridgeSettings();
                if (weighbridgeSetting != null)
                {
                    TCPServerAddress = weighbridgeSetting.Host;
                    TCPServerPort = weighbridgeSetting.Port;
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Master/GetWeighbridgeSettings/Exception:- " + ex.Message, ex);
            }
        }
        private void OpenWeighSoftApplication()
        {
            try
            {
                Process p = new Process();
                ProcessStartInfo s = new ProcessStartInfo(weghsoftApp)
                {
                    UseShellExecute = true
                };
                p.StartInfo = s;
                s.CreateNoWindow = true;
                s.WindowStyle = ProcessWindowStyle.Minimized;
                p.Start();
                //Thread.Sleep(2000);
                Task.Run(() =>
                {
                    InitializeTCP();
                });
            }
            catch (Exception ex)
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, ex.Message);
                WriteLog.WriteToFile("MainWindow/OpenWeighSoftApplication/Exception:- " + ex.Message, ex);
            }
        }
        private void Master_Loaded(object sender, RoutedEventArgs e)
        {
            onMasterWindowLoaded.Invoke(this, new UserControlEventArgs());
            //new Task(() =>
            //{
            //    InitializeTCP();
            //}).Start();
            ConstructCustomMasters();
            BuildMasters();
        }

        private void Master_Unloaded(object sender, RoutedEventArgs e)
        {
            //TCP_Client.Events.Connected -= Connected;
            //TCP_Client.Events.Disconnected -= Disconnected;
            //TCP_Client.Events.DataReceived -= DataReceived;
            //if (TCP_Client.IsConnected)
            //{
            //    TCP_Client.Disconnect();
            //    TCP_Client.Dispose();
            //}
        }
        private void InitializeTCP()
        {
            try
            {
                Ping pinger = new Ping();
                PingReply reply = pinger.Send($"{TCPServerAddress}");
                if (reply.Status == IPStatus.Success)
                {
                    TCP_Client.Connect();
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void Connected(object sender, ConnectionEventArgs e)
        {
            WriteLog.WriteToFile($"Master/InitializeTCP :- Server {e.IpPort} connected!!");
            ////Debug.WriteLine($"*** Server {e.IpPort} connected");
        }

        public void Disconnected(object sender, ConnectionEventArgs e)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                WeighmentLabel.Text = "Error";
            }));
            WriteLog.WriteToFile($"Master/InitializeTCP :- Server {e.IpPort} disconnected!!");
            ////Debug.WriteLine($"*** Server {e.IpPort} disconnected");
        }

        public void DataReceived(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            ////Debug.WriteLine($"[{e.IpPort}] {Encoding.UTF8.GetString(e.Data)}");
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                var weight = Encoding.UTF8.GetString(e.Data);
                string result = Regex.Match(weight, @"-?\d+").Value;
                WeighmentLabel.Text = result;
                onWeighmentReceived.Invoke(sender, new WeighmentEventArgs(result));
            }));
        }
        private void Single_onWeighmentReceived(object sender, WeighmentEventArgs e)
        {
            var we = e._weight as string;
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { WeighmentLabel.Text = we; }));
        }
        public void ConstructCustomMasters()
        {
            List<CustomMasterList> MasterLists = new List<CustomMasterList>();
            if (rolePriviliege.MaterialMasterAccess.HasValue && rolePriviliege.MaterialMasterAccess.Value)
            {
                MasterLists.Add(new CustomMasterList { CutomMasterName = "Material_Master", TableImage = @"/Assets/Icons/eMaterial.png", TableText = "Material Master" });

            }
            if (rolePriviliege.SupllierMasterAccess.HasValue && rolePriviliege.SupllierMasterAccess.Value)
            {
                MasterLists.Add(new CustomMasterList { CutomMasterName = "Supplier_Master", TableImage = @"/Assets/Icons/eSupplier.png", TableText = "Supplier Master" });

            }
            if (rolePriviliege.VehicleMasterAccess.HasValue && rolePriviliege.VehicleMasterAccess.Value)
            {
                MasterLists.Add(new CustomMasterList { CutomMasterName = "Vehicle_Master", TableImage = @"/Assets/Icons/VechicleEdge.png", TableText = "Vehicle Master" });

            }
            if (rolePriviliege.ShiftMasterAccess.HasValue && rolePriviliege.ShiftMasterAccess.Value)
            {
                MasterLists.Add(new CustomMasterList { CutomMasterName = "Shift_Master", TableImage = @"/Assets/Icons/eshift master.png", TableText = "Shift Master" });
            }
            if (rolePriviliege.RFIDMasterAccess.HasValue && rolePriviliege.RFIDMasterAccess.Value)
            {
                MasterLists.Add(new CustomMasterList { CutomMasterName = "RFID_Tag_Master", TableImage = @"/Assets/Icons/rfidImage.png", TableText = "RFIDTag Master" });
            }
            if (rolePriviliege.CustomMasterAccess.HasValue && rolePriviliege.CustomMasterAccess.Value)
            {
                List<CustomMasterList> CustomMasterLists = GetAllCustomMasters();
                foreach (var master in CustomMasterLists)
                {
                    MasterLists.Add(master);
                }
            }

            AllCustomFields = GetAllTicktetDataTemplates();
            int index = MasterLists.Count;
            foreach (var table in MasterLists)
            {
                zIndexArr.Add(index--);
                CustomMastereBuilder customMasterBuilder = new CustomMastereBuilder
                {
                    TableName = table.CutomMasterName,
                    TableImage = table.TableImage != null ? table.TableImage : @"/Assets/Icons/Custom_Field.png",
                    TableText = table.TableText != null ? table.TableText : table.CutomMasterName,
                    Fields = new List<CustomFieldBuilder>()
                };
                if (table.CutomMasterName == "Material_Master")
                {
                    customMasterBuilder.Fields.Add(new CustomFieldBuilder { FieldName = "MaterialCode", FieldCaption = "Material Code", FieldType = "NVARCHAR", ControlType = "TextBox", RegName = "MaterialCode", FieldImage = @"/Assets/Icons/eMaterial.png", IsMandatory = true });
                    customMasterBuilder.Fields.Add(new CustomFieldBuilder { FieldName = "MaterialName", FieldCaption = "Material Name", FieldType = "NVARCHAR", ControlType = "TextBox", RegName = "MaterialName", FieldImage = @"/Assets/Icons/eMaterial.png", IsMandatory = true });
                }
                else if (table.CutomMasterName == "Supplier_Master")
                {
                    customMasterBuilder.Fields.Add(new CustomFieldBuilder { FieldName = "SupplierCode", FieldCaption = "Supplier Code", FieldType = "NVARCHAR", ControlType = "TextBox", RegName = "SupplierCode", FieldImage = @"/Assets/Icons/eSupplier.png", IsMandatory = true });
                    customMasterBuilder.Fields.Add(new CustomFieldBuilder { FieldName = "SupplierName", FieldCaption = "Supplier Name", FieldType = "NVARCHAR", ControlType = "TextBox", RegName = "SupplierName", FieldImage = @"/Assets/Icons/eSupplier.png", IsMandatory = true });
                }
                else if (table.CutomMasterName == "Vehicle_Master")
                {
                    customMasterBuilder.Fields.Add(new CustomFieldBuilder { FieldName = "VehicleNumber", FieldCaption = "Vehicle Number", FieldType = "NVARCHAR", ControlType = "TextBox", RegName = "VehicleNumber", FieldImage = @"/Assets/Icons/Vehicle_Number.png", IsMandatory = true });
                    customMasterBuilder.Fields.Add(new CustomFieldBuilder { FieldName = "VehicleTareWeight", FieldCaption = "Vehicle Tare Weight", FieldType = "NVARCHAR", ControlType = "TextBox", RegName = "VehicleTareWeight", FieldImage = @"/Assets/Icons/Transactions.png", IsEnabled = false, IsMandatory = true });
                    customMasterBuilder.Fields.Add(new CustomFieldBuilder { FieldName = "Expiry", FieldCaption = "Expiry", FieldType = "DATETIME", ControlType = "TextBox", RegName = "Expiry", FieldImage = @"/Assets/Icons/Transactions.png", IsMandatory = true });
                    customMasterBuilder.Fields.Add(new CustomFieldBuilder { FieldName = "TaredTime", FieldCaption = "Tared Time", FieldType = "NVARCHAR", ControlType = "TextBox", RegName = "TaredTime", FieldImage = @"/Assets/Icons/Transactions.png", IsEnabled = false, IsMandatory = true });
                }
                else if (table.CutomMasterName == "Shift_Master")
                {
                    customMasterBuilder.Fields.Add(new CustomFieldBuilder { FieldName = "ShiftName", FieldCaption = "Shift Name", FieldType = "NVARCHAR", ControlType = "TextBox", RegName = "ShiftName", FieldImage = @"/Assets/Icons/eshift master.png", IsMandatory = true });
                    customMasterBuilder.Fields.Add(new CustomFieldBuilder { FieldName = "FromShift", FieldCaption = "From Shift", FieldType = "NVARCHAR", ControlType = "Timepicker", RegName = "FromShift", FieldImage = @"/Assets/Icons/clock-new.png", IsMandatory = true });
                    customMasterBuilder.Fields.Add(new CustomFieldBuilder { FieldName = "ToShift", FieldCaption = "To Shift", FieldType = "NVARCHAR", ControlType = "Timepicker", RegName = "ToShift", FieldImage = @"/Assets/Icons/clock-new.png", IsMandatory = true });
                }
                else if (table.CutomMasterName == "RFID_Tag_Master")
                {
                //    customMasterBuilder.Fields.Add(new CustomFieldBuilder { FieldName = "ShiftName", FieldCaption = "Shift Name", FieldType = "NVARCHAR", ControlType = "TextBox", RegName = "ShiftName", FieldImage = @"/Assets/Icons/eshift master.png", IsMandatory = true });
                //    customMasterBuilder.Fields.Add(new CustomFieldBuilder { FieldName = "FromShift", FieldCaption = "From Shift", FieldType = "NVARCHAR", ControlType = "Timepicker", RegName = "FromShift", FieldImage = @"/Assets/Icons/clock-new.png", IsMandatory = true });
                //    customMasterBuilder.Fields.Add(new CustomFieldBuilder { FieldName = "ToShift", FieldCaption = "To Shift", FieldType = "NVARCHAR", ControlType = "Timepicker", RegName = "ToShift", FieldImage = @"/Assets/Icons/clock-new.png", IsMandatory = true });
                }
                List<TicketDataTemplate> customFieldTemplates = AllCustomFields.Where(t => t.F_Table == table.CutomMasterName).ToList();
                foreach (var field in customFieldTemplates)
                {
                    CustomFieldBuilder customFieldBuilder = new CustomFieldBuilder();
                    customFieldBuilder.FieldTable = field.F_Table;
                    customFieldBuilder.FieldName = field.F_FieldName;
                    customFieldBuilder.FieldCaption = field.F_Caption;
                    customFieldBuilder.FieldType = field.F_Type;
                    customFieldBuilder.ControlType = field.ControlType;
                    customFieldBuilder.ControlTable = field.ControlTable;
                    customFieldBuilder.SelectionBasis = field.SelectionBasis;
                    customFieldBuilder.RegName = table.CutomMasterName + "_" + field.F_FieldName;
                    customFieldBuilder.FieldImage = @"/Assets/Icons/Custom_Field.png";
                    customFieldBuilder.IsMandatory = field.Mandatory;
                    int size = 0;
                    int.TryParse(field.F_Size, out size);
                    customFieldBuilder.FieldSize = size;
                    customMasterBuilder.Fields.Add(customFieldBuilder);
                }
                CustomMasterCollection.Add(customMasterBuilder);
            }
        }
        public List<CustomMasterList> GetAllCustomMasters()
        {
            try
            {
                DataTable table = _dbContext.GetAllData($"SELECT * FROM Custom_Master_List");
                string JSONString = JsonConvert.SerializeObject(table);
                var result = JsonConvert.DeserializeObject<List<CustomMasterList>>(JSONString);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetAllCustomMasters:" + ex.Message);
                return new List<CustomMasterList>();
            }
        }
        public List<TicketDataTemplate> GetAllTicktetDataTemplates()
        {
            try
            {
                AdminDBCall _dbContext = new AdminDBCall();
                DataTable table = _dbContext.GetAllData($"select * from Ticket_Data_Template");
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
        public Button CreateTabButton(CustomMastereBuilder master)
        {
            BrushConverter bc = new BrushConverter();
            Button tabBtn = new Button
            {
                Margin = new Thickness(0, 0, -70, 0),
                Background = (Brush)bc.ConvertFrom("#BBC1D1"),
                BorderBrush = (Brush)bc.ConvertFrom("#BBC1D1"),
                BorderThickness = new Thickness(2),
                Style = (Style)FindResource("BtnStyle"),
                Name = master.TableName + "Btn"
            };
            tabBtn.Click += TabBtn_Click;
            StackPanel btnContent = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Image btnImage = new Image
            {
                Source = new BitmapImage(new Uri(master.TableImage, UriKind.Relative)),
                Width = 40,
                Height = 40
            };
            TextBlock btnText = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 14,
                Text = master.TableText,
                FontFamily = new FontFamily("Segoe UI Semibold")
            };
            btnContent.Children.Add(btnImage);
            btnContent.Children.Add(btnText);
            tabBtn.Content = btnContent;
            return tabBtn;
        }

        private void TabBtn_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();
            currentTab = sender as Button;
            previousTab.Background = (Brush)bc.ConvertFrom("#BBC1D1");
            previousTab.Foreground = new SolidColorBrush(Colors.Black);
            var previousTabBorder = (previousTab.Template?.FindName("border_color", previousTab) as Border);
            if (previousTabBorder != null)
            {
                previousTabBorder.BorderThickness = new Thickness(0);
            }
            previousTab = currentTab;
            currentTab.Background = (Brush)bc.ConvertFrom("#FFAA33");
            currentTab.Foreground = new SolidColorBrush(Colors.White);
            (currentTab.Template.FindName("border_color", currentTab) as Border).BorderThickness = new Thickness(2);
            SetZIndex(GetZIndexArray(zIndexArr, int.Parse(currentTab.Uid)));
            WeightMonitor.Visibility = Visibility.Collapsed;
            if (currentTab.Name == "Material_MasterBtn")
            {
                CustomMastereBuilder master = CustomMasterCollection.FirstOrDefault(t => t.TableName == "Material_Master");
                Main.Content = new MaterialMastereUserControl(master.Fields);
            }
            else if (currentTab.Name == "Supplier_MasterBtn")
            {
                CustomMastereBuilder master = CustomMasterCollection.FirstOrDefault(t => t.TableName == "Supplier_Master");
                Main.Content = new SupplierMasterUserControl(master.Fields);
            }
            else if (currentTab.Name == "Vehicle_MasterBtn")
            {
                WeightMonitor.Visibility = Visibility.Visible;
                CustomMastereBuilder master = CustomMasterCollection.FirstOrDefault(t => t.TableName == "Vehicle_Master");
                Main.Content = new VehicleMasterUserControl(master.Fields);
            }
            else if (currentTab.Name == "Shift_MasterBtn")
            {
                CustomMastereBuilder master = CustomMasterCollection.FirstOrDefault(t => t.TableName == "Shift_Master");
                Main.Content = new ShiftMasterUserControl(master.Fields);
            }
            else if (currentTab.Name == "RFID_Tag_MasterBtn")
            {
                CustomMastereBuilder master = CustomMasterCollection.FirstOrDefault(t => t.TableName == "RFID_Tag_Master");
                Main.Content = new RFIDTagMasterControl(role, rolePriviliege);
            }
            else
            {
                string tableName = CustomMasterCollection[int.Parse(currentTab.Uid) - 1].TableName;
                CustomMastereBuilder builder = CustomMasterCollection.FirstOrDefault(t => t.TableName == tableName);
                Main.Content = new UserControlCustomMaster(builder);
            }
        }

        public void BuildMasters()
        {
            for (int i = 1; i <= CustomMasterCollection.Count; i++)
            {
                Button tabItem = CreateTabButton(CustomMasterCollection[i - 1]);
                tabItem.Uid = i.ToString();
                RegisterName("tab_" + CustomMasterCollection[i - 1]?.TableName, tabItem);
                TabContainer.Children.Add(tabItem);
            }
            var bc = new BrushConverter();
            Button firstTab = (Button)FindName("tab_" + CustomMasterCollection[0].TableName);
            firstTab.Background = (Brush)bc.ConvertFrom("#FFAA33");
            firstTab.Foreground = new SolidColorBrush(Colors.White);
            previousTab = firstTab;
            SetZIndex(GetZIndexArray(zIndexArr, 1));
            CustomMastereBuilder master = CustomMasterCollection.FirstOrDefault();
            if (master != null)
            {
                if (master.TableName == "Material_Master")
                {
                    Main.Content = new MaterialMastereUserControl(master.Fields);
                }
                else if (master.TableName == "Supplier_Master")
                {
                    Main.Content = new SupplierMasterUserControl(master.Fields);
                }
                else if (master.TableName == "Vehicle_Master")
                {
                    Main.Content = new VehicleMasterUserControl(master.Fields);
                }
                else if (master.TableName == "Shift_Master")
                {
                    Main.Content = new ShiftMasterUserControl(master.Fields);
                }
            }
        }
        public List<int> GetZIndexArray(List<int> arr, int position)
        {
            var arr1 = new List<int>();
            for (int i = position - 1; i >= 0; i--)
            {
                arr1.Add(arr[i]);
            }
            for (int i = position; i < arr.Count; i++)
            {
                arr1.Add(arr[i]);
            }
            return arr1;
        }
        public void SetZIndex(List<int> arr)
        {
            for (int i = 0; i < CustomMasterCollection.Count; i++)
            {
                Button tabBtn = (Button)FindName("tab_" + CustomMasterCollection[i].TableName);
                if (tabBtn != null)
                    Panel.SetZIndex(tabBtn, arr[i]);
            }
        }
    }
}
