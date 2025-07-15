using System;
using System.Net.Http;
using DP.Base.Contracts;
using Newtonsoft.Json;

namespace Acme.Contracts
{
    public class AcmeResponseObject
    {
        public string HistoryId { get; set; }

        public AcmeJobInfo JobInfo { get; set; }

        public static CallResult<AcmeResponseObject> FromHttpResponse(
            HttpResponseMessage responseMessage,
            Func<string, AcmeResponseObject> contentHandler)
        {
            try
            {
                var content = responseMessage.Content.ReadAsStringAsync().Result;

                if (!responseMessage.IsSuccessStatusCode)
                {
                    var msg = $"fatal: there was a problem sending the request.\n\n" +
                        $"Request Url {JsonConvert.SerializeObject(responseMessage.RequestMessage?.RequestUri)}\n\n" +
                        $"Request Body {JsonConvert.SerializeObject(responseMessage.RequestMessage?.Content?.ReadAsStringAsync())}\n\n" +
                        $"Response Body {content}";

                    return new CallResult<AcmeResponseObject>
                    {
                        Success = false,
                        DisplayMessage = msg
                    };
                }

                // build up the response based on handler that is passed in
                var responseObject = contentHandler(content);

                return new CallResult<AcmeResponseObject>
                {
                    Success = responseObject != null,
                    ReturnValue = responseObject
                };
            }
            catch (Exception e)
            {
                return new CallResult<AcmeResponseObject>
                {
                    Success = false,
                    Exception = e,
                    DisplayMessage = e.Message
                };
            }
        }
    }
}
