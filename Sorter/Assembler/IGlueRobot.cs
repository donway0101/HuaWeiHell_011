using System.Diagnostics;
using System.Threading.Tasks;
using Bp.Mes;

namespace Sorter
{
    public interface IGlueRobot
    {
        GlueParameter GlueParas { get; set; }

        /// <summary>
        /// From capture position, we know where to approach needle.
        /// </summary>
        /// <seealso cref="CaptureId.GlueLineNeedleOnPressureSensor"/>
        double NeedleOnZHeight { get; set; }

        /// <summary>
        /// Pressure will change in shape after pressure and laser will not,
        /// so the actual height of needle need a compensation.
        /// </summary>
        double NeedleOnZHeightCompensation { get; set; }

        /// <summary>
        /// When needle just touch a surface, the value shows on laser sensor.
        /// </summary>
        double LaserRefereceZHeight { get; set; }

        /// <summary>
        /// Tip x and y Offsets between new needle and old needle.
        /// </summary>
        Offset NeedleOffset { get; set; }

        Output NeedleOutput { get; set; }

        Output NeedleCleanPool { get; set; }

        bool VisionSimulateMode { get; set; }

        void ShotGlue(ushort delayMs = 600);
        void CloseGlue();
        void AddDelay(ushort delayMs);
        void AddDigitalOutput(OutputState state);
        void AddPoint(PointInfo point);
        PointInfoPulse ConvertToPulse(PointInfo point);
        void CheckEnoughSpace();
        void ClearInterpolationBuffer();
        void StartInterpolation();
        void WaitTillInterpolationEnd();

        double FindLaserRefereceHeight();
        double FindNeedleHeight();
        double GetLaserHeightValue();
        double GetPressureValue();
        void CalibrateNeedle();

        void Delay(int millisecondsTimeout);
        CapturePosition GetCapturePosition(CaptureId id);
        CapturePosition GetCapturePositionOffset(CaptureId id);
               
        AxisOffset GetRawVisionResult(CapturePosition capturePosition);
        Pose GetVisionResult(CapturePosition pos);
        GlueTargets GetVisionResultsForLaserAndWork();

        double GetPosition(Motor motor);
        double GetZHeight(CaptureId id);
        void MoveToCapture(CapturePosition target);
        void MoveToCapture(Pose target);
        void MoveToSafeHeight();
        void MoveToTarget(CapturePosition target);
        void MoveToTarget(Pose target);

        Task<WaitBlock> ShotGlueAsync(ushort delayMs);

        double CameraAboveChinaHeight { get; set; }
        double CameraAboveFixtureHeight { get; set; }
        int CurrentCycleId { get; set; }
        double DetectNeedleHeightSpeed { get; set; }
        double FixtureHeight { get; set; }
        double LaserAboveFixtureHeight { get; set; }
        double LaserSurfaceToNeckHeightOffset { get; set; }
        double LaserSurfaceToSpringHeightOffset { get; set; }

        double NeedleTouchPressure { get; set; }
        Task<WaitBlock> Preparation { get; set; }

        Stopwatch NeedleCleaningStopWatch { get; set; }
        int NeedleCleaningIntervalSec { get; set; }

        void AddPoint(PointInfo point, double speed);
        Task<WaitBlock> CalibrateNeedleAsync();
        void CleanNeedle(int shotSec = 20);
        Task<WaitBlock> CleanNeedleAsync(int delaySec = 30);
        ArcInfoPulse ConvertToPulse(ArcInfo arcInfo);
        Task<WaitBlock> FindLaserRefereceHeightAsync();
        Task<WaitBlock> FindNeedleHeightAsync();
        Offset FindNeedleOffset();
        CapturePosition GetCapturePositionWithUserOffset(CaptureId id);
        double GetNeedleToWorkSurfaceHeight();
        AxisOffset GetRawVisionResult(CapturePosition capturePosition, int retryTimes);
        double[] GetSurfaceDistance(GlueTargets targets);
        Pose GetVisionResult(CapturePosition capturePosition, int retryTimes);
        Task<WaitBlock> GetVisionResultsForLaserAndWorkAsync();
        void Glue(GlueTargets glueTargets, GlueParameter gluePara);
        void MoveTo(Pose target, MoveModeAMotor mode, ActionType type);
        void MoveToTarget(CapturePosition target, MoveModeAMotor mode, ActionType type);
        void MoveToTarget(Pose target, MoveModeAMotor mode, ActionType type);
        Task<WaitBlock> PrepareAsync();
        void Reset();
        void SetSpeed(double speed);
        void Setup();
        void ShotGlueOutput(int delaySec = 10);
        //void Work(GlueParameters gluePara);
        Task<WaitBlock> WorkAsync(int cycleId);
        //Task<WaitBlock> WorkAsync(int cycleId, GlueParameters gluePara);
    }

    public class Offset
    {
        public double XOffset { get; set; }
        public double YOffset { get; set; }
    }

    /// <summary>
    /// Arc info from camera.
    /// </summary>
    public class ArcInfo
    {
        public double XStart { get; set; }
        public double YStart { get; set; }
        public double XEnd { get; set; }
        public double YEnd { get; set; }
        public double XCenter { get; set; }
        public double YCenter { get; set; }
        public double Z { get; set; }
        public double CenterToStartXOffset { get; set; }
        public double CenterToStartYOffset { get; set; }

        /// <summary>
        /// Calculate arc center to arc start point offset.
        /// </summary>
        public void CalculateCenterOffset()
        {
            CenterToStartXOffset = XCenter - XStart;
            CenterToStartYOffset = YCenter - YStart;
        }
    }

    /// <summary>
    /// Point info from the camera.
    /// </summary>
    public class PointInfo
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    public class GroupPointInfo
    {
        public PointInfo[] Group1Points { get; set; }
        public PointInfo[] Group2Points { get; set; }
        public PointInfo[] Group3Points { get; set; }
        public PointInfo[] Group4Points { get; set; }
    }

    /// <summary>
    /// For laser height measure and glue point and glue arc.
    /// </summary>
    public class GlueTargets
    {
        public Pose[] LaserTargets { get; set; }
        public ArcInfo[] ArcTargets { get; set; }
        public PointInfo[] PointTargets { get; set; }
        public GroupPointInfo GroupPoints { get; set; }
    }

    public class GlueParameter
    {
        public CoordinateId Id { get; set; }

        public double GlueRadius { get; set; }

        public int PreShotTime { get; set; }

        public double GlueSpeed { get; set; }

        public double[] GlueHeightOffset { get; set; }

        public int GluePeriod { get; set; }

        public double RiseGlueSpeed { get; set; }

        public double RiseGlueHeight { get; set; }

        public int CloseGlueDelay { get; set; }

        public int PreClosePercentage { get; set; }

        public int SecondLineLessPreShot { get; set; }
    }

}