using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sorter
{
    public class VUnloadTrayStation : IMachineControl, ITrayStation
    {
        private readonly MotionController _mc;

        public int TrayLayerNumber { get; set; }
        public int CurrentTrayLayerIndex { get; set; }
        public double TrayLayerHeight { get; set; }
        public Motor MotorTray { get; set; }
        public Motor MotorConveyor { get; set; }

        public VUnloadTrayStation(MotionController controller)
        {
            _mc = controller;
        }

        public void LoadATray(int timeoutSec = 30)
        {
            PushIn();
            ConveyorIn(timeoutSec);
            LockTray();
        }

        public void UnloadATray(int timeoutSec = 30)
        {
            UnlockTray();
            ConveyorOut(timeoutSec);
            PushOut();
        }

        public void ConveyorIn(int timeoutSec = 30)
        {
            _mc.Jog(MotorConveyor, MotorConveyor.Velocity, MoveDirection.Positive);

            var state = false;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                if (stopwatch.ElapsedMilliseconds > timeoutSec*1000)
                {
                    _mc.Stop(MotorConveyor);
                    throw new Exception("Conveyor In timeout V laod tray station.");
                }
                state = GetInsideOpticalSensor();

            } while (state != false);

            _mc.Stop(MotorConveyor);
        }

        public void ConveyorOut(int timeoutSec = 30)
        {
            _mc.Jog(MotorConveyor, MotorConveyor.Velocity, MoveDirection.Negative);

            var state = false;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                if (stopwatch.ElapsedMilliseconds > timeoutSec * 1000)
                {
                    _mc.Stop(MotorConveyor);
                    throw new Exception("Conveyor In timeout V laod tray station.");
                }
                state = GetOutsideOpticalSensor();

            } while (state != false);

            _mc.Stop(MotorConveyor);
        }

        public void Delay(int delayMs = 100)
        {
            Thread.Sleep(delayMs);
        }

        public void DescendOneLayer()
        {
            _mc.MoveToTargetRelativeTillEnd(MotorTray, -TrayLayerHeight);
        }

        public void DescendToBottomLayer()
        {
            _mc.MoveToTargetTillEnd(MotorTray, 0);
        }

        public void Estop()
        {
            throw new NotImplementedException();
        }

        public bool GetInsideOpticalSensor()
        {
            return _mc.GetOpticalSensor(Input.VLoadConveyorInsideOpticalSensor);
        }

        public bool GetOutsideOpticalSensor()
        {
            return _mc.GetOpticalSensor(Input.VLoadConveyorOutsideOpticalSensor);
        }

        public void Home()
        {
            _mc.VLoadTrayCylinder(TrayCylinderState.Retract);
            _mc.Home(MotorTray);
            _mc.WaitTillHomeEnd(MotorTray);
        }

        public void LockTray()
        {
            _mc.VLoadConveyorLocker(LockState.On);
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Push a tray into conveyor from tray stack.
        /// </summary>
        public void PushIn()
        {
            _mc.VLoadConveyorCylinder(TrayCylinderState.PushOut);
            if (GetOutsideOpticalSensor()==true)
            {
                throw new Exception("Tray stuck on conveyor");
            }
            _mc.VLoadConveyorCylinder(TrayCylinderState.Retract);
        }

        /// <summary>
        /// Push a tray into tray stack from conveyor.
        /// </summary>
        public void PushOut()
        {
            _mc.VLoadTrayCylinder(TrayCylinderState.PushOut);
            if (GetOutsideOpticalSensor() == false)
            {
                throw new Exception("Tray stuck on conveyor");
            }
            _mc.VLoadTrayCylinder(TrayCylinderState.Retract);
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void RiseOneLayer()
        {
            _mc.MoveToTargetRelativeTillEnd(MotorTray, TrayLayerHeight);
        }

        public void RiseToTopLayer()
        {
            _mc.MoveToTargetTillEnd(MotorTray, (TrayLayerNumber - 1) * TrayLayerHeight);
        }

        public void SetSpeed(double speed = 1)
        {
            MotorTray.Velocity = speed;
            MotorConveyor.Velocity = speed;
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
            _mc.VLoadConveyorLocker(LockState.Off);
        }
    }
}
