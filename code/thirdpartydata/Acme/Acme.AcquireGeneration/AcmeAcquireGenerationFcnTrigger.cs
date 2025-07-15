using DP.Base.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Acme.Contracts;
using DurableFunctionsCommon;
using Ingress.Lib.Base.Contracts;
using Ingress.Lib.Base;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Azure.Functions.Worker.Http;

namespace Acme.AcquireGeneration
{
    public class AcmeAcquireGenerationFcnTrigger : OrchestrationClientTestingSupport
    {
        public const string AcmeHistoryId = "acmeHistoryId";
        public const string GetSettingsParam = "getSettings";

        internal IAcmeAcquireGenerationOrchestrator Orchestrator { get; }
        internal AcquireAcmeGenerationConfigSettings ConfigSettings { get; }
        internal ILogger<AcmeAcquireGenerationFcnTrigger> Log { get; private set; }
        internal IBlobContainerWrapper BlobContainerWrapper { get; }

        public AcmeAcquireGenerationFcnTrigger(
            IAcmeAcquireGenerationOrchestrator orchestrator,
            IOptions<AcquireAcmeGenerationConfigSettings> settings,
            IOptions<BlobConfigInfo> blobConfigInfo,
            ILogger<AcmeAcquireGenerationFcnTrigger> log)
        {
            this.Orchestrator = orchestrator;
            this.ConfigSettings = settings.Value;
            this.BlobContainerWrapper = new BlobContainerWrapper(blobConfigInfo.Value);
            this.Log = log;
        }

        [Function(nameof(AcquireAcmeGenerationHttpStart))]
        public async Task<IActionResult> AcquireAcmeGenerationHttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req,
            [DurableClient] DurableTaskClient orchestrationClient)
        {
            try
            {
                var httpClientType = ConfigSettings.UseHttpInternetMock ? nameof(AcmeHttpClientInternetMock) : nameof(AcmeHttpClient);
                Log.LogInformation($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] C# HTTP trigger function processed a request");
                Log.LogInformation($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] HttpClient Type: {httpClientType}");

                // Can use this if we previously timed-out waiting for RunId to finish at Acme.  Passing in this
                // value will bypass the Launching of the job at Acme and go right back to Waiting for the job to complete.
                var acmeHistoryId = req.Query[AcmeHistoryId];

                // Look to see if we just want to return some settings info
                var getSettings = req.Query[GetSettingsParam];
                if (getSettings != null && bool.Parse(getSettings))
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"Request Params: {GetSettingsParam}={getSettings}");
                    sb.AppendLine($"Wind Filespec: {AcmeHelpers.WindFileSpecPrefix.Replace("#","")}_version");
                    sb.AppendLine($"Solar Filespec: {AcmeHelpers.SolarFileSpecPrefix.Replace("#", "")}_version");
                    return new ObjectResult(sb.ToString());
                }

                string msg;

                // Basic Validation: Is it Json and can we parse it into our expected model? If not, error out right away.  
                var requestJson = await req.ReadAsStringAsync();
                Log.LogInformation($"RequestBody_JSON: {requestJson}");

                if (!requestJson.IsValidJson())
                {
                    msg = $"Bogus Generation Request Body specified, try again: {Environment.NewLine}{requestJson}";
                    Log.LogInformation(msg);
                    return new BadRequestObjectResult(msg);
                }

                // See if we need to read in a _file_ containing the projects.  This is needed because the ADF pipeline
                // has a problem forming up and/or sending a huge json request body.  
                if (requestJson.TryParseJson<AcquireGenerationInputFileModel>(out var acquireGenerationFileInput))
                {
                    msg = $"Getting Project List from file: {acquireGenerationFileInput.ProjectList}";
                    Log.LogInformation(msg);
                     
                    // Read in the file and re-assign requestJson to its contents 
                    var projectListFile = this.BlobContainerWrapper.GetBlob(acquireGenerationFileInput.ProjectList);
                    requestJson = await projectListFile.DownloadTextAsync();
                }

                if (!requestJson.TryParseJson<List<AcquireGenerationInputModel>>(out var acquireGenerationInput))
                {
                    msg = $"Unable to properly parse request body: ({requestJson}), try again.";
                    Log.LogInformation(msg);
                    return new BadRequestObjectResult(msg);
                }

                // Used to protect against > 1 of the same exact job request running at the same time (singleton orchestrator pattern)
                var orchestrationId = AcmeAcquireGenerationHelpers.GetOrchestrationId(acquireGenerationInput);

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
            catch (Exception ex)
            {
                Log.LogError(ex, "Epic Fail");
                return new BadRequestObjectResult(ex);
            }
        }
    }
}