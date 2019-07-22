using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sorter
{
    public class Error
    {
        public ErrorCode Code { get; set; }
        public string Message { get; set; }
        public string Remarks { get; set; }
    }

    public enum ErrorCode
    {
        Sucessful = 0,
        TobeCompleted = 44444,
        CameraDisconnected = 40001,
        ControllerConnectFail = 40002,
    }

    /// <summary>
    /// Bottom camera capture fail three times.
    /// </summary>
    public class NeedToPickAnotherPartException : Exception
    {
        public NeedToPickAnotherPartException()
        {
        }

        public NeedToPickAnotherPartException(string message)
            : base(message)
        {
        }

        public NeedToPickAnotherPartException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }


}
