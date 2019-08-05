using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTN;

namespace Sorter
{
    public struct CoreAndAxis
    {
        public short Core;
        public short Axis;
    }

    public struct InputCoreAndPin
    {
        public short Core;
        public short Pin;
    }

    public struct OutputCoreAndPin
    {
        public short Core;
        public short Pin;
    }


    public enum Axis
    {
        VTrayLoad = 101,
        VConveyorLoad = 102,
        VConveyorUnload = 103,
        LY = 104, 
        LX = 105, 
        LZ = 106,

        LRotateLoad = 107,
        LTrayLoad = 108,
        LConveyorLoad = 109,
        LTrayUnload = 110,
        WorkTable = 111,

        GlueLineX = 201,
        GlueLineY = 202,
        GlueLineZ = 203,
        GluePointY = 205,
        GluePointX = 204,
        GluePointZ = 206,

        VTrayUnload = 207,
        VZ = 208,
        VY = 209,
        VX = 210,      
        VRotateLoad = 211,
        VRotateUnload = 212,   
    }

    public enum MotorState
    {
        // Zero based, bit position.
        ServoAlarm = 1,
        FollowingError = 4,
        PositiveLimit = 5,
        NegativeLimit = 6,
        Enabled = 9,
        Moving = 10,
        InPosition = 11,
    }

    public enum MoveDirection
    {
        Negative = -1,
        Positive = 1,
    }

    public enum EdgeCapture
    {
        Falling = 0,
        Rising = 1,
    }

    public enum OutputState
    {
        On = 0,
        Off = 1,
    }

    public enum CoordinateId
    {
        None = 0,
        GluePoint = 1,
        GlueLine = 2,
    }

    /// <summary>
    /// Parameters that for controller a draw a circle.
    /// </summary>
    public class ArcInfoPulse
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public int ArcCenterToXStartOffset { get; set; }
        public int ArcCenterToYStartOffset { get; set; }

        public double Velocity { get; set; }
        public double Acceleration { get; set; }
    }

    public class PointInfoPulse
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public double Velocity { get; set; }
        public double Acceleration { get; set; }
    }

    public enum MoveModeAMotor
    {
        None,
        Abs = 1,
        Relative = 2,
    }

}
