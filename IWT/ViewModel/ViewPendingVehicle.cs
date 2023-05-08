using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWT.ViewModel
{
   public class ViewPendingVehicle : ViewBaseModel
    {
        public ViewPendingVehicle(string name, ViewPendingVehicle screen = null)
        {
            Name = name;
            Screen = screen;
        }
        public string Name { get; private set; }
        public ViewPendingVehicle Screen { get; private set; }
    }
}
