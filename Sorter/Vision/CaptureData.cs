using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bp.Mes;

namespace Sorter
{
    public static class CapturePositions
    {
        //    LTrayPickTop = 1,
        //    LLoadCompensationBottom=2,
        //    LLoadFixtureTop=3,

        //    VTrayPickTop = 4,
        //    VLoadCompensationBottom = 5,
        //    VLoadFixtureTop = 6,

        //    VUnloadFixtureTop = 7,
        //    VUnloadCompensationBottom = 8,
        //    VTrayPlaceTop = 9,

        public static CapturePosition VUnloadFixtureTop = new CapturePosition()
        {
            CaptureId = CaptureId.VUnloadHolderTop,
            XPosition = -20.015,
            YPosition = -272.759,
            ZPosition = -23.876,

        };
        public static CapturePosition VUnloadCompensationBottom = new CapturePosition()
        {
            CaptureId = CaptureId.VUnloadCompensationBottom,
            XPosition = 23.694,
            YPosition = -179.626,
            ZPosition = -7.386,
        };
        public static CapturePosition VTrayPlaceTop = new CapturePosition()
        {
            CaptureId = CaptureId.VTrayPlaceTop,
            XPosition = 156.178,
            YPosition = -25.312,
            ZPosition = -24.091,
        };

        public static CapturePosition VTrayPickTop = new CapturePosition()
        {
            CaptureId = CaptureId.VTrayPickTop,
            XPosition = 45.343,
            YPosition = -16.67,
            ZPosition = -25.471,
        };

        public static CapturePosition VLoadCompensationBottom = new CapturePosition()
        {
            CaptureId = CaptureId.VLoadCompensationBottom,
            XPosition =  24.002,
            YPosition = -223.848,
            ZPosition = -9.954,
        };
        public static CapturePosition VLoadFixtureTop = new CapturePosition()
        {
            CaptureId = CaptureId.VLoadHolderTop,
            XPosition = -20.228,
            YPosition = -272.827,
            ZPosition = -25.591,
        };

        public static CapturePosition LTrayPickTop = new CapturePosition()
        {
            CaptureId = CaptureId.LTrayPickTop,
            XPosition = -2.549,
            YPosition = 58.602,
            ZPosition = -28.674,
        };
        public static CapturePosition LLoadCompensationBottom = new CapturePosition()
        {
            CaptureId= CaptureId.LLoadCompensationBottom,
            XPosition = 52.724,
            YPosition = 176.225,
            ZPosition = -24.795,

        };
        public static CapturePosition LLoadFixtureTop = new CapturePosition()
        {
            CaptureId = CaptureId.LLoadHolderTop,
            XPosition = -10.218,
            YPosition = 255.591,
            ZPosition = -27.803,
        };
    }
}
