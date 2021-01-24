/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    public class JobScheduler : BaseClass
    {

        StockList OurStocks = new StockList()
        {
            HooverBags = 1,
            WashingUpLiquid = 0,
            SoapPowder = 0
        };

        ShoppingJob ShoppingList { get; } = new ShoppingJob();

        private UnemployedProgrammer Me;

        public JobScheduler(UnemployedProgrammer person, Action<string> uiLogger)
        {
            this.CallerName = "Job Task Master";

            this.UIMessenger = uiLogger;
            this.ShoppingList.UIMessenger = uiLogger;

            this.Me = person;
        }

        public async Task Start(UnemployedProgrammer me = null)
        {
            var taskTitle = "Run Jobs";
            this.LogThreadType();
            this.Me = me ?? this.Me;
            var taskstorun = new List<JobItem>();
            {
                taskstorun.Add(new JobItem() { Job = GoodMorning, Priority = PriorityType.Priority });
                taskstorun.Add(new JobItem() { Job = HaveBreakfast, Priority = PriorityType.Priority });
                taskstorun.Add(new JobItem() { Job = WashingUpChore, Priority = PriorityType.Priority });
                //taskstorun.Add(new JobItem() { Job = TheWashingChore, Priority = PriorityType.Normal });
                taskstorun.Add(new JobItem() { Job = GoToTheShops, Priority = PriorityType.Priority });
            }
            this.Me.QueueJobs(taskstorun);
            await me.PriorityJobsMonitorAsync();
            this.Me.QueueJob(new JobItem() { Job = HooveringChore, Priority = PriorityType.Priority }, false);
            this.Me.QueueJob(new JobItem() { Job = GoToTheShops, Priority = PriorityType.Priority });
            await me.AllJobsMonitorAsync();
            this.LogToUI("All done, feet up.", taskTitle);
            this.WriteDirectToUI("Daydream of a proper job!");
        }

        public Task GoodMorning(UnemployedProgrammer person)
        {
            var taskTitle = "Good Morning";
            person.ImMultitasking(taskTitle);
            this.LogToUI($"Morning - what's on the agenda today!", taskTitle);
            this.LogToUI("Breakfast first", taskTitle);
            person.ImIdle(taskTitle);
            return Task.CompletedTask;
        }


        public Task HaveBreakfast(UnemployedProgrammer person)
        {

            var taskTitle = "Having Breakfast";
            person.ImMultitasking(taskTitle);
            this.LogToUI($"In the fridge", taskTitle);
            this.LogToUI($"No eggs, so it's toast only.", taskTitle);
            ShoppingList.Add("Eggs");
            this.LogToUI($"Last two pieces of bread used up", taskTitle);
            ShoppingList.Add("Bread");
            person.ImBusy(taskTitle);
            return this.RunLongDelayTaskAsync(5, $"Eating").ContinueWith((task) =>
            {
                this.LogToUI($" ???? No Wine in fridge?", taskTitle);
                ShoppingList.Add("Wine");
                person.ImIdle(taskTitle);
                return Task.CompletedTask;
            });
        }

        TaskCompletionSource WashingUpChoreTask = new TaskCompletionSource();

        public Task WashingUpChore(UnemployedProgrammer person)
        {
            var taskTitle = $"Washing Up";
            var threadname = Thread.CurrentThread.Name;
            var completiontask = new Task(async () =>
            {
                this.LogToUI($"Back to the sink", taskTitle);
                person.ImBusy(taskTitle);
                await Task.Delay(5);
                person.ImIdle("Rubbers off");
                this.LogToUI($"Washing Up Done", taskTitle);
            });

            var WaitOnShoppingTask = new Task(async () =>
            {
                await person.IsIdleTask;
                this.LogToUI($"Check if I can do the washing up (any excuse will do!)", taskTitle);
                if (this.OurStocks.WashingUpLiquid < 1)
                {
                    this.LogToUI($"mmmm - can't find any Washing Up Liquid", taskTitle);
                    ShoppingList.Add("Washing Up Liquid");
                    this.LogToUI($"Can't continue till we have some washing Up Liquid!", taskTitle);
                    await ShoppingList.ShoppingTask;
                }
                completiontask.RunSynchronously();
            });

            var waitOnShoppingTask = WaitOnShoppingTask;
            waitOnShoppingTask.Start();
                        
            return Task.WhenAll(new Task[] { waitOnShoppingTask, completiontask });

        }

        public async Task HooveringChore(UnemployedProgrammer person)
        {
            var taskTitle = "Hoovering";

            var task = new Task(async () =>
            {
                person.ImBusy("Chained to the vacuum");
                await this.RunLongDelayTaskAsync(5, $"Chained to the vacuum");
                person.ImIdle("unchained from vacuum");
            });

            this.LogToUI($"Get the vacuum machine out");
            if (this.OurStocks.HooverBags < 1)
            {
                this.LogToUI($"No bags!", taskTitle);
                ShoppingList.Add($"Hoover Bags");
                this.LogToUI($"Can't continue till we have some bags!", taskTitle);
                await ShoppingList.ShoppingTask;
            }
            else
                this.LogToUI($"Thank goodness there's bags, I've already done the shopping once!", taskTitle);
            task.RunSynchronously();
        }

        public async Task TheWashingChore(UnemployedProgrammer person)
        {
            var taskTitle = "Clothes Washing";
            var threadname = Thread.CurrentThread.Name;
            var message = $"[{threadname}]>[{taskTitle}]";
            this.LogToUI($"Loading the washing machine and", taskTitle);
            this.LogToUI($"Check if we have powder", taskTitle);
            if (this.OurStocks.SoapPowder < 1)
            {
                this.LogToUI($"No soap powder!", taskTitle);
                ShoppingList.Add($"Soap Powder");
                this.LogToUI($"Can't continue till we have some powder!", taskTitle);
                await ShoppingList.ShoppingTask;
            }
            this.LogToUI($"Add the powder, Click the button and stand back", taskTitle);
            this.LogToUI($"washing...", taskTitle);
            // await Task.Yield();
            await this.RunLongDelayTaskAsync(14, "Washing Machine running");
            this.LogToUI($"PING!! PING!!! Washing complete!", taskTitle);
            await Task.WhenAny(new Task[] { person.IsIdleTask, person.IsMultiTaskingTask });
            this.LogToUI($"Emptying machine", taskTitle);
            person.ImBusy(taskTitle);
            await this.RunLongDelayTaskAsync(3, "Putting Washing Away");
            person.ImIdle(taskTitle);
        }

        public async Task GoToTheShops(UnemployedProgrammer person)
        {
            var taskTitle = "Shopping";
            var again = this.ShoppingList.TripsToShop == 0 ? "" : " again";
            var continuetask = new Task(async () =>
            {
                if (ShoppingList.NeedToShop)
                {
                    await this.RunLongDelayTaskAsync(4, $"Heading out to the shops{again}");
                    foreach (var item in ShoppingList)
                    {
                        await this.RunLongDelayTaskAsync(1, $"Getting {item}");
                    }
                    await this.RunLongDelayTaskAsync(1, $"Heading home");
                    this.LogToUI($"Back Home. Shopping done.", taskTitle);
                    ShoppingList.ShoppingDone();
                    person.ImIdle(taskTitle);
                }

            });

            await person.IsIdleTask.ContinueWith((task) => continuetask);
        }
    }
}

