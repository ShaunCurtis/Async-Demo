using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    class BreakfastJob 
    {
        public PriorityType Priority { get; set; }

        //public Task JobTask => JobTaskSource == null ? Task.CompletedTask: this.JobTaskSource.Task ;

        //private TaskCompletionSource JobTaskSource { get; set; }

        //public async Task Run(UnemployedProgrammer person)
        //{
        //    person.ImMultitasking();
        //    var taskTitle = "Breakfast";
        //    person.LogToUI($"In the fridge", taskTitle);
        //    person.LogToUI($"No eggs, so it's toast only.", taskTitle);
        //    person.ShoppingList.Add("Eggs");
        //    person.LogToUI($"Last two pieces of bread used up", taskTitle);
        //    person.ShoppingList.Add("Bread");
        //    person.ImBusy();
        //    await person.RunLongDelayTaskAsync(5, $"Eating").ContinueWith;
        //    person.LogToUI($" ???? No Wine in fridge?", taskTitle);
        //    person.ShoppingList.Add("Wine");
        //    person.ImIdle();
        //}

    }
}
