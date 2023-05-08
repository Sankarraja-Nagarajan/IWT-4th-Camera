using IWT.DBCall;
using IWT.Models;
using IWT.Shared;
using IWT.ViewModel;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Diagnostics;
using System.Text;
using System.Windows.Markup;
using System.Xml;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IWT.Setting_Pages
{
    /// <summary>
    /// Interaction logic for Email_Design.xaml
    /// </summary>
    public partial class Email_Design : UserControl, INotifyPropertyChanged
    {
        public static MasterDBCall masterDBCall = new MasterDBCall();
        public static CommonFunction commonFunction = new CommonFunction();
        private readonly ToastViewModel toastViewModel;
        public EmailDesign CurrentEmailDesign;
        string LastMessage;

        public event PropertyChangedEventHandler PropertyChanged;
        private string _RFTValue;
        public string RFTValue
        {
            get { return _RFTValue; }
            set
            {
                _RFTValue = value;
                OnPropertyChanged();
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Email_Design()
        {
            InitializeComponent();
            toastViewModel = new ToastViewModel();
            CurrentEmailDesign = new EmailDesign();
            Loaded += Email_Design_Loaded;
            Unloaded += Email_Design_Unloaded;
            GetTableColumnDetails("Transaction");
            this.DataContext = this;
        }

        private void Email_Design_Loaded(object sender, RoutedEventArgs e)
        {
 
            GetEmailDesigns();
        }

        private void Email_Design_Unloaded(object sender, RoutedEventArgs e)
        {
            //RichTextWebView.NavigationCompleted -= RichTextWebView_NavigationCompleted;
            //RichTextWebView.Loaded -= RichTextWebView_Loaded;
            //WPFChromiumWebBrowser.Loaded -= WPFChromiumWebBrowser_Loaded;
        }
        private void SmithHtmlEditor_DocumentReady(object sender, RoutedEventArgs e)
        {
            GetEmailDesigns();
        }

        private void RichTextWebView_Loaded(object sender, RoutedEventArgs e)
        {
            var RichTextWebsite = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RichTextWebsite");
            var htmlPath = System.IO.Path.Combine(RichTextWebsite, "RichText.html");
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
                WriteLog.WriteToFile("Email_Design/GetTableColumnDetails/Exception:- " + ex.Message, ex);
                return null;
            }
        }


        private void TransactionListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TransactionListView.SelectedItems?.Count > 0)
            {
                var selectedItem = TransactionListView.SelectedItems[0] as TableColumnDetails;
                string richText = GetRichText();
                if (!string.IsNullOrEmpty(richText))
                {
                    string HtmlText = HtmlFromXamlConverter1.ConvertXamlToHtml(richText, true);
                    HtmlText = HtmlText + "\u003Cspan>[" + selectedItem.ColumnName + "]\u003C/span>";
                    HtmlText = RestoreHtmlTags(HtmlText);
                    var xmalText = MarkupConverter.HtmlToXamlConverter.ConvertHtmlToXaml(HtmlText, true);
                    var r = SetRichText(xmalText);
                }
                
            }
        }

        private void SaveDesignButton_Click(object sender, RoutedEventArgs e)
        {
            string richText = GetRichText();
            if (!string.IsNullOrEmpty(richText))
            {
                string HtmlText = HtmlFromXamlConverter1.ConvertXamlToHtml(richText, true);
                HtmlText = RestoreHtmlTags(HtmlText);
                bool result = SaveEmailDesign(HtmlText);
                if (result)
                {
                    CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowSuccess, "Email Design saved successfully");
                    DialogHost.CloseDialogCommand.Execute(null, null);
                }
            }
            else
            {
                CustomNotificationWPF.ShowMessage(CustomNotificationWPF.ShowWarning, "Please fill the content");
            }
        }

        private string GetRichText()
        {
            var result = RFTValue?.ToString();
            return result;
        }

        private string TrimRichText(string text)
        {
            return text.Trim(new char[] { (char)34 });
        }

        private string RestoreHtmlTags(string text) {
            return text.Replace(@"\u003C", @"<");
        }

        private string SetRichText(string msg)
        {
            RFTValue = msg;
            return msg;
        }

        public List<EmailDesign> GetEmailDesigns()
        {
            try
            {
                var result = commonFunction.GetEmailDesigns();
                if (result.Count > 0)
                {
                    CurrentEmailDesign = result.FirstOrDefault();
                    if (CurrentEmailDesign != null)
                    {
                        var xmalText = MarkupConverter.HtmlToXamlConverter.ConvertHtmlToXaml(CurrentEmailDesign.DesignedContent, true);
                        var r = SetRichText(xmalText);

                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Email_Design/GetTableColumnDetails/Exception:- " + ex.Message, ex);
                return null;
            }
        }

        public bool SaveEmailDesign(string DesignedContent)
        {
            try
            {
                if (CurrentEmailDesign != null && CurrentEmailDesign.ID != 0)
                {
                    return UpdateEmailDesign(DesignedContent);
                }
                else
                {
                    return CreateEmailDesign(DesignedContent);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Email_Design/SaveEmailDesign/Exception:- " + ex.Message, ex);
                return false;
            }

        }

        public bool CreateEmailDesign(string DesignedContent)
        {
            try
            {
                string Query = "INSERT INTO Email_Design (DesignedContent,CreatedOn) values (@DesignedContent,@CreatedOn)";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@DesignedContent", DesignedContent);
                cmd.Parameters.AddWithValue("@CreatedOn", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                masterDBCall.InsertData(cmd, System.Data.CommandType.Text);
                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Email_Design/SaveEmailDesign/Exception:- " + ex.Message, ex);
                return false;
            }
        }
        public bool UpdateEmailDesign(string DesignedContent)
        {
            try
            {
                string Query = "UPDATE Email_Design SET DesignedContent=@DesignedContent,ModifiedOn=@ModifiedOn WHERE ID=@ID";
                SqlCommand cmd = new SqlCommand(Query);
                cmd.Parameters.AddWithValue("@ID", CurrentEmailDesign.ID);
                cmd.Parameters.AddWithValue("@DesignedContent", DesignedContent);
                cmd.Parameters.AddWithValue("@ModifiedOn", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                masterDBCall.InsertData(cmd, System.Data.CommandType.Text);
                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("Email_Design/SaveEmailDesign/Exception:- " + ex.Message, ex);
                return false;
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
    }


    public class RichTextBoxHelper : DependencyObject
    {
        private static HashSet<Thread> _recursionProtection = new HashSet<Thread>();

        public static string GetDocumentXaml(DependencyObject obj)
        {
            return (string)obj.GetValue(DocumentXamlProperty);
        }

        public static void SetDocumentXaml(DependencyObject obj, string value)
        {
            _recursionProtection.Add(Thread.CurrentThread);
            obj.SetValue(DocumentXamlProperty, value);
            _recursionProtection.Remove(Thread.CurrentThread);
        }

        public static readonly DependencyProperty DocumentXamlProperty = DependencyProperty.RegisterAttached(
            "DocumentXaml",
            typeof(string),
            typeof(RichTextBoxHelper),
            new FrameworkPropertyMetadata(
                "",
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (obj, e) => {
                    if (_recursionProtection.Contains(Thread.CurrentThread))
                        return;

                    var richTextBox = (RichTextBox)obj;

                    // Parse the XAML to a document (or use XamlReader.Parse())

                    try
                    {
                        var stream = new MemoryStream(Encoding.UTF8.GetBytes(GetDocumentXaml(richTextBox)));
                        var doc = (FlowDocument)XamlReader.Load(stream);

                        // Set the document
                        richTextBox.Document = doc;
                    }
                    catch (Exception)
                    {
                        richTextBox.Document = new FlowDocument();
                    }

                    // When the document changes update the source
                    richTextBox.TextChanged += (obj2, e2) =>
                    {
                        RichTextBox richTextBox2 = obj2 as RichTextBox;
                        if (richTextBox2 != null)
                        {
                            SetDocumentXaml(richTextBox, XamlWriter.Save(richTextBox2.Document));
                        }
                    };
                }
            )
        );
    }

    internal static class HtmlFromXamlConverter1
    {
        // ---------------------------------------------------------------------
        //
        // Internal Methods
        //
        // ---------------------------------------------------------------------

        #region Internal Methods
        internal static string ConvertXamlToHtml(string xamlString)
        {
            return ConvertXamlToHtml(xamlString, true);
        }
        /// <summary>
        /// Main entry point for Xaml-to-Html converter.
        /// Converts a xaml string into html string.
        /// </summary>
        /// <param name="xamlString">
        /// Xaml strinng to convert.
        /// </param>
        /// <returns>
        /// Html string produced from a source xaml.
        /// </returns>
        internal static string ConvertXamlToHtml(string xamlString, bool asFlowDocument)
        {
            XmlTextReader xamlReader;
            StringBuilder htmlStringBuilder;
            XmlTextWriter htmlWriter;

            if (!asFlowDocument)
            {
                xamlString = "<FlowDocument>" + xamlString + "</FlowDocument>";
            }

            xamlReader = new XmlTextReader(new StringReader(xamlString));

            htmlStringBuilder = new StringBuilder(100);
            htmlWriter = new XmlTextWriter(new StringWriter(htmlStringBuilder));

            if (!WriteFlowDocument(xamlReader, htmlWriter))
            {
                return "";
            }

            string htmlString = htmlStringBuilder.ToString();

            return htmlString;
        }

        #endregion Internal Methods

        // ---------------------------------------------------------------------
        //
        // Private Methods
        //
        // ---------------------------------------------------------------------

        #region Private Methods
        /// <summary>
        /// Processes a root level element of XAML (normally it's FlowDocument element).
        /// </summary>
        /// <param name="xamlReader">
        /// XmlTextReader for a source xaml.
        /// </param>
        /// <param name="htmlWriter">
        /// XmlTextWriter producing resulting html
        /// </param>
        private static bool WriteFlowDocument(XmlTextReader xamlReader, XmlTextWriter htmlWriter)
        {
            if (!ReadNextToken(xamlReader))
            {
                // Xaml content is empty - nothing to convert
                return false;
            }

            if (xamlReader.NodeType != XmlNodeType.Element || xamlReader.Name != "FlowDocument")
            {
                // Root FlowDocument elemet is missing
                return false;
            }

            // Create a buffer StringBuilder for collecting css properties for inline STYLE attributes
            // on every element level (it will be re-initialized on every level).
            StringBuilder inlineStyle = new StringBuilder();

            htmlWriter.WriteStartElement("HTML");
            htmlWriter.WriteStartElement("BODY");

            WriteFormattingProperties(xamlReader, htmlWriter, inlineStyle);

            WriteElementContent(xamlReader, htmlWriter, inlineStyle);

            htmlWriter.WriteEndElement();
            htmlWriter.WriteEndElement();

            return true;
        }

        /// <summary>
        /// Reads attributes of the current xaml element and converts
        /// them into appropriate html attributes or css styles.
        /// </summary>
        /// <param name="xamlReader">
        /// XmlTextReader which is expected to be at XmlNodeType.Element
        /// (opening element tag) position.
        /// The reader will remain at the same level after function complete.
        /// </param>
        /// <param name="htmlWriter">
        /// XmlTextWriter for output html, which is expected to be in
        /// after WriteStartElement state.
        /// </param>
        /// <param name="inlineStyle">
        /// String builder for collecting css properties for inline STYLE attribute.
        /// </param>
        private static void WriteFormattingProperties(XmlTextReader xamlReader, XmlTextWriter htmlWriter, StringBuilder inlineStyle)
        {
            Debug.Assert(xamlReader.NodeType == XmlNodeType.Element);

            // Clear string builder for the inline style
            inlineStyle.Remove(0, inlineStyle.Length);

            if (!xamlReader.HasAttributes)
            {
                return;
            }

            bool borderSet = false;

            while (xamlReader.MoveToNextAttribute())
            {
                string css = null;

                switch (xamlReader.Name)
                {
                    // Character fomatting properties
                    // ------------------------------
                    case "Background":
                        css = "background-color:" + ParseXamlColor(xamlReader.Value) + ";";
                        break;
                    case "FontFamily":
                        css = "font-family:" + xamlReader.Value + ";";
                        break;
                    case "FontStyle":
                        css = "font-style:" + xamlReader.Value.ToLower() + ";";
                        break;
                    case "FontWeight":
                        css = "font-weight:" + xamlReader.Value.ToLower() + ";";
                        break;
                    case "FontStretch":
                        break;
                    case "FontSize":
                        css = "font-size:" + xamlReader.Value + ";";
                        break;
                    case "Foreground":
                        css = "color:" + ParseXamlColor(xamlReader.Value) + ";";
                        break;
                    case "TextDecorations":
                        css = "text-decoration:underline;";
                        break;
                    case "TextEffects":
                        break;
                    case "Emphasis":
                        break;
                    case "StandardLigatures":
                        break;
                    case "Variants":
                        break;
                    case "Capitals":
                        break;
                    case "Fraction":
                        break;

                    // Paragraph formatting properties
                    // -------------------------------
                    case "Padding":
                        css = "padding:" + ParseXamlThickness(xamlReader.Value) + ";";
                        break;
                    case "Margin":
                        css = "margin:" + ParseXamlThickness(xamlReader.Value) + ";";
                        break;
                    case "BorderThickness":
                        css = "border-width:" + ParseXamlThickness(xamlReader.Value) + ";";
                        borderSet = true;
                        break;
                    case "BorderBrush":
                        css = "border-color:" + ParseXamlColor(xamlReader.Value) + ";";
                        borderSet = true;
                        break;
                    case "LineHeight":
                        break;
                    case "TextIndent":
                        css = "text-indent:" + xamlReader.Value + ";";
                        break;
                    case "TextAlignment":
                        css = "text-align:" + xamlReader.Value + ";";
                        break;
                    case "IsKeptTogether":
                        break;
                    case "IsKeptWithNext":
                        break;
                    case "ColumnBreakBefore":
                        break;
                    case "PageBreakBefore":
                        break;
                    case "FlowDirection":
                        break;

                    // Table attributes
                    // ----------------
                    case "Width":
                        css = "width:" + xamlReader.Value + ";";
                        break;
                    case "ColumnSpan":
                        htmlWriter.WriteAttributeString("COLSPAN", xamlReader.Value);
                        break;
                    case "RowSpan":
                        htmlWriter.WriteAttributeString("ROWSPAN", xamlReader.Value);
                        break;
                }

                if (css != null)
                {
                    inlineStyle.Append(css);
                }
            }

            if (borderSet)
            {
                inlineStyle.Append("border-style:solid;mso-element:para-border-div;");
            }

            // Return the xamlReader back to element level
            xamlReader.MoveToElement();
            Debug.Assert(xamlReader.NodeType == XmlNodeType.Element);
        }

        private static string ParseXamlColor(string color)
        {
            if (color.StartsWith("#"))
            {
                // Remove transparancy value
                color = "#" + color.Substring(3);
            }
            return color;
        }

        private static string ParseXamlThickness(string thickness)
        {
            string[] values = thickness.Split(',');

            for (int i = 0; i < values.Length; i++)
            {
                double value;
                if (double.TryParse(values[i], out value))
                {
                    values[i] = Math.Ceiling(value).ToString();
                }
                else
                {
                    values[i] = "1";
                }
            }

            string cssThickness;
            switch (values.Length)
            {
                case 1:
                    cssThickness = thickness;
                    break;
                case 2:
                    cssThickness = values[1] + " " + values[0];
                    break;
                case 4:
                    cssThickness = values[1] + " " + values[2] + " " + values[3] + " " + values[0];
                    break;
                default:
                    cssThickness = values[0];
                    break;
            }

            return cssThickness;
        }

        /// <summary>
        /// Reads a content of current xaml element, converts it
        /// </summary>
        /// <param name="xamlReader">
        /// XmlTextReader which is expected to be at XmlNodeType.Element
        /// (opening element tag) position.
        /// </param>
        /// <param name="htmlWriter">
        /// May be null, in which case we are skipping the xaml element;
        /// witout producing any output to html.
        /// </param>
        /// <param name="inlineStyle">
        /// StringBuilder used for collecting css properties for inline STYLE attribute.
        /// </param>
        private static void WriteElementContent(XmlTextReader xamlReader, XmlTextWriter htmlWriter, StringBuilder inlineStyle)
        {
            Debug.Assert(xamlReader.NodeType == XmlNodeType.Element);

            bool elementContentStarted = false;

            if (xamlReader.IsEmptyElement)
            {
                if (htmlWriter != null && !elementContentStarted && inlineStyle.Length > 0)
                {
                    // Output STYLE attribute and clear inlineStyle buffer.
                    htmlWriter.WriteAttributeString("STYLE", inlineStyle.ToString());
                    inlineStyle.Remove(0, inlineStyle.Length);
                }
                elementContentStarted = true;
            }
            else
            {
                while (ReadNextToken(xamlReader) && xamlReader.NodeType != XmlNodeType.EndElement)
                {
                    switch (xamlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (xamlReader.Name.Contains("."))
                            {
                                AddComplexProperty(xamlReader, inlineStyle);
                            }
                            else
                            {
                                if (htmlWriter != null && !elementContentStarted && inlineStyle.Length > 0)
                                {
                                    // Output STYLE attribute and clear inlineStyle buffer.
                                    htmlWriter.WriteAttributeString("STYLE", inlineStyle.ToString());
                                    inlineStyle.Remove(0, inlineStyle.Length);
                                }
                                elementContentStarted = true;
                                WriteElement(xamlReader, htmlWriter, inlineStyle);
                            }
                            Debug.Assert(xamlReader.NodeType == XmlNodeType.EndElement || xamlReader.NodeType == XmlNodeType.Element && xamlReader.IsEmptyElement);
                            break;
                        case XmlNodeType.Comment:
                            if (htmlWriter != null)
                            {
                                if (!elementContentStarted && inlineStyle.Length > 0)
                                {
                                    htmlWriter.WriteAttributeString("STYLE", inlineStyle.ToString());
                                }
                                htmlWriter.WriteComment(xamlReader.Value);
                            }
                            elementContentStarted = true;
                            break;
                        case XmlNodeType.CDATA:
                        case XmlNodeType.Text:
                        case XmlNodeType.SignificantWhitespace:
                            if (htmlWriter != null)
                            {
                                if (!elementContentStarted && inlineStyle.Length > 0)
                                {
                                    htmlWriter.WriteAttributeString("STYLE", inlineStyle.ToString());
                                }
                                htmlWriter.WriteString(xamlReader.Value);
                            }
                            elementContentStarted = true;
                            break;
                    }
                }

                Debug.Assert(xamlReader.NodeType == XmlNodeType.EndElement);
            }
        }

        /// <summary>
        /// Conberts an element notation of complex property into
        /// </summary>
        /// <param name="xamlReader">
        /// On entry this XmlTextReader must be on Element start tag;
        /// on exit - on EndElement tag.
        /// </param>
        /// <param name="inlineStyle">
        /// StringBuilder containing a value for STYLE attribute.
        /// </param>
        private static void AddComplexProperty(XmlTextReader xamlReader, StringBuilder inlineStyle)
        {
            Debug.Assert(xamlReader.NodeType == XmlNodeType.Element);

            if (inlineStyle != null && xamlReader.Name.EndsWith(".TextDecorations"))
            {
                inlineStyle.Append("text-decoration:underline;");
            }

            // Skip the element representing the complex property
            WriteElementContent(xamlReader, /*htmlWriter:*/null, /*inlineStyle:*/null);
        }

        /// <summary>
        /// Converts a xaml element into an appropriate html element.
        /// </summary>
        /// <param name="xamlReader">
        /// On entry this XmlTextReader must be on Element start tag;
        /// on exit - on EndElement tag.
        /// </param>
        /// <param name="htmlWriter">
        /// May be null, in which case we are skipping xaml content
        /// without producing any html output
        /// </param>
        /// <param name="inlineStyle">
        /// StringBuilder used for collecting css properties for inline STYLE attributes on every level.
        /// </param>
        private static void WriteElement(XmlTextReader xamlReader, XmlTextWriter htmlWriter, StringBuilder inlineStyle)
        {
            Debug.Assert(xamlReader.NodeType == XmlNodeType.Element);

            if (htmlWriter == null)
            {
                // Skipping mode; recurse into the xaml element without any output
                WriteElementContent(xamlReader, /*htmlWriter:*/null, null);
            }
            else
            {
                string htmlElementName = null;

                switch (xamlReader.Name)
                {
                    case "Run":
                    case "Span":
                        htmlElementName = "SPAN";
                        break;
                    case "LineBreak":
                        htmlElementName = "BR";
                        break;
                    case "InlineUIContainer":
                        htmlElementName = "SPAN";
                        break;
                    case "Bold":
                        htmlElementName = "B";
                        break;
                    case "Italic":
                        htmlElementName = "I";
                        break;
                    case "Paragraph":
                        htmlElementName = "P";
                        break;
                    case "BlockUIContainer":
                        htmlElementName = "DIV";
                        break;
                    case "Section":
                        htmlElementName = "DIV";
                        break;
                    case "Table":
                        htmlElementName = "TABLE";
                        break;
                    case "TableColumn":
                        htmlElementName = "COL";
                        break;
                    case "TableRowGroup":
                        htmlElementName = "TBODY";
                        break;
                    case "TableRow":
                        htmlElementName = "TR";
                        break;
                    case "TableCell":
                        htmlElementName = "TD";
                        break;
                    case "List":
                        string marker = xamlReader.GetAttribute("MarkerStyle");
                        if (marker == null || marker == "None" || marker == "Disc" || marker == "Circle" || marker == "Square" || marker == "Box")
                        {
                            htmlElementName = "UL";
                        }
                        else
                        {
                            htmlElementName = "OL";
                        }
                        break;
                    case "ListItem":
                        htmlElementName = "LI";
                        break;
                    default:
                        htmlElementName = null; // Ignore the element
                        break;
                }

                if (htmlWriter != null && htmlElementName != null)
                {
                    htmlWriter.WriteStartElement(htmlElementName);

                    WriteFormattingProperties(xamlReader, htmlWriter, inlineStyle);

                    WriteElementContent(xamlReader, htmlWriter, inlineStyle);

                    htmlWriter.WriteEndElement();
                }
                else
                {
                    // Skip this unrecognized xaml element
                    WriteElementContent(xamlReader, /*htmlWriter:*/null, null);
                }
            }
        }

        // Reader advance helpers
        // ----------------------

        /// <summary>
        /// Reads several items from xamlReader skipping all non-significant stuff.
        /// </summary>
        /// <param name="xamlReader">
        /// XmlTextReader from tokens are being read.
        /// </param>
        /// <returns>
        /// True if new token is available; false if end of stream reached.
        /// </returns>
        private static bool ReadNextToken(XmlReader xamlReader)
        {
            while (xamlReader.Read())
            {
                Debug.Assert(xamlReader.ReadState == ReadState.Interactive, "Reader is expected to be in Interactive state (" + xamlReader.ReadState + ")");
                switch (xamlReader.NodeType)
                {
                    case XmlNodeType.Element:
                    case XmlNodeType.EndElement:
                    case XmlNodeType.None:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                    case XmlNodeType.SignificantWhitespace:
                        return true;

                    case XmlNodeType.Whitespace:
                        if (xamlReader.XmlSpace == XmlSpace.Preserve)
                        {
                            return true;
                        }
                        // ignore insignificant whitespace
                        break;

                    case XmlNodeType.EndEntity:
                    case XmlNodeType.EntityReference:
                        //  Implement entity reading
                        //xamlReader.ResolveEntity();
                        //xamlReader.Read();
                        //ReadChildNodes( parent, parentBaseUri, xamlReader, positionInfo);
                        break; // for now we ignore entities as insignificant stuff

                    case XmlNodeType.Comment:
                        return true;
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.DocumentType:
                    case XmlNodeType.XmlDeclaration:
                    default:
                        // Ignorable stuff
                        break;
                }
            }
            return false;
        }

        #endregion Private Methods

        // ---------------------------------------------------------------------
        //
        // Private Fields
        //
        // ---------------------------------------------------------------------

        #region Private Fields

        #endregion Private Fields
    }

}
