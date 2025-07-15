using DurableFunctionsCommon;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ingress.Lib.Base;
using DP.Base.Contracts;
using Acme.Contracts;
using Microsoft.DurableTask;

namespace Acme.ProcessGeneration.Test
{
    public class AcmeProcessGenerationFcnSubOrchestrationTests : OrchestrationClientTestingSupport
    {
        private readonly Mock<ILogger<AcmeProcessGenerationSubOrchestrator>> loggerMock;

        public AcmeProcessGenerationFcnSubOrchestrationTests()
        {
            loggerMock = new Mock<ILogger<AcmeProcessGenerationSubOrchestrator>>();
        }

        [Fact]
        public void AcmeProcessGenerationSubOrchestratorAsync_Success()
        {
            var ctx = GetSubOrchestrationCtxMock(
                processZipFileSuccess: true);

            var subOrchestrator = GetSubOrchestrator();

            var ret = subOrchestrator.AcmeProcessGenerationSubOrchestratorAsync(ctx.Object).Result;

            Assert.True(ret.ReturnValue == "Saved Generation Data for foo.zip to: yay!");
        }

        [Fact]
        public void AcmeProcessGenerationSubOrchestratorAsync_Fail()
        {
            var ctx = GetSubOrchestrationCtxMock(
                processZipFileSuccess: false);

            var subOrchestrator = GetSubOrchestrator();

            var ret = subOrchestrator.AcmeProcessGenerationSubOrchestratorAsync(ctx.Object).Result;

            Assert.Contains("ProcessZipFile failed for foo.zip", ret.DisplayMessage);
        }


        private AcmeProcessGenerationSubOrchestrator GetSubOrchestrator()
        {
            IOptions<BlobConfigInfo> mockDestinationInfo =
                Options.Create<BlobConfigInfo>(new BlobConfigInfo
                {
                    ConnectionString = "foo",
                    ContainerName = "bar"
                });

            return new AcmeProcessGenerationSubOrchestrator(loggerMock.Object);
        }

        private Mock<TaskOrchestrationContext> GetSubOrchestrationCtxMock(bool processZipFileSuccess)
        {
            var orchestrationCtxMock = new Mock<TaskOrchestrationContext>();

            // Mock getting the inputs 
            orchestrationCtxMock
                 .Setup(x => x.GetInput<AcmeProcessGenerationContext>())
                 .Returns(new AcmeProcessGenerationContext() { ZipFile = "foo.zip" }
                );

            // Mock the Process Zip 
            orchestrationCtxMock
                .Setup(x => x.CallActivityAsync<CallResult<string>>(nameof(AcmeProcessGenerationActivityFcns.ProcessZipFile), It.IsAny<AcmeProcessGenerationContext>(), null))
                .ReturnsAsync(new CallResult<string>
                {
                    Success = processZipFileSuccess,
                    ReturnValue = processZipFileSuccess ? "yay!" : "boooo"
                });

            return orchestrationCtxMock;
        }
    }
}
