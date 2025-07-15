using DurableFunctionsCommon;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ingress.Lib.Base;
using Acme.Contracts;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using Microsoft.DurableTask;

namespace Acme.AcquireGeneration.Test
{

    public class AcmeAcquireGenerationFcnOrchestrationTests : OrchestrationClientTestingSupport
    {
        private readonly Mock<ILogger<AcmeAcquireGenerationOrchestrator>> loggerMock;

        public AcmeAcquireGenerationFcnOrchestrationTests()
        {
            loggerMock = new Mock<ILogger<AcmeAcquireGenerationOrchestrator>>();
        }

        [Fact]
        public void AcmeAcquireGenerationOrchestratorAsync_Success()
        {
            var projectList = new List<AcquireGenerationInputModel>()
            {
                AcmeAcquireGenerationTestHelpers.GetTestWindGenerationRequest("2021"),
            };

            // Make equivalent list of AcmeAcquireGenerationContexts for expected results
            var resultSet = new List<Tuple<AcmeAcquireGenerationContext, bool>>
            {
                new Tuple<AcmeAcquireGenerationContext, bool>(new AcmeAcquireGenerationContext() { Project = projectList[0] }, true), // this one passes
            };

            // Get an orchestrator that mocks the underlying throttling of the SubOrchestrator tasks
            var orchestrator = GetOrchestratorMock(resultSet);

            // build up mock orchestration context
            var orchestrationCtx = GetOrchestrationCtxMock(projectList);

            // Run Orchestration
            var ret = orchestrator.AcmeAcquireGenerationOrchestratorAsync(orchestrationCtx.Object).Result;

            Assert.True(ret.Details.Success.Count == 1);
            Assert.True(ret.Details.Failure.Count == 0);

            Assert.True(ret.Details.Success[0].Equals($"{projectList[0].ProjectNumber} => Success=True"));
        }

        [Fact]
        public void AcmeAcquireGenerationOrchestratorAsync_Failed()
        {
            var projectList = new List<AcquireGenerationInputModel>()
            {
                AcmeAcquireGenerationTestHelpers.GetTestWindGenerationRequest("2021"),
            };

            // Make equivalent list of AcmeAcquireGenerationContexts for expected results
            var resultSet = new List<Tuple<AcmeAcquireGenerationContext, bool>>
            {
                new Tuple<AcmeAcquireGenerationContext, bool>(new AcmeAcquireGenerationContext() { Project = projectList[0] }, false), // this one fails
            };

            // Get an orchestrator that mocks the underlying throttling of the SubOrchestrator tasks
            var orchestrator = GetOrchestratorMock(resultSet);

            // build up mock orchestration context
            var orchestrationCtx = GetOrchestrationCtxMock(projectList);

            // Run Orchestration
            var ret = orchestrator.AcmeAcquireGenerationOrchestratorAsync(orchestrationCtx.Object).Result;

            Assert.True(ret.Details.Success.Count == 0);
            Assert.True(ret.Details.Failure.Count == 1);

            Assert.True(ret.Details.Failure[0].Id.Equals($"{projectList[0].ProjectNumber}"));
            Assert.True(ret.Details.Failure[0].Error.Equals($"Success=False"));
        }

        [Fact]
        public void AcmeAcquireGenerationOrchestratorAsync_OnePassed_OneFailed()
        {
            var projectList = new List<AcquireGenerationInputModel>()
            {
                AcmeAcquireGenerationTestHelpers.GetTestWindGenerationRequest("2021"),
                AcmeAcquireGenerationTestHelpers.GetTestSolarGenerationRequest("2020")
            };

            // Make equivalent list of AcmeAcquireGenerationContexts for expected results
            var resultSet = new List<Tuple<AcmeAcquireGenerationContext, bool>>
            {
                new Tuple<AcmeAcquireGenerationContext, bool>(new AcmeAcquireGenerationContext() { Project = projectList[0] }, false), // this one fails
                new Tuple<AcmeAcquireGenerationContext, bool>(new AcmeAcquireGenerationContext() { Project = projectList[1] }, true)   // this one succeeds
            };

            // Get an orchestrator that mocks the underlying throttling of the SubOrchestrator tasks
            var orchestrator = GetOrchestratorMock(resultSet);

            // build up mock orchestration context
            var orchestrationCtx = GetOrchestrationCtxMock(projectList);

            // Run Orchestration
            var ret = orchestrator.AcmeAcquireGenerationOrchestratorAsync(orchestrationCtx.Object).Result;

            Assert.True(ret.Details.Success.Count == 1);
            Assert.True(ret.Details.Failure.Count == 1);

            Assert.True(ret.Details.Failure[0].Id.Equals($"{projectList[0].ProjectNumber}"));
            Assert.True(ret.Details.Failure[0].Error.Equals($"Success=False"));

            Assert.True(ret.Details.Success[0].Equals($"{projectList[1].ProjectNumber} => Success=True"));
        }

        private AcmeAcquireGenerationOrchestrator GetOrchestratorMock(List<Tuple<AcmeAcquireGenerationContext, bool>> resultSet)
        {
            Mock<IOptions<GlobalConfigSettings>> mockGlobalConfigSettings = new Mock<IOptions<GlobalConfigSettings>>();

            IOptions<BlobConfigInfo> mockDestinationInfo =
                    Options.Create<BlobConfigInfo>(new BlobConfigInfo
                    {
                        ConnectionString = "foo",
                        ContainerName = "bar"
                    });

            return new AcmeAcquireGenerationOrchestratorMock(resultSet, loggerMock, mockDestinationInfo, mockGlobalConfigSettings.Object);
        }

        private Mock<TaskOrchestrationContext> GetOrchestrationCtxMock(List<AcquireGenerationInputModel> projectList)
        {
            var orchestrationCtxMock = new Mock<TaskOrchestrationContext>();

            // Mock getting the inputs 
            orchestrationCtxMock
                 .Setup(x => x.GetInput<string>())
                 .Returns(JsonConvert.SerializeObject(projectList));

            return orchestrationCtxMock;
        }
    }
}
