/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using System.Threading.Tasks;
using System.Threading;
using AsyncDemoLibrary;

namespace AsyncChores
{
    class Program : BaseClass
    {

        //static void Main(string[] args)
        //{
        //    LogThreadType();
        //    var phonemessengerService = new PhoneMessengerService(LogToConsole);
        //    var phonemessengerServiceTask = Task.Run(() => phonemessengerService.Run());
        //    var me = new UnemployedProgrammer(phonemessengerService, LogToConsole);
        //    var chores = new MyChores(me, phonemessengerService, LogToConsole);
        //    // var chorestask = Task.Run(() => chores.Start(me)); 
        //    var chorestask = chores.Start(me);
        //    chorestask.Wait();
        //}

        static async Task Main(string[] args)
        {
            UILogger.LogThreadType(LogToConsole, "Main");
            var phonemessengerService = new PhoneMessengerService(LogToConsole);
            var phonemessengerServiceTask = Task.Run(() => phonemessengerService.Run());
            var me = new UnemployedProgrammer(phonemessengerService, LogToConsole);
            var chores = new JobScheduler(me, LogToConsole);
            var chorestask = chores.Start(me);
            // var chorestask = Task.Run(() => chores.Start(me)); 
            // chorestask.Wait();
            await chorestask;
        }

        static void LogToConsole(string message) =>
            Console.WriteLine(message);

    }
}
