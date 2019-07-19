using Bp.Mes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorter
{
    public class Tray
    {
        /// <summary>
        /// Tray hole offset
        /// </summary>
        public double XOffset { get; set; } = 18.5;
        public double YOffset { get; set; } = 18.5;
        public int RowCount { get; set; } = 5;
        public int ColumneCount { get; set; } = 5;
        public Part CurrentPart { get; set; }

        public CapturePosition BaseCapturePosition { get; set; }

        public Tray()
        {

        }

        public void Setup()
        {
            //set tray capture position.

        }

        public Part GetNextPart(Part part)
        {
            int xIndex,yIndex;
            xIndex = part.XIndex + 1;
            yIndex = part.YIndex;
            double x = part.CapturePos.XPosition + XOffset;
            double y = part.CapturePos.YPosition;

            if (part.XIndex == ColumneCount-1)
            {
                xIndex = 0;
                yIndex = yIndex + 1;
                x = BaseCapturePosition.XPosition;
                y = y + YOffset;
            }

            if (part.YIndex == RowCount - 1)
            {
                //Finished.
            }

            CapturePosition newCapture = new CapturePosition()
            {
                CaptureId = part.CapturePos.CaptureId,
                XPosition = x,
                YPosition = y,
                ZPosition = BaseCapturePosition.ZPosition
            };

            return new Part()
            { CapturePos = newCapture, XIndex = xIndex, YIndex = yIndex };
        }

    }

    public class Part
    {
        public int XIndex { get; set; }
        public int YIndex { get; set; }
        public double PickHeight { get; set; }
        public double PlaceHeight { get; set; }
        public double UnloadHeight { get; set; }

        public CapturePosition CapturePos { get; set; }

    }

    public struct Coordinate
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
