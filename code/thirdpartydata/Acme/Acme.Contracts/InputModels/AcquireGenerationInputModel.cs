using DP.Base.Contracts;
using DP.Base.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Acme.Contracts
{
    /// <summary>
    /// Model for acquring Generation from Acme.  
    /// </summary>
    public class AcquireGenerationInputModel
    {
        #region Maps for Insight's internal values to Acme representation
        /// <summary>
        /// Map of Insights PanelType values to Acme ModuleType values
        /// </summary>
        public static Dictionary<string, string> InsightsPanelTypeTypeToAcmeModuleTypeMap = new Dictionary<string, string>()
        {
            { "Standard", "standard" },
            { "Premium", "standard" },
            { "Thin Film", "thin film" }
        };

        /// <summary>
        /// Map of Insights RackingType values to Acme ArrayType values
        /// </summary>
        public static Dictionary<string, string> InsightsRackingTypeTypeToAcmeArrayTypeMap = new Dictionary<string, string>()
        {
            { "Fixed Tilt", "fixed-open-rack" },
            { "Single Axis", "1-axis" },
            { "Dual Axis", "2-axis" }
            //{ "???", "roof-mounted" } // Acme also supports roof-mounted
        };
        #endregion

        #region Properties
        /// <summary>
        ///  Generation Type: i.e. Solar or Wind
        /// </summary>
        public string Product { get; set; }

        /// <summary>
        /// internalID/Guid representation of the Project record
        /// </summary>
        [JsonProperty(PropertyName = "Project ID")] 
        public string ProjectId { get; set; }

        /// <summary>
        /// The "public" representation of the project, eg. PR-00001856
        /// </summary>
        [JsonProperty(PropertyName = "Project Number")] // 
        public string ProjectNumber { get; set; }

        /// <summary>
        /// Project Name - Friendly Name of the generation project.
        /// Getter removes all special characters 
        /// </summary>
        [JsonProperty(PropertyName = "Project Name")]
        public string ProjectName    
        {
            get => _name.RemoveSpecialCharacters();
            set => _name = value;
        }
        private string _name;  

        /// <summary>
        /// Latitude.  
        /// Getter removes trailing zeros after the decimal point
        /// </summary>
        public string Latitude {
            get => _latitude.IsNullOrEmpty() ? _latitude : string.Format("{0:G29}", decimal.Parse(_latitude));
            set => _latitude = value;
        }
        private string _latitude;

        /// <summary>
        /// Longitude.  
        /// Getter removes trailing zeros after the decimal point
        /// </summary>
        public string Longitude
        {
            get => _longitude.IsNullOrEmpty() ? _longitude : string.Format("{0:G29}", decimal.Parse(_longitude));
            set => _longitude = value;
        }
        private string _longitude;

        /// <summary>
        /// Weather Year
        /// </summary>
        public string WeatherYear { get; set; }

        /// <summary>
        /// Nameplate Capacity MWac << Note: AC
        /// </summary>
        [JsonProperty(PropertyName = "Project Total Nameplate Capacity (Mwac)")]
        public string NameplateCapacityMwac { get; set; }

        /// <summary>
        /// Nameplate Capacity MWdc << Note: DC
        /// </summary>
        [JsonProperty(PropertyName = "Project Total Nameplate Capacity (Mwdc)")]
        public string NameplateCapacityMwdc { get; set; }


        #region Wind Properties
        /// <summary>
        /// Turbine Manufacturer
        /// </summary>
        [JsonProperty(PropertyName = "Turbine Manufacturer")]
        public string TurbineManufacturer { get; set; }

        /// <summary>
        /// Turbine Model
        /// </summary>
        [JsonProperty(PropertyName = "Turbine Model")]
        public string TurbineModel { get; set; }

        /// <summary>
        /// Turbine Hub Height in meters - Getter removes trailing zeros after the decimal point
        /// </summary>
        [JsonProperty(PropertyName = "Turbine Hub Height (Meters)")]
        public string TurbineHubHeight
        {
            get => _turbineHubHeight.IsNullOrEmpty() ? _turbineHubHeight : string.Format("{0:G29}", decimal.Parse(_turbineHubHeight));
            set => _turbineHubHeight = value;
        }
        private string _turbineHubHeight;

        public string NumTurbines { get; set; }
        #endregion

        #region Solar Properties
        /// <summary>
        /// Azimuth  - Getter removes trailing zeros after the decimal point
        /// </summary>
        public string Azimuth
        {
            get => _azimuth.IsNullOrEmpty() ? _azimuth : string.Format("{0:G29}", decimal.Parse(_azimuth));
            set => _azimuth = value;
        }
        private string _azimuth;

        /// <summary>
        /// Tilt  - Getter removes trailing zeros after the decimal point
        /// </summary>
        public string Tilt
        {
            get => _tilt.IsNullOrEmpty() ? _tilt : string.Format("{0:G29}", decimal.Parse(_tilt));
            set => _tilt = value;
        }
        private string _tilt;

        /// <summary>
        /// DC/AC Ratio  - Getter removes trailing zeros after the decimal point 
        /// </summary>
        public string DcAcRatio
        {
            get => _dcacRatio.IsNullOrEmpty() ? _dcacRatio : string.Format("{0:G29}", decimal.Parse(_dcacRatio));
            set => _dcacRatio = value;
        }
        private string _dcacRatio;

        [JsonProperty(PropertyName = "Panel Type")]
        public string PanelType { get; set; }

        [JsonProperty(PropertyName = "Racking Type")]
        public string RackingType { get; set; }
        #endregion

        public string GroundCoverageRatio
        {
            get => _groundCoverageRatio.IsNullOrEmpty() ? _groundCoverageRatio : string.Format("{0:G29}", decimal.Parse(_groundCoverageRatio));
            set => _groundCoverageRatio = value;
        }
        private string _groundCoverageRatio;

        public string IsBifacial { get; set; }

        public string UseSnowLossModel { get; set; }

        #endregion // Properties

        #region helpers
        public static string GetModuleType(string panelType)
        {
            if (IsEmpty(panelType))
            {
                return null;
            }

            return InsightsPanelTypeTypeToAcmeModuleTypeMap[panelType];
        }

        public static string GetArrayType(string arrayType)
        {
            if (IsEmpty(arrayType))
            {
                return null;
            }

            return InsightsRackingTypeTypeToAcmeArrayTypeMap[arrayType];
        }

        public static bool IsEmpty(string s)
        {
            return string.IsNullOrEmpty(s) || s.EqualsIgnoreCase("N/A");
        }
        #endregion

        #region Acme Request Conversion
        /// <summary>
        /// Convert Model to AcmeWindGenerationRequest
        /// </summary>
        /// <returns></returns>
        public AcmeWindGenerationRequest ToAcmeWindGenerationRequest()
        {
            var turbineType = new TurbineType()
            {
                Vendor = TurbineManufacturer,
                Model = TurbineModel,
                HubHeightMeters = TurbineHubHeight,
                NumTurbines = NumTurbines,
            };

            var retVal = new AcmeWindGenerationRequest
            {
                WeatherYears = new List<string>() { WeatherYear },
                Latitude = Latitude,
                Longitude = Longitude,
                NameplateCapacity = NameplateCapacityMwac,  //Wind takes the ac value
                TurbineTypes = new List<TurbineType>() { turbineType }
            };

            return retVal;
        }

        /// <summary>
        /// Convert model to AcmeSolarGenerationRequest
        /// </summary>
        /// <returns></returns>
        public AcmeSolarGenerationRequest ToAcmeSolarGenerationRequest()
        {
            var retVal = new AcmeSolarGenerationRequest
            {
                WeatherYears = new List<string>() { WeatherYear },
                Latitude = Latitude,
                Longitude = Longitude,
                NameplateCapacity = NameplateCapacityMwdc,  //Solar takes the dc value
                DcAcRatio = DcAcRatio,

                ModuleType = GetModuleType(PanelType),
                ArrayType = GetArrayType(RackingType),
                Azimuth = Azimuth,
                Tilt = Tilt,
                
                UseSnowLossModel = UseSnowLossModel,
                GroundCoverageRatio = GroundCoverageRatio,
                IsBifacial = IsBifacial
            };

            return retVal;
        }
        #endregion

        #region Validation
        public CallResult ValidateWind()
        {
            var validateCoreResult = ValidateCore();
            if (!validateCoreResult.Success)
            {
                return validateCoreResult;
            }

            // wind specific values
            if (this.TurbineManufacturer.IsNullOrWhiteSpace())
            {
                return CallResult.CreateFailedResult($"Bogus value for TurbineManufacturer: {this.TurbineManufacturer}");
            }

            if (this.TurbineModel.IsNullOrWhiteSpace())
            {
                return CallResult.CreateFailedResult($"Bogus value for Turbine Model: {this.TurbineModel}");
            }

            if (this.TurbineHubHeight.IsNullOrWhiteSpace())
            {
                return CallResult.CreateFailedResult($"Bogus value for Turbine Hub Height: {this.TurbineHubHeight}");
            }

            if (this.NumTurbines.IsNullOrWhiteSpace())
            {
                return CallResult.CreateFailedResult($"Bogus value for Num Turbines: {this.NumTurbines}");
            }
            
            if (this.NameplateCapacityMwac.IsNullOrWhiteSpace() || !float.TryParse(this.NameplateCapacityMwac, out _))
            {
                return CallResult.CreateFailedResult($"Bogus value for NameplateCapacityMwac: {this.NameplateCapacityMwac}");
            }

            return new CallResult() { Success = true };
        }

        public CallResult ValidateSolar()
        {
            var validateCoreResult = ValidateCore();
            if (!validateCoreResult.Success)
            {
                return validateCoreResult;
            }

            // Solar specific values
            if (this.NameplateCapacityMwdc.IsNullOrWhiteSpace() || !float.TryParse(this.NameplateCapacityMwdc, out _))
            {
                return CallResult.CreateFailedResult($"Bogus value for NameplateCapacityMwdc: {this.NameplateCapacityMwdc}");
            }

            // we need to send this, so cannot be null
            if (this.IsBifacial.IsNullOrWhiteSpace())
            {
                return CallResult.CreateFailedResult($"Bogus value for IsBifacial: {this.IsBifacial}");
            }

            return new CallResult() { Success = true };
        }

        private CallResult ValidateCore()
        {
            if (this.ProjectNumber.IsNullOrWhiteSpace())
            {
                return CallResult.CreateFailedResult($"Bogus value for ProjectNumber: {this.ProjectNumber}");
            }

            if (this.ProjectName.IsNullOrWhiteSpace())
            {
                return CallResult.CreateFailedResult($"Bogus value for ProjectName: {this.ProjectName}");
            }

            if (this.Latitude.IsNullOrWhiteSpace())
            {
                return CallResult.CreateFailedResult($"Bogus value for Latitude: {this.Latitude}");
            }

            if (this.Longitude.IsNullOrWhiteSpace())
            {
                return CallResult.CreateFailedResult($"Bogus value for Longitude: {this.Longitude}");
            }

            if (this.WeatherYear.IsNullOrWhiteSpace())
            {
                return CallResult.CreateFailedResult($"Bogus value for WeatherYears: {string.Join(',', this.WeatherYear)}");
            }

            return new CallResult() { Success = true };
        }
        #endregion

    }
}