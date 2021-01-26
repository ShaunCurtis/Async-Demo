/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using System.Threading.Tasks;
using System.Threading;
using AsyncDemoLibrary;
using System.Diagnostics;

namespace Async_Demo
{
    class Program 
    {

        //static void Main(string[] args)
        //{
        //}

        static async Task Main(string[] args)
        {
            var watch = new Stopwatch();
            watch.Start();
            UILogger.LogThreadType(LogToConsole, "Main");
            // var phonemessengerService = new PhoneMessengerService(LogToConsole);
            // var phonemessengerServiceTask = Task.Run(() => phonemessengerService.Run());
            // var millisecs = LongRunningTasks.RunLongTask(10);
            // var millisecs = LongRunningTasks.RunLongProcessorTaskAsync(5, LogToConsole);
            // var millisecs = await LongRunningTasks.RunYieldingLongProcessorTaskAsync(5, LogToConsole);
            // var millisecs = await LongRunningTasks.RunLongIOTaskAsync(5, LogToConsole);
            var task = JobScheduler();
            UILogger.LogToUI(LogToConsole, $"Job Scheduler yielded to Main", "Main");
            await task;
            //var millisecs = LongRunningTasks.RunLongProcessorTaskAsync(5);
            UILogger.LogToUI(LogToConsole, $"final yield to Main", "Main");
            watch.Stop();
            UILogger.LogToUI(LogToConsole, $"Main ==> Completed in { watch.ElapsedMilliseconds} milliseconds", "Main");

            //return Task.CompletedTask;
        }

        public static void LogToConsole(string message) =>
            Console.WriteLine(message);

        static async Task JobScheduler()
        {
            var watch = new Stopwatch();
            watch.Start();
            var name = "Job Scheduler";
            var quickjob = new JobRunner("Quick Job", 3, JobRunner.JobType.Processor);
            var veryslowjob = new JobRunner("Very Slow Job", 7, JobRunner.JobType.Processor);
            var slowjob = new JobRunner("Slow Job", 5, JobRunner.JobType.Processor);
            var veryquickjob = new JobRunner("Very Quick Job", 2, JobRunner.JobType.Processor);
            quickjob.Run();
            veryslowjob.Run();
            slowjob.Run();
            veryquickjob.Run();
            UILogger.LogToUI(LogToConsole, $"All Jobs Scheduled", name);
            await Task.WhenAll(new Task[] { quickjob.JobTask, veryquickjob.JobTask }); ;
            UILogger.LogToUI(LogToConsole, $"Quick Jobs completed in {watch.ElapsedMilliseconds} milliseconds", name);
            await Task.WhenAll(new Task[] { slowjob.JobTask, quickjob.JobTask, veryquickjob.JobTask, veryslowjob.JobTask }); ;
            UILogger.LogToUI(LogToConsole, $"All Jobs completed in {watch.ElapsedMilliseconds} milliseconds", name);
            watch.Stop();
        }
    }

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
