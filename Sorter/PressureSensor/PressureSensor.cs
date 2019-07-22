using System;
using System.Collections.Generic;
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
        private string _portName;
        private int _baudRate;
        private Parity _parity;
        private int _dataBit;
        private StopBits _stopBits;

        private readonly object _sendLock = new object();
        //private const string CmdEnding = "\r";
        private string _response;

        public PressureSensor(string portName, int baudRate,
            Parity serialParity = Parity.None, int serialDataBit = 8,
            StopBits serialStopBits = StopBits.One)
        {
            _portName = portName;
            _baudRate = baudRate;
            _parity = serialParity;
            _dataBit = serialDataBit;
            _stopBits = serialStopBits;
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

            _started = true;
        }

        private void _serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _response += _serial.ReadExisting();
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
    }

}
