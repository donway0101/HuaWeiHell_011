using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorter
{
    public enum Input
    {
        None = 0,

        //Core 1, pin 01
        VSucker1Vacuum = 101,

        //Bigger than core2 32bit, user GetDiEx()

       // StartButton,
       // StopButton
       // ResetButton
       // EStopButton

        VLoadTrayOpticalSensor = 000,    
        VLoadConveyorOutsideOpticalSensor = 000,
        VLoadConveyorInsideOpticalSensor = 000,
        VUnloadTrayOpticalSensor = 000,
        VUnloadConveyorOutsideOpticalSensor = 000,
        VUnloadConveyorInsideOpticalSensor = 000,

        LLoadTrayOpticalSensor = 000,
        LLoadConveyorOutsideOpticalSensor = 000,
        LLoadConveyorInsideOpticalSensor = 000,
        LUnloadTrayOpticalSensor = 000,
        LUnloadConveyorOutsideOpticalSensor = 000,
        LUnloadConveyorInsideOpticalSensor = 000,

        LLoadTrayCylinderOut = 125,
        LLoadTrayCylinderIn = 126,
        LLoadConveyorCylinderOut = 000,
        LLoadConveyorCylinderIn = 000,
        VLoadTrayCylinderOut = 000,
        VLoadTrayCylinderIn = 000,
        VLoadConveyorCylinderOut = 000,
        VLoadConveyorCylinderIn = 000,

        LUnloadTrayCylinderOut = 000,
        LUnloadTrayCylinderIn = 000,
        LUnloadConveyorCylinderOut = 000,
        LUnloadConveyorCylinderIn = 000,
        VUnloadTrayCylinderOut = 000,
        VUnloadTrayCylinderIn = 000,
        VUnloadConveyorCylinderOut = 000,
        VUnloadConveyorCylinderIn = 000,

        VaccumLLoad = 211,
        VaccumVLoad = 209,
        VaccumVUnload = 210,

        VaccumHolerCenterV = 212,
        VaccumHolerCircleV = 119,

        VaccumHolerCenterGluePoint = 239,
        VaccumHolerCircleGluePoint = 238,

        VaccumHolerCenterGlueLine = 237,
        VaccumHolerCircleGlueLine = 236,

        VaccumHolerCenterL = 235,
        VaccumHolerCircleL = 234,

        VaccumHolerCenterReserve = 233,
        VaccumHolerCircleReserve = 222,

        VaccumHolerCenterUV = 221,
        VaccumHolerCircleUV = 220,

        LLoadConveyorBlockUpOut =129,
        LLoadConveyorBlockUpIn=130,
        LLoadConveyorTightUpOut=132,
        LLoadConveyorTightUpIn=131,
        LLoadConveyorTightPushOut=227,
        LLoadConveyorTightPushIn=228,

        VLoadConveyorTightUpOut = 111,
        VLoadConveyorTightUpIn = 110,
        VLoadConveyorTightPushOut = 223,
        VLoadConveyorTightPushIn = 224,

        VUnloadConveyorTightUpOut = 116,
        VUnloadConveyorTightUpIn = 115,
        VUnloadConveyorTightPushOut = 225,
        VUnloadConveyorTightPushIn = 226,

        VLoadHeadCylinderOut = 214,
        VLoadHeadCylinderIn = 213,

        VUnloadHeadCylinderOut = 216,
        VUnloadHeadCylinderIn = 215,
    }


    public enum Output
    {
        None = 0,

        //Core 1, pin 01
        LightBig = 223,

        UVLightTable = 224,
        UVLightHead = 208,

        LLoadTrayCylinder = 118,
        LLoadConveyorCylinder = 000,
        VLoadTrayCylinder = 000,
        VLoadConveyorCylinder = 000,

        LUnloadTrayCylinder = 000,
        LUnloadConveyorCylinder = 000,
        VUnloadTrayCylinder = 000,
        VUnloadConveyorCylinder = 000,

        LLoadConveyorBlockUp = 106,
        LLoadConveyorTightUp=119,
        LLoadConveyorTightPush = 117,

        VLoadConveyorTightUp = 107,
        VLoadConveyorTightPush = 108,

        VUnloadConveyorTightUp = 111,
        VUnloadConveyorTightPush = 109,

        VaccumLLoad =120,
        VaccumVLoad = 204,
        VaccumVUnload = 205,

        VaccumHolerCenterV = 212,
        VaccumHolerCircleV = 211,

        VaccumHolerCenterGluePoint = 222,
        VaccumHolerCircleGluePoint = 221,

        VaccumHolerCenterGlueLine = 220,
        VaccumHolerCircleGlueLine = 219,

        VaccumHolerCenterL = 218,
        VaccumHolerCircleL = 217,

        VaccumHolerCenterReserve = 216,
        VaccumHolerCircleReserve = 215,

        VaccumHolerCenterUV = 214,
        VaccumHolerCircleUV = 213,

        VLoadHeadCylinder = 113,
        VUnloadHeadCylinder = 114,

        GluePoint = 206,
        GlueLine = 207,

        GluePointClean = 116,
        GlueLineClean = 115,
    }
}
