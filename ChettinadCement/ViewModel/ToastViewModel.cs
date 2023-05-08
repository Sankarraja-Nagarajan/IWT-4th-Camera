using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace IWT.ViewModel
{
    public class ToastViewModel : INotifyPropertyChanged
    {
        private readonly Notifier notifier;

        public ToastViewModel()
        {
            notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.BottomRight,
                    offsetX: 25,
                    offsetY: 50);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(5),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(3));

                cfg.Dispatcher = Application.Current.Dispatcher;

                cfg.DisplayOptions.TopMost = false;
                cfg.DisplayOptions.Width = 250;
            });

            //notifier.ClearMessages();
        }

        public void OnUnloaded()
        {
            notifier.Dispose();
        }

        public void ShowInformation(string message)
        {
            notifier.ShowInformation(message);
        }

        public void ShowInformation(string message, MessageOptions opts)
        {
            notifier.ShowInformation(message, opts);
        }

        public void ShowSuccess(string message)
        {
            notifier.ShowSuccess(message);
        }

        public void ShowSuccess(string message, MessageOptions opts)
        {
            notifier.ShowSuccess(message, opts);
        }

        internal void ClearMessages(string msg)
        {
            //notifier.ClearMessages(msg);
        }
        public void ShowWarning(string message)
        {
            notifier.ShowWarning(message);
        }
        public void ShowWarning(string message, MessageOptions opts)
        {
            notifier.ShowWarning(message, opts);
        }

        public void ShowError(string message)
        {
            notifier.ShowError(message);
        }

        public void ShowError(string message, MessageOptions opts)
        {
            notifier.ShowError(message, opts);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
