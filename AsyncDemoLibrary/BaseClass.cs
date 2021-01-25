using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    public abstract class BaseClass
    {
        public Action<string> UIMessenger;

        protected string CallerName = "Application";

        protected Stopwatch watch = new Stopwatch();

        public void LogToUI(string message, string caller = null)
        {
            caller ??= this.CallerName;
            if (string.IsNullOrWhiteSpace(Thread.CurrentThread.Name))
                Thread.CurrentThread.Name = $"{caller} Thread";
            message = $"[{DateTime.Now.ToLongTimeString()}][{Thread.CurrentThread.Name}][{caller}] > {message}";
            UIMessenger?.Invoke(message);
        }

        public void WriteDirectToUI(string message) =>
            UIMessenger?.Invoke($"[{DateTime.Now.ToLongTimeString()}] {message}");

        public void LogThreadType(string caller = null)
        {
            caller ??= this.CallerName;
            var threadname = Thread.CurrentThread.IsThreadPoolThread ? "Threadpool Thread" : "Application Thread";
            this.LogToUI($" running on {threadname}", caller);
        }

        private string Message = "Long Running Tasks";

        public Task RunLongTaskAsync(long num, string message)
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

        public void RunLongTask(long num, string message)
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

        public async Task RunLongDelayTaskAsync(long num, string message)
        {
            this.Message = message ?? this.Message;
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

