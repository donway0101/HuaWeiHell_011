using System.Collections.Generic;

namespace Sorter
{
    public interface IMotionController
    {
        List<Motor> Motors { get; set; }

        void CheckDriverAlarm(Motor motor);
        void CheckEnabledAndNotMoving(Motor motor);
        void CheckSensor(Input sensor, bool state);
        void ClearFault(Motor motor, bool checkLimit = true);
        void Connect();
        void Disable(Motor motor);
        void Enable(Motor motor);
        void EnableAll();
        bool GetHomeSensor(Motor motor);
        bool GetInput(Input input);
        double GetReferencePosition(Motor motor);
        void Home(Motor motor);
        void HomeAllMotors(double speed = 10);
        bool IsDriverAlarmed(Motor motor);
        bool IsEnabled(Motor motor);
        bool IsHittingLimit(Motor motor);
        bool IsMoving(Motor motor);
        void Jog(Motor motor, double speed, MoveDirection direction);
        void MoveAwayFromLimit(Motor motor, double distance = 20);
        void MoveToTarget(Motor motor, double target);
        void MoveToTargetRelative(Motor motor, double target);
        void MoveToTargetRelativeTillEnd(Motor motor, double target);
        void MoveToTargetTillEnd(Motor motor, double target);
        void SetAcceleration();
        void SetCoordinateSystem();
        void SetDeceleration();
        void SetErrorBand(Motor motor);
        void SetJerk();
        void SetOutput(Output output, OutputState state);
        void SetPosition();
        void SetReferencePosition(Motor motor, double referencePos);
        void Setup();
        void SetVelocity();
        void Stop(Motor motor);
        void StopAllEmergency();
        void StopAllSoft();
    }
}