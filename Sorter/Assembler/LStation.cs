using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bp.Mes;

namespace Sorter
{
    public class LStation : IAssemblyRobot, IRobot
    {
        private readonly MotionController _mc;
        private readonly VisionServer _vision;
        private readonly RoundTable _table;
        private List<CapturePosition> _capturePositions;
        public List<CapturePosition> _capturePositionsOffsets;

        public Motor MotorA { get; set; }
        public Motor MotorX { get; set; }
        public Motor MotorY { get; set; }
        public Motor MotorZ { get; set; }

        public double SafeXArea { get; set; } = 80;
        public double SafeYArea { get; set; } = 80;
        public double SafeZHeight { get; set; } = -10;

        public Tray LoadTray { get; set; }
        public double LoadTrayHeight { get; set; } = -64.1;
        public double FixtureHeight { get; set; } = -57.6;

        public int UvDelayMs { get; set; } = 4;

        public bool CheckVacuumValue { get; set; } = false;
        public bool VisionSimulateMode { get; set; } = false;

        /// <summary>
        /// Not used.
        /// </summary>
        public Tray UnloadTray {get; set; }
        /// <summary>
        /// Not used.
        /// </summary>
        public double UnloadTrayHeight {get; set; }

        /// <summary>
        /// Preparation work for next cycle.
        /// </summary>
        public Task<WaitBlock> Preparation { get; set; }

        public bool HasPartOnLoadSucker { get; set; }

        public int CurrentCycleId { get; set; }

        public int MaxFailCount { get; set; } = 10;
        public Task<WaitBlock> ChangeLoadTray { get; set; }
        public Task<WaitBlock> ChangeUnloadTray { get; set; }

        public LStation(MotionController controller, VisionServer vision,
            RoundTable table, List<CapturePosition> positions, List<CapturePosition> offsets)
        {
            _mc = controller;
            _vision = vision;
            _table = table;
            _capturePositions = positions;
            _capturePositionsOffsets = offsets;
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

        /// <summary>
        /// Request vision server data.
        /// </summary>
        /// <param name="capturePosition"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get Z height for move to target method.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public double GetZHeight(CaptureId id)
        {
            switch (id)
            {
                case CaptureId.LTrayPickTop:
                    return LoadTrayHeight;
                case CaptureId.LLoadCompensationBottom:
                    var capturePos = GetCapturePosition(CaptureId.LLoadCompensationBottom);
                    return capturePos.ZPosition;
                case CaptureId.LLoadHolderTop:
                    return FixtureHeight;                   
                default:
                    throw new NotImplementedException("No such Z height in L staion:" + id);
            }
        }

        /// <summary>
        /// Get vision result and add user offsets.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
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
        /// Try to get vision result less than three times.
        /// </summary>
        /// <param name="capPos"></param>
        /// <param name="retryTimes"></param>
        /// <returns></returns>
        public Pose GetVisionResult(CapturePosition capPos, int retryTimes = 3)
        {
            int retryCount = 0;
            do
            {
                try
                {
                    return GetVisionResult(capPos);
                }
                catch (Exception)
                {
                    retryCount++;
                }
            } while (retryCount < 3);

            throw new VisionException() { CaptureId = capPos.CaptureId };
        }

        #region For testing.
        public async Task<WaitBlock> LDoingSomthingForALongTime()
        {
            try
            {
                //await Task.Delay(1);
                await SomeBasicTask();
                return new WaitBlock() { Message = "LDoingSomthingForALongTime OK" };
            }
            catch (Exception ex)
            {
                return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "LDoingSomthingForALongTime error: " + ex.Message };
            }
        }

        private async Task SomeBasicTask()
        {
            await SomethingHard();
        }

        private async Task SomethingHard()
        {
            await Task.Run(()=>{
                Thread.Sleep(1000);
                Thread.Sleep(1000);
                Thread.Sleep(1000);
                Thread.Sleep(1000);
            });
           
        }
        #endregion

        /// <summary>
        /// Suck a part from L tray hole.
        /// </summary>
        /// <param name="part"></param>
        private void PickPart(Part part)
        {
            MoveToCapture(part.CapturePos);
            var pickPose = GetVisionResult(part.CapturePos, 3);
            MoveToTarget(pickPose, MoveModeAMotor.Relative);
            Sucker(2, ActionType.Load);
            MoveToSafeHeight();

            HasPartOnLoadSucker = true;
            SetNextPartLoad();
        }

