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
        VY = 209,
        VX = 210,
        VZ = 105,
        VRotateLoad = 211,
        VRotateUnload = 212,
        VTrayLoad = 101,
        VTrayUnload = 104,

        GlueLineY = 202,
        GlueLineX = 201,
        GlueLineZ = 203,

        GluePointY = 205,
        GluePointX = 204,
        GluePointZ = 206,

        LY = 207,
        LX = 208,
        LZ = 106,
        LRotateLoad = 107,
        LTrayLoad = 108,
        LTrayUnload = 110,
        WorkTable = 111,

        VConveyorUnload = 103,
        VConveyorLoad = 102,

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
        GlueCurve = 2,
    }

    public class PointPulse
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int ArcCenterToXOffset { get; set; }
        public int ArcCenterToYOffset { get; set; }
    }

    public enum MoveModeAMotor
    {
        None,
        Abs = 1,
        Relative = 2,
    }

}
