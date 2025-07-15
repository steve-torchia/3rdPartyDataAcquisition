using Xunit;
using System;

namespace Acme.AcquireGeneration.Test
{
    public class AcmeAcquireGenerationHelpersTests
    {
        [Fact]
        public void GetFileNamePrefix_Success_Wind()
        {
            var windProjet = AcmeAcquireGenerationTestHelpers.GetTestWindGenerationRequest();

            var prefix = AcmeAcquireGenerationHelpers.GetFileNamePrefix(windProjet);

            Assert.Equal(
                "Wind_PR-000001_Vandelay Wind Industries_2021_40.8055772_-73.9655785_140_Kramerica_K-2468_85_0", 
                prefix);
        }

        [Fact]
        public void GetFileNamePrefix_Success_Solar()
        {
            var solarProj = AcmeAcquireGenerationTestHelpers.GetTestSolarGenerationRequest();

            var prefix = AcmeAcquireGenerationHelpers.GetFileNamePrefix(solarProj);

            Assert.Equal(
                "Solar_PR-000002_Vandelay Solar Industries_2021_40.8055772_-73.9655785_100_1.11_~_~_~_~_~_true_~",
                prefix);
        }

        [Fact]
        public void GetFileNamePrefix_Fail_BadGenType()
        {
            var solarProj = AcmeAcquireGenerationTestHelpers.GetTestSolarGenerationRequest();
            solarProj.Product = "Batteries";

            var ret = Assert.Throws<Exception>(() => AcmeAcquireGenerationHelpers.GetFileNamePrefix(solarProj));

            Assert.Equal("Error with generationType (Batteries)", ret.Message);
        }

        [Fact]
        public void GetFileNamePrefix_Fail_BadProjectData()
        {
            var ret = Assert.Throws<NullReferenceException>(() => AcmeAcquireGenerationHelpers.GetFileNamePrefix(null));
        }
    }
}
