using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bp.Mes;

namespace Sorter
{
    public class GlueCurveStation : IRobot, IGlueRobot
    {
        private readonly MotionController _mc;
        private readonly CoordinateId _coordinateId;
        private readonly PressureSensor _pressureSensor;
        private readonly LaserSensor _laserSensor;
        private readonly List<CapturePosition> _capturePositions;
        private readonly List<CapturePosition> _userOffsets;
        private readonly VisionServer _vision;
        private readonly object _workLock = new object();

        public Motor MotorA { get; set; }
        public Motor MotorX { get; set; }
        public Motor MotorY { get; set; }
        public Motor MotorZ { get; set; }
        public double SafeXArea { get; set; }
        public double SafeYArea { get; set; }
        public double SafeZHeight { get; set; } = -5;
        public double FixtureHeight { get; set; } = -20.45;
        public bool VisionSimulateMode { get; set; }        
        public double NeedleOnZHeight { get; set; }
        public Offset NeedleOffset { get; set; }
        public Output NeedleOutput { get; set; }
        public double GlueRadius { get; set; } = 0.2;
        public double GlueFinishRiseHeight { get; set; } = 0.2;
        public double GlueFinishLeaveHeight { get; set; } = 2;

        public double WorkSpeed { get; set; } = 10;
        public double DetectNeedleHeightSpeed { get; set; } = 0.05;
        public double NeedleTouchPressure { get; set; } = 0.02;
        public double LaserRefereceZHeight { get; set; }
        public double NeedleOnZHeightUserCompensation { get; set; }

        public GlueCurveStation(MotionController controller, VisionServer vision, CoordinateId id,
             List<CapturePosition> positions, List<CapturePosition> offsets,
            PressureSensor pressureSensor, LaserSensor laserSensor)
        {
            _mc = controller;
            _coordinateId = id;
            _capturePositions = positions;
            _userOffsets = offsets;
            _pressureSensor = pressureSensor;
            _laserSensor = laserSensor;
            _vision = vision;
        }

        public void AddDelay(ushort delayMs)
        {
            _mc.AddDelay((short)_coordinateId, delayMs);
        }

        public void AddDigitalOutput(OutputState state)
        {
            _mc.AddOutput(2, (short)_coordinateId, NeedleOutput, state);
        }

        public PointPulse ConverToPulsePoint(GluePosition point)
        {
            return new PointPulse()
            {
                X = (int)(MotorX.EncoderFactor * point.X * MotorX.Direction),
                Y = (int)(MotorY.EncoderFactor * point.Y * MotorY.Direction),
                Z = (int)(MotorY.EncoderFactor * point.Z * MotorZ.Direction),
                ArcCenterToXOffset = (int)(MotorX.EncoderFactor * point.XCircleCenter * MotorX.Direction),
                ArcCenterToYOffset = (int)(MotorY.EncoderFactor * point.YCircleCenter * MotorY.Direction),
            };
        }

        public PointPulse ConverToPulsePoint(ArcInfo arcInfo)
        {
            return new PointPulse()
            {
                X = (int)(MotorX.EncoderFactor * arcInfo.XEnd * MotorX.Direction),
                Y = (int)(MotorY.EncoderFactor * arcInfo.YEnd * MotorY.Direction),
                ArcCenterToXOffset = (int)(MotorX.EncoderFactor * arcInfo.CenterToStartXOffset * MotorX.Direction),
                ArcCenterToYOffset = (int)(MotorY.EncoderFactor * arcInfo.CenterToStartYOffset * MotorY.Direction),
            };
        }

        public void AddPoint(GluePosition point, short core = 2)
        {
            var target = ConverToPulsePoint(point);
            _mc.AddTargetPoint(core, (short)_coordinateId, target.X, target.Y, target.Z,
                MotorX.Velocity, MotorX.Acceleration, 0, 0);
        }

