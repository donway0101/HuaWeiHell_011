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
        public void LUnloadConveyorCylinder(TrayCylinderState state)
        {
            switch (state)
            {
                case TrayCylinderState.PushOut:
                    CylinderOut(Output.LUnloadConveyorCylinder, Input.LUnloadConveyorCylinderOut);
                    break;
                case TrayCylinderState.Retract:
                    CylinderIn(Output.LUnloadConveyorCylinder, Input.LUnloadConveyorCylinderIn);
                    break;
                default:
                    break;
            }
        }

        public void LLoadConveyorCylinder(TrayCylinderState state)
        {
            switch (state)
            {
                case TrayCylinderState.PushOut:
                    CylinderOut(Output.LLoadConveyorCylinder, Input.LLoadConveyorCylinderOut);
                    break;
                case TrayCylinderState.Retract:
                    CylinderIn(Output.LLoadConveyorCylinder, Input.LLoadConveyorCylinderIn);
                    break;
                default:
                    break;
            }
        }

        public void VUnloadConveyorCylinder(TrayCylinderState state)
        {
            switch (state)
            {
                case TrayCylinderState.PushOut:
                    CylinderOut(Output.VUnloadConveyorCylinder, Input.VUnloadConveyorCylinderOut);
                    break;
                case TrayCylinderState.Retract:
                    CylinderIn(Output.VUnloadConveyorCylinder, Input.VUnloadConveyorCylinderIn);
                    break;
                default:
                    break;
            }
        }

        public void VLoadConveyorCylinder(TrayCylinderState state)
        {
            switch (state)
            {
                case TrayCylinderState.PushOut:
                    CylinderOut(Output.VLoadConveyorCylinder, Input.VLoadConveyorCylinderOut);
                    break;
                case TrayCylinderState.Retract:
                    CylinderIn(Output.VLoadConveyorCylinder, Input.VLoadConveyorCylinderIn);
                    break;
                default:
                    break;
            }
        }

        public void LUnloadTrayCylinder(TrayCylinderState state)
        {
            switch (state)
            {
                case TrayCylinderState.PushOut:
                    CylinderOut(Output.LUnloadTrayCylinder, Input.LUnloadTrayCylinderOut);
                    break;
                case TrayCylinderState.Retract:
                    CylinderIn(Output.LUnloadTrayCylinder, Input.LUnloadTrayCylinderIn);
                    break;
                default:
                    break;
            }
        }

        public void LLoadTrayCylinder(TrayCylinderState state)
        {
            switch (state)
            {
                case TrayCylinderState.PushOut:
                    CylinderOut(Output.LLoadTrayCylinder, Input.LLoadTrayCylinderOut);
                    break;
                case TrayCylinderState.Retract:
                    CylinderIn(Output.LLoadTrayCylinder, Input.LLoadTrayCylinderIn);
                    break;
                default:
                    break;
            }
        }

        public void VUnloadTrayCylinder(TrayCylinderState state)
        {
            switch (state)
            {
                case TrayCylinderState.PushOut:
                    CylinderOut(Output.VUnloadTrayCylinder, Input.VUnloadTrayCylinderOut);
                    break;
                case TrayCylinderState.Retract:
                    CylinderIn(Output.VUnloadTrayCylinder, Input.VUnloadTrayCylinderIn);
                    break;
                default:
                    break;
            }
        }

        public void VLoadTrayCylinder(TrayCylinderState state)
        {
            switch (state)
            {
                case TrayCylinderState.PushOut:
                    CylinderOut(Output.VLoadTrayCylinder, Input.VLoadTrayCylinderOut);
                    break;
                case TrayCylinderState.Retract:
                    CylinderIn(Output.VLoadTrayCylinder, Input.VLoadTrayCylinderIn);
                    break;
                default:
                    break;
            }
        }

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

        public void CylinderOut(Output output, Input input, int timeoutMs=5000,
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

        public void CylinderIn(Output output, Input input, int timeoutMs = 5000,
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


    public struct CylinderIO
    {
        public Output Output { get; set; }
        public Input InputIn { get; set; }
        public Input InputOut { get; set; }
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

    public enum TrayCylinderState
    {
        PushOut,
        Retract,
    }
}
