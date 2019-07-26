using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bp.Mes;

namespace Sorter
{
    public class GluePointStation : IRobot, IGlueRobot
    {
        private readonly MotionController _mc;
        private readonly CoordinateId _coordinateId;
        private readonly PressureSensor _pressureSensor;
        private readonly LaserSensor _laserSensor;
        private readonly List<CapturePosition> _capturePositions;
        private readonly List<CapturePosition> _userOffsets;
        private readonly VisionServer _vision;

        public Output NeedleOutput { get; set; }

        /// <summary>
        /// Get it each time change a needle.
        /// </summary>
        public Offset NeedleOffset { get; set; }

        /// <summary>
        /// Get by pressure sensor.
        /// </summary>
        public double NeedleOnZHeight { get; set; }

        /// <summary>
        /// Laser move to the same point to get the height, then calculate it.
        /// </summary>
        public double LaserToNeedleHeightOffset { get; set; }

        /// <summary>
        /// At the same position where needle detect its height.
        /// </summary>
        public double LaserHeight { get; set; }

        public double GlueRadius { get; set; } = 0.2;

        public double GlueFinishRiseHeight { get; set; } = 1;
        public double GlueFinishLeaveHeight { get; set; } = 2;

        public Motor MotorA { get; set; }
        public Motor MotorX { get; set; }
        public Motor MotorY { get; set; }
        public Motor MotorZ { get; set; }
        public double SafeXArea { get; set; }
        public double SafeYArea { get; set; }
        public double SafeZHeight { get; set; } = -2;
        public double FixtureHeight { get; set; }
        public bool VisionSimulateMode { get; set; }
        public Task<WaitBlock> Preparation { get; set; }
        public double LaserToWorkHeightOffset { get; set; }
        public Task<WaitBlock> PreparationAsync { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public double LaserRefereceZHeight { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public double NeedleOnZHeightUserCompensation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public GluePointStation(MotionController controller, VisionServer vision, CoordinateId id,
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

        public void CaptureNeedleHeight()
        {
            throw new NotImplementedException();
            //var needCapPos = GetCapturePosition(CaptureId.GluePointPressureSensor);
            //MoveToCapture(needCapPos);
            ////Keep doing it.
            //var perssure = _pressureSensor.GetPressureValue();
            ////If overshoot, pull up and then goes down. and get the right height.
        }

        public double GetLaserHeightValue()
        {
            return _laserSensor.GetLaserHeight();
        }

        /// <summary>
        /// Positions where glue needle goes.
        /// </summary>
        /// <returns></returns>
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

        public double GetWorkPointHeight()
        {
            var laserHeight = _laserSensor.GetLaserHeight();
            var userOffset = GetCapturePositionOffset(CaptureId.GlueLineBeforeGlue);
            var workHeight = laserHeight - LaserToNeedleHeightOffset +
                GlueRadius + userOffset.ZPosition;
            return workHeight;
        }

        public void MoveToCapture(Pose target)
        {
            MoveToTarget(target);
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

        public PointPulse ConverToPulsePoint(GluePosition point)
        {
            return new PointPulse()
            {
                X = (int)(MotorX.EncoderFactor * point.X * MotorX.Direction),
                Y = (int)(MotorY.EncoderFactor * point.Y * MotorY.Direction),
                Z = (int)(MotorY.EncoderFactor * point.Z * MotorZ.Direction),
            };
        }

        public void AddPoint(GluePosition point, short core = 2)
        {
            var target = ConverToPulsePoint(point);
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

        public void SetSpeed(double speed)
        {
            MotorX.Velocity = speed;
            MotorY.Velocity = speed;
            MotorZ.Velocity = speed;
        }

        public void MoveToTarget(CapturePosition target)
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

        public void MoveToCapture(CapturePosition target)
        {
            var tar = Helper.ConvertToPose(target);
            MoveTo(tar);
        }

        public double GetPosition(Motor motor)
        {
            return _mc.GetPosition(motor);
        }

        public void Setup()
        {
            MotorX = _mc.MotorGluePointX;
            MotorY = _mc.MotorGluePointY;
            MotorZ = _mc.MotorGluePointZ;
            NeedleOutput = Output.GluePoint;
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

        /// <summary>
        /// Todo get eight points.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
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

        public CapturePosition GetCapturePosition(CaptureId id)
        {
            return Helper.GetCapturePosition(_capturePositions, id);
        }

        public CapturePosition GetCapturePositionOffset(CaptureId id)
        {
            return new CapturePosition();
            //Todo
        }

        public void MoveToSafeHeight()
        {
            if (GetPosition(MotorZ) < SafeZHeight)
            {
                _mc.MoveToTargetTillEnd(MotorZ, SafeZHeight);
            }
        }

        public void MoveToTarget(Pose target)
        {
            MoveTo(target);
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

        public AxisOffset GetVisionResult(CapturePosition capturePosition, int retryTimes)
        {
            throw new NotImplementedException();
        }

        public double GetZHeight(CaptureId id)
        {
            throw new NotImplementedException();
        }

        public void StartInterpolation()
        {
            _mc.StartInterpolation(_coordinateId);
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

        Pose IRobot.GetVisionResult(CapturePosition capturePosition, int retryTimes)
        {
            throw new NotImplementedException();
        }

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

        public void CloseGlue()
        {
            _mc.SetOutput(NeedleOutput, OutputState.Off);
        }

        public void ClearInterpolationBuffer()
        {
            throw new NotImplementedException();
        }

        public void GetLaserRefereceHeight()
        {
            throw new NotImplementedException();
        }

        public double GetLaserHeight()
        {
            throw new NotImplementedException();
        }

        public double GetPressureValue()
        {
            throw new NotImplementedException();
        }
    }


}
