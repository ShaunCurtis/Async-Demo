using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    class GoToShopsJob : JobItem
    {
        public GoToShopsJob(PriorityType ptype) : base(ptype)
        {
            this.JobName = "Go To Shops";
        }

        protected override async Task Run()
        {
            // Delay decision for other things to start first
            await Task.Delay(5);
            await this.CheckForIdle(this.Person);
            if (!this.JobStartedTask.IsCompleted) this.JobStartedTaskController.SetResult();
            this.JobStatus = JobStatusType.Running;
            this.Person.ImMultitasking(this.JobName);
            if (this.Person.ShoppingList.NeedToShop)
            {
                await this.Person.RunLongDelayTaskAsync(4, $"Heading out to the shops.");
                foreach (var item in this.Person.ShoppingList)
                {
                    await this.Person.RunLongDelayTaskAsync(1, $"Getting {item}");
                }
                await this.Person.RunLongDelayTaskAsync(1, $"Heading home");
                this.Person.LogToUI($"Back Home. Shopping done.", this.JobName);
                this.Person.ShoppingList.ShoppingDone();
            }
            else
                this.Person.LogToUI($"Alas, no shopping to do.", this.JobName);
            this.Person.ImIdle(this.JobName);
        }
    }
}
