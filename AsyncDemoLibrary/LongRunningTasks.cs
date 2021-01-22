/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    public class LongRunningTasks : UILogger
    {
        Stopwatch watch = new Stopwatch();

        public LongRunningTasks(Action<string> uiLogger) {
            this.UIMessenger = uiLogger;
            this.CallerName = "Long Running Tasks";
        }

        private string Message = "Long Running Tasks";

        public Task RunAsync(long num, string message)
        {
            this.Message = message ?? this.Message;
            watch.Reset();
            watch.Start();
            var counter = 0;
            for (long i = 0; i <= (num * 10000); i++)
            {
                bool isPrime = true;
                for (long j = 2; j < i; j++)
                {
                    if (i % j == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
                if (isPrime)
                {
                    counter++;
                }
            }
            watch.Stop();
            this.Log();
            return Task.CompletedTask;
        }

        public Task RunYieldingAsync(long num, string message)
        {
            this.Message = message ?? this.Message;
            watch.Reset();
            watch.Start();
            var counter = 0;
            for (long i = 0; i <= (num * 10000); i++)
            {
                bool isPrime = true;
                for (long j = 2; j < i; j++)
                {
                    if (i % j == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
                if (isPrime)
                {
                    counter++;
                    Task.Yield();
                }
            }
            watch.Stop();
            this.Log();
            return Task.CompletedTask;
        }

        public void Run(long num, string message)
        {
            this.Message = message ?? this.Message;
            watch.Reset();
            watch.Start();
            var counter = 0;
            for (long i = 0; i <= (num * 10000); i++)
            {
                bool isPrime = true;
                for (long j = 2; j < i; j++)
                {
                    if (i % j == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
                if (isPrime)
                {
                    counter++;
                }
            }
            watch.Stop();
            this.Log();
        }

        public async Task RunDelayAsync(long num, string message)
        {
            watch.Reset();
            watch.Start();
            await Task.Delay((int)num * 1000);
            watch.Stop();
            this.Log();
        }

        private void Log() =>
            this.LogToUI($"{Message} ==> Completed in ({watch.ElapsedMilliseconds} millisecs)");
    }
}

