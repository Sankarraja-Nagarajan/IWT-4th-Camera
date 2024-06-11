using IWT.DBCall;
using IWT.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
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
    /// Interaction logic for Export_Import.xaml
    /// </summary>
    public partial class Export_Import : Page
    {
        AdminDBCall adminDBCall = new AdminDBCall();
        string connectionString = "";
        string databaseName = ConfigurationManager.AppSettings["DatabaseName"].ToString();
        ScriptingOptions Options = new ScriptingOptions();
        private const string NoDataScript = "Cars";
        private string ExportFileName;
        private string ImportFileName;
        public Export_Import()
        {
            InitializeComponent();
            connectionString = adminDBCall.GetDecryptedConnectionStringDB();
        }
        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                ImportFileName = Log.Text = openFileDialog.FileName;
            ImportBtn.IsEnabled = !string.IsNullOrEmpty(ImportFileName);


        }
        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(ImportFileName))
                {
                    FileInfo file = new FileInfo(Log.Text);
                    string script1 = file.OpenText().ReadToEnd();
                    //SqlConnection conn = new SqlConnection(connectionString);
                    SqlConnection conn = new SqlConnection("server=DESKTOP-30IUE2U;database=IWT-Desktop;user=sa;password=Exalca@123;");
                    Server server1 = new Server(new ServerConnection(conn));
                    server1.ConnectionContext.ExecuteNonQuery(script1);
                    conn.Close();
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, $"Database imported successfully from the selected file {file.Name}");
                    ClearImportOptions();
                }
                else
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a file to import");
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Export_Import:- ImportBtn_Click - Inserted Successfully ");
            }

        }

        private void btnOpenFile1_Click(object sender, RoutedEventArgs e)
        {
            var openFolderDialog2 = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var result = openFolderDialog2.ShowDialog();
            if (!string.IsNullOrEmpty(result?.ToString()))
            {
                Ireport.Text = openFolderDialog2.SelectedPath;
                ExportFileName = Ireport.Text + "/" + databaseName + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".sql";
            }
            ExportBtn.IsEnabled = !string.IsNullOrEmpty(Ireport.Text);

        }

        private async void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Ireport.Text))
            {
                //Task.Run(() =>
                //{
                //    ExportDBTOFolder();
                //});
                new System.Threading.Thread(() =>
                {
                    ExportDBTOFolder();
                }).Start();
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please select a folder to export");
            }
        }


        private void ExportDBTOFolder()
        {

            var script = new StringBuilder();

            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(async() =>
            {
                try
                {
                    //using (SqlConnection con = )
                    //{
                    //    con.Open();
                    Server server = new Server(new ServerConnection(new SqlConnection(connectionString)));
                    //server.ConnectionContext.ConnectTimeout = 600;
                    server.ConnectionContext.StatementTimeout = 600;
                    Database database = server.Databases[databaseName];
                    //FileName = Ireport.Text + "/" + databaseName + ".sql";
                    ScriptingOptions options = new ScriptingOptions
                    {
                        //ScriptData = true,
                        //ScriptSchema = true,
                        //ScriptDrops = true,
                        //ScriptDrops = true,
                        Default = true,
                        Indexes = true,
                        ClusteredIndexes = true,
                        NonClusteredIndexes = true,
                        FullTextIndexes = true,
                        SchemaQualify = true,
                        //ScriptForCreateOrAlter = true,
                        ScriptDrops = true,
                        IncludeIfNotExists = true,
                        //ScriptSchema =true,
                        //ScriptForCreateDrop = true,
                        //ScriptForCreateOrAlter = true,
                        //ScriptData = true,
                        Statistics = true,
                        Triggers = true,
                        WithDependencies = true,
                        DriAll = true,
                        //ScriptForCreateDrop =true,
                        IncludeHeaders = true,

                    };
                    //SetupDropOptions();

                    foreach (Microsoft.SqlServer.Management.Smo.Table table in database.Tables)
                    {
                        foreach (var statement in table.EnumScript(options))
                        {
                            script.Append(statement);
                            script.Append(Environment.NewLine);
                        }
                    }

                    //SetupCreateOptions();

                    options.ScriptDrops = false;
                    options.ScriptForCreateDrop=true;
                    options.ScriptData = true;
                    

                    foreach (Microsoft.SqlServer.Management.Smo.Table table in database.Tables)
                    {
                        foreach (var statement in table.EnumScript(options))
                        {
                            script.Append(statement);
                            script.Append(Environment.NewLine);
                        }
                    }
                    //foreach (StoredProcedure sp in database.StoredProcedures)
                    //{
                    //    script.Append(sp);
                    //}

                    List<SqlSmoObject> list = new List<SqlSmoObject>();
                    DataTable dataTable = database.EnumObjects(DatabaseObjectTypes.StoredProcedure);
                    foreach (DataRow row in dataTable.Rows)
                    {
                        string sSchema = (string)row["Schema"];
                        if (sSchema == "sys" || sSchema == "INFORMATION_SCHEMA")
                            continue;
                        StoredProcedure sp = (StoredProcedure)server.GetSmoObject(
                           new Urn((string)row["Urn"]));
                        if (!sp.IsSystemObject)
                            list.Add(sp);
                    }
                    foreach (SqlSmoObject obj in list)
                    {
                        var obj1 = obj as StoredProcedure;
                        foreach (var statement in obj1.Script(Options))
                        {
                            script.Append(statement);
                            script.Append(Environment.NewLine);
                        }
                    }

                    //foreach (Microsoft.SqlServer.Management.Smo.Table table in database.Tables)
                    //{
                    //    if (!table.IsSystemObject)
                    //    {
                    //        if (NoDataScript.Contains(table.Name))
                    //        {
                    //            Options.ScriptData = false;
                    //            table.EnumScript(Options);
                    //            Options.ScriptData = true;
                    //        }
                    //        else
                    //            table.EnumScript(Options);
                    //    }
                    //}

                    File.WriteAllText(ExportFileName, script.ToString());
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Database exported successfully to the folder");
                    ClearExportOptions();
                    //}

                }
                catch (Exception ex)
                {
                    WriteLog.WriteToFile("Export_Import:- ExportDBTOFolder - Inserted Successfully ");

                }

            }));
        }

        private void ClearImportOptions()
        {
            ImportBtn.IsEnabled = false;
            Log.Text = "";
            ImportFileName = "";
        }

        private void ClearExportOptions()
        {
            ExportBtn.IsEnabled = false;
            Ireport.Text = "";
            ExportFileName = "";
        }




        private void SetupDropOptions()
        {
            Options = new ScriptingOptions
            {
                //ScriptData = true,
                //ScriptSchema = true,
                //ScriptDrops = true,
                //ScriptDrops = true,
                Default = true,
                Indexes = true,
                ClusteredIndexes = true,
                NonClusteredIndexes = true,
                FullTextIndexes = true,
                SchemaQualify = true,
                //ScriptForCreateOrAlter = true,
                ScriptDrops = true,
                IncludeIfNotExists = true,
                //ScriptSchema =true,
                //ScriptForCreateDrop = true,
                //ScriptForCreateOrAlter = true,
                //ScriptData = true,
                Statistics = true,
                Triggers = true,
                WithDependencies = true,
                DriAll = true,
                //ScriptForCreateDrop =true,
                IncludeHeaders = true,
            };
        }

        private void SetupCreateOptions()
        {
            Options = new ScriptingOptions
            {
                //ScriptData = true,
                //ScriptSchema = true,
                //ScriptDrops = true,
                //ScriptDrops = true,
                Default = true,
                Indexes = true,
                ClusteredIndexes = true,
                NonClusteredIndexes = true,
                FullTextIndexes = true,
                SchemaQualify = true,
                //ScriptForCreateOrAlter = true,
                //ScriptDrops = true,
                IncludeIfNotExists = true,
                ScriptSchema = true,
                ScriptForCreateDrop = true,
                //ScriptForCreateOrAlter = true,
                ScriptData = true,
                Statistics = true,
                Triggers = true,
                WithDependencies = true,
                DriAll = true,
                //ScriptForCreateDrop =true,
                IncludeHeaders = true,
            };
        }

        private void btnOpenFile2_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
