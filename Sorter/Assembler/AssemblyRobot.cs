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
    public class AssemblyRobot : IAssemblyRobot1
    {
        private readonly MotionController _mc;

        private readonly VisionServer _vision;

        private readonly RoundTable _table;

        private readonly CapturePosition[] _capturePositions;
        public Motor MotorX { get; set; }
        public Motor MotorY { get; set; }
        public Motor MotorZ { get; set; }
        public Motor MotorRotateLoad { get; set; }
        public Motor MotorRotateUnload { get; set; }

        public bool VisionSimulateMode { get; set; } = false;
        public bool VacuumSimulateMode { get; set; } = true;

        /// <summary>
        /// Sucker avoid collision.
        /// </summary>
        public double LSafeHeight { get; set; } = -30;

        public double VSafeHeight { get; set; } = -15;

        public double SafeXAreaSmaller { get; set; } = 100;

        public double SafeYAreaVBigger { get; set; } = -100;

        public double SafeYAreaLSmaller { get; set; } = 100;

        public StationId StationId { get; set; } = StationId.L;

        public Tray LoadTray { get; set; }

        public Tray UnloadTray { get; set; }

        public double LPickHeight { get; set; } = -57.44;
        public double LPlaceHeight { get; set; } = -52.63;
        public double LHolderHeight { get; set; } = -52.63;

        public double VPickHeight { get; set; } = -29.5;
        public double VPlaceHeight { get; set; } = -29.5;
        public double VHolderHeight { get; set; } = -30.55;

        public CapturePosition BaseTrayCapturePostitionLoad { get; set; }
        public CapturePosition BottomCapturePostitionLoad { get; set; }
        public CapturePosition HolderCapturePostitionLoad { get; set; }
        public CapturePosition BaseTrayCapturePostitionUnload { get; set; }
        public CapturePosition BottomCapturePostitionUnload { get; set; }
        public CapturePosition HolderCapturePostitionUnload { get; set; }

        public void Work()
        {

        }

        public AxisOffset GetVisionResult(CapturePosition capture)
        {
            if (VisionSimulateMode)
            {
                double offset = 65;
                if (StationId== StationId.V)
                {
                    offset = 37;
                }
                return new AxisOffset()
                {
                    XOffset = capture.XPosition + offset,
                    YOffset = capture.YPosition,
                };
            }
            return _vision.RequestVisionCalibration(capture);
        }

        /// <summary>
        /// Todo singular
        /// </summary>
        /// <param name="part"></param>
        public void LLoad(Part part)
        {
            MoveToTarget(part.CapturePos, ProcedureId.Load);
            var trayOffset = GetVisionResult(part.CapturePos);

            var pickPose = Helper.ConvertAxisOffsetToPose(trayOffset);
            pickPose.Z = LPickHeight;

            MoveToTarget(pickPose, ProcedureId.Load);
            VacuumSucker(VacuumState.On, ProcedureId.Load);
            MoveZToSafeHeight();

            MoveToTarget(BottomCapturePostitionLoad, ProcedureId.Load);
            GetVisionResult(BottomCapturePostitionLoad);

            MoveToTarget(HolderCapturePostitionLoad, ProcedureId.Load);
            var holderOffset = GetVisionResult(HolderCapturePostitionLoad);

            var holderPose = Helper.ConvertAxisOffsetToPose(holderOffset);
            holderPose.Z = LHolderHeight;

            MoveToTarget(holderPose, ProcedureId.Load);

            VacuumSucker(HolderId.L, VacuumState.On, VacuumArea.Center);
            VacuumSucker(VacuumState.Off, ProcedureId.Load);

            LoadTray.CurrentPart = LoadTray.GetNextPart(part);
            MoveToTarget(LoadTray.CurrentPart.CapturePos, ProcedureId.Load);
        }

        public void VUnloadAndLoad(Part unloadPart, Part loadPart)
        {
            //Unload and keep it in hand
            MoveToCapture(HolderCapturePostitionUnload);
            var trayOffset = GetVisionResult(HolderCapturePostitionUnload);
            var unloadPose = Helper.ConvertAxisOffsetToPose(trayOffset);
            unloadPose.Z = VHolderHeight;
            MoveToTarget(unloadPose, ProcedureId.Unload);

            VacuumSucker(VacuumState.On, ProcedureId.Unload);
            VacuumSucker(HolderId.V, VacuumState.Off, VacuumArea.Circle);
            MoveZToSafeHeight();

            //Load another hand
            MoveToCapture(HolderCapturePostitionLoad);
            var holderLoadOffset = GetVisionResult(HolderCapturePostitionLoad);
            var loadPose = Helper.ConvertAxisOffsetToPose(holderLoadOffset);
            loadPose.Z = VHolderHeight;
            MoveToTarget(loadPose, ProcedureId.Load);
            VacuumSucker(HolderId.V, VacuumState.On, VacuumArea.Circle);
            VacuumSucker(VacuumState.Off, ProcedureId.Load);
            MoveZToSafeHeight();

            //bottom camera for unload
            MoveToCapture(BottomCapturePostitionUnload);
            GetVisionResult(BottomCapturePostitionUnload); 
            
            MoveToCapture(unloadPart.CapturePos);
            UnloadTray.CurrentPart = UnloadTray.GetNextPart(unloadPart);
            var unloadTarget = GetVisionResult(unloadPart.CapturePos);
            var unloadTrayPose = Helper.ConvertAxisOffsetToPose(unloadTarget);
            unloadTrayPose.Z = VPlaceHeight;
            MoveToTarget(unloadTrayPose, ProcedureId.Unload);
            VacuumSucker(VacuumState.Off, ProcedureId.Unload);
            MoveZToSafeHeight();

            LoadTray.CurrentPart = LoadTray.GetNextPart(loadPart);
            // Load a new V
            MoveToCapture(LoadTray.CurrentPart.CapturePos);
            var newLoadOffset = GetVisionResult(LoadTray.CurrentPart.CapturePos);
            var newLoadTarget = Helper.ConvertAxisOffsetToPose(newLoadOffset);
            newLoadTarget.Z = VPickHeight;
            MoveToTarget(newLoadTarget, ProcedureId.Load);
            VacuumSucker(VacuumState.On, ProcedureId.Load);
            MoveZToSafeHeight();

            MoveToCapture(BottomCapturePostitionLoad);
            GetVisionResult(BottomCapturePostitionLoad);

            MoveToCapture(HolderCapturePostitionUnload); 
        }

        private void MoveToCapture(CapturePosition capturePosition)
        {
            MoveToTarget(capturePosition, ProcedureId.Load, true);
        }

        public void VLoadReadyOnce()
        {

        }      

        public Task<WaitBlock> LWork()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(100);
                    LLoad(LoadTray.CurrentPart);
                    return new WaitBlock() { Code = 0, Message = "L OK" };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = 40001, Message = "L station error: " + ex.Message };
                }
            });
        }

        public Task<WaitBlock> VWork()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(100);
                    VUnloadAndLoad(UnloadTray.CurrentPart, LoadTray.CurrentPart);
                    return new WaitBlock() { Code = 0, Message = "V OK" };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = 40001, Message = "V station error: " + ex.Message };
                }
            });
        }

        private void VacuumSucker(HolderId holderId, VacuumState state, VacuumArea area)
        {
            if (VacuumSimulateMode)
            {
                return;
            }
            _table.VacuumSucker(holderId, state, area);
        }

        public AssemblyRobot(MotionController controller, 
            StationId id, VisionServer vision, RoundTable table, CapturePosition[] positions)
        {
            _mc = controller;
            StationId = id;
            _vision = vision;
            _table = table;
            _capturePositions = positions;
        }

        public void Setup()
        {
            switch (StationId)
            {
                case StationId.V:
                    MotorX = _mc.MotorVX;
                    MotorY = _mc.MotorVY;
                    MotorZ = _mc.MotorVZ;
                    MotorRotateLoad = _mc.MotorVRotateLoad;
                    MotorRotateUnload = _mc.MotorVRotateUnload;

                    BaseTrayCapturePostitionLoad = FindCapturePosition(CaptureId.VTrayPickTop);
                    BottomCapturePostitionLoad = FindCapturePosition(CaptureId.VLoadCompensationBottom);
                    HolderCapturePostitionLoad = FindCapturePosition(CaptureId.VLoadHolderTop);

                    BaseTrayCapturePostitionUnload = FindCapturePosition(CaptureId.VTrayPlaceTop);
                    BottomCapturePostitionUnload = FindCapturePosition(CaptureId.VUnloadCompensationBottom);
                    HolderCapturePostitionUnload = FindCapturePosition(CaptureId.VUnloadHolderTop);

                    LoadTray = new Tray()
                    {
                        RowCount = 5,
                        ColumneCount = 5,
                        XOffset = 18.5,
                        YOffset = 18.5,
                        BaseCapturePosition = BaseTrayCapturePostitionLoad,
                        CurrentPart = new Part() { CapturePos = BaseTrayCapturePostitionLoad },
                    };

                    UnloadTray = new Tray()
                    {
                        RowCount = 5,
                        ColumneCount = 5,
                        XOffset = 18.5,
                        YOffset = 18.5,
                        BaseCapturePosition = BaseTrayCapturePostitionUnload,
                        CurrentPart = new Part() { CapturePos = BaseTrayCapturePostitionUnload },
                    };
                    break;

                case StationId.L:
                    MotorX = _mc.MotorLX;
                    MotorY = _mc.MotorLY;
                    MotorZ = _mc.MotorLZ;
                    MotorRotateLoad = _mc.MotorLRotateLoad;

                    BaseTrayCapturePostitionLoad = FindCapturePosition(CaptureId.LTrayPickTop);
                    BottomCapturePostitionLoad = FindCapturePosition(CaptureId.LLoadCompensationBottom);
                    HolderCapturePostitionLoad = FindCapturePosition(CaptureId.LLoadHolderTop);

                    LoadTray = new Tray()
                    {
                        RowCount = 5,
                        ColumneCount = 9,
                        XOffset = 16.0,
                        YOffset=10.0,
                        BaseCapturePosition = BaseTrayCapturePostitionLoad,
                        CurrentPart = new Part() { CapturePos = BaseTrayCapturePostitionLoad },
                    };                  
                    break;

                case StationId.GLine:
                    break;
                case StationId.GPoint:
                    break;
                default:
                    break;
            }
        }

        private CapturePosition FindCapturePosition(CaptureId id)
        {
            return Helper.FindCapturePosition(_capturePositions, id);
        }

        public void CylinderHead(HeadCylinderState state, ProcedureId procedure, bool justToCapture=false)
        {
            if (StationId== StationId.L || justToCapture)
            {
                return;
            }
            switch (procedure)
            {
                case ProcedureId.Load:
                    _mc.VLoadHeadCylinder(state);
                    break;
                case ProcedureId.Unload:
                    _mc.VUnloadHeadCylinder(state);
                    break;
                default:
                    break;
            }          
        }

        public void UnlockTray(ProcedureId procedure = ProcedureId.Load)
        {
            switch (StationId)
            {
                case StationId.V:
                    switch (procedure)
                    {
                        case ProcedureId.Load:
                            _mc.VLoadConveyorLocker(LockState.Off);
                            break;
                        case ProcedureId.Unload:
                            _mc.VUnloadConveyorLocker(LockState.Off);
                            break;
                        default:
                            break;
                    }
                    break;

                case StationId.L:
                    _mc.LLoadConveyorLocker(LockState.Off);
                    break;
                case StationId.GLine:
                    break;
                case StationId.GPoint:
                    break;
                default:
                    break;
            }
        }

        public void VacuumSucker(VacuumState state, ProcedureId procedure)
        {
            if (VacuumSimulateMode)
            {
                return;
            }

            switch (StationId)
            {
                case StationId.V:
                    switch (procedure)
                    {
                        case ProcedureId.Load:
                            switch (state)
                            {
                                case VacuumState.On:
                                    _mc.VLoadVacuum(VacuumState.On);
                                    break;
                                case VacuumState.Off:
                                    _mc.VLoadVacuum(VacuumState.Off);
                                    break;
                                default:
                                    break;
                            }
                            break;

                        case ProcedureId.Unload:
                            switch (state)
                            {
                                case VacuumState.On:
                                    _mc.VUnloadVacuum(VacuumState.On);
                                    break;
                                case VacuumState.Off:
                                    _mc.VUnloadVacuum(VacuumState.Off);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }

                   
                    break;
                case StationId.L:
                    switch (state)
                    {
                        case VacuumState.On:
                            _mc.LLoadVacuum(VacuumState.On);
                            break;
                        case VacuumState.Off:
                            _mc.LLoadVacuum(VacuumState.Off);
                            break;
                        default:
                            break;
                    }
                    break;
                case StationId.GLine:
                    break;
                case StationId.GPoint:
                    break;
                default:
                    break;
            }
        }

        public void LockTray(ProcedureId procedure = ProcedureId.Load)
        {
            switch (StationId)
            {
                case StationId.V:
                    switch (procedure)
                    {
                        case ProcedureId.Load:
                            _mc.VLoadConveyorLocker(LockState.On);
                            break;
                        case ProcedureId.Unload:
                            _mc.VUnloadConveyorLocker(LockState.On);
                            break;
                        default:
                            break;
                    }
                    break;

                case StationId.L:
                    _mc.LLoadConveyorLocker(LockState.On);
                    break;
                case StationId.GLine:
                    break;
                case StationId.GPoint:
                    break;
                default:
                    break;
            }
        }

        public void SetSpeed(double speed)
        {
            MotorX.Velocity = speed;
            MotorY.Velocity = speed;
            MotorZ.Velocity = speed;
            MotorRotateLoad.Velocity = speed;
            if (StationId== StationId.V)
            {
                MotorRotateUnload.Velocity = speed;
            }
        }

        private void Delay(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }

        public void CheckVacuum(Sucker sucker = Sucker.Load, bool vacuumState = true, int timeoutMs = 1000)
        {
            
        }

        public void MoveToTarget(Pose target, ProcedureId procedure, bool justToCapture = false)
        {
            switch (StationId)
            {
                case StationId.V:
                    VMoveToTarget(target, procedure, justToCapture);
                    break;
                case StationId.L:
                    LMoveToTarget(target);
                    break;
                //case StationId.GLine:
                //    break;
                //case StationId.GPoint:
                //    break;

                default:
                    throw new NotImplementedException("Can not move 4981684984164889.");
            }
        }
   
        public void MoveToTarget(CapturePosition target, ProcedureId procedure, bool justToCapture=false)
        {
            var tar = Helper.ConvertCapturePositionToPose(target);
            MoveToTarget(tar, procedure, justToCapture);
        }

        private void LMoveToTarget(Pose target)
        {
            CheckSafety();

            if (IsLMoveToSameArea(target))
            {
                _mc.MoveToTarget(MotorY, target.Y);
                _mc.MoveToTarget(MotorX, target.X);
                _mc.MoveToTargetRelative(MotorRotateLoad, target.RLoadAngle);
                _mc.WaitTillEnd(MotorX);
                _mc.WaitTillEnd(MotorY);
                _mc.WaitTillEnd(MotorRotateLoad);

                _mc.MoveToTargetTillEnd(MotorZ, target.Z);
            }
            else
            {
                //Move from conveyor to table.
                if (GetPosition(MotorY) < SafeYAreaLSmaller &&
                   target.Y > SafeYAreaLSmaller)
                {
                    _mc.MoveToTargetRelative(MotorRotateLoad, target.RLoadAngle);
                    _mc.MoveToTargetTillEnd(MotorX, target.X);
                    _mc.MoveToTargetTillEnd(MotorY, target.Y);
                    _mc.WaitTillEnd(MotorRotateLoad);

                    _mc.MoveToTargetTillEnd(MotorZ, target.Z);
                }
                else
                {
                    //Move from table to conveyor.
                    if (GetPosition(MotorX) < SafeXAreaSmaller &&
                        target.X > SafeXAreaSmaller)
                    {
                        _mc.MoveToTargetRelative(MotorRotateLoad, target.RLoadAngle);
                        _mc.MoveToTargetTillEnd(MotorY, target.Y);
                        _mc.MoveToTargetTillEnd(MotorX, target.X);
                        _mc.WaitTillEnd(MotorRotateLoad);

                        _mc.MoveToTargetTillEnd(MotorZ, target.Z);
                    }
                    else
                    {
                        throw new Exception("Robot is in unknown position, need to go home first.");
                    }
                }
            }
            
        }

        private void VMoveToTarget(Pose target, ProcedureId procedure, bool justToCapture=false)
        {
            CheckSafety();

            if (IsVMoveToSameArea(target))
            {
                _mc.MoveToTarget(MotorY, target.Y);
                _mc.MoveToTarget(MotorX, target.X);
                _mc.MoveToTargetRelative(MotorRotateLoad, target.RLoadAngle);
                _mc.WaitTillEnd(MotorX);
                _mc.WaitTillEnd(MotorY);
                _mc.WaitTillEnd(MotorRotateLoad);

                CylinderHead(HeadCylinderState.Down, procedure,justToCapture);
                _mc.MoveToTargetTillEnd(MotorZ, target.Z);
            }
            else
            {
                //Move from conveyor to table.
                if (GetPosition(MotorY) > SafeYAreaVBigger &&
                   target.Y < SafeYAreaVBigger)
                {
                    _mc.MoveToTargetRelative(MotorRotateLoad, target.RLoadAngle);
                    _mc.MoveToTargetTillEnd(MotorX, target.X);
                    _mc.MoveToTargetTillEnd(MotorY, target.Y);
                    _mc.WaitTillEnd(MotorRotateLoad);
                    CylinderHead(HeadCylinderState.Down, procedure, justToCapture);
                    _mc.MoveToTargetTillEnd(MotorZ, target.Z);
                }
                else
                {
                    //Move from table to conveyor.
                    if (GetPosition(MotorX) < SafeXAreaSmaller &&
                        target.X > SafeXAreaSmaller)
                    {
                        _mc.MoveToTargetRelative(MotorRotateLoad, target.RLoadAngle);
                        _mc.MoveToTargetTillEnd(MotorY, target.Y);
                        _mc.MoveToTargetTillEnd(MotorX, target.X);
                        _mc.WaitTillEnd(MotorRotateLoad);
                        CylinderHead(HeadCylinderState.Down, procedure, justToCapture);
                        _mc.MoveToTargetTillEnd(MotorZ, target.Z);
                    }
                    else
                    {
                        throw new Exception("Robot is in unknown position, need to go home first.");
                    }
                }
            }
        }

        private double GetPosition(Motor motor)
        {
            return _mc.GetPosition(motor);
        }

        private bool IsLMoveToSameArea(Pose target)
        {
            return (GetPosition(MotorY) <= SafeYAreaLSmaller && target.Y <= SafeYAreaLSmaller) ||
                     (GetPosition(MotorX) <= SafeXAreaSmaller && target.X <= SafeXAreaSmaller);
        }

        private bool IsVMoveToSameArea(Pose target)
        {
            return (GetPosition(MotorY) >= SafeYAreaVBigger && target.Y >= SafeYAreaVBigger) ||
                       (GetPosition(MotorX) <= SafeXAreaSmaller && target.X <= SafeXAreaSmaller);
        }

        private void MoveZToSafeHeight()
        {
            if (StationId == StationId.V)
            {
                if (GetPosition(MotorZ) < VSafeHeight)
                {
                    _mc.MoveToTargetTillEnd(MotorZ, VSafeHeight);
                }

                CylinderHead(HeadCylinderState.Up, ProcedureId.Load);
                CylinderHead(HeadCylinderState.Up, ProcedureId.Unload);
                _mc.MoveToTargetTillEnd(MotorZ, VSafeHeight);

            }
            else
            {
                if (GetPosition(MotorZ) < LSafeHeight)
                {
                    _mc.MoveToTargetTillEnd(MotorZ, LSafeHeight);
                }
            }
        }

        private void CheckSafety()
        {
            MoveZToSafeHeight();
        }
    }

    public enum Sucker
    {
        Load,
        Unload,
    }
}
