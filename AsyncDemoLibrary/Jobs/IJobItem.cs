using System;
using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    public interface IJobItem
    {

        public PriorityType Priority { get; set; }

        public Task JobTask { get; }

        public JobStatusType JobStatus { get; }

        public int SequenceNo { get; }

        public Action<bool> JobClosedAction { get; set; }

        public void RunJob(UnemployedProgrammer person, int sequenceno);

    }
}
