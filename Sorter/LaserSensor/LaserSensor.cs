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
    public class LaserSensor : IMachineControl
    {
        private SerialPort _serial;
        private bool _started;
        private readonly string _portName;
        private readonly int _baudRate;
        private readonly Parity _parity;
        private readonly int _dataBit;
        private readonly StopBits _stopBits;

        private readonly object _sendLock = new object();
        private bool _sensorResponsed;
        private string _response;
        private int _id;

        private byte[] requestHeightCommand = new byte[6] { 0x02, 0x43, 0xB0, 0x01, 0x03, 0xF2 };
        private byte[] response = new byte[32];

        public LaserSensor(string portName, int baudRate, int id,
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
                { ReadTimeout = 2000 };
            }

            if (_serial.IsOpen == false)
            {
                _serial.Open();
                _serial.DataReceived -= _serial_DataReceived;
                _serial.DataReceived += _serial_DataReceived;
            }

            //Test();

            _started = true;
        }

        public void Test()
        {
            try
            {
                GetLaserHeight(2);
            }
            catch (TimeoutException)
            {
                throw;
            }
            catch (Exception)
            {
                //Todo ..
            }
        }

        private void _serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //_response += _serial.ReadExisting();
            try
            {
                _serial.Read(response, 0, 20);
                _sensorResponsed = true;
            }
            catch (Exception)
            {
                //
            }          
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
                    throw new Exception("sensor communication exception: " + _id + ex.Message);
                }
            }
        }

        public void SendCmd(byte[] command)
        {
            lock (_sendLock)
            {
                _sensorResponsed = false;
                response = new byte[32];
                try
                {
                    _serial.Write(command, 0, command.Length);
                }
                catch (Exception ex)
                {
                    throw new Exception("Laser sensor communication exception: " + ex.Message);
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


        public double GetLaserHeight(int timeoutSec = 2)
        {
            SendCmd(requestHeightCommand);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (_sensorResponsed == false)
            {
                if (stopwatch.ElapsedMilliseconds > timeoutSec * 1000 / 4)
                {
                    SendCmd(requestHeightCommand);
                }

                if (stopwatch.ElapsedMilliseconds > timeoutSec * 1000 / 2)
                {
                    SendCmd(requestHeightCommand);
                }

                if (stopwatch.ElapsedMilliseconds > timeoutSec * 1000 / 2)
                {
                    SendCmd(requestHeightCommand);
                }

                if (stopwatch.ElapsedMilliseconds > timeoutSec * 1000)
                {
                    throw new TimeoutException("Wait height sensor response timeout: " + _id);
                }
            }

            int rawValue = response[2] * 256 + response[3];
            if (rawValue > 65535 / 2)
            {
                rawValue -= 65534;
            }

            var value = rawValue / 100.0;

            if (Math.Abs(value) > 15)
            {
                throw new Exception("Laser height sensor value out of range: " + _id);
            }

            return value;
        }
    }

}
