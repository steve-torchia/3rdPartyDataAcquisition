using DP.Base.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ingress.Lib.Base;
using System.Collections.Generic;
using System.Threading.Tasks;
using static DurableFunctionsCommon.DurableFunctionHelpers;
using Acme.Contracts;
using System;
using Microsoft.DurableTask;
using Moq;
using DurableFunctionsCommon;

namespace Acme.AcquireGeneration.Test
{
    public class AcmeAcquireGenerationOrchestratorMock : AcmeAcquireGenerationOrchestrator
    {
        private readonly List<Tuple<AcmeAcquireGenerationContext, bool>> ResultSet = new List<Tuple<AcmeAcquireGenerationContext, bool>>();


        public AcmeAcquireGenerationOrchestratorMock(List<Tuple<AcmeAcquireGenerationContext, bool>> resultSet, Mock<ILogger<AcmeAcquireGenerationOrchestrator>> loggerMock, IOptions<BlobConfigInfo> blobDestinationConfigInfo, IOptions<GlobalConfigSettings> globalConfigInfo)
            : base(loggerMock.Object, blobDestinationConfigInfo, null, globalConfigInfo)
        {
            // used to determine whether or not the underlying, throttled activities are succesful or now
            this.ResultSet = resultSet;
        }

        // Just override the logic of throttling for the sake of testing
        public async override Task<IEnumerable<CallResult<TResult>>> DurableFunctionThrottleAsync<T, TResult>(
            TaskOrchestrationContext context,
            IEnumerable<T> activities,
            DurableFunctionType functionType,
            string functionName,
            int degreeOfParallelism)
        {
            var retVal = new List<CallResult<AcmeAcquireGenerationContext>>();

            foreach (var r in this.ResultSet)
            {
                var result = new CallResult<AcmeAcquireGenerationContext>
                {
                    Success = r.Item2,
                    DisplayMessage = $"Success={r.Item2}",
                    ReturnValue = r.Item1,
                };

                retVal.Add(result);
            }

            return (IEnumerable<CallResult<TResult>>)await Task.FromResult(retVal);
        }
    }
}
