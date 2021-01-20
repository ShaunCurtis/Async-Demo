using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Async_Demo
{
    public class Chores
    {

        StockList OurStocks = new StockList()
        {
            HooverBags = 1,
            WashingUpLiquid = 0,
            SoapPowder = 0
        };

        ShoppingJob ShoppingList { get; } = new ShoppingJob();

        public Task ChoresTask { get; set; }

        private MessengerService Messenger;

        private List<string> MyMessages = new List<string>();

        private Busy BusyState = new Busy(false);

        private bool MessagesAlreadyWaiting = false;

        private ILongRunningTask LongRunTask;

        public Chores(MessengerService messenger)
        {
            if (string.IsNullOrWhiteSpace(Thread.CurrentThread.Name))
                Thread.CurrentThread.Name = "Chores Thread";
            var threadname = Thread.CurrentThread.IsThreadPoolThread ? "Threadpool" : "other";
            Console.WriteLine($"[Chores] running on a {threadname} thread Name: {Thread.CurrentThread.Name}");
            LongRunTask = new PrimeTask();
            Messenger = messenger;
            Messenger.PingMessage += OnMessageReceived;
            ChoresTask = DoMyChores();
        }

        public void OnMessageReceived(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Thread.CurrentThread.Name))
                Thread.CurrentThread.Name = "Phone Thread";
            var taskname = "My Phone";
            var threadname = Thread.CurrentThread.Name;
            var message = $"[{threadname}]>[{taskname}]";
            MyMessages.Add((string)sender);
            Console.WriteLine($"{message} Ping! New message at {DateTime.Now.ToLongTimeString()}. You have {MyMessages.Count} unread messages.");
            NotifyHaveMessages();
        }

        public async void NotifyHaveMessages()
        {
            if (!MessagesAlreadyWaiting)
            {
                MessagesAlreadyWaiting = true;
                var taskname = "Messages";
                await BusyState.Task;
                Console.WriteLine($"{taskname} > Reading Messages");
                var messages = new List<string>();
                messages.AddRange(MyMessages);
                MyMessages.Clear();
                foreach (var threadname in messages)
                {
                    Console.WriteLine($"{taskname} ==>> {threadname}");
                }
                MessagesAlreadyWaiting = false;
                Console.WriteLine($"{taskname} >> Phone back in pocket.");
            }
        }

        public async Task DoMyChores()
        {
            var chores = this.DoMorningChores();
            await chores;
            Console.WriteLine("Daydream of a proper job!");
        }

        public async Task DoMorningChores()
        {
            var taskname = "Morning Chores";
            var threadname = Thread.CurrentThread.Name;
            var message = $"[{threadname}]>[{taskname}]";

            // await Task.Yield();
            Console.WriteLine($"{message} - Morning - what's on the agenda today!");
            Console.WriteLine("Breakfast first");
            await HaveBreakfast();
            var doWashingUpChore = WashingUpChore(new Task[] { });
            var doTheWashingChore = TheWashingChore(new Task[] { });
            await GoToTheShops();
            await doWashingUpChore;
            var doHoovering = HooveringChore(new Task[] { doWashingUpChore });
            await GoToTheShops();
            Task.WaitAll(new Task[] { doTheWashingChore, doHoovering });
            Console.WriteLine("All done, feet up.");
        }

        public async Task HaveBreakfast()
        {
            var taskname = "Breakfast";
            var threadname = Thread.CurrentThread.Name;
            var message = $"[{threadname}]>[{taskname}]";
            Console.WriteLine($"{message} > In the fridge");
            Console.WriteLine($"{message} > No eggs, so it's toast only.");
            ShoppingList.Add("Eggs");
            Console.WriteLine($"{message} > Last two pieces of bread used up");
            ShoppingList.Add("Bread");
            await LongRunTask.RunAsync(5, $"{message} > Eating");
            Console.WriteLine($"{message} > ???? No Wine in fridge?");
            ShoppingList.Add("Wine");
        }

        public async Task WashingUpChore(Task[] awaitlist)
        {
            var taskname = $"Washing Up";
            var threadname = Thread.CurrentThread.Name;
            var message = $"[{threadname}]>[{taskname}]";
            taskname = $"[{threadname}][{taskname}]";
            var task = new Task(async () =>
            {
                BusyState.SetBusy("Doing the Washing Up");
                await LongRunTask.RunAsync(5, $"{message} > Washing up");
                BusyState.Finished();
            });

            Console.WriteLine($"{message} > Check if I can do the washing up (any excuse will do!)");
            if (this.OurStocks.WashingUpLiquid < 1)
            {
                Console.WriteLine($"{message} > mmmm - can't find any Washing Up Liquid");
                ShoppingList.Add("Washing Up Liquid");
                Console.WriteLine($"{message} > Can't continue till we have some washing Up Liquid!");
                await ShoppingList.ShoppingTask;
            }
            Task.WaitAll(awaitlist);
            task.RunSynchronously();
        }

        public async Task HooveringChore(Task[] awaitlist)
        {
            var taskname = "Hoovering";
            var threadname = Thread.CurrentThread.Name;
            var message = $"[{threadname}]>[{taskname}]";
            var task = new Task(async () =>
            {
                BusyState.SetBusy("Hoovering");
                await LongRunTask.RunAsync(10, $"{message} > On the machine");
                BusyState.Finished();
            });
            Console.WriteLine($"{message} > Get the machine out");
            if (this.OurStocks.HooverBags < 1)
            {
                Console.WriteLine($"{message} > No bags!");
                ShoppingList.Add($"Hoover Bags");
                Console.WriteLine($"{message} > Can't continue till we have some bags!");
                await ShoppingList.ShoppingTask;
            }
            else
                Console.WriteLine($"{message} > Thank god there's bags, I've already done the shopping once!");
            Task.WaitAll(awaitlist);
            task.RunSynchronously();
        }

        public async Task TheWashingChore(Task[] awaitlist)
        {
            var taskname = "Clothes Washing";
            var threadname = Thread.CurrentThread.Name;
            var message = $"[{threadname}]>[{taskname}]";
            Console.WriteLine($"{message} > Loading the washing machine and");
            Console.WriteLine($"{message} > Check if we have powder");
            if (this.OurStocks.SoapPowder < 1)
            {
                Console.WriteLine($"{message} > No soap powder!");
                ShoppingList.Add($"Soap Powder");
                Console.WriteLine($"{message} > Can't continue till we have some powder!");
                await ShoppingList.ShoppingTask;
            }
            Task.WaitAll(awaitlist);
            Console.WriteLine($"{message} > Add the powder, Click the button and stand back");
            Console.WriteLine($"{message} > washing...");
            await Task.Delay(7 * 1000);
            //await DoSomeWork(1000, $"{taskname} > Washing");
            Console.WriteLine($"{message} > Washing complete!");
        }

        public async Task GoToTheShops()
        {
            var taskname = "Shopping";
            var threadname = Thread.CurrentThread.Name;
            var message = $"[{threadname}]>[{taskname}]";
            var again = this.ShoppingList.TripsToShop == 0 ? "" : " again";
            if (ShoppingList.NeedToShop)
            {
                await LongRunTask.RunAsync(4, $"{taskname} > Heading out to the shops{again}");
                foreach (var item in ShoppingList)
                {
                    await LongRunTask.RunAsync(1, $"{taskname} > Getting {item}");
                }
                await LongRunTask.RunAsync(1, $"{message} > Heading home");
                Console.WriteLine($"{message} > Back Home. Shopping done.");
                ShoppingList.ShoppingDone();
            }
        }
    }
}

