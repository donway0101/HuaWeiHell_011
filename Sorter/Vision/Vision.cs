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
        public double ROffset { get; set; } = 1.0;
        public bool ResultOK { get; set; }

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
