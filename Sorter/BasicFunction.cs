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
                    return new WaitBlock() { };
                //}
                //catch (Exception)
                //{
                //    return new WaitBlock() { Code = code};
                //}
            }, cancellationTokenSource.Token);
        }

        public Task<WaitBlock> TaskTemplate()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(10000, cancellationTokenSource.Token);
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    await Task.Delay(10000, cancellationTokenSource.Token);
                    return new WaitBlock() { Code = 0 };
                }
                catch (Exception)
                {
                    return new WaitBlock() { Code = ErrorCode.TobeCompleted, };
                }
            }, cancellationTokenSource.Token);
        }

    }

    public class WaitBlock
    {
        public ErrorCode Code { get; set; } = ErrorCode.Sucessful;

        public string Message { get; set; } = "To be complete error message.";
    }

}
