using Ingress.Lib.Base;
using Acme.Contracts;
using DP.Base.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using DP.Base;

namespace Acme.AcquireGeneration
{
    public class AcmeAcquireGenerationHelpers : AcmeHelpers
    {
        internal static string GetOrchestrationId(List<AcquireGenerationInputModel> projectList)
        {
            // simple way to make a unique hash based on the project Ids. This is enough for an orchestration Id
            var pidList = projectList.Select(o => o.ProjectNumber + "_" + o.WeatherYear).ToList();
            var hash = HashUtils.GetOrderIndependentHashCode(pidList);
            return $"AcmeAcquireGen_{hash}";
        }

        internal static string GetFileNamePrefix(AcquireGenerationInputModel project)
        {
            try
            {
                if (project.Product.EqualsIgnoreCase(GenerationTypeWind))
                {
                    return WindFileSpecPrefix
                        .Replace(ProjectNumberToken, project.ProjectNumber)
                        .Replace(ProjectNameToken, project.ProjectName)
                        .Replace(GenerationTypeToken, GenerationTypeWind)
                        .Replace(WeatherYearToken, project.WeatherYear)
                        .Replace(LatToken, project.Latitude)
                        .Replace(LngToken, project.Longitude)
                        .Replace(NameplateToken, project.NameplateCapacityMwac)
                        .Replace(TurbineVendorToken, project.TurbineManufacturer)
                        .Replace(TurbineModelToken, project.TurbineModel)
                        .Replace(TurbineHubHeightToken, project.TurbineHubHeight)
                        .Replace(TurbineCountToken, project.NumTurbines)
                        .RemoveIllegalFilePathCharacters();
                }

                if (project.Product.EqualsIgnoreCase(GenerationTypeSolar))
                {
                    // Use a placeholder token for fields that come in empty - Makes it easier to read the filename

                    return SolarFileSpecPrefix
                        .Replace(ProjectNumberToken, project.ProjectNumber)
                        .Replace(ProjectNameToken, project.ProjectName)
                        .Replace(GenerationTypeToken, GenerationTypeSolar)
                        .Replace(WeatherYearToken, project.WeatherYear)
                        .Replace(LatToken, project.Latitude)
                        .Replace(LngToken, project.Longitude)
                        .Replace(NameplateToken, project.NameplateCapacityMwdc)
                        .Replace(ModuleTypeToken, AcquireGenerationInputModel.IsEmpty(project.PanelType) ? EmptyValueToken : project.PanelType)
                        .Replace(ArrayTypeToken, AcquireGenerationInputModel.IsEmpty(project.RackingType) ? EmptyValueToken : project.RackingType)
                        .Replace(TiltToken, AcquireGenerationInputModel.IsEmpty(project.Tilt) ? EmptyValueToken : project.Tilt)
                        .Replace(AzimuthToken, AcquireGenerationInputModel.IsEmpty(project.Azimuth) ? EmptyValueToken : project.Azimuth)
                        .Replace(DcAcRatioToken, AcquireGenerationInputModel.IsEmpty(project.DcAcRatio) ? EmptyValueToken : project.DcAcRatio)
                        .Replace(UseSnowLossModelToken, AcquireGenerationInputModel.IsEmpty(project.UseSnowLossModel) ? EmptyValueToken : project.UseSnowLossModel)
                        .Replace(GroundCoverageRatioToken, AcquireGenerationInputModel.IsEmpty(project.GroundCoverageRatio) ? EmptyValueToken : project.GroundCoverageRatio)
                        .Replace(IsBifacialToken, project.IsBifacial)
                        .RemoveIllegalFilePathCharacters();
                }

                throw new System.Exception($"Error with generationType ({project.Product})");
            }
            catch
            {
                throw;
            }
        }

        internal static AcmeAcquireGenerationContext GetAcmeAcquireGenerationContext(AcquireGenerationInputModel project, BlobConfigInfo destinationInfo)
        {
            return new AcmeAcquireGenerationContext
            {
                Project = project,
                BlobDestinationConfigInfo = destinationInfo
            };
        }

        internal static List<AcmeAcquireGenerationContext> GetAcmeGenerationContextList(List<AcquireGenerationInputModel> projectList, BlobConfigInfo destinationInfo)
        {
            var retVal = new List<AcmeAcquireGenerationContext>();

            foreach (var input in projectList)
            {
                retVal.Add(GetAcmeAcquireGenerationContext(input, destinationInfo));
            }

            return retVal;
        }
    }
}