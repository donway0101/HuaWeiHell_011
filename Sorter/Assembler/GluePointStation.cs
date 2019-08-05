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
    public class GluePointStation : IRobot, IGlueRobot
    {
        private static readonly log4net.ILog log =
         log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MotionController _mc;
        private readonly CoordinateId _coordinateId;
        private readonly PressureSensor _pressureSensor;
        private readonly LaserSensor _laserSensor;
        private readonly RoundTable _table;
        private readonly VisionServer _vision;
        private readonly List<CapturePosition> _capturePositions;
        private readonly List<CapturePosition> _capturePositionsOffsets;
        private readonly List<GlueParameter> _glueParameters;

        private static readonly object _motionLocker = new object();

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
        public double SafeZHeight { get; set; } = -2;
        public double FixtureHeight { get; set; } = -20.45;
        public bool VisionSimulateMode { get; set; }

        /// <summary>
        /// Get by pressure sensor.
        /// </summary>
        public double NeedleOnZHeight { get; set; }

        /// <summary>
        /// Comfirmed, it's distance to china is 0.2;
        /// </summary>
        public double NeedleOnZHeightCompensation { get; set; } = 0.3;

        /// <summary>
        /// Get it each time change a needle.
        /// </summary>
        public Offset NeedleOffset { get; set; }

        public Output NeedleOutput { get; set; }

        public Output NeedleCleanPool { get; set; }

        public double LaserRefereceZHeight { get; set; }

        public Task<WaitBlock> Preparation { get; set; }

        public int CurrentCycleId { get; set; }
        public double CameraAboveChinaHeight { get; set; } = -2.7;
        public double CameraAboveFixtureHeight {get; set; } = -10;
        public double DetectNeedleHeightSpeed { get; set; } = 0.05;
        public double LaserAboveFixtureHeight { get; set; } = -10;
        public double LaserSurfaceToNeckHeightOffset { get; set; } = 2.19;
        public double LaserSurfaceToSpringHeightOffset { get; set; } = 0.3;
        public double NeedleTouchPressure { get; set; } = 0.02;

        public Stopwatch NeedleCleaningStopWatch { get; set; } = new Stopwatch();
        public int NeedleCleaningIntervalSec { get; set; } = 120;
        public GlueParameter GlueParas { get; set; }

        public GluePointStation(MotionController controller, VisionServer vision,
            RoundTable table, CoordinateId id, List<GlueParameter> glueParameters,
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
            _glueParameters = glueParameters;
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

        public void MoveToCapture(Pose target)
        {
            MoveToTarget(target);
        }

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

        public void AddPoint(PointInfo point)
        {
            var target = ConvertToPulse(point);
            _mc.AddTargetPoint(2, (short)_coordinateId, target.X, target.Y, target.Z,
                target.Velocity, target.Acceleration, 0, 0);
        }

        public void AddDigitalOutput(OutputState state)
        {
            _mc.AddOutput(2, (short)_coordinateId, NeedleOutput, state);
        }

        public void AddDelay(ushort delayMs)
        {
            _mc.AddDelay(_coordinateId, delayMs);
        }

        public void AddDelay(int delayMs)
        {
            _mc.AddDelay(_coordinateId, (ushort)delayMs);
        }

        public void Delay(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }

        public void WaitTillInterpolationEnd()
        {
            _mc.WaitTillInterpolationEnd(_coordinateId);           
            Delay(200);
            _mc.Stop(MotorX);
        }

        public void CheckEnoughSpace()
        {
            if (_mc.IsCrdSpaceEnough(_coordinateId) == false)
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

            lock (_motionLocker)
            {
                _mc.MoveToTarget(MotorY, target.Y);
                _mc.MoveToTarget(MotorX, target.X);
                _mc.WaitTillEnd(MotorX);
                _mc.WaitTillEnd(MotorY);

                _mc.MoveToTargetTillEnd(MotorZ, target.Z);
            }           
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
            NeedleCleanPool = Output.GluePointClean;

            Preparation = Helper.DummyAsyncTask();
            LaserRefereceZHeight = Properties.Settings.Default.LaserReferenceGluePoint;
            GlueParas = Helper.GetGlueParameter(_glueParameters, _coordinateId);
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

        public CapturePosition GetCapturePosition(CaptureId id)
        {
            return Helper.GetCapturePosition(_capturePositions, id);
        }

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

        public void MoveToSafeHeight()
        {
            lock (_motionLocker)
            {
                if (GetPosition(MotorZ) < SafeZHeight)
                {
                    _mc.MoveToTargetTillEnd(MotorZ, SafeZHeight);
                }
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
                    XOffset = capturePosition.XPosition + 0,
                    YOffset = capturePosition.YPosition,
                };
            }
            else
            {
                return _vision.RequestVisionCalibration(capturePosition);
            }
        }

        public double GetZHeight(CaptureId id)
        {
            switch (id)
            {
                case CaptureId.GluePointBeforeGlue:
                    return LaserAboveFixtureHeight;
                case CaptureId.GluePointAfterGlue:
                    return CameraAboveFixtureHeight;
                case CaptureId.GluePointChina:
                    return CameraAboveChinaHeight;
                default:
                    throw new NotImplementedException("No such Z height in Glue staion:" + id);
            }
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
                    return new WaitBlock() { Code = ErrorCode.ShotGlueFail, Message = "ShotGlueAsync fail: " + ex.Message };
                }
            });
        }

        public void ShotGlue(ushort delayMs = 600)
        {
            lock (_motionLocker)
            {
                ClearInterpolationBuffer();

                AddDigitalOutput(OutputState.On);
                AddDelay(delayMs);
                AddDigitalOutput(OutputState.Off);

                CheckEnoughSpace();
                StartInterpolation();
                WaitTillInterpolationEnd();
            }            
        }

        public void CloseGlue()
        {
            _mc.SetOutput(NeedleOutput, OutputState.Off);
        }

        public void ClearInterpolationBuffer()
        {
            _mc.SetCoordinateSystem(_coordinateId);
            _mc.ClearInterpolationBuffer(2, _coordinateId);
        }

        public double FindLaserRefereceHeight()
        {
            var laserCapture = GetCapturePositionWithUserOffset(CaptureId.GluePointLaserOnPressureSensor);
            MoveToCapture(laserCapture);
            Delay(500);
            var currentZHeight = GetPosition(MotorZ);
            var LaserToNeedleHeightOffset = currentZHeight - NeedleOnZHeight;
            return GetLaserHeightValue() - LaserToNeedleHeightOffset;
        }

        public void CalibrateNeedle()
        {
            CleanNeedle(10);
            NeedleOnZHeight = FindNeedleHeight();
            LaserRefereceZHeight = FindLaserRefereceHeight();
            Properties.Settings.Default.LaserReferenceGluePoint = LaserRefereceZHeight;
            Properties.Settings.Default.Save();
            NeedleOffset = FindNeedleOffset();
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
                GetCapturePositionWithUserOffset(CaptureId.GluePointNeedleOnPressureSensor);
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

        private double TouchAndFindPressureSensor(double touchPointHeight, int timeoutSec = 120)
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


        public double GetPressureValue()
        {
            return _pressureSensor.GetPressureValue();
        }

        public GlueTargets GetVisionResultsForLaserAndWork()
        {
            var id = CaptureId.GluePointBeforeGlue;
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

            #region point infos
            var glueTargets = new PointInfo[4];
            int indexG = 0;
            glueTargets[indexG] = new PointInfo()
            {
                X = offset.StartPointX[indexG],
                Y = offset.StartPointY[indexG],
            };

            indexG++;
            glueTargets[indexG] = new PointInfo()
            {
                X = offset.StartPointX[indexG],
                Y = offset.StartPointY[indexG],
            };

            indexG++;
            glueTargets[indexG] = new PointInfo()
            {
                X = offset.StartPointX[indexG],
                Y = offset.StartPointY[indexG],
            };

            indexG++;
            glueTargets[indexG] = new PointInfo()
            {
                X = offset.StartPointX[indexG],
                Y = offset.StartPointY[indexG],
            };
            #endregion

            return new GlueTargets()
            {
                LaserTargets = laserTargets,
                PointTargets = glueTargets,
            };
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
                        Message = "Glue point Preparation Finished."
                    };
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.GluePointPrepareFail,
                        Message = "Glue point Preparation fail: " + ex.Message
                    };
                }
            });
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void AddPoint(PointInfo point, double speed)
        {
            var target = ConvertToPulse(point);
            _mc.AddTargetPoint(2, (short)_coordinateId, target.X, target.Y, target.Z,
                speed, target.Acceleration, 0, 0);
        }

        public async Task<WaitBlock> CalibrateNeedleAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    CalibrateNeedle();
                    return new WaitBlock()
                    {
                        Message = "CalibrateNeedleAsync Successful.",
                    };
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.NeedleCalibrationFail,
                        Message = "CalibrateNeedleAsync fail: " + ex.Message
                    };
                }
            });
        }

        public void CleanNeedle(int shotSec = 5)
        {
            if (NeedleCleaningStopWatch.ElapsedMilliseconds < NeedleCleaningIntervalSec * 1000)
            {
                return;
            }

            MoveToCapture(GetCapturePositionWithUserOffset(CaptureId.GluePointCleanNeedleShot));
            ShotGlueOutput(shotSec);
            RiseZALittleAndDown();
            Delay(2000);
            RiseZALittleAndDown();
            Delay(2000);

            MoveToCapture(GetCapturePositionWithUserOffset(CaptureId.GluePointCleanNeedleSuck));
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
                        Code = ErrorCode.CleanNeedleFail,
                        Message = "CleanNeedle fail: " + ex.Message
                    };
                }
            });
        }

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
                    return new WaitBlock() { Code = ErrorCode.FindNeedleHeightFail, Message = "GetLaserRefereceHeight fail: " + ex.Message };
                }
            });
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
                        Code = ErrorCode.FindNeedleHeightFail,
                        Message = "CaptureNeedleHeight fail: " + ex.Message
                    };
                }
            });
        }

        /// <summary>
        /// User laser to find china plane height, shot some glue on it, 
        /// and then find x and y offset on it.
        /// </summary>
        /// <returns></returns>
        public Offset FindNeedleOffset()
        {
            var findHeightCapture =
                GetCapturePositionWithUserOffset(CaptureId.GluePointLaserOnCalibrationChina);
            MoveToCapture(findHeightCapture);

            var height = GetNeedleToWorkSurfaceHeight();

            var shotCapture =
                GetCapturePositionWithUserOffset(CaptureId.GluePointNeedleOnCalibrationChina);
            shotCapture.ZPosition = findHeightCapture.ZPosition - height;

            MoveToCapture(shotCapture);

            ShotGlue(1000);

            var calCapture = GetCapturePosition(CaptureId.GluePointChina);

            MoveToCapture(calCapture);

            var result = GetVisionResult(calCapture, 3);

            return new Offset()
            {
                XOffset = result.X,
                YOffset = result.Y,
            };
        }


        public double GetNeedleToWorkSurfaceHeight()
        {
            return GetLaserHeightValue() - LaserRefereceZHeight;
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
            for (int i = 0; i < laserHeight.Length - 1; i++)
            {
                errors[i] = laserHeight[i + 1] - laserHeight[i];
            }

            foreach (var err in errors)
            {
                if (err > 0.1)
                {
                    throw new LaserException("Laser height not reliable" + Helper.ConvertToJsonString(laserHeight));
                }
            }

            return laserHeight;
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
                    return new WaitBlock()
                    {
                        Code = ErrorCode.VisionFail,
                        Message = "GetVisionResultsForLaserAndWorkAsync fail:" + ex.Message
                    };
                }
            });
        }

        public void Glue(GlueTargets glueTargets, GlueParameter gluePara)
        {
            lock (_motionLocker)
            {
                ClearInterpolationBuffer();

                PointInfo gluePoint;
                PointInfo risePoint;

                for (int i = 0; i < 4; i++)
                {
                    gluePoint = new PointInfo()
                    {
                        X = glueTargets.PointTargets[i].X,
                        Y = glueTargets.PointTargets[i].Y,
                        Z = glueTargets.PointTargets[i].Z + gluePara.GlueRadius,
                    };
                    AddPoint(gluePoint);
                    AddDigitalOutput(OutputState.On);
                    if (i == 0)
                    {
                        AddDelay(gluePara.PreShotTime);
                    }
                    else
                    {
                        AddDelay(gluePara.PreShotTime - gluePara.SecondLineLessPreShot);
                    }
                    AddDelay(gluePara.GluePeriod);
                    AddDigitalOutput(OutputState.Off);

                    risePoint = new PointInfo()
                    {
                        X = glueTargets.PointTargets[i].X,
                        Y = glueTargets.PointTargets[i].Y,
                        Z = glueTargets.PointTargets[i].Z + gluePara.RiseGlueHeight,
                    };
                    AddPoint(risePoint, gluePara.RiseGlueSpeed);
                    AddDelay(gluePara.CloseGlueDelay);
                }

                var lastRise = new PointInfo()
                {
                    X = glueTargets.PointTargets[3].X,
                    Y = glueTargets.PointTargets[3].Y,
                    Z = glueTargets.PointTargets[3].Z + 20,
                };
                AddPoint(lastRise);

                CheckEnoughSpace();
                StartInterpolation();
                WaitTillInterpolationEnd();
            }            
        }

        public void ShotGlueOutput(int delaySec = 10)
        {
            _mc.SetOutput(NeedleOutput, OutputState.On);
            Delay(delaySec * 1000);
            _mc.SetOutput(NeedleOutput, OutputState.Off);
        }

        public void Work()
        {
            MoveToCapture(GetCapturePositionWithUserOffset(CaptureId.GluePointBeforeGlue));
            var glueTargets = GetVisionResultsForLaserAndWork();
            var heights = GetSurfaceDistance(glueTargets);

            var workSurfaceHeight = GetPosition(MotorZ) -
                (Helper.FindEven(heights) - LaserSurfaceToNeckHeightOffset);

            foreach (var tar in glueTargets.PointTargets)
            {
                tar.Z = workSurfaceHeight;
            }

            var approachPoint = new Pose()
            {
                X = glueTargets.PointTargets[0].X,
                Y = glueTargets.PointTargets[0].Y,
                Z = workSurfaceHeight + 10,
            };

            MoveToTarget(approachPoint);

            string jstr = Helper.ConvertToJsonString(glueTargets);
            log.Info("Glue point target is " + Environment.NewLine +  jstr);

            Glue(glueTargets, GlueParas);
        }


        public async Task<WaitBlock> WorkAsync(int cycleId)
        {
            return await Task.Run( async () =>
            {
                #region Skip work due to error or empty or wait for other station.
                if (_table.Fixtures[(int)StationId.GluePoint].NG ||
                 _table.Fixtures[(int)StationId.GluePoint].IsEmpty || CurrentCycleId >= cycleId)
                {

                    CurrentCycleId = cycleId;
                    return new WaitBlock()
                    {
                        Message = "Glue point WorkAsync Skip due to previous station fail " +
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
                    return new WaitBlock() { Message = "Glue point Finished Successful." };
                }
                catch (VisionException)
                {
                    _table.Fixtures[(int)StationId.GluePoint].NG = true;
                    return new WaitBlock() { Message = "Glue point vision NG, set NG." };
                }
                catch (LaserException)
                {
                    _table.Fixtures[(int)StationId.GluePoint].NG = true;
                    return new WaitBlock() { Message = "Glue point laser NG, set NG." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.GluePointFail,
                        Message = "Glue point Finished fail." + ex.Message
                    };
                }
            });
        }

    }


}
