using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    class GoodMorningJob : IJobItem
    {
        public PriorityType Priority { get; set; }

        public Task JobTask => JobTaskSource == null ? Task.CompletedTask: this.JobTaskSource.Task ;

        private TaskCompletionSource JobTaskSource { get; set; }

        public Task Run(UnemployedProgrammer person)
        {
            person.ImMultitasking();
            var taskTitle = "Good Morning";
            person.LogToUI($"Morning - what's on the agenda today!", taskTitle);
            person.LogToUI("Breakfast first", taskTitle);
            person.ImIdle();
            return Task.CompletedTask;
        }

    }
}
