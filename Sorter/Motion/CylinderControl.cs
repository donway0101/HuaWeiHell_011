using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorter
{
    public partial class MotionController
    {
        public void VUnloadConveyorLocker(LockState state)
        {
            switch (state)
            {
                case LockState.On:
                    CylinderOut(Output.VUnloadConveyorTightUp, Input.VUnloadConveyorTightUpOut);
                    CylinderOut(Output.VUnloadConveyorTightPush, Input.VUnloadConveyorTightPushOut);
                    break;
                case LockState.Off:
                    CylinderIn(Output.VUnloadConveyorTightPush, Input.VUnloadConveyorTightPushIn);
                    CylinderIn(Output.VUnloadConveyorTightUp, Input.VUnloadConveyorTightUpIn);
                    break;
                default:
                    break;
            }
        }

        public void VLoadConveyorLocker(LockState state)
        {
            switch (state)
            {
                case LockState.On:
                    CylinderOut(Output.VLoadConveyorTightUp, Input.VLoadConveyorTightUpOut);
                    CylinderOut(Output.VLoadConveyorTightPush, Input.VLoadConveyorTightPushOut);
                    break;
                case LockState.Off:
                    CylinderIn(Output.VLoadConveyorTightPush, Input.VLoadConveyorTightPushIn);
                    CylinderIn(Output.VLoadConveyorTightUp, Input.VLoadConveyorTightUpIn);
                    break;
                default:
                    break;
            }
        }

        public void LLoadConveyorLocker(LockState state)
        {
            switch (state)
            {
                case LockState.On:
                    LLoadConveyorBlock(LockState.On);
                    CylinderOut(Output.LLoadConveyorTightUp, Input.LLoadConveyorTightUpOut);
                    CylinderOut(Output.LLoadConveyorTightPush, Input.LLoadConveyorTightPushOut);
                    break;
                case LockState.Off:
                    LLoadConveyorBlock(LockState.Off);
                    CylinderIn(Output.LLoadConveyorTightPush, Input.LLoadConveyorTightPushIn);
                    CylinderIn(Output.LLoadConveyorTightUp, Input.LLoadConveyorTightUpIn);
                    break;
                default:
                    break;
            }
        }

        public void LLoadConveyorBlock(LockState state)
        {
            switch (state)
            {
                case LockState.On:
                    CylinderOut(Output.LLoadConveyorBlockUp, Input.LLoadConveyorBlockUpOut);
                    break;
                case LockState.Off:
                    CylinderIn(Output.LLoadConveyorBlockUp, Input.LLoadConveyorBlockUpIn);
                    break;
                default:
                    break;
            }          
        }

        public void VLoadHeadCylinder(HeadCylinderState state)
        {
            switch (state)
            {
                case HeadCylinderState.Up:
                    CylinderIn(Output.VLoadHeadCylinder, Input.VLoadHeadCylinderIn);                 
                    break;
                case HeadCylinderState.Down:
                    CylinderOut(Output.VLoadHeadCylinder, Input.VLoadHeadCylinderOut);
                    break;
                default:
                    break;
            }
        }

        public void VUnloadHeadCylinder(HeadCylinderState state)
        {
            switch (state)
            {
                case HeadCylinderState.Up:
                    CylinderIn(Output.VUnloadHeadCylinder, Input.VUnloadHeadCylinderIn);
                    break;
                case HeadCylinderState.Down:
                    CylinderOut(Output.VUnloadHeadCylinder, Input.VUnloadHeadCylinderOut);
                    break;
                default:
                    break;
            }
        }

        public void CylinderOut(Output output, Input input, int timeoutMs=3000,
            OutputState outputState = OutputState.On, bool inputState=true)
        {
            SetOutput(output, outputState);
            bool state;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                if (stopwatch.ElapsedMilliseconds > timeoutMs)
                {
                    throw new Exception("Cylinder Out timeout: " + output);
                }
                state = GetInput(input);

            } while (state!=inputState);
        }

        public void CylinderIn(Output output, Input input, int timeoutMs = 3000,
           OutputState outputState = OutputState.Off, bool inputState = true)
        {
            SetOutput(output, outputState);
            bool state;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                if (stopwatch.ElapsedMilliseconds > timeoutMs)
                {
                    throw new Exception("Cylinder in timeout: " + output);
                }
                state = GetInput(input);
            } while (state != inputState);
        }
    }

    public enum LockState
    {
        On,
        Off,
    }

    public enum HeadCylinderState
    {
        Up,
        Down,
    }
}
