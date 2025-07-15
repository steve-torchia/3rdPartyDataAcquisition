using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;
using Acme.Contracts;
using Xunit;
using System.Collections.Generic;
using Ingress.Lib.Base;
using Microsoft.DurableTask.Client;
using System.Threading;
using Microsoft.DurableTask;
using Microsoft.Azure.Functions.Worker.Http;
using DurableFunctionsCommon;
using System.Collections.Specialized;

namespace Acme.AcquireGeneration.Test
{
    public class AcmeAcquireGenerationFcnTriggerTests
    {
        const string OrchestratorFunctionName = "TestFunctionName";
        const string OrchestrationInstanceId = "660875_20200701000000_20200701015900_15";

        private readonly Mock<IAcmeAcquireGenerationOrchestrator> orchestrationMock = new Mock<IAcmeAcquireGenerationOrchestrator>();
        private readonly Mock<DurableTaskClient> durableClientMock = new Mock<DurableTaskClient>("NameGoesHere");   
        private readonly Mock<ILogger<AcmeAcquireGenerationFcnTrigger>> loggerMock = new Mock<ILogger<AcmeAcquireGenerationFcnTrigger>>();

        private readonly Mock<HttpRequestData> httpRequestData;

        public AcmeAcquireGenerationFcnTriggerTests(bool useBogusRequestBody = false)
        {
            object body = null;
            if (useBogusRequestBody)
            {
                body= "fubar";
            }
            else
            {
                body = new List<AcquireGenerationInputModel>() { AcmeAcquireGenerationTestHelpers.GetTestWindGenerationRequest("2021") };
            }

            // "Mock" http request that gets sent to the HttpTrigger function
            httpRequestData = DurableFunctionTestHelpers.CreateMockRequest(body, GetQuery());

            var orchestrationOptionsMock = new StartOrchestrationOptions("hi");

            // Mock StartNewAsync method for the orchestrator
            durableClientMock
                .Setup(x => x.ScheduleNewOrchestrationInstanceAsync(
                    It.IsAny<TaskName>(),
                    It.IsAny<string>(),
                    orchestrationOptionsMock,
                    It.IsAny<CancellationToken>()));
                        //.ReturnsAsync(OrchestrationInstanceId);

            // Make sure the workflow returns something
            orchestrationMock
                .Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DurableTaskClient>(), It.IsAny<HttpRequestData>()))
                .ReturnsAsync(new OkObjectResult(new { result = "success!!" }));
        }

        [Fact]
        public async Task AcquireAcmeGenerationHttpStart_OrchestrationCompleted()
        {
            // Test trigger where orchestration was completed
            var httpTriggerFcn = GetHttpTriggerFcn(OrchestrationRuntimeStatus.Completed);

            // Call Orchestration trigger function
            var result = await httpTriggerFcn.AcquireAcmeGenerationHttpStart(
               httpRequestData.Object,
               durableClientMock.Object);

            Assert.True(result is OkObjectResult);
        }

        [Fact]
        public async Task AcquireAcmeGenerationHttpStart_OrchestrationInProcess()
        {
            // Test trigger where orchestration is still running
            var httpTriggerFcn = GetHttpTriggerFcn(OrchestrationRuntimeStatus.Running);

            // Call Orchestration trigger function
            var result = await httpTriggerFcn.AcquireAcmeGenerationHttpStart(
               httpRequestData.Object,
               durableClientMock.Object);

            Assert.True(result is BadRequestObjectResult);
            var r = (BadRequestObjectResult)result;
            Assert.Contains("already exists", r.Value.ToString());
        }

        private static NameValueCollection GetQuery()
        {
            // query as expected by the HTTP Trigger function
            NameValueCollection query = new NameValueCollection
            {
                { "HistoryId", "9999" }
            };

            return query;
        }

        private AcmeAcquireGenerationFcnTrigger GetHttpTriggerFcn(OrchestrationRuntimeStatus durableOrchestrationStatus)
        {
            IOptions<AcquireAcmeGenerationConfigSettings> mockConfig =
                Options.Create<AcquireAcmeGenerationConfigSettings>(new AcquireAcmeGenerationConfigSettings
                {
                });

            IOptions<BlobConfigInfo> mockBlobConfig = 
                Options.Create<BlobConfigInfo>(new BlobConfigInfo
                {
                    ConnectionString = "UseDevelopmentStorage=true",
                    ContainerName = "container_name_this_is"
                });

            // Create Trigger Function with a mocked underlying client status
            return new AcmeAcquireGenerationFcnTrigger(orchestrationMock.Object, mockConfig, mockBlobConfig, loggerMock.Object )
            {
                // Mock the CreateCheckStatusResponse method.  
                // ** Note: Moq doesn't support extension methods so as a workaround we have to write a little code to support injecting this method
                OrchestrationClientGetStatusAsyncMethod = (obj, val) =>
                {
                    return Task.FromResult(new OrchestrationMetadata("foo", val)
                    {
                        RuntimeStatus = durableOrchestrationStatus
                    });
                }
            };
        }
    }
}
