using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Reflection;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace DP.Base.Extensions
{
    public class RequiresCustomSerializationAttribute : Attribute
    {
    }

    public static class JsonExtensions
    {
        public static bool TryParseJson<T>(this string jsonString, out T result)
        {
            try
            {
                // Validate missing fields of object
                JsonSerializerSettings settings = new JsonSerializerSettings();
                
                result = JsonConvert.DeserializeObject<T>(jsonString, settings);
                return true;
            }
            catch (Exception)
            {
                result = default(T);
                return false;
            }
        }

        public static T SelectTokenEx<T>(this JObject configJObj, string fieldName, T defaultVal = default(T), bool recurse = true) 
        {
            var jToken = configJObj.GetTokenEx(fieldName, recurse);

            return jToken != null ? jToken.ToObject<T>() : defaultVal;
        }

        public static JToken GetTokenEx(this JObject configJObj, string fieldName, bool recurse = true)
        {
            if (recurse)
            {
                // use JPath to recur
                fieldName = "$.." + fieldName;
            }

            var jToken = configJObj.SelectToken(fieldName);
            return jToken;
        }

        // http://www.newtonsoft.com/json/help/html/serializationsettings.htm
        public static JsonSerializerSettings GetDefaultJsonSerializerSettings(params JsonConverter[] converters) =>
            new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),

                Converters = converters,

                //ReferenceLoopHandling = ReferenceLoopHandling.Ignore,

                // Right now by default missing values on deserialization are just ignored.
                // We could force JsonNet to error if we wanted by turning this on
                //MissingMemberHandling = MissingMemberHandling.Error;
            };

        public static string ToJsonEx<T>(this T o) => o.ToJson(GetDefaultJsonSerializerSettings());
        public static string ToJson<T>(this T o, params JsonConverter[] converters) => o.ToJson(GetDefaultJsonSerializerSettings(converters));
        public static string ToJson<T>(this T o, JsonSerializerSettings settings) =>  o == null ? null : JsonConvert.SerializeObject(o, settings);

        public static T FromJson<T>(this string s) => s.FromJson<T>(GetDefaultJsonSerializerSettings());
        public static T FromJson<T>(this string s, params JsonConverter[] converters) => s.FromJson<T>(GetDefaultJsonSerializerSettings(converters));
        public static T FromJson<T>(this string s, JsonSerializerSettings settings)
        {
#if DEBUG
            if (typeof(T).GetTypeInfo().GetCustomAttribute(typeof(RequiresCustomSerializationAttribute)) != null)
            {
                throw new InvalidOperationException($"The type {typeof(T).Name} requires custom serialization");
            }
#endif
            return s == null ? default(T) : JsonConvert.DeserializeObject<T>(s, settings);
        }

        /// <summary>
        /// Converts a Json ARRAY into a csv-formatted string
        /// Note: if the input is not a valid Json array, it will throw an exception
        /// </summary>
        /// <param name="strJsonArray"></param>
        /// <returns></returns>
        public static string ToCsv(this string strJsonArray)
        {
            var expandos = JsonConvert.DeserializeObject<ExpandoObject[]>(strJsonArray);

            var csvString = new StringWriter();
            using (var csv = new CsvWriter(csvString, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords((expandos as IEnumerable<dynamic>));
            }

            // return minus the trailing newline
            return csvString.ToString().TrimEnd('\r', '\n'); 
        }
    }
}
