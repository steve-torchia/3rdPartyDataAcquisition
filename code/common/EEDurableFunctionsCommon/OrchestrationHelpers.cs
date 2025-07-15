using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;

namespace DurableFunctionsCommon
{
    public class OrchestrationHelpers
    {
        public static object GetManagementUris(DurableTaskClient orchestrationClient, string orchestrationId, string functionKey = null, HttpRequestData req = null)
        {
            var tmp = orchestrationClient.CreateHttpManagementPayload(orchestrationId);

            // The urls come back with a "+" for encoding a space. This changes it to "%20" which is easier  
            // for clients like Postman to digest
            var statusUri = tmp.StatusQueryGetUri.Replace("+", "%20");
            var sendEventUri = tmp.SendEventPostUri.Replace("+", "%20");
            var terminateUri = tmp.TerminatePostUri.Replace("+", "%20");
            var purgeHistoryUri = tmp.PurgeHistoryDeleteUri.Replace("+", "%20");

            return new
            {
                statusUri,
                sendEventUri,
                terminateUri,
                purgeHistoryUri,
            };

        }
    }
}