/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using System.Threading.Tasks;

namespace AsyncDemoLibrary
{

    public class PhoneMessengerService : UILogger
    {
        public bool Live
        {
            get => _Live;
            set
            {
                if (value && !_Live)
                    this.MessengerTask = MessengerGenerator();
                _Live = value;
            }
        }

        public event EventHandler PingMessage;

        private bool _Live = true;

        private LongRunningTasks LongRunTask;

        protected Task MessengerTask { get; set; }

        public PhoneMessengerService(Action<string> uiLogger)
        {
            this.UIMessenger = uiLogger;
            this.CallerName = "Messenger Service";
            LongRunTask = new LongRunningTasks(uiLogger);
        }

        public Task Run()
        {
            this.LogThreadType(this.CallerName);
            this.MessengerTask = MessengerGenerator();
            return this.MessengerTask;
        }

        private async Task MessengerGenerator()
        {
            this.LogThreadType("Messenger");
            var firsttrip = true;
            do
            {
                await Task.Delay(3000);
                if (firsttrip) this.LogThreadType();
                PingMessage?.Invoke($"Hey, stuff going on at {DateTime.Now.ToLongTimeString()}!", EventArgs.Empty);
                firsttrip = false;
            } while (_Live);
        }
    }
}

