using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ingress.Lib.Base
{
    public class HttpClientBase<T>
    {
        protected HttpClient Client { get; set; }
        protected ILogger<T> Logger { get; set; }
        protected string BaseUrl { get; set; }

        public HttpClientBase(HttpClient client, string baseUrl, ILogger<T> logger)
        {
            Client = client;
            BaseUrl = baseUrl;
            Logger = logger;
        }

        /// <summary>
        /// An extension of the HttpClient Get method to retry until `successCondition` is true, or `retryCount` has been reached, whichever comes first.
        /// </summary>
        public virtual async Task<HttpResponseMessage> GetWithRetryAsync(string uri,
                                                                        Predicate<HttpResponseMessage> sucessCondition,
                                                                        int retryIntervalInSeconds = 2,
                                                                        int retryCount = 3)
        {
            var retryInterval = TimeSpan.FromSeconds(retryIntervalInSeconds);

            HttpResponseMessage response = null;

            for (int attempted = 0; attempted <= retryCount; attempted++)
            {
                if (attempted > 0)
                {
                    this.Logger.LogWarning(
                        $"http request attempt {attempted - 1} failed.\n" +
                        $"status code: {(int)response.StatusCode}{response.StatusCode}. content: {await response.Content.ReadAsStringAsync()}\n" +
                        $"waiting for {retryIntervalInSeconds} seconds, then trying again.");

                    await Task.Delay(retryInterval);
                }

                response = await Client.GetAsync(uri);

                if (sucessCondition(response))
                {
                    return response;
                }
            }

            return response;
        }
    }
}