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
        public void VLoadVacuum(VacuumState state, bool checkVacuum = true)
        {
            switch (state)
            {
                case VacuumState.On:
                    VacuumOn(Output.VaccumVLoad, Input.VaccumVLoad, checkVacuum);
                    break;
                case VacuumState.Off:
                    VacuumOff(Output.VaccumVLoad, Input.VaccumVLoad, checkVacuum);
                    break;
                default:
                    break;
            }
        }

        public void VUnloadVacuum(VacuumState state, bool checkVacuum = true)
        {
            switch (state)
            {
                case VacuumState.On:
                    VacuumOn(Output.VaccumVUnload, Input.VaccumVUnload, checkVacuum);
                    break;
                case VacuumState.Off:
                    VacuumOff(Output.VaccumVUnload, Input.VaccumVUnload, checkVacuum);
                    break;
                default:
                    break;
            }
        }

        public void LLoadVacuum(VacuumState state, bool checkVacuum = true)
        {
            switch (state)
            {
                case VacuumState.On:
                    VacuumOn(Output.VaccumLLoad, Input.VaccumLLoad, checkVacuum);
                    break;
                case VacuumState.Off:
                    VacuumOff(Output.VaccumLLoad, Input.VaccumLLoad, checkVacuum);
                    break;
                default:
                    break;
            }
        }

        public void Vacuum(VacuumState state, Output output, Input input, 
            bool checkVacuum = true, int delayMs = 500, int timeoutMs = 3000)
        {
            SetOutput(output, Helper.ConvertToOutputState(state));
            bool expectedState = Convert.ToBoolean(state);
            bool inputState = false;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                if (stopwatch.ElapsedMilliseconds > timeoutMs)
                {
                    throw new Exception("Vacuum timeout: " + output);
                }
                inputState = checkVacuum ? GetInput(input) : inputState;
            } while (inputState != expectedState);
            Delay(delayMs);
        }

        public void VacuumOn(Output output, Input input, bool checkVacuum = true,
            int delayMs = 500, int timeoutMs = 4000,
            OutputState outputState = OutputState.On, bool inputState = true)
        {
            SetOutput(output, outputState);
            //Delay(delayMs);
            bool state;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                if (stopwatch.ElapsedMilliseconds > timeoutMs)
                {
                    throw new Exception("Vacuum timeout: " + output);
                }
                state = checkVacuum ? GetInput(input) : inputState;
            } while (state != inputState);          
        }

        public void VacuumOff(Output output, Input input, bool checkVacuum = true,
            int delayMs = 500, int timeoutMs = 3000,
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
                    throw new Exception("Vacuum timeout: " + output);
                }
                state = checkVacuum ? GetInput(input) : inputState;
            } while (state != inputState);
            Delay(delayMs);
        }
    }

    public enum VacuumState
    {
        Off = 0,
        On = 1,      
    }


}
