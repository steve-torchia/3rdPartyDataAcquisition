using DP.Base.Contracts;
using DP.Base.Extensions;
using Ingress.Lib.Base;
using Ingress.Lib.Base.Contracts;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Acme.Contracts;
using Newtonsoft.Json;

namespace Acme.AcquireGeneration.Test
{
    public class AcmeAcquireGenerationTestHelpers
    {
        public const string DefaultConnectionString = "UseDevelopmentStorage=true";
        public const string DefaultContainerName = "thirdpartygeneration-test";

        public const string AcquirePath = "/modeled/acquired/acme/projects/ToBeProcessed/";

        public static AcquireGenerationInputModel GetTestWindGenerationRequest(string weatherYear = "2021")
        {
            return new AcquireGenerationInputModel()
            {
                ProjectId = "0E33CB80-F2E6-46EB-8FDF-543867608621",
                ProjectNumber = "PR-000001",
                ProjectName = "Vandelay Wind Industries",
                Product = "Wind",
                NameplateCapacityMwac = "140",
                WeatherYear = weatherYear,
                Latitude = "40.8055772",
                Longitude = "-73.9655785",
                TurbineManufacturer = "Kramerica",
                TurbineModel = "K-2468",
                TurbineHubHeight = "85",
                NumTurbines = "0",
                Azimuth = "",
                Tilt = "",
                PanelType = "",
                RackingType = ""
            };
        }

        public static AcquireGenerationInputModel GetTestSolarGenerationRequest(string weatherYear = "2021")
        {
            return new AcquireGenerationInputModel()
            {
                ProjectId = "970A0DA4-12E5-4974-B170-44F0669AE837",
                ProjectNumber = "PR-000002",
                ProjectName = "Vandelay Solar Industries",
                Product = "Solar",
                NameplateCapacityMwdc = "100",
                NameplateCapacityMwac = "90",
                WeatherYear = weatherYear,
                Latitude = "40.8055772",
                Longitude = "-73.9655785",
                TurbineManufacturer = "",
                TurbineModel = "",
                TurbineHubHeight = "",
                NumTurbines = "",
                Azimuth = "",
                Tilt = "",
                PanelType = "N/A",
                RackingType = "N/A",
                DcAcRatio = "1.11",
                IsBifacial = "true"
            };
        }

    public static AcmeAcquireGenerationContext GetDefaultCtx(string generationType = "wind", string weatherYear = "2021")
        {
            AcmeAcquireGenerationContext retVal = null;

            if (generationType.EqualsIgnoreCase("wind"))
            {
                retVal = AcmeAcquireGenerationHelpers.GetAcmeAcquireGenerationContext(GetTestWindGenerationRequest(weatherYear), GetTestDestinationInfo());
            }

            if (generationType.EqualsIgnoreCase("solar"))
            {
                retVal = AcmeAcquireGenerationHelpers.GetAcmeAcquireGenerationContext(GetTestSolarGenerationRequest(weatherYear), GetTestDestinationInfo());
            }

            return retVal;
        }

        public static string GetDestinationBlobUri(AcmeAcquireGenerationContext ctx)
        {
            var filePrefix = AcmeAcquireGenerationHelpers.GetFileNamePrefix(ctx.Project);

            return $"{AcquirePath}{filePrefix}";
        }

        public static string GetDefaultCtxJson(string startTime = null)
        {
            var ret = GetDefaultCtx(startTime);
            return ret.ToJsonEx();
        }

        public static BlobConfigInfo GetTestDestinationInfo(string connectionString = null, string containerName = null, string rootPath = null)
        {
            var blobDestinationInfo = new BlobConfigInfo()
            {
                ConnectionString = connectionString ?? DefaultConnectionString,
                ContainerName = containerName ?? DefaultContainerName,
            };

            return blobDestinationInfo;
        }

        public static IOptions<BlobConfigInfo> GetMockDestinationInfo()
        {
            return Options.Create<BlobConfigInfo>(new BlobConfigInfo
            {
                ConnectionString = "foo",
                ContainerName = "bar"
            });
        }

        public static Mock<IBlobContainerWrapper> GetBlobContainerWrapperMock(string connectionString = null, string containerName = null)
        {
            // skt_todo: utilize connectionString & containerName at some point?

            var blobContainerWrapperMock = new Mock<IBlobContainerWrapper>();

            blobContainerWrapperMock
                .Setup(x => x.UploadResult(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(Task.FromResult("hello there"));

            blobContainerWrapperMock
                .Setup(x => x.UploadResult(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(Task.FromResult("hello there"));

            blobContainerWrapperMock
                .Setup(x => x.UploadResult(It.IsAny<string>(), It.IsAny<MemoryStream>()))
                .Returns(Task.FromResult("hello there"));

            blobContainerWrapperMock
                .Setup(x => x.GetAbsoluteUri())
                .Returns(string.Empty);

            return blobContainerWrapperMock;
        }

        public static Mock<IAcmeHttpClient> GetHttpClientMock(bool success, string successMsg = null, string failureMsg = null, string returnVal = null)
        {
            var httpClient = new Mock<IAcmeHttpClient>();

            var retVal = new CallResult<string>
            {
                Success = success,
                DisplayMessage = success ? successMsg : failureMsg,
                ReturnValue = returnVal,
            };

            var httpMsgRetval = new CallResult<HttpResponseMessage>
            {
                Success = success,
                DisplayMessage = success ? successMsg : failureMsg,
                ReturnValue = new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(new string('*', 500))
                }
            };

            httpClient
                .Setup(x => x.LaunchGenerationForWeatherYearJob(It.IsAny<AcmeAcquireGenerationContext>()))
                .ReturnsAsync(retVal);

            httpClient
                .Setup(x => x.WaitForGenerationJobAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(retVal);

            httpClient
                .Setup(x => x.GetGenerationForWeatherYearResultsAsync(It.IsAny<AcmeAcquireGenerationContext>()))
                .ReturnsAsync(httpMsgRetval);

            return httpClient;
        }
    }
}

