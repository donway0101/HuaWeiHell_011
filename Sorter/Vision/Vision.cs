using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bp.Mes
{

    /// <summary>
    /// 抓拍点位
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

        GlueLineChina = 10,
        GlueLineBeforeGlue=11,
        GlueLineAfterGlue=12,
        GluePointChina = 13,
        GluePointBeforeGlue = 14,
        GluePointAfterGlue = 15,

        //Calibrate at china point.
        GluePointCalibration = 87,
        GlueCurveCalibration = 88,
        GluePointLaser = 89,
        GlueCurveLaser = 90,
        GluePointNeedle = 91,
        GlueCurveNeedle = 92,

        GluePointDevelopment =93,
        GlueCurveDevelopment=94,
        VDevelopment=95,
        LDevelopment = 96,

        /// <summary>
        /// To calculate needle to laser height offset.
        /// </summary>
        GluePointLaserSensor = 97,

        /// <summary>
        /// To capture needle height.
        /// </summary>
        GluePointPressureSensor =98,

        LBin =99,
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
        //??? affect json?
        public double Angle { get; set; }
        /// <summary>
        /// ="1", First capture, ="2", second capture, ="3" third tag
        /// </summary>
        public string Tag { get; set; } = "1";
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
        public double[] PointX { get; set; }
        public double[] PointY { get; set; }

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
