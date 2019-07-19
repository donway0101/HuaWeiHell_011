using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bp.Mes;

namespace Sorter
{
    public class VisionCapturePosition
    {
        public CapturePosition[] CapturePositions { get; set; }
        public string DefaultConfigName { get; set; } = "CapturePositions.config";

        public VisionCapturePosition()
        {
            CapturePositions = new CapturePosition[9];

            //CapturePositions[0] = VUnloadHolderTop;
            //CapturePositions[1] = VUnloadCompensationBottom;
            //CapturePositions[2] = VTrayPlaceTop;
            //CapturePositions[3] = VTrayPickTop;
            //CapturePositions[4] = VLoadCompensationBottom;
            //CapturePositions[5] = VLoadHolderTop;
            //CapturePositions[6] = LTrayPickTop;
            //CapturePositions[7] = LLoadCompensationBottom;
            //CapturePositions[8] = LLoadHolderTop;
        }

        public void LoadCapturePositions()
        {
            var str = Helper.ReadConfiguration(DefaultConfigName);
            CapturePositions = Helper.ConvertConfigToCapturePositions(str);
        }

        public void SaveCapturePositions()
        {
            var config = Helper.ConvertObjectToString(CapturePositions);
            Helper.SaveConfiguration(config, DefaultConfigName);
        }

        //public static CapturePosition VUnloadHolderTop = new CapturePosition()
        //{
        //    CaptureId = CaptureId.VUnloadHolderTop,
        //    XPosition = -20.015,
        //    YPosition = -272.759,
        //    ZPosition = -23.876,

        //};
        //public static CapturePosition VUnloadCompensationBottom = new CapturePosition()
        //{
        //    CaptureId = CaptureId.VUnloadCompensationBottom,
        //    XPosition = 23.694,
        //    YPosition = -179.626,
        //    ZPosition = -7.386,
        //};
        //public static CapturePosition VTrayPlaceTop = new CapturePosition()
        //{
        //    CaptureId = CaptureId.VTrayPlaceTop,
        //    XPosition = 156.178,
        //    YPosition = -25.312,
        //    ZPosition = -24.091,
        //};

        //public static CapturePosition VTrayPickTop = new CapturePosition()
        //{
        //    CaptureId = CaptureId.VTrayPickTop,
        //    XPosition = 45.343,
        //    YPosition = -16.67,
        //    ZPosition = -25.471,
        //};
        //public static CapturePosition VLoadCompensationBottom = new CapturePosition()
        //{
        //    CaptureId = CaptureId.VLoadCompensationBottom,
        //    XPosition = 24.002,
        //    YPosition = -223.848,
        //    ZPosition = -9.954,
        //};
        //public static CapturePosition VLoadHolderTop = new CapturePosition()
        //{
        //    CaptureId = CaptureId.VLoadHolderTop,
        //    XPosition = -20.228,
        //    YPosition = -272.827,
        //    ZPosition = -25.591,
        //};

        //public static CapturePosition LTrayPickTop = new CapturePosition()
        //{
        //    CaptureId = CaptureId.LTrayPickTop,
        //    XPosition = -2.549,
        //    YPosition = 58.602,
        //    ZPosition = -28.674,
        //};
        //public static CapturePosition LLoadCompensationBottom = new CapturePosition()
        //{
        //    CaptureId = CaptureId.LLoadCompensationBottom,
        //    XPosition = 52.724,
        //    YPosition = 176.225,
        //    ZPosition = -24.795,

        //};
        //public static CapturePosition LLoadHolderTop = new CapturePosition()
        //{
        //    CaptureId = CaptureId.LLoadHolderTop,
        //    XPosition = -10.218,
        //    YPosition = 255.591,
        //    ZPosition = -27.803,
        //};

    }

    public struct Pose
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double RLoadAngle { get; set; }
        public double RUnloadAngle { get; set; }
    }
    
}
