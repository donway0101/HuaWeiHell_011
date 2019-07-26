using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bp.Mes;

/// <summary>
/// Copy right：
/// All right reserved to B&P.
/// Author email: don_way@163.com
/// </summary>
namespace Sorter
{
    public class VStation : IAssemblyRobot, IRobot
    {
        private readonly MotionController _mc;
        private readonly VisionServer _vision;
        private readonly RoundTable _table;
        private List<CapturePosition> _capturePositions;
        private List<CapturePosition> _userPositionOffsets;

        /// <summary>
        /// Unload stepper motor
        /// </summary>
        public Motor MotorAUnload { get; set; }

        /// <summary>
        /// Load stepper motor.
        /// </summary>
        public Motor MotorA { get; set; }
        public Motor MotorX { get; set; }
        public Motor MotorY { get; set; }
        public Motor MotorZ { get; set; }

        public double SafeXArea { get; set; } = 80;
        public double SafeYArea { get; set; } = -100;
        public Tray UnloadTray { get; set; }
        public bool UnloadTrayCaptured { get; set; }
        public Tray LoadTray { get; set; }
        public double LoadTrayHeight { get; set; } = -35;
        public double UnloadTrayHeight { get; set; } = -32.634;
        public double FixtureHeight { get; set; } = -36.634;

        public bool CheckVacuumValue { get; set; } = false;
        public bool VisionSimulateMode { get; set; } = false;

        public double SafeZHeight { get; set; } = -10;

        /// <summary>
        /// User for next 
        /// </summary>
        public Task<WaitBlock> Preparation {get; set; }
        public Task<WaitBlock> PreparationAsync { get; set; }

        public VStation(MotionController controller, VisionServer vision,
            RoundTable table, List<CapturePosition> positions, List<CapturePosition> offsets)
        {
            _mc = controller;
            _vision = vision;
            _table = table;
            _capturePositions = positions;
            _userPositionOffsets = offsets;
        }

        public Task<WaitBlock> FindBaseUnloadPositionAsync()
        {
            return Task.Run(() => {
                try
                {
                    FindBaseUnloadPosition();
                    return new WaitBlock() { Message="Find tray location OK" };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "Find tray position fail:" + ex.Message };
                }
            });
        }

        public void FindBaseUnloadPosition()
        {
            var unloadCapture1 = GetCapturePosition(CaptureId.VTrayPlaceTop, "1");
            MoveToCapture(unloadCapture1);
            var unloadCaptureVisionResult1 = GetVisionResult(unloadCapture1);

            var unloadCapture2 = GetCapturePosition(CaptureId.VTrayPlaceTop, "2");
            MoveToCapture(unloadCapture2);
            var unloadCaptureVisionResult2 = GetVisionResult(unloadCapture2);

            var unloadCapture3 = GetCapturePosition(CaptureId.VTrayPlaceTop, "3");
            MoveToCapture(unloadCapture3);

            UnloadTray.TrayInfo = GetRawVisionResult(unloadCapture3);

            MoveToSafeHeight();

            UnloadTray.BaseCapturePosition = new CapturePosition()
            {
                XPosition = UnloadTray.TrayInfo.XOffset,
                YPosition =UnloadTray.TrayInfo.YOffset,
            };

            UnloadTray.CurrentPart = new Part()
            {
                XIndex = 0,
                YIndex = 0,
                TargetPose = new Pose()
                {
                    X = UnloadTray.TrayInfo.XOffset,
                    Y=UnloadTray.TrayInfo.YOffset,
                    RUnloadAngle = UnloadTray.TrayInfo.ROffset,
                    Z = UnloadTrayHeight,
                },
                UnloadHeight = UnloadTrayHeight,
            };

            UnloadTrayCaptured = true;
        }

        /// <summary>
        /// Goes to capture for position locating.
        /// </summary>
        /// <param name="target"></param>
        public void MoveToCapture(CapturePosition target)
        {
            var tar = Helper.ConvertToPose(target);
            MoveTo(tar, MoveModeAMotor.None, ActionType.None);
        }

