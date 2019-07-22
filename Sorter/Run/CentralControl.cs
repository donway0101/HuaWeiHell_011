using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bp.Mes;

namespace Sorter
{
    public class CentralControl : IMachineControl
    {
        private bool _checkVacuum = false;

        public MotionController Mc;

        public RoundTable WorkTable;
        public LStation LRobot;
        public VStation VRobot;
        public UVLight UVLight;
        public GlueRobot GlueLineRobot;
        public GlueRobot GluePointRobot;

        public TrayStation VLoadStation;
        public TrayStation VUnloadStation;
        public TrayStation LLoadStation;
        public TrayStation LUnloadStation;
        public VisionServer Vision;

        public CapturePosition[] CapturePositions;

        public PressureSensor PressureSensorGLine;
        public PressureSensor PressureSensorGPoint;
        public LaserSensor LaserSensorGLine;
        public LaserSensor LaserSensorGPoint;

        public CentralControl()
        {
        }

        public void Setup()
        {
            //Serial communication.

            Mc = new MotionController();
            Mc.Connect();
            Mc.Setup();
            Mc.ZeroAllPositions();

            UVLight = new UVLight(Mc);
            Vision = new VisionServer(Mc);

            WorkTable = new RoundTable(Mc);
            WorkTable.Setup();

            LoadCapturePositions();

            LRobot = new LStation(Mc, Vision, WorkTable, CapturePositions, null);
            LRobot.Setup();

            VRobot = new VStation(Mc, Vision, WorkTable, CapturePositions, null);
            VRobot.Setup();

            GlueLineRobot = new GlueRobot(Mc, CoordinateId.GlueLine, 
                PressureSensorGLine, LaserSensorGLine);
            GlueLineRobot.Setup();

            GluePointRobot = new GlueRobot(Mc, CoordinateId.GluePoint, 
                PressureSensorGPoint, LaserSensorGPoint);
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
            Mc.ClearAllFault();
            //Fast home to go near home sensor.
            Mc.HomeAllMotors(30, true);
            Mc.HomeAllMotors(homeSpeed, false);
        }

        public CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Todo cancellation.
        /// </summary>
        /// <returns></returns>
        public Task<WaitBlock> StartupLoad()
        {
            return Task.Run(async () =>
            {
                try
                {
                    //List<Task<WaitBlock>> waitBlocks = new List<Task<WaitBlock>>();

                    await Task.Delay(1);
                    var vLoad1 = VRobot.LoadAsync(VRobot.LoadTray.CurrentPart);
                    await vLoad1;
                    //Todo check multiple results. show full error information.
                    CheckTaskResult(vLoad1);
                    WorkTable.Turns();

                    //Glue point work
                    var vLoad2 = VRobot.LoadAsync(VRobot.LoadTray.CurrentPart);
                    await vLoad2;
                    CheckTaskResult(vLoad2);
                    WorkTable.Turns();

                    var vLoad3 = VRobot.LoadAsync(VRobot.LoadTray.CurrentPart);
                    //Glue point work
                    //Glue line work
                    await vLoad3;
                    CheckTaskResult(vLoad3);
                    WorkTable.Turns();

                    var vLoad4 = VRobot.LoadAsync(VRobot.LoadTray.CurrentPart);
                    //Glue point work
                    //Glue line work
                    var lLoad1 = LRobot.LoadAsync(LRobot.LoadTray.CurrentPart);
                    await vLoad4;
                    await lLoad1;
                    CheckTaskResult(vLoad4);
                    CheckTaskResult(lLoad1);
                    WorkTable.Turns();

                    var vLoad5 = VRobot.LoadAsync(VRobot.LoadTray.CurrentPart);
                    //Glue point work
                    //Glue line work                    
                    var lLoad2 = LRobot.LoadAsync(LRobot.LoadTray.CurrentPart);
                    var uVLight = UVLight.OnAsync(1000);
                    await vLoad5;
                    await lLoad2;
                    await uVLight;

                    CheckTaskResult(vLoad5);
                    CheckTaskResult(lLoad2);
                    CheckTaskResult(uVLight);
                    WorkTable.Turns();

                    var vLoad6 = VRobot.LoadAsync(VRobot.LoadTray.CurrentPart);
                    //Glue point work
                    //Glue line work
                    var lLoad3 = LRobot.LoadAsync(LRobot.LoadTray.CurrentPart);
                    await vLoad6;
                    await lLoad3;
                    CheckTaskResult(vLoad6);
                    CheckTaskResult(lLoad3);

                    //Pick a part for next V work.

                    WorkTable.Turns();
                    // Do work now....
                    VRobot.PrepareForNextLoad(VRobot.LoadTray.CurrentPart);

                    return new WaitBlock() { Code = 0 };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = ex.Message };
                }
            }, cancellationTokenSource.Token);
        }

