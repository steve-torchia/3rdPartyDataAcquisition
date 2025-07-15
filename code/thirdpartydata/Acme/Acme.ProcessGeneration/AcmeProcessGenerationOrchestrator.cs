using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DP.Base.Contracts;
using DurableFunctionsCommon;
using Ingress.Lib.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Acme.Contracts;
using Newtonsoft.Json;
using EEDurableFunctionsCommon;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask;
using Microsoft.Azure.Functions.Worker.Http;

namespace Acme.ProcessGeneration
{
    /// <summary>
    /// Encapsulates the Durable Function Orchestration method, which defines the overall workflow for  
    /// retrieving the Acme Generation
    /// </summary>
    public class AcmeProcessGenerationOrchestrator : OrchestrationClientTestingSupport, IAcmeProcessGenerationOrchestrator
    {
        internal ILogger<AcmeProcessGenerationOrchestrator> Log { get; private set; }

        internal BlobConfigInfo BlobDestinationConfigInfo { get; private set; }
        internal GlobalConfigSettings GlobalConfigSettings { get; private set; }

        public AcmeProcessGenerationOrchestrator(
            ILogger<AcmeProcessGenerationOrchestrator> log, 
            IOptions<BlobConfigInfo> blobDestinationConfigInfo,
            IOptions<GlobalConfigSettings> globalConfigSettings)
        {
            this.Log = log;
            this.GlobalConfigSettings = globalConfigSettings.Value;
            this.BlobDestinationConfigInfo = new BlobConfigInfo()
            {
                ConnectionString = blobDestinationConfigInfo.Value.ConnectionString,
                ContainerName = blobDestinationConfigInfo.Value.ContainerName,
            };
        }

        public async Task<IActionResult> Execute(
            string orchestrationId,
            string requestInputJson,
            DurableTaskClient orchestrationClient,
            HttpRequestData req)
        {
            // Launch the Durable Orchestrator
            await orchestrationClient.ScheduleNewOrchestrationInstanceAsync(
                nameof(AcmeProcessGenerationOrchestratorAsync),               
                requestInputJson,
                new StartOrchestrationOptions(orchestrationId));

            // Get the status for the passed in instanceId
            var orchestrationStatus = await OrchestrationClientGetStatusAsyncMethod.Invoke(orchestrationClient, orchestrationId);
            var shortStatus = new
            {
                currentStatus = orchestrationStatus.RuntimeStatus.ToString(),
                result = orchestrationStatus.SerializedOutput
            };

            // skt_todo: incorporate some wait or timeout logic here if need be

            Log.LogInformation($"tid={Thread.CurrentThread.ManagedThreadId} STARTED InstanceId={orchestrationId}, status={shortStatus}");

            // return the management URLs that can be used to check status or terminate this workflow
            var mgmtUrls = OrchestrationHelpers.GetManagementUris(orchestrationClient, orchestrationId, GlobalConfigSettings.FunctionKey, req);

            return new OkObjectResult(mgmtUrls);
        }

