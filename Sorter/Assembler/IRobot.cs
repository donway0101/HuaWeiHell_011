using Bp.Mes;
using System.Threading.Tasks;

namespace Sorter
{
    public interface IRobot
    {      
        Motor MotorA { get; set; }
        Motor MotorX { get; set; }
        Motor MotorY { get; set; }
        Motor MotorZ { get; set; }
        double SafeXArea { get; set; }
        double SafeYArea { get; set; }
        double SafeZHeight { get; set; }
        Tray UnloadTray { get; set; }
        Tray LoadTray { get; set; }
        double LoadTrayHeight { get; set; }
        double UnloadTrayHeight { get; set; }
        double FixtureHeight { get; set; }
        bool CheckVacuumValue { get; set; }
        bool VisionSimulateMode { get; set; }        

        void Load(Part part);
        void Unload(Part part);
        void UnloadAndLoad(Part unload, Part load);
        void MoveToTarget(CapturePosition target);
        void MoveToCapture(CapturePosition target);
        void SetSpeed(double speed);
        double GetPosition(Motor motor);
        void Setup();
        void Sucker(VacuumState state);
        void Sucker(VacuumState state, int retryTimes, ActionType action);
        Task<WaitBlock> Work();

        Pose GetVisionResult(CapturePosition pos);
        CapturePosition GetCapturePosition(CaptureId id);
        CapturePosition GetCapturePositionOffset(CaptureId id);
        void MoveToSafeHeight();
        void MoveToTarget(Pose target);
        AxisOffset GetRawVisionResult(CapturePosition capturePosition);
        AxisOffset GetVisionResult(CapturePosition capturePosition, int retryTimes);
        double GetZHeight(CaptureId id);
        Task<WaitBlock> LoadAsync(Part part);
        Task<WaitBlock> MoveToNextCaptureAsync(Part part);
        void CorrectAngle(double relativeAngle, ActionType action);
        void CorrectAngle(ref Part part, ActionType action);
        Task<WaitBlock> Preparation { get; set; }
        void NgBin(Part part);
        void SetNextPartLoad();
        void SetNextPartUnload();
        void RiseZALittleAndDown();
        void MoveRotaryMotor(double angle, MoveMode mode, ActionType type);

    }

    public enum StationId
    {
        V,
        L,
        GLine,
        GPoint,
    }

    /// <summary>
    /// For conveyors, and sucker vacuum head.
    /// </summary>
    public enum ActionType
    {
        None,
        Load,
        Unload,
    }
}