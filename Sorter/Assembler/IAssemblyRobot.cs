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
        void UnloadAndLoad(Part unload, Part load);
        void Bin(ActionType type);
        void Sucker(VacuumState state);
        void Sucker(int retryTimes, ActionType action);

        Task<WaitBlock> LoadAsync(Part part);

        Task<WaitBlock> ChangeLoadTray { get; set; }
        Task<WaitBlock> ChangeUnloadTray { get; set; }
        Task<WaitBlock> ChangeLoadTrayAsync();
        Task<WaitBlock> ChangeUnloadTrayAsync();

        void SetNextPartLoad();
        void SetNextPartUnload();
        
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
        GlueLine,
        PrepareV
    }
}