        public void AddArc(ArcInfo arcInfo)
        {
            var target = ConverToPulsePoint(arcInfo);
            _mc.AddArcXYC(2, (short)_coordinateId, target.X, target.Y, target.ArcCenterToXOffset,
                target.ArcCenterToYOffset, MotorX.Velocity, MotorX.Acceleration);
        }

        public void AddArc(GluePosition point, short core = 2)
        {
            var target = ConverToPulsePoint(point);
            var xCenterOffset = target.ArcCenterToXOffset - target.X;
            var yCenterOffset = target.ArcCenterToYOffset - target.Y;
            _mc.AddArcXYC(core, (short)_coordinateId, target.X, target.Y, xCenterOffset,
                yCenterOffset, MotorX.Velocity, MotorX.Acceleration);
        }

        public void AddPoint(int xPos, int yPos, int zPos,
            double synVel, double synAcc, double endVel = 0.0, short core = 2, short fifo = 0)
        {
            _mc.AddTargetPoint(core, (short)_coordinateId, xPos, yPos, zPos,
                MotorX.Velocity, MotorX.Acceleration, endVel, fifo);
        }

        public void GetLaserRefereceHeight()
        {
            var laserCapture = GetCapturePosition(CaptureId.GlueCurveLaser);
            MoveToCapture(laserCapture);
            var currentZHeight = GetPosition(MotorZ);
            var LaserToNeedleHeightOffset = currentZHeight - NeedleOnZHeight;
            LaserRefereceZHeight = GetLaserHeightValue() - LaserToNeedleHeightOffset;
        }

        /// <summary>
        /// Farther or close.
        /// </summary>
        /// <returns>If bigger than 0, Z neeed to move down</returns>
        public double GetNeedleToWorkPointHeightOffset(CapturePosition capturePos,
            double sensorPointToWorkPointHeightOffset)
        {
            MoveToCapture(capturePos);
            var currentZHeight = GetPosition(MotorZ);
            var deltaZ = currentZHeight - sensorPointToWorkPointHeightOffset;
            return GetLaserHeightValue() - LaserRefereceZHeight - deltaZ;
        }

        /// <summary>
        /// Go to china to calibrate, theoretically very small.
        /// </summary>
        /// <param name="calibratePoint"></param>
        /// <returns></returns>
        public double CheckLaserData(CapturePosition calibratePoint)
        {
            var laserSensorCap = GetCapturePosition(CaptureId.GlueCurveLaser);
            var chinaCalibrateCap = GetCapturePosition(CaptureId.GlueCurveCalibration);
            var zOffset = chinaCalibrateCap.ZPosition - laserSensorCap.ZPosition;
            var heightOffset = 
                GetNeedleToWorkPointHeightOffset(GetCapturePosition(CaptureId.GlueCurveLaser), zOffset);
            return heightOffset;
        }

        /// <summary>
        /// Detect needle height by touching pressure sensor.
        /// Saved needle to sensor height error less than 2 mm.
        /// </summary>
        /// <returns></returns>
        public void CaptureNeedleHeight()
        {
            var needleCapture = GetCapturePosition(CaptureId.GlueCurveNeedle);
            needleCapture.ZPosition += 2;
            MoveToCapture(needleCapture);

            needleCapture.ZPosition -= 2;
            var tempSpeed = WorkSpeed;
            SetSpeed(DetectNeedleHeightSpeed);
            ApproachPressureSensor(needleCapture.ZPosition);

            //Restore speed.
            SetSpeed(tempSpeed);
            MoveToSafeHeight();
        }

