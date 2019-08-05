using System;
using System.Collections.Generic;
using System.Threading;
using Bp.Mes;

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
        public List<GlueParameter> GlueParameters;
        public List<UserSetting> UserSettings;

        public SettingManager SettingManager;

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

        public VLoadTrayStation VLoadStation;
        public VUnloadTrayStation VUnloadStation;
        public LLoadTrayStation LLoadStation;
        public LUnloadTrayStation LUnloadStation;

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

            LoadCapturePositions();
            LoadUserOffsets();
            LoadDevelopPoints();
            LoadGlueParameters();
          
            LoadUserSettings();
            SettingManager = new SettingManager(this);

            LRobot = new LStation(Mc, Vision, WorkTable, CapturePositions, UserOffsets);
            LRobot.Setup();

            VLoadStation = new VLoadTrayStation(Mc);
            VLoadStation.Setup();         
            VUnloadStation = new VUnloadTrayStation(Mc);
            VUnloadStation.Setup();
            LLoadStation = new LLoadTrayStation(Mc);
            LLoadStation.Setup();
            LUnloadStation = new LUnloadTrayStation(Mc);
            LUnloadStation.Setup();

            VRobot = new VStation(Mc, Vision, WorkTable, VLoadStation, VUnloadStation, 
                CapturePositions, UserOffsets);
            VRobot.Setup();

            GlueLineRobot = new GlueLineStation(Mc, Vision, WorkTable, CoordinateId.GlueLine,
                GlueParameters, CapturePositions, UserOffsets,
                PressureSensorGlueLine, LaserSensorGlueLine);
            GlueLineRobot.Setup();

            GluePointRobot = new GluePointStation(Mc, Vision, WorkTable, CoordinateId.GluePoint,
                GlueParameters, CapturePositions, UserOffsets,
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

            VLoadStation.SetSpeed(speed);
            VUnloadStation.SetSpeed(speed);
            LLoadStation.SetSpeed(speed);
            LUnloadStation.SetSpeed(speed);
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

            HomeTrayMotors();
            ReadTrayStations();

            Mc.ClearAllFault();
            Mc.HomeAllMotors(homeSpeed);
            Mc.DisableAllLimits();
        }

        public void HomeTrayMotors()
        {
            VLoadStation.SetSpeed();
            VUnloadStation.SetSpeed();
            LLoadStation.SetSpeed();
            LUnloadStation.SetSpeed();

            VLoadStation.Home();
            VUnloadStation.Home();
            LLoadStation.Home();
            LUnloadStation.Home();

            VLoadStation.WaitTillHomeEnd();
            VUnloadStation.WaitTillHomeEnd();
            LLoadStation.WaitTillHomeEnd();
            LUnloadStation.WaitTillHomeEnd();         
        }

        public void ReadTrayStations()
        {
            LLoadStation.Ready();
            LUnloadStation.Ready();
            VLoadStation.Ready();
            VUnloadStation.Ready();

            LLoadStation.WaitTillReady();
            LUnloadStation.WaitTillReady();
            VLoadStation.WaitTillReady();
            VUnloadStation.WaitTillReady();
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

        public void LoadCapturePositions()
        {
            var str = Helper.ReadFile(Properties.Settings.Default.CapturePositions);
            CapturePositions = Helper.ConvertToCapturePositions(str);
        }

        public void LoadUserOffsets()
        {
            var str = Helper.ReadFile(Properties.Settings.Default.CapturePositionOffsets);
            UserOffsets = Helper.ConvertToCapturePositions(str);
        }

        public void LoadDevelopPoints()
        {
            var str = Helper.ReadFile(Properties.Settings.Default.DevelopmentPositions);
            DevelopPoints = Helper.ConvertToCapturePositions(str);
        }

        public void LoadGlueParameters()
        {
            var str = Helper.ReadFile(Properties.Settings.Default.GlueParameters);
            GlueParameters = Helper.ConvertToGlueParameters(str);
        }

        public void LoadUserSettings()
        {
            string jStr = Helper.ReadFile(Properties.Settings.Default.UserSettings);
            UserSettings = Helper.ConvertToUserSettings(jStr);
        }

        public CapturePosition GetDevelopPoints(string tag)
        {
            return Helper.GetDevelopmentPoints(DevelopPoints, tag);
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
