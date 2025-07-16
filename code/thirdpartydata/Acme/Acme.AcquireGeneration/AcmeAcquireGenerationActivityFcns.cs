using DP.Base.Contracts;
using Ingress.Lib.Base.Contracts;
using Microsoft.Extensions.Logging;
using Acme.Contracts;
using DurableFunctionsCommon;
using DP.Base.Extensions;
using Microsoft.Azure.Functions.Worker;

namespace Acme.AcquireGeneration
{
    /// <summary>
    /// Class which encapsulates the Durable Activity Functions used to fetch the Acme Generation Data
    /// </summary>
    public class AcmeAcquireGenerationActivityFcns : ActivityFunctionBase<AcmeAcquireGenerationActivityFcns>
    {
        private IAcmeHttpClient AcmeHttpClient { get; }

        public AcmeAcquireGenerationActivityFcns(IAcmeHttpClient acmeHttpClient, IBlobContainerWrapper blobContainerWrapper = null, ILogger<AcmeAcquireGenerationActivityFcns> log = null)
            : base(blobContainerWrapper, log)
        {
            this.AcmeHttpClient = acmeHttpClient ?? throw new ArgumentNullException(nameof(acmeHttpClient));
        }

        [Function(nameof(LaunchGenerationForWeatherYearJobAsync))]
        public async Task<CallResult<string>> LaunchGenerationForWeatherYearJobAsync(
            [ActivityTrigger] AcmeAcquireGenerationContext ctx)
        {
            try
            {
                Log.LogInformation($"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] {nameof(LaunchGenerationForWeatherYearJobAsync)}");

                var jobResponse = await this.AcmeHttpClient.LaunchGenerationForWeatherYearJob(ctx);

                if (!jobResponse.Success)
                {
                    return CallResult.CreateFailedResult<string>(jobResponse.DisplayMessage, jobResponse.Exception);
                }

                // Get the ID of Task/Job that we launched off at Acme.  We will need to check its status later to see when it's done
                return CallResult.CreateSuccessResult<string>(jobResponse.ReturnValue);
            }
            catch (Exception ex)
            {
                return CallResult.CreateFailedResult<string>($"Exception in {nameof(LaunchGenerationForWeatherYearJobAsync)}", ex);
            }
        }

        [Function(nameof(WaitForGenerationJobAsync))]
        public async Task<CallResult<string>> WaitForGenerationJobAsync(
            [ActivityTrigger] string historyId)
        {
            try
            {
                Log.LogInformation($"START: {nameof(WaitForGenerationJobAsync)}");

                // Wait for the URI of the completed csv file for the requested jobId.
                // Try 90 times with 10 second waits in between tries (15 minutes)
                // Why 90x of 10s: This should probably be configurable (TODO) but after working with Acme's API Developers, it was agreed that 
                // 15 minutes is a proper amount of time to wait for the job to complete. If it goes beyond that, it's likely that something 
                // went wrong at Acme and at that point we should fail, return the error and let operations know that they need to check with Acme.
                var waitResult = await this.AcmeHttpClient.WaitForGenerationJobAsync(historyId, retryCount: 90, retryIntervalSeconds: 10);

                if (!waitResult.Success)
                {
                    return CallResult.CreateFailedResult<string>(waitResult.ReturnValue, waitResult.Exception);
                }

                // returns the URI of the completed csv file 
                return CallResult.CreateSuccessResult<string>(waitResult.ReturnValue);
            }
            catch (Exception ex)
            {
                return CallResult.CreateFailedResult<string>($"Exception in {nameof(WaitForGenerationJobAsync)}", ex);
            }
            finally
            {
                Log.LogInformation($"END: {nameof(WaitForGenerationJobAsync)}");
            }
        }

        [Function(nameof(GetGenerationDataAndSaveToBlobAsync))]
        public async Task<CallResult<string>> GetGenerationDataAndSaveToBlobAsync(
            [ActivityTrigger] AcmeAcquireGenerationContext ctx)
        {
            try
            {
                var getResult = await this.AcmeHttpClient.GetGenerationForWeatherYearResultsAsync(ctx);

                if (!getResult.Success)
                {
                    return CallResult.CreateFailedResult<string>($"{getResult.DisplayMessage}");
                }

                var destinationFileName = GetDestinationFileName(ctx);

                var blobContainerWrapper = this.GetBlobContainerWrapper(ctx.BlobDestinationConfigInfo);

                await ValidateAndSave(getResult.ReturnValue, destinationFileName, blobContainerWrapper);

                // return full path 
                return CallResult.CreateSuccessResult<string>($"{blobContainerWrapper.GetAbsoluteUri()}/{destinationFileName}");
            }
            catch (Exception ex)
            {
                return CallResult.CreateFailedResult<string>($"Exception in {nameof(GetGenerationDataAndSaveToBlobAsync)}", ex);
            }
        }

        internal async Task ValidateAndSave(HttpResponseMessage httpResponse,
                                              string fileName,
                                              IBlobContainerWrapper destinationblobContainer)
        {
            // The check on the content length exists because sometimes the server will return a successful status code, but with 0 bytes. That's no good.
            // If even after the retries we still don't have a successful download, throw.
            if (!httpResponse.IsSuccessStatusCode || httpResponse.Content.Headers.ContentLength < AcmeAcquireGenerationHelpers.AcmeApiMinBytesDownloaded)
            {
                throw new ApplicationException($"download was not successful. status code: {httpResponse.StatusCode}.\n" +
                    $"content length: {httpResponse.Content.Headers.ContentLength} bytes.\n" +
                    $"content {await httpResponse.Content.ReadAsStringAsync()}");
            }
            else
            {
                // Given a successful response, we can extract out the stream
                var httpResponseStream = await httpResponse.Content.ReadAsStreamAsync();

                var buffer = new byte[httpResponseStream.Length];

                await httpResponseStream.ReadAsync(buffer, 0, buffer.Length);

                await destinationblobContainer.UploadResult(fileName, buffer);

                httpResponseStream.Dispose();
            }
        }

        /// <summary>
        /// File names for the project generation adhere to the following naming convention:
        /// FileSpec: {ProjectId}_{ProjectName}_{WeatherYear}_{Lat}_{Lng}_{Nameplate}_{TurbineVendor}_{TurbineModel}_{TurbineHubHeight}_{TurbineCount}_[VERSION]";
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        internal string GetDestinationFileName(AcmeAcquireGenerationContext ctx)
        {
            // File is prefixed by the orchestrationId
            var fileNamePrefix = AcmeAcquireGenerationHelpers.GetFileNamePrefix(ctx.Project);

            // Return the ToBeProcessedPath because that is where we need to put the file.  Add a timestamp to the end of the file name to make it unique.
            var toBeProcessedPath = $"{AcmeHelpers.FileSpecRootPath}{AcmeAcquireGenerationHelpers.ToBeProcessedPath}{fileNamePrefix}";
            return $"{toBeProcessedPath}_{DateTime.UtcNow.ToCompressedSortableDateTimePattern()}{AcmeAcquireGenerationHelpers.ZipFileExtension}";
        }
    }
}