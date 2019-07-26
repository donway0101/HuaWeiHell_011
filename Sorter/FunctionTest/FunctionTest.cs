using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sorter
{
    public partial class CentralControl : IMachineControl
    {      
        public async Task<WaitBlock> StartupLoadAsync(StartupLoadStep step)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    switch (step)
                    {
                        case StartupLoadStep.Step1:
                            var vLoad1 = VRobot.LoadAsync(VRobot.LoadTray.CurrentPart);
                            await vLoad1;
                            //Todo check multiple results. show full error information.
                            CheckTaskResult(vLoad1);
                            WorkTable.Turns();
                            break;

                        case StartupLoadStep.Step2:
                            var vLoad2 = VRobot.LoadAsync(VRobot.LoadTray.CurrentPart);
                            //Glue point work
                            await vLoad2;
                            //await glue point.
                            CheckTaskResult(vLoad2);
                            //check glue point result.
                            WorkTable.Turns();
                            break;

                        case StartupLoadStep.Step3:
                            var vLoad3 = VRobot.LoadAsync(VRobot.LoadTray.CurrentPart);
                            //Glue point work
                            //Glue curve work
                            await vLoad3;
                            //await glue point.
                            //await glue curve.
                            CheckTaskResult(vLoad3);
                            //check glue point result.
                            //check glue curve result.
                            WorkTable.Turns();
                            break;

                        case StartupLoadStep.Step4:
                            var vLoad4 = VRobot.LoadAsync(VRobot.LoadTray.CurrentPart);
                            //Glue point work
                            //Glue line work
                            var lLoad1 = LRobot.LoadAsync(LRobot.LoadTray.CurrentPart, UvBottomDelay);
                            await vLoad4;
                            //await glue point.
                            //await glue curve.
                            await lLoad1;
                            CheckTaskResult(vLoad4);
                            CheckTaskResult(lLoad1);
                            //check glue point result.
                            //check glue curve result.
                            WorkTable.Turns();
                            break;

                        case StartupLoadStep.Step5:
                            var vLoad5 = VRobot.LoadAsync(VRobot.LoadTray.CurrentPart);
                            //Glue point work
                            //Glue curve work                    
                            var lLoad2 = LRobot.LoadAsync(LRobot.LoadTray.CurrentPart, UvBottomDelay);
                            await vLoad5;
                            await lLoad2;
                            //await glue point.
                            //await glue curve.
                            CheckTaskResult(vLoad5);
                            CheckTaskResult(lLoad2);
                            //check glue point result.
                            //check glue curve result.
                            WorkTable.Turns();
                            break;

                        case StartupLoadStep.Step6:
                            var vLoad6 = VRobot.LoadAsync(VRobot.LoadTray.CurrentPart);
                            //Glue point work
                            //Glue curve work
                            var lLoad3 = LRobot.LoadAsync(LRobot.LoadTray.CurrentPart, UvBottomDelay);
                            var uVLight = UVLight.OnAsync(UvTopDelay);
                            await vLoad6;
                            await lLoad3;
                            //await glue point.
                            //await glue curve.
                            await uVLight;
                            CheckTaskResult(vLoad6);
                            CheckTaskResult(lLoad3);
                            //check glue point result.
                            //check glue curve result.
                            CheckTaskResult(uVLight);
                            WorkTable.Turns();
                            break;

                        case StartupLoadStep.Step7:
                            return new WaitBlock() { Message = "Startup Load Finished Successful." };

                        default:
                            break;
                    }
                    return new WaitBlock() { Message = "Startup Load Finished Successful." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.TobeCompleted,
                        Message = "Startup Load fail: " + ex.Message
                    };
                }
            });
        }

        public async Task<WaitBlock> StartupLoadAsync()
        {
            return await Task.Run(async () => 
            {
                try
                {
                    do
                    {
                        var task = StartupLoadAsync(_currentStartupStep);
                        await task;
                        CheckTaskResult(task);
                        _currentStartupStep ++;
                    } while ((int)_currentStartupStep < 7); 
                    
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.TobeCompleted,
                        Message = "Startup Load fail: " + ex.Message
                    };
                }

                return new WaitBlock() { Message = "Startup Load Finished Successful." };
            });

          
        }

        /// <summary>
        /// Keep assembling part.
        /// </summary>
        /// <returns></returns>
        public async Task<WaitBlock> Work()
        {
            return await Task.Run(async () =>
            {
                try
                {
                    do
                    {
                        if (VRobot.LoadTray.CurrentPart.XIndex > VRobot.LoadTray.ColumneCount)
                        {
                            VLoadTrayEmptyManualResetEvent.Reset();
                            VLoadTrayEmptyManualResetEvent.WaitOne();
                        }

                        var uVLight = UVLight.OnAsync(UvTopDelay);
                        var lLoad = LRobot.LoadAsync(LRobot.LoadTray.CurrentPart, UvBottomDelay);

                        var pre = VRobot.PrepareAsync();

                        await pre;
                        var vUnloadNLoad = VRobot.UnloadAndLoadAsync(VRobot.UnloadTray.CurrentPart,
                           VRobot.LoadTray.CurrentPart); ;
                       
                        await vUnloadNLoad;
                        await uVLight;
                        await lLoad;
                       
                        CheckTaskResult(pre);                       
                        CheckTaskResult(vUnloadNLoad);

                        CheckTaskResult(lLoad);
                        CheckTaskResult(uVLight);

                    } while (KeepThisShitRunning);

                    return new WaitBlock() { Message = "Work process stopped." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.TobeCompleted,
                        Message = "Work interrupted due to: " + ex.Message
                    };
                }
            });

        }

        public async Task<WaitBlock> VPrepareUnloadAndLoadAndPlace()
        {
            return await Task.Run(async () =>
            {
                try
                {
                    var pre = VRobot.PrepareAsync();
                    await pre;
                    CheckTaskResult(pre);
                    var uNl = VRobot.UnloadAndLoadAsync(VRobot.UnloadTray.CurrentPart,
                        VRobot.LoadTray.CurrentPart); ;
                    await uNl;
                    CheckTaskResult(uNl);

                    return new WaitBlock() { Message = "Startup Load Finished Successful." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.TobeCompleted,
                        Message = "Startup Load fail: " + ex.Message
                    };
                }               
            });

        }

        public Task<WaitBlock> GluePointStationTestRun()
        {
            return Task.Run(() =>
            {
                try
                {
                    //Get camera data.

                    //GluePosition[] points = new GluePosition[5];

                    //points[0] = new GluePosition() { X = -10, Y = 0 };
                    //points[0] = new GluePosition() { X = -10, Y = 10 };
                    //points[1] = new GluePosition() { X = -20, Y = 10 };
                    //points[2] = new GluePosition() { X = -20, Y = 0 };
                    //points[3] = new GluePosition() { X = -10, Y = 0 };
                    //points[4] = new GluePosition() { X = -10, Y = 0 };

                    //var gluePoint = GluePointRobot.Work(points);
                    //await gluePoint;
                    //CheckTaskResult(gluePoint);

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
                    //var startup = StartupLoad();
                    //await startup;
                    //CheckTaskResult(startup);

                    do
                    {
                        try
                        {
                            //var lRobotResult = LRobot.Work();
                            //var vRobotResult;// = VRobot.Work();
                            //var uvResult = UVLight.OnAsync(1000);

                            //await lRobotResult;
                            //await vRobotResult;
                            //await uvResult;

                            //CheckTaskResult(lRobotResult);
                            //CheckTaskResult(vRobotResult);
                            //CheckTaskResult(uvResult);

                            //WorkTable.Turns();
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
    }

    public enum StartupLoadStep
    {
        Step1 = 1,
        Step2 = 2,
        Step3 = 3,
        Step4 = 4,
        Step5 = 5,
        Step6 = 6,
        Step7 = 7,
    }
}