        private void ApproachPressureSensor(double touchPointHeight, int timeoutSec = 120)
        {
            //Max down 0.5mm
            touchPointHeight -= 0.5;
            _mc.MoveToTarget(MotorZ, touchPointHeight);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            do
            {
                if (GetPressureValue() > NeedleTouchPressure)
                {
                    _mc.Stop(MotorZ);
                    NeedleOnZHeight = _mc.GetPosition(MotorZ);
                    SetSpeed(1);
                    _mc.MoveToTargetRelativeTillEnd(MotorZ, 2);
                    break;
                }

                if (stopwatch.ElapsedMilliseconds > timeoutSec * 1000)
                {
                    throw new Exception("Find needle height timeout, maybe need to lower sensor: " + _coordinateId);
                }
            } while (true);
        }

        public void CheckEnoughSpace()
        {
            if (_mc.IsCrdSpaceEnough((short)_coordinateId) == false)
            {
                throw new Exception("Not enough space in the coordinate system.");
            }
        }

        public void Delay(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }

        public CapturePosition GetCapturePosition(CaptureId id)
        {
            return Helper.GetCapturePosition(_capturePositions, id);
        }

        public CapturePosition GetCapturePositionOffset(CaptureId id)
        {
            return new CapturePosition();
        }

        public double GetLaserHeightValue()
        {
            return _laserSensor.GetLaserHeight();
        }

        public double GetPosition(Motor motor)
        {
            return _mc.GetPosition(motor);
        }

        public AxisOffset GetRawVisionResult(CapturePosition capturePosition)
        {
            if (VisionSimulateMode)
            {
                return new AxisOffset()
                {
                    XOffset = capturePosition.XPosition + 65, //Camera to sucker distance.
                    YOffset = capturePosition.YPosition,
                };
            }
            else
            {
                //Todo fail check.
                return _vision.RequestVisionCalibration(capturePosition);
            }
        }

        public Pose GetVisionResult(CapturePosition pos)
        {
            var visionOffset = GetRawVisionResult(pos);
            var userOffset = GetCapturePositionOffset(pos.CaptureId);
            return new Pose()
            {
                X = visionOffset.XOffset + userOffset.XPosition,
                Y = visionOffset.YOffset + userOffset.YPosition,
                Z = GetZHeight(pos.CaptureId) + userOffset.ZPosition,
                A = visionOffset.ROffset + userOffset.Angle,
            };
        }

        public Pose GetVisionResult(CapturePosition capturePosition, int retryTimes)
        {
            int retryCount = 0;
            do
            {
                try
                {
                    return GetVisionResult(capturePosition);
                }
                catch (Exception)
                {
                    retryCount++;
                }
            } while (retryCount < 3);

            //To be developed.
            //NgBin(LoadTray.CurrentPart);

            throw new Exception("Vision fail three times.");
        }

        public Pose[] GetVisionResultsLaserAndWork(CapturePosition pos)
        {
            var visionOffset = GetRawVisionResult(pos);
            var userOffset = GetCapturePositionOffset(pos.CaptureId);
            //GetCapturePositionOffsets
            Pose[] poses = new Pose[4];
            int index = 0;
            poses[index] = new Pose()
            {
                //X = visionOffset.XGluePoint1 + userOffset.XPosition,
                //Y = visionOffset.YGluePoint1 + userOffset.YPosition,
                //Z = GetZHeight(pos.CaptureId) + userOffset.ZPosition,
                X = visionOffset.PointX[index],
                Y = visionOffset.PointY[index],
                Z = -23.347,
            };

            index++;
            poses[index] = new Pose()
            {
                X = visionOffset.PointX[index],
                Y = visionOffset.PointY[index],
                Z = -23.347,
            };

            index++;
            poses[index] = new Pose()
            {
                X = visionOffset.PointX[index],
                Y = visionOffset.PointY[index],
                Z = -23.347,
            };

            index++;
            poses[index] = new Pose()
            {
                X = visionOffset.PointX[index],
                Y = visionOffset.PointY[index],
                Z = -23.347,
            };

            //index++;
            //poses[index] = new Pose()
            //{
            //    X = visionOffset.PointX[index],
            //    Y = visionOffset.PointY[index],
            //    Z = -23.347,
            //};

            //index++;
            //poses[index] = new Pose()
            //{
            //    X = visionOffset.PointX[index],
            //    Y = visionOffset.PointY[index],
            //    Z = -23.347,
            //};

            //index++;
            //poses[index] = new Pose()
            //{
            //    X = visionOffset.PointX[index],
            //    Y = visionOffset.PointY[index],
            //    Z = -23.347,
            //};

            //index++;
            //poses[index] = new Pose()
            //{
            //    X = visionOffset.PointX[index],
            //    Y = visionOffset.PointY[index],
            //    Z = -23.347,
            //};

            return poses;
        }

