using System;
using System.Net.Http;

namespace DP.Base.Http
{
    public interface IDPHttpClientWrapper
    {
        string HostName { get; set; }
        int PortNumber { get; set; }
        string UriScheme { get; set; }

        Uri CreateUri(string hostname, int port, string path = "", string queryString = "");
        HttpResponseMessage Delete(string apiEndpoint, string queryString = "");
        HttpResponseMessage DeleteUntil(Predicate<HttpResponseMessage> successCondition, string apiEndpoint, string queryString = "", double retryIntervalInMilliseconds = 1000, int retryCount = 10);
        HttpResponseMessage DeleteWithRetry(string apiEndpoint, string queryString = "", double retryIntervalInMilliseconds = 1000, int retryCount = 10);
        HttpResponseMessage Get(string apiEndpoint, string queryString = "", object body = null);
        HttpResponseMessage GetUntil(Predicate<HttpResponseMessage> successCondition, string apiEndpoint, string queryString = "", object body = null, double retryIntervalInMilliseconds = 1000, int retryCount = 10);
        HttpResponseMessage GetWithRetry(string apiEndpoint, string queryString = "", object body = null, double retryIntervalInMilliseconds = 1000, int retryCount = 10);
        HttpResponseMessage Post(string apiEndpoint, string queryString = "", object body = null);
        HttpResponseMessage PostUntil(Predicate<HttpResponseMessage> successCondition, string apiEndpoint, string queryString = "", object body = null, double retryIntervalInMilliseconds = 1000, int retryCount = 10);
        HttpResponseMessage PostWithRetry(string apiEndpoint, string queryString = "", object body = null, double retryIntervalInMilliseconds = 1000, int retryCount = 10);
        HttpResponseMessage Put(string apiEndpoint, string queryString = "", object body = null);
        HttpResponseMessage PutUntil(Predicate<HttpResponseMessage> successCondition, string apiEndpoint, string queryString = "", object body = null, double retryIntervalInMilliseconds = 1000, int retryCount = 10);
        HttpResponseMessage PutWithRetry(string apiEndpoint, string queryString = "", object body = null, double retryIntervalInMilliseconds = 1000, int retryCount = 10);
    }
}