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

        public Motor MotorAngle { get; set; }
        public Motor MotorX { get; set; }
        public Motor MotorY { get; set; }
        public Motor MotorZ { get; set; }
        public double SafeXArea { get; set; }
        public double SafeYArea { get; set; }
        public Tray UnloadTray { get; set; }
        public Tray LoadTray { get; set; }
        public double LoadTrayHeight { get; set; }
        public double UnloadTrayHeight { get; set; }
        public double FixtureHeight { get; set; }
        public bool VacuumSimulateMode { get; set; }
        public bool VisionSimulateMode { get; set; }

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
            MoveToTarget(part.CapturePos);
            var pickPose = GetVisionResult(part.CapturePos);
            MoveToTarget(pickPose);
            VacuumSucker(VacuumState.On);
            //MoveZToSafeHeight();

            //MoveToTarget(BottomCapturePostitionLoad, ProcedureId.Load);
            //GetVisionResult(BottomCapturePostitionLoad);

            //MoveToTarget(HolderCapturePostitionLoad, ProcedureId.Load);
            //var holderOffset = GetVisionResult(HolderCapturePostitionLoad);

            //var holderPose = Helper.ConvertAxisOffsetToPose(holderOffset);
            //holderPose.Z = LHolderHeight;

            //MoveToTarget(holderPose, ProcedureId.Load);

            //VacuumSucker(FixtureId.L, VacuumState.On, VacuumArea.Center);
            //VacuumSucker(VacuumState.Off, ProcedureId.Load);

            //LoadTray.CurrentPart = LoadTray.GetNextPart(part);
            //MoveToTarget(LoadTray.CurrentPart.CapturePos, ProcedureId.Load);
        }

        public void MoveToCapture(CapturePosition target)
        {
            throw new NotImplementedException();
        }

        public void MoveToTarget(CapturePosition target)
        {
            throw new NotImplementedException();
        }

        public void MoveToTarget(Pose target)
        {
            throw new NotImplementedException();
        }

        public void SetSpeed(double speed)
        {
            throw new NotImplementedException();
        }

        public void Setup()
        {
            throw new NotImplementedException();
        }

        public void Unload(Part part)
        {
            throw new NotImplementedException();
        }

        public void UnloadAndLoad(Part part)
        {
            throw new NotImplementedException();
        }

        public void VacuumSucker(VacuumState state)
        {
            throw new NotImplementedException();
        }

        public Task<WaitBlock> Work()
        {
            throw new NotImplementedException();
        }
    }
}
