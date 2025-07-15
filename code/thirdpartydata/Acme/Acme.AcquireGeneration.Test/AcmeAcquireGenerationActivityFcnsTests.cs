using Ingress.Lib.Base;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Moq;
using System.Net.Http;
using Xunit;
using System.Collections.Generic;
using System;

namespace Acme.AcquireGeneration.Test
{
    public class AcmeAcquireGenerationActivityFcnsTests
    {
        private readonly Mock<ILogger<AcmeAcquireGenerationActivityFcns>> loggerMock;

        private const string HistoryId = "9999";
        private const string DefaultFailureMessage = "It Failed";
        private const string DefaultSuccessMessage = "It succeeded!";
        private const string BlobDestinationPath = "this is a blob destination path";
        private const string DownloadAndSaveFalureMsg = "Error trying to download and/or save results";

        private readonly Mock<CloudStorageAccount> cloudStorageMock;

        public AcmeAcquireGenerationActivityFcnsTests()
        {
            loggerMock = new Mock<ILogger<AcmeAcquireGenerationActivityFcns>>();
            cloudStorageMock = new Mock<CloudStorageAccount>();
        }

        [Fact]
        public void LaunchGenerationForWeatherYearJobAsync_Success()
        {
            var httpClient = AcmeAcquireGenerationTestHelpers.GetHttpClientMock(success: true, successMsg: DefaultSuccessMessage, returnVal: HistoryId);

            var ctx = AcmeAcquireGenerationTestHelpers.GetDefaultCtx();

            var fcn = new AcmeAcquireGenerationActivityFcns(httpClient.Object, log: loggerMock.Object);

            var ret = fcn.LaunchGenerationForWeatherYearJobAsync(ctx).Result;

            Assert.True(ret.Success);
            Assert.Equal(HistoryId, ret.ReturnValue);
        }

        [Fact]
        public void LaunchGenerationForWeatherYearJobAsync_Fail()
        {
            var httpClient = AcmeAcquireGenerationTestHelpers.GetHttpClientMock(success: false, failureMsg: DefaultFailureMessage);

            var ctx = AcmeAcquireGenerationTestHelpers.GetDefaultCtx();

            var fcn = new AcmeAcquireGenerationActivityFcns(httpClient.Object, log: loggerMock.Object);

            var ret = fcn.LaunchGenerationForWeatherYearJobAsync(ctx).Result;

            Assert.False(ret.Success);
            Assert.Equal(DefaultFailureMessage, ret.DisplayMessage);
        }

        [Fact]
        public void WaitForGenerationJobAsync_Success()
        {
            var httpClient = AcmeAcquireGenerationTestHelpers.GetHttpClientMock(true, successMsg: DefaultSuccessMessage, returnVal: HistoryId);

            var fcn = new AcmeAcquireGenerationActivityFcns(httpClient.Object, log: loggerMock.Object);

            var ret = fcn.WaitForGenerationJobAsync(HistoryId).Result;

            Assert.True(ret.Success);
            Assert.Equal(HistoryId, ret.ReturnValue);
        }

        [Fact]
        public void WaitForGenerationJobAsync_Fail()
        {
            var httpClient = AcmeAcquireGenerationTestHelpers.GetHttpClientMock(success: false, failureMsg: DefaultFailureMessage, returnVal: DefaultFailureMessage);

            var fcn = new AcmeAcquireGenerationActivityFcns(httpClient.Object, log: loggerMock.Object);

            var ret = fcn.WaitForGenerationJobAsync(HistoryId).Result;

            Assert.False(ret.Success);
            Assert.Equal(DefaultFailureMessage, ret.DisplayMessage);
        }

