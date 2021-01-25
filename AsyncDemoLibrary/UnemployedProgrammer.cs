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
            this.ShoppingList = new ShoppingJob(uiLogger);
            this.MessengerService = messenger;
            this.UIMessenger = uiLogger;
            MessengerService.PingMessage += OnMessageReceived;
            this.IdleTaskManager.SetResult();
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

        private bool LogStateChanges = false;

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
                        if (!BusyTaskManager.Task.IsCompleted)
                        {
                            BusyTaskManager.SetResult();
                            if (MultitaskingTaskManager.Task.IsCompleted) MultitaskingTaskManager = new TaskCompletionSource();
                            if (IdleTaskManager.Task.IsCompleted) IdleTaskManager = new TaskCompletionSource();
                        }
                        break;
                    case StateType.Multitasking:
                        if (!MultitaskingTaskManager.Task.IsCompleted)
                        {
                            MultitaskingTaskManager.SetResult();
                            if (IdleTaskManager.Task.IsCompleted) IdleTaskManager = new TaskCompletionSource();
                            if (BusyTaskManager.Task.IsCompleted) BusyTaskManager = new TaskCompletionSource();
                        }
                        break;
                    default:
                        if (!IdleTaskManager.Task.IsCompleted)
                        {
                            IdleTaskManager.SetResult();
                            if (MultitaskingTaskManager.Task.IsCompleted) MultitaskingTaskManager = new TaskCompletionSource();
                            if (BusyTaskManager.Task.IsCompleted) BusyTaskManager = new TaskCompletionSource();
                        }
                        break;
                }
            }
        }

        public Task IsBusyTask
        {
            get
            {
                if (BusyTaskManager.Task.IsCompleted && !this.IsBusy)
                    BusyTaskManager = new TaskCompletionSource();
                return BusyTaskManager.Task;
            }
        }

        public Task IsIdleTask
        {
            get
            {
                if (IdleTaskManager.Task.IsCompleted && !this.IsIdle)
                    IdleTaskManager = new TaskCompletionSource();
                return IdleTaskManager.Task;
            }
        }

        public Task IsMultiTaskingTask
        {
            get
            {
                if (MultitaskingTaskManager.Task.IsCompleted && !this.IsMultitasking)
                    MultitaskingTaskManager = new TaskCompletionSource();
                return MultitaskingTaskManager.Task;
            }
        }

        public bool IsBusy => this.State == StateType.Busy;

        public bool IsMultitasking => this.State == StateType.Multitasking;

        public bool IsIdle => this.State == StateType.Idle;

        private TaskCompletionSource IdleTaskManager = new TaskCompletionSource();
        private TaskCompletionSource MultitaskingTaskManager = new TaskCompletionSource();
        private TaskCompletionSource BusyTaskManager = new TaskCompletionSource();

        public void ImBusy(string message = null)
        {
            message = message ?? Message;
            if (this.LogStateChanges && !string.IsNullOrWhiteSpace(message)) this.LogToUI($"Busy > {message}");
            this.State = StateType.Busy;
        }

        public void ImIdle(string message = null)
        {
            message = message ?? Message;
            if (this.LogStateChanges && !string.IsNullOrWhiteSpace(message)) this.LogToUI($"Idle > {message}");
            this.State = StateType.Idle;
        }

        public void ImMultitasking(string message = null)
        {
            message = message ?? Message;
            if (this.LogStateChanges && !string.IsNullOrWhiteSpace(message)) this.LogToUI($"Multitasking > {message}");
            this.State = StateType.Multitasking;
        }

        #endregion

        #region Jobs Management Stuff

        private int NewJobNo => _JobNo++;

        private int _JobNo;

        public ShoppingJob ShoppingList { get; private set; }

        public StockList StockList { get; set; } = new StockList();

        private Queue<IJobItem> JobQueue { get; } = new Queue<IJobItem>();

        public void QueueJob(IJobItem job, bool startnow = true)
        {
            this.JobQueue.Enqueue(job);
            if (startnow)
                LoadAndRunJobs();
        }

        public void QueueJobs(List<IJobItem> jobs)
        {
            foreach (var job in jobs)
                this.JobQueue.Enqueue(job);
            LoadAndRunJobs();
        }

        private string Message { get; set; } = "On Task";

        private List<IJobItem> NormalJobs { get; } = new List<IJobItem>();

        private List<IJobItem> PriorityJobs { get; } = new List<IJobItem>();

        public Task PriorityTasks =>
            PriorityTasksController != null ?
            PriorityTasksController.Task :
            Task.CompletedTask;

        public Task AllTasks =>
            AllTasksController != null ?
            AllTasksController.Task :
            Task.CompletedTask;

        private TaskCompletionSource<bool> PriorityTasksController { get; set; } = new TaskCompletionSource<bool>();

        private TaskCompletionSource<bool> AllTasksController { get; set; } = new TaskCompletionSource<bool>();

        private void LoadAndRunJobs()
        {
            ClearAllTaskLists();
            while (this.JobQueue.Count != 0)
            {
                var job = this.JobQueue.Dequeue();
                if (AllTasksController.Task.IsCompleted)
                    AllTasksController = new TaskCompletionSource<bool>();
                if (job.Priority == PriorityType.Priority)
                {
                    if (PriorityTasksController.Task.IsCompleted)
                        PriorityTasksController = new TaskCompletionSource<bool>();
                    job.RunJob(this, this.NewJobNo);
                    this.PriorityJobs.Add(job);
                }
                else
                {
                    job.RunJob(this, this.NewJobNo);
                    this.NormalJobs.Add(job);
                }
            }
        }

        public Task PriorityJobsMonitorAsync()
        {
            var jobs = this.GetPriorityStartedTasksList();
            if (jobs.Count > 0)
                return Task.WhenAll(jobs.ToArray());
            else
                return Task.CompletedTask;
        }

        public Task AllJobsMonitorAsync()
        {
            var jobs = this.GetAllStartedTasksList();
            if (jobs.Count > 0)
                return Task.WhenAll(jobs.ToArray());
            else
                return Task.CompletedTask;
        }

        private List<Task> GetAllStartedTasksList()
        {
            this.ClearAllTaskLists();
            var list = new List<Task>();
            foreach (var item in PriorityJobs.Where(job => !job.JobTask.IsCompleted))
                list.Add(item.JobTask);
            foreach (var item in NormalJobs.Where(job => !job.JobTask.IsCompleted))
                list.Add(item.JobTask);
            return list;
        }

        private List<Task> GetPriorityStartedTasksList()
        {
            this.ClearPriorityTaskList();
            var list = new List<Task>();
            foreach (var item in PriorityJobs.Where(job => !job.JobTask.IsCompleted))
                list.Add(item.JobTask);
            return list;
        }

        public void ClearAllTaskLists()
        {
            this.ClearPriorityTaskList();
            var removelist = this.NormalJobs.Where(item => item.JobTask.IsCompleted).ToList();
            removelist.ForEach(item => this.NormalJobs.Remove(item));
            if (this.NormalJobs.Count == 0)
                if (!this.AllTasksController.Task.IsCompleted)
                    this.AllTasksController.SetResult(true);
        }

        public void ClearPriorityTaskList()
        {
            var removelist = this.PriorityJobs.Where(item => item.JobTask.IsCompleted).ToList();
            removelist.ForEach(item => this.PriorityJobs.Remove(item));
            if (this.PriorityJobs.Count == 0)
                if (!this.PriorityTasksController.Task.IsCompleted)
                    this.PriorityTasksController.SetResult(true);
        }

        #endregion
    }
}
