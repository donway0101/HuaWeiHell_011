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
        LStationWorkFail = 40003,
        LStationPrepareFail = 40004,
        VStationWorkFail = 40005,
        VStationPrepareFail = 40006,
        RoundTableFail = 40007,
        UVFail = 40008,
        ShotGlueFail=40009,
        LockTrayFail=40010,
        CleanNeedleFail,
        GluePointPrepareFail,
        GlueLinePrepareFail,
        NeedleCalibrationFail,
        FindNeedleHeightFail,
        ProductionFail,
        LoadTrayFail,
        UnloadTrayFail,
        VisionFail,
        GluePointFail,
        GlueLineFail,
        FindUnloadTrayFail,
    }

}
