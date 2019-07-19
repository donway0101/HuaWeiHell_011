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
        public void VLoadVacuum(VacuumState state)
        {
            switch (state)
            {
                case VacuumState.On:
                    VacuumOn(Output.VaccumVLoad, Input.VaccumVLoad);
                    break;
                case VacuumState.Off:
                    VacuumOff(Output.VaccumVLoad, Input.VaccumVLoad);
                    break;
                default:
                    break;
            }
        }

        public void VUnloadVacuum(VacuumState state)
        {
            switch (state)
            {
                case VacuumState.On:
                    VacuumOn(Output.VaccumVUnload, Input.VaccumVUnload);
                    break;
                case VacuumState.Off:
                    VacuumOff(Output.VaccumVUnload, Input.VaccumVUnload);
                    break;
                default:
                    break;
            }
        }

        public void LLoadVacuum(VacuumState state)
        {
            switch (state)
            {
                case VacuumState.On:
                    VacuumOn(Output.VaccumLLoad, Input.VaccumLLoad);
                    break;
                case VacuumState.Off:
                    VacuumOff(Output.VaccumLLoad, Input.VaccumLLoad);
                    break;
                default:
                    break;
            }
        }
        
        public void VacuumOn(Output output, Input input, int timeoutMs=3000,
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

        public void VacuumOff(Output output, Input input, int timeoutMs = 3000,
           OutputState outputState = OutputState.Off, bool inputState = false)
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

    public enum VacuumState
    {
        On,
        Off,
    }


}
