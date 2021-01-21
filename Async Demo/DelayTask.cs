using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Async_Demo
{
    public class DelayTask : UILogger, ILongRunningTask
    {

        Stopwatch watch = new Stopwatch();

        public DelayTask(Action<string> uiLogger) {
            this.UIMessenger = uiLogger;
            this.CallerName = "Activity Task";
        }

        private string Message = "Prime Task";

        public async Task RunAsync(long num, string message)
        {
            watch.Reset();
            watch.Start();
            await Task.Delay((int)num * 1000);
            watch.Stop();
            this.Log();
        }

        public async Task RunYieldingAsync(long num, string message)
        {
            watch.Reset();
            watch.Start();
            await Task.Yield();
            await Task.Delay((int)num * 1000);
            watch.Stop();
            this.Log();
        }

        public void Run(long num, string message)
        {
            if (string.IsNullOrWhiteSpace(Thread.CurrentThread.Name))
                Thread.CurrentThread.Name = "Long Running Task Thread";
            watch.Reset();
            watch.Start();
            Task.Delay((int)num * 1000);
            watch.Stop();
            this.Log();
        }

        private void Log() =>
            this.LogToUI($"{Message} ==> Completed in ({watch.ElapsedMilliseconds} millisecs)");

    }
}

