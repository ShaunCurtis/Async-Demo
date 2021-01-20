using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Async_Demo
{
    public class PrimeTask : ILongRunningTask
    {
        Stopwatch watch = new Stopwatch();

        public Task RunAsync(long num, string message)
        {
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
            Console.WriteLine($"[{Thread.CurrentThread.Name}] - {message}...({watch.ElapsedMilliseconds} millisecs)");
            return Task.CompletedTask;
        }

        public Task RunYieldingAsync(long num, string message)
        {
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
            Console.WriteLine($"[{Thread.CurrentThread.Name}] - {message}...({watch.ElapsedMilliseconds} millisecs)");
            return Task.CompletedTask;
        }

        public void Run(long num, string message)
        {
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
            Console.WriteLine($"[{Thread.CurrentThread.Name}] - {message}...({watch.ElapsedMilliseconds} millisecs)");
        }
    }
}

