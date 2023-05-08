using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWT.Shared
{
    public static class CustomNotificationWPF
    {
        public static NotificationManager notificationManager = new NotificationManager();

        public static void ShowMessage(Action<string, TimeSpan?> ActionName, string message, int ExpirationSeconds = 3)
        {
            //CloseOnScreenKeyboard();
            var expirationTime = TimeSpan.FromSeconds(ExpirationSeconds);
            ActionName(message, expirationTime);
        }

        public static void ShowInformation(string message,TimeSpan? expirationTime)
        {
            notificationManager.Show("Info",message,NotificationType.Information,"", expirationTime, onClick: () => { });
        }

        public static void ShowSuccess(string message, TimeSpan? expirationTime)
        {
            notificationManager.Show("Success", message, NotificationType.Success, "", expirationTime, onClick: () => { });
        }
        public static void ShowWarning(string message, TimeSpan? expirationTime)
        {
            notificationManager.Show("Warning", message, NotificationType.Warning, "", expirationTime, onClick: () => { });
        }

        public static void ShowError(string message, TimeSpan? expirationTime)
        {
            notificationManager.Show("Warning", message, NotificationType.Error, "", expirationTime, onClick: () => { });
        }

        public static void CloseOnScreenKeyboard()
        {
            try
            {
                foreach (System.Diagnostics.Process process in System.Diagnostics.Process.GetProcessesByName("osk"))
                {
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile($"MainWindow/CloseOnScreenKeyboard", ex);
            }
        }
    }
}
