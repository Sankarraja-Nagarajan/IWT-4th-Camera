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
    public class Viewvehicle : ViewBaseModel
    {
        public Viewvehicle()
        {
            vehicleNO = "";
            material = "";
            supplier = "";
        }
        private string vehicleNO="";
        private string material = "";
        private string supplier = "";
        public class Class1
        {
            public string LogedInPerson { get; set; }

            public string Name
            {
                get { return this.LogedInPerson; }
                set { this.LogedInPerson = value; }
            }
        }


        public static Class1 obj = new Class1(); 

        public class class2
        {
            public string materialname { get; set; }

            public string materialcode { get; set; }
            public string Name1
            {
                get { return this.materialname; }
                set { this.materialname = value; }
            }
            public string Name2
            {
                get { return this.materialcode; }
                set { this.materialcode = value; }
            }

        }

        public class class3
        {
            public string suppliername { get; set; }

            public string suppliercode { get; set; }
            public string Name3
            {
                get { return this.suppliername; }
                set { this.suppliername = value; }
            }
            public string Name4
            {
                get { return this.suppliercode; }
                set { this.suppliercode = value; }
            }

        }

        public static class2 obj1 = new class2();
        public static class3 obj3 = new class3();

        //public ICommand AddsupplierDialogCommand => new AnotherCommandImplementation(ExecuteaddsupplierDialog);

        //public string VehicleNO { get => vehicleNO; set => SetProperty(ref vehicleNO, value); }

        //public string Material { get => material; set => SetProperty(ref material, value); }



    }


}
