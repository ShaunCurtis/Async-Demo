/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncDemoLibrary
{

    public class UnemployedProgrammer : UILogger
    {
        TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

        public bool IsBusy => this._IsBusy;

        private bool _IsBusy = false;

        private bool _ThingsToDo = false;

        private PhoneMessengerService MessengerService;

        private List<Task> PendingJobs { get; } = new List<Task>();

        private List<Task> BusyJobs { get; } = new List<Task>();

        private List<Func<UnemployedProgrammer, Task>> _StackedJobs { get; } = new List<Func<UnemployedProgrammer, Task>>();

        public Task DoNotDisturb { get; set; } = Task.CompletedTask;

        private bool MessagesAlreadyWaiting = false;

        private List<string> MyMessages = new List<string>();

        public UnemployedProgrammer(PhoneMessengerService messenger, Action<string> uiLogger)
        {
            this.MessengerService = messenger;
            this.UIMessenger = uiLogger;
            MessengerService.PingMessage += OnMessageReceived;
        }

        public void OnMessageReceived(object sender, EventArgs e)
        {
            MyMessages.Add((string)sender);
            this.LogToUI($"Ping! New message at {DateTime.Now.ToLongTimeString()}. You have {MyMessages.Count} unread messages.", "My Phone");
            NotifyHaveMessages();
        }

        public async void NotifyHaveMessages()
        {
            if (!MessagesAlreadyWaiting)
            {
                MessagesAlreadyWaiting = true;
                var taskname = "Messages";
                await DoNotDisturb;
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

        public bool AddTasktoList(Func<UnemployedProgrammer, Task> action)
        {
            if (action != null) _StackedJobs.Add(action);
            if (!_ThingsToDo) DoMyJobs();
            return true;
        }

        public bool AddTaskstoList(List<Func<UnemployedProgrammer, Task>> actions)
        {
            foreach (var action in actions)
            {
                if (action != null) _StackedJobs.Add(action);
            }
            if (!_ThingsToDo) DoMyJobs();
            return true;
        }

        private void DoMyJobs()
        {
            if (_StackedJobs.Count > 0)
            {
                this.taskCompletionSource = new TaskCompletionSource<bool>();
                _ThingsToDo = true;
                foreach (var job in _StackedJobs)
                {
                    var task = job.Invoke(this);
                    this.PendingJobs.Add(task);
                }
                Task.WaitAll(PendingJobs.ToArray());
                _ThingsToDo = false;
                this.taskCompletionSource.SetResult(true);
            }


        }

        public void ImBusy(string message = null)
        {
            if (!string.IsNullOrWhiteSpace(message)) this.LogToUI($"Busy >  {Message}");
            _IsBusy = true;
        }

        public void ImIdle(string message = null)
        {
            if (!string.IsNullOrWhiteSpace(message)) this.LogToUI($"Finished > {Message}");
            _IsBusy = false;
        }

        public void SetIdle()
        {
            var taskname = "Busy Task";
            taskCompletionSource.SetResult(true);
            this.LogToUI($"finished {Message}", taskname);
        }

        public Task Task =>
            taskCompletionSource != null ?
            taskCompletionSource.Task :
            Task.CompletedTask;

        private string Message = "Doing Things";

        public void SetBusy(string message = null)
        {
            Message = message ?? Message;

            var taskname = "Busy Task";
            taskCompletionSource = new TaskCompletionSource<bool>();
            this.LogToUI($"Busy {Message}", taskname);
        }
    }
}

