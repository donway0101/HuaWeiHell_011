using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        /// <summary>
        /// Turn on light and turn off after a period.
        /// </summary>
        /// <param name="delayMs"></param>
        /// <returns></returns>
        public Task<WaitBlock> OnAsync(int delayMs)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1);
                    On(delayMs);
                    return new WaitBlock() { };
                }
                catch (Exception)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, };
                }
            });
        }


        public void On(int delayMs = 1000)
        {
            On();
            Delay(delayMs);
            Off();
        }

        public void On()
        {
            _mc.SetOutput(Output.UVLight, OutputState.On);
        }

        public void Off()
        {
            _mc.SetOutput(Output.UVLight, OutputState.Off);
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
            throw new NotImplementedException();
        }
    }
}
