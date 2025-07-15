using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DP.Base.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Acme.Contracts;
using Newtonsoft.Json;
using DurableFunctionsCommon;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Azure.Functions.Worker.Http;

namespace Acme.ProcessGeneration
{
    public class AcmeProcessGenerationFcnTrigger : OrchestrationClientTestingSupport
    {
        internal IAcmeProcessGenerationOrchestrator Orchestrator { get; }

        internal ILogger<AcmeProcessGenerationFcnTrigger> Log { get; private set; }

        public AcmeProcessGenerationFcnTrigger(
            IAcmeProcessGenerationOrchestrator orchestrator, 
            ILogger<AcmeProcessGenerationFcnTrigger> log)
        {
            this.Orchestrator = orchestrator;
            Log = log;
        }

        [Function(nameof(ProcessAcmeGenerationHttpStart))]
        public async Task<IActionResult> ProcessAcmeGenerationHttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req,
            [DurableClient] DurableTaskClient orchestrationClient)
        {
            // skt_todo: Auth?

            Log.LogInformation($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] C# HTTP trigger function processed a request");

            // get the request body 
            string msg;
            var requestJson = await req.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(requestJson))
            {
                msg = $"Empty Request Body specified ({requestJson})";
                Log.LogInformation(msg);

                // This is not an error for now as we don't really need anything in the body.
                // Regardless, we will plumb the body through the underlying functions just in case we need it later
                //return new BadRequestObjectResult(msg);
            }

            // Protect against > 1 of the same exact job request running in parallel (singleton orchestrator pattern)
            var orchestrationId = AcmeProcessGenerationHelpers.GetOrchestrationId();
            Log.LogInformation($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] OrchestrationId={orchestrationId}");

            var orchestrationStatus = await OrchestrationClientGetStatusAsyncMethod.Invoke(orchestrationClient, orchestrationId);

            if (orchestrationStatus == null
            || orchestrationStatus.RuntimeStatus == OrchestrationRuntimeStatus.Completed
            || orchestrationStatus.RuntimeStatus == OrchestrationRuntimeStatus.Failed
            || orchestrationStatus.RuntimeStatus == OrchestrationRuntimeStatus.Terminated)
            {
                // An instance with the specified ID doesn't exist or an existing one stopped running, so...
                // Kick off the Workflow/Orchestration
                return await Orchestrator.Execute(orchestrationId, requestJson, orchestrationClient, req);
            }
            else
            {
                var errMsg = $"An instance with ID '{orchestrationId}' already exists.";
                Log.LogWarning(errMsg);
                return new BadRequestObjectResult(errMsg);
            }
        }
    }
}