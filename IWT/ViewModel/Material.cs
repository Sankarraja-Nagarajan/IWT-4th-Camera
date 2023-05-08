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
    public class Material : ViewBaseModel
    {
        public ICommand AddmaterialDialogCommand => new AnotherCommandImplementation(ExecuteaddmaterialDialog);
        private async void ExecuteaddmaterialDialog(object _)
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new Addmaterial();

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);

            //check the result...
            ////Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            ////Debug.WriteLine("You can intercept the closing event, and cancel here.");
        }
        
    }
}
