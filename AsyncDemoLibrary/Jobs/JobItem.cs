using System;
using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    public enum JobStatusType { None, Activated, Running, Complete, InError }
    
    public class JobItem : IJobItem
    {

        public PriorityType Priority { get; set; }

        public Func<UnemployedProgrammer, Task> Job { get; set; }

        public Action<bool> JobClosedAction { get; set; }

        protected string JobName { get; set; } = "Unnamed Job";

        public Task JobTask => JobTaskController == null ? Task.CompletedTask : this.JobTaskController.Task;

        public Task JobStartedTask => JobStartedTaskController == null ? Task.CompletedTask : this.JobStartedTaskController.Task;

        public int SequenceNo { get; protected set; } = 1;

        protected UnemployedProgrammer Person { get; set; }

        protected TaskCompletionSource JobTaskController { get; set; } = new TaskCompletionSource();

        protected TaskCompletionSource JobStartedTaskController { get; set; } = new TaskCompletionSource();

        public JobStatusType JobStatus { get; protected set; } = JobStatusType.None;

        public JobItem(PriorityType ptype)
        {
            this.Priority = ptype;
        }

        protected virtual Task Run()
        {
            this.JobStartedTaskController.SetResult();
            return Task.CompletedTask;
        }

        public async void RunJob(UnemployedProgrammer person, int sequenceno)
        {
            this.JobStatus = JobStatusType.Activated;
            this.Person = person;
            this.SequenceNo = sequenceno;
            if (person != null) await Run();
            if (!JobTaskController.Task.IsCompleted) JobTaskController.SetResult();
            this.JobClosedAction?.Invoke(true);
            this.JobStatus = JobStatusType.Complete;
        }

        protected async Task CheckForIdle(UnemployedProgrammer person)
        {
            do
            {
                await person.IsIdleTask;
                await Task.Delay(SequenceNo);
            }
            while (!person.IsIdle);
        }

        protected async Task CheckForMultitasking(UnemployedProgrammer person)
        {
            do
            {
                await person.IsMultiTaskingTask;
                await Task.Delay(SequenceNo);
            }
            while (!person.IsMultitasking);
        }

        protected async Task CheckForBusy(UnemployedProgrammer person)
        {
            do
            {
                await person.IsBusyTask;
                await Task.Delay(SequenceNo);
            }
            while (!person.IsBusy);
        }

    }
}
