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
        CameraDisconnected = 40001,
        ControllerConnectFail = 40002,

    }
}
