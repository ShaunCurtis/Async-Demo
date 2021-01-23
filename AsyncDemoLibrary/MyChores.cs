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
    public class MyChores : BaseClass
    {

        StockList OurStocks = new StockList()
        {
            HooverBags = 1,
            WashingUpLiquid = 0,
            SoapPowder = 0
        };

        ShoppingJob ShoppingList { get; } = new ShoppingJob();

        public Task ChoresTask { get; set; }

        private PhoneMessengerService Messenger;

        private UnemployedProgrammer Me;

        public MyChores(UnemployedProgrammer person, PhoneMessengerService messenger, Action<string> uiLogger)
        {
            this.CallerName = "Chores Task Master";

            this.UIMessenger = uiLogger;
            this.ShoppingList.UIMessenger = uiLogger;

            this.Me = person;

            Messenger = messenger;

        }

        public async Task Start(UnemployedProgrammer me)
        {
            var taskname = "Run Chores";
            this.LogThreadType();
            this.Me = me;
            var taskstorun = new List<PersonalJob>();
            {
                taskstorun.Add(new PersonalJob() { Job = GoodMorning, Priority = PriorityType.MustCompleteNow });
                taskstorun.Add(new PersonalJob() { Job = HaveBreakfast, Priority = PriorityType.MustCompleteNow });
                taskstorun.Add(new PersonalJob() { Job = WashingUpChore, Priority = PriorityType.MustCompleteNow });
                taskstorun.Add(new PersonalJob() { Job = TheWashingChore, Priority = PriorityType.Normal});
                taskstorun.Add(new PersonalJob() { Job = GoToTheShops, Priority = PriorityType.MustCompleteNow });
            }
            this.Me.AddTaskstoList(taskstorun);
            await me.PriorityJobsMonitorAsync();
            taskstorun.Clear();
            {
                taskstorun.Add(new PersonalJob() { Job = HooveringChore, Priority = PriorityType.MustCompleteNow });
                taskstorun.Add(new PersonalJob() { Job = GoToTheShops, Priority = PriorityType.MustCompleteNow });
            }
            this.Me.AddTaskstoList(taskstorun);
            await me.AllJobsMonitorAsync();
            this.LogToUI("all done, feet up.", taskname);
            this.WriteDirectToUI("daydream of a proper job!");
        }

        public Task GoodMorning(UnemployedProgrammer person)
        {
            person.ImMultitasking();
            var taskname = "Good Morning";
            this.LogToUI($"Morning - what's on the agenda today!", taskname);
            this.LogToUI("Breakfast first", taskname);
            person.ImIdle();
            person.ClearAllTaskLists();
            return Task.CompletedTask;
        }


        public async Task HaveBreakfast(UnemployedProgrammer person)
        {
            person.ImMultitasking();
            var taskname = "Breakfast";
            this.LogToUI($"In the fridge", taskname);
            this.LogToUI($"No eggs, so it's toast only.", taskname);
            ShoppingList.Add("Eggs");
            this.LogToUI($"Last two pieces of bread used up", taskname);
            ShoppingList.Add("Bread");
            await this.RunLongDelayTaskAsync(5, $"Eating");
            this.LogToUI($" ???? No Wine in fridge?", taskname);
            ShoppingList.Add("Wine");
            person.ImIdle();
        }

        public async Task WashingUpChore(UnemployedProgrammer person)
        {
            var taskname = $"Washing Up";
            var threadname = Thread.CurrentThread.Name;
            var completiontask = new Task(() =>
            {
                person.ImBusy("Hands in the sink");
                Task.Delay(5);
                person.ImBusy("rubbers off");
                this.LogToUI($"Washing Up Done", taskname);
            });

            this.LogToUI($"Check if I can do the washing up (any excuse will do!)",taskname);
            if (this.OurStocks.WashingUpLiquid < 1)
            {
                this.LogToUI($"mmmm - can't find any Washing Up Liquid", taskname);
                ShoppingList.Add("Washing Up Liquid");
                this.LogToUI($"Can't continue till we have some washing Up Liquid!", taskname);
                await ShoppingList.ShoppingTask;
            }
            this.LogToUI($"Back to the sink", taskname);
            completiontask.RunSynchronously();
        }

        public async Task HooveringChore(UnemployedProgrammer person)
        {
            var taskname = "Hoovering";

            var task = new Task(async () =>
            {
                person.ImBusy("Chained to the vacuum");
                await this.RunLongDelayTaskAsync(5, $"Chained to the vacuum");
                person.ImIdle("unchained from vacuum");
            });

            this.LogToUI($"Get the machine out");
            if (this.OurStocks.HooverBags < 1)
            {
                this.LogToUI($"No bags!",taskname);
                ShoppingList.Add($"Hoover Bags");
                this.LogToUI($"Can't continue till we have some bags!", taskname);
                 await ShoppingList.ShoppingTask;
            }
            else
                this.LogToUI($"Thank goodness there's bags, I've already done the shopping once!", taskname);
            task.RunSynchronously();
        }

        public async Task TheWashingChore(UnemployedProgrammer person)
        {
            var taskname = "Clothes Washing";
            var threadname = Thread.CurrentThread.Name;
            var message = $"[{threadname}]>[{taskname}]";
            this.LogToUI($"Loading the washing machine and", taskname);
            this.LogToUI($"Check if we have powder", taskname);
            if (this.OurStocks.SoapPowder < 1)
            {
                this.LogToUI($"No soap powder!", taskname);
                ShoppingList.Add($"Soap Powder");
                this.LogToUI($"Can't continue till we have some powder!", taskname);
                await ShoppingList.ShoppingTask;
            }
            this.LogToUI($"Add the powder, Click the button and stand back", taskname);
            this.LogToUI($"washing...", taskname);
            // await Task.Yield();
            await this.RunLongDelayTaskAsync(14, "Washing Machine running");
            this.LogToUI($"PING!! PING!!! Washing complete!", taskname);
            await person.DoNotDisturbTask;
            this.LogToUI($"Emptying machine", taskname);
            person.ImBusy();
            await this.RunLongDelayTaskAsync(3, "Putting Washing Away");
            person.ImIdle();
        }

        public async Task GoToTheShops(UnemployedProgrammer person)
        {
            var taskname = "Shopping";
            var again = this.ShoppingList.TripsToShop == 0 ? "" : " again";
            if (ShoppingList.NeedToShop)
            {
                person.ImMultitasking();
                await this.RunLongDelayTaskAsync(4, $"Heading out to the shops{again}");
                foreach (var item in ShoppingList)
                {
                    await this.RunLongDelayTaskAsync(1, $"Getting {item}");
                }
                await this.RunLongDelayTaskAsync(1, $"Heading home");
                this.LogToUI($"Back Home. Shopping done.", taskname);
                ShoppingList.ShoppingDone();
                person.ImIdle();
            }
        }
    }
}

