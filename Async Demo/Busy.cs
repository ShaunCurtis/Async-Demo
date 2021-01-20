using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Async_Demo
{

    public class Busy
    {
        TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

        public void Finished()
        {
            if (string.IsNullOrWhiteSpace(Thread.CurrentThread.Name))
                Thread.CurrentThread.Name = "Busy Thread";
            var taskname = "Busy Task";
            var threadname = Thread.CurrentThread.Name;
            var message = $"[{threadname}]>[{taskname}]";
            taskCompletionSource.SetResult(true);
            Console.WriteLine($"{message} > finished {Message}");
        }

        public Task Task =>
            taskCompletionSource != null ?
            taskCompletionSource.Task :
            Task.CompletedTask;

        private string Message = "Doing Things";

        public void SetBusy(string message = null)
        {
            Message = message ?? Message;

            //if (string.IsNullOrWhiteSpace(Thread.CurrentThread.Name))
            //    Thread.CurrentThread.Name = "Busy Thread";
            var taskname = "Busy Task";
            var threadname = Thread.CurrentThread.Name;
            var mess = $"[{threadname}]>[{taskname}]";
            taskCompletionSource = new TaskCompletionSource<bool>();
            Console.WriteLine($"{mess} > Busy {Message}");
        }

        public Busy(bool isbusy)
        {
            taskCompletionSource = new TaskCompletionSource<bool>();
            if (!isbusy) taskCompletionSource.SetResult(true);
        }
    }
}

