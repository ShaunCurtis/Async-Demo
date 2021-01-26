using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    public static class LongRunningTasks
    {
        private static string ClassName = "LongRunningTasks";

        public static int Timermultiplier = 8;

        public async static Task<long> RunLongProcessorTaskAsync(long num, Action<string> uILogger, string name = "Processor Task")
        {
            UILogger.LogToUI(uILogger, $"{name} started", ClassName);
            var millisecs = await RunLongProcessorTaskAsync(num);
            UILogger.LogToUI(uILogger, $"{name} completed in {millisecs} millisecs", ClassName);
            return millisecs;
        }

        public async static Task<long> RunYieldingLongProcessorTaskAsync(long num, Action<string> uILogger, string name = "Processor Task")
        {
            UILogger.LogToUI(uILogger, $"{name} started", ClassName);
            var millisecs = await RunYieldingLongProcessorTaskAsync(num);
            UILogger.LogToUI(uILogger, $"{name} completed in {millisecs} millisecs", ClassName);
            return millisecs;
        }


        public static long RunLongProcessorTask(long num, Action<string> uILogger, string name = "Processor Task")
        {
            UILogger.LogToUI(uILogger, $"{name} started", ClassName);
            var millisecs = RunLongProcessorTask(num);
            UILogger.LogToUI(uILogger, $"{name} completed in {millisecs} millisecs", ClassName);
            return millisecs;
        }

        public async static Task<long> RunLongIOTaskAsync(long num, Action<string> uILogger, string name = "IO Task")
        {
            UILogger.LogToUI(uILogger, $"{name} started", ClassName);
            var millisecs = await RunLongIOTaskAsync(num);
            UILogger.LogToUI(uILogger, $"{name} completed in {millisecs} millisecs", ClassName);
            return millisecs;
        }


        public static Task<long> RunLongProcessorTaskAsync(long num)
        {
            var watch = new Stopwatch();

            num = num * Timermultiplier;
            watch.Start();
            var counter = 0;
            for (long x = 0; x <= num; x++)
            {
                for (long i = 0; i <= (10000); i++)
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
            }
            watch.Stop();
            return Task.FromResult(watch.ElapsedMilliseconds);
        }

        public async static Task<long> RunYieldingLongProcessorTaskAsync(long num)
        {
            var watch = new Stopwatch();

            num = num * Timermultiplier;
            watch.Start();
            var counter = 0;
            for (long x = 0; x <= num; x++)
            {
                for (long i = 0; i <= (10000); i++)
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
                        if (counter > 100)
                        {
                            // Console.WriteLine($"Thread ID:{Thread.CurrentThread.ManagedThreadId}");
                            await Task.Yield();
                            counter = 0;
                        }
                    }
                }
            }
            watch.Stop();
            return watch.ElapsedMilliseconds;
        }

        public static long RunLongProcessorTask(long num)
        {
            var watch = new Stopwatch();

            num = num * Timermultiplier;
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
            return watch.ElapsedMilliseconds;
        }

        public static async Task<long> RunLongIOTaskAsync(long num)
        {
            var watch = new Stopwatch();

            watch.Start();
            await Task.Delay((int)num * 1000);
            watch.Stop();
            return watch.ElapsedMilliseconds;
        }

    }
}
