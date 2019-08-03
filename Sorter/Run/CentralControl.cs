using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bp.Mes;
using System.IO.Ports;

namespace Sorter
{
    public partial class CentralControl : IMachineControl
    {
        private bool _checkVacuum = false;
        
        private bool[,] _startupSteps = new bool[10,10];

        private int _currentCycleId { get; set; } = 1;

        public List<CapturePosition> CapturePositions;
        public List<CapturePosition> UserOffsets;
        public List<CapturePosition> DevelopPoints;
        public ConfigManager UserConfigs;

        public PressureSensor PressureSensorGlueLine;
        public PressureSensor PressureSensorGluePoint;
        public LaserSensor LaserSensorGlueLine;
        public LaserSensor LaserSensorGluePoint;

        public MotionController Mc;

        public RoundTable WorkTable;
        public LStation LRobot;
        public VStation VRobot;
        public UVLight UVLight;
        public GlueLineStation GlueLineRobot;
        public GluePointStation GluePointRobot;

        public TrayStation VLoadStation;
        public TrayStation VUnloadStation;
        public TrayStation LLoadStation;
        public TrayStation LUnloadStation;

        public VisionServer Vision;

        public bool KeepThisShitRunning { get; set; }

        public int UvBottomDelay { get; set; } = 4;
        public int UvTopDelay { get; set; } = 5;

        public ManualResetEvent VLoadTrayEmptyManualResetEvent = new ManualResetEvent(true);

        public ManualResetEvent VUnloadTrayFullManualResetEvent = new ManualResetEvent(true);

        public RunningState _runningState = RunningState.None;

        public CentralControl()
        {
        }

        public void Setup()
        {
            LaserSensorGlueLine = new LaserSensor("COM4", 9600, 2);
            LaserSensorGluePoint = new LaserSensor("COM3", 9600, 1);

            PressureSensorGlueLine = new PressureSensor("COM6", 9600, 2);
            PressureSensorGluePoint = new PressureSensor("COM5", 9600, 1);

            LaserSensorGluePoint.Start();
            LaserSensorGlueLine.Start();
            PressureSensorGluePoint.Start();
            PressureSensorGlueLine.Start();

            Mc = new MotionController();
            Mc.Connect();
            Mc.Setup();
            Mc.ZeroAllPositions();

            Vision = new VisionServer(Mc);

            WorkTable = new RoundTable(Mc);
            WorkTable.Setup();

            UVLight = new UVLight(Mc, WorkTable);

            UserConfigs = new ConfigManager(this);
            LoadCapturePositions();
            LoadUserOffsets();
            LoadDevelopPoints();

            LRobot = new LStation(Mc, Vision, WorkTable, CapturePositions, UserOffsets);
            LRobot.Setup();

            VRobot = new VStation(Mc, Vision, WorkTable, CapturePositions, UserOffsets);
            VRobot.Setup();

            GlueLineRobot = new GlueLineStation(Mc, Vision, WorkTable, CoordinateId.GlueLine,
                CapturePositions, UserOffsets,
                PressureSensorGlueLine, LaserSensorGlueLine);
            GlueLineRobot.Setup();

            GluePointRobot = new GluePointStation(Mc, Vision, WorkTable, CoordinateId.GluePoint,
                CapturePositions, UserOffsets,
                PressureSensorGluePoint, LaserSensorGluePoint);
            GluePointRobot.Setup();

            VRobot.CheckVacuumValue = _checkVacuum;
            LRobot.CheckVacuumValue = _checkVacuum;            
        }

        public void SetSpeed(double speed=10)
        {
            LRobot.SetSpeed(speed);
            VRobot.SetSpeed(speed);
            GlueLineRobot.SetSpeed(speed);
            GluePointRobot.SetSpeed(speed);
            WorkTable.SetSpeed(speed);
        }

        /// <summary>
        /// Home motors.
        /// </summary>
        public void Start(double homeSpeed)
        {
            PressureSensorGluePoint.Test();
            PressureSensorGlueLine.Test();
            LaserSensorGlueLine.Test();
            LaserSensorGluePoint.Test();
            Mc.ClearAllFault();

            Mc.HomeAllMotors(homeSpeed, false);

            Mc.DisableAllLimits();
        }

        public CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            LRobot.Reset();
            VRobot.Reset();
        }

        public void Estop()
        {
            throw new NotImplementedException();
        }

        public void Delay(int delayMs)
        {
            throw new NotImplementedException();
        }

        public void LoadCapturePositions(string defaultConfigName = "CapturePositions.config")
        {
            var str = Helper.ReadFile(defaultConfigName);
            CapturePositions = Helper.ConvertToCapturePositions(str);
        }

        public void LoadUserOffsets(string defaultConfigName = "UserOffsets.config")
        {
            var str = Helper.ReadFile(defaultConfigName);
            UserOffsets = Helper.ConvertToCapturePositions(str);
        }

        public void LoadDevelopPoints(string defaultConfigName = "Development.config")
        {
            var str = Helper.ReadFile(defaultConfigName);
            DevelopPoints = Helper.ConvertToCapturePositions(str);
        }

        public CapturePosition GetDevelopPoints(string tag)
        {
            return Helper.GetDevelopmentPoints(DevelopPoints, tag);
        }

        public void SaveCapturePositions(string defaultConfigName = "CapturePositions.config")
        {
            var config = Helper.ConvertToJsonString(CapturePositions);
            Helper.WriteFile(config, config);
        }
    }

    public enum RunningState
    {
        None,
        Start,
        Stop,
        Pause,
    }

    /// <summary>
    /// Used as table fixture index, so do not change enum value.
    /// </summary>
    public enum StationId
    {
        V = 0,
        GluePoint = 1,
        GlueLine = 2,
        L = 3,
        Reserved = 4,
        UV = 5,
    }
}
