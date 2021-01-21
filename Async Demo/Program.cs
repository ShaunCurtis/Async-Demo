using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Async_Demo
{
    class Program
    {

        static ILongRunningTask LongTask = new PrimeTask(LogToConsole);

        static void Main(string[] args)
        {
            LogThreadType();

            // RunChores();
            var phonemessengerService = new PhoneMessengerService(LogToConsole);
            var chores = new Chores(phonemessengerService, LogToConsole);
            var chorestask = chores.Start();
            //var chorestask = Task.Run(() => chores.Start());
            chorestask.Wait();
            // await chores.ChoresTask;
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

        static async void RunChores()
        {
            var messengerService = new PhoneMessengerService(LogToConsole);
            var chores = new Chores(messengerService, LogToConsole);
        }

        static void QuickTest()
        {
            var task = TestYield();
            Console.WriteLine("Yielded");
            var task2 = TestYield2();
            task.Wait();
            task2.Wait();
        }

        public static async Task TestYield()
        {
            Console.WriteLine($"[TestYield-1] Main running on thread: {Thread.CurrentThread.Name}");
            var longtask = new DelayTask(LogToConsole);
//           var longtask = new PrimeTask();
            var task = longtask.RunYieldingAsync(5, "[TestYield-1]");
            //Console.WriteLine("Yielding");
            // await Task.Yield(LogToConsole);
            Console.WriteLine("[TestYield-1] Waiting");
            task.Wait();
//            await task;
            Console.WriteLine("Finished [TestYield-1]");
        }

        public static async Task TestYield2()
        {
            Console.WriteLine($"[TestYield2]Main running on thread: {Thread.CurrentThread.Name}");
            var longtask = new DelayTask(LogToConsole);
            //            var longtask = new PrimeTask(LogToConsole);
            var task = longtask.RunAsync(8, "[TestYield-2]");
            Console.WriteLine("Awaiting [TestYield-2]");
            await task;
            Console.WriteLine("Finished [TestYield-2]");
        }
    }
}

