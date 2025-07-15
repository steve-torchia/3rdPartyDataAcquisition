using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Acme.Contracts;
using Xunit;
using Newtonsoft.Json;
using Microsoft.DurableTask.Client;
using Microsoft.Azure.Functions.Worker.Http;
using DurableFunctionsCommon;
using Microsoft.DurableTask;
using System.Threading;
using System.Collections.Specialized;

namespace Acme.ProcessGeneration.Test
{
    public class AcmeProcessGenerationFcnTriggerTests
    {
        const string OrchestratorFunctionName = "TestFunctionName";
        const string OrchestrationInstanceId = "660875_20200701000000_20200701015900_15";

        private readonly Mock<IAcmeProcessGenerationOrchestrator> orchestrationMock = new Mock<IAcmeProcessGenerationOrchestrator>();
        private readonly Mock<DurableTaskClient> durableClientMock = new Mock<DurableTaskClient>("NameGoesHere");
        private readonly Mock<ILogger<AcmeProcessGenerationFcnTrigger>> loggerMock = new Mock<ILogger<AcmeProcessGenerationFcnTrigger>>();

        private readonly Mock<HttpRequestData> httpRequestData;

        public AcmeProcessGenerationFcnTriggerTests(bool useBogusRequestBody = false)
        {
            string body = null;
            if (useBogusRequestBody)
            {
                body = "fubar";
            }
            else
            {
                body = AcmeProcessGenerationTestHelpers.GetProcessRequest("2021");
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
        public async Task ProcessAcmeGenerationHttpStart_OrchestrationCompleted()
        {
            // Test trigger where orchestration was completed
            var httpTriggerFcn = GetHttpTriggerFcn(OrchestrationRuntimeStatus.Completed);

            // Call Orchestration trigger function
            var result = await httpTriggerFcn.ProcessAcmeGenerationHttpStart(
               httpRequestData.Object,
               durableClientMock.Object);

            Assert.True(result is OkObjectResult);
        }

        [Fact]
        public async Task ProcessAcmeGenerationHttpStart_OrchestrationInProcess()
        {
            // Test trigger where orchestration is still running
            var httpTriggerFcn = GetHttpTriggerFcn(OrchestrationRuntimeStatus.Running);

            // Call Orchestration trigger function
            var result = await httpTriggerFcn.ProcessAcmeGenerationHttpStart(
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

        private AcmeProcessGenerationFcnTrigger GetHttpTriggerFcn(OrchestrationRuntimeStatus durableOrchestrationStatus)
        {

            // Create Trigger Function with a mocked underlying client status
            return new AcmeProcessGenerationFcnTrigger(orchestrationMock.Object, loggerMock.Object)
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
