using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bp.Mes
{

    /// <summary>
    /// In file "CapturePosition.config ", these points are fix points,
    /// in "UserOffsets.config", it's some data use both by operator and program.
    /// </summary>
    public enum CaptureId
    {   
        None = 0,
        /// <summary>
        /// L抓取
        /// </summary>
        LTrayPickTop = 1,
        /// <summary>
        /// L矫正
        /// </summary>
        LLoadCompensationBottom = 2,
        /// <summary>
        /// L装配
        /// </summary>
        LLoadHolderTop = 3,

        /// <summary>
        /// V抓取
        /// </summary>
        VTrayPickTop = 4,
        /// <summary>
        /// V矫正
        /// </summary>
        VLoadCompensationBottom = 5,
        /// <summary>
        /// V装配
        /// </summary>
        VLoadHolderTop = 6,

        /// <summary>
        /// 成品抓取
        /// </summary>
        VUnloadHolderTop = 7,
        /// <summary>
        /// 成品矫正
        /// </summary>
        VUnloadCompensationBottom = 8,
        /// <summary>
        /// 成品装配
        /// </summary>
        VTrayPlaceTop = 9,

        /// <summary>
        /// Camera capture position on china plane.
        /// </summary>
        GlueLineChina = 10,

        /// <summary>
        /// To find laser target and glue targets.
        /// </summary>
        GlueLineBeforeGlue=11,

        /// <summary>
        /// Check glue result.
        /// </summary>
        GlueLineAfterGlue=12,

        /// <summary>
        /// Camera capture position on china plane.
        /// </summary>
        GluePointChina = 13,

        /// <summary>
        /// To find laser target and glue targets.
        /// </summary>
        GluePointBeforeGlue = 14,

        /// <summary>
        /// Check glue result.
        /// </summary>
        GluePointAfterGlue = 15,



        /// <summary>
        /// Where to clean the needle.
        /// </summary>
        GluePointCleanNeedleSuck = 78,

        /// <summary>
        /// Where to clean the needle.
        /// </summary>
        GlueLineCleanNeedleSuck = 79,

        /// <summary>
        /// laser height detect at china point.
        /// </summary>
        GluePointLaserOnCalibrationChina = 80,

        /// <summary>
        /// laser height detect at china point.
        /// </summary>
        GlueLineLaserOnCalibrationChina = 81,

        /// <summary>
        /// Especially for L assembly.
        /// </summary>
        LAssemblyOffset = 82,

        /// <summary>
        /// Stand by for next cycle.
        /// </summary>
        LLoadHolderTopReady = 83,

        /// <summary>
        /// Stand by for next cycle.
        /// </summary>
        VUnloadHolderTopReady = 84,

        /// <summary>
        /// Where to clean the needle.
        /// </summary>
        GluePointCleanNeedleShot = 85,

        /// <summary>
        /// Where to clean the needle.
        /// </summary>
        GlueLineCleanNeedleShot = 86,

        /// <summary>
        /// Needle Calibrate at china point.
        /// </summary>
        GluePointNeedleOnCalibrationChina = 87,

        /// <summary>
        /// Needle Calibrate at china point.
        /// </summary>
        GlueLineNeedleOnCalibrationChina = 88,

        /// <summary>
        /// Laser move to the same point where needle height was found, 
        /// and calculate zero reference position of laser to needle.
        /// </summary>
        GluePointLaserOnPressureSensor = 89,

        /// <summary>
        /// Laser move to the same point where needle height was found, 
        /// and calculate zero reference position of laser to needle.
        /// </summary>
        GlueLineLaserOnPressureSensor = 90,

        /// <summary>
        /// Needle touch on pressure sensor, To capture needle height.
        /// </summary>
        GluePointNeedleOnPressureSensor = 91,

        /// <summary>
        /// Needle touch on pressure sensor, To capture needle height.
        /// </summary>
        GlueLineNeedleOnPressureSensor = 92,

        /// <summary>
        /// Points for development, for testing.
        /// </summary>
        GluePointDevelopment = 93,

        /// <summary>
        /// Points for development, for testing.
        /// </summary>
        GlueLineDevelopment = 94,

        /// <summary>
        /// Points for development, for testing.
        /// </summary>
        VDevelopment = 95,

        /// <summary>
        /// Points for development, for testing.
        /// </summary>
        LDevelopment = 96,

        /// <summary>
        ///  Bin position of L station when vision or sucker exception happens.
        /// </summary>
        LBin = 99,

        /// <summary>
        /// Bin position of V station when vision or sucker exception happens.
        /// </summary>
        VBin=100,
    }

    /// <summary>
    /// Where to take picture.
    /// </summary>
    public class CapturePosition : PackBase
    {
        /// <summary>
        /// 拍照点
        /// </summary>
        public CaptureId CaptureId { get; set; } = CaptureId.LTrayPickTop;
        public double XPosition { get; set; }
        public double YPosition { get; set; }
        public double ZPosition { get; set; }
        public double Angle { get; set; }
        /// <summary>
        /// ="1", First capture, ="2", second capture, ="3" third tag
        /// </summary>
        public string Tag { get; set; } = "0";

        public string Remarks { get; set; } = "Default remarks";
    }


    /// <summary>
    /// 抓拍结果（相对位移）
    /// </summary>
    public class AxisOffset : PackBase
    {
        /// <summary>
        /// 拍照点
        /// </summary>
        public CaptureId CaptureId { get; set; } = CaptureId.LTrayPickTop;
        public double XOffset { get; set; } = 1.0;
        public double YOffset { get; set; } = 1.0;
        public double XOffset1 { get; set; } = 1.0;
        public double YOffset1 { get; set; } = 1.0;
        public double XOffset2 { get; set; } = 1.0;
        public double YOffset2 { get; set; } = 1.0;
        public double ROffset { get; set; } = 1.0;

        /// <summary>
        /// For points: each x,y is a point
        /// For lines: each two point is a line, from start to end.
        /// For circle: first start point, second end point, third center point.
        /// </summary>
        public double[] StartPointX { get; set; }
        public double[] StartPointY { get; set; }
        public double[] PreClosePointX { get; set; }
        public double[] PreClosePointY { get; set; }
        public double[] EndPointX { get; set; }
        public double[] EndPointY { get; set; }
        public double[] CenterPointX { get; set; }
        public double[] CenterPointY { get; set; }

        public double[] Group1PointX { get; set; }
        public double[] Group1PointY { get; set; }
        public double[] Group2PointX { get; set; }
        public double[] Group2PointY { get; set; }
        public double[] Group3PointX { get; set; }
        public double[] Group3PointY { get; set; }
        public double[] Group4PointX { get; set; }
        public double[] Group4PointY { get; set; }

        /// <summary>
        /// Laser guided target position for height laser sensor.
        /// </summary>
        public double[] LaserX { get; set; }
        public double[] LaserY { get; set; }

        public bool ResultOK { get; set; }

        /// <summary>
        /// V station has part on fixture.
        /// </summary>
        public bool ObjectDetected { get; set; }

    }


    /// <summary>
    /// Current axis position.
    /// </summary>
    public class AxisPosition : PackBase
    {
        /// <summary>
        /// 拍照点
        /// </summary>
        public CaptureId CaptureId { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double R { get; set; }
    }

    public enum CommandType
    {
        CapturePosition,
        AxisPosition,
        AxisOffset,
    }

}
