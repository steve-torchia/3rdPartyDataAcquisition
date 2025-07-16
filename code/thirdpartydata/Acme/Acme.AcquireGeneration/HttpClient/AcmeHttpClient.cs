using DP.Base.Contracts;
using Ingress.Lib.Base;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Acme.Contracts;
using System.Text;
using DP.Base.Extensions;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace Acme.AcquireGeneration
{
    public class AcmeHttpClient : HttpClientBase<IAcmeHttpClient>, IAcmeHttpClient
    {
        public string SolarRequestUrl { get; protected set; }
        public string WindRequestUrl { get; protected set; }
        public string GenerationJobStatusUrl { get; protected set; }

        public AcmeHttpClient(HttpClient client, ILogger<AcmeHttpClient> log, IOptions<AcmeSubscriptionInfo> acmeSubscriptionInfo)
            : base(client, acmeSubscriptionInfo.Value.ApiInfo.BaseUrl, log)
        {
            SolarRequestUrl = $"{BaseUrl}{acmeSubscriptionInfo.Value.ApiInfo.SolarPath}";
            WindRequestUrl = $"{BaseUrl}{acmeSubscriptionInfo.Value.ApiInfo.WindPath}";
            GenerationJobStatusUrl = $"{BaseUrl}{acmeSubscriptionInfo.Value.ApiInfo.StatusPath}";
        }

        public async Task<CallResult<string>> LaunchGenerationForWeatherYearJob(AcmeAcquireGenerationContext ctx)
        {
            var requestUri = GetGenerationRequestUri(ctx);

            var reqBody = GetHttpRequestContent(ctx);

            this.Logger.LogInformation($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] RequestUrl: {requestUri}, RequestBody: {reqBody.ReadAsStringAsync().Result}");
            
            var httpResponse = await this.Client.PostAsync(requestUri, reqBody);

            var response = AcmeResponseObject.FromHttpResponse(httpResponse, LaunchGenerationResponseHandler);

            if (response.Success)
            {
                // return the HistoryId of the Acme job that was launched from this request.
                return new CallResult<string>
                {
                    ReturnValue = response.ReturnValue.HistoryId,
                    Success = true,
                };
            }
            else
            {
                return new CallResult<string>
                {
                    DisplayMessage = response.DisplayMessage,
                    Exception = response.Exception,
                    Success = false,
                };
            }
        }

        public async Task<CallResult<string>> WaitForGenerationJobAsync(string historyId, int retryCount = 90, int retryIntervalSeconds = 10)
        {
            // Default wait is 90x10seconds = 15 minutes

            try
            {
                this.Logger.LogInformation($"{nameof(WaitForGenerationJobAsync)} START (HistoryId={historyId}, retryCount={retryCount}, retryInterval={retryIntervalSeconds})");
                var retryInterval = TimeSpan.FromSeconds(retryIntervalSeconds);
                var diagnosticMsgs = new List<string>();

                var url = $"{GenerationJobStatusUrl}/{historyId}";

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                for (var attempt = 1; attempt <= retryCount; attempt++)
                {
                    var httpResponse = await this.Client.GetAsync(url);
                    var content = await httpResponse.Content.ReadAsStringAsync();
                   
                    Logger.LogInformation($"RESPONSE: {httpResponse}{Environment.NewLine}CONTENT:{content}");

                    var result = AcmeResponseObject.FromHttpResponse(httpResponse, GetJobStatus);

                    if (result.Success)
                    {
                        return new CallResult<string>
                        {
                            ReturnValue = result.ReturnValue.JobInfo.ResultsUrl,
                            Success = true,
                        };
                    }

                    // If we got an exception back from the contentHandler, then something most likely went wrong enough at the 
                    // Acme end that it is not worth any more retries.
                    if (result.Exception != null)
                    {
                        return new CallResult<string> 
                        { 
                            Success = false,
                            ReturnValue = result.Exception.Message 
                        };
                    }

                    // Else the job is most likely still in a pending/running state....
                    var msg = $"Job Status (HistoryId={historyId}) not complete yet (attempt={attempt}) : {result.DisplayMessage}, ex={result.Exception?.Message}. Trying again (interval={retryIntervalSeconds}sec)...";
                    this.Logger.LogError(msg, result.Exception);
                    diagnosticMsgs.Add(msg);

                    await Task.Delay(retryInterval);
                }

                var failMsg = $"TIMEOUT waiting to get Job Status for HistoryId={historyId}, elapsedTime={stopwatch.Elapsed.TotalSeconds} seconds";
                this.Logger.LogError(failMsg);

                return new CallResult<string>
                {
                    Success = false,
                    ReturnValue = failMsg
                };
            }
            finally
            {
                this.Logger.LogInformation($"{nameof(WaitForGenerationJobAsync)} END");
            }
        }
        
        public async Task<CallResult<HttpResponseMessage>> GetGenerationForWeatherYearResultsAsync(AcmeAcquireGenerationContext ctx)
        {
            try
            {
                // This call to Acme has proved to be flaky during our testing, so we will use the retry mechanism to add some resilience.
                var httpResponse = await this.GetWithRetryAsync(
                    ctx.JobInfo.ResultsUrl,
                    (h) => h.IsSuccessStatusCode && h.Content.Headers.ContentLength > AcmeAcquireGenerationHelpers.AcmeApiMinBytesDownloaded);

                if (httpResponse.IsSuccessStatusCode)
                {
                    return CallResult.CreateSuccessResult<HttpResponseMessage>(httpResponse);
                }

                return CallResult.CreateFailedResult<HttpResponseMessage>($"Error trying to download and/or save Generation results: {httpResponse.StatusCode}, {httpResponse.ReasonPhrase}, {httpResponse?.RequestMessage?.RequestUri} ");
            }
            catch (Exception ex)
            {
                return CallResult.CreateFailedResult<HttpResponseMessage>("Error trying to download and/or save Generation results", ex);
            }
        }

        private HttpContent GetHttpRequestContent(AcmeAcquireGenerationContext ctx)
        {
            if (ctx.Project.Product.EqualsIgnoreCase("wind"))
            {
                return new StringContent(JsonConvert.SerializeObject(ctx.Project.ToAcmeWindGenerationRequest()), Encoding.UTF8, "application/json");
            }
            if (ctx.Project.Product.EqualsIgnoreCase("solar"))
            {
                return new StringContent(JsonConvert.SerializeObject(ctx.Project.ToAcmeSolarGenerationRequest()), Encoding.UTF8, "application/json");
            }

            throw new Exception($"Incorrect Generation Type: {ctx.Project.Product}");
        }

        private Uri GetGenerationRequestUri(AcmeAcquireGenerationContext ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }

            if (ctx.Project.Product.EqualsIgnoreCase("wind"))
            {
                return new Uri($"{WindRequestUrl}");
            }
            if (ctx.Project.Product.EqualsIgnoreCase("solar"))
            {
                return new Uri($"{SolarRequestUrl}");
            }

            throw new Exception($"Bogus generation type specified: {ctx.Project.Product}");
        }

        private static AcmeResponseObject LaunchGenerationResponseHandler(string jsonContent)
        {
            // Content should just be an integer
            if (!int.TryParse(jsonContent, out var historyId))
            {
                throw new Exception($"Unexpected value for HistoryId: {jsonContent}");
            }

            return new AcmeResponseObject() { HistoryId = jsonContent };
        }

        private static AcmeResponseObject GetJobStatus(string jsonContent)
        {
             // From Acme:
             //  ​returns "Pending" once job request has been successfully created
             //  returns either "Error" or "Completed" once job has been processed

            var jobInfo = JsonConvert.DeserializeObject<AcmeJobInfo>(jsonContent);

            if (jobInfo.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                return new AcmeResponseObject() { JobInfo = jobInfo };
            }

            if (jobInfo.Status.Equals("Error", StringComparison.OrdinalIgnoreCase))
            {
                throw new ApplicationException($"Job failed while fetching generation: {jobInfo.Message}");
            }

            return null;
        }
    }
}