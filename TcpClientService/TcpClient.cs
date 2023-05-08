using SuperSimpleTcp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TcpClientService.Helpers;

namespace TcpClientService
{
    public class TcpClient: IDisposable
    {
        public SimpleTcpClient Client { get; set; }
        private string ClientType { get; set; }
        public event EventHandler<TcpValueArgs> OnValueRecieved = delegate { };
        public event EventHandler<TcpEventArgs> OnConnected = delegate { };
        public event EventHandler<TcpEventArgs> OnDisconnected = delegate { };
        private DispatcherTimer RetryTimer;
        public TcpClient(string ip, int port, string type)
        {
            this.ClientType = type;
            Client = new SimpleTcpClient(ip, port);
            Client.Keepalive = new SimpleTcpKeepaliveSettings { EnableTcpKeepAlives = true };
            Client.Events.Connected += Connected;
            Client.Events.Disconnected += Disconnected;
            Client.Events.DataReceived += DataReceived;
            RetryTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            RetryTimer.Tick += RetryTimer_Tick;
            RetryTimer.Start();
        }

        private void RetryTimer_Tick(object sender, EventArgs e)
        {
            Initialize();
        }

        public void Initialize()
        {
            if (Client != null && !Client.IsConnected)
            {
                Task.Run(() =>
                {
                    try
                    {
                        Client.Connect();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"TCP Client :- {e.Message}");
                        OnDisconnected.Invoke(e, new TcpEventArgs());
                    }
                });
            }
            
        }

        private void Connected(object sender, ConnectionEventArgs e)
        {
            Debug.WriteLine($"TCP Client :- Server {e.IpPort} connected!!");
            OnConnected.Invoke(sender, new TcpEventArgs());
        }

        private void Disconnected(object sender, ConnectionEventArgs e)
        {
            Debug.WriteLine($"TCP Client :- Server {e.IpPort} disconnected!!");
            OnDisconnected.Invoke(sender, new TcpEventArgs());
        }

        private void DataReceived(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            string value = "";
            if (ClientType == "LOADCELL")
            {
                value = DataConversionHelper.ToWeighValue(e.Data);
            }
            else if (ClientType == "RFID")
            {
                value = DataConversionHelper.ToRfidValue(e.Data);
            }
            else if (ClientType == "PLC")
            {
                value = DataConversionHelper.ToPlcValue(e.Data);
            }
            OnValueRecieved.Invoke(sender, new TcpValueArgs(value));
        }
        public class TcpValueArgs : EventArgs
        {
            public string value { get; set; }
            public TcpValueArgs(string value) : base()
            {
                this.value = value;
            }
        }
        public class TcpEventArgs : EventArgs
        {

        }
        public void Dispose()
        {
            Client.Events.Connected -= Connected;
            Client.Events.Disconnected -= Disconnected;
            Client.Events.DataReceived -= DataReceived;
            Client.Dispose();
            RetryTimer.Tick -= RetryTimer_Tick;
            RetryTimer.Stop();
        }
    }
}
