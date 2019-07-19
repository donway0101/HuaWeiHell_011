using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bp.Mes;

namespace Sorter
{
    public class VisionServer
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Socket _clientSocket;
        private EndPoint _endPoint;
        private readonly string _ip;
        private readonly int _port;
        private bool _started;
        private const int _reConnectInterval = 2000;
        private object _captureResultLocker = new object();

        private readonly ManualResetEvent _visionResponseManualResetEvent = new ManualResetEvent(false);

        private readonly ManualResetEvent _connectManualResetEvent = new ManualResetEvent(true);
        private readonly ManualResetEvent _receiveManualResetEvent = new ManualResetEvent(false);

        public bool CaptureResponsed { get; set; }

        /// <summary>
        /// Vision calibrations from camera.
        /// </summary>
        public List<AxisOffset> CaptureResult { get; set; } = new List<AxisOffset>();

        private bool _connected2;
        public bool Connected
        {
            get { return _connected2; }
            set { _connected2 = value; }
        }

        public bool DataReceieved { get; set; }

        public string ReceivedMessage { get; set; }

        //Toto initialize
        public MotionController _mc { get; set; }

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

        public VisionServer(MotionController controller, string ip ="192.168.100.100", int port=6000)
        {
            _ip = ip;
            _port = port;
            Start();
        }

        public void Setup()
        {
            throw new NotImplementedException();
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

        public AxisOffset RequestVisionCalibration(CapturePosition capturePosition, int timeoutMs = 10000)
        {
            if (Connected==false)
            {
                throw new Exception("Vision server not connected.");
            }

            AxisOffset offsetResult = null;
            string JsonCommand = Handle.Instance.ObjToJsonstring(capturePosition);
            Send(JsonCommand);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (ResultFound(capturePosition.CaptureId, out offsetResult) == false)
            {
                if (stopwatch.ElapsedMilliseconds > timeoutMs)
                {
                    throw new Exception("RequestVisionCalibration fail, no response from Vision");
                }

                //_visionResponseManualResetEvent.Reset();
                //_visionResponseManualResetEvent.WaitOne();
                Thread.Sleep(50);
            }

            return offsetResult;
        }

        private bool ResultFound(CaptureId captureId, out AxisOffset offset)
        {
            lock (_captureResultLocker)
            {
                foreach (var result in CaptureResult)
                {
                    if (result.CaptureId == captureId)
                    {
                        offset = result;
                        CaptureResult.Remove(result);
                        return true;
                    }
                }
                offset = null;
                return false;      
            }
        }

        private void ResponseToVision(object obj)
        {
            string json = Bp.Mes.Handle.Instance.ObjToJsonstring(obj);
            Send(json);
        }

        public void ConvertVisonToObject(string json)
        {
            ///反序列化 
            BpHead head = Bp.Mes.Handle.Instance.JsonstringToObj(json);
            string type = head.type.Remove(0, 7);
            Enum.TryParse(type, out CommandType type1);
            object obj = head.obj;

            switch (type1)
            {
                case CommandType.CapturePosition:
                    break;

                case CommandType.AxisPosition:
                    AxisPosition pose = obj as AxisPosition;
                    ReportAxisPosition(pose.CaptureId);
                    break;

                case CommandType.AxisOffset:
                    AddVisionResultToList(obj as AxisOffset);
                    _visionResponseManualResetEvent.Set();
                    break;

                default:
                    throw new NotImplementedException("Vison command type error");
            }
        }

        private void ReportAxisPosition(CaptureId id)
        {
            Task.Run(()=>{
                try
                {
                    AxisPosition pose = new AxisPosition();
                    switch (id)
                    {
                        case CaptureId.LTrayPickTop:
                        //break;
                        case CaptureId.LLoadCompensationBottom:
                        //break;
                        case CaptureId.LLoadHolderTop:
                            pose.CaptureId = id;
                            pose.X = _mc.GetPosition(_mc.MotorLX);
                            pose.Y = _mc.GetPosition(_mc.MotorLY);
                            pose.Z = _mc.GetPosition(_mc.MotorLZ);
                            log.Info("X:" + pose.X + "Y: " + pose.Y +  "Z: " + pose.Z);
                            break;

                        case CaptureId.VTrayPickTop:
                        //break;
                        case CaptureId.VLoadCompensationBottom:
                        //break;
                        case CaptureId.VLoadHolderTop:
                        //break;
                        case CaptureId.VUnloadHolderTop:
                        //break;
                        case CaptureId.VUnloadCompensationBottom:
                        //break;
                        case CaptureId.VTrayPlaceTop:
                            pose.CaptureId = id;
                            pose.X = _mc.GetPosition(_mc.MotorVX);
                            pose.Y = _mc.GetPosition(_mc.MotorVY);
                            break;
                        default:
                            break;
                    }

                    ResponseToVision(pose);
                }
                catch (Exception )
                {
                    //Todo exception.
                    //throw;
                }
            });

        }

        public void AddVisionResultToList(AxisOffset offset)
        {
            lock (_captureResultLocker)
            {
                CaptureResult.Add(offset);
            }
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
                    ReceivedMessage = Encoding.Default.GetString(buffer);
                    Task.Run(() => { ConvertVisonToObject(ReceivedMessage); });                   
                    DataReceieved = true;
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

        public void Send(string cmd)
        {
            try
            {
                DataReceieved = false;
                byte[] buffer = Encoding.Default.GetBytes(cmd);
                _clientSocket.Send(buffer, 0, buffer.Length, 0);
            }
            catch (Exception ex)
            {
                Connected = false;
                _receiveManualResetEvent.Reset();
                _connectManualResetEvent.Set();
            }
        }

    }
}
