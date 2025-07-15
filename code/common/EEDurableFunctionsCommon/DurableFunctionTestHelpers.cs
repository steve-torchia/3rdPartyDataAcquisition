using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json;
using System.IO;
using System;
using Moq;
using System.Net.Http;
using System.Net;
using System.Collections.Specialized;

namespace DurableFunctionsCommon
{
    public class DurableFunctionTestHelpers
    {
        public static Mock<HttpRequestData> CreateMockRequest(object body, NameValueCollection query = null)
        {
            // Create the mock object
            var mockHttpRequestData = new Mock<HttpRequestData>(MockBehavior.Strict, new Mock<FunctionContext>().Object);

            // Define the JSON content to return in the request body

            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            var json = JsonConvert.SerializeObject(body);
            sw.Write(json);
            sw.Flush();
            ms.Position = 0;

            // Set up the properties and methods
            mockHttpRequestData.Setup(req => req.Method).Returns(HttpMethod.Get.Method);
            mockHttpRequestData.Setup(req => req.Query).Returns(query);
            mockHttpRequestData.Setup(req => req.Url).Returns(new Uri("https://example.com"));
            mockHttpRequestData.Setup(req => req.Headers).Returns(new HttpHeadersCollection());
            mockHttpRequestData.Setup(req => req.Body).Returns(ms);
            mockHttpRequestData.Setup(req => req.CreateResponse()).Returns(() =>
            {
                var mockHttpResponseData = new Mock<HttpResponseData>(new Mock<FunctionContext>().Object);
                mockHttpResponseData.Setup(res => res.StatusCode).Returns(HttpStatusCode.OK);
                mockHttpResponseData.Setup(res => res.Headers).Returns(new HttpHeadersCollection());
                mockHttpResponseData.Setup(res => res.Body).Returns(ms);
                return mockHttpResponseData.Object;
            });

            return mockHttpRequestData;
        }
    }
}