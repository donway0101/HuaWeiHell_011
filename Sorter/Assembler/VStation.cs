using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bp.Mes;

namespace Sorter
{
    public class VStation : IRobot
    {
        private readonly MotionController _mc;
        private readonly VisionServer _vision;
        private readonly RoundTable _table;

        /// <summary>
        /// Unload 
        /// </summary>
        public Motor MotorAUnload { get; set; }

        /// <summary>
        /// Load stepper motor.
        /// </summary>
        public Motor MotorA { get; set; }
        public Motor MotorX { get; set; }
        public Motor MotorY { get; set; }
        public Motor MotorZ { get; set; }
        public double SafeXArea { get; set; } = 100;
        public double SafeYArea { get; set; } = -100;
        public Tray UnloadTray { get; set; }
        public Tray LoadTray { get; set; }
        public double LoadTrayHeight { get; set; } = -35.935;
        public double UnloadTrayHeight { get; set; } = -32.634;
        public double FixtureHeight { get; set; } = -36.634;
        public bool CheckVacuumValue { get; set; } = false;
        public bool VisionSimulateMode { get; set; } = false;
        public double SafeZHeight { get; set; }

        /// <summary>
        /// Camera take three picture to get this position.
        /// </summary>
        public Pose BaseUnloadPosition { get; set; }
        public Task<WaitBlock> Preparation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private CapturePosition[] _capturePositions;
        private CapturePosition[] _capturePositionOffsets;

        public VStation(MotionController controller, VisionServer vision,
            RoundTable table, CapturePosition[] positions, CapturePosition[] offsets)
        {
            _mc = controller;
            _vision = vision;
            _table = table;
            _capturePositions = positions;
            _capturePositionOffsets = offsets;
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
        }

        public void MoveToCapture(CapturePosition target)
        {
            var tar = Helper.ConvertCapturePositionToPose(target);
            MoveTo(tar, ActionType.None);
        }

        public void MoveToTarget(CapturePosition target)
        {
            var tar = Helper.ConvertCapturePositionToPose(target);
            MoveTo(tar);
        }

        public void SetSpeed(double speed)
        {
            MotorX.Velocity = speed;
            MotorY.Velocity = speed;
            MotorZ.Velocity = speed;
            MotorA.Velocity = speed;
            MotorAUnload.Velocity = speed;
        }

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
                CurrentPart = new Part() { CapturePos = GetCapturePosition(CaptureId.VTrayPickTop) },
            };

            UnloadTray = new Tray()
            {
                RowCount = 5,
                ColumneCount = 5,
                XOffset = 18.5,
                YOffset = 18.5,
                
                BaseCapturePosition = GetCapturePosition(CaptureId.VTrayPlaceTop),
                CurrentPart = new Part() { CapturePos = GetCapturePosition(CaptureId.VTrayPickTop) },
            };

            var tempState = VisionSimulateMode;
            VisionSimulateMode = true;
            UnloadTray.CurrentPart.TargetPose = GetVisionResult(UnloadTray.CurrentPart.CapturePos);
            VisionSimulateMode = tempState;