        public double GetWorkPointHeight()
        {
            var laserHeight = _laserSensor.GetLaserHeight();
            var userOffset = GetCapturePositionOffset(CaptureId.GlueLineBeforeGlue);
            // var workHeight = laserHeight - LaserToNeedleHeightOffset +
            //    GlueRadius + userOffset.ZPosition;
            // return workHeight;
            throw new NotImplementedException();
        }

        public Pose[] GetWorkPoses()
        {
            var capPose = GetCapturePosition(CaptureId.GluePointBeforeGlue);
            MoveToCapture(capPose);
            var laserAndWorkPoses = GetVisionResultsLaserAndWork(capPose);
            //int index = 0;
            //MoveToCapture(laserAndWorkPoses[index]);
            //laserAndWorkPoses[index+4].Z = GetWorkPointHeight();

            //index++;
            //MoveToCapture(laserAndWorkPoses[index]);
            //laserAndWorkPoses[index + 4].Z = GetWorkPointHeight();

            //index++;
            //MoveToCapture(laserAndWorkPoses[index]);
            //laserAndWorkPoses[index + 4].Z = GetWorkPointHeight();

            //index++;
            //MoveToCapture(laserAndWorkPoses[index]);
            //laserAndWorkPoses[index + 4].Z = GetWorkPointHeight();

            Pose[] poses = new Pose[4];
            poses[0] = laserAndWorkPoses[0];
            poses[1] = laserAndWorkPoses[1];
            poses[2] = laserAndWorkPoses[2];
            poses[3] = laserAndWorkPoses[3];

            return poses;
        }

        public double GetZHeight(CaptureId id)
        {
            switch (id)
            {
                case CaptureId.LTrayPickTop:
                    throw new NotImplementedException("No such Z height in Glue staion:" + id);
                case CaptureId.LLoadCompensationBottom:
                    throw new NotImplementedException("No such Z height in Glue staion:" + id);
                case CaptureId.LLoadHolderTop:
                    throw new NotImplementedException("No such Z height in Glue staion:" + id);
                default:
                    throw new NotImplementedException("No such Z height in Glue staion:" + id);
            }
        }

        public void MoveToCapture(CapturePosition target)
        {
            var tar = Helper.ConvertToPose(target);
            MoveTo(tar);
        }

        private void MoveTo(Pose target)
        {
            MoveToSafeHeight();

            _mc.MoveToTarget(MotorY, target.Y);
            _mc.MoveToTarget(MotorX, target.X);
            _mc.WaitTillEnd(MotorX);
            _mc.WaitTillEnd(MotorY);

            _mc.MoveToTargetTillEnd(MotorZ, target.Z);
        }

        public void MoveToCapture(Pose target)
        {
            MoveTo(target);
        }

        public void MoveToSafeHeight()
        {
            if (GetPosition(MotorZ) < SafeZHeight)
            {
                _mc.MoveToTargetTillEnd(MotorZ, SafeZHeight);
            }
        }

        public void MoveToTarget(CapturePosition target)
        {
            var tar = Helper.ConvertToPose(target);
            MoveTo(tar);
        }

