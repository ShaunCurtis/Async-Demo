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
    public enum PriorityType { Normal, Priority }

    public enum StateType { Idle, Multitasking, Busy }

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
                var taskTitle = "Messages";
                await Task.WhenAny(new Task[] {this.IsMultiTaskingTask, this.IsIdleTask } );
                this.LogToUI($"Reading Messages");
                var messages = new List<string>();
                messages.AddRange(MyMessages);
                MyMessages.Clear();
                foreach (var message in messages)
                {
                    this.WriteDirectToUI($"{taskTitle} ==>> Message: {message}");
                }
                MessagesAlreadyWaiting = false;
            }
        }

        #endregion

        #region State Stuff

        private StateType _State = StateType.Idle;

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

        private void StateChanged(StateType oldstate, StateType newstate)
        {
            if (oldstate != newstate)
            {
                switch (newstate)
                {
                    case StateType.Busy:
                        if (!BusyTaskSource.Task.IsCompleted)
                        {
                            BusyTaskSource.SetResult();
                            if (MultitaskingTaskSource.Task.IsCompleted) MultitaskingTaskSource = new TaskCompletionSource();
                            if (IdleTaskSource.Task.IsCompleted) IdleTaskSource = new TaskCompletionSource();
                        }
                        break;
                    case StateType.Multitasking:
                        if (!MultitaskingTaskSource.Task.IsCompleted)
                        {
                            MultitaskingTaskSource.SetResult();
                            if (IdleTaskSource.Task.IsCompleted) IdleTaskSource = new TaskCompletionSource();
                            if (BusyTaskSource.Task.IsCompleted) BusyTaskSource = new TaskCompletionSource();
                        }
                        break;
                    default:
                        if (!IdleTaskSource.Task.IsCompleted)
                        {
                            IdleTaskSource.SetResult();
                            if (MultitaskingTaskSource.Task.IsCompleted) MultitaskingTaskSource = new TaskCompletionSource();
                            if (BusyTaskSource.Task.IsCompleted) BusyTaskSource = new TaskCompletionSource();
                        }
                        break;
                }
            }
        }

        public Task IsBusyTask
        {
            get
            {
                if (BusyTaskSource.Task.IsCompleted && !this.IsBusy)
                    BusyTaskSource = new TaskCompletionSource();
                return BusyTaskSource.Task;
            }
        }

        public Task IsIdleTask
        {
            get
            {
                if (IdleTaskSource.Task.IsCompleted && !this.IsIdle)
                    IdleTaskSource = new TaskCompletionSource();
                return IdleTaskSource.Task;
            }
        }

        public Task IsMultiTaskingTask
        {
            get
            {
                if (MultitaskingTaskSource.Task.IsCompleted && !this.IsMultitasking)
                    MultitaskingTaskSource = new TaskCompletionSource();
                return MultitaskingTaskSource.Task;
            }
        }

        public bool IsBusy => this.State == StateType.Busy;

        public bool IsMultitasking => this.State == StateType.Multitasking;

        public bool IsIdle => this.State == StateType.Idle;

        private TaskCompletionSource IdleTaskSource = new TaskCompletionSource();
        private TaskCompletionSource MultitaskingTaskSource = new TaskCompletionSource();
        private TaskCompletionSource BusyTaskSource = new TaskCompletionSource();

        public void ImBusy(string message = null)
        {
            message = message ?? Message;
            if (!string.IsNullOrWhiteSpace(message)) this.LogToUI($"Busy > {message}");
            this.State = StateType.Busy;
        }

        public void ImIdle(string message = null)
        {
            message = message ?? Message;
            if (!string.IsNullOrWhiteSpace(message)) this.LogToUI($"Idle > {message}");
            this.State = StateType.Idle;
        }

        public void ImMultitasking(string message = null)
        {
            message = message ?? Message;
            if (!string.IsNullOrWhiteSpace(message)) this.LogToUI($"Multitasking > {message}");
            this.State = StateType.Multitasking;
        }

        #endregion

        #region Jobs Management Stuff

        //public ShoppingJob ShoppingList { get; } = new ShoppingJob();

        private List<JobItem> JobQueue { get; } = new List<JobItem>();

        public void LoadNormalTask(Task task) => 
            this.NormalJobs.Add(task);

        public void LoadPriorityTask(Task task) =>
             this.PriorityJobs.Add(task);

        public void QueueJob(JobItem job, bool startnow = true)
        {
            this.JobQueue.Add(job);
            if (startnow)
                LoadAndRunJobs();
        }

        public void QueueJobs(List<JobItem> jobs)
        {
            foreach (var job in jobs)
                this.JobQueue.Add(job);
            LoadAndRunJobs();
        }

        private string Message { get; set; } = "On Task";

        private List<Task> NormalJobs { get; } = new List<Task>();

        private List<Task> PriorityJobs { get; } = new List<Task>();

        public Task PriorityTasks =>
            PriorityTasksCompletionSource != null ?
            PriorityTasksCompletionSource.Task :
            Task.CompletedTask;

        public Task AllTasks =>
            AllTasksCompletionSource != null ?
            AllTasksCompletionSource.Task :
            Task.CompletedTask;

        private TaskCompletionSource<bool> PriorityTasksCompletionSource { get; set; } = new TaskCompletionSource<bool>();

        private TaskCompletionSource<bool> AllTasksCompletionSource { get; set; } = new TaskCompletionSource<bool>();

        private void LoadAndRunJobs()
        {
            ClearAllTaskLists();
            if (JobQueue.Count > 0)
            {
                foreach (var job in JobQueue)
                {
                    if (AllTasksCompletionSource.Task.IsCompleted)
                        AllTasksCompletionSource = new TaskCompletionSource<bool>();
                    if (job.Priority == PriorityType.Priority)
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
                JobQueue.Clear();
            }
        }

        public Task PriorityJobsMonitorAsync()
        {
            ClearPriorityTaskList();
            if (PriorityJobs.Count > 0)
                return Task.WhenAll(PriorityJobs.ToArray());
            else
                return Task.CompletedTask;
        }

        public Task AllJobsMonitorAsync()
        {
            ClearAllTaskLists();
            var jobs = new List<Task>();
            {
                jobs.AddRange(this.PriorityJobs);
                jobs.AddRange(this.NormalJobs);
            }
            if (jobs.Count > 0)
                return Task.WhenAll(jobs.ToArray());
            else
                return Task.CompletedTask;
        }

        public void ClearAllTaskLists()
        {
            this.ClearPriorityTaskList();
            var removelist = this.NormalJobs.Where(item => item.IsCompleted).ToList();
            removelist.ForEach(item => this.NormalJobs.Remove(item));
            if (this.NormalJobs.Count == 0)
                this.AllTasksCompletionSource.SetResult(true);
        }

        public void ClearPriorityTaskList()
        {
            var removelist = this.PriorityJobs.Where(item => item.IsCompleted).ToList();
            removelist.ForEach(item => this.PriorityJobs.Remove(item));
            if (this.PriorityJobs.Count == 0)
                this.PriorityTasksCompletionSource.SetResult(true);
        }

        #endregion
    }
}

