using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DP.Base
{
    public static class Retry
    {
        /// <summary>
        /// Call like this:    Execute(MyFunction), or Execute(() => MyFunction())
        /// </summary>
        public static void Execute(Action action, int maxAttempts, int retryInterval)
        {
            Execute<object>(() =>
            {
                action();
                return null;
            }, retryInterval, maxAttempts);
        }

        /// <summary>
        /// Call like this:    Execute(() => MyFunction(arg1, arg2))
        /// </summary>
        public static TResult Execute<TResult>(Func<TResult> function, int maxAttempts, int retryInterval)
        {
            var exceptionList = new List<Exception>();

            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    if (i > 0)
                    {
                        Thread.Sleep(retryInterval);
                    }
                    return function();
                }
                catch (Exception ex)
                {
                    exceptionList.Add(ex);
                }
            }
            throw new AggregateException(exceptionList);
        }

        /// <summary>
        /// Call like this:    ExecuteAsync(() => TheFunctionAsync());
        /// </summary>
        public static Task ExecuteAsync<TResult>(Func<Task> asyncAction, int maxAttempts, int retryInterval)
        {
            return ExecuteAsync<object>(() =>
            {
                asyncAction();
                return null;
            }, retryInterval, maxAttempts);
        }

        /// <summary>
        /// Call like this:    ExecuteAsync(async () => await TheFunctionAsync(arg1, arg2));
        /// </summary>
        public static Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> asyncFunction, int maxAttempts, int retryInterval)
        {
            var exceptionList = new List<Exception>();

            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    if (i > 0)
                    {
                        Thread.Sleep(retryInterval);
                    }
                    return asyncFunction();
                }
                catch (Exception ex)
                {
                    exceptionList.Add(ex);
                }
            }
            throw new AggregateException(exceptionList);
        }
    }
}
