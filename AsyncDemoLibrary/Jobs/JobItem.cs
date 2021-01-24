using System;
using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    public class JobItem 
    {

        public PriorityType Priority { get; set; }

        public Func<UnemployedProgrammer, Task> Job { get; set; }

        //public Task JobTask => JobTaskSource == null ? Task.CompletedTask: this.JobTaskSource.Task ;

        //private TaskCompletionSource JobTaskSource { get; set; }

        //public Task Run(UnemployedProgrammer person)
        //{
        //    JobTaskSource.SetResult();
        //    return Task.CompletedTask;
        //}

    }
}
