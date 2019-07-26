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
        private List<CapturePosition> _userPositionOffsets;

        public Motor MotorA { get; set; }
        public Motor MotorX { get; set; }
        public Motor MotorY { get; set; }
        public Motor MotorZ { get; set; }

        public double SafeXArea { get; set; } = 80;
        public double SafeYArea { get; set; } = 80;
        public double SafeZHeight { get; set; } = -10;

        public Tray LoadTray { get; set; }
        public double LoadTrayHeight { get; set; } = -63.146;
        public double FixtureHeight { get; set; } = -57.2;

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
        //public Task<WaitBlock> Preparation { get; set; }

        public LStation(MotionController controller, VisionServer vision,
            RoundTable table, List<CapturePosition> positions, List<CapturePosition> offsets)
        {
            _mc = controller;
            _vision = vision;
            _table = table;
            _capturePositions = positions;
            _userPositionOffsets = offsets;
        }

        public CapturePosition GetCapturePosition(CaptureId id)
        {
            return Helper.GetCapturePosition(_capturePositions, id);
        }

        public CapturePosition GetCapturePositionOffset(CaptureId id)
        {
            return new CapturePosition();
            //Todo add offset 
            return Helper.GetCapturePosition(_userPositionOffsets, id);
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
        /// For efficency's shake.
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public async Task<WaitBlock> PreparationForNextCycleAsync(Part part)
        {
            return await Task.Run(() =>
            {
                try
                {
                    //MoveToCapture(part.CapturePos);
                    return new WaitBlock() { Message = "V prepare OK" };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "V prepare error: " + ex.Message };
                }
            });
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
        /// <param name="capturePosition"></param>
        /// <param name="retryTimes"></param>
        /// <returns></returns>
        public Pose GetVisionResult(CapturePosition capturePosition, int retryTimes = 3)
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

            throw new Exception("Vision fail three times. Ng bin action coming soon.");
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

        public async Task<WaitBlock> LoadAsync(Part part, int uVDelay = 4)
        {
            return await Task.Run(() =>
            {
                try
                {
                    Load(part, uVDelay);
                    return new WaitBlock() { Message = "L Load Finished Successful." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "L Load fail: " + ex.Message };
                }
            });
        }

        /// <summary>
        /// Suck a part from L tray hole.
        /// </summary>
        /// <param name="part"></param>
        private void PickPart(Part part)
        {
            MoveToCapture(part.CapturePos);
            var pickPose = GetVisionResult(part.CapturePos, 3);
            MoveToTarget(pickPose);
            Sucker(VacuumState.On);
            MoveToSafeHeight();
            SetNextPartLoad();
        }

        private Pose GetFixturePose()
        {
            var fixtureCapturePos = GetCapturePosition(CaptureId.LLoadHolderTop);
            MoveToCapture(fixtureCapturePos);
            return GetVisionResult(fixtureCapturePos);
        }

        private Pose AngleCompensation(double fixtureAngle)
        {
            //Get angle offset for loading.
            var bottomCamCapturePosLoad = GetCapturePosition(CaptureId.LLoadCompensationBottom);
            MoveToCapture(bottomCamCapturePosLoad);
            var bottomCamPos = GetVisionResult(bottomCamCapturePosLoad, 3);
            var angleOffset = fixtureAngle - bottomCamPos.A;
            CorrectAngle(angleOffset, MoveModeAMotor.Relative);
            var xNyOffset = GetVisionResult(bottomCamCapturePosLoad, 3);
            var finalAngelError = Math.Abs(fixtureAngle - xNyOffset.A);
            if (finalAngelError>0.7)
            {
                //Todo maybe need to compensate again.
                throw new Exception("Angle compensation NG, error is:" + finalAngelError);
            }

            return xNyOffset;
        }

        private void PlacePart(Pose pose, int uVDelay = 4)
        {
            MoveToTarget(pose, MoveModeAMotor.None);
            Sucker(FixtureId.L, VacuumState.On, VacuumArea.Center);
            _table.UVLightOn(uVDelay * 1000);
            Sucker(VacuumState.Off);
            MoveToSafeHeight();       
        }

        /// <summary>
        /// Load a part.
        /// </summary>
        /// <param name="part"></param>
        public void Load(Part part, int uVDelay = 4)
        {
            PickPart(part);

            var fixturePose = GetFixturePose();

            //5.0 test result.
            var xNyOffset = AngleCompensation(fixturePose.A + 3.0);

            //0.1 test result
            part.TargetPose.X = fixturePose.X - xNyOffset.X + 0.1;
            part.TargetPose.Y = fixturePose.Y - xNyOffset.Y + 0.1;
            part.TargetPose.Z = GetZHeight(CaptureId.LLoadHolderTop);

            PlacePart(part.TargetPose, uVDelay);         
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
            MoveTo(tar, MoveModeAMotor.None);
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
            };

            //Wired!
            //Preparation = Task.Run(() =>
            //{
            //    return new WaitBlock()
            //    {
            //        Code = ErrorCode.Sucessful,
            //        Message = "This is a empty task, just for preparation of loading."
            //    };
            //});

            //Preparation = EmptyTask();
        }

        //private Task<WaitBlock> EmptyTask()
        //{
        //    return Task.Run(async () =>
        //    {
        //        try
        //        {
        //            await Task.Delay(1);
        //            return new WaitBlock() { Code = ErrorCode.Sucessful };
        //        }
        //        catch (Exception ex)
        //        {
        //            return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = ex.Message };
        //        }
        //    });
        //}

        public void Unload(Part part)
        {
            throw new NotImplementedException();
        }

        public void Sucker(VacuumState state)
        {
            _mc.LLoadVacuum(state, CheckVacuumValue);
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

        public async Task<WaitBlock> LockTrayAsync(int delayMs = 1000)
        {
            return await Task.Run(async () => {
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

        public Task<WaitBlock> Work()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1);
                    try
                    {
                        Load(LoadTray.CurrentPart);
                    }
                    catch(NeedToPickAnotherPartException)
                    {
                        //To get next part.
                        SetNextPartLoad();
                        //Try another tray part.
                        try
                        {
                            Load(LoadTray.CurrentPart);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    
                    return new WaitBlock() { };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "L station error:" + ex.Message };
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

        public void CorrectAngle(double relativeAngle, MoveModeAMotor mode,
            ActionType action = ActionType.Load)
        {
            _mc.MoveToTargetRelativeTillEnd(MotorA, relativeAngle);
        }

        public void CorrectAngle(ref Part part, MoveModeAMotor mode, ActionType action)
        {
            //Get angle offset for load.
            var bottomCamCapturePosLoad = GetCapturePosition(CaptureId.LLoadCompensationBottom);
            MoveToCapture(bottomCamCapturePosLoad);
            var angleOffset = GetVisionResult(bottomCamCapturePosLoad, 3);
            CorrectAngle(angleOffset.A, MoveModeAMotor.Abs, ActionType.Unload);
            var xNyOffset = GetVisionResult(bottomCamCapturePosLoad, 3);
            part.XOffset = xNyOffset.X;
            part.YOffset = xNyOffset.Y;
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
            throw new NeedToPickAnotherPartException();
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
            throw new NotImplementedException();
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

        void IAssemblyRobot.Work()
        {
            throw new NotImplementedException();
        }

        public Task<WaitBlock> WorkAsync()
        {
            throw new NotImplementedException();
        }

        public void Load(Part part)
        {
            PickPart(part);

            var fixturePose = GetFixturePose();

            //5.0 test result.
            var xNyOffset = AngleCompensation(fixturePose.A + 3.0);

            //0.1 test result
            part.TargetPose.X = fixturePose.X - xNyOffset.X + 0.1;
            part.TargetPose.Y = fixturePose.Y - xNyOffset.Y + 0.1;
            part.TargetPose.Z = GetZHeight(CaptureId.LLoadHolderTop);

            PlacePart(part.TargetPose);

            SetNextPartLoad();
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
    }
}
