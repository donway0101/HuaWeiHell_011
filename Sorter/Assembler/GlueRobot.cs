using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bp.Mes;

namespace Sorter
{
    public class GlueRobot : IRobot
    {
        private readonly MotionController _mc;
        private CoordinateId _coordinateId;
        private PressureSensor _pressureSensor;
        private LaserSensor _laserSensor;

        public Motor MotorX { get; set; }
        public Motor MotorY { get; set; }
        public Motor MotorZ { get; set; }

        public double CalculationHeight { get; set; }

        /// <summary>
        /// For needle cleaning.
        /// </summary>
        public int GlueCount { get; set; }

        public Pose BottomCameraPose { get; set; }

        public Pose TrayApproachPose { get; set; }

        public double SuckerXOffset { get; set; } = 50.0;

        public Point VisionToSuckerOffset { get; set; }

        public Motor MotorA { get; set; }
        public double SafeXArea { get; set; }
        public double SafeYArea { get; set; }
        public double SafeZHeight { get; set; }
        public Tray UnloadTray { get; set; }
        public Tray LoadTray { get; set; }
        public double LoadTrayHeight { get; set; }
        public double UnloadTrayHeight { get; set; }
        public double FixtureHeight { get; set; }
        public bool CheckVacuumValue { get; set; }
        public bool VisionSimulateMode { get; set; }
        public Task<WaitBlock> Preparation { get; set; }
        public Output NeedleOutput { get; set; }

        /// <summary>
        /// Get it each time change a needle.
        /// </summary>
        public Offset NeedleOffset { get; set; }

        /// <summary>
        /// Get by pressure sensor.
        /// </summary>
        public double NeedleHeight { get; set; }

        /// <summary>
        /// Laser move to the same point to get the height, then calculate it.
        /// </summary>
        public double LaserToNeedleHeightOffset { get; set; }

        public GlueRobot(MotionController controller, CoordinateId id,
            PressureSensor pressureSensor, LaserSensor laserSensor)
        {
            _mc = controller;
            _coordinateId = id;
            _pressureSensor = pressureSensor;
            _laserSensor = laserSensor;
        }

        public void ClearInterpolationBuffer(short bufferId = 0)
        {
            _mc.Stop(MotorX);           
            _mc.SetCoordinateSystem(_coordinateId);
            _mc.ClearInterpolationBuffer(2, _coordinateId, bufferId);
        }

        /// <summary>
        /// Encoder factor is 1000, so synVel will just = motor.Velocity
        /// </summary>
        /// <param name="coordinateId"></param>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <param name="synVel"></param>
        /// <param name="synAcc"></param>
        /// <param name="endVel"></param>
        /// <param name="core"></param>
        /// <param name="fifo"></param>
        public void AddPoint(int xPos, int yPos, int zPos,
            double synVel, double synAcc, double endVel = 0.0, short core = 2, short fifo = 0)
        {
            _mc.AddTargetPoint(core, (short)_coordinateId, xPos, yPos, zPos,
                MotorX.Velocity, MotorX.Acceleration, endVel, fifo);
        }

        private PointPulse ConverDoublePointToPulsePoint(GluePosition point)
        {
            return new PointPulse()
            {
                X = (int)(MotorX.EncoderFactor * point.X * MotorX.Direction),
                Y = (int)(MotorY.EncoderFactor * point.Y * MotorY.Direction),
                Z = (int)(MotorY.EncoderFactor * point.Z * MotorZ.Direction),
            };
        }

        public void AddPoint(GluePosition point, short core=2)
        {
            var target = ConverDoublePointToPulsePoint(point);
            _mc.AddTargetPoint(core, (short)_coordinateId, target.X, target.Y, target.Z,
                MotorX.Velocity, MotorX.Acceleration, 0, 0);
        }

        public void AddDigitalOutput(OutputState state)
        {
            _mc.AddOutput(2, (short)_coordinateId, NeedleOutput, state);           
        }

