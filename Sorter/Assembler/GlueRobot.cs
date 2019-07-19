using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorter
{
    public class GlueRobot
    {
        private readonly MotionController _controller;

        public Motor MotorX { get; set; }
        public Motor MotorY { get; set; }
        public Motor MotorZ { get; set; }

        public double CalculationHeight { get; set; }

        /// <summary>
        /// For needle cleaning.
        /// </summary>
        public int GlueCount { get; set; }

        public Pose BottomCameraPose { get; set; }

        public Pose TrayApproachPose { get; set; }

        public double SuckerXOffset { get; set; } = 50.0;

        public Coordinate VisionToSuckerOffset { get; set; }

        public bool VisionSimulate { get; set; } = true;

        public GlueRobot(MotionController controller)
        {
            _controller = controller;
        }

        public void MoveToLaserCrossPoint()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="CalculationHeight"/>
        public void MoveToCalculationHeight()
        {

        }

        public double CaptureNeedleHeight()
        {
            throw new NotImplementedException();
        }

        public Coordinate DetectNeedleCoordinate()
        {
            throw new NotImplementedException();
        }

        public void OpenGlue()
        {

        }

        public void CloseGlue()
        {

        }

        public void SetSpeed(double speed)
        {
            MotorX.Velocity = speed;
            MotorY.Velocity = speed;
            MotorZ.Velocity = speed;
        }

        public void Setup(Motor motorX, Motor motorY, Motor motorZ)
        {
            MotorX = motorX;
            MotorY = motorY;
            MotorZ = motorZ;
        }
    }
}
