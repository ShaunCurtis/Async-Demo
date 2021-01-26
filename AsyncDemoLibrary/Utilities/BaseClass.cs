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

        public async Task RunLongTaskAsync(long num, string message)
        {
            this.Message = message ?? this.Message;
            var millisecs = await LongRunningTasks.RunLongProcessorTaskAsync(num);
            this.Log(millisecs);
        }

        public void RunLongTask(long num, string message)
        {
            this.Message = message ?? this.Message;
            var millisecs = LongRunningTasks.RunLongProcessorTask(num);
            this.Log(millisecs);
        }

        public async Task RunLongDelayTaskAsync(long num, string message)
        {
            this.Message = message ?? this.Message;
            var millisecs = await LongRunningTasks.RunLongIOTaskAsync(num);
            this.Log(millisecs);
        }

        private void Log(long millisecs) =>
            this.LogToUI($"{Message} ==> Completed in ({millisecs} millisecs)");
    }

}

