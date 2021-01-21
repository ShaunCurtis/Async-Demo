using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Async_Demo
{

    public class PhoneMessengerService : UILogger
    {
        public bool Live
        {
            get => _Live;
            set
            {
                if (value && !_Live)
                    this.MessengerTask = MessengeGenerator();
                _Live = value;
            }
        }

        public event EventHandler PingMessage;

        private bool _Live = true;

        protected Task MessengerTask { get; set; }

        public PhoneMessengerService(Action<string> uiLogger)
        {
            this.UIMessenger = uiLogger;
            this.CallerName = "Messenger Service";
            this.LogThreadType();
            this.MessengerTask = MessengeGenerator();
        }

        private async Task MessengeGenerator()
        {
            this.LogThreadType("Messenger");

            do
            {
                await Task.Delay(3000);
                PingMessage?.Invoke($"Hey, stuff going on at {DateTime.Now.ToLongTimeString()}!", EventArgs.Empty);
            } while (_Live);
        }
    }
}

