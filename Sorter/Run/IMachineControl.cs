namespace Sorter
{
    public interface IMachineControl
    {
        void SetSpeed(double speed = 1.0);
        void Setup();
        void Start();
        void Stop();
        void Pause();
        void Reset();
        void Estop();
        void Delay(int delayMs = 100);
    }
}