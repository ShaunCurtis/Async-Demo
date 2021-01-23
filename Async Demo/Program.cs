/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using System.Threading.Tasks;
using System.Threading;
using AsyncDemoLibrary;

namespace Async_Demo
{
    class Program
    {

        static void Main(string[] args)
        {
            LogThreadType();
            var phonemessengerService = new PhoneMessengerService(LogToConsole);
            var phonemessengerServiceTask = Task.Run(() => phonemessengerService.Run());
            var me = new UnemployedProgrammer(phonemessengerService, LogToConsole);
            var chores = new MyChores(me, phonemessengerService, LogToConsole);
            var chorestask = chores.Start(me);
        }

        protected static void LogThreadType(string caller = null)
        {
            caller ??= "Program";
            if (string.IsNullOrWhiteSpace(Thread.CurrentThread.Name))
                Thread.CurrentThread.Name = $"Main Thread";
            var threadname = Thread.CurrentThread.IsThreadPoolThread ? "Threadpool Thread" : "Application Thread";
            LogToConsole($"[{Thread.CurrentThread.Name}][{caller}] running on {threadname}");
        }

        static void LogToConsole(string message) =>
            Console.WriteLine(message);

    }
}
