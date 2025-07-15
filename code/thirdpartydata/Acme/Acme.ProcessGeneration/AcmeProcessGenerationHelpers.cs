using Ingress.Lib.Base;
using Acme.Contracts;
using DP.Base.Extensions;
using System;
using System.Linq;

namespace Acme.ProcessGeneration
{
    public class AcmeProcessGenerationHelpers : AcmeHelpers
    {
        public const string AcmeGenFileStartsWith = "Templates_Gens_";
        public const string AcmeMasterTemplateFileStartsWith = "MasterTemplate_Gens_";

        internal static string GetOrchestrationId()
        {
            try
            {
                var date = DateTime.Now.Date;
                return $"AcmeProcessGenerationOrchestrationId_{date.Year}";
            }
            catch
            {
                throw;
            }           
        }

        internal static AcmeProcessGenerationContext GetAcmeProcessGenerationContext(string requestJson, BlobConfigInfo blobConfigInfo, string zipFile = null)
        {
            var ctx = new AcmeProcessGenerationContext();

            ctx.ZipFile = zipFile;
            ctx.ProcessRequestJson = requestJson;
            ctx.BlobConfigInfo = blobConfigInfo;

            return ctx;
        }

        internal static string GetWeatherYearFromFileName(string filename)
        {
            return filename.Split("_")[WeatherYearOrdinal];
        }

        internal static string GetProjectIdFromFileName(string filename)
        {
            return filename.Split("_")[ProjectIdOrdinal];
        }
        
        internal static string GetProjectNameFromFileName(string filename)
        {
            return filename.Split("_")[ProjectNameOrdinal];
        }
    }
}