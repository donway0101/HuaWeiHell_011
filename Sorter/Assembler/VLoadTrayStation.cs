using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorter
{
    public class VLoadTrayStation : IMachineControl, ITrayStation
    {
        private readonly MotionController _mc;

        public int TrayLayerNumber { get; set; }
        public int CurrentTrayLayerIndex { get; set; }
        public double TrayLayerHeight { get; set; }
        public Motor MotorTray { get; set; }
        public Motor MotorConveyor { get; set; }

        public VLoadTrayStation(MotionController controller)
        {
            _mc = controller;
        }

        public void ConveyorIn()
        {
            throw new NotImplementedException();
        }

        public void ConveyorOut()
        {
            throw new NotImplementedException();
        }

        public void Delay(int delayMs = 100)
        {
            throw new NotImplementedException();
        }

        public void DescendOneLayer()
        {
            throw new NotImplementedException();
        }

        public void DescendToBottomLayer()
        {
            throw new NotImplementedException();
        }

        public void Estop()
        {
            throw new NotImplementedException();
        }

        public bool GetInsideOpticalSensor()
        {
            throw new NotImplementedException();
        }

        public bool GetOutsideOpticalSensor()
        {
            throw new NotImplementedException();
        }

        public void Home()
        {
            throw new NotImplementedException();
        }

        public void LockTray()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void PushIn()
        {
            throw new NotImplementedException();
        }

        public void PushOut()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void RiseOneLayer()
        {
            throw new NotImplementedException();
        }

        public void RiseToTopLayer()
        {
            throw new NotImplementedException();
        }

        public void SetSpeed(double speed = 1)
        {
            throw new NotImplementedException();
        }

        public void Setup()
        {
            MotorTray = _mc.MotorVTrayLoad;
            MotorConveyor = _mc.MotorVConveyorUnload;
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void UnlockTray()
        {
            throw new NotImplementedException();
        }
    }
}
