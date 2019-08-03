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
        /// <summary>
        /// Todo tray change
        /// </summary>
        /// <returns></returns>
        public async Task<WaitBlock> WorkAsync()
        {
            return await Task.Run(async () =>
            {
                string log = string.Empty;
                try
                {
                    
                    //Task<WaitBlock>[] blocks;

                    if (_currentCycleId == 0)
                    {
                        _currentCycleId = 1;
                    }

                    do
                    {
                        log = string.Empty;
                        //Stop production.
                        if (VRobot.StopProduction)
                        {
                            bool allEmpty = true;
                            foreach (var fixture in WorkTable.Fixtures)
                            {
                                if (fixture.IsEmpty==false)
                                {
                                    allEmpty = false;
                                }
                            }
                            if (allEmpty)
                            {
                                break;
                            }
                        }

                        //if (VRobot.LoadTray.CurrentPart.XIndex > VRobot.LoadTray.ColumneCount)
                        //{
                        //    VLoadTrayEmptyManualResetEvent.Reset();
                        //    VLoadTrayEmptyManualResetEvent.WaitOne();
                        //}

                        //blocks = new Task<WaitBlock>[2];

                        //blocks[0] = VRobot.WorkAsync();
                        //blocks[1] = LRobot.WorkAsync();
                        //blocks[2] = UVLight.WorkAsync();

                        //blocks[0] = VRobot.WorkAsync();
                        //blocks[0] = LRobot.WorkAsync();
                        //blocks[1] = UVLight.WorkAsync();

                        //await blocks[0];
                        //await blocks[1];
                        //Task.WaitAll(blocks);
                        //CheckTaskResults(blocks);
                        //Helper.CheckTaskResult(blocks[1]);

                        var vTask = VRobot.WorkAsync(_currentCycleId);
                        var gpTask = GluePointRobot.WorkAsync(_currentCycleId);
                        var glTask = GlueLineRobot.WorkAsync(_currentCycleId);
                        //var lTask = LRobot.WorkAsync(_currentCycleId);
                        //var uVTask = UVLight.WorkAsync(_currentCycleId);

                        await vTask;
                        await gpTask;
                        await glTask;
                        //await lTask;
                        //await uVTask;

                        log += vTask.Result.Message + Environment.NewLine;
                        log += gpTask.Result.Message + Environment.NewLine;
                        log += glTask.Result.Message + Environment.NewLine;
                        //log += lTask.Result.Message + Environment.NewLine;

                        Helper.CheckTaskResult(vTask);
                        Helper.CheckTaskResult(gpTask);
                        Helper.CheckTaskResult(glTask);
                        //Helper.CheckTaskResult(lTask);
                        //Helper.CheckTaskResult(uVTask);

                        var tableTask = WorkTable.TurnsAsync();
                        await tableTask;
                        Helper.CheckTaskResult(tableTask);

                        _currentCycleId++;

                    } while (KeepThisShitRunning);

                    return new WaitBlock() { Message = "Production stopped." };
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.TobeCompleted,
                        Message = "Production interruptted due to: " + ex.Message + " " + log,
                    };
                }
            });

        }

        public void CheckTaskResult(Task<WaitBlock> waitBlock)
        {
            Helper.CheckTaskResult(waitBlock);
        }

        public void CheckTaskResults(Task<WaitBlock>[] waitBlocks)
        {
            foreach (var block in waitBlocks)
            {
                if (block.Result.Code != ErrorCode.Sucessful)
                {
                    throw new Exception("Error Code: " + 
                        block.Result.Code + block.Result.Message);
                }
            }
        }

    }

}
