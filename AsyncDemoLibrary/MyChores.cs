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

        private LongRunningTasks LongRunTask;

        private UnemployedProgrammer Me;

        public MyChores(UnemployedProgrammer sucker, PhoneMessengerService messenger, Action<string> uiLogger)
        {
            this.CallerName = "Chores Task Master";

            this.UIMessenger = uiLogger;
            this.ShoppingList.UIMessenger = uiLogger;
            this.Me = sucker;

            LongRunTask = new LongRunningTasks(uiLogger);

            Messenger = messenger;

        }

        public async Task Start(UnemployedProgrammer me)
        {
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
            await me.PriorityTasks;
            taskstorun.Clear();
            {
                taskstorun.Add(new PersonalJob() { Job = HooveringChore, Priority = PriorityType.MustCompleteNow });
                taskstorun.Add(new PersonalJob() { Job = GoToTheShops, Priority = PriorityType.MustCompleteNow });
            }
            this.Me.AddTaskstoList(taskstorun);
            await me.AllTasks;
            //return Task.CompletedTask;
        }



        //public task domychores()
        //{
        //    var taskname = "morning chores";

        //    //// await task.yield();
        //    //this.logtoui($"morning - what's on the agenda today!", taskname);
        //    //this.logtoui("breakfast first", taskname);
        //    ////var dowashingupchore = washingupchore(new task[] { });
        //    //var dothewashingchore = thewashingchore(new task[] { });
        //    //await gototheshops();
        //    //await dowashingupchore;
        //    //var dohoovering = hooveringchore(new task[] { dowashingupchore });
        //    //await gototheshops();
        //    //task.waitall(new task[] { dothewashingchore, dohoovering });
        //    //this.logtoui("all done, feet up.", taskname);
        //    //this.writedirecttoui("daydream of a proper job!");
        //}

        public Task GoodMorning(UnemployedProgrammer sucker)
        {
            sucker.ImMultitasking();
            var taskname = "Good Morning";
            this.LogToUI($"Morning - what's on the agenda today!", taskname);
            this.LogToUI("Breakfast first", taskname);
            sucker.ImIdle();
            return Task.CompletedTask;
        }


        public async Task HaveBreakfast(UnemployedProgrammer sucker)
        {
            sucker.ImMultitasking();
            var taskname = "Breakfast";
            this.LogToUI($"In the fridge", taskname);
            this.LogToUI($"No eggs, so it's toast only.", taskname);
            ShoppingList.Add("Eggs");
            this.LogToUI($"Last two pieces of bread used up", taskname);
            ShoppingList.Add("Bread");
            await LongRunTask.RunAsync(5, $"Eating");
            this.LogToUI($" ???? No Wine in fridge?", taskname);
            ShoppingList.Add("Wine");
            sucker.ImIdle();
        }

        public async Task WashingUpChore(UnemployedProgrammer sucker)
        {
            var taskname = $"Washing Up";
            var threadname = Thread.CurrentThread.Name;
            var completiontask = new Task(() =>
            {
                sucker.ImBusy("Hands in the sink");
                Task.Delay(5);
                sucker.ImBusy("rubbers off");
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

        public async Task HooveringChore(UnemployedProgrammer sucker)
        {
            var taskname = "Hoovering";

            var task = new Task(async () =>
            {
                sucker.ImBusy("Chained to the vacuum");
                await LongRunTask.RunAsync(5, $"Chained to the vacuum");
                sucker.ImIdle("unchained");
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
                this.LogToUI($"Thank god there's bags, I've already done the shopping once!", taskname);
            task.RunSynchronously();
        }

        public async Task TheWashingChore(UnemployedProgrammer sucker)
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
            await this.RunLongTaskAsync(14, "Washing Machine running");
            this.LogToUI($"PING!! PING!!! Washing complete!", taskname);
            await sucker.DoNotDisturbTask;
            this.LogToUI($"Emptying machine", taskname);
            sucker.ImBusy();
            await this.RunLongTaskAsync(6, "Putting Washing Away");
            sucker.ImIdle();
        }

        public async Task GoToTheShops(UnemployedProgrammer sucker)
        {
            var taskname = "Shopping";
            var again = this.ShoppingList.TripsToShop == 0 ? "" : " again";
            if (ShoppingList.NeedToShop)
            {
                sucker.ImMultitasking();
                await LongRunTask.RunAsync(4, $"Heading out to the shops{again}");
                foreach (var item in ShoppingList)
                {
                    await LongRunTask.RunAsync(1, $"Getting {item}");
                }
                await LongRunTask.RunAsync(1, $"Heading home");
                this.LogToUI($"Back Home. Shopping done.", taskname);
                ShoppingList.ShoppingDone();
                sucker.ImIdle();
            }
        }
    }
}

