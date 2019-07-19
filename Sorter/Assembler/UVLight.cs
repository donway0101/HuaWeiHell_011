using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorter
{
    public class UVLight
    {
        private readonly MotionController _controller;

        public UVLight(MotionController controller)
        {
            _controller = controller;
        }

        public void On()
        {
            _controller.SetOutput(Output.UVLight, OutputState.On);
        }

        public void Off()
        {
            _controller.SetOutput(Output.UVLight, OutputState.Off);
        }
    }
}
