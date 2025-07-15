using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DP.Base.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static string GetContentAsString(this HttpResponseMessage response)
        {
            return response.Content.ReadAsStringAsync().Result;
        }

        public static T ParseContentAs<T>(this HttpResponseMessage response)
        {
            try
            {
                var content = response.GetContentAsString();

                var theObject = JsonConvert.DeserializeObject<T>(content);

                return theObject;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static async Task<string> SerializeToStringAsync(this HttpResponseMessage response)
        {
            var strResponse = await response.Content.ReadAsStringAsync();

            // Deserialize content
            var deserializedContent = strResponse.IsValidJson()
                ? JsonConvert.DeserializeObject(strResponse)
                : strResponse;

            // Decode headers
            Dictionary<string, IEnumerable<string>> headers = response.Headers
                .ToDictionary(
                    h => h.Key,
                    h => h.Value.Select(v => WebUtility.HtmlDecode(v))
                );

            // Decode content headers
            Dictionary<string, IEnumerable<string>> contentHeaders = response.Content.Headers
                .ToDictionary(
                    h => h.Key,
                    h => h.Value.Select(v => WebUtility.HtmlDecode(v))
                );

            var responseObject = new
            {
                StatusCode = response.StatusCode,
                ReasonPhrase = response.ReasonPhrase,
                Headers = headers,
                ContentHeaders = contentHeaders,
                Content = deserializedContent,
            };

            // Serialize with specific options to handle encoding
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            return System.Text.Json.JsonSerializer.Serialize(responseObject, options);
        }
    }
}