        public void AddDelay(ushort delayMs)
        {
            _mc.AddDelay((short)_coordinateId, delayMs);
        }

        public void Delay(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }


        public void ShotGlue(ushort delayMs = 600)
        {
            //ClearInterpolationBuffer();

            //AddDigitalOutput(OutputState.On);
            //AddDelay(1000);
            //AddDigitalOutput(OutputState.Off);
            //AddDelay(1000);

            //CheckEnoughSpace();
            //StartInterpolation();
            //WaitTillInterpolationEnd();

            _mc.SetOutput(NeedleOutput, OutputState.On);
            Delay(delayMs);
            _mc.SetOutput(NeedleOutput, OutputState.Off);
        }

        public void PointGlue(GluePosition[] points)
        {
            if (points.Length==0)
            {
                throw new Exception("Not points for the glue.");
            }

            if (_coordinateId == CoordinateId.GlueLine)
            {
                throw new Exception("This method is not for glue line.");
            }

            points[(int)GluePositionType.FirstPoint].GlueShotDelayMs = 100;
            points[(int)GluePositionType.FirstPoint].GlueShotPeriod = 600;

            ClearInterpolationBuffer();

            AddPoint(points[(int)GluePositionType.FirstPointApproach]);

            AddPoint(points[(int)GluePositionType.FirstPoint]);
            AddDigitalOutput(OutputState.On);
            AddDelay(points[(int)GluePositionType.FirstPoint].GlueShotDelayMs);

            AddDigitalOutput(OutputState.Off);
            AddDelay(points[(int)GluePositionType.FirstPoint].GlueShotPeriod);
            AddPoint(points[(int)GluePositionType.FirstPointApproach]);

            CheckEnoughSpace();
            StartInterpolation();
            WaitTillInterpolationEnd();
        }

        public void LineGlue(GluePosition[] points)
        {
            if (points.Length == 0)
            {
                throw new Exception("Not points for the glue.");
            }

            if (_coordinateId == CoordinateId.GluePoint)
            {
                throw new Exception("This method is not for glue line.");
            }

            points[(int)GluePositionType.FirstPoint].GlueShotDelayMs = 100;
            points[(int)GluePositionType.FirstPoint].GlueShotPeriod = 600;

            ClearInterpolationBuffer();

            AddPoint(points[(int)GluePositionType.FirstPointApproach]);

            AddPoint(points[(int)GluePositionType.FirstPoint]);
            AddDigitalOutput(OutputState.On);
            AddDelay(points[(int)GluePositionType.FirstPoint].GlueShotDelayMs);

            AddPoint(points[(int)GluePositionType.SecondPoint]);
            AddDigitalOutput(OutputState.Off);
            AddDelay(points[(int)GluePositionType.SecondPoint].GlueCloseDelayMs);

            AddPoint(points[(int)GluePositionType.SecondPointApproach]);

            CheckEnoughSpace();
            StartInterpolation();
            WaitTillInterpolationEnd();
        }

        public void WaitTillInterpolationEnd()
        {
            _mc.WaitTillInterpolationEnd(_coordinateId);
        }

        public void CheckEnoughSpace()
        {
            if (_mc.IsCrdSpaceEnough((short)_coordinateId) == false)
            {
                throw new Exception("Not enough space in the coordinate system.");
            }
        }

        public void MoveToLaserCrossPoint()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="CalculationHeight"/>
        public void MoveToCalibrationPosition()
        {

        }

        public void SetSpeed(double speed)
        {
            MotorX.Velocity = speed;
            MotorY.Velocity = speed;
            MotorZ.Velocity = speed;
        }

        public void Load(Part part)
        {
            throw new NotImplementedException();
        }

        public void Unload(Part part)
        {
            throw new NotImplementedException();
        }

        public void UnloadAndLoad(Part unload, Part load)
        {
            throw new NotImplementedException();
        }

