using DurableFunctionsCommon;
using Moq;
using Xunit;
using DP.Base.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ingress.Lib.Base;
using Acme.Contracts;
using Microsoft.DurableTask;

namespace Acme.AcquireGeneration.Test
{
    public class AcmeAcquireGenerationFcnSubOrchestrationTests : OrchestrationClientTestingSupport
    {
        private readonly Mock<ILogger<AcmeAcquireGenerationSubOrchestrator>> loggerMock;

        public AcmeAcquireGenerationFcnSubOrchestrationTests()
        {
            loggerMock = new Mock<ILogger<AcmeAcquireGenerationSubOrchestrator>>();
        }

        [Fact]
        public void AcmeAcquireGenerationOrchestratorAsync_Success()
        {
            // Mock IDurableClient
            var ctx = GetSubOrchestrationCtxMock(
                launchJobSuccess: true,
                waitForJobSuccess: true,
                getGenerationDataAndSaveSuccess: true);

            var subOrchestrator = GetSubOrchestrator();

            var ret = subOrchestrator.AcmeAcquireGenerationSubOrchestratorAsync(ctx.Object).Result;

            Assert.True(ret.Success);
            Assert.True(ret.DisplayMessage.Equals("Success: Saved Generation Data (HistoryId=999) to: SuccessfullySavedFile.csv"));
        }

        [Fact]
        public void AcmeAcquireGenerationOrchestratorAsync_FailedLaunch()
        {
            // Mock IDurableClient
            var ctx = GetSubOrchestrationCtxMock(
                launchJobSuccess: false,
                waitForJobSuccess: true, // immaterial 
                getGenerationDataAndSaveSuccess: true); // immaterial

            var subOrchestrator = GetSubOrchestrator();

            var ret = subOrchestrator.AcmeAcquireGenerationSubOrchestratorAsync(ctx.Object).Result;

            Assert.False(ret.Success);
            Assert.Contains("LaunchGenerationForWeatherYearJobAsync Failed:", ret.DisplayMessage);
        }

        [Fact]
        public void AcmeAcquireGenerationOrchestratorAsync_FailedWaitForResults()
        {
            // Mock IDurableClient
            var ctx = GetSubOrchestrationCtxMock(
                launchJobSuccess: true,
                waitForJobSuccess: false,
                getGenerationDataAndSaveSuccess: true); // immaterial

            var subOrchestrator = GetSubOrchestrator();

            var ret = subOrchestrator.AcmeAcquireGenerationSubOrchestratorAsync(ctx.Object).Result;

            Assert.False(ret.Success);
            Assert.Contains("WaitForGenerationJobAsync Failed (HistoryId=999): Bad Wait Display Message", ret.DisplayMessage);
        }

        [Fact]
        public void AcmeAcquireGenerationOrchestratorAsync_FailedDownloadAndSave()
        {
            // Mock IDurableClient
            var ctx = GetSubOrchestrationCtxMock(
                launchJobSuccess: true,
                waitForJobSuccess: true,
                getGenerationDataAndSaveSuccess: false);

            var subOrchestrator = GetSubOrchestrator();

            var ret = subOrchestrator.AcmeAcquireGenerationSubOrchestratorAsync(ctx.Object).Result;

            Assert.False(ret.Success);
            Assert.Contains("GetGenerationDataAndSaveToBlob Failed: Bad Save Display Message!", ret.DisplayMessage);
        }

        private AcmeAcquireGenerationSubOrchestrator GetSubOrchestrator()
        {
            return new AcmeAcquireGenerationSubOrchestrator(loggerMock.Object);
        }

        private Mock<TaskOrchestrationContext> GetSubOrchestrationCtxMock(bool launchJobSuccess, bool waitForJobSuccess, bool getGenerationDataAndSaveSuccess)
        {
            var subOrchestrationCtxMock = new Mock<TaskOrchestrationContext>();

            var project = AcmeAcquireGenerationTestHelpers.GetTestWindGenerationRequest("2021");
            var acmeAcquireGenerationContext = new AcmeAcquireGenerationContext()
            {
                Project = project,
            };

            // Mock getting the inputs 
            subOrchestrationCtxMock
                 .Setup(x => x.GetInput<AcmeAcquireGenerationContext>())
                 .Returns(acmeAcquireGenerationContext);

            // Mock the Launch Job call
            subOrchestrationCtxMock
                .Setup(x => x.CallActivityAsync<CallResult<string>>(nameof(AcmeAcquireGenerationActivityFcns.LaunchGenerationForWeatherYearJobAsync), acmeAcquireGenerationContext, null))
                .ReturnsAsync(new CallResult<string>
                {
                    Success = launchJobSuccess,
                    DisplayMessage = launchJobSuccess ? "Successful Launch Display Message" : "Bad Launch Display Message!",
                    ReturnValue = launchJobSuccess ? "999" : "Bad Launch Return Value!"
                });

            // Mock the wait-for-job-results call
            subOrchestrationCtxMock
                .Setup(x => x.CallActivityAsync<CallResult<string>>(nameof(AcmeAcquireGenerationActivityFcns.WaitForGenerationJobAsync), It.IsAny<string>(), null))
                .ReturnsAsync(new CallResult<string>
                {
                    Success = waitForJobSuccess,
                    DisplayMessage = waitForJobSuccess ? "Successful Wait Display Message" : "Bad Wait Display Message!",
                    ReturnValue = waitForJobSuccess ? "Successful Wait Return Value" : "Bad Wait Return Value",
                });

            // Mock the download-and-save results call
            subOrchestrationCtxMock
                .Setup(x => x.CallActivityAsync<CallResult<string>>(nameof(AcmeAcquireGenerationActivityFcns.GetGenerationDataAndSaveToBlobAsync), It.IsAny<AcmeAcquireGenerationContext>(), null))
                .ReturnsAsync(new CallResult<string>
                {
                    Success = getGenerationDataAndSaveSuccess,
                    DisplayMessage = getGenerationDataAndSaveSuccess ? "Successful Save Display Message" : "Bad Save Display Message!",
                    ReturnValue = getGenerationDataAndSaveSuccess ? "SuccessfullySavedFile.csv" : "Bad Save Return Value!",
                });

            return subOrchestrationCtxMock;
        }
    }
}
