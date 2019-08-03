namespace Sorter
{
    public interface ITrayStation
    {
        int TrayLayerNumber { get; set; }
        int CurrentTrayLayerIndex { get; set; }
        double TrayLayerHeight { get; set; }
        Motor MotorTray { get; set; }
        Motor MotorConveyor { get; set; }


        void Home();
        void ConveyorIn();
        void ConveyorOut();
        void LockTray();
        void UnlockTray();
        bool GetInsideOpticalSensor();
        bool GetOutsideOpticalSensor();
        void RiseOneLayer();
        void DescendOneLayer();
        void RiseToTopLayer();
        void DescendToBottomLayer();
        void PushIn();
        void PushOut();
    }
}