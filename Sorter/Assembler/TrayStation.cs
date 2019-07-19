using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorter
{
    public class TrayStation
    {

        private readonly MotionController _controller;
        public int CurrentTrayIndex { get; set; }

        public int TrayNumber { get; set; }

        /// <summary>
        /// Unit mm.
        /// </summary>
        public double TrayHeight { get; set; } = 10.0;

        /// <summary>
        /// Unit mm.
        /// </summary>
        public double HomeOffset { get; set; } = 100;

        public Motor TrayMotor { get; set; }

        public Motor ConveyorMotor { get; set; }

        public Motor OppsiteConveyorMotor { get; set; }

        public Output PushInCylinder { get; set; }

        public Input PushInCylinderOutSensor { get; set; }

        public Input PushInCylinderInSensor { get; set; }

        public Output PushOutCylinder { get; set; }

        public Input PushOutCylinderOutSensor { get; set; }

        public Input PushOutCylinderInSensor { get; set; }

        public Output BlockerCylinder { get; set; }

        public Input BlockerCylinderOutSensor { get; set; }

        public Input BlockerCylinderInSensor { get; set; }

        public Output TightUpCylinder { get; set; }
        public Input  TightUpCylinderOutSensor { get; set; }
        public Input  TightUpCylinderInSensor { get; set; }

        public Output TightPushCylinder { get; set; }
        public Input  TightPushCylinderOutSensor { get; set; }
        public Input  TightPushCylinderInSensor { get; set; }

        public Input InsideOpticalSensor { get; set; }

        public Input OutsideOpticalSensor { get; set; }

        public TrayStation(MotionController controller)
        {
            _controller = controller;
        }

        public void RiseOneLayer()
        {
            _controller.MoveToTargetRelativeTillEnd(TrayMotor, TrayHeight);
        }

        public void LowerOneLayer()
        {
            _controller.MoveToTargetRelativeTillEnd(TrayMotor, -TrayHeight);
        }

        public void GoToTopLayer()
        {
            _controller.MoveToTargetTillEnd(TrayMotor, -TrayHeight * TrayNumber);
        }

        public void GoToBottomLayer()
        {
            _controller.MoveToTargetTillEnd(TrayMotor, 0);
        }

        public void PushIn()
        {
            _controller.CylinderOut(PushInCylinder, PushInCylinderOutSensor);
            _controller.CylinderIn(PushInCylinder, PushInCylinderInSensor);
            _controller.CheckSensor(OutsideOpticalSensor, false);
        }

        public void PushOut()
        {
            _controller.CylinderOut(PushOutCylinder, PushOutCylinderOutSensor);
            _controller.CylinderIn(PushOutCylinder, PushOutCylinderInSensor);
            _controller.CheckSensor(OutsideOpticalSensor, true);
        }

        public void TightTray(bool riseConveyorBlocker = false)
        {
            _controller.CylinderOut(TightUpCylinder, TightUpCylinderOutSensor);
            _controller.CylinderOut(TightPushCylinder, TightPushCylinderOutSensor);
        }

        public void LooseTray(bool riseConveyorBlocker = false)
        {            
            _controller.CylinderIn(TightPushCylinder, TightPushCylinderInSensor);
            _controller.CylinderIn(TightUpCylinder, TightUpCylinderInSensor);
        }

        public void RiseConveyorBloker()
        {
            _controller.CylinderOut(BlockerCylinder, BlockerCylinderOutSensor);
        }

        /// <summary>
        /// At postion of to bottom layer.
        /// </summary>
        public void Home()
        {
            _controller.Home(_controller.MotorVTrayLoad);
            _controller.WaitTillHomeEnd(_controller.MotorVTrayLoad);
        }

        //Todo sensor may not stable.
        public void ConveyorDeliverIn(int timeoutSec = 30)
        {
            _controller.Jog(ConveyorMotor, ConveyorMotor.Velocity, MoveDirection.Positive);
            bool state;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                if (stopwatch.ElapsedMilliseconds > timeoutSec * 1000)
                {
                    throw new Exception("ConveyorDeliverIn timeout");
                }
                state = _controller.GetInput(InsideOpticalSensor);
            } while (state != true);
            ConveyorStop();
        }

        public void ConveyorDeliverOut(int timeoutSec = 30)
        {
            _controller.Jog(ConveyorMotor, ConveyorMotor.Velocity, MoveDirection.Negative);
            bool state;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                if (stopwatch.ElapsedMilliseconds > timeoutSec * 1000)
                {
                    throw new Exception("ConveyorDeliverOut timeout");
                }
                state = _controller.GetInput(InsideOpticalSensor);
            } while (state != true);
            ConveyorStop();
        }

        public void ConveyorStop()
        {
            _controller.Stop(ConveyorMotor);
        }

        public void LoadTray()
        {
            PushOut();
            ConveyorDeliverIn();
            TightTray();
        }

        public void UnloadTray()
        {
            LooseTray();
            ConveyorDeliverOut();
            PushIn();
        }
    }
}
