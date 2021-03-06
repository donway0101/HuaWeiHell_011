﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTN;

namespace Sorter
{
    /// <summary>
    /// Encoder error?
    /// Home search repeatability
    /// Driver fault
    /// 
    /// </summary>
    public partial class MotionController : IMotionController
    {
        private static readonly log4net.ILog log =
          log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly object _motionLocker = new object();

        #region Motor
        public Motor MotorVX = new Motor();
        public Motor MotorVY = new Motor();
        public Motor MotorVZ = new Motor();
        public Motor MotorVRotateLoad = new Motor();
        public Motor MotorVRotateUnload = new Motor();
        public Motor MotorVTrayLoad = new Motor();
        public Motor MotorVTrayUnload = new Motor();
        public Motor MotorGluePointX = new Motor();
        public Motor MotorGluePointY = new Motor();
        public Motor MotorGluePointZ = new Motor();
        public Motor MotorGlueLineX = new Motor();
        public Motor MotorGlueLineY = new Motor();
        public Motor MotorGlueLineZ = new Motor();
        public Motor MotorLX = new Motor();
        public Motor MotorLY = new Motor();
        public Motor MotorLZ = new Motor();
        public Motor MotorLRotateLoad = new Motor();
        public Motor MotorLTrayLoad = new Motor();
        public Motor MotorLTrayUnload = new Motor();
        public Motor MotorWorkTable = new Motor();

        public Motor MotorVConveyorUnload = new Motor();
        public Motor MotorVConveyorLoad = new Motor();
        public Motor MotorLConveyorLoad = new Motor();
        //public Motor MotorLConveyorUnload = new Motor();       


        public List<Motor> Motors { get; set; } = new List<Motor>();
        #endregion

        public MotionController()
        {

        }

        public void Connect()
        {
            Run(mc.GTN_Open(5, 1), "Controller Connect fail");           
        }      

        #region Setup
        public void Setup()
        {
            ControllerSetup();
            AxisSetup();
        }

