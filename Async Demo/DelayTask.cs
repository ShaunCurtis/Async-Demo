using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Async_Demo
{
    public class DelayTask : ILongRunningTask
    {
        Stopwatch watch = new Stopwatch();

        public async Task RunAsync(long num, string message)
        {
            watch.Reset();
            watch.Start();
            await Task.Delay((int)num * 1000);
            watch.Stop();
            Console.WriteLine($"[{Thread.CurrentThread.Name}] - {message}...{watch.ElapsedMilliseconds} millisecs");
        }

        public async Task RunYieldingAsync(long num, string message)
        {
            watch.Reset();
            watch.Start();
            await Task.Yield();
            await Task.Delay((int)num * 1000);
            watch.Stop();
            Console.WriteLine($"[{Thread.CurrentThread.Name}] - {message}...{watch.ElapsedMilliseconds} millisecs");
        }

        public void Run(long num, string message)
        {
            watch.Reset();
            watch.Start();
            Task.Delay((int)num * 1000);
            watch.Stop();
            Console.WriteLine($"[{Thread.CurrentThread.Name}] - {message}...{watch.ElapsedMilliseconds} millisecs");
        }
    }
}

