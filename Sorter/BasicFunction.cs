using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sorter
{
    public class BasicFunction
    {
        public CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public Task<WaitBlock> TaskTest()
        {
            return Task.Run(async () =>
            {
                int code = 0;
                //try
                //{
                    await Task.Delay(10000, cancellationTokenSource.Token);
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    code = 1;
                    await Task.Delay(10000, cancellationTokenSource.Token);
                    code = 2;
                    return new WaitBlock() { Code = code};
                //}
                //catch (Exception)
                //{
                //    return new WaitBlock() { Code = code};
                //}
            }, cancellationTokenSource.Token);
        }
    }

    public class WaitBlock
    {
        public int Code { get; set; } = 0;

        public string Message { get; set; } = "OK";
    }

}
