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
        double FixtureHeight { get; set; }

        bool VisionSimulateMode { get; set; }

        int CurrentCycleId { get; set; }

        void MoveToSafeHeight();

        void RiseZALittleAndDown();

        void MoveTo(Pose target, MoveModeAMotor mode, ActionType type);
        void MoveToTarget(Pose target, MoveModeAMotor mode, ActionType type);
        void MoveToTarget(CapturePosition target, MoveModeAMotor mode, ActionType type);
        void MoveToCapture(CapturePosition target);
        void SetSpeed(double speed);
        double GetPosition(Motor motor);
        void Setup();
        void Reset();
        Task<WaitBlock> WorkAsync(int cycleId);
        Task<WaitBlock> PrepareAsync();
        Task<WaitBlock> Preparation { get; set; }
        CapturePosition GetCapturePosition(CaptureId id);
        CapturePosition GetCapturePositionOffset(CaptureId id);
        CapturePosition GetCapturePositionWithUserOffset(CaptureId id);
        double GetZHeight(CaptureId id);
        Pose GetVisionResult(CapturePosition pos);
        AxisOffset GetRawVisionResult(CapturePosition capturePosition);
        Pose GetVisionResult(CapturePosition capturePosition, int retryTimes);
        

    }

}