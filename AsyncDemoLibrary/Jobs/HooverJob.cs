using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    class HooverJob : JobItem
    {

        public HooverJob(PriorityType ptype) : base(ptype)
        {
            this.JobName = "Hoovering";
        }

        protected override async Task Run()
        {
            await this.CheckForIdle(this.Person);
            this.JobStatus = JobStatusType.Running;
            this.Person.ImMultitasking(this.JobName);
            if (!this.JobStartedTask.IsCompleted) this.JobStartedTaskController.SetResult();
            this.Person.LogToUI($"Get the vacuum machine out");
            if (this.Person.StockList.HooverBags < 1)
            {
                this.Person.LogToUI($"Bag full, No new bags!!!", this.JobName);
                this.Person.ShoppingList.Add("Hoover Bags");
                this.Person.LogToUI($"Can't continue till we have some more bags!", this.JobName);
                this.Person.ImIdle(this.JobName);
                await this.Person.ShoppingList.ShoppingTask;
                await this.CheckForIdle(this.Person);
            }
            else
                this.Person.LogToUI($"Thank goodness there's bags, I've already done the shopping once!", this.JobName);
            this.Person.ImBusy(this.JobName);
            this.Person.LogToUI($"Chained to the Hoover!", this.JobName);
            await this.Person.RunLongDelayTaskAsync(5, $"Hi ho, Hi ho, it's ....");
            this.Person.LogToUI($"Hoovering finished", this.JobName);
            this.Person.ImIdle(this.JobName);
        }
    }
}
