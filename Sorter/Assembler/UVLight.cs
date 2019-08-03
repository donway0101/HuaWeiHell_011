using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sorter
{
    public class UVLight : IMachineControl
    {
        private readonly MotionController _mc;
        private readonly RoundTable _table;

        public int UvDelaySec { get; set; } = 5;

        public int CurrentCycleId { get; set; }

        public UVLight(MotionController controller, RoundTable table)
        {
            _mc = controller;
            _table = table;
        }

        public async Task<WaitBlock> OnAsync(int delayMs = 1000)
        {
            return await Task.Run(() =>
            {
                try
                {
                    On();
                    Thread.Sleep(delayMs);
                    Off();
                    return new WaitBlock() { Message = "UV Light Finished Successful." };
                }
                catch (Exception)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "UV Light Finished Successful." };
                }
            });
        }

        public void On(int delaySec = 5)
        {
            On();
            Thread.Sleep(delaySec * 1000);
            Off();
        }

        public void On()
        {
            _mc.SetOutput(Output.UVLightTable, OutputState.On);
        }

        public void Off()
        {
            _mc.SetOutput(Output.UVLightTable, OutputState.Off);
        }

        public void SetSpeed(double speed = 1)
        {
            throw new NotImplementedException();
        }

        public void Setup()
        {
            throw new NotImplementedException();
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
            Thread.Sleep(delayMs);
        }

        public async Task<WaitBlock> WorkAsync(int cycleId)
        {
            return await Task.Run(() =>
            {
                #region Skip work
                if (_table.Fixtures[(int)StationId.UV].NG 
                || _table.Fixtures[(int)StationId.UV].IsEmpty || CurrentCycleId >= cycleId)
                {
                    CurrentCycleId = cycleId;
                    return new WaitBlock()
                    {
                        Message = "UV WorkAsync Skip due to previous station fail or is empty."
                    };
                }
                #endregion

                try
                {
                    On(UvDelaySec);
                    CurrentCycleId = cycleId;
                    return new WaitBlock()
                    {
                        Message = "UV WorkAsync."
                    };
                }
                catch (Exception ex)
                {
                    return new WaitBlock()
                    {
                        Code = ErrorCode.TobeCompleted,
                        Message = "UV WorkAsync fails." + ex.Message,
                    };
                }  
            });
        }
    }
}
