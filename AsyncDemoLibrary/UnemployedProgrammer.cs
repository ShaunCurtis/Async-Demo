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

        private string Message = "Doing Things";

        private List<Task> NormalJobs { get; } = new List<Task>();

        private List<Task> PriorityJobs { get; } = new List<Task>();

        private List<PersonalJob> _StackedJobs { get; } = new List<PersonalJob>();

        public bool AddTasktoList(PersonalJob job, bool startnow = true)
        {
            if (job != null)
                this._StackedJobs.Add(job);
            if (!this.JobsRunning && startnow)
                StartJobs();
            return true;
        }

        public bool AddTaskstoList(List<PersonalJob> jobs, bool startnow = true)
        {
            foreach (var job in jobs)
            {
                this._StackedJobs.Add(job);
            }
            if (startnow)
                StartJobs();
            return true;
        }

        private TaskCompletionSource<bool> PriorityTasksCompletionSource = new TaskCompletionSource<bool>();

        private bool PriorityJobRunnerActive;

        private Task RunPriorityJobsAsync()
        {
            if (!this.PriorityJobRunnerActive)
            {
                // Don't stop processing till both job queues are empty

                this.PriorityJobRunnerActive = true;
                // While routine makes sure we haven't been assigned new Priority tasks that are still running while we were busy
                while (PriorityJobs.Count > 0)
                {
                    Task.WaitAll(PriorityJobs.ToArray());
                    this.ClearTaskList(PriorityJobs);
                }
                this.PriorityTasksCompletionSource.SetResult(true);
            }
            return Task.CompletedTask;
        }

        private bool PriorityJobsRunning;

        private Task PriorityJobsMonitorAsync()
        {
            if (!this.PriorityJobsRunning)
            {
                this.PriorityJobsRunning = true;

                // While routine makes sure we haven't been assigned new Priority tasks that are still running while we were busy
                while (PriorityJobs.Count > 0)
                {
                    Task.WaitAll(PriorityJobs.ToArray());
                    this.ClearTaskList(PriorityJobs);
                }
                this.PriorityJobsRunning = false;
                this.PriorityTasksCompletionSource.SetResult(true);
            }
            return Task.CompletedTask;
        }


        private TaskCompletionSource<bool> AllTasksCompletionSource = new TaskCompletionSource<bool>();

        private bool NormalJobRunnerActive;

        private Task RunNormalJobsAsync()
        {
            if (!this.NormalJobRunnerActive)
            {

                this.NormalJobRunnerActive = true;
                // While routine makes sure we haven't been assigned new tasks that are still running while we were busy
                while (NormalJobs.Count > 0)
                {
                    // We get a list of all jobs so we don't complete until all jobs have run to completion
                    var jobs = new List<Task>();
                    {
                        jobs.AddRange(this.PriorityJobs);
                        jobs.AddRange(this.NormalJobs);
                    }
                    Task.WaitAll(jobs.ToArray());
                    this.ClearTaskList(NormalJobs);
                }
                this.AllTasksCompletionSource.SetResult(true);
                this.NormalJobRunnerActive = false;
            }
            return Task.CompletedTask;
        }

        private bool JobsRunning;

        private Task MonitorAllJobsAsync()
        {
            if (!this.JobsRunning)
            {
                this.JobsRunning = true;

                if (this.PriorityJobs.Count > 0 && !this.PriorityTasks.IsCompleted) this.PriorityJobsMonitorAsync();

                // While routine makes sure we haven't been assigned new tasks that are still running while we were busy
                while (NormalJobs.Count > 0)
                {
                    // We get a list of all jobs so we don't complete until all jobs have run to completion
                    var jobs = new List<Task>();
                    {
                        jobs.AddRange(this.PriorityJobs);
                        jobs.AddRange(this.NormalJobs);
                    }
                    Task.WaitAll(jobs.ToArray());

                    // Clear out all the finished jobs
                    this.ClearTaskList(NormalJobs);

                    // If new priority jobs have been loaded while we are still running normal jobs restart the PriorityJobMonitor
                    if (this.PriorityJobs.Count > 0 && !this.PriorityTasks.IsCompleted) this.PriorityJobsMonitorAsync();
                }
                this.AllTasksCompletionSource.SetResult(true);
                this.NormalJobRunnerActive = false;
            }
            return Task.CompletedTask;
        }

        private void ClearTaskList(List<Task> tasks)
        {
            var removelist = tasks.Where(item => item.IsCompleted).ToList();
            removelist.ForEach(item => tasks.Remove(item));
        }

        private void StartJobs()
        {
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
                if (this.PriorityJobs.Count > 0 && !this.PriorityTasks.IsCompleted) this.MonitorAllJobsAsync();
                if (this.NormalJobs.Count > 0 && !this.AllTasks.IsCompleted) this.MonitorAllJobsAsync();
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
            if (!string.IsNullOrWhiteSpace(message)) this.LogToUI($" I'm Busy >  {Message}");
            this.State = StateType.Busy;
        }

        public void ImIdle(string message = null)
        {
            if (!string.IsNullOrWhiteSpace(message)) this.LogToUI($"I'm Idle > {Message}");
            this.State = StateType.Idle;
        }

        public void ImMultitasking(string message = null)
        {
            if (!string.IsNullOrWhiteSpace(message)) this.LogToUI($"I'm multitasking > {Message}");
            this.State = StateType.Multitasking;
        }

/// <summary>
/// Public Method to change State
/// </summary>
/// <param name="state"></param>
/// <param name="message"></param>
        public void ChangeState(StateType state, string message = null)
        {
            if (state != this.State)
            {
                if (!string.IsNullOrWhiteSpace(message)) this.LogToUI($"I'm {state.ToString()} > {Message}");
                this.State = state;
            }
        }

        /// <summary>
        /// Event trigger when stats is changed
        /// </summary>
        /// <param name="oldstate"></param>
        /// <param name="newstate"></param>
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

