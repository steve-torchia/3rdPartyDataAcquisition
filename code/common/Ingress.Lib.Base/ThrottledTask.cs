using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ingress.Lib.Base
{
    /// <summary>
    /// Methods for throttling the execution of a sequence of methods that return tasks.
    /// </summary>
    /// 
    /// This is useful when we don't want to kick off all the tasks at once.
    /// Imagine if you have a list of 50K tasks that all try to upload something over the network to some target.
    /// Depending on the target, you might not want all 50k tasks to start at once. But you do want to have some number of concurrent requests in flight. 
    /// 
    /// It's is not possible to do this with `Task.WaitAll()`
    /// 
    /// It is not possible to do this with Parallel.ForEach as it does not work with async methods
    /// See: https://stackoverflow.com/a/19292500 and https://stackoverflow.com/a/19284387
    public static class ThrottledTask
    {
        /// <summary>
        /// Throttle the execution of a list of methods that return <see cref="Task"/>
        /// </summary>
        public static async Task WhenAll(IEnumerable<Func<Task>> funcs,
                                         int maxConcurrentTasks)
        {
            using (var semaphore = new SemaphoreSlim(maxConcurrentTasks))
            {
                var tasks = new List<Task>();

                foreach (var func in funcs)
                {
                    await semaphore.WaitAsync();

                    // We wrap the function call in a `Task.Run()` because we don't want it to block
                    // Without this, if the first part of func() was CPU-bound, we would end up blocking
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await func();
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks);
            }
        }
    }
}
