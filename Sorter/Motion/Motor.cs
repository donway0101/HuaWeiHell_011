using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorter
{
    public class Motor
    {
        public Axis Id { get; set; }

        public bool IsMoving { get; set; }

        public long MovementId { get; set; }

        public string Name { get; set; }

        public double FeedbackPosition { get; set; }

        public bool CloseLoop { get; set; } = true;

        /// <summary>
        /// Encoder counts per round.
        /// </summary>
        public double EncCtsPerRound { get; set; }
        
        public double BallScrewLead { get; set; } = 1.0; 

        public double EncoderFactor { get; set; }

        public double HomeOffset { get; set; }

        public bool HomeComplete { get; set; }

        /// <summary>
        /// Critical error on acceleration.
        /// </summary>
        public double CriticalErrAcc { get; set; }

        public double CriticalErrVel { get; set; }

        public double CriticalErrIdle { get; set; } = 0.1;

        public double SoftLimitNegative { get; set; }

        public double SoftLimitPositive { get; set; }

        public double SpeedFactor { get; set; } = 1;

        public double Direction { get; set; } = 1.0;

        /// <summary>
        /// Unit mm/sec
        /// </summary>
        public double Velocity { get; set; } = 1.0; 

        /// <summary>
        /// Unit mm/sec^2
        /// </summary>
        public double Acceleration { get; set; } = 1.0;

        /// <summary>
        /// Unit mm/sec^2
        /// </summary>
        public double Deceleration { get; set; } = 1.0;

        public short SmoothTime { get; set; } = 50; //ms

        /// <summary>
        /// Velocity to acceleration factor.
        /// </summary>
        public double VelToAccFactor { get; set; } = 1.0;

        /// <summary>
        /// Velocity to smooth time factor.
        /// </summary>
        public double VelToSmoothTimeFactor { get; set; } = 1.0;

        public short HomeSearchMode { get; set; } = 11;

        public MoveDirection HomeSearchDirection { get; set; } = MoveDirection.Negative;

        public MoveDirection HomeSearchIndexDirection { get; set; } = MoveDirection.Positive;

        public EdgeCapture EdgeCaptureMode = EdgeCapture.Falling;

        public double TargetPosition { get; set; }

        public double HomeLimitSpeed { get; set; } = 10;

        public double HomeIndexSpeed { get; set; } = 10;

        public double MaxTravel { get; set; } = 800;

    }
}
