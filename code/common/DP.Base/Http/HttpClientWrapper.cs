using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DP.Base.Contracts;
using Newtonsoft.Json;

namespace DP.Base.Http
{
    public class HttpClientWrapper : IDPHttpClientWrapper
    {
        /// <summary>
        /// Microsoft Documentation recommends only using one HttpClient instance over the life of the application
        /// The inner client is exposed in cases where the operations exposed  by this class are not enough.
        /// </summary>
        public static HttpClient InnerClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(300)
        };

        public string HostName { get; set; }

        public int PortNumber { get; set; }

        public string UriScheme { get; set; }

        private const double DefaultWait = 1000;

        private const int DefaultRetryCount = 10;

        public HttpClientWrapper()
        {
            this.HostName = "localhost";

            this.PortNumber = 80;

            this.UriScheme = Uri.UriSchemeHttp;
        }

        public HttpClientWrapper(string host, int port, string uriScheme)
        {
            this.HostName = host;

            this.PortNumber = port;

            this.UriScheme = uriScheme;

            if (uriScheme == Uri.UriSchemeHttps)
            {
                // https://stackoverflow.com/questions/22251689/make-https-call-using-httpclient
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            }
        }

        public Uri CreateUri(string hostname, int port, string path = "", string queryString = "")
        {
            var uriString = $"{this.UriScheme}://{hostname}:{port}{path}";

            if (!string.IsNullOrWhiteSpace(queryString))
            {
                uriString += $"?{queryString}";
            }

            return new Uri(uriString);
        }

        /// <summary>
        /// Simple Get Request.
        /// </summary>
        /// <param name="apiEndpoint">The endpoint you want to hit. e.g. /api/hello</param>
        /// <param name="queryString">The query string, if any. Do not include the '?' at the begining, but do include the ampersand in between each key-value pair.</param>
        /// <param name="body">The body of the request,if any.</param>
        /// <returns>The HttpResponseMessage result</returns>
        public HttpResponseMessage Get(string apiEndpoint, string queryString = "", object body = null)
        {
            var json = JsonConvert.SerializeObject(body);

            var uri = this.CreateUri(this.HostName, this.PortNumber, apiEndpoint, queryString);

            var responseMessage = InnerClient.GetAsync(uri).Result;

            return responseMessage;
        }

        /// <summary>
        /// A Get Request with retry capabilities. Will retry until the response is successful or the retry count is hit, whichever comes first.
        /// </summary>
        /// <param name="apiEndpoint">The endpoint you want to hit. e.g. /api/hello</param>
        /// <param name="queryString">The query string, if any. Do not include the '?' at the begining, but do include the ampersand in between each key-value pair.</param>
        /// <param name="body">The body of the request, if any.</param>
        /// <param name="retryIntervalInMilliseconds">Number of milliseconds to wait before retrying.</param>
        /// <param name="retryCount">The retry Count</param>
        /// <returns>The HttpResponseMessage result</returns>
        public HttpResponseMessage GetWithRetry(string apiEndpoint, string queryString = "", object body = null, double retryIntervalInMilliseconds = DefaultWait, int retryCount = DefaultRetryCount)
        {
            return RetryHttpCall.Do(this.Get, apiEndpoint, queryString, body, TimeSpan.FromMilliseconds(retryIntervalInMilliseconds), retryCount, x => x.IsSuccessStatusCode);
        }

        /// <summary>
        /// A Get Request with retry capabilities. Will retry until the predicate is true or the retry count is hit, whichever comes first.
        /// </summary>
        /// <param name="successCondition">The endpoint you want to hit. e.g. /api/hello</param>
        /// <param name="apiEndpoint">The query string, if any. Do not include the '?' at the begining, but do include the ampersand in between each key-value pair.</param>
        /// <param name="queryString">The body of the request,if any.</param>
        /// <param name="body">The body of the request, if any.</param>
        /// <param name="retryIntervalInMilliseconds">Number of milliseconds to wait before retrying.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <returns>The HttpResponseMessage result</returns>
        public HttpResponseMessage GetUntil(Predicate<HttpResponseMessage> successCondition, string apiEndpoint, string queryString = "", object body = null, double retryIntervalInMilliseconds = DefaultWait, int retryCount = DefaultRetryCount)
        {
            return RetryHttpCall.Do(this.Get, apiEndpoint, queryString, body, TimeSpan.FromMilliseconds(retryIntervalInMilliseconds), retryCount, successCondition);
        }

