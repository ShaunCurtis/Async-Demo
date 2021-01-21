using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Async_Demo
{
    public class PrimeTask : UILogger, ILongRunningTask
    {
        Stopwatch watch = new Stopwatch();

        public PrimeTask(Action<string> uiLogger) {
            this.UIMessenger = uiLogger;
            this.CallerName = "Activity Task";
        }

        private string Message = "Prime Task";

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

        private void Log() =>
            this.LogToUI($"{Message} ==> Completed in ({watch.ElapsedMilliseconds} millisecs)");
    }
}

