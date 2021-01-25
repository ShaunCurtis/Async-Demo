using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    class BreakfastJob : JobItem
    {
        public BreakfastJob(PriorityType ptype) : base(ptype)
        {
            this.JobName = "Breakfast";
        }

        protected override async Task Run()
        {
            await this.CheckForIdle(this.Person);
            this.JobStatus = JobStatusType.Running;
            this.Person.ImMultitasking();
            if (!this.JobStartedTask.IsCompleted) this.JobStartedTaskController.SetResult();
            this.Person.LogToUI($"In the fridge", this.JobName);
            this.Person.LogToUI($"No eggs, so it's toast only.", this.JobName);
            this.Person.ShoppingList.Add("Eggs");
            this.Person.LogToUI($"Last two pieces of bread used up", this.JobName);
            this.Person.ShoppingList.Add("Bread");
            this.Person.ImBusy(this.JobName);
            await this.Person.RunLongDelayTaskAsync(5, $"Eating");
            this.Person.LogToUI($" ???? No Wine in fridge?", this.JobName);
            this.Person.ShoppingList.Add("Wine");
            this.Person.ImIdle(this.JobName);
        }

    }
}