        public Task<WaitBlock> LStationTestRun()
        {
            return Task.Run(async () =>
            {
                try
                {
                    var lLoad1 = LRobot.LoadAsync(LRobot.LoadTray.CurrentPart);
                    await lLoad1;
                    CheckTaskResult(lLoad1);
                    //WorkTable.Turns();

                    return new WaitBlock() { Code = 0 };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = ex.Message };
                }
            });
        }

        public Task<WaitBlock> VStationTestRun()
        {
            return Task.Run(async () =>
            {
                try
                {
                    var vLoad = VRobot.LoadAsync(VRobot.LoadTray.CurrentPart);
                    await vLoad;
                    CheckTaskResult(vLoad);
                    //WorkTable.Turns();

                    return new WaitBlock() { Code = 0 };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = ex.Message };
                }
            });
        }

        public Task<WaitBlock> GluePointStationTestRun()
        {
            return Task.Run(async () =>
            {
                try
                {
                    //Get camera data.

                    GluePosition[] points = new GluePosition[5];

                    points[0] = new GluePosition() { X = -10, Y = 0 };
                    points[0] = new GluePosition() { X = -10, Y = 10 };
                    points[1] = new GluePosition() { X = -20, Y = 10 };
                    points[2] = new GluePosition() { X = -20, Y = 0 };
                    points[3] = new GluePosition() { X = -10, Y = 0 };
                    points[4] = new GluePosition() { X = -10, Y = 0 };

                    var gluePoint = GluePointRobot.Work(points);
                    await gluePoint;
                    CheckTaskResult(gluePoint);

                    return new WaitBlock() { Code = 0 };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = ex.Message };
                }
            });
        }

        public void CheckTaskResult(Task<WaitBlock> waitBlock)
        {
            Helper.CheckTaskResult(waitBlock);
        }

        public Task<WaitBlock> RunThisFuckingShit()
        {
            bool keepShitRunning = true;
            
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1);
                    var startup = StartupLoad();
                    await startup;
                    CheckTaskResult(startup);

                    do
                    {
                        try
                        {
                            var lRobotResult = LRobot.Work();
                            var vRobotResult = VRobot.Work();
                            var uvResult = UVLight.OnAsync(1000);

                            await lRobotResult;
                            await vRobotResult;
                            await uvResult;

                            CheckTaskResult(lRobotResult);
                            CheckTaskResult(vRobotResult);
                            CheckTaskResult(uvResult);

                            WorkTable.Turns();
                        }
                        catch (Exception exx)
                        {
                            throw new Exception(exx.Message);
                        }

                    } while (keepShitRunning);

                    return new WaitBlock() { Code = 0 };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = ex.Message };
                }
            });
        }

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
            CapturePositions = Helper.ConvertConfigToCapturePositions(str);
        }

        public void SaveCapturePositions(string defaultConfigName = "CapturePositions.config")
        {
            var config = Helper.ConvertObjectToString(CapturePositions);
            Helper.SaveConfiguration(config, config);
        }
    }
}
