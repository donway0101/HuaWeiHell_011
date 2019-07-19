using Bp.Mes;
using System.Threading.Tasks;

namespace Sorter
{
    public interface IRobot
    {      
        Motor MotorAngle { get; set; }
        Motor MotorX { get; set; }
        Motor MotorY { get; set; }
        Motor MotorZ { get; set; }
        double SafeXArea { get; set; }
        double SafeYArea { get; set; }
        Tray UnloadTray { get; set; }
        Tray LoadTray { get; set; }
        double LoadTrayHeight { get; set; }
        double UnloadTrayHeight { get; set; }
        bool VacuumSimulateMode { get; set; }
        bool VisionSimulateMode { get; set; }
        

        void Load(Part part);
        void Unload(Part part);
        void UnloadAndLoad(Part part);
        void MoveToTarget(CapturePosition target);
        void MoveToCapture(CapturePosition target);
        void SetSpeed(double speed);
        void Setup();
        void VacuumSucker(VacuumState state);
        Task<WaitBlock> Work();
    }
}