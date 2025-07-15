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

namespace Acme.ProcessGeneration.Test
{
    public class AcmeProcessGenerationTestHelpers
    {
        public const string DefaultConnectionString = "UseDevelopmentStorage=true";
        public const string DefaultContainerName = "thirdpartygeneration-test";

        public const string AcquirePath = "/modeled/Acme/acquired/projects/ToBeProcessed/";

 
        public static AcmeProcessGenerationContext GetDefaultCtx(string generationType = "wind", string weatherYear = "2021")
        {
            AcmeProcessGenerationContext retVal = null;

            //if (generationType.EqualsIgnoreCase("wind"))
            //{
            //    var objJson = JsonConvert.SerializeObject(GetTestWindGenerationRequest(weatherYear));
            //    retVal = AcmeProcessGenerationHelpers.GetAcmeAcquireGenerationContext(objJson, GetTestDestinationInfo());
            //}

            //if (generationType.EqualsIgnoreCase("solar"))
            //{
            //    var objJson = JsonConvert.SerializeObject(GetTestSolarGenerationRequest(weatherYear));
            //    retVal = AcmeProcessGenerationHelpers.GetAcmeAcquireGenerationContext(objJson, GetTestDestinationInfo());
            //}

            return retVal;
        }

        public static string GetProcessRequest(string requestJson = null)
        {
            var retVal = new
            {
                body = requestJson,
            };

            return JsonConvert.SerializeObject(retVal);
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
    }
}

