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

        public UVLight(MotionController controller)
        {
            _mc = controller;
        }

        public async Task<WaitBlock> OnAsync(int delayMs = 1000)
        {
            return await Task.Run(async () => {
                try
                {
                    On();
                    await Task.Delay(delayMs * 1000);
                    Off();
                    return new WaitBlock() { Message = "UV Light Finished Successful." };
                }
                catch (Exception)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, Message = "UV Light Finished Successful." };
                } 
            }); 
        }

        public void On()
        {
            _mc.SetOutput(Output.UVLightTop, OutputState.On);
        }

        public void Off()
        {
            _mc.SetOutput(Output.UVLightTop, OutputState.Off);
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
    }
}
