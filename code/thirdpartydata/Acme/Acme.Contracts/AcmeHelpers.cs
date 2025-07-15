using System;
using System.Collections.Generic;

namespace Acme.Contracts
{
    public class AcmeHelpers
    {
        /// <summary>
        /// Static Map of how to convert incoming Acme local timestamps to UTC.
        /// 
        /// We do not have to worry about any other properties of the TimeZone we are getting the timestamps from. Acme has
        /// already done the DST-related conversions - which is why we only need to do a straightforward offset converstion
        ///
        /// Per Acme:
        /// We do adjust for DST by removing its effects (skipped hour in spring, duplicated hour in fall) from the time series.
        /// In other words, the final datetimes should not have any duplicated or skipped hours. Note that this formatting ​has not
        /// changed between versions of the API -- only the implied time zone has changed.
        /// Acme Ticket: https://acme.freshdesk.com/helpdesk/tickets/3945
        /// </summary>
        public static Dictionary<string, int> UtcOffsetMap = new Dictionary<string, int>() {

            // N.American Codes that will come from Acme
            { "PST", 8 },
            { "MST", 7 },
            { "CST", 6 },
            { "EST", 5},

            // European codes that will come from Acme
            { "WET", 0 },
            { "CET", -1 },
            { "EET", -2 },
            { "FET", -3 },
            { "GET", -4 },
        };
        
        /// <summary>
        /// Use to regulate the number of concurrent jobs that we will have out to Acme. 
        /// </summary>
        public const int AcmeMaxConcurrentJobs = 10; 

        public const string GenerationTypeWind = "Wind";
        public const string GenerationTypeSolar = "Solar";
        public const string ZipFileExtension = ".zip";
        public const string CsvFileExtension = ".csv";
        public const string ProjectNumberToken = "#ProjectNumber";
        public const string ProjectNameToken = "#ProjectName#";
        public const string LatToken = "#Lat#";
        public const string LngToken = "#Lng#";
        public const string NameplateToken = "#Nameplate#";
        public const string WeatherYearToken = "#WeatherYear#";
        public const string GenerationTypeToken = "#GenerationType#";
        public const string EmptyValueToken = "~";

        // Wind
        public const string TurbineVendorToken = "#TurbineVendor#";
        public const string TurbineModelToken = "#TurbineModel#";
        public const string TurbineCountToken = "#TurbineCount#";
        public const string TurbineHubHeightToken = "#TurbineHubHeight#";

        // Solar
        public const string ModuleTypeToken = "#ModuleType#";
        public const string ArrayTypeToken = "#ArrayType#";
        public const string TiltToken = "#Tilt#";
        public const string AzimuthToken = "#Azimuth#";
        public const string DcAcRatioToken = "#DcAcRatio#";
        public const string IsBifacialToken = "#IsBifacial#";
        public const string GroundCoverageRatioToken = "#GroundCoverageRatio#";
        public const string UseSnowLossModelToken = "#UseSnowLossModel#";

        public static readonly string BaseFileSpecPrefix = $"{GenerationTypeToken}_{ProjectNumberToken}_{ProjectNameToken}_{WeatherYearToken}";

        public static readonly string FileSpecCommonAttributes = $"{LatToken}_{LngToken}_{NameplateToken}";

        public static readonly string WindFileSpecPrefix = $"{BaseFileSpecPrefix}_{FileSpecCommonAttributes}_{TurbineVendorToken}_{TurbineModelToken}_{TurbineHubHeightToken}_{TurbineCountToken}";

        public static readonly string SolarFileSpecPrefix = 
            $"{BaseFileSpecPrefix}_{FileSpecCommonAttributes}_{DcAcRatioToken}_{ModuleTypeToken}_{ArrayTypeToken}_{TiltToken}_{AzimuthToken}_{GroundCoverageRatioToken}_{IsBifacialToken}_{UseSnowLossModelToken}";

        public static readonly int WeatherYearOrdinal = Array.FindIndex(BaseFileSpecPrefix.Split("_"), s => s == WeatherYearToken);

        public static readonly int ProjectIdOrdinal = Array.FindIndex(BaseFileSpecPrefix.Split("_"), s => s == ProjectNumberToken);

        public static readonly int ProjectNameOrdinal = Array.FindIndex(BaseFileSpecPrefix.Split("_"), s => s == ProjectNameToken);

        public const int AcmeApiMinBytesDownloaded = 100;

        public static readonly string FileSpecRootPath = "modeled/";
        
        public static readonly string AcquiredPath = "acquired/acme/projects/";
        public static readonly string ToBeProcessedPath = $"{AcquiredPath}ToBeProcessed/";
        public static readonly string ProcessedPath = "processed/acme/projects/";

        // 24-hour format (HH)
        public static readonly string AcmeTimeStampFormat = "yyyy-MM-dd HH:mm:ss";
    }
}