        [Fact]
        public void GetGenerationDataAndSaveToBlobAsync_Success()
        {
            var httpClient = AcmeAcquireGenerationTestHelpers.GetHttpClientMock(success: true, successMsg: DefaultSuccessMessage);

            var ctx = AcmeAcquireGenerationTestHelpers.GetDefaultCtx();

            var blobContainerWrapper = AcmeAcquireGenerationTestHelpers.GetBlobContainerWrapperMock();

            var fcn = new AcmeAcquireGenerationActivityFcns(httpClient.Object, blobContainerWrapper.Object);

            var ret = fcn.GetGenerationDataAndSaveToBlobAsync(ctx).Result;

            Assert.True(ret.Success);
            var tmp = AcmeAcquireGenerationTestHelpers.GetDestinationBlobUri(ctx);
            Assert.StartsWith(tmp, ret.ReturnValue);
        }


        public const string RealBlobContainerConnectionString = "UseDevelopmentStorage=true";
        public const string RealBlobContainerContainerName = "thirdpartygeneration-test";

        /// <summary>
        /// Use this to test writing to real blob instead of mock
        /// </summary>
        //[Fact]
        [Fact(Skip = "manual only for debugging")]
        public void GetGenerationDataAndSaveToBlobAsync_Success_RealBlobDestination()
        {
            var httpClient = AcmeAcquireGenerationTestHelpers.GetHttpClientMock(success: true, successMsg: DefaultSuccessMessage);

            var ctx = AcmeAcquireGenerationTestHelpers.GetDefaultCtx();

            var realBlobContainerWrapper = new BlobContainerWrapper(
                AcmeAcquireGenerationTestHelpers.GetTestDestinationInfo(RealBlobContainerConnectionString, RealBlobContainerContainerName));

            var fcn = new AcmeAcquireGenerationActivityFcns(httpClient.Object, realBlobContainerWrapper, log: loggerMock.Object);

            var ret = fcn.GetGenerationDataAndSaveToBlobAsync(ctx).Result;

            Assert.True(ret.Success);
        }

        [Fact]
        public void GetGenerationDataAndSaveToBlobAsync_Fail()
        {
            var httpClient = AcmeAcquireGenerationTestHelpers.GetHttpClientMock(success: false, failureMsg: DownloadAndSaveFalureMsg);

            var ctx = AcmeAcquireGenerationTestHelpers.GetDefaultCtx();

            var fcn = new AcmeAcquireGenerationActivityFcns(httpClient.Object, log: loggerMock.Object);

            var ret = fcn.GetGenerationDataAndSaveToBlobAsync(ctx).Result;

            Assert.False(ret.Success);
            Assert.Equal(DownloadAndSaveFalureMsg, ret.DisplayMessage);
        }

        [Fact]
        public void ValidateAndSave_Success()
        {
            var httpClient = AcmeAcquireGenerationTestHelpers.GetHttpClientMock(success: true, successMsg: DefaultSuccessMessage);
            var fcn = new AcmeAcquireGenerationActivityFcns(httpClient.Object, log: loggerMock.Object);
            var blobContainerWrapper = AcmeAcquireGenerationTestHelpers.GetBlobContainerWrapperMock();

            var httpResponse = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(new string('*', 500))
            };

            var ret = fcn.ValidateAndSave(httpResponse, "filename.csv", blobContainerWrapper.Object);

            Assert.Null(ret.Exception);
        }

        [Fact]
        public void ValidateAndSave_Fail()
        {
            var httpClient = AcmeAcquireGenerationTestHelpers.GetHttpClientMock(success: true, successMsg: DefaultSuccessMessage);
            var fcn = new AcmeAcquireGenerationActivityFcns(httpClient.Object, log: loggerMock.Object);
            var blobContainerWrapper = AcmeAcquireGenerationTestHelpers.GetBlobContainerWrapperMock();

            var httpResponse = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                Content = new StringContent(new string('*', 500))
            };

            var ret = fcn.ValidateAndSave(httpResponse, "filename.csv", blobContainerWrapper.Object);

            Assert.NotNull(ret.Exception);
        }
    }
}
