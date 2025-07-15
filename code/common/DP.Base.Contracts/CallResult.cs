using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DP.Base.Contracts;
using DP.Base.Contracts.Logging;
using Newtonsoft.Json;

namespace DP.Base.Contracts
{
    [DataContract]
    public class CallResult : ICallResult
    {
        protected static Random random = new Random();
        protected const int RetryCount = 3;

        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public Exception Exception { get; set; }
        [DataMember]
        public string DisplayMessage { get; set; } // message that might be displayed to the user
       
        [DataMember]
        public List<ICallResult> Children { get; set; } // child call results, used for chaining

        public CallResult(CallResult child = null)
        {
            if (child != null)
            {
                this.Children = new List<ICallResult>();
                this.Children.Add(child);
            }
        }

        private delegate TResult ExecuteWithRetryAsyncDelegateFunc<out TResult>();

        public static Task<CallResult> ExecuteWithRetryAsync<T>(Func<T, Task<CallResult>> action, T arg, ILogger logger, CancellationToken cancellationToken, int initialDelay = 10)
        {
            return ExecuteWithRetryAsyncInternal(() => { return action(arg); }, logger, cancellationToken, initialDelay);
        }

        public static Task<CallResult> ExecuteWithRetryAsync<T1, T2>(Func<T1, T2, Task<CallResult>> action, T1 arg1, T2 arg2, ILogger logger, CancellationToken cancellationToken, int initialDelay = 10)
        {
            return ExecuteWithRetryAsyncInternal(() => { return action(arg1, arg2); }, logger, cancellationToken, initialDelay);
        }

        private static async Task<CallResult> ExecuteWithRetryAsyncInternal(ExecuteWithRetryAsyncDelegateFunc<Task<CallResult>> action, ILogger logger, CancellationToken cancellationToken, int initialDelay = 10)
        {
            var retVal = new CallResult { Success = false };
            int delay = initialDelay;
            int delayMultiplier = 2;

            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    retVal = await action();
                    if (retVal.Success)
                    {
                        break;
                    }
                    else
                    {
                        if (i < RetryCount)
                        {
                            logger.LogMessage($"Action {action.Method.Name}: re-trying action due to error {retVal.DisplayMessage}. Attempt number: {i + 1}", null, null, LogLevel.Warn);
                            delayMultiplier = 1;
                        }
                        else
                        {
                            logger.LogMessage($"Action {action.Method.Name}: giving up, the retry count exceeded the number of attempts allowed due to error {retVal.DisplayMessage}", null, null, LogLevel.Error);
                            break;
                        }
                    }
                }
                catch (TaskCanceledException tex)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        retVal = new CallResult { DisplayMessage = $"{nameof(CallResult.ExecuteWithRetryAsync)} action {action.Method.Name}: cancellation requested", Success = false, Exception = tex };
                        break;
                    }

