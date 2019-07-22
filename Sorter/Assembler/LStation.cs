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

        public Tray LoadTray { get; set; }
        public double LoadTrayHeight { get; set; } = -63.146;
        public double FixtureHeight { get; set; } = -56.683;

        public bool CheckVacuumValue { get; set; } = false;
        public bool VisionSimulateMode { get; set; } = false;
        public Tray UnloadTray { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public double UnloadTrayHeight { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private CapturePosition[] _capturePositions;
        private CapturePosition[] _capturePositionOffsets;


        /// <summary>
        /// Preparation work for next cycle.
        /// </summary>
        public Task<WaitBlock> Preparation { get; set; }

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

        public Task<WaitBlock> MoveToNextCaptureAsync(Part part)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1);
                    MoveToCapture(part.CapturePos);
                    return new WaitBlock() { Message = "V prepare OK" };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "V prepare error: " + ex.Message };
                }
            });
        }

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

        public Task<WaitBlock> LoadAsync(Part part)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1);
                    Load(part);
                    return new WaitBlock() { Message = "L Load OK" };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "L Load error: " + ex.Message };
                }
            });
        }

        private void PickPart(Part part)
        {
            MoveToCapture(part.CapturePos);
            var pickPose = GetVisionResult(part.CapturePos);
            MoveToTarget(pickPose);
            Sucker(VacuumState.On);
            MoveToSafeHeight();
        }

        private void PlacePart(Part part)
        {
            var fixtureCapturePos = GetCapturePosition(CaptureId.LLoadHolderTop);
            MoveToCapture(fixtureCapturePos);
            var placePose = GetVisionResult(fixtureCapturePos);
            //Add bottom camera compensation.
            placePose.X += part.XOffset;
            placePose.Y += part.YOffset;
            MoveToTarget(placePose);
            Sucker(FixtureId.L, VacuumState.On, VacuumArea.Center);
            Sucker(VacuumState.Off);
            MoveToSafeHeight();
        }

        public async void Load(Part part)
        {
            //Todo deal with preparation.
            //await Preparation;
            //Helper.CheckTaskResult(Preparation);
            await Task.Delay(1);

            PickPart(part);
            CorrectAngle(ref part, ActionType.Load);
            PlacePart(part);

            SetNextPartLoad();
            Preparation = MoveToNextCaptureAsync(LoadTray.CurrentPart);
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
            MoveTo(tar, MoveMode.Abs);
        }

        private void MoveTo(Pose target, MoveMode mode = MoveMode.Relative)
        {
            MoveToSafeHeight();

            if (IsMoveToSameArea(target))
            {
                _mc.MoveToTarget(MotorY, target.Y);
                _mc.MoveToTarget(MotorX, target.X);
                MoveRotaryMotor(target.A, mode, ActionType.Load);
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
                    MoveRotaryMotor(target.A, mode, ActionType.Load);
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
                        MoveRotaryMotor(target.A, mode, ActionType.Load);
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

        private bool IsMoveToSameArea(Pose target)
        {
            return (GetPosition(MotorY) <= SafeYArea && target.Y <= SafeYArea) ||
                     (GetPosition(MotorX) <= SafeXArea && target.X <= SafeXArea);
        }

        public void MoveToTarget(CapturePosition target)
        {
            var tar = Helper.ConvertCapturePositionToPose(target);
            MoveTo(tar, MoveMode.Relative);
        }

        public void MoveToTarget(Pose target)
        {
            MoveTo(target, MoveMode.Relative);
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
                CurrentPart = new Part() { CapturePos = GetCapturePosition(CaptureId.LTrayPickTop), },
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

            Preparation = EmptyTask();
        }

        private Task<WaitBlock> EmptyTask()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1);
                    return new WaitBlock() { Code = ErrorCode.Sucessful };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = ex.Message };
                }
            });
        }

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

        public double GetPosition(Motor motor)
        {
            return _mc.GetPosition(motor);
        }

        AxisOffset IRobot.GetRawVisionResult(CapturePosition capturePosition)
        {
            throw new NotImplementedException();
        }

        public void UnloadAndLoad(Part unload, Part load)
        {
            throw new NotImplementedException();
        }

        public void Sucker(VacuumState state, int retryTimes, ActionType action)
        {
            throw new NotImplementedException();
        }

        public void CorrectAngle(double relativeAngle, ActionType action)
        {
            _mc.MoveToTargetRelativeTillEnd(MotorA, -relativeAngle);
        }

        public void CorrectAngle(ref Part part, ActionType action)
        {
            //Get angle offset for load.
            var bottomCamCapturePosLoad = GetCapturePosition(CaptureId.LLoadCompensationBottom);
            MoveToCapture(bottomCamCapturePosLoad);
            var angleOffset = GetVisionResult(bottomCamCapturePosLoad, 3);
            CorrectAngle(angleOffset.A, ActionType.Unload);
            var xNyOffset = GetVisionResult(bottomCamCapturePosLoad, 3);
            part.XOffset = xNyOffset.X;
            part.YOffset = xNyOffset.Y;
        }

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
            } while (retryCount<3);

            NgBin(LoadTray.CurrentPart);

            throw new Exception("Vision fail three times.");
        }

        AxisOffset IRobot.GetVisionResult(CapturePosition capturePosition, int retryTimes)
        {
            throw new NotImplementedException();
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

        public void MoveRotaryMotor(double angle, MoveMode mode, ActionType type)
        {
            switch (mode)
            {
                case MoveMode.None:
                    break;
                case MoveMode.Abs:
                    _mc.MoveToTarget(MotorZ, angle);
                    break;
                case MoveMode.Relative:
                    _mc.MoveToTargetRelative(MotorZ, angle);
                    break;
                default:
                    break;
            }
        }
    }
}
