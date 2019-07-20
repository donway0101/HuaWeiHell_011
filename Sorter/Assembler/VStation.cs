using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bp.Mes;

namespace Sorter
{
    public class VStation : IRobot
    {


        public Motor MotorA { get; set; }
        public Motor MotorX { get; set; }
        public Motor MotorY { get; set; }
        public Motor MotorZ { get; set; }
        public double SafeXArea { get; set; }
        public double SafeYArea { get; set; }
        public Tray UnloadTray { get; set; }
        public Tray LoadTray { get; set; }
        public double LoadTrayHeight { get; set; }
        public double UnloadTrayHeight { get; set; }
        public bool VacuumSimulateMode { get; set; }
        public bool VisionSimulateMode { get; set; }
        public double SafeZHeight { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Load(Part part)
        {
            throw new NotImplementedException();
        }

        public void MoveToCapture(CapturePosition target)
        {
            throw new NotImplementedException();
        }

        public void MoveToTarget(CapturePosition target)
        {
            throw new NotImplementedException();
        }

        public void SetSpeed(double speed)
        {
            throw new NotImplementedException();
        }

        public void Setup()
        {
            throw new NotImplementedException();
        }

        public void Unload(Part part)
        {
            throw new NotImplementedException();
        }

        public void UnloadAndLoad(Part part)
        {
            throw new NotImplementedException();
        }

        public void Sucker(VacuumState state)
        {
            throw new NotImplementedException();
        }

        public Task<WaitBlock> Work()
        {
            throw new NotImplementedException();
        }

        public double GetPosition(Motor motor)
        {
            throw new NotImplementedException();
        }
    }
}