                    if (i >= RetryCount - 1)
                    {
                        retVal = new CallResult { DisplayMessage = $"{nameof(CallResult.ExecuteWithRetryAsync)} action {action.Method.Name}: giving up, the retry count exceeded the number of attempts allowed due to exception {tex.Message}", Success = false, Exception = tex };
                        logger.LogException(tex, retVal.DisplayMessage, null, null);
                        break;
                    }
                    else
                    {
                        logger.LogException(tex, $"Action {action.Method.Name}: re-trying action due to exception {tex.Message}. Attempt number: {i + 1}", null, null, LogLevel.Warn);
                        delayMultiplier = random.Next(5, 20);
                    }
                }
                catch (TimeoutException tex)
                {
                    if (i >= RetryCount - 1)
                    {
                        retVal = new CallResult { DisplayMessage = $"{nameof(CallResult.ExecuteWithRetryAsync)} action {action.Method.Name}: giving up, the retry count exceeded the number of attempts allowed due to exception {tex.Message}", Success = false, Exception = tex };
                        logger.LogException(tex, retVal.DisplayMessage, null, null);
                        break;
                    }
                    else
                    {
                        logger.LogException(tex, $"Action {action.Method.Name}: re-trying action due to exception {tex.Message}. Attempt number: {i + 1}", null, null, LogLevel.Warn);
                        delayMultiplier = random.Next(5, 20);
                    }
                }
                catch (Exception ex)
                {
                    if (i >= RetryCount - 1)
                    {
                        retVal = new CallResult { DisplayMessage = $"{nameof(CallResult.ExecuteWithRetryAsync)} action {action.Method.Name}: giving up, the retry count exceeded the number of attempts allowed due to exception {ex.Message}", Success = false, Exception = ex };
                        logger.LogException(ex, retVal.DisplayMessage, null, null);
                        break;
                    }
                    else
                    {
                        logger.LogException(ex, $"Action {action.Method.Name}: re-trying action due to exception {ex.Message}. Attempt number: {i + 1}", null, null, LogLevel.Warn);
                        delayMultiplier = 2;
                    }
                }

                await Task.Delay(delay);
                delay *= delayMultiplier;
            }

            return retVal;
        }

        public static CallResult<T> CreateFailedResult<T>(string message)
        {
            return CreateFailedResult<T>(message, (IEnumerable<ICallResult>)null);
        }

        public static CallResult<T> CreateFailedResult<T>(string message, Exception ex)
        {
            return CreateFailedResult<T>(message, (IEnumerable<ICallResult>)null, ex);
        }

        public static CallResult<T> CreateFailedResult<T>(string message, ICallResult childResult, Exception ex = null)
        {
            var childResults = (childResult != null) ? new[] { childResult } : null;
            return CreateFailedResult<T>(message, childResults, ex);
        }

        public static CallResult<T> CreateFailedResult<T>(string message, IEnumerable<ICallResult> childResults, Exception ex = null)
        {
            return new CallResult<T>
            {
                DisplayMessage = message,
                Children = (childResults != null && childResults.Any()) ? childResults.ToList() : null,
                Success = false,
                ReturnValue = default(T),
                Exception = ex,
            };
        }

        public static CallResult CreateFailedResult(string message, IEnumerable<ICallResult> childResults = null, Exception ex = null)
        {
            return new CallResult
            {
                DisplayMessage = message,
                Children = (childResults != null && childResults.Any()) ? childResults.ToList() : null,
                Success = false,
                Exception = ex,
            };
        }

        public static CallResult<T> CreateSuccessResult<T>(T returnValue)
        {
            return new CallResult<T>
            {
                Success = true,
                ReturnValue = returnValue
            };
        }
    }

    [DataContract]
    public class CallResult<T> : CallResult, ICallResult<T>
    {
        public CallResult(CallResult children = null)
            : base(children)
        {
        }

        [DataMember]
        public T ReturnValue { get; set; }

        private delegate TResult ExecuteWithRetryAsyncOfTDelegateFunc<out TResult>();

        public static Task<CallResult<T>> ExecuteWithRetryAsync<T1>(Func<T1, Task<CallResult<T>>> action, T1 arg, ILogger logger, CancellationToken cancellationToken, int initialDelay = 10)
        {
            return ExecuteWithRetryAsyncInternal(() => { return action(arg); }, logger, cancellationToken, initialDelay);
        }

        public static Task<CallResult<T>> ExecuteWithRetryAsync<T1, T2>(Func<T1, T2, Task<CallResult<T>>> action, T1 arg1, T2 arg2, ILogger logger, CancellationToken cancellationToken, int initialDelay = 10)
        {
            return ExecuteWithRetryAsyncInternal(() => { return action(arg1, arg2); }, logger, cancellationToken, initialDelay);
        }

        public static Task<CallResult<T>> ExecuteWithRetryAsync<T1, T2, T3>(Func<T1, T2, T3, Task<CallResult<T>>> action, T1 arg1, T2 arg2, T3 arg3, ILogger logger, CancellationToken cancellationToken, int initialDelay = 10)
        {
            return ExecuteWithRetryAsyncInternal(() => { return action(arg1, arg2, arg3); }, logger, cancellationToken, initialDelay);
        }

        private static async Task<CallResult<T>> ExecuteWithRetryAsyncInternal(ExecuteWithRetryAsyncOfTDelegateFunc<Task<CallResult<T>>> action, ILogger logger, CancellationToken cancellationToken, int initialDelay = 10)
        {
            var retVal = new CallResult<T> { Success = false };
            int delay = initialDelay;
            int delayMultiplier = 2;

            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    retVal = await action();
                    if (retVal.Success)
                    {
                        break;
                    }
                    else
                    {
                        if (i < RetryCount)
                        {
                            logger.LogMessage($"Action {action.Method.Name}: re-trying action due to error {retVal.DisplayMessage}. Attempt number: {i + 1}", null, null, LogLevel.Warn);
                            delayMultiplier = 1;
                        }
                        else
                        {
                            logger.LogMessage($"Action {action.Method.Name}: giving up, the retry count exceeded the number of attempts allowed due to error {retVal.DisplayMessage}", null, null, LogLevel.Error);
                            break;
                        }
                    }
                }
                catch (TaskCanceledException tex)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        retVal = new CallResult<T> { DisplayMessage = $"{nameof(CallResult<T>.ExecuteWithRetryAsync)} action {action.Method.Name}: cancellation requested", Success = false, Exception = tex };
                        break;
                    }

                    if (i >= RetryCount - 1)
                    {
                        retVal = new CallResult<T> { DisplayMessage = $"{nameof(CallResult<T>.ExecuteWithRetryAsync)} action {action.Method.Name}: giving up, the retry count exceeded the number of attempts allowed due to exception {tex.Message}", Success = false, Exception = tex };
                        logger.LogException(tex, retVal.DisplayMessage, null, null);
                        break;
                    }
                    else
                    {
                        logger.LogException(tex, $"Action {action.Method.Name}: re-trying action due to exception {tex.Message}. Attempt number: {i + 1}", null, null, LogLevel.Warn);
                        delayMultiplier = random.Next(5, 20);
                    }
                }
                catch (TimeoutException tex)
                {
                    if (i >= RetryCount - 1)
                    {
                        retVal = new CallResult<T> { DisplayMessage = $"{nameof(CallResult<T>.ExecuteWithRetryAsync)} action {action.Method.Name}: giving up, the retry count exceeded the number of attempts allowed due to exception {tex.Message}", Success = false, Exception = tex };
                        logger.LogException(tex, retVal.DisplayMessage, null, null);
                        break;
                    }
                    else
                    {
                        logger.LogException(tex, $"Action {action.Method.Name}: re-trying action due to exception {tex.Message}. Attempt number: {i + 1}", null, null, LogLevel.Warn);
                        delayMultiplier = random.Next(5, 20);
                    }
                }
                catch (Exception ex)
                {
                    if (i >= RetryCount - 1)
                    {
                        retVal = new CallResult<T> { DisplayMessage = $"{nameof(CallResult<T>.ExecuteWithRetryAsync)} action {action.Method.Name}: giving up, the retry count exceeded the number of attempts allowed due to exception {ex.Message}", Success = false, Exception = ex };
                        logger.LogException(ex, retVal.DisplayMessage, null, null);
                        break;
                    }
                    else
                    {
                        logger.LogException(ex, $"Action {action.Method.Name}: re-trying action due to exception {ex.Message}. Attempt number: {i + 1}", null, null, LogLevel.Warn);
                        delayMultiplier = 2;
                    }
                }

                await Task.Delay(delay);
                delay *= delayMultiplier;
            }

            return retVal;
        }
    }

    public static class CallResultExtensions
    {
        /// <summary>
        /// Returns a new CallResult of the specified type, copying all values from "source" except ReturnValue.
        /// </summary>
        public static CallResult<TDest> ChangeType<TDest>(this CallResult source)
        {
            // NOTE: we could create a version of this that takes CallResult<TSource> and tries to convert from TSource to TDest
            return new CallResult<TDest>
            {
                DisplayMessage = source.DisplayMessage,
                Children = source.Children,
                Exception = source.Exception,
                Success = source.Success,
                ReturnValue = default(TDest),    // obviously can't copy return value because type might not be same
            };
        }
    }
}