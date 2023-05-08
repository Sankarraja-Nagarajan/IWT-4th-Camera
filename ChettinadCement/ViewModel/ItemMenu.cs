using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace IWT.ViewModel
{
    public class ItemMenu
    {
        //public ItemMenu(string header, List<SubItem> subItems, PackIconKind icon)
        //{
        //    Header = header;
        //    SubItems = subItems;
        //    Icon = icon;
        //}

        public ItemMenu(string header, UserControl screen, string icon)
        {
            Header = header;
            Screen = screen;
            Icon = icon;
        }

        public string Header { get; private set; }
        public string Icon { get; private set; }
        //public List<SubItem> SubItems { get; private set; }
        public UserControl Screen { get; private set; }
    }
}