        /// <summary>
        /// Set all speed.
        /// </summary>
        /// <param name="speed"></param>
        public void SetSpeed(double speed)
        {
            MotorX.Velocity = speed;
            MotorY.Velocity = speed;
            MotorZ.Velocity = speed;
            MotorA.Velocity = speed;
            MotorAUnload.Velocity = speed;
        }

        /// <summary>
        /// Motor setup and tray info setup.
        /// </summary>
        public void Setup()
        {
            MotorX = _mc.MotorVX;
            MotorY = _mc.MotorVY;
            MotorZ = _mc.MotorVZ;
            MotorA = _mc.MotorVRotateLoad;
            MotorAUnload = _mc.MotorVRotateUnload;

            LoadTray = new Tray()
            {
                RowCount = 5,
                ColumneCount = 5,
                XOffset = 18.5,
                YOffset = 18.5,
                BaseCapturePosition = GetCapturePosition(CaptureId.VTrayPickTop),
                CurrentPart = new Part()
                {
                    CapturePos = GetCapturePosition(CaptureId.VTrayPickTop),
                    TargetPose = new Pose(),
                },
            };

            UnloadTray = new Tray()
            {
                RowCount = 5,
                ColumneCount = 5,
                XOffset = 18.5,
                YOffset = 18.5,
                
                BaseCapturePosition = GetCapturePosition(CaptureId.VTrayPlaceTop),
                CurrentPart = new Part() {
                    CapturePos = GetCapturePosition(CaptureId.VTrayPickTop),
                    TargetPose = new Pose(),
                },
            };

            //Set as a simulate mode.
            var tempState = VisionSimulateMode;
            VisionSimulateMode = true;
            UnloadTray.CurrentPart.TargetPose = GetVisionResult(UnloadTray.CurrentPart.CapturePos);
            VisionSimulateMode = tempState;

            UnloadTray.TrayHeight = UnloadTrayHeight;
        }

