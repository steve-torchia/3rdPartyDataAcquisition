using DP.Base.Contracts;
using Microsoft.DurableTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DurableFunctionsCommon.DurableFunctionHelpers;

namespace DurableFunctionsCommon
{
    public abstract class OrchestrationBase : OrchestrationClientTestingSupport
    {
        /// <summary>
        /// Used to Throttle Sub-Orchestrators or Activity Functions with the specified degree of parallelism
        /// https://github.com/Azure/azure-functions-durable-extension/issues/596
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="activities"></param>
        /// <param name="functionName"></param>
        /// <param name="functionType"></param>
        /// <param name="degreeOfParallelism"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async virtual Task<IEnumerable<CallResult<TResult>>> DurableFunctionThrottleAsync<T, TResult>(
            TaskOrchestrationContext context,
            IEnumerable<T> activities,
            DurableFunctionType functionType,
            string functionName,
            int degreeOfParallelism)
        {
            if (degreeOfParallelism <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(degreeOfParallelism));
            }

            var runningActivities = new List<Task<CallResult<TResult>>>();

            foreach (var activity in activities)
            {
                var pendingOperations = runningActivities.Where(p => !p.IsCompleted);
                if (pendingOperations.Count() >= degreeOfParallelism)
                {
                    // wait for the pending count to get below the degree of parallelism
                    await Task.WhenAny(pendingOperations);
                }

                Task<CallResult<TResult>> result = null;

                if (functionType == DurableFunctionType.SubOrchestrator)
                {
                    result = context.CallSubOrchestratorAsync<CallResult<TResult>>(functionName, activity);
                }
                else if (functionType == DurableFunctionType.Activity)
                {
                    result = context.CallActivityAsync<CallResult<TResult>>(functionName, activity);
                }
                else
                {
                    throw new NotImplementedException($"FunctionType {functionType} not supported");
                }

                runningActivities.Add(result);
            }

            var results = await Task.WhenAll(runningActivities);

            return results;
        }
    }
}