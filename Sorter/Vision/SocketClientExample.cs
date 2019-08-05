using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sorter
{
    public class SocketClientExample
    {
        private Socket _clientSocket;
        private EndPoint _endPoint;
        private readonly string _ip;
        private readonly int _port;
        private bool _started;
        private const int _reConnectInterval = 2000;

        private readonly ManualResetEvent _connectManualResetEvent = new ManualResetEvent(true);
        private readonly ManualResetEvent _receiveManualResetEvent = new ManualResetEvent(false);

        //Todo event
        private bool _connected2;
        public bool Connected
        {
            get { return _connected2; }
            set { _connected2 = value; }
        }

        #region Events

        public delegate void InfoOccuredEventHandler(object sender, Info info);

        public event InfoOccuredEventHandler InfoOccured;

        protected void OnInfoOccured(Info info)
        {
            InfoOccured?.Invoke(this, info);
        }

        public delegate void WarningOccuredEventHandler(object sender, Warning warning);

        public event WarningOccuredEventHandler WarningOccured;

        protected void OnWarningOccured(Warning warning)
        {
            WarningOccured?.Invoke(this, warning);
        }

        public delegate void ErrorOccuredEventHandler(object sender, Error error);

        public event ErrorOccuredEventHandler ErrorOccured;

        protected void OnErrorOccured(Error error)
        {
            ErrorOccured?.Invoke(this, error);
        }
        #endregion

        public SocketClientExample(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public void Start()
        {
            if (_started)
                return;

            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _endPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);

            Task.Run(() => { Connect(); });
            Task.Run(() => { ReceiveMessage(); });

            _started = true;
        }

        private void Connect()
        {
            while (true)
            {
                _connectManualResetEvent.WaitOne();
                try
                {
                    _clientSocket.Close();
                    _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _clientSocket.Connect(_endPoint);

                    _receiveManualResetEvent.Set();
                    _connectManualResetEvent.Reset();

                    Connected = true;

                    OnInfoOccured(new Info() { Code = InfoCode.CameraConnected });
                }
                catch (Exception)
                {
                    Task.Delay(_reConnectInterval);
                }
            }
        }

        private void ReceiveMessage()
        {
            while (true)
            {
                _receiveManualResetEvent.WaitOne();
                try
                {
                    byte[] buffer = new byte[512];
                    int rec = _clientSocket.Receive(buffer, 0, buffer.Length, 0);
                    if (rec <= 0)
                    {
                        throw new SocketException();
                    }
                    Array.Resize(ref buffer, rec);
                    string ClientReceivedMessage = Encoding.Default.GetString(buffer);
                }
                catch (Exception)
                {
                    Connected = false;
                    _receiveManualResetEvent.Reset();
                    _connectManualResetEvent.Set();
                    OnErrorOccured(new Error() { Code = ErrorCode.CameraDisconnected });
                }
            }
        }      

        private void Send(string cmd)
        {
            try
            {
                byte[] buffer = Encoding.Default.GetBytes(cmd);
                _clientSocket.Send(buffer, 0, buffer.Length, 0);
            }
            catch (Exception)
            {
                Connected = false;
                _receiveManualResetEvent.Reset();
                _connectManualResetEvent.Set();
            }
        }

    }
}