        private Pose GetFixturePose()
        {
            var fixtureCapturePos = GetCapturePosition(CaptureId.LLoadHolderTop);
            MoveToCapture(fixtureCapturePos);
            return GetVisionResult(fixtureCapturePos);
        }

        private Pose AngleCompensation(double fixtureAngle, double userCompensation)
        {
            //Get angle offset for loading.
            var bottomCamCapturePosLoad = GetCapturePosition(CaptureId.LLoadCompensationBottom);
            MoveToCapture(bottomCamCapturePosLoad);

            var slidAngle = GetVisionResult(bottomCamCapturePosLoad, 3);

            var angleCompensation = userCompensation + 180.0 - fixtureAngle - slidAngle.A;
            CorrectAngle(angleCompensation, MoveModeAMotor.Relative);

            var xNyOffset = GetVisionResult(bottomCamCapturePosLoad, 3);

            //var finalAngelError = Math.Abs(fixtureAngle - xNyOffset.A);

            //Todo check error.
            //if (finalAngelError>0.7)
            //{
            //    //Todo maybe need to compensate again.
            //    throw new Exception("Angle compensation NG, error is:" + finalAngelError);
            //}

            return xNyOffset;
        }

        /// <summary>
        /// With UV delay time.
        /// </summary>
        /// <param name="pose"></param>
        private void PlacePart(Pose pose)
        {
            MoveToTarget(pose, MoveModeAMotor.None);

            Sucker(FixtureId.L, VacuumState.On, VacuumArea.Center);
            UVLightOn(UvDelayMs*1000);
            Sucker(VacuumState.Off);

            MoveToSafeHeight();

            HasPartOnLoadSucker = false;
        }

        public Task UVLightOnAsync(int delayMs = 1000)
        {
            return Task.Run(() => {
                UVLightOn(delayMs);
            });
        }

        public void UVLightOn(int delayMs = 1000)
        {
            _mc.SetOutput(Output.UVLightHead, OutputState.On);
            Delay(delayMs);
            _mc.SetOutput(Output.UVLightHead, OutputState.Off);
        }

        public void Delay(int delayMs)
        {
            Thread.Sleep(delayMs);
        }

        /// <summary>
        /// Avoid collision.
        /// </summary>
        public void MoveToSafeHeight()
        {
            if (GetPosition(MotorZ) < SafeZHeight)
            {
                _mc.MoveToTargetTillEnd(MotorZ, SafeZHeight);
            }
        }

        /// <summary>
        /// Move to capture position, angle rotation is not included.
        /// </summary>
        /// <param name="target"></param>
        public void MoveToCapture(CapturePosition target)
        {
            var tar = Helper.ConvertToPose(target);
            MoveTo(tar, MoveModeAMotor.Abs);
        }

        public void MoveTo(Pose target, MoveModeAMotor mode, ActionType type = ActionType.Load)
        {
            MoveToSafeHeight();

            if (IsMoveToSameArea(target))
            {
                _mc.MoveToTarget(MotorY, target.Y);
                _mc.MoveToTarget(MotorX, target.X);
                MoveAngleMotor(target.A, mode, ActionType.Load);
                _mc.WaitTillEnd(MotorA);
                _mc.WaitTillEnd(MotorX);
                _mc.WaitTillEnd(MotorY);
                
                _mc.MoveToTargetTillEnd(MotorZ, target.Z);
            }
            else
            {
                //Move from conveyor to table.
                if (GetPosition(MotorY) < SafeYArea &&
                   target.Y > SafeYArea)
                {
                    MoveAngleMotor(target.A, mode, ActionType.Load);
                    _mc.MoveToTargetTillEnd(MotorX, target.X);
                    _mc.MoveToTargetTillEnd(MotorY, target.Y);
                    _mc.WaitTillEnd(MotorA);

                    _mc.MoveToTargetTillEnd(MotorZ, target.Z);
                }
                else
                {
                    //Move from table to conveyor.
                    if (GetPosition(MotorX) < SafeXArea &&
                        target.X > SafeXArea)
                    {
                        MoveAngleMotor(target.A, mode, ActionType.Load);
                        _mc.MoveToTargetTillEnd(MotorY, target.Y);
                        _mc.MoveToTargetTillEnd(MotorX, target.X);
                        _mc.WaitTillEnd(MotorA);

                        _mc.MoveToTargetTillEnd(MotorZ, target.Z);
                    }
                    else
                    {
                        throw new Exception("L robot move to target fail, unknow move stratege.");
                    }
                }
            }
        }

