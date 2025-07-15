using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Acme.Contracts;

namespace Acme.AcquireGeneration
{

    /// <summary>
    /// Helpful Mock that redirects requests to a mock Acme API out on mockable.io.  Usefull when you want to make
    /// actual outbound/internet calls but do not want to hit the actual Acme Endpoint
    /// see:  https://www.mockable.io/a/?????????
    /// </summary>
    public class AcmeHttpClientInternetMock : AcmeHttpClient
    {
        public AcmeHttpClientInternetMock(HttpClient client, ILogger<AcmeHttpClientInternetMock> log, IOptions<AcmeSubscriptionInfo> acmeSubInfo)
            : base(client, log, acmeSubInfo)
        { 
        }
    }
}