using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWT.ViewModel
{
   public class ValidatorViewModel:ViewBaseModel
    {
        private string _name;
        private string _name2;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public string Name1
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}
