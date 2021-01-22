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
    public class Chores : UILogger
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

        private List<string> MyMessages = new List<string>();

        private Busy ImBusy = new Busy(false);

        private bool MessagesAlreadyWaiting = false;

        private LongRunningTasks LongRunTask;

        public Chores(PhoneMessengerService messenger, Action<string> uiLogger)
        {
            this.CallerName = "Chores Task Master";

            this.UIMessenger = uiLogger;
            this.ShoppingList.UIMessenger = uiLogger;
            this.ImBusy.UIMessenger = uiLogger;

            LongRunTask = new LongRunningTasks(uiLogger);

            Messenger = messenger;

            Messenger.PingMessage += OnMessageReceived;
        }

        public Task Start()
        {
            this.LogThreadType();
            this.ChoresTask = DoMyChores();
            return this.ChoresTask;
        }

        public void OnMessageReceived(object sender, EventArgs e)
        {
            MyMessages.Add((string)sender);
            this.LogToUI($"Ping! New message at {DateTime.Now.ToLongTimeString()}. You have {MyMessages.Count} unread messages.", "My Phone") ;
            NotifyHaveMessages();
        }

        public async void NotifyHaveMessages()
        {
            if (!MessagesAlreadyWaiting)
            {
                MessagesAlreadyWaiting = true;
                var taskname = "Messages";
                await ImBusy.Task;
                this.LogToUI($"Reading Messages");
                var messages = new List<string>();
                messages.AddRange(MyMessages);
                MyMessages.Clear();
                foreach (var message in messages)
                {
                    this.WriteDirectToUI($"{taskname} ==>> Message: {message}");
                }
                MessagesAlreadyWaiting = false;
                this.LogToUI($"Phone back in pocket.");
            }
        }

        public async Task DoMyChores()
        {
            var taskname = "Morning Chores";

            // await Task.Yield();
            this.LogToUI($"Morning - what's on the agenda today!", taskname);
            this.LogToUI("Breakfast first", taskname);
            await HaveBreakfast();
            var doWashingUpChore = WashingUpChore(new Task[] { });
            var doTheWashingChore = TheWashingChore(new Task[] { });
            await GoToTheShops();
            await doWashingUpChore;
            var doHoovering = HooveringChore(new Task[] { doWashingUpChore });
            await GoToTheShops();
            Task.WaitAll(new Task[] { doTheWashingChore, doHoovering });
            this.LogToUI("All done, feet up.", taskname);
            this.WriteDirectToUI("Daydream of a proper job!");
        }

        public async Task HaveBreakfast()
        {
            var taskname = "Breakfast";
            this.LogToUI($"In the fridge", taskname);
            this.LogToUI($"No eggs, so it's toast only.", taskname);
            ShoppingList.Add("Eggs");
            this.LogToUI($"Last two pieces of bread used up", taskname);
            ShoppingList.Add("Bread");
            await LongRunTask.RunAsync(5, $"Eating");
            this.LogToUI($" ???? No Wine in fridge?", taskname);
            ShoppingList.Add("Wine");
        }

        public async Task WashingUpChore(Task[] awaitlist)
        {
            var taskname = $"Washing Up";
            var threadname = Thread.CurrentThread.Name;
            var completiontask = new Task(() =>
            {
                ImBusy.SetBusy("Doing the Washing Up");
                LongRunTask.Run(10, $"Washing up");
                ImBusy.SetIdle();
                this.LogToUI($"Washing Up Done", taskname);
            });
            //var completiontask = new Task(() =>
            //{
            //    LongRunTask.Run(10, $"Washing up");
            //    this.LogToUI($"Washing Up Done", taskname);
            //});

            this.LogToUI($"Check if I can do the washing up (any excuse will do!)",taskname);
            if (this.OurStocks.WashingUpLiquid < 1)
            {
                this.LogToUI($"mmmm - can't find any Washing Up Liquid", taskname);
                ShoppingList.Add("Washing Up Liquid");
                this.LogToUI($"Can't continue till we have some washing Up Liquid!", taskname);
                await ShoppingList.ShoppingTask;
            }
            Task.WaitAll(awaitlist);
            this.LogToUI($"Back to the sink", taskname);
            completiontask.RunSynchronously();
        }

        public async Task HooveringChore(Task[] awaitlist)
        {
            var taskname = "Hoovering";

            var task = new Task(async () =>
            {
                ImBusy.SetBusy("Hoovering");
                await LongRunTask.RunAsync(10, $"Chained to the vacuum");
                ImBusy.SetIdle();
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
            Task.WaitAll(awaitlist);
            task.RunSynchronously();
        }

        public async Task TheWashingChore(Task[] awaitlist)
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
            Task.WaitAll(awaitlist);
            this.LogToUI($"Add the powder, Click the button and stand back", taskname);
            this.LogToUI($"washing...", taskname);
            await LongRunTask.RunAsync(10, "Washing Machine running");
            this.LogToUI($"PING!! PING!!! Washing complete!", taskname);
        }

        public async Task GoToTheShops()
        {
            var taskname = "Shopping";
            var threadname = Thread.CurrentThread.Name;
            var message = $"[{threadname}]>[{taskname}]";
            var again = this.ShoppingList.TripsToShop == 0 ? "" : " again";
            if (ShoppingList.NeedToShop)
            {
                await LongRunTask.RunAsync(4, $"Heading out to the shops{again}");
                foreach (var item in ShoppingList)
                {
                    await LongRunTask.RunAsync(1, $"Getting {item}");
                }
                await LongRunTask.RunAsync(1, $"Heading home");
                this.LogToUI($"Back Home. Shopping done.", taskname);
                ShoppingList.ShoppingDone();
            }
        }
    }
}