            UnloadTray.TrayHeight = UnloadTrayHeight;
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
                XOffset1 = visionOffset.XOffset1,
                XOffset2 = visionOffset.XOffset2,
                YOffset1 = visionOffset.YOffset1,
                YOffset2 = visionOffset.YOffset2,
            };
        }

        public CapturePosition GetCapturePosition(CaptureId id)
        {
            return Helper.FindCapturePosition(_capturePositions, id);
        }

        public CapturePosition GetCapturePosition(CaptureId id, string tag = "1")
        {
            return Helper.GetCapturePosition(_capturePositions, id, tag);
        }

        public void Unload(Part unloadPart)
        {
            //var unloadCapturePos = GetCapturePosition(CaptureId.VUnloadHolderTop);
            //MoveToCapture(unloadCapturePos);
            //var pickPose = GetVisionResult(unloadPart.CapturePos);
            //MoveToTarget(pickPose, ActionType.Load);
            ////Todo Table sucker off
            //Sucker(VacuumState.On, ActionType.Load);

            //var bottomCamCapturePosLoad = GetCapturePosition(CaptureId.VLoadCompensationBottom);
            //MoveToCapture(bottomCamCapturePosLoad);
            //var angleOffset = GetVisionResult(bottomCamCapturePosLoad);
            //CorrectAngle(angleOffset.A, ActionType.Unload);
            //var coordinateOffset = GetVisionResult(bottomCamCapturePosLoad);

            //unloadPart.TargetPose.X += coordinateOffset.X;
            //unloadPart.TargetPose.Y += coordinateOffset.Y;

            MoveToTarget(unloadPart.TargetPose, ActionType.Unload);
            //Sucker(VacuumState.Off, ActionType.Unload);
            MoveToSafeHeight();

            UnloadTray.CurrentPart = UnloadTray.GetNextPartForUnload(unloadPart);
        }

        /// <summary>
        /// Load a part and get next part.
        /// </summary>
        /// <param name="part">Para LoadTray current part.</param>
        public void Load(Part part)
        {
            MoveToCapture(part.CapturePos);
            var pickPose = GetVisionResult(part.CapturePos);
            MoveToTarget(pickPose, ActionType.Load);
            Sucker(VacuumState.On, ActionType.Load);

            var bottomCamCapturePosLoad = GetCapturePosition(CaptureId.VLoadCompensationBottom);
            MoveToCapture(bottomCamCapturePosLoad);
            GetVisionResult(bottomCamCapturePosLoad);

            var fixtureCapturePosLoad = GetCapturePosition(CaptureId.VLoadHolderTop);
            MoveToCapture(fixtureCapturePosLoad);
            var loadPose = GetVisionResult(fixtureCapturePosLoad);
            MoveToTarget(loadPose, ActionType.Load);
            Sucker(VacuumState.Off, ActionType.Load);
            Sucker(FixtureId.V, VacuumState.On, VacuumArea.Circle);
            MoveToSafeHeight();

            LoadTray.CurrentPart = LoadTray.GetNextPartForLoad(part);
            //MoveToNextCaptureAsync(LoadTray.CurrentPart);
        }

        public Task<WaitBlock> LoadAsync(Part part)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1);
                    Load(part);
                    return new WaitBlock() { Message = "V Load OK" };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "V Load error: " + ex.Message };
                }
            });
        }

        public Task<WaitBlock> MoveToNextCaptureAsync(Part part)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1);
                    MoveToCapture(part.CapturePos);
                    return new WaitBlock() { Message = "V Load OK" };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "V Load error: " + ex.Message };
                }
            });
        }

        private void Unload()
        {
            //Unload a finished product.
            var fixtureCapturePosUnload = GetCapturePosition(CaptureId.VUnloadHolderTop);
            MoveToCapture(fixtureCapturePosUnload);
            var unloadPose = GetVisionResult(fixtureCapturePosUnload);
            MoveToTarget(unloadPose, ActionType.Unload);
            Sucker(FixtureId.V, VacuumState.Off, VacuumArea.Center);
            Sucker(FixtureId.V, VacuumState.Off, VacuumArea.Circle);
            Sucker(VacuumState.On, ActionType.Unload);
        }

        private void Load()
        {
            //Load a Part
            var fixtureCapturePosLoad = GetCapturePosition(CaptureId.VLoadHolderTop);
            MoveToCapture(fixtureCapturePosLoad);
            var loadPose = GetVisionResult(fixtureCapturePosLoad);
            MoveToTarget(loadPose, ActionType.Load);
            Sucker(FixtureId.V, VacuumState.On, VacuumArea.Circle);
            Sucker(VacuumState.Off, ActionType.Load);
        }

        private void Place(Part part)
        {
            if (VisionSimulateMode)
            {
                var unloadCapture = GetCapturePosition(CaptureId.VTrayPlaceTop);
                MoveToCapture(unloadCapture);
                var unloadPos = GetVisionResult(unloadCapture);
                MoveToTarget(unloadPos, ActionType.Unload);
            }
            else
            {
                MoveToTarget(part.TargetPose, ActionType.Unload);
                Sucker(VacuumState.Off, ActionType.Unload);
                MoveToSafeHeight();
                UnloadTray.CurrentPart = UnloadTray.GetNextPartForUnload(part);
            }
        }

        private void Pick(Part loadPart)
        {
            //Pick a new part for next load.
            MoveToCapture(loadPart.CapturePos);
            var pickPose = GetVisionResult(loadPart.CapturePos);
            MoveToTarget(pickPose, ActionType.Load);
            Sucker(VacuumState.On, ActionType.Load);
        }

        public void PrepareForNextLoad(Part loadPart)
        {
            Pick(loadPart);
            CorrectAngle(ref loadPart, ActionType.Load);

            var nextFixtureCapturePosUnload = GetCapturePosition(CaptureId.VUnloadHolderTop);
            MoveToCapture(nextFixtureCapturePosUnload);
        }

        public void UnloadAndLoad(Part unloadPart, Part loadPart)
        {
            Unload();
            Load();
            CorrectAngle(ref unloadPart, ActionType.Unload);
            Place(unloadPart);

            SetNextPartLoad();
            SetNextPartUnload();

            PrepareForNextLoad(LoadTray.CurrentPart);
        }

        /// <summary>
        /// Correct angle and return x offset and y offset.
        /// </summary>
        /// <param name="part"></param>
        public  void CorrectAngle(ref Part part, ActionType action)
        {
            switch (action)
            {
                case ActionType.None:
                    break;
                case ActionType.Load:
                    //Get angle offset for unload.
                    var bottomCamCapturePosLoad = GetCapturePosition(CaptureId.VLoadCompensationBottom);
                    MoveToCapture(bottomCamCapturePosLoad);
                    var angleOffsetLoad = GetVisionResult(bottomCamCapturePosLoad);
                    CorrectAngle(angleOffsetLoad.A, ActionType.Unload);
                    var xNyOffsetLoad = GetVisionResult(bottomCamCapturePosLoad);
                    part.XOffset = xNyOffsetLoad.X;
                    part.YOffset += xNyOffsetLoad.Y;
                    break;
                case ActionType.Unload:
                    //Get angle offset for unload.
                    var bottomCamCapturePosUnload = GetCapturePosition(CaptureId.VUnloadCompensationBottom);
                    MoveToCapture(bottomCamCapturePosUnload);
                    var angleOffsetUnload = GetVisionResult(bottomCamCapturePosUnload);
                    CorrectAngle(angleOffsetUnload.A, ActionType.Unload);
                    var xNyOffsetUnload = GetVisionResult(bottomCamCapturePosUnload);
                    part.XOffset += xNyOffsetUnload.X;
                    part.YOffset += xNyOffsetUnload.Y;
                    break;
                default:
                    break;
            }
        }

        public void Sucker(VacuumState state)
        {
            throw new NotImplementedException();
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

        public Task<WaitBlock> Work()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1);
                    UnloadAndLoad(UnloadTray.CurrentPart, LoadTray.CurrentPart);
                    return new WaitBlock() { Message = "V OK" };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "V Station error: " + ex.Message };
                }
            });
        }

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

        public void MoveToTarget(Pose target)
        {
            MoveTo(target);
        }

        public void MoveToTarget(Pose target, ActionType procedure)
        {
            MoveTo(target, procedure);
        }

        private bool IsMoveToSameArea(Pose target)
        {
            return (GetPosition(MotorY) >= SafeYArea && target.Y >= SafeYArea) ||
                       (GetPosition(MotorX) <= SafeXArea && target.X <= SafeXArea);
        }  

        private void MoveTo(Pose target, ActionType procedure = ActionType.None)
        {
            MoveToSafeHeight();

            if (IsMoveToSameArea(target))
            {
                _mc.MoveToTarget(MotorY, target.Y);
                _mc.MoveToTarget(MotorX, target.X);
                MoveAngleMotor(target, procedure);

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
                    MoveAngleMotor(target, procedure);
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
                        MoveAngleMotor(target, procedure);
                        _mc.MoveToTargetTillEnd(MotorY, target.Y);
                        _mc.MoveToTargetTillEnd(MotorX, target.X);
                        WaitTillEndAngleMotor();
                        CylinderHead(HeadCylinderState.Down, procedure);
                        _mc.MoveToTargetTillEnd(MotorZ, target.Z);
                    }
                    else
                    {
                        throw new Exception("Robot is in unknown position, need to go home first.");
                    }
                }
            }
        }

        private void WaitTillEndAngleMotor()
        {
            _mc.WaitTillEnd(MotorA);
            _mc.WaitTillEnd(MotorAUnload);
        }

        private void MoveAngleMotor(Pose target, ActionType procedure)
        {
            switch (procedure)
            {
                case ActionType.None:
                    break;
                case ActionType.Load:
                    _mc.MoveToTargetRelative(MotorA, target.A);
                    break;
                case ActionType.Unload:
                    _mc.MoveToTargetRelative(MotorAUnload, target.A);
                    break;
                default:
                    break;
            }
        }

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
            _mc.MoveToTargetRelativeTillEnd(MotorZ, 1);
        }

        public void CorrectAngle(double relativeAngle, ActionType action)
        {
            switch (action)
            {
                case ActionType.None:
                    break;
                case ActionType.Load:
                    _mc.MoveToTargetRelativeTillEnd(MotorA, -relativeAngle);
                    break;
                case ActionType.Unload:
                    _mc.MoveToTargetRelativeTillEnd(MotorAUnload, -relativeAngle);
                    break;
                default:
                    break;
            }
            
        }

        public AxisOffset GetVisionResult(CapturePosition capturePosition, int retryTimes)
        {
            throw new NotImplementedException();
        }

        public void NgBin(Part part)
        {
            throw new NotImplementedException();
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

        public void MoveRotaryMotor(double angle, MoveMode mode, ActionType type)
        {
            throw new NotImplementedException();
        }
    }
}
