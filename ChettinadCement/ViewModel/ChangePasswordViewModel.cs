using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IWT.ViewModel
{
    public class ChangePasswordViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _OldPassword;
        private string _NewPassword;
        private string _ConfirmPassword;

        public string OldPassword
        {
            get { return _OldPassword; }
            set
            {
                _OldPassword = value;
                OnPropertyChanged();
            }
        }

        public string NewPassword
        {
            get { return _NewPassword; }
            set
            {
                _NewPassword = value;
                OnPropertyChanged();
            }
        }
        public string ConfirmPassword
        {
            get { return _ConfirmPassword; }
            set
            {
                _ConfirmPassword = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

}
