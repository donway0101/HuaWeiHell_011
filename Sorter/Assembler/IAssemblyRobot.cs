using Bp.Mes;
using System.Threading.Tasks;

namespace Sorter
{
    public interface IAssemblyRobot
    {      
        Tray UnloadTray { get; set; }
        Tray LoadTray { get; set; }
        double LoadTrayHeight { get; set; }
        double UnloadTrayHeight { get; set; }
        bool CheckVacuumValue { get; set; }     

        void Load(Part part);
        void Unload(Part part);
        void UnloadAndLoad(Part unload, Part load);
        void Sucker(VacuumState state);
        void Sucker(VacuumState state, int retryTimes, ActionType action);
        void Work();

        Task<WaitBlock> LoadAsync(Part part);
        Task<WaitBlock> PreparationForNextCycleAsync(Part part);

        void NgBin(Part part);
        void SetNextPartLoad();
        void SetNextPartUnload();
        void RiseZALittleAndDown();
        void MoveAngleMotor(double angle, MoveModeAMotor mode, ActionType type);

    }

    /// <summary>
    /// For conveyors, and sucker vacuum head.
    /// </summary>
    public enum ActionType
    {
        None,
        Load,
        Unload,
        UnloadAndLoad,
        GluePoint,
        GlueCurve,
        PrepareV
    }
}