        public void MoveToTarget(CapturePosition target)
        {
            throw new NotImplementedException();
        }

        public void MoveToCapture(CapturePosition target)
        {
            throw new NotImplementedException();
        }

        public double GetPosition(Motor motor)
        {
            throw new NotImplementedException();
        }

        public void Setup()
        {
            switch (_coordinateId)
            {
                case CoordinateId.None:
                    break;
                case CoordinateId.GluePoint:
                    MotorX = _mc.MotorGluePointX;
                    MotorY = _mc.MotorGluePointY;
                    MotorZ = _mc.MotorGluePointZ;
                    NeedleOutput = Output.GluePoint;
                    break;

                case CoordinateId.GlueLine:
                    MotorX = _mc.MotorGlueLineX;
                    MotorY = _mc.MotorGlueLineY;
                    MotorZ = _mc.MotorGlueLineZ;
                    NeedleOutput = Output.GlueLine;
                    break;

                default:
                    break;
            }
        }

        public void Sucker(VacuumState state)
        {
            throw new NotImplementedException();
        }

        public void Sucker(VacuumState state, int retryTimes, ActionType action)
        {
            throw new NotImplementedException();
        }

        public Task<WaitBlock> Work(GluePosition[] points)
        {
            //PointGlue
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1);
                    PointGlue(points);
                    return new WaitBlock() { Message = "Glue OK" };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "Glue station error: " + ex.Message };
                }
            });
        }

        public Pose GetVisionResult(CapturePosition pos)
        {
            throw new NotImplementedException();
        }

        public CapturePosition GetCapturePosition(CaptureId id)
        {
            throw new NotImplementedException();
        }

        public CapturePosition GetCapturePositionOffset(CaptureId id)
        {
            throw new NotImplementedException();
        }

        public void MoveToSafeHeight()
        {
            throw new NotImplementedException();
        }

        public void MoveToTarget(Pose target)
        {
            throw new NotImplementedException();
        }

        public AxisOffset GetRawVisionResult(CapturePosition capturePosition)
        {
            throw new NotImplementedException();
        }

        public AxisOffset GetVisionResult(CapturePosition capturePosition, int retryTimes)
        {
            throw new NotImplementedException();
        }

        public double GetZHeight(CaptureId id)
        {
            throw new NotImplementedException();
        }

        public Task<WaitBlock> LoadAsync(Part part)
        {
            throw new NotImplementedException();
        }

        public Task<WaitBlock> MoveToNextCaptureAsync(Part part)
        {
            throw new NotImplementedException();
        }

        public void CorrectAngle(double relativeAngle, ActionType action)
        {
            throw new NotImplementedException();
        }

        public void CorrectAngle(ref Part part, ActionType action)
        {
            throw new NotImplementedException();
        }

        public void NgBin(Part part)
        {
            throw new NotImplementedException();
        }

        public void SetNextPartLoad()
        {
            throw new NotImplementedException();
        }

        public void SetNextPartUnload()
        {
            throw new NotImplementedException();
        }

        public void RiseZALittleAndDown()
        {
            throw new NotImplementedException();
        }

        public void StartInterpolation()
        {
            _mc.StartInterpolation(_coordinateId);
        }

        public void MoveRotaryMotor(double angle, MoveMode mode, ActionType type)
        {
            throw new NotImplementedException();
        }

        public Task<WaitBlock> Work()
        {
            throw new NotImplementedException();
        }
    }

    public enum GlueMode
    {
        None = 0,
        Point = 2,
        Line = 1,
    }

    public class GluePosition
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public ushort GlueShotDelayMs { get; set; }
        public ushort GlueCloseDelayMs { get; set; }
        public ushort GlueShotPeriod { get; set; }
        public double GlueLineSpeed { get; set; }
        public double RiseSpeed { get; set; }
    }

    public enum GluePositionType
    {
        FirstPointApproach = 0,
        FirstPoint=1,
        SecondPointApproach=2,
        SecondPoint=3,
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
