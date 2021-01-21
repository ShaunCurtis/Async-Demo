﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Async_Demo
{
    public abstract class UILogger
    {
        public Action<string> UIMessenger;

        protected string CallerName = "Application";

        protected void LogToUI(string message, string caller = null)
        {
            if (string.IsNullOrWhiteSpace(Thread.CurrentThread.Name))
                Thread.CurrentThread.Name = $"{caller} Thread";
            caller ??= this.CallerName;
            message = $"[{Thread.CurrentThread.Name}][{caller}] > {message}";
            UIMessenger?.Invoke(message);
        }

        protected void WriteDirectToUI(string message) =>
            UIMessenger?.Invoke(message);

        protected void LogThreadType(string caller = null)
        {
            caller ??= this.CallerName;
            var threadname = Thread.CurrentThread.IsThreadPoolThread ? "Threadpool Thread" : "Application Thread";
            this.LogToUI($" running on {threadname}", caller);
        }
    }
}