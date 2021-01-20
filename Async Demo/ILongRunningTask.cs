using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Async_Demo
{

        public interface ILongRunningTask
        {
        
        public void Run(long seed, string message);

        public Task RunAsync(long seed, string message);

        public Task RunYieldingAsync(long seed, string message);

    }
}

