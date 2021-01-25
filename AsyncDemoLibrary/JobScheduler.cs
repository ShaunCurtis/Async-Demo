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

        private UnemployedProgrammer Me;

        public JobScheduler(UnemployedProgrammer person, Action<string> uiLogger)
        {
            this.CallerName = "Job Scheduler";

            this.UIMessenger = uiLogger;
            this.Me = person;
            this.Me.StockList = OurStocks;
        }

        public async Task Start(UnemployedProgrammer me = null)
        {
            var taskTitle = "Run Jobs";
            this.LogThreadType();
            this.Me = me ?? this.Me;

            this.Me.QueueJob(new GoodMorningJob(PriorityType.Priority));
            this.Me.QueueJob(new BreakfastJob(PriorityType.Priority));

            await me.PriorityJobsMonitorAsync();

            this.Me.QueueJob(new LaundryJob(PriorityType.Normal));
            this.Me.QueueJob(new WashingUpJob(PriorityType.Priority));
            this.Me.QueueJob(new GoToShopsJob(PriorityType.Priority));

            await me.PriorityJobsMonitorAsync();

            this.LogToUI("First Priority Done.", taskTitle);

            this.Me.QueueJob(new HooverJob(PriorityType.Priority));
            this.Me.QueueJob(new GoToShopsJob(PriorityType.Priority));

            await me.AllJobsMonitorAsync();

            this.LogToUI("All done, feet up.", taskTitle);
            this.WriteDirectToUI("Daydream of a proper job!");
        }

    }
}

