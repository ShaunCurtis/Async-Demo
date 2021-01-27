/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Async_Demo
{ 
    class JobRunner
    {
        public enum JobType { IO, Processor, YieldingProcessor } 

        public JobRunner(string name, int secs, JobType type = JobType.IO)
        {
            this.Name = name;
            this.Seconds = secs;
            this.Type = type;
        }

        public string Name { get; private set; }

        public int Seconds { get; private set; }

        public JobType Type { get; set; }

        private TaskCompletionSource JobTaskController { get; set; } = new TaskCompletionSource();

        private bool IsRunning;

        public Task JobTask => this.JobTaskController == null ? Task.CompletedTask : this.JobTaskController.Task;

        public async void Run()
        {
            if (!this.IsRunning) {
                this.IsRunning = true;
                this.JobTaskController = new TaskCompletionSource();
                switch (this.Type)
                {
                    case JobType.Processor:
                        await LongRunningTasks.RunLongProcessorTaskAsync(Seconds, Program.LogToConsole, Name);
                        break;
                    
                    case JobType.YieldingProcessor:
                        await LongRunningTasks.RunYieldingLongProcessorTaskAsync(Seconds, Program.LogToConsole, Name);
                        break;

                    default:
                        await LongRunningTasks.RunLongIOTaskAsync(Seconds, Program.LogToConsole, Name);
                        break;
                }

                this.JobTaskController.TrySetResult();
                this.IsRunning = false;
            }
        }
    }
}
