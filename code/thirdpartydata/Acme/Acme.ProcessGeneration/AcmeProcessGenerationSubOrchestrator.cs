using System;
using System.Threading;
using System.Threading.Tasks;
using DP.Base.Contracts;
using Acme.Contracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace Acme.ProcessGeneration
{
    public class AcmeProcessGenerationSubOrchestrator
    {
        internal ILogger<AcmeProcessGenerationSubOrchestrator> Log { get; private set; }

        public AcmeProcessGenerationSubOrchestrator(ILogger<AcmeProcessGenerationSubOrchestrator> log)
        {
            this.Log = log;
        }

        [Function(nameof(AcmeProcessGenerationSubOrchestratorAsync))]
        public async Task<CallResult<string>> AcmeProcessGenerationSubOrchestratorAsync(
            [OrchestrationTrigger] TaskOrchestrationContext subOrchestrationCtx)
        {
            string diagnosticMsg;

            try
            {
                var ctx = subOrchestrationCtx.GetInput<AcmeProcessGenerationContext>();

                var processZipFileResult = await subOrchestrationCtx.CallActivityAsync<CallResult<string>>(
                    nameof(AcmeProcessGenerationActivityFcns.ProcessZipFile),
                    ctx);

                if (!processZipFileResult.Success)
                {
                    diagnosticMsg = $" {nameof(AcmeProcessGenerationActivityFcns.ProcessZipFile)} failed for {ctx.ZipFile}: {processZipFileResult.DisplayMessage}, Exception={processZipFileResult.Exception}";
                    Log.LogError($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {diagnosticMsg}");
                    return CallResult.CreateFailedResult<string>(diagnosticMsg, processZipFileResult.Exception);
                }
                else
                {
                    diagnosticMsg = $"Saved Generation Data for {ctx.ZipFile} to: {processZipFileResult.ReturnValue}";
                    Log.LogInformation($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {diagnosticMsg}");
                    return CallResult.CreateSuccessResult<string>(diagnosticMsg);
                }
            }
            catch (Exception ex)
            {
                diagnosticMsg = $"Exception in {nameof(AcmeProcessGenerationSubOrchestratorAsync)}: {ex}";
                Log.LogError($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {diagnosticMsg}");
                return CallResult.CreateFailedResult<string>(diagnosticMsg, ex);
            }
        }
    }
}