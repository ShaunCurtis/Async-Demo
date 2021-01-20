using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Async_Demo
{

    public class MessengerService
    {
        public bool live { get; set; } = true;

        public Task MessangerTask { get; set; }

        public MessengerService()
        {
            var message = Thread.CurrentThread.IsThreadPoolThread ? "Threadpool" : "Application";
            Console.WriteLine($"[Messenger] running on {message} thread Name: {Thread.CurrentThread.Name}");
            MessangerTask = Task.Run(() => Messanger());
        }

        private async Task Messanger()
        {
            if (string.IsNullOrWhiteSpace(Thread.CurrentThread.Name))
                Thread.CurrentThread.Name = "Messenger Thread";

            var message = Thread.CurrentThread.IsThreadPoolThread ? "Threadpool" : "Application";
            Console.WriteLine($"[Messenger Service] running on a {message} thread Name: {Thread.CurrentThread.Name}");

            do
            {
                await Task.Delay(3000);
                //                    Console.WriteLine("[Messanger Service] sending message");
                PingMessage?.Invoke($"Hey, stuff going on at {DateTime.Now.ToLongTimeString()}!", EventArgs.Empty);
            } while (live);
        }

        public event EventHandler PingMessage;
    }
}

