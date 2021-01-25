using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    class GoodMorningJob : JobItem
    {

        public GoodMorningJob(PriorityType ptype) : base(ptype) {
            this.JobName = "Good Morning";
        }

        protected async override Task Run()
        {
            this.JobStatus = JobStatusType.Running;
            await this.CheckForIdle(this.Person);
            this.JobStartedTaskController.SetResult();
            this.Person.ImMultitasking(this.JobName);
            this.Person.LogToUI($"Morning - what's on the agenda today!", this.JobName);
            this.Person.LogToUI("Breakfast first", this.JobName);
            this.Person.ImIdle(this.JobName);
        }

    }
}
