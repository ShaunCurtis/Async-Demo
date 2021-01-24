using System.Threading.Tasks;

namespace AsyncDemoLibrary
{
    public interface IJobItem
    {

        public PriorityType Priority { get; set; }

        public Task JobTask { get; }

        public Task Run(UnemployedProgrammer person);

    }
}
