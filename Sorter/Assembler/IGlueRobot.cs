using System.Threading.Tasks;
using Bp.Mes;

namespace Sorter
{
    public interface IGlueRobot
    {
        double GlueRadius { get; set; }
        double NeedleOnZHeight { get; set; }
        double LaserRefereceZHeight { get; set; }
        double NeedleOnZHeightUserCompensation { get; set; }
        Offset NeedleOffset { get; set; }
        Output NeedleOutput { get; set; }

        bool VisionSimulateMode { get; set; }

        void ShotGlue(ushort delayMs = 600);
        void CloseGlue();
        void AddDelay(ushort delayMs);
        void AddDigitalOutput(OutputState state);
        void AddPoint(GluePosition point, short core = 2);
        void AddPoint(int xPos, int yPos, int zPos, double synVel, double synAcc, double endVel = 0, short core = 2, short fifo = 0);
        PointPulse ConverToPulsePoint(GluePosition point);
        void CheckEnoughSpace();
        void ClearInterpolationBuffer();
        void StartInterpolation();
        void WaitTillInterpolationEnd();

        void GetLaserRefereceHeight();
        void CaptureNeedleHeight();
        double GetLaserHeightValue();
        double GetPressureValue();
        double GetWorkPointHeight();

        void Delay(int millisecondsTimeout);
        CapturePosition GetCapturePosition(CaptureId id);
        CapturePosition GetCapturePositionOffset(CaptureId id);
               
        AxisOffset GetRawVisionResult(CapturePosition capturePosition);
        Pose GetVisionResult(CapturePosition pos);
        AxisOffset GetVisionResult(CapturePosition capturePosition, int retryTimes);
        Pose[] GetVisionResultsLaserAndWork(CapturePosition pos);

        double GetPosition(Motor motor);
        Pose[] GetWorkPoses();
        double GetZHeight(CaptureId id);
        void MoveToCapture(CapturePosition target);
        void MoveToCapture(Pose target);
        void MoveToSafeHeight();
        void MoveToTarget(CapturePosition target);
        void MoveToTarget(Pose target);            
    }

    public class GluePosition
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double XCircleCenter { get; set; }
        public double YCircleCenter { get; set; }

        public ushort GlueShotDelayMs { get; set; }
        public ushort GlueCloseDelayMs { get; set; }
        public ushort GlueLeaveDelayMs { get; set; }
        public ushort GlueShotPeriod { get; set; }
        public double GlueLineSpeed { get; set; }
        public double RiseSpeed { get; set; }
    }

    public enum GluePositionType
    {
        FirstPointApproach = 0,
        FirstPoint = 1,
        SecondPointApproach = 2,
        SecondPoint = 3,
        ThirdPointApproach = 4,
        ThirdPoint = 5,
        FourthPointApproach = 6,
        FourthPoint = 7,
    }

    public class Offset
    {
        public double XOffset { get; set; }
        public double YOffset { get; set; }
    }

}