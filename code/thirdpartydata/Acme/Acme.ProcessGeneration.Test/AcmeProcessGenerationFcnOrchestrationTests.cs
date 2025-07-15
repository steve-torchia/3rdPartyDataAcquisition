using DurableFunctionsCommon;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ingress.Lib.Base;
using DP.Base.Contracts;
using Acme.Contracts;
using System.Collections.Generic;
using Microsoft.DurableTask;

namespace Acme.ProcessGeneration.Test
{
    public class AcmeProcessGenerationFcnOrchestrationTests : OrchestrationClientTestingSupport
    {
        private readonly Mock<ILogger<AcmeProcessGenerationOrchestrator>> loggerMock;

        public AcmeProcessGenerationFcnOrchestrationTests()
        {
            loggerMock = new Mock<ILogger<AcmeProcessGenerationOrchestrator>>();
        }

        [Fact]
        public void AcmeProcessGenerationOrchestratorAsync_Success()
        {
            // Mock IDurableClient
            var ctx = GetOrchestrationCtxMock(
                getFileListSuccess: true,
                processJobSuccess: true,
                moveFileResult: true);

            var orchestrator = GetOrchestrator();

            var ret = orchestrator.AcmeProcessGenerationOrchestratorAsync(ctx.Object).Result;

            Assert.True(ret.Success.Count == 2);
            Assert.True(ret.Success[0] == "PR-00001853");
            Assert.True(ret.Success[1] == "PR-00002891");
        }

        [Fact]
        public void AcmeProcessGenerationOrchestratorAsync_FailedLaunch()
        {
            // Mock IDurableClient
            var ctx = GetOrchestrationCtxMock(
                getFileListSuccess: false,
                processJobSuccess: true, // immaterial
                moveFileResult: true); // immaterial

            var orchestrator = GetOrchestrator();

            var ret = orchestrator.AcmeProcessGenerationOrchestratorAsync(ctx.Object).Result;

            Assert.True(ret.Success.Count == 0);
            Assert.True(ret.Failure.Count == 0);
            Assert.Contains("GetListOfGenerationFilesToProcess Failed", ret.Notes[0]);
        }

        [Fact]
        public void AcmeProcessGenerationOrchestratorAsync_FailedProcess()
        {
            // Mock IDurableClient
            var ctx = GetOrchestrationCtxMock(
                getFileListSuccess: true,
                processJobSuccess: false,
                moveFileResult: true); // immaterial

            var orchestrator = GetOrchestrator();

            var ret = orchestrator.AcmeProcessGenerationOrchestratorAsync(ctx.Object).Result;

            Assert.True(ret.Success.Count == 0);
            Assert.True(ret.Failure.Count == 2);
            Assert.True(ret.Failure[0] == "PR-00001853");
            Assert.True(ret.Failure[1] == "PR-00002891");
        }

        [Fact]
        public void AcmeProcessGenerationOrchestratorAsync_FailedFileMove()
        {
            // Mock IDurableClient
            var ctx = GetOrchestrationCtxMock(
                getFileListSuccess: true,
                processJobSuccess: true,
                moveFileResult: false);

            var orchestrator = GetOrchestrator();

            var ret = orchestrator.AcmeProcessGenerationOrchestratorAsync(ctx.Object).Result;

            Assert.True(ret.Success.Count == 0);
            Assert.True(ret.Failure.Count == 2);
            Assert.True(ret.Details.Failure[0].Id == "PR-00001853");
            Assert.True(ret.Details.Failure[0].Error == "ERROR trying to Move zip file: Wind_PR-00001853_Hidalgo Wind Farm LLC Los Mirasoles_2021_26.49_-98.38_250_VESTAS_V110-2.0_80_1_2.zip => boooo!");
            Assert.True(ret.Details.Failure[1].Id == "PR-00002891");
            Assert.True(ret.Details.Failure[1].Error == "ERROR trying to Move zip file: Solar_PR-00002891_Mechanicsville Solar LLC_2021_37.67_-77.2_26_~_~_~_~_1.zip => boooo!");
        }

        private AcmeProcessGenerationOrchestrator GetOrchestrator()
        {
            Mock<IOptions<GlobalConfigSettings>> mockGlobalConfigSettings = new Mock<IOptions<GlobalConfigSettings>>();

            IOptions<BlobConfigInfo> mockDestinationInfo =
                Options.Create<BlobConfigInfo>(new BlobConfigInfo
                {
                    ConnectionString = "foo",
                    ContainerName = "bar"
                });

            return new AcmeProcessGenerationOrchestrator(loggerMock.Object, mockDestinationInfo, mockGlobalConfigSettings.Object);
        }

        private Mock<TaskOrchestrationContext> GetOrchestrationCtxMock(bool getFileListSuccess, bool processJobSuccess, bool moveFileResult)
        {
            var orchestrationCtxMock = new Mock<TaskOrchestrationContext>();

            // Mock getting the inputs 
            orchestrationCtxMock
                 .Setup(x => x.GetInput<string>())
                 .Returns(string.Empty);

            // Mock the GetList Call
            orchestrationCtxMock
                .Setup(x => x.CallActivityAsync<CallResult<List<string>>>(nameof(AcmeProcessGenerationActivityFcns.GetListOfGenerationFilesToProcess), It.IsAny<AcmeProcessGenerationContext>(), null))
                .ReturnsAsync(new CallResult<List<string>>
                {
                    Success = getFileListSuccess,
                    ReturnValue = getFileListSuccess
                    ? new List<string>
                    {
                        "Wind_PR-00001853_Hidalgo Wind Farm LLC Los Mirasoles_2021_26.49_-98.38_250_VESTAS_V110-2.0_80_1_2.zip",
                        "Solar_PR-00002891_Mechanicsville Solar LLC_2021_37.67_-77.2_26_~_~_~_~_1.zip"
                    }
                    : new List<string>()
                });

            // Mock the SubOrchestration
            orchestrationCtxMock
                .Setup(x => x.CallSubOrchestratorAsync<CallResult<string>>(nameof(AcmeProcessGenerationSubOrchestrator.AcmeProcessGenerationSubOrchestratorAsync), It.IsAny<AcmeProcessGenerationContext>(), null))
                .ReturnsAsync(new CallResult<string>
                {
                    Success = processJobSuccess,
                    ReturnValue = processJobSuccess ? "yay!" : "boooo!"
                });

            // mock the move file activity
            orchestrationCtxMock
                .Setup(x => x.CallActivityAsync<CallResult>(nameof(AcmeProcessGenerationActivityFcns.MoveZipFilesToRawInputFolder), It.IsAny<AcmeProcessGenerationContext>(), null))
                .ReturnsAsync(new CallResult
                {
                    Success = moveFileResult,
                    DisplayMessage = moveFileResult ? "yay!" : "boooo!"
                });

            return orchestrationCtxMock;
        }
    }
}
