using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Acme.Contracts;
using DP.Base.Contracts;
using System.Threading;
using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace Acme.AcquireGeneration
{
    public class AcmeAcquireGenerationSubOrchestrator
    {
        internal ILogger<AcmeAcquireGenerationSubOrchestrator> Log { get; private set; }

        public AcmeAcquireGenerationSubOrchestrator(ILogger<AcmeAcquireGenerationSubOrchestrator> log)
        {
            this.Log = log;
        }

        [Function(nameof(AcmeAcquireGenerationSubOrchestratorAsync))]
        public async Task<CallResult<AcmeAcquireGenerationContext>> AcmeAcquireGenerationSubOrchestratorAsync(
            [OrchestrationTrigger] TaskOrchestrationContext subOrchestrationCtx)
        {
            string diagnosticMsg;
            AcmeAcquireGenerationContext acmeCtx = null;

            try
            {
                acmeCtx = subOrchestrationCtx.GetInput<AcmeAcquireGenerationContext>();

                // 1. Kick off Wind Generation Job at Acme
                var launchWindJobResult = await subOrchestrationCtx.CallActivityAsync<CallResult<string>>(
                    nameof(AcmeAcquireGenerationActivityFcns.LaunchGenerationForWeatherYearJobAsync),
                    acmeCtx);

                if (!launchWindJobResult.Success)
                {
                    diagnosticMsg = $"{nameof(AcmeAcquireGenerationActivityFcns.LaunchGenerationForWeatherYearJobAsync)} " +
                        $"Failed: {launchWindJobResult.DisplayMessage}, {launchWindJobResult.Exception}";

                    Log.LogError($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {diagnosticMsg}");

                    return new CallResult<AcmeAcquireGenerationContext>()
                    {
                        Success = false,
                        DisplayMessage = diagnosticMsg,
                        ReturnValue = acmeCtx,
                        Exception = launchWindJobResult.Exception,
                    };
                }

                // 2. Successful request, now wait for the job to finish at Acme
                if (!subOrchestrationCtx.IsReplaying)
                {
                    diagnosticMsg = $"Wait for Acme HistoryId={launchWindJobResult.ReturnValue}";
                    Log.LogInformation($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {diagnosticMsg}");
                }

                var waitForJobResult = await subOrchestrationCtx.CallActivityAsync<CallResult<string>>(
                    nameof(AcmeAcquireGenerationActivityFcns.WaitForGenerationJobAsync),
                    launchWindJobResult.ReturnValue);

                if (!waitForJobResult.Success)
                {
                    diagnosticMsg = $"{nameof(AcmeAcquireGenerationActivityFcns.WaitForGenerationJobAsync)} " +
                        $"Failed (HistoryId={launchWindJobResult.ReturnValue}): {waitForJobResult.DisplayMessage}, {waitForJobResult.Exception}";
                    Log.LogError($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {diagnosticMsg}");

                    return new CallResult<AcmeAcquireGenerationContext>()
                    {
                        Success = false,
                        DisplayMessage = diagnosticMsg,
                        ReturnValue = acmeCtx,
                        Exception = waitForJobResult.Exception,
                    };
                }

                // 3. Successfully finished waiting for job to complete. Get the URI of the finished work so we can download it and store it in the data lake
                if (!subOrchestrationCtx.IsReplaying)
                {
                    acmeCtx.JobInfo.ResultsUrl = waitForJobResult.ReturnValue;
                    diagnosticMsg = $"Download and Save (HistoryId={launchWindJobResult.ReturnValue}): {acmeCtx.JobInfo.ResultsUrl}";
                    Log.LogInformation($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {diagnosticMsg}");
                }

                // 4. Job is complete, go get the resulting CSV file from the URI that came back from Acme
                var getGenerationDataResult = await subOrchestrationCtx.CallActivityAsync<CallResult<string>>(
                    nameof(AcmeAcquireGenerationActivityFcns.GetGenerationDataAndSaveToBlobAsync),
                    acmeCtx);

                if (!getGenerationDataResult.Success)
                {
                    diagnosticMsg = $"GetGenerationDataAndSaveToBlob Failed: {getGenerationDataResult.DisplayMessage}, {getGenerationDataResult.Exception}";
                    Log.LogError($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {diagnosticMsg}");
                    
                    return new CallResult<AcmeAcquireGenerationContext>()
                    {
                        Success = false,
                        DisplayMessage = diagnosticMsg,
                        ReturnValue = acmeCtx,
                        Exception = getGenerationDataResult.Exception,
                    };
                }

                diagnosticMsg = $"Success: Saved Generation Data (HistoryId={launchWindJobResult.ReturnValue}) to: {getGenerationDataResult.ReturnValue}";
                if (!subOrchestrationCtx.IsReplaying)
                {                    
                    Log.LogInformation($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {diagnosticMsg}");
                }
                
                return new CallResult<AcmeAcquireGenerationContext>()
                {
                    Success = true,
                    ReturnValue = acmeCtx,
                    DisplayMessage = diagnosticMsg,
                };

            }
            catch (Exception ex)
            {
                diagnosticMsg = $"Exception in {nameof(AcmeAcquireGenerationSubOrchestratorAsync)}: {ex}";
                Log.LogError($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {diagnosticMsg}");
               
                return new CallResult<AcmeAcquireGenerationContext>()
                {
                    Success = false,
                    DisplayMessage = diagnosticMsg,
                    ReturnValue = acmeCtx,
                    Exception = ex,
                };
            }
        }
    }
}