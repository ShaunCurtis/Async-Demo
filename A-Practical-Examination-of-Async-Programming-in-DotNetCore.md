# A Practical Examination of Async Programming in DotNetCore

My first article on this subject provided an overview of async programming in DotNetCore and explained some of the key concepts.  You'll find the article [here](https://www.codeproject.com/Articles/5276310/Understanding-and-Using-Async-Programming-in-DotNe).  This article takes a practical approach to demonstrating some of of those key concepts, and introducing more complex coding patterns.  The article is based around a DotNetCore console application.

You'll need a DotNetCore compatible development environment, normally either either Visual Studio or Visual Code, and a copy of the Repo associated with this project to run the code.

> **DISCLAIMER** - The code is **Experimental**, not **Production**.  Designed to be concise with minimal error trapping and handling to keep it easy to read and understand. Classes are kept simple for the same reason.

## Code Repository

The code in available in a GitHub Repo [here](https://github.com/ShaunCurtis/Async-Demo).  The code for this project is in *Async-Demo*.  Ignore any other projects - they are for a further Async Programming article.

## Library Classes

Before we start your need to be aware of two helper classses

1. `LongRunningTasks` - emulates work.
   1. `RunLongProcessorTaskAsync` and `RunLongProcessorTask` use prime number calculations to emulate a processor heavy task.
   2. `RunYieldingLongProcessorTaskAsync` is a version that yields every 100 calculations.
   3. `RunLongIOTaskAsync` uses `Task.Delay` to emulate a slow I/O operations.
4. `UILogger` provides an abstraction layer for logging information to the UI.  You pass a delegate `Action` to the methods.  `UILogger` builds the message, and then calls the `Action` to actually write it to wherever the `Action` is configured to write to. In our case `LogToConsole` in `Program`,  which runs `Console.WriteLine`.  It could just as easily write to a text file.

## Getting Started

Our first challenge is the switch from sync to async.

Make sure you're running the correct framework and latest language version. (C# 7.1 onwards supports a Task based `Main`).

```xml
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net5</TargetFramework>
    <LangVersion>latest</LangVersion>
    <RootNamespace>Async_Demo</RootNamespace>
  </PropertyGroup>
```
Pre #7.1, `Main` could only run synchronously, and you needed a "NONO", using `Wait`, to prevent `Main` dropping out the bottom and closing the program. Post #7.1, declare `Main` to return a `Task`.

The `async` `Main` pattern is shown below.  Declaring `async` depends on whether on not there's an `await` in the code

```c#
// With await
static async Task Main(string[] args)
{
    // code
    // await somewhere in here
}

// No awaits
static Task Main(string[] args)
{
    // code
    // no awaits
    return Task.CompletedTask;
}
```
Note:

1. If you use the `async` keyword but don't have an `await`, the compiler warns, but then compiles anyway, treating the method as synchronous code.
2. You can't declare a method as `async` and return a `Task`.  You simply return the correct value and the compiler will do all the donkey work.

So let's run some code. Our first run:

```c#
static Task Main(string[] args)
{
    var watch = new Stopwatch();
    watch.Start();
    UILogger.LogThreadType(LogToConsole, "Main");
    var millisecs = LongRunningTasks.RunLongProcessorTask(5);
    watch.Stop();
    UILogger.LogToUI(LogToConsole, $"Main ==> Completed in { watch.ElapsedMilliseconds} milliseconds", "Main");
    return Task.CompletedTask;
}
```

The Task ran *synchronously* as expected. A bunch of synchronous code inside a `Task`. No yielding.

```text
[11:35:32][Main Thread][Main] >  running on Application Thread
[11:35:32][Main Thread][LongRunningTasks] > ProcessorTask started
[11:35:36][Main Thread][LongRunningTasks] > ProcessorTask completed in 3399 millisecs
[11:35:36][Main Thread][Main] > Main ==> Completed in 3523 milliseconds
Press any key to close this window . . .
````

Our second run:

```c#
static async Task Main(string[] args)
{
    var watch = new Stopwatch();
    watch.Start();
    UILogger.LogThreadType(LogToConsole, "Main");
    var millisecs = await LongRunningTasks.RunLongProcessorTaskAsync(5, LogToConsole);
    UILogger.LogToUI(LogToConsole, $"Yielded to Main", "Main");
    watch.Stop();
    UILogger.LogToUI(LogToConsole, $"Main ==> Completed in { watch.ElapsedMilliseconds} milliseconds", "Main");
}
```

The Task ran *synchronously* - no yielding.  Logical because there was no reason to yield. `RunLongProcessorTaskAsync` is a synchronous bunch of code inside a Task - calculating prime numbers - so it ran to completion.  The `await` is redundant, it may be a `Task` but it doesn't yield, so never gives up the thread until complete.

```text
[11:42:43][Main Thread][Main] >  running on Application Thread
[11:42:43][Main Thread][LongRunningTasks] > ProcessorTask started
[11:42:46][Main Thread][LongRunningTasks] > ProcessorTask completed in 3434 millisecs
[11:42:46][Main Thread][Main] > Yielded
[11:42:46][Main Thread][Main] > Main ==> Completed in 3593 milliseconds
````

Our third run:

```c#
static async Task Main(string[] args)
{
    var watch = new Stopwatch();
    watch.Start();
    UILogger.LogThreadType(LogToConsole, "Main");
    var millisecs = LongRunningTasks.RunYieldingLongProcessorTaskAsync(5, LogToConsole);
    UILogger.LogToUI(LogToConsole, $"Yielded to Main", "Main");
    watch.Stop();
    UILogger.LogToUI(LogToConsole, $"Main ==> Completed in { watch.ElapsedMilliseconds} milliseconds", "Main");
}
```

Before we look at the result, let's look at the difference between `RunLongProcessorTaskAsync` and `RunYieldingLongProcessorTaskAsync`.  We've added a `Task.Yield()` to yield control every 100 primes.

```c#
if (isPrime)
{
    counter++;
    // only present in Yielding version
    if (counter > 100)
    {
        await Task.Yield();
        counter = 0;
    }
}
```

The long running task didn't complete. `RunYieldingLongProcessorTaskAsync` yielded back to `Main` after the first 100 primes had been calculated - a little short of 173 millisecs - and `Main` ran to completion during the yield.

```text
[12:13:56][Main Thread][Main] >  running on Application Thread
[12:13:56][Main Thread][LongRunningTasks] > ProcessorTask started
[12:13:57][Main Thread][Main] > Yielded to Main
[12:13:57][Main Thread][Main] > Main ==> Completed in 173 milliseconds
````

If we update `Main` to `await` the long processor task

```c#
    var millisecs = await LongRunningTasks.RunYieldingLongProcessorTaskAsync(5, LogToConsole);
```

It runs to completion.  Although it yields, we `await` on the `RunYieldingLongProcessorTaskAsync` `Task` to complete, before moving on in `Main`.  There's another important point to note here.  Look at which thread the long running task ran on, and compare it to previous runs.  It jumped to a new thread `[LongRunningTasks Thread]` after starting on [Main Thread].

```text
[12:45:10][Main Thread:1][Main] >  running on Application Thread
[12:45:11][Main Thread:1][LongRunningTasks] > ProcessorTask started
[12:45:14][LongRunningTasks Thread:7][LongRunningTasks] > ProcessorTask completed in 3892 millisecs
[12:45:14][LongRunningTasks Thread:7][Main] > Yielded to Main
[12:45:14][LongRunningTasks Thread:7][Main] > Main ==> Completed in 4037 milliseconds
```

Add a quick `Console.Write` in `RunYieldingLongProcessorTaskAsync` to see which thread each yielded iteration runs on - writing the `ManagedThreadId`.

```c#
counter++;
if (counter > 100)
{
    Console.WriteLine($"Thread ID:{Thread.CurrentThread.ManagedThreadId}");
    await Task.Yield();
    counter = 0;
}
```

The result is shown below.  Notice the regular thread jumping.  Yield creates a new continuation `Task`, and schedules it to run asynchronously.  On the first `Task.Yield` the application thread scheduler passes the new `Task` to the application pool, and for then on the application pool Scheduler makes decisions on where to run Tasks.
> `Task.Yield()`, to quote Microsoft "Creates an awaitable task that asynchronously yields back to the current context when awaited." I translate that to mean it's syntactic sugar for yielding control up the tree and creating a continuation `Task` that gets posted back to the Scheduler to run when it schedules it. To quote further "A context that, when awaited, will asynchronously transition back into the current context at the time of the await."  In other words, it doesn't `await` unless you tell it to.  Hit the first yield in the continuation and processing trucks on through to the code below `Task.Yield()`.  I've tested it.

```text
[12:38:16][Main Thread:1][Main] >  running on Application Thread
[12:38:16][Main Thread:1][LongRunningTasks] > ProcessorTask started
Thread ID:1
Thread ID:4
Thread ID:4
Thread ID:6
Thread ID:6
Thread ID:7
```

Finally, change over to the `RunLongIOTaskAsync` long running task.

```c#
    var millisecs = await LongRunningTasks.RunLongIOTaskAsync(5, LogToConsole);
```

If you don't `await`, the same as before:

```text
[14:26:46][Main Thread:1][Main] >  running on Application Thread
[14:26:47][Main Thread:1][LongRunningTasks] > IOTask started
[14:26:47][Main Thread:1][Main] > Yielded to Main
[14:26:47][Main Thread:1][Main] > Main ==> Completed in 322 milliseconds
```

And if you `await` it runs to completion, again with the thread switch.

```text
[14:27:16][Main Thread:1][Main] >  running on Application Thread
[14:27:16][Main Thread:1][LongRunningTasks] > IOTask started
[14:27:21][LongRunningTasks Thread:4][LongRunningTasks] > IOTask completed in 5092 millisecs
[14:27:21][LongRunningTasks Thread:4][Main] > Yielded to Main
[14:27:21][LongRunningTasks Thread:4][Main] > Main ==> Completed in 5274 milliseconds

```

## More Complexity

Ok, now to move closer to reality and code doing something.

### JobRunner

`JobRunner` is a simple class to run and control asynchronous jobs.  For our purposes, it runs one of the long running tasks to simulate work, but you can use the basic pattern for real world situations.

It's self-evident what most of the code does, but I'll introduce `TaskCompletionSource`.

> To quote MS "Represents the producer side of a Task\<TResult\> unbound to a delegate, providing access to the consumer side through the Task property."  You get a `Task` exposed by `TaskCompletionSource.Task` that you control through the `TaskCompletionSource` instance - in other words, a manually controlled `Task` uncoupled from the method.

The `Task` that represents the state of the `JobRunner` is exposed as the `JobTask` property.  If the underlying `TaskCompletionSource` isn't set it returns a simple `Task.CompletedTask` object, otherwise it returns the `Task` of `JobTaskController`.  The `Run` method uses the async event pattern - we need a block of code that runs asynchronously, yielding control with `await`.  `Run` controls the `Task` state, but the `Task` itself is independant of `Run`.  `IsRunning` ensures you can't start the job once it's running.

```c#
class JobRunner
{
    public enum JobType { IO, Processor, YieldingProcessor } 

    public JobRunner(string name, int secs, JobType type = JobType.IO)
    {
        this.Name = name;
        this.Seconds = secs;
        this.Type = type;
    }

    public string Name { get; private set; }
    public int Seconds { get; private set; }
    public JobType Type { get; set; }
    private bool IsRunning;

    public Task JobTask => this.JobTaskController == null ? Task.CompletedTask : this.JobTaskController.Task;
    private TaskCompletionSource JobTaskController { get; set; } = new TaskCompletionSource();

    public async void Run()
    {
        if (!this.IsRunning) {
            this.IsRunning = true;
            this.JobTaskController = new TaskCompletionSource();
            switch (this.Type)
            {
                case JobType.Processor:
                    await LongRunningTasks.RunLongProcessorTaskAsync(Seconds, Program.LogToConsole, Name);
                    break;
                    
                case JobType.YieldingProcessor:
                    await LongRunningTasks.RunYieldingLongProcessorTaskAsync(Seconds, Program.LogToConsole, Name);
                    break;

                default:
                    await LongRunningTasks.RunLongIOTaskAsync(Seconds, Program.LogToConsole, Name);
                    break;
            }

            this.JobTaskController.TrySetResult();
            this.IsRunning = false;
        }
    }
}
```

### JobScheduler

`JobScheduler` is the method used to actually schedule the jobs.  It's separated from `Main` to demonstrate some key behaviours of async programming.

1. `Stopwatch` provides timing.
2. Creates four different *IO* jobs.
3. Starts the four jobs.
4. Uses `Task.WhenAll` to wait on certain tasks before continuing.  Note the `Task`s are the `JobTask`s exposed by the `JobRunnner` instances.

> `WhenAll` is one of several static `Task` methods.  `WhenAll` creates a single `Task` which `awaits` all the Tasks in the submitted array.  It's status will change to *Complete* when all the Tasks complete.  `WhenAny` is similar, but will be set to *Complete* when any are complete.  They could be named *AwaitAll* and *AwaitAny*.  `WaitAll` and `WaitAny` are blocking versions and similar to `Wait`.  Not sure about the reasons for the slightly confusing naming conversion - I'm sure there was one.

```c#
static async Task JobScheduler()
{
    var watch = new Stopwatch();
    watch.Start();
    var name = "Job Scheduler";
    var quickjob = new JobRunner("Quick Job", 3);
    var veryslowjob = new JobRunner("Very Slow Job", 7);
    var slowjob = new JobRunner("Slow Job", 5);
    var veryquickjob = new JobRunner("Very Quick Job", 2);
    quickjob.Run();
    veryslowjob.Run();
    slowjob.Run();
    veryquickjob.Run();
    UILogger.LogToUI(LogToConsole, $"All Jobs Scheduled", name);
    await Task.WhenAll(new Task[] { quickjob.JobTask, veryquickjob.JobTask }); ;
    UILogger.LogToUI(LogToConsole, $"Quick Jobs completed in {watch.ElapsedMilliseconds} milliseconds", name);
    await Task.WhenAll(new Task[] { slowjob.JobTask, quickjob.JobTask, veryquickjob.JobTask, veryslowjob.JobTask }); ;
    UILogger.LogToUI(LogToConsole, $"All Jobs completed in {watch.ElapsedMilliseconds} milliseconds", name);
    watch.Stop();
}
```

We now need to make some changes to `Main`:

```c#
static async Task Main(string[] args)
{
    var watch = new Stopwatch();
    watch.Start();
    UILogger.LogThreadType(LogToConsole, "Main");
    var task = JobScheduler();
    UILogger.LogToUI(LogToConsole, $"Job Scheduler yielded to Main", "Main");
    await task;
    UILogger.LogToUI(LogToConsole, $"final yield to Main", "Main");
    watch.Stop();
    UILogger.LogToUI(LogToConsole, $"Main ==> Completed in { watch.ElapsedMilliseconds} milliseconds", "Main");

    //return Task.CompletedTask;
}

```

When you run this you get the output below.  The interesting bits to note are:

1. Each of the jobs start, and then yield at their first await, passing control back to the caller - in this case `JobSchedular`.
2. `JobScheduler` runs to it's first `await` and yields back to `Main`.
3. When the first two jobs finish their `JobTask` is set to complete and `JobScheduler` continues to the next `await`.
4. `JobScheduler` completes in a little over the time needed to run the longest Job.

```text
[16:58:52][Main Thread:1][Main] >  running on Application Thread
[16:58:52][Main Thread:1][LongRunningTasks] > Quick Job started
[16:58:52][Main Thread:1][LongRunningTasks] > Very Slow Job started
[16:58:52][Main Thread:1][LongRunningTasks] > Slow Job started
[16:58:52][Main Thread:1][LongRunningTasks] > Very Quick Job started
[16:58:52][Main Thread:1][Job Scheduler] > All Jobs Scheduled
[16:58:52][Main Thread:1][Main] > Job Scheduler yielded to Main
[16:58:54][LongRunningTasks Thread:4][LongRunningTasks] > Very Quick Job completed in 2022 millisecs
[16:58:55][LongRunningTasks Thread:4][LongRunningTasks] > Quick Job completed in 3073 millisecs
[16:58:55][LongRunningTasks Thread:4][Job Scheduler] > Quick Jobs completed in 3090 milliseconds
[16:58:57][LongRunningTasks Thread:4][LongRunningTasks] > Slow Job completed in 5003 millisecs
[16:58:59][LongRunningTasks Thread:6][LongRunningTasks] > Very Slow Job completed in 7014 millisecs
[16:58:59][LongRunningTasks Thread:6][Job Scheduler] > All Jobs completed in 7111 milliseconds
[16:58:59][LongRunningTasks Thread:6][Main] > final yield to Main
[16:58:59][LongRunningTasks Thread:6][Main] > Main ==> Completed in 7262 milliseconds
```

Now change the job type over to `Processor` as below:

```c#
var quickjob = new JobRunner("Quick Job", 3, JobRunner.JobType.Processor);
var veryslowjob = new JobRunner("Very Slow Job", 7, JobRunner.JobType.Processor);
var slowjob = new JobRunner("Slow Job", 5, JobRunner.JobType.Processor);
var veryquickjob = new JobRunner("Very Quick Job", 2, JobRunner.JobType.Processor);
```
When you run this, you'll see everything is run sequentially on the `Main Thread`.  At first you think why?  We have more than one thread available and the Scheduler has demonstrated it's ability to switch tasks between threads. Why isn't it switching?

The answer is very simple.  Once we initialise the JobRunnner object we run them in to the Scheduler one at a time.  As the code we run is sequential - calculating primes without breaks - we don't execute the next line of code (feeding in the second job) until the first job completes.

```text
[17:59:48][Main Thread:1][Main] >  running on Application Thread
[17:59:48][Main Thread:1][LongRunningTasks] > Quick Job started
[17:59:53][Main Thread:1][LongRunningTasks] > Quick Job completed in 4355 millisecs
[17:59:53][Main Thread:1][LongRunningTasks] > Very Slow Job started
[17:59:59][Main Thread:1][LongRunningTasks] > Very Slow Job completed in 6057 millisecs
[17:59:59][Main Thread:1][LongRunningTasks] > Slow Job started
[18:00:03][Main Thread:1][LongRunningTasks] > Slow Job completed in 4209 millisecs
[18:00:03][Main Thread:1][LongRunningTasks] > Very Quick Job started
[18:00:05][Main Thread:1][LongRunningTasks] > Very Quick Job completed in 1737 millisecs
[18:00:05][Main Thread:1][Job Scheduler] > All Jobs Scheduled
[18:00:05][Main Thread:1][Job Scheduler] > Quick Jobs completed in 16441 milliseconds
[18:00:05][Main Thread:1][Job Scheduler] > All Jobs completed in 16441 milliseconds
[18:00:05][Main Thread:1][Main] > Job Scheduler yielded to Main
[18:00:05][Main Thread:1][Main] > final yield to Main
[18:00:05][Main Thread:1][Main] > Main ==> Completed in 16591 milliseconds
```

Now, change the jobs over to run `YieldingProcessor`. 

```c#
var quickjob = new JobRunner("Quick Job", 3, JobRunner.JobType.YieldingProcessor);
var veryslowjob = new JobRunner("Very Slow Job", 7, JobRunner.JobType.YieldingProcessor);
var slowjob = new JobRunner("Slow Job", 5, JobRunner.JobType.YieldingProcessor);
var veryquickjob = new JobRunner("Very Quick Job", 2, JobRunner.JobType.YieldingProcessor);
```

The result is very different.  The time taken will depend on the number of processor cores and threads on your computer.  You can see all the jobs start quickly and completion in 11 seconds, with the slowest job taking 9 seconds.  The key difference here is that the processor long running job yields regularly.  This gives the Scheduler a chance to divy out out the work to other threads.

Yielding Processor code
```text
[17:50:12][Main Thread:1][Main] >  running on Application Thread
[17:50:12][Main Thread:1][LongRunningTasks] > Quick Job started
[17:50:12][Main Thread:1][LongRunningTasks] > Very Slow Job started
[17:50:12][Main Thread:1][LongRunningTasks] > Slow Job started
[17:50:12][Main Thread:1][LongRunningTasks] > Very Quick Job started
[17:50:12][Main Thread:1][Job Scheduler] > All Jobs Scheduled
[17:50:12][Main Thread:1][Main] > Job Scheduler yielded to Main
[17:50:16][LongRunningTasks Thread:7][LongRunningTasks] > Very Quick Job completed in 4131 millisecs
[17:50:18][LongRunningTasks Thread:7][LongRunningTasks] > Quick Job completed in 6063 millisecs
[17:50:18][LongRunningTasks Thread:7][Job Scheduler] > Quick Jobs completed in 6158 milliseconds
[17:50:21][LongRunningTasks Thread:6][LongRunningTasks] > Slow Job completed in 9240 millisecs
[17:50:23][LongRunningTasks Thread:9][LongRunningTasks] > Very Slow Job completed in 11313 millisecs
[17:50:23][LongRunningTasks Thread:9][Job Scheduler] > All Jobs completed in 11411 milliseconds
[17:50:23][LongRunningTasks Thread:9][Main] > final yield to Main
[17:50:23][LongRunningTasks Thread:9][Main] > Main ==> Completed in 11534 milliseconds
```

## Conclusions and Wrap Up.

Hopefully helpful/informative?  Some of the key points that I've learned in my voyage down the async road, and are demonstrated here are:

1. **Async and Await All The Way**. Don't mix synchronous and asynchronous methods.  Start at the bottom - the data or process interface - and code async all the way up though the data and business/logic layers to the UI.   
2. You can't run asynchronously if you don't yield. You've got to give the task schedulers a chance!  Wrapping a few synchronous routines in `Task` is talking-the-talk not walking-the-walk.
3. Fire and forget `void` return methods need to yield to pass control back to the caller.  They are no different to Task returning methods in their behaviour. They just don't return a Task for you to await or monitor progress.
4. If you're writing processor intensive activities - modelling, big numbercrunching,..  make sure to make them async with plenty of yielding at appropriate places.
5. ONLY use `Task.Run` in the UI, right up at the top of the call stack.  NEVER EVER use it in libraries.  And don't use it at all unless you have a solid reason.
6. Use logging and breakpoints on `awaits` to see when you hit them.  How quickly your code falls back to the outside `await` is a  very good indicator of responsiveness.  Take out your outside `await` and see how quickly you drop out the bottom!
7. You may have noticed no `ContinueWith`.  I don't often use it.  Normally a simple `await` followed by continuation code achieves the same result.  I've read commentary that it's heavier on processing, because it creates a new task whereas await/continuation reuses the same `Task`.  I haven't delved deeply enough into the code yet to check.
8. Always use *Async* and *Await*, don't get fancy.
9. If your library provides both async and sync calls, code them separately.  "Code it once" best practice doesn't apply here.  NEVER call one from the other if you don't want to shoot yourself in the foot at some point!
