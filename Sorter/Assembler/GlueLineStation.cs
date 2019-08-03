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
    public class GlueLineStation : IRobot, IGlueRobot
    {
        private readonly MotionController _mc;
        private readonly CoordinateId _coordinateId;
        private readonly RoundTable _table;
        private readonly PressureSensor _pressureSensor;
        private readonly LaserSensor _laserSensor;
        private readonly List<CapturePosition> _capturePositions;
        private readonly List<CapturePosition> _capturePositionsOffsets;
        private readonly VisionServer _vision;

        /// <summary>
        /// Avoid conflict between work and needle cleaning.
        /// </summary>
        private readonly object _workLock = new object();

        public Motor MotorA { get; set; }
        public Motor MotorX { get; set; }
        public Motor MotorY { get; set; }
        public Motor MotorZ { get; set; }
        public double SafeXArea { get; set; }
        public double SafeYArea { get; set; }
        public double SafeZHeight { get; set; } = -3;
        public double FixtureHeight { get; set; } = -20.45;

        /// <summary>
        /// 
        /// </summary>
        public bool VisionSimulateMode { get; set; } 
        
        /// <summary>
        /// Capture by pressure sensor.
        /// </summary>
        public double NeedleOnZHeight { get; set; }

        /// <summary>
        /// Comfirmed: it's distance to china is just 0.3.
        /// </summary>
        public double NeedleOnZHeightCompensation { get; set; } = 0.2;

        /// <summary>
        /// New needle to old needle offset.
        /// </summary>
        public Offset NeedleOffset { get; set; }
        public Output NeedleOutput { get; set; }
        public Output NeedleCleanPool { get; set; }

        /// <summary>
        /// Distance between needle and surface
        /// </summary>
        public double GlueRadius { get; set; } = 0.2;

        /// <summary>
        /// How slow is the needle to approach pressure.
        /// </summary>
        public double DetectNeedleHeightSpeed { get; set; } = 0.05;

        /// <summary>
        /// How much pressure will needle press the sensor.
        /// </summary>
        public double NeedleTouchPressure { get; set; } = 0.02;

        /// <summary>
        /// When needle and laser touches the same plane, the value of laser sensor.
        /// </summary>
        public double LaserRefereceZHeight { get; set; }

        /// <summary>
        /// When glue station is not in working cycle.
        /// </summary>
        public Task<WaitBlock> Preparation { get; set; }

        /// <summary>
        /// If other station has deal with error, this station has to wait.
        /// </summary>
        public int CurrentCycleId { get; set; }

        /// <summary>
        /// Before needle touches part, laser helps finding height between part and needle.
        /// </summary>
        public double LaserAboveFixtureHeight { get; set; } = -10;

        public double CameraAboveFixtureHeight { get; set; } = -10;

        public double CameraAboveChinaHeight { get; set; } = -2.67;

        /// <summary>
        /// Constant height about part.
        /// </summary>
        public double LaserSurfaceToNeckHeightOffset { get; set; } = 2.19;

        /// <summary>
        /// Constant height about part.
        /// </summary>
        public double LaserSurfaceToSpringHeightOffset { get; set; } = 1.30;

        public Stopwatch NeedleCleaningStopWatch { get; set; } = new Stopwatch();
        public int NeedleCleaningIntervalSec { get; set; } = 120;

        public GlueLineStation(MotionController controller, VisionServer vision,
            RoundTable table, CoordinateId id,
             List<CapturePosition> positions, List<CapturePosition> offsets,
            PressureSensor pressureSensor, LaserSensor laserSensor)
        {
            _mc = controller;
            _coordinateId = id;
            _table = table;
            _capturePositions = positions;
            _capturePositionsOffsets = offsets;
            _pressureSensor = pressureSensor;
            _laserSensor = laserSensor;
            _vision = vision;
        }

        /// <summary>
        /// Add delay in controller program buffer.
        /// </summary>
        /// <param name="delayMs"></param>
        public void AddDelay(int delayMs)
        {
            _mc.AddDelay((short)_coordinateId, (ushort)delayMs);
        }

        public void AddDelay(ushort delayMs)
        {
            _mc.AddDelay((short)_coordinateId, delayMs);
        }

        /// <summary>
        /// Add digital output in controller motion program.
        /// </summary>
        /// <param name="state"></param>
        public void AddDigitalOutput(OutputState state)
        {
            _mc.AddOutput(2, (short)_coordinateId, NeedleOutput, state);
        }

        /// <summary>
        /// Will be use in glue point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public PointInfoPulse ConvertToPulse(PointInfo point)
        {
            return new PointInfoPulse()
            {
                X = (int)(MotorX.EncoderFactor * point.X * MotorX.Direction),
                Y = (int)(MotorY.EncoderFactor * point.Y * MotorY.Direction),
                Z = (int)(MotorY.EncoderFactor * point.Z * MotorZ.Direction),

                Velocity = MotorX.Velocity,
                Acceleration = MotorX.Acceleration,
            };
        }

        /// <summary>
        /// Convert camera point to controller pulse.
        /// </summary>
        /// <param name="arcInfo"></param>
        /// <returns></returns>
        public ArcInfoPulse ConvertToPulse(ArcInfo arcInfo)
        {
            arcInfo.CalculateCenterOffset();

            return new ArcInfoPulse()
            {
                X = (int)(MotorX.EncoderFactor * arcInfo.XEnd * MotorX.Direction),
                Y = (int)(MotorY.EncoderFactor * arcInfo.YEnd * MotorY.Direction),
                Z = (int)(MotorZ.EncoderFactor * arcInfo.Z * MotorZ.Direction),

                ArcCenterToXStartOffset =
                    (int)(MotorX.EncoderFactor * arcInfo.CenterToStartXOffset * MotorX.Direction),
                ArcCenterToYStartOffset =
                    (int)(MotorY.EncoderFactor * arcInfo.CenterToStartYOffset * MotorY.Direction),

                Velocity = MotorX.Velocity,
                Acceleration = MotorX.Acceleration,
            };
        }

        /// <summary>
        /// Add target point, including xyz to controller buffer.
        /// </summary>
        /// <param name="point"></param>
        public void AddPoint(PointInfo point)
        {
            var target = ConvertToPulse(point);
            _mc.AddTargetPoint(2, (short)_coordinateId, target.X, target.Y, target.Z,
                target.Velocity, target.Acceleration, 0, 0);
        }

        public void AddPoint(PointInfo point, double speed)
        {
            var target = ConvertToPulse(point);
            _mc.AddTargetPoint(2, (short)_coordinateId, target.X, target.Y, target.Z,
                speed, target.Acceleration, 0, 0);
        }

        public void AddPoint(PointInfo point, double syncSpeed, double endSpeed)
        {
            var target = ConvertToPulse(point);
            _mc.AddTargetPoint(2, (short)_coordinateId, target.X, target.Y, target.Z,
                syncSpeed, target.Acceleration, endSpeed, 0);
        }

        /// <summary>
        /// Add a arc to controller buffer.
        /// </summary>
        /// <param name="arcInfo"></param>
        public void AddArc(ArcInfo arcInfo)
        {
            var target = ConvertToPulse(arcInfo);
            _mc.AddArcXYC(2, (short)_coordinateId, target.X, target.Y, 
                target.ArcCenterToXStartOffset, target.ArcCenterToYStartOffset, 
                target.Velocity, target.Acceleration);
        }

        public void AddArc(ArcInfo arcInfo, double velocity)
        {
            var target = ConvertToPulse(arcInfo);
            _mc.AddArcXYC(2, (short)_coordinateId, target.X, target.Y,
                target.ArcCenterToXStartOffset, target.ArcCenterToYStartOffset,
                velocity, target.Acceleration);
        }

        /// <summary>
        /// Move laser to the same place where needle find its height.
        /// </summary>
        /// <seealso cref="FindNeedleHeight"/>
        public double FindLaserRefereceHeight()
        {
            var laserCapture = GetCapturePositionWithUserOffset(CaptureId.GlueLineLaserOnPressureSensor);
            MoveToCapture(laserCapture);
            Delay(500);
            var currentZHeight = GetPosition(MotorZ);
            var LaserToNeedleHeightOffset = currentZHeight - NeedleOnZHeight;
            return GetLaserHeightValue() - LaserToNeedleHeightOffset;
        }

        /// <summary>
        /// First Find needle height, second find laser referece, and last find needle x and y offset.
        /// </summary>
        public void CalibrateNeedle()
        {
            CleanNeedle(10);
            NeedleOnZHeight = FindNeedleHeight();
            LaserRefereceZHeight = FindLaserRefereceHeight();
            Properties.Settings.Default.LaserReferenceGlueLine = LaserRefereceZHeight;
            Properties.Settings.Default.Save();
            NeedleOffset = FindNeedleOffset();
        }

        /// <summary>
        /// User laser to find china plane height, shot some glue on it, 
        /// and then find x and y offset on it.
        /// </summary>
        /// <returns></returns>
        public Offset FindNeedleOffset()
        {
            var findHeightCapture =
                GetCapturePositionWithUserOffset(CaptureId.GlueLineLaserOnCalibrationChina);
            MoveToCapture(findHeightCapture);

            var height = GetNeedleToWorkSurfaceHeight();

            var shotCapture =
                GetCapturePositionWithUserOffset(CaptureId.GlueLineNeedleOnCalibrationChina);
            shotCapture.ZPosition = findHeightCapture.ZPosition - height;

            MoveToCapture(shotCapture);

            ShotGlue(1000);

            var calCapture = GetCapturePosition(CaptureId.GlueLineChina);

            MoveToCapture(calCapture);

            var result = GetVisionResult(calCapture, 3);

            return new Offset()
            {
                XOffset = result.X,
                YOffset = result.Y,
            };
        }

        public async Task<WaitBlock> FindLaserRefereceHeightAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    LaserRefereceZHeight = FindLaserRefereceHeight();
                    return new WaitBlock() { Message = "GetLaserRefereceHeight Successful." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "GetLaserRefereceHeight fail: " + ex.Message };
                }
            });
        }

        public async Task<WaitBlock> CalibrateNeedleAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    CalibrateNeedle();
                    return new WaitBlock() {
                        Message = "CalibrateNeedleAsync Successful.",
                    };
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.TobeCompleted,
                        Message = "CalibrateNeedleAsync fail: " + ex.Message
                    };
                }
            });
        }

        /// <summary>
        /// Laser is the eye to the needle. 
        /// When they are on the same plane, laser has a referece.
        /// </summary>
        /// <returns></returns>
        public double GetNeedleToWorkSurfaceHeight()
        {
            return  GetLaserHeightValue() - LaserRefereceZHeight;
        }

        public async Task<WaitBlock> FindNeedleHeightAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    NeedleOnZHeight = FindNeedleHeight();
                    return new WaitBlock() { Message = "CaptureNeedleHeight Successful." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.TobeCompleted,
                        Message = "CaptureNeedleHeight fail: " + ex.Message
                    };
                }
            });
        }
      

        public void CleanNeedle(int shotSec = 5)
        {
            if (NeedleCleaningStopWatch.ElapsedMilliseconds<NeedleCleaningIntervalSec*1000)
            {
                return;
            }

            MoveToCapture(GetCapturePositionWithUserOffset(CaptureId.GlueLineCleanNeedleShot));
            ShotGlueOutput(shotSec);
            RiseZALittleAndDown();
            Delay(2000);
            RiseZALittleAndDown();
            Delay(2000);

            MoveToCapture(GetCapturePositionWithUserOffset(CaptureId.GlueLineCleanNeedleSuck));
            _mc.SetOutput(NeedleCleanPool, OutputState.On);
            Delay(2000);
            _mc.SetOutput(NeedleCleanPool, OutputState.Off);
            MoveToSafeHeight();
        }

        public void RiseZALittleAndDown()
        {
            _mc.MoveToTargetRelativeTillEnd(MotorZ, 3);
            _mc.MoveToTargetRelativeTillEnd(MotorZ, -3);
        }

        public async Task<WaitBlock> CleanNeedleAsync(int delaySec = 30)
        {
            return await Task.Run(() =>
            {
                try
                {
                    CleanNeedle(delaySec);
                    return new WaitBlock() { Message = "CleanNeedle Successful." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.TobeCompleted,
                        Message = "CleanNeedle fail: " + ex.Message
                    };
                }
            });
        }

        /// <summary>
        /// Detect needle height by touching pressure sensor.
        /// Saved needle to sensor height error less than 2 mm.
        /// </summary>
        /// <returns></returns>
        public double FindNeedleHeight()
        {
            if (GetPressureValue() >= 0.005)
            {
                throw new Exception("Pressure sensor has zero offset when powerup, please reset it.");
            }

            double needleSafeHeight = 1.0;

            var needleCapture = 
                GetCapturePositionWithUserOffset(CaptureId.GlueLineNeedleOnPressureSensor);
            needleCapture.ZPosition += needleSafeHeight;
            MoveToCapture(needleCapture);

            var tempSpeed = MotorX.Velocity;
            SetSpeed(DetectNeedleHeightSpeed);

            var needleHeight = TouchAndFindPressureSensor(needleCapture.ZPosition - needleSafeHeight);

            //Restore speed.
            SetSpeed(tempSpeed);
            MoveToSafeHeight();

            return needleHeight;
        }

        private double TouchAndFindPressureSensor(double touchPointHeight, int timeoutSec = 60)
        {
            double needleHeight = 0;

            //Max down 0.5mm
            touchPointHeight -= 0.5;
            _mc.MoveToTarget(MotorZ, touchPointHeight);

            var stopwatch = new Stopwatch();
            stopwatch.Start();


            do
            {
                //if (_mc.IsMoving(MotorZ) == false)
                //{
                //    _mc.MoveToTargetRelative(MotorZ, -0.1);
                //    if (GetPosition(MotorZ)<=touchPointHeight)
                //    {
                //        throw new Exception("Find needle height timeout, maybe need to lower sensor: " + _coordinateId);
                //    }
                //}

                if (GetPressureValue() > NeedleTouchPressure)
                {
                    _mc.Stop(MotorZ);
                    Delay(800);
                    needleHeight = _mc.GetPosition(MotorZ) + NeedleOnZHeightCompensation;
                    SetSpeed(1);
                    _mc.MoveToTargetRelativeTillEnd(MotorZ, 2);
                    return needleHeight;
                }

                if (stopwatch.ElapsedMilliseconds > timeoutSec * 1000)
                {
                    throw new Exception("Find needle height timeout, maybe need to lower sensor: " + _coordinateId);
                }

            } while (true);
        }

        /// <summary>
        /// Ensure enough space to running a program.
        /// </summary>
        public void CheckEnoughSpace()
        {
            if (_mc.IsCrdSpaceEnough((short)_coordinateId) == false)
            {
                throw new Exception("Not enough space in the coordinate system.");
            }
        }

        /// <summary>
        /// Thread delay.
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        public void Delay(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }

        public CapturePosition GetCapturePosition(CaptureId id)
        {
            return Helper.GetCapturePosition(_capturePositions, id);
        }

        /// <summary>
        /// Get both capture position with user offsets.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CapturePosition GetCapturePositionWithUserOffset(CaptureId id)
        {
            var capPos = GetCapturePosition(id);
            var userOffset = GetCapturePositionOffset(id);

            return new CapturePosition()
            {
                CaptureId = id,
                XPosition = capPos.XPosition + userOffset.XPosition,
                YPosition = capPos.YPosition + userOffset.YPosition,
                ZPosition = capPos.ZPosition + userOffset.ZPosition,
                Angle = capPos.Angle + userOffset.Angle,
            };
        }

        public CapturePosition GetCapturePositionOffset(CaptureId id)
        {
            try
            {
                return Helper.GetCapturePosition(_capturePositionsOffsets, id);
            }
            catch (Exception)
            {
                return new CapturePosition();
            }
        }

        public double GetLaserHeightValue()
        {
            try
            {
                return _laserSensor.GetLaserHeight();
            }
            catch (Exception)
            {
                throw new LaserException("Laser sensor not readable or value out of range.");
            }          
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

            throw new VisionException("Vision fail three times.");
        }

        public AxisOffset GetRawVisionResult(CapturePosition capturePosition, int retryTimes)
        {
            int retryCount = 0;
            do
            {
                try
                {
                    return GetRawVisionResult(capturePosition);
                }
                catch (Exception)
                {
                    retryCount++;
                }
            } while (retryCount < 3);

            throw new VisionException("Vision fail three times.");
        }

        public double GetZHeight(CaptureId id)
        {
            switch (id)
            {
                case CaptureId.GlueLineBeforeGlue:
                    return LaserAboveFixtureHeight;
                case CaptureId.GlueLineAfterGlue:
                    return CameraAboveFixtureHeight;
                case CaptureId.GlueLineChina:
                    return CameraAboveChinaHeight;
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
        }

        public void Setup()
        {
            MotorX = _mc.MotorGlueLineX;
            MotorY = _mc.MotorGlueLineY;
            MotorZ = _mc.MotorGlueLineZ;
            NeedleOutput = Output.GlueLine;
            NeedleCleanPool = Output.GlueLineClean;

            Preparation = Helper.DummyAsyncTask();

            LaserRefereceZHeight = Properties.Settings.Default.LaserReferenceGlueLine;
        }

        /// <summary>
        /// By running buffer program.
        /// </summary>
        /// <param name="delayMs"> Can not overflow ushort value.</param>
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

        public void ShotGlueOutput(int delaySec = 10)
        {
            _mc.SetOutput(NeedleOutput, OutputState.On);
            Delay(delaySec * 1000);
            _mc.SetOutput(NeedleOutput, OutputState.Off);
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

        public async Task<WaitBlock> WorkAsync(int cycleId)
        {
            return await Task.Run(async () =>
            {
                #region Skip work due to error or empty or wait for other station.
                if (_table.Fixtures[(int)StationId.GlueLine].NG ||
                 _table.Fixtures[(int)StationId.GlueLine].IsEmpty || CurrentCycleId >= cycleId)
                {
                    CurrentCycleId = cycleId;
                    return new WaitBlock()
                    {
                        Message = "Glue Line WorkAsync Skip due to previous station fail " +
                        "or is empty, or has to wait for other station."
                    };
                }
                #endregion

                string remarks = string.Empty;

                try
                {
                    Preparation = PrepareAsync();
                    await Preparation;
                    Helper.CheckTaskResult(Preparation);

                    Work();
                    MoveToSafeHeight();
                    //NeedleCleaningStopWatch.Restart();
                    CurrentCycleId++;
                    return new WaitBlock() { Message = "Glue line Finished Successful." };
                }
                catch (VisionException)
                {
                    //Todo move to safe position.
                    MoveToSafeHeight();
                    _table.Fixtures[(int)StationId.GlueLine].NG = true;
                    return new WaitBlock() { Message = "Glue line vision NG, set NG." };
                }
                catch (LaserException)
                {
                    MoveToSafeHeight();
                    _table.Fixtures[(int)StationId.GlueLine].NG = true;
                    return new WaitBlock() { Message = "Glue line laser NG, set NG." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.TobeCompleted,
                        Message = "Glue line Finished fail." + ex.Message
                    };
                }
            });
        }

        public async Task<WaitBlock> WorkAsync(int cycleId, GlueParameters gluePara)
        {
            return await Task.Run(() => {
                try
                {
                    //Work(gluePara);
                    Work();
                    return new WaitBlock() { Message = "Glue line Finished Successful." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.TobeCompleted,
                        Message = "Glue line Finished fail." + ex.Message
                    };
                }
            });
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

        /// <summary>
        /// Get laser targets and glue targets from vision.
        /// </summary>
        /// <returns></returns>
        public GlueTargets GetVisionResultsForLaserAndWork()
        {
            var id = CaptureId.GlueLineBeforeGlue; 
            var offset = GetRawVisionResult(GetCapturePosition(id), 3);

            #region Laser targets.
            var laserTargets = new Pose[4];
            int indexL = 0;
            laserTargets[indexL] = new Pose()
            {
                X = offset.LaserX[indexL],
                Y = offset.LaserY[indexL],
                Z = GetZHeight(id),
            };

            indexL++;
            laserTargets[indexL] = new Pose()
            {
                X = offset.LaserX[indexL],
                Y = offset.LaserY[indexL],
                Z = GetZHeight(id),
            };

            indexL++;
            laserTargets[indexL] = new Pose()
            {
                X = offset.LaserX[indexL],
                Y = offset.LaserY[indexL],
                Z = GetZHeight(id),
            };

            indexL++;
            laserTargets[indexL] = new Pose()
            {
                X = offset.LaserX[indexL],
                Y = offset.LaserY[indexL],
                Z = GetZHeight(id),
            };

            #endregion

            #region Group points infos
            var glueTargets = new GroupPointInfo();
            glueTargets.Group1Points = new PointInfo[11];
            glueTargets.Group2Points = new PointInfo[11];
            glueTargets.Group3Points = new PointInfo[11];
            glueTargets.Group4Points = new PointInfo[11];

            for (int i = 0; i < glueTargets.Group1Points.Length; i++)
            {
                glueTargets.Group1Points[i] = new PointInfo();
                glueTargets.Group1Points[i].X = offset.Group1PointX[i];
                glueTargets.Group1Points[i].Y = offset.Group1PointY[i];

                glueTargets.Group2Points[i] = new PointInfo();
                glueTargets.Group2Points[i].X = offset.Group2PointX[i];
                glueTargets.Group2Points[i].Y = offset.Group2PointY[i];

                glueTargets.Group3Points[i] = new PointInfo();
                glueTargets.Group3Points[i].X = offset.Group3PointX[i];
                glueTargets.Group3Points[i].Y = offset.Group3PointY[i];

                glueTargets.Group4Points[i] = new PointInfo();
                glueTargets.Group4Points[i].X = offset.Group4PointX[i];
                glueTargets.Group4Points[i].Y = offset.Group4PointY[i];
            }

            #endregion

            return new GlueTargets()
            {
                LaserTargets = laserTargets,
                //ArcTargets = glueTargets,
                GroupPoints = glueTargets,
            };
        }

        public double[] GetSurfaceDistance(GlueTargets targets)
        {
            //First neck surface height, second spring surface height.
            double[] result = new double[2];

            double[] laserHeight = new double[4];

            int index = 0;
            MoveToTarget(targets.LaserTargets[index]);
            Delay(100);
            laserHeight[index] = GetNeedleToWorkSurfaceHeight();

            index++;
            MoveToTarget(targets.LaserTargets[index]);
            Delay(100);
            laserHeight[index] = GetNeedleToWorkSurfaceHeight();

            index++;
            MoveToTarget(targets.LaserTargets[index]);
            Delay(100);
            laserHeight[index] = GetNeedleToWorkSurfaceHeight();

            index++;
            MoveToTarget(targets.LaserTargets[index]);
            Delay(100);
            laserHeight[index] = GetNeedleToWorkSurfaceHeight();

            //check data reliablity.
            double[] errors = new double[laserHeight.Length - 1];
            for (int i = 0; i < laserHeight.Length-1; i++)
            {
                errors[i] = laserHeight[i + 1] - laserHeight[i];
            }

            foreach (var err in errors)
            {
                if (err>0.1)
                {
                    throw new LaserException("Laser height not reliable" + Helper.ConvertToJsonString(laserHeight));
                }
            }

            return laserHeight;
        }

        public void Work()
        {
            GlueParameters gluePara = new GlueParameters()
            {
                PreShotTime = 200,
                GlueSpeed = 5,
                RiseGlueSpeed = 0.5,
                RiseGlueHeight = 2,
                CloseGlueDelay = 300,
                SecondLineLessPreShot = 100,               
            };

            MoveToCapture(GetCapturePositionWithUserOffset(CaptureId.GlueLineBeforeGlue));
            var glueTargets = GetVisionResultsForLaserAndWork();
            var heights = GetSurfaceDistance(glueTargets);
            var workSurfaceHeight = GetPosition(MotorZ) - 
                (Helper.FindEven(heights) - LaserSurfaceToSpringHeightOffset);

            for (int i = 0; i < glueTargets.GroupPoints.Group1Points.Length; i++)
            {
                glueTargets.GroupPoints.Group1Points[i].Z = workSurfaceHeight;
                glueTargets.GroupPoints.Group2Points[i].Z = workSurfaceHeight;
                glueTargets.GroupPoints.Group3Points[i].Z = workSurfaceHeight;
                glueTargets.GroupPoints.Group4Points[i].Z = workSurfaceHeight;
            }

            var approachPoint = new Pose()
            {
                X = glueTargets.GroupPoints.Group1Points[0].X,
                Y = glueTargets.GroupPoints.Group1Points[0].Y,
                Z = glueTargets.GroupPoints.Group1Points[0].Z + 10,
            };

            MoveToTarget(approachPoint);

            Glue(glueTargets, gluePara);
        }

        public void Glue(GlueTargets glueTargets, GlueParameters gluePara)
        {
            ClearInterpolationBuffer();

            #region First point.
            int index = 0; //1
            //glueTargets.GroupPoints.Group1Points[index].Z += gluePara.GlueHeightOffset[index];
            AddPoint(glueTargets.GroupPoints.Group1Points[index]);
            AddDigitalOutput(OutputState.On);
            AddDelay(gluePara.PreShotTime);

            index++; //2
            AddPoint(glueTargets.GroupPoints.Group1Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //3
            AddPoint(glueTargets.GroupPoints.Group1Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //4
            AddPoint(glueTargets.GroupPoints.Group1Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //5
            AddPoint(glueTargets.GroupPoints.Group1Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //6
            AddPoint(glueTargets.GroupPoints.Group1Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //7
            AddPoint(glueTargets.GroupPoints.Group1Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //8
            AddPoint(glueTargets.GroupPoints.Group1Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //9
            AddPoint(glueTargets.GroupPoints.Group1Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //10
            AddPoint(glueTargets.GroupPoints.Group1Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //11
            AddPoint(glueTargets.GroupPoints.Group1Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);

            AddDigitalOutput(OutputState.Off);
            glueTargets.GroupPoints.Group1Points[index].Z += gluePara.RiseGlueHeight;
            AddPoint(glueTargets.GroupPoints.Group1Points[index], gluePara.RiseGlueSpeed, 0);

            AddDelay(gluePara.CloseGlueDelay);
            #endregion

            #region Second point.
            index = 0; //1
            AddPoint(glueTargets.GroupPoints.Group2Points[index]);
            AddDigitalOutput(OutputState.On);
            AddDelay(gluePara.PreShotTime - gluePara.SecondLineLessPreShot);

            index++; //2
            AddPoint(glueTargets.GroupPoints.Group2Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //3
            AddPoint(glueTargets.GroupPoints.Group2Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //4
            AddPoint(glueTargets.GroupPoints.Group2Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //5
            AddPoint(glueTargets.GroupPoints.Group2Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //6
            AddPoint(glueTargets.GroupPoints.Group2Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //7
            AddPoint(glueTargets.GroupPoints.Group2Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //8
            AddPoint(glueTargets.GroupPoints.Group2Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //9
            AddPoint(glueTargets.GroupPoints.Group2Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //10
            AddPoint(glueTargets.GroupPoints.Group2Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //11
            AddPoint(glueTargets.GroupPoints.Group2Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);

            AddDigitalOutput(OutputState.Off);
            glueTargets.GroupPoints.Group2Points[index].Z += gluePara.RiseGlueHeight;
            AddPoint(glueTargets.GroupPoints.Group2Points[index], gluePara.RiseGlueSpeed, 0);

            AddDelay(gluePara.CloseGlueDelay);
            #endregion

            #region Third point.
            index = 0; //1
            AddPoint(glueTargets.GroupPoints.Group3Points[index]);
            AddDigitalOutput(OutputState.On);
            AddDelay(gluePara.PreShotTime - gluePara.SecondLineLessPreShot);

            index++; //2
            AddPoint(glueTargets.GroupPoints.Group3Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //3
            AddPoint(glueTargets.GroupPoints.Group3Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //4
            AddPoint(glueTargets.GroupPoints.Group3Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //5
            AddPoint(glueTargets.GroupPoints.Group3Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //6
            AddPoint(glueTargets.GroupPoints.Group3Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //7
            AddPoint(glueTargets.GroupPoints.Group3Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //8
            AddPoint(glueTargets.GroupPoints.Group3Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //9
            AddPoint(glueTargets.GroupPoints.Group3Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //10
            AddPoint(glueTargets.GroupPoints.Group3Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //11
            AddPoint(glueTargets.GroupPoints.Group3Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);

            AddDigitalOutput(OutputState.Off);
            glueTargets.GroupPoints.Group3Points[index].Z += gluePara.RiseGlueHeight;
            AddPoint(glueTargets.GroupPoints.Group3Points[index], gluePara.RiseGlueSpeed, 0);

            AddDelay(gluePara.CloseGlueDelay);
            #endregion

            #region Fourth point.
            index = 0; //1
            AddPoint(glueTargets.GroupPoints.Group4Points[index]);
            AddDigitalOutput(OutputState.On);
            AddDelay(gluePara.PreShotTime - gluePara.SecondLineLessPreShot);

            index++; //2
            AddPoint(glueTargets.GroupPoints.Group4Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //3
            AddPoint(glueTargets.GroupPoints.Group4Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //4
            AddPoint(glueTargets.GroupPoints.Group4Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //5
            AddPoint(glueTargets.GroupPoints.Group4Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //6
            AddPoint(glueTargets.GroupPoints.Group4Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //7
            AddPoint(glueTargets.GroupPoints.Group4Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //8
            AddPoint(glueTargets.GroupPoints.Group4Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //9
            AddPoint(glueTargets.GroupPoints.Group4Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //10
            AddPoint(glueTargets.GroupPoints.Group4Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);
            index++; //11
            AddPoint(glueTargets.GroupPoints.Group4Points[index], gluePara.GlueSpeed, gluePara.GlueSpeed);

            AddDigitalOutput(OutputState.Off);
            glueTargets.GroupPoints.Group4Points[index].Z += gluePara.RiseGlueHeight;
            AddPoint(glueTargets.GroupPoints.Group4Points[index], gluePara.RiseGlueSpeed, 0);

            AddDelay(gluePara.CloseGlueDelay);
            #endregion

            glueTargets.GroupPoints.Group4Points[index].Z += 20;
            AddPoint( glueTargets.GroupPoints.Group4Points[index]);

            CheckEnoughSpace();
            StartInterpolation();
            WaitTillInterpolationEnd();
        }

        public void GlueOld(GlueTargets glueTargets, GlueParameters gluePara)
        {
            ClearInterpolationBuffer();



            #region First point.
            //Go to first point and delay.
            int index = 0;
            var firstPoint = new PointInfo()
            {
                X = glueTargets.ArcTargets[index].XStart,
                Y = glueTargets.ArcTargets[index].YStart,
                Z = glueTargets.ArcTargets[index].Z + gluePara.GlueHeightOffset[index],
            };
            AddPoint(firstPoint);
            AddDigitalOutput(OutputState.On);
            AddDelay(gluePara.PreShotTime);

            AddArc(glueTargets.ArcTargets[index], gluePara.GlueSpeed);
            //Todo  Pre close to avoid sharp ending.
            AddDigitalOutput(OutputState.Off);

            var firstRise = new PointInfo()
            {
                X = glueTargets.ArcTargets[index].XEnd,
                Y = glueTargets.ArcTargets[index].YEnd,
                Z = glueTargets.ArcTargets[index].Z + gluePara.RiseGlueHeight,
            };
            AddPoint(firstRise, gluePara.RiseGlueSpeed);

            AddDelay(gluePara.CloseGlueDelay);
            #endregion

            #region Second point.
            index++;
            var secondPoint = new PointInfo()
            {
                X = glueTargets.ArcTargets[index].XStart,
                Y = glueTargets.ArcTargets[index].YStart,
                Z = glueTargets.ArcTargets[index].Z + gluePara.GlueHeightOffset[index],
            };
            AddPoint(secondPoint);
            AddDigitalOutput(OutputState.On);
            AddDelay(gluePara.PreShotTime - gluePara.SecondLineLessPreShot);

            AddArc(glueTargets.ArcTargets[index], gluePara.GlueSpeed);
            //Todo  Pre close to avoid sharp ending.
            AddDigitalOutput(OutputState.Off);

            var secondRise = new PointInfo()
            {
                X = glueTargets.ArcTargets[index].XEnd,
                Y = glueTargets.ArcTargets[index].YEnd,
                Z = glueTargets.ArcTargets[index].Z + gluePara.RiseGlueHeight,
            };
            AddPoint(secondRise, gluePara.RiseGlueSpeed);

            AddDelay(gluePara.CloseGlueDelay);
            #endregion

            #region third point.
            index++;
            var thirdPoint = new PointInfo()
            {
                X = glueTargets.ArcTargets[index].XStart,
                Y = glueTargets.ArcTargets[index].YStart,
                Z = glueTargets.ArcTargets[index].Z + gluePara.GlueHeightOffset[index],
            };
            AddPoint(thirdPoint);
            AddDigitalOutput(OutputState.On);
            AddDelay(gluePara.PreShotTime - gluePara.SecondLineLessPreShot);

            AddArc(glueTargets.ArcTargets[index], gluePara.GlueSpeed);
            //Todo  Pre close to avoid sharp ending.
            AddDigitalOutput(OutputState.Off);

            var thirdRise = new PointInfo()
            {
                X = glueTargets.ArcTargets[index].XEnd,
                Y = glueTargets.ArcTargets[index].YEnd,
                Z = glueTargets.ArcTargets[index].Z + gluePara.RiseGlueHeight,
            };
            AddPoint(thirdRise, gluePara.RiseGlueSpeed);

            AddDelay(gluePara.CloseGlueDelay);
            #endregion

            #region fourth point.
            index++;
            var fourthPoint = new PointInfo()
            {
                X = glueTargets.ArcTargets[index].XStart,
                Y = glueTargets.ArcTargets[index].YStart,
                Z = glueTargets.ArcTargets[index].Z + gluePara.GlueHeightOffset[index],
            };
            AddPoint(fourthPoint);
            AddDigitalOutput(OutputState.On);
            AddDelay(gluePara.PreShotTime - gluePara.SecondLineLessPreShot);

            AddArc(glueTargets.ArcTargets[index], gluePara.GlueSpeed);
            //Todo  Pre close to avoid sharp ending.
            AddDigitalOutput(OutputState.Off);

            var fourthRise = new PointInfo()
            {
                X = glueTargets.ArcTargets[index].XEnd,
                Y = glueTargets.ArcTargets[index].YEnd,
                Z = glueTargets.ArcTargets[index].Z + gluePara.RiseGlueHeight,
            };
            AddPoint(fourthRise, gluePara.RiseGlueSpeed);

            AddDelay(gluePara.CloseGlueDelay);
            #endregion

            var finishRise = new PointInfo()
            {
                X = glueTargets.ArcTargets[index].XEnd,
                Y = glueTargets.ArcTargets[index].YEnd,
                Z = glueTargets.ArcTargets[index].Z + gluePara.RiseGlueHeight + 20,
            };
            AddPoint(finishRise, 10);

            CheckEnoughSpace();
            StartInterpolation();
            WaitTillInterpolationEnd();
        }

        public async Task<WaitBlock> GetVisionResultsForLaserAndWorkAsync()
        {
            return await Task.Run(() => {
                try
                {
                    GetVisionResultsForLaserAndWork();

                    return new WaitBlock() { Message = "GetVisionResultsForLaserAndWorkAsync OK" };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() {
                        Code = ErrorCode.TobeCompleted,
                        Message = "GetVisionResultsForLaserAndWorkAsync fail:" + ex.Message };
                }
            });
        }

        public async Task<WaitBlock> PrepareAsync()
        {
            return await Task.Run(() =>
            {
                string remarks = string.Empty;

                try
                {
                    CleanNeedle();
                    return new WaitBlock()
                    {
                        Message = "Glue line Preparation Finished."
                    };
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.TobeCompleted,
                        Message = "Glue line Preparation fail: " + ex.Message
                    };
                }
            });
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

 
 

}