        /// <summary>
        /// For picking and for unload tray locating.
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
                XOffset1 = visionOffset.XOffset1,
                XOffset2 = visionOffset.XOffset2,
                YOffset1 = visionOffset.YOffset1,
                YOffset2 = visionOffset.YOffset2,
            };
        }

        /// <summary>
        /// Find capture positions.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CapturePosition GetCapturePosition(CaptureId id)
        {
            return Helper.GetCapturePosition(_capturePositions, id);
        }

        /// <summary>
        /// For unload tray locating.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public CapturePosition GetCapturePosition(CaptureId id, string tag = "1")
        {
            return Helper.GetCapturePosition(_capturePositions, id, tag);
        }

        private void UnloadPick(Part part)
        {
            if (UnloadTrayCaptured==false)
            {
                throw new Exception("Unload tray has't been captured by camera.");
            }
            var unloadCapturePos = GetCapturePosition(CaptureId.VUnloadHolderTop);
            MoveToCapture(unloadCapturePos);
            var pickPose = GetVisionResult(unloadCapturePos);
            MoveToTarget(pickPose, MoveModeAMotor.Abs, ActionType.Unload);
            Sucker(FixtureId.V, VacuumState.Off, VacuumArea.Circle);
            Sucker(FixtureId.V, VacuumState.Off, VacuumArea.Center);
            Sucker(VacuumState.On, ActionType.Unload);
            MoveToSafeHeight();
        }

        private Pose AngleCompensationUnload(ref Part part)
        {
            var bottomCamCapturePosLoad = GetCapturePosition(CaptureId.VUnloadCompensationBottom);
            MoveToCapture(bottomCamCapturePosLoad);
            Delay(500);
            var angleOffset = GetVisionResult(bottomCamCapturePosLoad);
            MoveAngleMotor( -angleOffset.A, MoveModeAMotor.Relative, ActionType.Unload);
            var xNyOffset = GetVisionResult(bottomCamCapturePosLoad);
            part.TargetPose.X += xNyOffset.X;
            part.TargetPose.Y += xNyOffset.Y;
            return xNyOffset;
        }

        private void UnloadPlace(Part part)
        {
            MoveToTarget(part.TargetPose, MoveModeAMotor.None, ActionType.Unload);
            Sucker(VacuumState.Off, ActionType.Unload);
            MoveToSafeHeight();
        }

        /// <summary>
        /// Unload a part.
        /// </summary>
        /// <param name="part"></param>
        public void Unload(Part part)
        {
            UnloadPick(part);
            AngleCompensationUnload(ref part);
            UnloadPlace(part);

            UnloadTray.CurrentPart = UnloadTray.GetNextPartForUnload(part);
        }

        public async Task<WaitBlock> UnloadAsync(Part part)
        {
            return await Task.Run(() =>
            {
                try
                {
                    Unload(part);
                    return new WaitBlock() { Message = "V unload Finished Successful." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "V unload fail: " + ex.Message };
                }
            });
        }

        public async Task<WaitBlock> PrepareAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    Prepare();
                    return new WaitBlock() { Message = "V unload Finished Successful." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "V unload fail: " + ex.Message };
                }
            });
        }

        private void Prepare()
        {
            LoadPick(LoadTray.CurrentPart);
            AngleCompensationLoad();
            MoveToTarget(GetCapturePosition(CaptureId.VLoadHolderTop), 
                MoveModeAMotor.None, ActionType.None);
        }

        private void Delay(int ms)
        {
            Thread.Sleep(ms);
        }

        private void LoadPick(Part part)
        {
            MoveToCapture(part.CapturePos);
            var pickPose = GetVisionResult(part.CapturePos, 3);
            pickPose.Z = GetZHeight(CaptureId.VTrayPickTop);
            MoveToTarget(pickPose, MoveModeAMotor.None, ActionType.Load);
            Sucker(VacuumState.On, ActionType.Load);
            MoveToSafeHeight();
            SetNextPartLoad();
        }

        private void AngleCompensationLoad()
        {
            //Get angle offset for loading.
            var bottomCamCapturePosLoad = GetCapturePosition(CaptureId.VLoadCompensationBottom);
            MoveToCapture(bottomCamCapturePosLoad);
            Delay(500);
            var bottomCamPos = GetVisionResult(bottomCamCapturePosLoad, 3);
            // 0724 no angle correction right now for V load.
            //MoveAngleMotor(bottomCamPos.A, MoveModeAMotor.Relative, ActionType.Load);
            //var xNyOffset = GetVisionResult(bottomCamCapturePosLoad, 3);

            //return xNyOffset;
            //return null;
        }

        private void LoadPlace(Part part)
        {
            MoveToTarget(part.TargetPose, MoveModeAMotor.None, ActionType.Load);
            Sucker(FixtureId.V, VacuumState.On, VacuumArea.Circle);
            Sucker(VacuumState.Off, ActionType.Load);
            MoveToSafeHeight();
        }

        private Pose GetFixturePose()
        {
            var fixtureCapturePos = GetCapturePosition(CaptureId.VLoadHolderTop);
            MoveToCapture(fixtureCapturePos);
            return GetVisionResult(fixtureCapturePos, 3);
        }

        /// <summary>
        /// Load a part.
        /// </summary>
        /// <param name="part">Para LoadTray current part.</param>
        public void Load(Part part)
        {
            LoadPick(part);

            //No angle correction right now. 0724
            AngleCompensationLoad();

            part.TargetPose =  GetFixturePose();
            part.TargetPose.Z = GetZHeight(CaptureId.VLoadHolderTop);

            LoadPlace(part);

            LoadTray.CurrentPart = LoadTray.GetNextPartForLoad(part);
        }

        /// <summary>
        /// asynchranous Loading a part.
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public async Task<WaitBlock> LoadAsync(Part part)
        {
            return await Task.Run(() =>
            {
                try
                {
                    Load(part);
                    return new WaitBlock() { Message = "V Load Finished Successful." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "V Load fail: " + ex.Message };
                }
            });
        }

        /// <summary>
        /// For next unload and load
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public Task<WaitBlock> PreparationForNextCycleAsync(Part part)
        {
            return Task.Run(() =>
            {
                try
                {
                    //MoveToCapture(part.CapturePos);
                    return new WaitBlock() { Message = "V Load OK" };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "V Load error: " + ex.Message };
                }
            });
        }

        public void PrepareForNextLoad(Part loadPart)
        {
            //Pick(loadPart);
            //CorrectAngle(ref loadPart, MoveModeAMotor.Abs, ActionType.Load);

            //var nextFixtureCapturePosUnload = GetCapturePosition(CaptureId.VUnloadHolderTop);
            //MoveToCapture(nextFixtureCapturePosUnload);
        }

        #region For testing
        public async Task<WaitBlock> LDoingSomthingForALongTime()
        {
            try
            {
                //await Task.Delay(1);
                await SomeBasicTask();
                return new WaitBlock() { Code = ErrorCode.ControllerConnectFail, Message = "V Error" };
            }
            catch (Exception ex)
            {
                return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "V error: " + ex.Message };
            }
        }

        private async Task SomeBasicTask()
        {
            await SomethingHard();
        }

        private async Task SomethingHard()
        {
            await Task.Run(() =>
            {
                Thread.Sleep(1000);
                Thread.Sleep(1000);
                Thread.Sleep(1000);
                Thread.Sleep(1000);
            });

        } 
        #endregion

        public void UnloadAndLoad(Part unloadPart, Part loadPart)
        {
            UnloadPick(unloadPart);
            SetNextPartLoad();

            loadPart.TargetPose = GetFixturePose();
            loadPart.TargetPose.Z = GetZHeight(CaptureId.VLoadHolderTop);

            LoadPlace(loadPart);
            AngleCompensationUnload(ref unloadPart);
            UnloadPlace(unloadPart);

            SetNextPartUnload();
        }

        public async Task<WaitBlock> UnloadAndLoadAsync(Part unloadPart, Part loadPart)
        {
            return await Task.Run(() =>
            {
                try
                {
                    UnloadAndLoad(unloadPart, loadPart);
                    return new WaitBlock() { Message = "V Load Finished Successful." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "V Load fail: " + ex.Message };
                }
            });
        }


        public void Sucker(VacuumState state)
        {
            _mc.VLoadVacuum(state, CheckVacuumValue);
        }

        public void Sucker(VacuumState state, ActionType procedure)
        {
            switch (procedure)
            {
                case ActionType.None:
                    break;
                case ActionType.Load:
                    _mc.VLoadVacuum(state , CheckVacuumValue);
                    break;
                case ActionType.Unload:
                    _mc.VUnloadVacuum(state, CheckVacuumValue);
                    break;
                default:
                    break;
            }          
        }

        public void Sucker(FixtureId id, VacuumState state, VacuumArea area)
        {
            _table.Sucker(id, state, area, CheckVacuumValue);
        }

        /// <summary>
        /// Get encoder position or reference position of motor.
        /// </summary>
        /// <param name="motor"></param>
        /// <returns></returns>
        public double GetPosition(Motor motor)
        {
            return _mc.GetPosition(motor);
        }

        public CapturePosition GetCapturePositionOffset(CaptureId id)
        {
            return new CapturePosition();
            //Todo add offset 
            //throw new NotImplementedException();
        }

        public void MoveToSafeHeight()
        {
            if (GetPosition(MotorZ) < SafeZHeight)
            {
                CylinderHead(HeadCylinderState.Up, ActionType.Load);
                CylinderHead(HeadCylinderState.Up, ActionType.Unload);
                _mc.MoveToTargetTillEnd(MotorZ, SafeZHeight);
            }
        }

        public void MoveToTarget(Pose target, MoveModeAMotor mode, ActionType procedure)
        {
            MoveTo(target, mode, procedure);
        }

        private bool IsMoveToSameArea(Pose target)
        {
            return (GetPosition(MotorY) >= SafeYArea && target.Y >= SafeYArea) ||
                       (GetPosition(MotorX) <= SafeXArea && target.X <= SafeXArea);
        }  

        /// <summary>
        /// Move robot to target.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="mode"></param>
        /// <param name="procedure"></param>
        public void MoveTo(Pose target, MoveModeAMotor mode = MoveModeAMotor.None,
            ActionType procedure = ActionType.Load)
        {
            MoveToSafeHeight();

            if (IsMoveToSameArea(target))
            {
                _mc.MoveToTarget(MotorY, target.Y);
                _mc.MoveToTarget(MotorX, target.X);
                MoveAngleMotor(target.A, mode, procedure);

                _mc.WaitTillEnd(MotorX);
                _mc.WaitTillEnd(MotorY);
                WaitTillEndAngleMotor();

                CylinderHead(HeadCylinderState.Down, procedure);
                _mc.MoveToTargetTillEnd(MotorZ, target.Z);
            }
            else
            {
                //Move from conveyor to table.
                if (GetPosition(MotorY) > SafeYArea &&
                   target.Y < SafeYArea)
                {
                    MoveAngleMotor(target.A, mode, procedure);
                    _mc.MoveToTargetTillEnd(MotorX, target.X);
                    _mc.MoveToTargetTillEnd(MotorY, target.Y);
                    WaitTillEndAngleMotor();

                    CylinderHead(HeadCylinderState.Down, procedure);
                    _mc.MoveToTargetTillEnd(MotorZ, target.Z);
                }
                else
                {
                    //Move from table to conveyor.
                    if (GetPosition(MotorX) < SafeXArea &&
                        target.X > SafeXArea)
                    {
                        MoveAngleMotor(target.A, mode, procedure);
                        _mc.MoveToTargetTillEnd(MotorY, target.Y);
                        _mc.MoveToTargetTillEnd(MotorX, target.X);
                        WaitTillEndAngleMotor();

                        CylinderHead(HeadCylinderState.Down, procedure);
                        _mc.MoveToTargetTillEnd(MotorZ, target.Z);
                    }
                    else
                    {
                        throw new Exception("V robot move to routine goes into bug6516516498513.");
                    }
                }
            }
        }

        private void WaitTillEndAngleMotor()
        {
            _mc.WaitTillEnd(MotorA);
            _mc.WaitTillEnd(MotorAUnload);
        }

        /// <summary>
        /// Move stepper rotation motor.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="mode"></param>
        /// <param name="procedure"></param>
        public void MoveAngleMotor(double angle, MoveModeAMotor mode, ActionType procedure)
        {
            var stepperMotor = MotorA;

            switch (procedure)
            {
                case ActionType.None:
                    break;
                case ActionType.Load:
                    stepperMotor = MotorA;
                    break;
                case ActionType.Unload:
                    stepperMotor = MotorAUnload;
                    break;
                default:
                    break;
            }

            switch (mode)
            {
                case MoveModeAMotor.None:
                    //Motor goes to its current position, meet the wait till end condition.
                    _mc.MoveToTarget(MotorA, MotorA.TargetPosition);
                    _mc.MoveToTarget(MotorAUnload, MotorA.TargetPosition);
                    break;

                case MoveModeAMotor.Abs:
                    _mc.MoveToTarget(stepperMotor, angle);
                    break;
                case MoveModeAMotor.Relative:
                    _mc.MoveToTargetRelative(stepperMotor, angle);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Cylinder with sucker head.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="procedure"></param>
        public void CylinderHead(HeadCylinderState state, ActionType procedure)
        {
            switch (procedure)
            {
                case ActionType.None:
                    break;
                case ActionType.Load:
                    _mc.VLoadHeadCylinder(state);
                    break;
                case ActionType.Unload:
                    _mc.VUnloadHeadCylinder(state);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Get vision raw data.
        /// </summary>
        /// <param name="capturePosition"></param>
        /// <returns></returns>
        public AxisOffset GetRawVisionResult(CapturePosition capturePosition)
        {
            if (VisionSimulateMode)
            {
                return new AxisOffset()
                {
                    XOffset = capturePosition.XPosition + 37, //Camera to sucker distance.
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
        /// Get Z height for moving to target.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public double GetZHeight(CaptureId id)
        {
            switch (id)
            {
                case CaptureId.VTrayPickTop:
                    return LoadTrayHeight;
                case CaptureId.VLoadCompensationBottom:
                    return GetCapturePosition(CaptureId.VLoadCompensationBottom).ZPosition;
                case CaptureId.VLoadHolderTop:
                    return FixtureHeight;
                case CaptureId.VUnloadHolderTop:
                    return FixtureHeight;
                case CaptureId.VUnloadCompensationBottom:
                    return GetCapturePosition(CaptureId.VUnloadCompensationBottom).ZPosition;
                case CaptureId.VTrayPlaceTop:
                    return UnloadTrayHeight;
                default:
                    throw new NotImplementedException("No such Z height in V station:" + id);
            }
        }

        /// <summary>
        /// Unlock tray of load or unload.
        /// </summary>
        /// <param name="procedure"></param>
        public void UnlockTray(ActionType procedure = ActionType.Load)
        {
            switch (procedure)
            {
                case ActionType.Load:
                    _mc.VLoadConveyorLocker(LockState.Off);
                    break;
                case ActionType.Unload:
                    _mc.VUnloadConveyorLocker(LockState.Off);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Lock tray for load or unload.
        /// </summary>
        /// <param name="procedure"></param>
        public void LockTray(ActionType procedure = ActionType.Load)
        {
            switch (procedure)
            {
                case ActionType.Load:
                    _mc.VLoadConveyorLocker(LockState.On);
                    break;
                case ActionType.Unload:
                    _mc.VUnloadConveyorLocker(LockState.On);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Has retry times.
        /// </summary>
        /// <param name="retryTimes"></param>
        /// <param name="action"></param>
        public void Sucker(int retryTimes, ActionType action)
        {
            int retryCount = 0;
            do
            {
                try
                {
                    Sucker(VacuumState.On, action);
                    return;
                }
                catch (Exception)
                {
                    retryCount++;
                    Sucker(VacuumState.Off, action);
                    RiseZALittleAndDown();
                }
            } while (retryCount > retryTimes);

            //Todo throw new NeedToSuckANewLoadException.
            // deal with it.
        }

        public void RiseZALittleAndDown()
        {
            _mc.MoveToTargetRelativeTillEnd(MotorZ, 1);
            _mc.MoveToTargetRelativeTillEnd(MotorZ, -1);
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

            throw new Exception("Vision fail three times. Ng bin action coming soon.");
        }

        public void NgBin(Part part)
        {
            //throw new NotImplementedException();
        }

        public void SetNextPartLoad()
        {
            LoadTray.CurrentPart = LoadTray.GetNextPartForLoad(LoadTray.CurrentPart);
        }

        public void SetNextPartUnload()
        {
            UnloadTray.CurrentPart = UnloadTray.GetNextPartForUnload(UnloadTray.CurrentPart);
        }

        public void Sucker(VacuumState state, int retryTimes, ActionType action)
        {
            throw new NotImplementedException();
        }

        void IAssemblyRobot.Work()
        {
            throw new NotImplementedException();
        }

        public Task<WaitBlock> WorkAsync()
        {
            throw new NotImplementedException();
        }

        public void MoveToTarget(CapturePosition target, MoveModeAMotor mode, ActionType type)
        {
            var tar = Helper.ConvertToPose(target);
            MoveToTarget(tar, mode, type);
        }
    }
}