        public void MoveToTarget(Pose target)
        {
            MoveToSafeHeight();

            _mc.MoveToTarget(MotorY, target.Y);
            _mc.MoveToTarget(MotorX, target.X);

            _mc.WaitTillEnd(MotorX);
            _mc.WaitTillEnd(MotorY);

            _mc.MoveToTargetTillEnd(MotorZ, target.Z);
        }

        public void SetSpeed(double speed)
        {
            MotorX.Velocity = speed;
            MotorY.Velocity = speed;
            MotorZ.Velocity = speed;
            WorkSpeed = speed;
        }

        public void Setup()
        {
            MotorX = _mc.MotorGlueCurveX;
            MotorY = _mc.MotorGlueCurveY;
            MotorZ = _mc.MotorGlueCurveZ;
            NeedleOutput = Output.GlueCurve;
        }

        /// <summary>
        /// By running buffer program.
        /// </summary>
        /// <param name="delayMs"></param>
        public void ShotGlue(ushort delayMs = 600)
        {
            ClearInterpolationBuffer();

            AddDigitalOutput(OutputState.On);
            AddDelay(delayMs);
            AddDigitalOutput(OutputState.Off);

            CheckEnoughSpace();
            StartInterpolation();
            WaitTillInterpolationEnd();
        }

        public async Task<WaitBlock> ShotGlueAsync(ushort delayMs = 600)
        {
            return await Task.Run(() =>
            {
                try
                {
                    ShotGlue(delayMs);
                    return new WaitBlock() { Message = "ShotGlueAsync Successful." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "ShotGlueAsync fail: " + ex.Message };
                }
            });
        }

        public async Task<WaitBlock> DrawArcAsync(ArcInfo arcInfo)
        {
            return await Task.Run(() =>
            {
                try
                {
                    DrawArc(arcInfo);
                    return new WaitBlock() { Message = "ShotGlueAsync Successful." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "ShotGlueAsync fail: " + ex.Message };
                }
            });
        }

        public void DrawArc(ArcInfo arcInfo)
        {
            ClearInterpolationBuffer();

            AddArc(arcInfo);

            CheckEnoughSpace();
            StartInterpolation();
            WaitTillInterpolationEnd();
        }

        public void StartInterpolation()
        {
            _mc.StartInterpolation(_coordinateId);
        }

        public void WaitTillInterpolationEnd()
        {
            _mc.WaitTillInterpolationEnd(_coordinateId);
        }

        public void MoveTo(Pose target, MoveModeAMotor mode, ActionType type)
        {
            throw new NotImplementedException();
        }

        public void MoveToTarget(Pose target, MoveModeAMotor mode, ActionType type)
        {
            throw new NotImplementedException();
        }

        public void MoveToTarget(CapturePosition target, MoveModeAMotor mode, ActionType type)
        {
            throw new NotImplementedException();
        }

        public Task<WaitBlock> WorkAsync()
        {
            throw new NotImplementedException();
        }

        public void CloseGlue()
        {
            _mc.SetOutput(NeedleOutput, OutputState.Off);
        }

        public void ClearInterpolationBuffer()
        {
            _mc.Stop(MotorX);
            _mc.SetCoordinateSystem(_coordinateId);
            _mc.ClearInterpolationBuffer(2, _coordinateId);
        }

        public double GetPressureValue()
        {
            return _pressureSensor.GetPressureValue();
        }

        AxisOffset IGlueRobot.GetVisionResult(CapturePosition capturePosition, int retryTimes)
        {
            throw new NotImplementedException();
        }
    }

    public class ArcInfo
    {
        public double XStart { get; set; }
        public double YStart { get; set; }
        public double XEnd { get; set; }
        public double YEnd { get; set; }
        public double XCenter { get; set; }
        public double YCenter { get; set; }
        public double CenterToStartXOffset { get; set; }
        public double CenterToStartYOffset { get; set; }

        public void CalculateCenterOffset()
        {
            CenterToStartXOffset = XCenter - XStart;
            CenterToStartYOffset = YCenter - YStart;
        }
    }

}