        /// <summary>
        /// Simple Post Request.
        /// </summary>
        /// <param name="apiEndpoint">The endpoint you want to hit. e.g. /api/hello</param>
        /// <param name="queryString">The query string, if any. Do not include the '?' at the begining, but do include the ampersand in between each key-value pair.</param>
        /// <param name="body">The body of the request,if any.</param>
        /// <returns>The HttpResponseMessage result</returns>
        public HttpResponseMessage Post(string apiEndpoint, string queryString = "", object body = null)
        {
            HttpContent content = null;
            if (body is HttpContent)
            {
                content = (HttpContent)body;
            }
            else
            {
                var json = JsonConvert.SerializeObject(body);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var uri = this.CreateUri(this.HostName, this.PortNumber, apiEndpoint, queryString);
            var responseMessage = InnerClient.PostAsync(uri, content).Result;

            return responseMessage;
        }

        /// <summary>
        /// A Post Request with retry capabilities. Will retry until the response is successful or the retry count is hit, whichever comes first.
        /// </summary>
        /// <param name="apiEndpoint">The endpoint you want to hit. e.g. /api/hello</param>
        /// <param name="queryString">The query string, if any. Do not include the '?' at the begining, but do include the ampersand in between each key-value pair.</param>
        /// <param name="body">The body of the request, if any.</param>
        /// <param name="retryIntervalInMilliseconds">Number of milliseconds to wait before retrying.</param>
        /// <param name="retryCount">The retry Count</param>
        /// <returns>The HttpResponseMessage result</returns>
        public HttpResponseMessage PostWithRetry(string apiEndpoint, string queryString = "", object body = null, double retryIntervalInMilliseconds = DefaultWait, int retryCount = DefaultRetryCount)
        {
            return RetryHttpCall.Do(this.Post, apiEndpoint, queryString, body, TimeSpan.FromMilliseconds(retryIntervalInMilliseconds), retryCount, x => x.IsSuccessStatusCode);
        }

        /// <summary>
        /// A Post Request with retry capabilities. Will retry until the predicate is true or the retry count is hit, whichever comes first.
        /// </summary>
        /// <param name="successCondition">The endpoint you want to hit. e.g. /api/hello</param>
        /// <param name="apiEndpoint">The query string, if any. Do not include the '?' at the begining, but do include the ampersand in between each key-value pair.</param>
        /// <param name="queryString">The body of the request,if any.</param>
        /// <param name="body">The body of the request, if any.</param>
        /// <param name="retryIntervalInMilliseconds">Number of milliseconds to wait before retrying.param>
        /// <param name="retryCount">The retry count.</param>
        /// <returns>The HttpResponseMessage result</returns>
        public HttpResponseMessage PostUntil(Predicate<HttpResponseMessage> successCondition, string apiEndpoint, string queryString = "", object body = null, double retryIntervalInMilliseconds = DefaultWait, int retryCount = DefaultRetryCount)
        {
            return RetryHttpCall.Do(this.Post, apiEndpoint, queryString, body, TimeSpan.FromMilliseconds(retryIntervalInMilliseconds), retryCount, successCondition);
        }

        /// <summary>
        /// Simple Put Request.
        /// </summary>
        /// <param name="apiEndpoint">The endpoint you want to hit. e.g. /api/hello</param>
        /// <param name="queryString">The query string, if any. Do not include the '?' at the begining, but do include the ampersand in between each key-value pair.</param>
        /// <param name="body">The body of the request,if any.</param>
        /// <returns>The HttpResponseMessage result</returns>
        public HttpResponseMessage Put(string apiEndpoint, string queryString = "", object body = null)
        {
            var json = JsonConvert.SerializeObject(body);

            var uri = this.CreateUri(this.HostName, this.PortNumber, apiEndpoint, queryString);

            var responseMessage = InnerClient.PutAsync(uri, new StringContent(json, Encoding.UTF8, "application/json")).Result;

            return responseMessage;
        }

        /// <summary>
        /// A Put Request with retry capabilities. Will retry until the response is successful or the retry count is hit, whichever comes first.
        /// </summary>
        /// <param name="apiEndpoint">The endpoint you want to hit. e.g. /api/hello</param>
        /// <param name="queryString">The query string, if any. Do not include the '?' at the begining, but do include the ampersand in between each key-value pair.</param>
        /// <param name="body">The body of the request, if any.</param>
        /// <param name="retryIntervalInMilliseconds">Number of milliseconds to wait before retrying.</param>
        /// <param name="retryCount">The retry Count</param>
        /// <returns>The HttpResponseMessage result</returns>
        public HttpResponseMessage PutWithRetry(string apiEndpoint, string queryString = "", object body = null, double retryIntervalInMilliseconds = DefaultWait, int retryCount = DefaultRetryCount)
        {
            return RetryHttpCall.Do(this.Put, apiEndpoint, queryString, body, TimeSpan.FromMilliseconds(retryIntervalInMilliseconds), retryCount, x => x.IsSuccessStatusCode);
        }

        /// <summary>
        /// A Put Request with retry capabilities. Will retry until the predicate is true or the retry count is hit, whichever comes first.
        /// </summary>
        /// <param name="successCondition">The endpoint you want to hit. e.g. /api/hello</param>
        /// <param name="apiEndpoint">The query string, if any. Do not include the '?' at the begining, but do include the ampersand in between each key-value pair.</param>
        /// <param name="queryString">The body of the request,if any.</param>
        /// <param name="body">The body of the request, if any.</param>
        /// <param name="retryIntervalInMilliseconds">Number of milliseconds to wait before retrying.param>
        /// <param name="retryCount">The retry count.</param>
        /// <returns>The HttpResponseMessage result</returns>
        public HttpResponseMessage PutUntil(Predicate<HttpResponseMessage> successCondition, string apiEndpoint, string queryString = "", object body = null, double retryIntervalInMilliseconds = DefaultWait, int retryCount = DefaultRetryCount)
        {
            return RetryHttpCall.Do(this.Put, apiEndpoint, queryString, body, TimeSpan.FromMilliseconds(retryIntervalInMilliseconds), retryCount, successCondition);
        }

        /// <summary>
        /// Simple Delete Request.
        /// </summary>
        /// <param name="apiEndpoint">The endpoint you want to hit. e.g. /api/hello</param>
        /// <param name="queryString">The query string, if any. Do not include the '?' at the begining, but do include the ampersand in between each key-value pair.</param>
        /// <returns>The HttpResponseMessage result</returns>
        public HttpResponseMessage Delete(string apiEndpoint, string queryString = "")
        {
            var uri = this.CreateUri(this.HostName, this.PortNumber, apiEndpoint, queryString);

            var responseMessage = InnerClient.DeleteAsync(uri).Result;

            return responseMessage;
        }

        /// <summary>
        /// A Delete Request with retry capabilities. Will retry until the response is successful or the retry count is hit, whichever comes first.
        /// </summary>
        /// <param name="apiEndpoint">The endpoint you want to hit. e.g. /api/hello</param>
        /// <param name="queryString">The query string, if any. Do not include the '?' at the begining, but do include the ampersand in between each key-value pair.</param>
        /// <param name="retryIntervalInMilliseconds">Number of milliseconds to wait before retrying.</param>
        /// <param name="retryCount">The retry Count</param>
        /// <returns>The HttpResponseMessage result</returns>
        public HttpResponseMessage DeleteWithRetry(string apiEndpoint, string queryString = "", double retryIntervalInMilliseconds = DefaultWait, int retryCount = DefaultRetryCount)
        {
            return RetryHttpCall.Do(this.Delete, apiEndpoint, queryString, TimeSpan.FromMilliseconds(retryIntervalInMilliseconds), retryCount, x => x.IsSuccessStatusCode);
        }

        /// <summary>
        /// A Delete Request with retry capabilities. Will retry until the predicate is true or the retry count is hit, whichever comes first.
        /// </summary>
        /// <param name="successCondition">The endpoint you want to hit. e.g. /api/hello</param>
        /// <param name="apiEndpoint">The query string, if any. Do not include the '?' at the begining, but do include the ampersand in between each key-value pair.</param>
        /// <param name="queryString">The body of the request,if any.</param>
        /// <param name="retryIntervalInMilliseconds">Number of milliseconds to wait before retrying.param>
        /// <param name="retryCount">The retry count.</param>
        /// <returns>The HttpResponseMessage result</returns>
        public HttpResponseMessage DeleteUntil(Predicate<HttpResponseMessage> successCondition, string apiEndpoint, string queryString = "", double retryIntervalInMilliseconds = DefaultWait, int retryCount = DefaultRetryCount)
        {
            return RetryHttpCall.Do(this.Delete, apiEndpoint, queryString, TimeSpan.FromMilliseconds(retryIntervalInMilliseconds), retryCount, successCondition);
        }
    }

