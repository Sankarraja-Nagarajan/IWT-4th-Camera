using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWT.Shared
{
    public interface IMarkupConverter
    {
        string ConvertXamlToHtml(string xamlText);
        string ConvertHtmlToXaml(string htmlText);
        string ConvertRtfToHtml(string rtfText);
    }

    public class MarkupConverter : IMarkupConverter
    {
        public string ConvertXamlToHtml(string xamlText)
        {
            return HtmlFromXamlConverter.ConvertXamlToHtml(xamlText, false);
        }
        public string ConvertHtmlToXaml(string htmlText)
        {
            return HtmlToXamlConverter.ConvertHtmlToXaml(htmlText, true);
        }
        public string ConvertRtfToHtml(string rtfText)
        {
            return RtfToHtmlConverter.ConvertRtfToHtml(rtfText);
        }
    }
}
