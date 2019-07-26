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
        
        private StartupLoadStep _currentStartupStep = StartupLoadStep.Step1;
       

        public List<CapturePosition> CapturePositions;
        public List<CapturePosition> UserOffsets;
        public List<CapturePosition> DevelopPoints;

        public PressureSensor PressureSensorGlueCurve;
        public PressureSensor PressureSensorGluePoint;
        public LaserSensor LaserSensorGlueCurve;
        public LaserSensor LaserSensorGluePoint;

        public MotionController Mc;

        public RoundTable WorkTable;
        public LStation LRobot;
        public VStation VRobot;
        public UVLight UVLight;
        public GlueCurveStation GlueCurveRobot;
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
            LaserSensorGlueCurve = new LaserSensor("COM3", 9600, 2);
            LaserSensorGluePoint = new LaserSensor("COM4", 9600, 1);

            //LaserSensorGluePoint.Start();
            //LaserSensorGlueCurve.Start();

            Mc = new MotionController();
            Mc.Connect();
            Mc.Setup();
            Mc.ZeroAllPositions();

            UVLight = new UVLight(Mc);
            Vision = new VisionServer(Mc);

            WorkTable = new RoundTable(Mc);
            WorkTable.Setup();

            LoadCapturePositions();
            LoadUserOffsets();
            LoadDevelopPoints();

            LRobot = new LStation(Mc, Vision, WorkTable, CapturePositions, UserOffsets);
            LRobot.Setup();

            VRobot = new VStation(Mc, Vision, WorkTable, CapturePositions, UserOffsets);
            VRobot.Setup();

            GlueCurveRobot = new GlueCurveStation(Mc, Vision, CoordinateId.GlueCurve,
                CapturePositions, UserOffsets,
                PressureSensorGlueCurve, LaserSensorGlueCurve);
            GlueCurveRobot.Setup();

            GluePointRobot = new GluePointStation(Mc, Vision, CoordinateId.GluePoint,
                CapturePositions, UserOffsets,
                PressureSensorGlueCurve, LaserSensorGlueCurve);
            GluePointRobot.Setup();

            VRobot.CheckVacuumValue = _checkVacuum;
            LRobot.CheckVacuumValue = _checkVacuum;
            
        }

        public void SetSpeed(double speed=10)
        {
            LRobot.SetSpeed(speed);
            VRobot.SetSpeed(speed);
            GlueCurveRobot.SetSpeed(speed);
            GluePointRobot.SetSpeed(speed);
            WorkTable.SetSpeed(speed);
        }

        /// <summary>
        /// Home motors.
        /// </summary>
        public void Start(double homeSpeed)
        {
            Mc.ClearAllFault();
            //Fast home to go near home sensor.
            Mc.HomeAllMotors(30, true);
            Mc.HomeAllMotors(homeSpeed, false);
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
            throw new NotImplementedException();
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
            var str = Helper.ReadConfiguration(defaultConfigName);
            CapturePositions = Helper.ConvertToCapturePositions(str);
        }

        public void LoadUserOffsets(string defaultConfigName = "UserOffsets.config")
        {
            var str = Helper.ReadConfiguration(defaultConfigName);
            UserOffsets = Helper.ConvertToCapturePositions(str);
        }

        public void LoadDevelopPoints(string defaultConfigName = "Development.config")
        {
            var str = Helper.ReadConfiguration(defaultConfigName);
            DevelopPoints = Helper.ConvertToCapturePositions(str);
        }

        public CapturePosition GetDevelopPoints(string tag)
        {
            return Helper.GetDevelopmentPoints(DevelopPoints, tag);
        }

        public void SaveCapturePositions(string defaultConfigName = "CapturePositions.config")
        {
            var config = Helper.ConvertToJsonString(CapturePositions);
            Helper.SaveConfiguration(config, config);
        }
    }

    public enum RunningState
    {
        None,
        Start,
        Stop,
        Pause,
    }
}