    public static class RetryHttpCall
    {
        public delegate HttpResponseMessage HttpCall();

        private static HttpResponseMessage DoInternal(HttpCall method, Predicate<HttpResponseMessage> successCondition, TimeSpan retryInterval, int retryCount)
        {
            HttpResponseMessage response = null;

            for (int attempted = 0; attempted <= retryCount; attempted++)
            {
                if (attempted > 0)
                {
                    Thread.Sleep(retryInterval);
                }

                response = method();

                if (successCondition(response))
                {
                    return response;
                }
            }

            return response;
        }

        // Method with Two arguments
        public static HttpResponseMessage Do<TInput1, TInput2>(
            Func<TInput1, TInput2, HttpResponseMessage> method,
            TInput1 arg1,
            TInput2 arg2,
            TimeSpan retryInterval,
            int retryCount,
            Predicate<HttpResponseMessage> successCondition)
        {
            return RetryHttpCall.DoInternal(() => { return method(arg1, arg2); }, successCondition, retryInterval, retryCount);
        }

        // Method with Three arguments
        public static HttpResponseMessage Do<TInput1, TInput2, TInput3>(
            Func<TInput1, TInput2, TInput3, HttpResponseMessage> method,
            TInput1 arg1,
            TInput2 arg2,
            TInput3 arg3,
            TimeSpan retryInterval,
            int retryCount,
            Predicate<HttpResponseMessage> successCondition)
        {
            return RetryHttpCall.DoInternal(() => { return method(arg1, arg2, arg3); }, successCondition, retryInterval, retryCount);
        }
    }
}
