namespace Sorter
{
    public interface ITrayStation
    {
        int TrayLayerNumber { get; set; }
        int CurrentTrayLayerIndex { get; set; }
        double TrayLayerHeight { get; set; }
        double BottomFirstLayerHeight { get; set; }
        double TraySpeed { get; set; }
        double ConveyorSpeed { get; set; }

        Motor MotorTray { get; set; }
        Motor MotorConveyor { get; set; }

        void Home();
        void LoadATray(int timeoutSec = 30);
        void UnloadATray(int timeoutSec = 30);
        void ConveyorIn(int timeoutSec);
        void ConveyorOut(int timeoutSec);
        void LockTray();
        void UnlockTray();
        bool GetInsideOpticalSensor();
        bool GetOutsideOpticalSensor();
        void RiseOneLayer();
        void DescendOneLayer();
        void RiseToTopLayer();
        void DescendToBottomLayer();
        void Ready();
        void WaitTillReady();
        void PushIn();
        void PushOut();
    }
}