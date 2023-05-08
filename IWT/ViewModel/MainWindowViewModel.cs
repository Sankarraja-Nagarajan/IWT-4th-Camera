using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWT.ViewModel
{
    internal class MainWindowViewModel:ViewBaseModel
    {
        private string _weight;

        public string Weight { get => _weight; set => SetProperty(ref _weight,value); }
    }
}
