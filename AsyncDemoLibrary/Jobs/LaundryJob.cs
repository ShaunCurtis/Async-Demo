using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    class LaundryJob : JobItem 
    {
        public LaundryJob(PriorityType ptype) : base(ptype) {
            this.JobName = "Laundry";
        }

        protected override async Task Run()
        {
            await this.CheckForIdle(this.Person);

            this.JobStatus = JobStatusType.Running;
            this.Person.ImMultitasking(this.JobName);
            if (!this.JobStartedTask.IsCompleted) this.JobStartedTaskController.SetResult();
            this.Person.LogToUI($"Loading the washing machine and", this.JobName);
            this.Person.LogToUI($"Check if we have powder", this.JobName);
            if (this.Person.StockList.SoapPowder < 1)
            {
                this.Person.LogToUI($"No soap powder!", this.JobName);
                this.Person.ShoppingList.Add($"Soap Powder");
                this.Person.LogToUI($"Can't continue till we have some powder!", this.JobName);
                this.Person.ImIdle(this.JobName);
                await this.Person.ShoppingList.ShoppingTask;
            }
            await this.CheckForIdle(this.Person);
            this.Person.ImMultitasking(this.JobName);
            this.Person.LogToUI($"Add the powder, Click the button and stand back", this.JobName);
            this.Person.LogToUI($"washing...", this.JobName);
            this.Person.ImIdle(this.JobName);
            await this.Person.RunLongDelayTaskAsync(15, "Chug, chug, chug,..Washing Machine running");
            await this.CheckForIdle(this.Person);
            this.Person.LogToUI($"Emptying machine", this.JobName);
            this.Person.ImBusy(this.JobName);
            await this.Person.RunLongDelayTaskAsync(3, "Putting Washing Away");
            this.Person.ImIdle(this.JobName);
        }
    }
}