        public void AxisSetup(double homeSpeed = 5.0)
        {
            double allowedError = 0.6;
            double defaultFindLimitSpeed = homeSpeed;
            double defaultFindHomeSpeed = homeSpeed;
            double defaultHomeOffset = 0;
            double defaultSpeed = 10.0;
            double defaultAcceleration = 5;

            //Todo region.
            #region LY
            MotorLY.Id = Axis.LY;
            MotorLY.EncCtsPerRound = 1000.0;
            MotorLY.BallScrewLead = 1.0;
            MotorLY.EncoderFactor =
            MotorLY.EncCtsPerRound /
            MotorLY.BallScrewLead;
            MotorLY.Acceleration = defaultAcceleration;
            MotorLY.Deceleration = defaultAcceleration;
            MotorLY.Velocity = defaultSpeed;
            MotorLY.HomeSearchMode = mc.HOME_MODE_LIMIT_HOME;
            MotorLY.HomeSearchDirection = MoveDirection.Negative;
            MotorLY.HomeSearchIndexDirection = MoveDirection.Negative;
            MotorLY.EdgeCaptureMode = EdgeCapture.Falling;
            MotorLY.HomeOffset = defaultHomeOffset;
            MotorLY.Direction = 1.0;
            MotorLY.CriticalErrIdle = allowedError;
            MotorLY.HomeLimitSpeed = defaultFindLimitSpeed;
            MotorLY.HomeIndexSpeed = defaultFindHomeSpeed;
            #endregion

            #region LX
            MotorLX.Id = Axis.LX;
            MotorLX.EncCtsPerRound = 1000.0;
            MotorLX.BallScrewLead = 1.0;
            MotorLX.EncoderFactor =
            MotorLX.EncCtsPerRound /
            MotorLX.BallScrewLead;
            MotorLX.Acceleration = defaultAcceleration;
            MotorLX.Deceleration = defaultAcceleration;
            MotorLX.Velocity = defaultSpeed;
            MotorLX.HomeSearchMode = mc.HOME_MODE_LIMIT_HOME;
            MotorLX.HomeSearchDirection = MoveDirection.Negative;
            MotorLX.HomeSearchIndexDirection = MoveDirection.Negative;
            MotorLX.EdgeCaptureMode = EdgeCapture.Falling;
            MotorLX.HomeOffset = defaultHomeOffset;
            MotorLX.Direction = 1.0;
            MotorLX.CriticalErrIdle = allowedError;
            MotorLX.HomeLimitSpeed= defaultFindLimitSpeed;
            MotorLX.HomeIndexSpeed = defaultFindHomeSpeed;
            #endregion

            #region LZ
            MotorLZ.Id = Axis.LZ;
            MotorLZ.EncCtsPerRound = 10000.0;
            MotorLZ.BallScrewLead = 10.0;
            MotorLZ.EncoderFactor =
            MotorLZ.EncCtsPerRound /
            MotorLZ.BallScrewLead;
            MotorLZ.Acceleration = defaultAcceleration;
            MotorLZ.Deceleration = defaultAcceleration;
            MotorLZ.Velocity = defaultSpeed;
            MotorLZ.HomeSearchMode = mc.HOME_MODE_LIMIT_HOME;
            MotorLZ.HomeSearchDirection = MoveDirection.Positive;
            MotorLZ.HomeSearchIndexDirection = MoveDirection.Positive;
            MotorLZ.EdgeCaptureMode = EdgeCapture.Falling;
            MotorLZ.HomeOffset = defaultHomeOffset;
            MotorLZ.Direction = -1.0;
            MotorLZ.CriticalErrIdle = allowedError;
            MotorLZ.HomeLimitSpeed= defaultFindLimitSpeed;
            MotorLZ.HomeIndexSpeed = defaultFindHomeSpeed;
            #endregion

            #region LTrayLoad
            MotorLTrayLoad.Id = Axis.LTrayLoad;
            MotorLTrayLoad.EncCtsPerRound = 10000.0;
            MotorLTrayLoad.BallScrewLead = 10.0;
            MotorLTrayLoad.EncoderFactor =
            MotorLTrayLoad.EncCtsPerRound /
            MotorLTrayLoad.BallScrewLead;
            MotorLTrayLoad.Acceleration = defaultAcceleration;
            MotorLTrayLoad.Deceleration = defaultAcceleration;
            MotorLTrayLoad.Velocity = defaultSpeed;
            MotorLTrayLoad.HomeSearchMode = mc.HOME_MODE_LIMIT_HOME;
            MotorLTrayLoad.HomeSearchDirection = MoveDirection.Negative;
            MotorLTrayLoad.HomeSearchIndexDirection = MoveDirection.Negative;
            MotorLTrayLoad.EdgeCaptureMode = EdgeCapture.Falling;
            MotorLTrayLoad.HomeOffset = -3.0;
            MotorLTrayLoad.Direction = 1.0;
            MotorLTrayLoad.CriticalErrIdle = allowedError;
            MotorLTrayLoad.HomeLimitSpeed= defaultFindLimitSpeed;
            MotorLTrayLoad.HomeIndexSpeed = defaultFindHomeSpeed;
            #endregion

            #region LTrayUnload
            MotorLTrayUnload.Id = Axis.LTrayUnload;
            MotorLTrayUnload.EncCtsPerRound = 10000.0;
            MotorLTrayUnload.BallScrewLead = 10.0;
            MotorLTrayUnload.EncoderFactor =
            MotorLTrayUnload.EncCtsPerRound /
            MotorLTrayUnload.BallScrewLead;
            MotorLTrayUnload.Acceleration = defaultAcceleration;
            MotorLTrayUnload.Deceleration = defaultAcceleration;
            MotorLTrayUnload.Velocity = defaultSpeed;
            MotorLTrayUnload.HomeSearchMode = mc.HOME_MODE_LIMIT_HOME;
            MotorLTrayUnload.HomeSearchDirection = MoveDirection.Negative;
            MotorLTrayUnload.HomeSearchIndexDirection = MoveDirection.Negative;
            MotorLTrayUnload.EdgeCaptureMode = EdgeCapture.Falling;
            MotorLTrayUnload.HomeOffset = -3.0;
            MotorLTrayUnload.Direction = 1.0;
            MotorLTrayUnload.CriticalErrIdle = allowedError;
            MotorLTrayUnload.HomeLimitSpeed = defaultFindLimitSpeed;
            MotorLTrayUnload.HomeIndexSpeed = defaultFindHomeSpeed;
            #endregion

            #region VY
            MotorVY.Id = Axis.VY;
            MotorVY.EncCtsPerRound = 1000.0;
            MotorVY.BallScrewLead = 1.0;
            MotorVY.EncoderFactor =
            MotorVY.EncCtsPerRound /
            MotorVY.BallScrewLead;
            MotorVY.Acceleration = defaultAcceleration;
            MotorVY.Deceleration = defaultAcceleration;
            MotorVY.Velocity = defaultSpeed;
            MotorVY.HomeSearchMode = mc.HOME_MODE_LIMIT_HOME;
            MotorVY.HomeSearchDirection = MoveDirection.Positive;
            MotorVY.HomeSearchIndexDirection = MoveDirection.Positive;
            MotorVY.EdgeCaptureMode = EdgeCapture.Falling;
            MotorVY.HomeOffset = defaultHomeOffset;
            MotorVY.Direction = -1.0;
            MotorVY.CriticalErrIdle = allowedError;
            MotorVY.HomeLimitSpeed = defaultFindLimitSpeed;
            MotorVY.HomeIndexSpeed = defaultFindHomeSpeed;
            #endregion

            #region VX
            MotorVX.Id = Axis.VX;
            MotorVX.EncCtsPerRound = 1000.0;
            MotorVX.BallScrewLead = 1.0;
            MotorVX.EncoderFactor =
            MotorVX.EncCtsPerRound /
            MotorVX.BallScrewLead;
            MotorVX.Acceleration = defaultAcceleration;
            MotorVX.Deceleration = defaultAcceleration;
            MotorVX.Velocity = defaultSpeed;
            MotorVX.HomeSearchMode = mc.HOME_MODE_LIMIT_HOME;
            MotorVX.HomeSearchDirection = MoveDirection.Negative;
            MotorVX.HomeSearchIndexDirection = MoveDirection.Negative;
            MotorVX.EdgeCaptureMode = EdgeCapture.Falling;
            MotorVX.HomeOffset = defaultHomeOffset;
            MotorVX.Direction = 1.0;
            MotorVX.CriticalErrIdle = allowedError;
            MotorVX.HomeLimitSpeed= defaultFindLimitSpeed;
            MotorVX.HomeIndexSpeed = defaultFindHomeSpeed;
            #endregion

            #region VZ
            MotorVZ.Id = Axis.VZ;
            MotorVZ.EncCtsPerRound = 10000.0;
            MotorVZ.BallScrewLead = 10.0;
            MotorVZ.EncoderFactor =
            MotorVZ.EncCtsPerRound /
            MotorVZ.BallScrewLead;
            MotorVZ.Acceleration = defaultAcceleration;
            MotorVZ.Deceleration = defaultAcceleration;
            MotorVZ.Velocity = defaultSpeed;
            MotorVZ.HomeSearchMode = mc.HOME_MODE_LIMIT_HOME;
            MotorVZ.HomeSearchDirection = MoveDirection.Positive;
            MotorVZ.HomeSearchIndexDirection = MoveDirection.Positive;
            MotorVZ.EdgeCaptureMode = EdgeCapture.Falling;
            MotorVZ.HomeOffset = defaultHomeOffset;
            MotorVZ.Direction = -1.0;
            MotorVZ.CriticalErrIdle = allowedError;
            MotorVZ.HomeLimitSpeed= defaultFindLimitSpeed;
            MotorVZ.HomeIndexSpeed = defaultFindHomeSpeed;
            #endregion

            #region VRL
            MotorVRotateLoad.Id = Axis.VRotateLoad;
            MotorVRotateLoad.EncCtsPerRound = 71.0;
            MotorVRotateLoad.BallScrewLead = 1.0;
            MotorVRotateLoad.EncoderFactor =
            MotorVRotateLoad.EncCtsPerRound /
            MotorVRotateLoad.BallScrewLead;
            MotorVRotateLoad.Acceleration = defaultAcceleration;
            MotorVRotateLoad.Deceleration = defaultAcceleration;
            MotorVRotateLoad.Velocity = 45.0;
            MotorVRotateLoad.HomeSearchMode = mc.HOME_MODE_HOME;
            MotorVRotateLoad.HomeSearchDirection = MoveDirection.Positive;
            MotorVRotateLoad.HomeSearchIndexDirection = MoveDirection.Positive;
            MotorVRotateLoad.EdgeCaptureMode = EdgeCapture.Falling;
            MotorVRotateLoad.HomeOffset = -60.0;
            MotorVRotateLoad.Direction = -1.0;
            MotorVRotateLoad.CriticalErrIdle = allowedError;
            MotorVRotateLoad.HomeLimitSpeed = 60.0;
            MotorVRotateLoad.HomeIndexSpeed = 60.0;
            MotorVRotateLoad.CloseLoop = false;
            #endregion

            #region VRU
            MotorVRotateUnload.Id = Axis.VRotateUnload;
            MotorVRotateUnload.EncCtsPerRound = 71.0;
            MotorVRotateUnload.BallScrewLead = 1.0;
            MotorVRotateUnload.EncoderFactor =
            MotorVRotateUnload.EncCtsPerRound /
            MotorVRotateUnload.BallScrewLead;
            MotorVRotateUnload.Acceleration = defaultAcceleration;
            MotorVRotateUnload.Deceleration = defaultAcceleration;
            MotorVRotateUnload.Velocity = 45.0;
            MotorVRotateUnload.HomeSearchMode = mc.HOME_MODE_HOME;
            MotorVRotateUnload.HomeSearchDirection = MoveDirection.Positive;
            MotorVRotateUnload.HomeSearchIndexDirection = MoveDirection.Positive;
            MotorVRotateUnload.EdgeCaptureMode = EdgeCapture.Falling;
            MotorVRotateUnload.HomeOffset = -70.0;
            MotorVRotateUnload.Direction = -1.0;
            MotorVRotateUnload.CriticalErrIdle = allowedError;
            MotorVRotateUnload.HomeLimitSpeed = defaultFindLimitSpeed;
            MotorVRotateUnload.HomeIndexSpeed = defaultFindHomeSpeed;
            MotorVRotateUnload.CloseLoop = false;
            #endregion

            #region VTrayLoad
            MotorVTrayLoad.Id = Axis.VTrayLoad;
            MotorVTrayLoad.EncCtsPerRound = 10000.0;
            MotorVTrayLoad.BallScrewLead = 10.0;
            MotorVTrayLoad.EncoderFactor =
            MotorVTrayLoad.EncCtsPerRound /
            MotorVTrayLoad.BallScrewLead;
            MotorVTrayLoad.Acceleration = defaultAcceleration;
            MotorVTrayLoad.Deceleration = defaultAcceleration;
            MotorVTrayLoad.Velocity = defaultSpeed;
            MotorVTrayLoad.HomeSearchMode = mc.HOME_MODE_LIMIT_HOME;
            MotorVTrayLoad.HomeSearchDirection = MoveDirection.Negative;
            MotorVTrayLoad.HomeSearchIndexDirection = MoveDirection.Negative;
            MotorVTrayLoad.EdgeCaptureMode = EdgeCapture.Falling;
            MotorVTrayLoad.HomeOffset = -3.0;
            MotorVTrayLoad.Direction = 1.0;
            MotorVTrayLoad.CriticalErrIdle = allowedError;
            MotorVTrayLoad.HomeLimitSpeed= defaultFindLimitSpeed;
            MotorVTrayLoad.HomeIndexSpeed = defaultFindHomeSpeed;
            #endregion

            #region VTrayUnload
            MotorVTrayUnload.Id = Axis.VTrayUnload;
            MotorVTrayUnload.EncCtsPerRound = 10000.0;
            MotorVTrayUnload.BallScrewLead = 10.0;
            MotorVTrayUnload.EncoderFactor =
            MotorVTrayUnload.EncCtsPerRound /
            MotorVTrayUnload.BallScrewLead;
            MotorVTrayUnload.Acceleration = defaultAcceleration;
            MotorVTrayUnload.Deceleration = defaultAcceleration;
            MotorVTrayUnload.Velocity = defaultSpeed;
            MotorVTrayUnload.HomeSearchMode = mc.HOME_MODE_LIMIT_HOME;
            MotorVTrayUnload.HomeSearchDirection = MoveDirection.Negative;
            MotorVTrayUnload.HomeSearchIndexDirection = MoveDirection.Negative;
            MotorVTrayUnload.EdgeCaptureMode = EdgeCapture.Falling;
            MotorVTrayUnload.HomeOffset = -3.0;
            MotorVTrayUnload.Direction = 1.0;
            MotorVTrayUnload.CriticalErrIdle = allowedError;
            MotorVTrayUnload.HomeLimitSpeed= defaultFindLimitSpeed;
            MotorVTrayUnload.HomeIndexSpeed = defaultFindHomeSpeed;
            #endregion

            #region GPY
            MotorGluePointY.Id = Axis.GluePointY;
            MotorGluePointY.EncCtsPerRound = 1000.0;
            MotorGluePointY.BallScrewLead = 1.0;
            MotorGluePointY.EncoderFactor =
            MotorGluePointY.EncCtsPerRound /
            MotorGluePointY.BallScrewLead;
            MotorGluePointY.Acceleration = defaultAcceleration;
            MotorGluePointY.Deceleration = defaultAcceleration;
            MotorGluePointY.Velocity = defaultSpeed;
            MotorGluePointY.HomeSearchMode = mc.HOME_MODE_LIMIT_HOME;
            MotorGluePointY.HomeSearchDirection = MoveDirection.Positive;
            MotorGluePointY.HomeSearchIndexDirection = MoveDirection.Positive;
            MotorGluePointY.EdgeCaptureMode = EdgeCapture.Falling;
            MotorGluePointY.HomeOffset = defaultHomeOffset;
            MotorGluePointY.Direction = -1.0;
            MotorGluePointY.CriticalErrIdle = allowedError;
            MotorGluePointY.HomeLimitSpeed= defaultFindLimitSpeed;
            MotorGluePointY.HomeIndexSpeed = defaultFindHomeSpeed;
            #endregion

            #region GPX
            MotorGluePointX.Id = Axis.GluePointX;
            MotorGluePointX.EncCtsPerRound = 1000.0;
            MotorGluePointX.BallScrewLead = 1.0;
            MotorGluePointX.EncoderFactor =
            MotorGluePointX.EncCtsPerRound /
            MotorGluePointX.BallScrewLead;
            MotorGluePointX.Acceleration = defaultAcceleration;
            MotorGluePointX.Deceleration = defaultAcceleration;
            MotorGluePointX.Velocity = defaultSpeed;
            MotorGluePointX.HomeSearchMode = mc.HOME_MODE_LIMIT_HOME;
            MotorGluePointX.HomeSearchDirection = MoveDirection.Positive;
            MotorGluePointX.HomeSearchIndexDirection = MoveDirection.Positive;
            MotorGluePointX.EdgeCaptureMode = EdgeCapture.Falling;
            MotorGluePointX.HomeOffset = defaultHomeOffset;
            MotorGluePointX.Direction = -1.0;
            MotorGluePointX.CriticalErrIdle = allowedError;
            MotorGluePointX.HomeLimitSpeed= defaultFindLimitSpeed;
            MotorGluePointX.HomeIndexSpeed = defaultFindHomeSpeed;
            #endregion

            #region GPZ
            MotorGluePointZ.Id = Axis.GluePointZ;
            MotorGluePointZ.EncCtsPerRound = 10000.0;
            MotorGluePointZ.BallScrewLead = 10.0;
            MotorGluePointZ.EncoderFactor =
            MotorGluePointZ.EncCtsPerRound /
            MotorGluePointZ.BallScrewLead;
            MotorGluePointZ.Acceleration = defaultAcceleration;
            MotorGluePointZ.Deceleration = defaultAcceleration;
            MotorGluePointZ.Velocity = defaultSpeed;
            MotorGluePointZ.HomeSearchMode = mc.HOME_MODE_LIMIT_HOME;
            MotorGluePointZ.HomeSearchDirection = MoveDirection.Positive;
            MotorGluePointZ.HomeSearchIndexDirection = MoveDirection.Positive;
            MotorGluePointZ.EdgeCaptureMode = EdgeCapture.Falling;
            MotorGluePointZ.HomeOffset = defaultHomeOffset;
            MotorGluePointZ.Direction = -1.0;
            MotorGluePointZ.CriticalErrIdle = allowedError;
            MotorGluePointZ.HomeLimitSpeed= defaultFindLimitSpeed;
            MotorGluePointZ.HomeIndexSpeed = defaultFindHomeSpeed;
            #endregion

            #region GLY
            MotorGlueLineY.Id = Axis.GlueLineY;
            MotorGlueLineY.EncCtsPerRound = 1000.0;
            MotorGlueLineY.BallScrewLead = 1.0;
            MotorGlueLineY.EncoderFactor =
            MotorGlueLineY.EncCtsPerRound /
            MotorGlueLineY.BallScrewLead;
            MotorGlueLineY.Acceleration = defaultAcceleration;
            MotorGlueLineY.Deceleration = defaultAcceleration;
            MotorGlueLineY.Velocity = defaultSpeed;
            MotorGlueLineY.HomeSearchMode = mc.HOME_MODE_LIMIT_HOME;
            MotorGlueLineY.HomeSearchDirection = MoveDirection.Negative;
            MotorGlueLineY.HomeSearchIndexDirection = MoveDirection.Negative;
            MotorGlueLineY.EdgeCaptureMode = EdgeCapture.Falling;
            MotorGlueLineY.HomeOffset = defaultHomeOffset;
            MotorGlueLineY.Direction = 1.0;
            MotorGlueLineY.CriticalErrIdle = allowedError;
            MotorGlueLineY.HomeLimitSpeed= defaultFindLimitSpeed;
            MotorGlueLineY.HomeIndexSpeed = defaultFindHomeSpeed;
            #endregion

            #region GLX
            MotorGlueLineX.Id = Axis.GlueLineX;
            MotorGlueLineX.EncCtsPerRound = 1000.0;
            MotorGlueLineX.BallScrewLead = 1.0;
            MotorGlueLineX.EncoderFactor =
            MotorGlueLineX.EncCtsPerRound /
            MotorGlueLineX.BallScrewLead;
            MotorGlueLineX.Acceleration = defaultAcceleration;
            MotorGlueLineX.Deceleration = defaultAcceleration;
            MotorGlueLineX.Velocity = defaultSpeed;
            MotorGlueLineX.HomeSearchMode = mc.HOME_MODE_LIMIT_HOME;
            MotorGlueLineX.HomeSearchDirection = MoveDirection.Positive;
            MotorGlueLineX.HomeSearchIndexDirection = MoveDirection.Positive;
            MotorGlueLineX.EdgeCaptureMode = EdgeCapture.Falling;
            MotorGlueLineX.HomeOffset = defaultHomeOffset;
            MotorGlueLineX.Direction = -1.0;
            MotorGlueLineX.CriticalErrIdle = allowedError;
            MotorGlueLineX.HomeLimitSpeed= defaultFindLimitSpeed;
            MotorGlueLineX.HomeIndexSpeed = defaultFindHomeSpeed;
            #endregion

            #region GLZ
            MotorGlueLineZ.Id = Axis.GlueLineZ;
            MotorGlueLineZ.EncCtsPerRound = 10000.0;
            MotorGlueLineZ.BallScrewLead = 10.0;
            MotorGlueLineZ.EncoderFactor = 
            MotorGlueLineZ.EncCtsPerRound / 
            MotorGlueLineZ.BallScrewLead;
            MotorGlueLineZ.Acceleration = defaultAcceleration;
            MotorGlueLineZ.Deceleration = defaultAcceleration;
            MotorGlueLineZ.Velocity = defaultSpeed;
            MotorGlueLineZ.HomeSearchMode = mc.HOME_MODE_LIMIT_HOME;
            MotorGlueLineZ.HomeSearchDirection = MoveDirection.Positive;
            MotorGlueLineZ.HomeSearchIndexDirection = MoveDirection.Positive;
            MotorGlueLineZ.EdgeCaptureMode = EdgeCapture.Falling;
            MotorGlueLineZ.HomeOffset = defaultHomeOffset;
            MotorGlueLineZ.Direction = -1.0;
            MotorGlueLineZ.CriticalErrIdle = allowedError;
            MotorGlueLineZ.HomeLimitSpeed= defaultFindLimitSpeed;
            MotorGlueLineZ.HomeIndexSpeed = defaultFindHomeSpeed;
            #endregion

            #region MotorLRotateLoad
            MotorLRotateLoad.Id = Axis.LRotateLoad;
            MotorLRotateLoad.EncCtsPerRound = 71.0;
            MotorLRotateLoad.BallScrewLead = 1.0;
            MotorLRotateLoad.EncoderFactor =
            MotorLRotateLoad.EncCtsPerRound /
            MotorLRotateLoad.BallScrewLead;
            MotorLRotateLoad.Acceleration = 10.0;
            MotorLRotateLoad.Deceleration = 10.0;
            MotorLRotateLoad.Velocity = 45.0;
            MotorLRotateLoad.HomeSearchMode = mc.HOME_MODE_HOME;
            MotorLRotateLoad.HomeSearchDirection = MoveDirection.Positive;
            MotorLRotateLoad.HomeSearchIndexDirection = MoveDirection.Positive;
            MotorLRotateLoad.EdgeCaptureMode = EdgeCapture.Falling;
            MotorLRotateLoad.HomeOffset = 13.577;
            MotorLRotateLoad.Direction = -1.0;
            MotorLRotateLoad.CriticalErrIdle = allowedError;
            MotorLRotateLoad.HomeLimitSpeed = 60.0;
            MotorLRotateLoad.HomeIndexSpeed = defaultFindHomeSpeed;
            MotorLRotateLoad.CloseLoop = false;
            #endregion

            #region Table
            MotorWorkTable.Id = Axis.WorkTable;
            MotorWorkTable.EncCtsPerRound = 1000;// 10000.0;  1000cts = 1degree
            MotorWorkTable.BallScrewLead = 1.0;// 360.0/30.0;
            MotorWorkTable.EncoderFactor =
            MotorWorkTable.EncCtsPerRound /
            MotorWorkTable.BallScrewLead;
            MotorWorkTable.Acceleration = defaultAcceleration;
            MotorWorkTable.Deceleration = defaultAcceleration;
            MotorWorkTable.Velocity = 60.0;
            MotorWorkTable.HomeSearchMode = mc.HOME_MODE_HOME;
            MotorWorkTable.HomeSearchDirection = MoveDirection.Positive;
            MotorWorkTable.HomeSearchIndexDirection = MoveDirection.Positive;
            MotorWorkTable.EdgeCaptureMode = EdgeCapture.Falling;
            MotorWorkTable.HomeOffset = defaultHomeOffset;
            MotorWorkTable.Direction = 1.0;
            MotorWorkTable.CriticalErrIdle = allowedError;
            MotorWorkTable.HomeLimitSpeed = 45.0;
            MotorWorkTable.HomeIndexSpeed = 30.0;
            #endregion

            #region Conveyors
            MotorVConveyorUnload.Id = Axis.VConveyorUnload;
            MotorVConveyorUnload.EncCtsPerRound = 1000.0;
            MotorVConveyorUnload.BallScrewLead = 1;
            MotorVConveyorUnload.EncoderFactor =
            MotorVConveyorUnload.EncCtsPerRound /
            MotorVConveyorUnload.BallScrewLead;
            MotorVConveyorUnload.Acceleration = defaultAcceleration;
            MotorVConveyorUnload.Deceleration = defaultAcceleration;
            MotorVConveyorUnload.Velocity = defaultSpeed;
            MotorVConveyorUnload.HomeSearchMode = mc.HOME_MODE_HOME;
            MotorVConveyorUnload.HomeSearchDirection = MoveDirection.Positive;
            MotorVConveyorUnload.HomeSearchIndexDirection = MoveDirection.Positive;
            MotorVConveyorUnload.EdgeCaptureMode = EdgeCapture.Falling;
            MotorVConveyorUnload.HomeOffset = defaultHomeOffset;
            MotorVConveyorUnload.Direction = 1.0;
            MotorVConveyorUnload.CriticalErrIdle = allowedError;
            MotorVConveyorUnload.HomeLimitSpeed = 90.0;
            MotorVConveyorUnload.HomeIndexSpeed = 90.0;

            MotorVConveyorLoad.Id = Axis.VConveyorLoad;
            MotorVConveyorLoad.EncCtsPerRound = 1000.0;
            MotorVConveyorLoad.BallScrewLead = 1;
            MotorVConveyorLoad.EncoderFactor =
            MotorVConveyorLoad.EncCtsPerRound /
            MotorVConveyorLoad.BallScrewLead;
            MotorVConveyorLoad.Acceleration = defaultAcceleration;
            MotorVConveyorLoad.Deceleration = defaultAcceleration;
            MotorVConveyorLoad.Velocity = defaultSpeed;
            MotorVConveyorLoad.HomeSearchMode = mc.HOME_MODE_HOME;
            MotorVConveyorLoad.HomeSearchDirection = MoveDirection.Positive;
            MotorVConveyorLoad.HomeSearchIndexDirection = MoveDirection.Positive;
            MotorVConveyorLoad.EdgeCaptureMode = EdgeCapture.Falling;
            MotorVConveyorLoad.HomeOffset = defaultHomeOffset;
            MotorVConveyorLoad.Direction = 1.0;
            MotorVConveyorLoad.CriticalErrIdle = allowedError;
            MotorVConveyorLoad.HomeLimitSpeed = 90.0;
            MotorVConveyorLoad.HomeIndexSpeed = 90.0;

            MotorLConveyorLoad.Id = Axis.LConveyorLoad;
            MotorLConveyorLoad.EncCtsPerRound = 1000.0;
            MotorLConveyorLoad.BallScrewLead = 1;
            MotorLConveyorLoad.EncoderFactor =
            MotorLConveyorLoad.EncCtsPerRound /
            MotorLConveyorLoad.BallScrewLead;
            MotorLConveyorLoad.Acceleration = defaultAcceleration;
            MotorLConveyorLoad.Deceleration = defaultAcceleration;
            MotorLConveyorLoad.Velocity = defaultSpeed;
            MotorLConveyorLoad.HomeSearchMode = mc.HOME_MODE_HOME;
            MotorLConveyorLoad.HomeSearchDirection = MoveDirection.Positive;
            MotorLConveyorLoad.HomeSearchIndexDirection = MoveDirection.Positive;
            MotorLConveyorLoad.EdgeCaptureMode = EdgeCapture.Falling;
            MotorLConveyorLoad.HomeOffset = defaultHomeOffset;
            MotorLConveyorLoad.Direction = 1.0;
            MotorLConveyorLoad.CriticalErrIdle = allowedError;
            MotorLConveyorLoad.HomeLimitSpeed = 90.0;
            MotorLConveyorLoad.HomeIndexSpeed = 90.0; 
            #endregion

            #region Add to list.
            Motors.Clear();

            Motors.Add(MotorLX);
            Motors.Add(MotorLY);
            Motors.Add(MotorLZ);
            Motors.Add(MotorLRotateLoad);
            Motors.Add(MotorLTrayLoad);
            Motors.Add(MotorLTrayUnload);

            Motors.Add(MotorVX);
            Motors.Add(MotorVY);
            Motors.Add(MotorVZ);
            Motors.Add(MotorVRotateLoad);
            Motors.Add(MotorVRotateUnload);
            Motors.Add(MotorVTrayLoad);
            Motors.Add(MotorVTrayUnload);

            Motors.Add(MotorGlueLineX);
            Motors.Add(MotorGlueLineY);
            Motors.Add(MotorGlueLineZ);

            Motors.Add(MotorGluePointX);
            Motors.Add(MotorGluePointY);
            Motors.Add(MotorGluePointZ);

            Motors.Add(MotorVConveyorUnload);
            Motors.Add(MotorVConveyorLoad);
            Motors.Add(MotorLConveyorLoad);

            Motors.Add(MotorWorkTable); 
            #endregion

            foreach (var mtr in Motors)
            {
                //ZeroPosition(mtr); Zero after motor enabled.
                SetErrorBand(mtr);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="motor"></param>
        /// <seealso cref="WaitTillEnd(Motor, int)"/>
        public void SetErrorBand(Motor motor)
        {
            var ca = ConvertToAxis(motor);
            // Time unit is 250us
            int band = (int)(motor.CriticalErrIdle * motor.EncoderFactor);
            mc.GTN_SetAxisBand(ca.Core, ca.Axis, band, 20);
        }

        private void ControllerSetup()
        {
            //Reset two cores
            Run(mc.GTN_Reset(1), "Setup fail");
            Run(mc.GTN_Reset(2), "Setup fail");
            //Load config file.
            Run(mc.GTN_LoadConfig(1, "gtn_core1.cfg"), "Setup fail");
            Run(mc.GTN_LoadConfig(2, "gtn_core2.cfg"), "Setup fail");
            //Clear fault, 12 motors of each core.
            Run(mc.GTN_ClrSts(1, 1, 12), "Setup fail");
            Run(mc.GTN_ClrSts(2, 1, 12), "Setup fail");
        }

        #endregion

        #region Helper

        /// <summary>
        /// Todo throw a class Error.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        private void ThrowException(int code, string description)
        {
            throw new Exception(code + " " + description);
        }

        private double ConvertToPulseVel(Motor motor, double userVel)
        {
            return userVel * motor.EncoderFactor / 1000.0;
        }

        private int ConvertToPulseDistance(Motor motor, double distance)
        {
            return Convert.ToInt32(motor.Direction * motor.EncoderFactor * distance);
        }

        /// <summary>
        /// Todo test acc.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="userAcc"></param>
        /// <returns></returns>
        private double ConvertToPulseAcc(Motor motor, double userAcc)
        {
            // User unit mm/sec^2;
            // Controller unit pulse/ms^2
            // 1mm / sec^2 = 1*encoderFactor pulse / (1000ms*1000ms), 
            // 1mm / sec^2 / encoderFactor / 1000000.0 = 1pulse / ms^2
            // PulseAcc unit = User unit / (encoderFactor * 1000000.0)

            return userAcc / motor.EncoderFactor / 1000000.0;
        }

        /// <summary>
        /// Convert motor Id to controller core and axis number.
        /// </summary>
        /// <param name="motor"></param>
        /// <returns></returns>
        private CoreAndAxis ConvertToAxis(Motor motor)
        {
            CoreAndAxis coreAndAxis = new CoreAndAxis();
            var mtr = (short)motor.Id;
            coreAndAxis.Core = (short)(mtr / 100);
            coreAndAxis.Axis = (short)(mtr % 100);
            return coreAndAxis;
        }

        private InputCoreAndPin ConvertToInput(Input input)
        {
            InputCoreAndPin cp = new InputCoreAndPin();
            cp.Core = (short)((short)input / 100);
            cp.Pin = (short)((short)input % 100);
            return cp;
        }

        private InputCoreAndPin ConvertToOutput(Output output)
        {
            InputCoreAndPin cp = new InputCoreAndPin();
            cp.Core = (short)((short)output / 100);
            cp.Pin = (short)((short)output % 100);
            return cp;
        }

        private void Run(short returnValue, string exceptionMessage)
        {
            if (returnValue != 0)
            {
                throw new Exception("Error code: " + returnValue + ", " + exceptionMessage);
            }
        }

        public void Delay(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }

        private int ConvertToAxisMask(short axis)
        {
            return 1 << (axis - 1);
        }

        /// <summary>
        /// Digital output.
        /// </summary>
        /// <param name="doVel"></param>
        /// <returns></returns>
        private ushort ConvertToDoMask(short doVel)
        {
            return Convert.ToUInt16(1 << (doVel - 1));
        }

        #endregion      

        /// <summary>
        /// Servo alarm, following error, limit.
        /// </summary>
        public void ClearFault(Motor motor, bool checkLimit = true)
        {
            var ca = ConvertToAxis(motor);
            Run(mc.GTN_ClrSts(ca.Core, ca.Axis, 1), "Clear fault exception:" + motor.Id);
            Delay(10);

            if (IsDriverAlarmed(motor))
            {
                throw new Exception("Clear fault fail, driver is still alarmed: " + motor.Id);
            }
            
            if (checkLimit && IsHittingLimit(motor))
            {
                throw new Exception("Clear fault fail, motor is still inside limit: " + motor.Id);
            }
        }

        public void ClearAllFault()
        {
            foreach (var mtr in Motors)
            {
                ClearFault(mtr);
            }
        }

        /// <summary>
        /// By default, linear motor need second home.
        /// </summary>
        public void Home(Motor motor) 
        {
            ClearFault(motor, false);
            Enable(motor);
            CheckEnabledAndNotMoving(motor);

            var exceptionInfo = "Home exception: " + motor.Id;
            var ca = ConvertToAxis(motor);

            MoveAwayFromLimit(motor);
            
            Run(mc.GTN_GetHomePrm(ca.Core, ca.Axis, out mc.THomePrm homingParam), exceptionInfo);

            homingParam.mode = motor.HomeSearchMode;
            homingParam.moveDir = (short)((short)motor.HomeSearchDirection * (short)motor.Direction);
            homingParam.indexDir = (short)((short)motor.HomeSearchIndexDirection * (short)motor.Direction);
            homingParam.edge = (short)motor.EdgeCaptureMode;
            homingParam.velHigh = ConvertToPulseVel(motor, motor.HomeLimitSpeed);
            homingParam.velLow = ConvertToPulseVel(motor, motor.HomeIndexSpeed);
            homingParam.acc = 0.5;// ConvertToPulseAcc(motor.EncoderFactor, motor.Acceleration);
            homingParam.dec = 0.5;//ConvertToPulseAcc(motor.EncoderFactor, motor.Acceleration);
            homingParam.homeOffset = ConvertToPulseDistance(motor, motor.HomeOffset);
            homingParam.searchHomeDistance = Math.Abs(ConvertToPulseDistance(motor, motor.MaxTravel));
            homingParam.searchIndexDistance = Math.Abs(ConvertToPulseDistance(motor, motor.MaxTravel));
            homingParam.escapeStep = Math.Abs(ConvertToPulseDistance(motor, 10)); //Away from sensor.

            Run(mc.GTN_GoHome(ca.Core, ca.Axis, ref homingParam), exceptionInfo);
        }

        private bool IsHoming(Motor motor)
        {
            var ca = ConvertToAxis(motor);
            mc.GTN_GetHomeStatus(ca.Core, ca.Axis, out mc.THomeStatus homeStatus);
            return homeStatus.run == 0;
        }

        public void WaitTillHomeEnd(Motor motor, int timeout = 60)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            bool homeEnd = false;

            while (homeEnd == false)
            {
                if (stopwatch.ElapsedMilliseconds > timeout * 1000)
                {
                        throw new Exception("WaitTillEnd in pos fail: " + motor.Id);
                }

                homeEnd = IsHoming(motor);
                Delay(20);
                CheckDriverAlarm(motor);
            }           
        }

        public void DisableLimits(Motor motor)
        {
            var ca = ConvertToAxis(motor);
            mc.GTN_LmtsOff(ca.Core, ca.Axis, -1);
        }

        public void DisableAllLimits()
        {
            foreach (var mtr in Motors)
            {
                DisableLimits(mtr);
            }
        }

        public void HomeAllMotors(double speed = 10.0)
        {
            //Set home speed here.
            AxisSetup(speed);

            Enable(MotorLZ);
            Enable(MotorVZ);
            Enable(MotorGlueLineZ);
            Enable(MotorGluePointZ);
            Delay(500);
            ZeroAllPositions();

            Home(MotorLZ);
            Home(MotorVZ);
            Home(MotorGlueLineZ);
            Home(MotorGluePointZ);
            WaitTillHomeEnd(MotorLZ);
            WaitTillHomeEnd(MotorVZ);
            WaitTillHomeEnd(MotorGlueLineZ);
            WaitTillHomeEnd(MotorGluePointZ);

            EnableAll();
            Delay(5000);
            ZeroAllPositions();

            Home(MotorWorkTable);
            Home(MotorLRotateLoad);
            Home(MotorVRotateLoad);
            Home(MotorVRotateUnload);

            Home(MotorLX);
            Home(MotorVX);
            Home(MotorGlueLineX);
            Home(MotorGluePointX);
            WaitTillHomeEnd(MotorLX);
            WaitTillHomeEnd(MotorVX);
            WaitTillHomeEnd(MotorGlueLineX);
            WaitTillHomeEnd(MotorGluePointX);

            Home(MotorLY);
            Home(MotorVY);
            Home(MotorGlueLineY);
            Home(MotorGluePointY);
            WaitTillHomeEnd(MotorLY);
            WaitTillHomeEnd(MotorVY);
            WaitTillHomeEnd(MotorGlueLineY);
            WaitTillHomeEnd(MotorGluePointY);

            WaitTillHomeEnd(MotorLRotateLoad);
            WaitTillHomeEnd(MotorVRotateLoad);
            WaitTillHomeEnd(MotorVRotateUnload);
            WaitTillHomeEnd(MotorWorkTable);

            ZeroAllPositions(3000);
        }

        public void MoveAwayFromLimit(Motor motor, double distance = 20)
        {
            if (GetState(motor, MotorState.PositiveLimit))
            {
                ZeroPosition(motor);
                MoveToTargetTillEnd(motor, -motor.Direction * distance);
            }
            if (GetState(motor, MotorState.NegativeLimit))
            {
                ZeroPosition(motor);
                MoveToTargetTillEnd(motor, motor.Direction * distance);
            }

            if (motor.HomeSearchMode == mc.HOME_MODE_HOME && GetHomeSensor(motor))
            {
                ZeroPosition(motor);
                MoveToTargetTillEnd(motor, motor.Direction * distance * 2.0); //Step sensor is wide.
            }
        }

        public void SetPosition()
        {

        }

        public void SetVelocity()
        {

        }

        public void SetAcceleration()
        {

        }

        public void SetDeceleration()
        {

        }

        public void SetJerk()
        {

        }

        /// <summary>
        /// Clear both encoder and reference position.
        /// </summary>
        /// <param name="motor"></param>
        public void ZeroPosition(Motor motor)
        {
            motor.HomeComplete = false;
            var ca = ConvertToAxis(motor);
            Run(mc.GTN_ZeroPos(ca.Core, ca.Axis, 1), "Zero Position exception:" + motor.Id);
        }

        /// <summary>
        /// Delay 500ms and zero all enconders.
        /// </summary>
        public void ZeroAllPositions(int delayMs = 1000)
        {
            Delay(delayMs);
            foreach (var mtr in Motors)
            {
                ZeroPosition(mtr);
            }
        }

        /// <summary>
        /// Get encoder position.
        /// </summary>
        /// <param name="motor"></param>
        /// <returns></returns>
        public double GetPosition(Motor motor)
        {
            var ca = ConvertToAxis(motor);

            if (motor.CloseLoop==false)
            {
                Run(mc.GTN_GetPrfPos(ca.Core, ca.Axis, out double prfPos, 1, out uint pClock),
                "Get profile position exception: " + motor.Id);
                return motor.Direction * prfPos / motor.EncoderFactor;
            }
            else
            {
                Run(mc.GTN_GetEncPos(ca.Core, ca.Axis, out double encPos, 1, out uint innerClock),
                "Get encoder position exception: " + motor.Id);
                return motor.Direction * encPos / motor.EncoderFactor;
            }                     
        }

        /// <summary>
        /// Get target position
        /// </summary>
        /// <param name="motor"></param>
        /// <returns></returns>
        public double GetReferencePosition(Motor motor)
        {
            var ca = ConvertToAxis(motor);
            Run(mc.GTN_GetPos(ca.Core, ca.Axis, out int encPos),
              "Get target position exception: " + motor.Id);
            return motor.Direction * Convert.ToDouble(encPos) / motor.EncoderFactor;
        }

        public double GetError(Motor motor)
        {
            if (motor.CloseLoop == false)
            {
                return 0.0;
            }
            else
            {
                var ca = ConvertToAxis(motor);

                double prfPos = -1.0;
                double encPos = -1.0;

                Run(mc.GTN_GetPrfPos(ca.Core, ca.Axis, out prfPos, 1, out uint pClock),
                "Get profile position exception: " + motor.Id);
                Run(mc.GTN_GetEncPos(ca.Core, ca.Axis, out encPos, 1, out uint innerClock),
                "Get encoder position exception: " + motor.Id);

                return  (prfPos - encPos) * motor.Direction  / motor.EncoderFactor;
            }
        }

        /// <summary>
        /// Set reference position. Danger
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="referencePos"></param>
        public void SetReferencePosition(Motor motor, double referencePos)
        {
            throw new NotImplementedException();
        }

        public void Enable(Motor motor)
        {
            var ca = ConvertToAxis(motor);
            Run(mc.GTN_AxisOn(ca.Core, ca.Axis), "Enable exception: " + motor.Id );
        }

        public void EnableAll()
        {
            foreach (var mtr in Motors)
            {
                if (GetState(mtr, MotorState.Enabled))
                {
                    continue;
                }
                Enable(mtr);
            }
        }

        public bool IsEnabled(Motor motor)
        {
            return GetState(motor, MotorState.Enabled);
        }

        public bool IsHittingLimit(Motor motor)
        {
            return GetState(motor, MotorState.NegativeLimit) || GetState(motor, MotorState.PositiveLimit);
        }

        public bool IsDriverAlarmed(Motor motor)
        {
            return GetState(motor, MotorState.ServoAlarm);
        }

        public bool IsMoving(Motor motor)
        {
            return GetState(motor, MotorState.Moving);
        }

        public void CheckEnabledAndNotMoving(Motor motor)
        {
            if (IsEnabled(motor) == false || IsMoving(motor))
            {
                motor.HomeComplete = IsEnabled(motor);
                throw new Exception("Motor " + motor.Id + " is not enabled or moving.");
            }
        }

        public void CheckDriverAlarm(Motor motor)
        {
            if (IsEnabled(motor) == false || IsDriverAlarmed(motor))
            {
                motor.HomeComplete = IsEnabled(motor);
                throw new Exception("Motor " + motor.Id + " alarmed.");
            }
        }

        public void Disable(Motor motor)
        {
            var ca = ConvertToAxis(motor);
            Run(mc.GTN_AxisOff(ca.Core, ca.Axis), "Disable exception: " + motor.Id);
        }

        public bool GetInput(Input input)
        {
            var cp = ConvertToInput(input);

            if (((int)input % 100) > 32)
            {
                int[] inputValue = new int[3];
                Run(mc.GTN_GetDiEx(cp.Core, mc.MC_GPI, out inputValue[0], 2),
                    "Get input value exception: " + input);
                inputValue[1] = -(inputValue[1] + 1);
                return Helper.GetBit(inputValue[1], cp.Pin % 32 - 1);
            }
            else
            {
                Run(mc.GTN_GetDi(cp.Core, mc.MC_GPI, out int inputValue),
                    "Get input value exception:" + input);
                inputValue = -(inputValue + 1);
                return Helper.GetBit(inputValue, cp.Pin - 1);
            }
        }

        public bool GetHomeSensor(Motor motor)
        {
            var ca = ConvertToAxis(motor);
            Run(mc.GTN_GetDi(ca.Core, mc.MC_HOME, out int inputValue),
                    "Get home sensor exception:" + motor.Id);
            inputValue = -(inputValue + 1);
            return !Helper.GetBit(inputValue, ca.Axis - 1);
        }

        public void CheckSensor(Input sensor, bool state)
        {
            if (GetInput(sensor) != state)
            {
                throw new Exception("CheckSensor exception: Sensor " + sensor + " is not " + state);
            }
        }

        public void SetOutput(Output output, OutputState state)
        {
            var cp = ConvertToOutput(output);
            Run(mc.GTN_SetDoBit(cp.Core, mc.MC_GPO, cp.Pin, (short)state),
                   "Get input value exception:" + output);
        }

        public void Stop(Motor motor)
        {
            var ca = ConvertToAxis(motor);
            int mask = 0;
            Helper.SetBit(ref mask, ca.Axis-1);
            short rtn = mc.GTN_Stop(ca.Core, mask, 0x000);
        }

        public void StopAllEmergency()
        {
            Run(mc.GTN_Stop(1, 0xfff, 0xfff), "Stop all fail.");
            Run(mc.GTN_Stop(2, 0xfff, 0xfff), "Stop all fail.");
        }

        public void StopAllSoft()
        {
            Run(mc.GTN_Stop(1, 0x000, 0xfff), "Stop all fail.");
            Run(mc.GTN_Stop(2, 0x000, 0xfff), "Stop all fail.");
        }

        public void Jog(Motor motor, double speed, MoveDirection direction)
        {
            lock (_motionLocker)
            {
                var dire = (double)direction;
                CheckEnabledAndNotMoving(motor);
                var exceptionInfo = "Jog exception: " + motor.Id;
                var ca = ConvertToAxis(motor);

                mc.TJogPrm jogParam;
                Run(mc.GTN_PrfJog(ca.Core, ca.Axis), exceptionInfo);
                Run(mc.GTN_GetJogPrm(ca.Core, ca.Axis, out jogParam), exceptionInfo);

                jogParam.acc = 0.5;//ConvertToPulseAcc(motor.EncoderFactor, motor.Acceleration); 
                jogParam.dec = 0.5;// ConvertToPulseAcc(motor.EncoderFactor, motor.Acceleration);
                jogParam.smooth = 0.99; //A factor, [0,1], biger, more smooth.

                Run(mc.GTN_SetJogPrm(ca.Core, ca.Axis, ref jogParam), exceptionInfo);
                Run(mc.GTN_SetVel(ca.Core, ca.Axis,
                    dire * motor.Direction * ConvertToPulseVel(motor, speed)),
                    exceptionInfo);
                Run(mc.GTN_Update(ca.Core, ConvertToAxisMask(ca.Axis)), exceptionInfo);
            }            
        }      

        public void MoveToTarget(Motor motor, double target)
        {
            lock (_motionLocker)
            {
                if (motor.IsMoving)
                {
                    throw new Exception("Motor is moving, can not start a new move: " + motor.Id);
                }

                Delay(20);
                CheckEnabledAndNotMoving(motor);
                //motor.TargetPosition = target;
                var exceptionInfo = "Move To Target exception:" + motor.Id;
                var ca = ConvertToAxis(motor);

                Stop(motor);

                //Clear profile position
                Run(mc.GTN_SetPrfPos(ca.Core, ca.Axis, 0), "Clear profile position fail: " + motor.Id);

                //Set point to point mode.
                Run(mc.GTN_PrfTrap(ca.Core, ca.Axis), exceptionInfo);

                Run(mc.GTN_GetTrapPrm(ca.Core, ca.Axis, out mc.TTrapPrm trapParam), exceptionInfo);
                trapParam.acc = 0.5;//ConvertToPulseAcc(motor.EncoderFactor, motor.Acceleration);
                trapParam.dec = 0.5;// ConvertToPulseAcc(motor.EncoderFactor, motor.Acceleration);
                trapParam.smoothTime = motor.SmoothTime;
                Run(mc.GTN_SetTrapPrm(ca.Core, ca.Axis, ref trapParam), exceptionInfo);

                var vel = ConvertToPulseVel(motor, motor.Velocity);
                Run(mc.GTN_SetVel(ca.Core, ca.Axis, vel), exceptionInfo);

                var tarPulse = ConvertToPulseDistance(motor, target);

                motor.MovementId++;
                log.Info("Start movement of " + motor.Id + " to target " + tarPulse + ", ID: " + motor.MovementId);

                motor.TargetPosition = target;
                Run(mc.GTN_SetPos(ca.Core, ca.Axis, tarPulse), exceptionInfo);

                var targetPos = GetReferencePosition(motor);

                if (Math.Abs(target - targetPos) > 0.01)
                {
                    throw new Exception("GTN_SetPos fail for " + motor.Id);
                }

                Run(mc.GTN_Update(ca.Core, ConvertToAxisMask(ca.Axis)), exceptionInfo);

                motor.IsMoving = true;                
                Delay(20);
            }
            
        }


        public void MoveToTargetTillEnd(Motor motor, double target)
        {
            MoveToTarget(motor, target);
            WaitTillEnd(motor);
        }

        /// <summary>
        /// Stepper motor need to set as inner pulse feedback.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="target"></param>
        public void MoveToTargetRelative(Motor motor, double target)
        {
            CheckEnabledAndNotMoving(motor);

            double currentPos = GetPosition(motor);
            target += currentPos;  
            
            MoveToTarget(motor, target);
            motor.TargetPosition = target;
        }

        public void MoveToTargetRelativeTillEnd(Motor motor, double target)
        {
            MoveToTargetRelative(motor, target);
            WaitTillEnd(motor);
        }

        /// <summary>
        /// Todo check limit.
        /// </summary>
        /// <param name="timeout">second</param>
        /// <seealso cref="SetErrorBand(Motor)"/>
        public void WaitTillEnd(Motor motor, int timeout = 30)
        {
            try
            {
                if (motor.IsMoving==false)
                {
                    throw new Exception("Can not wait till end for a motor that is not moving: " + motor.Id);
                }

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                bool inpos = false;
                bool enabled = false;
                bool driverFault = true;
                bool isMoving = true;
                bool negativeLimitTriggered = false;
                bool positiveLimitTriggered = false;

                while (inpos == false || enabled == false || driverFault == true || isMoving == true)
                {
                    if (stopwatch.ElapsedMilliseconds > timeout * 1000)
                    {
                        if (inpos == false)
                        {
                            throw new Exception("WaitTillEnd inpos false: " + motor.Id);
                        }
                        if (enabled == false)
                        {
                            throw new Exception("WaitTillEnd motor disabled: " + motor.Id);
                        }
                        if (driverFault == true)
                        {
                            throw new Exception("WaitTillEnd driver fault: " + motor.Id);
                        }
                    }

                    if (positiveLimitTriggered)
                    {
                        throw new Exception("Positive limit triggered: " + motor.Id);
                    }
                    if (negativeLimitTriggered)
                    {
                        throw new Exception("Negative limit triggered: " + motor.Id);
                    }

                    inpos = GetState(motor, MotorState.InPosition);
                    enabled = GetState(motor, MotorState.Enabled);
                    driverFault = GetState(motor, MotorState.ServoAlarm);
                    isMoving = GetState(motor, MotorState.Moving);
                    negativeLimitTriggered = GetState(motor, MotorState.NegativeLimit);
                    positiveLimitTriggered = GetState(motor, MotorState.PositiveLimit);
                }

                Delay(50);
                var target = motor.TargetPosition;
                var current = GetPosition(motor);
                var fe = Math.Abs(target - current);
                log.Info("End of movement of " + motor.Id + ", ID " + motor.MovementId + ", following error is " + fe);

                if (fe > motor.CriticalErrIdle)
                {
                    string waitTillEndException = string.Empty;
                    waitTillEndException = motor.Id + " fatal following error: " + fe +
                        ", Programed target is " + target + ", GTN_GetPos is " + GetReferencePosition(motor) +
                        ", GTN_GetEncPos is " + current;

                    log.Info(waitTillEndException);
                    throw new Exception(waitTillEndException);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                motor.IsMoving = false;
            }            
        }

        public bool GetState(Motor motor, MotorState state)
        {
            var ca = ConvertToAxis(motor);
            Run(mc.GTN_GetSts(ca.Core, ca.Axis, out int stateValue, 1, out uint controllerClock), 
                "Get state exception:" + motor.Id);
            return Helper.GetBit(stateValue, (int)state);
        }

        /// <summary>
        /// Big bug, should have not stop all axis.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="coordinateId"></param>
        /// <param name="fifo"></param>
        public void ClearInterpolationBuffer(short core, CoordinateId coordinateId, short fifo = 0)
        {
            //Run(mc.GTN_Stop(2, 0xff, 0), "Stop all interpolation fail");
            Run(mc.GTN_CrdClear(core, (short)coordinateId, fifo), 
                "ClearInterpolationBuffer exception");
        }

        /// <summary>
        /// Add to motion buffer.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="coordinateId"></param>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <param name="synVel"></param>
        /// <param name="synAcc"></param>
        /// <param name="endVel"></param>
        /// <param name="fifo"></param>
        public void AddTargetPoint(short core, short coordinateId, int xPos, int yPos, int zPos,
            double synVel, double synAcc, double endVel, short fifo)
        {
            Run(mc.GTN_LnXYZ(core, coordinateId, xPos, yPos, zPos, synVel, synAcc, endVel, fifo), 
                "Set target point exception");
        }

        public void AddArcXYC(short core, short coordinateId, int xPos, int yPos, 
            double xCenter, double yCenter, double synVel, double synAcc, 
            double endVel =0, short circleDir = 1, short fifo = 0)
        {
            Run(mc.GTN_ArcXYC(core, coordinateId, xPos, yPos, xCenter, yCenter,
                circleDir, synVel, synAcc, endVel, fifo),
                "Set target point exception");
        }

        public void AddOutput(short core, short coordinateId, Output output, OutputState state, short fifo =0)
        {
            var cp = ConvertToOutput(output);
            switch (state)
            {
                case OutputState.On:
                    Run(mc.GTN_BufIO(core, coordinateId, mc.MC_GPO, ConvertToDoMask(cp.Pin), 
                        0, fifo), "Add digital output exception");
                    break;
                case OutputState.Off:
                    Run(mc.GTN_BufIO(core, coordinateId, mc.MC_GPO, ConvertToDoMask(cp.Pin),
                        ConvertToDoMask(cp.Pin), fifo), "Add digital output exception");
                    break;
                default:
                    break;
            }
            
        }

        public void AddDelay(CoordinateId id,
          ushort delayMs, short core = 2, short fifo = 0)
        {
            Run(mc.GTN_BufDelay(core, (short)id, delayMs, fifo),
                "Add digital output exception");
        }

        public bool IsCrdSpaceEnough(CoordinateId id, short core = 2, short fifo = 0)
        {
            Run(mc.GTN_CrdSpace(core, (short)id, out int pSpace, fifo), "No space for buffer");
            //Todo Right?
            return pSpace > 10;
        }

        public void StartInterpolation(CoordinateId mask, short core = 2, short option = 0)
        {
            Run(mc.GTN_CrdStart(core, (short)mask, option), "Start interpolation exception.");
        }

        public bool IsInterpolationFinished(CoordinateId mask, short core = 2, short fifo = 0)
        {
            Run(mc.GTN_CrdStatus(core, (short)mask, out short pRun, out int pSegment, fifo),
                "Get coordinate running error");
            return pRun == 0;    
        }

        public void WaitTillInterpolationEnd(CoordinateId id, int timeoutSec = 120)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            bool end = false;

            while (end == false)
            {
                if (stopwatch.ElapsedMilliseconds > timeoutSec * 1000)
                {
                    if (end == false)
                    {
                        throw new Exception("Interpolation finish timeout: " + id);
                    }
                }

                Delay(20);
                end = IsInterpolationFinished(id);
            }
        }

        /// <summary>
        /// Set max vel and acc and smooth time.
        /// </summary>
        /// <param name="coordinate"></param>
        public void SetCoordinateSystem(CoordinateId coordinate)
        {
            mc.TCrdPrm crdprm;

            switch (coordinate)
            {
                case CoordinateId.GluePoint:
                    crdprm.dimension = 3;//坐标系的维数为3维
                    crdprm.synVelMax = 200;//最大合成速度为500
                    crdprm.synAccMax = 10;//最大合成加速度为100
                    crdprm.evenTime = 10;//最小匀速时间为50ms
                    crdprm.profile1 = 0;//规划器1对应X轴
                    crdprm.profile2 = 0;//规划器2对应Y轴
                    crdprm.profile3 = 0;//规划器3对应Z轴
                    crdprm.profile4 = 1;//规划器2对应A轴
                    crdprm.profile5 = 2;//规划器1对应X轴
                    crdprm.profile6 = 3;//规划器2对应Y轴
                    crdprm.profile7 = 0;//规划器3对应Z轴
                    crdprm.profile8 = 0;//规划器2对应A轴

                    crdprm.setOriginFlag = 1;//1表示需要用户指定坐标原点的规划位置
                    crdprm.originPos1 = 0;//1轴的规划位置为0
                    crdprm.originPos2 = 0;//2轴的规划位置为0
                    crdprm.originPos3 = 0;//3轴的规划位置为0
                    crdprm.originPos4 = 0;//2轴的规划位置为0
                    crdprm.originPos5 = 0;//1轴的规划位置为0
                    crdprm.originPos6 = 0;//2轴的规划位置为0
                    crdprm.originPos7 = 0;//3轴的规划位置为0
                    crdprm.originPos8 = 0;//2轴的规划位置为0

                    
                    break;

                case CoordinateId.GlueLine:
                    crdprm.dimension = 3;//坐标系的维数为3维
                    crdprm.synVelMax = 200;//最大合成速度为500
                    crdprm.synAccMax = 10;//最大合成加速度为100
                    crdprm.evenTime = 10;//最小匀速时间为50ms
                    crdprm.profile1 = 1;//规划器1对应X轴
                    crdprm.profile2 = 2;//规划器2对应Y轴
                    crdprm.profile3 = 3;//规划器3对应Z轴
                    crdprm.profile4 = 0;//A轴
                    crdprm.profile5 = 0;//规划器1对应X轴
                    crdprm.profile6 = 0;//规划器2对应Y轴
                    crdprm.profile7 = 0;//规划器3对应Z轴
                    crdprm.profile8 = 0;//A轴

                    crdprm.setOriginFlag = 1;//1表示需要用户指定坐标原点的规划位置
                    crdprm.originPos1 = 0;//1轴的规划位置为0
                    crdprm.originPos2 = 0;//2轴的规划位置为0
                    crdprm.originPos3 = 0;//3轴的规划位置为0
                    crdprm.originPos4 = 0;//2轴的规划位置为0
                    crdprm.originPos5 = 0;//1轴的规划位置为0
                    crdprm.originPos6 = 0;//2轴的规划位置为0
                    crdprm.originPos7 = 0;//3轴的规划位置为0
                    crdprm.originPos8 = 0;//2轴的规划位置为0
                    break;

                default:
                    throw new NotImplementedException("Set coordinate");
            }

            Run(mc.GTN_SetCrdPrm(2, (short)coordinate, ref crdprm), "Set coordinate system error.");
        }

        public void SetCoordinateSystem()
        {
            throw new NotImplementedException();
        }

        public bool GetOpticalSensor(Input input, int delayMs = 20, int readTimes = 3)
        {
            int times = 0;
            do
            {
                if (GetInput(input) == false)
                {
                    return false;
                }
                Delay(delayMs);
                times++;
            } while (times<readTimes);

            return true;
        }
    }


}
