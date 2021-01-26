using System;
using System.Threading;

namespace AsyncDemoLibrary
{
    public static class UILogger
    {
        public static void LogToUI(Action<string> uIMessenger, string message, string caller = null)
        {
            caller ??= "App";
            if (string.IsNullOrWhiteSpace(Thread.CurrentThread.Name))
                Thread.CurrentThread.Name = $"{caller} Thread";
            message = $"[{DateTime.Now.ToLongTimeString()}][{Thread.CurrentThread.Name}:{Thread.CurrentThread.ManagedThreadId}][{caller}] > {message}";
            uIMessenger?.Invoke(message);
        }

        public static void WriteDirectToUI(Action<string> uIMessenger, string message) =>
            uIMessenger?.Invoke($"[{DateTime.Now.ToLongTimeString()}] {message}");

        public static void LogThreadType(Action<string> uIMessenger, string caller = null)
        {
            caller ??= "App";
            var threadname = Thread.CurrentThread.IsThreadPoolThread ? "Threadpool Thread" : "Application Thread";
            LogToUI(uIMessenger, $" running on {threadname}", caller);
        }

    }
}
