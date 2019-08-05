using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sorter
{
    public class LUnloadTrayStation : IMachineControl, ITrayStation
    {
        private readonly MotionController _mc;

        public int TrayLayerNumber { get; set; }
        public int CurrentTrayLayerIndex { get; set; }
        public double TrayLayerHeight { get; set; } = 10.0;
        public Motor MotorTray { get; set; }
        public Motor MotorConveyor { get; set; }

        public double BottomFirstLayerHeight { get; set; } = 118.85;

        public double TraySpeed { get; set; } = 30;
        public double ConveyorSpeed { get; set; } = 30;

        public LUnloadTrayStation(MotionController controller)
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
            //Todo move sensor   
            PushOut();
            DescendOneLayer();
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

            } while (state == false);

            _mc.Stop(MotorConveyor);
        }

        public void ConveyorOut(int timeoutSec = 30)
        {
            _mc.Jog(MotorConveyor, MotorConveyor.Velocity, MoveDirection.Positive);

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

            } while (state == false);
            Delay(3000);
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
            _mc.MoveToTargetTillEnd(MotorTray, BottomFirstLayerHeight);
        }

        public void Ready()
        {
            _mc.MoveToTarget(MotorTray, BottomFirstLayerHeight);
        }

        public void WaitTillReady()
        {
            _mc.WaitTillEnd(MotorTray);
        }

        public void Estop()
        {
            throw new NotImplementedException();
        }

        public bool GetInsideOpticalSensor()
        {
            return _mc.GetOpticalSensor(Input.LUnloadConveyorInsideOpticalSensor);
        }

        public bool GetOutsideOpticalSensor()
        {
            return _mc.GetOpticalSensor(Input.LUnloadConveyorOutsideOpticalSensor);
        }

        public void Home()
        {
            _mc.ZeroPosition(MotorTray);
            _mc.LUnloadTrayCylinder(TrayCylinderState.Retract);
            MotorTray.HomeLimitSpeed = TraySpeed;
            _mc.Home(MotorTray);
        }

        public void WaitTillHomeEnd()
        {
            _mc.WaitTillHomeEnd(MotorTray);
            _mc.ZeroPosition(MotorTray);
        }

        public void LockTray()
        {
            _mc.LLoadConveyorLocker(LockState.On);
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
            //_mc.LUnloadTrayCylinder(TrayCylinderState.PushOut);
            //if (GetOutsideOpticalSensor() == false)
            //{
            //    throw new Exception("Tray stuck on conveyor");
            //}
            //_mc.LUnloadTrayCylinder(TrayCylinderState.Retract);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Push a tray into tray stack from conveyor.
        /// </summary>
        public void PushOut()
        {
            _mc.LUnloadConveyorCylinder(TrayCylinderState.PushOut);
            _mc.LUnloadConveyorCylinder(TrayCylinderState.Retract);
            if (GetOutsideOpticalSensor() == true)
            {
                throw new Exception("Tray stuck on conveyor");
            }          
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

        public void SetSpeed(double speed = 10)
        {
            MotorTray.Velocity = TraySpeed;
            MotorConveyor.Velocity = ConveyorSpeed;
        }

        public void Setup()
        {
            MotorTray = _mc.MotorLTrayUnload;
            MotorConveyor = _mc.MotorLConveyorLoad;
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
            _mc.LLoadConveyorLocker(LockState.Off);
        }
    }
}
