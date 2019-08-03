using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bp.Mes;

namespace Sorter
{
    /// <summary>
    /// Bottom camera capture fail three times.
    /// </summary>
    public class SuckerException : Exception
    {
        /// <summary>
        /// Used as a location ID.
        /// </summary>
        public CaptureId CaptureId { get; set; }

        public ActionType Type { get; set; }

        public SuckerException()
        {
        }

        public SuckerException(string message)
            : base(message)
        {
        }

        public SuckerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class VisionException : Exception
    {
        /// <summary>
        /// Used as a location ID.
        /// </summary>
        public CaptureId CaptureId { get; set; }

        public VisionException()
        {
        }

        public VisionException(string message)
            : base(message)
        {
        }

        public VisionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class LaserException : Exception
    {
        public LaserException()
        {
        }

        public LaserException(string message)
            : base(message)
        {
        }

        public LaserException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }


    /// <summary>
    /// Copy it change it, and leave it back.
    /// </summary>
    public class ExceptionTemplate : Exception
    {
        public ExceptionTemplate()
        {
        }

        public ExceptionTemplate(string message)
            : base(message)
        {
        }

        public ExceptionTemplate(string message, Exception inner)
            : base(message, inner)
        {
        }
    }


}
