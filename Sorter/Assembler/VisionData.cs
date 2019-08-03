using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bp.Mes;

namespace Sorter
{
    public class Pose
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double A { get; set; }
        public double RUnloadAngle { get; set; }

        /// <summary>
        /// Offset is for unload tray locating.
        /// </summary>
        public double XOffset1 { get; set; }

        /// <summary>
        /// Offset is for unload tray locating.
        /// </summary>
        public double YOffset1 { get; set; }

        /// <summary>
        /// Offset is for unload tray locating.
        /// </summary>
        public double XOffset2 { get; set; }

        /// <summary>
        /// Offset is for unload tray locating.
        /// </summary>
        public double YOffset2 { get; set; }
    }
    
}
