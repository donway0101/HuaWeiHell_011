using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bp.Mes;

namespace Sorter
{
    public class LStation : IRobot
    {
        private readonly MotionController _mc;
        private readonly VisionServer _vision;
        private readonly RoundTable _table;

        public Motor MotorA { get; set; }
        public Motor MotorX { get; set; }
        public Motor MotorY { get; set; }
        public Motor MotorZ { get; set; }

        public double SafeXArea { get; set; } = 100;
        public double SafeYArea { get; set; } = 100;
        public double SafeZHeight { get; set; } = -30;

        public Tray UnloadTray { get; set; }
        public Tray LoadTray { get; set; }
        public double LoadTrayHeight { get; set; } = -50;
        public double UnloadTrayHeight { get; set; }
        public double FixtureHeight { get; set; } = -40;

        public bool VacuumSimulateMode { get; set; } = true;
        public bool VisionSimulateMode { get; set; } = true;

        private CapturePosition[] _capturePositions;
        private CapturePosition[] _capturePositionOffsets;

        public LStation(MotionController controller, VisionServer vision, 
            RoundTable table, CapturePosition[] positions, CapturePosition[] offsets)
        {
            _mc = controller;
            _vision = vision;
            _table = table;
            _capturePositions = positions;
            _capturePositionOffsets = offsets;
        }

        public CapturePosition GetCapturePosition(CaptureId id)
        {
            return Helper.FindCapturePosition(_capturePositions, id);
        }

        public CapturePosition GetCapturePositionOffset(CaptureId id)
        {
            return new CapturePosition();
            //Todo add offset 
            return Helper.FindCapturePosition(_capturePositionOffsets, id);
        }

        private AxisOffset GetRawVisionResult(CapturePosition capturePosition)
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

        private double GetZHeight(CaptureId id)
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
                    throw new NotImplementedException("No such Z height:" + id);
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

        public void Load(Part part)
        {
            MoveToCapture(part.CapturePos);
            var pickPose = GetVisionResult(part.CapturePos);
            MoveToTarget(pickPose);
            Sucker(VacuumState.On);
            MoveToSafeHeight();

            var bottomCamCapturePos = GetCapturePosition(CaptureId.LLoadCompensationBottom);
            MoveToCapture(bottomCamCapturePos);
            GetVisionResult(bottomCamCapturePos);

            var fixtureCapturePos = GetCapturePosition(CaptureId.LLoadHolderTop);
            MoveToCapture(fixtureCapturePos);
            var placePose = GetVisionResult(fixtureCapturePos);
            MoveToTarget(placePose);

            Sucker(FixtureId.L, VacuumState.On, VacuumArea.Center);
            Sucker(VacuumState.Off);

            LoadTray.CurrentPart = LoadTray.GetNextPart(part);
            MoveToCapture(LoadTray.CurrentPart.CapturePos);
        }

        public void MoveToSafeHeight()
        {
            if (GetPosition(MotorZ) < SafeZHeight)
            {
                _mc.MoveToTargetTillEnd(MotorZ, SafeZHeight);
            }
        }

        public void MoveToCapture(CapturePosition target)
        {
            var tar = Helper.ConvertCapturePositionToPose(target);
            MoveTo(tar);
        }

        private void MoveTo(Pose target)
        {
            MoveToSafeHeight();

            if (IsLMoveToSameArea(target))
            {
                _mc.MoveToTarget(MotorY, target.Y);
                _mc.MoveToTarget(MotorX, target.X);
                _mc.MoveToTargetRelative(MotorA, target.A);
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
                    _mc.MoveToTargetRelative(MotorA, target.A);
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
                        _mc.MoveToTargetRelative(MotorA, target.A);
                        _mc.MoveToTargetTillEnd(MotorY, target.Y);
                        _mc.MoveToTargetTillEnd(MotorX, target.X);
                        _mc.WaitTillEnd(MotorA);

                        _mc.MoveToTargetTillEnd(MotorZ, target.Z);
                    }
                    else
                    {
                        throw new Exception("Robot is in unknown position, need to go home first.");
                    }
                }
            }
        }

        private bool IsLMoveToSameArea(Pose target)
        {
            return (GetPosition(MotorY) <= SafeYArea && target.Y <= SafeYArea) ||
                     (GetPosition(MotorX) <= SafeXArea && target.X <= SafeXArea);
        }

        public void MoveToTarget(CapturePosition target)
        {
            var tar = Helper.ConvertCapturePositionToPose(target);
            MoveTo(tar);
        }

        public void MoveToTarget(Pose target)
        {
            MoveTo(target);
        }

        public void SetSpeed(double speed)
        {
            throw new NotImplementedException();
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
                CurrentPart = new Part() { CapturePos = GetCapturePosition(CaptureId.LTrayPickTop), },
                BaseCapturePosition = GetCapturePosition(CaptureId.LTrayPickTop),
            };
        }

        public void Unload(Part part)
        {
            throw new NotImplementedException();
        }

        public void UnloadAndLoad(Part part)
        {
            throw new NotImplementedException();
        }

        public void Sucker(VacuumState state)
        {
            if (VacuumSimulateMode)
                return;
            _mc.LLoadVacuum(state);
        }

        public void Sucker(FixtureId id, VacuumState state, VacuumArea area)
        {
            if (VacuumSimulateMode)
                return;
            _table.VacuumSucker(id, state, area);
        }

        public Task<WaitBlock> Work()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1);
                    Load(LoadTray.CurrentPart);
                    return new WaitBlock() { Code = 0, Message = "V OK" };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = 40001, Message = "V station error: " + ex.Message };
                }
            });
        }

        public double GetPosition(Motor motor)
        {
            return _mc.GetPosition(motor);
        }
    }
}
