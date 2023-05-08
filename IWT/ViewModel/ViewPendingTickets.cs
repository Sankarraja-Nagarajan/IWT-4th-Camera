using IWT.TransactionPages;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IWT.ViewModel
{
    public class ViewPendingTickets: ViewBaseModel
    {

        public static Class1 obj = new Class1();
        public class Class1
        {
            public string LogedInPerson { get; set; }

            public string Name
            {
                get { return this.LogedInPerson; }
                set { this.LogedInPerson = value; }
            }
        }
        //public ICommand PendingVehicleDialogCommand => new AnotherCommandImplementation(ExecutePendingvehicleDialog);
        public async void ExecutePendingvehicleDialog()
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new PendingVehicleDialog();

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);

            //check the result...
            //Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        //private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        //{
        //    //Debug.WriteLine("You can intercept the closing event, and cancel here.");
        //}
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            var result = eventArgs?.Parameter;
            VehicleNo = "TN72 AB0001";
            //Console.WriteLine(result);
            ////Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }

        private string _vehicleNo;

        public string VehicleNo { get => _vehicleNo; set => SetProperty(ref _vehicleNo,value); }
    }
}
