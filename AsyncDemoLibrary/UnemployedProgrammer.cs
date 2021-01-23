/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    public enum PriorityType { Normal, MustCompleteNow }

    public enum StateType { Idle, Multitasking, Busy }

    public class PersonalJob
    {

        public PriorityType Priority { get; set; }

        public Func<UnemployedProgrammer, Task> Job { get; set; }
    }

    public class UnemployedProgrammer : BaseClass
    {
        public UnemployedProgrammer(PhoneMessengerService messenger, Action<string> uiLogger)
        {
            this.MessengerService = messenger;
            this.UIMessenger = uiLogger;
            MessengerService.PingMessage += OnMessageReceived;
        }

        #region Messenging Stuff

        private PhoneMessengerService MessengerService;

        private bool MessagesAlreadyWaiting = false;

        private List<string> MyMessages = new List<string>();

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
                await DoNotDisturbTask;
                this.LogToUI($"Reading Messages");
                var messages = new List<string>();
                messages.AddRange(MyMessages);
                MyMessages.Clear();
                foreach (var message in messages)
                {
                    this.WriteDirectToUI($"{taskname} ==>> Message: {message}");
                }
                MessagesAlreadyWaiting = false;
            }
        }

        #endregion

        #region Jobs Management Stuff

        public Task PriorityTasks =>
            PriorityTasksCompletionSource != null ?
            PriorityTasksCompletionSource.Task :
            Task.CompletedTask;

        public Task AllTasks =>
            AllTasksCompletionSource != null ?
            AllTasksCompletionSource.Task :
            Task.CompletedTask;

        private string Message { get; set; } = "On task";

        private List<Task> NormalJobs { get; } = new List<Task>();

        private List<Task> PriorityJobs { get; } = new List<Task>();

        private List<PersonalJob> _StackedJobs { get; } = new List<PersonalJob>();

        public void AddTasktoList(PersonalJob job, bool startnow = true)
        {
            if (job != null)
                this._StackedJobs.Add(job);
            if (!this.JobsRunning && startnow)
                LoadJobs();
        }

        public void AddTaskstoList(List<PersonalJob> jobs)
        {
            foreach (var job in jobs)
                this._StackedJobs.Add(job);
                LoadJobs();
        }

        private TaskCompletionSource<bool> PriorityTasksCompletionSource = new TaskCompletionSource<bool>();

        private bool PriorityJobsRunning;

        public Task PriorityJobsMonitorAsync()
        {
            if (!this.PriorityJobsRunning)
            {
                this.PriorityJobsRunning = true;

                if (PriorityJobs.Count > 0)
                    return Task.WhenAll(PriorityJobs.ToArray());
            }
            return Task.CompletedTask;
        }

        private TaskCompletionSource<bool> AllTasksCompletionSource = new TaskCompletionSource<bool>();

        private bool JobsRunning;

        public Task AllJobsMonitorAsync()
        {
            if (!this.JobsRunning)
            {
                this.JobsRunning = true;

                    var jobs = new List<Task>();
                    {
                        jobs.AddRange(this.PriorityJobs);
                        jobs.AddRange(this.NormalJobs);
                    }
                    return Task.WhenAll(jobs.ToArray());
            }
            return Task.CompletedTask;
        }

        public void ClearPriorityTaskList()
        {
            var removelist = this.PriorityJobs.Where(item => item.IsCompleted).ToList();
            removelist.ForEach(item => this.PriorityJobs.Remove(item));
            if (this.PriorityJobs.Count == 0)
            {
                this.PriorityJobsRunning = false;
                this.PriorityTasksCompletionSource.SetResult(true);
            }
        }

        public void ClearAllTaskLists()
        {
            this.ClearPriorityTaskList();
            var removelist = this.NormalJobs.Where(item => item.IsCompleted).ToList();
            removelist.ForEach(item => this.NormalJobs.Remove(item));
            if (this.NormalJobs.Count == 0)
            {
                this.JobsRunning = false;
                this.AllTasksCompletionSource.SetResult(true);
            }
        }

        private void LoadJobs()
        {
            ClearAllTaskLists();
            if (_StackedJobs.Count > 0)
            {
                foreach (var job in _StackedJobs)
                {
                    if (AllTasksCompletionSource.Task.IsCompleted)
                        AllTasksCompletionSource = new TaskCompletionSource<bool>();
                    if (job.Priority == PriorityType.MustCompleteNow)
                    {
                        if (PriorityTasksCompletionSource.Task.IsCompleted)
                            PriorityTasksCompletionSource = new TaskCompletionSource<bool>();
                        var task = job.Job.Invoke(this);
                        this.PriorityJobs.Add(task);
                    }
                    else
                    {
                        var task = job.Job.Invoke(this);
                        this.NormalJobs.Add(task);
                    }
                }
                _StackedJobs.Clear();
            }
        }

        #endregion

        #region State Stuff

        public StateType State
        {
            get => this._State;
            protected set
            {
                if (value != this._State)
                {
                    var oldstate = this._State;
                    this._State = value;
                    this.StateChanged(oldstate, value);
                }
            }
        }

        public bool IsBusy => this.State == StateType.Busy;

        public bool IsMultitasking => this.State == StateType.Multitasking;

        public bool IsIdle => this.State == StateType.Idle;

        public Task DoNotDisturbTask
        {
            get
            {
                if (this.IsBusy && DNDTasksCompletionSource.Task.IsCompleted)
                    DNDTasksCompletionSource = new TaskCompletionSource<bool>();
                return DNDTasksCompletionSource.Task;
            }
        }

        private StateType _State = StateType.Idle;

        private TaskCompletionSource<bool> DNDTasksCompletionSource = new TaskCompletionSource<bool>();

        public void ImBusy(string message = null)
        {
            message = message ?? Message;
            if (!string.IsNullOrWhiteSpace(message)) this.LogToUI($" I'm Busy > {message}");
            this.State = StateType.Busy;
        }

        public void ImIdle(string message = null)
        {
            message = message ?? Message;
            if (!string.IsNullOrWhiteSpace(message)) this.LogToUI($"I'm Idle > {message}");
            this.State = StateType.Idle;
        }

        public void ImMultitasking(string message = null)
        {
            message = message ?? Message;
            if (!string.IsNullOrWhiteSpace(message)) this.LogToUI($"I'm multitasking > {message}");
            this.State = StateType.Multitasking;
        }

        private void StateChanged(StateType oldstate, StateType newstate)
        {
            if (oldstate == StateType.Busy && newstate != StateType.Busy)
                DNDTasksCompletionSource.SetResult(true);
            else if (newstate == StateType.Busy && DNDTasksCompletionSource.Task.IsCompleted)
                DNDTasksCompletionSource = new TaskCompletionSource<bool>();
        }

        #endregion
    }
}