        private bool IsMoveToSameArea(Pose target)
        {
            return (GetPosition(MotorY) <= SafeYArea && target.Y <= SafeYArea) ||
                     (GetPosition(MotorX) <= SafeXArea && target.X <= SafeXArea);
        }

        public void MoveToTarget(CapturePosition target, MoveModeAMotor mode, ActionType type = ActionType.Load)
        {
            var tar = Helper.ConvertToPose(target);
            MoveTo(tar, MoveModeAMotor.Relative);
        }

        public void MoveToTarget(Pose target, MoveModeAMotor mode = MoveModeAMotor.Abs, 
            ActionType type = ActionType.Load)
        {
            MoveTo(target, mode);
        }

        public void SetSpeed(double speed)
        {
            MotorX.Velocity = speed;
            MotorY.Velocity = speed;
            MotorZ.Velocity = speed;
            MotorA.Velocity = speed;
        }

        public void Setup()
        {
            MotorX = _mc.MotorLX;
            MotorY = _mc.MotorLY;
            MotorZ = _mc.MotorLZ;
            MotorA = _mc.MotorLRotateLoad;

            LoadTray = new Tray()
            {
                RowCount = 4,
                ColumneCount = 12,
                XOffset = 15.0,
                YOffset = 15.0,
                CurrentPart = new Part() {
                    CapturePos = GetCapturePosition(CaptureId.LTrayPickTop),
                    TargetPose = new Pose() { },
                },
                BaseCapturePosition = GetCapturePosition(CaptureId.LTrayPickTop),
                YIncreaseDirection = 1,
            };

            ResetPreparation();
        }

        public void ResetPreparation()
        {
            Preparation = Helper.DummyAsyncTask();
        }

        public void Unload(Part part)
        {
            throw new NotImplementedException();
        }

        public void Sucker(VacuumState state)
        {
            try
            {
                _mc.LLoadVacuum(state, true);
            }
            catch (Exception)
            {
                throw new SuckerException() { CaptureId = CaptureId.LTrayPickTop, };
            }
        }

        public void Sucker(FixtureId id, VacuumState state, VacuumArea area)
        {
            _table.Sucker(id, state, area, CheckVacuumValue);
        }

        public void UnlockTray(ActionType procedure = ActionType.Load)
        {
            _mc.LLoadConveyorLocker(LockState.Off);
        }

        public void LockTray()
        {
            _mc.LLoadConveyorLocker(LockState.On);
        }