        [Function(nameof(AcmeProcessGenerationOrchestratorAsync))]
        public async Task<OrchestrationResults> AcmeProcessGenerationOrchestratorAsync(
            [OrchestrationTrigger] TaskOrchestrationContext orchestrationCtx)
        {
            var orchestrationResults = new OrchestrationResults();

            string diagnosticMsg;

            try
            {
                // skt_todo: consolidate logging/statusreporting/etc...
                if (!orchestrationCtx.IsReplaying)
                {
                    Log.LogInformation($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] BEGIN Orchestrator");
                }

                // Get the inputs that came from the trigger/request
                var requestInputJson = orchestrationCtx.GetInput<string>();
                var acmeCtx = AcmeProcessGenerationHelpers.GetAcmeProcessGenerationContext(requestInputJson, this.BlobDestinationConfigInfo); 

                // 1. Get the list of zip files we need to process
                var getFileListResult = await orchestrationCtx.CallActivityAsync<CallResult<List<string>>>(
                    nameof(AcmeProcessGenerationActivityFcns.GetListOfGenerationFilesToProcess),
                    acmeCtx);

                // no point in continuing if we cannot get the list of files to process
                if (!getFileListResult.Success)
                {
                    diagnosticMsg = ($"{nameof(AcmeProcessGenerationActivityFcns.GetListOfGenerationFilesToProcess)} " +
                        $"Failed: {getFileListResult.DisplayMessage}, {getFileListResult.Exception}");
                    orchestrationResults.Notes.Add(diagnosticMsg);
                    Log.LogError(JsonConvert.SerializeObject(orchestrationResults));
                    return orchestrationResults;
                }

                if (getFileListResult.ReturnValue == null)
                { 
                    // No files to process, just get out of here
                    orchestrationResults.Notes.Add($"{nameof(AcmeProcessGenerationActivityFcns.GetListOfGenerationFilesToProcess)} " +
                        $"{getFileListResult.DisplayMessage}, {getFileListResult.Exception}");
                    Log.LogInformation(JsonConvert.SerializeObject(orchestrationResults));
                    return orchestrationResults;
                }

                // get list of files we need to process
                var zipFilesToBeProcessed = getFileListResult.ReturnValue;

                foreach (var zipFile in zipFilesToBeProcessed)
                {
                    var projectNumber = zipFile.Split('_')[1];

                    diagnosticMsg = $"Attempting to process: {zipFile}";

                    Log.LogInformation($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {diagnosticMsg}");
                    orchestrationResults.Notes.Add(diagnosticMsg);

                    var ctx = AcmeProcessGenerationHelpers.GetAcmeProcessGenerationContext(requestInputJson, this.BlobDestinationConfigInfo, zipFile);

                    // Sub Orchestrator will manage the "processing" work needed for each zip file
                    var subOrchResult = await orchestrationCtx.CallSubOrchestratorAsync<CallResult<string>>(
                         nameof(AcmeProcessGenerationSubOrchestrator.AcmeProcessGenerationSubOrchestratorAsync),
                         ctx,
                         null);

                    if (!subOrchResult.Success)
                    {
                        diagnosticMsg = $"ERROR trying to process file={ctx.ZipFile} => {subOrchResult.DisplayMessage}";
                        orchestrationResults.AddFailure(projectNumber, diagnosticMsg);
                        Log.LogError($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {JsonConvert.SerializeObject(orchestrationResults)}");
                        orchestrationCtx.SetCustomStatus(DurableFunctionHelpers.Orchestration.CustomStatus.ErrorCondition);
                    }
                    else
                    {
                        diagnosticMsg = $"SUCCESS trying to process file={ctx.ZipFile}";
                        Log.LogInformation($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {diagnosticMsg}");
                        orchestrationResults.Notes.Add(subOrchResult.ReturnValue);
                        orchestrationResults.Notes.Add(diagnosticMsg);
                        orchestrationCtx.SetCustomStatus(DurableFunctionHelpers.Orchestration.CustomStatus.Success);

                        // Move zip file out of the ToBeProcessed folder so it's out of the "processing queue"
                        var moveFilesResult = await orchestrationCtx.CallActivityAsync<CallResult>(
                            nameof(AcmeProcessGenerationActivityFcns.MoveZipFilesToRawInputFolder),
                            ctx);

                        if (!moveFilesResult.Success)
                        {
                            diagnosticMsg = $"ERROR trying to Move zip file: {ctx.ZipFile} => {moveFilesResult.DisplayMessage}";
                            orchestrationResults.AddFailure(projectNumber, diagnosticMsg);
                            Log.LogError($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {diagnosticMsg}");
                            orchestrationCtx.SetCustomStatus(DurableFunctionHelpers.Orchestration.CustomStatus.ErrorCondition);
                        }
                        else
                        {
                            diagnosticMsg = $"SUCCESS trying to Move zip file: {ctx.ZipFile} => {moveFilesResult.DisplayMessage}";
                            Log.LogInformation($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {diagnosticMsg}");
                            orchestrationResults.AddSuccess(projectNumber, diagnosticMsg);
                            orchestrationCtx.SetCustomStatus(DurableFunctionHelpers.Orchestration.CustomStatus.Success);
                        }
                    }
                }

                return orchestrationResults;
            }
            catch (Exception ex)
            {
                orchestrationResults.Notes.Add($"Exception in {nameof(AcmeProcessGenerationOrchestratorAsync)}: {ex}");
                return orchestrationResults;
            }
        }
    }
}