using DP.Base.Contracts;
using System.Net.Http;
using System.Threading.Tasks;
using Acme.Contracts;

namespace Acme.AcquireGeneration

{
    public interface IAcmeHttpClient
    {
        Task<CallResult<string>> LaunchGenerationForWeatherYearJob(AcmeAcquireGenerationContext ctx);
        Task<CallResult<string>> WaitForGenerationJobAsync(string historyId, int retryCount, int retryIntervalSeconds);
        Task<CallResult<HttpResponseMessage>> GetGenerationForWeatherYearResultsAsync(AcmeAcquireGenerationContext ctx);

    }
}

