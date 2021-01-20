using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Async_Demo
{
    class Program
    {

        static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main Thread";
            var message = Thread.CurrentThread.IsThreadPoolThread ? "Threadpool" : "Application";
            Console.WriteLine($"Main running on a {message} thread Name: {Thread.CurrentThread.Name}");

            var task = TestYield();
            Console.WriteLine("Yielded");
            var test2 = TestYield2();
            task.Wait();

            //var messengerService = new MessengerService();
            //var chores = new Chores(messengerService);
            //chores.ChoresTask.Wait();
        }

        public static async Task TestYield()
        {
            Console.WriteLine($"[TestYield] Main running on thread: {Thread.CurrentThread.Name}");
            var longtask = new DelayTask();
            var task = longtask.RunAsync(15, "running 1");
            Console.WriteLine("Yielding");
            // await Task.Yield();
            task.Wait();
//            await task;
            Console.WriteLine("Finished task");
        }

        public static async Task TestYield2()
        {
            Console.WriteLine($"[TestYield2]Main running on thread: {Thread.CurrentThread.Name}");
            var longtask = new DelayTask();
            var task = longtask.RunAsync(15, "running 2");
            Console.WriteLine("Awaiting 2");
            await task;
            Console.WriteLine("Finished task");
        }


    }
}

