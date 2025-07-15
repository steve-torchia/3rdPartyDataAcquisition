using EEDurableFunctionsCommon;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Acme.Contracts
{
    public interface IAcmeProcessGenerationOrchestrator
    {
        Task<OrchestrationResults> AcmeProcessGenerationOrchestratorAsync([OrchestrationTrigger] TaskOrchestrationContext orchestrationCtx);

        Task<IActionResult> Execute(string orchestrationId, string requestInputJson, DurableTaskClient orchestrationClient, HttpRequestData req);
    }
}