using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sorter
{
    public class PressureSensor : IMachineControl
    {
        private SerialPort _serial;
        private bool _started;
        private readonly string _portName;
        private readonly int _baudRate;
        private readonly Parity _parity;
        private readonly int _dataBit;
        private readonly StopBits _stopBits;
        private readonly int _id;

        private readonly object _sendLock = new object();
        //private const string CmdEnding = "\r";
        private string _response;

        private bool _pressureUpdated = false;

        public double PressureValue { get; set; }

        public PressureSensor(string portName, int baudRate, int id,
            Parity serialParity = Parity.None, int serialDataBit = 8,
            StopBits serialStopBits = StopBits.One)
        {
            _portName = portName;
            _baudRate = baudRate;
            _parity = serialParity;
            _dataBit = serialDataBit;
            _stopBits = serialStopBits;
            _id = id;
        }

        public void Start()
        {
            if (_started)
            {
                return;
            }

            if (_serial == null)
            {
                _serial = new SerialPort(
                    _portName, _baudRate, _parity, _dataBit, _stopBits)
                { ReadTimeout = 1000 };
            }

            if (_serial.IsOpen == false)
            {
                _serial.Open();
                _serial.DataReceived -= _serial_DataReceived;
                _serial.DataReceived += _serial_DataReceived;
            }

            //GetPressureValue(500);

            _started = true;
        }

        private void _serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _response += _serial.ReadExisting();
            UpdateValue(_response);
        }

        public void Stop()
        {
            if (_serial != null)
            {
                _serial.Close();
                _serial.Dispose();
                Delay(500);
            }
        }

        public void SendCmd(string command)
        {
            lock (_sendLock)
            {
                _response = string.Empty;
                string cmd = command; // Maybe need ending.
                try
                {
                    _serial.Write(cmd);
                }
                catch (Exception ex)
                {
                    throw new Exception("Pressure sensor communication exception: " + ex.Message);
                }
            }
        }

        private void Delay(int milliSecond)
        {
            Thread.Sleep(milliSecond);
        }

        public void SetSpeed(double speed = 1)
        {
            throw new NotImplementedException();
        }

        public void Setup()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Estop()
        {
            throw new NotImplementedException();
        }

        void IMachineControl.Delay(int delayMs)
        {
            throw new NotImplementedException();
        }

        public void Test()
        {
            GetPressureValue(1000);
        }

        public double GetPressureValue(int timeoutMs = 100)
        {
            _pressureUpdated = false;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            do
            {
                if (_pressureUpdated)
                {
                    return PressureValue;
                }

                if (stopwatch.ElapsedMilliseconds > timeoutMs)
                {
                    throw new Exception("Get pressure sensor timeout: " + _id);
                }

            } while (true);
        }

        public void UpdateValue(string _recvString)
        {
            //_recvString += _serialPort.ReadExisting();
            try
            {
                if (_recvString.Length == 0)
                    return;

                string[] sdata = _recvString.Split(new char[] { (char)0x0D, (char)0x0A },
                    StringSplitOptions.RemoveEmptyEntries);

                string svalues;

                if (_recvString[_recvString.Length - 1] == (char)0x0D ||
                    _recvString[_recvString.Length - 1] == (char)0x0A)
                {
                    _recvString = "";
                    _response = "";
                    svalues = sdata[sdata.Length - 1];
                }
                else
                {
                    _recvString = sdata[sdata.Length - 1];
                    if (sdata.Length > 1)
                    {
                        svalues = sdata[sdata.Length - 2];
                    }
                    else
                    {
                        svalues = "";
                    }
                }

                if (svalues.Length > 0)
                {
                    string[] values = svalues.Split(new char[] { ',' });
                    if (values.Length == 3)
                    {
                        PressureValue = double.Parse(values[2]);
                        _pressureUpdated = true;
                    }
                }
            }
            catch (Exception ex)
            {
                //Todo debug.
                //throw new Exception("Pressure sensor data error:" + ex.Message);
            }

            
        }
    }

}
