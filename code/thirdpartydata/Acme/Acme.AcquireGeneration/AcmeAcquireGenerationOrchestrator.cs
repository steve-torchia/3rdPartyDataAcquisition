using DurableFunctionsCommon;
using Ingress.Lib.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Acme.Contracts;
using DP.Base.Extensions;
using EEDurableFunctionsCommon;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask;
using Microsoft.Azure.Functions.Worker.Http;

namespace Acme.AcquireGeneration
{
    /// <summary>
    /// Encapsulates the Durable Function Orchestration method, which defines the overall workflow for  
    /// retrieving the Acme Generation
    /// </summary>
    public class AcmeAcquireGenerationOrchestrator : OrchestrationBase, IAcmeAcquireGenerationOrchestrator
    {
        internal ILogger<AcmeAcquireGenerationOrchestrator> Log { get; private set; }
        internal GlobalConfigSettings GlobalConfigSettings { get; private set; } 
        internal BlobConfigInfo BlobDestinationConfigInfo { get; private set; }

        internal int MaxAcquireConcurrency { get; private set; }

        public AcmeAcquireGenerationOrchestrator(
            ILogger<AcmeAcquireGenerationOrchestrator> log,
            IOptions<BlobConfigInfo> blobDestinationConfigInfo,
            IOptions<AcquireAcmeGenerationConfigSettings> acquireSettings,
            IOptions<GlobalConfigSettings> globalConfigSettings)
        {
            this.Log = log;
            this.GlobalConfigSettings = globalConfigSettings.Value;
            this.BlobDestinationConfigInfo = new BlobConfigInfo()
            {
                ConnectionString = blobDestinationConfigInfo.Value.ConnectionString,
                ContainerName = blobDestinationConfigInfo.Value.ContainerName,
            };

            // tests, local secrets, set out on dev and test env

            // set to a value between 1 and 10 if specified, else default to 10
            if (acquireSettings == null)
            {
                this.MaxAcquireConcurrency = AcmeHelpers.AcmeMaxConcurrentJobs;
            }
            else
            {
                this.MaxAcquireConcurrency =
                    (acquireSettings.Value?.MaxConcurrentAcquireTasks >= 1 && acquireSettings.Value?.MaxConcurrentAcquireTasks <= 10)
                    ? acquireSettings.Value.MaxConcurrentAcquireTasks
                    : AcmeHelpers.AcmeMaxConcurrentJobs;
            }
        }

        public async Task<IActionResult> Execute(
            string orchestrationId,
            string requestInputJson,
            DurableTaskClient orchestrationClient,
            HttpRequestData req)
        {
            // Launch the Durable Orchestrator
            await orchestrationClient.ScheduleNewOrchestrationInstanceAsync(
                nameof(AcmeAcquireGenerationOrchestratorAsync),
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


        [Function(nameof(AcmeAcquireGenerationOrchestratorAsync))]
        public async Task<OrchestrationResults> AcmeAcquireGenerationOrchestratorAsync(
            [OrchestrationTrigger] TaskOrchestrationContext orchestrationCtx)
        {
            var orchestrationResults = new OrchestrationResults();
            string diagnosticMsg;
            string msg;

            try
            {
                // skt_todo: consolidate logging/statusreporting/etc...
                if (!orchestrationCtx.IsReplaying)
                {
                    Log.LogInformation($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] BEGIN Orchestrator");
                }

                // Get the inputs that came from the trigger/request
                var projectListJson = orchestrationCtx.GetInput<string>();
                var projectList = JsonConvert.DeserializeObject<List<AcquireGenerationInputModel>>(projectListJson);

                // Do some basic validation before we bother to send the job off to the suborch...
                foreach (var project in projectList.ToList())
                {
                    if (project.Product.EqualsIgnoreCase("wind"))
                    {
                        var validateCheck = project.ValidateWind();
                        if (!validateCheck.Success)
                        {
                            msg = $"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {validateCheck.DisplayMessage}";
                            Log.LogInformation(msg);
                            orchestrationResults.AddFailure(project.ProjectNumber, validateCheck.DisplayMessage);
                            projectList.Remove(project);
                        }
                    }
                    else if (project.Product.EqualsIgnoreCase("solar"))
                    {
                        var validateCheck = project.ValidateSolar();
                        if (!validateCheck.Success)
                        {
                            msg = $"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {validateCheck.DisplayMessage}";
                            Log.LogInformation(msg);
                            orchestrationResults.AddFailure(project.ProjectNumber, validateCheck.DisplayMessage);
                            projectList.Remove(project);
                        }
                    }
                    else
                    {
                        msg = $"Bogus Generation Type (Product) specified: {project.Product}";
                        Log.LogInformation(msg);
                        orchestrationResults.AddFailure(project.ProjectNumber, msg);
                        projectList.Remove(project);
                    }
                }

                // Now move on and try to get generation for projects that passed initial validation:

                orchestrationCtx.SetCustomStatus(DurableFunctionHelpers.Orchestration.CustomStatus.Running);

                // Build up a list of context objects, each of which contains individual project info
                var ctxList = AcmeAcquireGenerationHelpers.GetAcmeGenerationContextList(projectList, this.BlobDestinationConfigInfo);


                // Kick off the Sub-Orchestrators:
                //   Each Sub-Orchestrator is tasked with calling the three Activity Functions for one project (launch,wait,save)
                //   Acme only supports 10 concurrent requests at a time so we will throttle requests with a max degree of parallelism if we have > 10
                Log.LogInformation($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {nameof(AcmeAcquireGenerationOrchestrator)} ctor: MaxConcurrency={this.MaxAcquireConcurrency} ");

                var subOrchestrationResults = await DurableFunctionThrottleAsync<AcmeAcquireGenerationContext, AcmeAcquireGenerationContext>(
                    orchestrationCtx,
                    ctxList,
                    DurableFunctionHelpers.DurableFunctionType.SubOrchestrator,
                    nameof(AcmeAcquireGenerationSubOrchestrator.AcmeAcquireGenerationSubOrchestratorAsync),
                    this.MaxAcquireConcurrency);

                foreach (var result in subOrchestrationResults)
                {
                    if (!result.Success)
                    {
                        orchestrationResults.AddFailure(result.ReturnValue.Project.ProjectNumber, result.DisplayMessage);
                    }
                    else
                    {
                        orchestrationResults.AddSuccess(result.ReturnValue.Project.ProjectNumber, result.DisplayMessage);
                    }
                }

                if (orchestrationResults.Failure.Any())
                {
                    orchestrationCtx.SetCustomStatus(DurableFunctionHelpers.Orchestration.CustomStatus.ErrorCondition);
                }
                else
                {
                    orchestrationCtx.SetCustomStatus(DurableFunctionHelpers.Orchestration.CustomStatus.Success);
                }

                // Save the output to AppInsights as well
                Log.LogInformation(JsonConvert.SerializeObject(orchestrationResults));

                return orchestrationResults;
            }
            catch (Exception ex)
            {
                orchestrationResults.Notes.Add($"Exception in {nameof(AcmeAcquireGenerationOrchestratorAsync)}: {ex}");
                Log.LogError(JsonConvert.SerializeObject(orchestrationResults), ex);
                return orchestrationResults;
            }
        }
    }
}