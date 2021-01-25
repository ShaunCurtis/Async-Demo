using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    class WashingUpJob : JobItem
    {

        public WashingUpJob(PriorityType ptype) : base(ptype)
        {
            this.JobName = "Washing Up";
        }

        protected override async Task Run()
        {
            await this.CheckForIdle(this.Person);
            this.JobStatus = JobStatusType.Running;
            if (!this.JobStartedTask.IsCompleted) this.JobStartedTaskController.SetResult();
            this.Person.ImMultitasking(this.JobName);
            this.Person.LogToUI($"Check if I can do the washing up (any excuse will do!)", this.JobName);
            if (this.Person.StockList.WashingUpLiquid < 1)
            {
                this.Person.LogToUI($"mmmm - can't find any Washing Up Liquid", this.JobName);
                this.Person.ShoppingList.Add("Washing Up Liquid");
                this.Person.LogToUI($"Can't continue till we have some washing Up Liquid!", this.JobName);
                this.Person.ImIdle(this.JobName);
                await this.Person.ShoppingList.ShoppingTask;
            }
            await this.CheckForIdle(this.Person);
            this.Person.ImBusy(this.JobName);
            this.Person.LogToUI($"Back to the sink. Marigolds on!", this.JobName);
            await this.Person.RunLongDelayTaskAsync(5, $"washing up.");
            this.Person.LogToUI($"Washing Up Done", this.JobName);
            this.Person.ImIdle("Marigolds off");
        }

    }
}
