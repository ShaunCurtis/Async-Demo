/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System.Threading.Tasks;

namespace AsyncDemoLibrary
{

    public class Busy :UILogger
    {
        TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
        public Busy(bool isbusy)
        {
            taskCompletionSource = new TaskCompletionSource<bool>();
            if (!isbusy) taskCompletionSource.SetResult(true);
        }

        public void SetIdle()
        {
            var taskname = "Busy Task";
            taskCompletionSource.SetResult(true);
            this.LogToUI($"finished {Message}",taskname);
        }

        public Task Task =>
            taskCompletionSource != null ?
            taskCompletionSource.Task :
            Task.CompletedTask;

        private string Message = "Doing Things";

        public void SetBusy(string message = null)
        {
            Message = message ?? Message;

            var taskname = "Busy Task";
            taskCompletionSource = new TaskCompletionSource<bool>();
            this.LogToUI($"Busy {Message}", taskname);
        }
    }
}

