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

        void MoveToSafeHeight();
        
        void MoveTo(Pose target, MoveModeAMotor mode, ActionType type);
        void MoveToTarget(Pose target, MoveModeAMotor mode, ActionType type);
        void MoveToTarget(CapturePosition target, MoveModeAMotor mode, ActionType type);
        void MoveToCapture(CapturePosition target);
        void SetSpeed(double speed);
        double GetPosition(Motor motor);
        void Setup();
        Task<WaitBlock> WorkAsync();
       
        CapturePosition GetCapturePosition(CaptureId id);
        CapturePosition GetCapturePositionOffset(CaptureId id);
        double GetZHeight(CaptureId id);
        Pose GetVisionResult(CapturePosition pos);
        AxisOffset GetRawVisionResult(CapturePosition capturePosition);
        Pose GetVisionResult(CapturePosition capturePosition, int retryTimes);
        //Task<WaitBlock> PreparationAsync { get; set; }

    }

    public enum StationId
    {
        V,
        L,
        GlueCurve,
        GluePoint,
    }
}