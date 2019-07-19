using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sorter
{
    public class CentralControl
    {
        public MotionController Mc;

        public RoundTable WorkTable;
        public AssemblyRobot VRobot;
        public AssemblyRobot LRobot;
        public GlueRobot GlueLineRobot;
        public GlueRobot GluePointRobot;
        public TrayStation VLoadStation;
        public TrayStation VUnloadStation;
        public TrayStation LLoadStation;
        public TrayStation LUnloadStation;
        public VisionServer Vision;

        public VisionCapturePosition CapturePositions;

        public CentralControl()
        {
            CapturePositions = new VisionCapturePosition();
        }

        public void Setup()
        {
            Mc = new MotionController();
            Mc.Connect();
            Mc.Setup();

            Vision = new VisionServer(Mc);
            WorkTable = new RoundTable(Mc);
            WorkTable.Setup();

            CapturePositions.LoadCapturePositions();
            LRobot = new AssemblyRobot(Mc, StationId.L, Vision, WorkTable, CapturePositions.CapturePositions);
            VRobot = new AssemblyRobot(Mc, StationId.V, Vision, WorkTable, CapturePositions.CapturePositions);
            LRobot.Setup();
            VRobot.Setup();
        }

        public void SetSpeed(double speed=10)
        {
            LRobot.SetSpeed(speed);
            VRobot.SetSpeed(speed);
            WorkTable.SetSpeed(speed);
        }

        /// <summary>
        /// Home motors.
        /// </summary>
        public void Start()
        {
            //Fast home to go near home sensor.
            Mc.HomeAllMotors(50, true);
            Mc.HomeAllMotors(3, true);
        }

        public CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Todo cancellation.
        /// </summary>
        /// <returns></returns>
        public Task<WaitBlock> VRobotTest()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(100, cancellationTokenSource.Token);
                    //cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    //code = 1;
                    //await Task.Delay(10000, cancellationTokenSource.Token);
                    //code = 2;

                    return new WaitBlock() { Code = 0 };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = 1, Message = ex.Message };
                }
            }, cancellationTokenSource.Token);
        }

        public Task<WaitBlock> LRobotTest()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(100, cancellationTokenSource.Token);
                    //cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    //code = 1;
                    //await Task.Delay(10000, cancellationTokenSource.Token);
                    //code = 2;

                    return new WaitBlock() { Code = 0 };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = 1, Message = ex.Message };
                }
            }, cancellationTokenSource.Token);
        }


        public Task<WaitBlock> GlueLineRobotTest()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(100, cancellationTokenSource.Token);
                    //cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    //code = 1;
                    //await Task.Delay(10000, cancellationTokenSource.Token);
                    //code = 2;
                    //GlueLineRobot.MoveToTray3(Sucker.Load);

                    return new WaitBlock() { Code = 0 };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = 1, Message = ex.Message };
                }
            }, cancellationTokenSource.Token);
        }

        public Task<WaitBlock> GluePointRobotTest()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(100, cancellationTokenSource.Token);
                    //cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    //code = 1;
                    //await Task.Delay(10000, cancellationTokenSource.Token);
                    //code = 2;
                    //GluePointRobot.MoveToTray4(Sucker.Load);

                    return new WaitBlock() { Code = 0 };
                }
                catch (Exception ex)
                {
                    return new WaitBlock() { Code = 1, Message = ex.Message };
                }
            }, cancellationTokenSource.Token);
        }

    }
}
