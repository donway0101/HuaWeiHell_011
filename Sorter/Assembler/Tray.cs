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
        public int RowCount { get; set; } = 4;
        public int ColumneCount { get; set; } = 12;
        public bool IsReady { get; set; }
        public Part CurrentPart { get; set; }
        public double TrayHeight { get; set; }

        public CapturePosition BaseCapturePosition { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="VStation.FindBaseUnloadPosition"/>
        public AxisOffset TrayInfo { get; set; } = new AxisOffset();

        public Tray()
        {

        }

        public Part GetNextPartForLoad(Part part)
        {
            //Assume 
            int xIndex,yIndex;
            xIndex = part.XIndex + 1;
            yIndex = part.YIndex;
            double x = part.CapturePos.XPosition + XOffset;
            double y = part.CapturePos.YPosition;

            //Go to next line.
            if (part.XIndex == ColumneCount-1)
            {
                //Go to next line, index change.
                xIndex = 0; yIndex = yIndex + 1;

                //Coordinate change.
                x = BaseCapturePosition.XPosition; y = y - YOffset;
            }

            if (part.YIndex == RowCount - 1)
            {
                IsReady = false;
                xIndex = 0;
                yIndex = 0;
            }

            CapturePosition newCapture = new CapturePosition()
            {
                CaptureId = part.CapturePos.CaptureId,
                XPosition = x,
                YPosition = y,
                ZPosition = BaseCapturePosition.ZPosition
            };

            Pose pose = new Pose()
            {
                Z = BaseCapturePosition.ZPosition,
            };

            return new Part()
            { CapturePos = newCapture, TargetPose=pose, XIndex = xIndex, YIndex = yIndex };
        }

        /// <summary>
        /// base part is at right top corner, 
        /// </summary>
        /// <param name="part"></param>
        /// <param name="TrayInfo"></param>
        /// <returns></returns>
        public Part GetNextPartForUnload(Part part)
        {
            //Index increase.
            int xIndex, yIndex;
            xIndex = part.XIndex + 1;
            yIndex = part.YIndex;

            // Go to next row.
            if (part.XIndex == ColumneCount -1)
            {
                xIndex = 0;
                yIndex = yIndex + 1;
            }

            //Coordinate of robot placement.
            double x = BaseCapturePosition.XPosition + xIndex * TrayInfo.XOffset1 + yIndex * TrayInfo.XOffset2;
            double y = BaseCapturePosition.YPosition + xIndex * TrayInfo.YOffset1 + yIndex * TrayInfo.YOffset2;

            // End of tray.
            if (part.YIndex == RowCount - 1)
            {
                IsReady = false;
                xIndex = 0;
                yIndex = 0;
            }

            return new Part()
            {
                //CapturePos = newCapture,
                XIndex = xIndex,
                YIndex = yIndex,
                TargetPose = new Pose()
                {
                    X = x,
                    Y = y,
                    RUnloadAngle = part.TargetPose.RUnloadAngle,
                    Z = TrayHeight,
                },
            };

        }

        /// <summary>
        /// For unload tray, not vision each time places.
        /// </summary>
        public void SetPoses()
        {
            throw new NotImplementedException();
        }

    }

    public class Part
    {
        public bool Empty { get; set; }

        /// <summary>
        /// Tray hole index.
        /// </summary>
        public int XIndex { get; set; }

        /// <summary>
        /// Tray hole index.
        /// </summary>
        public int YIndex { get; set; }

        /// <summary>
        /// Bottom camera compensation.
        /// </summary>
        public double XOffset { get; set; }

        /// <summary>
        /// Bottom camera compensation.
        /// </summary>
        public double YOffset { get; set; }
        public double PickHeight { get; set; }
        public double PlaceHeight { get; set; }
        public double UnloadHeight { get; set; }

        public CapturePosition CapturePos { get; set; }

        /// <summary>
        /// For robot.
        /// </summary>
        public Pose TargetPose { get; set; }

    }

    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
    }


}
