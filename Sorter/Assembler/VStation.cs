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
        private readonly ITrayStation _loadTrayStation;
        private readonly ITrayStation _unloadTrayStation;
        private List<CapturePosition> _capturePositions;
        private List<CapturePosition> _capturePositionsOffsets;

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

        public double SafeXArea { get; set; } = 30;
        public double SafeYArea { get; set; } = -90;
        public Tray UnloadTray { get; set; }
        public bool UnloadTrayCaptured { get; set; }
        public bool LoadTrayLocked { get; set; }
        public bool UnloadTrayLocked { get; set; }
        public Tray LoadTray { get; set; }
        public double LoadTrayHeight { get; set; } = -35;
        public double UnloadTrayHeight { get; set; } = -32.634;
        public double FixtureHeight { get; set; } = -37.134;

        public bool CheckVacuumValue { get; set; } = false;
        public bool VisionSimulateMode { get; set; } = false;

        public double SafeZHeight { get; set; } = -10;

        /// <summary>
        /// User for next 
        /// </summary>
        public Task<WaitBlock> Preparation {get; set; }

        public bool HasPartOnLoadSucker { get; set; }

        public bool HasPartOnUnloadSucker { get; set; }

        public int CurrentCycleId { get; set; }

        public int MaxFailCount { get; set; } = 10;

        public bool StopProduction { get; set; }
        public Task<WaitBlock> ChangeLoadTray { get; set; }
        public Task<WaitBlock> ChangeUnloadTray { get; set; }

        public VStation(MotionController controller, VisionServer vision, RoundTable table,
            VLoadTrayStation vLoadTrayStation, VUnloadTrayStation vUnloadTrayStation,
            List<CapturePosition> positions, List<CapturePosition> offsets)
        {
            _mc = controller;
            _vision = vision;
            _table = table;
            _loadTrayStation = vLoadTrayStation;
            _unloadTrayStation = vUnloadTrayStation;
            _capturePositions = positions;
            _capturePositionsOffsets = offsets;
        }

        public async Task<WaitBlock> FindBaseUnloadPositionAsync()
        {
            return await Task.Run(() => {
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
            MoveTo(tar, MoveModeAMotor.Abs, ActionType.None);
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
                YIncreaseDirection = -1,
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

            Preparation = Helper.DummyAsyncTask();
            ChangeLoadTray = Helper.DummyAsyncTask();
            ChangeUnloadTray = Helper.DummyAsyncTask();
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
            var pickPose = GetVisionResult(unloadCapturePos, 3);

            MoveToTarget(pickPose, MoveModeAMotor.Abs, ActionType.Unload);
            Sucker(FixtureId.V, VacuumState.Off, VacuumArea.Circle);
            Sucker(FixtureId.V, VacuumState.Off, VacuumArea.Center);
            Sucker(3, ActionType.Unload);

            MoveToSafeHeight();

            _table.Fixtures[(int)StationId.V].IsEmpty = true;
            HasPartOnUnloadSucker = true;
        }

        /// <summary>
        /// Todo open expetion when vision is ready.
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        private Pose AngleCompensationUnload(ref Part part)
        {
            var bottomCamCapturePosLoad = GetCapturePosition(CaptureId.VUnloadCompensationBottom);
            MoveToCapture(bottomCamCapturePosLoad);
            Delay(500);

            Pose xNyOffset = new Pose();
            var angleOffset = GetVisionResult(bottomCamCapturePosLoad, 3);
            MoveAngleMotor(-angleOffset.A, MoveModeAMotor.Relative, ActionType.Unload);
            xNyOffset = GetVisionResult(bottomCamCapturePosLoad, 3);

            part.TargetPose.X += xNyOffset.X;
            part.TargetPose.Y += xNyOffset.Y;

            return xNyOffset;
        }

        /// <summary>
        /// Only place successful will set next part.
        /// </summary>
        /// <param name="part"></param>
        private void UnloadPlace(Part part)
        {
            MoveToTarget(part.TargetPose, MoveModeAMotor.None, ActionType.Unload);
            Sucker(VacuumState.Off, ActionType.Unload);
            MoveToSafeHeight();

            SetNextPartUnload();
        }

        private void Delay(int ms)
        {
            Thread.Sleep(ms);
        }
 
        /// <summary>
        /// Todo add angle correction.
        /// </summary>
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

            _table.Fixtures[(int)StationId.V].IsEmpty = false;
            _table.Fixtures[(int)StationId.V].NG = false;
        }

        private Pose GetFixturePose()
        {
            var fixtureCapturePos = GetCapturePosition(CaptureId.VLoadHolderTop);
            MoveToCapture(fixtureCapturePos);
            return GetVisionResult(fixtureCapturePos, 3);
        }

        public void Load(Part part)
        {
            throw new NotImplementedException();
        }

        public Task<WaitBlock> LoadAsync(Part part)
        {
            throw new NotImplementedException();
        }

        public void PickAndReady(Part part)
        {
            MoveToCapture(part.CapturePos);
            var pickPose = GetVisionResult(part.CapturePos, 3);
            MoveToTarget(pickPose, MoveModeAMotor.None, ActionType.Load);
            Sucker(3, ActionType.Load);
            MoveToSafeHeight();
            HasPartOnLoadSucker = true;

            SetNextPartLoad();
            AngleCompensationLoad();
            MoveToTarget(GetCapturePosition(CaptureId.VUnloadHolderTopReady),
                MoveModeAMotor.None, ActionType.None);         
        }

        /// <summary>
        /// Allow table rotation.
        /// </summary>
        /// <param name="unloadPart"></param>
        /// <param name="loadPart"></param>
        /// <seealso cref="PlaceOrBin"/>
        public void UnloadAndLoad(Part unloadPart, Part loadPart)
        {
            if (_table.Fixtures[(int)StationId.V].IsEmpty == false)
            {
                UnloadPick(unloadPart);
            }

            if (StopProduction == false)
            {
                loadPart.TargetPose = GetFixturePose();
                loadPart.TargetPose.Z = GetZHeight(CaptureId.VLoadHolderTop);
                LoadPlace(loadPart);
            }                            
        }

        /// <summary>
        /// The part is already on sucker.
        /// </summary>
        /// <seealso cref="UnloadAndLoad(Part, Part)"/>
        public void PlaceOrBin(Part unloadPart)
        {
            if (HasPartOnUnloadSucker == false)
            {
                return;
            }

            if (_table.Fixtures[(int)StationId.V].NG)
            {
                Bin(ActionType.Unload);
            }
            else
            {
                AngleCompensationUnload(ref unloadPart);
                UnloadPlace(unloadPart);
            }
        }

        public void Sucker(VacuumState state)
        {
            throw new NotImplementedException();
        }

        public void Sucker(VacuumState state, ActionType procedure)
        {
            try
            {
                switch (procedure)
                {
                    case ActionType.None:
                        break;
                    case ActionType.Load:
                        _mc.VLoadVacuum(state, true);
                        //_mc.VLoadVacuum(state , CheckVacuumValue);
                        break;
                    case ActionType.Unload:
                        _mc.VUnloadVacuum(state, true);
                        //_mc.VUnloadVacuum(state, CheckVacuumValue);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                throw new SuckerException() { Type = procedure };
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
                //Todo each capture delay for 500 ms.
                Delay(500);
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

        public async Task<WaitBlock> LockTrayAsync(ActionType type)
        {
            return await Task.Run(() => {
                try
                {
                    LockTray(type);
                    return new WaitBlock() { Message = "LockTray Finished Successful." };
                }
                catch (Exception)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.TobeCompleted,
                        Message = "LockTray Finished Successful."
                    };
                }
            });
        }

        public void Sucker(int retryTimes, ActionType action)
        {
            int failCount = 0;
            do
            {
                try
                {
                    Sucker(VacuumState.On, action);
                    return;
                }
                catch (SuckerException)
                {
                    failCount++;
                    Sucker(VacuumState.Off, action);
                    RiseZALittleAndDown();
                }
            } while (failCount < retryTimes);

            throw new SuckerException() ;
        }

        public void RiseZALittleAndDown()
        {
            _mc.MoveToTargetRelativeTillEnd(MotorZ, 3);
            _mc.MoveToTargetRelativeTillEnd(MotorZ, -3.3);
        }

        public Pose GetVisionResult(CapturePosition capPos, int retryTimes)
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

        public void SetNextPartLoad()
        {
            LoadTray.CurrentPart = LoadTray.GetNextPartForLoad(LoadTray.CurrentPart);
            if (LoadTray.NeedChanged == true)
            {
                ChangeLoadTray = ChangeLoadTrayAsync();
            }
        }

        public void SetNextPartUnload()
        {
            UnloadTray.CurrentPart = UnloadTray.GetNextPartForUnload(UnloadTray.CurrentPart);
            if (UnloadTray.NeedChanged==true)
            {
                ChangeUnloadTray = ChangeUnloadTrayAsync();
            }
        }

        public void MoveToTarget(CapturePosition target, MoveModeAMotor mode, ActionType type)
        {
            var tar = Helper.ConvertToPose(target);
            MoveToTarget(tar, mode, type);
        }

        public void Bin(ActionType type) 
        {
            MoveToCapture(GetCapturePosition(CaptureId.VBin));
            Sucker(VacuumState.Off, type);
        }

        public void Prepare()
        {
            PlaceOrBin(UnloadTray.CurrentPart);

            if (StopProduction == false)
            {
                PickAndReady(LoadTray.CurrentPart);
            }           
        }

        /// <summary>
        /// If operator take care of the work, then set prepare as successful.
        /// </summary>
        /// <returns></returns>
        public async Task<WaitBlock> PrepareAsync()
        {
            return await Task.Run(async() =>
            {
                int failCount = 0;
                string remarks = string.Empty;

                do
                {
                    try
                    {
                        await ChangeLoadTray;
                        Helper.CheckTaskResult(ChangeLoadTray);

                        await ChangeUnloadTray;
                        Helper.CheckTaskResult(ChangeUnloadTray);

                        Prepare();
                        return new WaitBlock()
                        {
                            Message = "V Preparation Finished."
                        };
                    }

                    #region Vision exception
                    catch (VisionException vex)
                    {
                        failCount++;
                        try
                        {
                            switch (vex.CaptureId)
                            {
                                case CaptureId.VTrayPickTop:
                                        SetNextPartLoad();
                                        continue;

                                case CaptureId.VLoadCompensationBottom:
                                        Bin(ActionType.Load);
                                        HasPartOnLoadSucker = false;
                                        continue;

                                case CaptureId.VUnloadCompensationBottom:
                                        Bin(ActionType.Unload);
                                        HasPartOnUnloadSucker = false;
                                        continue;

                                default:
                                    throw new NotImplementedException("Error 465465413216894");
                            }
                        }
                        catch (Exception ex)
                        {
                            return new WaitBlock()
                            {
                                Code = ErrorCode.VStationPrepareFail,
                                Message = ex.Message
                            };
                        }
                    }
                    #endregion

                    #region Sucker exception.
                    catch (SuckerException)
                    {
                        failCount++;
                        try
                        {
                            SetNextPartLoad();
                            continue;
                        }
                        catch (Exception ex)
                        {
                            return new WaitBlock()
                            {
                                Code = ErrorCode.VStationPrepareFail,
                                Message = ex.Message
                            };
                        }
                    }

                    #endregion

                    catch (Exception ex)
                    {
                        return new WaitBlock()
                        {
                            Code = ErrorCode.TobeCompleted,
                            Message = "V Preparation fail: " + ex.Message
                        };
                    }

                } while (failCount < MaxFailCount);

                return new WaitBlock()
                {
                    Code = ErrorCode.VStationPrepareFail,
                    Message = "Preparation reaches max count.",
                };
            });
        }

        public void SetForTest()
        {
            LockTray(ActionType.Load);
            LockTray(ActionType.Unload);
        }

        /// <summary>
        /// Need to wait last prepare work to finish.
        /// </summary>
        /// <returns></returns>
        public async Task<WaitBlock> WorkAsync(int cycleId)
        {
            return await Task.Run( async() =>
            {
                #region Skip work,wait for other station.
                if (CurrentCycleId >= cycleId)
                {
                    CurrentCycleId = cycleId;
                    return new WaitBlock()
                    {
                        Message = "VWorkAsync Skip, has to wait for other station."
                    };
                }
                #endregion

                int failCount = 0;
                string remarks = string.Empty;

                do
                {
                    try
                    {
                        await Preparation;
                        Helper.CheckTaskResult(Preparation);

                        if (HasPartOnLoadSucker == false)
                        {
                            Preparation = PrepareAsync();
                            await Preparation;
                            Helper.CheckTaskResult(Preparation);
                        }

                        UnloadAndLoad(UnloadTray.CurrentPart, LoadTray.CurrentPart);

                        Preparation = PrepareAsync();

                        CurrentCycleId = cycleId;
                        return new WaitBlock()
                        {
                            Message = "V Work Finished."
                        };
                    }

                    #region Vision exception.
                    catch (VisionException vEx)
                    {
                        failCount++;
                        try
                        {
                            switch (vEx.CaptureId)
                            {
                                case CaptureId.VLoadHolderTop:
                                    UnloadPick(UnloadTray.CurrentPart);
                                    Bin(ActionType.Unload);
                                    continue;
                                case CaptureId.VUnloadHolderTop:
                                    _table.Fixtures[(int)StationId.V].IsEmpty = true;
                                    return new WaitBlock()
                                    {
                                        Code = ErrorCode.VStationWorkFail,
                                        Message = "Has to remove V part on fixture manually.",
                                    };

                                default:
                                    throw new NotImplementedException("Vision exception 1654651654981");
                            }
                        }
                        catch (Exception ex)
                        {
                            return new WaitBlock()
                            {
                                Code = ErrorCode.VStationWorkFail,
                                Message = ex.Message
                            };
                        }
                        
                    } 
                    #endregion

                    catch (Exception ex)
                    {
                        return new WaitBlock()
                        {
                            Code = ErrorCode.VStationWorkFail,
                            Message = ex.Message
                        };
                    }

                } while (failCount < MaxFailCount);

                return new WaitBlock()
                {
                    Code = ErrorCode.VStationWorkFail,
                    Message = "Reaches max fail count."
                };

            });
        }

        public void Reset()
        {
            if (Preparation.Result.Code != ErrorCode.Sucessful)
            {
                Preparation = Helper.DummyAsyncTask();
            }
        }

        public async Task<WaitBlock> ChangeLoadTrayAsync()
        {
            return await Task.Run(() =>
            {
                string remarks = string.Empty;

                try
                {
                    _loadTrayStation.UnloadATray();
                    _loadTrayStation.LoadATray();

                    LoadTray.CurrentPart.XIndex = 0;
                    LoadTray.CurrentPart.YIndex = 0;

                    return new WaitBlock()
                    {
                        Message = "ChangeLoadTray Finished."
                    };
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.TobeCompleted,
                        Message = "ChangeLoadTray fail: " + ex.Message
                    };
                }
            });
        }

        public async Task<WaitBlock> ChangeUnloadTrayAsync()
        {
            return await Task.Run(() =>
            {
                string remarks = string.Empty;

                try
                {
                    _loadTrayStation.UnloadATray();
                    _loadTrayStation.LoadATray();

                    LoadTray.CurrentPart.XIndex = 0;
                    LoadTray.CurrentPart.YIndex = 0;

                    return new WaitBlock()
                    {
                        Message = "ChangeUnloadTray Finished."
                    };
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.TobeCompleted,
                        Message = "ChangeUnloadTray fail: " + ex.Message
                    };
                }
            });
        }
    }
}