        public async Task<WaitBlock> LockTrayAsync()
        {
            return await Task.Run(() => {
                try
                {
                    LockTray();
                    return new WaitBlock() { Message = "LockTray Finished Successful." };
                }
                catch (Exception)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "LockTray Finished Successful." };
                }
            });
        }

        /// <summary>
        /// Get motor encoder position or reference position of motors.
        /// </summary>
        /// <param name="motor"></param>
        /// <returns></returns>
        public double GetPosition(Motor motor)
        {
            return _mc.GetPosition(motor);
        }

        public void UnloadAndLoad(Part unload, Part load)
        {
            throw new NotImplementedException();
        }

        public void Sucker(VacuumState state, int retryTimes, ActionType action)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Correct direction counter clockwise
        /// </summary>
        /// <param name="relativeAngle"></param>
        /// <param name="mode"></param>
        /// <param name="action"></param>
        public void CorrectAngle(double relativeAngle, MoveModeAMotor mode,
            ActionType action = ActionType.Load)
        {
            _mc.MoveToTargetRelativeTillEnd(MotorA, relativeAngle);
        }

        /// <summary>
        /// Need to suck another product.
        /// Reset prepare for next task?
        /// </summary>
        /// <param name="part"></param>
        public void NgBin(Part part)
        {
            MoveToCapture(GetCapturePosition(CaptureId.LBin));
            Sucker(VacuumState.Off);
            //May be try to suck again to test if product drops.
            throw new SuckerException();
        }

        public void SetNextPartLoad()
        {
            LoadTray.CurrentPart = LoadTray.GetNextPartForLoad(LoadTray.CurrentPart);
        }

        public void SetNextPartUnload()
        {
            throw new NotImplementedException();
        }

        public void RiseZALittleAndDown()
        {
            _mc.MoveToTargetRelativeTillEnd(MotorZ, 2);
            _mc.MoveToTargetRelativeTillEnd(MotorZ, -2.5);
        }

        public void MoveAngleMotor(double angle, MoveModeAMotor mode,
            ActionType type = ActionType.Load)
        {
            switch (mode)
            {
                case MoveModeAMotor.None:
                    //Motor goes to its current position, meet the wait till end condition.
                    _mc.MoveToTarget(MotorA, MotorA.TargetPosition);
                    break;
                case MoveModeAMotor.Abs:
                    _mc.MoveToTarget(MotorA, angle);
                    break;
                case MoveModeAMotor.Relative:
                    _mc.MoveToTargetRelative(MotorA, angle);
                    break;
                default:
                    break;
            }
        }

        public void Load(Part part)
        {
            var fixturePose = GetFixturePose();

            var userAssemblyOffset = GetCapturePositionOffset(CaptureId.LAssemblyOffset);

            var xNyOffset = AngleCompensation(fixturePose.A, userAssemblyOffset.Angle);

            part.TargetPose.X = fixturePose.X - xNyOffset.X + userAssemblyOffset.XPosition;
            part.TargetPose.Y = fixturePose.Y - xNyOffset.Y + userAssemblyOffset.YPosition;
            part.TargetPose.Z = GetZHeight(CaptureId.LLoadHolderTop) + userAssemblyOffset.ZPosition;

            PlacePart(part.TargetPose);
        }

        public async Task<WaitBlock> LoadAsync(Part part)
        {
            return await Task.Run(() =>
            {
                try
                {
                    Load(part);
                    return new WaitBlock() { Message = "L Load Finished Successful." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "L Load fail: " + ex.Message };
                }
            });
        }

        public async Task<WaitBlock> ChangeLoadTrayAsync()
        {
            throw new NotImplementedException();
        }

        public void Bin(ActionType type = ActionType.Load)
        {
            MoveToCapture(GetCapturePosition(CaptureId.LBin));
            Sucker(VacuumState.Off);
        }

        public void Prepare()
        {
            PickPart(LoadTray.CurrentPart);
            MoveToCapture(GetCapturePosition(CaptureId.LLoadHolderTopReady));
        }

        public async Task<WaitBlock> PrepareAsync()
        {
            return await Task.Run(() =>
            {

                int failCount = 0;
                string remarks = string.Empty;
                do
                {
                    try
                    {
                        Prepare();
                        return new WaitBlock()
                        {
                            Message = "L Preparation Finished."
                        };
                    }

                    #region Vision excepiton
                    catch (VisionException vex)
                    {
                        failCount++;
                        try
                        {
                            switch (vex.CaptureId)
                            {
                                case CaptureId.LTrayPickTop:
                                    SetNextPartLoad();
                                    HasPartOnLoadSucker = false;
                                    continue;

                                default:
                                    throw new NotImplementedException("Error 465465413216894");
                            }
                        }
                        catch (Exception ex)
                        {
                            return new WaitBlock()
                            {
                                Code = ErrorCode.LStationPrepareFail,
                                Message = ex.Message
                            };
                        }
                    }
                    #endregion

                    catch (SuckerException sEx)
                    {
                        failCount++;
                        try
                        {
                            switch (sEx.CaptureId)
                            {
                                case CaptureId.LTrayPickTop:
                                    SetNextPartLoad();
                                    HasPartOnLoadSucker = false;
                                    continue;

                                default:
                                    throw new NotImplementedException("Error 49874616549746516");
                            }
                        }
                        catch (Exception ex)
                        {
                            return new WaitBlock()
                            {
                                Code = ErrorCode.LStationPrepareFail,
                                Message = ex.Message
                            };
                        }                       
                    }

                    catch (Exception ex)
                    {
                        return new WaitBlock()
                        {
                            Code = ErrorCode.LStationPrepareFail,
                            Message = ex.Message
                        };
                    }

                } while (failCount < MaxFailCount);

                return new WaitBlock()
                {
                    Code = ErrorCode.LStationPrepareFail,
                    Message = "Reaches max fail count."
                };

            });
        }

        public void Sucker(int retryTimes, ActionType action)
        {
            int failCount = 0;
            do
            {
                try
                {
                    Sucker(VacuumState.On);
                    return;
                }
                catch (SuckerException)
                {
                    failCount++;
                    Sucker(VacuumState.Off);
                    RiseZALittleAndDown();
                }
            } while (failCount<retryTimes);

            throw new SuckerException() { CaptureId = CaptureId.LTrayPickTop, };
        }

        public void SetForTest()
        {
            _table.Sucker(FixtureId.L, VacuumState.On, VacuumArea.Circle);
            LockTray();
            _table.Fixtures[(int)StationId.L].IsEmpty = false;
        }

        /// <summary>
        /// If vision fail, try another new part.
        /// </summary>
        /// <returns></returns>
        public async Task<WaitBlock> WorkAsync(int cycleId)
        {
            return await Task.Run(async () =>
            {
                #region Skip work due to error or empty or wait for other station.
                if (_table.Fixtures[(int)StationId.L].NG || 
                 _table.Fixtures[(int)StationId.L].IsEmpty || CurrentCycleId >= cycleId)
                {
                    CurrentCycleId = cycleId;
                    return new WaitBlock()
                    {
                        Message = "LWorkAsync Skip due to previous station fail " +
                        "or is empty, or has to wait for other station."
                    };
                }
                #endregion

                #region Work
                int failCount = 0;
                string remarks = string.Empty;

                do
                {
                    try
                    {
                        #region Normal procedure
                        //Wait for last preparation to finish.
                        // it's finished by default after powerup.
                        await Preparation;
                        Helper.CheckTaskResult(Preparation);

                        //If it's first time to prepare.
                        if (HasPartOnLoadSucker == false)
                        {
                            Preparation = PrepareAsync();
                            await Preparation;
                            Helper.CheckTaskResult(Preparation);
                        }

                        Load(LoadTray.CurrentPart);

                        //Trigger a preparation for next cycle.
                        Preparation = PrepareAsync();

                        //Finish one cycle, goes into next cycle.
                        CurrentCycleId = cycleId;
                        return new WaitBlock()
                        {
                            Message = "LWorkAsync Finished smoothly.",
                        }; 
                        #endregion
                    }

                    #region Vision exception
                    catch (VisionException vEx)
                    {
                        failCount++;
                        try
                        {
                            switch (vEx.CaptureId)
                            {
                                case CaptureId.LLoadCompensationBottom:
                                    Bin();
                                    HasPartOnLoadSucker = false;
                                    continue;

                                case CaptureId.LLoadHolderTop:
                                    _table.Fixtures[(int)StationId.L].NG = true;
                                    return new WaitBlock()
                                    {
                                        Code = ErrorCode.Sucessful,
                                        Message = "Set fixture as NG",
                                    };

                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        catch (Exception vEx1)
                        {
                            return new WaitBlock()
                            {
                                Code = ErrorCode.LStationWorkFail,
                                Message = vEx1.Message,
                            };
                        }                        
                    } 
                    #endregion

                    #region Exception that need human's assistant.
                    catch (Exception ex)
                    {
                        return new WaitBlock()
                        {
                            Code = ErrorCode.LStationWorkFail,
                            Message = ex.Message,
                        };
                    } 
                    #endregion

                } while (failCount < MaxFailCount);

                #region Return when reaches max fail count.
                return new WaitBlock()
                {
                    Code = ErrorCode.LStationWorkFail,
                    Message = "Fail count reaches max number or fixture NG.",
                }; 
                #endregion

                #endregion

            });
        }

        public void Reset()
        {
            if (Preparation.Result.Code != ErrorCode.Sucessful)
            {
                Preparation = Helper.DummyAsyncTask();
            }
        }

        public Task<WaitBlock> ChangeUnloadTrayAsync()
        {
            throw new NotImplementedException();
        }
    }
}
